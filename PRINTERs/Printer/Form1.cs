using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Printer
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {           
            ucPrinter1.LOAD();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 1;
            List<string> DATA2 = new List<string>();
            while (true)
            {
                DATA2.Add(i.ToString());
                i++;
                if (i == 10000) break;
            }
            ucPrinter1.AddData(DATA2);
        }
    }
}
