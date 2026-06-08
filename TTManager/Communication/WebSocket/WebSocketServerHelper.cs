using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
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
        private bool _disposed;
        private Task? _listenerTask;
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
        #endregion

        #region Constructors
        public WebSocketServerHelper()
        {
        }

        public WebSocketServerHelper(int port, string path = "/ws")
        {
            Port = port;
            Path = path;
        }
        #endregion

        #region Private Methods
        private void OnWebSocketServerCallback(WebSocketServerState state, string data)
        {
            WebSocketServerCallback?.Invoke(state, data);
        }

        private string GetPrefix()
        {
            string path = Path.TrimStart('/');
            return $"http://+:{Port}/{path}/";
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
                        _ = Task.Run(() => HandleWebSocketAsync(context, cancellationToken), cancellationToken);
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

            try
            {
                WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                System.Net.WebSockets.WebSocket webSocket = wsContext.WebSocket;

                connection = new ClientConnection(clientId, webSocket);
                _clients.TryAdd(clientId, connection);

                OnWebSocketServerCallback(WebSocketServerState.Connected,
                    $"Client connected: {clientId} | Total: {_clients.Count}");

                byte[] buffer = new byte[4096];

                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
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

                            OnWebSocketServerCallback(WebSocketServerState.Received,
                                $"[{clientId}] {receivedData}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        break;
                    }
                }

                await CloseClientAsync(webSocket, WebSocketCloseStatus.NormalClosure,
                    $"[{clientId}] Client disconnected");
            }
            catch (Exception ex)
            {
                OnWebSocketServerCallback(WebSocketServerState.Error,
                    $"[{clientId}] Handler error: {ex.Message}");
            }
            finally
            {
                if (connection != null)
                {
                    _clients.TryRemove(clientId, out _);
                }
            }
        }

        private async Task CloseClientAsync(System.Net.WebSockets.WebSocket webSocket, WebSocketCloseStatus status, string reason)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(status, reason, CancellationToken.None);
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
                        kvp.Value.Socket.Dispose();
                    }
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

            _listenerCts?.Cancel();
            _serverCts?.Cancel();
            _listenerCts?.Dispose();
            _serverCts?.Dispose();
            _listenerCts = null;
            _serverCts = null;
            _listenerTask = null;
        }
        #endregion

        #region Public Methods
        public void Start()
        {
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
            catch (Exception ex)
            {
                OnWebSocketServerCallback(WebSocketServerState.Error,
                    $"Failed to start server: {ex.Message}");
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

        public void DisconnectClient(Guid clientId)
        {
            if (_clients.TryGetValue(clientId, out ClientConnection? connection))
            {
                try
                {
                    connection.Socket.Dispose();
                }
                catch
                {
                }
                _clients.TryRemove(clientId, out _);

                OnWebSocketServerCallback(WebSocketServerState.Disconnected,
                    $"Client {clientId} disconnected | Active: {_clients.Count}");
            }
        }

        public IReadOnlyCollection<Guid> GetConnectedClients()
        {
            return _clients.Keys.ToList().AsReadOnly();
        }

        public void Dispose()
        {
            if (_disposed) return;

            CleanupServer();
            _disposed = true;
            OnWebSocketServerCallback(WebSocketServerState.Disconnected, "WebSocket server disposed");
        }
        #endregion

        private sealed class ClientConnection
        {
            public Guid Id { get; }
            public System.Net.WebSockets.WebSocket Socket { get; }

            public ClientConnection(Guid id, System.Net.WebSockets.WebSocket socket)
            {
                Id = id;
                Socket = socket;
            }
        }
    }
}
