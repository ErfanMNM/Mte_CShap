using CProject.Views;
using Sunny.UI;

namespace CProject
{
    public partial class MainForm : UIForm
    {
        public FDashboard _fdashboard = new FDashboard();
        public MainForm()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            InitUI(TabBody);
        }

        private void InitUI(UITabControl mainTabBody)
        {
            MainTabControl = mainTabBody;
            MainNavMenu.TabControl = mainTabBody;

            MainNavMenu.CreateNode(AddPage(_fdashboard, 1001));
            MainNavMenu.SelectPage(1001);
        }


    }
}
