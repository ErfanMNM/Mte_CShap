using System.Windows.Forms;
using Sunny.UI;
using TApp.Infrastructure;
using TTManager.Diaglogs;

namespace TApp.Views.Auth
{
    /// <summary>
    /// Page hiển thị khi ứng dụng ở chế độ VÔ HIỆU HÓA.
    /// </summary>
    public partial class FDeactive : UIPage
    {
        public event Action? OnReactivateRequested;

        public FDeactive()
        {
            InitializeComponent();
        }

        private void btnReactivate_Click(object sender, EventArgs e)
        {
            // Yêu cầu nhập mật khẩu để kích hoạt lại
            using (var enterPassword = new Entertext())
            {
                enterPassword.TileText = "Nhập mật khẩu để kích hoạt lại hệ thống";
                enterPassword.TextValue = string.Empty;
                enterPassword.IsPassword = true;

                if (enterPassword.ShowDialog() == DialogResult.OK)
                {
                    string password = enterPassword.TextValue;
                    
                    // Gửi sự kiện yêu cầu kích hoạt lại (MainForm sẽ xử lý)
                    OnReactivateRequested?.Invoke();
                }
            }
        }
    }
}

