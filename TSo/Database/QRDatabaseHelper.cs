using System.Data.SQLite;
using System.Data;
using TSo.Models;
using TSo.Configs;

namespace TSo.Database;

public static class QRDatabaseHelper
{
    private const string CreateTableSql = @"
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
            Reason TEXT NOT NULL DEFAULT ''
        );
        CREATE INDEX IF NOT EXISTS IDX_QR_QRContent ON QRProducts(QRContent);
        CREATE INDEX IF NOT EXISTS IDX_QR_BatchCode ON QRProducts(BatchCode);
        PRAGMA journal_mode=WAL;
    ";

    private const string CreateActiveUniqueSql = @"
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

    public static void Init()
    {
        EnsureDatabase(AppConfigs.QRDatadbPath);
        EnsureActiveUniqueDatabase(AppConfigs.ActiveUniqueDbPath);
    }

    private static void EnsureDatabase(string dbPath)
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = new SQLiteCommand(CreateTableSql, con);
        cmd.ExecuteNonQuery();
    }

    private static void EnsureActiveUniqueDatabase(string dbPath)
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = new SQLiteCommand(CreateActiveUniqueSql, con);
        cmd.ExecuteNonQuery();
    }

    public static int AddOrActivateCode(QRProductRecord record, string dbPath)
    {
        EnsureDatabase(dbPath);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = @"
            INSERT INTO QRProducts
            (QRContent, BatchCode, Barcode, Status, UserName,
             TimeStampActive, TimeUnixActive, ProductionDatetime, Reason)
            VALUES
            (@QRContent, @BatchCode, @Barcode, @Status, @UserName,
             @TimeStampActive, @TimeUnixActive, @ProductionDatetime, @Reason);
            SELECT last_insert_rowid();";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@QRContent", record.QRContent);
        cmd.Parameters.AddWithValue("@BatchCode", record.BatchCode);
        cmd.Parameters.AddWithValue("@Barcode", record.Barcode);
        cmd.Parameters.AddWithValue("@Status", record.Status.ToString());
        cmd.Parameters.AddWithValue("@UserName", record.UserName);
        cmd.Parameters.AddWithValue("@TimeStampActive", record.TimeStampActive);
        cmd.Parameters.AddWithValue("@TimeUnixActive", record.TimeUnixActive);
        cmd.Parameters.AddWithValue("@ProductionDatetime", record.ProductionDatetime);
        cmd.Parameters.AddWithValue("@Reason", record.Reason ?? "");

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static bool AddActiveCodeUnique(QRProductRecord record, string dbPath)
    {
        EnsureActiveUniqueDatabase(dbPath);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = @"
            INSERT OR IGNORE INTO ActiveUniqueQR
            (QRContent, BatchCode, Barcode, UserName,
             TimeStampActive, TimeUnixActive, ProductionDatetime)
            VALUES
            (@QRContent, @BatchCode, @Barcode, @UserName,
             @TimeStampActive, @TimeUnixActive, @ProductionDatetime);";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@QRContent", record.QRContent);
        cmd.Parameters.AddWithValue("@BatchCode", record.BatchCode);
        cmd.Parameters.AddWithValue("@Barcode", record.Barcode);
        cmd.Parameters.AddWithValue("@UserName", record.UserName);
        cmd.Parameters.AddWithValue("@TimeStampActive", record.TimeStampActive);
        cmd.Parameters.AddWithValue("@TimeUnixActive", record.TimeUnixActive);
        cmd.Parameters.AddWithValue("@ProductionDatetime", record.ProductionDatetime);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool DeactivateCode(string qrContent, string reason, string userName)
    {
        return UpdateStatus(qrContent, "Deactive", reason, userName);
    }

    public static bool UpdateStatus(string qrContent, string newStatus, string reason, string userName, string dbPath = null!)
    {
        dbPath ??= AppConfigs.QRDatadbPath;
        EnsureDatabase(dbPath);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = @"
            UPDATE QRProducts
            SET Status = @Status, Reason = @Reason, UserName = @UserName
            WHERE QRContent = @QRContent";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@Status", newStatus);
        cmd.Parameters.AddWithValue("@Reason", reason ?? "");
        cmd.Parameters.AddWithValue("@UserName", userName);
        cmd.Parameters.AddWithValue("@QRContent", qrContent);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static List<QRProductRecord> GetByQRContent(string qrContent, int limit = 5)
    {
        EnsureDatabase(AppConfigs.QRDatadbPath);
        var records = new List<QRProductRecord>();

        using var con = new SQLiteConnection($"Data Source={AppConfigs.QRDatadbPath}");
        con.Open();

        var sql = @"
            SELECT ID, QRContent, BatchCode, Barcode, Status, UserName,
                   TimeStampActive, TimeUnixActive, ProductionDatetime, Reason
            FROM QRProducts
            WHERE QRContent LIKE @QRContent
            LIMIT @Limit";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@QRContent", $"%{qrContent}%");
        cmd.Parameters.AddWithValue("@Limit", limit);

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            records.Add(ReadRecord(rd));
        }

        return records;
    }

    public static List<QRProductRecord> GetActiveByQRContent(string qrContent)
    {
        EnsureActiveUniqueDatabase(AppConfigs.ActiveUniqueDbPath);
        var records = new List<QRProductRecord>();

        using var con = new SQLiteConnection($"Data Source={AppConfigs.ActiveUniqueDbPath}");
        con.Open();

        var sql = @"
            SELECT ID, QRContent, BatchCode, Barcode, Status, UserName,
                   TimeStampActive, TimeUnixActive, ProductionDatetime, Reason = ''
            FROM ActiveUniqueQR
            WHERE QRContent LIKE @QRContent
            LIMIT 1";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@QRContent", $"%{qrContent}%");

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            records.Add(ReadRecord(rd));
        }

        return records;
    }

    public static bool ActiveCodeExists(string qrContent)
    {
        EnsureActiveUniqueDatabase(AppConfigs.ActiveUniqueDbPath);

        using var con = new SQLiteConnection($"Data Source={AppConfigs.ActiveUniqueDbPath}");
        con.Open();

        var sql = "SELECT COUNT(*) FROM ActiveUniqueQR WHERE QRContent = @QRContent COLLATE NOCASE";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@QRContent", qrContent);

        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public static HashSet<string> LoadActiveToHashSet()
    {
        EnsureActiveUniqueDatabase(AppConfigs.ActiveUniqueDbPath);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var con = new SQLiteConnection($"Data Source={AppConfigs.ActiveUniqueDbPath}");
        con.Open();

        using var cmd = new SQLiteCommand("SELECT QRContent FROM ActiveUniqueQR", con);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            set.Add(rd.GetString(0));
        }

        return set;
    }

    public static int GetActiveCodeCount()
    {
        EnsureActiveUniqueDatabase(AppConfigs.ActiveUniqueDbPath);

        using var con = new SQLiteConnection($"Data Source={AppConfigs.ActiveUniqueDbPath}");
        con.Open();

        using var cmd = new SQLiteCommand("SELECT COUNT(*) FROM ActiveUniqueQR", con);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static int GetRowCount(string? batchCode = null, string? status = null)
    {
        EnsureDatabase(AppConfigs.QRDatadbPath);

        using var con = new SQLiteConnection($"Data Source={AppConfigs.QRDatadbPath}");
        con.Open();

        var sql = "SELECT COUNT(*) FROM QRProducts WHERE 1=1";
        if (!string.IsNullOrEmpty(batchCode)) sql += " AND BatchCode = @BatchCode";
        if (!string.IsNullOrEmpty(status)) sql += " AND Status = @Status";

        using var cmd = new SQLiteCommand(sql, con);
        if (!string.IsNullOrEmpty(batchCode))
            cmd.Parameters.AddWithValue("@BatchCode", batchCode);
        if (!string.IsNullOrEmpty(status))
            cmd.Parameters.AddWithValue("@Status", status);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static int GetRecordCountByBatch(string batchCode)
    {
        return GetRowCount(batchCode);
    }

    public static int GetActiveCountByBatch(string batchCode)
    {
        EnsureActiveUniqueDatabase(AppConfigs.ActiveUniqueDbPath);

        using var con = new SQLiteConnection($"Data Source={AppConfigs.ActiveUniqueDbPath}");
        con.Open();

        var sql = "SELECT COUNT(*) FROM ActiveUniqueQR WHERE BatchCode = @BatchCode";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@BatchCode", batchCode);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static long GetHourlyProduction(long timeunix)
    {
        EnsureDatabase(AppConfigs.QRDatadbPath);

        using var con = new SQLiteConnection($"Data Source={AppConfigs.QRDatadbPath}");
        con.Open();

        var sql = "SELECT COUNT(*) FROM QRProducts WHERE TimeUnixActive >= @timeunix";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@timeunix", timeunix);

        return Convert.ToInt64(cmd.ExecuteScalar()) * 4;
    }

    private static QRProductRecord ReadRecord(IDataRecord rd)
    {
        var statusStr = rd["Status"]?.ToString() ?? "Pass";
        var status = Enum.TryParse<e_Production_Status>(statusStr, true, out var s) ? s : e_Production_Status.Pass;

        return new QRProductRecord
        {
            Id = Convert.ToInt32(rd["ID"]),
            QRContent = rd["QRContent"]?.ToString() ?? "",
            BatchCode = rd["BatchCode"]?.ToString() ?? "",
            Barcode = rd["Barcode"]?.ToString() ?? "",
            Status = status,
            UserName = rd["UserName"]?.ToString() ?? "",
            TimeStampActive = rd["TimeStampActive"]?.ToString() ?? "",
            TimeUnixActive = Convert.ToInt64(rd["TimeUnixActive"]),
            ProductionDatetime = rd["ProductionDatetime"]?.ToString() ?? "",
            Reason = rd["Reason"]?.ToString() ?? ""
        };
    }
}
