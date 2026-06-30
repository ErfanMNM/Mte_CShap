using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Load cartons từ Carton database vào Dictionary để lookup nhanh O(1)
    /// </summary>
    public static class CartonDictionaryLoader
    {
        /// <summary>
        /// Load toàn bộ cartons từ PO vào Dictionary
        /// </summary>
        public static (bool success, string message, int count) LoadAllCartonsToDictionary(
            string orderNo,
            Dictionary<int, CartonInfo> dictionary)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return (false, "orderNo không được trống.", 0);

                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                    return (false, $"Carton DB của PO '{orderNo}' không tồn tại.", 0);

                dictionary.Clear();
                int count = 0;

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = "SELECT * FROM Carton ORDER BY ID;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                using var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    var info = new CartonInfo
                    {
                        Id = Convert.ToInt32(rd["ID"] ?? 0),
                        CartonCode = rd["cartonCode"]?.ToString() ?? "0",
                        StartDatetime = rd["Start_Datetime"]?.ToString() ?? "0",
                        CompletedDatetime = rd["Completed_Datetime"]?.ToString() ?? "0",
                        ActivateUser = rd["ActivateUser"]?.ToString() ?? "",
                        ProductionDate = rd["ProductionDate"]?.ToString() ?? "0"
                    };

                    dictionary[info.Id] = info;
                    count++;
                }

                return (true, $"Đã load {count} cartons vào Dictionary.", count);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi load cartons: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Tạo N cartons trống và load vào Dictionary
        /// </summary>
        public static (bool success, string message, int count) CreateAndLoadCartons(
            string orderNo, int count, Dictionary<int, CartonInfo> dictionary)
        {
            try
            {
                // Ensure carton DB exists
                string dbPath = Config.GetCartonPath(orderNo);
                if (!File.Exists(dbPath))
                {
                    var initResult = POCreator.InitPO(orderNo);
                    if (!initResult.IsSuccess)
                        return (false, initResult.Message, 0);
                }

                // Create empty cartons
                var createResult = POCreator.CreateEmptyCartons(orderNo, count);
                if (!createResult.success)
                    return (false, createResult.message, 0);

                // Load lại vào dictionary
                return LoadAllCartonsToDictionary(orderNo, dictionary);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo cartons: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Start một carton (bắt đầu đóng gói)
        /// </summary>
        public static Result StartCartonInDatabase(string orderNo, int cartonId, string activateUser)
        {
            return POCarton.StartCarton(orderNo, cartonId, activateUser);
        }

        /// <summary>
        /// Complete một carton (hoàn thành đóng gói)
        /// </summary>
        public static Result CompleteCartonInDatabase(string orderNo, int cartonId, string activateUser)
        {
            return POCarton.CompleteCarton(orderNo, cartonId, activateUser);
        }

        /// <summary>
        /// Reset carton về trạng thái trống
        /// </summary>
        public static Result ResetCartonInDatabase(string orderNo, int cartonId)
        {
            return POCarton.ResetCarton(orderNo, cartonId);
        }

        /// <summary>
        /// Tìm carton nhanh trong Dictionary
        /// </summary>
        public static bool FindCarton(Dictionary<int, CartonInfo> dictionary, int cartonId, out CartonInfo? info)
        {
            return dictionary.TryGetValue(cartonId, out info);
        }

        /// <summary>
        /// Tìm thùng tiếp theo chưa đóng (để tiếp tục đóng)
        /// </summary>
        public static int? FindNextOpenCarton(Dictionary<int, CartonInfo> dictionary)
        {
            foreach (var kvp in dictionary.OrderBy(x => x.Key))
            {
                if (kvp.Value.StartDatetime != "0" && kvp.Value.CompletedDatetime == "0")
                    return kvp.Key;
            }
            return null;
        }
    }
}
