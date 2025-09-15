using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HslCommunication;
using HslCommunication.Profinet.Siemens;
using Sunny.UI;
using TApp.Configs;
using MTs.Communication;
using MTs.Globals;

namespace TApp.Views.Communications
{
    public partial class SocketTranferSiemen : UIPage
    {
        #region Constants & Enums

        private const int MaxLogItems = 1000;                 // Giới hạn log để ListBox không phình
        private const string SOH = "<SOH>";
        private const string STX = "<STX>";
        private const string ETX = "<ETX>";
        private const string EOT = "<EOT>";
        private const string LF = "<LF>";
        private const string CR = "<CR>";
        private const string BLOCK = "|B|";

        private enum eFunctionCode { WP, RP, GS, QS, ST }

        #endregion

        #region Fields

        private TcpServerHelper? _server;
        private readonly HashSet<string> _clientKeys = new();
        private readonly SynchronizationContext? _ui;
        public SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1200) { Rack = 0, Slot = 0 };
        public SiemensS7Net plc02 = new SiemensS7Net(SiemensPLCS.S1200) { Rack = 0, Slot = 0 };

        #endregion

        #region Ctor

        public SocketTranferSiemen()
        {
            InitializeComponent();
            _ui = SynchronizationContext.Current;

            // forward log ra UI
            OnLog += AppendLog;
        }

        #endregion

        #region Public API (giữ nguyên)

        // Event UI layer có thể subscribe
        public event Action<string>? OnLog;
        public event Action<IReadOnlyCollection<string>>? OnClientsChanged;

        public bool IsStarted => _server?.Started == true;
        public string? CurrentIP => _server?.IP;
        public int? CurrentPort => _server?.Port;

        /// <summary>Khởi chạy: fill config, connect PLC, mở socket server nếu chưa mở.</summary>
        public void START()
        {
            ipPort.Text = AppConfigs.Current.TCP_Port.ToString();
            ipPLCIP.Text = AppConfigs.Current.PLC_IP;
            ipPLCPort.Text = AppConfigs.Current.PLC_Port.ToString();

            // Connect PLC (non-blocking)
            if (PTranferSiemenGlobals.PLCState != ePLCState.Connected)
                _ = ConnectPlcAsync(ipPLCIP.Text, ipPLCPort.Text);

            if (PTranferSiemenGlobals.PLCState02 != ePLCState.Connected)
                _ = ConnectPlcAsync(ipIPPLC02.Text, ipPORTPCL02.Text);
            // Toggle server theo trạng thái hiện tại
            try
            {
                if (!IsStarted)
                {
                    if (!TryReadPort(ipPort?.Text, out var port)) { AppendLog("Port không hợp lệ"); return; }
                    InitializeServer("0.0.0.0", port);
                    StartServer();
                    if (btnOpen != null) btnOpen.Text = "Đóng";
                }
                else
                {
                    StopServer();
                    if (btnOpen != null) btnOpen.Text = "Mở";
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Lỗi mở/kết thúc: {ex.Message}");
            }
        }

        public void InitializeServer(string ip, int port)
        {
            try
            {
                if (_server != null)
                {
                    _server.ServerCallback -= HandleServerCallback;
                    _server.Stop();
                }

                _server = new TcpServerHelper(ip, port);
                _server.ServerCallback += HandleServerCallback;
                Log($"Khởi tạo server tại {ip}:{port}");
            }
            catch (Exception ex)
            {
                Log($"Lỗi khởi tạo server: {ex.Message}");
            }
        }

        public void StartServer()
        {
            if (_server == null) { Log("Server chưa được khởi tạo"); return; }
            _server.Start();
        }

        public void StopServer()
        {
            try { _server?.Stop(); }
            catch (Exception ex) { Log($"Lỗi dừng server: {ex.Message}"); }
        }

        public Task<bool> BroadcastAsync(string data)
            => _server?.BroadcastAsync(data) ?? Task.FromResult(false);

        public Task<bool> SendToAsync(string clientKey, string data)
            => _server?.SendToAsync(clientKey, data) ?? Task.FromResult(false);

        public IReadOnlyCollection<string> GetClientKeys() => new List<string>(_clientKeys);

        public void CloseApp()
        {
            try
            {
                StopServer();
                plc.ConnectClose();
                plc02.ConnectClose();
            }
            catch { /* nuốt lỗi */ }
        }

        #endregion

        #region UI lifecycle

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (_server != null)
                {
                    _server.ServerCallback -= HandleServerCallback;
                    _server.Stop();
                    _server = null;
                }
            }
            catch { /* nuốt lỗi teardown */ }
            base.OnHandleDestroyed(e);
        }

        #endregion

        #region Server callbacks

        private void HandleServerCallback(TcpServerState state, string data)
        {
            switch (state)
            {
                case TcpServerState.Started:
                    PostToUI(() => Log($"Đã mở: {data}"));
                    break;

                case TcpServerState.Stopped:
                    PostToUI(() =>
                    {
                        _clientKeys.Clear();
                        ClientsChanged();
                        Log("Đã dừng server");
                        if (btnOpen != null) btnOpen.Text = "Mở";
                    });
                    break;

                case TcpServerState.ClientConnected:
                    PostToUI(() =>
                    {
                        _clientKeys.Add(data);
                        ClientsChanged();
                        Log($"Client vào: {data}");
                    });
                    break;

                case TcpServerState.ClientDisconnected:
                    PostToUI(() =>
                    {
                        _clientKeys.Remove(data);
                        ClientsChanged();
                        Log($"Client ra: {data}");
                    });
                    break;

                case TcpServerState.Received:
                    PostToUI(() =>
                    {
                        // Format: "{clientKey}|{payload}"
                        var idx = data.IndexOf('|');
                        if (idx > 0)
                        {
                            var clientKey = data[..idx];
                            var msg = data[(idx + 1)..];
                            HandleContent(msg, clientKey);
                            Log($"[{clientKey}] {msg}");
                        }
                        else
                        {
                            Log($"Nhận: {data}");
                        }
                    });
                    break;

                case TcpServerState.Error:
                    PostToUI(() => Log($"[Error] {data}"));
                    break;
            }
        }

        #endregion

        #region Protocol handling

        private sealed record ParsedFrame(
            string MessageId,
            string PlcId,
            string FunctionCode,
            string Address,
            string Data
        );

        private void HandleContent(string rawContent, string clientKey)
        {
            
            var sw = Stopwatch.StartNew();

            // Chuẩn hoá control chars → token
            var content = NormalizeControlChars(rawContent);

            // Parse khung
            if (!TryParse(content, out var frame))
            {
                Log("Khung dữ liệu không hợp lệ");
                return;
            }

            if (!Enum.TryParse(frame.FunctionCode, ignoreCase: true, out eFunctionCode func))
            {
                Log($"FunctionCode không hợp lệ: {frame.FunctionCode}");
                return;
            }

            switch (func)
            {
                case eFunctionCode.WP:
                    HandleWrite(frame, clientKey);
                    break;

                case eFunctionCode.RP:
                    HandleRead(frame, clientKey, sw);
                    break;

                case eFunctionCode.GS:
                    Log($"Lấy trạng thái: MesageID={frame.MessageId}, Address={frame.Address}, Data={frame.Data}");
                    break;

                case eFunctionCode.QS:
                    Log($"Truy vấn nhanh: MesageID={frame.MessageId}, Address={frame.Address}, Data={frame.Data}");
                    break;

                case eFunctionCode.ST:
                    Log($"Gán giá trị: MesageID={frame.MessageId}, Address={frame.Address}, Data={frame.Data}");
                    break;

                default:
                    Log($"FunctionCode không được hỗ trợ: {frame.FunctionCode}");
                    break;
            }
        }

        private void HandleWrite(ParsedFrame f, string clientKey)
        {

            OperateResult write;
            try
            {
                if(f.PlcId == "01")
                {
                    if (f.Data.Contains('[') && f.Data.Contains(']'))
                        write = plc.Write(f.Address, f.Data.ToStringArray<uint>());
                    else
                        write = plc.Write(f.Address, uint.Parse(f.Data));
                }
                else
                {
                    if (f.Data.Contains('[') && f.Data.Contains(']'))
                        write = plc02.Write(f.Address, f.Data.ToStringArray<uint>());
                    else
                        write = plc02.Write(f.Address, uint.Parse(f.Data));
                }

            }
            catch (Exception ex)
            {
                write = new OperateResult() { IsSuccess = false, Message = ex.Message };
            }

            if (write.IsSuccess)
                Log($"Ghi đến PLC thành công: Address={f.Address}, Data={f.Data}");
            else
                Log($"Ghi đến PLC thất bại: Address={f.Address}, Data={f.Data}, Error={write.Message}");

            var response = BuildResponse(f.MessageId, "RP", write.IsSuccess, write.Message);
            _ = SafeSendAsync(clientKey, response);
        }

        private void HandleRead(ParsedFrame f, string clientKey, Stopwatch sw)
        {


            // Data = số lượng word (UInt32) cần đọc
            ushort length = 1;
            if (!string.IsNullOrWhiteSpace(f.Data) &&
                (!ushort.TryParse(f.Data, out length) || length <= 0))
            {
                Log($"Data không hợp lệ, không thể chuyển về số lượng từ cần đọc: {f.Data}");
                length = 1;
            }

            var plcz = f.PlcId == "01" ? this.plc : plc02;

            var read = plcz.ReadUInt32(f.Address, length);
            string dataString = string.Empty;

            if (read.IsSuccess)
            {
                dataString = string.Join(",", read.Content.Select(x => x.ToString()));
            }
            else
            {
                Log($"Đọc từ PLC thất bại: Address={f.Address}, Error={read.Message}");
            }

            sw.Stop();
            var response = BuildResponse(f.MessageId, "RP", read.IsSuccess, read.IsSuccess ? dataString : read.Message);
            _ = SafeSendAsync(clientKey, response, onSent: ok =>

            {
                if (ok) Log($"Đã gửi phản hồi đến Client: {response}, tốn {sw.ElapsedMilliseconds} ms");
                else Log($"Gửi phản hồi đến Client thất bại: {response}");
            });
        }

        private static string NormalizeControlChars(string s) =>
            s.Replace("\u0001", SOH)
             .Replace("\u0002", STX)
             .Replace("\u0003", ETX)
             .Replace("\u0004", EOT)
             .Replace("\n", LF)
             .Replace("\r", CR);

        private static bool TryParse(string content, out ParsedFrame frame)
        {
            frame = default!;

            // content format:
            // <SOH>{MessageId}|B|{FunctionCode}<STX>{Address}|B|{Data}<LF>
            // Tách phần trước/ sau STX
            var parts1 = content.Split(STX);
            if (parts1.Length < 2) return false;

            var beforeStx = parts1[0];
            var afterStx = parts1[1].Replace(LF, string.Empty);

            // Trước STX: SOH + MessageId |B| plcid |B| FunctionCode
            var parts2 = beforeStx.Split(BLOCK);
            var messageId = parts2[0].Replace(SOH, string.Empty);
            var plcid = parts2.Length > 1 ? parts2[1] : string.Empty;
            var function = parts2.Length > 2 ? parts2[2] : string.Empty;

            // Sau STX: Address |B| Data
            var parts3 = afterStx.Split(BLOCK);
            var address = parts3.Length > 0 ? parts3[0] : string.Empty;
            var data = parts3.Length > 1 ? parts3[1] : string.Empty;

            frame = new ParsedFrame(messageId, plcid, function, address, data);
            return !string.IsNullOrWhiteSpace(messageId) && !string.IsNullOrWhiteSpace(function);
        }

        private static string BuildResponse(string messageId, string funcCode, bool ok, string payload)
            => $"{SOH}{messageId}{BLOCK}{funcCode}{STX}{ok}{BLOCK}{payload}{LF}";

        private async Task SafeSendAsync(string clientKey, string response, Action<bool>? onSent = null)
        {
            try
            {
                var ok = await SendToAsync(clientKey, response);
                onSent?.Invoke(ok);
                if (ok) Log($"Đã gửi phản hồi đến Client: {response}");
                else Log($"Gửi phản hồi đến Client thất bại: {response}");
            }
            catch (Exception ex)
            {
                Log($"Lỗi gửi phản hồi: {ex.Message}");
                onSent?.Invoke(false);
            }
        }

        #endregion

        #region PLC

        private async Task ConnectPlcAsync(string ip, string portText, bool plc02true = false)
        {
            if (plc02true)
            {
                if (PTranferSiemenGlobals.PLCState02 == ePLCState.Connected)
                {
                    AppendLog("Đã kết nối PLC02");
                    return;
                }

                AppendLog("Bắt đầu kết nối PLC 02");

                if (!int.TryParse(portText, out var port) || port <= 0 || port > 65535)
                    throw new ArgumentException("PLC02 Port không hợp lệ");

                plc02.CommunicationPipe = new HslCommunication.Core.Pipe.PipeTcpNet(ip, port)
                {
                    ConnectTimeOut = 5000,
                    ReceiveTimeOut = 10000,
                };

                var result = await Task.Run(() => plc.ConnectServer()).ConfigureAwait(false);

                PTranferSiemenGlobals.PLCState = result.IsSuccess ? ePLCState.Connected : ePLCState.Error;

                // Quay về UI
                this.Invoke(new Action(() => AppendLog($"Kết nối PLC: {result.Message}")));

                if (PTranferSiemenGlobals.PLCState02 == ePLCState.Connected)
                {
                    AppendLog("Đã kết nối PLC 02");
                    return;
                }

                return;
            }


            if (PTranferSiemenGlobals.PLCState == ePLCState.Connected)
            {
                AppendLog("Đã kết nối PLC");
                return;
            }

            

            try
            {
                AppendLog("Bắt đầu kết nối PLC");

                if (!int.TryParse(portText, out var port) || port <= 0 || port > 65535)
                    throw new ArgumentException("PLC Port không hợp lệ");

                plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeTcpNet(ip, port)
                {
                    ConnectTimeOut = 5000,
                    ReceiveTimeOut = 10000,
                };

                var result = await Task.Run(() => plc.ConnectServer()).ConfigureAwait(false);

                PTranferSiemenGlobals.PLCState = result.IsSuccess ? ePLCState.Connected : ePLCState.Error;

                // Quay về UI
                this.Invoke(new Action(() => AppendLog($"Kết nối PLC: {result.Message}")));
            }
            catch (Exception ex)
            {
                AppendLog($"Lỗi kết nối PLC: {ex.Message}");
                PTranferSiemenGlobals.PLCState = ePLCState.Error;
            }
        }

        #endregion

        #region Helpers

        private static bool TryReadPort(string? text, out int port)
        {
            port = 0;
            return int.TryParse(text, out port) && port > 0 && port <= 65535;
        }

        private void PostToUI(Action a)
        {
            try
            {
                if (_ui != null) _ui.Post(_ => a(), null);
                else a();
            }
            catch { /* tránh crash UI */ }
        }

        private void ClientsChanged()
        {
            try { OnClientsChanged?.Invoke(GetClientKeys()); } catch { }
        }

        private void Log(string message)
        {
            try
            {
                OnLog?.Invoke(message);
            }
            catch { }
        }

        private void AppendLog(string message)
        {
            try
            {
                if (opShow == null) return;

                // invoke lại UI nếu cần
                if (opShow.InvokeRequired)
                {
                    opShow.Invoke(new Action(() => AppendLog(message)));
                    return;
                }

                // giữ giới hạn log
                while (opShow.Items.Count >= MaxLogItems)
                    opShow.Items.RemoveAt(0);

                opShow.Items.Add($"{DateTime.Now:HH:mm:ss} | {message}");
                var count = opShow.Items.Count;
                if (count > 0) opShow.TopIndex = count - 1;
            }
            catch { }
        }

        #endregion

        #region UI events (giữ signature & logic)

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var content = ipConten?.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(content)) return;

                if (!IsStarted) { AppendLog("Server chưa mở"); return; }

                var ok = await BroadcastAsync(content);
                AppendLog(ok ? $"Đã gửi: {content}" : $"Gửi thất bại: {content}");
            }
            catch (Exception ex)
            {
                AppendLog($"Lỗi gửi: {ex.Message}");
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsStarted)
                {
                    if (!TryReadPort(ipPort?.Text, out var port)) { AppendLog("Port không hợp lệ"); return; }

                    InitializeServer("0.0.0.0", port);
                    StartServer();
                    if (btnOpen != null) btnOpen.Text = "Đóng";
                }
                else
                {
                    StopServer();
                    if (btnOpen != null) btnOpen.Text = "Mở";
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Lỗi mở/kết thúc: {ex.Message}");
            }
        }

        private void btnConnectPLC_Click(object sender, EventArgs e)
        {
            if (PTranferSiemenGlobals.PLCState == ePLCState.Connected)
            {
                AppendLog("Đã kết nối PLC");
                return;
            }

            _ = ConnectPlcAsync(ipPLCIP.Text, ipPLCPort.Text);
        }

        private void btnSendToPLC_Click(object sender, EventArgs e)
        {
            string memoryAddress = ipPLCMemory.Text; // Địa chỉ bộ nhớ PLC
            string value = ipPLCValue.Text;          // Giá trị cần ghi

            if (string.IsNullOrWhiteSpace(memoryAddress) || string.IsNullOrWhiteSpace(value))
            {
                AppendLog("Địa chỉ bộ nhớ hoặc giá trị không được để trống.");
                this.ShowErrorDialog("Địa chỉ bộ nhớ hoặc giá trị không được để trống.");
                return;
            }

            OperateResult write;
            try
            {
                write = (value.Contains('[') && value.Contains(']'))
                    ? plc.Write(memoryAddress, value.ToStringArray<uint>())
                    : plc.Write(memoryAddress, uint.Parse(value));
            }
            catch (Exception ex)
            {
                write = new OperateResult() { IsSuccess = false, Message = ex.Message };
            }

            if (write.IsSuccess) AppendLog("Ghi [M100] thành công");
            else AppendLog($"Ghi [M100] thất bại: {write.Message}");
        }

        private void opShow_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInfoDialog(opShow.SelectedItem?.ToString());
        }

        #endregion

        private void btnConnectPLC02_Click(object sender, EventArgs e)
        {

        }
    }
}
