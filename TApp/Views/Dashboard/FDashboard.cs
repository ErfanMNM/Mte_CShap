using HslCommunication;
using MTs.Datalogic;
using Sunny.UI;
using System.Collections.Concurrent;
using System.ComponentModel;
using TApp.Configs;
using TApp.Dialogs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Models;
using TApp.Utils;
using static TTManager.PLCHelpers.OmronPLC_Hsl;

namespace TApp.Views.Dashboard
{
    public partial class FDashboard : UIPage
    {
        #region Fields
        private DatalogicCamera _datalogicCamera_C1;
        private PLCStatus _plcStatus = PLCStatus.Disconnect;
        private bool _batchChangeMode = false;
        private int _blinkAlarm = 0;
        private bool _lastDeactiveState = false;
        #endregion

        #region Events
        public event Action<int> ChangePage;
        /// <summary>
        /// Event được gọi khi trạng thái DEACTIVE từ PLC thay đổi.
        /// </summary>
        public event Action<bool> DeactiveStateChanged;
        #endregion

        #region Constructor & Page Load
        public FDashboard()
        {
            InitializeComponent();
        }

        public void Start()
        {
            InitializeConfigs();
            InitializeERP();
            InitializeDevices();
            InitializeProductionInformation();
            InitializeProductionDatabase();
            InitializeDashboardUI();
            InitializeBackgroundWorkers();
        }
        #endregion

        #region Public Methods
        public bool Send_Result_To_PLC(e_PLC_Result rs)
        {
            OperateResult write = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reject_DM"), (short)rs);
            return write.IsSuccess;
        }

        /// <summary>
        /// Kiểm tra trạng thái DEACTIVE từ PLC khi khởi động.
        /// </summary>
        public bool CheckDeactiveStateOnStartup()
        {
            try
            {
                if (omronPLC_Hsl1.plc == null || _plcStatus != PLCStatus.Connected)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync("System", e_LogType.Warning, "PLC chưa kết nối, không thể kiểm tra trạng thái DEACTIVE", "", "WARN-FDASH-DEACTIVE-01");
                    return false;
                }

                OperateResult<int> readResult = omronPLC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Deactive_DM"));
                if (readResult.IsSuccess)
                {
                    bool isDeactive = readResult.Content == 1;
                    _lastDeactiveState = isDeactive;
                    return isDeactive;
                }
                else
                {
                    GlobalVarialbles.Logger?.WriteLogAsync("System", e_LogType.Error, "Lỗi đọc PLC_Deactive_DM khi khởi động", readResult.ToMessageShowString(), "ERR-FDASH-DEACTIVE-01");
                    DisplayErrorToUI("FDE_0001", "Lỗi đọc PLC_Deactive_DM khi khởi động", readResult.ToMessageShowString());
                    return false;
                }
            }
            catch (Exception ex)
            {
                GlobalVarialbles.Logger?.WriteLogAsync("System", e_LogType.Error, "Lỗi kiểm tra trạng thái DEACTIVE khi khởi động", ex.Message, "ERR-FDASH-DEACTIVE-02");
                DisplayErrorToUI("FDE_0002", "Lỗi kiểm tra trạng thái DEACTIVE khi khởi động", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gửi lệnh DEACTIVE xuống PLC (ghi PLC_Deactive_DM = 1).
        /// </summary>
        public bool SetDeactiveState(bool isDeactive)
        {
            try
            {
                if (omronPLC_Hsl1.plc == null || _plcStatus != PLCStatus.Connected)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "PLC chưa kết nối, không thể gửi lệnh DEACTIVE", "", "ERR-FDASH-DEACTIVE-03");
                    DisplayErrorToUI("FDE_0003", "PLC chưa kết nối, không thể gửi lệnh DEACTIVE");
                    return false;
                }

                short value = (short)(isDeactive ? 1 : 0);
                OperateResult writeResult = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Deactive_DM"), value);

                if (writeResult.IsSuccess)
                {
                    // Đọc lại để xác nhận
                    Thread.Sleep(100);
                    OperateResult<int> readResult = omronPLC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Deactive_DM"));
                    if (readResult.IsSuccess && readResult.Content == value)
                    {
                        _lastDeactiveState = isDeactive;
                        return true;
                    }
                }

                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi gửi lệnh DEACTIVE xuống PLC", writeResult.ToMessageShowString(), "ERR-FDASH-DEACTIVE-04");
                DisplayErrorToUI("FDE_0004", "Lỗi gửi lệnh DEACTIVE xuống PLC", writeResult.ToMessageShowString());
                return false;
            }
            catch (Exception ex)
            {
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi gửi lệnh DEACTIVE", ex.Message, "ERR-FDASH-DEACTIVE-05");
                DisplayErrorToUI("FDE_0005", "Lỗi gửi lệnh DEACTIVE", ex.Message);
                return false;
            }
        }
        #endregion

        #region Initialization
        private void InitializeBackgroundWorkers()
        {
            if (!WK_Dequeue.IsBusy) WK_Dequeue.RunWorkerAsync();
            if (!WK_Load_Counter.IsBusy) WK_Load_Counter.RunWorkerAsync();
            if (!WK_Render_HMI.IsBusy) WK_Render_HMI.RunWorkerAsync();
        }

        private void InitializeDevices()
        {
            InitializeCameras();
            InitializePLC();
        }

        private void InitializeDashboardUI()
        {
            try
            {
                opLineName.Text = AppConfigs.Current.Line_Name;
                networkStrength1.StartMonitoring();
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo giao diện Dashboard: {ex.Message}");
            }
        }

        private void InitializeProductionDatabase()
        {
            try
            {
                QRDatabaseHelper.InitDatabases();
                FD_Globals.ActiveSet = QRDatabaseHelper.LoadActiveToHashSet();
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo database sản xuất: {ex.Message}");
            }
        }

        private void InitializeProductionInformation()
        {
            try
            {
                string dbPath = "Database/Production/batch_history.db";
                BatchHistoryHelper.EnsureDatabase(dbPath);

                BatchHistoryModel lastBatch = BatchHistoryHelper.GetLatest(dbPath);
                if (lastBatch != null)
                {
                    FD_Globals.productionData.BatchCode = lastBatch.BatchCode;
                    FD_Globals.productionData.Barcode = lastBatch.Barcode;
                    ipBatchNo.Text = lastBatch.BatchCode;
                    ipBarcode.Text = lastBatch.Barcode;
                }
                else
                {
                    FD_Globals.productionData.BatchCode = "NNN";
                    FD_Globals.productionData.Barcode = "000";
                    ipBatchNo.Text = "NNN";
                    ipBarcode.Text = "000";
                }

                LoadProductionCounters(FD_Globals.productionData.BatchCode);
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi lấy dữ liệu cũ: {ex.Message}");
            }
        }

        private void LoadProductionCounters(string batchCode)
        {
            FD_Globals.productionData.productCameraCounter.Total = QRDatabaseHelper.GetRecordCountByBatch(batchCode);
            FD_Globals.productionData.productCameraCounter.Pass = QRDatabaseHelper.GetRowCount(batchCode, e_Production_Status.Pass.ToString());
            FD_Globals.productionData.productCameraCounter.Duplicate = QRDatabaseHelper.GetRowCount(batchCode, e_Production_Status.Duplicate.ToString());
            FD_Globals.productionData.productCameraCounter.Error = QRDatabaseHelper.GetRowCount(batchCode, e_Production_Status.Error.ToString());
            FD_Globals.productionData.productCameraCounter.Timeout = QRDatabaseHelper.GetRowCount(batchCode, e_Production_Status.Timeout.ToString());
            FD_Globals.productionData.productCameraCounter.ReadFail = QRDatabaseHelper.GetRowCount(batchCode, e_Production_Status.ReadFail.ToString());
            FD_Globals.productionData.productCameraCounter.FormatError = QRDatabaseHelper.GetRowCount(batchCode, e_Production_Status.FormatError.ToString());
        }

        private void InitializeConfigs()
        {
            try
            {
                AppConfigs.Current.Load();
                while (string.IsNullOrEmpty(AppConfigs.Current.Line_Name))
                {
                    AppConfigs.Current.SetDefault();
                    Thread.Sleep(100); // Chờ đến khi có line name
                }

                PLCAddressWithGoogleSheetHelper.FilePath = AppConfigs.Current.credentialPLCAddressPath;
                PLCAddressWithGoogleSheetHelper.Init("1V2xjY6AA4URrtcwUorQE54Ud5KyI7Ev2hpDPMMcXVTI", "PLC " + AppConfigs.Current.Line_Name + "!A1:D100");

                erP_Google2.credentialPath = AppConfigs.Current.credentialERPPath;
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi tải cấu hình: {ex.Message}");
            }
        }

        private void InitializeCameras()
        {
            if (string.IsNullOrEmpty(AppConfigs.Current.Camera_01_IP)) throw new InvalidOperationException("Lỗi: DA001 không có IP camera 1.");
            if (AppConfigs.Current.Camera_01_Port <= 0) throw new InvalidOperationException("Lỗi: DA001 không có Port camera 1.");

            try
            {
                _datalogicCamera_C1 = new DatalogicCamera(AppConfigs.Current.Camera_01_IP, AppConfigs.Current.Camera_01_Port);
                _datalogicCamera_C1.ClientCallback += DatalogicCameraC1_ClientCallback;
                _datalogicCamera_C1.Connect();
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Khởi tạo camera thành công", $"{{'IP':'{AppConfigs.Current.Camera_01_IP}','Port':'{AppConfigs.Current.Camera_01_Port}'}}", "INFO-FDASH-01");
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo camera: {ex.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi khởi tạo camera", ex.Message, "ERR-FDASH-01");
            }
        }

        private void InitializePLC()
        {
            if (AppConfigs.Current.PLC_IP is null) throw new InvalidOperationException("Lỗi: DA001 không có PLC ip.");
            try
            {
                omronPLC_Hsl1.PLC_IP = AppConfigs.Current.PLC_Test_Mode ? "127.0.0.1" : AppConfigs.Current.PLC_IP;
                omronPLC_Hsl1.PLC_PORT = AppConfigs.Current.PLC_Test_Mode ? 9600 : AppConfigs.Current.PLC_Port;
                omronPLC_Hsl1.Time_Update = AppConfigs.Current.PLC_Time_Refresh;
                omronPLC_Hsl1.PLC_Ready_DM = PLCAddressWithGoogleSheetHelper.Get("PLC_Ready_DM");
                omronPLC_Hsl1.InitPLC();
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo PLC: {ex.Message}");
            }
        }

        public void InitializeERP()
        {
            erP_Google2.credentialPath = AppConfigs.Current.credentialERPPath;
            erP_Google2.SUB_INV = AppConfigs.Current.ERP_Sub_Inv;
            erP_Google2.ORG_CODE = AppConfigs.Current.ERP_Org_Code;
            erP_Google2.DatasetID = AppConfigs.Current.ERP_DatasetID;
            erP_Google2.TableID = AppConfigs.Current.ERP_TableID;
            erP_Google2.LineName = AppConfigs.Current.Line_Name;
        }
        #endregion

        #region Device Communication (Camera & PLC)
        private void DatalogicCameraC1_ClientCallback(eDatalogicCameraState state, string data)
        {
            switch (state)
            {
                case eDatalogicCameraState.Connected: HandleCameraConnection(state, "Camera kết nối thành công", "INFO-FDASH-02"); break;
                case eDatalogicCameraState.Disconnected: HandleCameraConnection(state, "Camera mất kết nối", "ERR-FDASH-02", e_LogType.Error); break;
                case eDatalogicCameraState.Reconnecting: HandleCameraConnection(state, "Camera đang kết nối lại", "WARN-FDASH-01", e_LogType.Warning); break;
                case eDatalogicCameraState.Received: HandleCameraDataReceived(data); break;
            }
        }

        private void omronPLC_Hsl1_PLCStatus_OnChange(object sender, PLCStatusEventArgs e)
        {
            if (_plcStatus == e.Status) return;

            _plcStatus = e.Status;
            string message = e.Status == PLCStatus.Connected ? "PLC kết nối thành công" : "PLC mất kết nối";
            string code = e.Status == PLCStatus.Connected ? "INFO-FDASH-03" : "ERR-FDASH-03";
            e_LogType logType = e.Status == PLCStatus.Connected ? e_LogType.Info : e_LogType.Error;
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, logType, message, "", code);
        }

        private void Camera_ProcessData(string data)
        {
            // Tách riêng trường hợp camera trả về "FAIL" (lỗi đọc)
            if (string.Equals(data, "FAIL", StringComparison.OrdinalIgnoreCase))
            {
                Send_Result_To_PLC(e_PLC_Result.Fail);
                Send_Result_Content(e_Production_Status.ReadFail, data);
                return;
            }

            if (FD_Globals.ActiveSet.Contains(data))
            {
                Send_Result_To_PLC(e_PLC_Result.Fail);
                Send_Result_Content(e_Production_Status.Duplicate, data);
                return;
            }

            if (!IsValidQRContent(data))
            {
                Send_Result_To_PLC(e_PLC_Result.Fail);
                Send_Result_Content(e_Production_Status.FormatError, data);
                return;
            }

            FD_Globals.ActiveSet.Add(data); // Update RAM
            Send_Result_To_PLC(e_PLC_Result.Pass);
            Send_Result_Content(e_Production_Status.Pass, data);

            if (AppConfigs.Current.Data_Mode == "normal")
            {
                var record = CreateQRProductRecord(data, e_Production_Status.Pass);
                FD_Globals.QueueActive.Enqueue(record);
            }
            else
            {
                // In "non-normal" mode, data is already being added to QueueRecord by Send_Result_Content
            }
        }
        #endregion

        #region Background Worker Handlers
        private void WK_Camera_DoWork(object sender, DoWorkEventArgs e)
        {
            string data = e.Argument as string ?? string.Empty;
            Camera_ProcessData(data);
        }

        private void WK_Render_HMI_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Render_HMI.CancellationPending)
            {
                Render_PLC_Status();
                Render_App_Status();
                Render_Camera_Status();
                Render_Camera_Counter();
                Render_Order_Statistics();
                Render_Production_Statistics();
                CheckDeactiveStateFromPLC();
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái DEACTIVE từ PLC và gửi event nếu có thay đổi.
        /// </summary>
        private void CheckDeactiveStateFromPLC()
        {
            try
            {
                if (omronPLC_Hsl1.plc == null || _plcStatus != PLCStatus.Connected)
                    return;

                OperateResult<int> readResult = omronPLC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Deactive_DM"));
                if (readResult.IsSuccess)
                {
                    bool currentDeactiveState = readResult.Content == 1;
                    if (currentDeactiveState != _lastDeactiveState)
                    {
                        _lastDeactiveState = currentDeactiveState;
                        DeactiveStateChanged?.Invoke(currentDeactiveState);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không làm gián đoạn luồng chính
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Debug, "Lỗi kiểm tra trạng thái DEACTIVE từ PLC", ex.Message, "ERR-FDASH-DEACTIVE-06");
                DisplayErrorToUI("FDE_0006", "Lỗi kiểm tra trạng thái DEACTIVE từ PLC", ex.Message);
            }
        }

        private void WK_Dequeue_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Dequeue.CancellationPending)
            {
                try
                {
                    ProcessQueueRecord();
                    ProcessQueueActive();
                    UpdateAlarmDisplay();
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Debug, "Lỗi ghi bản ghi sản xuất", ex.Message, "ERR-F-02");
                    DisplayErrorToUI("FDE_0007", "Lỗi ghi bản ghi sản xuất", ex.Message);
                }
            }
        }

        private void WK_Load_PLC_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Load_Counter.CancellationPending)
            {
                UpdateCountersFromPLC();
                UpdateProductionPerHour();
                Thread.Sleep(1000);
            }
        }
        #endregion

        #region UI Event Handlers
        private void btnChangeBatch_Click(object sender, EventArgs e)
        {
            btnChangeBatch.Enabled = false;
            btnChangeBatch.Text = "Đang tải...";
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Nhấn đổi lô sản xuất", $"{{'BatchCode':'{ipBatchNo.Text.Trim()}','Barcode':'{ipBarcode.Text.Trim()}'}}", "UA-F-03");

            using (var dialog = new DChangeBatch())
            {
                dialog.BatchCode = ipBatchNo.Text.Trim();
                
                dialog.Barcode = ipBarcode.Text.Trim();
                dialog.CurrentUser = GlobalVarialbles.CurrentUser;
                dialog.bt = erP_Google2.LoadExcelToProductListD(AppConfigs.Current.production_list_path);

                string loadErpResult = erP_Google2.Load_Erp_to_Cbb_With_Line_Name(dialog.ipBatch);
                ProcessChangeBatchDialog(dialog, loadErpResult);
            }
            btnChangeBatch.Enabled = true;
            btnChangeBatch.Text = "Đổi lô";
        }

        private void btnABatch_Click(object sender, EventArgs e)
        {
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng nhấn nút chỉnh sửa lô", "", "UA-F-01");
            try
            {
                if (!_batchChangeMode) HandleEnterBatchChangeMode();
                else HandleConfirmBatchChange();
                _batchChangeMode = !_batchChangeMode;
            }
            catch (Exception ex)
            {
                ResetBatchChangeMode();
                GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi đổi lô", ex.Message, "ERP-F-02");
                this.ShowErrorDialog($"Lỗi đổi lô, bạn có thể liên hệ NCC máy theo số 0876 00 01 00: {ex.Message}");
            }
        }

        private void btnResetCounterPLC_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.InvokeIfRequired(() => { btnResetCounterPLC.Enabled = false; btnResetCounterPLC.Text = "Đang gửi..."; });
                string logDetail = $"Tổng:{FD_Globals.productionData.PLC_Counter.Total}, Tốt:{FD_Globals.productionData.PLC_Counter.Pass}, Xấu:{FD_Globals.productionData.PLC_Counter.Fail}, QT:{FD_Globals.productionData.PLC_Counter.Timeout}";
                OperateResult rs = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reset_Counter_DM"), (short)1);
                if (rs.IsSuccess)
                {
                    GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng xóa số đếm", $"Xóa thành công: {logDetail}", "FD-UA-1");
                    this.ShowSuccessTip("Gửi lệnh reset counter PLC thành công!");
                }
                else
                {
                    GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng xóa số đếm", "Xóa Thất Bại Phía PLC", "FD-UA-1");
                    this.ShowErrorDialog("Gửi lệnh reset counter PLC thất bại!");
                }
                Thread.Sleep(1000);
                this.InvokeIfRequired(() => { btnResetCounterPLC.Enabled = true; btnResetCounterPLC.Text = "Xóa đếm"; });
            });
        }

        private void ipBatchNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ipBatchNo.SelectedItem != null)
            {
                ipBarcode.Text = erP_Google2.LoadExcelToProductList(ipBatchNo.SelectedItem.ToString(), AppConfigs.Current.production_list_path);
            }
        }
        private void btnClearPLC_Click(object sender, EventArgs e)
        {
            try
            {
                btnClearPLC.Enabled = false;
                btnClearPLC.Text = "Đang xóa...";

                // Gửi tín hiệu Clear xuống PLC để xóa lỗi
                OperateResult clearRs = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Clear_DM"), (short)1);
                if (!clearRs.IsSuccess)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser?.Username ?? "System",
                        e_LogType.Error,
                        "Gửi lệnh Clear PLC thất bại",
                        clearRs.ToMessageShowString(),
                        "ERR-FDASH-CLEAR-PLC"
                    );
                    this.ShowErrorDialog("Gửi lệnh xóa lỗi xuống PLC thất bại.");
                    return;
                }

                // Delay ngắn để tránh nhấn liên tục
                Thread.Sleep(500);

                // Kiểm tra nếu không có lỗi thì không cần xóa
                if (FD_Globals.AlarmCount <= 0)
                {
                    this.ShowInfoDialog("Không có lỗi FormatError nào để xóa.");
                    btnClearPLC.Enabled = true;
                    btnClearPLC.Text = "Xóa lỗi";
                    return;
                }

                // Xác nhận trước khi xóa
                if (!this.ShowAskDialog($"Bạn có chắc chắn muốn xóa {FD_Globals.AlarmCount} lỗi FormatError?\nLịch sử xóa sẽ được lưu lại."))
                {
                    btnClearPLC.Enabled = true;
                    btnClearPLC.Text = "Xóa lỗi";
                    return;
                }

                // Lưu log trước khi xóa
                int clearedCount = FD_Globals.AlarmCount;
                DateTime clearTime = DateTime.Now;
                string username = GlobalVarialbles.CurrentUser?.Username ?? "System";
                string batchCode = FD_Globals.productionData?.BatchCode ?? "N/A";
                string barcode = FD_Globals.productionData?.Barcode ?? "N/A";

                // Tạo log record
                var clearLog = new FormatErrorClearLog(clearTime, username, clearedCount, batchCode, barcode);
                FD_Globals.FormatErrorClearHistory.Add(clearLog);

                // Ghi log vào hệ thống
                string logDetail = $"{{\"ClearedCount\":{clearedCount},\"BatchCode\":\"{batchCode}\",\"Barcode\":\"{barcode}\",\"ClearTime\":\"{clearTime:yyyy-MM-dd HH:mm:ss}\"}}";
                GlobalVarialbles.Logger?.WriteLogAsync(
                    username,
                    e_LogType.UserAction,
                    $"Xóa lỗi FormatError",
                    logDetail,
                    "UA-FDASH-CLEAR-FORMAT-ERROR"
                );

                // Reset AlarmCount về 0
                FD_Globals.AlarmCount = 0;

                // Cập nhật UI
                UpdateAlarmDisplay();

                // Hiển thị thông báo thành công
                this.ShowSuccessTip($"Đã xóa {clearedCount} lỗi FormatError thành công!");

                btnClearPLC.Enabled = true;
                btnClearPLC.Text = "Xóa lỗi";
            }
            catch (Exception ex)
            {
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser?.Username ?? "System",
                    e_LogType.Error,
                    "Lỗi khi xóa FormatError",
                    ex.Message,
                    "ERR-FDASH-CLEAR-FORMAT-ERROR"
                );
                this.ShowErrorDialog($"Lỗi khi xóa FormatError: {ex.Message}");

                btnClearPLC.Enabled = true;
                btnClearPLC.Text = "Xóa lỗi";
            }
        }
        private void btnScan_Click(object sender, EventArgs e) => ChangePage?.Invoke(1003);
        private void btnPLCSetting_Click(object sender, EventArgs e) => ChangePage?.Invoke(1005);
        private void uiSymbolButton3_Click(object sender, EventArgs e) => ChangePage?.Invoke(1004);
        private void opFail_DoubleClick(object sender, EventArgs e) => this.ShowInfoDialog($"Số quá thời gian: {FD_Globals.productionData.PLC_Counter.Timeout}");
        private void opNoteCameraView_SelectedIndexChanged(object sender, EventArgs e) { /* Designer Required */ }
        #endregion

        #region UI Rendering
        private void Render_Order_Statistics() => this.InvokeIfRequired(() => { opBatchCount.Value = FD_Globals.ActiveSet.Count; opProductionSpeed.Value = FD_Globals.productionData.ProductionPerHour; });
        private void Render_Camera_Counter() => this.InvokeIfRequired(() => { opSCount.Text = $"{FD_Globals.productionData.productCameraCounter.Total} - {FD_Globals.productionData.productCameraCounter.Pass} - {FD_Globals.productionData.productCameraCounter.Fail}"; });
        private void Render_Production_Statistics() => this.InvokeIfRequired(() => { opTotalCount.Value = FD_Globals.productionData.PLC_Counter.Total; opPassCount.Value = FD_Globals.productionData.PLC_Counter.Pass; opFail.Value = FD_Globals.productionData.PLC_Counter.Fail; opTimeout.Value = FD_Globals.productionData.PLC_Counter.Timeout; });

        private void Render_App_Status()
        {
            bool deviceDisconnected = FD_Globals.pLCStatus != PLCStatus.Connected || FD_Globals.CameraStatus != CameraStatus.Connected;

            e_AppState state;
            if (_lastDeactiveState)
            {
                state = e_AppState.Deactive; // PLC yêu cầu vô hiệu hóa
            }
            else if (deviceDisconnected)
            {
                state = e_AppState.Stopped; // Mất kết nối thiết bị -> dừng máy, bộ đá vẫn hoạt động
            }
            else
            {
                state = ipBatchNo.Enabled ? e_AppState.Editing : e_AppState.Ready;
            }

            GlobalVarialbles.CurrentAppState = state;

            switch (state)
            {
                case e_AppState.Ready: SetAppStatus("Sẵn Sàng", Color.FromArgb(0, 192, 0), 1); break;
                case e_AppState.Editing: SetAppStatus("Cấu hình", Color.Blue, 4); break;
                case e_AppState.Stopped: SetAppStatus("DỪNG MÁY / MẤT KẾT NỐI", Color.OrangeRed, 2); break;
                case e_AppState.Deactive: SetAppStatus("VÔ HIỆU HÓA", Color.Red, 3); break;
                case e_AppState.Error: SetAppStatus("LỖI", Color.Red, 5); break;
            }
        }

        private void Render_PLC_Status()
        {
            if (FD_Globals.pLCStatus == _plcStatus) return;
            FD_Globals.pLCStatus = _plcStatus;
            if (_plcStatus == PLCStatus.Connected) SetDeviceStatus(opPLCStatus, opPLCLed, "Kết Nối", Color.FromArgb(0, 192, 0), false);
            else SetDeviceStatus(opPLCStatus, opPLCLed, "Lỗi K01", Color.Red, true);
        }

        private void Render_Camera_Status()
        {
            switch (FD_Globals.CameraStatus)
            {
                case CameraStatus.Disconnected: SetDeviceStatus(opCameraStatus, opCameraLed, "Lỗi K01", Color.Red, true); break;
                case CameraStatus.Connected: SetDeviceStatus(opCameraStatus, opCameraLed, "Kết Nối", Color.FromArgb(0, 192, 0), false); break;
                case CameraStatus.Error: SetDeviceStatus(opCameraStatus, opCameraLed, "Lỗi K02", Color.Red, true); break;
                case CameraStatus.Reconnecting: SetDeviceStatus(opCameraStatus, opCameraLed, "...", Color.OrangeRed, true, Color.Yellow); break;
            }
        }

        private void Render_Production_Result(QRProductRecord qRProductRecord)
        {
            this.InvokeIfRequired(() =>
            {
                opView.Items.Insert(0, $"#{qRProductRecord.ID} - {qRProductRecord.Status} - {qRProductRecord.QRContent}");
                if (opView.Items.Count >= 50) opView.Items.RemoveAt(opView.Items.Count - 1);
                opResopse.Text = qRProductRecord.QRContent;

                switch (qRProductRecord.Status)
                {
                    case e_Production_Status.Pass: SetResultStatus("TỐT", Color.Green); break;
                    case e_Production_Status.Fail: SetResultStatus("XẤU", Color.Red); break;
                    case e_Production_Status.Error: SetResultStatus("LỖI", Color.Orange); break;
                    case e_Production_Status.Duplicate: SetResultStatus("TRÙNG", Color.Yellow); break;
                    case e_Production_Status.NotFound: SetResultStatus("KHÔNG CÓ", Color.Yellow); break;
                    case e_Production_Status.Timeout: SetResultStatus("HẾT GIỜ", Color.Red); break;
                    case e_Production_Status.ReadFail: SetResultStatus("LỖI ĐỌC", Color.Red); break;
                    case e_Production_Status.FormatError: SetResultStatus("LỖI NT", Color.Red); FD_Globals.AlarmCount++; break;
                }
            });
        }
        #endregion

        #region Private Helper Methods
        private void HandleCameraConnection(eDatalogicCameraState state, string logMessage, string logCode, e_LogType logType = e_LogType.Info)
        {
            var cameraStatus = state switch
            {
                eDatalogicCameraState.Connected => CameraStatus.Connected,
                eDatalogicCameraState.Disconnected => CameraStatus.Disconnected,
                eDatalogicCameraState.Reconnecting => CameraStatus.Reconnecting,
                _ => FD_Globals.CameraStatus
            };

            if (FD_Globals.CameraStatus != cameraStatus)
            {
                FD_Globals.CameraStatus = cameraStatus;
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, logType, logMessage, "", logCode);
            }
        }

        private void HandleCameraDataReceived(string data)
        {
            // Kiểm tra nếu hệ thống đang ở chế độ DEACTIVE thì không xử lý
            if (_lastDeactiveState)
            {
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Warning,
                    "Bỏ qua dữ liệu camera - hệ thống đang ở chế độ VÔ HIỆU HÓA",
                    $"{{'QRContent':'{data}'}}",
                    "WARN-FDASH-DEACTIVE-01"
                );
                return;
            }

            if (!WK_Camera.IsBusy)
            {
                WK_Camera.RunWorkerAsync(data);
            }
            else
            {
                Send_Result_To_PLC(e_PLC_Result.Fail);
                Send_Result_Content(e_Production_Status.Timeout, data);
            }
        }

        private bool IsValidQRContent(string data) => data.Length >= 16 && data.Contains(FD_Globals.productionData.Barcode);

        private void Send_Result_Content(e_Production_Status status, string data)
        {
            FD_Globals.productionData.productCameraCounter.Increment(status);
            var record = CreateQRProductRecord(data, status);
            FD_Globals.QueueRecord.Enqueue(record);
        }

        private QRProductRecord CreateQRProductRecord(string qrContent, e_Production_Status status, string reason = "")
        {
            return new QRProductRecord
            {
                QRContent = qrContent,
                Status = status,
                BatchCode = FD_Globals.productionData.BatchCode,
                Barcode = FD_Globals.productionData.Barcode,
                UserName = GlobalVarialbles.CurrentUser.Username,
                TimeStampActive = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"),
                TimeUnixActive = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ProductionDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssK"),
                Reason = reason
            };
        }

        private void HandleEnterBatchChangeMode()
        {
            //btnABatch.Enabled = false;
            //btnABatch.Symbol = 61473; // loading
            string rs = erP_Google2.Load_Erp_to_Cbb_With_Line_Name(ipBatchNo);
            if (rs == "OK")
            {
                //btnABatch.FillColor = Color.OrangeRed;
                //btnABatch.Symbol = 61533; // confirm
                //btnABatch.Enabled = true;
                ipBatchNo.Enabled = true;
                ipBatchNo.FillColor = Color.Yellow;
            }
            else
            {
                this.ShowErrorDialog("Tải xuống ERP thất bại, HÃY CHỤP LẠI THÔNG BÁO NÀY. Vui lòng nhấn vào mục Kiểm tra-> chọn mục B09 -> Nhấn bắt đầu và đợi một lát. Nếu A01 báo thành công nhưng hiện trống, vui lòng kiểm tra Tên line, Tên nhà máy, Tên xưởng đã đúng chưa? Nếu đã đúng hãy liên hệ người quản lý ERP hoặc IT nhà máy");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Tải lô từ ERP thất bại", $"{{'Thông tin lỗi':'{rs}'}}", "ERP-F-01");
                ResetBatchChangeMode();
            }
        }

        private void HandleConfirmBatchChange()
        {
            if (!this.ShowAskDialog("Bạn có chắc chắn thay đổi lô sản xuất?")) return;

            try
            {
                ResetBatchChangeMode(false);
                FD_Globals.productionData.BatchCode = ipBatchNo.Text.Trim();
                FD_Globals.productionData.Barcode = ipBarcode.Text;

                string dbPath = "Database/Production/batch_history.db";
                BatchHistoryHelper.AddHistory(dbPath, FD_Globals.productionData.BatchCode, FD_Globals.productionData.Barcode, GlobalVarialbles.CurrentUser.Username, DateTime.Now);

                this.ShowSuccessTip("Đổi lô thành công!");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Đổi lô thành công", $"{{'Lô mới':'{FD_Globals.productionData.BatchCode}','Barcode mới':'{FD_Globals.productionData.Barcode}'}}", "UA-F-02");
            }
            catch (Exception ex)
            {
                this.ShowErrorTip($"Lỗi đổi lô: {ex.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi đổi lô", ex.Message, "ERR-F-01");
            }
        }

        private void ResetBatchChangeMode(bool resetBatchChangeVar = true)
        {
            //btnABatch.Enabled = true;
            //btnABatch.FillColor = Color.FromArgb(0, 192, 0);
            //btnABatch.Symbol = 563629; // Reset symbol
            ipBatchNo.Enabled = false;
            ipBatchNo.FillColor = Color.White;
            if (resetBatchChangeVar) _batchChangeMode = false;
        }

        private void ProcessQueueRecord()
        {
            if (FD_Globals.QueueRecord.TryDequeue(out QRProductRecord record))
            {
                QRDatabaseHelper.AddOrActivateCode(record.QRContent, record.BatchCode, record.UserName, record.Barcode, record.TimeStampActive, record.TimeUnixActive, record.TimeStampActive, record.Status);
                Render_Production_Result(record);
            }
        }

        private void ProcessQueueActive()
        {
            if (AppConfigs.Current.Data_Mode == "normal" && FD_Globals.QueueActive.TryDequeue(out QRProductRecord otherRecord))
            {
                QRDatabaseHelper.AddActiveCodeUnique(otherRecord.QRContent, otherRecord.BatchCode, otherRecord.Barcode, otherRecord.UserName, otherRecord.TimeStampActive, otherRecord.TimeUnixActive);
            }
        }

        private void UpdateCountersFromPLC()
        {
            if(GlobalVarialbles.CurrentAppState == e_AppState.Ready)
            {
                omronPLC_Hsl1.Ready = 1;
            }
            else
            {
                omronPLC_Hsl1.Ready = 0;
            }
           // GlobalVarialbles.CurrentAppState
            OperateResult<int[]> result = omronPLC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Total_Count_DM"), 5);
            if (result.IsSuccess)
            {
                FD_Globals.productionData.PLC_Counter.Total = result.Content[0];
                FD_Globals.productionData.PLC_Counter.ReadFail = result.Content[1];
                FD_Globals.productionData.PLC_Counter.Pass = result.Content[2];
                FD_Globals.productionData.PLC_Counter.Timeout = result.Content[4];
            }
            else
            {
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi đọc số liệu PLC", result.ToMessageShowString(), "PLC-F-01");
                FD_Globals.productionData.PLC_Counter.Total = -1;
                FD_Globals.productionData.PLC_Counter.ReadFail = -1;
                FD_Globals.productionData.PLC_Counter.Pass = -1;
                FD_Globals.productionData.PLC_Counter.Timeout = -1;
                DisplayErrorToUI("FDE_0008", "Lỗi đọc số liệu PLC", result.ToMessageShowString());
            }
        }

        private void UpdateProductionPerHour()
        {
            List<HourlyProduction> hourlyPassProduction = QRDatabaseHelper.GetHourlyProduction(DateTime.Now, null);
            FD_Globals.productionData.ProductionPerHour = hourlyPassProduction.FirstOrDefault(p => p.Hour == DateTime.Now.Hour)?.Count ?? 0;
        }

        private void ProcessChangeBatchDialog(DChangeBatch dialog, string loadErpResult)
        {
            if (loadErpResult == "OK")
            {
                dialog.opERPStatus.Text = "ERP OK";
                dialog.opERPStatus.FillColor = Color.FromArgb(0, 192, 0);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ipBarcode.Text = dialog.Barcode;
                    ipBatchNo.Text = dialog.BatchCode;
                    FD_Globals.productionData.BatchCode = dialog.BatchCode;
                    FD_Globals.productionData.Barcode = dialog.Barcode;
                    BatchHistoryHelper.AddHistory("Database/Production/batch_history.db", FD_Globals.productionData.BatchCode, FD_Globals.productionData.Barcode, GlobalVarialbles.CurrentUser.Username, DateTime.Now);
                    this.ShowSuccessTip("Đổi lô thành công!");
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Đổi lô thành công", $"{{'BatchCode':'{dialog.BatchCode}','Barcode':'{dialog.Barcode}'}}", "UA-F-03");
                }
                else
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Hủy đổi lô sản xuất", $"{{'BatchCode':'{dialog.BatchCode}','Barcode':'{dialog.Barcode}'}}", "UA-F-03");
                }
            }
            else
            {
                dialog.opERPStatus.Text = "ERP Lỗi";
                dialog.opERPStatus.FillColor = Color.Red;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ipBarcode.Text = dialog.Barcode;
                    ipBatchNo.Text = dialog.BatchCode;
                    FD_Globals.productionData.BatchCode = dialog.BatchCode;
                    FD_Globals.productionData.Barcode = dialog.Barcode;
                    BatchHistoryHelper.AddHistory("Database/Production/batch_history.db", FD_Globals.productionData.BatchCode, FD_Globals.productionData.Barcode, GlobalVarialbles.CurrentUser.Username, DateTime.Now);
                    this.ShowSuccessTip("Đổi lô thành công!");
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Đổi lô thành công", $"{{'BatchCode':'{dialog.BatchCode}','Barcode':'{dialog.Barcode}'}}", "UA-F-03");
                }
                else
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Hủy đổi lô sản xuất", $"{{'BatchCode':'{dialog.BatchCode}','Barcode':'{dialog.Barcode}'}}", "UA-F-03");
                }

                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Tải lô từ ERP thất bại", $"{{'Thông tin lỗi':'{loadErpResult}'}}", "ERP-F-01");
            }
        }

        private void SetAppStatus(string text, Color color, int code) => this.InvokeIfRequired(() => { opAppStatus.Text = text; opAppStatus.RectColor = color; opAppStatus.ForeColor = color; opAppStatusCode.Value = code; });
        private void SetResultStatus(string text, Color color) => this.InvokeIfRequired(() => { opResultStatus.Text = text; opResultStatus.FillColor = color; });

        private void SetDeviceStatus(UIPanel label, UILedBulb led, string text, Color color, bool blink, Color? ledColor = null)
        {
            if (label.Text != text)
            {
                this.InvokeIfRequired(() =>
                {
                    label.Text = text;
                    label.RectColor = color;
                    label.ForeColor = color;
                    led.Blink = blink;
                    led.Color = ledColor ?? (color == Color.Red ? Color.Red : Color.FromArgb(192, 255, 192));
                });
            }
        }

        private void UpdateAlarmDisplay()
        {
            if (FD_Globals.AlarmCount >= 1)
            {
                _blinkAlarm = (_blinkAlarm + 1) % 10; // Cycle blink state
                Color color = _blinkAlarm < 5 ? Color.Red : Color.Yellow;
                SetAlarm($"LỖI SAI ĐỊNH DẠNG MÃ - ĐÂY LÀ LỖI NGHIÊM TRỌNG: {FD_Globals.AlarmCount}", color, color);
            }
            else
            {
                SetAlarm(FD_Globals.AlarmCount.ToString(), Color.FromArgb(243, 249, 255), Color.FromArgb(80, 160, 255));
            }
        }

        private void SetAlarm(string text, Color fillColor, Color rectColor) => this.InvokeIfRequired(() => { opAlarm.Text = text; opAlarm.FillColor = fillColor; opAlarm.RectColor = rectColor; });

        /// <summary>
        /// Hiển thị lỗi lên dialog và ghi vào opView (uiListBox). Tag code dùng để truy vết.
        /// </summary>
        private void DisplayErrorToUI(string code, string message, string detail = "")
        {
            this.InvokeIfRequired(() =>
            {
                try
                {
                    string dialogMessage = string.IsNullOrEmpty(detail) ? $"{code}: {message}" : $"{code}: {message}\n{detail}";
                    this.ShowErrorDialog(dialogMessage);
                }
                catch
                {
                    // Bỏ qua nếu dialog fail (tránh crash UI)
                }

                opView.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] [{code}] {message}");
                if (opView.Items.Count > 200) opView.Items.RemoveAt(opView.Items.Count - 1);
            });
        }
        #endregion

        
    }

    #region Nested Types
    /// <summary>
    /// Lưu thông tin log các lần xóa lỗi FormatError
    /// </summary>
    public class FormatErrorClearLog
    {
        public DateTime ClearTime { get; set; }
        public string Username { get; set; }
        public int ClearedCount { get; set; }
        public string BatchCode { get; set; }
        public string Barcode { get; set; }

        public FormatErrorClearLog(DateTime clearTime, string username, int clearedCount, string batchCode, string barcode)
        {
            ClearTime = clearTime;
            Username = username;
            ClearedCount = clearedCount;
            BatchCode = batchCode;
            Barcode = barcode;
        }
    }

    public static class FD_Globals
    {
        public static CameraStatus CameraStatus { get; set; } = CameraStatus.Disconnected;
        public static ProductionData productionData { get; set; } = new ProductionData();
        public static int AlarmCount { get; set; } = 0;
        public static PLCStatus pLCStatus { get; set; } = PLCStatus.Disconnect;
        public static HashSet<string> ActiveSet { get; set; } = new HashSet<string>();
        public static ConcurrentQueue<QRProductRecord> QueueRecord { get; set; } = new ConcurrentQueue<QRProductRecord>();
        public static ConcurrentQueue<QRProductRecord> QueueActive { get; set; } = new ConcurrentQueue<QRProductRecord>();
        /// <summary>
        /// Lịch sử các lần xóa lỗi FormatError
        /// </summary>
        public static List<FormatErrorClearLog> FormatErrorClearHistory { get; set; } = new List<FormatErrorClearLog>();
    }
    #endregion
}
