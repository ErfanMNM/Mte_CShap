using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using TApp.Models;

namespace TApp.Helpers
{
    public static class QRDatabaseHelper
    {
        public const string DefaultDbPath = @"C:\MASAN\QRDatabase.db";

        // DB phụ: chỉ lưu mã active & unique để check trùng nhanh
        public const string ActiveUniqueDbPath = @"C:\MASAN\ActiveUnique.db";

        private const string CREATE_TABLE_SQL_UNIQUE = @"
            CREATE TABLE IF NOT EXISTS ActiveUniqueQR (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                QRContent TEXT NOT NULL,
                BatchCode TEXT NOT NULL,
                Barcode TEXT NOT NULL,
                UserName TEXT NOT NULL,
                TimeStampActive TEXT NOT NULL,
                TimeUnixActive INTEGER NOT NULL,
                ProductionDatetime TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IDX_AU_QR_QRContent ON ActiveUniqueQR(QRContent);
        ";

        // schema mới: Status TEXT, Reason TEXT
        private const string CREATE_TABLE_SQL = @"
            CREATE TABLE IF NOT EXISTS QRProducts (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                QRContent TEXT NOT NULL,
                BatchCode TEXT NOT NULL,
                Barcode TEXT NOT NULL,
                Status TEXT NOT NULL,
                UserName TEXT NOT NULL,
                TimeStampActive TEXT NOT NULL,
                TimeUnixActive INTEGER NOT NULL,
                ProductionDatetime TEXT NOT NULL,
                Reason TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IDX_QR_QRContent ON QRProducts(QRContent);
            CREATE INDEX IF NOT EXISTS IDX_QR_BatchCode ON QRProducts(BatchCode);
        ";

        // ================== DB CHÍNH: QRProducts (record đầy đủ) ==================

        public static (bool MainDbExisted, bool ActiveDbExisted) InitDatabases()
        {
            bool mainExisted = File.Exists(DefaultDbPath);
            bool activeExisted = File.Exists(ActiveUniqueDbPath);

            // Tạo nếu chưa có + đảm bảo schema
            EnsureDatabase(DefaultDbPath);
            EnsureActiveUniqueDatabase(ActiveUniqueDbPath);

            return (mainExisted, activeExisted);
        }

        public static void EnsureDatabase(string dbPath = DefaultDbPath)
        {
            string folder = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(CREATE_TABLE_SQL, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Nếu DB cũ chưa có cột Reason/Status TEXT thì tốt nhất là backup & recreate.
                // Ở đây tao không migrate tự động.
            }
        }

        /// <summary>
        /// Batch này đã có dòng nào trong bể chưa?
        /// </summary>
        public static bool BatchHasData(string batchCode, string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                string sql = "SELECT COUNT(1) FROM QRProducts WHERE BatchCode = @BatchCode;";
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Thêm hoặc Active code -> Status = Pass (mặc định là OK).
        /// </summary>
        public static void AddOrActivateCode(
    string qrContent,
    string batchCode,
    string barcode,
    string userName,
    DateTime productionDateTime,
    string status,                     // 👈 thêm status vào đây
    string reason = "",                // 👈 optional reason luôn
    string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            long unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string timeStampNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string prodTime = productionDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string checkSql = "SELECT COUNT(1) FROM QRProducts WHERE QRContent = @QRContent;";
                using (var checkCmd = new SQLiteCommand(checkSql, con))
                {
                    checkCmd.Parameters.AddWithValue("@QRContent", qrContent);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exists > 0)
                    {
                        string updateSql = @"
                    UPDATE QRProducts
                    SET BatchCode = @BatchCode,
                        Barcode = @Barcode,
                        Status = @Status,
                        UserName = @UserName,
                        TimeStampActive = @TimeStampActive,
                        TimeUnixActive = @TimeUnixActive,
                        ProductionDatetime = @ProductionDatetime,
                        Reason = @Reason
                    WHERE QRContent = @QRContent;
                ";

                        using (var cmd = new SQLiteCommand(updateSql, con))
                        {
                            cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                            cmd.Parameters.AddWithValue("@Barcode", barcode);
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            cmd.Parameters.AddWithValue("@TimeStampActive", timeStampNow);
                            cmd.Parameters.AddWithValue("@TimeUnixActive", unixNow);
                            cmd.Parameters.AddWithValue("@ProductionDatetime", prodTime);
                            cmd.Parameters.AddWithValue("@Reason", reason ?? "");
                            cmd.Parameters.AddWithValue("@QRContent", qrContent);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string insertSql = @"
                    INSERT INTO QRProducts
                    (QRContent, BatchCode, Barcode, Status, UserName,
                     TimeStampActive, TimeUnixActive, ProductionDatetime, Reason)
                    VALUES
                    (@QRContent, @BatchCode, @Barcode, @Status, @UserName,
                     @TimeStampActive, @TimeUnixActive, @ProductionDatetime, @Reason);
                ";

                        using (var cmd = new SQLiteCommand(insertSql, con))
                        {
                            cmd.Parameters.AddWithValue("@QRContent", qrContent);
                            cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                            cmd.Parameters.AddWithValue("@Barcode", barcode);
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            cmd.Parameters.AddWithValue("@TimeStampActive", timeStampNow);
                            cmd.Parameters.AddWithValue("@TimeUnixActive", unixNow);
                            cmd.Parameters.AddWithValue("@ProductionDatetime", prodTime);
                            cmd.Parameters.AddWithValue("@Reason", reason ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Update status bất kỳ + ghi Reason (ví dụ ReadFail, Duplicate, Error, Timeout, Deactive).
        /// </summary>
        public static bool UpdateStatus(
            string qrContent,
            string newStatus,
            string reason,
            string userName,
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                UPDATE QRProducts
                SET Status = @Status,
                    Reason = @Reason,
                    UserName = @UserName
                WHERE QRContent = @QRContent;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@Reason", reason ?? "");
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@QRContent", qrContent);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        /// <summary>
        /// Hủy (deactive) 1 mã với lý do.
        /// </summary>
        public static bool DeactivateCode(string qrContent, string reason, string userName, string dbPath = DefaultDbPath)
        {
            return UpdateStatus(qrContent, "Deactive", reason, userName, dbPath);
        }

        public static QRProductRecord GetByQRContent(string qrContent, string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT ID, QRContent, BatchCode, Barcode, Status, UserName,
                       TimeStampActive, TimeUnixActive, ProductionDatetime, Reason
                FROM QRProducts
                WHERE QRContent = @QRContent
                LIMIT 1;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@QRContent", qrContent);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                            return null;

                        return new QRProductRecord
                        {
                            ID = rd.GetInt32(0),
                            QRContent = rd.GetString(1),
                            BatchCode = rd.GetString(2),
                            Barcode = rd.GetString(3),
                            Status = rd.GetString(4),
                            UserName = rd.GetString(5),
                            TimeStampActive = rd.GetString(6),
                            TimeUnixActive = rd.GetInt64(7),
                            ProductionDatetime = rd.GetString(8),
                            Reason = rd.GetString(9)
                        };
                    }
                }
            }
        }

        public static Dictionary<string, QRProductRecord> LoadToDictionary(
            bool onlyActivePass = false,
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            var dict = new Dictionary<string, QRProductRecord>(StringComparer.OrdinalIgnoreCase);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT ID, QRContent, BatchCode, Barcode, Status, UserName,
                       TimeStampActive, TimeUnixActive, ProductionDatetime, Reason
                FROM QRProducts
            ";

                if (onlyActivePass)
                    sql += " WHERE Status = 'Pass';";

                using (var cmd = new SQLiteCommand(sql, con))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var rec = new QRProductRecord
                        {
                            ID = rd.GetInt32(0),
                            QRContent = rd.GetString(1),
                            BatchCode = rd.GetString(2),
                            Barcode = rd.GetString(3),
                            Status = rd.GetString(4),
                            UserName = rd.GetString(5),
                            TimeStampActive = rd.GetString(6),
                            TimeUnixActive = rd.GetInt64(7),
                            ProductionDatetime = rd.GetString(8),
                            Reason = rd.GetString(9)
                        };

                        dict[rec.QRContent] = rec;
                    }
                }
            }

            return dict;
        }

        public static int GetRowCount(string batchCode = null, string status = null, string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = "SELECT COUNT(*) FROM QRProducts WHERE 1=1";
                if (!string.IsNullOrEmpty(batchCode))
                    sql += " AND BatchCode = @BatchCode";
                if (!string.IsNullOrEmpty(status))
                    sql += " AND Status = @Status";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(batchCode))
                        cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", status);

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public static List<BatchProductionSummary> GetProductionByBatch(
            string statusFilter = "Pass",
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            var list = new List<BatchProductionSummary>();

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT BatchCode, COUNT(*) AS Cnt
                FROM QRProducts
                WHERE (@Status IS NULL OR Status = @Status)
                GROUP BY BatchCode
                ORDER BY BatchCode;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    if (string.IsNullOrEmpty(statusFilter))
                        cmd.Parameters.AddWithValue("@Status", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Status", statusFilter);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new BatchProductionSummary
                            {
                                BatchCode = rd.GetString(0),
                                Total = rd.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return list;
        }

        public static List<HourlyProduction> GetHourlyProduction(
            DateTime day,
            string batchCode = null,
            string statusFilter = "Pass",
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            var list = new List<HourlyProduction>();
            string dayStr = day.ToString("yyyy-MM-dd");

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT CAST(strftime('%H', ProductionDatetime) AS INTEGER) AS Hour,
                       COUNT(*) AS Cnt
                FROM QRProducts
                WHERE date(ProductionDatetime) = date(@Day)
                  AND (@BatchCode IS NULL OR BatchCode = @BatchCode)
                  AND (@Status IS NULL OR Status = @Status)
                GROUP BY Hour
                ORDER BY Hour;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Day", dayStr);

                    if (string.IsNullOrEmpty(batchCode))
                        cmd.Parameters.AddWithValue("@BatchCode", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@BatchCode", batchCode);

                    if (string.IsNullOrEmpty(statusFilter))
                        cmd.Parameters.AddWithValue("@Status", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Status", statusFilter);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new HourlyProduction
                            {
                                Hour = rd.GetInt32(0),
                                Count = rd.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return list;
        }

        public static List<DailyProduction> GetDailyProduction(
            DateTime fromDate,
            DateTime toDate,
            string batchCode = null,
            string statusFilter = "Pass",
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            var list = new List<DailyProduction>();
            string fromStr = fromDate.ToString("yyyy-MM-dd");
            string toStr = toDate.ToString("yyyy-MM-dd");

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT date(ProductionDatetime) AS D,
                       COUNT(*) AS Cnt
                FROM QRProducts
                WHERE date(ProductionDatetime) >= date(@FromDate)
                  AND date(ProductionDatetime) <= date(@ToDate)
                  AND (@BatchCode IS NULL OR BatchCode = @BatchCode)
                  AND (@Status IS NULL OR Status = @Status)
                GROUP BY D
                ORDER BY D;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromStr);
                    cmd.Parameters.AddWithValue("@ToDate", toStr);

                    if (string.IsNullOrEmpty(batchCode))
                        cmd.Parameters.AddWithValue("@BatchCode", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@BatchCode", batchCode);

                    if (string.IsNullOrEmpty(statusFilter))
                        cmd.Parameters.AddWithValue("@Status", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Status", statusFilter);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new DailyProduction
                            {
                                Date = rd.GetString(0),
                                Count = rd.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return list;
        }

        // ================== DB PHỤ: ActiveUniqueQR (mã active & unique) ==================

        public static void EnsureActiveUniqueDatabase(string dbPath = ActiveUniqueDbPath)
        {
            string folder = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(CREATE_TABLE_SQL_UNIQUE, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Thêm mã active vào DB phụ (unique theo QRContent).
        /// Trả về true nếu insert thành công, false nếu đã tồn tại (bị IGNORE).
        /// </summary>
        public static bool AddActiveCodeUnique(
            string qrContent,
            string batchCode,
            string barcode,
            string userName,
            DateTime productionDateTime,
            string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            long unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string timeStampNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string prodTime = productionDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                    INSERT OR IGNORE INTO ActiveUniqueQR
                    (QRContent, BatchCode, Barcode, UserName,
                     TimeStampActive, TimeUnixActive, ProductionDatetime)
                    VALUES
                    (@QRContent, @BatchCode, @Barcode, @UserName,
                     @TimeStampActive, @TimeUnixActive, @ProductionDatetime);
                ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@QRContent", qrContent);
                    cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                    cmd.Parameters.AddWithValue("@Barcode", barcode);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@TimeStampActive", timeStampNow);
                    cmd.Parameters.AddWithValue("@TimeUnixActive", unixNow);
                    cmd.Parameters.AddWithValue("@ProductionDatetime", prodTime);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0; // true = mới thêm, false = đã có
                }
            }
        }

        /// <summary>
        /// Xóa 1 mã ra khỏi DB active (nếu cần revoke).
        /// </summary>
        public static bool RemoveActiveCode(string qrContent, string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = "DELETE FROM ActiveUniqueQR WHERE QRContent = @QRContent;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@QRContent", qrContent);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        /// <summary>
        /// Kiểm tra mã đã tồn tại trong DB active chưa.
        /// </summary>
        public static bool ActiveCodeExists(string qrContent, string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = "SELECT COUNT(*) FROM ActiveUniqueQR WHERE QRContent = @QRContent;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@QRContent", qrContent);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Load toàn bộ mã active vào HashSet để check trùng siêu nhanh.
        /// </summary>
        public static HashSet<string> LoadActiveToHashSet(string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = "SELECT QRContent FROM ActiveUniqueQR;";

                using (var cmd = new SQLiteCommand(sql, con))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        set.Add(rd.GetString(0));
                    }
                }
            }

            return set;
        }

        /// <summary>
        /// Lấy tổng số mã active đang nằm trong DB phụ.
        /// </summary>
        public static int GetActiveCodeCount(string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM ActiveUniqueQR;";
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }


        public static int GetActiveCountByBatch(string batchCode, string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = "SELECT COUNT(*) FROM ActiveUniqueQR WHERE BatchCode = @BatchCode;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

    }
}
