using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;

namespace GProject;

/// <summary>
/// Singleton WebSocket hub cho production state broadcast.
/// Giống pattern với PLCHub và CameraHub.
/// Frontend subscribe để nhận real-time state updates.
/// </summary>
public class ProductionHub
{
    public static readonly ProductionHub Instance = new();

    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    private readonly object _registerLock = new();

    private ProductionHub() { }

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

    public async Task BroadcastStateAsync(
        string state,
        string? previousState,
        string? orderNo,
        string? productName,
        int orderQty,
        object activeCounter,
        string? lastWarning,
        bool isAppReady,
        bool isDeviceReady,
        int codesCount,
        int cartonsCount)
    {
        var payload = JsonSerializer.Serialize(new
        {
            state,
            previousState,
            orderNo,
            productName,
            orderQty,
            activeCounter,
            lastWarning,
            isAppReady,
            isDeviceReady,
            codesCount,
            cartonsCount,
            at = DateTime.UtcNow
        });
        var bytes = Encoding.UTF8.GetBytes(payload);
        var segment = new ArraySegment<byte>(bytes);

        // Remove stale connections
        List<WebSocket> stale;
        lock (_registerLock)
        {
            stale = _clients.Values.Where(c => c.State != WebSocketState.Open).ToList();
            foreach (var ws in stale)
            {
                var key = _clients.FirstOrDefault(kv => kv.Value == ws).Key;
                _clients.TryRemove(key, out _);
            }
        }

        foreach (var ws in stale)
        {
            try { await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); }
            catch { }
        }

        if (_clients.IsEmpty) return;

        WebSocket[] openClients;
        lock (_registerLock)
        {
            openClients = _clients.Values.Where(c => c.State == WebSocketState.Open).ToArray();
        }

        foreach (var ws in openClients)
        {
            try
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[ProductionHub] Failed to send — removing stale client");
                lock (_registerLock)
                {
                    var key = _clients.FirstOrDefault(kv => kv.Value == ws).Key;
                    _clients.TryRemove(key, out _);
                }
            }
        }
    }
}
