using MHG_Printer.Infrastructure;
using MHG_Printer.Utils;
using Sunny.UI;
using System.Windows.Forms;

namespace MHG_Printer.Views
{
    public partial class Dashboard : UIPage
    {
        public Dashboard()
        {
            InitializeComponent();
            backgroundWorker1.RunWorkerAsync();
        }

        public bool SetDeactiveState(bool isDeactive) => true;

        string cauhinh = "if<1;1>";

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                
            }
        }
    }
}
