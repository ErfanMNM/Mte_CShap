using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTs.Communication;
using HslCommunication.Profinet.Siemens;
using TApp.Configs;
using MTs.Globals;
using HslCommunication;

namespace TApp.Views.Communications
{
    public partial class SocketTranferSiemen : UIPage
    {
        private TcpServerHelper? _server;
        private readonly HashSet<string> _clientKeys = new();
        private readonly SynchronizationContext? _ui;
        private const int MaxLogItems = 1000; // tránh phình ListBox
        SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1200);

        public SocketTranferSiemen()
        {
            InitializeComponent();
            _ui = SynchronizationContext.Current;

            // Forward server logs to UI list
            OnLog += AppendLog;


        }

        public void START()
        {
            ipPort.Text = AppConfigs.Current.TCP_Port.ToString();
            ipPLCIP.Text = AppConfigs.Current.PLC_IP;
            ipPLCPort.Text = AppConfigs.Current.PLC_Port.ToString();
        }

        // Expose simple events so the UI layer (controls) can subscribe
        public event Action<string>? OnLog;

        public event Action<IReadOnlyCollection<string>>? OnClientsChanged;

        public bool IsStarted => _server?.Started == true;
        public string? CurrentIP => _server?.IP;
        public int? CurrentPort => _server?.Port;

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
            if (_server == null)
            {
                Log("Server chưa được khởi tạo");
                return;
            }
            _server.Start();
        }

        public void StopServer()
        {
            try
            {
                if (_server != null)
                {
                    _server.Stop();
                }
            }
            catch (Exception ex)
            {
                Log($"Lỗi dừng server: {ex.Message}");
            }
        }

        public Task<bool> BroadcastAsync(string data)
            => _server?.BroadcastAsync(data) ?? Task.FromResult(false);

        public Task<bool> SendToAsync(string clientKey, string data)
            => _server?.SendToAsync(clientKey, data) ?? Task.FromResult(false);

        public IReadOnlyCollection<string> GetClientKeys()
        {
            // trả ảnh chụp cho binding
            return new List<string>(_clientKeys);
        }

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
                        var idx = data.IndexOf('|');
                        if (idx > 0)
                        {
                            var client = data.Substring(0, idx);
                            var msg = data.Substring(idx + 1);
                            HandleContent(msg);
                            Log($"[{client}] {msg}");
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

        private void HandleContent(string content)
        {

        }

        private void PostToUI(Action a)
        {
            try
            {
                if (_ui != null)
                    _ui.Post(_ => a(), null);
                else
                    a();
            }
            catch { /* tránh crash UI */ }
        }

        private void ClientsChanged()
        {
            try { OnClientsChanged?.Invoke(GetClientKeys()); } catch { }
        }

        private void Log(string message)
        {
            try { OnLog?.Invoke(message); } catch { }
        }

        private void AppendLog(string message)
        {
            try
            {
                if (opShow == null) return;

                // giữ giới hạn log
                while (opShow.Items.Count >= MaxLogItems)
                {
                    opShow.Items.RemoveAt(0);
                }

                opShow.Items.Add($"{DateTime.Now:HH:mm:ss} | {message}");
                var count = opShow.Items.Count;
                if (count > 0) opShow.TopIndex = count - 1;
            }
            catch { }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var content = ipConten?.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(content)) return;

                if (!IsStarted)
                {
                    AppendLog("Server chưa mở");
                    return;
                }

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
                    if (!int.TryParse(ipPort?.Text, out var port) || port <= 0 || port > 65535)
                    {
                        AppendLog("Port không hợp lệ");
                        return;
                    }

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

            try
            {
                
                plc.Rack = 0;
                plc.Slot = 0;
                plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeTcpNet("127.0.0.1", 102)
                {
                    ConnectTimeOut = 5000,
                    ReceiveTimeOut = 10000,
                };

                OperateResult result = plc.ConnectServer();

                if (result.IsSuccess)
                {
                    PTranferSiemenGlobals.PLCState = ePLCState.Connected;
                }
                else
                {
                    PTranferSiemenGlobals.PLCState = ePLCState.Error;
                }

                AppendLog($"Kết nối PLC: {result.Message}");
            }
            catch (Exception ex)
            {
                AppendLog($"Lỗi kết nối PLC: {ex.Message}");
                PTranferSiemenGlobals.PLCState = ePLCState.Error;
            }
        }

        private void btnSendToPLC_Click(object sender, EventArgs e)
        {
            string MemoryAddress = ipPLCMemory.Text; // Địa chỉ bộ nhớ PLC
            string Value = ipPLCValue.Text; // Giá trị cần ghi

            if (string.IsNullOrWhiteSpace(MemoryAddress) || string.IsNullOrWhiteSpace(Value))
            {
                AppendLog("Địa chỉ bộ nhớ hoặc giá trị không được để trống.");
                this.ShowErrorDialog("Địa chỉ bộ nhớ hoặc giá trị không được để trống.");
                return;
            }
            OperateResult? write = new OperateResult();

            if (Value.Contains("[") && Value.Contains("]"))
            {
                write = plc.Write(MemoryAddress, Value.ToStringArray<uint>());
            }
            else 
            {
                write = plc.Write(MemoryAddress, uint.Parse(Value));
            }
            
            if (write.IsSuccess)
            {
                AppendLog("Ghi [M100] thành công");
            }
            else
            {
                AppendLog($"Ghi [M100] thất bại: {write.Message}");
            }
        }
    }
}
