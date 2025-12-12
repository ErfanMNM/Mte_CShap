using Sunny.UI;
using TApp.Configs;
using TApp.Infrastructure;

namespace TApp.Views.Auth
{
    public partial class Login : UIPage
    {
        #region Constructor & Initialization
        public Login()
        {
            InitializeComponent();
        }

        public void INIT()
        {
            ucLogin1.IS2FAEnabled = AppConfigs.Current.AppTwoFA_Enabled;
            ucLogin1.INIT();
        }
        #endregion

        #region Event Handlers
        private void ucLogin1_OnLoginAction(object sender, TTManager.Auth.LoginActionEventArgs e)
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
        #endregion

        #region Private Helper Methods
        private void HandleSuccessfulLogin(TTManager.Auth.LoginActionEventArgs e)
        {
            this.ShowSuccessTip("Đăng nhập thành công, vui lòng chờ trong giây lát");
            GlobalVarialbles.CurrentUser = ucLogin1.CurrentUser;

            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Đăng nhập thành công",
                $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}','Role':'{GlobalVarialbles.CurrentUser.Role}'}}",
                "UA-LOGIN-01"
            );
        }

        private void HandleFailedLogin(TTManager.Auth.LoginActionEventArgs e)
        {
            this.ShowErrorTip($"{e.Message}");

            GlobalVarialbles.Logger?.WriteLogAsync(
                "Anonymous",
                e_LogType.Error,
                "Đăng nhập thất bại",
                $"{{'Message':'{e.Message}'}}",
                "ERR-LOGIN-01"
            );
        }
        #endregion

        private void Login_Initialize(object sender, EventArgs e)
        {
            ucLogin1.ClearInputs();
        }
    }
}
