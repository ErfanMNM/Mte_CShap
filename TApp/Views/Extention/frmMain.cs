using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTManager.Communication.CC320;

namespace TApp.Views.Extention
{
    public partial class frmMain : UIPage
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private CPN_CC320 cpn_CC3201 = new CPN_CC320();

        private void frmMain_Load(object sender, EventArgs e)
        {
            
        }

        public void INIT ()
        {
            //cài delay sau khi đã kết nối thành công
            cpn_CC3201.CPN_ClientCallBack += Cpn_CC3201_CPN_ClientCallBack;
            cpn_CC3201.LOAD();
        }

        public void setPASS()
        {
            cpn_CC3201.SET_PASS();
        }

        public void setFAIL()
        {
            cpn_CC3201.SET_FAIL();
        }

        private void Cpn_CC3201_CPN_ClientCallBack(enumComponent_Client e, string _strData)
        {           
            switch (e)
            {
                case enumComponent_Client.Connected:
                    // cài delay
                    cpn_CC3201.SET_DELAY(500, 5000, 200);
                    //Bật tag lên: Do cc320 mặc định tắt tag
                    Thread.Sleep(100);
                    cpn_CC3201.ENABLE_TAG();
                    break;
                case enumComponent_Client.Disconnected:
                    break;
                case enumComponent_Client.DataReceived:

                    //this.Invoke(new Action(() =>
                    //{
                    //    lblTag.Text = _strData;
                    //}));


                    break;
                case enumComponent_Client.Error:                    
                    break;
                default:
                    break;
            }
            this.Invoke((Action)(() => {
                ldata.Insert(0, e.ToString() + ": " + _strData.Replace("\r","").Replace("\n",""));
                while (ldata.Count > 100)
                {
                    ldata.RemoveAt(ldata.Count - 1);
                }
                richTextBox1.Text = string.Join("\n", ldata);
            }));
        }
        List<string> ldata = new List<string>();
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            cpn_CC3201.UNLOAD();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            cpn_CC3201.SEND_COMMAND("RR2," + numericUpDown4.Value.ToString());
        }
       
    }
}
