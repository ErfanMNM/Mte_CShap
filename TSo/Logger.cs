using System.Collections.Concurrent;
using System.Text;
using TSo.Models;

namespace TSo;

public enum LogType
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Debug = 3,
    System = 4,
    UserAction = 5,
    DeviceAction = 6,
    Maintenance = 7,
    DataChange = 8,
    Critical = 9
}

public class Logger
{
    private readonly string _logFilePath;
    private readonly object _lock = new();
    private readonly BlockingCollection<LogEntry> _queue = new(10000);

    private record LogEntry(DateTime Timestamp, string Username, LogType Type, string Message, string? Detail, string? Code);

    public Logger(string logPath)
    {
        _logFilePath = logPath;
        var dir = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        Task.Run(ProcessQueue);
    }

    public void Log(string username, LogType type, string message, string? detail = null, string? code = null)
    {
        var entry = new LogEntry(DateTime.UtcNow, username, type, message, detail, code);
        if (!_queue.TryAdd(entry, 100))
        {
            FallbackWrite(entry);
        }

        var log = new ActivityLog
        {
            Username = username,
            LogType = type.ToString(),
            Message = message,
            Detail = detail,
            Code = code,
            Timestamp = entry.Timestamp
        };
        GlobalState.ActivityLogs.Enqueue(log);

        while (GlobalState.ActivityLogs.Count > 1000)
            GlobalState.ActivityLogs.TryDequeue(out _);
    }

    private void ProcessQueue()
    {
        foreach (var entry in _queue.GetConsumingEnumerable())
        {
            Write(entry);
        }
    }

    private void Write(LogEntry entry)
    {
        lock (_lock)
        {
            try
            {
                var line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{entry.Type}][{entry.Username}] [{entry.Code ?? "---"}] {entry.Message}";
                if (!string.IsNullOrEmpty(entry.Detail))
                    line += $" | {entry.Detail}";
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch
            {
                // Never crash on logging failure
            }
        }
    }

    private void FallbackWrite(LogEntry entry)
    {
        Write(entry);
    }

    public List<ActivityLog> GetRecentLogs(int count = 100)
    {
        return GlobalState.ActivityLogs.TakeLast(count).Reverse().ToList();
    }
}
