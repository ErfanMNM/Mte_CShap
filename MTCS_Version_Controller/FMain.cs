using Sunny.UI;
using System.Windows.Forms;
using System.Drawing;
using System;

namespace MTCS_Version_Controller
{
    public partial class FMain : UIForm
    {
        public FMain()
        {
            InitializeComponent();
        }

        private void uiSwitch1_ValueChanged(object sender, bool value)
        {
            if (value)
            {
                uiStyleManager1.Style = UIStyle.Black;
                uiLabel1.Text = "Light Mode";
            }
            else
            {
                uiStyleManager1.Style = UIStyle.Green;
                uiLabel1.Text = "Dark Mode";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
