using Sunny.UI;
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
        public SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1200);

        public SocketTranferSiemen()
        {
            InitializeComponent();
            _ui = SynchronizationContext.Current;

            // Forward server logs to UI list
            OnLog += AppendLog;

            plc.Rack = 0;
            plc.Slot = 0;
            

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
            // chuyển đổi từ utf-8 sang ký tự đặc biệt
            content = content.Replace("\u0001", "<SOH>")
                             .Replace("\u0002", "<STX>")
                             .Replace("\u0003", "<ETX>")
                             .Replace("\u0004", "<EOT>")
                             .Replace("\n", "<LF>")
                             .Replace("\r", "<CR>");
            //chặt nội dung lấy các thông số

            string[] parts1 = content.Split("<STX>");


            if (parts1.Length < 2) return; // Không có phần sau <STX>
            string afterStx = parts1[1];
            //phần trước <STX>
            string beforeStx = parts1[0];
            //tách block phần trước <STX> |B|
            string[] parts2 = beforeStx.Split("|B|");
            //lấy block đầu tiên là MesageID
            string MesageID = parts2[0].Replace("<SOH>","");
            //lấy block thứ 2 là FunctionCode
            string FunctionCode = parts2.Length > 1 ? parts2[1] : "";

            //xóa <LF> ở cuối
            afterStx = afterStx.Replace("<LF>", "");
            //tách block phần sau <STX> |B|
            string[] parts3 = afterStx.Split("|B|");
            //lấy block thứ 1 là Address
            string Address = parts3.Length > 0 ? parts3[0] : "";
            //lấy block thứ 2 là Data
            string Data = parts3.Length > 1 ? parts3[1] : "";

            //xử lý FunctionCode

            //chuyển FunctionCode về enum
            if (!Enum.TryParse(FunctionCode, out eFunctionCode func))
            {
                Log($"FunctionCode không hợp lệ: {FunctionCode}");
                return;
            }

            //xử lý theo FunctionCode dùng switch case

            switch (func)
            {
                case eFunctionCode.WP:
                    //Ghi từ Client
                    Log($"Ghi từ Client: MesageID={MesageID}, Address={Address}, Data={Data}");
                    //Gửi giá trị Data đến PLC tại địa chỉ Address
                    OperateResult? write = new OperateResult();
                    if (Data.Contains("[") && Data.Contains("]"))
                    {
                        write = plc.Write(Address, Data.ToStringArray<uint>());
                    }
                    else
                    {
                        write = plc.Write(Address, uint.Parse(Data));
                    }
                    if (write.IsSuccess)
                    {
                        Log($"Ghi đến PLC thành công: Address={Address}, Data={Data}");
                    }
                    else
                    {
                        Log($"Ghi đến PLC thất bại: Address={Address}, Data={Data}, Error={write.Message}");
                    }
                    //gửi phản hồi về Client
                    var response = $"<SOH>{MesageID}|B|RP<STX>{write.IsSuccess}|B|{write.Message}<LF>";
                    var ok = SendToAsync(_clientKeys.First(), response);
                    if (ok.Result)
                    {
                        Log($"Đã gửi phản hồi đến Client: {response}");
                    }
                    else
                    {
                        Log($"Gửi phản hồi đến Client thất bại: {response}");
                    }

                    break;
                case eFunctionCode.RP:
                    //Đọc từ Client
                    Log($"Đọc từ Client: MesageID={MesageID}, Address={Address}, Data={Data}");
                    //đổi Data về số lượng từ cần đọc ushort
                    ushort length = 1;
                    if (!string.IsNullOrWhiteSpace(Data))
                    {
                        if (!ushort.TryParse(Data, out length) || length <= 0)
                        {
                            Log($"Data không hợp lệ, không thể chuyển về số lượng từ cần đọc: {Data}");
                            //return;
                        }
                    }

                    //Đọc giá trị từ PLC tại địa chỉ Address
                    OperateResult<uint[]> read = plc.ReadUInt32(Address, length);
                    if (read.IsSuccess)
                    {
                        Log($"Đọc từ PLC thành công: Address={Address}, Value={read.Content}");
                    }
                    else
                    {
                        Log($"Đọc từ PLC thất bại: Address={Address}, Error={read.Message}");
                    }

                    break;
                case eFunctionCode.GS:
                    //Lấy trạng thái
                    Log($"Lấy trạng thái: MesageID={MesageID}, Address={Address}, Data={Data}");
                    break;
                case eFunctionCode.QS:
                    //Truy vấn nhanh
                    Log($"Truy vấn nhanh: MesageID={MesageID}, Address={Address}, Data={Data}");
                    break;
                case eFunctionCode.ST:
                    //Dừng
                    Log($"Gán giá trị: MesageID={MesageID}, Address={Address}, Data={Data}");
                    break;
                default:
                    Log($"FunctionCode không được hỗ trợ: {FunctionCode}");
                    break;
            }




        }

        private enum eFunctionCode
        {
            WP,
            RP,
            GS,
            QS,
            ST
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
            try
            {

                OnLog?.Invoke(message);
                LogBootstrap.Logger.Log("SocketServer", "INFO", message, "SocketTranferSiemen");

            }
            catch { }
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

                //invoke UI thread nếu cần
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
                AppendLog($"Bắt đầu kết nối PLC");

                plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeTcpNet(ipPLCIP.Text, ipPLCPort.Text.ToInt())
                {
                    ConnectTimeOut = 5000,
                    ReceiveTimeOut = 10000,
                };
                Task.Run(() =>
                {
                    OperateResult result = plc.ConnectServer();

                    if (result.IsSuccess)
                    {
                        PTranferSiemenGlobals.PLCState = ePLCState.Connected;
                    }
                    else
                    {
                        PTranferSiemenGlobals.PLCState = ePLCState.Error;
                    }
                    this.Invoke(new Action(() => { AppendLog($"Kết nối PLC: {result.Message}"); }));
                });
                
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

        private void opShow_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInfoDialog(opShow.SelectedItem.ToString());
        }
    }
}
