using System.Data;
using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Quản lý thùng carton
    /// </summary>
    public static class POCarton
    {
        private static string GetConnectionString(string dbPath) => $"Data Source={dbPath}";

        /// <summary>
        /// Tạo thùng mới
        /// </summary>
        public static Result Create(string orderNo, CartonData data)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                SQLiteHelper.ExecuteNonQuery(GetConnectionString(dbPath), @"
                    INSERT INTO Carton (cartonCode, Start_Datetime, Completed_Datetime, ActivateUser, ProductionDate)
                    VALUES (@cartonCode, @Start_Datetime, @Completed_Datetime, @ActivateUser, @ProductionDate)",
                    new SqliteParameter("@cartonCode", data.CartonCode ?? "0"),
                    new SqliteParameter("@Start_Datetime", data.StartDatetime ?? "0"),
                    new SqliteParameter("@Completed_Datetime", data.CompletedDatetime ?? "0"),
                    new SqliteParameter("@ActivateUser", data.ActivateUser ?? ""),
                    new SqliteParameter("@ProductionDate", data.ProductionDate ?? "0"));

                return Result.Success($"Tạo thùng '{data.CartonCode}' thành công.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi tạo carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thùng
        /// </summary>
        public static Result Update(string orderNo, CartonData data)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(dbPath), @"
                    UPDATE Carton SET cartonCode = @cartonCode, Start_Datetime = @Start_Datetime,
                    Completed_Datetime = @Completed_Datetime, ActivateUser = @ActivateUser, ProductionDate = @ProductionDate
                    WHERE ID = @ID",
                    new SqliteParameter("@ID", data.Id),
                    new SqliteParameter("@cartonCode", data.CartonCode ?? "0"),
                    new SqliteParameter("@Start_Datetime", data.StartDatetime ?? "0"),
                    new SqliteParameter("@Completed_Datetime", data.CompletedDatetime ?? "0"),
                    new SqliteParameter("@ActivateUser", data.ActivateUser ?? ""),
                    new SqliteParameter("@ProductionDate", data.ProductionDate ?? "0"));

                return rows > 0
                    ? Result.Success($"Cập nhật carton ID '{data.Id}' thành công.")
                    : Result.Fail($"Không tìm thấy carton ID: {data.Id}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi cập nhật carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa thùng
        /// </summary>
        public static Result Delete(string orderNo, int cartonId)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(dbPath),
                    "DELETE FROM Carton WHERE ID = @ID",
                    new SqliteParameter("@ID", cartonId));

                return rows > 0
                    ? Result.Success($"Xóa carton ID '{cartonId}' thành công.")
                    : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi xóa carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả thùng
        /// </summary>
        public static Result GetAll(string orderNo)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(dbPath), "SELECT * FROM Carton ORDER BY ID");
                return Result.FromDataTable(table, $"Lấy {table.Rows.Count} thùng thành công.", "Không có thùng nào.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi lấy carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thùng theo mã
        /// </summary>
        public static Result GetByCartonCode(string orderNo, string cartonCode)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail("Carton DB không tồn tại.");

                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(dbPath),
                    "SELECT * FROM Carton WHERE cartonCode = @cartonCode LIMIT 1",
                    new SqliteParameter("@cartonCode", cartonCode));
                return Result.FromDataTable(table, "Lấy thùng thành công.", $"Không tìm thấy thùng: {cartonCode}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Bắt đầu đóng thùng (set Start_Datetime)
        /// </summary>
        public static Result StartCarton(string orderNo, int cartonId, string activateUser)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(dbPath), @"
                    UPDATE Carton SET Start_Datetime = @Start_Datetime, ActivateUser = @ActivateUser WHERE ID = @ID",
                    new SqliteParameter("@ID", cartonId),
                    new SqliteParameter("@Start_Datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SqliteParameter("@ActivateUser", activateUser ?? ""));

                return rows > 0
                    ? Result.Success($"Start carton ID '{cartonId}' thành công.")
                    : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi start carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Hoàn thành thùng (set Completed_Datetime)
        /// </summary>
        public static Result CompleteCarton(string orderNo, int cartonId, string activateUser)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(dbPath), @"
                    UPDATE Carton SET Completed_Datetime = @Completed_Datetime, ActivateUser = @ActivateUser WHERE ID = @ID",
                    new SqliteParameter("@ID", cartonId),
                    new SqliteParameter("@Completed_Datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SqliteParameter("@ActivateUser", activateUser ?? ""));

                return rows > 0
                    ? Result.Success($"Complete carton ID '{cartonId}' thành công.")
                    : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi complete carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset thùng về trạng thái trống
        /// </summary>
        public static Result ResetCarton(string orderNo, int cartonId)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(dbPath),
                    "UPDATE Carton SET cartonCode = '0', Start_Datetime = '0', Completed_Datetime = '0' WHERE ID = @ID",
                    new SqliteParameter("@ID", cartonId));

                return rows > 0
                    ? Result.Success($"Reset carton ID '{cartonId}' thành công.")
                    : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi reset carton: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tổng số thùng
        /// </summary>
        public static int GetTotalCartonCount(string orderNo)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath)) return 0;
                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath), "SELECT COUNT(*) FROM Carton");
                return Convert.ToInt32(result);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Lấy số thùng đã đóng
        /// </summary>
        public static int GetClosedCartonCount(string orderNo)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath)) return 0;
                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath),
                    "SELECT COUNT(*) FROM Carton WHERE Completed_Datetime <> '0'");
                return Convert.ToInt32(result);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Lấy ID thùng lớn nhất
        /// </summary>
        public static int GetMaxCartonId(string orderNo)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath)) return 0;
                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath), "SELECT MAX(ID) FROM Carton");
                return result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            catch { return 0; }
        }
    }
}
