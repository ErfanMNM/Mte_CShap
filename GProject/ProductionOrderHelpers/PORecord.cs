using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Xử lý ghi log camera - thay thế POActivator (Record) và POPacking (Record)
    /// 1 camera duy nhất ghi vào 1 bảng Records chung
    /// </summary>
    public static class PORecord
    {
        /// <summary>
        /// Ghi bản ghi từ camera (chung cho cả activate và packing)
        /// </summary>
        public static Result Record(string orderNo, RecordData data)
        {
            try
            {
                string dbPath = Config.GetRecordPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Record DB '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"INSERT INTO Records (Code, cartonCode, Status, PLC_Status,
                                     ActivateDate, ActivateUser, PackingDate, PackingUser, ProductionDate)
                                     VALUES (@Code, @cartonCode, @Status, @PLC_Status,
                                     @ActivateDate, @ActivateUser, @PackingDate, @PackingUser, @ProductionDate);";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", data.Code ?? "FAIL");
                cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                cmd.Parameters.AddWithValue("@Status", data.Status ?? "0");
                cmd.Parameters.AddWithValue("@PLC_Status", data.PLCStatus ?? "FAIL");
                cmd.Parameters.AddWithValue("@ActivateDate", data.ActivateDate ?? "0");
                cmd.Parameters.AddWithValue("@ActivateUser", data.ActivateUser ?? "");
                cmd.Parameters.AddWithValue("@PackingDate", data.PackingDate ?? "0");
                cmd.Parameters.AddWithValue("@PackingUser", data.PackingUser ?? "");
                cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                cmd.ExecuteNonQuery();

                return Result.Success($"Ghi record '{data.Code}' thành công.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi ghi Record: {ex.Message}");
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
        /// Cập nhật trạng thái mã đồng thời: Status=1, ActivateDate, ActivateUser, cartonCode, PackingDate, ProductionDate
        /// Dùng cho camera - update nhãn ngay khi quét mã
        /// </summary>
        public static Result UpdateCodeStatusAndCarton(
            string orderNo, string code, string activateDate, string activateUser,
            string packingDate, string cartonCode, string productionDate)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = @"UPDATE UniqueCodes SET
                    Status = 1,
                    ActivateDate = COALESCE(NULLIF(@ActivateDate, ''), ActivateDate),
                    ActivateUser = COALESCE(NULLIF(@ActivateUser, ''), ActivateUser),
                    PackingDate = COALESCE(NULLIF(@PackingDate, ''), PackingDate),
                    cartonCode = COALESCE(NULLIF(@cartonCode, ''), cartonCode),
                    ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                    WHERE Code = @Code;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Code", code ?? "");
                cmd.Parameters.AddWithValue("@ActivateDate", activateDate ?? "");
                cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                cmd.Parameters.AddWithValue("@PackingDate", packingDate ?? "");
                cmd.Parameters.AddWithValue("@cartonCode", cartonCode ?? "0");
                cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                int rows = cmd.ExecuteNonQuery();

                return rows > 0
                    ? Result.Success($"Update code '{code}' thành công.")
                    : Result.Fail($"Không tìm thấy mã: {code}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi update code: {ex.Message}");
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
        /// Lấy số mã đã active trong PO (Status = 1)
        /// </summary>
        public static int GetActiveCount(string orderNo) => POLoader.GetCodeCount(orderNo, status: 1);

        /// <summary>
        /// Lấy số mã chưa active trong PO (Status = 0)
        /// </summary>
        public static int GetUnusedCount(string orderNo) => POLoader.GetCodeCount(orderNo, status: 0);

        /// <summary>
        /// Lấy số mã đã đóng thùng (cartonCode <> '0')
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
        /// Lấy mã thùng có nhiều sản phẩm đã đóng nhất
        /// </summary>
        public static string? GetLastPackedCartonCode(string orderNo)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath)) return null;
                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = @"SELECT cartonCode FROM UniqueCodes
                                    WHERE cartonCode <> '0'
                                    GROUP BY cartonCode
                                    ORDER BY COUNT(*) DESC
                                    LIMIT 1;";
                return cmd.ExecuteScalar()?.ToString();
            }
            catch { return null; }
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

        /// <summary>
        /// Tổng hợp counter từ bảng Records (Record_{orderNo}.db).
        /// Dùng khi PO resume sau khi restart để khôi phục FailTotal/FormatFail/... từ DB.
        /// Trả về: PassTotal, FailTotal, FormatFailCount, ReadFailCount,
        ///         TimeoutCount, DuplicateCount, NotFoundCount, ErrorCount, TotalCount
        /// </summary>
        public static ProductCounter GetCountersFromRecordDB(string orderNo)
        {
            var c = new ProductCounter();
            try
            {
                string dbPath = Config.GetRecordPath(orderNo);
                if (!File.Exists(dbPath)) return c;

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = @"SELECT
                      SUM(CASE WHEN Status='Pass'             THEN 1 ELSE 0 END),
                      SUM(CASE WHEN Status='FormatError' OR Status='FormatFail'
                                                                  THEN 1 ELSE 0 END),
                      SUM(CASE WHEN Status='ReadFail'           THEN 1 ELSE 0 END),
                      SUM(CASE WHEN Status='Timeout'            THEN 1 ELSE 0 END),
                      SUM(CASE WHEN Status='Duplicate'          THEN 1 ELSE 0 END),
                      SUM(CASE WHEN Status='NotFound'           THEN 1 ELSE 0 END),
                      SUM(CASE WHEN Status='Error'              THEN 1 ELSE 0 END),
                      COUNT(*)
                    FROM Records;";
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    c.PassTotal       = SafeInt(r, 0);
                    c.FormatFailCount = SafeInt(r, 1);
                    c.ReadFailCount   = SafeInt(r, 2);
                    c.TimeoutCount    = SafeInt(r, 3);
                    c.DuplicateCount  = SafeInt(r, 4);
                    c.NotFoundCount   = SafeInt(r, 5);
                    c.ErrorCount      = SafeInt(r, 6);
                    c.TotalCount      = SafeInt(r, 7);
                    // FailTotal: mọi row không phải Pass đều là fail (BAO GỒM cả Duplicate).
                    // Convention cũ ở Models.cs:204 cũ đã loại Duplicate, nhưng user yêu cầu
                    // Duplicate cũng tính vào FailTotal (PLC vẫn FAIL cho mã trùng lặp).
                    c.FailTotal = c.FormatFailCount + c.ReadFailCount + c.TimeoutCount
                                + c.NotFoundCount + c.DuplicateCount + c.ErrorCount;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Warning(ex, "[PORecord] GetCountersFromRecordDB fail for {OrderNo}", orderNo);
            }
            return c;
        }

        private static int SafeInt(SqliteDataReader r, int col)
            => r.IsDBNull(col) ? 0 : Convert.ToInt32(r.GetValue(col));
    }
}
