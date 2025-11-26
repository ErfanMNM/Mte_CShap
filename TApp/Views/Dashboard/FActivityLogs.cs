using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using TApp.Infrastructure;
using TApp.Helpers;
using MTs.Auditrails;

namespace TApp.Views.Dashboard
{
    public partial class FActivityLogs : UIPage
    {
        public FActivityLogs()
        {
            InitializeComponent();
        }

        int size = 50; // Số lượng bản ghi mỗi trang

        public override void Init()
        {
            base.Init();

            // Ghi log mở trang
            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Mở trang xem nhật ký hoạt động",
                "",
                "UA-ACTLOG-01"
            );

            if (!WK_AutoLog.IsBusy)
            {
                WK_AutoLog.RunWorkerAsync();
            }

            // Thiết lập ComboBox loại log
            ipLogType.Items.Clear();
            ipLogType.Items.Add("Tất cả");
            foreach (e_LogType logType in Enum.GetValues(typeof(e_LogType)))
            {
                ipLogType.Items.Add(logType.ToString());
            }
            ipLogType.SelectedIndex = 0;

            uiPagination1.ActivePage = 1;
            size = Convert.ToInt32(ipSize.Text);

            // Gán mặc định -7 ngày đến hiện tại
            ipDateFrom.Value = DateTime.Now.AddDays(-7);
            ipDateTo.Value = DateTime.Now;
        }

        private void uiPagination1_PageChanged(object sender, object pagingSource, int pageIndex, int count)
        {
            if (!WK_Getlogs.IsBusy)
            {
                WK_Getlogs.RunWorkerAsync();
            }
        }

        private void WK_AutoLog_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_AutoLog.CancellationPending)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        string selectedLogType = "Tất cả";
        int LogCount = 0;

        private void btnGetLogs_Click(object sender, EventArgs e)
        {
            // Ghi log người dùng nhấn nút lấy nhật ký
            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Lấy nhật ký hoạt động",
                $"{{'LogType':'{selectedLogType}','DateFrom':'{ipDateFrom.Value:yyyy-MM-dd}','DateTo':'{ipDateTo.Value:yyyy-MM-dd}'}}",
                "UA-ACTLOG-02"
            );

            btnGetLogs.Enabled = false;

            if (!WK_Getlogs.IsBusy)
            {
                WK_Getlogs.RunWorkerAsync();
            }
        }

        DataTable LogsData;
        DateTime DateFrom;
        DateTime DateTo;
        bool getALL = false;

        private void WK_Getlogs_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    btnGetLogs.Enabled = false;
                    ipSize.Enabled = false;
                    selectedLogType = ipLogType.SelectedItem?.ToString() ?? "Tất cả";
                    DateFrom = ipDateFrom.Value;
                    DateTo = ipDateTo.Value;
                }));

                // Lấy logs từ GlobalVarialbles.Logger
                List<LogEntry<e_LogType>> logEntries;

                if (selectedLogType == "Tất cả")
                {
                    // Lấy tất cả logs trong khoảng thời gian
                    logEntries = GlobalVarialbles.Logger.GetLogsByTime(DateFrom, DateTo);
                }
                else
                {
                    // Lọc theo loại log
                    if (Enum.TryParse(selectedLogType, out e_LogType logType))
                    {
                        var allLogs = GlobalVarialbles.Logger.GetLogsByTime(DateFrom, DateTo);
                        logEntries = allLogs.Where(log => log.Action.Equals(logType)).ToList();
                    }
                    else
                    {
                        logEntries = GlobalVarialbles.Logger.GetLogsByTime(DateFrom, DateTo);
                    }
                }

                // Chuyển đổi List sang DataTable
                LogsData = ConvertToDataTable(logEntries);
                LogCount = LogsData?.Rows.Count ?? 0;

                // Phân trang
                if (!getALL && LogsData != null && LogsData.Rows.Count > 0)
                {
                    int skip = (uiPagination1.ActivePage - 1) * size;
                    var filteredRows = LogsData.AsEnumerable().Skip(skip).Take(size);
                    if (filteredRows.Any())
                    {
                        LogsData = filteredRows.CopyToDataTable();
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    btnGetLogs.Enabled = true;
                    ipSize.Enabled = true;
                    this.ShowErrorTip($"Lỗi khi lấy nhật ký: {ex.Message}", 2000);

                    // Ghi log lỗi
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "Lỗi lấy nhật ký hoạt động",
                        ex.Message,
                        "ERR-ACTLOG-01"
                    );
                }));
                return;
            }
        }

        private DataTable ConvertToDataTable(List<LogEntry<e_LogType>> logEntries)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Thời gian", typeof(string));
            dt.Columns.Add("Người dùng", typeof(string));
            dt.Columns.Add("Mã", typeof(string));
            dt.Columns.Add("Loại", typeof(string));
            dt.Columns.Add("Mô tả", typeof(string));
            dt.Columns.Add("Thông số", typeof(string));

            foreach (var entry in logEntries)
            {
                dt.Rows.Add(
                    entry.Id,
                    entry.TimeISO,
                    entry.User,
                    entry.Code,
                    entry.Action.ToString(),
                    entry.Description,
                    entry.JsonParams
                );
            }

            return dt;
        }

        private void WK_Getlogs_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.ShowSuccessTip("Lấy nhật ký hoạt động thành công", 2000);

            opTotalCount.Text = LogCount.ToString();
            btnGetLogs.Enabled = true;
            ipSize.Enabled = true;
            opDataG.DataSource = LogsData;
            uiPagination1.TotalCount = LogCount;
            uiPagination1.PageSize = size;
        }

        private void FActivityLogs_Initialize(object sender, EventArgs e)
        {
            // Đã ghi log trong Init()
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            try
            {
                string savePath = FileHelper.Get_Save_File_Path();
                if (!string.IsNullOrEmpty(savePath))
                {
                    var ex_pdf = ReportClass.ExportReportToPDF(opDataG.DataSource as DataTable, savePath, "Nhật ký hoạt động");

                    if (ex_pdf.IsSucces)
                    {
                        // Ghi log xuất báo cáo thành công
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.UserAction,
                            "Xuất báo cáo nhật ký hoạt động PDF thành công",
                            $"{{'HashCode':'{ex_pdf.HashCode}','FilePath':'{ex_pdf.FilePath}'}}",
                            "UA-ACTLOG-03"
                        );

                        this.ShowSuccessTip($"Xuất báo cáo thành công! {ex_pdf.FilePath}", 2000);
                    }
                    else
                    {
                        this.ShowErrorTip("Xuất báo cáo thất bại!", 2000);
                    }
                }
                else
                {
                    this.ShowErrorTip("Bạn đã hủy xuất báo cáo", 2000);
                }
            }
            catch (Exception ex)
            {
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Error,
                    "Lỗi xuất báo cáo nhật ký hoạt động",
                    ex.Message,
                    "ERR-ACTLOG-02"
                );
                this.ShowErrorTip($"Lỗi xuất báo cáo: {ex.Message}", 2000);
            }
        }

        private void ipSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                size = Convert.ToInt32(ipSize.Text);

                if (!WK_Getlogs.IsBusy)
                {
                    WK_Getlogs.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorTip($"Lỗi khi thay đổi kích thước trang: {ex.Message}", 2000);
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                string savePath = FileHelper.Get_Save_File_Path_CSV();
                if (!string.IsNullOrEmpty(savePath))
                {
                    var ex_csv = CsvHelper.ExportDataTableToCsv(opDataG.DataSource as DataTable, savePath);
                    if (ex_csv.IsSucces)
                    {
                        // Ghi log xuất CSV thành công
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.UserAction,
                            "Xuất tệp CSV báo cáo nhật ký hoạt động",
                            $"{{'FilePath':'{savePath}'}}",
                            "UA-ACTLOG-04"
                        );

                        this.ShowSuccessTip($"Xuất tệp CSV thành công! {savePath}", 2000);
                    }
                    else
                    {
                        this.ShowErrorTip("Xuất CSV thất bại!", 2000);
                    }
                }
                else
                {
                    this.ShowErrorTip("Bạn đã hủy xuất báo cáo", 2000);
                }
            }
            catch (Exception ex)
            {
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Error,
                    "Lỗi xuất CSV nhật ký hoạt động",
                    ex.Message,
                    "ERR-ACTLOG-03"
                );
                this.ShowErrorTip($"Lỗi xuất báo cáo: {ex.Message}", 2000);
            }
        }

        private void btnGetAll_Click(object sender, EventArgs e)
        {
            if (this.ShowAskDialog("Bạn có chắc chắn tải hết dữ liệu? Máy sẽ có nguy cơ bị treo vài phút"))
            {
                getALL = true;
                if (!WK_Getlogs.IsBusy)
                {
                    WK_Getlogs.RunWorkerAsync();
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Làm mới dữ liệu
            if (!WK_Getlogs.IsBusy)
            {
                WK_Getlogs.RunWorkerAsync();
            }
        }
    }
}
