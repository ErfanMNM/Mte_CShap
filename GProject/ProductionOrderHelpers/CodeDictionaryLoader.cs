using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Load codes từ PO database vào Dictionary để lookup nhanh O(1)
    /// </summary>
    public static class CodeDictionaryLoader
    {
        /// <summary>
        /// Load toàn bộ codes từ PO vào Dictionary
        /// </summary>
        public static (bool success, string message, int count) LoadAllCodesToDictionary(
            string orderNo,
            Dictionary<string, CodeInfo> dictionary)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return (false, "orderNo không được trống.", 0);

                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return (false, $"Database PO '{orderNo}' không tồn tại.", 0);

                dictionary.Clear();
                int count = 0;

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = "SELECT * FROM UniqueCodes;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                using var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    var info = new CodeInfo
                    {
                        Code = rd["Code"]?.ToString() ?? "",
                        OrderNo = orderNo,
                        Status = Convert.ToInt32(rd["Status"] ?? 0),
                        CartonCode = rd["cartonCode"]?.ToString() ?? "0",
                        ActivateDate = rd["ActivateDate"]?.ToString() ?? "0",
                        ProductionDate = rd["ProductionDate"]?.ToString() ?? "0",
                        ActivateUser = rd["ActivateUser"]?.ToString() ?? "",
                        PackingDate = rd["PackingDate"]?.ToString() ?? "0"
                    };

                    if (!string.IsNullOrEmpty(info.Code))
                    {
                        dictionary[info.Code] = info;
                        count++;
                    }
                }

                return (true, $"Đã load {count} codes vào Dictionary.", count);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi load codes: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Load chỉ codes chưa activate (Status=0) vào Dictionary
        /// </summary>
        public static (bool success, string message, int count) LoadUnusedCodesToDictionary(
            string orderNo,
            Dictionary<string, CodeInfo> dictionary)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return (false, "orderNo không được trống.", 0);

                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return (false, $"Database PO '{orderNo}' không tồn tại.", 0);

                dictionary.Clear();
                int count = 0;

                using var con = new SqliteConnection($"Data Source={dbPath}");
                con.Open();
                const string sql = "SELECT * FROM UniqueCodes WHERE Status = 0;";
                using var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                using var rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    var info = new CodeInfo
                    {
                        Code = rd["Code"]?.ToString() ?? "",
                        OrderNo = orderNo,
                        Status = 0,
                        CartonCode = "0",
                        ActivateDate = "0",
                        ProductionDate = rd["ProductionDate"]?.ToString() ?? "0",
                        ActivateUser = "",
                        PackingDate = "0"
                    };

                    if (!string.IsNullOrEmpty(info.Code))
                    {
                        dictionary[info.Code] = info;
                        count++;
                    }
                }

                return (true, $"Đã load {count} codes chưa sử dụng vào Dictionary.", count);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi load codes: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái code trong database sau khi activate
        /// </summary>
        public static Result ActivateCodeInDatabase(string orderNo, string code,
            string activateDate, string activateUser, string productionDate)
        {
            return POActivator.ActivateCode(orderNo, code, activateDate, activateUser, productionDate);
        }

        /// <summary>
        /// Cập nhật trạng thái code trong database sau khi pack
        /// </summary>
        public static Result PackCodeInDatabase(string orderNo, string code,
            string cartonCode, string packingDate, string packingUser, string productionDate)
        {
            return POPacking.PackCode(orderNo, code, cartonCode, packingDate, packingUser, productionDate);
        }

        /// <summary>
        /// Tìm mã nhanh trong Dictionary
        /// </summary>
        public static bool FindCode(Dictionary<string, CodeInfo> dictionary, string code, out CodeInfo? info)
        {
            return dictionary.TryGetValue(code, out info);
        }
    }
}
