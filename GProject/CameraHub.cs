using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Glib.Omron;

namespace GProject;

/// <summary>
/// Trạng thái mã khi camera quét — khớp với enum e_Production_Status của MASAN FDashboard.
/// Dùng để FE render badge "TỐT/TRÙNG/KHÔNG ĐỌC/LỖI" và phân loại record.
/// </summary>
public enum e_Production_Status
{
    Pass,
    Fail,
    Duplicate,
    NotFound,
    Error,
    ReadFail,
    Timeout
}

/// <summary>
/// Kết quả đọc mã từ 1 camera — chứa đầy đủ thông tin để ghi history + broadcast FE.
/// </summary>
public record CameraReadResult(
    string Camera,
    string Code,
    e_Production_Status Status,
    bool PlcSent,
    string? CartonCode,
    int? CartonId,
    DateTime At);

/// <summary>
/// Mục lịch sử camera — ring buffer 200 entry gần nhất.
/// </summary>
public record CameraHistoryEntry(
    long Id,
    DateTime At,
    string Code,
    e_Production_Status Status,
    bool PlcSent,
    string? CartonCode,
    int? CartonId);

public class CameraHub
{
    public static readonly CameraHub Instance = new();

    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    private readonly object _registerLock = new();

    private const int HistoryCapacity = 200;
    private readonly ConcurrentQueue<CameraHistoryEntry> _history = new();
    private long _historySeq;

    private string _lastScannedCode = "";
    private DateTime? _lastScannedAt;
    private DateTime? _lastEventAt;

    /// <summary>Last scanned code from any camera (for REST polling endpoint).</summary>
    public string LastScannedCode => _lastScannedCode;

    /// <summary>Timestamp of last scanned code.</summary>
    public DateTime? LastScannedAt => _lastScannedAt;

    /// <summary>Timestamp of last camera event (any state).</summary>
    public DateTime? LastEventAt => _lastEventAt;

    private CameraHub() { }

    public void Register(WebSocket ws)
    {
        lock (_registerLock)
        {
            _clients[Guid.NewGuid()] = ws;
        }
    }

    public void Unregister(WebSocket ws)
    {
        lock (_registerLock)
        {
            foreach (var kv in _clients.Where(kv => kv.Value == ws).ToList())
            {
                _clients.TryRemove(kv.Key, out _);
            }
        }
    }

    public int ClientCount => _clients.Count;

    /// <summary>
    /// Ghi 1 entry vào ring buffer lịch sử. Tự động trim xuống còn HistoryCapacity entry.
    /// </summary>
    public void RecordHistory(CameraReadResult r)
    {
        _lastScannedCode = r.Code;
        _lastScannedAt = r.At;

        var entry = new CameraHistoryEntry(
            Interlocked.Increment(ref _historySeq),
            r.At, r.Code, r.Status, r.PlcSent, r.CartonCode, r.CartonId);
        _history.Enqueue(entry);
        while (_history.Count > HistoryCapacity && _history.TryDequeue(out _)) { }
    }

    /// <summary>
    /// Lấy tối đa <paramref name="limit"/> entry gần nhất (theo thứ tự thời gian).
    /// </summary>
    public IReadOnlyList<CameraHistoryEntry> GetHistory(int limit)
    {
        var all = _history.ToArray();
        if (limit <= 0 || limit >= all.Length) return all;
        return all[^limit..];
    }

    /// <summary>
    /// Broadcast event kết nối (Connected/Disconnected/Reconnecting) tới FE.
    /// Payload giữ nguyên format cũ để tương thích ngược.
    /// </summary>
    public Task BroadcastAsync(string camera, eOmronCodeReaderState state, string data)
    {
        _lastEventAt = DateTime.UtcNow;

        var payload = JsonSerializer.Serialize(new
        {
            camera,
            state = state.ToString(),
            data,
            at = _lastEventAt
        });
        return SendAsync(payload);
    }

    /// <summary>
    /// Broadcast event "CodeScanned" có kèm status — FE render badge TỐT/TRÙNG/LỖI/...
    /// </summary>
    public Task BroadcastCodeStatus(CameraReadResult r)
    {
        _lastEventAt = r.At;

        var payload = JsonSerializer.Serialize(new
        {
            camera = r.Camera,
            state = "CodeScanned",
            data = r.Code,
            status = r.Status.ToString(),
            plcSent = r.PlcSent,
            cartonCode = r.CartonCode,
            cartonId = r.CartonId,
            at = r.At
        });
        return SendAsync(payload);
    }

    private async Task SendAsync(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        List<WebSocket> clientsToRemove;
        lock (_registerLock)
        {
            clientsToRemove = _clients.Values
                .Where(c => c.State != WebSocketState.Open)
                .ToList();
            foreach (var ws in clientsToRemove)
            {
                _clients.TryRemove(FindKey(ws), out _);
            }
        }

        foreach (var ws in clientsToRemove)
        {
            try { await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); }
            catch { /* ignore — already closed */ }
        }

        if (_clients.IsEmpty) return;

        WebSocket[] openClients;
        lock (_registerLock)
        {
            openClients = _clients.Values
                .Where(c => c.State == WebSocketState.Open)
                .ToArray();
        }

        foreach (var ws in openClients)
        {
            try
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                // Client disconnected mid-send; remove it so the receive loop cleans up.
                lock (_registerLock)
                {
                    _clients.TryRemove(FindKey(ws), out _);
                }
            }
        }
    }

    private Guid FindKey(WebSocket ws)
    {
        lock (_registerLock)
        {
            return _clients.First(kv => kv.Value == ws).Key;
        }
    }
}
