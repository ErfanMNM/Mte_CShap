using System.Data;
using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Quản lý lịch sử chạy PO
    /// </summary>
    public static class POHistoryManager
    {
        private static string GetConnectionString(string dbPath) => $"Data Source={dbPath}";

        /// <summary>
        /// Đảm bảo POHistory.db tồn tại
        /// </summary>
        public static void EnsureHistoryDB()
        {
            Config.EnsurePOHistory();
        }

        /// <summary>
        /// Ghi bắt đầu chạy PO
        /// </summary>
        public static Result RecordStart(string orderNo, string productionDate, string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                EnsureHistoryDB();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                SQLiteHelper.ExecuteNonQuery(GetConnectionString(Config.GetPOHistoryPath()), @"
                    INSERT INTO POHistory (PO, ProductionDate, StartTime, EndTime, Status, UserName)
                    VALUES (@PO, @ProductionDate, @StartTime, @EndTime, @Status, @UserName)",
                    new SqliteParameter("@PO", orderNo),
                    new SqliteParameter("@ProductionDate", productionDate ?? ""),
                    new SqliteParameter("@StartTime", now),
                    new SqliteParameter("@EndTime", ""),
                    new SqliteParameter("@Status", "Running"),
                    new SqliteParameter("@UserName", userName ?? ""));

                return Result.Success($"Đã ghi bắt đầu PO '{orderNo}'.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi ghi Start PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Ghi kết thúc PO
        /// </summary>
        public static Result RecordEnd(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                EnsureHistoryDB();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                SQLiteHelper.ExecuteNonQuery(GetConnectionString(Config.GetPOHistoryPath()), @"
                    UPDATE POHistory SET EndTime = @EndTime, Status = @Status WHERE PO = @PO AND EndTime = ''",
                    new SqliteParameter("@PO", orderNo),
                    new SqliteParameter("@EndTime", now),
                    new SqliteParameter("@Status", "Completed"));

                return Result.Success($"Đã ghi kết thúc PO '{orderNo}'.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi ghi End PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy PO đang chạy cuối cùng
        /// </summary>
        public static POHistoryData? GetLastRunningPO()
        {
            try
            {
                EnsureHistoryDB();
                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(Config.GetPOHistoryPath()),
                    "SELECT * FROM POHistory WHERE EndTime = '' AND Status = 'Running' ORDER BY ID DESC LIMIT 1");
                return table.Rows.Count > 0 ? POHistoryData.FromDataRow(table.Rows[0]) : null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Lấy PO chạy cuối cùng (bất kể trạng thái)
        /// </summary>
        public static POHistoryData? GetLastPO()
        {
            try
            {
                EnsureHistoryDB();
                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(Config.GetPOHistoryPath()),
                    "SELECT * FROM POHistory ORDER BY ID DESC LIMIT 1");
                return table.Rows.Count > 0 ? POHistoryData.FromDataRow(table.Rows[0]) : null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Lấy lịch sử PO theo orderNo
        /// </summary>
        public static Result GetByOrderNo(string orderNo)
        {
            try
            {
                EnsureHistoryDB();
                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(Config.GetPOHistoryPath()),
                    "SELECT * FROM POHistory WHERE PO = @PO ORDER BY ID DESC",
                    new SqliteParameter("@PO", orderNo));
                return Result.FromDataTable(table, $"Lấy {table.Rows.Count} bản ghi thành công.", "Không có bản ghi nào.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra PO có đang chạy không
        /// </summary>
        public static bool IsPORunning(string orderNo)
        {
            try
            {
                EnsureHistoryDB();
                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(Config.GetPOHistoryPath()),
                    "SELECT COUNT(*) FROM POHistory WHERE PO = @PO AND EndTime = '' AND Status = 'Running'",
                    new SqliteParameter("@PO", orderNo));
                return Convert.ToInt32(result) > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Ghi nhận khi ProductionDate được thay đổi
        /// </summary>
        public static Result RecordProductionDateChange(string orderNo, string oldDate, string newDate, string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                EnsureHistoryDB();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                SQLiteHelper.ExecuteNonQuery(GetConnectionString(Config.GetPOHistoryPath()), @"
                    INSERT INTO POHistory (PO, ProductionDate, StartTime, EndTime, Status, UserName)
                    VALUES (@PO, @ProductionDate, @StartTime, @EndTime, @Status, @UserName)",
                    new SqliteParameter("@PO", orderNo),
                    new SqliteParameter("@ProductionDate", $"ChangeDate: {oldDate} -> {newDate}"),
                    new SqliteParameter("@StartTime", now),
                    new SqliteParameter("@EndTime", ""),
                    new SqliteParameter("@Status", "DateChanged"),
                    new SqliteParameter("@UserName", userName ?? ""));

                return Result.Success($"Đã ghi thay đổi ProductionDate PO '{orderNo}': {oldDate} -> {newDate}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi ghi thay đổi ProductionDate: {ex.Message}");
            }
        }
    }
}
