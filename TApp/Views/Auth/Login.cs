using Sunny.UI;
using TApp.Configs;
using TApp.Infrastructure;
using TApp.Views.Dashboard;

namespace TApp.Views.Auth
{
    public partial class Login : UIPage
    {
        public Login()
        {
            InitializeComponent();
        }

        public void INIT()
        {
            ucLogin1.IS2FAEnabled = AppConfigs.Current.AppTwoFA_Enabled;
            ucLogin1.INIT();
        }

        private void ucLogin1_OnLoginAction(object sender, TTManager.Auth.LoginActionEventArgs e)
        {
            if (e.Status)
            {
                // Hiển thị thông báo đăng nhập thành công
                this.ShowSuccessTip($"Đăng nhập thành công, vui lòng chờ trong giây lát");
                //ghi thông tin user
                GlobalVarialbles.CurrentUser = ucLogin1.CurrentUser;

                // Ghi log đăng nhập thành công
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.UserAction,
                    "Đăng nhập thành công",
                    $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}','Role':'{GlobalVarialbles.CurrentUser.Role}'}}",
                    "UA-LOGIN-01"
                );
            }
            else
            {
                this.ShowErrorTip($"{e.Message}");

                // Ghi log đăng nhập thất bại
                GlobalVarialbles.Logger?.WriteLogAsync(
                    "Anonymous",
                    e_LogType.Error,
                    "Đăng nhập thất bại",
                    $"{{'Message':'{e.Message}'}}",
                    "ERR-LOGIN-01"
                );
            }
        }
    }
}
