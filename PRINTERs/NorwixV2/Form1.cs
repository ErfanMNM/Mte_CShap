using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NorwixV2
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            uc_NorwixV21.IP = "127.0.0.1";
            uc_NorwixV21.LOAD();
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> dd = new List<string>();
            uc_NorwixV21.ClearData();
            for (int i = 0; i < 1000000; i++)
            {
                dd.Add("i.tcx.com.vn/89360173635050A50916045000Jc3LA");
            }             
            uc_NorwixV21.AddData(dd);
        }
    }
}
