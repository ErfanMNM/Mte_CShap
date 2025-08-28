using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTs.Communication
{
    public enum TcpClientState
    {
        Connected,
        Disconnected,
        Received,
        Reconnecting
    }

    public partial class TcpClient : Component
    {
        #region Fields
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private bool _isStartup = true;
        private Socket? _client;
        private IAsyncResult? _asyncResult;
        private AsyncCallback? _callbackFunction;
        private readonly byte[] _dataBuffer = new byte[256];
        #endregion

        #region Properties
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool Connected { get; private set; } = false;
        #endregion

        #region Events
        public delegate void ClientEventHandler(TcpClientState state, string data);
        public event ClientEventHandler? ClientCallback;
        #endregion

        #region Constructors
        public TcpClient()
        {
            InitializeComponent();
            InitializeWorker();
        }

        public TcpClient(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            InitializeWorker();
        }
        #endregion

        #region Private Methods
        private void InitializeWorker()
        {
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += Worker_DoWork;
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (!_worker.CancellationPending)
            {
                if (!Connected)
                {
                    OnClientCallback(TcpClientState.Reconnecting, "Attempting to reconnect...");
                    Thread.Sleep(3000);
                    ConnectInternal();
                }
                else
                {
                    Thread.Sleep(1000); // Check connection status periodically
                }
            }
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

        private void ConnectInternal()
        {
            try
            {
                // First ping the host to check if it's reachable
                if (!PingHost(IP))
                {
                    return;
                }

                // Clean up existing connection
                CleanupSocket();

                // Create new socket and connect
                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _client.ReceiveTimeout = 30000; // 30 second timeout
                _client.SendTimeout = 30000;

                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

                _client.Connect(endPoint);

                if (_client.Connected)
                {
                    Connected = true;
                    OnClientCallback(TcpClientState.Connected, "Connected successfully");
                    WaitForData();
                }
            }
            catch (Exception ex)
            {
                Connected = false;
                OnClientCallback(TcpClientState.Disconnected, $"Connection failed: {ex.Message}");
                CleanupSocket();
            }
        }

        private void WaitForData()
        {
            try
            {
                if (_client == null || !_client.Connected)
                    return;

                if (_callbackFunction == null)
                {
                    _callbackFunction = new AsyncCallback(OnDataReceived);
                }

                var socketPacket = new SocketPacket { Socket = _client };
                _asyncResult = _client.BeginReceive(
                    socketPacket.DataBuffer,
                    0,
                    socketPacket.DataBuffer.Length,
                    SocketFlags.None,
                    _callbackFunction,
                    socketPacket);
            }
            catch (Exception ex)
            {
                OnClientCallback(TcpClientState.Disconnected, $"Error waiting for data: {ex.Message}");
                HandleDisconnection();
            }
        }

        private void OnDataReceived(IAsyncResult asyncResult)
        {
            try
            {
                var socketPacket = asyncResult.AsyncState as SocketPacket;
                if (socketPacket?.Socket == null)
                    return;

                int bytesReceived = socketPacket.Socket.EndReceive(asyncResult);

                if (bytesReceived == 0)
                {
                    // Connection closed by remote host
                    HandleDisconnection();
                    return;
                }

                // Convert received data to string
                string receivedData = Encoding.UTF8.GetString(socketPacket.DataBuffer, 0, bytesReceived);

                if (!string.IsNullOrEmpty(receivedData))
                {
                    OnClientCallback(TcpClientState.Received, receivedData);
                }

                // Continue listening for more data
                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                // Socket was disposed, ignore
            }
            catch (Exception ex)
            {
                OnClientCallback(TcpClientState.Disconnected, $"Error receiving data: {ex.Message}");
                HandleDisconnection();
            }
        }

        private void HandleDisconnection()
        {
            Connected = false;
            CleanupSocket();

            // Restart worker if it's not already running
            if (!_worker.IsBusy && !_worker.CancellationPending)
            {
                _worker.RunWorkerAsync();
            }
        }

        private void CleanupSocket()
        {
            try
            {
                _client?.Close();
                _client?.Dispose();
            }
            catch { }
            finally
            {
                _client = null;
            }
        }

        private void OnClientCallback(TcpClientState state, string data)
        {
            ClientCallback?.Invoke(state, data);
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
            if (string.IsNullOrEmpty(IP) || Port <= 0)
            {
                OnClientCallback(TcpClientState.Disconnected, "Invalid IP address or port");
                return;
            }

            if (_isStartup)
            {
                _isStartup = false;
                _worker.RunWorkerAsync();
            }
            else if (!Connected)
            {
                ConnectInternal();
            }
        }

        public void Disconnect()
        {
            try
            {
                Connected = false;

                // Cancel background worker
                if (_worker.IsBusy)
                {
                    _worker.CancelAsync();
                }

                CleanupSocket();
                OnClientCallback(TcpClientState.Disconnected, "Disconnected");
            }
            catch (Exception ex)
            {
                OnClientCallback(TcpClientState.Disconnected, $"Error during disconnect: {ex.Message}");
            }
        }

        public bool Send(string data)
        {
            if (string.IsNullOrEmpty(data) || _client == null || !Connected)
            {
                return false;
            }

            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                int bytesSent = _client.Send(dataBytes);
                return bytesSent == dataBytes.Length;
            }
            catch (SocketException ex)
            {
                OnClientCallback(TcpClientState.Disconnected, $"Send failed: {ex.Message}");
                HandleDisconnection();
                return false;
            }
            catch (Exception ex)
            {
                OnClientCallback(TcpClientState.Disconnected, $"Send error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAsync(string data)
        {
            if (string.IsNullOrEmpty(data) || _client == null || !Connected)
            {
                return false;
            }

            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                await Task.Run(() => _client.Send(dataBytes));
                return true;
            }
            catch (Exception ex)
            {
                OnClientCallback(TcpClientState.Disconnected, $"Async send error: {ex.Message}");
                HandleDisconnection();
                return false;
            }
        }
        private class SocketPacket
        {
            public Socket? Socket { get; set; }
            public byte[] DataBuffer { get; } = new byte[256];
        }
        #endregion
    }
}