using Sunny.UI;
using System.Reflection.Emit;
using System.Text.Json;
using TTManager.Communication.WebSocket;
using TTManager.Omron;
using TTManager.PDA;
using TTManager.PLCHelpers;
using VNQR.Configs;
using VNQR.Infrastructure;
using VNQR.Utils;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;

namespace VNQR
{
    public partial class VNQRMainForm : UIForm
    {
        #region Fields
        private OmronCamera? omronCamera;
        private OmronPLC_Hsl.PLCStatus? PLC_Status = OmronPLC_Hsl.PLCStatus.Disconnect;
        private readonly PdaScanManager _pdaManager = new();
        private readonly WebSocketServerHelper? _wsServer;
        private readonly int _wsPort = 8080;
        #endregion
        public VNQRMainForm()
        {
            InitializeComponent();

            _wsServer = new WebSocketServerHelper(_wsPort, "/ws");
            _wsServer.WebSocketServerCallback += WsServer_Callback;
            _wsServer.Start();

            omronCamera = new OmronCamera(OmronCamera.e_CameraModel.V430, AppConfigs.Current.Camera_01_IP, AppConfigs.Current.Camera_01_Port);
            omronCamera.ClientCallback += OmronCamera_ClientCallback;
            omronCamera.Connect();
            _pdaManager.OnScanReceived += Pda_ScanCallback;
            _ = StartPdaServerSafely();
        }

        private void WsServer_Callback(WebSocketServerState state, string data)
        {
            MainFormVariable.listbox.Enqueue($"[WS] {state}: {data}");
        }

        private async Task BroadcastDeviceStatusAsync()
        {
            if (_wsServer == null || _wsServer.ClientCount == 0) return;

            var status = new
            {
                type = "device_status",
                timestamp = DateTime.Now,
                camera = new
                {
                    state = gvr.CameraState.ToString(),
                    ip = AppConfigs.Current.Camera_01_IP
                },
                plc = new
                {
                    state = PLC_Status?.ToString() ?? "Unknown",
                    connected = PLC_Status == OmronPLC_Hsl.PLCStatus.Connected
                },
                app = new
                {
                    state = gvr.AppState.ToString()
                }
            };

            string json = JsonSerializer.Serialize(status);
            await _wsServer.BroadcastAsync(json);
        }

        private async Task StartPdaServerSafely()
        {
            try
            {
                await _pdaManager.StartAsync();
                System.Diagnostics.Debug.WriteLine("PDA Server started on port 6969.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PDA Server failed: {ex.Message}");
            }
        }

        private void Pda_ScanCallback(ScanData data)
        {
            MainFormVariable.listbox.Enqueue($"[{data.Time:HH:mm:ss}] PDA: {data.PdaName}, Code: {data.Code}");
        }

        private void OmronCamera_ClientCallback(eOmronCameraState state, string data)
        {
            gvr.CameraState = state;

            switch (state)
            {
                case eOmronCameraState.Connected:
                    MainFormVariable.listbox.Enqueue("[Camera] Đã kết nối");
                    break;
                case eOmronCameraState.Disconnected:
                    MainFormVariable.listbox.Enqueue("[Camera] Mất kết nối");
                    break;
                case eOmronCameraState.Received:
                    HandleCameraDataReceived(data);
                    break;
                case eOmronCameraState.Reconnecting:
                    MainFormVariable.listbox.Enqueue("[Camera] Đang kết nối lại...");
                    break;
            }

            _ = BroadcastDeviceStatusAsync();
        }

        private void HandleCameraDataReceived(string data)
        {
            if (gvr.AppState != e_AppState.Running) { return; }
        }

        private void omronplC_Hsl1_PLCStatus_OnChange(object sender, OmronPLC_Hsl.PLCStatusEventArgs e)
        {
            PLC_Status = e.Status;
            switch (e.Status)
            {
                case OmronPLC_Hsl.PLCStatus.Connected:
                    MainFormVariable.listbox.Enqueue("[PLC] Đã kết nối");
                    break;
                case OmronPLC_Hsl.PLCStatus.Disconnect:
                    MainFormVariable.listbox.Enqueue("[PLC] Mất kết nối");
                    break;
            }

            _ = BroadcastDeviceStatusAsync();
        }

        private void VNQRMainForm_Load(object sender, EventArgs e)
        {
            mainWK.RunWorkerAsync();
            updateWK.RunWorkerAsync();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            mainWK.CancelAsync();
            updateWK.CancelAsync();
            _ = _pdaManager.StopAsync();
            _wsServer?.Dispose();
            base.OnFormClosing(e);
        }

        private void mainWK_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!mainWK.CancellationPending)
            {
                switch (gvr.AppState)
                {
                    case e_AppState.Checking:
                        {
                            // 1. Nếu chưa có file lịch sử POHistory.db -> tạo mới -> chuyển sang Idle.
                            var lastPO = VNQR.Helpers.po.POHistoryManager.GetLastPO();
                            if (lastPO == null)
                            {
                                VNQR.Helpers.po.POHistoryManager.EnsureHistoryDB();
                                MainFormVariable.listbox.Enqueue($"[AppStart] POHistory trống, khởi tạo thành công.");
                                gvr.AppState = e_AppState.Idle;
                                break;
                            }

                            // 2. Lấy PO gần nhất đang chạy dở (Status = 'Running')
                            var runningPO = VNQR.Helpers.po.POHistoryManager.GetLastRunningPO();
                            if (runningPO != null && runningPO.Rows.Count > 0)
                            {
                                var row = runningPO.Rows[0];
                                string orderNo = row["PO"]?.ToString() ?? "";
                                string productionDate = row["ProductionDate"]?.ToString() ?? "";
                                string userName = row["UserName"]?.ToString() ?? "";
                                string startTime = row["StartTime"]?.ToString() ?? "";

                                bool poExists = VNQR.Helpers.po.POLoader.Exists(orderNo);
                                if (poExists)
                                {
                                    MainFormVariable.listbox.Enqueue($"[Resume] Phát hiện PO '{orderNo}' đang chạy dở (Start: {startTime}).");
                                    MainFormVariable.listbox.Enqueue($"[Resume] ProductionDate: {productionDate} | User: {userName}");
                                    // TODO: Load lại trạng thái PO và tiếp tục
                                    gvr.AppState = e_AppState.Idle;
                                }
                                else
                                {
                                    MainFormVariable.listbox.Enqueue($"[Warning] PO '{orderNo}' trong lịch sử không tồn tại trong PO_List. Bỏ qua.");
                                    gvr.AppState = e_AppState.Idle;
                                }
                            }
                            else
                            {
                                var lastPO2 = VNQR.Helpers.po.POHistoryManager.GetLastPO();
                                if (lastPO2 != null && lastPO2.Rows.Count > 0)
                                {
                                    var row = lastPO2.Rows[0];
                                    string orderNo = row["PO"]?.ToString() ?? "";
                                    string status = row["Status"]?.ToString() ?? "";
                                    MainFormVariable.listbox.Enqueue($"[Info] PO gần nhất: '{orderNo}' | Status: {status}");
                                }
                                gvr.AppState = e_AppState.Idle;
                            }
                            break;
                        }
                    case e_AppState.Idle:
                        gvr.AppState = e_AppState.Running;
                        break;
                    case e_AppState.Running:
                        break;
                    case e_AppState.Error:
                        break;
                    case e_AppState.NotUsed:
                        break;
                }
                Thread.Sleep(100);
            }
        }

        private void updateWK_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!updateWK.CancellationPending)
            {
                if (MainFormVariable.listbox.Count > 0)
                {
                    string data = MainFormVariable.listbox.Dequeue();
                    this.InvokeIfRequired(() =>
                    {
                        listBox1.Items.Insert(0, data);
                        if (listBox1.Items.Count > 100)
                        {
                            listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
                        }
                    });
                }
                Thread.Sleep(100);
            }
        }
    }

    public static class MainFormVariable
    {
        public static Queue<string> listbox = new Queue<string>();
    }
}
