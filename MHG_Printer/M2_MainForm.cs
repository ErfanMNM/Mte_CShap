using MHG_Printer.Utils;
using Sunny.UI;
using System.Net.NetworkInformation;

namespace MHG_Printer
{
    public partial class M2_MainForm : UIForm
    {

        public M2_MainForm()
        {
            InitializeComponent();
            StartClock();
            InitializeHeadNav();
        }

        private void StartClock()
        {
            clock.RunWorkerAsync();
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

        private void InitializeHeadNav()
        {
            headNav.Nodes.Clear();
            headNav.Nodes.Add(string.Empty);

            headNav.SetNodeSymbol(headNav.Nodes[0], 559585);
            //var logoutNode = headNav.CreateChildNode(headNav.Nodes[0], "Đăng xuất", 3002);
            //headNav.SetNodeSymbol(logoutNode, 559834);


            var shutdownNode = headNav.CreateChildNode(headNav.Nodes[0], "Thoát", 3001);
            headNav.SetNodeSymbol(shutdownNode, 61457);
        }

        private void headNav_MenuItemClick(string itemText, int menuIndex, int pageIndex)
        {
            switch (pageIndex)
            {
                case 3001:
                    CloseApplication();
                    break;
            }
        }

        private void CloseApplication()
        {
            clock.CancelAsync();
            Application.Exit();
        }
    }
}


