using MTs.Communication;
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
using Timer = System.Windows.Forms.Timer;

namespace TApp.Views.Extention
{
    public partial class FAddCode : UIPage
    {
        #region Fields
        private int _totalAdded = 0;
        private int _totalSuccess = 0;
        private Timer _queueMonitor;
        private readonly List<QRProductRecord> _pendingRecords = new List<QRProductRecord>();
        private SerialClientHelper _datalogicScanner;
        #endregion

        #region Constructor & Initialization
        public FAddCode()
        {
            InitializeComponent();
            InitializeQueueMonitor();
        }

        private void FAddCode_Initialize(object sender, EventArgs e)
        {
            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Mở trang thêm mã thủ công",
                "",
                "UA-FADDCODE-01"
            );
            InitializeScanner();
        }

        private void FAddCode_Finalize(object sender, EventArgs e)
        {
            DisconnectScanner();
        }

        private void InitializeQueueMonitor()
        {
            _queueMonitor = new Timer
            {
                Interval = 500
            };
            _queueMonitor.Tick += QueueMonitor_Tick;
            _queueMonitor.Start();
        }

        public void InitializeScanner()
        {
            if (string.IsNullOrEmpty(AppConfigs.Current.Handheld_COM_Port))
            {
                AddConsoleLog("[CẢNH BÁO] Chưa cấu hình cổng COM cho máy quét cầm tay.", Color.Orange);
                AddConsoleLog("[THÔNG BÁO] Vui lòng cấu hình cổng COM trong Settings để sử dụng máy quét.", Color.Orange);
                return;
            }

            try
            {
                AddConsoleLog($"[THÔNG BÁO] Đang khởi tạo kết nối máy quét tại {AppConfigs.Current.Handheld_COM_Port}...", Color.Gainsboro);
                _datalogicScanner = new SerialClientHelper(AppConfigs.Current.Handheld_COM_Port, 9600);
                _datalogicScanner.SerialClientCallback += DatalogicScanner_SerialClientCallback;
                _datalogicScanner.Connect();
            }
            catch (Exception ex)
            {
                AddConsoleLog($"[LỖI] Không thể khởi tạo máy quét: {ex.Message}", Color.Red);
                AddConsoleLog("[THÔNG BÁO] Bạn vẫn có thể nhập mã thủ công hoặc quét bằng máy quét khác.", Color.Orange);
            }
        }

        private void DatalogicScanner_SerialClientCallback(SerialClientState state, string data)
        {
            switch (state)
            {
                case SerialClientState.Connected:
                    AddConsoleLog($"[THÀNH CÔNG] Đã kết nối máy quét tại {AppConfigs.Current.Handheld_COM_Port}", Color.Green);
                    break;
                case SerialClientState.Disconnected:
                    AddConsoleLog($"[NGẮT KẾT NỐI] Máy quét đã ngắt kết nối: {data}", Color.Orange);
                    break;
                case SerialClientState.Received:
                    this.InvokeIfRequired(() =>
                    {
                        ipQRContent.Text = data.Trim();
                        btnAdd_Click(null, null);
                    });
                    break;
                case SerialClientState.Error:
                    AddConsoleLog($"[LỖI] Lỗi máy quét: {data}", Color.Red);
                    break;
            }
        }

        private void DisconnectScanner()
        {
            if (_datalogicScanner != null)
            {
                try
                {
                    if (_datalogicScanner.Connected)
                    {
                        _datalogicScanner.Disconnect();
                    }
                    _datalogicScanner.SerialClientCallback -= DatalogicScanner_SerialClientCallback;
                    _datalogicScanner = null;
                    AddConsoleLog("[THÔNG BÁO] Đã ngắt kết nối máy quét", Color.Orange);
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không throw để đảm bảo ngắt kết nối hoàn tất
                    System.Diagnostics.Debug.WriteLine($"Error disconnecting scanner: {ex.Message}");
                }
            }
        }
        #endregion

        #region UI Event Handlers
        private void ipQRContent_DoubleClick(object sender, EventArgs e)
        {
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

            if (WK_Add.IsBusy)
            {
                UpdateStatus("Đang xử lý...", Color.Gainsboro, 61761);
                return;
            }

            WK_Add.RunWorkerAsync(ipQRContent.Text.Trim());
        }
        #endregion

        #region Background Worker Methods
        private void WK_Add_DoWork(object sender, DoWorkEventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                UpdateStatus("Đang xử lý...", Color.Gainsboro, 61761);
            });

            string qrCode = e.Argument as string;

            if (!ValidateQRCodeInput(qrCode))
            {
                return;
            }

            if (!ValidateBatchInformation())
            {
                return;
            }

            ProcessQRCodeAddition(qrCode);
        }

        private bool ValidateQRCodeInput(string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode))
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Mã QR không hợp lệ!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] Mã QR rỗng hoặc không hợp lệ", Color.Red);
                });
                return false;
            }

            if (FD_Globals.ActiveSet.Contains(qrCode))
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Mã đã tồn tại!", Color.Orange, 61527);
                    AddConsoleLog($"[CẢNH BÁO] Mã {qrCode} đã tồn tại trong hệ thống", Color.Orange);
                });
                return false;
            }

            if (qrCode.Length < 16)
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Mã sai định dạng!", Color.Red, 61453);
                    AddConsoleLog(qrCode.Contains(FD_Globals.productionData.Barcode) ?
                        $"[LỖI] Có thể bạn đã quét nhầm mã vạch, vui lòng che lại rồi quét" :
                        $"[LỖI] Vui lòng quét mã đúng định dạng", Color.Red);
                });
                return false;
            }

            if (!qrCode.Contains(FD_Globals.productionData.Barcode))
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Mã sai định dạng!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] Mã không chứa mã vạch sản phẩm hiện tại", Color.Red);
                });
                return false;
            }
            return true;
        }

        private bool ValidateBatchInformation()
        {
            string currentBatch = FD_Globals.productionData.BatchCode;
            if (string.IsNullOrWhiteSpace(currentBatch) || currentBatch == "NNN")
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Chưa có thông tin lô sản xuất!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] Chưa thiết lập lô sản xuất. Vui lòng đổi lô trước khi thêm mã", Color.Red);
                });
                return false;
            }
            return true;
        }

        private void ProcessQRCodeAddition(string qrCode)
        {
            string currentBatch = FD_Globals.productionData.BatchCode;
            string currentBarcode = FD_Globals.productionData.Barcode;

            try
            {
                var record = new QRProductRecord
                {
                    QRContent = qrCode,
                    Status = e_Production_Status.Pass,
                    BatchCode = currentBatch,
                    Barcode = currentBarcode,
                    UserName = GlobalVarialbles.CurrentUser.Username + "Quét tay",
                    TimeStampActive = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK"),
                    TimeUnixActive = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    ProductionDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ssK"),
                    Reason = "Manual Add"
                };

                FD_Globals.QueueActive.Enqueue(record);
                FD_Globals.QueueRecord.Enqueue(record);
                FD_Globals.ActiveSet.Add(qrCode);

                this.InvokeIfRequired(() =>
                {
                    _pendingRecords.Add(record);
                    _totalAdded++;
                    _totalSuccess++;

                    UpdateStatus("Thêm mã thành công!", Color.LimeGreen, 61527);
                    AddConsoleLog($"[OK] Đã thêm mã: {qrCode.Substring(0, Math.Min(30, qrCode.Length))}...", Color.Green);

                    ipQRContent.Text = string.Empty;
                    ipQRContent.Focus();

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
                HandleQRCodeAdditionError(ex, qrCode);
            }
        }

        private void HandleQRCodeAdditionError(Exception ex, string qrCode)
        {
            this.InvokeIfRequired(() =>
            {
                UpdateStatus("Lỗi xử lý!", Color.Red, 61453);
                AddConsoleLog($"[LỖI] Không thể thêm mã: {ex.Message}", Color.Red);

                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Error,
                    "Lỗi thêm mã kích hoạt thủ công",
                    ex.Message,
                    "ERR-FADD-01"
                );
            });
        }
        #endregion

        #region Queue Management
        private void QueueMonitor_Tick(object sender, EventArgs e)
        {
            UpdateQueueDisplay();
        }

        private void UpdateQueueDisplay()
        {
            this.InvokeIfRequired(() =>
            {
                opQueueCount.Text = FD_Globals.QueueActive.Count.ToString();

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

                    foreach (DataGridViewColumn column in opQueueTable.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                }
            });
        }
        #endregion

        #region Helper Methods
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
        #endregion

        #region Cleanup
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Đóng kết nối máy quét
                if (_datalogicScanner != null)
                {
                    try
                    {
                        if (_datalogicScanner.Connected)
                        {
                            _datalogicScanner.Disconnect();
                        }
                        _datalogicScanner.SerialClientCallback -= DatalogicScanner_SerialClientCallback;
                        _datalogicScanner = null;
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nhưng không throw để đảm bảo dispose hoàn tất
                        System.Diagnostics.Debug.WriteLine($"Error disposing scanner: {ex.Message}");
                    }
                }

                // Dừng timer
                if (_queueMonitor != null)
                {
                    _queueMonitor.Stop();
                    _queueMonitor.Dispose();
                    _queueMonitor = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
