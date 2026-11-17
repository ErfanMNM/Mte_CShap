using TTManager.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TTManager.Datalogic
{
    public enum eDatalogicCameraState
    {
        Connected,
        Disconnected,
        Received,
        Reconnecting
    }
    public sealed class DatalogicCamera
    {
        #region Fields
        private Socket? _client;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private long _connectionVersion;
        #endregion

        #region Properties
        public string ? IP { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool Connected { get; private set; } = false;
        public long ConnectionVersion => Interlocked.Read(ref _connectionVersion);
        #endregion

        #region Events
        public delegate void ClientEventHandler(eDatalogicCameraState state, string data);
        public event ClientEventHandler? ClientCallback;
        #endregion

        #region Constructors
        public DatalogicCamera(string ip, int port)
        {
            IP = ip;
            Port = port;
        }
        #endregion

        #region Private Methods
        private void StartReconnection()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!Connected)
                    {
                        OnClientCallback(eDatalogicCameraState.Reconnecting, "Attempting to reconnect...");
                        await ConnectInternalAsync();
                    }
                    await Task.Delay(3000, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
        }

        private bool PingHost(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = ping.Send(ip, 5000); // 5 second timeout
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task ConnectInternalAsync()
        {
            try
            {
                if (!PingHost(IP))
                {
                    return;
                }

                CleanupSocket();

                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 30000,
                    SendTimeout = 30000
                };

                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

                await _client.ConnectAsync(endPoint);

                if (_client.Connected)
                {
                    // Công bố phiên mới trước khi cho phép gửi dữ liệu.
                    Interlocked.Increment(ref _connectionVersion);
                    Connected = true;
                    OnClientCallback(eDatalogicCameraState.Connected, "Connected successfully");
                    _cancellationTokenSource?.Cancel(); // Stop reconnection attempts
                    _ = Task.Run(WaitForData); // Start listening for data
                }
            }
            catch (Exception ex)
            {
                HandleDisconnection($"Connection failed: {ex.Message}");
            }
        }

        private async Task WaitForData()
        {
            try
            {
                if (_client == null) return;
                var buffer = new byte[256];
                while (Connected && _client != null && _client.Connected)
                {
                    int bytesReceived = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (bytesReceived == 0)
                    {
                        HandleDisconnection("Connection closed by remote host.");
                        return;
                    }

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    if (!string.IsNullOrEmpty(receivedData))
                    {
                        OnClientCallback(eDatalogicCameraState.Received, receivedData);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleDisconnection($"Error receiving data: {ex.Message}");
            }
        }

        private void HandleDisconnection(string reason)
        {
            if (!Connected) return; // Avoid redundant actions

            Connected = false;
            OnClientCallback(eDatalogicCameraState.Disconnected, reason);
            CleanupSocket();
            StartReconnection();
        }

        private void CleanupSocket()
        {
            _client?.Close();
            _client?.Dispose();
            _client = null;
        }

        private void OnClientCallback(eDatalogicCameraState state, string data)
        {
            ClientCallback?.Invoke(state, data);
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
            if (string.IsNullOrEmpty(IP) || Port <= 0)
            {
                OnClientCallback(eDatalogicCameraState.Disconnected, "Invalid IP address or port");
                return;
            }

            if (!Connected)
            {
                StartReconnection();
            }
        }

        public void Disconnect()
        {
            _cancellationTokenSource?.Cancel();
            HandleDisconnection("Disconnected by user.");
        }

        public async Task<bool> SendAsync(string data, long? expectedConnectionVersion = null)
        {
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            await _sendLock.WaitAsync().ConfigureAwait(false);
            try
            {
                Socket? client = _client;
                if (client == null || !Connected) return false;
                // Kiểm tra sau khi chờ lượt gửi để không phát lệnh cũ qua phiên mới.
                if (expectedConnectionVersion.HasValue && expectedConnectionVersion.Value != ConnectionVersion)
                    return false;

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                int totalSent = 0;
                while (totalSent < dataBytes.Length)
                {
                    // Một lần SendAsync có thể chỉ gửi được một phần dữ liệu.
                    int bytesSent = await client.SendAsync(
                        new ArraySegment<byte>(dataBytes, totalSent, dataBytes.Length - totalSent),
                        SocketFlags.None).ConfigureAwait(false);
                    if (bytesSent == 0)
                    {
                        HandleDisconnection("Connection closed while sending data.");
                        return false;
                    }
                    totalSent += bytesSent;
                }
                return true;
            }
            catch (Exception ex)
            {
                HandleDisconnection($"Send failed: {ex.Message}");
                return false;
            }
            finally
            {
                _sendLock.Release();
            }
        }
        #endregion
    }
}
