using System.Diagnostics;
using System.Globalization;
using LibUA.Core;
using Sunny.UI;
using TTManager.Communication.OPCUA;

namespace CProject.Views
{
    public partial class Page_OPC_MHG : UIPage
    {
        private const int MaxLogItems = 500;
        private const string DefaultEndpoint = "opc.tcp://DESKTOP-3LR82CB:53530/OPCUA/SimulationServer";
        private const string DisconnectedLabel = "Đã ngắt kết nối";
        private const string ConnectingLabel = "Đang kết nối…";
        private const string ConnectedLabel = "Đã kết nối";

        private OpcUaClientHelper? _opcClient;
        private OpcUaClientHelper.OpcUaClientEventHandler? _opcCallback;
        private bool _busy;

        public Page_OPC_MHG()
        {
            InitializeComponent();
            cboWriteType.SelectedIndex = 0;
            SetConnectionUi(false);
            AppendLog("Sẵn sàng. Nhập Endpoint rồi bấm Kết nối.");
        }

        // ===== UI helpers =====================================================

        private void SetConnectionUi(bool connected, string statusText = "")
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool, string>(SetConnectionUi), connected, statusText);
                return;
            }

            string suffix = string.IsNullOrEmpty(statusText) ? string.Empty : $" - {statusText}";
            lblStatus.Text = connected
                ? $"{ConnectedLabel}{suffix}"
                : (string.IsNullOrEmpty(statusText) ? DisconnectedLabel : statusText);

            btnConnect.Enabled = !connected && !_busy;
            btnDisconnect.Enabled = connected && !_busy;
            txtEndpoint.Enabled = !connected && !_busy;
            txtTimeout.Enabled = !connected && !_busy;
            btnClearEndpoint.Enabled = !_busy;

            tabApiGroups.Enabled = connected && !_busy;
            if (!connected)
            {
                ClearReadFields();
            }
        }

        private void ClearReadFields()
        {
            txtReadValue.Text = string.Empty;
            txtReadType.Text = string.Empty;
            txtReadStatus.Text = string.Empty;
            txtReadTimestamp.Text = string.Empty;
            txtReadServerTimestamp.Text = string.Empty;
        }

        private void AppendLog(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            if (lstLog.InvokeRequired)
            {
                lstLog.BeginInvoke(new Action(() => AppendLogInternal(line)));
            }
            else
            {
                AppendLogInternal(line);
            }
        }

        private void AppendLogInternal(string line)
        {
            lstLog.Items.Insert(0, line);
            if (lstLog.Items.Count > MaxLogItems)
            {
                lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
            }
            lstLog.TopIndex = 0;
        }

        private void SetBusy(bool busy, string? busyMessage = null)
        {
            _busy = busy;
            bool connected = _opcClient?.Connected ?? false;
            SetConnectionUi(connected, busyMessage ?? string.Empty);
        }

        private bool ValidateEndpointAndTimeout(out int timeoutMs)
        {
            timeoutMs = 0;
            string endpoint = txtEndpoint.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(endpoint))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Endpoint.");
                return false;
            }

            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri) ||
                !string.Equals(uri.Scheme, "opc.tcp", StringComparison.OrdinalIgnoreCase))
            {
                UIMessageBox.ShowWarning("Endpoint không hợp lệ. Phải có dạng opc.tcp://host:port[/path].");
                return false;
            }

            string timeoutText = txtTimeout.Text?.Trim() ?? string.Empty;
            if (!int.TryParse(timeoutText, NumberStyles.Integer, CultureInfo.InvariantCulture, out timeoutMs) || timeoutMs <= 0)
            {
                UIMessageBox.ShowWarning("Timeout (ms) phải là số nguyên dương.");
                return false;
            }

            return true;
        }

        private static bool TryParseNodeId(string raw, out string normalized)
        {
            normalized = (raw ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            try
            {
                NodeId? parsed = NodeId.TryParse(normalized);
                if (parsed == null || parsed.IsNull())
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string FormatTimestamp(DateTime? ts)
        {
            return ts.HasValue ? ts.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) : "(rỗng)";
        }

        private bool TryParseWriteValue(string type, string raw, out object? value, out string? error)
        {
            value = null;
            error = null;
            string text = raw ?? string.Empty;

            switch (type)
            {
                case "String":
                    value = text;
                    return true;
                case "Boolean":
                    if (bool.TryParse(text.Trim(), out bool b))
                    {
                        value = b;
                        return true;
                    }
                    if (text.Trim() == "1") { value = true; return true; }
                    if (text.Trim() == "0") { value = false; return true; }
                    error = "Boolean phải là true/false (hoặc 1/0).";
                    return false;
                case "Int32":
                    if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                    {
                        value = i;
                        return true;
                    }
                    error = "Int32 không hợp lệ.";
                    return false;
                case "Double":
                    if (double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    {
                        value = d;
                        return true;
                    }
                    error = "Double không hợp lệ.";
                    return false;
                default:
                    error = $"Data Type không hỗ trợ: {type}";
                    return false;
            }
        }

        // ===== Lifecycle =====================================================

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent == null)
            {
                DisposeClient();
            }
        }

        private void DisposeClient()
        {
            if (_opcClient == null) return;

            if (_opcCallback != null)
            {
                _opcClient.OpcUaClientCallback -= _opcCallback;
                _opcCallback = null;
            }

            try
            {
                _opcClient.DisconnectAsync().GetAwaiter().GetResult();
            }
            catch
            {
            }

            try
            {
                _opcClient.Dispose();
            }
            catch
            {
            }

            _opcClient = null;
        }

        // ===== Event handlers ================================================

        private void btnClearEndpoint_Click(object? sender, EventArgs e)
        {
            txtEndpoint.Text = DefaultEndpoint;
            txtTimeout.Text = "10000";
            AppendLog("Đã reset Endpoint/Timeout về mặc định.");
        }

        private async void btnConnect_Click(object? sender, EventArgs e)
        {
            if (_busy) return;
            if (!ValidateEndpointAndTimeout(out int timeoutMs)) return;

            string endpoint = txtEndpoint.Text.Trim();
            SetBusy(true, ConnectingLabel);
            AppendLog($"Bắt đầu kết nối tới {endpoint} (timeout {timeoutMs} ms).");

            DisposeClient();

            var client = new OpcUaClientHelper(endpoint) { Timeout = timeoutMs };
            var callback = new OpcUaClientHelper.OpcUaClientEventHandler(OnOpcUaClientCallback);
            client.OpcUaClientCallback += callback;

            _opcClient = client;
            _opcCallback = callback;

            try
            {
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                AppendLog($"Kết nối ném exception: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnDisconnect_Click(object? sender, EventArgs e)
        {
            if (_busy || _opcClient == null) return;

            SetBusy(true, "Đang ngắt kết nối…");
            AppendLog("Yêu cầu ngắt kết nối.");

            try
            {
                await _opcClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                AppendLog($"Ngắt kết nối ném exception: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnClearLog_Click(object? sender, EventArgs e)
        {
            lstLog.Items.Clear();
            AppendLog("Đã xóa nhật ký.");
        }

        private void btnCopyLog_Click(object? sender, EventArgs e)
        {
            try
            {
                if (lstLog.Items.Count == 0) return;
                var sb = new System.Text.StringBuilder();
                for (int i = lstLog.Items.Count - 1; i >= 0; i--)
                {
                    sb.AppendLine(lstLog.Items[i]?.ToString());
                }
                Clipboard.SetText(sb.ToString());
                AppendLog($"Đã copy {lstLog.Items.Count} dòng log vào clipboard.");
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowWarning($"Không copy được: {ex.Message}");
            }
        }

        private void OnOpcUaClientCallback(OpcUaClientState state, string data)
        {
            AppendLog($"[{state}] {data}");

            bool connected = _opcClient?.Connected ?? false;
            switch (state)
            {
                case OpcUaClientState.Connected:
                    SetConnectionUi(true, data);
                    break;
                case OpcUaClientState.Disconnected:
                    SetConnectionUi(false, data);
                    break;
                case OpcUaClientState.Error:
                    SetConnectionUi(connected, $"Lỗi: {data}");
                    break;
                case OpcUaClientState.Received:
                    SetConnectionUi(connected);
                    break;
            }
        }

        // ===== Operations ====================================================

        private async void btnRead_Click(object? sender, EventArgs e)
        {
            if (_busy) return;
            if (_opcClient == null || !_opcClient.Connected)
            {
                UIMessageBox.ShowWarning("Chưa kết nối tới OPC UA server.");
                return;
            }

            string nodeId = txtReadNodeId.Text;
            if (!TryParseNodeId(nodeId, out string normalized))
            {
                UIMessageBox.ShowWarning("NodeId không hợp lệ. Ví dụ: ns=2;s=Tag1, ns=3;i=1001.");
                return;
            }

            ClearReadFields();
            SetBusy(true, "Đang đọc…");
            var sw = Stopwatch.StartNew();

            try
            {
                DataValue? dataValue = await _opcClient.ReadAsync(normalized);
                sw.Stop();

                if (dataValue == null)
                {
                    AppendLog($"Read '{normalized}' thất bại: helper trả về null (server không phản hồi).");
                    UIMessageBox.ShowError("Không đọc được giá trị. Xem log để biết chi tiết.");
                    return;
                }

                txtReadValue.Text = dataValue.Value?.ToString() ?? "(null)";
                txtReadType.Text = dataValue.Value?.GetType().FullName ?? "(null)";
                txtReadStatus.Text = dataValue.StatusCode.HasValue
                    ? dataValue.StatusCode.Value.ToString()
                    : "(rỗng)";
                txtReadTimestamp.Text = FormatTimestamp(dataValue.SourceTimestamp);
                txtReadServerTimestamp.Text = FormatTimestamp(dataValue.ServerTimestamp);

                AppendLog($"Read '{normalized}' OK trong {sw.ElapsedMilliseconds} ms - value='{txtReadValue.Text}', type={txtReadType.Text}, status={txtReadStatus.Text}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppendLog($"Read '{normalized}' ném exception sau {sw.ElapsedMilliseconds} ms: {ex.Message}");
                UIMessageBox.ShowError($"Read lỗi:\n{ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnWrite_Click(object? sender, EventArgs e)
        {
            if (_busy) return;
            if (_opcClient == null || !_opcClient.Connected)
            {
                UIMessageBox.ShowWarning("Chưa kết nối tới OPC UA server.");
                return;
            }

            string nodeId = txtWriteNodeId.Text;
            if (!TryParseNodeId(nodeId, out string normalized))
            {
                UIMessageBox.ShowWarning("NodeId không hợp lệ. Ví dụ: ns=2;s=Tag1, ns=3;i=1001.");
                return;
            }

            string type = cboWriteType.SelectedItem?.ToString() ?? "String";
            if (!TryParseWriteValue(type, txtWriteValue.Text, out object? value, out string? parseError))
            {
                UIMessageBox.ShowWarning(parseError ?? "Giá trị không hợp lệ.");
                return;
            }

            SetBusy(true, "Đang ghi…");
            var sw = Stopwatch.StartNew();

            try
            {
                bool ok = await _opcClient.WriteAsync(normalized, value!);
                sw.Stop();

                if (ok)
                {
                    AppendLog($"Write '{normalized}' = '{value}' OK trong {sw.ElapsedMilliseconds} ms.");
                    UIMessageBox.ShowInfo($"Ghi thành công '{normalized}'.");
                }
                else
                {
                    AppendLog($"Write '{normalized}' thất bại sau {sw.ElapsedMilliseconds} ms. Xem log để biết StatusCode.");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppendLog($"Write '{normalized}' ném exception sau {sw.ElapsedMilliseconds} ms: {ex.Message}");
                UIMessageBox.ShowError($"Write lỗi:\n{ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}
