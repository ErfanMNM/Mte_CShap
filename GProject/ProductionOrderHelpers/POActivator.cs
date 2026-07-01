using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Xử lý camera active - kích hoạt mã sản phẩm
    /// </summary>
    public static class POActivator
    {
        /// <summary>
        /// Ghi bản ghi từ camera active
        /// </summary>
        public static Result Record(string orderNo, RecordData data)
        {
            try
            {
                string dbPath = Config.GetRecordActivePath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Record_Active '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"INSERT INTO Records (Code, cartonCode, Status, PLC_Status, ActivateDate, ActivateUser, ProductionDate)
                                     VALUES (@Code, @cartonCode, @Status, @PLC_Status, @ActivateDate, @ActivateUser, @ProductionDate);";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", data.Code ?? "FAIL");
                cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                cmd.Parameters.AddWithValue("@Status", data.Status ?? "0");
                cmd.Parameters.AddWithValue("@PLC_Status", data.PLCStatus ?? "FAIL");
                cmd.Parameters.AddWithValue("@ActivateDate", data.ActivateDate ?? "0");
                cmd.Parameters.AddWithValue("@ActivateUser", data.ActivateUser ?? "");
                cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                cmd.ExecuteNonQuery();

                return Result.Success($"Ghi record Active '{data.Code}' thành công.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi ghi Record Active: {ex.Message}");
            }
        }

        /// <summary>
        /// Kích hoạt một mã
        /// </summary>
        public static Result ActivateCode(string orderNo, string code, string activateDate, string activateUser, string productionDate)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"UPDATE UniqueCodes SET Status = 1, ActivateDate = COALESCE(NULLIF(@ActivateDate, ''), ActivateDate),
                                     ActivateUser = COALESCE(NULLIF(@ActivateUser, ''), ActivateUser),
                                     ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                                     WHERE Code = @Code;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@ActivateDate", activateDate ?? "");
                cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                int rows = cmd.ExecuteNonQuery();
                return rows > 0
                    ? Result.Success($"Activate code '{code}' thành công.")
                    : Result.Fail($"Không tìm thấy mã: {code}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi activate code: {ex.Message}");
            }
        }

        /// <summary>
        /// Hủy kích hoạt một mã
        /// </summary>
        public static Result DeactivateCode(string orderNo, string code)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"UPDATE UniqueCodes SET Status = 0, ActivateDate = '0', ActivateUser = '', cartonCode = '0' WHERE Code = @Code;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", code);
                int rows = cmd.ExecuteNonQuery();
                return rows > 0
                    ? Result.Success($"Deactivate code '{code}' thành công.")
                    : Result.Fail($"Không tìm thấy mã: {code}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi deactivate code: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy số mã đã active trong PO
        /// </summary>
        public static int GetActiveCount(string orderNo) => POLoader.GetCodeCount(orderNo, status: 1);

        /// <summary>
        /// Lấy số mã chưa active trong PO
        /// </summary>
        public static int GetUnusedCount(string orderNo) => POLoader.GetCodeCount(orderNo, status: 0);
    }
}
