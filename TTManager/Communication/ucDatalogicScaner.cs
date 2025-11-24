using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTManager.Communication
{
    public partial class ucDatalogicScaner : UserControl
    {
        SerialClientHelper DatalogicScaner;

        public string PortName { get; set; } = "COM1";
        public int baudRate { get; set; } = 9600;

        public ucDatalogicScaner()
        {
            InitializeComponent();
        }

        public void InitializeScanner()
        {
            DatalogicScaner = new SerialClientHelper(PortName, baudRate);
            DatalogicScaner.Connect();
            DatalogicScaner.SerialClientCallback += DatalogicScaner_SerialClientCallback;
        }

        private void DatalogicScaner_SerialClientCallback(SerialClientState state, string data)
        {

        }
    }
}
