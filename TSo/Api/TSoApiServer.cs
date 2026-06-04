using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using TSo.Models;
using TSo.Services;
using TSo.Configs;

namespace TSo.Api;

public class TSoApiServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly AuthService _authService;
    private readonly QRService _qrService;
    private readonly PLCService _plcService;
    private readonly CameraService _cameraService;
    private readonly BatchService _batchService;
    private readonly ScannerService _scannerService;
    private readonly Logger _logger;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private Task? _processQueueTask;
    private CancellationTokenSource? _processCts;
    private bool _disposed;

    public TSoApiServer(
        AuthService authService,
        QRService qrService,
        PLCService plcService,
        CameraService cameraService,
        BatchService batchService,
        ScannerService scannerService,
        Logger logger,
        string prefix)
    {
        _authService = authService;
        _qrService = qrService;
        _plcService = plcService;
        _cameraService = cameraService;
        _batchService = batchService;
        _scannerService = scannerService;
        _logger = logger;
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }

    public void Start()
    {
        _listener.Start();
        _logger.Log("System", LogType.System, $"TSo API server started on {_listener.Prefixes.FirstOrDefault()}");

        _cts = new CancellationTokenSource();
        _serverTask = Task.Run(() => ServerLoop(_cts.Token));

        _processCts = new CancellationTokenSource();
        _processQueueTask = Task.Run(() => ProcessQueueLoop(_processCts.Token));
    }

    private async Task ServerLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var ctx = await _listener.GetContextAsync();
                if (ctx.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(() => HandleWebSocket(ctx));
                }
                else
                {
                    _ = Task.Run(() => HandleRequest(ctx));
                }
            }
            catch (HttpListenerException) { break; }
            catch (ObjectDisposedException) { break; }
            catch { }
        }
    }

    private async Task HandleRequest(HttpListenerContext ctx)
    {
        var path = ctx.Request.Url?.AbsolutePath ?? "/";
        var method = ctx.Request.HttpMethod;

        try
        {
            int statusCode = 200;
            object? response = null;

            if (path == "/" || path == "/api")
            {
                response = new { name = "TSo API", version = "1.0", status = "running" };
            }
            else if (path == "/api/auth/login" && method == "POST")
            {
                response = await HandleLogin(ctx);
            }
            else if (path == "/api/auth/logout" && method == "POST")
            {
                response = await HandleLogout(ctx);
            }
            else if (path == "/api/auth/session" && method == "GET")
            {
                (response, statusCode) = await HandleGetSession(ctx);
            }
            else if (path == "/api/auth/2fa/qrurl" && method == "GET")
            {
                (response, statusCode) = await Handle2FAQrUrl(ctx);
            }
            else if (path == "/api/batch/current" && method == "GET")
            {
                response = await HandleGetCurrentBatch(ctx);
            }
            else if (path == "/api/batch/change" && method == "POST")
            {
                (response, statusCode) = await HandleChangeBatch(ctx);
            }
            else if (path == "/api/batch/list" && method == "GET")
            {
                response = await HandleGetBatchList(ctx);
            }
            else if (path == "/api/batch/history" && method == "GET")
            {
                response = await HandleGetBatchHistory(ctx);
            }
            else if (path == "/api/qr/activate" && method == "POST")
            {
                response = await HandleQRActivate(ctx);
            }
            else if (path == "/api/qr/manual-add" && method == "POST")
            {
                (response, statusCode) = await HandleQRManualAdd(ctx);
            }
            else if (path == "/api/qr/search" && method == "GET")
            {
                (response, statusCode) = await HandleQRSearch(ctx);
            }
            else if (path == "/api/qr/deactivate" && method == "POST")
            {
                (response, statusCode) = await HandleQRDeactivate(ctx);
            }
            else if (path == "/api/dashboard" && method == "GET")
            {
                response = HandleDashboard();
            }
            else if (path == "/api/counters" && method == "GET")
            {
                response = HandleGetCounters();
            }
            else if (path == "/api/status/devices" && method == "GET")
            {
                response = await HandleGetDeviceStatus(ctx);
            }
            else if (path == "/api/status/system" && method == "GET")
            {
                response = await HandleGetSystemStatus(ctx);
            }
            else if (path == "/api/system/deactivate" && method == "POST")
            {
                (response, statusCode) = await HandleSystemDeactivate(ctx);
            }
            else if (path == "/api/system/reactivate" && method == "POST")
            {
                (response, statusCode) = await HandleSystemReactivate(ctx);
            }
            else if (path == "/api/system/reset-counters" && method == "POST")
            {
                (response, statusCode) = await HandleResetCounters(ctx);
            }
            else if (path == "/api/system/clear-errors" && method == "POST")
            {
                (response, statusCode) = await HandleClearErrors(ctx);
            }
            else if (path == "/api/logs" && method == "GET")
            {
                response = await HandleGetLogs(ctx);
            }
            else if (path == "/api/config" && method == "GET")
            {
                response = HandleGetConfig();
            }
            else if (path == "/api/config" && method == "POST")
            {
                (response, statusCode) = await HandleSaveConfig(ctx);
            }
            else
            {
                statusCode = 404;
                response = ApiResponse.Fail("Endpoint not found");
            }

            if (response != null)
                await WriteJson(ctx, response, statusCode);
            else
                await WriteJson(ctx, ApiResponse.Fail("Unhandled"), statusCode);
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, $"Request error: {path}", ex.Message);
            ctx.Response.StatusCode = 500;
            await WriteJson(ctx, ApiResponse.Fail("Internal error"));
        }
    }

    private async Task HandleWebSocket(HttpListenerContext ctx)
    {
        try
        {
            var wsCtx = await ctx.AcceptWebSocketAsync(null);
            var buffer = new byte[4096];

            while (wsCtx.WebSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await wsCtx.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    await wsCtx.WebSocket.CloseAsync((WebSocketCloseStatus)1000, "", CancellationToken.None);
                    break;
                }

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var msg = JsonConvert.DeserializeObject<WsIncomingMessage>(text);
                        if (msg != null)
                        {
                            var response = ProcessWsMessage(msg);
                            var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                            await wsCtx.WebSocket.SendAsync(
                                new ArraySegment<byte>(responseBytes),
                                System.Net.WebSockets.WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    private object ProcessWsMessage(WsIncomingMessage msg)
    {
        return msg.type switch
        {
            "ping" => new { type = "pong", timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            "dashboard" => new
            {
                type = "dashboard",
                data = new DashboardResponse
                {
                    CurrentBatch = GlobalState.CurrentBatch,
                    CameraCounters = GlobalState.CameraCounters,
                    PLCCounters = GlobalState.PLCCounters,
                    DeviceStatus = GlobalState.DeviceStatus,
                    SystemStatus = GlobalState.SystemStatus,
                    ProductionPerHour = GlobalState.ProductionPerHour,
                    ActiveCodesTotal = GlobalState.ActiveCodesTotal
                }
            },
            "counters" => new
            {
                type = "counters",
                data = new
                {
                    Camera = GlobalState.CameraCounters,
                    PLC = GlobalState.PLCCounters,
                    ProductionPerHour = GlobalState.ProductionPerHour
                }
            },
            _ => new { type = "error", message = "Unknown message type" }
        };
    }

    private async Task ProcessQueueLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                while (GlobalState.QueueRecord.TryDequeue(out var record))
                    Database.QRDatabaseHelper.AddOrActivateCode(record, AppConfigs.QRDatadbPath);
                while (GlobalState.QueueActive.TryDequeue(out var activeRecord))
                    Database.QRDatabaseHelper.AddActiveCodeUnique(activeRecord, AppConfigs.ActiveUniqueDbPath);

                UpdateProductionSpeed();
                UpdateSystemStatus();
                await Task.Delay(100, ct);
            }
            catch (OperationCanceledException) { break; }
            catch { }
        }
    }

    private void UpdateProductionSpeed()
    {
        if (GlobalState.Config.Production_Speed_Mode == 1)
        {
            var resetTimeout = GlobalState.Config.Production_Speed_Reset_Timeout;
            if (resetTimeout <= 0) resetTimeout = 30;
            var currentMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var diff = (currentMs - GlobalState.LastProductTimestampMs) / 1000.0;
            if (GlobalState.LastProductTimestampMs > 0 && diff > resetTimeout)
                GlobalState.ProductionPerHour = 0;
        }
        else
        {
            var timestampMs = DateTimeOffset.UtcNow.AddMinutes(-15).ToUnixTimeMilliseconds();
            GlobalState.ProductionPerHour = (int)Database.QRDatabaseHelper.GetHourlyProduction(timestampMs);
        }
    }

    private void UpdateSystemStatus()
    {
        var deactive = _plcService.ReadDeactiveState();
        GlobalState.SystemStatus.IsDeactive = deactive == 1;
        GlobalState.SystemStatus.IsAuthenticated = GlobalState.IsAuthenticated;
        GlobalState.SystemStatus.CurrentUser = GlobalState.CurrentSession?.Username ?? "";
        GlobalState.SystemStatus.CurrentRole = GlobalState.CurrentSession?.Role ?? "";
        GlobalState.SystemStatus.LastUpdate = DateTime.UtcNow;
        GlobalState.SystemStatus.State = deactive == 1 ? "Deactive" :
                                        GlobalState.IsAuthenticated ? "Ready" : "Login";
    }

    public void Stop()
    {
        _cts?.Cancel();
        _processCts?.Cancel();
        _listener.Stop();
        _logger.Log("System", LogType.System, "TSo API server stopped");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _cts?.Dispose();
        _processCts?.Dispose();
    }

    #region Handlers

    private async Task<object> HandleLogin(HttpListenerContext ctx)
    {
        var req = await ParseBody<LoginRequest>(ctx);
        if (req == null) return ApiResponse.Fail("Invalid request body");
        return _authService.Login(req);
    }

    private async Task<object> HandleLogout(HttpListenerContext ctx)
    {
        var token = GetToken(ctx);
        _authService.Logout(token);
        return ApiResponse.Ok("Logged out");
    }

    private async Task<(object response, int statusCode)> HandleGetSession(HttpListenerContext ctx)
    {
        var session = GetSession(ctx);
        if (session == null) return (ApiResponse.Fail("Not authenticated"), 401);
        return (ApiResponse<SessionInfo>.Ok(session), 200);
    }

    private async Task<(object response, int statusCode)> Handle2FAQrUrl(HttpListenerContext ctx)
    {
        var username = ctx.Request.QueryString["username"] ?? "";
        if (string.IsNullOrEmpty(username)) return (ApiResponse.Fail("Username required"), 400);
        var url = _authService.Get2FAQrUrl(username);
        return (ApiResponse<string>.Ok(url ?? "", "2FA QR URL"), 200);
    }

    private async Task<object> HandleGetCurrentBatch(HttpListenerContext ctx)
    {
        _batchService.GetCurrentBatch();
        return ApiResponse<BatchInfo>.Ok(GlobalState.CurrentBatch);
    }

    private async Task<(object response, int statusCode)> HandleChangeBatch(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);

        var req = await ParseBody<BatchChangeRequest>(ctx);
        if (req == null) return (ApiResponse.Fail("Invalid body"), 400);

        var batch = _batchService.ChangeBatch(req.BatchCode, req.Barcode, session.Username);
        return (ApiResponse<BatchInfo>.Ok(batch, "Batch changed"), 200);
    }

    private async Task<object> HandleGetBatchList(HttpListenerContext ctx)
    {
        var list = _batchService.GetBatchListFromExcel();
        return ApiResponse<BatchListResponse>.Ok(list);
    }

    private async Task<object> HandleGetBatchHistory(HttpListenerContext ctx)
    {
        var limit = int.TryParse(ctx.Request.QueryString["limit"] ?? "", out var l) ? l : 50;
        var history = _batchService.GetBatchHistory(limit);
        return ApiResponse<List<BatchInfo>>.Ok(history);
    }

    private async Task<object> HandleQRActivate(HttpListenerContext ctx)
    {
        var req = await ParseBody<QRActivateRequest>(ctx);
        if (req == null) return ApiResponse.Fail("Invalid body");

        var session = GetSession(ctx);
        var result = _qrService.ProcessCameraQR(req.QRContent, session?.Username);

        if (result.Status == e_Production_Status.Pass)
            _plcService.SendRejectResult(0);
        else
            _plcService.SendRejectResult(1);

        return new QRActivateResponse
        {
            Success = result.Success,
            Status = result.Status,
            QRContent = result.QRContent,
            Reason = result.Reason,
            RecordId = result.RecordId
        };
    }

    private async Task<(object response, int statusCode)> HandleQRManualAdd(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);

        var req = await ParseBody<QRActivateRequest>(ctx);
        if (req == null) return (ApiResponse.Fail("Invalid body"), 400);

        var result = _qrService.ProcessManualAdd(req.QRContent, session.Username);
        return (new QRActivateResponse
        {
            Success = result.Success,
            Status = result.Status,
            QRContent = result.QRContent,
            Reason = result.Reason,
            RecordId = result.RecordId
        }, 200);
    }

    private async Task<(object response, int statusCode)> HandleQRSearch(HttpListenerContext ctx)
    {
        var qr = ctx.Request.QueryString["code"] ?? "";
        if (string.IsNullOrEmpty(qr)) return (ApiResponse.Fail("QR code required"), 400);

        var resp = _qrService.Search(qr);
        return (ApiResponse<QRSearchResponse>.Ok(resp), 200);
    }

    private async Task<(object response, int statusCode)> HandleQRDeactivate(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);

        var req = await ParseBody<QRActivateRequest>(ctx);
        if (req == null) return (ApiResponse.Fail("Invalid body"), 400);

        var reason = ctx.Request.QueryString["reason"] ?? "Manual deactivation";
        var success = _qrService.DeactivateQR(req.QRContent, reason, session.Username);
        return (success ? ApiResponse.Ok("Deactivated") : ApiResponse.Fail("Failed to deactivate"), success ? 200 : 400);
    }

    private object HandleDashboard()
    {
        return new DashboardResponse
        {
            CurrentBatch = GlobalState.CurrentBatch,
            CameraCounters = GlobalState.CameraCounters,
            PLCCounters = GlobalState.PLCCounters,
            DeviceStatus = GlobalState.DeviceStatus,
            SystemStatus = GlobalState.SystemStatus,
            ProductionPerHour = GlobalState.ProductionPerHour,
            ActiveCodesTotal = GlobalState.ActiveCodesTotal
        };
    }

    private object HandleGetCounters()
    {
        return new
        {
            Camera = GlobalState.CameraCounters,
            PLC = GlobalState.PLCCounters,
            ProductionPerHour = GlobalState.ProductionPerHour,
            ActiveCodesTotal = GlobalState.ActiveCodesTotal
        };
    }

    private async Task<object> HandleGetDeviceStatus(HttpListenerContext ctx)
    {
        return ApiResponse<DeviceStatus>.Ok(GlobalState.DeviceStatus);
    }

    private async Task<object> HandleGetSystemStatus(HttpListenerContext ctx)
    {
        return ApiResponse<SystemStatus>.Ok(GlobalState.SystemStatus);
    }

    private async Task<(object response, int statusCode)> HandleSystemDeactivate(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);
        if (session.Role != "Admin") return (ApiResponse.Fail("Admin role required"), 403);

        var success = _plcService.SendRejectResult(1);
        GlobalState.SystemStatus.IsDeactive = true;
        GlobalState.SystemStatus.State = "Deactive";
        _logger.Log(session.Username, LogType.UserAction, "System deactivated via API");
        return (success ? ApiResponse.Ok("System deactivated") : ApiResponse.Fail("PLC write failed"), success ? 200 : 500);
    }

    private async Task<(object response, int statusCode)> HandleSystemReactivate(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);
        if (session.Role != "Admin") return (ApiResponse.Fail("Admin role required"), 403);

        var success = _plcService.SendRejectResult(0);
        GlobalState.SystemStatus.IsDeactive = false;
        GlobalState.SystemStatus.State = "Ready";
        _logger.Log(session.Username, LogType.UserAction, "System reactivated via API");
        return (success ? ApiResponse.Ok("System reactivated") : ApiResponse.Fail("PLC write failed"), success ? 200 : 500);
    }

    private async Task<(object response, int statusCode)> HandleResetCounters(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);

        var success = _plcService.ResetCounters();
        _logger.Log(session.Username, LogType.UserAction, "PLC counters reset via API");
        return (success ? ApiResponse.Ok("Counters reset") : ApiResponse.Fail("PLC write failed"), success ? 200 : 500);
    }

    private async Task<(object response, int statusCode)> HandleClearErrors(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);

        var success = _plcService.ClearErrors();
        GlobalState.AlarmCount = 0;
        return (success ? ApiResponse.Ok("Errors cleared") : ApiResponse.Fail("PLC write failed"), success ? 200 : 500);
    }

    private async Task<object> HandleGetLogs(HttpListenerContext ctx)
    {
        var count = int.TryParse(ctx.Request.QueryString["count"] ?? "", out var c) ? c : 100;
        var logs = _logger.GetRecentLogs(count);
        return ApiResponse<List<ActivityLog>>.Ok(logs);
    }

    private object HandleGetConfig()
    {
        return new
        {
            LineName = GlobalState.Config.Line_Name,
            PLC_IP = GlobalState.Config.PLC_IP,
            PLC_Port = GlobalState.Config.PLC_Port,
            Camera_01_IP = GlobalState.Config.Camera_01_IP,
            Camera_01_Port = GlobalState.Config.Camera_01_Port,
            Handheld_COM_Port = GlobalState.Config.Handheld_COM_Port,
            Data_Mode = GlobalState.Config.Data_Mode,
            AppTwoFA_Enabled = GlobalState.Config.AppTwoFA_Enabled,
            Server_Host = GlobalState.Config.Server_Host
        };
    }

    private async Task<(object response, int statusCode)> HandleSaveConfig(HttpListenerContext ctx)
    {
        var session = RequireAuth(ctx);
        if (session == null) return (ApiResponse.Fail("Auth required"), 401);

        GlobalState.Config.Save();
        return (ApiResponse.Ok("Config saved"), 200);
    }

    #endregion

    #region Helpers

    private SessionInfo? GetSession(HttpListenerContext ctx)
    {
        var token = GetToken(ctx);
        return _authService.ValidateToken(token);
    }

    private SessionInfo? RequireAuth(HttpListenerContext ctx)
    {
        var session = GetSession(ctx);
        return session;
    }

    private static string? GetToken(HttpListenerContext ctx)
    {
        var auth = ctx.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return auth["Bearer ".Length..].Trim();
        return ctx.Request.QueryString["token"];
    }

    private static async Task<T?> ParseBody<T>(HttpListenerContext ctx) where T : class
    {
        try
        {
            using var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(body);
        }
        catch { return null; }
    }

    private static async Task WriteJson(HttpListenerContext ctx, object data, int statusCode = 200)
    {
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";
        ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
        ctx.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        ctx.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

        if (ctx.Request.HttpMethod == "OPTIONS")
        {
            ctx.Response.StatusCode = 204;
            return;
        }

        var json = JsonConvert.SerializeObject(data, Formatting.None,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
    }

    private class WsIncomingMessage
    {
        public string type { get; set; } = "";
        public object? data { get; set; }
    }

    #endregion
}
