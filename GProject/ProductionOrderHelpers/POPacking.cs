using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Xử lý camera packing - đóng mã vào thùng
    /// </summary>
    public static class POPacking
    {
        /// <summary>
        /// Ghi bản ghi từ camera packing
        /// </summary>
        public static Result Record(string orderNo, RecordData data)
        {
            try
            {
                string dbPath = Config.GetRecordPackingPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Record_Packing '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"INSERT INTO Records (Code, cartonCode, Status, PLC_Status, PackingDate, PackingUser, ProductionDate)
                                     VALUES (@Code, @cartonCode, @Status, @PLC_Status, @PackingDate, @PackingUser, @ProductionDate);";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", data.Code ?? "FAIL");
                cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                cmd.Parameters.AddWithValue("@Status", data.Status ?? "0");
                cmd.Parameters.AddWithValue("@PLC_Status", data.PLCStatus ?? "FAIL");
                cmd.Parameters.AddWithValue("@PackingDate", data.PackingDate ?? "0");
                cmd.Parameters.AddWithValue("@PackingUser", data.PackingUser ?? "");
                cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                cmd.ExecuteNonQuery();

                return Result.Success($"Ghi record Packing '{data.Code}' thành công.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi ghi Record Packing: {ex.Message}");
            }
        }

        /// <summary>
        /// Đóng một mã vào thùng
        /// </summary>
        public static Result PackCode(string orderNo, string code, string cartonCode, string packingDate, string packingUser, string productionDate)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"UPDATE UniqueCodes SET cartonCode = @cartonCode,
                                     PackingDate = COALESCE(NULLIF(@PackingDate, ''), PackingDate),
                                     ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                                     WHERE Code = @Code;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@cartonCode", cartonCode ?? "0");
                cmd.Parameters.AddWithValue("@PackingDate", packingDate ?? "");
                cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                int rows = cmd.ExecuteNonQuery();
                return rows > 0
                    ? Result.Success($"Pack code '{code}' vào thùng '{cartonCode}' thành công.")
                    : Result.Fail($"Không tìm thấy mã: {code}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi pack code: {ex.Message}");
            }
        }

        /// <summary>
        /// Gỡ một mã khỏi thùng
        /// </summary>
        public static Result UnpackCode(string orderNo, string code)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = "UPDATE UniqueCodes SET cartonCode = '0', PackingDate = '0' WHERE Code = @Code;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", code);
                int rows = cmd.ExecuteNonQuery();
                return rows > 0
                    ? Result.Success($"Unpack code '{code}' thành công.")
                    : Result.Fail($"Không tìm thấy mã: {code}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi unpack code: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy số mã đã đóng thùng
        /// </summary>
        public static int GetPackedCount(string orderNo)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath)) return 0;
                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM UniqueCodes WHERE cartonCode <> '0';";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        /// <summary>
        /// Lấy số mã trong một thùng cụ thể
        /// </summary>
        public static int GetCodeCountInCarton(string orderNo, string cartonCode)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath)) return 0;
                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM UniqueCodes WHERE cartonCode = @cartonCode;";
                cmd.Parameters.AddWithValue("@cartonCode", cartonCode);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }
    }
}
