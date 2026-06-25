using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Text;

namespace GProject.DataPoolHelper;

/// <summary>
/// DataPool - Core class cho từng bể dữ liệu.
/// Dùng như: DataPool_s1.PoolName = "abc";
/// </summary>
public class DataPool
{
    public static string DefaultDataPath => @"C:\GProject\DataPool";

    public string PoolName { get; set; } = "default";
    public string DataPath { get; set; } = DefaultDataPath;
    public string DbPath => Path.Combine(DataPath, PoolName + ".db");
    public string UserName { get; set; } = "System";

    public DataPool() { }

    public DataPool(string poolName, string? dataPath = null)
    {
        PoolName = poolName;
        if (!string.IsNullOrEmpty(dataPath))
            DataPath = dataPath;
    }

    private void EnsureCreated()
    {
        if (string.IsNullOrWhiteSpace(PoolName))
            throw new ArgumentException("PoolName không được trống.");

        if (!Directory.Exists(DataPath))
            Directory.CreateDirectory(DataPath);

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

        using var con = new SqliteConnection($"Data Source={DbPath}");
        con.Open();
        using var cmd = new SqliteCommand(sql, con);
        cmd.ExecuteNonQuery();
    }

    public Result Add(string code, string batchID, string createID = "Reader", string note = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Fail("Mã code không được trống.");
            if (string.IsNullOrWhiteSpace(batchID))
                return Result.Fail("BatchID không được trống khi thêm từ reader.");

            EnsureCreated();

            var existing = GetByCode(code);
            if (existing.IsSuccess && existing.Data?.Rows.Count > 0)
            {
                int currentStatus = Convert.ToInt32(existing.Data.Rows[0]["Status"]);
                if (currentStatus == (int)CodeStatus.Used)
                    return Result.Fail($"Mã '{code}' đã được sử dụng trước đó.");

                if (UpdateStatus(code, (int)CodeStatus.Used, batchID, createID, note))
                    return Result.Ok($"Đã cập nhật mã '{code}' thành đã dùng.", 1);
                return Result.Fail("Không thể cập nhật trạng thái mã.");
            }

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using var con = new SqliteConnection($"Data Source={DbPath}");
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

            return Result.Ok($"Đã thêm mã mới '{code}' và đánh dấu đã dùng.", 1);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Lỗi khi thêm mã: {ex.Message}");
        }
    }

    public Result AddManual(string code, string? batchID = null, string? note = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result.Fail("Mã code không được trống.");

            EnsureCreated();

            if (CodeExists(code))
                return Result.Fail($"Mã '{code}' đã tồn tại trong bể.");

            if (batchID != null && !string.IsNullOrWhiteSpace(batchID))
            {
                return Add(code, batchID, $"User:{UserName}", note ?? "");
            }

            string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();
            const string sql = @"
                INSERT INTO Codes (Code, Status, BatchID, CreateTime, CreateID, Note)
                VALUES (@Code, @Status, @BatchID, @CreateTime, @CreateID, @Note);";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", code.Trim());
            cmd.Parameters.AddWithValue("@Status", (int)CodeStatus.Unused);
            cmd.Parameters.AddWithValue("@BatchID", "");
            cmd.Parameters.AddWithValue("@CreateTime", createTime);
            cmd.Parameters.AddWithValue("@CreateID", $"User:{UserName}");
            cmd.Parameters.AddWithValue("@Note", note ?? "");
            cmd.ExecuteNonQuery();

            return Result.Ok($"Đã thêm mã '{code}' thủ công.", 1);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Lỗi khi thêm mã thủ công: {ex.Message}");
        }
    }

    public Result ImportFromFile(string csvPath, string createID, string codeColumn, string noteColumn = "", string note = "")
    {
        try
        {
            if (!File.Exists(csvPath))
                return Result.Fail($"Không tìm thấy file CSV: {csvPath}");
            if (string.IsNullOrWhiteSpace(createID))
                return Result.Fail("CreateID không được trống.");
            if (string.IsNullOrWhiteSpace(codeColumn))
                return Result.Fail("Tên cột Code không được trống.");

            EnsureCreated();

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

            using var con = new SqliteConnection($"Data Source={DbPath}");
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

            return Result.Ok(
                $"Nhập từ file hoàn tất: {successCount} thêm mới, {skipCount} bị bỏ qua (trùng mã).",
                successCount);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Lỗi khi nhập từ file: {ex.Message}");
        }
    }

    public bool CodeExists(string code)
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();
            using var cmd = new SqliteCommand("SELECT COUNT(1) FROM Codes WHERE Code = @Code;", con);
            cmd.Parameters.AddWithValue("@Code", code);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch { return false; }
    }

    public Result GetByCode(string code)
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
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
            return Result.Fail($"Lỗi truy vấn: {ex.Message}");
        }
    }

    public Result GetAll()
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();
            const string sql = "SELECT * FROM Codes ORDER BY ID ASC;";
            using var cmd = new SqliteCommand(sql, con);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            return Result.Ok($"Lấy {table.Rows.Count} mã thành công.", table.Rows.Count, table);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Lỗi truy vấn: {ex.Message}");
        }
    }

    public Result GetByStatus(CodeStatus status)
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
            con.Open();
            string sql = "SELECT * FROM Codes WHERE Status = @Status ORDER BY ID ASC;";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@Status", (int)status);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            return Result.Ok($"Lấy {table.Rows.Count} mã (Status={status}) thành công.", table.Rows.Count, table);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Lỗi truy vấn: {ex.Message}");
        }
    }

    public Result GetUnused() => GetByStatus(CodeStatus.Unused);
    public Result GetUsed() => GetByStatus(CodeStatus.Used);

    public bool UpdateStatus(string code, int newStatus, string batchID = "", string createID = "", string note = "")
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
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

    public Result MarkUsed(string code, string batchID, string createID = "", string note = "")
    {
        if (string.IsNullOrWhiteSpace(batchID))
            return Result.Fail("BatchID không được trống khi đánh dấu đã dùng.");

        if (UpdateStatus(code, (int)CodeStatus.Used, batchID, createID, note))
            return Result.Ok($"Đã đánh dấu mã '{code}' là đã dùng (BatchID: {batchID}).");
        return Result.Fail($"Không tìm thấy mã '{code}'.");
    }

    public Result MarkUnused(string code)
    {
        if (UpdateStatus(code, (int)CodeStatus.Unused, "", "", ""))
            return Result.Ok($"Đã reset mã '{code}' về chưa dùng.");
        return Result.Fail($"Không tìm thấy mã '{code}'.");
    }

    public Result Delete(string code)
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
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
            return Result.Fail($"Lỗi khi xóa mã: {ex.Message}");
        }
    }

    public PoolStats GetStats()
    {
        try
        {
            EnsureCreated();
            using var con = new SqliteConnection($"Data Source={DbPath}");
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
