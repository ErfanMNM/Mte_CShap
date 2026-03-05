using TTManager.Communication;
using Sunny.UI;
using System.ComponentModel;
using TApp.Configs;
using TApp.Helpers;
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

        #region Constructor
        public FScan()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialization
        private void FScan_Initialize(object sender, EventArgs e)
        {
            InitializeScanner();
        }

        private void FScan_Finalize(object sender, EventArgs e)
        {
            DisconnectScanner();
        }
        #endregion

        #region Scanner Management
        public void InitializeScanner()
        {
            if (string.IsNullOrEmpty(AppConfigs.Current.Handheld_COM_Port))
            {
                AddConsoleLog("[CẢNH BÁO] Chưa cấu hình cổng COM cho máy quét cầm tay.");
                return;
            }

            try
            {
                _datalogicScanner = new SerialClientHelper(AppConfigs.Current.Handheld_COM_Port, 9600);
                _datalogicScanner.SerialClientCallback += DatalogicScanner_SerialClientCallback;
                _datalogicScanner.Connect();
                AddConsoleLog("Giao diện phần mềm 1.0");
            }
            catch (Exception ex)
            {
                AddConsoleLog($"[LỖI] Không thể kết nối máy quét: {ex.Message}");
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
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không throw để đảm bảo ngắt kết nối hoàn tất
                    System.Diagnostics.Debug.WriteLine($"Error disconnecting scanner: {ex.Message}");
                }
            }
        }

        private void DatalogicScanner_SerialClientCallback(SerialClientState state, string data)
        {
            switch (state)
            {
                case SerialClientState.Connected:
                    // Handle connected state if needed
                    break;
                case SerialClientState.Disconnected:
                    // Handle disconnected state if needed
                    break;
                case SerialClientState.Received:
                    this.InvokeIfRequired(() =>
                    {
                        ipQRContent.Text = data.Trim();
                    });
                    WK_Find.RunWorkerAsync(data.Trim());
                    break;
                case SerialClientState.Error:
                    // Handle error state if needed
                    break;
            }
        }
        #endregion

        #region UI Event Handlers
        private void ipQRContent_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập toàn bộ mã QR hoặc 1 phần mã";
                enterText.TextValue = ipQRContent.Text;
                enterText.IsPassword = false;
                enterText.EnterClicked += (s, args) =>
                {
                    ipQRContent.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ipQRContent.Text))
            {
                UpdateStatus("Vui lòng nhập mã QR!", Color.Orange, 61527);
                return;
            }

            // Kiểm tra nếu WK_Find đang chạy
            if (WK_Find.IsBusy)
            {
                UpdateStatus("Đang xử lý...", Color.Gainsboro, 61761);
                return;
            }

            WK_Find.RunWorkerAsync(ipQRContent.Text.Trim());
        }
        #endregion

        #region Background Worker Methods
        private void WK_Find_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Đang tìm kiếm...", Color.Gainsboro, 61761);
                    // Clear grid
                    opInfoTable.DataSource = null;
                    opTime.Text = "";
                });

                string? qrCode = e.Argument as string;

                if (qrCode == null)
                {
                    this.InvokeIfRequired(() =>
                    {
                        UpdateStatus("Mã QR không hợp lệ!", Color.Red, 61453);
                    });
                    return;
                }

                // Lấy dữ liệu từ cả hai database
                TResult resultActive = QRDatabaseHelper.GetActiveByQRContent(qrCode);
                TResult resultAll = QRDatabaseHelper.GetByQRContent(qrCode);

                if (!resultAll.issuccess)
                {
                    this.InvokeIfRequired(() =>
                    {
                        UpdateStatus("Lỗi truy xuất database!", Color.Orange, 61527);
                    });
                    return;
                }

                if (resultAll.data == null || resultAll.data.Rows.Count < 1)
                {
                    this.InvokeIfRequired(() =>
                    {
                        UpdateStatus("Không tìm thấy mã QR!", Color.Red, 61453);
                    });
                    return;
                }

                // Hiển thị dữ liệu lên grid
                this.InvokeIfRequired(() =>
                {
                    opInfoTable.DataSource = resultAll.data;

                    // Tự động resize columns
                    foreach (DataGridViewColumn column in opInfoTable.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                });

                // Kiểm tra mã có active không
                if (resultActive.issuccess && resultActive.data != null && resultActive.data.Rows.Count > 0)
                {
                    this.InvokeIfRequired(() =>
                    {
                        UpdateStatus("Mã đã kích hoạt", Color.LimeGreen, 61527);

                        // Hiển thị thông tin thời gian từ active database
                        var row = resultActive.data.Rows[0];
                        opTime.Text = row["TimeStampActive"].ToString();
                    });
                }
                else
                {
                    // Kiểm tra status trong database chính
                    var firstRow = resultAll.data.Rows[0];
                    string status = firstRow["Status"].ToString();

                    this.InvokeIfRequired(() =>
                    {
                        if (status.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                        {
                            UpdateStatus("Mã chưa kích hoạt (Pass)", Color.Orange, 61527);
                        }
                        else
                        {
                            UpdateStatus($"Mã có status: {status}", Color.DarkOrange, 61527);
                        }

                        // Hiển thị thông tin thời gian
                        opTime.Text = firstRow["TimeStampActive"].ToString();
                    });
                }
            }
            catch (Exception ex)
            {
                this.InvokeIfRequired(() =>
                {
                    UpdateStatus("Lỗi trong quá trình tìm kiếm!", Color.Red, 61453);
                    AddConsoleLog($"[LỖI] {ex.Message}");
                });
            }
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

        private void AddConsoleLog(string message)
        {
            this.InvokeIfRequired(() =>
            {
                opConsole.Items.Insert(0, message);
            });
        }
        #endregion
    }
}
