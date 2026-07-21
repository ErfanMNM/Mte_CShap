#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TTManager.Communication.WebSocket
{
    public enum WebSocketServerState
    {
        Listening,
        Connected,
        Received,
        Disconnected,
        Error
    }

    public sealed class WebSocketServerHelper : IDisposable
    {
        #region Fields
        private HttpListener? _httpListener;
        private CancellationTokenSource? _listenerCts;
        private CancellationTokenSource? _serverCts;
        private readonly ConcurrentDictionary<Guid, ClientConnection> _clients = new();
        private readonly object _callbackLock = new();
        private readonly SynchronizationContext? _syncContext;
        private bool _disposed;
        private Task? _listenerTask;
        private readonly int _receiveBufferSize;
        private readonly ConcurrentDictionary<string, int> _topicStats = new();
        #endregion

        #region Properties
        public int Port { get; set; } = 8080;
        public string Path { get; set; } = "/ws";
        public bool Listening => _httpListener?.IsListening ?? false;
        public int ClientCount => _clients.Count;
        #endregion

        #region Events
        public delegate void WebSocketServerEventHandler(WebSocketServerState state, string data);
        public event WebSocketServerEventHandler? WebSocketServerCallback;

        public delegate void ClientTopicEventHandler(Guid clientId, string topic);
        public event ClientTopicEventHandler? ClientSubscribed;
        public event ClientTopicEventHandler? ClientUnsubscribed;

        public delegate void WebSocketServerMessageEventHandler(WebSocketServerMessage message);
        public event WebSocketServerMessageEventHandler? WebSocketServerMessageCallback;
        #endregion

        #region Constructors
        public WebSocketServerHelper() : this(4096)
        {
        }

        public WebSocketServerHelper(int receiveBufferSize)
        {
            _receiveBufferSize = receiveBufferSize > 0 ? receiveBufferSize : 4096;
            _syncContext = SynchronizationContext.Current;
        }

        public WebSocketServerHelper(int port, string path = "/ws") : this(port, path, 4096)
        {
        }

        public WebSocketServerHelper(int port, string path, int receiveBufferSize)
        {
            Port = port;
            Path = path;
            _receiveBufferSize = receiveBufferSize > 0 ? receiveBufferSize : 4096;
            _syncContext = SynchronizationContext.Current;
        }
        #endregion

        #region Private Methods
        private void OnWebSocketServerCallback(WebSocketServerState state, string data)
        {
            var handler = WebSocketServerCallback;
            if (handler == null) return;

            var callback = new WebSocketServerEventHandler(handler);
            try
            {
                if (_syncContext != null)
                {
                    lock (_callbackLock)
                    {
                        _syncContext.Post(_ =>
                        {
                            try
                            {
                                callback.Invoke(state, data);
                            }
                            catch
                            {
                            }
                        }, null);
                    }
                }
                else
                {
                    lock (_callbackLock)
                    {
                        callback.Invoke(state, data);
                    }
                }
            }
            catch
            {
            }
        }

        private void RaiseMessage(WebSocketServerState state, Guid clientId, string data)
        {
            var handler = WebSocketServerMessageCallback;
            if (handler == null) return;

            var topics = GetTopics(clientId);
            var message = new WebSocketServerMessage(clientId, state, data, topics);

            var callback = new WebSocketServerMessageEventHandler(handler);
            try
            {
                if (_syncContext != null)
                {
                    _syncContext.Post(_ =>
                    {
                        try { callback.Invoke(message); } catch { }
                    }, null);
                }
                else
                {
                    callback.Invoke(message);
                }
            }
            catch
            {
            }
        }

        private string GetPrefix()
        {
            string path = Path.TrimStart('/');
            return $"http://+:{Port}/{path}/";
        }

        private string GetFallbackPrefix()
        {
            string path = Path.TrimStart('/');
            return $"http://localhost:{Port}/{path}/";
        }

        private static string? ExtractTopicFromSubscribeCommand(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return null;
            string trimmed = data.Trim();

            if (trimmed.StartsWith("SUBSCRIBE ", StringComparison.OrdinalIgnoreCase))
            {
                string topic = trimmed.Substring("SUBSCRIBE ".Length).Trim();
                return string.IsNullOrEmpty(topic) ? null : topic;
            }
            if (trimmed.StartsWith("UNSUBSCRIBE ", StringComparison.OrdinalIgnoreCase))
            {
                string topic = trimmed.Substring("UNSUBSCRIBE ".Length).Trim();
                return string.IsNullOrEmpty(topic) ? null : topic;
            }
            return null;
        }

        private static bool IsSubscribeAction(string data, out string action)
        {
            string trimmed = data.Trim();
            if (trimmed.StartsWith("UNSUBSCRIBE", StringComparison.OrdinalIgnoreCase)) { action = "unsubscribe"; return true; }
            if (trimmed.StartsWith("SUBSCRIBE", StringComparison.OrdinalIgnoreCase)) { action = "subscribe"; return true; }
            action = string.Empty;
            return false;
        }

        private static string? ExtractTopicFromJson(string data)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(data);
                if (!doc.RootElement.TryGetProperty("action", out var actionEl)) return null;
                string action = actionEl.GetString() ?? string.Empty;
                if (!string.Equals(action, "subscribe", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(action, "unsubscribe", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                if (!doc.RootElement.TryGetProperty("topic", out var topicEl)) return null;
                string? topic = topicEl.GetString();
                return string.IsNullOrWhiteSpace(topic) ? null : topic.Trim();
            }
            catch
            {
                return null;
            }
        }

        private bool TryHandleSubscribeMessage(Guid clientId, ClientConnection connection, string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return false;

            string? action = null;
            string? topic = null;

            IsSubscribeAction(data, out string cmdAction);
            string? textTopic = ExtractTopicFromSubscribeCommand(data);
            if (textTopic != null)
            {
                topic = textTopic;
                action = cmdAction;
            }
            else
            {
                string? jsonTopic = ExtractTopicFromJson(data);
                if (jsonTopic != null)
                {
                    topic = jsonTopic;
                    string lower = data.ToLowerInvariant();
                    if (lower.Contains("\"unsubscribe\"")) action = "unsubscribe";
                    else if (lower.Contains("\"subscribe\"")) action = "subscribe";
                }
            }

            if (action == null || topic == null) return false;

            string normalized = topic.Trim().ToLowerInvariant();

            if (string.Equals(action, "subscribe", StringComparison.OrdinalIgnoreCase))
            {
                connection.Topics.Add(normalized);
                _topicStats.AddOrUpdate(normalized, 1, (_, v) => v + 1);
                _ = SendToClientAsync(clientId, $"OK sub {normalized}");
                ClientSubscribed?.Invoke(clientId, normalized);
                return true;
            }

            if (string.Equals(action, "unsubscribe", StringComparison.OrdinalIgnoreCase))
            {
                if (connection.Topics.Remove(normalized))
                {
                    _topicStats.AddOrUpdate(normalized, 0, (_, v) => Math.Max(0, v - 1));
                }
                _ = SendToClientAsync(clientId, $"OK unsub {normalized}");
                ClientUnsubscribed?.Invoke(clientId, normalized);
                return true;
            }

            return false;
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await _httpListener!.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = HandleWebSocketAsync(context, cancellationToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnWebSocketServerCallback(WebSocketServerState.Error, $"Accept error: {ex.Message}");
                }
            }
        }

        private async Task HandleWebSocketAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            Guid clientId = Guid.NewGuid();
            ClientConnection? connection = null;
            bool addedToClients = false;

            try
            {
                WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                System.Net.WebSockets.WebSocket webSocket = wsContext.WebSocket;

                connection = new ClientConnection(clientId, webSocket);
                if (_clients.TryAdd(clientId, connection))
                {
                    addedToClients = true;
                }

                OnWebSocketServerCallback(WebSocketServerState.Connected,
                    $"Client connected: {clientId} | Total: {_clients.Count}");
                RaiseMessage(WebSocketServerState.Connected, clientId,
                    $"Client connected: {clientId} | Total: {_clients.Count}");

                byte[] buffer = new byte[_receiveBufferSize];

                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                                $"[{clientId}] Client initiated close");
                            RaiseMessage(WebSocketServerState.Disconnected, clientId,
                                $"[{clientId}] Client initiated close");
                            break;
                        }

                        if (result.MessageType == WebSocketMessageType.Text || result.MessageType == WebSocketMessageType.Binary)
                        {
                            int byteCount = result.Count;
                            string receivedData;

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                receivedData = Encoding.UTF8.GetString(buffer, 0, byteCount);
                            }
                            else
                            {
                                receivedData = Convert.ToBase64String(buffer, 0, byteCount);
                            }

                            if (result.MessageType == WebSocketMessageType.Text &&
                                TryHandleSubscribeMessage(clientId, connection, receivedData))
                            {
                                continue;
                            }

                            OnWebSocketServerCallback(WebSocketServerState.Received,
                                $"[{clientId}] {receivedData}");
                            RaiseMessage(WebSocketServerState.Received, clientId, receivedData);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                            $"[{clientId}] Connection closed prematurely");
                        RaiseMessage(WebSocketServerState.Disconnected, clientId,
                            $"[{clientId}] Connection closed prematurely");
                        break;
                    }
                    catch (WebSocketException ex)
                    {
                        OnWebSocketServerCallback(WebSocketServerState.Error,
                            $"[{clientId}] WebSocket error: {ex.Message}");
                        RaiseMessage(WebSocketServerState.Error, clientId,
                            $"[{clientId}] WebSocket error: {ex.Message}");
                        break;
                    }
                }

                if (webSocket.State == WebSocketState.Open)
                {
                    await CloseClientGracefullyAsync(webSocket, WebSocketCloseStatus.NormalClosure, "Client disconnected");
                }
            }
            catch (OperationCanceledException)
            {
                OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                    $"[{clientId}] Connection cancelled");
            }
            catch (Exception ex)
            {
                OnWebSocketServerCallback(WebSocketServerState.Error,
                    $"[{clientId}] Handler error: {ex.Message}");
                RaiseMessage(WebSocketServerState.Error, clientId,
                    $"[{clientId}] Handler error: {ex.Message}");
            }
            finally
            {
                if (addedToClients && connection != null)
                {
                    _clients.TryRemove(clientId, out _);
                    OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                        $"[{clientId}] Client removed | Active: {_clients.Count}");
                    RaiseMessage(WebSocketServerState.Disconnected, clientId,
                        $"[{clientId}] Client removed | Active: {_clients.Count}");
                }
            }
        }

        private async Task CloseClientGracefullyAsync(System.Net.WebSockets.WebSocket webSocket, WebSocketCloseStatus status, string reason)
        {
            try
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await webSocket.CloseAsync(status, reason, cts.Token);
                }
            }
            catch
            {
            }
            finally
            {
                try
                {
                    if (webSocket.State != WebSocketState.Closed)
                    {
                        webSocket.Dispose();
                    }
                }
                catch
                {
                }
            }
        }

        private void CleanupServer()
        {
            foreach (var kvp in _clients)
            {
                try
                {
                    if (kvp.Value.Socket.State == WebSocketState.Open)
                    {
                        try
                        {
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                            kvp.Value.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutdown", cts.Token).Wait(cts.Token);
                        }
                        catch
                        {
                        }
                    }
                    kvp.Value.Socket.Dispose();
                }
                catch
                {
                }
            }

            _clients.Clear();

            if (_httpListener != null)
            {
                try
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }
                catch
                {
                }
                _httpListener = null;
            }

            try
            {
                _listenerCts?.Cancel();
                _serverCts?.Cancel();
                _listenerCts?.Dispose();
                _serverCts?.Dispose();
            }
            catch
            {
            }

            _listenerCts = null;
            _serverCts = null;
            _listenerTask = null;
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WebSocketServerHelper));
            }

            if (Listening)
            {
                Stop();
            }

            try
            {
                _listenerCts = new CancellationTokenSource();
                _serverCts = new CancellationTokenSource();

                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(GetPrefix());
                _httpListener.Start();

                CancellationToken linkedToken = CancellationTokenSource
                    .CreateLinkedTokenSource(_listenerCts.Token, _serverCts.Token).Token;

                _listenerTask = Task.Run(() => AcceptClientsAsync(linkedToken), linkedToken);

                OnWebSocketServerCallback(WebSocketServerState.Listening,
                    $"Server started on port {Port}{Path}");
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5 || ex.ErrorCode == 1231)
            {
                // Access denied - try fallback to localhost
                try
                {
                    _httpListener = new HttpListener();
                    _httpListener.Prefixes.Add(GetFallbackPrefix());
                    _httpListener.Start();

                    _listenerCts ??= new CancellationTokenSource();
                    _serverCts ??= new CancellationTokenSource();

                    CancellationToken linkedToken = CancellationTokenSource
                        .CreateLinkedTokenSource(_listenerCts.Token, _serverCts.Token).Token;

                    _listenerTask = Task.Run(() => AcceptClientsAsync(linkedToken), linkedToken);

                    OnWebSocketServerCallback(WebSocketServerState.Listening,
                        $"Server started on localhost:{Port}{Path} (fallback - run 'netsh http add urlacl' for full access)");
                }
                catch (Exception fallbackEx)
                {
                    OnWebSocketServerCallback(WebSocketServerState.Error,
                        $"Failed to start server: {fallbackEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                OnWebSocketServerCallback(WebSocketServerState.Error,
                    $"Failed to start server: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            CleanupServer();
            OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                "Server stopped");
        }

        public async Task<bool> SendToClientAsync(Guid clientId, string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            if (!_clients.TryGetValue(clientId, out ClientConnection? connection))
            {
                OnWebSocketServerCallback(WebSocketServerState.Error,
                    $"Client {clientId} not found");
                return false;
            }

            if (connection.Socket.State != WebSocketState.Open)
            {
                _clients.TryRemove(clientId, out _);
                OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                    $"Client {clientId} is not connected");
                return false;
            }

            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                await connection.Socket.SendAsync(
                    new ArraySegment<byte>(dataBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                OnWebSocketServerCallback(WebSocketServerState.Error,
                    $"Send to {clientId} failed: {ex.Message}");
                _clients.TryRemove(clientId, out _);
                return false;
            }
        }

        public async Task BroadcastAsync(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            if (_disposed)
            {
                return;
            }

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            var deadClients = new List<Guid>();

            foreach (var kvp in _clients)
            {
                if (kvp.Value.Socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await kvp.Value.Socket.SendAsync(
                            new ArraySegment<byte>(dataBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                        deadClients.Add(kvp.Key);
                    }
                }
                else
                {
                    deadClients.Add(kvp.Key);
                }
            }

            foreach (Guid id in deadClients)
            {
                _clients.TryRemove(id, out _);
            }

            if (deadClients.Count > 0)
            {
                OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                    $"Removed {deadClients.Count} dead client(s) | Active: {_clients.Count}");
            }
        }

        public async Task BroadcastAsync(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            if (_disposed)
            {
                return;
            }

            var deadClients = new List<Guid>();

            foreach (var kvp in _clients)
            {
                if (kvp.Value.Socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await kvp.Value.Socket.SendAsync(
                            new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    catch
                    {
                        deadClients.Add(kvp.Key);
                    }
                }
                else
                {
                    deadClients.Add(kvp.Key);
                }
            }

            foreach (Guid id in deadClients)
            {
                _clients.TryRemove(id, out _);
            }
        }

        public async Task DisconnectClientAsync(Guid clientId)
        {
            if (_clients.TryGetValue(clientId, out ClientConnection? connection))
            {
                await CloseClientGracefullyAsync(connection.Socket, WebSocketCloseStatus.NormalClosure, "Disconnected by server");
                _clients.TryRemove(clientId, out _);

                OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                    $"Client {clientId} disconnected | Active: {_clients.Count}");
            }
        }

        public void DisconnectClient(Guid clientId)
        {
            if (_clients.TryGetValue(clientId, out ClientConnection? connection))
            {
                _ = Task.Run(() => CloseClientGracefullyAsync(connection.Socket, WebSocketCloseStatus.NormalClosure, "Disconnected by server"));
                _clients.TryRemove(clientId, out _);

                OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                    $"Client {clientId} disconnected | Active: {_clients.Count}");
            }
        }

        public IReadOnlyCollection<Guid> GetConnectedClients()
        {
            return _clients.Keys.ToArray();
        }

        public async Task SubscribeAsync(Guid clientId, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return;
            if (!_clients.TryGetValue(clientId, out ClientConnection? connection)) return;
            string normalized = topic.Trim().ToLowerInvariant();
            if (connection.Topics.Add(normalized))
            {
                _topicStats.AddOrUpdate(normalized, 1, (_, v) => v + 1);
            }
            await SendToClientAsync(clientId, $"OK sub {normalized}");
            ClientSubscribed?.Invoke(clientId, normalized);
        }

        public async Task UnsubscribeAsync(Guid clientId, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) return;
            if (!_clients.TryGetValue(clientId, out ClientConnection? connection)) return;
            string normalized = topic.Trim().ToLowerInvariant();
            if (connection.Topics.Remove(normalized))
            {
                _topicStats.AddOrUpdate(normalized, 0, (_, v) => Math.Max(0, v - 1));
            }
            await SendToClientAsync(clientId, $"OK unsub {normalized}");
            ClientUnsubscribed?.Invoke(clientId, normalized);
        }

        public IReadOnlyCollection<string> GetTopics(Guid clientId)
        {
            if (_clients.TryGetValue(clientId, out ClientConnection? connection))
            {
                return connection.Topics.ToArray();
            }
            return Array.Empty<string>();
        }

        public IReadOnlyDictionary<string, int> GetTopicStats()
        {
            return _topicStats
                .Where(kvp => kvp.Value > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<int> SendToTopicAsync(string topic, string data)
        {
            if (string.IsNullOrEmpty(data)) return 0;
            if (_disposed) return 0;

            string normalized = topic.Trim().ToLowerInvariant();
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            var deadClients = new List<Guid>();
            int sent = 0;

            foreach (var kvp in _clients)
            {
                if (!kvp.Value.Topics.Contains(normalized)) continue;
                if (kvp.Value.Socket.State != WebSocketState.Open)
                {
                    deadClients.Add(kvp.Key);
                    continue;
                }
                try
                {
                    await kvp.Value.Socket.SendAsync(
                        new ArraySegment<byte>(dataBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    sent++;
                }
                catch
                {
                    deadClients.Add(kvp.Key);
                }
            }

            foreach (Guid id in deadClients)
            {
                _clients.TryRemove(id, out _);
            }

            return sent;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            CleanupServer();

            lock (_callbackLock)
            {
                WebSocketServerCallback = null;
            }

            OnWebSocketServerCallback(WebSocketServerState.Disconnected, "WebSocket server disposed");
        }
        #endregion

        private sealed class ClientConnection
        {
            public Guid Id { get; }
            public System.Net.WebSockets.WebSocket Socket { get; }
            public HashSet<string> Topics { get; } = new(StringComparer.Ordinal);

            public ClientConnection(Guid id, System.Net.WebSockets.WebSocket socket)
            {
                Id = id;
                Socket = socket;
            }
        }
    }
}
