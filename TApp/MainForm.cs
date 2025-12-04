using MTs.Auditrails;
using Sunny.UI;
using System.Windows.Forms;
using TApp.Configs;
using TApp.Infrastructure;
using TApp.Utils;
using TApp.Views.Auth;
using TApp.Views.Dashboard;
using TApp.Views.Extention;
using TApp.Views.Settings;
using TTManager.Auth;
using static TApp.Views.Dashboard.FDashboard;

namespace TApp
{
    /// <summary>
    /// Form chính của ứng dụng – quản lý điều hướng, trạng thái đăng nhập
    /// và vòng đời của các trang con.
    /// </summary>
    public partial class MainForm : UIForm
    {
        #region Fields

        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;

        // Các trang cấu hình/chức năng
        private readonly PAppSetting PAppSetting = new PAppSetting();
        private readonly FDashboard fDashboard = new FDashboard();
        private readonly FScan fScan = new FScan();
        private readonly Login fLogin = new Login();
        private readonly FAddCode fAddCode = new FAddCode();
        private readonly PLCSetting PLCSetting = new PLCSetting();
        private readonly FActivityLogs fActivityLogs = new FActivityLogs();
        private readonly FExtention fExtention = new FExtention();

        #endregion

        #region App State

        /// <summary>
        /// Trạng thái giao diện hiện tại (đang hiển thị màn hình nào).
        /// </summary>
        public static e_App_Render_State AppRenderState = e_App_Render_State.LOGIN;

        /// <summary>
        /// Trạng thái logic của ứng dụng (LOGIN / ACTIVE / DEACTIVE).
        /// </summary>
        public static e_App_State AppState = e_App_State.LOGIN;

        /// <summary>
        /// Cờ điều khiển ACTIVE hay DEACTIVE khi đã có người dùng đăng nhập.
        /// </summary>
        public static bool ACTIVE_State = true;

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();

            try
            {
                InitializeLogger();

                // Tạo log ghi nhận mở ứng dụng
                GlobalVarialbles.Logger?.WriteLogAsync("System", e_LogType.System, "Mở ứng dụng");

                InitializeUIStyles();
                InitializeNavigation();
                InitializeHeadNav();

                ToggleFullScreen();
                HideToTray();

                InitializeConfigs();
                StartPage();

                // Khởi động background workers
                WK1.RunWorkerAsync();
                clock.RunWorkerAsync();
            }
            catch (Exception)
            {
                this.ShowErrorDialog("Lỗi chương trình");
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
                "ALL",
                "Main.ptl"
            );

            GlobalVarialbles.Logger = new LogHelper<e_LogType>(logPath);
        }

        private void InitializeUIStyles()
        {
            UIStyles.CultureInfo = CultureInfos.en_US;
            UIStyles.GlobalFont = true;
            UIStyles.GlobalFontName = "Tahoma";
        }

        /// <summary>
        /// Khởi tạo điều hướng thông qua NavMenu và MainTab.
        /// </summary>
        private void InitializeNavigation()
        {
            MainTabControl = MainTab;
            NavMenu.TabControl = MainTab;
            headNav.TabControl = MainTab;

            NavMenu.Nodes.Clear();

            // Các trang chức năng chính
            NavMenu.CreateNode(AddPage(fDashboard, 1001));
            NavMenu.CreateNode(AddPage(PAppSetting, 1002));
            NavMenu.CreateNode(AddPage(fScan, 1003));
            NavMenu.CreateNode(AddPage(fAddCode, 1004));
            NavMenu.CreateNode(AddPage(PLCSetting, 1005));
            NavMenu.CreateNode(AddPage(fActivityLogs, 1006));
            NavMenu.CreateNode(AddPage(fExtention, 1007));

            // Trang đăng nhập
            NavMenu.CreateNode(AddPage(fLogin, 2001));
            NavMenu.SelectPage(2001);
            NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

            // Ẩn menu cho tới khi đăng nhập
            NavMenu.Visible = false;
            NavMenu.Enabled = false;

            AppRenderState = e_App_Render_State.LOGIN;
        }

        /// <summary>
        /// Khởi tạo thanh điều hướng phía trên (headNav).
        /// </summary>
        private void InitializeHeadNav()
        {
            headNav.Nodes.Clear();
            headNav.Nodes.Add(string.Empty);

            headNav.SetNodeSymbol(headNav.Nodes[0], 559585);
            var logoutNode = headNav.CreateChildNode(headNav.Nodes[0], "Đăng xuất", 3002);
            headNav.SetNodeSymbol(logoutNode, 559834);

            var DeactiveNode = headNav.CreateChildNode(headNav.Nodes[0], "VÔ HIỆU HÓA", 3003);
            headNav.SetNodeSymbol(DeactiveNode, 61508);


            var shutdownNode = headNav.CreateChildNode(headNav.Nodes[0], "TẮT MÁY", 3001);
            headNav.SetNodeSymbol(shutdownNode, 61457);

            

            
        }

        private void InitializeConfigs()
        {
            AppConfigs.Current.Load();
        }

        /// <summary>
        /// Khởi tạo các page con sau khi cấu hình đã được load.
        /// </summary>
        public void StartPage()
        {
            PAppSetting.ShowTitle = false;
            PAppSetting.START();

            fDashboard.Start();
            fLogin.INIT();
            fScan.InitializeScanner();
            PLCSetting.INIT();
            fExtention.InitializeERP();

            fDashboard.ChangePage += FDashboard_ChangePage;
        }

        #endregion

        #region Navigation & UI helpers

        private void FDashboard_ChangePage(int pageIndex)
        {
            NavMenu.SelectPage(pageIndex);
        }

        public void SelectPage(int pageIndex)
        {
            this.InvokeIfRequired(() => { NavMenu.SelectPage(pageIndex); });
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

        #endregion

        #region Tray Icon & Window Events

        private void HideToTray()
        {
            // Tạo menu chuột phải ở tray
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Mở lại", null, OnShowClicked);
            trayMenu.Items.Add("Thoát", null, OnExitClicked);

            // Tạo icon tray
            trayIcon = new NotifyIcon
            {
                Text = "Ứng dụng của tôi",
                Icon = Icon,
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            // Double click icon tray => mở lại
            trayIcon.DoubleClick += OnShowClicked;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (AppConfigs.Current.AppHideEnable &&
                WindowState == FormWindowState.Minimized)
            {
                Hide();
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

        private void CloseApplication()
        {
            // Tắt icon tray trước khi thoát
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }

            // Tắt ứng dụng
            Application.Exit();
        }

        #endregion

        #region Button Event Handlers

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseApplication();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            if (AppRenderState == e_App_Render_State.ACTIVE)
            {
                NavMenu.SelectPage(1001);
            }
        }

        private void headNav_MenuItemClick(string itemText, int menuIndex, int pageIndex)
        {
            switch (pageIndex)
            {
                case 3001:
                    CloseApplication();
                    break;

                case 3002:
                    GlobalVarialbles.CurrentUser.Username = string.Empty;
                    AppState = e_App_State.LOGIN;
                    break;
                case 3003:
                    //Tính năng vô hiệu hóa
                    break;
            }
        }

        #endregion

        #region Background Workers

        private void WK1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!WK1.CancellationPending)
            {
                App_State_Process();
                Thread.Sleep(100);
            }
        }

        private void clock_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
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

        #endregion

        #region App State Processing

        private void HandleLoginState()
        {
            if (AppRenderState != e_App_Render_State.LOGIN)
            {
                AppRenderState = e_App_Render_State.LOGIN;

                this.Invoke(new Action(() =>
                {
                    NavMenu.CreateNode("DM", 2001);
                    NavMenu.SelectedNode = NavMenu.Nodes[2];
                    NavMenu.SelectPage(2001);
                    NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

                    NavMenu.Enabled = false;
                    NavMenu.Visible = false;
                    NavMenu.Size = new Size(0, 636);
                }));
            }

            if (GlobalVarialbles.CurrentUser.Username != string.Empty)
            {
                UpdateUserDisplay();
                AppState = ACTIVE_State ? e_App_State.ACTIVE : e_App_State.DEACTIVE;
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
                    NavMenu.SelectPage(1001);
                    NavMenu.Enabled = true;
                    NavMenu.Visible = true;
                    NavMenu.Size = new Size(144, 636);
                }));
            }

            if (GlobalVarialbles.CurrentUser.Username == string.Empty)
            {
                AppState = e_App_State.LOGIN;
            }
            else
            {
                AppState = ACTIVE_State ? e_App_State.ACTIVE : e_App_State.DEACTIVE;
            }
        }

        private void HandleDeactiveState()
        {
            if (AppRenderState != e_App_Render_State.DEACTIVE)
            {
                this.Invoke(new Action(() =>
                {
                    if (AppRenderState == e_App_Render_State.LOGIN &&
                        NavMenu.Nodes.Count > 0)
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
                AppState = ACTIVE_State ? e_App_State.ACTIVE : e_App_State.DEACTIVE;
            }
        }

        private void UpdateUserDisplay()
        {
            this.InvokeIfRequired(() =>
            {
                switch (GlobalVarialbles.CurrentUser.Role)
                {
                    case "Admin":
                        opUser.Text = $"[Quản Lý] {GlobalVarialbles.CurrentUser.Username}";
                        opUser.ForeColor = Color.Red;
                        break;

                    case "Operator":
                        opUser.Text = $"[Vận Hành] {GlobalVarialbles.CurrentUser.Username}";
                        opUser.ForeColor = Color.Green;
                        break;

                    default:
                        opUser.Text = "Không xác định";
                        break;
                }
            });
        }

        private void App_State_Process()
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

        #endregion
    }

    #region App Enums

    public enum e_App_State
    {
        LOGIN,
        ACTIVE,
        DEACTIVE
    }

    /// <summary>
    /// Trạng thái giao diện của ứng dụng.
    /// </summary>
    public enum e_App_Render_State
    {
        LOGIN,
        ACTIVE,
        DEACTIVE
    }

    #endregion
}
