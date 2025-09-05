using Sunny.UI;
using TApp.Configs;

namespace TApp
{
    public partial class MainForm : UIForm
    {
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;

        public MainForm()
        {
            InitializeComponent();
            ToggleFullScreen();
            HideToTray();
            InitializeConfigs();
            LogBootstrap.EnsureInitialized();
            LogBootstrap.Logger.Log("System", "INFO", "App Opened", "Ứng dụng khởi động thành công");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseApplication();
        }

        private void CloseApplication()
        {
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
            Application.Exit();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }
}
