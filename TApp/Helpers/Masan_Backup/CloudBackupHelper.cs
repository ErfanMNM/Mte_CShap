using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTManager.Masan;

namespace TApp.Helpers.Masan_Backup
{
    public static class CloudBackupHelper
    {
        public static void EnsureSchema(string dbPath)
        {
            using var con = new SQLiteConnection($"Data Source={dbPath};Version=3;Pooling=True");
            con.Open();

            using (var cmd = new SQLiteCommand(@"
            PRAGMA journal_mode=WAL;

                CREATE TABLE IF NOT EXISTS BackupLog (
                  ID INTEGER PRIMARY KEY AUTOINCREMENT,
                  FileName TEXT NOT NULL,
                  Status TEXT NOT NULL,
                  TimeStartUp TEXT NOT NULL,
                  TimeCompleted TEXT,
                  TimeUnixQR INTEGER NOT NULL,
                  Message TEXT
                );
            CREATE INDEX IF NOT EXISTS IDX_BackupLog_TimeUnixQR ON BackupLog(TimeUnixQR);
        ", con))
            {
                cmd.ExecuteNonQuery();
            }
        }


        public static long GetLastTimeBackup(string dbPath)
        {
            EnsureSchema(dbPath);
            try
            {
                using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    con.Open();
                    string sql = $"SELECT TimeUnixQR FROM `BackupLog` WHERE `Status` = '1' ORDER BY `ID` DESC LIMIT 1";


                    using (var cmd = new SQLiteCommand(sql, con))
                    {

                        var adapter = new SQLiteDataAdapter(cmd);
                        var table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count > 0)
                        {
                            long time = Convert.ToInt64(table.Rows[0]["TimeUnixQR"]);
                            return time;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu sao lưu: {ex.Message}");
            }
        }

        public static long InsertLog(string logDb, string file, string status, string timeStart, string timeCompleted, long unix, string msg)
        {
            EnsureSchema(logDb);
            using var con = new SQLiteConnection($"Data Source={logDb};Version=3;");
            con.Open();

            using var cmd = new SQLiteCommand(@"
            INSERT INTO BackupLog (FileName, Status, TimeStartUp, TimeCompleted, TimeUnixQR, Message)
            VALUES (@f, @s, @ts, @tc, @u, @m);
            SELECT last_insert_rowid();
        ", con);

            cmd.Parameters.AddWithValue("@f", file);
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@ts", timeStart);
            cmd.Parameters.AddWithValue("@tc", timeCompleted);
            cmd.Parameters.AddWithValue("@u", unix);
            cmd.Parameters.AddWithValue("@m", msg);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public static TResult GetData(string dbPath)
        {
            EnsureSchema(dbPath);
            try
            {
                using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    con.Open();
                    string sql = $"SELECT * FROM `BackupLog` ORDER BY `ID` DESC LIMIT 25";
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
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
    }
}
