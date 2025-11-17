using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTManager.Internet
{
    public partial class NetworkStrength : UserControl
    {
        public NetworkStrength()
        {
            InitializeComponent();
        }

        public void StartMonitoring()
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        public void StopMonitoring()
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!backgroundWorker1.CancellationPending)
            {
                int level = NetworkStrengthHelper.GetNetworkStrength();

                // Báo ra UI (invoke)
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    opInternetSignal.Level = level;

                    // tuỳ mày muốn tô màu đẹp hơn
                    switch (level)
                    {
                        case 1:
                            opInternetStatus.Text = "Không có";
                            opInternetStatus.ForeColor = Color.Red;
                            opInternetStatus.RectColor = Color.Red;
                            break;
                        case 2:
                            opInternetStatus.Text = "Rất yếu";
                            opInternetStatus.ForeColor = Color.FromArgb(255, 128, 0);
                            opInternetStatus.RectColor = Color.FromArgb(255, 128, 0);
                            break;
                        case 3:
                            opInternetStatus.Text = "Yếu";
                            opInternetStatus.ForeColor = Color.Red;
                            opInternetStatus.RectColor = Color.Yellow;

                            break;
                        case 4:
                            opInternetStatus.Text = "Tốt";
                            opInternetStatus.ForeColor = Color.Green;
                            opInternetStatus.RectColor = Color.Green;
                            break;
                        case 5:
                            opInternetStatus.Text = "Rất tốt";
                            opInternetStatus.ForeColor = Color.DarkGreen;
                            opInternetStatus.RectColor = Color.DarkGreen;
                            break;
                        default:
                            opInternetStatus.Text = "Lỗi";
                            opInternetStatus.ForeColor = Color.Red;
                            opInternetStatus.RectColor = Color.Red;
                            break;
                    }
                }));

                Thread.Sleep(2000); // đo mỗi 2s
            }
        }
    }
}
