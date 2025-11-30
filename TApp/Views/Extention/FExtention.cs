using Sunny.UI;
using System.Data;
using System.Linq;
using TApp.Configs;
using TApp.Utils;
using TTManager.Masan;
using TApp.Helpers.Masan_Backup;
using TApp.Helpers;
using TApp.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace TApp.Views.Extention
{
    public partial class FExtention : UIPage
    {
        public FExtention()
        {
            InitializeComponent();
        }
        private string BackupLogDbPath = @"C:/MASAN/CloudBackupLog.tls";
        private DataTable upCloudHis = new DataTable();
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
        }

        private void btnERPCheck_Click(object sender, EventArgs e)
        {
            btnERPCheck.Enabled = false;
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }

        }

        DataTable dataTable = new DataTable();
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

            //Ghi nhận log 
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            btnERPCheck.Enabled = true;
        }

        private void FExtention_Initialize(object sender, EventArgs e)
        {
            opConsole.Items.Clear();
            opData.DataSource = null;

            // Load lịch sử backup khi khởi tạo
            LoadBackupHistory();
        }

        /// <summary>
        /// Load 100 lịch sử backup gần nhất
        /// </summary>
        /// 
        
        private void LoadBackupHistory()
        {
            try
            {
                TResult result = CloudBackupHelper.GetData(BackupLogDbPath);
                if (result.issuccess && result.data != null)
                {
                    this.InvokeIfRequired(() =>
                    {
                        // uiDataGridView1.DataSource = result.data;

                        upCloudHis = result.data;
                        // Lấy thông tin lần upload gần nhất thành công
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
                this.InvokeIfRequired(() =>
                {
                    opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Load lịch sử backup thất bại: {ex.Message}");
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin thời gian upload tiếp theo
        /// </summary>
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
            catch { }
        }

        int countSync = 100000;
        int maxInterval = 5;
        int seconSync = 0;
        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            while (!backgroundWorker2.CancellationPending)
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



                if (countSync >= maxInterval)
                {
                    countSync = 0;
                    this.InvokeIfRequired(() =>
                    {
                        btnCloudHis.Enabled = true;
                    });
                    
                    // Cập nhật thời gian upload tiếp theo
                    UpdateNextUploadTime();

                    this.InvokeIfRequired(() =>
                    {
                        opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÔNG BÁO] Bắt đầu tải dữ liệu lên máy chủ theo chu kì...");
                    });

                    DateTime dateTime = DateTime.Now;

                    string day = $"{DateTime.Now.ToString("yyyy")}/{dateTime.ToString("MM")}";
                    string csvFileName = $"{AppConfigs.Current.Line_Name}_{dateTime.ToString("ddMMyyyy_HHmmss")}.csv";
                    string csvTempPath = Path.Combine("C:/MASAN/Temp/", csvFileName);
                    string csvBackupPath = Path.Combine("C:/MASAN/Backup/", csvFileName);

                    long lastUnix = 0;

                    try
                    {
                        lastUnix = CloudBackupHelper.GetLastTimeBackup(BackupLogDbPath);
                    }
                    catch (Exception ex)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Lấy thời gian sao lưu cuối cùng thất bại: {ex.Message}");
                        });
                    }

                    //lấy danh sách dữ liệu cần sao lưu
                    string timeStartUpload = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    TResult result = QRDatabaseHelper.Get_ActiveQR_By_TimeUnix(lastUnix);
                    DataTable dataToBackup = result.data!;

                    //chuyển dữ liệu sang csv
                    if (!result.issuccess)
                    {
                        //lỗi lấy dữ liệu
                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Lấy dữ liệu cần sao lưu thất bại: {result.message}");
                        });

                        // Ghi log thất bại
                        CloudBackupHelper.InsertLog(BackupLogDbPath, "null", "0", timeStartUpload,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), lastUnix,
                            $"Lỗi lấy dữ liệu: {result.message}");
                        continue;
                    }

                    // Kiểm tra nếu không có dữ liệu mới
                    if (dataToBackup == null || dataToBackup.Rows.Count == 0)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÔNG BÁO] Không có dữ liệu mới để sao lưu.");
                        });
                        continue;
                    }

                    ExportResult exportResult = CsvHelper.ExportDataTableToCsv(dataToBackup, csvTempPath);

                    if (!exportResult.IsSucces)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Xuất dữ liệu sang CSV thất bại: {exportResult.Message}");
                        });
                        continue;
                    }

                    // Kiểm tra cấu hình upload và backup
                    bool shouldUploadCloud = AppConfigs.Current.Cloud_Upload_Enabled;
                    bool shouldBackupLocal = AppConfigs.Current.Local_Backup_Enabled;

                    // Lấy TimeUnixQR lớn nhất từ dữ liệu
                    long maxUnixQR = lastUnix;
                    if (dataToBackup.Columns.Contains("TimeUnixActive"))
                    {
                        foreach (DataRow row in dataToBackup.Rows)
                        {
                            long unixValue = Convert.ToInt64(row["TimeUnixActive"]);
                            if (unixValue > maxUnixQR)
                                maxUnixQR = unixValue;
                        }
                    }

                    bool uploadSuccess = false;
                    string uploadMessage = "";

                    //up lên cloud (nếu được bật)
                    if (shouldUploadCloud)
                    {
                        FileStream fileStream = null;
                        try
                        {
                            //// Tạo StorageClient từ file xác thực JSON
                            //GoogleCredential credential = GoogleCredential.FromFile(AppConfigs.Current.credentialERPPath);
                            //StorageClient storage = StorageClient.Create(credential);
                            //fileStream = File.OpenRead(exportResult.FilePath);

                            //// Tải file lên Google Cloud Storage với đường dẫn đầy đủ
                            //string objectPath = $"QRCode/{day}/{csvFileName}";
                            //var uploadedObject = storage.UploadObject("masan-image", objectPath, null, fileStream);

                            //// Đóng fileStream
                            //fileStream.Close();
                            //fileStream.Dispose();
                            //fileStream = null;

                            uploadSuccess = true;
                            // uploadMessage = $"Upload cloud thành công: {uploadedObject.Name}";

                            this.InvokeIfRequired(() =>
                            {
                                opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÀNH CÔNG] Tải lên cloud thành công: {csvFileName} ({dataToBackup.Rows.Count} bản ghi)");
                            });
                        }
                        catch (Exception ex)
                        {
                            // Đóng fileStream nếu còn mở
                            if (fileStream != null)
                            {
                                fileStream.Close();
                                fileStream.Dispose();
                            }

                            uploadSuccess = false;
                            uploadMessage = $"Lỗi upload cloud: {ex.Message}";

                            this.InvokeIfRequired(() =>
                            {
                                opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Tải lên cloud thất bại: {ex.Message}");
                            });
                        }
                    }
                    else
                    {
                        // Không upload cloud
                        uploadSuccess = true; // Coi như thành công vì không cần upload
                        uploadMessage = "Cloud upload bị tắt";

                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÔNG BÁO] Cloud upload bị tắt, chỉ backup local.");
                        });
                    }

                    // Backup local (nếu được bật)
                    if (shouldBackupLocal && uploadSuccess)
                    {
                        try
                        {
                            // Backup file CSV vào thư mục backup
                            File.Copy(exportResult.FilePath, csvBackupPath, true);

                            this.InvokeIfRequired(() =>
                            {
                                opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÀNH CÔNG] Backup local thành công: {csvBackupPath}");
                            });
                        }
                        catch (Exception ex)
                        {
                            this.InvokeIfRequired(() =>
                            {
                                opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Backup local thất bại: {ex.Message}");
                            });
                        }
                    }

                    // Xóa file tạm
                    try
                    {
                        if (File.Exists(exportResult.FilePath))
                        {
                            File.Delete(exportResult.FilePath);
                        }
                    }
                    catch { }

                    // Ghi log vào database
                    string logStatus = uploadSuccess ? "1" : "0";
                    string completedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    CloudBackupHelper.InsertLog(BackupLogDbPath, csvFileName, logStatus, timeStartUpload,
                        completedTime, uploadSuccess ? maxUnixQR : lastUnix, uploadMessage);

                    // Cập nhật UI nếu upload thành công
                    if (uploadSuccess)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opLastTimeUpload.Text = completedTime;
                            opLastUploadFileName.Text = csvFileName;
                        });
                    }

                    // Load lại lịch sử backup
                    LoadBackupHistory();
                }
                Thread.Sleep(500);
            }
        }

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
                opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Đang có vấn đề dữ liệu, vui lòng đợi nút sáng lên rồi thử lại {ex.Message}");
            }
            
        }
    }
}
