using HslCommunication;
using MTs.Auditrails;
using MTs.Datalogic;
using Sunny.UI;
using System.ComponentModel;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Models;
using TApp.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
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

            InitializeConfigs();
            InitializeDevices();
            InitalizeProductionInfomation();
            InitalizeProductionDatabase();
            InitializeDashboardUI();
            
            InitializeBackgroundWK();
        }

        private void InitializeBackgroundWK()
        {
            if (!WK_Dequeue.IsBusy)
            {
                WK_Dequeue.RunWorkerAsync();
            }

            if (!WK_Load_Counter.IsBusy)
            {
                WK_Load_Counter.RunWorkerAsync();
            }
        }

        private void InitializeDevices()
        {
            InitializeCameras();
            InitializePLC();
            RunBackgroundWorkers();
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

        private void InitalizeProductionDatabase()
        {
            try
            {
                // cho batch mới
                var info = QRDatabaseHelper.InitDatabases();

                // Sau đó load HashSet luôn nếu mày dùng
                FD_Globals.ActiveSet = QRDatabaseHelper.LoadActiveToHashSet();

                // existedBefore = true -> bể đã có từ trước
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo database sản xuất: {ex.Message}");
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
                Helpers.BatchHistoryModel lastBatch = BatchHistoryHelper.GetLatest(dbPath);

                if (lastBatch is not null)
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

                //lấy counter theo lô 
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
                    //chờ đến khi có line name
                    Thread.Sleep(100);
                }

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
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khởi tạo camera: {ex.Message}");
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
                        //gửi fail về PLC
                        Send_Result_To_PLC(e_PLC_Result.Fail);
                        Send_Result_Content(e_Production_Status.Timeout, data);
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
            //kiểm tra mã đã active chưa
            if (FD_Globals.ActiveSet.Contains(data))
            {

                Send_Result_To_PLC(e_PLC_Result.Fail);
                // Duplicate → xử lý theo flow Duplicate
                Send_Result_Content(e_Production_Status.Duplicate, data);
            }
            else
            {
                //kiểm tra đúng cấu trúc mã
                bool isValid = IsValidQRContent(data);
                if (!isValid)
                {
                    // mã không đúng cấu trúc
                    Send_Result_To_PLC(e_PLC_Result.Fail);
                    Send_Result_Content(e_Production_Status.FormatError, data);
                    return;
                }

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
                    FD_Globals.ActiveSet.Add(data); // update RAM luôn
                    // thành công
                    Send_Result_To_PLC(e_PLC_Result.Pass);
                    Send_Result_Content(e_Production_Status.Pass, data);
                }
                else
                {
                    // lỗi lưu dữ liệu
                    Send_Result_To_PLC(e_PLC_Result.Fail);
                    Send_Result_Content(e_Production_Status.Error, data);
                }
            }

        }

        private bool IsValidQRContent(string data)
        {
            //mã chứa barcode của sản phẩm
            if (data.Contains(FD_Globals.productionData.Barcode))
            {
                return true;
            }
            return false;

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

        public bool Send_Result_To_PLC(e_PLC_Result rs)
        {
            OperateResult write = omronPLC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reject_DM"), (short)rs);
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

        private void Render_App_Status()
        {
            if (FD_Globals.pLCStatus != PLCStatus.Connected || FD_Globals.CameraStatus != CameraStatus.Connected)
            {
                GlobalVarialbles.CurrentAppState = e_AppState.DeviceError;
            }
            else
            {
                if (ipBatchNo.Enabled == true)
                {
                    GlobalVarialbles.CurrentAppState = e_AppState.Editing;

                }
                else
                {
                    GlobalVarialbles.CurrentAppState = e_AppState.Ready;
                }
            }

            switch (GlobalVarialbles.CurrentAppState)
            {
                case e_AppState.Initializing:
                    break;
                case e_AppState.Ready:
                    this.InvokeIfRequired(() =>
                    {
                        opAppStatus.Text = "Sẵn Sàng";
                        opAppStatus.RectColor = Color.FromArgb(0, 192, 0);
                        opAppStatus.ForeColor = Color.FromArgb(0, 192, 0);
                        opAppStatusCode.Value = 1;
                    });
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

                    if (FD_Globals.pLCStatus != PLCStatus.Disconnect)
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
                    if (opCameraStatus.Text != "Lỗi K01")
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

        int blinkAlarm = 0;
        private void Render_Production_Result(QRProductRecord qRProductRecord)
        {
            this.InvokeIfRequired(() =>
            {
                opView.Items.Insert(0, $"#{qRProductRecord.ID.ToString()} - {qRProductRecord.Status} - {qRProductRecord.QRContent}");
                if (opView.Items.Count >= 50)
                {
                    opView.Items.RemoveAt(opView.Items.Count - 1);
                }
                opResopse.Text = qRProductRecord.QRContent;
                switch (qRProductRecord.Status) //// Pass, ReadFail, Duplicate, Error, Timeout, Deactive
                {
                    case e_Production_Status.Pass:
                        opResultStatus.Text = "TỐT";
                        opResultStatus.FillColor = Color.Green;
                        break;
                    case e_Production_Status.Fail:
                        opResultStatus.Text = "XẤU";
                        opResultStatus.FillColor = Color.Red;
                        break;
                    case e_Production_Status.Error:
                        opResultStatus.Text = "LỖI";
                        opResultStatus.FillColor = Color.Orange;
                        break;
                    case e_Production_Status.Duplicate:
                        opResultStatus.Text = "TRÙNG";
                        opResultStatus.FillColor = Color.Yellow;
                        break;
                    case e_Production_Status.NotFound:
                        opResultStatus.Text = "KHÔNG CÓ";
                        opResultStatus.FillColor = Color.Yellow;
                        break;
                    case e_Production_Status.Timeout:
                        opResultStatus.Text = "HẾT GIỜ";
                        opResultStatus.FillColor = Color.Red;
                        break;
                    case e_Production_Status.ReadFail:
                        opResultStatus.Text = "LỖI ĐỌC";
                        opResultStatus.FillColor = Color.Red;
                        break;
                    case e_Production_Status.FormatError:
                        opResultStatus.Text = "LỖI NT";
                        opResultStatus.FillColor = Color.Red;

                        FD_Globals.AlarmCount++;
                        break;
                }


            });
        }
        private bool batchChangeMode = false;
        private void btnChangeBatch_Click(object sender, EventArgs e)
        {
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Người dùng nhấn đổi lô", "", "UA-F-01");
            try
            {
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

                        GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Tải lô từ ERP thất bại", "{'Thông tin lỗi':'" + rs + "'}", "ERP-F-01");

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
            if (ipBatchNo.SelectedItem is not null)
            {
                ipBarcode.Text = erP_Google2.LoadExcelToProductList(ipBatchNo.SelectedItem.ToString(), AppConfigs.Current.production_list_path);
            }

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

        private void WK_Dequeue_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Dequeue.CancellationPending)
            {
                try
                {
                    if (FD_Globals.QueueRecord.Count > 0)
                    {
                        QRProductRecord record = FD_Globals.QueueRecord.Dequeue();

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
                    if (FD_Globals.AlarmCount >= 1)
                    {
                        opAlarm.Text = $"LỖI SAI ĐỊNH DẠNG MÃ - ĐÂY LÀ LỖI NGHIÊM TRỌNG: {FD_Globals.AlarmCount}";
                        blinkAlarm++;
                        if (blinkAlarm >= 5)
                        {
                            opAlarm.FillColor = Color.Red;
                            opAlarm.RectColor = Color.Red;
                            blinkAlarm = 0;
                        }
                        else
                        {
                            opAlarm.FillColor = Color.Yellow;
                            opAlarm.RectColor = Color.Yellow;
                        }

                    }
                    else
                    {
                        opAlarm.Text = $"-";
                        opAlarm.RectColor = Color.FromArgb(80, 160, 255);
                        opAlarm.FillColor = Color.FromArgb(243, 249, 255);
                    }
                    
                    Thread.Sleep(100);
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


                

                Thread.Sleep(1000); // Cập nhật lại sau mỗi 60 giây
            }
        }

        private void opFail_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInfoDialog($"Số quá thời gian:{FD_Globals.productionData.PLC_Counter.Timeout.ToString()}");
        }
    }

    public static class FD_Globals
    {
        public static CameraStatus CameraStatus { get; set; } = CameraStatus.Disconnected;
        public static ProductionData productionData { get; set; } = new ProductionData();

        public static int AlarmCount { get; set; } = 0;
        public static PLCStatus pLCStatus { get; set; }
         = PLCStatus.Disconnect;
        public static HashSet<string> ActiveSet { get; set; } = new HashSet<string>();

        public static Queue<QRProductRecord> QueueRecord { get; set; } = new Queue<QRProductRecord>();

        public static int LineSpeed { get; set; }
        public static int ProductionPerHour { get; set; } = 0;
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
