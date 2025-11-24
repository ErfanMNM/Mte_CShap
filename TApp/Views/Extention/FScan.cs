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

            TResult result = QRDatabaseHelper.GetActiveByQRContent(qrCode);

            TResult result1 = QRDatabaseHelper.GetByQRContent(qrCode);

            if (result1.issuccess)
            {
                if (result.count < 1)
                {
                    //không tìm thấy
                    this.InvokeIfRequired(() =>
                    {
                        opStatus.Text = "Không tìm thấy mã QR!";
                        opStatus.BackColor = Color.Red;
                        opStatus.Symbol = 61453;
                        opStatus.SymbolColor = Color.White;
                        opStatus.ForeColor = Color.White;
                    });
                }
                else
                {
                    if (result.issuccess)
                    {
                        if (result.count > 0)
                        {
                                //đã kích hoạt
                                this.InvokeIfRequired(() =>
                                {
                                    opStatus.Text = "Mã đã kích hoạt";
                                    opStatus.BackColor = Color.LimeGreen;
                                    opStatus.Symbol = 61527;
                                    opStatus.SymbolColor = Color.White;
                                    opStatus.ForeColor = Color.White;
                                });
                        }
                        else
                        {
                            //đã hủy
                            this.InvokeIfRequired(() =>
                            {
                                opStatus.Text = "Mã chưa kích hoạt";
                                opStatus.BackColor = Color.Orange;
                                opStatus.Symbol = 61527;
                                opStatus.SymbolColor = Color.White;
                                opStatus.ForeColor = Color.White;
                            });
                        }
                    }

                }
            }
            else
            {
                //lỗi truy xuất database
                this.InvokeIfRequired(() =>
                {
                    opStatus.Text = "Lỗi truy xuất database!";
                    opStatus.BackColor = Color.Orange;
                    opStatus.Symbol = 61527;
                    opStatus.SymbolColor = Color.White;
                    opStatus.ForeColor = Color.White;
                });
            }
        }
    }
}
