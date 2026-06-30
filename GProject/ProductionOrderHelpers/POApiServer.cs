using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.ProductionOrderHelpers
{
    #region Request/Response Models

    public class CreatePORequest
    {
        [JsonPropertyName("orderNo")] public string OrderNo { get; set; } = "";
        [JsonPropertyName("site")] public string Site { get; set; } = "";
        [JsonPropertyName("factory")] public string Factory { get; set; } = "";
        [JsonPropertyName("productionLine")] public string ProductionLine { get; set; } = "";
        [JsonPropertyName("productionDate")] public string ProductionDate { get; set; } = "";
        [JsonPropertyName("shift")] public string Shift { get; set; } = "";
        [JsonPropertyName("orderQty")] public int OrderQty { get; set; }
        [JsonPropertyName("lotNumber")] public string LotNumber { get; set; } = "";
        [JsonPropertyName("productCode")] public string ProductCode { get; set; } = "";
        [JsonPropertyName("productName")] public string ProductName { get; set; } = "";
        [JsonPropertyName("gtin")] public string Gtin { get; set; } = "";
        [JsonPropertyName("customerOrderNo")] public string CustomerOrderNo { get; set; } = "";
        [JsonPropertyName("uom")] public string Uom { get; set; } = "PCS";
        [JsonPropertyName("userName")] public string UserName { get; set; } = "API";
        [JsonPropertyName("autoLoadCodes")] public bool AutoLoadCodes { get; set; } = true;
        [JsonPropertyName("cartonCapacity")] public int CartonCapacity { get; set; } = 24;
    }

    public class CreatePOResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
        [JsonPropertyName("orderNo")] public string? OrderNo { get; set; }
        [JsonPropertyName("loadedCodesCount")] public int LoadedCodesCount { get; set; }
        [JsonPropertyName("createdCartonsCount")] public int CreatedCartonsCount { get; set; }
    }

    public class DeletePOResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
        [JsonPropertyName("canDelete")] public bool CanDelete { get; set; }
        [JsonPropertyName("reason")] public string Reason { get; set; } = "";
    }

    public class CheckDeleteResponse
    {
        [JsonPropertyName("canDelete")] public bool CanDelete { get; set; }
        [JsonPropertyName("reason")] public string Reason { get; set; } = "";
        [JsonPropertyName("usedCodesCount")] public int UsedCodesCount { get; set; }
        [JsonPropertyName("totalCodesCount")] public int TotalCodesCount { get; set; }
    }

    public class ProductionStatusResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; } = true;
        [JsonPropertyName("message")] public string Message { get; set; } = "";
        [JsonPropertyName("state")] public string State { get; set; } = "";
        [JsonPropertyName("hasPO")] public bool HasPO { get; set; }
        [JsonPropertyName("orderNo")] public string? OrderNo { get; set; }
        [JsonPropertyName("productName")] public string? ProductName { get; set; }
        [JsonPropertyName("orderQty")] public int OrderQty { get; set; }
        [JsonPropertyName("totalCodes")] public int TotalCodes { get; set; }
        [JsonPropertyName("activeCodes")] public int ActiveCodes { get; set; }
        [JsonPropertyName("packedCodes")] public int PackedCodes { get; set; }
        [JsonPropertyName("cartonCount")] public int CartonCount { get; set; }
        [JsonPropertyName("closedCartonCount")] public int ClosedCartonCount { get; set; }
        [JsonPropertyName("progressPercent")] public double ProgressPercent { get; set; }
    }

    public class ApiResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
        [JsonPropertyName("count")] public int Count { get; set; }
    }

    public class ActivateCodeRequest
    {
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("activateDate")] public string ActivateDate { get; set; } = "";
        [JsonPropertyName("activateUser")] public string ActivateUser { get; set; } = "";
        [JsonPropertyName("productionDate")] public string ProductionDate { get; set; } = "";
    }

    public class PackCodeRequest
    {
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("cartonCode")] public string CartonCode { get; set; } = "";
        [JsonPropertyName("packingDate")] public string PackingDate { get; set; } = "";
        [JsonPropertyName("packingUser")] public string PackingUser { get; set; } = "";
        [JsonPropertyName("productionDate")] public string ProductionDate { get; set; } = "";
    }

    public class CartonOperationRequest
    {
        [JsonPropertyName("cartonId")] public int CartonId { get; set; }
        [JsonPropertyName("activateUser")] public string ActivateUser { get; set; } = "";
    }

    #endregion

    /// <summary>
    /// PO API Server - REST API endpoints cho Production Order Management
    /// </summary>
    public static class POApiServer
    {
        /// <summary>
        /// Khởi tạo PO database
        /// </summary>
        public static void Initialize()
        {
            GProduction.Initialize();
            LogInfo("POApiServer", "Production Helper initialized");
        }

        #region PO Endpoints

        public static async Task<IResult> HandleCreatePO(HttpContext context)
        {
            try
            {
                var request = await context.Request.ReadFromJsonAsync<CreatePORequest>();
                if (request == null)
                    return Results.BadRequest(new CreatePOResponse { Success = false, Message = "Invalid request body." });

                LogInfo("POApiServer", $"Creating PO: {request.OrderNo}");

                if (string.IsNullOrWhiteSpace(request.OrderNo))
                    return Results.BadRequest(new CreatePOResponse { Success = false, Message = "orderNo is required." });

                if (request.OrderQty <= 24)
                    return Results.BadRequest(new CreatePOResponse { Success = false, Message = "orderQty must be > 24." });

                var poInfo = new POInfo
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

                if (GProduction.POLoader.Exists(poInfo.OrderNo))
                    return Results.Conflict(new CreatePOResponse { Success = false, Message = $"PO '{poInfo.OrderNo}' already exists." });

                var createResult = GProduction.POLoader.Create(poInfo);
                if (!createResult.IsSuccess)
                    return Results.Json(new CreatePOResponse { Success = false, Message = createResult.Message }, statusCode: 400);

                LogInfo("POApiServer", $"PO '{poInfo.OrderNo}' created in PO_List.");

                GProduction.POCreator.InitPO(poInfo.OrderNo);
                GProduction.POHistoryManager.RecordStart(poInfo.OrderNo, poInfo.ProductionDate, request.UserName ?? "API");

                int loadedCodes = 0;
                if (request.AutoLoadCodes && !string.IsNullOrWhiteSpace(poInfo.Gtin))
                {
                    var loadResult = GProduction.POLoader.LoadCodesFromDataPool(poInfo.OrderNo, poInfo.Gtin);
                    if (loadResult.success) loadedCodes = loadResult.loadedCount;
                }

                int createdCartons = 0;
                if (request.OrderQty > 0 && request.CartonCapacity > 0)
                {
                    var cartonResult = GProduction.POCreator.CreateRequiredCartons(poInfo.OrderNo, request.OrderQty, request.CartonCapacity);
                    if (cartonResult.success) createdCartons = cartonResult.createdCount;
                }

                LogInfo("POApiServer", $"PO '{poInfo.OrderNo}' created. Loaded {loadedCodes} codes, {createdCartons} cartons.");

                return Results.Json(new CreatePOResponse
                {
                    Success = true,
                    Message = $"PO '{poInfo.OrderNo}' created successfully.",
                    OrderNo = poInfo.OrderNo,
                    LoadedCodesCount = loadedCodes,
                    CreatedCartonsCount = createdCartons
                });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error creating PO: {ex.Message}", ex);
                return Results.Json(new CreatePOResponse { Success = false, Message = $"Server error: {ex.Message}" }, statusCode: 500);
            }
        }

        public static Task<IResult> HandleGetAllPO()
        {
            try
            {
                var result = GProduction.POLoader.GetAll();
                var poList = new List<object>();
                if (result.IsSuccess && result.Data != null)
                {
                    foreach (System.Data.DataRow row in result.Data.Rows)
                    {
                        poList.Add(new
                        {
                            orderNo = row["orderNo"]?.ToString(),
                            productName = row["productName"]?.ToString(),
                            orderQty = Convert.ToInt32(row["orderQty"] ?? 0),
                            productionDate = row["productionDate"]?.ToString(),
                            gtin = row["gtin"]?.ToString(),
                            status = "Active",
                            createdTime = row["CreatedTime"]?.ToString()
                        });
                    }
                }
                return Task.FromResult(Results.Json(new { success = true, count = poList.Count, data = poList }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error getting all PO: {ex.Message}", ex);
                return Task.FromResult(Results.Json(new { success = false, message = ex.Message }, statusCode: 500));
            }
        }

        public static Task<IResult> HandleGetPO(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new { success = false, message = "orderNo is required." }));

                var result = GProduction.POLoader.GetByOrderNo(orderNo);
                if (!result.IsSuccess || result.Data == null || result.Data.Rows.Count == 0)
                    return Task.FromResult(Results.NotFound(new { success = false, message = $"PO '{orderNo}' not found." }));

                var row = result.Data.Rows[0];
                var poInfo = POInfo.FromDataRow(row);
                int activeCodes = GProduction.POActivator.GetActiveCount(orderNo);
                int packedCodes = GProduction.POPacking.GetPackedCount(orderNo);
                int cartonCount = GProduction.POCarton.GetTotalCartonCount(orderNo);
                int closedCartons = GProduction.POCarton.GetClosedCartonCount(orderNo);

                return Task.FromResult(Results.Json(new
                {
                    success = true,
                    data = new
                    {
                        orderNo = poInfo.OrderNo,
                        site = poInfo.Site,
                        factory = poInfo.Factory,
                        productionLine = poInfo.ProductionLine,
                        productionDate = poInfo.ProductionDate,
                        shift = poInfo.Shift,
                        orderQty = poInfo.OrderQty,
                        lotNumber = poInfo.LotNumber,
                        productCode = poInfo.ProductCode,
                        productName = poInfo.ProductName,
                        gtin = poInfo.Gtin,
                        customerOrderNo = poInfo.CustomerOrderNo,
                        uom = poInfo.Uom,
                        createdTime = poInfo.CreatedTime,
                        modifiedTime = poInfo.ModifiedTime,
                        stats = new
                        {
                            activeCodes,
                            packedCodes,
                            cartonCount,
                            closedCartons,
                            progressPercent = poInfo.OrderQty > 0 ? Math.Round((double)packedCodes / poInfo.OrderQty * 100, 2) : 0
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error getting PO '{orderNo}': {ex.Message}", ex);
                return Task.FromResult(Results.Json(new { success = false, message = ex.Message }, statusCode: 500));
            }
        }

        public static Task<IResult> HandleCanDeletePO(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new { success = false, message = "orderNo is required." }));

                if (!GProduction.POLoader.Exists(orderNo))
                    return Task.FromResult(Results.NotFound(new CheckDeleteResponse { CanDelete = false, Reason = "PO not found." }));

                int usedCodes = GProduction.POActivator.GetActiveCount(orderNo);
                int totalCodes = GProduction.POLoader.GetCodeCount(orderNo);

                if (usedCodes > 0)
                    return Task.FromResult(Results.Json(new CheckDeleteResponse
                    {
                        CanDelete = false,
                        Reason = $"PO đã sử dụng {usedCodes}/{totalCodes} mã. Không thể xóa.",
                        UsedCodesCount = usedCodes,
                        TotalCodesCount = totalCodes
                    }));

                return Task.FromResult(Results.Json(new CheckDeleteResponse
                {
                    CanDelete = true,
                    Reason = "PO chưa chạy mã nào. Có thể xóa.",
                    UsedCodesCount = usedCodes,
                    TotalCodesCount = totalCodes
                }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error checking delete PO '{orderNo}': {ex.Message}", ex);
                return Task.FromResult(Results.Json(new { success = false, message = ex.Message }, statusCode: 500));
            }
        }

        public static Task<IResult> HandleDeletePO(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new DeletePOResponse { Success = false, Message = "orderNo is required." }));

                if (!GProduction.POLoader.Exists(orderNo))
                    return Task.FromResult(Results.NotFound(new DeletePOResponse { Success = false, Message = "PO not found." }));

                int usedCodes = GProduction.POActivator.GetActiveCount(orderNo);
                if (usedCodes > 0)
                {
                    int totalCodes = GProduction.POLoader.GetCodeCount(orderNo);
                    return Task.FromResult(Results.Json(new DeletePOResponse
                    {
                        Success = false,
                        CanDelete = false,
                        Reason = $"PO đã sử dụng {usedCodes}/{totalCodes} mã. Không thể xóa.",
                        Message = $"PO đã sử dụng {usedCodes}/{totalCodes} mã. Không thể xóa."
                    }, statusCode: 400));
                }

                var deleteResult = GProduction.POLoader.Delete(orderNo);
                return Task.FromResult(Results.Json(new DeletePOResponse
                {
                    Success = deleteResult.IsSuccess,
                    CanDelete = deleteResult.IsSuccess,
                    Message = deleteResult.Message
                }, statusCode: deleteResult.IsSuccess ? 200 : 400));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error deleting PO '{orderNo}': {ex.Message}", ex);
                return Task.FromResult(Results.Json(new DeletePOResponse { Success = false, Message = $"Server error: {ex.Message}" }, statusCode: 500));
            }
        }

        #endregion

        #region Codes Endpoints

        public static Task<IResult> HandleGetCodes(string orderNo, int? status = null, string? cartonCode = null, int limit = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new { success = false, message = "orderNo is required." }));

                var result = GProduction.POLoader.GetCodes(orderNo, status, cartonCode);
                if (!result.IsSuccess || result.Data == null)
                    return Task.FromResult(Results.Json(new { success = false, message = result.Message }, statusCode: 404));

                var codes = new List<object>();
                int count = 0;
                foreach (System.Data.DataRow row in result.Data.Rows)
                {
                    if (count++ >= limit) break;
                    codes.Add(new
                    {
                        id = Convert.ToInt32(row["ID"]),
                        code = row["Code"]?.ToString(),
                        status = Convert.ToInt32(row["Status"] ?? 0),
                        cartonCode = row["cartonCode"]?.ToString(),
                        activateDate = row["ActivateDate"]?.ToString(),
                        activateUser = row["ActivateUser"]?.ToString(),
                        packingDate = row["PackingDate"]?.ToString()
                    });
                }
                return Task.FromResult(Results.Json(new { success = true, count = codes.Count, data = codes }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error getting codes for PO '{orderNo}': {ex.Message}", ex);
                return Task.FromResult(Results.Json(new { success = false, message = ex.Message }, statusCode: 500));
            }
        }

        public static async Task<IResult> HandleActivateCode(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new { success = false, message = "orderNo is required." });

                var request = await context.Request.ReadFromJsonAsync<ActivateCodeRequest>();
                if (request == null || string.IsNullOrWhiteSpace(request.Code))
                    return Results.BadRequest(new { success = false, message = "code is required." });

                var result = GProduction.POActivator.ActivateCode(orderNo, request.Code,
                    request.ActivateDate ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    request.ActivateUser ?? "API",
                    request.ProductionDate ?? "");

                return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error activating code in PO '{orderNo}': {ex.Message}", ex);
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        public static async Task<IResult> HandlePackCode(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new { success = false, message = "orderNo is required." });

                var request = await context.Request.ReadFromJsonAsync<PackCodeRequest>();
                if (request == null || string.IsNullOrWhiteSpace(request.Code))
                    return Results.BadRequest(new { success = false, message = "code is required." });

                var result = GProduction.POPacking.PackCode(orderNo, request.Code, request.CartonCode,
                    request.PackingDate ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    request.PackingUser ?? "API",
                    request.ProductionDate ?? "");

                return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error packing code in PO '{orderNo}': {ex.Message}", ex);
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        #endregion

        #region Carton Endpoints

        public static Task<IResult> HandleGetCartons(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new { success = false, message = "orderNo is required." }));

                var result = GProduction.POCarton.GetAll(orderNo);
                if (!result.IsSuccess || result.Data == null)
                    return Task.FromResult(Results.Json(new { success = false, message = result.Message }, statusCode: 404));

                var cartons = new List<object>();
                foreach (System.Data.DataRow row in result.Data.Rows)
                {
                    cartons.Add(new
                    {
                        id = Convert.ToInt32(row["ID"]),
                        cartonCode = row["cartonCode"]?.ToString(),
                        startDatetime = row["Start_Datetime"]?.ToString(),
                        completedDatetime = row["Completed_Datetime"]?.ToString(),
                        activateUser = row["ActivateUser"]?.ToString(),
                        productionDate = row["ProductionDate"]?.ToString(),
                        status = row["Completed_Datetime"]?.ToString() != "0" ? "Closed" : (row["Start_Datetime"]?.ToString() != "0" ? "Open" : "Empty")
                    });
                }
                return Task.FromResult(Results.Json(new { success = true, count = cartons.Count, data = cartons }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error getting cartons for PO '{orderNo}': {ex.Message}", ex);
                return Task.FromResult(Results.Json(new { success = false, message = ex.Message }, statusCode: 500));
            }
        }

        public static async Task<IResult> HandleStartCarton(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new { success = false, message = "orderNo is required." });

                var request = await context.Request.ReadFromJsonAsync<CartonOperationRequest>();
                if (request == null || request.CartonId <= 0)
                    return Results.BadRequest(new { success = false, message = "cartonId is required." });

                var result = GProduction.POCarton.StartCarton(orderNo, request.CartonId, request.ActivateUser ?? "API");
                return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error starting carton in PO '{orderNo}': {ex.Message}", ex);
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        public static async Task<IResult> HandleCompleteCarton(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new { success = false, message = "orderNo is required." });

                var request = await context.Request.ReadFromJsonAsync<CartonOperationRequest>();
                if (request == null || request.CartonId <= 0)
                    return Results.BadRequest(new { success = false, message = "cartonId is required." });

                var result = GProduction.POCarton.CompleteCarton(orderNo, request.CartonId, request.ActivateUser ?? "API");
                return Results.Json(new ApiResponse { Success = result.IsSuccess, Message = result.Message });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error completing carton in PO '{orderNo}': {ex.Message}", ex);
                return Results.Json(new ApiResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        #endregion

        #region Production Control

        public static Task<IResult> HandleGetProductionStatus()
        {
            try
            {
                return Task.FromResult(Results.Json(new ProductionStatusResponse
                {
                    Success = true,
                    State = "Ready",
                    HasPO = false,
                    ProgressPercent = 0
                }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error getting production status: {ex.Message}", ex);
                return Task.FromResult(Results.Json(new ProductionStatusResponse { Success = false, Message = ex.Message }, statusCode: 500));
            }
        }

        #endregion

        #region Helpers

        private static void LogInfo(string source, string message)
        {
            Log.Information("[{Source}] {Message}", source, message);
        }

        private static void LogError(string source, string message, Exception? ex = null)
        {
            if (ex != null)
                Log.Error(ex, "[{Source}] {Message}", source, message);
            else
                Log.Error("[{Source}] {Message}", source, message);
        }

        #endregion
    }
}
