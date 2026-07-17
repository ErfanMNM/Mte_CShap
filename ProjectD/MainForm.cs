using System.Collections.Concurrent;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.Sqlite;

namespace ProjectD
{
    public partial class MainForm : Form
    {
        private TcpListener? _tcpListener;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly object _clientsLock = new();
        private List<string> _codeList = new();
        private bool _isServerRunning;

        public MainForm()
        {
            InitializeComponent();
        }

        #region TCP Server

        private async void btnStartStop_Click(object? sender, EventArgs e)
        {
            if (_isServerRunning)
            {
                StopServer();
            }
            else
            {
                StartServer();
            }
        }

        private void StartServer()
        {
            if (!int.TryParse(txtPort.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                Log("ERROR: Invalid port number!");
                return;
            }

            try
            {
                _cts = new CancellationTokenSource();
                _tcpListener = new TcpListener(IPAddress.Any, port);
                _tcpListener.Start();
                _isServerRunning = true;

                btnStartStop.Text = "Stop";
                lblStatus.Text = "Running";
                lblStatus.ForeColor = Color.Green;
                txtPort.Enabled = false;
                lblClientCount.Text = "Clients: 0";

                Log($"Server started on port {port}");

                _ = AcceptClientsAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                Log($"ERROR: Failed to start server - {ex.Message}");
                _isServerRunning = false;
            }
        }

        private void StopServer()
        {
            try
            {
                _cts?.Cancel();
                _tcpListener?.Stop();

                lock (_clientsLock)
                {
                    foreach (var client in _clients.Values)
                    {
                        try { client.Close(); } catch { }
                    }
                    _clients.Clear();
                }

                _isServerRunning = false;
                btnStartStop.Text = "Start";
                lblStatus.Text = "Stopped";
                lblStatus.ForeColor = Color.Red;
                txtPort.Enabled = true;
                lblClientCount.Text = "Clients: 0";

                Log("Server stopped");
            }
            catch (Exception ex)
            {
                Log($"ERROR: Failed to stop server - {ex.Message}");
            }
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _tcpListener != null)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(token);
                    var clientId = Guid.NewGuid().ToString();

                    lock (_clientsLock)
                    {
                        _clients[clientId] = client;
                        lblClientCount.Text = $"Clients: {_clients.Count}";
                    }

                    Log($"Client connected: {clientId} ({((IPEndPoint?)client.Client.RemoteEndPoint)?.Address})");
                    _ = HandleClientAsync(clientId, client, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        Log($"ERROR: Accept client error - {ex.Message}");
                    }
                }
            }
        }

        private async Task HandleClientAsync(string clientId, TcpClient client, CancellationToken token)
        {
            var buffer = new byte[4096];
            try
            {
                var stream = client.GetStream();
                while (!token.IsCancellationRequested && client.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, token);
                    if (bytesRead == 0) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Log($"Received from {clientId}: {message.Trim()}");
                }
            }
            catch (Exception)
            {
                // Client disconnected
            }
            finally
            {
                lock (_clientsLock)
                {
                    _clients.TryRemove(clientId, out _);
                    lblClientCount.Text = $"Clients: {_clients.Count}";
                }
                try { client.Close(); } catch { }
                Log($"Client disconnected: {clientId}");
            }
        }

        private async Task SendToAllClientsAsync(string message)
        {
            if (_clients.IsEmpty)
            {
                Log("WARNING: No clients connected!");
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            var disconnectedClients = new List<string>();

            lock (_clientsLock)
            {
                foreach (var kvp in _clients)
                {
                    try
                    {
                        if (kvp.Value.Connected)
                        {
                            kvp.Value.GetStream().Write(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            disconnectedClients.Add(kvp.Key);
                        }
                    }
                    catch
                    {
                        disconnectedClients.Add(kvp.Key);
                    }
                }
            }

            foreach (var id in disconnectedClients)
            {
                lock (_clientsLock)
                {
                    _clients.TryRemove(id, out _);
                }
            }

            if (_clients.IsEmpty)
            {
                lblClientCount.Text = "Clients: 0";
            }
        }

        #endregion

        #region SQLite Data Loading

        private void btnBrowse_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "SQLite Database|*.db;*.sqlite;*.sqlite3|All Files|*.*",
                Title = "Select SQLite Database"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtDbPath.Text = dialog.FileName;
            }
        }

        private void btnLoadData_Click(object? sender, EventArgs e)
        {
            LoadSqliteData();
        }

        private void LoadSqliteData()
        {
            var dbPath = txtDbPath.Text.Trim();
            var tableName = txtTableName.Text.Trim();
            var columnName = txtColumnName.Text.Trim();

            if (string.IsNullOrEmpty(dbPath))
            {
                Log("ERROR: Please select a database file!");
                return;
            }

            if (string.IsNullOrEmpty(tableName))
            {
                Log("ERROR: Please enter table name!");
                return;
            }

            if (string.IsNullOrEmpty(columnName))
            {
                Log("ERROR: Please enter column name!");
                return;
            }

            if (!File.Exists(dbPath))
            {
                Log($"ERROR: Database file not found: {dbPath}");
                return;
            }

            try
            {
                _codeList.Clear();
                dgvData.DataSource = null;

                var connectionString = $"Data Source={dbPath}";
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var query = $"SELECT {columnName} FROM {tableName}";
                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                var dt = new DataTable();
                dt.Columns.Add("Index", typeof(int));
                dt.Columns.Add(columnName, typeof(string));

                int index = 0;
                while (reader.Read())
                {
                    var code = reader[0]?.ToString() ?? "";
                    _codeList.Add(code);
                    dt.Rows.Add(index++, code);
                }

                dgvData.DataSource = dt;
                lblRowCount.Text = $"Rows: {_codeList.Count}";
                Log($"Loaded {_codeList.Count} codes from {tableName}.{columnName}");
            }
            catch (Exception ex)
            {
                Log($"ERROR: Failed to load data - {ex.Message}");
            }
        }

        #endregion

        #region Send Functions

        private string FormatMessage(string code)
        {
            var prefix = txtPrefix.Text;
            var suffix = txtSuffix.Text;
            var endChar = txtEndChar.Text;

            var result = prefix + code + suffix;

            if (!string.IsNullOrEmpty(endChar))
            {
                var resolvedEndChar = endChar
                    .Replace("\\r\\n", "\r\n")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t");
                result += resolvedEndChar;
            }

            return result;
        }

        private async void btnSendAll_Click(object? sender, EventArgs e)
        {
            await SendBatchAsync();
        }

        private async Task SendBatchAsync()
        {
            if (_clients.IsEmpty)
            {
                Log("ERROR: No clients connected!");
                return;
            }

            if (_codeList.Count == 0)
            {
                Log("ERROR: No codes loaded!");
                return;
            }

            int startIndex = (int)numStartIndex.Value;
            int count = (int)numCount.Value;

            if (startIndex >= _codeList.Count)
            {
                Log($"ERROR: Start index {startIndex} >= list size {_codeList.Count}");
                return;
            }

            int endIndex = (count == 0) ? _codeList.Count : Math.Min(startIndex + count, _codeList.Count);
            int actualCount = endIndex - startIndex;

            if (!int.TryParse(txtDelayMs.Text.Trim(), out int delayMs) || delayMs < 0)
            {
                delayMs = 120;
            }

            Log($"Starting batch send: {actualCount} codes from index {startIndex}, delay {delayMs}ms");
            btnSendAll.Enabled = false;
            btnSendOne.Enabled = false;

            try
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    var message = FormatMessage(_codeList[i]);
                    await SendToAllClientsAsync(message);
                    Log($"Sent [{i}/{endIndex}]: {message.Replace("\r", "\\r").Replace("\n", "\\n")}");

                    if (i < endIndex - 1 && delayMs > 0)
                    {
                        await Task.Delay(delayMs);
                    }
                }
                Log($"Batch send completed: {actualCount} codes sent");
            }
            catch (Exception ex)
            {
                Log($"ERROR: Send error - {ex.Message}");
            }
            finally
            {
                btnSendAll.Enabled = true;
                btnSendOne.Enabled = true;
            }
        }

        private async void btnSendOne_Click(object? sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count == 0)
            {
                Log("WARNING: Please select a row in the data grid!");
                return;
            }

            var selectedIndex = dgvData.SelectedRows[0].Index;
            if (selectedIndex >= 0 && selectedIndex < _codeList.Count)
            {
                var message = FormatMessage(_codeList[selectedIndex]);
                await SendToAllClientsAsync(message);
                Log($"Sent single [{selectedIndex}]: {message.Replace("\r", "\\r").Replace("\n", "\\n")}");
            }
        }

        private async void btnSendManualCode_Click(object? sender, EventArgs e)
        {
            var code = txtManualCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                Log("WARNING: Please enter a code!");
                return;
            }

            var message = FormatMessage(code);
            await SendToAllClientsAsync(message);
            Log($"Sent manual: {message.Replace("\r", "\\r").Replace("\n", "\\n")}");
        }

        #endregion

        #region Log

        private void Log(string message)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
                }));
            }
        }

        private void btnClearLog_Click(object? sender, EventArgs e)
        {
            txtLog.Clear();
        }

        #endregion

        #region Form Events

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            StopServer();
            _cts?.Dispose();
        }

        #endregion
    }
}
