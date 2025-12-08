using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using HslCommunication;
using HslCommunication.Profinet.Omron;
using MTs.Auditrails;
using SQLitePCL;
using Sunny.UI;
using System.Data;
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
        private string BackupLogDbPath = @"C:/MASAN/CloudBackupLog.tls";
        private DataTable upCloudHis = new DataTable();
        private DataTable dataTable = new DataTable();
        private int countSync = 100000;
        private int maxInterval = 5;
        private OmronFinsUdp plc;
        private LogHelper <e_LogType> PLC_IOT_Logs = new LogHelper<e_LogType>("C:/MASANQR/IOT/logs.ttl");
        #endregion

        #region Constructor & Initialization
        public FExtention()
        {
            InitializeComponent();
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
                    string csvTempPath = Path.Combine("C:/MASANQR/Temp/", csvFileName);
                    string csvBackupPath = Path.Combine("C:/MASANQR/Backup/", csvFileName);

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
                            opLastUploadFileName.Text = csvFileName;
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
                            opLastUploadFileName.Text = lastSuccess["FileName"].ToString();
                        }
                        else
                        {
                            opLastTimeUpload.Text = "Chưa có dữ liệu";
                            opLastUploadFileName.Text = "Chưa có dữ liệu";
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
                //xóa dữ liệu cũ
                OperateResult write = plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Batch_Code_DM"), "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]".ToStringArray<int>());
                if (write.IsSuccess)
                {
                    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username,e_LogType.Info, "Xóa dữ liệu IOT SCADA thành công D5008");
                }
                else
                {
                    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Xóa dữ liệu IOT SCADA thất bại :" + write.Message);
                }


                //Ghi dữ liệu mới
                //batch code
                OperateResult wbatchcode = plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Batch_Code_DM"), FD_Globals.productionData.BatchCode, Encoding.ASCII);
                if (write.IsSuccess)
                {
                    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Gửi dữ liệu Batch Thành công");
                }
                else
                {
                    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu Batch Thất bại :" + wbatchcode.Message);
                }

                //barcode
                OperateResult wbarcode = plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Barcode_DM"), FD_Globals.productionData.BatchCode, Encoding.ASCII);
                if (write.IsSuccess)
                {
                    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Gửi dữ liệu Barcode Thành công");
                }
                else
                {
                    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu Barcode Thất bại :" + wbatchcode.Message);
                }


                //barcodeformaterror

                //systemstatusDM
                //OperateResult wsyscode = plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_App_System_Status_DM"), GlobalVarialbles.CurrentAppState, Encoding.ASCII);
                //if (write.IsSuccess)
                //{
                //    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Gửi dữ liệu Barcode Thành công");
                //}
                //else
                //{
                //    PLC_IOT_Logs.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Gửi dữ liệu Barcode Thất bại :" + wbatchcode.Message);
                //}

                Thread.Sleep(5000);
            }
        }
    }
}
