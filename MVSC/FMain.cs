using Sunny.UI;

namespace MVSC
{
    public partial class FMain : UIForm
    {
        public FMain()
        {
            InitializeComponent();
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
    }
}
