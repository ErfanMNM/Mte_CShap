using Sunny.UI;
using MHG_Printer.Infrastructure;

namespace MHG_Printer.Views
{
    public partial class Login : UIPage
    {
        public Login()
        {
            InitializeComponent();
        }

        public void INIT()
        {
            ucLogin1.IS2FAEnabled = false;
            ucLogin1.INIT();
        }

        private void ucLogin1_OnLoginAction(object? sender, TTManager.Auth.LoginActionEventArgs e)
        {
            if (e.Status)
            {
                HandleSuccessfulLogin(e);
            }
            else
            {
                HandleFailedLogin(e);
            }
        }

        private void HandleSuccessfulLogin(TTManager.Auth.LoginActionEventArgs e)
        {
            this.ShowSuccessTip("Đăng nhập thành công, vui lòng chờ trong giây lát");
            GlobalVariables.CurrentUser = ucLogin1.CurrentUser;

            GlobalVariables.AppRenderState = e_AppRenderState.ACTIVE;

            GlobalVariables.Logger?.WriteLogAsync(
                GlobalVariables.CurrentUser.Username,
                e_LogType.UserAction,
                "Đăng nhập thành công",
                $"{{'Username':'{GlobalVariables.CurrentUser.Username}','Role':'{GlobalVariables.CurrentUser.Role}'}}",
                "UA-LOGIN-01"
            );
        }

        private void HandleFailedLogin(TTManager.Auth.LoginActionEventArgs e)
        {
            this.ShowErrorTip($"{e.Message}");

            GlobalVariables.Logger?.WriteLogAsync(
                "Anonymous",
                e_LogType.Error,
                "Đăng nhập thất bại",
                $"{{'Message':'{e.Message}'}}",
                "ERR-LOGIN-01"
            );
        }

        private void Login_Initialize(object sender, EventArgs e)
        {
            ucLogin1.ClearInputs();
        }
    }
}
