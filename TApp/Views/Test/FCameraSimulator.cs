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
    public class FCameraSimulator : UIPage
    {
        private readonly UITextBox txtPort = new UITextBox();
        private readonly UITextBox txtPrefix = new UITextBox();
        private readonly UITextBox txtRandomLen = new UITextBox();
        private readonly UITextBox txtBarcode = new UITextBox();
        private readonly UITextBox txtFull = new UITextBox();
        private readonly UIListBox opLog = new UIListBox();
        private readonly UISymbolButton btnStart = new UISymbolButton();
        private readonly UISymbolButton btnStop = new UISymbolButton();
        private readonly UISymbolButton btnSendRandom = new UISymbolButton();
        private readonly UISymbolButton btnSendFull = new UISymbolButton();

        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentBag<TcpClient> _clients = new ConcurrentBag<TcpClient>();
        private readonly object _stateLock = new object();
        private readonly Random _random = new Random();

        public FCameraSimulator()
        {
            Text = "Giả lập Camera";
            InitializeUI();
            txtBarcode.Text = FD_Globals.productionData?.Barcode ?? string.Empty;
        }

        private void InitializeUI()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(8),
                AutoSize = true,
            };

            // Row 0: Server control
            var rowServer = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 4,
                RowCount = 1,
                Height = 40,
            };
            rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            var lblPort = new UILabel { Text = "Port", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            txtPort.Text = "51236";
            txtPort.Dock = DockStyle.Fill;
            btnStart.Text = "Start";
            btnStart.Symbol = 61515; // play
            btnStart.Dock = DockStyle.Fill;
            btnStart.Click += (s, e) => StartServer();

            btnStop.Text = "Stop";
            btnStop.Symbol = 61516; // stop
            btnStop.Dock = DockStyle.Fill;
            btnStop.Click += (s, e) => StopServer();

            rowServer.Controls.Add(lblPort, 0, 0);
            rowServer.Controls.Add(txtPort, 1, 0);
            rowServer.Controls.Add(btnStart, 2, 0);
            rowServer.Controls.Add(btnStop, 3, 0);

            // Row 1: Prefix + random + barcode
            var rowRandom = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 7,
                RowCount = 1,
                Height = 40
            };
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));   // lbl prefix
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));    // prefix text
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));   // lbl len
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));   // len text
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));   // lbl barcode
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));    // barcode text
            rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // buttons

            var lblPrefix = new UILabel { Text = "Prefix", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            txtPrefix.Dock = DockStyle.Fill;
            txtPrefix.Text = "PREFIX";
            var lblRandLen = new UILabel { Text = "Len", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            txtRandomLen.Dock = DockStyle.Fill;
            txtRandomLen.Text = "16";
            var lblBarcode = new UILabel { Text = "Barcode", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            txtBarcode.Dock = DockStyle.Fill;
            var btnLoadBarcode = new UISymbolButton { Text = "Lấy BC", Symbol = 61639, Dock = DockStyle.Left, Width = 70 };
            btnLoadBarcode.Click += (s, e) => { txtBarcode.Text = FD_Globals.productionData?.Barcode ?? string.Empty; };
            btnSendRandom.Text = "Gửi Prefix+Random";
            btnSendRandom.Symbol = 61527;
            btnSendRandom.Dock = DockStyle.Fill;
            btnSendRandom.Click += (s, e) => SendPrefixRandom();

            rowRandom.Controls.Add(lblPrefix, 0, 0);
            rowRandom.Controls.Add(txtPrefix, 1, 0);
            rowRandom.Controls.Add(lblRandLen, 2, 0);
            rowRandom.Controls.Add(txtRandomLen, 3, 0);
            rowRandom.Controls.Add(lblBarcode, 4, 0);
            rowRandom.Controls.Add(txtBarcode, 5, 0);
            var panelButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            panelButtons.Controls.Add(btnLoadBarcode);
            panelButtons.Controls.Add(btnSendRandom);
            rowRandom.Controls.Add(panelButtons, 6, 0);

            // Row 2: Full string
            var rowFull = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 1,
                Height = 40
            };
            rowFull.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            rowFull.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            txtFull.Dock = DockStyle.Fill;
            txtFull.Watermark = "Nhập full chuỗi gửi đi";
            btnSendFull.Text = "Gửi Full";
            btnSendFull.Symbol = 61527;
            btnSendFull.Dock = DockStyle.Fill;
            btnSendFull.Click += (s, e) => SendFull();

            rowFull.Controls.Add(txtFull, 0, 0);
            rowFull.Controls.Add(btnSendFull, 1, 0);

            // Row 3: Log
            opLog.Dock = DockStyle.Fill;
            opLog.Font = new Font("Consolas", 10);

            root.Controls.Add(rowServer, 0, 0);
            root.Controls.Add(rowRandom, 0, 1);
            root.Controls.Add(rowFull, 0, 2);
            root.Controls.Add(opLog, 0, 3);
            root.SetRowSpan(opLog, 1);
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Controls.Add(root);
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
    }
}

