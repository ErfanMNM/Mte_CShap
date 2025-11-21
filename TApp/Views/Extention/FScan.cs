using Sunny.UI;
using TTManager.Diaglogs;

namespace TApp.Views.Extention
{
    public partial class FScan : UIPage
    {
        public FScan()
        {
            InitializeComponent();
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
    }
}
