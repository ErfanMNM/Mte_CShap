using HslCommunication;
using MTs.Auditrails;
using MTs.Datalogic;
using Sunny.UI;
using System.ComponentModel;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Utils;
using static TTManager.PLCHelpers.OmronPLC_Hsl;

namespace TApp.Views.Dashboard
{
    public partial class FDashboard : UIPage
    {
        private DatalogicCamera? _datalogicCamera_C1;

        private PLCStatus _plcStatus = PLCStatus.Disconnect;
        public FDashboard()
        {
            InitializeComponent();
        }

        public void Start()
        {
            try
            {
                InitializeConfigs();
                InitializeDevices();
                InitalizeProductionInfomation();
                InitalizeProductionDatabase();
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo Dashboard: {ex.Message}");
            }

        }

        

        private void InitializeDevices()
        {

            InitializeCameras();
            InitializePLC();
            RunBackgroundWorkers();
        }

        private void InitalizeProductionDatabase()
        {
            try
            {
                // cho batch mới
                bool existedBefore = QRDatabaseHelper.CheckAndCreateDatabaseForBatch("");
                // existedBefore = true -> bể đã có từ trước
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khởi tạo database sản xuất: {ex.Message}");
            }
        }

        private void InitalizeProductionInfomation()
        {
            try
            {
                //string dbPath = Path.Combine(
                //    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                //    "MTE",
                //    "Production",
                //    "batch_history.db"
                //);]
                string dbPath = "Database/Production/batch_history.db";

                // đảm bảo DB + table tồn tại
                BatchHistoryHelper.EnsureDatabase(dbPath);

                //lấy thông tin lô cũ nếu có
                BatchHistoryModel lastBatch = BatchHistoryHelper.GetLatest(dbPath);

                if (lastBatch is not null)
                {
                    FD_Globals.productionData.BatchCode = lastBatch.BatchCode;
                    FD_Globals.productionData.Barcode = lastBatch.Barcode;
                    
                    ipBatchNo.Text = lastBatch.BatchCode;
                    ipBarcode.Text = lastBatch.Barcode;
                }
                else
                {
                    FD_Globals.productionData.BatchCode ="NNN";
                    FD_Globals.productionData.Barcode ="000";
                    ipBatchNo.Text = "NNN";
                    ipBarcode.Text = "000";
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi lấy dữ liệu cũ: {ex.Message}");
            }
        }

        private void InitializeConfigs()
        {
            try
            {


                AppConfigs.Current.Load();
                while (string.IsNullOrEmpty(AppConfigs.Current.Line_Name))
                {
                    AppConfigs.Current.SetDefault();
                    //chờ đến khi có line name
                    Thread.Sleep(100);
                }

                PLCAddressWithGoogleSheetHelper.Init("1V2xjY6AA4URrtcwUorQE54Ud5KyI7Ev2hpDPMMcXVTI", "PLC " + AppConfigs.Current.Line_Name + "!A1:D100");

                erP_Google2.credentialPath = AppConfigs.Current.credentialERPPath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi tải cấu hình: {ex.Message}");
            }


        }

        private void InitializeCameras()
        {
            if (string.IsNullOrEmpty(AppConfigs.Current.Camera_01_IP))
                throw new InvalidOperationException("Lỗi: DA001 không có IP camera 1.");
            if (AppConfigs.Current.Camera_01_Port <= 0)
                throw new InvalidOperationException("Lỗi: DA001 không có Port camera 1.");

            try
            {
                _datalogicCamera_C1 = new DatalogicCamera(AppConfigs.Current.Camera_01_IP, AppConfigs.Current.Camera_01_Port);
                _datalogicCamera_C1.ClientCallback += DatalogicCameraC1_ClientCallback;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khởi tạo camera: {ex.Message}");
            }

        }

        private void InitializePLC()
        {
            if (AppConfigs.Current.PLC_IP is null)
                throw new InvalidOperationException("Lỗi: DA001 không có PLC ip.");
            try
            {
                omronPLC_Hsl1.PLC_IP = AppConfigs.Current.PLC_IP;
                omronPLC_Hsl1.PLC_PORT = AppConfigs.Current.PLC_Port;

                if (AppConfigs.Current.PLC_Test_Mode)
                {
                    omronPLC_Hsl1.PLC_IP = "127.0.0.1";
                    omronPLC_Hsl1.PLC_PORT = 9600;
                }

                omronPLC_Hsl1.Time_Update = AppConfigs.Current.PLC_Time_Refresh;
                omronPLC_Hsl1.PLC_Ready_DM = PLCAddressWithGoogleSheetHelper.Get("PLC_Ready_DM");

                omronPLC_Hsl1.InitPLC();

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khởi tạo PLC: {ex.Message}");
            }

        }

        private void RunBackgroundWorkers()
        {
            if (!WK_Render_HMI.IsBusy)
            {
                WK_Render_HMI.RunWorkerAsync();
            }
        }

        private void DatalogicCameraC1_ClientCallback(eDatalogicCameraState state, string data)
        {
            switch (state)
            {
                case eDatalogicCameraState.Connected:
                    if (FD_Globals.CameraStatus != CameraStatus.Connected)
                    {
                        FD_Globals.CameraStatus = CameraStatus.Connected;
                    }

                    break;
                case eDatalogicCameraState.Disconnected:
                    if (FD_Globals.CameraStatus != CameraStatus.Disconnected)
                    {
                        FD_Globals.CameraStatus = CameraStatus.Disconnected;
                    }
                    break;
                case eDatalogicCameraState.Received:

                    if (!WK_Camera.IsBusy)
                    {
                        WK_Camera.RunWorkerAsync(data);
                    }
                    else
                    {
                        // Xử lý khi worker đang bận
                    }
                    break;
                case eDatalogicCameraState.Reconnecting:
                    if (FD_Globals.CameraStatus != CameraStatus.Reconnecting)
                    {
                        FD_Globals.CameraStatus = CameraStatus.Reconnecting;
                    }
                    break;
            }
        }

        private void Camera_ProcessData(string data)
        {
            // Xử lý dữ liệu nhận được từ camera Datalogic

        }

        private void Send_Result_Content(e_Production_Status status, string data)
        {

            FD_Globals.productionData.PLC_Counter.Increment(status);

            switch (status)
            {
                case e_Production_Status.Pass:

                    break;
                case e_Production_Status.Duplicate:

                    break;
                case e_Production_Status.NotFound:

                    break;
                case e_Production_Status.Error:

                    break;
                case e_Production_Status.ReadFail:

                    break;
            }
        }

        public bool Send_Result_To_PLC(e_PLC_Result rs)
        {
            OperateResult write = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Result_DM"), (short)rs);
            return write.IsSuccess;
        }

        private void omronplC_Hsl1_PLCStatus_OnChange(object sender, PLCStatusEventArgs e)
        {
            switch (e.Status)
            {
                case PLCStatus.Connected:
                    if (_plcStatus != PLCStatus.Connected)
                    {
                        _plcStatus = PLCStatus.Connected;
                    }
                    break;
                case PLCStatus.Disconnect:
                    if (_plcStatus != PLCStatus.Disconnect)
                    {
                        _plcStatus = PLCStatus.Disconnect;
                    }
                    break;
            }
        }

        private void WK_Camera_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void WK_Render_HMI_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Render_HMI.CancellationPending)
            {
                Render_PLC_Status();
                Render_App_Status();
                Render_Camera_Status();
                Thread.Sleep(500);
            }
        }

        private void Render_App_Status()
        {
            if(FD_Globals.pLCStatus != PLCStatus.Connected || FD_Globals.CameraStatus != CameraStatus.Connected)
            {
                GlobalVarialbles.CurrentAppState = e_AppState.DeviceError;
            }
            else
            {

            }

            switch (GlobalVarialbles.CurrentAppState)
            {
                case e_AppState.Initializing:
                    break;
                case e_AppState.Ready:
                    break;
                case e_AppState.Stopped:
                    break;
                case e_AppState.Error:
                    break;
                case e_AppState.Editing:

                    this.InvokeIfRequired(() =>
                    {
                        opAppStatus.Text = "Cấu hình";
                        opAppStatus.RectColor = Color.Blue;
                        opAppStatus.ForeColor = Color.Blue;
                        opAppStatusCode.Value = 4;
                    });
                    break;
                case e_AppState.DeviceError:
                    this.InvokeIfRequired(() =>
                    {
                        opAppStatus.Text = "Lỗi TB";
                        opAppStatus.RectColor = Color.Red;
                        opAppStatus.ForeColor = Color.Red;
                        opAppStatusCode.Value = 5;
                    });
                    break;
            }
        }

        private void Render_PLC_Status()
        {
            switch (_plcStatus)
            {
                case PLCStatus.Connected:

                    if (FD_Globals.pLCStatus != PLCStatus.Connected)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            FD_Globals.pLCStatus = PLCStatus.Connected;
                            opPLCStatus.Text = "Kết Nối";
                            opPLCStatus.RectColor = Color.FromArgb(0, 192, 0);
                            opPLCStatus.ForeColor = Color.FromArgb(0, 192, 0);
                            opPLCLed.Blink = false;
                            opPLCLed.Color = Color.FromArgb(192, 255, 192);
                        });
                        
                    }
                    break;
                case PLCStatus.Disconnect:

                    if(FD_Globals.pLCStatus != PLCStatus.Disconnect)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            FD_Globals.pLCStatus = PLCStatus.Disconnect;
                            opPLCStatus.Text = "Lỗi K01";
                            opPLCStatus.RectColor = Color.Red;
                            opPLCStatus.ForeColor = Color.Red;
                            opPLCLed.Blink = true;
                            opPLCLed.Color = Color.Red;
                        });
                    }
                    
                    break;
            }
        }

        private void Render_Camera_Status()
        {
            switch (FD_Globals.CameraStatus)
            {
                case CameraStatus.Disconnected:
                    if (opCameraStatus.Text != "Lỗi K02")
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opCameraStatus.Text = "Lỗi K01";
                            opCameraStatus.RectColor = Color.Red;
                            opCameraStatus.ForeColor = Color.Red;
                            opCameraLed.Blink = true;
                            opCameraLed.Color = Color.Red;
                        });
                    }

                    break;
                case CameraStatus.Connected:
                    if (opCameraStatus.Text != "Kết Nối")
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opCameraStatus.Text = "Kết Nối";
                            opCameraStatus.RectColor = Color.FromArgb(0, 192, 0);
                            opCameraStatus.ForeColor = Color.FromArgb(0, 192, 0);
                            opCameraLed.Blink = false;
                            opCameraLed.Color = Color.FromArgb(192, 255, 192);
                        });
                    }
                    break;
                case CameraStatus.Error:
                    if (opCameraStatus.Text != "Lỗi K02")
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opCameraStatus.Text = "Lỗi K02";
                            opCameraStatus.RectColor = Color.Red;
                            opCameraStatus.ForeColor = Color.Red;
                            opCameraLed.Blink = true;
                            opCameraLed.Color = Color.Red;
                        });
                    }
                    break;
                case CameraStatus.Reconnecting:
                    if (opCameraStatus.Text != "...")
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opCameraStatus.Text = "...";
                            opCameraStatus.RectColor = Color.Yellow;
                            opCameraStatus.ForeColor = Color.Yellow;
                            opCameraLed.Blink = true;
                            opCameraLed.Color = Color.Yellow;
                        });
                    }
                    break;
            }
        }

        private bool batchChangeMode = false;
        private void btnChangeBatch_Click(object sender, EventArgs e)
        {
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username,e_LogType.UserAction,"Người dùng nhấn đổi lô","","UA-F-01");
            try
            {
                GlobalVarialbles.CurrentAppState = e_AppState.Editing;
                if (!batchChangeMode)
                {
                    btnChangeBatch.Enabled = false;
                    btnChangeBatch.Text = "Đang tải...";
                    string rs = erP_Google2.Load_Erp_to_Cbb_With_Line_Name(ipBatchNo);
                    if (rs == "OK")
                    {
                        btnChangeBatch.FillColor = Color.OrangeRed;
                        btnChangeBatch.Text = "Lưu";
                        btnChangeBatch.Enabled = true;
                        ipBatchNo.Enabled = true;
                        ipBatchNo.FillColor = Color.Yellow;
                    }
                    else
                    {
                        this.ShowErrorDialog("Tải xuống ERP thất bại, HÃY CHỤP LẠI THÔNG BÁO NÀY. Vui lòng nhấn vào mục Kiểm tra-> chọn mục B09 -> Nhấn bắt đầu và đợi một lát. Nếu A01 báo thành công nhưng hiện trống, vui lòng kiểm tra Tên line, Tên nhà máy, Tên xưởng đã đúng chưa? Nếu đã đúng hãy liên hệ người quản lý ERP hoặc IT nhà máy");

                        GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Tải lô từ ERP thất bại", "{'Thông tin lỗi':'"+rs+"'}", "ERP-F-01");

                        btnChangeBatch.FillColor = Color.FromArgb(0, 192, 0);
                        btnChangeBatch.Text = "Đổi Lô";
                        return;
                    }


                }
                else
                {

                    var result = this.ShowAskDialog("Bạn có chắc chắn thay đổi lô sản xuất?");
                    if (result)
                    {
                        try
                        {
                            btnChangeBatch.Enabled = true;
                            btnChangeBatch.FillColor = Color.FromArgb(0, 192, 0);
                            btnChangeBatch.Text = "Đổi Lô";
                            ipBatchNo.Enabled = false;
                            ipBatchNo.FillColor = Color.White;
                            FD_Globals.productionData.BatchCode = ipBatchNo.Text.Trim();
                            FD_Globals.productionData.Barcode = ipBarcode.Text;
                            //chương trình tạo dữ liệu lô mới

                            string dbPath = "Database/Production/batch_history.db";
                            BatchHistoryHelper.AddHistory(
                                dbPath,
                                FD_Globals.productionData.BatchCode,
                                FD_Globals.productionData.Barcode.ToString(),
                                GlobalVarialbles.CurrentUser.Username,
                                DateTime.Now
                            );

                            this.ShowSuccessTip("Đổi lô thành công!");

                            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Đổi lô thành công", $"{{'Lô mới':'{FD_Globals.productionData.BatchCode}','Barcode mới':'{FD_Globals.productionData.Barcode}'}}", "UA-F-02");
                        }
                        catch (Exception ex)
                        {
                            this.ShowErrorTip($"Lỗi đổi lô: {ex.Message}");
                            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi đổi lô", ex.Message, "ERR-F-01");
                        }
                    }

                    

                }
                batchChangeMode = !batchChangeMode;
                GlobalVarialbles.CurrentAppState = e_AppState.Ready;
            }
            catch (Exception ex)
            {
                btnChangeBatch.Enabled = true;
                btnChangeBatch.FillColor = Color.FromArgb(0, 192, 0);
                btnChangeBatch.Text = "Đổi Lô";
                GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi đổi lô", ex.Message, "ERP-F-02");
                this.ShowErrorDialog($"Lỗi đổi lô, bạn có thể liên hệ NCC máy theo số 0876 00 01 00: {ex.Message}");

                GlobalVarialbles.CurrentAppState = e_AppState.Ready;
            }

            
        }

        private void ipBatchNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(ipBatchNo.SelectedItem is not null)
            {
                ipBarcode.Text = erP_Google2.LoadExcelToProductList(ipBatchNo.SelectedItem.ToString(), AppConfigs.Current.production_list_path);
            }
                
        }
    }

    public static class FD_Globals
    {
        public static CameraStatus CameraStatus = CameraStatus.Disconnected;
        public static ProductionData productionData = new ProductionData();
        public static PLCStatus pLCStatus = PLCStatus.Disconnect;
    }

    public enum e_LogType
    {
        [Description("Thông tin")]
        Info = 0,

        [Description("Cảnh báo")]
        Warning = 1,

        [Description("Lỗi")]
        Error = 2,

        [Description("Debug")]
        Debug = 3,

        [Description("Hệ thống")]
        System = 4,

        [Description("Người dùng")]
        UserAction = 5,

        [Description("Thiết bị")]
        DeviceAction = 6,

        [Description("Bảo trì")]
        Maintenance = 7,

        [Description("Thay đổi dữ liệu")]
        DataChange = 8,
        
        Critical = 9
    }
}
