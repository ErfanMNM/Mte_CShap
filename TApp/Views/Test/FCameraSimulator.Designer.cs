using Sunny.UI;
using System.Windows.Forms;
using System.Drawing;
using TApp.Views.Dashboard;

namespace TApp.Views.Test
{
    partial class FCameraSimulator
    {
        private UITextBox txtPort;
        private UITextBox txtPrefix;
        private UITextBox txtRandomLen;
        private UITextBox txtBarcode;
        private UITextBox txtFull;
        private UIListBox opLog;
        private UISymbolButton btnStart;
        private UISymbolButton btnStop;
        private UISymbolButton btnSendRandom;
        private UISymbolButton btnSendFull;
        private UISymbolButton btnLoadBarcode;

        private TableLayoutPanel root;
        private TableLayoutPanel rowServer;
        private TableLayoutPanel rowRandom;
        private TableLayoutPanel rowFull;
        private FlowLayoutPanel panelButtons;

        private void InitializeSimulatorComponent()
        {
            this.txtPort = new UITextBox();
            this.txtPrefix = new UITextBox();
            this.txtRandomLen = new UITextBox();
            this.txtBarcode = new UITextBox();
            this.txtFull = new UITextBox();
            this.opLog = new UIListBox();
            this.btnStart = new UISymbolButton();
            this.btnStop = new UISymbolButton();
            this.btnSendRandom = new UISymbolButton();
            this.btnSendFull = new UISymbolButton();
            this.btnLoadBarcode = new UISymbolButton();
            this.root = new TableLayoutPanel();
            this.rowServer = new TableLayoutPanel();
            this.rowRandom = new TableLayoutPanel();
            this.rowFull = new TableLayoutPanel();
            this.panelButtons = new FlowLayoutPanel();
            this.SuspendLayout();

            // root
            this.root.Dock = DockStyle.Fill;
            this.root.ColumnCount = 1;
            this.root.RowCount = 4;
            this.root.Padding = new Padding(8);
            this.root.AutoSize = true;
            this.root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // rowServer
            this.rowServer.Dock = DockStyle.Top;
            this.rowServer.ColumnCount = 4;
            this.rowServer.RowCount = 1;
            this.rowServer.Height = 40;
            this.rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            this.rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            this.rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            this.rowServer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            var lblPort = new UILabel { Text = "Port", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            this.txtPort.Text = "51236";
            this.txtPort.Dock = DockStyle.Fill;
            this.btnStart.Text = "Start";
            this.btnStart.Symbol = 61515; // play
            this.btnStart.Dock = DockStyle.Fill;
            this.btnStart.Click += (s, e) => StartServer();
            this.btnStop.Text = "Stop";
            this.btnStop.Symbol = 61516; // stop
            this.btnStop.Dock = DockStyle.Fill;
            this.btnStop.Click += (s, e) => StopServer();

            this.rowServer.Controls.Add(lblPort, 0, 0);
            this.rowServer.Controls.Add(this.txtPort, 1, 0);
            this.rowServer.Controls.Add(this.btnStart, 2, 0);
            this.rowServer.Controls.Add(this.btnStop, 3, 0);

            // rowRandom
            this.rowRandom.Dock = DockStyle.Top;
            this.rowRandom.ColumnCount = 7;
            this.rowRandom.RowCount = 1;
            this.rowRandom.Height = 40;
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));   // lbl prefix
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));    // prefix text
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));   // lbl len
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));   // len text
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));   // lbl barcode
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));    // barcode text
            this.rowRandom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // buttons

            var lblPrefix = new UILabel { Text = "Prefix", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            this.txtPrefix.Dock = DockStyle.Fill;
            this.txtPrefix.Text = "PREFIX";
            var lblRandLen = new UILabel { Text = "Len", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            this.txtRandomLen.Dock = DockStyle.Fill;
            this.txtRandomLen.Text = "16";
            var lblBarcode = new UILabel { Text = "Barcode", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            this.txtBarcode.Dock = DockStyle.Fill;
            this.btnLoadBarcode.Text = "Lấy BC";
            this.btnLoadBarcode.Symbol = 61639;
            this.btnLoadBarcode.Dock = DockStyle.Left;
            this.btnLoadBarcode.Width = 70;
            this.btnLoadBarcode.Click += (s, e) => { this.txtBarcode.Text = FD_Globals.productionData?.Barcode ?? string.Empty; };
            this.btnSendRandom.Text = "Gửi Prefix+Random";
            this.btnSendRandom.Symbol = 61527;
            this.btnSendRandom.Dock = DockStyle.Fill;
            this.btnSendRandom.Click += (s, e) => SendPrefixRandom();

            this.panelButtons.Dock = DockStyle.Fill;
            this.panelButtons.FlowDirection = FlowDirection.LeftToRight;
            this.panelButtons.Controls.Add(this.btnLoadBarcode);
            this.panelButtons.Controls.Add(this.btnSendRandom);

            this.rowRandom.Controls.Add(lblPrefix, 0, 0);
            this.rowRandom.Controls.Add(this.txtPrefix, 1, 0);
            this.rowRandom.Controls.Add(lblRandLen, 2, 0);
            this.rowRandom.Controls.Add(this.txtRandomLen, 3, 0);
            this.rowRandom.Controls.Add(lblBarcode, 4, 0);
            this.rowRandom.Controls.Add(this.txtBarcode, 5, 0);
            this.rowRandom.Controls.Add(this.panelButtons, 6, 0);

            // rowFull
            this.rowFull.Dock = DockStyle.Top;
            this.rowFull.ColumnCount = 2;
            this.rowFull.RowCount = 1;
            this.rowFull.Height = 40;
            this.rowFull.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            this.rowFull.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            this.txtFull.Dock = DockStyle.Fill;
            this.txtFull.Watermark = "Nhập full chuỗi gửi đi";
            this.btnSendFull.Text = "Gửi Full";
            this.btnSendFull.Symbol = 61527;
            this.btnSendFull.Dock = DockStyle.Fill;
            this.btnSendFull.Click += (s, e) => SendFull();

            this.rowFull.Controls.Add(this.txtFull, 0, 0);
            this.rowFull.Controls.Add(this.btnSendFull, 1, 0);

            // Log
            this.opLog.Dock = DockStyle.Fill;
            this.opLog.Font = new Font("Consolas", 10);

            // Add to root
            this.root.Controls.Add(this.rowServer, 0, 0);
            this.root.Controls.Add(this.rowRandom, 0, 1);
            this.root.Controls.Add(this.rowFull, 0, 2);
            this.root.Controls.Add(this.opLog, 0, 3);

            this.Controls.Add(this.root);
            this.Name = "FCameraSimulator";
            this.Size = new Size(874, 679);
            this.ResumeLayout(false);
        }
    }
}

