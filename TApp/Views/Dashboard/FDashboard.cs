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
        public event Action<int> ChangePage;
        private DatalogicCamera? _datalogicCamera_C1;
        private PLCStatus _plcStatus = PLCStatus.Disconnect;
        private bool _batchChangeMode = false;
        private int _blinkAlarm = 0;

        #endregion

        #region Constructor

        public FDashboard()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Methods

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

        public bool Send_Result_To_PLC(e_PLC_Result rs)
        {
            OperateResult write = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reject_DM"), (short)rs);
            return write.IsSuccess;
        }

        #endregion

        #region Initialization Methods

        private void InitializeBackgroundWorkers()
        {
            if (!WK_Dequeue.IsBusy)
            {
                WK_Dequeue.RunWorkerAsync();
            }

            if (!WK_Load_Counter.IsBusy)
            {
                WK_Load_Counter.RunWorkerAsync();
            }

            if (!WK_Render_HMI.IsBusy)
            {
                WK_Render_HMI.RunWorkerAsync();
            }
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
                // Cho batch mới
                QRDatabaseHelper.InitDatabases();

                // Sau đó load HashSet
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

                // Đảm bảo DB + table tồn tại
                BatchHistoryHelper.EnsureDatabase(dbPath);

                // Lấy thông tin lô cũ nếu có
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

                // Lấy counter theo lô 
                FD_Globals.productionData.productCameraCounter.Total = QRDatabaseHelper.GetRecordCountByBatch(FD_Globals.productionData.BatchCode);
                FD_Globals.productionData.productCameraCounter.Pass = QRDatabaseHelper.GetRowCount(FD_Globals.productionData.BatchCode, e_Production_Status.Pass.ToString());
                FD_Globals.productionData.productCameraCounter.Duplicate = QRDatabaseHelper.GetRowCount(FD_Globals.productionData.BatchCode, e_Production_Status.Duplicate.ToString());
                FD_Globals.productionData.productCameraCounter.Error = QRDatabaseHelper.GetRowCount(FD_Globals.productionData.BatchCode, e_Production_Status.Error.ToString());
                FD_Globals.productionData.productCameraCounter.Timeout = QRDatabaseHelper.GetRowCount(FD_Globals.productionData.BatchCode, e_Production_Status.Timeout.ToString());
                FD_Globals.productionData.productCameraCounter.ReadFail = QRDatabaseHelper.GetRowCount(FD_Globals.productionData.BatchCode, e_Production_Status.ReadFail.ToString());
                FD_Globals.productionData.productCameraCounter.FormatError = QRDatabaseHelper.GetRowCount(FD_Globals.productionData.BatchCode, e_Production_Status.FormatError.ToString());
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi lấy dữ liệu cũ: {ex.Message}");
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
                    // Chờ đến khi có line name
                    Thread.Sleep(100);
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
            if (string.IsNullOrEmpty(AppConfigs.Current.Camera_01_IP))
                throw new InvalidOperationException("Lỗi: DA001 không có IP camera 1.");
            if (AppConfigs.Current.Camera_01_Port <= 0)
                throw new InvalidOperationException("Lỗi: DA001 không có Port camera 1.");

            try
            {
                _datalogicCamera_C1 = new DatalogicCamera(AppConfigs.Current.Camera_01_IP, AppConfigs.Current.Camera_01_Port);
                _datalogicCamera_C1.ClientCallback += DatalogicCameraC1_ClientCallback;
                _datalogicCamera_C1.Connect();

                // Ghi log khởi tạo camera
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Info,
                    "Khởi tạo camera thành công",
                    $"{{'IP':'{AppConfigs.Current.Camera_01_IP}','Port':'{AppConfigs.Current.Camera_01_Port}'}}",
                    "INFO-FDASH-01"
                );
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo camera: {ex.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Error,
                    "Lỗi khởi tạo camera",
                    ex.Message,
                    "ERR-FDASH-01"
                );
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

        #region Event Handlers

        private void DatalogicCameraC1_ClientCallback(eDatalogicCameraState state, string data)
        {
            switch (state)
            {
                case eDatalogicCameraState.Connected:
                    if (FD_Globals.CameraStatus != CameraStatus.Connected)
                    {
                        FD_Globals.CameraStatus = CameraStatus.Connected;
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Info,
                            "Camera kết nối thành công",
                            "",
                            "INFO-FDASH-02"
                        );
                    }
                    break;

                case eDatalogicCameraState.Disconnected:
                    if (FD_Globals.CameraStatus != CameraStatus.Disconnected)
                    {
                        FD_Globals.CameraStatus = CameraStatus.Disconnected;
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Error,
                            "Camera mất kết nối",
                            "",
                            "ERR-FDASH-02"
                        );
                    }
                    break;

                case eDatalogicCameraState.Received:
                    if (!WK_Camera.IsBusy)
                    {
                        WK_Camera.RunWorkerAsync(data);
                    }
                    else
                    {
                        // Xử lý khi worker đang bận, gửi fail về PLC
                        Send_Result_To_PLC(e_PLC_Result.Fail);
                        Send_Result_Content(e_Production_Status.Timeout, data);
                    }
                    break;

                case eDatalogicCameraState.Reconnecting:
                    if (FD_Globals.CameraStatus != CameraStatus.Reconnecting)
                    {
                        FD_Globals.CameraStatus = CameraStatus.Reconnecting;
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Warning,
                            "Camera đang kết nối lại",
                            "",
                            "WARN-FDASH-01"
                        );
                    }
                    break;
            }
        }

        private void omronPLC_Hsl1_PLCStatus_OnChange(object sender, PLCStatusEventArgs e)
        {
            if (_plcStatus != e.Status)
            {
                _plcStatus = e.Status;

                // Ghi log thay đổi trạng thái PLC
                if (e.Status == PLCStatus.Connected)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Info,
                        "PLC kết nối thành công",
                        "",
                        "INFO-FDASH-03"
                    );
                }
                else if (e.Status == PLCStatus.Disconnect)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "PLC mất kết nối",
                        "",
                        "ERR-FDASH-03"
                    );
                }
            }
        }

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
                Thread.Sleep(500);
            }
        }

        private void WK_Dequeue_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Dequeue.CancellationPending)
            {
                try
                {
                    if (FD_Globals.QueueRecord.Count > 0)
                    {
                        if (FD_Globals.QueueRecord.TryDequeue(out QRProductRecord record))
                        {
                            QRDatabaseHelper.AddOrActivateCode(
                                qrContent: record.QRContent,
                                batchCode: record.BatchCode,
                                userName: record.UserName,
                                barcode: record.Barcode,
                                TimeStampActive: record.TimeStampActive,
                                TimeUnixActive: record.TimeUnixActive,
                                productionDateTime: record.TimeStampActive,
                                status: record.Status
                            );
                            Render_Production_Result(record);

                        }
                    }


                    if (AppConfigs.Current.Data_Mode == "normal")
                    {

                        if (FD_Globals.QueueActive.Count > 0)
                        {
                            if (FD_Globals.QueueActive.TryDequeue(out QRProductRecord otherRecord))
                            {
                                QRDatabaseHelper.AddActiveCodeUnique(
                                    qrContent: otherRecord.QRContent,
                                    batchCode: otherRecord.BatchCode,
                                    barcode: otherRecord.Barcode,
                                    userName: otherRecord.UserName,
                                    TimeStampActive: otherRecord.TimeStampActive,
                                    TimeUnixActive: otherRecord.TimeUnixActive
                                );
                            }
                        }


                    }
                    UpdateAlarmDisplay();
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Debug, "Lỗi ghi bản ghi sản xuất", ex.Message, "ERR-F-02");
                }
            }
        }

        private void WK_Load_PLC_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Load_Counter.CancellationPending)
            {
                OperateResult<int[]> result = omronPLC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Total_Count_DM"), 5);
                if (result.IsSuccess)
                {
                    FD_Globals.productionData.PLC_Counter.Total = result.Content[0];
                    FD_Globals.productionData.PLC_Counter.ReadFail = result.Content[1];
                    FD_Globals.productionData.PLC_Counter.Pass = result.Content[2];
                    FD_Globals.productionData.PLC_Counter.Timeout = result.Content[3];
                }
                else
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi đọc số liệu PLC", result.ToMessageShowString(), "PLC-F-01");
                    FD_Globals.productionData.PLC_Counter.Total = -1;
                    FD_Globals.productionData.PLC_Counter.ReadFail = -1;
                    FD_Globals.productionData.PLC_Counter.Pass = -1;
                    FD_Globals.productionData.PLC_Counter.Timeout = -1;
                }

                List<HourlyProduction> hourlyPassProduction = QRDatabaseHelper.GetHourlyProduction(DateTime.Now, null);
                FD_Globals.ProductionPerHour = hourlyPassProduction.Where(p => p.Hour == DateTime.Now.Hour).Select(p => p.Count).FirstOrDefault();

                Thread.Sleep(1000);
            }
        }

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
                if (loadErpResult == "OK")
                {
                    btnChangeBatch.Enabled = true;
                    btnChangeBatch.Text = "Đổi lô";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        ipBarcode.Text = dialog.Barcode;
                        ipBatchNo.Text = dialog.BatchCode;

                        FD_Globals.productionData.BatchCode = dialog.BatchCode;
                        FD_Globals.productionData.Barcode = dialog.Barcode;

                        string dbPath = "Database/Production/batch_history.db";
                        BatchHistoryHelper.AddHistory(dbPath, FD_Globals.productionData.BatchCode, FD_Globals.productionData.Barcode, GlobalVarialbles.CurrentUser.Username, DateTime.Now);

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
                    btnChangeBatch.Enabled = true;
                    btnChangeBatch.Text = "Đổi lô";
                    this.ShowErrorDialog("Tải xuống ERP thất bại, HÃY CHỤP LẠI THÔNG BÁO NÀY. Vui lòng nhấn vào mục Kiểm tra-> chọn mục B09 -> Nhấn bắt đầu và đợi một lát. Nếu A01 báo thành công nhưng hiện trống, vui lòng kiểm tra Tên line, Tên nhà máy, Tên xưởng đã đúng chưa? Nếu đã đúng hãy liên hệ người quản lý ERP hoặc IT nhà máy");
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Tải lô từ ERP thất bại", $"{{'Thông tin lỗi':'{loadErpResult}'}}", "ERP-F-01");
                }
            }
        }

        private void btnABatch_Click(object sender, EventArgs e)
        {
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng nhấn nút chỉnh sửa lô", "", "UA-F-01");

            try
            {
                if (!_batchChangeMode) // Chế độ: Bắt đầu chỉnh sửa
                {
                    HandleEnterBatchChangeMode();
                }
                else // Chế độ: Xác nhận thay đổi
                {
                    HandleConfirmBatchChange();
                }

                _batchChangeMode = !_batchChangeMode;
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

        private void btnResetCounterPLC_Click(object sender, EventArgs e)
        {
            //ghi log 

            Task.Run(() =>
            {
                this.InvokeIfRequired(() =>
                {
                    btnResetCounterPLC.Enabled = false;
                    btnResetCounterPLC.Text = "Đang gửi...";
                });

                string total = FD_Globals.productionData.PLC_Counter.Total.ToString();
                string pass = FD_Globals.productionData.PLC_Counter.Pass.ToString();
                string fail = FD_Globals.productionData.PLC_Counter.Fail.ToString();
                string timeout = FD_Globals.productionData.PLC_Counter.Timeout.ToString();
                OperateResult rs = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reset_Counter_DM"), (short)1);
                if (rs.IsSuccess)
                {
                    GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng xóa só đếm", $"Xóa thành công :{total},{pass},{fail},{timeout}", "FD-UA-1");
                    this.ShowSuccessTip("Gửi lệnh reset counter PLC thành công!");
                }
                else
                {
                    GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng xóa số đếm", $"Xóa Thất Bại Phía PLC", "FD-UA-1");
                    this.ShowErrorDialog("Gửi lệnh reset counter PLC thất bại!");
                }

                Thread.Sleep(1000);

                this.InvokeIfRequired(() =>
                {
                    btnResetCounterPLC.Enabled = true;
                    btnResetCounterPLC.Text = "Xóa đếm";
                });
            });
        }

        private void ipBatchNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ipBatchNo.SelectedItem != null)
            {
                ipBarcode.Text = erP_Google2.LoadExcelToProductList(ipBatchNo.SelectedItem.ToString(), AppConfigs.Current.production_list_path);
            }
        }

        private void opFail_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInfoDialog($"Số quá thời gian: {FD_Globals.productionData.PLC_Counter.Timeout}");
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            ChangePage?.Invoke(1003);
        }

        private void btnClearPLC_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.InvokeIfRequired(() =>
                {
                    btnClearPLC.Enabled = false;
                    btnClearPLC.Text = "Đang gửi...";
                });

                OperateResult rs = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Clear_DM"), (short)1);

                if (rs.IsSuccess)
                {
                    GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng xóa dữ liệu PLC", $"Xóa thành công", "FD-UA-2");
                    this.ShowSuccessTip("Gửi lệnh xóa dữ liệu PLC thành công!");
                }
                else
                {
                    GlobalVarialbles.Logger.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng xóa dữ liệu PLC", $"Xóa Thất Bại Phía PLC", "FD-UA-2");
                    this.ShowErrorDialog("Gửi lệnh xóa dữ liệu PLC thất bại!");
                }

                Thread.Sleep(1000);
                this.InvokeIfRequired(() =>
                {
                    btnClearPLC.Enabled = true;
                    btnClearPLC.Text = "Xóa lỗi";
                });

            });
        }

        private void opNoteCameraView_SelectedIndexChanged(object sender, EventArgs e) { /* Do nothing */ }

        #endregion

        #region Private Helper Methods

        private void Camera_ProcessData(string data)
        {
            // Kiểm tra mã đã active chưa
            if (FD_Globals.ActiveSet.Contains(data))
            {
                Send_Result_To_PLC(e_PLC_Result.Fail);
                Send_Result_Content(e_Production_Status.Duplicate, data); // Duplicate
            }
            else
            {
                // Kiểm tra đúng cấu trúc mã
                if (!IsValidQRContent(data))
                {
                    Send_Result_To_PLC(e_PLC_Result.Fail);
                    Send_Result_Content(e_Production_Status.FormatError, data); // Mã không đúng cấu trúc
                    return;
                }

                //nếu datamode là normal thì lưu vào queue để ghi db
                if (AppConfigs.Current.Data_Mode == "normal")
                {
                    FD_Globals.ActiveSet.Add(data); // Update RAM
                    Send_Result_To_PLC(e_PLC_Result.Pass);
                    Send_Result_Content(e_Production_Status.Pass, data); // Thành công

                    FD_Globals.QueueActive.Enqueue(new QRProductRecord
                    {
                        QRContent = data,
                        Status = e_Production_Status.Pass,
                        BatchCode = FD_Globals.productionData.BatchCode,
                        Barcode = FD_Globals.productionData.Barcode,
                        UserName = GlobalVarialbles.CurrentUser.Username,
                        TimeStampActive = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"),
                        TimeUnixActive = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                        ProductionDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssK"),
                        Reason = string.Empty
                    });

                }
                else
                {
                    // Ghi vào DB active + unique
                    bool saved = QRDatabaseHelper.AddActiveCodeUnique(
                        qrContent: data,
                        batchCode: FD_Globals.productionData.BatchCode,
                        barcode: FD_Globals.productionData.Barcode,
                        userName: GlobalVarialbles.CurrentUser.Username,
                        TimeStampActive: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"),
                        TimeUnixActive: DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    );

                    if (saved)
                    {
                        FD_Globals.ActiveSet.Add(data); // Update RAM
                        Send_Result_To_PLC(e_PLC_Result.Pass);
                        Send_Result_Content(e_Production_Status.Pass, data); // Thành công
                    }
                    else
                    {
                        Send_Result_To_PLC(e_PLC_Result.Fail);
                        Send_Result_Content(e_Production_Status.Error, data); // Lỗi lưu dữ liệu
                    }
                }



            }
        }

        public bool IsValidQRContent(string data)
        {
            // Mã chứa barcode của sản phẩm và lớn hơn 15 ký tự

            if (data.Length < 16)
                return false;
            return data.Contains(FD_Globals.productionData.Barcode);
        }

        private void Send_Result_Content(e_Production_Status status, string data)
        {
            FD_Globals.productionData.productCameraCounter.Increment(status);

            FD_Globals.QueueRecord.Enqueue(new QRProductRecord
            {
                QRContent = data,
                Status = status,
                BatchCode = FD_Globals.productionData.BatchCode,
                Barcode = FD_Globals.productionData.Barcode,
                UserName = GlobalVarialbles.CurrentUser.Username,
                TimeStampActive = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"),
                TimeUnixActive = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ProductionDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssK"),
                Reason = string.Empty
            });
        }

        private void HandleEnterBatchChangeMode()
        {
            btnABatch.Enabled = false;
            btnABatch.Symbol = 61473; // Symbol "loading"

            string rs = erP_Google2.Load_Erp_to_Cbb_With_Line_Name(ipBatchNo);
            if (rs == "OK")
            {
                btnABatch.FillColor = Color.OrangeRed;
                btnABatch.Symbol = 61533; // Symbol "confirm"
                btnABatch.Enabled = true;
                ipBatchNo.Enabled = true;
                ipBatchNo.FillColor = Color.Yellow;
            }
            else
            {
                this.ShowErrorDialog("Tải xuống ERP thất bại, HÃY CHỤP LẠI THÔNG BÁO NÀY. Vui lòng nhấn vào mục Kiểm tra-> chọn mục B09 -> Nhấn bắt đầu và đợi một lát. Nếu A01 báo thành công nhưng hiện trống, vui lòng kiểm tra Tên line, Tên nhà máy, Tên xưởng đã đúng chưa? Nếu đã đúng hãy liên hệ người quản lý ERP hoặc IT nhà máy");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Tải lô từ ERP thất bại", $"{{'Thông tin lỗi':'{rs}'}}", "ERP-F-01");
                btnABatch.FillColor = Color.FromArgb(0, 192, 0);
                btnABatch.Symbol = 563629; // Reset symbol
                _batchChangeMode = false; // Stay in the same mode
            }
        }

        private void HandleConfirmBatchChange()
        {
            var result = this.ShowAskDialog("Bạn có chắc chắn thay đổi lô sản xuất?");
            if (result)
            {
                try
                {
                    btnABatch.Enabled = true;
                    btnABatch.FillColor = Color.FromArgb(0, 192, 0);
                    ipBatchNo.Enabled = false;
                    ipBatchNo.FillColor = Color.White;

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
        }

        #endregion

        #region UI Rendering Methods

        private void Render_Order_Statistics()
        {
            this.InvokeIfRequired(() =>
            {
                opBatchCount.Value = FD_Globals.ActiveSet.Count;
                opProductionSpeed.Value = FD_Globals.ProductionPerHour;
            });
        }

        private void Render_Camera_Counter()
        {
            this.InvokeIfRequired(() =>
            {
                opSCount.Text = $"{FD_Globals.productionData.productCameraCounter.Total} - {FD_Globals.productionData.productCameraCounter.Pass} - {FD_Globals.productionData.productCameraCounter.Fail}";
            });
        }

        private void Render_Production_Statistics()
        {
            this.InvokeIfRequired(() =>
            {
                opTotalCount.Value = FD_Globals.productionData.PLC_Counter.Total;
                opPassCount.Value = FD_Globals.productionData.PLC_Counter.Pass;
                opFail.Value = FD_Globals.productionData.PLC_Counter.Fail;
            });
        }

        private void Render_App_Status()
        {
            if (FD_Globals.pLCStatus != PLCStatus.Connected || FD_Globals.CameraStatus != CameraStatus.Connected)
            {
                GlobalVarialbles.CurrentAppState = e_AppState.DeviceError;
            }
            else
            {
                GlobalVarialbles.CurrentAppState = ipBatchNo.Enabled ? e_AppState.Editing : e_AppState.Ready;
            }

            switch (GlobalVarialbles.CurrentAppState)
            {
                case e_AppState.Ready:
                    SetAppStatus("Sẵn Sàng", Color.FromArgb(0, 192, 0), 1);
                    break;
                case e_AppState.Editing:
                    SetAppStatus("Cấu hình", Color.Blue, 4);
                    break;
                case e_AppState.DeviceError:
                    SetAppStatus("Lỗi TB", Color.Red, 5);
                    break;
            }
        }

        private void SetAppStatus(string text, Color color, int code)
        {
            this.InvokeIfRequired(() =>
            {
                opAppStatus.Text = text;
                opAppStatus.RectColor = color;
                opAppStatus.ForeColor = color;
                opAppStatusCode.Value = code;
            });
        }

        private void Render_PLC_Status()
        {
            if (FD_Globals.pLCStatus != _plcStatus)
            {
                FD_Globals.pLCStatus = _plcStatus;
                switch (_plcStatus)
                {
                    case PLCStatus.Connected:
                        SetDeviceStatus(opPLCStatus, opPLCLed, "Kết Nối", Color.FromArgb(0, 192, 0), false);
                        break;
                    case PLCStatus.Disconnect:
                        SetDeviceStatus(opPLCStatus, opPLCLed, "Lỗi K01", Color.Red, true);
                        break;
                }
            }
        }

        private void Render_Camera_Status()
        {
            switch (FD_Globals.CameraStatus)
            {
                case CameraStatus.Disconnected:
                    SetDeviceStatus(opCameraStatus, opCameraLed, "Lỗi K01", Color.Red, true);
                    break;
                case CameraStatus.Connected:
                    SetDeviceStatus(opCameraStatus, opCameraLed, "Kết Nối", Color.FromArgb(0, 192, 0), false);
                    break;
                case CameraStatus.Error:
                    SetDeviceStatus(opCameraStatus, opCameraLed, "Lỗi K02", Color.Red, true);
                    break;
                case CameraStatus.Reconnecting:
                    SetDeviceStatus(opCameraStatus, opCameraLed, "...", Color.OrangeRed, true, Color.Yellow);
                    break;
            }
        }

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

        private void Render_Production_Result(QRProductRecord qRProductRecord)
        {
            this.InvokeIfRequired(() =>
            {
                opView.Items.Insert(0, $"#{qRProductRecord.ID} - {qRProductRecord.Status} - {qRProductRecord.QRContent}");
                if (opView.Items.Count >= 50)
                {
                    opView.Items.RemoveAt(opView.Items.Count - 1);
                }
                opResopse.Text = qRProductRecord.QRContent;

                switch (qRProductRecord.Status)
                {
                    case e_Production_Status.Pass:
                        SetResultStatus("TỐT", Color.Green);
                        break;
                    case e_Production_Status.Fail:
                        SetResultStatus("XẤU", Color.Red);
                        break;
                    case e_Production_Status.Error:
                        SetResultStatus("LỖI", Color.Orange);
                        break;
                    case e_Production_Status.Duplicate:
                        SetResultStatus("TRÙNG", Color.Yellow);
                        break;
                    case e_Production_Status.NotFound:
                        SetResultStatus("KHÔNG CÓ", Color.Yellow);
                        break;
                    case e_Production_Status.Timeout:
                        SetResultStatus("HẾT GIỜ", Color.Red);
                        break;
                    case e_Production_Status.ReadFail:
                        SetResultStatus("LỖI ĐỌC", Color.Red);
                        break;
                    case e_Production_Status.FormatError:
                        SetResultStatus("LỖI NT", Color.Red);
                        FD_Globals.AlarmCount++;
                        break;
                }
            });
        }

        private void SetResultStatus(string text, Color color)
        {
            opResultStatus.Text = text;
            opResultStatus.FillColor = color;
        }

        private void UpdateAlarmDisplay()
        {
            if (FD_Globals.AlarmCount >= 1)
            {
                _blinkAlarm++;
                if (_blinkAlarm >= 5)
                {
                    SetAlarm($"LỖI SAI ĐỊNH DẠNG MÃ - ĐÂY LÀ LỖI NGHIÊM TRỌNG: {FD_Globals.AlarmCount}", Color.Red, Color.Red);
                    _blinkAlarm = 0;
                }
                else
                {
                    SetAlarm($"LỖI SAI ĐỊNH DẠNG MÃ - ĐÂY LÀ LỖI NGHIÊM TRỌNG: {FD_Globals.AlarmCount}", Color.Yellow, Color.Yellow);
                }
            }
            else
            {
                SetAlarm("-", Color.FromArgb(243, 249, 255), Color.FromArgb(80, 160, 255));
            }
        }

        private void SetAlarm(string text, Color fillColor, Color rectColor)
        {
            this.InvokeIfRequired(() =>
            {
                opAlarm.Text = text;
                opAlarm.FillColor = fillColor;
                opAlarm.RectColor = rectColor;
            });
        }

        #endregion


        private void uiSymbolButton3_Click(object sender, EventArgs e)
        {
            //trả sự kiện đổi page về dashboard
            ChangePage?.Invoke(1004);
        }

        private void btnPLCSetting_Click(object sender, EventArgs e)
        {
            ChangePage?.Invoke(1005);
        }
    }

    #region Nested Types

    public static class FD_Globals
    {
        public static CameraStatus CameraStatus { get; set; } = CameraStatus.Disconnected;
        public static ProductionData productionData { get; set; } = new ProductionData();
        public static int AlarmCount { get; set; } = 0;
        public static PLCStatus pLCStatus { get; set; } = PLCStatus.Disconnect;
        public static HashSet<string> ActiveSet { get; set; } = new HashSet<string>();
        public static ConcurrentQueue<QRProductRecord> QueueRecord { get; set; } = new ConcurrentQueue<QRProductRecord>();

        public static ConcurrentQueue<QRProductRecord> QueueActive { get; set; } = new ConcurrentQueue<QRProductRecord>();
        public static int LineSpeed { get; set; }
        public static int ProductionPerHour { get; set; } = 0;
    }

    #endregion

}

