using Sunny.UI;
using System.Windows.Forms;
using TApp.Configs;
using TApp.Infrastructure;
using TApp.Utils;
using TApp.Views.Auth;
using TApp.Views.Communications;
using TApp.Views.Dashboard;
using TApp.Views.Settings;
using TTManager.Auth;

namespace TApp
{
    public partial class MainForm : UIForm
    {
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;

        private PAppSetting PAppSetting = new PAppSetting();

        private FDashboard fDashboard = new FDashboard();

        private Login fLogin = new Login();

        public static e_App_Render_State AppRenderState = e_App_Render_State.LOGIN;

        public static e_App_State AppState = e_App_State.LOGIN;

        public MainForm()
        {
            InitializeComponent();
            UIStyles.CultureInfo = CultureInfos.en_US;
            UIStyles.GlobalFont = true;
            UIStyles.GlobalFontName = "Tahoma";
            //UIStyles.InitColorful(Color.Green, Color.White);

            MainTabControl = MainTab;
            NavMenu.TabControl = MainTab;
            headNav.TabControl = MainTab;

            NavMenu.Nodes.Clear();

            NavMenu.CreateNode(AddPage(fDashboard, 1002));
            NavMenu.CreateNode(AddPage(PAppSetting, 1001));

            NavMenu.CreateNode(AddPage(fLogin, 2001));
            NavMenu.SelectPage(2001);
            NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

            NavMenu.Visible = false;
            NavMenu.Enabled = false;

            AppRenderState = e_App_Render_State.LOGIN;

            headNav.Nodes.Clear();
            headNav.Nodes.Add("");

            headNav.SetNodeSymbol(headNav.Nodes[0], 559585);
            var node = headNav.CreateChildNode(headNav.Nodes[0], "Tắt máy", 3001);
            headNav.SetNodeSymbol(node, 61457);


            ToggleFullScreen();
            HideToTray();
            InitializeConfigs();
            StartPage();

            WK1.RunWorkerAsync();
        }

        public void StartPage()
        {
            PAppSetting.ShowTitle = false;
            PAppSetting.START();
            fDashboard.Start();
            fLogin.INIT();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {

            //WindowState = FormWindowState.Minimized;
            CloseApplication();
        }

        private void CloseApplication()
        {
            //hỏi trước khi thoát

            if (trayIcon != null)
            {
                trayIcon.Visible = false; // dọn icon trước khi thoát
            }
            //tắt các tiến trình đang chạy
            Application.Exit();
        }

        private void ToggleFullScreen()
        {
            if (WindowState != FormWindowState.Maximized)
            {
                Tag = WindowState;
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                if (Tag is FormWindowState previousState)
                {
                    WindowState = previousState;
                }
                else
                {
                    WindowState = FormWindowState.Normal;
                }
                FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        private void HideToTray()
        {
            // Tạo menu chuột phải ở tray
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Mở lại", null, OnShowClicked);
            trayMenu.Items.Add("Thoát", null, OnExitClicked);

            // Tạo icon tray
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Ứng dụng của tôi";
            trayIcon.Icon = Icon; // dùng icon của form, bạn có thể set icon riêng .ico

            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Double click icon tray => mở lại
            trayIcon.DoubleClick += OnShowClicked;

        }

        private void InitializeConfigs()
        {
            AppConfigs.Current.Load();

        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (AppConfigs.Current.AppHideEnable)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                }
            }

        }

        private void OnShowClicked(object? sender, EventArgs e)
        {

            ToggleFullScreen();
            Show();
            BringToFront();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false; // dọn icon trước khi thoát
            }
            CloseApplication();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        private void headNav_MenuItemClick(string itemText, int menuIndex, int pageIndex)
        {
            switch (pageIndex)
            {
                case 3001:
                    CloseApplication();
                    break;
                default:
                    break;
            }
        }

        private void WK1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!WK1.CancellationPending)
            {
                
                Thread.Sleep(100);
            }
        }


        #region Private Methods - State Processing
        private void Login_Process()
        {
            switch (AppState)
            {
                case e_App_State.LOGIN:
                    HandleLoginState();
                    break;
                case e_App_State.ACTIVE:
                    HandleActiveState();
                    break;
                case e_App_State.DEACTIVE:
                    HandleDeactiveState();
                    break;
            }
        }

        private void HandleLoginState()
        {
            if (AppRenderState != e_App_Render_State.LOGIN)
            {
                AppRenderState = e_App_Render_State.LOGIN;
                this.Invoke(new Action(() =>
                {
                    NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();
                    NavMenu.CreateNode("DM", 2001);
                    NavMenu.SelectPage(2001);
                    NavMenu.Enabled = false;
                    NavMenu.Visible = false;
                }));
            }

            if (GlobalVarialbles.CurrentUser.Username != string.Empty)
            {
                UpdateUserDisplay();
                   // AppState = ACTIVE_State ? e_App_State.ACTIVE : e_App_State.DEACTIVE;
            }
            else
            {
                AppState = e_App_State.LOGIN;
            }
        }

        private void HandleActiveState()
        {
            if (AppRenderState != e_App_Render_State.ACTIVE)
            {
                AppRenderState = e_App_Render_State.ACTIVE;
                this.Invoke(new Action(() =>
                {
                    NavMenu.SelectPage(1002);
                    NavMenu.Enabled = true;
                    NavMenu.Visible = true;
                }));
            }

            if (GlobalVarialbles.CurrentUser.Username == string.Empty)
            {
                            AppState = e_App_State.LOGIN;
            }
            else
            {
               // AppState = ACTIVE_State ? e_App_State.ACTIVE : e_App_State.DEACTIVE;
            }
        }

        private void HandleDeactiveState()
        {
            if (AppRenderState != e_App_Render_State.DEACTIVE)
            {
                this.Invoke(new Action(() =>
                {
                    if (AppRenderState == e_App_Render_State.LOGIN)
                    {
                        NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();
                    }
                    NavMenu.CreateNode("DMA", 2001);
                    NavMenu.SelectPage(2001);
                    NavMenu.Enabled = false;
                    NavMenu.Visible = false;
                }));
                AppRenderState = e_App_Render_State.DEACTIVE;
            }

            if (GlobalVarialbles.CurrentUser.Username == string.Empty)
            {
                AppState = e_App_State.LOGIN;
            }
            else
            {
               // AppState = Globals.ACTIVE_State ? e_App_State.ACTIVE : e_App_State.DEACTIVE;
            }
        }

        private void UpdateUserDisplay()
        {
            //this.InvokeIfRequired(() =>
            //{
            //    switch (Globals.CurrentUser.Role)
            //    {
            //        case "Admin":
            //            opUser.Text = $"[ADMIN] {Globals.CurrentUser.Username}";
            //            opUser.ForeColor = Color.Red;
            //            break;
            //        case "Operator":
            //            opUser.Text = $"[OPERATOR] {Globals.CurrentUser.Username}";
            //            opUser.ForeColor = Color.Yellow;
            //            break;
            //        case "Worker":
            //            opUser.Text = $"[WORKER] {Globals.CurrentUser.Username}";
            //            opUser.ForeColor = Color.Green;
            //            break;
            //        default:
            //            opUser.Text = "Không xác định";
            //            break;
            //    }
            //});
        }

        private void App_State_Process()
        {
            switch (AppState)
            {
                case e_App_State.LOGIN:
                    HandleLoginStatusDisplay();
                    break;
                case e_App_State.ACTIVE:
                    HandleActiveStatusDisplay();
                    break;
                case e_App_State.DEACTIVE:
                    //Show();
                    break;
            }
        }
        #endregion
    }


    public enum e_App_State
        {
            LOGIN,
            ACTIVE,
            DEACTIVE
        }

        //trạng thái giao diện của ứng dụng
        public enum e_App_Render_State
        {
            LOGIN,
            ACTIVE,
            DEACTIVE
        }
    }
}
