using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace MTs.Auditrails
{
    /// <summary>
    /// LogHelper: Ghi log vào SQLite (.mtl) để dễ tái sử dụng.
    /// - Lưu tại vị trí tùy chọn khi khởi tạo.
    /// - Trường bắt buộc: User, LogType; thêm Message, Content.
    /// - Thời gian: ISO-8601 (Utc) và EpochTicks (ticks từ 1970-01-01).
    /// </summary>
    public sealed class LogHelper : IDisposable
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private bool _initialized;
        private bool _disposed;

        /// <summary>
        /// Đường dẫn file CSDL SQLite (.mtl).
        /// </summary>
        public string DatabasePath => _dbPath;

        /// <summary>
        /// Khởi tạo LogHelper với đường dẫn file hoặc thư mục lưu.
        /// - Nếu truyền thư mục, file mặc định: auditrail.mtl
        /// - Đảm bảo đuôi .mtl
        /// </summary>
        public LogHelper(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty.", nameof(path));

            // Nếu là thư mục hoặc có dấu kết thúc thư mục -> gắn tên file mặc định
            if (Directory.Exists(path) || path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
            {
                path = Path.Combine(path, "auditrail.mtl");
            }

            // Đảm bảo đuôi .mtl
            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                path = path + ".mtl";
            }
            else if (!string.Equals(ext, ".mtl", StringComparison.OrdinalIgnoreCase))
            {
                path = Path.ChangeExtension(path, ".mtl");
            }

            // Tạo thư mục nếu chưa có
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _dbPath = Path.GetFullPath(path);

            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Default
            }.ToString();

            Initialize();
        }

        /// <summary>
        /// Tạo LogHelper từ thư mục và tên file (không kèm đuôi).
        /// </summary>
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

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Cải thiện độ bền và hỗ trợ ghi song song cơ bản
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
                pragma.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
CREATE TABLE IF NOT EXISTS Logs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    User TEXT NOT NULL,
    LogType TEXT NOT NULL,
    Message TEXT,
    Content TEXT,
    IsoTime TEXT NOT NULL,
    EpochTicks INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_Logs_Time ON Logs(EpochTicks);
";
                command.ExecuteNonQuery();
            }

            _initialized = true;
        }

        /// <summary>
        /// Ghi một dòng log.
        /// </summary>
        /// <param name="user">Người dùng (bắt buộc, mặc định "unknown").</param>
        /// <param name="logType">Loại log (bắt buộc, mặc định "INFO").</param>
        /// <param name="message">Tiêu đề / thông điệp ngắn.</param>
        /// <param name="content">Nội dung chi tiết.</param>
        public void Log(string user, string logType, string message, string content)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LogHelper));

            user = string.IsNullOrWhiteSpace(user) ? "unknown" : user;
            logType = string.IsNullOrWhiteSpace(logType) ? "INFO" : logType;

            var utcNow = DateTime.UtcNow;
            var iso = utcNow.ToString("o"); // ISO 8601 (UTC)
            // Ticks đếm từ mốc 1970-01-01 (Unix epoch) theo đơn vị tick .NET (100ns)
            var epochTicks = (utcNow - DateTime.UnixEpoch).Ticks;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Logs (User, LogType, Message, Content, IsoTime, EpochTicks)
VALUES ($user, $type, $message, $content, $iso, $ticks);
";

            cmd.Parameters.AddWithValue("$user", user);
            cmd.Parameters.AddWithValue("$type", logType);
            cmd.Parameters.AddWithValue("$message", (object?)message ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$content", (object?)content ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$iso", iso);
            cmd.Parameters.AddWithValue("$ticks", epochTicks);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Ghi log và trả về Id tự tăng vừa tạo.
        /// </summary>
        public long LogAndReturnId(string user, string logType, string message, string content)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LogHelper));

            user = string.IsNullOrWhiteSpace(user) ? "unknown" : user;
            logType = string.IsNullOrWhiteSpace(logType) ? "INFO" : logType;

            var utcNow = DateTime.UtcNow;
            var iso = utcNow.ToString("o");
            var epochTicks = (utcNow - DateTime.UnixEpoch).Ticks;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Logs (User, LogType, Message, Content, IsoTime, EpochTicks)
VALUES ($user, $type, $message, $content, $iso, $ticks);
SELECT last_insert_rowid();
";

            cmd.Parameters.AddWithValue("$user", user);
            cmd.Parameters.AddWithValue("$type", logType);
            cmd.Parameters.AddWithValue("$message", (object?)message ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$content", (object?)content ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$iso", iso);
            cmd.Parameters.AddWithValue("$ticks", epochTicks);

            var result = cmd.ExecuteScalar();
            return (result is long id) ? id : Convert.ToInt64(result);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        /// <summary>
        /// Ví dụ sử dụng:
        /// var logger = new MTs.Auditrails.LogHelper(@"C:\\Logs\\myapp.mtl");
        /// logger.Log("thuc", "ERROR", "Lỗi khi xử lý", "StackTrace/Chi tiết...");
        /// 
        /// Hoặc:
        /// var logger = MTs.Auditrails.LogHelper.FromDirectory(@"C:\\Logs", "audit");
        /// </summary>
        private static void _exampleUsage() { }
    }
}

