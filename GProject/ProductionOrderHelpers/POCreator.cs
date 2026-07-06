using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Trạng thái file database của PO
    /// </summary>
    public class DBFileStatus
    {
        public string FileName { get; set; } = "";
        public string Path { get; set; } = "";
        public bool Exists { get; set; }
        public long? FileSize { get; set; }
    }

    /// <summary>
    /// Trạng thái tổng hợp database của PO
    /// </summary>
    public class PODatabaseStatus
    {
        public string OrderNo { get; set; } = "";
        public bool IsFullyInitialized { get; set; }
        public List<DBFileStatus> Files { get; set; } = new();
        public int TotalCodes { get; set; }
        public int LoadedCodes { get; set; }
        public int RequiredCartons { get; set; }
        public int CreatedCartons { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Tạo và khởi tạo database files cho PO
    /// </summary>
    public static class POCreator
    {
        /// <summary>
        /// Khởi tạo tất cả database files cho PO
        /// </summary>
        public static Result InitPO(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                string basePath = Config.GetBasePathByOrderNo(orderNo);
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                // Tạo database theo thứ tự: UniqueCodes -> Record -> Carton
                CreateDatabaseIfNotExists(Config.GetPODBPath(orderNo), Config.SQL_CREATE_PO_CODES);
                CreateDatabaseIfNotExists(Config.GetRecordPath(orderNo), Config.SQL_CREATE_RECORD);
                CreateDatabaseIfNotExists(Config.GetCartonPath(orderNo), Config.SQL_CREATE_CARTON);

                return Result.Success($"Khởi tạo PO '{orderNo}' thành công.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi khởi tạo PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo database file nếu chưa tồn tại
        /// </summary>
        private static void CreateDatabaseIfNotExists(string dbPath, string sqlSchema)
        {
            if (File.Exists(dbPath))
                return;

            string folder = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using var con = new SqliteConnection($"Data Source={dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sqlSchema;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Tạo N thùng trống cho PO
        /// </summary>
        public static (bool success, string message, int createdCount) CreateEmptyCartons(string orderNo, int count)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return (false, "Carton DB chưa tồn tại.", 0);

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                using var tx = con.BeginTransaction();
                const string sql = @"INSERT INTO Carton (cartonCode, Start_Datetime, Completed_Datetime, ActivateUser, ProductionDate) 
                                     VALUES ('0', '0', '0', 'System', '0');";
                for (int i = 0; i < count; i++)
                {
                    using var cmd = new SqliteCommand(sql, con, tx);
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
                return (true, $"Tạo {count} thùng trống thành công.", count);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thùng: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Tạo đầy đủ số thùng cần thiết cho PO (dựa trên orderQty và cartonCapacity)
        /// </summary>
        public static (bool success, string message, int createdCount) CreateRequiredCartons(string orderNo, int orderQty, int cartonCapacity = 24)
        {
            try
            {
                // Tính số thùng cần thiết
                int requiredCartons = (int)Math.Ceiling((double)orderQty / cartonCapacity);
                return CreateEmptyCartons(orderNo, requiredCartons);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thùng: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái database files của PO
        /// </summary>
        public static PODatabaseStatus CheckPODatabaseStatus(string orderNo, int orderQty = 0, int cartonCapacity = 24)
        {
            var status = new PODatabaseStatus { OrderNo = orderNo };

            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    status.Message = "orderNo không được trống.";
                    return status;
                }

                // Kiểm tra 3 file database
                var dbFiles = new (string name, string path)[]
                {
                    ("UniqueCodes", Config.GetPODBPath(orderNo)),
                    ("Record", Config.GetRecordPath(orderNo)),
                    ("Carton", Config.GetCartonPath(orderNo))
                };

                bool allFilesExist = true;
                foreach (var (name, path) in dbFiles)
                {
                    var fileInfo = new FileInfo(path);
                    var fileStatus = new DBFileStatus
                    {
                        FileName = Path.GetFileName(path),
                        Path = path,
                        Exists = fileInfo.Exists,
                        FileSize = fileInfo.Exists ? fileInfo.Length : null
                    };
                    status.Files.Add(fileStatus);
                    if (!fileInfo.Exists) allFilesExist = false;
                }

                status.IsFullyInitialized = allFilesExist;

                // Lấy thông tin codes từ UniqueCodes
                string podbPath = Config.GetPODBPath(orderNo);
                if (File.Exists(podbPath))
                {
                    try
                    {
                        using var con = new SqliteConnection($"Data Source={podbPath}");
                        con.Open();

                        using var cmd = con.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM UniqueCodes";
                        status.LoadedCodes = Convert.ToInt32(cmd.ExecuteScalar());

                        status.TotalCodes = orderQty > 0 ? orderQty : status.LoadedCodes;
                    }
                    catch { }
                }

                // Lấy thông tin cartons
                string cartonPath = Config.GetCartonPath(orderNo);
                if (File.Exists(cartonPath))
                {
                    try
                    {
                        using var con = new SqliteConnection($"Data Source={cartonPath}");
                        con.Open();

                        using var cmd = con.CreateCommand();
                        cmd.CommandText = "SELECT COUNT(*) FROM Carton";
                        status.CreatedCartons = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch { }
                }

                // Tính số thùng cần thiết
                status.RequiredCartons = orderQty > 0 && cartonCapacity > 0
                    ? (int)Math.Ceiling((double)orderQty / cartonCapacity)
                    : status.CreatedCartons;

                // Tạo message
                if (status.IsFullyInitialized && status.LoadedCodes >= status.TotalCodes && status.CreatedCartons >= status.RequiredCartons)
                {
                    status.IsFullyInitialized = true;
                    status.Message = "PO ready - all files initialized and data loaded.";
                }
                else
                {
                    status.IsFullyInitialized = false;
                    var issues = new List<string>();
                    if (!allFilesExist) issues.Add("missing db files");
                    if (status.LoadedCodes < status.TotalCodes) issues.Add($"codes ({status.LoadedCodes}/{status.TotalCodes})");
                    if (status.CreatedCartons < status.RequiredCartons) issues.Add($"cartons ({status.CreatedCartons}/{status.RequiredCartons})");
                    status.Message = $"PO not ready: {string.Join(", ", issues)}";
                }

                return status;
            }
            catch (Exception ex)
            {
                status.Message = $"Lỗi kiểm tra: {ex.Message}";
                return status;
            }
        }

        /// <summary>
        /// Đảm bảo PO database sẵn sàng, tự tạo/cập nhật nếu thiếu
        /// </summary>
        public static (bool success, string message, int codesLoaded, int cartonsCreated) EnsurePODatabaseReady(
            string orderNo, string gtin, int orderQty, int cartonCapacity = 24, bool autoLoadCodes = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return (false, "orderNo không được trống.", 0, 0);

                int codesLoaded = 0;
                int cartonsCreated = 0;

                // Kiểm tra trạng thái hiện tại
                var status = CheckPODatabaseStatus(orderNo, orderQty, cartonCapacity);

                // 1. Khởi tạo database files nếu thiếu
                if (!status.IsFullyInitialized)
                {
                    var initResult = InitPO(orderNo);
                    if (!initResult.IsSuccess)
                        return (false, $"Lỗi khởi tạo: {initResult.Message}", 0, 0);
                }

                // 2. Nạp mã từ DataPool nếu cần
                if (autoLoadCodes && !string.IsNullOrWhiteSpace(gtin))
                {
                    // Kiểm tra lại sau khi init
                    status = CheckPODatabaseStatus(orderNo, orderQty, cartonCapacity);
                    if (status.LoadedCodes < orderQty)
                    {
                        var loadResult = POLoader.LoadCodesFromDataPool(orderNo, gtin);
                        if (loadResult.success)
                            codesLoaded = loadResult.loadedCount;
                    }
                }

                // 3. Tạo thùng nếu thiếu
                if (orderQty > 0 && cartonCapacity > 0)
                {
                    status = CheckPODatabaseStatus(orderNo, orderQty, cartonCapacity);
                    if (status.CreatedCartons < status.RequiredCartons)
                    {
                        var cartonResult = CreateRequiredCartons(orderNo, orderQty, cartonCapacity);
                        if (cartonResult.success)
                            cartonsCreated = cartonResult.createdCount;
                    }
                }

                // Kiểm tra kết quả cuối cùng
                var finalStatus = CheckPODatabaseStatus(orderNo, orderQty, cartonCapacity);
                if (finalStatus.IsFullyInitialized)
                {
                    return (true, $"PO ready. Loaded {codesLoaded} codes, {cartonsCreated} cartons.", codesLoaded, cartonsCreated);
                }
                else
                {
                    return (false, $"PO partially ready. {finalStatus.Message}", codesLoaded, cartonsCreated);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}", 0, 0);
            }
        }
    }
}
