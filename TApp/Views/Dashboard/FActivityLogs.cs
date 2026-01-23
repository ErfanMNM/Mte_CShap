using TTManager.Audit;
using Sunny.UI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using TApp.Helpers;
using TApp.Infrastructure;

namespace TApp.Views.Dashboard
{
    public partial class FActivityLogs : UIPage
    {
        #region Fields
        private int _pageSize = 50;
        #endregion

        #region Constructor & Page Initialization
        public FActivityLogs()
        {
            InitializeComponent();
        }

        public override void Init()
        {
            base.Init();
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Mở trang xem nhật ký hoạt động", "", "UA-ACTLOG-01");

            SetupLogTypeComboBox();
            ipDateFrom.Value = DateTime.Now.AddDays(-7);
            ipDateTo.Value = DateTime.Now;
            _pageSize = int.TryParse(ipSize.Text, out int size) ? size : 50;
            uiPagination1.ActivePage = 1;
        }

        private void SetupLogTypeComboBox()
        {
            ipLogType.Items.Clear();
            ipLogType.Items.Add("Tất cả");
            foreach (e_LogType logType in Enum.GetValues(typeof(e_LogType)))
            {
                ipLogType.Items.Add(logType.ToString());
            }
            ipLogType.SelectedIndex = 0;
        }
        #endregion

        #region UI Event Handlers
        private void btnGetLogs_Click(object sender, EventArgs e)
        {
            FetchLogs();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FetchLogs();
        }

        private void uiPagination1_PageChanged(object sender, object pagingSource, int pageIndex, int count)
        {
            FetchLogs();
        }

        private void ipSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ipSize.Text, out int size))
            {
                _pageSize = size;
                uiPagination1.ActivePage = 1; // Reset to first page
                FetchLogs();
            }
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            ExportData("PDF", FileHelper.Get_Save_File_Path, (dt, path) => ReportClass.ExportReportToPDF(dt, path, "Nhật ký hoạt động"), "UA-ACTLOG-03", "ERR-ACTLOG-02");
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            ExportData("CSV", FileHelper.Get_Save_File_Path_CSV, CsvHelper.ExportDataTableToCsv, "UA-ACTLOG-04", "ERR-ACTLOG-03");
        }

        private void btnGetAll_Click(object sender, EventArgs e)
        {
            if (this.ShowAskDialog("Bạn có chắc chắn tải hết dữ liệu? Hành động này có thể khiến ứng dụng bị treo trong giây lát."))
            {
                FetchLogs(fetchAll: true);
            }
        }
        #endregion

        #region Background Worker Handlers
        private class LogFilterCriteria
        {
            public string LogType { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public bool FetchAll { get; set; }
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
        }

        private void WK_Getlogs_DoWork(object sender, DoWorkEventArgs e)
        {
            var criteria = e.Argument as LogFilterCriteria;
            try
            {
                var logEntries = FetchAndFilterLogs(criteria.DateFrom, criteria.DateTo, criteria.LogType);
                var fullDataTable = ConvertToDataTable(logEntries);
                e.Result = new { FullData = fullDataTable, Criteria = criteria };
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void WK_Getlogs_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnGetLogs.Enabled = true;
            ipSize.Enabled = true;

            if (e.Result is Exception ex)
            {
                this.ShowErrorTip($"Lỗi khi lấy nhật ký: {ex.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi lấy nhật ký hoạt động", ex.Message, "ERR-ACTLOG-01");
                return;
            }

            var result = (dynamic)e.Result;
            DataTable fullDataTable = result.FullData;
            LogFilterCriteria criteria = result.Criteria;
            int totalCount = fullDataTable.Rows.Count;

            DataTable pagedData = criteria.FetchAll ? fullDataTable : ApplyPagination(fullDataTable, criteria.PageIndex, criteria.PageSize);

            opDataG.DataSource = pagedData;
            opTotalCount.Text = totalCount.ToString();
            uiPagination1.TotalCount = totalCount;
            uiPagination1.PageSize = criteria.PageSize;

            this.ShowSuccessTip("Lấy nhật ký hoạt động thành công");
        }
        #endregion

        #region Data & Export Logic
        private void FetchLogs(bool fetchAll = false)
        {
            if (WK_Getlogs.IsBusy) return;

            btnGetLogs.Enabled = false;
            ipSize.Enabled = false;

            var criteria = new LogFilterCriteria
            {
                LogType = ipLogType.SelectedItem?.ToString() ?? "Tất cả",
                DateFrom = ipDateFrom.Value,
                DateTo = ipDateTo.Value,
                FetchAll = fetchAll,
                PageIndex = uiPagination1.ActivePage,
                PageSize = _pageSize
            };

            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Lấy nhật ký hoạt động", $"{{'LogType':'{criteria.LogType}','DateFrom':'{criteria.DateFrom:yyyy-MM-dd}','DateTo':'{criteria.DateTo:yyyy-MM-dd}'}}", "UA-ACTLOG-02");
            WK_Getlogs.RunWorkerAsync(criteria);
        }

        private List<LogEntry<e_LogType>> FetchAndFilterLogs(DateTime from, DateTime to, string logTypeString)
        {
            var allLogs = GlobalVarialbles.Logger.GetLogsByTime(from, to);
            if (logTypeString == "Tất cả" || !Enum.TryParse(logTypeString, out e_LogType logType))
            {
                return allLogs;
            }
            return allLogs.Where(log => log.Action.Equals(logType)).ToList();
        }

        private DataTable ApplyPagination(DataTable source, int pageIndex, int pageSize)
        {
            if (source == null || source.Rows.Count == 0) return source;
            int skip = (pageIndex - 1) * pageSize;
            var pagedRows = source.AsEnumerable().Skip(skip).Take(pageSize);
            return pagedRows.Any() ? pagedRows.CopyToDataTable() : source.Clone();
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

            foreach (var entry in logEntries.OrderByDescending(log => log.Id))
            {
                dt.Rows.Add(entry.Id, entry.TimeISO, entry.User, entry.Code, entry.Action.ToString(), entry.Description, entry.JsonParams);
            }
            return dt;
        }

        private void ExportData(string exportType, Func<string> getSavePathFunc, Func<DataTable, string, ExportResult> exportFunc, string successLogCode, string errorLogCode)
        {
            try
            {
                string savePath = getSavePathFunc();
                if (string.IsNullOrEmpty(savePath))
                {
                    this.ShowInfoTip("Đã hủy xuất báo cáo");
                    return;
                }

                var exportResult = exportFunc(opDataG.DataSource as DataTable, savePath);
                if (exportResult.IsSucces)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, $"Xuất báo cáo {exportType} thành công", $"{{'FilePath':'{exportResult.FilePath}'}}", successLogCode);
                    this.ShowSuccessTip($"Xuất báo cáo thành công! {exportResult.FilePath}");
                }
                else
                {
                    this.ShowErrorTip($"Xuất báo cáo thất bại: {exportResult.Message}");
                }
            }
            catch (Exception ex)
            {
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, $"Lỗi xuất báo cáo {exportType}", ex.Message, errorLogCode);
                this.ShowErrorTip($"Lỗi xuất báo cáo: {ex.Message}");
            }
        }
        #endregion
    }
}
