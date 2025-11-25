using Sunny.UI;
using System.ComponentModel;
using System.Data;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Models;
using TApp.Utils;
using TApp.Views.Dashboard;
using TTManager.Diaglogs;
using Timer = System.Threading.Timer;

namespace TApp.Views.Extention
{
    public partial class FAddCode : UIPage
    {
        private int _totalAdded = 0;
        private int _totalSuccess = 0;
        private Timer _queueMonitor;
        private List<QRProductRecord> _pendingRecords = new List<QRProductRecord>();

        public FAddCode()
        {
            InitializeComponent();
            InitializeQueueMonitor();
        }

        private void InitializeQueueMonitor()
        {
            // Timer để cập nhật số lượng queue mỗi 500ms
            _queueMonitor = new Timer();
            _queueMonitor.Interval = 500;
            _queueMonitor.Tick += QueueMonitor_Tick;
            _queueMonitor.Start();
        }

        private void QueueMonitor_Tick(object sender, EventArgs e)
        {
            UpdateQueueDisplay();
        }

        private void UpdateQueueDisplay()
        {
            this.InvokeIfRequired(() =>
            {
                opQueueCount.Text = FDashboard.FD_Globals.QueueActive.Count.ToString();

                // Cập nhật danh sách pending records
                if (_pendingRecords.Count > 0)
                {
                    var dt = new DataTable();
                    dt.Columns.Add("STT", typeof(int));
                    dt.Columns.Add("Mã QR", typeof(string));
                    dt.Columns.Add("Batch", typeof(string));
                    dt.Columns.Add("Barcode", typeof(string));
                    dt.Columns.Add("Thời gian", typeof(string));

                    int index = 1;
                    foreach (var record in _pendingRecords.TakeLast(50))
                    {
                        dt.Rows.Add(index++, record.QRContent, record.BatchCode, record.Barcode, record.TimeStampActive);
                    }

                    opQueueTable.DataSource = dt;

                    // Tự động resize columns
                    foreach (DataGridViewColumn column in opQueueTable.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                }
            });
        }

        private void ipQRContent_DoubleClick(object sender, EventArgs e)
        {
            // Bật bàn phím ảo
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập mã QR cần thêm vào hệ thống";
                enterText.TextValue = ipQRContent.Text;
                enterText.IsPassword = false;
                enterText.EnterClicked += (s, args) =>
                {
                    ipQRContent.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }

        private void ipQRContent_KeyDown(object sender, KeyEventArgs e)
        {
            // Cho phép thêm mã bằng phím Enter
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAdd_Click(sender, e);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ipQRContent.Text))
            {
                UpdateStatus("Vui lòng nhập mã QR!", Color.Orange, 61527);
                return;
            }

            // Kiểm tra nếu WK_Add đang chạy
            if (WK_Add.IsBusy)
            {
                UpdateStatus("Đang xử lý...", Color.Gainsboro, 61761);
                return;
            }

            WK_Add.RunWorkerAsync(ipQRContent.Text.Trim());
        }

        private void WK_Add_DoWork(object sender, DoWorkEventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                UpdateStatus("Đang xử lý...", Color.Gainsboro, 61761);
            });

            string qrCode = e.Argument as string;

            if (string.IsNullOrWhiteSpace(qrCode))
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Mã QR không hợp lệ!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] Mã QR rỗng hoặc không hợp lệ", Color.Red);
                });
                return;
            }

            // Kiểm tra mã đã tồn tại trong ActiveSet chưa
            if (FDashboard.FD_Globals.ActiveSet.Contains(qrCode))
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Mã đã tồn tại!", Color.Orange, 61527);
                    AddConsoleLog($"[CẢNH BÁO] Mã {qrCode} đã tồn tại trong hệ thống", Color.Orange);
                });
                return;
            }

            // Lấy thông tin batch hiện tại
            string currentBatch = FDashboard.FD_Globals.productionData.BatchCode;
            string currentBarcode = FDashboard.FD_Globals.productionData.Barcode;

            // Kiểm tra batch có hợp lệ không
            if (string.IsNullOrWhiteSpace(currentBatch) || currentBatch == "NNN")
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Chưa có thông tin lô sản xuất!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] Chưa thiết lập lô sản xuất. Vui lòng đổi lô trước khi thêm mã", Color.Red);
                });
                return;
            }

            try
            {
                // Tạo record mới
                var record = new QRProductRecord
                {
                    QRContent = qrCode,
                    Status = e_Production_Status.Pass,
                    BatchCode = currentBatch,
                    Barcode = currentBarcode,
                    UserName = GlobalVarialbles.CurrentUser.Username,
                    TimeStampActive = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"),
                    TimeUnixActive = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    ProductionDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssK"),
                    Reason = "Manual Add"
                };

                // Thêm vào QueueActive để xử lý trong luồng riêng
                FDashboard.FD_Globals.QueueActive.Enqueue(record);

                // Thêm vào ActiveSet để tránh trùng lặp
                FDashboard.FD_Globals.ActiveSet.Add(qrCode);

                // Thêm vào danh sách pending để hiển thị
                this.InvokeIfRequired(() =>
                {
                    _pendingRecords.Add(record);
                    _totalAdded++;
                    _totalSuccess++;

                    UpdateStatus("Thêm mã thành công!", Color.LimeGreen, 61527);
                    AddConsoleLog($"[OK] Đã thêm mã: {qrCode.Substring(0, Math.Min(30, qrCode.Length))}...", Color.Green);

                    // Xóa textbox để sẵn sàng nhập mã tiếp theo
                    ipQRContent.Text = string.Empty;
                    ipQRContent.Focus();

                    // Ghi log
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.UserAction,
                        "Thêm mã kích hoạt thủ công",
                        $"{{'QRContent':'{qrCode}','BatchCode':'{currentBatch}','Barcode':'{currentBarcode}'}}",
                        "UA-FADD-01"
                    );
                });
            }
            catch (Exception ex)
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Lỗi xử lý!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] Không thể thêm mã: {ex.Message}", Color.Red);

                    // Ghi log lỗi
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "Lỗi thêm mã kích hoạt thủ công",
                        ex.Message,
                        "ERR-FADD-01"
                    );
                });
            }
        }

        private void UpdateStatus(string text, Color color, int symbol)
        {
            this.InvokeIfRequired(() =>
            {
                opStatus.Text = text;
                opStatus.BackColor = color;
                opStatus.Symbol = symbol;
                opStatus.SymbolColor = color == Color.Gainsboro ? Color.Black : Color.White;
                opStatus.ForeColor = color == Color.Gainsboro ? Color.Black : Color.White;
            });
        }

        private void AddConsoleLog(string message, Color color)
        {
            this.InvokeIfRequired(() =>
            {
                string timeStamp = DateTime.Now.ToString("HH:mm:ss");
                opConsole.Items.Insert(0, $"[{timeStamp}] {message}");

                if (opConsole.Items.Count > 100)
                {
                    opConsole.Items.RemoveAt(opConsole.Items.Count - 1);
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (_queueMonitor != null)
                {
                    _queueMonitor.Stop();
                    _queueMonitor.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
