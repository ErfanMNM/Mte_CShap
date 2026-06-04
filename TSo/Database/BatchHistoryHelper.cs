using System.Data.SQLite;
using TSo.Models;
using TSo.Configs;

namespace TSo.Database;

public static class BatchHistoryHelper
{
    private const string CreateTableSql = @"
        CREATE TABLE IF NOT EXISTS BatchHistory (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            BatchCode TEXT NOT NULL,
            Barcode TEXT NOT NULL,
            CreatedBy TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IDX_BH_BatchCode ON BatchHistory(BatchCode);
        PRAGMA journal_mode=WAL;
    ";

    public static void EnsureDatabase(string dbPath)
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = new SQLiteCommand(CreateTableSql, con);
        cmd.ExecuteNonQuery();
    }

    public static void AddHistory(string dbPath, string batchCode, string barcode, string createdBy, DateTime createdAt)
    {
        EnsureDatabase(dbPath);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = @"INSERT INTO BatchHistory (BatchCode, Barcode, CreatedBy, CreatedAt)
                    VALUES (@batchCode, @barcode, @createdBy, @createdAt)";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@batchCode", batchCode);
        cmd.Parameters.AddWithValue("@barcode", barcode);
        cmd.Parameters.AddWithValue("@createdBy", createdBy);
        cmd.Parameters.AddWithValue("@createdAt", createdAt.ToString("yyyy-MM-dd HH:mm:ss"));

        cmd.ExecuteNonQuery();
    }

    public static BatchInfo? GetLatest(string dbPath)
    {
        EnsureDatabase(dbPath);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = "SELECT ID, BatchCode, Barcode, CreatedBy, CreatedAt FROM BatchHistory ORDER BY ID DESC LIMIT 1";
        using var cmd = new SQLiteCommand(sql, con);
        using var rd = cmd.ExecuteReader();

        if (rd.Read())
        {
            return new BatchInfo
            {
                BatchCode = rd.GetString(1),
                Barcode = rd.GetString(2),
                CreatedBy = rd.GetString(3),
                CreatedAt = DateTime.Parse(rd.GetString(4))
            };
        }

        return null;
    }

    public static List<BatchInfo> GetHistory(string dbPath, int limit = 50)
    {
        EnsureDatabase(dbPath);
        var list = new List<BatchInfo>();

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = "SELECT ID, BatchCode, Barcode, CreatedBy, CreatedAt FROM BatchHistory ORDER BY ID DESC LIMIT @limit";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@limit", limit);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new BatchInfo
            {
                BatchCode = rd.GetString(1),
                Barcode = rd.GetString(2),
                CreatedBy = rd.GetString(3),
                CreatedAt = DateTime.Parse(rd.GetString(4))
            });
        }

        return list;
    }
}
