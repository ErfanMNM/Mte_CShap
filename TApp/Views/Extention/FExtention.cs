using Sunny.UI;
using System.Data;
using TApp.Configs;
using TApp.Utils;
using TTManager.Masan;
using TApp.Helpers.Masan_Backup;
using TApp.Helpers;

namespace TApp.Views.Extention
{
    public partial class FExtention : UIPage
    {
        public FExtention()
        {
            InitializeComponent();
        }
        private string BackupLogDbPath = @"C:/MASAN/CloudBackupLog.tls";

        public void InitializeERP()
        {
            erP_Google1.credentialPath = AppConfigs.Current.credentialERPPath;
            erP_Google1.SUB_INV = AppConfigs.Current.ERP_Sub_Inv;
            erP_Google1.ORG_CODE = AppConfigs.Current.ERP_Org_Code;
            erP_Google1.DatasetID = AppConfigs.Current.ERP_DatasetID;
            erP_Google1.TableID = AppConfigs.Current.ERP_TableID;
            erP_Google1.LineName = AppConfigs.Current.Line_Name;

            if(AppConfigs.Current.Cloud_Connection_Enabled)
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
        }

        int countSync = 100000;
        int maxInterval = 5;
        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!backgroundWorker2.CancellationPending)
            {
                maxInterval = (AppConfigs.Current.Cloud_Refresh_Interval_Minute*60* 1000)/500;
                countSync++;
                this.InvokeIfRequired(() =>
                {
                    opC1.Text = (maxInterval - countSync).ToString();
                });

                if (countSync >= maxInterval)
                {
                    countSync = 0;

                    this.InvokeIfRequired(() =>
                    {
                        opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÔNG BÁO] Bắt đầu tải dữ liệu lên máy chủ theo chu kì...");
                    });

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

                    TResult result = QRDatabaseHelper.Get_ActiveQR_By_TimeUnix(lastUnix);
                    //chuyển dữ liệu sang csv
                    if(!result.issuccess)
                    {
                        //lỗi lấy dữ liệu
                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Lấy dữ liệu cần sao lưu thất bại: {result.message}");
                        });
                        continue;
                    }

                    DataTable dataToBackup = result.data!;

                    string csvPath = Path.Combine(Path.GetTempPath(), $"CloudBackup_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");

                    ExportResult exportResult = CsvHelper.ExportDataTableToCsv(dataToBackup, csvPath);

                    if (!exportResult.IsSucces)
                    {
                        this.InvokeIfRequired(() =>
                        {
                            opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [LỖI] Xuất dữ liệu sang CSV thất bại: {exportResult.Message}");
                        });
                        continue;
                    }
                    //báo thành công
                    this.InvokeIfRequired(() =>
                    {
                        opConsole.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffk")} - [THÀNH CÔNG] Xuất dữ liệu sang CSV thành công: {csvPath}");
                    });
                }
                Thread.Sleep(500);
            }
        }
    } 
}
