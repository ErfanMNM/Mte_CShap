using System.Text.Json.Serialization;
using System.Data.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTManager.Audit;
using VNQR.Infrastructure;

namespace VNQR.Helpers
{
    /// <summary>Audit action codes for PO management.</summary>
    public enum POActions
    {
        Create,
        Delete,
        Update,
        LoadCodes,
        StartProduction,
        StopProduction,
    }

    /// <summary>
    /// Dữ liệu nhận khi tạo PO mới qua API.
    /// </summary>
    public class CreatePORequest
    {
        [JsonPropertyName("orderNo")]
        public string OrderNo { get; set; } = "";

        [JsonPropertyName("site")]
        public string Site { get; set; } = "";

        [JsonPropertyName("factory")]
        public string Factory { get; set; } = "";

        [JsonPropertyName("productionLine")]
        public string ProductionLine { get; set; } = "";

        [JsonPropertyName("productionDate")]
        public string ProductionDate { get; set; } = "";

        [JsonPropertyName("shift")]
        public string Shift { get; set; } = "";

        [JsonPropertyName("orderQty")]
        public int OrderQty { get; set; }

        [JsonPropertyName("lotNumber")]
        public string LotNumber { get; set; } = "";

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; } = "";

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = "";

        [JsonPropertyName("gtin")]
        public string Gtin { get; set; } = "";

        [JsonPropertyName("customerOrderNo")]
        public string CustomerOrderNo { get; set; } = "";

        [JsonPropertyName("uom")]
        public string Uom { get; set; } = "";

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = "API";

        [JsonPropertyName("autoLoadCodes")]
        public bool AutoLoadCodes { get; set; } = true;
    }

    /// <summary>
    /// Phản hồi từ server khi tạo PO.
    /// </summary>
    public class CreatePOResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("orderNo")]
        public string? OrderNo { get; set; }

        [JsonPropertyName("loadedCodesCount")]
        public int LoadedCodesCount { get; set; }
    }

    /// <summary>
    /// Health check response.
    /// </summary>
    public class HealthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "OK";

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [JsonPropertyName("appState")]
        public string AppState { get; set; } = "";
    }

    // ================== DATAPOOL REQUEST/RESPONSE ==================

    /// <summary>
    /// Request body for adding single code manually.
    /// </summary>
    public class AddCodeManualRequest
    {
        [JsonPropertyName("poolName")]
        public string PoolName { get; set; } = "";

        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("batchID")]
        public string BatchID { get; set; } = "";

        [JsonPropertyName("note")]
        public string Note { get; set; } = "";

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = "API";
    }

    /// <summary>
    /// Request body for adding code from reader (camera).
    /// </summary>
    public class AddCodeReaderRequest
    {
        [JsonPropertyName("poolName")]
        public string PoolName { get; set; } = "";

        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("batchID")]
        public string BatchID { get; set; } = "";

        [JsonPropertyName("note")]
        public string Note { get; set; } = "";
    }

    /// <summary>
    /// Request body for creating a new pool.
    /// </summary>
    public class CreatePoolRequest
    {
        [JsonPropertyName("poolName")]
        public string PoolName { get; set; } = "";
    }

    /// <summary>
    /// Request body for importing codes from CSV file.
    /// </summary>
    public class ImportCodesFromFileRequest
    {
        [JsonPropertyName("poolName")]
        public string PoolName { get; set; } = "";

        [JsonPropertyName("csvPath")]
        public string CsvPath { get; set; } = "";

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = "API";

        [JsonPropertyName("createID")]
        public string CreateID { get; set; } = "";

        [JsonPropertyName("codeColumn")]
        public string CodeColumn { get; set; } = "Code";

        [JsonPropertyName("noteColumn")]
        public string NoteColumn { get; set; } = "";

        [JsonPropertyName("note")]
        public string Note { get; set; } = "";
    }

    /// <summary>
    /// Generic API response.
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// Response for PO delete operation.
    /// </summary>
    public class DeletePOResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("canDelete")]
        public bool CanDelete { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";
    }

    /// <summary>
    /// Response for PO delete-check (can-delete status).
    /// </summary>
    public class CheckDeleteResponse
    {
        [JsonPropertyName("canDelete")]
        public bool CanDelete { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";

        [JsonPropertyName("usedCodesCount")]
        public int UsedCodesCount { get; set; }

        [JsonPropertyName("totalCodesCount")]
        public int TotalCodesCount { get; set; }
    }

    /// <summary>
    /// Response for audit log query.
    /// </summary>
    public class AuditLogResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("totalCount")]
        public long TotalCount { get; set; }

        [JsonPropertyName("data")]
        public List<AuditLogEntry> Data { get; set; } = new();
    }

    public class AuditLogEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("timeISO")]
        public string? TimeISO { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("jsonParams")]
        public string? JsonParams { get; set; }
    }

    /// <summary>
    /// REST API Server cho phép tạo PO từ bên ngoài.
    /// Chạy trên background thread, không ảnh hưởng luồng chính.
    /// </summary>
    public class POApiServer : IDisposable
    {
        private WebApplication? _app;
        private readonly int _port;
        private readonly string _host;
        private readonly ILogger? _logger;
        private readonly Action<string, string>? _onLog;

        /// <summary>Audit logger for PO operations.</summary>
        private readonly LogHelper<POActions>? _auditLog;

        /// <summary>Audit DB path.</summary>
        private static readonly string AuditDbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "logs", "po_audit.db");

        /// <summary>
        /// Tạo POApiServer.
        /// </summary>
        /// <param name="port">Port lắng nghe (mặc định 9999).</param>
        /// <param name="host">Host bind (mặc định 0.0.0.0).</param>
        /// <param name="logger">ILogger để ghi log.</param>
        /// <param name="onLog">Callback log (source, message). Nếu null, dùng Console.</param>
        public POApiServer(int port = 9999, string host = "0.0.0.0", ILogger? logger = null, Action<string, string>? onLog = null)
        {
            _port = port;
            _host = host;
            _logger = logger;
            _onLog = onLog;

            try
            {
                _auditLog = new LogHelper<POActions>(AuditDbPath);
                Log("POApiServer", $"Audit log initialized at: {AuditDbPath}");
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"WARNING: Failed to init audit log: {ex.Message}");
            }
        }

        /// <summary>
        /// Khởi động API Server bất đồng bộ.
        /// Server chạy trên background thread.
        /// </summary>
        public async Task StartAsync()
        {
            var builder = WebApplication.CreateBuilder();

            // Enable CORS for browser clients (e.g. iot-scada-admin-panel on localhost:3001)
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
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

            // Enable CORS - must be before MapXxx calls
            _app.UseCors();

            // Health check
            _app.MapGet("/api/health", HandleHealth);

            // PO endpoints
            _app.MapPost("/api/po", HandleCreatePO);
            _app.MapGet("/api/po/{orderNo}", HandleGetPO);
            _app.MapGet("/api/po/list/all", HandleGetAllPO);
            _app.MapDelete("/api/po/{orderNo}", HandleDeletePO);
            _app.MapGet("/api/po/{orderNo}/can-delete", HandleCanDeletePO);
            _app.MapGet("/api/audit", HandleGetAuditLogs);
            _app.MapGet("/api/audit/{code}", HandleGetAuditLogsByCode);

            // DataPool endpoints
            _app.MapPost("/api/datapool/add", HandleDataPoolAddCode);
            _app.MapPost("/api/datapool/add-reader", HandleDataPoolAddFromReader);
            _app.MapPost("/api/datapool/import-file", HandleDataPoolImportFromFile);
            _app.MapGet("/api/datapool/{poolName}/codes", HandleDataPoolGetCodes);
            _app.MapGet("/api/datapool/{poolName}/code/{code}", HandleDataPoolGetCode);
            _app.MapPut("/api/datapool/{poolName}/code/{code}/status", HandleDataPoolUpdateStatus);
            _app.MapDelete("/api/datapool/{poolName}/code/{code}", HandleDataPoolDeleteCode);
            _app.MapGet("/api/datapool/pools", HandleDataPoolListPools);
            _app.MapPost("/api/datapool/pools", HandleDataPoolCreatePool);

            Log("POApiServer", $"DataPool endpoints: POST /api/datapool/add, POST /api/datapool/add-reader, POST /api/datapool/import-file, POST /api/datapool/pools");

            Log("POApiServer", $"Started on http://{_host}:{_port}");
            Log("POApiServer", $"Endpoints: POST /api/po, GET /api/po/{{orderNo}}, GET /api/po/list/all");
            Log("POApiServer", $"CORS enabled - allowing any origin/method/header");
            Log("POApiServer", $"Full API available at http://{{your_ip}}:{_port}/api/health");

            await _app.StartAsync();
        }

        /// <summary>
        /// Dừng API Server.
        /// </summary>
        public async Task StopAsync()
        {
            if (_app != null)
            {
                Log("POApiServer", "Stopping...");
                await _app.StopAsync();
                _app = null;
                Log("POApiServer", "Stopped.");
            }
        }

        private IResult HandleHealth(HttpContext context)
        {
            return Results.Json(new HealthResponse
            {
                Status = "OK",
                Timestamp = DateTime.Now,
                AppState = GV.AppState.ToString()
            });
        }

        private async Task<IResult> HandleCreatePO(HttpContext context)
        {
            try
            {
                var request = await context.Request.ReadFromJsonAsync<CreatePORequest>();
                if (request == null)
                {
                    return Results.BadRequest(new CreatePOResponse
                    {
                        Success = false,
                        Message = "Invalid request body."
                    });
                }

                Log("POApiServer", $"Creating PO: {request.OrderNo}");

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.OrderNo))
                {
                    return Results.BadRequest(new CreatePOResponse
                    {
                        Success = false,
                        Message = "orderNo is required."
                    });
                }

                if (request.OrderQty <= 24)
                {
                    return Results.BadRequest(new CreatePOResponse
                    {
                        Success = false,
                        Message = "orderQty must be > 24."
                    });
                }

                // Map to POInfo
                var poInfo = new po.POInfo
                {
                    OrderNo = request.OrderNo.Trim(),
                    Site = request.Site?.Trim() ?? "",
                    Factory = request.Factory?.Trim() ?? "",
                    ProductionLine = request.ProductionLine?.Trim() ?? "",
                    ProductionDate = request.ProductionDate?.Trim() ?? DateTime.Now.ToString("yyyy-MM-dd"),
                    Shift = request.Shift?.Trim() ?? "",
                    OrderQty = request.OrderQty,
                    LotNumber = request.LotNumber?.Trim() ?? "",
                    ProductCode = request.ProductCode?.Trim() ?? "",
                    ProductName = request.ProductName?.Trim() ?? "",
                    Gtin = request.Gtin?.Trim() ?? "",
                    CustomerOrderNo = request.CustomerOrderNo?.Trim() ?? "",
                    Uom = request.Uom?.Trim() ?? "PCS"
                };

                // Check if PO already exists
                if (po.POLoader.Exists(poInfo.OrderNo))
                {
                    return Results.Conflict(new CreatePOResponse
                    {
                        Success = false,
                        Message = $"PO '{poInfo.OrderNo}' already exists."
                    });
                }

                // Step 1: Create PO in PO_List
                var createResult = po.POLoader.Create(poInfo);
                if (!createResult.IsSuccess)
                {
                    return Results.Json(new CreatePOResponse
                    {
                        Success = false,
                        Message = createResult.Message
                    }, statusCode: 400);
                }

                Log("POApiServer", $"PO '{poInfo.OrderNo}' created in PO_List.");

                // Step 2: Initialize PO databases (UniqueCodes, Records, Carton)
                var initResult = po.POCreator.InitPO(poInfo.OrderNo);
                if (!initResult.IsSuccess)
                {
                    Log("POApiServer", $"Warning: Init PO DB failed: {initResult.Message}");
                }
                else
                {
                    Log("POApiServer", $"PO databases initialized for '{poInfo.OrderNo}'.");
                }

                // Step 3: Record to history (Status = Running)
                var historyResult = po.POHistoryManager.RecordStart(
                    poInfo.OrderNo,
                    poInfo.ProductionDate,
                    request.UserName ?? "API"
                );

                // Step 4: Load codes from DataPool if enabled
                int loadedCodes = 0;
                if (request.AutoLoadCodes && !string.IsNullOrWhiteSpace(poInfo.Gtin))
                {
                    var loadResult = po.POLoader.LoadCodesFromDataPool(poInfo.OrderNo, poInfo.Gtin);
                    if (loadResult.success)
                    {
                        loadedCodes = loadResult.loadedCount;
                        Log("POApiServer", $"Loaded {loadedCodes} codes from DataPool '{poInfo.Gtin}'.");
                    }
                    else
                    {
                        Log("POApiServer", $"Warning: Could not load codes: {loadResult.message}");
                    }
                }

                Log("POApiServer", $"PO '{poInfo.OrderNo}' created successfully. Loaded {loadedCodes} codes.");

                // Audit log for PO creation
                var createAuditJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    orderNo = poInfo.OrderNo,
                    gtin = poInfo.Gtin,
                    productCode = poInfo.ProductCode,
                    productName = poInfo.ProductName,
                    orderQty = poInfo.OrderQty,
                    createdBy = request.UserName ?? "API",
                    loadedCodes
                });
                _auditLog?.Log(request.UserName ?? "API", POActions.Create,
                    $"Created PO: {poInfo.OrderNo} (GTIN: {poInfo.Gtin}, Qty: {poInfo.OrderQty})",
                    createAuditJson, "PO");

                return Results.Json(new CreatePOResponse
                {
                    Success = true,
                    Message = $"PO '{poInfo.OrderNo}' created successfully.",
                    OrderNo = poInfo.OrderNo,
                    LoadedCodesCount = loadedCodes
                });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error creating PO: {ex.Message}");
                return Results.Json(new CreatePOResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}"
                }, statusCode: 500);
            }
        }

        private IResult HandleGetPO(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    return Results.BadRequest(new { success = false, message = "orderNo is required." });
                }

                var result = po.POLoader.GetByOrderNo(orderNo);
                if (!result.issuccess)
                {
                    return Results.NotFound(new { success = false, message = result.Message });
                }

                var row = result.data.Rows[0];
                return Results.Json(new
                {
                    success = true,
                    data = new
                    {
                        orderNo = row["orderNo"]?.ToString(),
                        site = row["site"]?.ToString(),
                        factory = row["factory"]?.ToString(),
                        productionLine = row["productionLine"]?.ToString(),
                        productionDate = row["productionDate"]?.ToString(),
                        shift = row["shift"]?.ToString(),
                        orderQty = Convert.ToInt32(row["orderQty"] ?? 0),
                        lotNumber = row["lotNumber"]?.ToString(),
                        productCode = row["productCode"]?.ToString(),
                        productName = row["productName"]?.ToString(),
                        gtin = row["gtin"]?.ToString(),
                        customerOrderNo = row["customerOrderNo"]?.ToString(),
                        uom = row["uom"]?.ToString(),
                        createdTime = row["CreatedTime"]?.ToString(),
                        modifiedTime = row["ModifiedTime"]?.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
            }
        }

        private IResult HandleGetAllPO(HttpContext context)
        {
            try
            {
                var result = po.POLoader.GetAll();
                if (!result.issuccess)
                {
                    return Results.Json(new { success = false, message = result.Message, data = Array.Empty<object>() });
                }

                var poList = new List<object>();
                foreach (System.Data.DataRow row in result.data.Rows)
                {
                    poList.Add(new
                    {
                        orderNo = row["orderNo"]?.ToString(),
                        productName = row["productName"]?.ToString(),
                        orderQty = Convert.ToInt32(row["orderQty"] ?? 0),
                        productionDate = row["productionDate"]?.ToString(),
                        status = row["CreatedTime"]?.ToString()
                    });
                }

                return Results.Json(new
                {
                    success = true,
                    count = poList.Count,
                    data = poList
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
            }
        }

        // ================== DELETE PO ==================

        /// <summary>
        /// Check if a PO can be deleted (no codes used).
        /// GET /api/po/{orderNo}/can-delete
        /// </summary>
        private IResult HandleCanDeletePO(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new { success = false, message = "orderNo is required." });

                // Check PO exists
                if (!po.POLoader.Exists(orderNo))
                    return Results.NotFound(new CheckDeleteResponse { CanDelete = false, Reason = "PO not found." });

                // Count used codes
                var usedResult = CountUsedCodes(orderNo);
                var totalResult = CountTotalCodes(orderNo);
                var used = usedResult.count;
                var total = totalResult.count;

                if (used > 0)
                {
                    return Results.Json(new CheckDeleteResponse
                    {
                        CanDelete = false,
                        Reason = $"PO đã sử dụng {used}/{total} mã. Không thể xóa.",
                        UsedCodesCount = used,
                        TotalCodesCount = total
                    });
                }

                return Results.Json(new CheckDeleteResponse
                {
                    CanDelete = true,
                    Reason = "PO chưa chạy mã nào. Có thể xóa.",
                    UsedCodesCount = used,
                    TotalCodesCount = total
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a PO (only if no codes used).
        /// DELETE /api/po/{orderNo}
        /// </summary>
        private IResult HandleDeletePO(HttpContext context, string orderNo)
        {
            try
            {
                var userName = context.Request.Headers["X-User-Name"].FirstOrDefault() ?? "API";

                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new DeletePOResponse { Success = false, Message = "orderNo is required." });

                // Check PO exists
                if (!po.POLoader.Exists(orderNo))
                {
                    _auditLog?.Log(userName, POActions.Delete, $"Attempted to delete non-existent PO: {orderNo}", $"{{\"orderNo\":\"{orderNo}\"}}", "PO");
                    return Results.NotFound(new DeletePOResponse { Success = false, Message = "PO not found." });
                }

                // Check if any codes are used
                var usedResult = CountUsedCodes(orderNo);
                var totalResult = CountTotalCodes(orderNo);

                if (usedResult.count > 0)
                {
                    var reason = $"PO đã sử dụng {usedResult.count}/{totalResult.count} mã. Không thể xóa.";
                    _auditLog?.Log(userName, POActions.Delete, $"Delete blocked: {orderNo} - {reason}",
                        $"{{\"orderNo\":\"{orderNo}\",\"usedCodes\":{usedResult.count},\"totalCodes\":{totalResult.count}}}", "PO");
                    return Results.Json(new DeletePOResponse
                    {
                        Success = false,
                        CanDelete = false,
                        Reason = reason,
                        Message = reason
                    }, statusCode: 400);
                }

                // Get PO info before delete for audit
                var poResult = po.POLoader.GetByOrderNo(orderNo);
                var gtin = "";
                var orderQty = 0;
                if (poResult.issuccess && poResult.data.Rows.Count > 0)
                {
                    gtin = poResult.data.Rows[0]["gtin"]?.ToString() ?? "";
                    orderQty = Convert.ToInt32(poResult.data.Rows[0]["orderQty"] ?? 0);
                }

                // Perform delete
                var deleteResult = po.POLoader.Delete(orderNo);

                if (deleteResult.IsSuccess)
                {
                    // Log successful deletion
                    var auditJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        orderNo,
                        gtin,
                        orderQty,
                        deletedBy = userName,
                        deletedAt = DateTime.UtcNow.ToString("o")
                    });
                    _auditLog?.Log(userName, POActions.Delete, $"Deleted PO: {orderNo} (GTIN: {gtin}, Qty: {orderQty})", auditJson, "PO");

                    Log("POApiServer", $"PO '{orderNo}' deleted by {userName}.");

                    return Results.Json(new DeletePOResponse
                    {
                        Success = true,
                        CanDelete = true,
                        Message = $"Xóa PO '{orderNo}' thành công."
                    });
                }
                else
                {
                    _auditLog?.Log(userName, POActions.Delete, $"Failed to delete PO: {orderNo} - {deleteResult.Message}",
                        $"{{\"orderNo\":\"{orderNo}\",\"error\":\"{deleteResult.Message}\"}}", "PO");
                    return Results.Json(new DeletePOResponse
                    {
                        Success = false,
                        Message = deleteResult.Message
                    }, statusCode: 400);
                }
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error deleting PO '{orderNo}': {ex.Message}");
                _auditLog?.Log("SYSTEM", POActions.Delete, $"Exception deleting PO: {orderNo} - {ex.Message}", $"{{\"orderNo\":\"{orderNo}\"}}", "PO");
                return Results.Json(new DeletePOResponse { Success = false, Message = $"Server error: {ex.Message}" }, statusCode: 500);
            }
        }

        /// <summary>
        /// Count used codes in a PO.
        /// </summary>
        private (int count, bool success) CountUsedCodes(string orderNo)
        {
            try
            {
                var dbPath = VNQR.Helpers.po.Config.GetRecordActivePath(orderNo);
                if (!File.Exists(dbPath)) return (0, true);

                using var conn = new SQLiteConnection($"Data Source={dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM UniqueCodes WHERE Status != 0";
                return (Convert.ToInt32(cmd.ExecuteScalar()), true);
            }
            catch { return (0, false); }
        }

        /// <summary>
        /// Count total codes in a PO.
        /// </summary>
        private (int count, bool success) CountTotalCodes(string orderNo)
        {
            try
            {
                var dbPath = VNQR.Helpers.po.Config.GetRecordActivePath(orderNo);
                if (!File.Exists(dbPath)) return (0, true);

                using var conn = new SQLiteConnection($"Data Source={dbPath}");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM UniqueCodes";
                return (Convert.ToInt32(cmd.ExecuteScalar()), true);
            }
            catch { return (0, false); }
        }

        // ================== AUDIT LOG ENDPOINTS ==================

        /// <summary>
        /// Get audit logs for PO operations.
        /// GET /api/audit?limit=100&offset=0
        /// </summary>
        private IResult HandleGetAuditLogs(HttpContext context)
        {
            try
            {
                if (_auditLog == null)
                    return Results.Json(new AuditLogResponse { Success = false, Message = "Audit log not initialized." });

                var limit = 100;
                var offset = 0;
                if (int.TryParse(context.Request.Query["limit"].FirstOrDefault(), out var l)) limit = Math.Clamp(l, 1, 1000);
                if (int.TryParse(context.Request.Query["offset"].FirstOrDefault(), out var o)) offset = Math.Max(o, 0);

                var logs = _auditLog.QueryRecent(limit, offset);
                var total = _auditLog.Count();

                return Results.Json(new AuditLogResponse
                {
                    Success = true,
                    Count = logs.Count,
                    TotalCount = total,
                    Data = logs.Select(e => new AuditLogEntry
                    {
                        Id = e.Id,
                        TimeISO = e.TimeISO,
                        User = e.User,
                        Code = e.Code,
                        Action = e.Action.ToString(),
                        Description = e.Description,
                        JsonParams = e.JsonParams
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new AuditLogResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Get audit logs filtered by business code (e.g. "PO").
        /// GET /api/audit/PO?limit=100&offset=0
        /// </summary>
        private IResult HandleGetAuditLogsByCode(HttpContext context, string code)
        {
            try
            {
                if (_auditLog == null)
                    return Results.Json(new AuditLogResponse { Success = false, Message = "Audit log not initialized." });

                var limit = 100;
                var offset = 0;
                if (int.TryParse(context.Request.Query["limit"].FirstOrDefault(), out var l)) limit = Math.Clamp(l, 1, 1000);
                if (int.TryParse(context.Request.Query["offset"].FirstOrDefault(), out var o)) offset = Math.Max(o, 0);

                var logs = _auditLog.QueryByCode(code);
                var paged = logs.Skip(offset).Take(limit).ToList();

                return Results.Json(new AuditLogResponse
                {
                    Success = true,
                    Count = paged.Count,
                    TotalCount = logs.Count,
                    Data = paged.Select(e => new AuditLogEntry
                    {
                        Id = e.Id,
                        TimeISO = e.TimeISO,
                        User = e.User,
                        Code = e.Code,
                        Action = e.Action.ToString(),
                        Description = e.Description,
                        JsonParams = e.JsonParams
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new AuditLogResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        // ================== DATAPOOL HANDLERS ==================

        /// <summary>
        /// Add single code manually.
        /// POST /api/datapool/add
        /// </summary>
        private IResult HandleDataPoolAddCode(HttpContext context)
        {
            try
            {
                var request = context.Request.ReadFromJsonAsync<AddCodeManualRequest>().Result;
                if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.Code))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName and code are required." });

                Log("POApiServer", $"Adding code to DataPool '{request.PoolName}': {request.Code}");

                var result = VNQR.DataPool.Import.Manual(
                    request.PoolName,
                    request.Code,
                    request.Status,
                    request.BatchID ?? "",
                    "",
                    request.Note ?? "",
                    request.UserName ?? "API"
                );

                return Results.Json(new ApiResponse
                {
                    Success = result.issuccess,
                    Message = result.Message,
                    Count = result.count
                }, statusCode: result.issuccess ? 200 : 400);
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error adding code: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Add code from reader (camera) - auto marks as Used.
        /// POST /api/datapool/add-reader
        /// </summary>
        private IResult HandleDataPoolAddFromReader(HttpContext context)
        {
            try
            {
                var request = context.Request.ReadFromJsonAsync<AddCodeReaderRequest>().Result;
                if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.BatchID))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName, code, and batchID are required." });

                Log("POApiServer", $"Adding code from reader to DataPool '{request.PoolName}': {request.Code}");

                var result = VNQR.DataPool.Import.FromReader(
                    request.PoolName,
                    request.Code,
                    request.BatchID,
                    "Reader",
                    request.Note ?? ""
                );

                return Results.Json(new ApiResponse
                {
                    Success = result.issuccess,
                    Message = result.Message,
                    Count = result.count
                }, statusCode: result.issuccess ? 200 : 400);
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error adding code from reader: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Import codes from CSV file.
        /// POST /api/datapool/import-file
        /// </summary>
        private IResult HandleDataPoolImportFromFile(HttpContext context)
        {
            try
            {
                var request = context.Request.ReadFromJsonAsync<ImportCodesFromFileRequest>().Result;
                if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.CsvPath) || string.IsNullOrWhiteSpace(request.CreateID))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName, csvPath, and createID are required." });

                Log("POApiServer", $"Importing codes from file to DataPool '{request.PoolName}': {request.CsvPath}");

                var result = VNQR.DataPool.Import.FromFile(
                    request.PoolName,
                    request.CsvPath,
                    request.UserName ?? "API",
                    request.CreateID,
                    request.CodeColumn ?? "Code",
                    request.NoteColumn ?? "",
                    request.Note ?? ""
                );

                return Results.Json(new ApiResponse
                {
                    Success = result.issuccess,
                    Message = result.Message,
                    Count = result.count
                }, statusCode: result.issuccess ? 200 : 400);
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error importing from file: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Get all codes in a pool.
        /// GET /api/datapool/{poolName}/codes
        /// </summary>
        private IResult HandleDataPoolGetCodes(HttpContext context, string poolName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName is required." });

                // Có thể lọc theo status qua query string ?status=0|1
                int? status = null;
                if (context.Request.Query.TryGetValue("status", out var statusVal))
                {
                    if (int.TryParse(statusVal.ToString(), out int s))
                        status = s;
                }

                var result = VNQR.DataPool.Query.GetAllCodes(poolName, status);
                if (!result.issuccess || result.data == null)
                    return Results.Json(new ApiResponse { Success = false, Message = result.Message }, statusCode: 404);

                var codes = new List<object>();
                foreach (System.Data.DataRow row in result.data.Rows)
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

                return Results.Json(new
                {
                    success = true,
                    count = codes.Count,
                    data = codes
                });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error getting codes: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Get single code info.
        /// GET /api/datapool/{poolName}/code/{code}
        /// </summary>
        private IResult HandleDataPoolGetCode(HttpContext context, string poolName, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName and code are required." });

                var result = VNQR.DataPool.Import.GetByCode(poolName, code);
                if (!result.issuccess || result.data == null || result.data.Rows.Count == 0)
                    return Results.NotFound(new ApiResponse { Success = false, Message = "Code not found." });

                var row = result.data.Rows[0];
                return Results.Json(new
                {
                    success = true,
                    data = new
                    {
                        id = row["ID"],
                        code = row["Code"]?.ToString(),
                        status = Convert.ToInt32(row["Status"]),
                        batchID = row["BatchID"]?.ToString(),
                        createTime = row["CreateTime"]?.ToString(),
                        createID = row["CreateID"]?.ToString(),
                        note = row["Note"]?.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error getting code: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Update code status.
        /// PUT /api/datapool/{poolName}/code/{code}/status
        /// </summary>
        private IResult HandleDataPoolUpdateStatus(HttpContext context, string poolName, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName and code are required." });

                var body = context.Request.ReadFromJsonAsync<System.Text.Json.JsonElement>().Result;
                if (body.ValueKind != System.Text.Json.JsonValueKind.Object)
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "Invalid JSON body." });

                int newStatus = body.TryGetProperty("status", out var statusProp) && statusProp.ValueKind == System.Text.Json.JsonValueKind.Number
                    ? statusProp.GetInt32()
                    : -1;
                string batchID = body.TryGetProperty("batchID", out var batchProp) && batchProp.ValueKind == System.Text.Json.JsonValueKind.String
                    ? batchProp.GetString() ?? ""
                    : "";
                string note = body.TryGetProperty("note", out var noteProp) && noteProp.ValueKind == System.Text.Json.JsonValueKind.String
                    ? noteProp.GetString() ?? ""
                    : "";

                if (newStatus < 0 || newStatus > 1)
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "status must be 0 (Unused) or 1 (Used)." });

                var ok = VNQR.DataPool.Updater.Update(poolName, code, newStatus, batchID, "", note);
                if (ok)
                    return Results.Json(new ApiResponse { Success = true, Message = "Status updated.", Count = 1 });

                return Results.NotFound(new ApiResponse { Success = false, Message = "Code not found." });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error updating status: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// Delete a code.
        /// DELETE /api/datapool/{poolName}/code/{code}
        /// </summary>
        private IResult HandleDataPoolDeleteCode(HttpContext context, string poolName, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code))
                    return Results.BadRequest(new ApiResponse { Success = false, Message = "poolName and code are required." });

                var ok = VNQR.DataPool.Updater.Delete(poolName, code);
                if (ok)
                    return Results.Json(new ApiResponse { Success = true, Message = "Code deleted.", Count = 1 });

                return Results.NotFound(new ApiResponse { Success = false, Message = "Code not found." });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error deleting code: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>
        /// List all available pools.
        /// GET /api/datapool/pools
        /// </summary>
        private IResult HandleDataPoolListPools(HttpContext context)
        {
            try
            {
                var result = VNQR.DataPool.Lister.ListPools();
                if (!result.issuccess)
                    return Results.Json(new ApiResponse { Success = false, Message = result.Message }, statusCode: 500);

                var pools = new List<object>();
                if (result.data != null)
                {
                    foreach (System.Data.DataRow row in result.data.Rows)
                    {
                        // Bỏ qua thư mục Phieu/ chứa file phiếu tạo
                        var name = row["name"]?.ToString() ?? "";
                        if (string.Equals(name, "Phieu", StringComparison.OrdinalIgnoreCase))
                            continue;

                        pools.Add(new
                        {
                            name = name,
                            fileName = row["fileName"]?.ToString() ?? "",
                            size = Convert.ToInt64(row["size"])
                        });
                    }
                }

                return Results.Json(new
                {
                    success = true,
                    count = pools.Count,
                    data = pools
                });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error listing pools: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// Create a new pool (ensures the pool DB file exists).
        /// POST /api/datapool/pools
        private IResult HandleDataPoolCreatePool(HttpContext context)
        {
            try
            {
                var body = context.Request.ReadFromJsonAsync<CreatePoolRequest>().GetAwaiter().GetResult();
                if (body == null || string.IsNullOrWhiteSpace(body.PoolName))
                    return Results.Json(new ApiResponse { Success = false, Message = "poolName is required." }, statusCode: 400);

                VNQR.DataPool.PoolHelper.EnsurePool(body.PoolName);
                return Results.Json(new ApiResponse { Success = true, Message = $"Pool '{body.PoolName}' created." });
            }
            catch (Exception ex)
            {
                Log("POApiServer", $"Error creating pool: {ex.Message}");
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        private void Log(string source, string message)
        {
            var logLine = $"[{DateTime.Now:HH:mm:ss}] [{source}] {message}";
            if (_logger != null)
                _logger.LogInformation(logLine);
            else if (_onLog != null)
                _onLog(source, message);
            else
                Console.WriteLine(logLine);
        }

        public void Dispose()
        {
            _auditLog?.Dispose();
            StopAsync().GetAwaiter().GetResult();
        }
    }
}
