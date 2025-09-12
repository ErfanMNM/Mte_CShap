using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTs.Communication
{
    public enum TcpServerState
    {
        Started,
        Stopped,
        ClientConnected,
        ClientDisconnected,
        Received,
        Error
    }

    public sealed class TcpServerHelper
    {
        #region Fields
        private Socket? _listener;
        private readonly ConcurrentDictionary<string, Socket> _clients = new();
        private CancellationTokenSource? _cts;
        #endregion

        #region Properties
        public string IP { get; set; } = "";
        public int Port { get; set; }
        public bool Started { get; private set; } = false;
        #endregion

        #region Events
        public delegate void ServerEventHandler(TcpServerState state, string data);
        public event ServerEventHandler? ServerCallback;
        #endregion

        #region Constructors
        public TcpServerHelper(string ip, int port)
        {
            IP = ip;
            Port = port;
        }
        #endregion

        #region Private Methods
        private void OnServerCallback(TcpServerState state, string data)
        {
            ServerCallback?.Invoke(state, data);
        }

        private static string GetClientKey(Socket socket)
        {
            try
            {
                return socket.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        private void CleanupListener()
        {
            try
            {
                _listener?.Close();
                _listener?.Dispose();
            }
            catch { }
            finally
            {
                _listener = null;
            }
        }

        private void CleanupClients()
        {
            foreach (var kv in _clients)
            {
                try { kv.Value.Close(); kv.Value.Dispose(); } catch { }
            }
            _clients.Clear();
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            if (_listener == null) return;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptAsync(token).ConfigureAwait(false);
                    var key = GetClientKey(client);
                    if (_clients.TryAdd(key, client))
                    {
                        OnServerCallback(TcpServerState.ClientConnected, key);
                        _ = Task.Run(() => ReceiveLoopAsync(key, client, token), token);
                    }
                    else
                    {
                        try { client.Close(); client.Dispose(); } catch { }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnServerCallback(TcpServerState.Error, $"Accept failed: {ex.Message}");
                    await Task.Delay(500, token).ConfigureAwait(false);
                }
            }
        }

        private async Task ReceiveLoopAsync(string key, Socket client, CancellationToken token)
        {
            var buffer = new byte[256];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytes;
                    try
                    {
                        bytes = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    if (bytes <= 0)
                    {
                        break; // disconnected
                    }
                    var message = Encoding.UTF8.GetString(buffer, 0, bytes);
                    if (!string.IsNullOrEmpty(message))
                    {
                        OnServerCallback(TcpServerState.Received, $"{key}|{message}");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                OnServerCallback(TcpServerState.Error, $"Receive error ({key}): {ex.Message}");
            }
            finally
            {
                if (_clients.TryRemove(key, out var s))
                {
                    try { s.Close(); s.Dispose(); } catch { }
                    OnServerCallback(TcpServerState.ClientDisconnected, key);
                }
            }
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            if (Started) return;
            if (Port <= 0)
            {
                OnServerCallback(TcpServerState.Error, "Invalid port");
                return;
            }

            try
            {
                CleanupListener();
                CleanupClients();

                _cts = new CancellationTokenSource();
                var address = string.IsNullOrWhiteSpace(IP) ? IPAddress.Any : IPAddress.Parse(IP);
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 0,
                    SendTimeout = 0
                };
                _listener.Bind(new IPEndPoint(address, Port));
                _listener.Listen(100);

                Started = true;
                OnServerCallback(TcpServerState.Started, $"Listening on {address}:{Port}");
                _ = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                Started = false;
                OnServerCallback(TcpServerState.Error, $"Start failed: {ex.Message}");
                Stop();
            }
        }

        public void Stop()
        {
            if (!Started && _listener == null) return;
            try
            {
                _cts?.Cancel();
            }
            catch { }

            CleanupListener();
            CleanupClients();
            Started = false;
            OnServerCallback(TcpServerState.Stopped, "Server stopped");
        }

        public async Task<bool> BroadcastAsync(string data)
        {
            if (!Started || string.IsNullOrEmpty(data)) return false;
            var payload = Encoding.UTF8.GetBytes(data);
            bool allOk = true;
            foreach (var kv in _clients)
            {
                try
                {
                    int sent = await kv.Value.SendAsync(new ArraySegment<byte>(payload), SocketFlags.None).ConfigureAwait(false);
                    allOk &= (sent == payload.Length);
                }
                catch (Exception ex)
                {
                    allOk = false;
                    OnServerCallback(TcpServerState.Error, $"Send failed ({kv.Key}): {ex.Message}");
                }
            }
            return allOk;
        }

        public async Task<bool> SendToAsync(string clientKey, string data)
        {
            if (!_clients.TryGetValue(clientKey, out var client)) return false;
            if (string.IsNullOrEmpty(data)) return false;
            try
            {
                var payload = Encoding.UTF8.GetBytes(data);
                int sent = await client.SendAsync(new ArraySegment<byte>(payload), SocketFlags.None).ConfigureAwait(false);
                return sent == payload.Length;
            }
            catch (Exception ex)
            {
                OnServerCallback(TcpServerState.Error, $"Send failed ({clientKey}): {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}

