using System.Windows.Forms;
using TTManager.PDA;

namespace TTManager
{
    public partial class TTMain : Form
    {
        private readonly PdaScanManager _pdaManager = new();
        private readonly System.Windows.Forms.Timer _scanPollTimer;

        public TTMain()
        {
            InitializeComponent();

            _scanPollTimer = new System.Windows.Forms.Timer();
            _scanPollTimer.Interval = 500;
            _scanPollTimer.Tick += PollScanQueue;
            _scanPollTimer.Start();

            _pdaManager.OnScanReceived += HandlePdaScan;
            _pdaManager.OnLog += HandleLog;

            Load += TTMain_Load;
        }

        private async void TTMain_Load(object? sender, EventArgs e)
        {
            try
            {
                await _pdaManager.StartAsync();
                HandleLog("PDA API Server đang chạy trên port 6969");
            }
            catch (Exception ex)
            {
                HandleLog($"Lỗi khởi động PDA Server: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _scanPollTimer.Stop();
            _ = _pdaManager.StopAsync();
            base.OnFormClosing(e);
        }

        private void PollScanQueue(object? sender, EventArgs e)
        {
            while (true)
            {
                var scan = _pdaManager.DequeueScan();
                if (scan == null) break;
                HandlePdaScan(scan);
            }
        }

        private void HandlePdaScan(ScanData scan)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[PDA SCAN] PdaName={scan.PdaName} Code={scan.Code} Time={scan.Time:HH:mm:ss}");
        }

        private void HandleLog(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
