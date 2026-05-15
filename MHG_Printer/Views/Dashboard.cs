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
        }

        public bool SetDeactiveState(bool isDeactive) => true;
    }
}
