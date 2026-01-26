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

namespace MteCs
{
    public partial class MainForm : UIForm
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeHeadNav();
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

            //var DeactiveNode = headNav.CreateChildNode(headNav.Nodes[0], "VÔ HIỆU HÓA", 3003);
            //headNav.SetNodeSymbol(DeactiveNode, 61508);

            var shutdownNode = headNav.CreateChildNode(headNav.Nodes[0], "Đóng ứng dụng", 3001);
            headNav.SetNodeSymbol(shutdownNode, 61457);
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
    }
}
