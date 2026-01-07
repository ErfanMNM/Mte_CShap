using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using TApp.Models;
using TTManager.Masan;
using ZXing;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace TApp.Helpers
{
    public static class QRDatabaseHelper
    {
        public const string DefaultDbPath = @"C:\MASANQR\QRDatabase.db";

        // DB phụ: chỉ lưu mã active & unique để check trùng nhanh
        public const string ActiveUniqueDbPath = @"C:\MASANQR\ActiveUnique.db";

        private const string CREATE_TABLE_SQL_UNIQUE = @"
            CREATE TABLE IF NOT EXISTS ActiveUniqueQR (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                QRContent TEXT NOT NULL,
                Status TEXT NOT NULL DEFAULT 1,
                BatchCode TEXT NOT NULL,
                Barcode TEXT NOT NULL,
                UserName TEXT NOT NULL,
                TimeStampActive TEXT NOT NULL,
                TimeUnixActive INTEGER NOT NULL,
                ProductionDatetime TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IDX_AU_QR_QRContent ON ActiveUniqueQR(QRContent);

            PRAGMA journal_mode=WAL;
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

            PRAGMA journal_mode=WAL;
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
        public static void AddOrActivateCode(
            string qrContent,
            string batchCode,
            string barcode,
            string userName,
            string TimeStampActive,
            long TimeUnixActive,
            string productionDateTime,
            e_Production_Status status,                     // 👈 thêm status vào đây
            string reason = "",                // 👈 optional reason luôn
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
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
                            cmd.Parameters.AddWithValue("@Status", status.ToString());
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            cmd.Parameters.AddWithValue("@TimeStampActive", TimeStampActive);
                            cmd.Parameters.AddWithValue("@TimeUnixActive", TimeUnixActive);
                            cmd.Parameters.AddWithValue("@ProductionDatetime", productionDateTime);
                            cmd.Parameters.AddWithValue("@Reason", reason ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    //}
                //}
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

        public static TResult GetByQRContent(string qrContent, string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            try
            {
                using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    con.Open();

                    string sql = @"
                SELECT ID, QRContent, BatchCode, Barcode, Status, UserName,
                       TimeStampActive, TimeUnixActive, ProductionDatetime, Reason
                FROM QRProducts
                WHERE QRContent LIKE @QRContent
                LIMIT 5;
            ";

                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@QRContent", $"%{qrContent}%");

                        var adapter = new SQLiteDataAdapter(cmd);
                        var table = new DataTable();
                        
                        adapter.Fill(table);

                        return (table.Rows.Count > 0)
                            ? new TResult(true, "Lấy thông tin mã thành công.", table)
                            : new TResult(true, "Không tìm thấy");
                    }
                }
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi khi truy vấn database: {ex.Message}");
            }
                
        }

        public static TResult Get_ActiveQR_By_TimeUnix(long timeunix, string dbPath = ActiveUniqueDbPath)
        {
            EnsureDatabase(dbPath);

            try
            {
                using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    con.Open();

                    string sql = @"
                SELECT *
                FROM ActiveUniqueQR
                WHERE TimeUnixActive > @u
                ORDER BY TimeUnixActive ASC;
            ";

                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@u", timeunix);

                        var adapter = new SQLiteDataAdapter(cmd);
                        var table = new DataTable();
                        adapter.Fill(table);

                        return (table.Rows.Count > 0)
                            ? new TResult(true, "Lấy thông tin mã thành công.", table)
                            : new TResult(true, "Không tìm thấy");
                    }
                }
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi khi truy vấn database: {ex.Message}");
            }

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


        public static long GetHourlyProduction(
            long timeunix,
            string batchCode = null,
            string statusFilter = "Pass",
            string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);
            long count = 0;

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"SELECT COUNT(*) 
                   FROM QRProducts 
                   WHERE TimeUnixActive <= @timeunix;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@timeunix", timeunix);

                    count = Convert.ToInt64(cmd.ExecuteScalar());
                }
            }

            long speed = count * 4; // quy đổi về sản lượng/giờ (15 phút x4)

            return speed;

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
            string TimeStampActive,
            long TimeUnixActive,
            string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                    INSERT INTO ActiveUniqueQR
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
                    cmd.Parameters.AddWithValue("@TimeStampActive", TimeStampActive);
                    cmd.Parameters.AddWithValue("@TimeUnixActive", TimeUnixActive);
                    cmd.Parameters.AddWithValue("@ProductionDatetime", TimeStampActive);

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


        public static int GetRecordCodeCount(string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                string sql = "SELECT COUNT(*) FROM QRProducts;";
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

        public static int GetRecordCountByBatch(string batchCode, string dbPath = DefaultDbPath)
        {
            EnsureDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = "SELECT COUNT(*) FROM QRProducts WHERE BatchCode = @BatchCode;";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@BatchCode", batchCode);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }


        public static TResult GetActiveByQRContent(string qrContent, string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT *
                FROM ActiveUniqueQR
                WHERE QRContent LIKE @QRContent
                LIMIT 1;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@QRContent", $"%{qrContent}%");

                    var adapter = new SQLiteDataAdapter(cmd);
                    var table = new DataTable();
                    adapter.Fill(table);

                    return (table.Rows.Count > 0)
                        ? new TResult(true, "Lấy thông tin mã thành công.", table)
                        : new TResult(false, "Không tìm thấy");
                }
            }
        }

        /// <summary>
        /// Lấy tất cả các mã active từ ActiveUnique database.
        /// </summary>
        public static TResult GetAllActiveCodes(int limit = 100, int offset = 0, string dbPath = ActiveUniqueDbPath)
        {
            EnsureActiveUniqueDatabase(dbPath);

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();

                string sql = @"
                SELECT ID, QRContent, Status, BatchCode, Barcode, UserName,
                       TimeStampActive, TimeUnixActive, ProductionDatetime
                FROM ActiveUniqueQR
                ORDER BY TimeUnixActive DESC
                LIMIT @Limit OFFSET @Offset;
            ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Limit", limit);
                    cmd.Parameters.AddWithValue("@Offset", offset);

                    var adapter = new SQLiteDataAdapter(cmd);
                    var table = new DataTable();
                    adapter.Fill(table);

                    return (table.Rows.Count > 0)
                        ? new TResult(true, "Lấy danh sách mã active thành công.", table)
                        : new TResult(false, "Không có mã active nào.");
                }
            }
        }
    }

 
}
