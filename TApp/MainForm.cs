using Sunny.UI;
using TApp.Configs;
using TApp.Views.Communications;
using TApp.Views.Settings;

namespace TApp
{
    public partial class MainForm : UIForm
    {
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;

        private PAppSetting PAppSetting = new PAppSetting();
        private SocketTranferSiemen PSocketTransfer = new SocketTranferSiemen();

        public MainForm()
        {
            InitializeComponent();
            UIStyles.CultureInfo = CultureInfos.en_US;
            UIStyles.GlobalFont = true;
            UIStyles.GlobalFontName = "Tahoma";

            MainTabControl = MainTab;
            NavMenu.TabControl = MainTab;
            NavMenu.Nodes.Clear();

            NavMenu.CreateNode(AddPage(PAppSetting, 1001));
            NavMenu.CreateNode(AddPage(PSocketTransfer, 2001));

            ToggleFullScreen();
            HideToTray();
            InitializeConfigs();
            StartPage();
            LogBootstrap.EnsureInitialized();
            LogBootstrap.Logger.Log("System", "INFO", "App Opened", "Ứng dụng khởi động thành công");
        }

        public void StartPage()
        {
            PAppSetting.ShowTitle = false;
            PAppSetting.START();
            PSocketTransfer.START();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            //CloseApplication();
        }

        private void CloseApplication()
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false; // dọn icon trước khi thoát
            }
            //tắt các tiến trình đang chạy
            PSocketTransfer.CloseApp();
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
            if(AppConfigs.Current.AppHideEnable)
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
    }
}
