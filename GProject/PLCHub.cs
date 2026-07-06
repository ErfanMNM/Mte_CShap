using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;

namespace GProject;

public class PLCHub
{
    public static readonly PLCHub Instance = new();

    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    private readonly object _registerLock = new();

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

    public async Task BroadcastStateAsync(string state, string? message = null)
    {
        if (_clients.IsEmpty) return;

        var payload = JsonSerializer.Serialize(new
        {
            state,
            message,
            ip = Environment.GetEnvironmentVariable("PLC_IP") ?? "127.0.0.1",
            port = int.TryParse(Environment.GetEnvironmentVariable("PLC_PORT"), out var p) ? p : 9600,
            at = DateTime.UtcNow
        });
        var bytes = Encoding.UTF8.GetBytes(payload);
        var segment = new ArraySegment<byte>(bytes);

        var snapshot = _clients.Values.Where(c => c.State == WebSocketState.Open).ToList();
        foreach (var ws in snapshot)
        {
            try
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[PLCHub] Failed to send PLC state to client");
            }
        }
    }
}
