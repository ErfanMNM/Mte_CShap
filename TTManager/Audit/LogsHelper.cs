using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTManager.Audit
{
    /// <summary>
    /// Log entry model.
    /// </summary>
    public class LogEntry<TAction> where TAction : struct, IConvertible
    {
        public int Id { get; set; }
        public string? TimeISO { get; set; }
        public long TimestampMs { get; set; }
        public string? User { get; set; }
        /// <summary>Business code / module (e.g. "PO", "AUTH", "SYS").</summary>
        public string? Code { get; set; }
        public TAction Action { get; set; }
        public string? Description { get; set; }
        public string? JsonParams { get; set; }
    }

    /// <summary>
    /// Async audit logger using a background worker with batch inserts and WAL mode.
    /// Usage:
    ///   var logger = new LogHelper&lt;POActions&gt;("data/audit.db");
    ///   await logger.LogAsync("admin", POActions.Delete, "Deleted PO-001", "{}", "PO");
    ///   var logs = logger.Query(limit: 100);
    /// </summary>
    public class LogHelper<TAction> : IAsyncDisposable, IDisposable where TAction : struct, IConvertible
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly BlockingCollection<LogEntry<TAction>> _queue = new BlockingCollection<LogEntry<TAction>>(new ConcurrentQueue<LogEntry<TAction>>());
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _worker;
        private readonly object _disposeLock = new object();
        private bool _disposed;

        private const int BatchSize = 50;
        private const int FlushIntervalMs = 2000;

        public LogHelper(string dbPath)
        {
            if (!typeof(TAction).IsEnum)
                throw new ArgumentException("TAction must be an enum type.");

            _dbPath = dbPath;
            _connectionString = $"Data Source={_dbPath};Version=3;";

            InitDatabase();

            _worker = Task.Factory.StartNew(
                () => ProcessQueue(_cts.Token),
                TaskCreationOptions.LongRunning
            );
        }

        /// <summary>One-time DB initialization: create dirs, file, WAL, indexes.</summary>
        private void InitDatabase()
        {
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();

            // WAL mode once — persists for the database file
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                        TimeISO      TEXT,
                        TimestampMs  INTEGER,
                        User         TEXT,
                        Code         TEXT,
                        Action       TEXT,
                        Description  TEXT,
                        JsonParams   TEXT
                    );
                    
                    CREATE INDEX IF NOT EXISTS IX_Logs_Action      ON Logs(Action);
                    CREATE INDEX IF NOT EXISTS IX_Logs_User       ON Logs(User);
                    CREATE INDEX IF NOT EXISTS IX_Logs_TimestampMs ON Logs(TimestampMs);
                    CREATE INDEX IF NOT EXISTS IX_Logs_Code       ON Logs(Code);
                ";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Queue a log entry for async persistence.
        /// </summary>
        public Task LogAsync(string user, TAction action, string description,
            string jsonParams = "", string code = "SYS")
        {
            var now = DateTime.UtcNow;
            var entry = new LogEntry<TAction>
            {
                TimeISO     = now.ToString("o"),
                TimestampMs = (long)(now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
                User        = user,
                Code        = code,
                Action      = action,
                Description = description ?? "",
                JsonParams  = jsonParams ?? ""
            };

            if (!_queue.TryAdd(entry, 3000, _cts.Token))
            {
                // Queue full or cancelled — write to fallback file synchronously
                FallbackLog(entry);
            }

            return Task.CompletedTask;
        }

        /// <summary>Synchronous overload for fire-and-forget.</summary>
        public void Log(string user, TAction action, string description,
            string jsonParams = "", string code = "SYS")
        {
            LogAsync(user, action, description, jsonParams, code);
        }

        private void ProcessQueue(CancellationToken ct)
        {
            var batch = new List<LogEntry<TAction>>(BatchSize);
            var timer = new System.Timers.Timer(FlushIntervalMs);
            timer.Elapsed += (_, _) => FlushBatch(batch);
            timer.AutoReset = true;
            timer.Start();

            try
            {
                using var conn = new SQLiteConnection(_connectionString);
                conn.Open();

                while (!ct.IsCancellationRequested)
                {
                    if (_queue.TryTake(out var entry, 500, ct))
                    {
                        batch.Add(entry);
                        if (batch.Count >= BatchSize)
                            FlushBatch(batch, conn);
                    }
                    else if (_queue.IsAddingCompleted && _queue.IsCompleted)
                    {
                        break;
                    }
                }

                // Drain remaining on shutdown
                while (_queue.TryTake(out var remaining, Timeout.Infinite, ct))
                    batch.Add(remaining);

                FlushBatch(batch, conn);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                // Flush what we have to fallback
                FlushBatch(batch);
                FallbackLogRaw($"ProcessQueue fatal: {ex}");
            }
            finally
            {
                timer.Stop();
                timer.Dispose();
            }

            // Final checkpoint so WAL doesn't grow
            try
            {
                using var conn = new SQLiteConnection(_connectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private void FlushBatch(List<LogEntry<TAction>> batch, SQLiteConnection? conn = null)
        {
            if (batch.Count == 0) return;

            var toFlush = new List<LogEntry<TAction>>(batch);
            batch.Clear();

            try
            {
                if (conn != null)
                {
                    using var tx = conn.BeginTransaction();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO Logs (TimeISO, TimestampMs, User, Code, Action, Description, JsonParams)
                        VALUES (@TimeISO, @TimestampMs, @User, @Code, @Action, @Description, @JsonParams)";

                    var pTimeISO    = cmd.Parameters.Add("@TimeISO",    System.Data.DbType.String);
                    var pTimestamp  = cmd.Parameters.Add("@TimestampMs", System.Data.DbType.Int64);
                    var pUser       = cmd.Parameters.Add("@User",       System.Data.DbType.String);
                    var pCode       = cmd.Parameters.Add("@Code",       System.Data.DbType.String);
                    var pAction     = cmd.Parameters.Add("@Action",     System.Data.DbType.String);
                    var pDesc       = cmd.Parameters.Add("@Description",System.Data.DbType.String);
                    var pJson       = cmd.Parameters.Add("@JsonParams", System.Data.DbType.String);

                    foreach (var e in toFlush)
                    {
                        pTimeISO.Value    = e.TimeISO;
                        pTimestamp.Value  = e.TimestampMs;
                        pUser.Value       = e.User;
                        pCode.Value       = e.Code;
                        pAction.Value     = e.Action.ToString();
                        pDesc.Value       = e.Description;
                        pJson.Value       = e.JsonParams;
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                // Fallback: write to text file
                foreach (var e in toFlush)
                    FallbackLog(e);
                FallbackLogRaw($"FlushBatch error: {ex}");
            }
        }

        private void FallbackLog(LogEntry<TAction> entry)
        {
            try
            {
                var path = Path.Combine(
                    Path.GetDirectoryName(_dbPath) ?? ".",
                    "audit_fallback.log"
                );
                var line = $"[{DateTime.UtcNow:o}] [FALLBACK] User={entry.User} Code={entry.Code} Action={entry.Action} Desc={entry.Description} Params={entry.JsonParams}";
                File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        private void FallbackLogRaw(string msg)
        {
            try
            {
                var path = Path.Combine(
                    Path.GetDirectoryName(_dbPath) ?? ".",
                    "audit_fallback.log"
                );
                File.AppendAllText(path, $"[{DateTime.UtcNow:o}] {msg}{Environment.NewLine}", Encoding.UTF8);
            }
            catch { }
        }

        /// <summary>Query recent logs (synchronous, for web API responses).</summary>
        public List<LogEntry<TAction>> QueryRecent(int limit = 100, int offset = 0)
        {
            var logs = new List<LogEntry<TAction>>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Logs ORDER BY Id DESC LIMIT @Limit OFFSET @Offset";
            cmd.Parameters.AddWithValue("@Limit", limit);
            cmd.Parameters.AddWithValue("@Offset", offset);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(ReadEntry(reader));
            return logs;
        }

        /// <summary>Query logs by time range.</summary>
        public List<LogEntry<TAction>> QueryByTime(DateTime from, DateTime to)
        {
            var logs = new List<LogEntry<TAction>>();
            var fromMs = (long)(from.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var toMs   = (long)(to.ToUniversalTime()   - new DateTime(1970, 1, 1)).TotalMilliseconds;

            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Logs WHERE TimestampMs BETWEEN @From AND @To ORDER BY Id DESC";
            cmd.Parameters.AddWithValue("@From", fromMs);
            cmd.Parameters.AddWithValue("@To",   toMs);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(ReadEntry(reader));
            return logs;
        }

        /// <summary>Query logs by action enum.</summary>
        public List<LogEntry<TAction>> QueryByAction(TAction action)
        {
            var logs = new List<LogEntry<TAction>>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Logs WHERE Action = @Action ORDER BY Id DESC";
            cmd.Parameters.AddWithValue("@Action", action.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(ReadEntry(reader));
            return logs;
        }

        /// <summary>Query logs by user.</summary>
        public List<LogEntry<TAction>> QueryByUser(string user)
        {
            var logs = new List<LogEntry<TAction>>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Logs WHERE User = @User ORDER BY Id DESC";
            cmd.Parameters.AddWithValue("@User", user);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(ReadEntry(reader));
            return logs;
        }

        /// <summary>Query logs by business code (e.g. "PO", "AUTH").</summary>
        public List<LogEntry<TAction>> QueryByCode(string code)
        {
            var logs = new List<LogEntry<TAction>>();
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Logs WHERE Code = @Code ORDER BY Id DESC";
            cmd.Parameters.AddWithValue("@Code", code);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(ReadEntry(reader));
            return logs;
        }

        /// <summary>Get total log count.</summary>
        public long Count()
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Logs";
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        private LogEntry<TAction> ReadEntry(SQLiteDataReader reader)
        {
            TAction action = default;
            var actionStr = reader["Action"]?.ToString();
            if (!string.IsNullOrEmpty(actionStr))
            {
                try { action = (TAction)Enum.Parse(typeof(TAction), actionStr); }
                catch { }
            }

            return new LogEntry<TAction>
            {
                Id          = Convert.ToInt32(reader["Id"]),
                TimeISO     = reader["TimeISO"]?.ToString(),
                TimestampMs = Convert.ToInt64(reader["TimestampMs"]),
                User        = reader["User"]?.ToString(),
                Code        = reader["Code"]?.ToString(),
                Action      = action,
                Description = reader["Description"]?.ToString(),
                JsonParams  = reader["JsonParams"]?.ToString()
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Run(() => Dispose());
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (_disposed) return;
                _disposed = true;
            }

            if (disposing)
            {
                _queue.CompleteAdding();
                _cts.Cancel();
                try { _worker.Wait(5000); } catch { }
                _cts.Dispose();
                _queue.Dispose();
            }
        }

        ~LogHelper() { Dispose(false); }
    }
}
