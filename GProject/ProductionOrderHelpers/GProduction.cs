namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// GProduction - Main wrapper class cho Production Order Management
    /// 
    /// Sử dụng:
    ///   GProduction.POLoader.GetAll();
    ///   GProduction.POCreator.InitPO("PO001");
    ///   GProduction.PORecordHelper.ActivateCode("PO001", "code123", ...);
    ///   GProduction.PORecordHelper.PackCode("PO001", "code123", "CTN001", ...);
    /// </summary>
    public static class GProduction
    {
        /// <summary>
        /// Khởi tạo Production Helper
        /// </summary>
        public static void Initialize()
        {
            Config.EnsurePOList();
            Config.EnsurePOHistory();
        }

        /// <summary>
        /// CRUD cho PO List
        /// </summary>
        public static class POLoader
        {
            public static void EnsurePOList() => ProductionOrderHelpers.POLoader.EnsurePOList();
            public static Result GetAll() => ProductionOrderHelpers.POLoader.GetAll();
            public static Result GetByOrderNo(string orderNo) => ProductionOrderHelpers.POLoader.GetByOrderNo(orderNo);
            public static Result GetByGTIN(string gtin) => ProductionOrderHelpers.POLoader.GetByGTIN(gtin);
            public static Result Create(POInfo po) => ProductionOrderHelpers.POLoader.Create(po);
            public static Result Update(string orderNo, POInfo po) => ProductionOrderHelpers.POLoader.Update(orderNo, po);
            public static Result Delete(string orderNo) => ProductionOrderHelpers.POLoader.Delete(orderNo);
            public static bool Exists(string orderNo) => ProductionOrderHelpers.POLoader.Exists(orderNo);
            public static Result GetCodes(string orderNo, int? status = null, string? cartonCode = null, int limit = 100, int offset = 0)
                => ProductionOrderHelpers.POLoader.GetCodes(orderNo, status, cartonCode, limit, offset);
            public static int CountCodes(string orderNo, int? status = null, string? cartonCode = null)
                => ProductionOrderHelpers.POLoader.CountCodes(orderNo, status, cartonCode);
            public static Result GetCodeByCode(string orderNo, string code)
                => ProductionOrderHelpers.POLoader.GetCodeByCode(orderNo, code);
            public static bool CodeExists(string orderNo, string code)
                => ProductionOrderHelpers.POLoader.CodeExists(orderNo, code);
            public static int GetCodeCount(string orderNo, int? status = null)
                => ProductionOrderHelpers.POLoader.GetCodeCount(orderNo, status);
            public static (bool success, string message, int loadedCount) LoadCodesFromDataPool(string orderNo, string gtin, int? limitQty = null)
                => ProductionOrderHelpers.POLoader.LoadCodesFromDataPool(orderNo, gtin, limitQty);
        }

        /// <summary>
        /// Tạo database PO
        /// </summary>
        public static class POCreator
        {
            public static Result InitPO(string orderNo) => ProductionOrderHelpers.POCreator.InitPO(orderNo);
            public static (bool success, string message, int createdCount) CreateEmptyCartons(string orderNo, int count)
                => ProductionOrderHelpers.POCreator.CreateEmptyCartons(orderNo, count);
            public static (bool success, string message, int createdCount) CreateRequiredCartons(string orderNo, int orderQty, int cartonCapacity = 24)
                => ProductionOrderHelpers.POCreator.CreateRequiredCartons(orderNo, orderQty, cartonCapacity);
            public static PODatabaseStatus CheckPODatabaseStatus(string orderNo, int orderQty = 0, int cartonCapacity = 24)
                => ProductionOrderHelpers.POCreator.CheckPODatabaseStatus(orderNo, orderQty, cartonCapacity);
            public static (bool success, string message, int codesLoaded, int cartonsCreated) EnsurePODatabaseReady(
                string orderNo, string gtin, int orderQty, int cartonCapacity = 24, bool autoLoadCodes = true)
                => ProductionOrderHelpers.POCreator.EnsurePODatabaseReady(orderNo, gtin, orderQty, cartonCapacity, autoLoadCodes);
        }

        /// <summary>
        /// Camera - Ghi log và xử lý mã
        /// </summary>
        public static class PORecordHelper
        {
            public static Result Record(string orderNo, RecordData data)
                => ProductionOrderHelpers.PORecord.Record(orderNo, data);
            public static Result ActivateCode(string orderNo, string code, string activateDate, string activateUser, string productionDate)
                => ProductionOrderHelpers.PORecord.ActivateCode(orderNo, code, activateDate, activateUser, productionDate);
            public static Result DeactivateCode(string orderNo, string code)
                => ProductionOrderHelpers.PORecord.DeactivateCode(orderNo, code);
            public static Result PackCode(string orderNo, string code, string cartonCode, string packingDate, string packingUser, string productionDate)
                => ProductionOrderHelpers.PORecord.PackCode(orderNo, code, cartonCode, packingDate, packingUser, productionDate);
            public static Result UnpackCode(string orderNo, string code)
                => ProductionOrderHelpers.PORecord.UnpackCode(orderNo, code);
            public static int GetActiveCount(string orderNo)
                => ProductionOrderHelpers.PORecord.GetActiveCount(orderNo);
            public static int GetUnusedCount(string orderNo)
                => ProductionOrderHelpers.PORecord.GetUnusedCount(orderNo);
            public static int GetPackedCount(string orderNo)
                => ProductionOrderHelpers.PORecord.GetPackedCount(orderNo);
            public static int GetCodeCountInCarton(string orderNo, string cartonCode)
                => ProductionOrderHelpers.PORecord.GetCodeCountInCarton(orderNo, cartonCode);
        }

        /// <summary>
        /// Quản lý thùng carton
        /// </summary>
        public static class POCarton
        {
            public static Result Create(string orderNo, CartonData data)
                => ProductionOrderHelpers.POCarton.Create(orderNo, data);
            public static Result Update(string orderNo, CartonData data)
                => ProductionOrderHelpers.POCarton.Update(orderNo, data);
            public static Result Delete(string orderNo, int cartonId)
                => ProductionOrderHelpers.POCarton.Delete(orderNo, cartonId);
            public static Result GetAll(string orderNo)
                => ProductionOrderHelpers.POCarton.GetAll(orderNo);
            public static Result GetByCartonCode(string orderNo, string cartonCode)
                => ProductionOrderHelpers.POCarton.GetByCartonCode(orderNo, cartonCode);
            public static Result StartCarton(string orderNo, int cartonId, string activateUser)
                => ProductionOrderHelpers.POCarton.StartCarton(orderNo, cartonId, activateUser);
            public static Result CompleteCarton(string orderNo, int cartonId, string activateUser)
                => ProductionOrderHelpers.POCarton.CompleteCarton(orderNo, cartonId, activateUser);
            public static Result ResetCarton(string orderNo, int cartonId)
                => ProductionOrderHelpers.POCarton.ResetCarton(orderNo, cartonId);
            public static int GetTotalCartonCount(string orderNo)
                => ProductionOrderHelpers.POCarton.GetTotalCartonCount(orderNo);
            public static int GetClosedCartonCount(string orderNo)
                => ProductionOrderHelpers.POCarton.GetClosedCartonCount(orderNo);
            public static int GetMaxCartonId(string orderNo)
                => ProductionOrderHelpers.POCarton.GetMaxCartonId(orderNo);
        }

        /// <summary>
        /// Lịch sử PO
        /// </summary>
        public static class POHistoryManager
        {
            public static void EnsureHistoryDB() => ProductionOrderHelpers.POHistoryManager.EnsureHistoryDB();
            public static Result RecordStart(string orderNo, string productionDate, string userName)
                => ProductionOrderHelpers.POHistoryManager.RecordStart(orderNo, productionDate, userName);
            public static Result RecordEnd(string orderNo)
                => ProductionOrderHelpers.POHistoryManager.RecordEnd(orderNo);
            public static POHistoryData? GetLastRunningPO()
                => ProductionOrderHelpers.POHistoryManager.GetLastRunningPO();
            public static POHistoryData? GetLastPO()
                => ProductionOrderHelpers.POHistoryManager.GetLastPO();
            public static Result GetByOrderNo(string orderNo)
                => ProductionOrderHelpers.POHistoryManager.GetByOrderNo(orderNo);
            public static bool IsPORunning(string orderNo)
                => ProductionOrderHelpers.POHistoryManager.IsPORunning(orderNo);
        }

        /// <summary>
        /// Load codes vào Dictionary cho lookup nhanh
        /// </summary>
        public static class CodeDictionaryLoader
        {
            public static (bool success, string message, int count) LoadAllCodesToDictionary(
                string orderNo, Dictionary<string, CodeInfo> dictionary)
                => ProductionOrderHelpers.CodeDictionaryLoader.LoadAllCodesToDictionary(orderNo, dictionary);

            public static (bool success, string message, int count) LoadUnusedCodesToDictionary(
                string orderNo, Dictionary<string, CodeInfo> dictionary)
                => ProductionOrderHelpers.CodeDictionaryLoader.LoadUnusedCodesToDictionary(orderNo, dictionary);

            public static Result ActivateCodeInDatabase(string orderNo, string code,
                string activateDate, string activateUser, string productionDate)
                => ProductionOrderHelpers.CodeDictionaryLoader.ActivateCodeInDatabase(orderNo, code, activateDate, activateUser, productionDate);

            public static Result PackCodeInDatabase(string orderNo, string code,
                string cartonCode, string packingDate, string packingUser, string productionDate)
                => ProductionOrderHelpers.CodeDictionaryLoader.PackCodeInDatabase(orderNo, code, cartonCode, packingDate, packingUser, productionDate);

            public static bool FindCode(Dictionary<string, CodeInfo> dictionary, string code, out CodeInfo? info)
                => ProductionOrderHelpers.CodeDictionaryLoader.FindCode(dictionary, code, out info);
        }

        /// <summary>
        /// Load cartons vào Dictionary cho lookup nhanh
        /// </summary>
        public static class CartonDictionaryLoader
        {
            public static (bool success, string message, int count) LoadAllCartonsToDictionary(
                string orderNo, Dictionary<int, CartonInfo> dictionary)
                => ProductionOrderHelpers.CartonDictionaryLoader.LoadAllCartonsToDictionary(orderNo, dictionary);

            public static (bool success, string message, int count) CreateAndLoadCartons(
                string orderNo, int count, Dictionary<int, CartonInfo> dictionary)
                => ProductionOrderHelpers.CartonDictionaryLoader.CreateAndLoadCartons(orderNo, count, dictionary);

            public static Result StartCartonInDatabase(string orderNo, int cartonId, string activateUser)
                => ProductionOrderHelpers.CartonDictionaryLoader.StartCartonInDatabase(orderNo, cartonId, activateUser);

            public static Result CompleteCartonInDatabase(string orderNo, int cartonId, string activateUser)
                => ProductionOrderHelpers.CartonDictionaryLoader.CompleteCartonInDatabase(orderNo, cartonId, activateUser);

            public static Result ResetCartonInDatabase(string orderNo, int cartonId)
                => ProductionOrderHelpers.CartonDictionaryLoader.ResetCartonInDatabase(orderNo, cartonId);

            public static bool FindCarton(Dictionary<int, CartonInfo> dictionary, int cartonId, out CartonInfo? info)
                => ProductionOrderHelpers.CartonDictionaryLoader.FindCarton(dictionary, cartonId, out info);

            public static int? FindNextOpenCarton(Dictionary<int, CartonInfo> dictionary)
                => ProductionOrderHelpers.CartonDictionaryLoader.FindNextOpenCarton(dictionary);
        }
    }
}
