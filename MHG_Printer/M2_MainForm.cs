using MHG_Printer.Utils;
using Sunny.UI;

namespace MHG_Printer
{
    public partial class M2_MainForm : UIForm
    {

        public M2_MainForm()
        {
            InitializeComponent();
            StartClock();
        }

        private void StartClock()
        {
            clock.RunWorkerAsync();
        }

        private void clock_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!clock.CancellationPending)
            {
                this.InvokeIfRequired(() =>
                {
                    opAppClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffK");
                });

                Thread.Sleep(100);
            }
        }
    }
}


