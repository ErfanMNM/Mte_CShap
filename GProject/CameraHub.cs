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
        var payload = JsonSerializer.Serialize(new
        {
            camera,
            state = state.ToString(),
            data,
            at = DateTime.UtcNow
        });
        var bytes = Encoding.UTF8.GetBytes(payload);
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

    private static Guid FindKey(WebSocket ws)
    {
        lock (_registerLock)
        {
            return _clients.First(kv => kv.Value == ws).Key;
        }
    }
}