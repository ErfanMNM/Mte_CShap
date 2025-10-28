using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTs.Auditrails
{
    public class LogEntry<TAction> where TAction : struct, IConvertible
    {
        public int Id { get; set; }
        public string TimeISO { get; set; } = string.Empty;
        public long TimestampMs { get; set; }
        public string User { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public TAction Action { get; set; }
        public string Description { get; set; } = string.Empty;
        public string JsonParams { get; set; } = string.Empty;
    }

    public class LogHelper<TAction> : IDisposable where TAction : struct, IConvertible
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly BlockingCollection<LogEntry<TAction>> _logQueue = new BlockingCollection<LogEntry<TAction>>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _logWorker;

        public LogHelper(string dbPath)
        {
            if (!typeof(TAction).IsEnum)
                throw new ArgumentException("TAction phải là enum");

            _dbPath = dbPath;
            _connectionString = $"Data Source={_dbPath};Version=3;";
            InitDatabase();

            _logWorker = Task.Factory.StartNew(ProcessLogQueue,
                TaskCreationOptions.LongRunning);
        }

        private void InitDatabase()
        { 
            string? directory = Path.GetDirectoryName(_dbPath);

            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var pragmaCmd = conn.CreateCommand())
                {
                    pragmaCmd.CommandText = "PRAGMA journal_mode=WAL;";
                    pragmaCmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            TimeISO TEXT,
                            TimestampMs INTEGER,
                            User TEXT,
                            Code TEXT NOT NULL DEFAULT 0,
                            Action TEXT,
                            Description TEXT,
                            JsonParams TEXT
                        );";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Task WriteLogAsync(string user, TAction action, string description, string jsonParams = "", string Code = "SE001")
        {
            var now = DateTime.UtcNow;
            var timestampMs = (long)(now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var timeISO = now.ToString("o");

            var entry = new LogEntry<TAction>
            {
                TimeISO = timeISO,
                TimestampMs = timestampMs,
                User = user,
                Code = Code,
                Action = action,
                Description = description,
                JsonParams = jsonParams ?? ""
            };

            _logQueue.Add(entry);
            return Task.CompletedTask;
        }

        private void ProcessLogQueue()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                while (!_cts.IsCancellationRequested || !_logQueue.IsCompleted)
                {
                    LogEntry<TAction> ?entry;
                    entry = null;
                    try
                    {
                        if (!_logQueue.TryTake(out entry, Timeout.Infinite, _cts.Token))
                            continue;

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                INSERT INTO Logs (TimeISO, TimestampMs, User, Action, Description, JsonParams)
                                VALUES (@TimeISO, @TimestampMs, @User, @Action, @Description, @JsonParams);";
                            cmd.Parameters.AddWithValue("@TimeISO", entry.TimeISO);
                            cmd.Parameters.AddWithValue("@TimestampMs", entry.TimestampMs);
                            cmd.Parameters.AddWithValue("@User", entry.User);
                            cmd.Parameters.AddWithValue("@Code", entry.Code);
                            cmd.Parameters.AddWithValue("@Action", entry.Action.ToString());
                            cmd.Parameters.AddWithValue("@Description", entry.Description);
                            cmd.Parameters.AddWithValue("@JsonParams", entry.JsonParams ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            string ? path = Path.GetDirectoryName(_dbPath);

                            if (path == null)
                            {
                                path = string.Empty;
                            }
                            var fallbackPath = Path.Combine(path, "fallback_log.txt");
                            var sb = new StringBuilder();
                            sb.AppendLine($"[{DateTime.UtcNow:o}] FAIL TO WRITE LOG");
                            if (entry != null)
                            {
                                sb.AppendLine($"User: {entry.User}");
                                sb.AppendLine($"Action: {entry.Action}");
                                sb.AppendLine($"Description: {entry.Description}");
                                sb.AppendLine($"JsonParams: {entry.JsonParams}");
                            }
                            sb.AppendLine($"Error: {ex}");
                            sb.AppendLine();

                            File.AppendAllText(fallbackPath, sb.ToString(), Encoding.UTF8);
                        }
                        catch
                        {
                            // bỏ qua nếu lỗi fallback
                        }
                    }
                }
            }
        }

        public List<LogEntry<TAction>> GetLogs(int limit = 100, int offset = 0)
        {
            var logs = new List<LogEntry<TAction>>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Logs ORDER BY Id DESC LIMIT @Limit OFFSET @Offset;";
                    cmd.Parameters.AddWithValue("@Limit", limit);
                    cmd.Parameters.AddWithValue("@Offset", offset);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(ReadLogEntry(reader));
                        }
                    }
                }
            }
            return logs;
        }

        public List<LogEntry<TAction>> GetLogsByTime(DateTime from, DateTime to)
        {
            var logs = new List<LogEntry<TAction>>();
            var fromMs = (long)(from.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var toMs = (long)(to.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Logs WHERE TimestampMs BETWEEN @From AND @To ORDER BY Id DESC;";
                    cmd.Parameters.AddWithValue("@From", fromMs);
                    cmd.Parameters.AddWithValue("@To", toMs);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(ReadLogEntry(reader));
                        }
                    }
                }
            }
            return logs;
        }

        public List<LogEntry<TAction>> GetLogsByAction(TAction action)
        {
            var logs = new List<LogEntry<TAction>>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Logs WHERE Action = @Action ORDER BY Id DESC;";
                    cmd.Parameters.AddWithValue("@Action", action.ToString());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(ReadLogEntry(reader));
                        }
                    }
                }
            }
            return logs;
        }

        public List<LogEntry<TAction>> GetLogsByUser(string user)
        {
            var logs = new List<LogEntry<TAction>>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Logs WHERE User = @User ORDER BY Id DESC;";
                    cmd.Parameters.AddWithValue("@User", user);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(ReadLogEntry(reader));
                        }
                    }
                }
            }
            return logs;
        }

        private LogEntry<TAction> ReadLogEntry(SQLiteDataReader reader)
        {
            TAction action = default(TAction);
            try
            {
                action = (TAction)Enum.Parse(typeof(TAction), reader["Action"].ToString());
            }
            catch { /* fallback giữ default */ }

            return new LogEntry<TAction>
            {
                Id = Convert.ToInt32(reader["Id"]),
                TimeISO = reader["TimeISO"].ToString(),
                TimestampMs = Convert.ToInt64(reader["TimestampMs"]),
                User = reader["User"].ToString(),
                Action = action,
                Description = reader["Description"].ToString(),
                JsonParams = reader["JsonParams"].ToString()
            };
        }

        public void Dispose()
        {
            _cts.Cancel();
            _logQueue.CompleteAdding();
            _logWorker.Wait();
        }
    }
}
