

using System.Data.SQLite;

namespace MTs.Auditrails
{
    /// <summary>
    /// LogHelper: Ghi log vào SQLite (.mtl) để tái sử dụng.
    /// - Trường bắt buộc: User, LogType; thêm Message, Content.
    /// - Thời gian: ISO-8601 (Utc) + EpochTicks (ticks từ 1970-01-01).
    /// </summary>
    public sealed class LogHelper : IDisposable
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private bool _initialized;
        private bool _disposed;

        public string DatabasePath => _dbPath;

        public LogHelper(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty.", nameof(path));

            // Nếu là thư mục hoặc kết thúc bằng dấu slash -> gắn file mặc định
            if (Directory.Exists(path) || path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
            {
                path = Path.Combine(path, "auditrail.mtl");
            }

            // Đảm bảo đuôi .mtl
            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                path += ".mtl";
            else if (!ext.Equals(".mtl", StringComparison.OrdinalIgnoreCase))
                path = Path.ChangeExtension(path, ".mtl");

            // Tạo thư mục nếu chưa có
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _dbPath = Path.GetFullPath(path);

            // System.Data.SQLite builder
            var csb = new SQLiteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Version = 3,
                // FailIfMissing = false, // mặc định tạo file nếu chưa có
                // JournalMode có thể đặt ở đây, nhưng ta bật bằng PRAGMA cho chắc kèo
            };
            _connectionString = csb.ToString();

            Initialize();
        }

        public static LogHelper FromDirectory(string directoryPath, string fileNameWithoutExtension = "auditrail")
        {
            var fileName = (fileNameWithoutExtension ?? "auditrail").Trim();
            if (string.IsNullOrEmpty(fileName)) fileName = "auditrail";
            var fullPath = Path.Combine(directoryPath ?? string.Empty, fileName + ".mtl");
            return new LogHelper(fullPath);
        }

        private void Initialize()
        {
            if (_initialized) return;

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // Bật WAL + FK (tách lệnh cho an toàn)
            using (var pragma1 = new SQLiteCommand("PRAGMA journal_mode=WAL;", connection))
                pragma1.ExecuteNonQuery();
            using (var pragma2 = new SQLiteCommand("PRAGMA foreign_keys=ON;", connection))
                pragma2.ExecuteNonQuery();

            // Tạo bảng + index
            using (var createTable = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS Logs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    User TEXT NOT NULL,
                    LogType TEXT NOT NULL,
                    Message TEXT,
                    Content TEXT,
                    IsoTime TEXT NOT NULL,
                    EpochTicks INTEGER NOT NULL
                );", connection))
            {
                createTable.ExecuteNonQuery();
            }
            using (var createIdx = new SQLiteCommand(
                "CREATE INDEX IF NOT EXISTS IX_Logs_Time ON Logs(EpochTicks);", connection))
            {
                createIdx.ExecuteNonQuery();
            }

            _initialized = true;
        }

        /// <summary>Ghi một dòng log.</summary>
        public void Log(string user, string logType, string message, string content)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LogHelper));

            user = string.IsNullOrWhiteSpace(user) ? "unknown" : user;
            logType = string.IsNullOrWhiteSpace(logType) ? "INFO" : logType;

            var utcNow = DateTime.UtcNow;
            var iso = utcNow.ToString("o");
            var epochTicks = (utcNow - DateTime.UnixEpoch).Ticks;

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var cmd = new SQLiteCommand(@"
                INSERT INTO Logs (User, LogType, Message, Content, IsoTime, EpochTicks)
                VALUES (@user, @type, @message, @content, @iso, @ticks);
            ", connection);

            cmd.Parameters.AddWithValue("@user", user);
            cmd.Parameters.AddWithValue("@type", logType);
            cmd.Parameters.AddWithValue("@message", (object?)message ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@content", (object?)content ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iso", iso);
            cmd.Parameters.AddWithValue("@ticks", epochTicks);

            cmd.ExecuteNonQuery();
        }

        /// <summary>Ghi log và trả về Id tự tăng.</summary>
        public long LogAndReturnId(string user, string logType, string message, string content)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LogHelper));

            user = string.IsNullOrWhiteSpace(user) ? "unknown" : user;
            logType = string.IsNullOrWhiteSpace(logType) ? "INFO" : logType;

            var utcNow = DateTime.UtcNow;
            var iso = utcNow.ToString("o");
            var epochTicks = (utcNow - DateTime.UnixEpoch).Ticks;

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using (var insert = new SQLiteCommand(@"
                INSERT INTO Logs (User, LogType, Message, Content, IsoTime, EpochTicks)
                VALUES (@user, @type, @message, @content, @iso, @ticks);
            ", connection))
            {
                insert.Parameters.AddWithValue("@user", user);
                insert.Parameters.AddWithValue("@type", logType);
                insert.Parameters.AddWithValue("@message", (object?)message ?? DBNull.Value);
                insert.Parameters.AddWithValue("@content", (object?)content ?? DBNull.Value);
                insert.Parameters.AddWithValue("@iso", iso);
                insert.Parameters.AddWithValue("@ticks", epochTicks);
                insert.ExecuteNonQuery();
            }

            // Lấy Id vừa chèn (cùng connection)
            using var getId = new SQLiteCommand("SELECT last_insert_rowid();", connection);
            var result = getId.ExecuteScalar();
            return (result is long id) ? id : Convert.ToInt64(result);
        }

        public void Dispose() => _disposed = true;

        private static void _exampleUsage() { }
    }
}
