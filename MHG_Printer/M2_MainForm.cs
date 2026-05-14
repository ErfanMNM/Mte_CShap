using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUA;
using LibUA.Core;
using Sunny.UI;

namespace MHG_Printer
{
    public partial class M2_MainForm : UIForm
    {
        private Client? _opcClient;
        private bool _opcConnected;

        public M2_MainForm()
        {
            InitializeComponent();
        }

      
    }
}


