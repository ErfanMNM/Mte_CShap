using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glib.Omron;
using GProject.DataPoolHelper;
using GProject.ProductionOrderHelpers;
using GProject.Production;
using GProject.Auth;
using GProject.Infrastructure;
using GProject.IoT;
using Serilog;

namespace GProject;

public class GProjectApiServer : IDisposable
{
    private WebApplication? _app;
    private readonly int _port;
    private readonly string _host;
    private readonly Action<string, string>? _onLog;

    public GProjectApiServer(int port = 9999, string host = "0.0.0.0", Action<string, string>? onLog = null)
    {
        _port = port;
        _host = host;
        _onLog = onLog;
    }

    public async Task StartAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:3001", "http://localhost:3000", "http://127.0.0.1:3001", "http://127.0.0.1:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        builder.WebHost
            .UseUrls($"http://{_host}:{_port}")
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

        _app = builder.Build();

        // Global exception handler middleware
        _app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                Log.Error(ex, "Unhandled exception in request {Path} {Method}", context.Request.Path, context.Request.Method);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\":false,\"message\":\"Internal server error\"}");
            });
        });

        // Auth middleware
        _app.Use(async (context, next) =>
        {
            await AuthMiddleware.InvokeAsync(context, next);
        });

        _app.UseCors();

        // WebSocket middleware
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(30)
        };
        _app.UseWebSockets(webSocketOptions);

        _app.Map("/ws/camera", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            CameraHub.Instance.Register(ws);
            Log.Information("[WebSocket] Camera client connected. Total clients: {Count}", CameraHub.Instance.ClientCount);

            try
            {
                var buffer = new byte[1024];
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    // We do not process inbound messages; clients are pure subscribers.
                }
            }
            finally
            {
                CameraHub.Instance.Unregister(ws);
                Log.Information("[WebSocket] Camera client disconnected. Total clients: {Count}", CameraHub.Instance.ClientCount);
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                    }
                    catch { }
                }
            }
        });

        // Auth endpoints
        _app.MapPost("/api/auth/login", HandleLogin);
        _app.MapPost("/api/auth/logout", HandleLogout);
        _app.MapGet("/api/auth/me", HandleGetCurrentUser);
        _app.MapGet("/api/auth/history", HandleGetLoginHistory);
        _app.MapGet("/api/auth/users", HandleGetUsers);
        _app.MapPost("/api/auth/users", HandleCreateUser);
        _app.MapPut("/api/auth/users/{id}", HandleUpdateUser);
        _app.MapDelete("/api/auth/users/{id}", HandleDeleteUser);

        // DataPool endpoints
        _app.MapGet("/api/health", HandleHealth);
        _app.MapGet("/api/datapool/pools", HandleListPools);
        _app.MapGet("/api/datapool/pools/stats", HandleGetPoolsStats);
        _app.MapPost("/api/datapool/pools", HandleCreatePool);
        _app.MapGet("/api/datapool/{poolName}/codes", HandleGetCodes);
        _app.MapGet("/api/datapool/{poolName}/codes/search", HandleSearchCodes);
        _app.MapGet("/api/datapool/{poolName}/code/{code}", HandleGetCode);
        _app.MapPost("/api/datapool/add", HandleAddCode);
        _app.MapPost("/api/datapool/add-reader", HandleAddFromReader);
        _app.MapPost("/api/datapool/import-file", HandleImportFromFile);
        _app.MapPost("/api/datapool/import-content", HandleImportFromContent);
        _app.MapPut("/api/datapool/{poolName}/code/{code}/status", HandleUpdateStatus);
        _app.MapDelete("/api/datapool/{poolName}/code/{code}", HandleDeleteCode);

        // PO endpoints
        _app.MapPost("/api/po", (Func<HttpContext, Task<IResult>>)POApiServer.HandleCreatePO);
        _app.MapGet("/api/po/list", POApiServer.HandleGetAllPO);
        _app.MapGet("/api/po/{orderNo}", (string orderNo) => POApiServer.HandleGetPO(orderNo));
        _app.MapGet("/api/po/{orderNo}/can-delete", (string orderNo) => POApiServer.HandleCanDeletePO(orderNo));
        _app.MapGet("/api/po/{orderNo}/status", (string orderNo) => POApiServer.HandleCheckPOStatus(orderNo));
        _app.MapPost("/api/po/{orderNo}/ensure-ready", async (HttpContext context, string orderNo) 
            => await POApiServer.HandleEnsurePODatabaseReady(context, orderNo));
        _app.MapDelete("/api/po/{orderNo}", (string orderNo) => POApiServer.HandleDeletePO(orderNo));
        _app.MapGet("/api/po/{orderNo}/codes", (string orderNo, int? status, string? cartonCode, int limit = 100, int offset = 0) 
            => POApiServer.HandleGetCodes(orderNo, status, cartonCode, limit, offset));
        _app.MapPost("/api/po/{orderNo}/activate", async (HttpContext context, string orderNo) 
            => await POApiServer.HandleActivateCode(context, orderNo));
        _app.MapPost("/api/po/{orderNo}/pack", async (HttpContext context, string orderNo) 
            => await POApiServer.HandlePackCode(context, orderNo));
        _app.MapGet("/api/po/{orderNo}/cartons", (string orderNo) => POApiServer.HandleGetCartons(orderNo));
        _app.MapPost("/api/po/{orderNo}/cartons/start", async (HttpContext context, string orderNo) 
            => await POApiServer.HandleStartCarton(context, orderNo));
        _app.MapPost("/api/po/{orderNo}/cartons/complete", async (HttpContext context, string orderNo) 
            => await POApiServer.HandleCompleteCarton(context, orderNo));
        _app.MapPut("/api/po/{orderNo}/production-date", HandleUpdateProductionDate);

        // Production status
        _app.MapGet("/api/production/status", POApiServer.HandleGetProductionStatus);

        // Camera history ring buffer
        _app.MapGet("/api/camera-history", CameraHistoryHandler.HandleGet);

        // Carton PDA endpoints
        _app.MapGet("/api/carton/current-po", POApiServer.HandleGetCurrentPO);
        _app.MapPost("/api/carton/scan", (ctx) =>  POApiServer.HandleCartonScan(ctx));
        _app.MapGet("/api/carton/{cartonCode}/info", (string cartonCode) => POApiServer.HandleCartonInfo(cartonCode));

        // Production state machine control endpoints
        _app.MapGet("/api/production/state", (Delegate)HandleGetProductionState);
        _app.MapPost("/api/production/select-po", (Delegate)HandleSelectPO);
        _app.MapPost("/api/production/start-editing", (Delegate)HandleStartEditing);
        _app.MapPost("/api/production/set-po", (Delegate)HandleSetPO);
        _app.MapPost("/api/production/start", (Delegate)HandleProductionStart);
        _app.MapPost("/api/production/stop", (Delegate)HandleProductionStop);
        _app.MapPost("/api/production/reset", (Delegate)HandleProductionReset);
        _app.MapGet("/api/production/ping", (Delegate)HandleProductionPing);
        _app.MapPost("/api/production/retry", (Delegate)HandleProductionRetry);

        // Device status (aggregates Camera, PLC, Production states)
        _app.MapGet("/api/devices/status", HandleGetDevicesStatus);

        // PLC recipe endpoints
        _app.MapGet("/api/plc/recipe/active", (Delegate)HandlePlcGetActiveRecipe);
        _app.MapGet("/api/plc/recipe/list", (Delegate)HandlePlcGetAllRecipes);
        _app.MapGet("/api/plc/recipe/from-plc", (Delegate)HandlePlcReadRecipeFromDevice);
        _app.MapPost("/api/plc/recipe/save", (Delegate)HandlePlcSaveRecipe);
        _app.MapPost("/api/plc/recipe/active", (Delegate)HandlePlcSetActiveRecipe);
        _app.MapDelete("/api/plc/recipe/{id:int}", (Delegate)HandlePlcDeleteRecipe);

        // Logs API (SAdmin only - see LogApi.Permission)
        _app.MapGet("/api/logs", GProject.Logs.LogApi.HandleList);
        _app.MapGet("/api/logs/levels", GProject.Logs.LogApi.HandleListLevels);
        _app.MapGet("/api/logs/tags", GProject.Logs.LogApi.HandleListTags);
        _app.MapGet("/api/logs/{id:long}", GProject.Logs.LogApi.HandleGet);

        // Recipe custom registers endpoints
        _app.MapGet("/api/plc/recipe/{recipeId:int}/registers", (Delegate)HandlePlcGetRegisters);
        _app.MapPost("/api/plc/recipe/{recipeId:int}/registers", (Delegate)HandlePlcSaveRegisters);
        _app.MapPost("/api/plc/recipe/{recipeId:int}/registers/read", (Delegate)HandlePlcReadRegistersFromDevice);
        _app.MapPost("/api/plc/recipe/{recipeId:int}/registers/write", (Delegate)HandlePlcWriteRegistersToDevice);

        // AWS IoT endpoints
        _app.MapGet("/api/aws/status", HandleGetAWSStatus);
        _app.MapPost("/api/aws/connect", HandleAWConnect);
        _app.MapPost("/api/aws/disconnect", HandleAWSDisconnect);
        _app.MapPut("/api/aws/config", HandleAWSUpdateConfig);
        _app.MapGet("/api/aws/config", HandleAWSGetConfig);
        _app.MapPost("/api/aws/test", HandleAWSTestPublish);

        // Initialize PO database
        POApiServer.Initialize();

        LogInfo("GProjectApiServer", $"PO endpoints registered");
        LogInfo("GProjectApiServer", $"Started on http://{_host}:{_port}");

        await _app.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_app != null)
        {
            LogInfo("GProjectApiServer", "Stopping...");
            await _app.StopAsync();
            _app = null;
        }
    }


    #region Auth Handlers
    private IResult HandleLogin(HttpContext context)
    {
        try
        {
            var request = context.Request.ReadFromJsonAsync<LoginRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Results.Json(new { success = false, message = "Username and password are required." }, statusCode: 400);

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var (user, sessionId) = AuthService.Login(request.Username, request.Password, ipAddress, userAgent);

            if (user == null || sessionId == null)
                return Results.Json(new { success = false, message = "Invalid username or password." }, statusCode: 401);

            // Set session cookie
            context.Response.Cookies.Append(AuthMiddleware.SessionCookieName, sessionId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            // Transition BE state machine to Checking after successful login
            ProductionStateMachine.Instance.SetState(e_ProductionState.Checking, $"login {user.Username}");

            return Results.Json(new
            {
                success = true,
                user = new { id = user.Id, username = user.Username, displayName = user.DisplayName, role = user.Role },
                productionState = ProductionStateMachine.Instance.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("Auth", $"Login error: {ex.Message}", ex);
            return Results.Json(new { success = false, message = "Login failed." }, statusCode: 500);
        }
    }

    private IResult HandleLogout(HttpContext context)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser != null)
        {
            AuthService.Logout(currentUser.SessionId);
        }

        context.Response.Cookies.Delete(AuthMiddleware.SessionCookieName);
        return Results.Json(new { success = true, message = "Logged out." });
    }

    private IResult HandleGetCurrentUser(HttpContext context)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser == null)
            return AuthHelper.Unauthorized("Not authenticated.");

        return Results.Json(new
        {
            success = true,
            user = new
            {
                sessionId = currentUser.SessionId,
                userId = currentUser.UserId,
                username = currentUser.Username,
                role = currentUser.Role,
                expiresAt = currentUser.ExpiresAt.ToString("o")
            }
        });
    }

    private IResult HandleGetLoginHistory(HttpContext context)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser == null)
            return AuthHelper.Unauthorized();

        // Only SAdmin and Administrator can view history
        if (!AuthHelper.HasPermission(currentUser.Role, "view_history"))
            return AuthHelper.Forbidden("Not allowed.");

        var limit = 100;
        var history = AuthService.GetLoginHistory(limit);
        return Results.Json(new { success = true, count = history.Count, data = history });
    }

    private IResult HandleGetUsers(HttpContext context)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser == null)
            return AuthHelper.Unauthorized();

        if (!AuthHelper.HasPermission(currentUser.Role, "manage_users"))
            return AuthHelper.Forbidden("Not allowed.");

        var users = AuthService.GetAllUsers();
        return Results.Json(new { success = true, count = users.Count, data = users });
    }

    private IResult HandleCreateUser(HttpContext context)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser == null)
            return AuthHelper.Unauthorized();

        if (!AuthHelper.HasPermission(currentUser.Role, "manage_users"))
            return AuthHelper.Forbidden("Not allowed.");

        try
        {
            var request = context.Request.ReadFromJsonAsync<CreateUserRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Role))
                return Results.Json(new { success = false, message = "Username, password, and role are required." }, statusCode: 400);

            var validRoles = new[] { "SAdmin", "Administrator", "Operator", "Viewer" };
            if (!validRoles.Contains(request.Role))
                return Results.Json(new { success = false, message = "Invalid role." }, statusCode: 400);

            var success = AuthService.CreateUser(request.Username, request.Password, request.DisplayName ?? request.Username, request.Role, currentUser.Username);
            return Results.Json(new { success, message = success ? "User created." : "Failed to create user." });
        }
        catch (Exception ex)
        {
            LogError("Auth", $"Create user error: {ex.Message}", ex);
            return Results.Json(new { success = false, message = "Failed to create user." }, statusCode: 500);
        }
    }

    private IResult HandleUpdateUser(HttpContext context, string id)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser == null)
            return AuthHelper.Unauthorized();

        if (!AuthHelper.HasPermission(currentUser.Role, "manage_users"))
            return AuthHelper.Forbidden("Not allowed.");

        try
        {
            var request = context.Request.ReadFromJsonAsync<UpdateUserRequest>().GetAwaiter().GetResult();
            if (request == null)
                return Results.Json(new { success = false, message = "Invalid request." }, statusCode: 400);

            if (request.Role != null)
            {
                var validRoles = new[] { "SAdmin", "Administrator", "Operator", "Viewer" };
                if (!validRoles.Contains(request.Role))
                    return Results.Json(new { success = false, message = "Invalid role." }, statusCode: 400);
            }

            var success = AuthService.UpdateUser(id, request.Password, request.DisplayName, request.Role, request.IsActive);
            return Results.Json(new { success, message = success ? "User updated." : "Failed to update user." });
        }
        catch (Exception ex)
        {
            LogError("Auth", $"Update user error: {ex.Message}", ex);
            return Results.Json(new { success = false, message = "Failed to update user." }, statusCode: 500);
        }
    }

    private IResult HandleDeleteUser(HttpContext context, string id)
    {
        var currentUser = AuthMiddleware.GetCurrentUser(context);
        if (currentUser == null)
            return AuthHelper.Unauthorized();

        if (!AuthHelper.HasPermission(currentUser.Role, "manage_users"))
            return AuthHelper.Forbidden("Not allowed.");

        // Cannot delete yourself
        if (currentUser.UserId == id)
            return Results.Json(new { success = false, message = "Cannot delete yourself." }, statusCode: 400);

        var success = AuthService.DeleteUser(id);
        return Results.Json(new { success, message = success ? "User deleted." : "Failed to delete user." });
    }
    #endregion

    #region DataPool Handlers
    private IResult HandleHealth(HttpContext context)
    {
        return Results.Json(new HealthResponse { Status = "OK", Timestamp = DateTime.Now });
    }

    private IResult HandleListPools(HttpContext context)
    {
        try
        {
            var result = DataPoolStatic.ListAllPools();
            if (!result.IsSuccess)
                return Results.Json(new ApiResponse { Success = false, Message = result.Message }, statusCode: 500);

            var pools = new List<object>();
            if (result.Data != null)
            {
                foreach (System.Data.DataRow row in result.Data.Rows)
                {
                    var name = row["name"]?.ToString() ?? "";
                    if (string.Equals(name, "Phieu", StringComparison.OrdinalIgnoreCase)) continue;
                    pools.Add(new { name, fileName = row["fileName"]?.ToString() ?? "", size = Convert.ToInt64(row["size"]) });
                }
            }
            return Results.Json(new { success = true, count = pools.Count, data = pools });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error listing pools: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleGetPoolsStats(HttpContext context)
    {
        try
        {
            var poolsResult = DataPoolStatic.ListAllPools();
            if (!poolsResult.IsSuccess)
                return Results.Json(new ApiResponse { Success = false, Message = poolsResult.Message }, statusCode: 500);

            var allStats = DataPoolStatic.GetAllPoolStats();
            var statsDict = allStats.ToDictionary(s => s.Name, s => new {
                totalCodes = s.Total,
                availableCodes = s.Unused,
                usedCodes = s.Used
            });

            var pools = new List<object>();
            if (poolsResult.Data != null)
            {
                foreach (System.Data.DataRow row in poolsResult.Data.Rows)
                {
                    var name = row["name"]?.ToString() ?? "";
                    if (string.Equals(name, "Phieu", StringComparison.OrdinalIgnoreCase)) continue;

                    object poolObj;
                    if (statsDict.TryGetValue(name, out var stats))
                    {
                        poolObj = new {
                            name,
                            fileName = row["fileName"]?.ToString() ?? "",
                            size = Convert.ToInt64(row["size"]),
                            totalCodes = stats.totalCodes,
                            availableCodes = stats.availableCodes,
                            usedCodes = stats.usedCodes
                        };
                    }
                    else
                    {
                        poolObj = new {
                            name,
                            fileName = row["fileName"]?.ToString() ?? "",
                            size = Convert.ToInt64(row["size"]),
                            totalCodes = 0,
                            availableCodes = 0,
                            usedCodes = 0
                        };
                    }
                    pools.Add(poolObj);
                }
            }
            return Results.Json(new { success = true, count = pools.Count, data = pools });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting pool stats: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleCreatePool(HttpContext context)
    {
        try
        {
            var body = context.Request.ReadFromJsonAsync<CreatePoolRequest>().GetAwaiter().GetResult();
            if (body == null || string.IsNullOrWhiteSpace(body.PoolName))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName is required." }, statusCode: 400);

            DataPoolStatic.EnsurePool(body.PoolName);
            return Results.Json(new ApiResponse { Success = true, Message = $"Pool '{body.PoolName}' created." });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error creating pool: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleGetCodes(HttpContext context, string poolName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName)) return Results.BadRequest();

            int? status = null;
            if (context.Request.Query.TryGetValue("status", out var statusVal) && int.TryParse(statusVal.ToString(), out int s))
                status = s;

            int limit = 20;
            if (context.Request.Query.TryGetValue("limit", out var limitVal) && int.TryParse(limitVal.ToString(), out int l))
                limit = l;

            var result = DataPoolStatic.GetAll(poolName, status, limit);
            if (!result.IsSuccess || result.Data == null)
                return Results.Json(new ApiResponse { Success = false, Message = result.Message }, statusCode: 404);

            var codes = new List<object>();
            foreach (System.Data.DataRow row in result.Data.Rows)
            {
                codes.Add(new
                {
                    id = Convert.ToInt32(row["ID"]),
                    code = row["Code"]?.ToString() ?? "",
                    status = Convert.ToInt32(row["Status"]),
                    batchID = row["BatchID"]?.ToString() ?? "",
                    createTime = row["CreateTime"]?.ToString() ?? "",
                    createID = row["CreateID"]?.ToString() ?? "",
                    note = row["Note"]?.ToString() ?? ""
                });
            }
            return Results.Json(new { success = true, count = codes.Count, data = codes });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting codes from pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleSearchCodes(HttpContext context, string poolName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName)) return Results.BadRequest();

            string? keyword = null;
            if (context.Request.Query.TryGetValue("q", out var kw))
                keyword = kw.ToString();

            if (string.IsNullOrWhiteSpace(keyword))
                return Results.Json(new ApiResponse { Success = false, Message = "q (keyword) is required." }, statusCode: 400);

            int limit = 50;
            if (context.Request.Query.TryGetValue("limit", out var limitVal) && int.TryParse(limitVal.ToString(), out int l))
                limit = l;

            var result = DataPoolStatic.SearchCodes(poolName, keyword, limit);
            if (!result.IsSuccess || result.Data == null)
                return Results.Json(new ApiResponse { Success = false, Message = result.Message }, statusCode: 404);

            var codes = new List<object>();
            foreach (System.Data.DataRow row in result.Data.Rows)
            {
                codes.Add(new
                {
                    id = Convert.ToInt32(row["ID"]),
                    code = row["Code"]?.ToString() ?? "",
                    status = Convert.ToInt32(row["Status"]),
                    batchID = row["BatchID"]?.ToString() ?? "",
                    createTime = row["CreateTime"]?.ToString() ?? "",
                    createID = row["CreateID"]?.ToString() ?? "",
                    note = row["Note"]?.ToString() ?? ""
                });
            }
            return Results.Json(new { success = true, count = codes.Count, data = codes });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error searching codes in pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleGetCode(HttpContext context, string poolName, string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code)) return Results.BadRequest();

            var result = DataPoolStatic.GetByCode(poolName, code);
            if (!result.IsSuccess || result.Data == null || result.Data.Rows.Count == 0)
                return Results.NotFound(new ApiResponse { Success = false, Message = "Code not found." });

            var row = result.Data.Rows[0];
            return Results.Json(new
            {
                success = true,
                data = new
                {
                    id = Convert.ToInt32(row["ID"]),
                    code = row["Code"]?.ToString() ?? "",
                    status = Convert.ToInt32(row["Status"]),
                    batchID = row["BatchID"]?.ToString() ?? "",
                    createTime = row["CreateTime"]?.ToString() ?? "",
                    createID = row["CreateID"]?.ToString() ?? "",
                    note = row["Note"]?.ToString() ?? ""
                }
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting code '{code}' from pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleAddCode(HttpContext context)
    {
        string? poolName = null;
        try
        {
            var request = context.Request.ReadFromJsonAsync<AddCodeManualRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.Code))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName and code are required." }, statusCode: 400);

            poolName = request.PoolName;
            var result = DataPoolStatic.Import_Manual(request.PoolName, request.Code, request.Status, request.BatchID ?? "", "", request.Note ?? "", request.UserName ?? "API");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error adding code to pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleAddFromReader(HttpContext context)
    {
        string? poolName = null;
        try
        {
            var request = context.Request.ReadFromJsonAsync<AddCodeReaderRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.BatchID))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName, code, and batchID are required." }, statusCode: 400);

            poolName = request.PoolName;
            var result = DataPoolStatic.Import_FromReader(request.PoolName, request.Code, request.BatchID, "Reader", request.Note ?? "");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error adding code from reader to pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleImportFromFile(HttpContext context)
    {
        string? poolName = null;
        try
        {
            var request = context.Request.ReadFromJsonAsync<ImportCodesFromFileRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.CsvPath) || string.IsNullOrWhiteSpace(request.CreateID))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName, csvPath, and createID are required." }, statusCode: 400);

            poolName = request.PoolName;
            var result = DataPoolStatic.Import_FromFile(request.PoolName, request.CsvPath, request.UserName ?? "API", request.CreateID, request.CodeColumn ?? "Code", request.NoteColumn ?? "", request.Note ?? "");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error importing from file to pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleImportFromContent(HttpContext context)
    {
        string? poolName = null;
        try
        {
            var request = context.Request.ReadFromJsonAsync<ImportCodesFromContentRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.CsvContent) || string.IsNullOrWhiteSpace(request.CreateID))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName, csvContent, and createid are required." }, statusCode: 400);

            poolName = request.PoolName;
            LogInfo("GProjectApiServer", $"Importing {request.CsvContent.Split('\n').Length - 1} codes to pool '{request.PoolName}'");

            // Parse CSV content and insert
            var result = DataPoolStatic.Import_FromContent(request.PoolName, request.CsvContent, request.UserName ?? "API", request.CreateID, request.Note ?? "");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error importing from content to pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleUpdateStatus(HttpContext context, string poolName, string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code)) return Results.BadRequest();

            var body = context.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>().GetAwaiter().GetResult();

            int newStatus = -1;
            string batchID = "", note = "";

            if (body.TryGetProperty("status", out var statusProp) && statusProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                newStatus = statusProp.GetInt32();
            if (body.TryGetProperty("batchID", out var batchProp) && batchProp.ValueKind == System.Text.Json.JsonValueKind.String)
                batchID = batchProp.GetString() ?? "";
            if (body.TryGetProperty("note", out var noteProp) && noteProp.ValueKind == System.Text.Json.JsonValueKind.String)
                note = noteProp.GetString() ?? "";

            if (newStatus < 0 || newStatus > 1)
                return Results.Json(new ApiResponse { Success = false, Message = "status must be 0 or 1." }, statusCode: 400);

            var ok = DataPoolStatic.UpdateStatus(poolName, code, newStatus, batchID, "", note);
            return ok
                ? Results.Json(new ApiResponse { Success = true, Message = "Status updated.", Count = 1 })
                : Results.NotFound(new ApiResponse { Success = false, Message = "Code not found." });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error updating status for code '{code}' in pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleDeleteCode(HttpContext context, string poolName, string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code)) return Results.BadRequest();

            var result = DataPoolStatic.Delete(poolName, code);
            return result.IsSuccess
                ? Results.Json(new ApiResponse { Success = true, Message = result.Message, Count = 1 })
                : Results.NotFound(new ApiResponse { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error deleting code '{code}' from pool '{poolName}': {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }
    #endregion

    #region Production State Machine Handlers
    private async Task<IResult> HandleGetProductionState(HttpContext context)
    {
        try
        {
            var sm = ProductionStateMachine.Instance;
            await Task.CompletedTask;
            return Results.Json(new
            {
                success = true,
                currentState = sm.CurrentState.ToString(),
                previousState = sm.PreviousState.ToString(),
                orderNo = ProductionStateMachine.ProductionData?.OrderNo ?? "",
                productName = ProductionStateMachine.ProductionData?.ProductName ?? "",
                orderQty = ProductionStateMachine.ProductionData?.OrderQty ?? 0,
                activeCounter = new
                {
                    PassTotal = sm.ActiveCounter.PassTotal,
                    FailTotal = sm.ActiveCounter.FailTotal,
                    DuplicateCount = sm.ActiveCounter.DuplicateCount,
                    NotFoundCount = sm.ActiveCounter.NotFoundCount,
                    ReadFailCount = sm.ActiveCounter.ReadFailCount,
                    ErrorCount = sm.ActiveCounter.ErrorCount,
                    TimeoutCount = sm.ActiveCounter.TimeoutCount,
                    TotalCount = sm.ActiveCounter.TotalCount,
                    CartonID = sm.ActiveCounter.CartonID,
                    CartonCode = sm.ActiveCounter.CartonCode
                },
                codesCount = sm.Dictionary_Codes.Count,
                cartonsCount = sm.Dictionary_Cartons.Count,
                lastWarning = sm.LastWarning,
                isAppReady = sm.IsAppReady,
                isDeviceReady = sm.IsDeviceReady
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting production state: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task<IResult> HandleStartEditing(HttpContext context)
    {
        try
        {
            await Task.CompletedTask;
            var sm = ProductionStateMachine.Instance;
            
            // Cho phep tu Ready hoac Editing
            if (sm.CurrentState != e_ProductionState.Ready && 
                sm.CurrentState != e_ProductionState.Editing)
                return Results.Json(new
                {
                    success = false,
                    message = $"Không thể đổi PO từ trạng thái {sm.CurrentState}. Cần ở trạng thái Ready."
                }, statusCode: 400);

            // Kiem tra da chay san pham chua
            if (sm.ActiveCounter.TotalCount > 0)
                return Results.Json(new
                {
                    success = false,
                    message = "Không thể đổi PO khi đã chạy hàng. Vui lòng Reset PO trước."
                }, statusCode: 400);

            sm.SetState(e_ProductionState.Editing, "user request edit PO");
            return Results.Json(new
            {
                success = true,
                message = "Đã chuyển sang chế độ chỉnh sửa",
                state = sm.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error StartEditing: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task<IResult> HandleSetPO(HttpContext context)
    {
        try
        {
            var body = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
            if (body == null || !body.TryGetValue("orderNo", out var orderNo) || string.IsNullOrWhiteSpace(orderNo))
                return Results.Json(new ApiResponse { Success = false, Message = "orderNo is required" }, statusCode: 400);

            var sm = ProductionStateMachine.Instance;
            
            // Chi cho phep tu Editing
            if (sm.CurrentState != e_ProductionState.Editing)
                return Results.Json(new
                {
                    success = false,
                    message = $"Không thể cài lệnh từ trạng thái {sm.CurrentState}. Cần ở trạng thái Editing."
                }, statusCode: 400);

            var po = GProduction.POLoader.GetByOrderNo(orderNo);
            if (!po.IsSuccess || po.Data == null || po.Data.Rows.Count == 0)
                return Results.Json(new ApiResponse { Success = false, Message = $"PO '{orderNo}' không tồn tại" }, statusCode: 404);

            ProductionStateMachine.ProductionData = POInfo.FromDataRow(po.Data.Rows[0]);
            sm.SetState(e_ProductionState.CheckPO, $"set PO {orderNo}");

            return Results.Json(new
            {
                success = true,
                message = $"Đã cài PO '{orderNo}', đang khởi tạo...",
                state = sm.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error SetPO: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task<IResult> HandleSelectPO(HttpContext context)
    {
        try
        {
            var body = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
            if (body == null || !body.TryGetValue("orderNo", out var orderNo) || string.IsNullOrWhiteSpace(orderNo))
                return Results.Json(new ApiResponse { Success = false, Message = "orderNo is required" }, statusCode: 400);

            // Kiem tra totalCount > 0 -> da chay hang roi, khong cho doi PO
            if (ProductionStateMachine.Instance.ActiveCounter.TotalCount > 0)
                return Results.Json(new ApiResponse 
                { 
                    Success = false, 
                    Message = "Không thể đổi PO khi đã chạy hàng. Vui lòng Reset PO trước." 
                }, statusCode: 400);

            var po = GProduction.POLoader.GetByOrderNo(orderNo);
            if (!po.IsSuccess || po.Data == null || po.Data.Rows.Count == 0)
                return Results.Json(new ApiResponse { Success = false, Message = $"PO '{orderNo}' không tồn tại" }, statusCode: 404);

            ProductionStateMachine.ProductionData = POInfo.FromDataRow(po.Data.Rows[0]);
            ProductionStateMachine.Instance.SetState(e_ProductionState.CheckPO, $"select PO {orderNo}");

            return Results.Json(new
            {
                success = true,
                message = $"Đã chọn PO '{orderNo}', chuyển sang CheckPO",
                orderNo = ProductionStateMachine.ProductionData.OrderNo,
                state = ProductionStateMachine.Instance.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error selecting PO: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task<IResult> HandleProductionStart(HttpContext context)
    {
        try
        {
            await Task.CompletedTask;
            var sm = ProductionStateMachine.Instance;
            if (sm.CurrentState != e_ProductionState.Ready)
                return Results.Json(new
                {
                    success = false,
                    message = $"Không thể Start từ trạng thái {sm.CurrentState}. Cần ở trạng thái Ready."
                }, statusCode: 400);

            if (ProductionStateMachine.ProductionData != null)
            {
                GProduction.POHistoryManager.RecordStart(
                    ProductionStateMachine.ProductionData.OrderNo,
                    ProductionStateMachine.ProductionData.ProductionDate,
                    "API");
            }

            sm.SetState(e_ProductionState.PushingToDic, "start production");
            return Results.Json(new
            {
                success = true,
                message = "Đã bắt đầu sản xuất",
                state = sm.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error starting production: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task<IResult> HandleProductionStop(HttpContext context)
    {
        try
        {
            await Task.CompletedTask;
            var sm = ProductionStateMachine.Instance;
            if (sm.CurrentState != e_ProductionState.Running && sm.CurrentState != e_ProductionState.Paused)
                return Results.Json(new
                {
                    success = false,
                    message = $"Không thể Stop từ trạng thái {sm.CurrentState}"
                }, statusCode: 400);

            if (ProductionStateMachine.ProductionData != null)
            {
                GProduction.POHistoryManager.RecordEnd(
                    ProductionStateMachine.ProductionData.OrderNo);
            }

            sm.SetState(e_ProductionState.WaitingStop, "stop production");
            sm.SetState(e_ProductionState.Ready, "ready for new PO");

            return Results.Json(new
            {
                success = true,
                message = "Đã dừng sản xuất",
                state = sm.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error stopping production: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task<IResult> HandleProductionReset(HttpContext context)
    {
        try
        {
            await Task.CompletedTask;
            ProductionStateMachine.Instance.ResetForLogout();
            return Results.Json(new
            {
                success = true,
                message = "Đã reset state machine",
                state = ProductionStateMachine.Instance.CurrentState.ToString()
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error resetting production: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    /// <summary>
    /// Cập nhật ProductionDate cho PO hiện tại
    /// PUT /api/po/{orderNo}/production-date
    /// </summary>
    private async Task<IResult> HandleUpdateProductionDate(HttpContext context, string orderNo)
    {
        try
        {
            // Validate orderNo
            if (string.IsNullOrWhiteSpace(orderNo))
                return Results.Json(new ApiResponse { Success = false, Message = "orderNo is required" }, statusCode: 400);

            // Get current PO from state machine
            var sm = ProductionStateMachine.Instance;
            if (ProductionStateMachine.ProductionData == null || ProductionStateMachine.ProductionData.OrderNo != orderNo)
            {
                return Results.Json(new ApiResponse 
                { 
                    Success = false, 
                    Message = $"PO '{orderNo}' không được chọn trong hệ thống." 
                }, statusCode: 400);
            }

            // Parse request body
            var body = await context.Request.ReadFromJsonAsync<UpdateProductionDateRequest>();
            if (body == null || string.IsNullOrWhiteSpace(body.productionDate))
                return Results.Json(new ApiResponse { Success = false, Message = "productionDate is required" }, statusCode: 400);

            // Get username from header or body
            string userName = context.Request.Headers.TryGetValue("X-User-Name", out var userHeader)
                ? userHeader.ToString()
                : body.userName ?? "API";

            // Call state machine to update
            var (success, message) = sm.UpdateProductionDate(body.productionDate, userName);

            if (!success)
                return Results.Json(new ApiResponse { Success = false, Message = message }, statusCode: 400);

            return Results.Json(new ApiResponse { Success = true, Message = message });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error updating production date: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    /// <summary>
    /// Ping endpoint - retained for compatibility. State is now delivered via REST polling
    /// on /api/devices/status, so this endpoint is a no-op success response.
    /// </summary>
    private async Task<IResult> HandleProductionPing(HttpContext context)
        {
            await Task.CompletedTask;
            return Results.Json(new { success = true, at = DateTime.UtcNow });
        }

    /// <summary>
    /// Retry kiểm tra và chạy sản xuất sau khi đã thêm mã vào pool
    /// Chỉ hoạt động khi đang ở trạng thái InsufficientCodes
    /// </summary>
    private async Task<IResult> HandleProductionRetry(HttpContext context)
    {
        try
        {
            var sm = ProductionStateMachine.Instance;
            if (sm.CurrentState != e_ProductionState.InsufficientCodes)
            {
                return Results.Json(new RetryRunResponse
                {   
                    Success = false,
                    Message = $"Chi co the retry khi o trang thai InsufficientCodes. State hien tai: {sm.CurrentState}"
                }, statusCode: 400);
            }

            var result = sm.RetryRunProduction();
            return Results.Json(new RetryRunResponse
            {
                Success = result.success,
                Message = result.message,
                AvailableCodes = result.availableCodes,
                OrderQty = ProductionStateMachine.ProductionData?.OrderQty ?? 0,
                NeededCodes = Math.Max(0, (ProductionStateMachine.ProductionData?.OrderQty ?? 0) - result.availableCodes)
            }, statusCode: result.success ? 200 : 400);
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error in HandleProductionRetry: {ex.Message}", ex);
            return Results.Json(new RetryRunResponse
            {
                Success = false,
                Message = ex.Message
            }, statusCode: 500);
        }
    }

    /// <summary>
    /// Trả về trạng thái tất cả thiết bị: Camera, PLC, Production.
    /// Dùng cho Frontend REST polling thay thế WebSocket state sync.
    /// </summary>
    private IResult HandleGetDevicesStatus(HttpContext context)
    {
        try
        {
            // --- Camera ---
            var cameraReader = OmronCodeReader.Instance;
            var cameraHub = CameraHub.Instance;

            string cameraState;
            if (cameraReader == null)
            {
                cameraState = "NotInitialized";
            }
            else if (cameraReader.Connected)
            {
                cameraState = "Connected";
            }
            else
            {
                cameraState = "Disconnected";
            }

            // --- PLC ---
            var plc = Program.GetPLCMonitor();
            string plcState;
            string? plcIp = null;
            int? plcPort = null;
            if (plc != null)
            {
                plcState = plc.State switch
                {
                    PLCMonitorLite.PLCConnectionState.Connected => "Connected",
                    PLCMonitorLite.PLCConnectionState.Reconnecting => "Reconnecting",
                    _ => "Disconnected"
                };
                plcIp = Global.omronPLC?.PLC_IP;
                plcPort = Global.omronPLC?.PLC_PORT;
            }
            else
            {
                plcState = "NotInitialized";
            }

            // --- Production ---
            var sm = ProductionStateMachine.Instance;

            int _packedCodes = 0;
            int _cartonCount = 0, _closedCartons = 0;
            if (ProductionStateMachine.ProductionData != null)
            {
                _packedCodes = GProduction.PORecordHelper.GetPackedCount(ProductionStateMachine.ProductionData.OrderNo);
                _cartonCount = GProduction.POCarton.GetTotalCartonCount(ProductionStateMachine.ProductionData.OrderNo);
                _closedCartons = GProduction.POCarton.GetClosedCartonCount(ProductionStateMachine.ProductionData.OrderNo);
            }
            int _orderQtySnap = ProductionStateMachine.ProductionData?.OrderQty ?? 0;

            return Results.Json(new
            {
                success = true,
                at = DateTime.UtcNow,

                // Camera: OmronCodeReader (connection) + CameraHub (scan history)
                camera = new
                {
                    state = cameraState,
                    ip = cameraReader?.IP ?? "—",
                    port = cameraReader?.Port ?? 0,
                    connected = cameraReader?.Connected ?? false,
                    lastCode = cameraHub.LastScannedCode,
                    lastCodeAt = cameraHub.LastScannedAt,
                    lastEventAt = cameraHub.LastEventAt,
                    clientCount = cameraHub.ClientCount
                },

                // PLC: PLCMonitorLite state (Global.omronPLC la ket noi PLC duy nhat)
                plc = new
                {
                    state = plcState,
                    ip = plcIp,
                    port = plcPort,
                    connected = plc?.State == PLCMonitorLite.PLCConnectionState.Connected,
                    clientCount = 0
                },

                // Production: full state machine snapshot
                production = new
                {
                    state = sm.CurrentState.ToString(),
                    previousState = sm.PreviousState.ToString(),
                    orderNo = ProductionStateMachine.ProductionData?.OrderNo ?? "",
                    productName = ProductionStateMachine.ProductionData?.ProductName ?? "",
                    productionDate = ProductionStateMachine.ProductionData?.ProductionDate ?? "",
                    orderQty = _orderQtySnap,
                    activeCounter = new
                    {
                        PassTotal = sm.ActiveCounter.PassTotal,
                        FailTotal = sm.ActiveCounter.FailTotal,
                        DuplicateCount = sm.ActiveCounter.DuplicateCount,
                        NotFoundCount = sm.ActiveCounter.NotFoundCount,
                        ReadFailCount = sm.ActiveCounter.ReadFailCount,
                        ErrorCount = sm.ActiveCounter.ErrorCount,
                        TimeoutCount = sm.ActiveCounter.TimeoutCount,
                        TotalCount = sm.ActiveCounter.TotalCount,
                        CartonID = sm.ActiveCounter.CartonID,
                        CartonCode = sm.ActiveCounter.CartonCode
                    },
                    cartonCount = _cartonCount,
                    cartonClosedCount = _closedCartons,
                    itemsInCarton = sm.PackageCounter.PassTotal,
                    cartonCapacity = sm.ActiveCounter.CartonCapacity,
                    progressPercent = _orderQtySnap > 0 ? Math.Round((double)_packedCodes / _orderQtySnap * 100, 2) : 0,
                    hasPO = ProductionStateMachine.ProductionData != null,
                    codesCount = sm.Dictionary_Codes.Count,
                    cartonsCount = sm.Dictionary_Cartons.Count,
                    lastWarning = sm.LastWarning,
                    isAppReady = sm.IsAppReady,
                    isDeviceReady = sm.IsDeviceReady
                }
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting devices status: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }
    #endregion

    #region === PLC RECIPE ===
    private IResult HandlePlcGetActiveRecipe()
    {
        try
        {
            PLCRecipeDb.EnsureCreated();
            var r = PLCRecipeDb.GetActive();
            if (r == null)
                return Results.Json(new { success = true, message = "Chưa có recipe active nào.", data = (PLCRecipe?)null });
            return Results.Json(new { success = true, message = "OK", data = r });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting active recipe: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandlePlcGetAllRecipes()
    {
        try
        {
            PLCRecipeDb.EnsureCreated();
            var list = PLCRecipeDb.GetAll();
            return Results.Json(new { success = true, message = "OK", data = list, count = list.Count });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting recipes: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandlePlcReadRecipeFromDevice()
    {
        try
        {
            var r = Program.GetPLCMonitor().ReadRecipe();
            if (!r.Success)
                return Results.Json(new { success = false, message = r.Error, data = (object?)null });

            return Results.Json(new
            {
                success = true,
                message = "OK",
                data = new
                {
                    delayCamera = r.Value[0],
                    delayReject = r.Value[1],
                    rejectStreng = r.Value[2],
                }
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error reading from PLC: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task HandlePlcSaveRecipe(HttpContext context)
    {
        try
        {
            var body = await JsonSerializer.DeserializeAsync<RecipePayload>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body == null || string.IsNullOrWhiteSpace(body.RecipeName))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = "Thiếu RecipeName." });
                return;
            }

            var username = AuthMiddleware.GetCurrentUser(context)?.Username ?? "Operator";

            PLCRecipeDb.EnsureCreated();
            var r = new PLCRecipe
            {
                Id = body.Id,
                RecipeName = body.RecipeName.Trim(),
                DelayCamera = body.DelayCamera,
                DelayReject = body.DelayReject,
                RejectStreng = body.RejectStreng,
                IsActive = body.IsActive,
                CreatedBy = username,
            };
            var saved = PLCRecipeDb.Save(r);

            // Nếu lưu recipe active thì ghi xuống PLC
            string plcWriteMsg = "";
            if (saved.IsActive)
            {
                var err = Program.GetPLCMonitor().WriteRecipe(saved.DelayCamera, saved.DelayReject, saved.RejectStreng);
                plcWriteMsg = string.IsNullOrEmpty(err) ? "Đã ghi xuống PLC." : $"Lỗi ghi PLC: {err}";
            }

            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = $"Lưu thành công. {plcWriteMsg}",
                data = saved
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error saving recipe: {ex.Message}", ex);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    private async Task HandlePlcSetActiveRecipe(HttpContext context)
    {
        try
        {
            var body = await JsonSerializer.DeserializeAsync<SetActivePayload>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body == null || body.Id <= 0)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = "Thiếu Id." });
                return;
            }

            var ok = PLCRecipeDb.SetActive(body.Id);
            if (!ok)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = $"Không tìm thấy recipe id={body.Id}." });
                return;
            }

            var r = PLCRecipeDb.GetById(body.Id);
            // Ghi xuống PLC
            string plcWriteMsg = "";
            if (r != null)
            {
                var err = Program.GetPLCMonitor().WriteRecipe(r.DelayCamera, r.DelayReject, r.RejectStreng);
                plcWriteMsg = string.IsNullOrEmpty(err) ? "Đã ghi xuống PLC." : $"Lỗi ghi PLC: {err}";
            }

            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = $"Đã đặt active. {plcWriteMsg}",
                data = r
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error set active: {ex.Message}", ex);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    private IResult HandlePlcDeleteRecipe(int id)
    {
        try
        {
            var (ok, msg) = PLCRecipeDb.Delete(id);
            if (!ok)
                return Results.Json(new ApiResponse { Success = false, Message = msg }, statusCode: 400);
            return Results.Json(new { success = true, message = msg });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error delete recipe: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private class RecipePayload
    {
        public int Id { get; set; }
        public string RecipeName { get; set; } = "";
        public int DelayCamera { get; set; }
        public int DelayReject { get; set; }
        public int RejectStreng { get; set; }
        public bool IsActive { get; set; }
    }

    private class SetActivePayload
    {
        public int Id { get; set; }
    }

    private IResult HandlePlcGetRegisters(int recipeId)
    {
        try
        {
            var list = RecipeRegisterDb.GetByRecipe(recipeId);
            return Results.Json(new { success = true, message = "OK", data = list, count = list.Count });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error getting registers: {ex.Message}", ex);
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private async Task HandlePlcSaveRegisters(HttpContext context, int recipeId)
    {
        try
        {
            var body = await JsonSerializer.DeserializeAsync<RegistersPayload>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (body == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = "Body trống." });
                return;
            }

            var saved = RecipeRegisterDb.SaveAll(recipeId, body.Registers ?? new List<RecipeRegister>());
            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = $"Đã lưu {saved.Count} thanh ghi.",
                data = saved,
                count = saved.Count,
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error saving registers: {ex.Message}", ex);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    private async Task HandlePlcReadRegistersFromDevice(HttpContext context, int recipeId)
    {
        try
        {
            var plc = Program.GetPLCMonitor();
            var regs = RecipeRegisterDb.GetByRecipe(recipeId);
            var reads = new List<object>();
            foreach (var r in regs)
            {
                var rr = plc.ReadRegister(r.Address, r.DataType);
                reads.Add(new
                {
                    id = r.Id,
                    name = r.Name,
                    address = r.Address,
                    dataType = r.DataType,
                    ok = rr.Success,
                    value = rr.Value,
                    error = rr.Success ? "" : rr.Error,
                });
            }
            await context.Response.WriteAsJsonAsync(new { success = true, message = "OK", data = reads });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error reading registers from PLC: {ex.Message}", ex);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    private async Task HandlePlcWriteRegistersToDevice(HttpContext context, int recipeId)
    {
        try
        {
            var plc = Program.GetPLCMonitor();
            var body = await JsonSerializer.DeserializeAsync<RegistersWritePayload>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var regs = RecipeRegisterDb.GetByRecipe(recipeId);
            var results = new List<object>();
            int okCount = 0, errCount = 0;
            foreach (var r in regs)
            {
                string? val = body?.Values != null && body.Values.TryGetValue(r.Id.ToString(), out var v)
                    ? v
                    : (body?.Values != null && body.Values.TryGetValue(r.Name, out var vn) ? vn : r.DefaultValue);
                var err = plc.WriteRegister(r.Address, r.DataType, val ?? r.DefaultValue);
                results.Add(new
                {
                    id = r.Id,
                    name = r.Name,
                    address = r.Address,
                    ok = string.IsNullOrEmpty(err),
                    error = err,
                });
                if (string.IsNullOrEmpty(err)) okCount++; else errCount++;
            }
            await context.Response.WriteAsJsonAsync(new
            {
                success = errCount == 0,
                message = $"OK: {okCount}, lỗi: {errCount}.",
                data = results,
            });
        }
        catch (Exception ex)
        {
            LogError("GProjectApiServer", $"Error writing registers to PLC: {ex.Message}", ex);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    private class RegistersPayload
    {
        public List<RecipeRegister>? Registers { get; set; }
    }

    private class RegistersWritePayload
    {
        public Dictionary<string, string>? Values { get; set; }
    }
    #endregion

    #region AWS IoT Handlers

    /// <summary>Lấy trạng thái AWS IoT</summary>
    private IResult HandleGetAWSStatus(HttpContext context)
    {
        var stateMachine = ProductionStateMachine.Instance;
        return Results.Json(new
        {
            success = true,
            status = stateMachine.AWSStatus.ToString(),
            pendingCount = stateMachine.AWSPendingCount,
            isEnabled = G.AWS.Enabled,
            endpoint = G.AWS.Enabled ? G.AWS.Endpoint : null
        });
    }

    /// <summary>Kết nối AWS IoT</summary>
    private async Task<IResult> HandleAWConnect(HttpContext context)
    {
        try
        {
            var stateMachine = ProductionStateMachine.Instance;
            var success = await stateMachine.ConnectAWSAsync();
            return Results.Json(new
            {
                success,
                message = success ? "Kết nối AWS IoT thành công" : "Kết nối AWS IoT thất bại",
                status = stateMachine.AWSStatus.ToString()
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[AWS] Lỗi kết nối");
            return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
        }
    }

    /// <summary>Ngắt kết nối AWS IoT</summary>
    private async Task<IResult> HandleAWSDisconnect(HttpContext context)
    {
        try
        {
            var stateMachine = ProductionStateMachine.Instance;
            await stateMachine.DisconnectAWSAsync();
            return Results.Json(new
            {
                success = true,
                message = "Đã ngắt kết nối AWS IoT",
                status = stateMachine.AWSStatus.ToString()
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[AWS] Lỗi ngắt kết nối");
            return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
        }
    }

    /// <summary>Cập nhật cấu hình AWS</summary>
    private IResult HandleAWSUpdateConfig(HttpContext context)
    {
        try
        {
            var config = context.Request.ReadFromJsonAsync<AWSIoTConfig>().GetAwaiter().GetResult();
            if (config == null)
                return Results.Json(new { success = false, message = "Invalid config" }, statusCode: 400);

            G.AWS = config;

            // Reinitialize AWS client nếu cần
            var stateMachine = ProductionStateMachine.Instance;
            if (G.AWS.Enabled)
            {
                stateMachine.InitAWS(config);
            }

            return Results.Json(new
            {
                success = true,
                message = "Đã cập nhật cấu hình AWS",
                config = new
                {
                    enabled = G.AWS.Enabled,
                    devMode = G.AWS.DevMode,
                    endpoint = G.AWS.Endpoint,
                    clientId = G.AWS.ClientId,
                    thingName = G.AWS.ThingName,
                    autoSend = G.AWS.AutoSend
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[AWS] Lỗi cập nhật config");
            return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
        }
    }

    /// <summary>Lấy cấu hình AWS</summary>
    private IResult HandleAWSGetConfig(HttpContext context)
    {
        return Results.Json(new
        {
            success = true,
            config = new
            {
                enabled = G.AWS.Enabled,
                devMode = G.AWS.DevMode,
                endpoint = G.AWS.Endpoint,
                clientId = G.AWS.ClientId,
                thingName = G.AWS.ThingName,
                autoSend = G.AWS.AutoSend,
                // Không trả về certificate paths/password vì bảo mật
                hasRootCAPath = !string.IsNullOrEmpty(G.AWS.RootCAPath),
                hasClientCertPath = !string.IsNullOrEmpty(G.AWS.ClientCertPath)
            }
        });
    }

    /// <summary>Test publish message lên AWS</summary>
    private async Task<IResult> HandleAWSTestPublish(HttpContext context)
    {
        try
        {
            var stateMachine = ProductionStateMachine.Instance;
            if (stateMachine.AWSStatus != AWSIoTStatus.Connected)
            {
                return Results.Json(new { success = false, message = "Chưa kết nối AWS IoT" }, statusCode: 400);
            }

            var testPayload = new AWSSendPayload
            {
                message_id = $"TEST-{Guid.NewGuid():N}",
                orderNo = "TEST",
                uniqueCode = "TEST-CODE-001",
                gtin = "1234567890123",
                cartonCode = "TEST-CTN-001",
                status = 1,
                activate_datetime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                production_date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                thing_name = G.AWS.ThingName
            };

            // Publish trực tiếp qua AWS client
            var (success, message) = await stateMachine.AWSClientInternal!.PublishAsync(
                G.AWS.PublishTopic,
                System.Text.Json.JsonSerializer.Serialize(testPayload)
            );

            return Results.Json(new
            {
                success,
                message = message,
                payload = testPayload
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[AWS] Lỗi test publish");
            return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
        }
    }

    #endregion

    private void LogInfo(string source, string message)
    {
        Log.Information("[{Source}] {Message}", source, message);
    }

    private void LogError(string source, string message, Exception? ex = null)
    {
        if (ex != null)
            Log.Error(ex, "[{Source}] {Message}", source, message);
        else
            Log.Error("[{Source}] {Message}", source, message);
    }

    public void Dispose() => StopAsync().GetAwaiter().GetResult();

    private class HealthResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "OK";
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ApiResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
        [JsonPropertyName("count")] public int Count { get; set; }
    }

    private class LoginRequest
    {
        [JsonPropertyName("username")] public string Username { get; set; } = "";
        [JsonPropertyName("password")] public string Password { get; set; } = "";
    }
    private class CreateUserRequest
    {
        [JsonPropertyName("username")] public string Username { get; set; } = "";
        [JsonPropertyName("password")] public string Password { get; set; } = "";
        [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
        [JsonPropertyName("role")] public string Role { get; set; } = "";
    }
    private class UpdateUserRequest
    {
        [JsonPropertyName("password")] public string? Password { get; set; }
        [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
        [JsonPropertyName("role")] public string? Role { get; set; }
        [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
    }
    private class CreatePoolRequest { [JsonPropertyName("poolName")] public string PoolName { get; set; } = ""; }
    private class AddCodeManualRequest
    {
        [JsonPropertyName("poolName")] public string PoolName { get; set; } = "";
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("status")] public int? Status { get; set; }
        [JsonPropertyName("batchID")] public string? BatchID { get; set; }
        [JsonPropertyName("note")] public string? Note { get; set; }
        [JsonPropertyName("userName")] public string? UserName { get; set; }
    }
    private class AddCodeReaderRequest
    {
        [JsonPropertyName("poolName")] public string PoolName { get; set; } = "";
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("batchID")] public string BatchID { get; set; } = "";
        [JsonPropertyName("note")] public string? Note { get; set; }
    }
    private class ImportCodesFromFileRequest
    {
        [JsonPropertyName("poolName")] public string PoolName { get; set; } = "";
        [JsonPropertyName("csvPath")] public string CsvPath { get; set; } = "";
        [JsonPropertyName("userName")] public string? UserName { get; set; }
        [JsonPropertyName("createID")] public string CreateID { get; set; } = "";
        [JsonPropertyName("codeColumn")] public string? CodeColumn { get; set; }
        [JsonPropertyName("noteColumn")] public string? NoteColumn { get; set; }
        [JsonPropertyName("note")] public string? Note { get; set; }
    }
    private class ImportCodesFromContentRequest
    {
        [JsonPropertyName("poolName")] public string PoolName { get; set; } = "";
        [JsonPropertyName("csvContent")] public string CsvContent { get; set; } = "";
        [JsonPropertyName("userName")] public string? UserName { get; set; }
        [JsonPropertyName("createID")] public string CreateID { get; set; } = "";
        [JsonPropertyName("note")] public string? Note { get; set; }
    }

    private class UpdateProductionDateRequest
    {
        [JsonPropertyName("productionDate")] public string productionDate { get; set; } = "";
        [JsonPropertyName("userName")] public string? userName { get; set; }
    }
}