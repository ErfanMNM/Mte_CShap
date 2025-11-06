using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            }
            else
            {
                this.ShowErrorTip($"{e.Message}");
            }
        }
    }
}
