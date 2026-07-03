using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Glib.Omron;

namespace GProject;

public class CameraHub
{
    public static readonly CameraHub Instance = new();

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

    public async Task BroadcastAsync(string camera, eOmronCodeReaderState state, string data)
    {
        if (_clients.IsEmpty) return;

        var payload = JsonSerializer.Serialize(new
        {
            camera,
            state = state.ToString(),
            data,
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
            catch
            {
                // Client disconnected mid-send; will be cleaned up on receive loop exit.
            }
        }
    }
}