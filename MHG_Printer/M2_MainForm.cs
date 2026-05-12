using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUA;
using LibUA.Core;
using Sunny.UI;

namespace MHG_Printer
{
    public partial class M2_MainForm : UIForm
    {
        private Client? _opcClient;
        private bool _opcConnected;

        public M2_MainForm()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object? sender, EventArgs e)
        {
            await ConnectOpcUaAsync();
        }

        private async void btnDisconnect_Click(object? sender, EventArgs e)
        {
            await DisconnectOpcUaAsync();
        }

        private async void btnRead_Click(object? sender, EventArgs e)
        {
            await ReadOpcUaAsync();
        }

        private async void btnWrite_Click(object? sender, EventArgs e)
        {
            await WriteOpcUaAsync();
        }

        private async void M2_MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            await DisconnectOpcUaAsync();
        }

        private async Task ConnectOpcUaAsync()
        {
            SetBusy(true);
            try
            {
                await DisconnectOpcUaAsync(false);
                string endpointUrl = txtEndpoint.Text.Trim();
                WriteLog($"Connecting: {endpointUrl}");

                _opcClient = CreateClient(endpointUrl);
                _opcClient.OnConnectionClosed += () => BeginInvoke(() => SetConnected(false, "Connection closed by server."));

                WriteLog("Step 1/4: TCP connect...");
                CheckStatus(await RunOpcUaStepAsync(() => _opcClient.Connect(), "TCP connect"), "Connect");

                WriteLog("Step 2/4: Open secure channel...");
                CheckStatus(await RunOpcUaStepAsync(() => _opcClient.OpenSecureChannel(MessageSecurityMode.None, SecurityPolicy.None, null), "Open secure channel"), "Open secure channel");

                WriteLog("Step 3/4: Create session...");
                CheckStatus(await RunOpcUaStepAsync(() => _opcClient.CreateSession(CreateApplicationDescription(), "M2 OPC UA Test", 60000), "Create session"), "Create session");

                WriteLog("Step 4/4: Activate anonymous session...");
                CheckStatus(await RunOpcUaStepAsync(() => _opcClient.ActivateSession(new UserIdentityAnonymousToken("anonymous"), Array.Empty<string>()), "Activate session"), "Activate session");

                SetConnected(true, "Connected successfully.");
            }
            catch (Exception ex)
            {
                ForceDisposeOpcClient();
                SetConnected(false, $"Connect failed: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task DisconnectOpcUaAsync(bool writeLog = true)
        {
            if (_opcClient == null)
            {
                SetConnected(false, writeLog ? "Disconnected." : string.Empty);
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    if (_opcClient.IsConnected)
                    {
                        _opcClient.CloseSession();
                        _opcClient.CloseSecureChannel();
                        _opcClient.Disconnect();
                    }
                }
                catch
                {
                }
                finally
                {
                    _opcClient.Dispose();
                    _opcClient = null;
                }
            });

            SetConnected(false, writeLog ? "Disconnected." : string.Empty);
        }

        private void ForceDisposeOpcClient()
        {
            try
            {
                _opcClient?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _opcClient = null;
                _opcConnected = false;
            }
        }

        private async Task ReadOpcUaAsync()
        {
            if (_opcClient == null || !_opcConnected) return;

            SetBusy(true);
            try
            {
                var requests = new[]
                {
                    new ReadValueId(ParseNodeId(txtNodeId.Text.Trim()), NodeAttribute.Value, string.Empty, new QualifiedName(string.Empty))
                };

                DataValue[] results = Array.Empty<DataValue>();
                CheckStatus(await Task.Run(() => _opcClient.Read(requests, out results)), "Read");

                DataValue? dataValue = results.Length > 0 ? results[0] : null;
                txtReadValue.Text = dataValue?.Value?.ToString() ?? string.Empty;
                WriteLog($"Read OK: {txtNodeId.Text.Trim()} = {txtReadValue.Text}");
            }
            catch (Exception ex)
            {
                WriteLog($"Read failed: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task WriteOpcUaAsync()
        {
            if (_opcClient == null || !_opcConnected) return;

            SetBusy(true);
            try
            {
                object value = ConvertWriteValue(txtWriteValue.Text.Trim(), cboWriteType.Text);
                var requests = new[]
                {
                    new WriteValue(ParseNodeId(txtNodeId.Text.Trim()), NodeAttribute.Value, string.Empty, new DataValue(value, StatusCode.Good, DateTime.UtcNow, DateTime.UtcNow))
                };

                uint[] results = Array.Empty<uint>();
                CheckStatus(await Task.Run(() => _opcClient.Write(requests, out results)), "Write");

                if (results.Length > 0 && results[0] != (uint)StatusCode.Good)
                {
                    WriteLog($"Write failed: {(StatusCode)results[0]}");
                    return;
                }

                WriteLog($"Write OK: {txtNodeId.Text.Trim()} = {value}");
            }
            catch (Exception ex)
            {
                WriteLog($"Write failed: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static Client CreateClient(string endpointUrl)
        {
            if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out Uri? uri))
            {
                throw new ArgumentException($"Invalid endpoint url: {endpointUrl}");
            }

            if (!string.Equals(uri.Scheme, "opc.tcp", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Endpoint must use opc.tcp scheme.");
            }

            string path = uri.AbsolutePath == "/" ? string.Empty : uri.AbsolutePath.TrimStart('/');
            return string.IsNullOrEmpty(path)
                ? new Client(uri.Host, uri.Port, 10000, 65536)
                : new Client(uri.Host, uri.Port, path, 10000, 65536);
        }

        private static async Task<StatusCode> RunOpcUaStepAsync(Func<StatusCode> action, string actionName)
        {
            Task<StatusCode> actionTask = Task.Run(action);
            Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
            Task completedTask = await Task.WhenAny(actionTask, timeoutTask);

            if (completedTask != actionTask)
            {
                throw new TimeoutException($"{actionName} timeout after 10 seconds.");
            }

            return await actionTask;
        }

        private static ApplicationDescription CreateApplicationDescription()
        {
            return new ApplicationDescription(
                "urn:MHG_Printer:M2OpcUaTest",
                "urn:MHG_Printer:M2OpcUaTest",
                new LocalizedText("M2 OPC UA Test"),
                ApplicationType.Client,
                string.Empty,
                string.Empty,
                Array.Empty<string>());
        }

        private static NodeId ParseNodeId(string nodeId)
        {
            NodeId? parsed = NodeId.TryParse(nodeId);
            if (parsed == null || parsed.IsNull())
            {
                throw new ArgumentException($"Invalid node id: {nodeId}");
            }

            return parsed;
        }

        private static object ConvertWriteValue(string value, string typeName)
        {
            return typeName switch
            {
                "Int32" => int.Parse(value),
                "Double" => double.Parse(value),
                "Boolean" => bool.Parse(value),
                _ => value
            };
        }

        private static void CheckStatus(StatusCode statusCode, string action)
        {
            if (statusCode != StatusCode.Good)
            {
                throw new InvalidOperationException($"{action} failed: {statusCode}");
            }
        }

        private void SetConnected(bool connected, string message)
        {
            _opcConnected = connected;
            lblStatus.Text = connected ? "Connected" : "Disconnected";
            lblStatus.ForeColor = connected ? System.Drawing.Color.SeaGreen : System.Drawing.Color.Firebrick;
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            btnRead.Enabled = connected;
            btnWrite.Enabled = connected;

            if (!string.IsNullOrWhiteSpace(message))
            {
                WriteLog(message);
            }
        }

        private void SetBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            btnConnect.Enabled = !busy && !_opcConnected;
            btnDisconnect.Enabled = !busy && _opcConnected;
            btnRead.Enabled = !busy && _opcConnected;
            btnWrite.Enabled = !busy && _opcConnected;
        }

        private void WriteLog(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}


