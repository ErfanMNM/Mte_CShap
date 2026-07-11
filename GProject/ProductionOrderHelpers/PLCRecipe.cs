using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// PLC recipe — lưu 3 thông số DelayCamera / DelayReject / RejectStreng
    /// cho PLC Omron. Hỗ trợ nhiều recipe, chỉ 1 recipe active tại 1 thời điểm.
    /// </summary>
    public class PLCRecipe
    {
        public int Id { get; set; }
        public string RecipeName { get; set; } = "Default";
        public int DelayCamera { get; set; } = 1000;       // ms
        public int DelayReject { get; set; } = 2000;       // ms
        public int RejectStreng { get; set; } = 20;        // 0-100
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = "Operator";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DAL cho bảng plc_recipes — file lưu tại C:/GProject/PODatabases/PLCRecipes.db
    /// </summary>
    public static class PLCRecipeDb
    {
        private const string DbFileName = "PLCRecipes.db";

        public static string GetDbPath()
        {
            var folder = Config.BasePath;
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);
            return System.IO.Path.Combine(folder, DbFileName);
        }

        public static string GetConnectionString() => $"Data Source={GetDbPath()}";

        /// <summary>Create schema if missing + seed recipe Default (active).</summary>
        public static void EnsureCreated()
        {
            var dbPath = GetDbPath();
            var folder = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(folder) && !System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            using var con = new SqliteConnection($"Data Source={dbPath}");
            con.Open();

            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS plc_recipes (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        recipe_name TEXT UNIQUE NOT NULL,
                        delay_camera INTEGER NOT NULL DEFAULT 1000,
                        delay_reject INTEGER NOT NULL DEFAULT 2000,
                        reject_streng INTEGER NOT NULL DEFAULT 20,
                        is_active INTEGER NOT NULL DEFAULT 0,
                        created_by TEXT NOT NULL DEFAULT 'Operator',
                        created_at TEXT NOT NULL,
                        updated_at TEXT NOT NULL
                    );
                    CREATE INDEX IF NOT EXISTS IDX_PR_active ON plc_recipes(is_active);
                    PRAGMA journal_mode=WAL;
                ";
                cmd.ExecuteNonQuery();
            }

            // Seed Default recipe nếu bảng rỗng.
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM plc_recipes";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    var now = DateTime.UtcNow.ToString("o");
                    using var ins = con.CreateCommand();
                    ins.CommandText = @"
                        INSERT INTO plc_recipes
                            (recipe_name, delay_camera, delay_reject, reject_streng, is_active, created_by, created_at, updated_at)
                        VALUES ('Default', 1000, 2000, 20, 1, 'System', $now, $now)";
                    ins.Parameters.AddWithValue("$now", now);
                    ins.ExecuteNonQuery();
                    Log.Information("[PLCRecipeDb] Seeded default recipe.");
                }
            }
        }

        public static List<PLCRecipe> GetAll()
        {
            var list = new List<PLCRecipe>();
            using var con = new SqliteConnection(GetConnectionString());
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT id, recipe_name, delay_camera, delay_reject, reject_streng,
                       is_active, created_by, created_at, updated_at
                FROM plc_recipes
                ORDER BY is_active DESC, id ASC";
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(MapRow(rd));
            return list;
        }

        public static PLCRecipe? GetById(int id)
        {
            using var con = new SqliteConnection(GetConnectionString());
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT id, recipe_name, delay_camera, delay_reject, reject_streng,
                       is_active, created_by, created_at, updated_at
                FROM plc_recipes WHERE id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? MapRow(rd) : null;
        }

        public static PLCRecipe? GetActive()
        {
            using var con = new SqliteConnection(GetConnectionString());
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT id, recipe_name, delay_camera, delay_reject, reject_streng,
                       is_active, created_by, created_at, updated_at
                FROM plc_recipes WHERE is_active=1 ORDER BY id ASC LIMIT 1";
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? MapRow(rd) : null;
        }

        /// <summary>Insert if not exists, else update by id (or by name if id=0).</summary>
        public static PLCRecipe Save(PLCRecipe r)
        {
            EnsureCreated();
            var now = DateTime.UtcNow.ToString("o");
            using var con = new SqliteConnection(GetConnectionString());
            con.Open();

            // Resolve id: if id==0, find by name first
            if (r.Id == 0 && !string.IsNullOrWhiteSpace(r.RecipeName))
            {
                using var find = con.CreateCommand();
                find.CommandText = "SELECT id FROM plc_recipes WHERE recipe_name=$n";
                find.Parameters.AddWithValue("$n", r.RecipeName);
                var existing = find.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                    r.Id = Convert.ToInt32(existing);
            }

            if (r.Id == 0)
            {
                using var ins = con.CreateCommand();
                ins.CommandText = @"
                    INSERT INTO plc_recipes
                        (recipe_name, delay_camera, delay_reject, reject_streng, is_active, created_by, created_at, updated_at)
                    VALUES ($n, $dc, $dr, $rs, $act, $cb, $now, $now)";
                ins.Parameters.AddWithValue("$n", r.RecipeName);
                ins.Parameters.AddWithValue("$dc", r.DelayCamera);
                ins.Parameters.AddWithValue("$dr", r.DelayReject);
                ins.Parameters.AddWithValue("$rs", r.RejectStreng);
                ins.Parameters.AddWithValue("$act", r.IsActive ? 1 : 0);
                ins.Parameters.AddWithValue("$cb", r.CreatedBy);
                ins.Parameters.AddWithValue("$now", now);
                ins.ExecuteNonQuery();
                r.Id = GetLastInsertRowId(con);
                r.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                using var upd = con.CreateCommand();
                upd.CommandText = @"
                    UPDATE plc_recipes SET
                        recipe_name = $n,
                        delay_camera = $dc,
                        delay_reject = $dr,
                        reject_streng = $rs,
                        updated_at = $now
                    WHERE id = $id";
                upd.Parameters.AddWithValue("$n", r.RecipeName);
                upd.Parameters.AddWithValue("$dc", r.DelayCamera);
                upd.Parameters.AddWithValue("$dr", r.DelayReject);
                upd.Parameters.AddWithValue("$rs", r.RejectStreng);
                upd.Parameters.AddWithValue("$now", now);
                upd.Parameters.AddWithValue("$id", r.Id);
                upd.ExecuteNonQuery();
            }

            r.UpdatedAt = DateTime.UtcNow;
            return r;
        }

        public static bool SetActive(int id)
        {
            EnsureCreated();
            using var con = new SqliteConnection(GetConnectionString());
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "UPDATE plc_recipes SET is_active=0";
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = con.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = "UPDATE plc_recipes SET is_active=1 WHERE id=$id";
                    cmd.Parameters.AddWithValue("$id", id);
                    var n = cmd.ExecuteNonQuery();
                    if (n == 0) { tx.Rollback(); return false; }
                }
                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Returns (deleted, message). Cannot delete "Default" or the currently active recipe.</summary>
        public static (bool ok, string message) Delete(int id)
        {
            EnsureCreated();
            using var con = new SqliteConnection(GetConnectionString());
            con.Open();

            using (var chk = con.CreateCommand())
            {
                chk.CommandText = "SELECT recipe_name, is_active FROM plc_recipes WHERE id=$id";
                chk.Parameters.AddWithValue("$id", id);
                using var rd = chk.ExecuteReader();
                if (!rd.Read()) return (false, $"Không tìm thấy recipe id={id}.");
                var name = rd.GetString(0);
                var active = rd.GetInt32(1);
                if (name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    return (false, "Không thể xóa recipe mặc định 'Default'.");
                if (active == 1)
                    return (false, "Không thể xóa recipe đang active. Hãy chuyển active sang recipe khác trước.");
            }

            using var del = con.CreateCommand();
            del.CommandText = "DELETE FROM plc_recipes WHERE id=$id";
            del.Parameters.AddWithValue("$id", id);
            del.ExecuteNonQuery();
            return (true, "Đã xóa.");
        }

        private static int GetLastInsertRowId(SqliteConnection con)
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static PLCRecipe MapRow(SqliteDataReader rd)
        {
            return new PLCRecipe
            {
                Id = rd.GetInt32(0),
                RecipeName = rd.GetString(1),
                DelayCamera = rd.GetInt32(2),
                DelayReject = rd.GetInt32(3),
                RejectStreng = rd.GetInt32(4),
                IsActive = rd.GetInt32(5) == 1,
                CreatedBy = rd.GetString(6),
                CreatedAt = DateTime.TryParse(rd.GetString(7), out var c) ? c : DateTime.UtcNow,
                UpdatedAt = DateTime.TryParse(rd.GetString(8), out var u) ? u : DateTime.UtcNow,
            };
        }
    }
}
