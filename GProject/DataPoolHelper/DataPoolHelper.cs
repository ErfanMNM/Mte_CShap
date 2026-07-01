using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Text;
using Serilog;

namespace GProject.DataPoolHelper;

/// <summary>
/// Static helper class cho DataPool.
/// </summary>
public static class DataPoolStatic
{
    public static string DefaultDataPath => @"C:\GProject\DataPool";

    private static string _dataPath = DefaultDataPath;
    public static string DataPath
    {
        get => _dataPath;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
                _dataPath = value;
        }
    }

    public static string GetPoolPath(string poolName)
    {
        if (string.IsNullOrWhiteSpace(poolName))
            throw new ArgumentException("PoolName không được trống.", nameof(poolName));
        return Path.Combine(_dataPath, poolName + ".db");
    }

    public static string GetPhieuPath(string createID)
    {
        if (string.IsNullOrWhiteSpace(createID))
            throw new ArgumentException("CreateID không được trống.", nameof(createID));
        return Path.Combine(_dataPath, "Phieu", createID + ".db");
    }

    public static void EnsurePool(string poolName)
    {
        string dbPath = GetPoolPath(poolName);
        string folder = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder ?? throw new InvalidOperationException("Folder path is null"));

        const string sql = @"
            CREATE TABLE IF NOT EXISTS Codes (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Status INTEGER NOT NULL DEFAULT 0,
                BatchID TEXT NOT NULL DEFAULT '',
                CreateTime TEXT NOT NULL,
                CreateID TEXT NOT NULL,
                Note TEXT NOT NULL DEFAULT ''
            );
            CREATE INDEX IF NOT EXISTS IDX_Codes_Status ON Codes(Status);
            CREATE INDEX IF NOT EXISTS IDX_Codes_BatchID ON Codes(BatchID);
            CREATE INDEX IF NOT EXISTS IDX_Codes_CreateID ON Codes(CreateID);
            PRAGMA journal_mode=WAL;
        ";

        using var con = new SqliteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = new SqliteCommand(sql, con);
        cmd.ExecuteNonQuery();
    }

    public static void EnsurePhieu(string createID)
    {
        string dbPath = GetPhieuPath(createID);
        string folder = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder ?? throw new InvalidOperationException("Folder path is null"));

        const string sql = @"
            CREATE TABLE IF NOT EXISTS PhieuTao (
                CreateID TEXT PRIMARY KEY,
                UserName TEXT NOT NULL,
                CreateTime TEXT NOT NULL,
                Note TEXT NOT NULL DEFAULT '',
                ImportMethod TEXT NOT NULL,
                ImportSource TEXT NOT NULL DEFAULT '',
                ImportCount INTEGER NOT NULL DEFAULT 0
            );
            PRAGMA journal_mode=WAL;
        ";

        using var con = new SqliteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = new SqliteCommand(sql, con);
        cmd.ExecuteNonQuery();
    }

    public static Result Import_Manual(
        string poolName,
        string code,
        int? status = null,
        string batchID = "",
        string createID = "",
        string note = "",
        string userName = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return Result.Fail("PoolName không được trống.");
            if (string.IsNullOrWhiteSpace(code))
                return Result.Fail("Mã code không được trống.");

            EnsurePool(poolName);

            string finalCreateID = string.IsNullOrWhiteSpace(createID)
                ? $"User:{userName}"
                : createID;
            int finalStatus = status ?? (int)CodeStatus.Unused;
            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (CodeExists(poolName, code))
                return Result.Fail($"Mã '{code}' đã tồn tại trong bể.");

            if (finalStatus == (int)CodeStatus.Used && string.IsNullOrWhiteSpace(batchID))
                return Result.Fail("Mã đã dùng phải có BatchID.");

            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            const string sql = @"
                INSERT INTO Codes (Code, Status, BatchID, CreateTime, CreateID, Note)
                VALUES (@Code, @Status, @BatchID, @CreateTime, @CreateID, @Note);";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", code.Trim());
            cmd.Parameters.AddWithValue("@Status", finalStatus);
            cmd.Parameters.AddWithValue("@BatchID", batchID ?? "");
            cmd.Parameters.AddWithValue("@CreateTime", createTime);
            cmd.Parameters.AddWithValue("@CreateID", finalCreateID);
            cmd.Parameters.AddWithValue("@Note", note ?? "");
            cmd.ExecuteNonQuery();

            return Result.Ok("Nhập mã thủ công thành công.", 1);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi khi nhập mã thủ công");
            return Result.Fail($"Lỗi khi nhập mã thủ công: {ex.Message}");
        }
    }

    public static Result Import_FromReader(
        string poolName,
        string code,
        string batchID,
        string createID = "Reader",
        string note = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return Result.Fail("PoolName không được trống.");
            if (string.IsNullOrWhiteSpace(code))
                return Result.Fail("Mã code không được trống.");
            if (string.IsNullOrWhiteSpace(batchID))
                return Result.Fail("BatchID không được trống khi thêm từ reader.");

            EnsurePool(poolName);

            var existing = GetByCode(poolName, code);
            if (existing.IsSuccess && existing.Data != null && existing.Data.Rows.Count > 0)
            {
                int currentStatus = Convert.ToInt32(existing.Data.Rows[0]["Status"]);
                if (currentStatus == (int)CodeStatus.Used)
                    return Result.Fail($"Mã '{code}' đã được sử dụng trước đó.");

                if (UpdateStatus(poolName, code, (int)CodeStatus.Used, batchID, createID, note))
                    return Result.Ok("Đã cập nhật mã chưa dùng thành đã dùng.", 1);
                return Result.Fail("Không thể cập nhật trạng thái mã.");
            }

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            const string sql = @"
                INSERT INTO Codes (Code, Status, BatchID, CreateTime, CreateID, Note)
                VALUES (@Code, @Status, @BatchID, @CreateTime, @CreateID, @Note);";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", code.Trim());
            cmd.Parameters.AddWithValue("@Status", (int)CodeStatus.Used);
            cmd.Parameters.AddWithValue("@BatchID", batchID);
            cmd.Parameters.AddWithValue("@CreateTime", createTime);
            cmd.Parameters.AddWithValue("@CreateID", createID);
            cmd.Parameters.AddWithValue("@Note", note ?? "");
            cmd.ExecuteNonQuery();

            return Result.Ok("Đã thêm mã mới và đánh dấu đã dùng.", 1);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi khi nhập mã từ camera");
            return Result.Fail($"Lỗi khi nhập mã từ camera: {ex.Message}");
        }
    }

    public static Result Import_FromFile(
        string poolName,
        string csvPath,
        string userName,
        string createID,
        string codeColumn,
        string noteColumn = "",
        string note = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return Result.Fail("PoolName không được trống.");
            if (!File.Exists(csvPath))
                return Result.Fail($"Không tìm thấy file CSV: {csvPath}");
            if (string.IsNullOrWhiteSpace(createID))
                return Result.Fail("CreateID không được trống.");
            if (string.IsNullOrWhiteSpace(codeColumn))
                return Result.Fail("Tên cột Code không được trống.");

            EnsurePool(poolName);
            EnsurePhieu(createID);

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2)
                return Result.Fail("File CSV trống hoặc không có dữ liệu.");

            var header = SplitCsvLine(lines[0]);
            int codeIdx = Array.IndexOf(header, codeColumn);
            if (codeIdx < 0)
                return Result.Fail($"Không tìm thấy cột '{codeColumn}' trong file CSV.");

            int? noteIdx = string.IsNullOrWhiteSpace(noteColumn)
                ? null
                : Array.IndexOf(header, noteColumn);

            int successCount = 0;
            int skipCount = 0;

            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            using var tx = con.BeginTransaction();
            const string insertSql = @"
                INSERT OR IGNORE INTO Codes (Code, Status, BatchID, CreateTime, CreateID, Note)
                VALUES (@Code, @Status, @BatchID, @CreateTime, @CreateID, @Note);";

            for (int i = 1; i < lines.Length; i++)
            {
                var row = SplitCsvLine(lines[i]);
                if (row.Length <= codeIdx) continue;
                string code = row[codeIdx].Trim();
                if (string.IsNullOrWhiteSpace(code)) continue;

                string rowNote = (noteIdx.HasValue && row.Length > noteIdx.Value)
                    ? row[noteIdx.Value].Trim()
                    : "";

                using var cmd = new SqliteCommand(insertSql, con, tx);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@Status", (int)CodeStatus.Unused);
                cmd.Parameters.AddWithValue("@BatchID", "");
                cmd.Parameters.AddWithValue("@CreateTime", createTime);
                cmd.Parameters.AddWithValue("@CreateID", createID);
                cmd.Parameters.AddWithValue("@Note", rowNote);
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) successCount++;
                else skipCount++;
            }
            tx.Commit();

            using var conPhieu = new SqliteConnection($"Data Source={GetPhieuPath(createID)}");
            conPhieu.Open();
            const string phieuSql = @"
                INSERT OR REPLACE INTO PhieuTao
                (CreateID, UserName, CreateTime, Note, ImportMethod, ImportSource, ImportCount)
                VALUES (@CreateID, @UserName, @CreateTime, @Note, @ImportMethod, @ImportSource, @ImportCount);";
            using var cmdPhieu = new SqliteCommand(phieuSql, conPhieu);
            cmdPhieu.Parameters.AddWithValue("@CreateID", createID);
            cmdPhieu.Parameters.AddWithValue("@UserName", userName ?? "");
            cmdPhieu.Parameters.AddWithValue("@CreateTime", createTime);
            cmdPhieu.Parameters.AddWithValue("@Note", note ?? "");
            cmdPhieu.Parameters.AddWithValue("@ImportMethod", "File");
            cmdPhieu.Parameters.AddWithValue("@ImportSource", csvPath);
            cmdPhieu.Parameters.AddWithValue("@ImportCount", successCount);
            cmdPhieu.ExecuteNonQuery();

            return Result.Ok(
                $"Nhập từ file hoàn tất: {successCount} thêm mới, {skipCount} bị bỏ qua (trùng mã).",
                successCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi khi nhập từ file");
            return Result.Fail($"Lỗi khi nhập từ file: {ex.Message}");
        }
    }

    public static Result Import_FromContent(
        string poolName,
        string csvContent,
        string userName,
        string createID,
        string note = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return Result.Fail("PoolName không được trống.");
            if (string.IsNullOrWhiteSpace(csvContent))
                return Result.Fail("CSV content không được trống.");
            if (string.IsNullOrWhiteSpace(createID))
                return Result.Fail("CreateID không được trống.");

            EnsurePool(poolName);
            EnsurePhieu(createID);

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
                return Result.Fail("CSV content phải có ít nhất 1 header và 1 dòng dữ liệu.");

            var headers = SplitCsvLine(lines[0]);
            int codeIdx = 0;
            int noteIdx = -1;

            // Auto-detect code column
            for (int i = 0; i < headers.Length; i++)
            {
                var h = headers[i].ToLower();
                if (h.Contains("code") || h.Contains("gtin") || h.Contains("qr") || h.Contains("id"))
                {
                    codeIdx = i;
                    break;
                }
            }

            int successCount = 0;
            int skipCount = 0;

            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            using var tx = con.BeginTransaction();
            const string insertSql = @"
                INSERT OR IGNORE INTO Codes (Code, Status, BatchID, CreateTime, CreateID, Note)
                VALUES (@Code, @Status, @BatchID, @CreateTime, @CreateID, @Note);";

            for (int i = 1; i < lines.Length; i++)
            {
                var row = SplitCsvLine(lines[i]);
                if (row.Length <= codeIdx) continue;
                string code = row[codeIdx].Trim();
                if (string.IsNullOrWhiteSpace(code)) continue;

                string rowNote = noteIdx >= 0 && row.Length > noteIdx ? row[noteIdx].Trim() : "";

                using var cmd = new SqliteCommand(insertSql, con, tx);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@Status", (int)CodeStatus.Unused);
                cmd.Parameters.AddWithValue("@BatchID", "");
                cmd.Parameters.AddWithValue("@CreateTime", createTime);
                cmd.Parameters.AddWithValue("@CreateID", createID);
                cmd.Parameters.AddWithValue("@Note", rowNote);
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) successCount++;
                else skipCount++;
            }
            tx.Commit();

            using var conPhieu = new SqliteConnection($"Data Source={GetPhieuPath(createID)}");
            conPhieu.Open();
            const string phieuSql = @"
                INSERT OR REPLACE INTO PhieuTao
                (CreateID, UserName, CreateTime, Note, ImportMethod, ImportSource, ImportCount)
                VALUES (@CreateID, @UserName, @CreateTime, @Note, @ImportMethod, @ImportSource, @ImportCount);";
            using var cmdPhieu = new SqliteCommand(phieuSql, conPhieu);
            cmdPhieu.Parameters.AddWithValue("@CreateID", createID);
            cmdPhieu.Parameters.AddWithValue("@UserName", userName ?? "");
            cmdPhieu.Parameters.AddWithValue("@CreateTime", createTime);
            cmdPhieu.Parameters.AddWithValue("@Note", note ?? "");
            cmdPhieu.Parameters.AddWithValue("@ImportMethod", "Content");
            cmdPhieu.Parameters.AddWithValue("@ImportSource", "ClientUpload");
            cmdPhieu.Parameters.AddWithValue("@ImportCount", successCount);
            cmdPhieu.ExecuteNonQuery();

            return Result.Ok(
                $"Nhập từ content hoàn tất: {successCount} thêm mới, {skipCount} bị bỏ qua (trùng mã).",
                successCount);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi khi nhập từ content");
            return Result.Fail($"Lỗi khi nhập từ content: {ex.Message}");
        }
    }

    public static bool CodeExists(string poolName, string code)
    {
        try
        {
            EnsurePool(poolName);
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            using var cmd = new SqliteCommand("SELECT COUNT(1) FROM Codes WHERE Code = @Code;", con);
            cmd.Parameters.AddWithValue("@Code", code);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch { return false; }
    }

    public static Result GetByCode(string poolName, string code)
    {
        try
        {
            EnsurePool(poolName);
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            const string sql = "SELECT * FROM Codes WHERE Code = @Code LIMIT 1;";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", code);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            if (table.Rows.Count > 0)
                return Result.Ok("Lấy thông tin mã thành công.", 1, table);
            return Result.Fail("Không tìm thấy mã.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi truy vấn GetByCode");
            return Result.Fail($"Lỗi truy vấn: {ex.Message}");
        }
    }

    public static Result GetAll(string poolName, int? status = null, int limit = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return Result.Fail("PoolName không được trống.");

            EnsurePool(poolName);

            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            string sql = status.HasValue
                ? "SELECT * FROM Codes WHERE Status = @Status ORDER BY ID ASC"
                : "SELECT * FROM Codes ORDER BY ID ASC";
            if (limit > 0)
                sql += $" LIMIT {limit}";
            sql += ";";
            using var cmd = new SqliteCommand(sql, con);
            if (status.HasValue)
                cmd.Parameters.AddWithValue("@Status", status.Value);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            return Result.Ok($"Lấy {table.Rows.Count} mã thành công.", table.Rows.Count, table);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi truy vấn GetAll");
            return Result.Fail($"Lỗi truy vấn: {ex.Message}");
        }
    }

    public static Result SearchCodes(string poolName, string keyword, int limit = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(poolName))
                return Result.Fail("PoolName không được trống.");
            if (string.IsNullOrWhiteSpace(keyword))
                return Result.Fail("Keyword không được trống.");

            EnsurePool(poolName);
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();

            string sql = @"SELECT * FROM Codes 
                           WHERE Code LIKE @Keyword 
                           ORDER BY ID ASC 
                           LIMIT @Limit;";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
            cmd.Parameters.AddWithValue("@Limit", limit);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            return Result.Ok($"Tìm thấy {table.Rows.Count} mã.", table.Rows.Count, table);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi tìm kiếm SearchCodes");
            return Result.Fail($"Lỗi tìm kiếm: {ex.Message}");
        }
    }

    public static bool UpdateStatus(string poolName, string code, int newStatus, string batchID = "", string createID = "", string note = "")
    {
        try
        {
            EnsurePool(poolName);
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            const string sql = @"
                UPDATE Codes
                SET Status = @Status,
                    BatchID = COALESCE(NULLIF(@BatchID, ''), BatchID),
                    CreateID = COALESCE(NULLIF(@CreateID, ''), CreateID),
                    Note = COALESCE(NULLIF(@Note, ''), Note)
                WHERE Code = @Code;";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Status", newStatus);
            cmd.Parameters.AddWithValue("@BatchID", batchID ?? "");
            cmd.Parameters.AddWithValue("@CreateID", createID ?? "");
            cmd.Parameters.AddWithValue("@Note", note ?? "");
            cmd.Parameters.AddWithValue("@Code", code);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public static Result MarkUsed(string poolName, string code, string batchID, string createID = "", string note = "")
    {
        if (string.IsNullOrWhiteSpace(batchID))
            return Result.Fail("BatchID không được trống khi đánh dấu đã dùng.");
        if (UpdateStatus(poolName, code, (int)CodeStatus.Used, batchID, createID, note))
            return Result.Ok($"Đã đánh dấu mã '{code}' là đã dùng.");
        return Result.Fail($"Không tìm thấy mã '{code}'.");
    }

    public static Result MarkUnused(string poolName, string code)
    {
        if (UpdateStatus(poolName, code, (int)CodeStatus.Unused, "", "", ""))
            return Result.Ok($"Đã reset mã '{code}' về chưa dùng.");
        return Result.Fail($"Không tìm thấy mã '{code}'.");
    }

    public static Result Delete(string poolName, string code)
    {
        try
        {
            EnsurePool(poolName);
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();
            using var cmd = new SqliteCommand("DELETE FROM Codes WHERE Code = @Code;", con);
            cmd.Parameters.AddWithValue("@Code", code);
            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                return Result.Ok($"Đã xóa mã '{code}'.");
            return Result.Fail($"Không tìm thấy mã '{code}'.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi khi xóa mã");
            return Result.Fail($"Lỗi khi xóa mã: {ex.Message}");
        }
    }

    public static Result ListAllPools()
    {
        try
        {
            if (!Directory.Exists(_dataPath))
                return Result.Ok("Thư mục DataPool chưa tồn tại.", 0, new DataTable());

            var table = new DataTable();
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("fileName", typeof(string));
            table.Columns.Add("size", typeof(long));

            var files = Directory.GetFiles(_dataPath, "*.db");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                table.Rows.Add(
                    Path.GetFileNameWithoutExtension(file),
                    info.Name,
                    info.Length);
            }

            return Result.Ok($"Tìm thấy {table.Rows.Count} pools.", table.Rows.Count, table);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DataPoolHelper: Lỗi liệt kê pools");
            return Result.Fail($"Lỗi liệt kê pools: {ex.Message}");
        }
    }

    public static PoolStats GetPoolStats(string poolName)
    {
        try
        {
            EnsurePool(poolName);
            using var con = new SqliteConnection($"Data Source={GetPoolPath(poolName)}");
            con.Open();

            int total = 0, unused = 0, used = 0;

            using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Codes;", con))
                total = Convert.ToInt32(cmd.ExecuteScalar());
            using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Codes WHERE Status = 0;", con))
                unused = Convert.ToInt32(cmd.ExecuteScalar());
            using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Codes WHERE Status = 1;", con))
                used = Convert.ToInt32(cmd.ExecuteScalar());

            return new PoolStats(total, unused, used);
        }
        catch
        {
            return new PoolStats(0, 0, 0);
        }
    }

    public static List<PoolStats> GetAllPoolStats()
    {
        var result = new List<PoolStats>();
        try
        {
            if (!Directory.Exists(_dataPath)) return result;

            var files = Directory.GetFiles(_dataPath, "*.db");
            foreach (var file in files)
            {
                var poolName = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(poolName, "Phieu", StringComparison.OrdinalIgnoreCase)) continue;
                var stats = GetPoolStats(poolName);
                stats.Name = poolName;
                result.Add(stats);
            }
        }
        catch { }
        return result;
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuote = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuote && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuote = !inQuote;
                }
            }
            else if (c == ',' && !inQuote)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result.ToArray();
    }
}
