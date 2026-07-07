using System.Linq;
using System.Text.Json.Serialization;
using GProject.Production;
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
        [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
        [JsonPropertyName("passCount")] public int PassCount { get; set; }
        [JsonPropertyName("failCount")] public int FailCount { get; set; }
        [JsonPropertyName("duplicateCount")] public int DuplicateCount { get; set; }
        [JsonPropertyName("currentCartonId")] public int CurrentCartonId { get; set; }
        [JsonPropertyName("currentCartonCode")] public string CurrentCartonCode { get; set; } = "";
        [JsonPropertyName("itemsInCarton")] public int ItemsInCarton { get; set; }
        [JsonPropertyName("cartonCapacity")] public int CartonCapacity { get; set; }
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

    public class EnsureReadyRequest
    {
        [JsonPropertyName("autoLoadCodes")] public bool AutoLoadCodes { get; set; } = true;
        [JsonPropertyName("cartonCapacity")] public int CartonCapacity { get; set; } = 24;
    }

    public class PODatabaseStatusResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("orderNo")] public string OrderNo { get; set; } = "";
        [JsonPropertyName("isFullyInitialized")] public bool IsFullyInitialized { get; set; }
        [JsonPropertyName("files")] public List<DBFileStatus> Files { get; set; } = new();
        [JsonPropertyName("totalCodes")] public int TotalCodes { get; set; }
        [JsonPropertyName("loadedCodes")] public int LoadedCodes { get; set; }
        [JsonPropertyName("requiredCartons")] public int RequiredCartons { get; set; }
        [JsonPropertyName("createdCartons")] public int CreatedCartons { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
    }

    public class EnsureReadyResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; } = "";
        [JsonPropertyName("codesLoaded")] public int CodesLoaded { get; set; }
        [JsonPropertyName("cartonsCreated")] public int CartonsCreated { get; set; }
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
                int activeCodes = GProduction.PORecordHelper.GetActiveCount(orderNo);
                int packedCodes = GProduction.PORecordHelper.GetPackedCount(orderNo);
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

                int usedCodes = GProduction.PORecordHelper.GetActiveCount(orderNo);
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

                int usedCodes = GProduction.PORecordHelper.GetActiveCount(orderNo);
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

        public static Task<IResult> HandleCheckPOStatus(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new PODatabaseStatusResponse { Success = false, Message = "orderNo is required." }));

                if (!GProduction.POLoader.Exists(orderNo))
                    return Task.FromResult(Results.NotFound(new PODatabaseStatusResponse { Success = false, Message = $"PO '{orderNo}' not found." }));

                // Lấy thông tin PO để biết orderQty
                var poResult = GProduction.POLoader.GetByOrderNo(orderNo);
                int orderQty = 0;
                string gtin = "";
                if (poResult.IsSuccess && poResult.Data != null && poResult.Data.Rows.Count > 0)
                {
                    orderQty = Convert.ToInt32(poResult.Data.Rows[0]["orderQty"]);
                    gtin = poResult.Data.Rows[0]["gtin"]?.ToString() ?? "";
                }

                var status = GProduction.POCreator.CheckPODatabaseStatus(orderNo, orderQty);

                return Task.FromResult(Results.Json(new PODatabaseStatusResponse
                {
                    Success = true,
                    OrderNo = status.OrderNo,
                    IsFullyInitialized = status.IsFullyInitialized,
                    Files = status.Files,
                    TotalCodes = status.TotalCodes,
                    LoadedCodes = status.LoadedCodes,
                    RequiredCartons = status.RequiredCartons,
                    CreatedCartons = status.CreatedCartons,
                    Message = status.Message
                }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error checking PO status '{orderNo}': {ex.Message}", ex);
                return Task.FromResult(Results.Json(new PODatabaseStatusResponse { Success = false, Message = ex.Message }, statusCode: 500));
            }
        }

        public static async Task<IResult> HandleEnsurePODatabaseReady(HttpContext context, string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Results.BadRequest(new EnsureReadyResponse { Success = false, Message = "orderNo is required." });

                if (!GProduction.POLoader.Exists(orderNo))
                    return Results.NotFound(new EnsureReadyResponse { Success = false, Message = $"PO '{orderNo}' not found." });

                var request = await context.Request.ReadFromJsonAsync<EnsureReadyRequest>();
                bool autoLoadCodes = request?.AutoLoadCodes ?? true;
                int cartonCapacity = request?.CartonCapacity ?? 24;

                // Lấy thông tin PO
                var poResult = GProduction.POLoader.GetByOrderNo(orderNo);
                if (!poResult.IsSuccess || poResult.Data == null || poResult.Data.Rows.Count == 0)
                    return Results.Json(new EnsureReadyResponse { Success = false, Message = "PO not found." }, statusCode: 404);

                var row = poResult.Data.Rows[0];
                string gtin = row["gtin"]?.ToString() ?? "";
                int orderQty = Convert.ToInt32(row["orderQty"]);

                var result = GProduction.POCreator.EnsurePODatabaseReady(orderNo, gtin, orderQty, cartonCapacity, autoLoadCodes);

                LogInfo("POApiServer", $"EnsurePODatabaseReady({orderNo}): {result.message}");

                return Results.Json(new EnsureReadyResponse
                {
                    Success = result.success,
                    Message = result.message,
                    CodesLoaded = result.codesLoaded,
                    CartonsCreated = result.cartonsCreated
                });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error ensuring PO ready '{orderNo}': {ex.Message}", ex);
                return Results.Json(new EnsureReadyResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        #endregion

        #region Codes Endpoints

        public static Task<IResult> HandleGetCodes(string orderNo, int? status = null, string? cartonCode = null, int limit = 100, int offset = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Task.FromResult(Results.BadRequest(new { success = false, message = "orderNo is required." }));

                // Get total count for pagination
                int totalCount = GProduction.POLoader.CountCodes(orderNo, status, cartonCode);

                var result = GProduction.POLoader.GetCodes(orderNo, status, cartonCode, limit, offset);
                if (!result.IsSuccess || result.Data == null)
                    return Task.FromResult(Results.Json(new { success = false, message = result.Message }, statusCode: 404));

                var codes = new List<object>();
                foreach (System.Data.DataRow row in result.Data.Rows)
                {
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
                return Task.FromResult(Results.Json(new { success = true, count = codes.Count, total = totalCount, offset = offset, limit = limit, data = codes }));
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

                var result = GProduction.PORecordHelper.ActivateCode(orderNo, request.Code,
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

                var result = GProduction.PORecordHelper.PackCode(orderNo, request.Code, request.CartonCode,
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
                var sm = ProductionStateMachine.Instance;
                int packedCodes = 0;
                int cartonCount = 0, closedCartons = 0;
                string? orderNo = null, productName = null;
                int orderQty = 0;

                if (ProductionStateMachine.ProductionData != null)
                {
                    orderNo = ProductionStateMachine.ProductionData.OrderNo;
                    productName = ProductionStateMachine.ProductionData.ProductName;
                    orderQty = ProductionStateMachine.ProductionData.OrderQty;
                    packedCodes = GProject.ProductionOrderHelpers.GProduction.PORecordHelper.GetPackedCount(orderNo);
                    cartonCount = GProject.ProductionOrderHelpers.GProduction.POCarton.GetTotalCartonCount(orderNo);
                    closedCartons = GProject.ProductionOrderHelpers.GProduction.POCarton.GetClosedCartonCount(orderNo);
                }

                return Task.FromResult(Results.Json(new ProductionStatusResponse
                {
                    Success = true,
                    State = sm.CurrentState.ToString(),
                    HasPO = ProductionStateMachine.ProductionData != null,
                    OrderNo = orderNo,
                    ProductName = productName,
                    OrderQty = orderQty,
                    TotalCount = sm.ActiveCounter.TotalCount,
                    PassCount = sm.ActiveCounter.PassCount,
                    FailCount = sm.ActiveCounter.FailCount,
                    DuplicateCount = sm.ActiveCounter.DuplicateCount,
                    CartonCount = cartonCount,
                    ClosedCartonCount = closedCartons,
                    CurrentCartonId = sm.ActiveCounter.CartonID,
                    CurrentCartonCode = sm.ActiveCounter.CartonCode,
                    ItemsInCarton = sm.PackageCounter.PassCount,
                    CartonCapacity = sm.ActiveCounter.CartonCapacity,
                    ProgressPercent = orderQty > 0 ? Math.Round((double)packedCodes / orderQty * 100, 2) : 0
                }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"Error getting production status: {ex.Message}", ex);
                return Task.FromResult(Results.Json(new ProductionStatusResponse { Success = false, Message = ex.Message }, statusCode: 500));
            }
        }

        #endregion

        #region Carton PDA Handlers

        /// <summary>POST /api/carton/scan</summary>
        public static async Task<IResult> HandleCartonScan(HttpContext context)
        {
            try
            {
                CartonScanRequest? req = await context.Request.ReadFromJsonAsync<CartonScanRequest>();
                if (req == null || string.IsNullOrWhiteSpace(req.CartonCode))
                    return Results.Json(new CartonScanResponse { Success = false, Message = "Invalid request" });

                var sm = ProductionStateMachine.Instance;
                var activeStates = new[] { e_ProductionState.Running, e_ProductionState.Paused };
                if (!activeStates.Contains(sm.CurrentState) || ProductionStateMachine.ProductionData == null)
                    return Results.Json(new CartonScanResponse { Success = false, Message = $"PO not running (state={sm.CurrentState})" });

                var task = new CartonWriteTask
                {
                    Type = CartonWriteType.ScanCarton,
                    OrderNo = ProductionStateMachine.ProductionData.OrderNo,
                    CartonCode = req.CartonCode,
                    MachineName = req.MachineName,
                    ScannedAt = req.ScannedAt,
                    Mode = req.Mode
                };

                var result = await CartonWriteQueueManager.EnqueueAsync(ProductionStateMachine.ProductionData.OrderNo, task);

                return Results.Json(new CartonScanResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Status = result.Status,
                    CartonIndex = result.CartonIndex,
                    OrderNo = ProductionStateMachine.ProductionData.OrderNo,
                    ProductCount = result.ProductCount,
                    ActivateDate = result.ActivateDate
                });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"HandleCartonScan error: {ex.Message}", ex);
                return Results.Json(new CartonScanResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>GET /api/carton/{cartonCode}/info</summary>
        public static IResult HandleCartonInfo(string cartonCode)
        {
            try
            {
                var sm = ProductionStateMachine.Instance;
                var activeStates = new[] { e_ProductionState.Running, e_ProductionState.Paused };
                if (!activeStates.Contains(sm.CurrentState) || ProductionStateMachine.ProductionData == null)
                    return Results.Json(new CartonInfoResponse { Success = false, Message = $"PO not running (state={sm.CurrentState})" });

                var info = POCartonCode.GetCartonInfo(ProductionStateMachine.ProductionData.OrderNo, cartonCode);

                return Results.Json(new CartonInfoResponse
                {
                    Success = info.Success,
                    CartonCode = info.CartonCode,
                    CartonIndex = info.CartonIndex,
                    ActivateDate = info.StartDatetime,
                    ActivateUser = info.ActivateUser,
                    ProductCount = info.ProductCount,
                    Status = info.Status,
                    Message = info.Message
                });
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"HandleCartonInfo error: {ex.Message}", ex);
                return Results.Json(new CartonInfoResponse { Success = false, Message = ex.Message }, statusCode: 500);
            }
        }

        /// <summary>GET /api/carton/current-po</summary>
        public static Task<IResult> HandleGetCurrentPO()
        {
            try
            {
                var sm = ProductionStateMachine.Instance;
                if (ProductionStateMachine.ProductionData == null)
                    return Task.FromResult(Results.Json(new CurrentPOResponse { Success = false, Message = "No PO loaded" }));

                return Task.FromResult(Results.Json(new CurrentPOResponse
                {
                    Success = true,
                    OrderNo = ProductionStateMachine.ProductionData.OrderNo,
                    ProductName = ProductionStateMachine.ProductionData.ProductName,
                    OrderQty = ProductionStateMachine.ProductionData.OrderQty,
                    State = sm.CurrentState.ToString()
                }));
            }
            catch (Exception ex)
            {
                LogError("POApiServer", $"HandleGetCurrentPO error: {ex.Message}", ex);
                return Task.FromResult(Results.Json(new CurrentPOResponse { Success = false, Message = ex.Message }, statusCode: 500));
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
