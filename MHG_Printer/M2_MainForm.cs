using MHG_Printer.Infrastructure;
using MHG_Printer.Utils;
using MHG_Printer.Views;
using Sunny.UI;
using System.Windows.Forms;

namespace MHG_Printer
{
    public partial class M2_MainForm : UIForm
    {
        #region Fields

        private readonly Dashboard fDashboard = new Dashboard();
        private readonly Login fLogin = new Login();

        #endregion

        #region Constructor

        public M2_MainForm()
        {
            InitializeComponent();

            try
            {
                InitializeLogger();
                InitializeNavigation();
                InitializeHeadNav();
                StartPage();

                WK1.RunWorkerAsync();
                clock.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog("Lỗi chương trình: " + ex.Message);
            }
        }

        #endregion

        #region Initialization Helpers

        private void InitializeLogger()
        {
            string logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MTE",
                "Logs",
                "MHG_Printer",
                "Main.ptl"
            );

            GlobalVariables.Logger = new TTManager.Audit.LogHelper<e_LogType>(logPath);
        }

        private void InitializeNavigation()
        {
            MainTabControl = MainTab;
            NavMenu.TabControl = MainTab;

            NavMenu.Nodes.Clear();

            NavMenu.CreateNode(AddPage(fDashboard, 1001));
            NavMenu.CreateNode(AddPage(fLogin, 2001));

            NavMenu.SelectPage(1001);
            NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

            NavMenu.Visible = false;
            NavMenu.Enabled = false;
            NavMenu.Size = new Size(0, 636);
        }

        private void InitializeHeadNav()
        {
            headNav.Nodes.Clear();
            headNav.Nodes.Add(string.Empty);

            headNav.SetNodeSymbol(headNav.Nodes[0], 559585);

            var logoutNode = headNav.CreateChildNode(headNav.Nodes[0], "Đăng xuất", 3002);
            headNav.SetNodeSymbol(logoutNode, 559834);

            var shutdownNode = headNav.CreateChildNode(headNav.Nodes[0], "Thoát", 3001);
            headNav.SetNodeSymbol(shutdownNode, 61457);
        }

        private void StartPage()
        {
            fLogin.INIT();
        }

        #endregion

        #region Navigation Helpers

        public new void SelectPage(int pageIndex)
        {
            this.InvokeIfRequired(() => { NavMenu.SelectPage(pageIndex); });
        }

        private void btnHome_Click(object? sender, EventArgs e)
        {
            NavMenu.SelectPage(1001);
        }

        #endregion

        #region Event Handlers

        private void headNav_MenuItemClick(string itemText, int menuIndex, int pageIndex)
        {
            switch (pageIndex)
            {
                case 3001:
                    CloseApplication();
                    break;
                case 3002:
                    HandleLogout();
                    break;
            }
        }

        #endregion

        #region App State Processing

        private void UpdateUserDisplay()
        {
            this.InvokeIfRequired(() =>
            {
                switch (GlobalVariables.CurrentUser.Role)
                {
                    case "Admin":
                        opUser.Text = $"[Quản Lý] {GlobalVariables.CurrentUser.Username}";
                        opUser.ForeColor = Color.Red;
                        break;
                    case "Operator":
                        opUser.Text = $"[Vận Hành] {GlobalVariables.CurrentUser.Username}";
                        opUser.ForeColor = Color.Green;
                        break;
                    default:
                        opUser.Text = GlobalVariables.CurrentUser.Username;
                        break;
                }
            });
        }

        private void HandleLogout()
        {
            GlobalVariables.CurrentUser.Username = string.Empty;
            GlobalVariables.CurrentUser.Role = string.Empty;
            GlobalVariables.AppRenderState = e_AppRenderState.LOGIN;
        }

        #endregion

        #region Background Workers

        private void WK1_DoWork(object? sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!WK1.CancellationPending)
            {
                App_State_Process();
                Thread.Sleep(100);
            }
        }

        private void clock_DoWork(object? sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!clock.CancellationPending)
            {
                this.InvokeIfRequired(() =>
                {
                    opAppClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK");
                });

                Thread.Sleep(100);
            }
        }

        private void App_State_Process()
        {
            switch (GlobalVariables.AppRenderState)
            {
                case e_AppRenderState.LOGIN:
                    HandleLoginState();
                    break;
                case e_AppRenderState.ACTIVE:
                    HandleActiveState();
                    break;
            }
        }

        private void HandleLoginState()
        {
            this.InvokeIfRequired(() =>
            {
                NavMenu.CreateNode("DM", 2001);
                NavMenu.SelectedNode = NavMenu.Nodes[NavMenu.Nodes.Count - 1];
                NavMenu.SelectPage(2001);
                NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

                NavMenu.Enabled = false;
                NavMenu.Visible = false;
                NavMenu.Size = new Size(0, 636);
            });

            if (GlobalVariables.CurrentUser.Username != string.Empty)
            {
                UpdateUserDisplay();
                GlobalVariables.CurrentAppState = e_AppState.Ready;
            }
            else
            {
                GlobalVariables.CurrentAppState = e_AppState.Stopped;
            }
        }

        private void HandleActiveState()
        {
            this.InvokeIfRequired(() =>
            {
                NavMenu.SelectPage(1001);
                NavMenu.Enabled = true;
                NavMenu.Visible = true;
                NavMenu.Size = new Size(144, 636);
            });

            if (GlobalVariables.CurrentUser.Username == string.Empty)
            {
                GlobalVariables.CurrentAppState = e_AppState.Stopped;
            }
            else
            {
                GlobalVariables.CurrentAppState = e_AppState.Ready;
            }
        }

        #endregion

        #region Application Lifecycle

        private void CloseApplication()
        {
            GlobalVariables.Logger?.Dispose();
            Application.Exit();
        }

        #endregion
    }
}
