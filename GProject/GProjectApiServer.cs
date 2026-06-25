using System.Text.Json.Serialization;
using GProject.DataPoolHelper;

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
        _app.UseCors();

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

        Log("GProjectApiServer", $"DataPool endpoints registered");
        Log("GProjectApiServer", $"Started on http://{_host}:{_port}");

        await _app.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_app != null)
        {
            Log("GProjectApiServer", "Stopping...");
            await _app.StopAsync();
            _app = null;
        }
    }

    #region Handlers
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
            Log("GProjectApiServer", $"Error listing pools: {ex.Message}");
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
            Log("GProjectApiServer", $"Error getting pool stats: {ex.Message}");
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
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleAddCode(HttpContext context)
    {
        try
        {
            var request = context.Request.ReadFromJsonAsync<AddCodeManualRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.Code))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName and code are required." }, statusCode: 400);

            var result = DataPoolStatic.Import_Manual(request.PoolName, request.Code, request.Status, request.BatchID ?? "", "", request.Note ?? "", request.UserName ?? "API");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleAddFromReader(HttpContext context)
    {
        try
        {
            var request = context.Request.ReadFromJsonAsync<AddCodeReaderRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.BatchID))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName, code, and batchID are required." }, statusCode: 400);

            var result = DataPoolStatic.Import_FromReader(request.PoolName, request.Code, request.BatchID, "Reader", request.Note ?? "");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleImportFromFile(HttpContext context)
    {
        try
        {
            var request = context.Request.ReadFromJsonAsync<ImportCodesFromFileRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.CsvPath) || string.IsNullOrWhiteSpace(request.CreateID))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName, csvPath, and createID are required." }, statusCode: 400);

            var result = DataPoolStatic.Import_FromFile(request.PoolName, request.CsvPath, request.UserName ?? "API", request.CreateID, request.CodeColumn ?? "Code", request.NoteColumn ?? "", request.Note ?? "");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }

    private IResult HandleImportFromContent(HttpContext context)
    {
        try
        {
            var request = context.Request.ReadFromJsonAsync<ImportCodesFromContentRequest>().GetAwaiter().GetResult();
            if (request == null || string.IsNullOrWhiteSpace(request.PoolName) || string.IsNullOrWhiteSpace(request.CsvContent) || string.IsNullOrWhiteSpace(request.CreateID))
                return Results.Json(new ApiResponse { Success = false, Message = "poolName, csvContent, and createID are required." }, statusCode: 400);

            Log("GProjectApiServer", $"Importing {request.CsvContent.Split('\n').Length - 1} codes to pool '{request.PoolName}'");

            // Parse CSV content and insert
            var result = DataPoolStatic.Import_FromContent(request.PoolName, request.CsvContent, request.UserName ?? "API", request.CreateID, request.Note ?? "");
            return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message, Count = result.Count }, statusCode: result.IsSuccess ? 200 : 400);
        }
        catch (Exception ex)
        {
            Log("GProjectApiServer", $"Error importing content: {ex.Message}");
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
            return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
        }
    }
    #endregion

    private void Log(string source, string message)
    {
        var logLine = $"[{DateTime.Now:HH:mm:ss}] [{source}] {message}";
        if (_onLog != null) _onLog(source, message);
        else Console.WriteLine(logLine);
    }

    public void Dispose() => StopAsync().GetAwaiter().GetResult();

    #region DTOs
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
    #endregion
}
