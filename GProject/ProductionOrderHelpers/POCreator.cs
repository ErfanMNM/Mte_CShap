using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
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

                // Tạo database theo thứ tự: UniqueCodes -> Record_Active -> Record_Packing -> Carton
                CreateDatabaseIfNotExists(Config.GetPODBPath(orderNo), Config.SQL_CREATE_PO_CODES);
                CreateDatabaseIfNotExists(Config.GetRecordActivePath(orderNo), Config.SQL_CREATE_RECORD_ACTIVE);
                CreateDatabaseIfNotExists(Config.GetRecordPackingPath(orderNo), Config.SQL_CREATE_RECORD_PACKING);
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
    }
}
