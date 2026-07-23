using System.Data.SQLite;
using TApp.Models;

namespace TApp.Helpers
{
    public static class BatchHistoryHelper
    {
        private const string TABLE_SQL = @"
        CREATE TABLE IF NOT EXISTS POHistory (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            POItem TEXT NOT NULL,
            POLot TEXT NOT NULL,
            UserName TEXT NOT NULL,
            ProductionDate TEXT NOT NULL,
            TimeStamp TEXT NOT NULL
        );";

        /// <summary>
        /// Đảm bảo folder tồn tại, tạo file SQLite nếu chưa có, và tạo table nếu chưa có.
        /// Gọi hàm này trước khi xài các hàm khác.
        /// </summary>
        public static void EnsureDatabase(string dbPath)
        {
            // Tạo thư mục nếu chưa có
            string dir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // SQLite sẽ tự tạo file nếu chưa tồn tại khi con.Open()
            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(TABLE_SQL, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Thêm log mới.
        /// </summary>
        public static void AddHistory(
            string dbPath,
            string POItem,
            string POLot,
            string userName,
            DateTime productionDate)
        {
            EnsureDatabase(dbPath); // đảm bảo chắc cú

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
            INSERT INTO BatchHistory 
            (POItem, POLot, UserName, ProductionDate, TimeStamp)
            VALUES 
            (@POItem, @POLot, @UserName, @ProductionDate, @TimeStamp);";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@POItem", POItem);
                    cmd.Parameters.AddWithValue("@POLot", POLot);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@ProductionDate", productionDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@TimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Lấy bản ghi mới nhất (dựa trên TimeStamp, fallback theo ID).
        /// Trả về null nếu không có dữ liệu.
        /// </summary>
        public static BatchHistoryModel GetLatest(string dbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
            SELECT ID, POItem, POLot, UserName, ProductionDate, TimeStamp
            FROM BatchHistory
            ORDER BY datetime(TimeStamp) DESC, ID DESC
            LIMIT 1;";

                using (var cmd = new SQLiteCommand(sql, con))
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        return null;

                    return new BatchHistoryModel
                    {
                        ID = rd.GetInt32(0),
                        POItem = rd.GetString(1),
                        POLot = rd.GetString(2),
                        UserName = rd.GetString(3),
                        ProductionDate = rd.GetString(4),
                        TimeStamp = rd.GetString(5)
                    };
                }
            }
        }

        /// <summary>
        /// Lấy tổng số bản ghi (phục vụ phân trang).
        /// </summary>
        public static int GetTotalCount(string dbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM BatchHistory;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Lấy danh sách có phân trang.
        /// pageIndex: bắt đầu từ 1
        /// pageSize: số dòng / trang
        /// </summary>
        public static List<BatchHistoryModel> GetPage(
            string dbPath,
            int pageIndex,
            int pageSize,
            out int totalRows,
            out int totalPages)
        {
            EnsureDatabase(dbPath);

            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            totalRows = GetTotalCount(dbPath);
            totalPages = (int)Math.Ceiling(totalRows / (double)pageSize);

            int offset = (pageIndex - 1) * pageSize;

            var list = new List<BatchHistoryModel>();

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
            SELECT ID, POItem, POLot, UserName, ProductionDate, TimeStamp
            FROM BatchHistory
            ORDER BY datetime(TimeStamp) DESC, ID DESC
            LIMIT @Limit OFFSET @Offset;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Limit", pageSize);
                    cmd.Parameters.AddWithValue("@Offset", offset);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new BatchHistoryModel
                            {
                                ID = rd.GetInt32(0),
                                POItem = rd.GetString(1),
                                POLot = rd.GetString(2),
                                UserName = rd.GetString(3),
                                ProductionDate = rd.GetString(4),
                                TimeStamp = rd.GetString(5)
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
