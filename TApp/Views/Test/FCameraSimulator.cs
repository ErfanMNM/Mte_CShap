using Sunny.UI;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using TApp.Utils;
using TApp.Views.Dashboard;
using TApp.Infrastructure;

namespace TApp.Views.Test
{
    /// <summary>
    /// Page giả lập Camera TCP Server để test luồng nhận barcode.
    /// Cho phép gửi (prefix cố định + chuỗi random) hoặc gửi full chuỗi tùy ý.
    /// </summary>
    public partial class FCameraSimulator : UIPage
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentBag<TcpClient> _clients = new ConcurrentBag<TcpClient>();
        private readonly object _stateLock = new object();
        private readonly Random _random = new Random();

        public FCameraSimulator()
        {
            InitializeSimulatorComponent();
            Text = "Giả lập Camera";
            txtBarcode.Text = FD_Globals.productionData?.Barcode ?? string.Empty;
        }

        private void StartServer()
        {
            if (!IsSA())
            {
                this.ShowErrorDialog("Chỉ tài khoản 'SA' mới được dùng trang giả lập camera.");
                return;
            }

            lock (_stateLock)
            {
                if (_listener != null) return;
                if (!int.TryParse(txtPort.Text.Trim(), out int port) || port <= 0 || port > 65535)
                {
                    this.ShowErrorDialog("Port không hợp lệ.");
                    return;
                }

                _cts = new CancellationTokenSource();
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                Task.Run(() => AcceptLoop(_cts.Token));
                Log($"[START] Server lắng nghe tại port {port}");
            }
        }

        private void StopServer()
        {
            lock (_stateLock)
            {
                _cts?.Cancel();
                _listener?.Stop();
                _listener = null;

                while (_clients.TryTake(out var c))
                {
                    try { c.Close(); } catch { }
                }

                Log("[STOP] Dừng server và đóng kết nối.");
            }
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _listener != null)
                {
                    var client = await _listener.AcceptTcpClientAsync(token);
                    _clients.Add(client);
                    Log($"[CONNECT] {client.Client.RemoteEndPoint}");
                    _ = Task.Run(() => ReadLoop(client, token));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log($"[ERROR] AcceptLoop: {ex.Message}");
            }
        }

        private async Task ReadLoop(TcpClient client, CancellationToken token)
        {
            using (client)
            {
                try
                {
                    var stream = client.GetStream();
                    var buffer = new byte[1024];
                    while (!token.IsCancellationRequested && client.Connected)
                    {
                        int read = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (read <= 0) break;
                        string msg = Encoding.ASCII.GetString(buffer, 0, read).Trim();
                        if (!string.IsNullOrEmpty(msg))
                        {
                            Log($"[RECV] {msg}");
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Log($"[ERROR] ReadLoop: {ex.Message}");
                }
                finally
                {
                    Log($"[DISCONNECT] {client.Client.RemoteEndPoint}");
                }
            }
        }

        private void SendPrefixRandom()
        {
            string prefix = (txtPrefix.Text ?? string.Empty) + (txtBarcode.Text ?? string.Empty);
            if (!int.TryParse(txtRandomLen.Text.Trim(), out int len) || len <= 0) len = 8;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var sb = new StringBuilder(prefix);
            for (int i = 0; i < len; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }
            SendToClients(sb.ToString());
        }

        private void SendFull()
        {
            string payload = txtFull.Text ?? string.Empty;
            if (string.IsNullOrEmpty(payload))
            {
                this.ShowErrorDialog("Chuỗi gửi đang trống.");
                return;
            }
            SendToClients(payload);
        }

        private void SendToClients(string payload)
        {
            byte[] data = Encoding.ASCII.GetBytes(payload + "\r\n");
            int sent = 0;
            foreach (var client in _clients.ToArray())
            {
                try
                {
                    if (client.Connected)
                    {
                        client.GetStream().Write(data, 0, data.Length);
                        sent++;
                    }
                }
                catch (Exception ex)
                {
                    Log($"[ERROR] Gửi tới {client.Client.RemoteEndPoint}: {ex.Message}");
                }
            }
            Log($"[SEND] {payload} (đã gửi tới {sent} client)");
        }

        private void Log(string message)
        {
            opLog.InvokeIfRequired(() =>
            {
                opLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
                if (opLog.Items.Count > 300) opLog.Items.RemoveAt(opLog.Items.Count - 1);
            });
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            StopServer();
        }

        private bool IsSA() =>
            string.Equals(GlobalVarialbles.CurrentUser?.Username, "SA", StringComparison.OrdinalIgnoreCase);

        private void InitializeComponent()
        {

        }
    }
}

