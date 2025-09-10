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
using TApp.Views;
using TApp.Views.Communications;

namespace TApp
{
    public partial class MainForm : UIForm
    {
        #region Private Fields - Page Controls
                DatalogicScaner PageDatalogicScaner = new DatalogicScaner();
        FlowControl PageFlowControl = new FlowControl();
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();
            InitializationUI();
            RenderControlForm();
        }

        #endregion

        #region Private Methods - Form Events

        #endregion

        #region Private Methods - InitializationUI
        private void InitializationUI()
        {
            try
            {
                UIStyles.CultureInfo = CultureInfos.en_US;
                UIStyles.GlobalFont = true;
                UIStyles.GlobalFontName = "Tahoma";

                MainTabControl = MainTab;
                NavMenu.TabControl = MainTab;
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError($"InitializationUI Exception: {ex.Message}");
            }
        }

        private void RenderControlForm()
        {
            try
            {
                // Add Pages to Tab Control
                NavMenu.Nodes.Clear();
                NavMenu.CreateNode(AddPage(PageDatalogicScaner, 1001));
                NavMenu.CreateNode(AddPage(PageFlowControl, 1002));
                MainTab.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError($"RenderControlForm Exception: {ex.Message}");
            }
        }
            #endregion
        }
}
