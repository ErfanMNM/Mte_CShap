using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.Logs;

/// <summary>
/// Reads log entries from the SQLite log database produced by the Raycoon SQLite sink.
/// </summary>
public static class LogRepository
{
    private const string DbPath = @"C:\GProject\Logs\gproject.db";
    private const string ConnStr = $"Data Source={DbPath};Mode=ReadOnly";

    // Extracts the leading "[Tag]" from a rendered message, e.g.
    // "[DataPoolHelper] Lỗi khi nhập mã thủ công" -> "DataPoolHelper"
    // Does NOT match "[Camera:camera]" because ':' is excluded.
    private static readonly Regex TagRegex = new(@"^\[([^\[\]:]+)\]", RegexOptions.Compiled);

    public static string DatabasePath => DbPath;

    public static bool DatabaseExists() => File.Exists(DbPath);

    public static LogPage Query(LogQuery q)
    {
        var page = q.Page < 1 ? 1 : q.Page;
        var pageSize = q.PageSize switch
        {
            < 1 => 50,
            > 500 => 500,
            _ => q.PageSize,
        };
        var sortDesc = !string.Equals(q.Sort, "asc", StringComparison.OrdinalIgnoreCase);

        var where = new List<string>();
        var parameters = new List<SqliteParameter>();

        // Default range: last 24h when caller does not specify from/to.
        var from = q.From ?? DateTime.UtcNow.AddHours(-24);
        var to = q.To ?? DateTime.UtcNow;
        where.Add("Timestamp >= @from AND Timestamp <= @to");
        parameters.Add(new SqliteParameter("@from", from.ToString("o")));
        parameters.Add(new SqliteParameter("@to", to.ToString("o")));

        if (!string.IsNullOrWhiteSpace(q.LevelName))
        {
            where.Add("LevelName = @level");
            parameters.Add(new SqliteParameter("@level", q.LevelName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(q.MessageContains))
        {
            where.Add("Message LIKE @msg");
            parameters.Add(new SqliteParameter("@msg", "%" + q.MessageContains + "%"));
        }

        // Tag filter is applied in C# after fetching, because the tag is encoded in the
        // rendered Message ("[DataPoolHelper] Lỗi..."), not in a dedicated column.
        // We still want a sane upper bound for in-memory filtering to avoid fetching
        // an unbounded result set.
        var tagFilter = string.IsNullOrWhiteSpace(q.TagContains) ? null : q.TagContains.Trim();

        var whereSql = string.Join(" AND ", where);
        var orderSql = sortDesc
            ? "ORDER BY Timestamp DESC, Id DESC"
            : "ORDER BY Timestamp ASC, Id ASC";

        if (!DatabaseExists())
        {
            return new LogPage(Array.Empty<LogEntry>(), 0, page, pageSize, 0, false, false);
        }

        using var con = new SqliteConnection(ConnStr);
        con.Open();

        long totalCount;
        using (var countCmd = new SqliteCommand($"SELECT COUNT(*) FROM Logs WHERE {whereSql}", con))
        {
            foreach (var p in parameters) countCmd.Parameters.Add(Clone(p));
            totalCount = (long)(countCmd.ExecuteScalar() ?? 0L);
        }

        if (totalCount == 0)
        {
            return new LogPage(Array.Empty<LogEntry>(), 0, page, pageSize, 0, false, false);
        }

        var totalPages = (int)Math.Max(1, Math.Ceiling(totalCount / (double)pageSize));
        var offset = (page - 1) * pageSize;

        var items = new List<LogEntry>(pageSize);
        using (var cmd = new SqliteCommand(
            $"SELECT Id, Timestamp, Level, LevelName, Message, MessageTemplate, Exception, Properties, SourceContext, MachineName, ThreadId " +
            $"FROM Logs WHERE {whereSql} {orderSql} LIMIT @limit OFFSET @offset",
            con))
        {
            foreach (var p in parameters) cmd.Parameters.Add(Clone(p));
            cmd.Parameters.Add(new SqliteParameter("@limit", pageSize));
            cmd.Parameters.Add(new SqliteParameter("@offset", offset));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var msg = reader.IsDBNull(4) ? "" : reader.GetString(4);
                items.Add(new LogEntry(
                    Id: reader.GetInt64(0),
                    Timestamp: ParseTimestamp(reader.GetString(1)),
                    Level: reader.IsDBNull(3) ? "" : reader.GetString(3),
                    LevelName: reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Message: msg,
                    MessageTemplate: reader.IsDBNull(5) ? null : reader.GetString(5),
                    Exception: reader.IsDBNull(6) ? null : reader.GetString(6),
                    PropertiesJson: reader.IsDBNull(7) ? null : reader.GetString(7),
                    SourceContext: reader.IsDBNull(8) ? null : reader.GetString(8),
                    MachineName: reader.IsDBNull(9) ? null : reader.GetString(9),
                    ThreadId: reader.IsDBNull(10) ? null : reader.GetInt32(10)));
            }
        }

        // Apply tag filter on the rendered Message.
        if (tagFilter != null)
        {
            items = items.Where(e => ExtractTag(e.Message)?.Contains(tagFilter, StringComparison.OrdinalIgnoreCase) == true).ToList();
            // Recompute total for consistency when tag filter was applied client-side.
            totalCount = items.Count;
            totalPages = (int)Math.Max(1, Math.Ceiling(totalCount / (double)pageSize));
        }

        var hasNext = page < totalPages;
        var hasPrev = page > 1;

        return new LogPage(items, totalCount, page, pageSize, totalPages, hasNext, hasPrev);
    }

    public static LogEntry? GetById(long id)
    {
        if (!DatabaseExists()) return null;

        using var con = new SqliteConnection(ConnStr);
        con.Open();

        using var cmd = new SqliteCommand(
            "SELECT Id, Timestamp, Level, LevelName, Message, MessageTemplate, Exception, Properties, SourceContext, MachineName, ThreadId " +
            "FROM Logs WHERE Id = @id LIMIT 1",
            con);
        cmd.Parameters.Add(new SqliteParameter("@id", id));

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new LogEntry(
            Id: reader.GetInt64(0),
            Timestamp: ParseTimestamp(reader.GetString(1)),
            Level: reader.IsDBNull(3) ? "" : reader.GetString(3),
            LevelName: reader.IsDBNull(3) ? "" : reader.GetString(3),
            Message: reader.IsDBNull(4) ? "" : reader.GetString(4),
            MessageTemplate: reader.IsDBNull(5) ? null : reader.GetString(5),
            Exception: reader.IsDBNull(6) ? null : reader.GetString(6),
            PropertiesJson: reader.IsDBNull(7) ? null : reader.GetString(7),
            SourceContext: reader.IsDBNull(8) ? null : reader.GetString(8),
            MachineName: reader.IsDBNull(9) ? null : reader.GetString(9),
            ThreadId: reader.IsDBNull(10) ? null : reader.GetInt32(10));
    }

    public static IReadOnlyList<string> ListDistinctLevels()
    {
        const string sql = "SELECT DISTINCT LevelName FROM Logs WHERE LevelName IS NOT NULL ORDER BY LevelName";
        var result = new List<string>();
        if (!DatabaseExists()) return result;

        using var con = new SqliteConnection(ConnStr);
        con.Open();
        using var cmd = new SqliteCommand(sql, con);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var s = reader.GetString(0);
            if (!string.IsNullOrEmpty(s)) result.Add(s);
        }
        return result;
    }

    /// <summary>
    /// Best-effort list of tags found in the leading "[Tag]" of each rendered message.
    /// </summary>
    public static IReadOnlyList<string> ListDistinctTags(int maxRows = 5000)
    {
        if (!DatabaseExists()) return Array.Empty<string>();

        using var con = new SqliteConnection(ConnStr);
        con.Open();

        using var cmd = new SqliteCommand(
            $"SELECT Message FROM (SELECT Message FROM Logs ORDER BY Id DESC LIMIT @limit)",
            con);
        cmd.Parameters.Add(new SqliteParameter("@limit", maxRows));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (reader.IsDBNull(0)) continue;
            var t = ExtractTag(reader.GetString(0));
            if (!string.IsNullOrEmpty(t)) seen.Add(t);
        }
        return seen.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static string? ExtractTag(string message)
    {
        if (string.IsNullOrEmpty(message)) return null;
        var m = TagRegex.Match(message);
        return m.Success ? m.Groups[1].Value : null;
    }

    private static SqliteParameter Clone(SqliteParameter src)
        => new(src.ParameterName, src.Value);

    private static DateTime ParseTimestamp(string iso)
    {
        if (DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            return dt;
        Log.Warning("[LogRepository] Failed to parse timestamp: {Raw}", iso);
        return DateTime.UtcNow;
    }
}
