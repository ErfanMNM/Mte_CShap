using MTs.Auditrails;
using Sunny.UI;
using System.Windows.Forms;
using System.Threading.Tasks;
using TApp.Configs;
using TApp.Infrastructure;
using TApp.Utils;
using TApp.Views.Auth;
using TApp.Views.Dashboard;
using TApp.Views.Extention;
using TApp.Views.Test;
using TApp.Views.Settings;
using TTManager.Auth;
using TTManager.Diaglogs;
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
        private readonly FDeactive fDeactive = new FDeactive();
        private readonly FCameraSimulator fCameraSimulator = new FCameraSimulator();

        private readonly string _userDbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TanTien",
            "Users",
            "users.database");

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

        /// <summary>
        /// Trạng thái hệ thống gửi PLC / UI (e_AppState).
        /// </summary>
        private e_AppState _globalAppState = e_AppState.Initializing;

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
            //NavMenu.CreateNode(AddPage(fCameraSimulator, 1008));

            // Trang đăng nhập
            NavMenu.CreateNode(AddPage(fLogin, 2001));
            NavMenu.SelectPage(2001);
            NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

            // Trang vô hiệu hóa (ẩn khỏi menu, chỉ hiển thị khi DEACTIVE)
            NavMenu.CreateNode(AddPage(fDeactive, 2002));
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
            //fScan.InitializeScanner();
            PLCSetting.INIT();
            fExtention.InitializeERP();

            fDashboard.ChangePage += FDashboard_ChangePage;
            fDashboard.DeactiveStateChanged += FDashboard_DeactiveStateChanged;
            fDeactive.OnReactivateRequested += FDeactive_OnReactivateRequested;

            // Kiểm tra trạng thái DEACTIVE từ PLC khi khởi động
            CheckDeactiveStateOnStartup();
        }

        #endregion

        #region Navigation & UI helpers

        private void FDashboard_ChangePage(int pageIndex)
        {
            NavMenu.SelectPage(pageIndex);
        }

        /// <summary>
        /// Xử lý khi trạng thái DEACTIVE từ PLC thay đổi.
        /// </summary>
        private void FDashboard_DeactiveStateChanged(bool isDeactive)
        {
            this.InvokeIfRequired(() =>
            {
                if (isDeactive)
                {
                    ACTIVE_State = false;
                    AppState = e_App_State.DEACTIVE;
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.System,
                        "Hệ thống bị vô hiệu hóa từ PLC",
                        "{{'Source':'PLC','State':'DEACTIVE'}}",
                        "SYS-DEACTIVE-01"
                    );
                }
                else
                {
                    ACTIVE_State = true;
                    if (GlobalVarialbles.CurrentUser.Username != string.Empty)
                    {
                        AppState = e_App_State.ACTIVE;
                    }
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.System,
                        "Hệ thống được kích hoạt lại từ PLC",
                        "{{'Source':'PLC','State':'ACTIVE'}}",
                        "SYS-DEACTIVE-02"
                    );
                }
            });
        }

        /// <summary>
        /// Kiểm tra trạng thái DEACTIVE từ PLC khi khởi động app.
        /// </summary>
        private void CheckDeactiveStateOnStartup()
        {
            Task.Run(() =>
            {
                // Chờ một chút để PLC kết nối
                Thread.Sleep(2000);
                
                bool isDeactive = fDashboard.CheckDeactiveStateOnStartup();
                if (isDeactive)
                {
                    this.InvokeIfRequired(() =>
                    {
                        ACTIVE_State = false;
                        AppState = e_App_State.DEACTIVE;
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            "System",
                            e_LogType.System,
                            "Phát hiện hệ thống đang ở chế độ VÔ HIỆU HÓA khi khởi động",
                            "{{'Source':'Startup','PLC_Deactive_DM':'1'}}",
                            "SYS-DEACTIVE-03"
                        );
                    });
                }
            });
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
                    HandleDeactivateRequest();
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
                SetGlobalAppState(e_AppState.Stopped); // Đang ở màn hình đăng nhập

                this.Invoke(new Action(() =>
                {
                    NavMenu.CreateNode("DM", 2001);
                    NavMenu.SelectedNode = NavMenu.Nodes[NavMenu.Nodes.Count - 1];
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
                SetGlobalAppState(e_AppState.Ready); // Trạng thái hoạt động bình thường

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
                AppRenderState = e_App_Render_State.DEACTIVE;
                SetGlobalAppState(e_AppState.Deactive); // Đang bị vô hiệu hóa

                this.Invoke(new Action(() =>
                {
                    // Xóa node cũ nếu có (từ LOGIN hoặc ACTIVE)
                    //if (NavMenu.Nodes.Count > 0)
                    //{
                    //    NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();
                    //}
                   // // Hiển thị page DEACTIVE (tương tự như Login)
                   NavMenu.CreateNode("DMA", 2002);
                    NavMenu.SelectedNode = NavMenu.Nodes[NavMenu.Nodes.Count - 1];
                    NavMenu.SelectPage(2002);
                    NavMenu.Nodes[NavMenu.Nodes.Count - 1].Remove();

                    NavMenu.Enabled = false;
                    NavMenu.Visible = false;
                    NavMenu.Size = new Size(0, 636);
                }));
            }

            // Khi DEACTIVE, luôn giữ trạng thái DEACTIVE cho đến khi được kích hoạt lại
            if (!ACTIVE_State)
            {
                AppState = e_App_State.DEACTIVE;
            }
            else
            {
                // Nếu ACTIVE_State = true nhưng đang ở DEACTIVE state, chuyển về ACTIVE hoặc LOGIN
                if (GlobalVarialbles.CurrentUser.Username == string.Empty)
                {
                    AppState = e_App_State.LOGIN;
                }
                else
                {
                    AppState = e_App_State.ACTIVE;
                }
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

        /// <summary>
        /// Cập nhật trạng thái hệ thống (e_AppState) dùng chung để render và gửi PLC.
        /// Logic gộp:
        /// - LOGIN              -> Stopped
        /// - ACTIVE             -> Ready
        /// - DEACTIVE           -> Deactive
        /// FDashboard có thể ghi đè (Editing / Error / Stopped khi mất kết nối).
        /// </summary>
        private void SetGlobalAppState(e_AppState state)
        {
            _globalAppState = state;
            GlobalVarialbles.CurrentAppState = state;
        }

        /// <summary>
        /// Xử lý yêu cầu vô hiệu hóa hệ thống từ nút headNav.
        /// </summary>
        private void HandleDeactivateRequest()
        {
            // Hỏi xác nhận đơn giản (không cần mật khẩu nếu người dùng bấm HỦY)
            if (!this.ShowAskDialog("Bạn có chắc chắn muốn VÔ HIỆU HÓA hệ thống?"))
            {
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.UserAction,
                    "Người dùng hủy yêu cầu vô hiệu hóa hệ thống",
                    $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                    "UA-DEACTIVE-03"
                );
                return;
            }

            // Yêu cầu nhập mật khẩu để vô hiệu hóa
            using (var enterPassword = new TTManager.Diaglogs.Entertext())
            {
                enterPassword.TileText = "Nhập mật khẩu để vô hiệu hóa hệ thống";
                enterPassword.TextValue = string.Empty;
                enterPassword.IsPassword = true;

                if (enterPassword.ShowDialog() != DialogResult.OK)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.UserAction,
                        "Người dùng hủy nhập mật khẩu khi vô hiệu hóa hệ thống",
                        $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                        "UA-DEACTIVE-04"
                    );
                    return;
                }

                string password = enterPassword.TextValue;
                if (!UserHelper.ValidateCredentials(GlobalVarialbles.CurrentUser.Username, password, _userDbPath))
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "Xác thực mật khẩu vô hiệu hóa hệ thống thất bại",
                        $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                        "ERR-DEACTIVE-AUTH-PW"
                    );
                    this.ShowErrorDialog("Mật khẩu không đúng, không thể vô hiệu hóa hệ thống.");
                    return;
                }
            }

            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Người dùng yêu cầu vô hiệu hóa hệ thống",
                $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}','Role':'{GlobalVarialbles.CurrentUser.Role}','Action':'DEACTIVATE_REQUEST'}}",
                "UA-DEACTIVE-01"
            );

            Task.Run(() =>
            {
                bool success = fDashboard.SetDeactiveState(true);
                this.InvokeIfRequired(() =>
                {
                    if (success)
                    {
                        ACTIVE_State = false;
                        AppState = e_App_State.DEACTIVE;

                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.UserAction,
                            "Vô hiệu hóa hệ thống thành công",
                            $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}','PLC_Deactive_DM':'1'}}",
                            "UA-DEACTIVE-02"
                        );

                        this.ShowSuccessTip("Hệ thống đã được vô hiệu hóa!");
                    }
                    else
                    {
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Error,
                            "Lỗi khi vô hiệu hóa hệ thống - không thể gửi lệnh xuống PLC",
                            $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                            "ERR-DEACTIVE-01"
                        );

                        this.ShowErrorDialog("Không thể vô hiệu hóa hệ thống. Vui lòng kiểm tra kết nối PLC.");
                    }
                });
            });
        }

        /// <summary>
        /// Xử lý yêu cầu kích hoạt lại hệ thống từ page DEACTIVE.
        /// </summary>
        private void FDeactive_OnReactivateRequested()
        {
            // 1) Yêu cầu mật khẩu
            using (var enterPassword = new TTManager.Diaglogs.Entertext())
            {
                enterPassword.TileText = "Nhập mật khẩu để kích hoạt lại hệ thống";
                enterPassword.TextValue = string.Empty;
                enterPassword.IsPassword = true;

                if (enterPassword.ShowDialog() != DialogResult.OK)
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.UserAction,
                        "Người dùng hủy yêu cầu kích hoạt lại hệ thống",
                        $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                        "UA-REACTIVE-03"
                    );
                    return;
                }

                string password = enterPassword.TextValue;

                // Kiểm tra mật khẩu
                if (!UserHelper.ValidateCredentials(GlobalVarialbles.CurrentUser.Username, password, _userDbPath))
                {
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "Xác thực mật khẩu kích hoạt lại hệ thống thất bại",
                        $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                        "ERR-REACTIVE-AUTH-PW"
                    );
                    this.ShowErrorDialog("Mật khẩu không đúng, không thể kích hoạt lại hệ thống.");
                    return;
                }
            }

            // 2) Nếu bật 2FA thì yêu cầu mã
            if (AppConfigs.Current.AppTwoFA_Enabled)
            {
                using (var enter2FA = new TTManager.Diaglogs.Entertext())
                {
                    enter2FA.TileText = "Nhập mã 2FA để kích hoạt lại hệ thống";
                    enter2FA.TextValue = string.Empty;
                    enter2FA.IsPassword = false;

                    if (enter2FA.ShowDialog() != DialogResult.OK)
                    {
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.UserAction,
                            "Người dùng hủy nhập mã 2FA khi kích hoạt lại hệ thống",
                            $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                            "UA-REACTIVE-04"
                        );
                        return;
                    }

                    string code2FA = enter2FA.TextValue.Trim();
                    if (!UserHelper.Validate2FA(GlobalVarialbles.CurrentUser.Username, code2FA, _userDbPath, 3))
                    {
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Error,
                            "Xác thực 2FA kích hoạt lại hệ thống thất bại",
                            $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                            "ERR-REACTIVE-AUTH-2FA"
                        );
                        this.ShowErrorDialog("Mã 2FA không đúng, không thể kích hoạt lại hệ thống.");
                        return;
                    }
                }
            }

            // Đã xác thực: gửi lệnh xuống PLC
            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Người dùng yêu cầu kích hoạt lại hệ thống",
                $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}','Role':'{GlobalVarialbles.CurrentUser.Role}','Action':'REACTIVATE_REQUEST'}}",
                "UA-REACTIVE-01"
            );

            Task.Run(() =>
            {
                bool success = fDashboard.SetDeactiveState(false);
                this.InvokeIfRequired(() =>
                {
                    if (success)
                    {
                        ACTIVE_State = true;

                        if (GlobalVarialbles.CurrentUser.Username != string.Empty)
                        {
                            AppState = e_App_State.ACTIVE;
                        }
                        else
                        {
                            AppState = e_App_State.LOGIN;
                        }

                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.UserAction,
                            "Kích hoạt lại hệ thống thành công",
                            $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}','PLC_Deactive_DM':'0'}}",
                            "UA-REACTIVE-02"
                        );

                        this.ShowSuccessTip("Hệ thống đã được kích hoạt lại!");
                    }
                    else
                    {
                        GlobalVarialbles.Logger?.WriteLogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Error,
                            "Lỗi khi kích hoạt lại hệ thống - không thể gửi lệnh xuống PLC",
                            $"{{'Username':'{GlobalVarialbles.CurrentUser.Username}'}}",
                            "ERR-REACTIVE-01"
                        );

                        this.ShowErrorDialog("Không thể kích hoạt lại hệ thống. Vui lòng kiểm tra kết nối PLC.");
                    }
                });
            });
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
