using MTs.Communication;
using Sunny.UI;
using System.ComponentModel;
using System.Data;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Utils;
using TTManager.Diaglogs;
using TTManager.Masan;

namespace TApp.Views.Extention
{
    public partial class FScan : UIPage
    {
        #region Fields
        private SerialClientHelper _datalogicScanner;
        #endregion

        #region Constructor & Initialization
        public FScan()
        {
            InitializeComponent();
        }

        private void FScan_Initialize(object sender, EventArgs e)
        {
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Mở trang quét tra cứu mã", "", "UA-FSCAN-01");
            InitializeScanner();
        }

        public void InitializeScanner()
        {
            if (string.IsNullOrEmpty(AppConfigs.Current.Handheld_COM_Port))
            {
                this.ShowWarningTip("Chưa cấu hình cổng COM cho máy quét cầm tay.");
                return;
            }
            _datalogicScanner = new SerialClientHelper(AppConfigs.Current.Handheld_COM_Port, 9600);
            _datalogicScanner.SerialClientCallback += DatalogicScanner_SerialClientCallback;
            _datalogicScanner.Connect();
        }
        #endregion

        #region Scanner Handling
        private void DatalogicScanner_SerialClientCallback(SerialClientState state, string data)
        {
            if (state == SerialClientState.Received)
            {
                this.InvokeIfRequired(() =>
                {
                    ipQRContent.Text = data.Trim();
                    FindData(data.Trim());
                });
            }
        }
        #endregion

        #region UI Event Handlers
        private void ipQRContent_DoubleClick(object sender, EventArgs e)
        {
            using (var enterText = new Entertext())
            {
                enterText.TileText = "Nhập toàn bộ mã QR hoặc 1 phần mã";
                enterText.TextValue = ipQRContent.Text;
                enterText.IsPassword = false;
                enterText.EnterClicked += (s, args) =>
                {
                    ipQRContent.Text = enterText.TextValue;
                    FindData(enterText.TextValue);
                };
                enterText.ShowDialog();
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            string qrCode = ipQRContent.Text.Trim();
            if (string.IsNullOrWhiteSpace(qrCode))
            {
                UpdateStatusLabel(ScanStatus.Warning, "Vui lòng nhập mã QR!");
                return;
            }
            FindData(qrCode);
        }
        #endregion

        #region Background Worker & Data Logic
        private enum ScanStatus { Searching, Success, SuccessButNotActive, NotFound, Error, Warning }

        private class ScanResult
        {
            public ScanStatus Status { get; set; }
            public DataTable Data { get; set; }
            public string TimeStamp { get; set; }
            public string Message { get; set; }
            public string ScannedCode { get; set; }
        }

        private void FindData(string qrCode)
        {
            if (WK_Find.IsBusy)
            {
                UpdateStatusLabel(ScanStatus.Warning, "Đang xử lý yêu cầu trước đó...");
                return;
            }
            UpdateStatusLabel(ScanStatus.Searching, "Đang tìm kiếm...");
            WK_Find.RunWorkerAsync(qrCode);
        }

        private void WK_Find_DoWork(object sender, DoWorkEventArgs e)
        {
            string qrCode = e.Argument as string;
            var result = new ScanResult { ScannedCode = qrCode };

            TResult resultAll = QRDatabaseHelper.GetByQRContent(qrCode);
            if (!resultAll.issuccess)
            {
                result.Status = ScanStatus.Error;
                result.Message = $"Lỗi truy xuất database: {resultAll.message}";
                e.Result = result;
                return;
            }

            if (resultAll.data.Rows.Count == 0)
            {
                result.Status = ScanStatus.NotFound;
                result.Message = $"Mã: {qrCode} không tồn tại trong hệ thống.";
                e.Result = result;
                return;
            }

            result.Data = resultAll.data;
            AnalyzeScanResults(result, qrCode);
            e.Result = result;
        }

        private void AnalyzeScanResults(ScanResult result, string qrCode)
        {
            TResult resultActive = QRDatabaseHelper.GetActiveByQRContent(qrCode);
            if (resultActive.issuccess && resultActive.data.Rows.Count > 0)
            {
                result.Status = ScanStatus.Success;
                result.Message = "Mã đã được kích hoạt.";
                result.TimeStamp = resultActive.data.Rows[0]["TimeStampActive"].ToString();
            }
            else
            {
                string statusInMainDb = result.Data.Rows[0]["Status"].ToString();
                if (statusInMainDb.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = ScanStatus.SuccessButNotActive;
                    result.Message = "Mã hợp lệ nhưng chưa được kích hoạt.";
                }
                else
                {
                    result.Status = ScanStatus.Warning;
                    result.Message = $"Mã có trạng thái: {statusInMainDb}";
                }
                result.TimeStamp = result.Data.Rows[0]["TimeStampActive"].ToString();
            }
        }

        private void WK_Find_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is ScanResult result)
            {
                UpdateStatusLabel(result.Status, result.Message);
                opInfoTable.DataSource = result.Data;
                opTime.Text = result.TimeStamp;

                if (result.Data != null)
                {
                    foreach (DataGridViewColumn column in opInfoTable.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                }

                if (result.Status != ScanStatus.Searching)
                {
                    opConsole.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {result.Message}");
                }

                if (result.Status == ScanStatus.Success || result.Status == ScanStatus.SuccessButNotActive)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Tìm kiếm mã thành công", $"{{'QR':'{result.ScannedCode}', 'Count':'{result.Data?.Rows.Count ?? 0}'}}", "UA-FSCAN-02");
                }
            }
        }
        #endregion

        #region UI Update Methods
        private void UpdateStatusLabel(ScanStatus status, string text)
        {
            Color backColor, foreColor;
            int symbol;

            switch (status)
            {
                case ScanStatus.Searching:
                    backColor = Color.Gainsboro;
                    foreColor = Color.Black;
                    symbol = 61761; // Loading symbol
                    break;
                case ScanStatus.Success:
                    backColor = Color.LimeGreen;
                    foreColor = Color.White;
                    symbol = 61527; // Success symbol
                    break;
                case ScanStatus.SuccessButNotActive:
                    backColor = Color.Orange;
                    foreColor = Color.White;
                    symbol = 61527; // Warning symbol
                    break;
                case ScanStatus.NotFound:
                case ScanStatus.Error:
                    backColor = Color.Red;
                    foreColor = Color.White;
                    symbol = 61453; // Error symbol
                    break;
                case ScanStatus.Warning:
                default:
                    backColor = Color.DarkOrange;
                    foreColor = Color.White;
                    symbol = 61527; // Warning symbol
                    break;
            }

            this.InvokeIfRequired(() =>
            {
                opStatus.Text = text;
                opStatus.BackColor = backColor;
                opStatus.ForeColor = foreColor;
                opStatus.Symbol = symbol;
                opStatus.SymbolColor = foreColor;
            });
        }
        #endregion
    }
}
