using MHG_Cartoning.Views;
using Sunny.UI;

namespace MHG_Cartoning
{
    public partial class MainForm : UIForm
    {
        public Page_OPC_Production_Order _page_OPC_Production_Order = new Page_OPC_Production_Order();
        private Page_Settings page_Settings = new Page_Settings();
        public MainForm()
        {
            InitializeComponent();

            MainTabControl = uiTabControl1;
            uiNavMenu1.TabControl = uiTabControl1;

            uiNavMenu1.CreateNode(AddPage(_page_OPC_Production_Order, 1001));
            uiNavMenu1.CreateNode(AddPage(page_Settings, 1002));
            uiNavMenu1.SelectPage(1001);

        }

    }
}
