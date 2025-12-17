using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using HslCommunication;
using HslCommunication.Profinet.Omron;
using MTs.Auditrails;
using SQLitePCL;
using Sunny.UI;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TApp.Configs;
using TApp.Helpers;
using TApp.Helpers.Masan_Backup;
using TApp.Infrastructure;
using TApp.Utils;
using TApp.Views.Dashboard;
using TTManager.Masan;

namespace TApp.Views.Extention
{
    public partial class FExtention : UIPage
    {
        #region Fields
        private string BackupLogDbPath = @"C:/MASANQR/GGCloud/CloudBackupLog.tls";
        private string PlcSendHistoryPath = Path.Combine(@"C:\MASANQR", "IOT", "opc_send_history.txt");
        private DataTable upCloudHis = new DataTable();
        private DataTable dataTable = new DataTable();
        private int countSync = 100000;
        private int countSyncOPC = 100000;
        private int maxInterval = 5;
        private int maxIntervalOPCms = 5000;
        private OmronFinsUdp plc;
        private LogHelper<e_LogType> PLC_IOT_Logs;
        #endregion

        #region Constructor & Initialization
        public FExtention()
        {
            InitializeComponent();

            // Đảm bảo các thư mục C:\MASANQR tồn tại
            try
            {
                string root = @"C:\MASANQR";
                Directory.CreateDirectory(root);
                Directory.CreateDirectory(Path.Combine(root, "Temp"));
                Directory.CreateDirectory(Path.Combine(root, "Backup"));
                Directory.CreateDirectory(Path.Combine(root, "IOT"));

                string backupFolder = Path.GetDirectoryName(BackupLogDbPath) ?? root;
                Directory.CreateDirectory(backupFolder);

                string iotLogPath = Path.Combine(root, "IOT", "logs.ttl");
                PLC_IOT_Logs = new LogHelper<e_LogType>(iotLogPath);
            }
            catch
            {
                // Nếu có lỗi khi tạo thư mục/log, vẫn cho phép form khởi tạo
            }
        }

        private void FExtention_Initialize(object sender, EventArgs e)
        {
            opConsole.Items.Clear();
            opData.DataSource = null;
            LoadBackupHistory();
        }
        #endregion

        #region ERP Integration
        public void InitializeERP()
        {
            erP_Google1.credentialPath = AppConfigs.Current.credentialERPPath;
            erP_Google1.SUB_INV = AppConfigs.Current.ERP_Sub_Inv;
            erP_Google1.ORG_CODE = AppConfigs.Current.ERP_Org_Code;
            erP_Google1.DatasetID = AppConfigs.Current.ERP_DatasetID;
            erP_Google1.TableID = AppConfigs.Current.ERP_TableID;
            erP_Google1.LineName = AppConfigs.Current.Line_Name;

            if (AppConfigs.Current.Cloud_Connection_Enabled)
            {
                if (!backgroundWorker2.IsBusy)
                {
                    backgroundWorker2.RunWorkerAsync();
                }
                if (!WK_IOT_SCADA.IsBusy)
                {
                    WK_IOT_SCADA.RunWorkerAsync();
                }
            }

            plc = new OmronFinsUdp();
            plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeUdpNet(AppConfigs.Current.PLC_IP, AppConfigs.Current.PLC_Port)
            {
                ReceiveTimeOut = 10000,
            };
            plc.PlcType = OmronPlcType.CSCJ;
            plc.SA1 = 1;
            plc.GCT = 2;
            plc.DA1 = 0;
            plc.SID = 0;
            plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
            plc.ByteTransform.IsStringReverseByteWord = true;
        }

        private void btnERPCheck_Click(object sender, EventArgs e)
        {
            btnERPCheck.Enabled = false;
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                opConsole.Items.Insert(0, "[THÔNG BÁO] Bắt đầu lấy dữ liệu ERP...");
            });
            TResult result = erP_Google1.Get_Erp_To_Table();
            if (result.issuccess)
            {
                if (result.data != null)
                {
                    dataTable = result.data;
                    this.InvokeIfRequired(() =>
                    {
                        opData.DataSource = dataTable;
                        opConsole.Items.Insert(0, $"[THÀNH CÔNG] Lấy dữ liệu ERP thành công! Số bản ghi: {dataTable.Rows.Count}");
                    });
                }
                else
                {
                    this.InvokeIfRequired(() =>
                    {
                        opConsole.Items.Insert(0, $"[LỖI] Dữ liệu ERP trả về rỗng!");
                    });
                }
            }
            else
            {
                this.InvokeIfRequired(() =>
                {
                    opConsole.Items.Insert(0, $"[LỖI] Lấy dữ liệu ERP thất bại: {result.message}");
                });
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            btnERPCheck.Enabled = true;
        }
        #endregion

        #region Cloud Backup
        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!backgroundWorker2.CancellationPending)
            {
                UpdateSyncCounterDisplay();

                if (countSync >= maxInterval)
                {
                    countSync = 0;
                    this.InvokeIfRequired(() =>
                    {
                        btnCloudHis.Enabled = true;
                    });

                    UpdateNextUploadTime();
                    LogConsoleMessage("[THÔNG BÁO] Bắt đầu tải dữ liệu lên máy chủ theo chu kì...");

                    string timeStartUpload = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    (DataTable dataToBackup, long lastUnix, string errorMessage) = PrepareBackupFiles();

                    if (dataToBackup == null)
                    {
                        //LogBackupResult("null", "0", timeStartUpload, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), lastUnix, errorMessage);
                        continue;
                    }

                    if (dataToBackup.Rows.Count == 0)
                    {
                        LogConsoleMessage("[THÔNG BÁO] Không có dữ liệu mới để sao lưu.");
                        continue;
                    }

                    string csvFileName = $"{AppConfigs.Current.Line_Name}_{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.csv";
                    string csvTempRoot = @"C:\MASANQR\Temp";
                    string csvBackupRoot = @"C:\MASANQR\Backup";
                    Directory.CreateDirectory(csvTempRoot);
                    Directory.CreateDirectory(csvBackupRoot);
                    string csvTempPath = Path.Combine(csvTempRoot, csvFileName);
                    string csvBackupPath = Path.Combine(csvBackupRoot, csvFileName);

                    ExportResult exportResult = CsvHelper.ExportDataTableToCsv(dataToBackup, csvTempPath);

                    if (!exportResult.IsSucces)
                    {
                        LogConsoleMessage($"[LỖI] Xuất dữ liệu sang CSV thất bại: {exportResult.Message}");
                        LogBackupResult(csvFileName, "0", timeStartUpload, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), lastUnix, $"Lỗi xuất CSV: {exportResult.Message}");
                        continue;
                    }

                    bool shouldUploadCloud = AppConfigs.Current.Cloud_Upload_Enabled;
                    bool shouldBackupLocal = AppConfigs.Current.Local_Backup_Enabled;

                    long maxUnixQR = GetMaxTimeUnixActive(dataToBackup, lastUnix);
                    var upload = PerformCloudUpload(shouldUploadCloud, exportResult.FilePath, csvFileName, dataToBackup.Rows.Count);
                    bool uploadSuccess = upload.Item1;
                    string uploadMessage = upload.Item2; //uploadSuccess ? "Upload cloud thành công" : "Upload cloud thất bại";

                    if (shouldBackupLocal && uploadSuccess)
                    {
                        PerformLocalBackup(exportResult.FilePath, csvBackupPath);
                    }

                    CleanUpTempFile(exportResult.FilePath);
                    LogBackupResult(csvFileName, uploadSuccess ? "1" : "0", timeStartUpload, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), uploadSuccess ? maxUnixQR : lastUnix, uploadMessage);

                    if (uploadSuccess)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opLastTimeUpload.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            //opLastUploadFileName.Text = csvFileName;
                        });
                    }
                    LoadBackupHistory();
                }
                Thread.Sleep(500);
            }
        }

        private void UpdateSyncCounterDisplay()
        {
            maxInterval = (AppConfigs.Current.Cloud_Refresh_Interval_Minute * 60 * 1000) / 500;
            countSync++;

            if (countSync % 2 == 0)
            {
                this.InvokeIfRequired(() =>
                {
                    opC1.Text = ((maxInterval - countSync) / 2).ToString();
                });
            }
        }

        private (DataTable, long, string) PrepareBackupFiles()
        {
            long lastUnix = 0;
            try
            {
                lastUnix = CloudBackupHelper.GetLastTimeBackup(BackupLogDbPath);
            }
            catch (Exception ex)
            {
                LogConsoleMessage($"[LỖI] Lấy thời gian sao lưu cuối cùng thất bại: {ex.Message}");
                return (null, lastUnix, $"Lỗi lấy thời gian sao lưu: {ex.Message}");
            }

            TResult result = QRDatabaseHelper.Get_ActiveQR_By_TimeUnix(lastUnix);
            if (!result.issuccess)
            {
                LogConsoleMessage($"[LỖI] Lấy dữ liệu cần sao lưu thất bại: {result.message}");
                return (null, lastUnix, $"Lỗi lấy dữ liệu: {result.message}");
            }
            return (result.data, lastUnix, string.Empty);
        }

        private long GetMaxTimeUnixActive(DataTable data, long defaultUnix)
        {
            long maxUnixQR = defaultUnix;
            if (data.Columns.Contains("TimeUnixActive"))
            {
                foreach (DataRow row in data.Rows)
                {
                    long unixValue = Convert.ToInt64(row["TimeUnixActive"]);
                    if (unixValue > maxUnixQR)
                        maxUnixQR = unixValue;
                }
            }
            return maxUnixQR;
        }

        private (bool, string) PerformCloudUpload(bool shouldUploadCloud, string filePath, string csvFileName, int rowCount)
        {
            if (!shouldUploadCloud)
            {
                LogConsoleMessage("[THÔNG BÁO] Cloud upload bị tắt, chỉ backup local.");
                return (true, "Bỏ qua tải lên"); // Coi như thành công vì không cần upload
            }

            FileStream fileStream = null;
            try
            {
                // Commented out actual cloud upload logic as it requires Google Cloud credentials
                // and should be handled securely.
                // Example of what would be here:

                GoogleCredential credential = GoogleCredential.FromFile(AppConfigs.Current.credentialERPPath);
                StorageClient storage = StorageClient.Create(credential);
                fileStream = File.OpenRead(filePath);
                string objectPath = $"QRCode/{DateTime.Now.ToString("yyyy")}/{DateTime.Now.ToString("MM")}/{csvFileName}";
                var uploadedObject = storage.UploadObject("masan-image", objectPath, null, fileStream);
                fileStream.Close();
                fileStream.Dispose();
                fileStream = null;


                LogConsoleMessage($"[THÀNH CÔNG] Tải lên cloud thành công: {csvFileName} ({rowCount} bản ghi)");
                return (true, "Tải lên thành công");
            }
            catch (Exception ex)
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
                LogConsoleMessage($"[LỖI] Tải lên cloud thất bại: {ex.Message}");
                return (false, ex.Message);
            }
        }

        private void PerformLocalBackup(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
                LogConsoleMessage($"[THÀNH CÔNG] Backup local thành công: {destinationFilePath}");
            }
            catch (Exception ex)
            {
                LogConsoleMessage($"[LỖI] Backup local thất bại: {ex.Message}");
            }
        }

        private void CleanUpTempFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch { /* Ignore errors during temp file cleanup */ }
        }

        private void LogBackupResult(string fileName, string status, string timeStart, string timeCompleted, long lastUnix, string message)
        {
            CloudBackupHelper.InsertLog(BackupLogDbPath, fileName, status, timeStart, timeCompleted, lastUnix, message);
        }

        #endregion

        #region UI Event Handlers
        private void opConsole_DoubleClick(object sender, EventArgs e)
        {
            this.ShowInfoDialog(opConsole.SelectedItem as string);
        }

        private void btnCloudHis_Click(object sender, EventArgs e)
        {
            btnCloudHis.Enabled = false;
            try
            {
                opData.DataSource = upCloudHis;
            }
            catch (Exception ex)
            {
                LogConsoleMessage($"[LỖI] Đang có vấn đề dữ liệu, vui lòng đợi nút sáng lên rồi thử lại {ex.Message}");
            }
        }
        #endregion

        #region Helper Methods
        private void AppendPlcSendHistory(string content)
        {
            try
            {
                string root = @"C:\MASANQR\IOT";
                Directory.CreateDirectory(root);

                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff};{GlobalVarialbles.CurrentUser?.Username ?? "Unknown"};{content}";
                File.AppendAllLines(PlcSendHistoryPath, new[] { line }, Encoding.UTF8);

                string[] lines = File.ReadAllLines(PlcSendHistoryPath, Encoding.UTF8);
                if (lines.Length > 10)
                {
                    var lastLines = lines.Skip(lines.Length - 10).ToArray();
                    File.WriteAllLines(PlcSendHistoryPath, lastLines, Encoding.UTF8);
                }
            }
            catch
            {
                // Bỏ qua lỗi ghi lịch sử để không ảnh hưởng đến luồng chính
            }
        }

        private void LoadBackupHistory()
        {
            try
            {
                TResult result = CloudBackupHelper.GetData(BackupLogDbPath);
                if (result.issuccess && result.data != null)
                {
                    this.InvokeIfRequired(() =>
                    {
                        upCloudHis = result.data;
                        var successRows = result.data.AsEnumerable()
                            .Where(row => row["Status"].ToString() == "1")
                            .OrderByDescending(row => row["ID"]);

                        if (successRows.Any())
                        {
                            var lastSuccess = successRows.First();
                            opLastTimeUpload.Text = lastSuccess["TimeCompleted"].ToString();
                            //opLastUploadFileName.Text = lastSuccess["FileName"].ToString();
                        }
                        else
                        {
                            opLastTimeUpload.Text = "Chưa có dữ liệu";
                            //opLastUploadFileName.Text = "Chưa có dữ liệu";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogConsoleMessage($"[LỖI] Load lịch sử backup thất bại: {ex.Message}");
            }
        }

        private void UpdateNextUploadTime()
        {
            try
            {
                int intervalMinutes = AppConfigs.Current.Cloud_Refresh_Interval_Minute;
                DateTime nextTime = DateTime.Now.AddMinutes(intervalMinutes);

                this.InvokeIfRequired(() =>
                {
                    opNextUploadTime.Text = nextTime.ToString("yyyy-MM-dd HH:mm:ss");
                });
            }
            catch { /* Ignore */ }
        }

        private void LogConsoleMessage(string message)
        {
            this.InvokeIfRequired(() =>
            {
                opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - {message}");
            });
        }
        #endregion

        private void WK_IOT_SCADA_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!WK_IOT_SCADA.CancellationPending)
            {
                Thread.Sleep(500);
                maxIntervalOPCms = AppConfigs.Current.OPC_UA_Time_Refresh/500;
                countSyncOPC++;
                if (countSyncOPC % 2 == 0)
                {
                    this.InvokeIfRequired(() =>
                    {
                        opOPCCountdown.Text = ((maxIntervalOPCms - countSyncOPC) / 2).ToString();
                    });
                }

                if (countSyncOPC < maxIntervalOPCms)
                {
                    continue;
                }

                countSyncOPC = 0;

                //Ghi dữ liệu mới - Đọc từ PLC trước, nếu trùng thì không gửi
                //batch code
                string batchCodeAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Batch_Code_DM");
                string newBatchCode = FD_Globals.productionData.BatchCode ?? "";
                OperateResult<string> readBatchCode = plc.ReadString(batchCodeAddr, 40, Encoding.ASCII);
                bool needWriteBatchCode = true;
                
                if (readBatchCode.IsSuccess)
                {
                    string currentBatchCode = readBatchCode.Content?.TrimEnd('\0') ?? "";
                    if (currentBatchCode == newBatchCode)
                    {
                        needWriteBatchCode = false;
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, $"Batch Code trùng với PLC, bỏ qua gửi: {newBatchCode}");
                    }
                }
                
                if (needWriteBatchCode)
                {
                    OperateResult wbatchcode = plc.Write(batchCodeAddr, newBatchCode, Encoding.ASCII);
                    if (wbatchcode.IsSuccess)
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Gửi dữ liệu Batch Thành công");
                    }
                    else
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu Batch Thất bại :" + wbatchcode.Message);
                    }
                }

                //barcode
                string barcodeAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Barcode_DM");
                string newBarcode = FD_Globals.productionData.Barcode ?? "";
                OperateResult<string> readBarcode = plc.ReadString(barcodeAddr, 40, Encoding.ASCII);
                bool needWriteBarcode = true;
                
                if (readBarcode.IsSuccess)
                {
                    string currentBarcode = readBarcode.Content?.TrimEnd('\0') ?? "";
                    if (currentBarcode == newBarcode)
                    {
                        needWriteBarcode = false;
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, $"Barcode trùng với PLC, bỏ qua gửi: {newBarcode}");
                    }
                }
                
                if (needWriteBarcode)
                {
                    OperateResult wbarcode = plc.Write(barcodeAddr, newBarcode, Encoding.ASCII);
                    if (wbarcode.IsSuccess)
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Gửi dữ liệu Barcode Thành công");
                    }
                    else
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu Barcode Thất bại :" + wbarcode.Message, "", "FDE_0009");
                    }
                }

                //barcodeformaterror -> gửi số lỗi sai định dạng QR/Barcode
                string formatFailAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Barcode_Format_Fail_DM");
                int newAlarmCount = FD_Globals.AlarmCount;
                OperateResult<int> readFormatFail = plc.ReadInt32(formatFailAddr);
                bool needWriteFormatFail = true;
                
                if (readFormatFail.IsSuccess && readFormatFail.Content == newAlarmCount)
                {
                    needWriteFormatFail = false;
                }
                
                if (needWriteFormatFail)
                {
                    OperateResult wBarcodeFormatError = plc.Write(formatFailAddr, newAlarmCount);
                    if (!wBarcodeFormatError.IsSuccess)
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu Barcode FormatError thất bại :" + wBarcodeFormatError.Message, "", "FDE_0010");
                    }
                }

                //systemstatusDM -> gửi trạng thái hệ thống (AppState)
                string systemStatusAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_App_System_Status_DM");
                short newSystemStatus = (short)GlobalVarialbles.CurrentAppState;
                OperateResult<short> readSystemStatus = plc.ReadInt16(systemStatusAddr);
                bool needWriteSystemStatus = true;
                
                if (readSystemStatus.IsSuccess && readSystemStatus.Content == newSystemStatus)
                {
                    needWriteSystemStatus = false;
                }
                
                if (needWriteSystemStatus)
                {
                    OperateResult wSystemStatus = plc.Write(systemStatusAddr, newSystemStatus);
                    if (!wSystemStatus.IsSuccess)
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu trạng thái hệ thống thất bại :" + wSystemStatus.Message, "", "FDE_0011");
                    }
                }

                //số đếm
                string allFailAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Count_AllFail_DM");
                int newAllFail = (int)FD_Globals.productionData.PLC_Counter.Fail;
                OperateResult<int> readAllFail = plc.ReadInt32(allFailAddr);
                bool needWriteAllFail = true;
                
                if (readAllFail.IsSuccess && readAllFail.Content == newAllFail)
                {
                    needWriteAllFail = false;
                }
                
                if (needWriteAllFail)
                {
                    OperateResult wAllFail = plc.Write(allFailAddr, newAllFail);
                    if (!wAllFail.IsSuccess)
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu số đếm lỗi thất bại :" + wAllFail.Message, "", "FDE_0011");
                    }
                }

                //productionSpeed
                string productionSpeedAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Production_Speed");
                int newProductionSpeed = (int)FD_Globals.productionData.ProductionPerHour;
                OperateResult<int> readProductionSpeed = plc.ReadInt32(productionSpeedAddr);
                bool needWriteProductionSpeed = true;
                
                if (readProductionSpeed.IsSuccess && readProductionSpeed.Content == newProductionSpeed)
                {
                    needWriteProductionSpeed = false;
                }
                
                if (needWriteProductionSpeed)
                {
                    OperateResult wPS = plc.Write(productionSpeedAddr, newProductionSpeed);
                    if (!wPS.IsSuccess)
                    {
                        PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu tốc độ thất bại :" + wPS.Message, "", "FDE_0011");
                    }
                }

                // Ghi lại lịch sử gửi xuống (tối đa 10 lần gần nhất)
                string historyContent =
                    $"Batch={newBatchCode},Write={needWriteBatchCode};" +
                    $"Barcode={newBarcode},Write={needWriteBarcode};" +
                    $"AlarmCount={newAlarmCount},Write={needWriteFormatFail};" +
                    $"SystemStatus={newSystemStatus},Write={needWriteSystemStatus};" +
                    $"AllFail={newAllFail},Write={needWriteAllFail};" +
                    $"ProductionSpeed={newProductionSpeed},Write={needWriteProductionSpeed}";
                AppendPlcSendHistory(historyContent);

            }
        }

        private void btnOPCHis_Click(object sender, EventArgs e)
        {
            btnOPCHis.Enabled = false;
            try
            {
                LogConsoleMessage("[THÔNG BÁO] Bắt đầu đọc dữ liệu D memory từ WK_IOT...");
                
                DataTable dtOPCHis = new DataTable();
                dtOPCHis.Columns.Add("Tên", typeof(string));
                dtOPCHis.Columns.Add("Địa chỉ", typeof(string));
                dtOPCHis.Columns.Add("Giá trị", typeof(string));
                dtOPCHis.Columns.Add("Ghi chú", typeof(string));

                // Đọc PLC_Batch_Code_DM (string, 40 words)
                try
                {
                    string batchCodeAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Batch_Code_DM");
                    OperateResult<string> batchCodeResult = plc.ReadString(batchCodeAddr, 40, Encoding.ASCII);
                    if (batchCodeResult.IsSuccess)
                    {
                        dtOPCHis.Rows.Add("PLC_Batch_Code_DM", batchCodeAddr, batchCodeResult.Content?.TrimEnd('\0') ?? "", "Batch Code (String)");
                    }
                    else
                    {
                        dtOPCHis.Rows.Add("PLC_Batch_Code_DM", batchCodeAddr, $"Lỗi: {batchCodeResult.Message}", "Batch Code (String)");
                    }
                }
                catch (Exception ex)
                {
                    dtOPCHis.Rows.Add("PLC_Batch_Code_DM", "N/A", $"Lỗi: {ex.Message}", "Batch Code (String)");
                }

                // Đọc PLC_Barcode_DM (string)
                try
                {
                    string barcodeAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Barcode_DM");
                    OperateResult<string> barcodeResult = plc.ReadString(barcodeAddr, 40, Encoding.ASCII);
                    if (barcodeResult.IsSuccess)
                    {
                        dtOPCHis.Rows.Add("PLC_Barcode_DM", barcodeAddr, barcodeResult.Content?.TrimEnd('\0') ?? "", "Barcode (String)");
                    }
                    else
                    {
                        dtOPCHis.Rows.Add("PLC_Barcode_DM", barcodeAddr, $"Lỗi: {barcodeResult.Message}", "Barcode (String)");
                    }
                }
                catch (Exception ex)
                {
                    dtOPCHis.Rows.Add("PLC_Barcode_DM", "N/A", $"Lỗi: {ex.Message}", "Barcode (String)");
                }

                // Đọc PLC_Barcode_Format_Fail_DM (int)
                try
                {
                    string formatFailAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Barcode_Format_Fail_DM");
                    OperateResult<int> formatFailResult = plc.ReadInt32(formatFailAddr);
                    if (formatFailResult.IsSuccess)
                    {
                        dtOPCHis.Rows.Add("PLC_Barcode_Format_Fail_DM", formatFailAddr, formatFailResult.Content.ToString(), "Số lỗi định dạng QR/Barcode (Int32)");
                    }
                    else
                    {
                        dtOPCHis.Rows.Add("PLC_Barcode_Format_Fail_DM", formatFailAddr, $"Lỗi: {formatFailResult.Message}", "Số lỗi định dạng QR/Barcode (Int32)");
                    }
                }
                catch (Exception ex)
                {
                    dtOPCHis.Rows.Add("PLC_Barcode_Format_Fail_DM", "N/A", $"Lỗi: {ex.Message}", "Số lỗi định dạng QR/Barcode (Int32)");
                }

                // Đọc PLC_App_System_Status_DM (short/int)
                try
                {
                    string systemStatusAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_App_System_Status_DM");
                    OperateResult<short> systemStatusResult = plc.ReadInt16(systemStatusAddr);
                    if (systemStatusResult.IsSuccess)
                    {
                        dtOPCHis.Rows.Add("PLC_App_System_Status_DM", systemStatusAddr, systemStatusResult.Content.ToString(), "Trạng thái hệ thống (Int16)");
                    }
                    else
                    {
                        dtOPCHis.Rows.Add("PLC_App_System_Status_DM", systemStatusAddr, $"Lỗi: {systemStatusResult.Message}", "Trạng thái hệ thống (Int16)");
                    }
                }
                catch (Exception ex)
                {
                    dtOPCHis.Rows.Add("PLC_App_System_Status_DM", "N/A", $"Lỗi: {ex.Message}", "Trạng thái hệ thống (Int16)");
                }

                // Đọc PLC_Count_AllFail_DM (int)
                try
                {
                    string allFailAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_Count_AllFail_DM");
                    OperateResult<int> allFailResult = plc.ReadInt32(allFailAddr);
                    if (allFailResult.IsSuccess)
                    {
                        dtOPCHis.Rows.Add("PLC_Count_AllFail_DM", allFailAddr, allFailResult.Content.ToString(), "Số đếm lỗi (Int32)");
                    }
                    else
                    {
                        dtOPCHis.Rows.Add("PLC_Count_AllFail_DM", allFailAddr, $"Lỗi: {allFailResult.Message}", "Số đếm lỗi (Int32)");
                    }
                }
                catch (Exception ex)
                {
                    dtOPCHis.Rows.Add("PLC_Count_AllFail_DM", "N/A", $"Lỗi: {ex.Message}", "Số đếm lỗi (Int32)");
                }

                // Load vào datagrid
                opData.DataSource = dtOPCHis;
                LogConsoleMessage($"[THÀNH CÔNG] Đọc dữ liệu D memory thành công! Số bản ghi: {dtOPCHis.Rows.Count}");
            }
            catch (Exception ex)
            {
                LogConsoleMessage($"[LỖI] Đọc dữ liệu D memory thất bại: {ex.Message}");
            }
            finally
            {
                btnOPCHis.Enabled = true;
            }
        }

        private void btnReadOPCSendHistory_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(PlcSendHistoryPath))
                {
                    this.ShowInfoDialog("Chưa có lịch sử gửi OPC nào.");
                    return;
                }

                var lines = File.ReadAllLines(PlcSendHistoryPath, Encoding.UTF8);
                DataTable dt = new DataTable();
                dt.Columns.Add("Thời gian", typeof(string));
                dt.Columns.Add("Người dùng", typeof(string));
                dt.Columns.Add("Nội dung", typeof(string));

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(new[] { ';' }, 3);
                    string time = parts.Length > 0 ? parts[0] : "";
                    string user = parts.Length > 1 ? parts[1] : "";
                    string detail = parts.Length > 2 ? parts[2] : "";

                    dt.Rows.Add(time, user, detail);
                }

                opData.DataSource = dt;
                LogConsoleMessage($"[THÔNG BÁO] Đã tải {dt.Rows.Count} bản ghi lịch sử gửi OPC.");
            }
            catch (Exception ex)
            {
                LogConsoleMessage($"[LỖI] Đọc lịch sử gửi OPC thất bại: {ex.Message}");
            }
        }
    }
}
