using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Read-only helpers cho CartonCode. Không viết trực tiếp — luôn đi qua CartonWriteQueue.
    /// </summary>
    public static class POCartonCode
    {
        private static string GetConnStr(string dbPath) => $"Data Source={dbPath}";

        /// <summary>
        /// Lấy thông tin chi tiết của một thùng theo cartonCode.
        /// </summary>
        public static CartonDetailInfo GetCartonInfo(string orderNo, string cartonCode)
        {
            var result = new CartonDetailInfo { CartonCode = cartonCode };
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return result.WithError("Carton DB not found");

                var carton = POCarton.GetByCartonCode(orderNo, cartonCode);
                if (!carton.IsSuccess || carton.Data == null || carton.Data.Rows.Count == 0)
                    return result.WithError("Khong tim thay thung");

                var row = carton.Data.Rows[0];
                result.CartonIndex = Convert.ToInt32(row["ID"]);
                result.StartDatetime = row["Start_Datetime"]?.ToString() ?? "0";
                result.ActivateUser = row["ActivateUser"]?.ToString() ?? "";

                int count = GProduction.POLoader.CountCodes(orderNo, status: 1, cartonCode: cartonCode);
                result.ProductCount = count;
                result.Status = result.StartDatetime != "0" ? "OK" : "WARN";
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[POCartonCode] GetCartonInfo failed for {CartonCode}", cartonCode);
                return result.WithError(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin thùng cuối cùng được scan bởi một machine.
        /// </summary>
        public static CartonScanInfo? GetLastCarton(string orderNo, string machineName)
        {
            try
            {
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return null;

                using var con = new SqliteConnection(GetConnStr(dbPath));
                con.Open();

                using var cmd = new SqliteCommand(@"
                    SELECT c.CartonCode, c.Start_Datetime, c.ActivateUser,
                           cc.CartonIndex, cc.ScanAt, cc.Mode, cc.Result
                    FROM CartonCode cc
                    JOIN Carton c ON c.cartonCode = cc.CartonCode
                    WHERE cc.MachineName = @mn
                    ORDER BY cc.ID DESC LIMIT 1", con);
                cmd.Parameters.AddWithValue("@mn", machineName);

                using var r = cmd.ExecuteReader();
                if (!r.Read())
                    return null;

                return new CartonScanInfo
                {
                    CartonCode = r.GetString(0),
                    StartDatetime = r.GetString(1),
                    ActivateUser = r.GetString(2),
                    CartonIndex = r.GetInt32(3),
                    ScanAt = r.GetString(4),
                    Mode = r.GetString(5),
                    Result = r.GetString(6)
                };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[POCartonCode] GetLastCarton failed for {Machine}", machineName);
                return null;
            }
        }
    }
}
