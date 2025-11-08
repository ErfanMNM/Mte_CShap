using HslCommunication;
using MTs.Datalogic;
using Sunny.UI;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
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

        private void WK_Camera_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

        }

        private void WK_Render_HMI_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!WK_Render_HMI.CancellationPending)
            {
                Render_PLC_Status();
                Thread.Sleep(500);
            }
        }

        private void Render_PLC_Status()
        {
            switch (_plcStatus)
            {
                case PLCStatus.Connected:

                    opPLCStatus.Text = "Kết Nối";
                    opPLCStatus.RectColor = Color.FromArgb(0, 192, 0);
                    opPLCStatus.ForeColor = Color.FromArgb(0, 192, 0);

                    opPLCLed.Blink = false;
                    opPLCLed.Color = Color.FromArgb(192, 255, 192);
                    break;
                case PLCStatus.Disconnect:

                    opPLCStatus.Text = "Lỗi K01";
                    opPLCStatus.RectColor = Color.Red;
                    opPLCStatus.ForeColor = Color.Red;

                    opPLCLed.Blink = true;
                    opPLCLed.Color = Color.Red;
                    break;
            }
        }

        private bool batchChangeMode = false;
        private void btnChangeBatch_Click(object sender, EventArgs e)
        {
            
            if (!batchChangeMode)
            {
                btnChangeBatch.FillColor = Color.OrangeRed;
                btnChangeBatch.Text = "Lưu";
                ipBatchNo.Enabled = true;
                ipBatchNo.FillColor = Color.Yellow;
                erP_Google1.Load_Erp_to_Cbb_With_Line_Name(ipBatchNo);
            }
            else
            {
                btnChangeBatch.FillColor = Color.FromArgb(0, 192, 0);
                btnChangeBatch.Text = "Đổi Lô";
                ipBatchNo.Enabled = false;
                ipBatchNo.FillColor = Color.White;
                FD_Globals.productionData.BatchCode = ipBatchNo.Text.Trim();

            }

            batchChangeMode = !batchChangeMode;
        }
    }

    public static class FD_Globals
    {
        public static CameraStatus CameraStatus = CameraStatus.Disconnected;
        public static ProductionData productionData = new ProductionData();
    }
}
