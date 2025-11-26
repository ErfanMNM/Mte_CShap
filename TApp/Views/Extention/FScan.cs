using MTs.Communication;
using Sunny.UI;
using TApp.Configs;
using TApp.Helpers;
using TApp.Utils;
using TTManager.Diaglogs;

namespace TApp.Views.Extention
{
    public partial class FScan : UIPage
    {
        private SerialClientHelper DatalogicScaner;

        public int DatalogicScaner_DataReceived { get; private set; }

        public FScan()
        {
            InitializeComponent();
        }

        public void InitializeScanner()
        {
            DatalogicScaner = new SerialClientHelper(AppConfigs.Current.Handheld_COM_Port, 9600);
            DatalogicScaner.SerialClientCallback += DatalogicScaner_SerialClientCallback;
            DatalogicScaner.Connect();
        }

        private void DatalogicScaner_SerialClientCallback(SerialClientState state, string data)
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

        private void ipQRContent_DoubleClick(object sender, EventArgs e)
        {
            //bật bàn phím ảo
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập toàn bộ mã QR hoặc 1 phần mã";
                enterText.TextValue = ipQRContent.Text;
                enterText.IsPassword = false; // Thiết lập chế độ nhập mật khẩu
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
                opStatus.Text = "Vui lòng nhập mã QR!";
                opStatus.BackColor = Color.Orange;
                opStatus.Symbol = 61527;
                opStatus.SymbolColor = Color.White;
                opStatus.ForeColor = Color.White;
                return;
            }

            // Kiểm tra nếu WK_Find đang chạy
            if (WK_Find.IsBusy)
            {
                opStatus.Text = "Đang xử lý...";
                return;
            }

            WK_Find.RunWorkerAsync(ipQRContent.Text.Trim());
        }


        private void WK_Find_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            this.InvokeIfRequired(() =>
            {
                opStatus.Text = "Đang tìm kiếm...";
                opStatus.BackColor = Color.Gainsboro;
                opStatus.Symbol = 61761;
                opStatus.SymbolColor = Color.Black;
                opStatus.ForeColor = Color.Black;

                // Clear grid
                opInfoTable.DataSource = null;
                opTime.Text = "";
            });

            string? qrCode = e.Argument as string;

            if (qrCode == null)
            {
                //chửi
                this.InvokeIfRequired(() =>
                {
                    opStatus.Text = "Mã QR không hợp lệ!";
                    opStatus.BackColor = Color.Red;
                    opStatus.Symbol = 61453;
                    opStatus.SymbolColor = Color.White;
                    opStatus.ForeColor = Color.White;
                });

                return;
            }

            // Lấy dữ liệu từ cả hai database
            TResult resultActive = QRDatabaseHelper.GetActiveByQRContent(qrCode);
            TResult resultAll = QRDatabaseHelper.GetByQRContent(qrCode);

            if (!resultAll.issuccess)
            {
                //lỗi truy xuất database
                this.InvokeIfRequired(() =>
                {
                    opStatus.Text = "Lỗi truy xuất database!";
                    opStatus.BackColor = Color.Orange;
                    opStatus.Symbol = 61527;
                    opStatus.SymbolColor = Color.White;
                    opStatus.ForeColor = Color.White;

                    opConsole.Items.Insert(0, $"[LỖI] Không thể truy xuất database: {resultAll.message}");
                });
                return;
            }

            if (resultAll.count < 1)
            {
                //không tìm thấy
                this.InvokeIfRequired(() =>
                {
                    opStatus.Text = "Không tìm thấy mã QR!";
                    opStatus.BackColor = Color.Red;
                    opStatus.Symbol = 61453;
                    opStatus.SymbolColor = Color.White;
                    opStatus.ForeColor = Color.White;
                    opConsole.Items.Insert(0, $"Mã : {qrCode} Không tìm thấy");
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
            if (resultActive.issuccess && resultActive.count > 0)
            {
                //đã kích hoạt
                this.InvokeIfRequired(() =>
                {
                    opStatus.Text = "Mã đã kích hoạt";
                    opStatus.BackColor = Color.LimeGreen;
                    opStatus.Symbol = 61527;
                    opStatus.SymbolColor = Color.White;
                    opStatus.ForeColor = Color.White;

                    // Hiển thị thông tin thời gian từ active database
                    if (resultActive.data.Rows.Count > 0)
                    {
                        var row = resultActive.data.Rows[0];
                        opTime.Text = row["TimeStampActive"].ToString();
                    }
                });
            }
            else
            {
                // Kiểm tra status trong database chính
                if (resultAll.data.Rows.Count > 0)
                {
                    var firstRow = resultAll.data.Rows[0];
                    string status = firstRow["Status"].ToString();

                    this.InvokeIfRequired(() =>
                    {
                        if (status.Equals("Pass", StringComparison.OrdinalIgnoreCase))
                        {
                            opStatus.Text = "Mã chưa kích hoạt (Pass)";
                            opStatus.BackColor = Color.Orange;
                            opStatus.Symbol = 61527;
                            opStatus.SymbolColor = Color.White;
                            opStatus.ForeColor = Color.White;
                        }
                        else
                        {
                            opStatus.Text = $"Mã có status: {status}";
                            opStatus.BackColor = Color.DarkOrange;
                            opStatus.Symbol = 61527;
                            opStatus.SymbolColor = Color.White;
                            opStatus.ForeColor = Color.White;
                        }

                        // Hiển thị thông tin thời gian
                        opTime.Text = firstRow["TimeStampActive"].ToString();
                    });
                }
            }
        }
    }
}
