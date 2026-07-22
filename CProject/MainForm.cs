using CProject.Views;
using Sunny.UI;

namespace CProject
{
    public partial class MainForm : UIForm
    {
        public FDashboard _fdashboard = new FDashboard();
        public PageSocketUI pageSocketUI = new PageSocketUI();
        public Page_OPC_MHG _page_OPC_MHG = new Page_OPC_MHG();
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
            MainNavMenu.CreateNode(AddPage(pageSocketUI, 1002));
            MainNavMenu.CreateNode(AddPage(_page_OPC_MHG, 1003));
            MainNavMenu.SelectPage(1003);
        }


    }
}
