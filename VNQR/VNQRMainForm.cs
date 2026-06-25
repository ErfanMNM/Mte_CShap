using Sunny.UI;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTManager.Communication.WebSocket;
using TTManager.Internet;
using TTManager.Omron;
using TTManager.PDA;
using TTManager.PLCHelpers;
using VNQR.Configs;
using VNQR.Helpers;
using VNQR.Infrastructure;
using VNQR.Utils;

namespace VNQR
{
    public partial class VNQRMainForm : UIForm
    {
        #region Fields
        private OmronCamera? _camera_Active;
        private OmronCamera? _camera_Package;
        private OmronPLC_Hsl.PLCStatus? _plcStatus = OmronPLC_Hsl.PLCStatus.Disconnect;
        private readonly PdaScanManager _pdaManager = new();
        private readonly POApiServer _poApiServer;
        private readonly int _poApiPort = 9999;
        private readonly WebSocketServerHelper? _wsServer;
        private readonly int _wsPort = 8080;
        private CancellationTokenSource? _productionCTS;
        private Thread? _productionThread;
        #endregion

        public VNQRMainForm()
        {
            InitializeComponent();

            _wsServer = new WebSocketServerHelper(_wsPort, "/ws");
            _wsServer.WebSocketServerCallback += WsServer_Callback;
            _wsServer.WebSocketServerCallback += (state, data) =>
            {
                if (state == WebSocketServerState.Listening || state == WebSocketServerState.Connected)
                    _ = BroadcastDeviceStatusAsync();
            };
            _wsServer.Start();

            AppConfigs.Current.SetDefault();

            _camera_Active = new OmronCamera(OmronCamera.e_CameraModel.V430, AppConfigs.Current.Camera_Active_IP, AppConfigs.Current.Camera_Active_Port);
            _camera_Active.ClientCallback += Camera_Active_Callback;
            _camera_Active.Connect();

            _camera_Package = new OmronCamera(OmronCamera.e_CameraModel.V430, AppConfigs.Current.Camera_Package_IP, AppConfigs.Current.Camera_Package_Port);
            _camera_Package.ClientCallback += Camera_Package_Callback;
            _camera_Package.Connect();

            omronplC_Hsl.PLC_IP = "127.0.0.1";
            omronplC_Hsl.PLC_PORT = 9600;
            omronplC_Hsl.PLCStatus_OnChange += omronplC_Hsl_PLCStatus_OnChange;
            omronplC_Hsl.InitPLC();

            _pdaManager.OnScanReceived += Pda_ScanCallback;
            _ = StartPdaServerSafely();

            _poApiServer = new POApiServer(_poApiPort, "0.0.0.0", null, (src, msg) => MainFormVariable.listbox.Enqueue($"[{src}] {msg}"));
            _ = StartPOApiServerSafely();

            // Setup Production Manager
            ProductionManager.OnLog += (msg) => MainFormVariable.listbox.Enqueue(msg);
            ProductionManager.OnStateChanged += () => this.InvokeIfRequired(() => UpdateProductionUI());
            ProductionManager.OnCountersUpdated += () => this.InvokeIfRequired(() => UpdateCounterDisplay());

            MainTabControl = uiTabControl1;
            uiNavMenu1.TabControl = uiTabControl1;
        }

        private void WsServer_Callback(WebSocketServerState state, string data)
        {
            MainFormVariable.listbox.Enqueue($"[WS] {state}: {data}");
        }

        private async Task BroadcastDeviceStatusAsync()
        {
            if (_wsServer == null || _wsServer.ClientCount == 0) return;

            int networkStrength = NetworkStrengthHelper.GetNetworkStrength();

            var status = new
            {
                type = "device_status",
                timestamp = DateTime.Now,
                camera = new
                {
                    active = new
                    {
                        state = GV.CameraState_Active.ToString(),
                        ip = AppConfigs.Current.Camera_Active_IP
                    },
                    package = new
                    {
                        state = GV.CameraState_Package.ToString(),
                        ip = AppConfigs.Current.Camera_Package_IP
                    }
                },
                plc = new
                {
                    state = _plcStatus?.ToString() ?? "Unknown",
                    connected = _plcStatus == OmronPLC_Hsl.PLCStatus.Connected
                },
                app = new
                {
                    state = GV.AppState.ToString()
                },
                production = new
                {
                    orderNo = GV.OrderNo,
                    productName = GV.ProductName,
                    orderQty = GV.OrderQty,
                    totalCount = GV.TotalCount,
                    passCount = GV.PassCount,
                    failCount = GV.FailCount,
                    duplicateCount = GV.DuplicateCount,
                    cartonCount = GV.CartonCount,
                    cartonClosedCount = GV.CartonClosedCount,
                    currentCartonId = GV.CurrentCartonId,
                    currentCartonCode = GV.CurrentCartonCode,
                    itemsInCarton = GV.ItemsInCurrentCarton
                },
                networkStrength
            };

            string json = JsonSerializer.Serialize(status);
            await _wsServer.BroadcastAsync(json);
        }

        private async Task StartPdaServerSafely()
        {
            try
            {
                await _pdaManager.StartAsync();
                MainFormVariable.listbox.Enqueue("[PDA] Server started on port 6969.");
            }
            catch (Exception ex)
            {
                MainFormVariable.listbox.Enqueue($"[PDA] Server failed: {ex.Message}");
            }
        }

        private async Task StartPOApiServerSafely()
        {
            try
            {
                await _poApiServer.StartAsync();
                MainFormVariable.listbox.Enqueue($"[POAPI] REST API started on port {_poApiPort}.");
            }
            catch (Exception ex)
            {
                MainFormVariable.listbox.Enqueue($"[POAPI] Server failed: {ex.Message}");
            }
        }

        private void Pda_ScanCallback(ScanData data)
        {
            MainFormVariable.listbox.Enqueue($"[{data.Time:HH:mm:ss}] PDA: {data.PdaName}, Code: {data.Code}");

            // Xử lý PDA scan như Camera Active
            if (GV.AppState == e_ProductionState.Running)
            {
                var result = ProductionManager.ProcessCameraActiveCode(data.Code);
                if (!result.success)
                {
                    MainFormVariable.listbox.Enqueue($"[PDA] Lỗi: {result.message}");
                }
            }
        }

        private void Camera_Active_Callback(eOmronCameraState state, string data)
        {
            GV.CameraState_Active = state;

            switch (state)
            {
                case eOmronCameraState.Connected:
                    MainFormVariable.listbox.Enqueue("[Camera Active] Da ket noi");
                    break;
                case eOmronCameraState.Disconnected:
                    MainFormVariable.listbox.Enqueue("[Camera Active] Mat ket noi");
                    break;
                case eOmronCameraState.Received:
                    ProcessCameraActiveData(data);
                    break;
                case eOmronCameraState.Reconnecting:
                    MainFormVariable.listbox.Enqueue("[Camera Active] Dang ket noi lai...");
                    break;
            }

            _ = BroadcastDeviceStatusAsync();
        }

        private void ProcessCameraActiveData(string data)
        {
            if (GV.AppState != e_ProductionState.Running) return;

            MainFormVariable.listbox.Enqueue($"[CamActive] Data: {data}");

            var result = ProductionManager.ProcessCameraActiveCode(data);

            if (result.success)
            {
                // Gui PLC OK
                SendPLCResult(true);
            }
            else
            {
                // Gui PLC FAIL
                SendPLCResult(false);
                MainFormVariable.listbox.Enqueue($"[CamActive] Loi: {result.message}");
            }
        }

        private void Camera_Package_Callback(eOmronCameraState state, string data)
        {
            GV.CameraState_Package = state;

            switch (state)
            {
                case eOmronCameraState.Connected:
                    MainFormVariable.listbox.Enqueue("[Camera Package] Da ket noi");
                    break;
                case eOmronCameraState.Disconnected:
                    MainFormVariable.listbox.Enqueue("[Camera Package] Mat ket noi");
                    break;
                case eOmronCameraState.Received:
                    ProcessCameraPackageData(data);
                    break;
                case eOmronCameraState.Reconnecting:
                    MainFormVariable.listbox.Enqueue("[Camera Package] Dang ket noi lai...");
                    break;
            }

            _ = BroadcastDeviceStatusAsync();
        }

        private void ProcessCameraPackageData(string data)
        {
            if (GV.AppState != e_ProductionState.Running) return;

            MainFormVariable.listbox.Enqueue($"[CamPackage] Data: {data}");

            var result = ProductionManager.ProcessCameraPackageCode(data);

            if (!result.success)
            {
                MainFormVariable.listbox.Enqueue($"[CamPackage] Loi: {result.message}");
            }
        }

        private void SendPLCResult(bool ok)
        {
            try
            {
                // TODO: Implement PLC communication
                // Ví dụ: omronplC_Hsl.plc.Write("D100", ok ? 1 : 0);
            }
            catch (Exception ex)
            {
                MainFormVariable.listbox.Enqueue($"[PLC] Loi gui: {ex.Message}");
            }
        }

        private void omronplC_Hsl_PLCStatus_OnChange(object sender, OmronPLC_Hsl.PLCStatusEventArgs e)
        {
            _plcStatus = e.Status;
            switch (e.Status)
            {
                case OmronPLC_Hsl.PLCStatus.Connected:
                    MainFormVariable.listbox.Enqueue("[PLC] Da ket noi");
                    break;
                case OmronPLC_Hsl.PLCStatus.Disconnect:
                    MainFormVariable.listbox.Enqueue("[PLC] Mat ket noi");
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
            _productionCTS?.Cancel();
            _productionThread?.Join(1000);
            _ = _pdaManager.StopAsync();
            ProductionInfo.Unload();
            _poApiServer.Dispose();
            _wsServer?.Dispose();

            // Unsubscribe events
            omronplC_Hsl.PLCStatus_OnChange -= omronplC_Hsl_PLCStatus_OnChange;

            Environment.Exit(0);
            base.OnFormClosing(e);
        }

        private void mainWK_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!mainWK.CancellationPending)
            {
                switch (GV.AppState)
                {
                    case e_ProductionState.Checking:
                        {
                            var lastPO = po.POHistoryManager.GetLastPO();
                            if (lastPO == null)
                            {
                                po.POHistoryManager.EnsureHistoryDB();
                                MainFormVariable.listbox.Enqueue("[AppStart] POHistory trong, khoi tao thanh cong.");
                                GV.AppState = e_ProductionState.NoSelectedPO;
                                break;
                            }

                            var runningPO = po.POHistoryManager.GetLastRunningPO();
                            if (runningPO != null && runningPO.Rows.Count > 0)
                            {
                                var row = runningPO.Rows[0];
                                string orderNo = row["PO"]?.ToString() ?? "";

                                bool poExists = po.POLoader.Exists(orderNo);
                                if (poExists)
                                {
                                    MainFormVariable.listbox.Enqueue($"[Resume] Phat hien PO '{orderNo}' dang chay.");
                                    // TODO: Resume production
                                    GV.AppState = e_ProductionState.Running;
                                }
                                else
                                {
                                    MainFormVariable.listbox.Enqueue($"[Warning] PO '{orderNo}' khong ton tai.");
                                    GV.AppState = e_ProductionState.NoSelectedPO;
                                }
                            }
                            else
                            {
                                GV.AppState = e_ProductionState.NoSelectedPO;
                            }
                            break;
                        }
                    case e_ProductionState.NoSelectedPO:
                        // Cho phep nguoi dung chon PO
                        break;
                    case e_ProductionState.Running:
                        // San xuat dang chay
                        break;
                }
                Thread.Sleep(100);
            }
        }

        private void updateWK_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!updateWK.CancellationPending)
            {
                // Update status labels
                this.InvokeIfRequired(() => UpdateUI());

                // Process log queue
                while (MainFormVariable.listbox.TryDequeue(out string? data))
                {
                    this.InvokeIfRequired(() =>
                    {
                        listBox1.Items.Insert(0, data);
                        if (listBox1.Items.Count > 100)
                            listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
                    });
                }

                Thread.Sleep(100);
            }
        }

        private void UpdateUI()
        {
            // Update status
            opAppStatus.Text = GV.AppState.ToString();
            opPLCStatus.Text = _plcStatus?.ToString() ?? "Unknown";
        }

        private void UpdateProductionUI()
        {
            // Production info labels will be updated when the form is fully initialized
            // Controls like opOrderNo, opProductName are defined in Designer
        }

        private void UpdateCounterDisplay()
        {
            // Counter labels will be updated when the form is fully initialized
            // Controls like opTotal, opPass, opFail, opDuplicate, opCarton are defined in Designer
        }


    }

    public static class MainFormVariable
    {
        public static ConcurrentQueue<string> listbox = new ConcurrentQueue<string>();
    }
}
