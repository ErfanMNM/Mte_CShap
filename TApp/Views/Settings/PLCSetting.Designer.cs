namespace TApp.Views.Settings
{
    partial class PLCSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            tabPage3 = new TabPage();
            uiTableLayoutPanel10 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel11 = new Sunny.UI.UITableLayoutPanel();
            uiNumPadTextBox5 = new Sunny.UI.UINumPadTextBox();
            ipCPLPort = new Sunny.UI.UINumPadTextBox();
            uiPanel11 = new Sunny.UI.UIPanel();
            ipValueCust = new Sunny.UI.UINumPadTextBox();
            opValueCus = new Sunny.UI.UITextBox();
            ipCPLCIP = new Sunny.UI.UITextBox();
            uiSymbolButton1 = new Sunny.UI.UISymbolButton();
            uiSymbolButton2 = new Sunny.UI.UISymbolButton();
            tabPage1 = new TabPage();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiPanel3 = new Sunny.UI.UIPanel();
            ipDelayTriger = new Sunny.UI.UINumPadTextBox();
            ipDelayReject = new Sunny.UI.UINumPadTextBox();
            ipRejectStreng = new Sunny.UI.UINumPadTextBox();
            opDelayTriger = new Sunny.UI.UITextBox();
            opDelayReject = new Sunny.UI.UITextBox();
            opRejectStreng = new Sunny.UI.UITextBox();
            uiTitlePanel3 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel22 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel23 = new Sunny.UI.UITableLayoutPanel();
            btnSave = new Sunny.UI.UISymbolButton();
            btnUndo = new Sunny.UI.UISymbolButton();
            btnDelete = new Sunny.UI.UISymbolButton();
            uiTableLayoutPanel16 = new Sunny.UI.UITableLayoutPanel();
            btnNewRecipe = new Sunny.UI.UISymbolButton();
            ipRecipe = new Sunny.UI.UIComboBox();
            uiTabControl1 = new Sunny.UI.UITabControl();
            omronplC_Hsl1 = new TTManager.PLCHelpers.OmronPLC_Hsl(components);
            backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            tabPage3.SuspendLayout();
            uiTableLayoutPanel10.SuspendLayout();
            uiTableLayoutPanel11.SuspendLayout();
            tabPage1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            uiTableLayoutPanel2.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiTitlePanel3.SuspendLayout();
            uiTableLayoutPanel22.SuspendLayout();
            uiTableLayoutPanel23.SuspendLayout();
            uiTableLayoutPanel16.SuspendLayout();
            uiTabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // backgroundWorker1
            // 
           // backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(uiTableLayoutPanel10);
            tabPage3.Location = new Point(0, 40);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(840, 612);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Tùy Chỉnh";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // uiTableLayoutPanel10
            // 
            uiTableLayoutPanel10.ColumnCount = 2;
            uiTableLayoutPanel10.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 78.77698F));
            uiTableLayoutPanel10.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.22302F));
            uiTableLayoutPanel10.Controls.Add(uiTableLayoutPanel11, 0, 0);
            uiTableLayoutPanel10.Controls.Add(uiSymbolButton1, 0, 1);
            uiTableLayoutPanel10.Controls.Add(uiSymbolButton2, 1, 1);
            uiTableLayoutPanel10.Location = new Point(3, 3);
            uiTableLayoutPanel10.Name = "uiTableLayoutPanel10";
            uiTableLayoutPanel10.RowCount = 2;
            uiTableLayoutPanel10.RowStyles.Add(new RowStyle(SizeType.Percent, 79.74683F));
            uiTableLayoutPanel10.RowStyles.Add(new RowStyle(SizeType.Percent, 20.25316F));
            uiTableLayoutPanel10.Size = new Size(834, 395);
            uiTableLayoutPanel10.TabIndex = 0;
            uiTableLayoutPanel10.TagString = null;
            // 
            // uiTableLayoutPanel11
            // 
            uiTableLayoutPanel11.ColumnCount = 3;
            uiTableLayoutPanel11.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.49616F));
            uiTableLayoutPanel11.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39.32412F));
            uiTableLayoutPanel11.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.17972F));
            uiTableLayoutPanel11.Controls.Add(uiNumPadTextBox5, 0, 1);
            uiTableLayoutPanel11.Controls.Add(ipCPLPort, 2, 0);
            uiTableLayoutPanel11.Controls.Add(uiPanel11, 0, 0);
            uiTableLayoutPanel11.Controls.Add(ipValueCust, 1, 1);
            uiTableLayoutPanel11.Controls.Add(opValueCus, 2, 1);
            uiTableLayoutPanel11.Controls.Add(ipCPLCIP, 1, 0);
            uiTableLayoutPanel11.Dock = DockStyle.Fill;
            uiTableLayoutPanel11.Location = new Point(3, 3);
            uiTableLayoutPanel11.Name = "uiTableLayoutPanel11";
            uiTableLayoutPanel11.RowCount = 5;
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            uiTableLayoutPanel11.Size = new Size(651, 309);
            uiTableLayoutPanel11.TabIndex = 1;
            uiTableLayoutPanel11.TagString = null;
            // 
            // uiNumPadTextBox5
            // 
            uiNumPadTextBox5.Dock = DockStyle.Fill;
            uiNumPadTextBox5.FillColor = Color.White;
            uiNumPadTextBox5.Font = new Font("Microsoft Sans Serif", 12F);
            uiNumPadTextBox5.Location = new Point(2, 63);
            uiNumPadTextBox5.Margin = new Padding(2);
            uiNumPadTextBox5.Minimum = 0D;
            uiNumPadTextBox5.MinimumSize = new Size(63, 0);
            uiNumPadTextBox5.Name = "uiNumPadTextBox5";
            uiNumPadTextBox5.NumPadType = Sunny.UI.NumPadType.Integer;
            uiNumPadTextBox5.Padding = new Padding(0, 0, 30, 2);
            uiNumPadTextBox5.Size = new Size(175, 57);
            uiNumPadTextBox5.SymbolDropDown = 557532;
            uiNumPadTextBox5.SymbolNormal = 557532;
            uiNumPadTextBox5.SymbolSize = 30;
            uiNumPadTextBox5.TabIndex = 18;
            uiNumPadTextBox5.Text = "D100";
            uiNumPadTextBox5.TextAlignment = ContentAlignment.MiddleLeft;
            uiNumPadTextBox5.Watermark = "";
            // 
            // ipCPLPort
            // 
            ipCPLPort.Dock = DockStyle.Fill;
            ipCPLPort.FillColor = Color.White;
            ipCPLPort.Font = new Font("Microsoft Sans Serif", 12F);
            ipCPLPort.Location = new Point(437, 2);
            ipCPLPort.Margin = new Padding(2);
            ipCPLPort.Minimum = 0D;
            ipCPLPort.MinimumSize = new Size(63, 0);
            ipCPLPort.Name = "ipCPLPort";
            ipCPLPort.NumPadType = Sunny.UI.NumPadType.Integer;
            ipCPLPort.Padding = new Padding(0, 0, 30, 2);
            ipCPLPort.Size = new Size(212, 57);
            ipCPLPort.SymbolDropDown = 557532;
            ipCPLPort.SymbolNormal = 557532;
            ipCPLPort.SymbolSize = 30;
            ipCPLPort.TabIndex = 16;
            ipCPLPort.Text = "9600";
            ipCPLPort.TextAlignment = ContentAlignment.MiddleLeft;
            ipCPLPort.Watermark = "";
            // 
            // uiPanel11
            // 
            uiPanel11.Dock = DockStyle.Fill;
            uiPanel11.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel11.Location = new Point(2, 2);
            uiPanel11.Margin = new Padding(2);
            uiPanel11.MinimumSize = new Size(1, 1);
            uiPanel11.Name = "uiPanel11";
            uiPanel11.Size = new Size(175, 57);
            uiPanel11.TabIndex = 0;
            uiPanel11.Text = "Thông tin PLC";
            uiPanel11.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // ipValueCust
            // 
            ipValueCust.Dock = DockStyle.Fill;
            ipValueCust.FillColor = Color.White;
            ipValueCust.Font = new Font("Microsoft Sans Serif", 12F);
            ipValueCust.Location = new Point(181, 63);
            ipValueCust.Margin = new Padding(2);
            ipValueCust.Minimum = 0D;
            ipValueCust.MinimumSize = new Size(63, 0);
            ipValueCust.Name = "ipValueCust";
            ipValueCust.NumPadType = Sunny.UI.NumPadType.Integer;
            ipValueCust.Padding = new Padding(0, 0, 30, 2);
            ipValueCust.Size = new Size(252, 57);
            ipValueCust.SymbolDropDown = 557532;
            ipValueCust.SymbolNormal = 557532;
            ipValueCust.SymbolSize = 30;
            ipValueCust.TabIndex = 4;
            ipValueCust.TextAlignment = ContentAlignment.MiddleLeft;
            ipValueCust.Watermark = "";
            // 
            // opValueCus
            // 
            opValueCus.Cursor = Cursors.IBeam;
            opValueCus.Dock = DockStyle.Fill;
            opValueCus.FillDisableColor = Color.White;
            opValueCus.FillReadOnlyColor = Color.White;
            opValueCus.Font = new Font("Microsoft Sans Serif", 12F);
            opValueCus.Location = new Point(437, 63);
            opValueCus.Margin = new Padding(2);
            opValueCus.Minimum = 0D;
            opValueCus.MinimumSize = new Size(1, 16);
            opValueCus.Name = "opValueCus";
            opValueCus.Padding = new Padding(5);
            opValueCus.ReadOnly = true;
            opValueCus.ShowText = false;
            opValueCus.Size = new Size(212, 57);
            opValueCus.TabIndex = 7;
            opValueCus.Text = "0";
            opValueCus.TextAlignment = ContentAlignment.MiddleLeft;
            opValueCus.Watermark = "";
            // 
            // ipCPLCIP
            // 
            ipCPLCIP.Cursor = Cursors.IBeam;
            ipCPLCIP.Font = new Font("Microsoft Sans Serif", 12F);
            ipCPLCIP.Location = new Point(183, 5);
            ipCPLCIP.Margin = new Padding(4, 5, 4, 5);
            ipCPLCIP.MinimumSize = new Size(1, 16);
            ipCPLCIP.Name = "ipCPLCIP";
            ipCPLCIP.Padding = new Padding(5);
            ipCPLCIP.ShowText = false;
            ipCPLCIP.Size = new Size(248, 51);
            ipCPLCIP.TabIndex = 17;
            ipCPLCIP.Text = "192.168.250.1";
            ipCPLCIP.TextAlignment = ContentAlignment.MiddleLeft;
            ipCPLCIP.Watermark = "";
            // 
            // uiSymbolButton1
            // 
            uiSymbolButton1.Cursor = Cursors.Hand;
            uiSymbolButton1.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton1.Location = new Point(3, 318);
            uiSymbolButton1.MinimumSize = new Size(1, 1);
            uiSymbolButton1.Name = "uiSymbolButton1";
            uiSymbolButton1.Size = new Size(651, 74);
            uiSymbolButton1.TabIndex = 2;
            uiSymbolButton1.Text = "Đọc lên";
            uiSymbolButton1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            uiSymbolButton1.Click += uiSymbolButton1_Click;
            // 
            // uiSymbolButton2
            // 
            uiSymbolButton2.Cursor = Cursors.Hand;
            uiSymbolButton2.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton2.Location = new Point(660, 318);
            uiSymbolButton2.MinimumSize = new Size(1, 1);
            uiSymbolButton2.Name = "uiSymbolButton2";
            uiSymbolButton2.Size = new Size(171, 74);
            uiSymbolButton2.TabIndex = 3;
            uiSymbolButton2.Text = "Ghi xuống";
            uiSymbolButton2.TipsFont = new Font("Microsoft Sans Serif", 9F);
            uiSymbolButton2.Click += uiSymbolButton2_Click;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(uiTableLayoutPanel1);
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(840, 612);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Camera Trước";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel2, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 73.36601F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 26.63399F));
            uiTableLayoutPanel1.Size = new Size(840, 612);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            //uiTableLayoutPanel1.Paint += uiTableLayoutPanel1_Paint;
            // 
            // uiTitlePanel2
            // 
            uiTitlePanel2.Controls.Add(webView21);
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel2.Location = new Point(2, 2);
            uiTitlePanel2.Margin = new Padding(2);
            uiTitlePanel2.MinimumSize = new Size(1, 1);
            uiTitlePanel2.Name = "uiTitlePanel2";
            uiTitlePanel2.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel2.ShowText = false;
            uiTitlePanel2.Size = new Size(836, 445);
            uiTitlePanel2.TabIndex = 3;
            uiTitlePanel2.Text = "ẢNH TỪ CAMERA";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(1, 35);
            webView21.Name = "webView21";
            webView21.Size = new Size(834, 409);
            webView21.TabIndex = 0;
            webView21.ZoomFactor = 1D;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55.63549F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44.36451F));
            uiTableLayoutPanel2.Controls.Add(uiTitlePanel1, 0, 0);
            uiTableLayoutPanel2.Controls.Add(uiTitlePanel3, 1, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(2, 451);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(836, 159);
            uiTableLayoutPanel2.TabIndex = 4;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel3);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 2);
            uiTitlePanel1.Margin = new Padding(2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(461, 155);
            uiTitlePanel1.TabIndex = 4;
            uiTitlePanel1.Text = "THÔNG SỐ PLC";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 3;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.14286F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34.5055F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.35165F));
            uiTableLayoutPanel3.Controls.Add(uiPanel1, 0, 0);
            uiTableLayoutPanel3.Controls.Add(uiPanel2, 0, 1);
            uiTableLayoutPanel3.Controls.Add(uiPanel3, 0, 2);
            uiTableLayoutPanel3.Controls.Add(ipDelayTriger, 1, 0);
            uiTableLayoutPanel3.Controls.Add(ipDelayReject, 1, 1);
            uiTableLayoutPanel3.Controls.Add(ipRejectStreng, 1, 2);
            uiTableLayoutPanel3.Controls.Add(opDelayTriger, 2, 0);
            uiTableLayoutPanel3.Controls.Add(opDelayReject, 2, 1);
            uiTableLayoutPanel3.Controls.Add(opRejectStreng, 2, 2);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(1, 35);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 3;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            uiTableLayoutPanel3.Size = new Size(459, 119);
            uiTableLayoutPanel3.TabIndex = 0;
            uiTableLayoutPanel3.TagString = null;
            // 
            // uiPanel1
            // 
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel1.Location = new Point(2, 2);
            uiPanel1.Margin = new Padding(2);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.Size = new Size(166, 35);
            uiPanel1.TabIndex = 0;
            uiPanel1.Text = "Trễ chụp (Delay Triger)";
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel2
            // 
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel2.Location = new Point(2, 41);
            uiPanel2.Margin = new Padding(2);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.Size = new Size(166, 35);
            uiPanel2.TabIndex = 1;
            uiPanel2.Text = "Trễ loại (Delay Reject)";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel3
            // 
            uiPanel3.Dock = DockStyle.Fill;
            uiPanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel3.Location = new Point(2, 80);
            uiPanel3.Margin = new Padding(2);
            uiPanel3.MinimumSize = new Size(1, 1);
            uiPanel3.Name = "uiPanel3";
            uiPanel3.Size = new Size(166, 37);
            uiPanel3.TabIndex = 2;
            uiPanel3.Text = "Độ mạnh bộ đá";
            uiPanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // ipDelayTriger
            // 
            ipDelayTriger.Dock = DockStyle.Fill;
            ipDelayTriger.FillColor = Color.White;
            ipDelayTriger.Font = new Font("Microsoft Sans Serif", 12F);
            ipDelayTriger.Location = new Point(172, 2);
            ipDelayTriger.Margin = new Padding(2);
            ipDelayTriger.Minimum = 0D;
            ipDelayTriger.MinimumSize = new Size(63, 0);
            ipDelayTriger.Name = "ipDelayTriger";
            ipDelayTriger.NumPadType = Sunny.UI.NumPadType.Integer;
            ipDelayTriger.Padding = new Padding(0, 0, 30, 2);
            ipDelayTriger.Size = new Size(154, 35);
            ipDelayTriger.SymbolDropDown = 557532;
            ipDelayTriger.SymbolNormal = 557532;
            ipDelayTriger.SymbolSize = 30;
            ipDelayTriger.TabIndex = 3;
            ipDelayTriger.TextAlignment = ContentAlignment.MiddleLeft;
            ipDelayTriger.Watermark = "";
            // 
            // ipDelayReject
            // 
            ipDelayReject.Dock = DockStyle.Fill;
            ipDelayReject.FillColor = Color.White;
            ipDelayReject.Font = new Font("Microsoft Sans Serif", 12F);
            ipDelayReject.Location = new Point(172, 41);
            ipDelayReject.Margin = new Padding(2);
            ipDelayReject.Minimum = 0D;
            ipDelayReject.MinimumSize = new Size(63, 0);
            ipDelayReject.Name = "ipDelayReject";
            ipDelayReject.NumPadType = Sunny.UI.NumPadType.Integer;
            ipDelayReject.Padding = new Padding(0, 0, 30, 2);
            ipDelayReject.Size = new Size(154, 35);
            ipDelayReject.SymbolDropDown = 557532;
            ipDelayReject.SymbolNormal = 557532;
            ipDelayReject.SymbolSize = 30;
            ipDelayReject.TabIndex = 4;
            ipDelayReject.TextAlignment = ContentAlignment.MiddleLeft;
            ipDelayReject.Watermark = "";
            // 
            // ipRejectStreng
            // 
            ipRejectStreng.Dock = DockStyle.Fill;
            ipRejectStreng.FillColor = Color.White;
            ipRejectStreng.Font = new Font("Microsoft Sans Serif", 12F);
            ipRejectStreng.Location = new Point(172, 80);
            ipRejectStreng.Margin = new Padding(2);
            ipRejectStreng.Minimum = 0D;
            ipRejectStreng.MinimumSize = new Size(63, 0);
            ipRejectStreng.Name = "ipRejectStreng";
            ipRejectStreng.NumPadType = Sunny.UI.NumPadType.Integer;
            ipRejectStreng.Padding = new Padding(0, 0, 30, 2);
            ipRejectStreng.Size = new Size(154, 37);
            ipRejectStreng.SymbolDropDown = 557532;
            ipRejectStreng.SymbolNormal = 557532;
            ipRejectStreng.SymbolSize = 30;
            ipRejectStreng.TabIndex = 5;
            ipRejectStreng.TextAlignment = ContentAlignment.MiddleLeft;
            ipRejectStreng.Watermark = "";
            // 
            // opDelayTriger
            // 
            opDelayTriger.Cursor = Cursors.IBeam;
            opDelayTriger.Dock = DockStyle.Fill;
            opDelayTriger.FillDisableColor = Color.White;
            opDelayTriger.FillReadOnlyColor = Color.White;
            opDelayTriger.Font = new Font("Microsoft Sans Serif", 12F);
            opDelayTriger.Location = new Point(330, 2);
            opDelayTriger.Margin = new Padding(2);
            opDelayTriger.Minimum = 0D;
            opDelayTriger.MinimumSize = new Size(1, 16);
            opDelayTriger.Name = "opDelayTriger";
            opDelayTriger.Padding = new Padding(5);
            opDelayTriger.ReadOnly = true;
            opDelayTriger.ShowText = false;
            opDelayTriger.Size = new Size(127, 35);
            opDelayTriger.TabIndex = 6;
            opDelayTriger.Text = "0";
            opDelayTriger.TextAlignment = ContentAlignment.MiddleLeft;
            opDelayTriger.Watermark = "";
            // 
            // opDelayReject
            // 
            opDelayReject.Cursor = Cursors.IBeam;
            opDelayReject.Dock = DockStyle.Fill;
            opDelayReject.FillDisableColor = Color.White;
            opDelayReject.FillReadOnlyColor = Color.White;
            opDelayReject.Font = new Font("Microsoft Sans Serif", 12F);
            opDelayReject.Location = new Point(330, 41);
            opDelayReject.Margin = new Padding(2);
            opDelayReject.Minimum = 0D;
            opDelayReject.MinimumSize = new Size(1, 16);
            opDelayReject.Name = "opDelayReject";
            opDelayReject.Padding = new Padding(5);
            opDelayReject.ReadOnly = true;
            opDelayReject.ShowText = false;
            opDelayReject.Size = new Size(127, 35);
            opDelayReject.TabIndex = 7;
            opDelayReject.Text = "0";
            opDelayReject.TextAlignment = ContentAlignment.MiddleLeft;
            opDelayReject.Watermark = "";
            // 
            // opRejectStreng
            // 
            opRejectStreng.Cursor = Cursors.IBeam;
            opRejectStreng.Dock = DockStyle.Fill;
            opRejectStreng.FillDisableColor = Color.White;
            opRejectStreng.FillReadOnlyColor = Color.White;
            opRejectStreng.Font = new Font("Microsoft Sans Serif", 12F);
            opRejectStreng.Location = new Point(330, 80);
            opRejectStreng.Margin = new Padding(2);
            opRejectStreng.Minimum = 0D;
            opRejectStreng.MinimumSize = new Size(1, 16);
            opRejectStreng.Name = "opRejectStreng";
            opRejectStreng.Padding = new Padding(5);
            opRejectStreng.ReadOnly = true;
            opRejectStreng.ShowText = false;
            opRejectStreng.Size = new Size(127, 37);
            opRejectStreng.TabIndex = 8;
            opRejectStreng.Text = "0";
            opRejectStreng.TextAlignment = ContentAlignment.MiddleLeft;
            opRejectStreng.Watermark = "";
            // 
            // uiTitlePanel3
            // 
            uiTitlePanel3.Controls.Add(uiTableLayoutPanel22);
            uiTitlePanel3.Dock = DockStyle.Fill;
            uiTitlePanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel3.Location = new Point(467, 2);
            uiTitlePanel3.Margin = new Padding(2);
            uiTitlePanel3.MinimumSize = new Size(1, 1);
            uiTitlePanel3.Name = "uiTitlePanel3";
            uiTitlePanel3.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel3.ShowText = false;
            uiTitlePanel3.Size = new Size(367, 155);
            uiTitlePanel3.TabIndex = 5;
            uiTitlePanel3.Text = "ĐIỀU CHỈNH";
            uiTitlePanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel22
            // 
            uiTableLayoutPanel22.ColumnCount = 1;
            uiTableLayoutPanel22.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48.02632F));
            uiTableLayoutPanel22.Controls.Add(uiTableLayoutPanel23, 0, 1);
            uiTableLayoutPanel22.Controls.Add(uiTableLayoutPanel16, 0, 0);
            uiTableLayoutPanel22.Dock = DockStyle.Fill;
            uiTableLayoutPanel22.Location = new Point(1, 35);
            uiTableLayoutPanel22.Name = "uiTableLayoutPanel22";
            uiTableLayoutPanel22.RowCount = 2;
            uiTableLayoutPanel22.RowStyles.Add(new RowStyle(SizeType.Percent, 49.04762F));
            uiTableLayoutPanel22.RowStyles.Add(new RowStyle(SizeType.Percent, 50.95238F));
            uiTableLayoutPanel22.Size = new Size(365, 119);
            uiTableLayoutPanel22.TabIndex = 1;
            uiTableLayoutPanel22.TagString = null;
            // 
            // uiTableLayoutPanel23
            // 
            uiTableLayoutPanel23.ColumnCount = 3;
            uiTableLayoutPanel23.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35.59322F));
            uiTableLayoutPanel23.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30.79096F));
            uiTableLayoutPanel23.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            uiTableLayoutPanel23.Controls.Add(btnSave, 0, 0);
            uiTableLayoutPanel23.Controls.Add(btnUndo, 1, 0);
            uiTableLayoutPanel23.Controls.Add(btnDelete, 2, 0);
            uiTableLayoutPanel23.Dock = DockStyle.Fill;
            uiTableLayoutPanel23.Location = new Point(3, 61);
            uiTableLayoutPanel23.Name = "uiTableLayoutPanel23";
            uiTableLayoutPanel23.RowCount = 1;
            uiTableLayoutPanel23.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel23.Size = new Size(359, 55);
            uiTableLayoutPanel23.TabIndex = 1;
            uiTableLayoutPanel23.TagString = null;
            // 
            // btnSave
            // 
            btnSave.Cursor = Cursors.Hand;
            btnSave.Dock = DockStyle.Fill;
            btnSave.FillColor = Color.Blue;
            btnSave.FillHoverColor = Color.Blue;
            btnSave.FillPressColor = Color.FromArgb(0, 0, 192);
            btnSave.FillSelectedColor = Color.Navy;
            btnSave.Font = new Font("Microsoft Sans Serif", 12F);
            btnSave.Location = new Point(3, 3);
            btnSave.MinimumSize = new Size(1, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(122, 49);
            btnSave.Symbol = 361926;
            btnSave.TabIndex = 0;
            btnSave.Text = "Áp dụng";
            btnSave.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSave.Click += btnSave_Click;
            // 
            // btnUndo
            // 
            btnUndo.Cursor = Cursors.Hand;
            btnUndo.Dock = DockStyle.Fill;
            btnUndo.Font = new Font("Microsoft Sans Serif", 12F);
            btnUndo.Location = new Point(131, 3);
            btnUndo.MinimumSize = new Size(1, 1);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(104, 49);
            btnUndo.Symbol = 61587;
            btnUndo.TabIndex = 1;
            btnUndo.Text = "Tải lên";
            btnUndo.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnUndo.Click += btnUndo_Click;
            // 
            // btnDelete
            // 
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Dock = DockStyle.Fill;
            btnDelete.FillColor = Color.FromArgb(255, 128, 128);
            btnDelete.Font = new Font("Microsoft Sans Serif", 12F);
            btnDelete.Location = new Point(241, 3);
            btnDelete.MinimumSize = new Size(1, 1);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(115, 49);
            btnDelete.Symbol = 61460;
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Xóa";
            btnDelete.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnDelete.Click += btnDelete_Click;
            // 
            // uiTableLayoutPanel16
            // 
            uiTableLayoutPanel16.ColumnCount = 2;
            uiTableLayoutPanel16.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69.46565F));
            uiTableLayoutPanel16.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30.53435F));
            uiTableLayoutPanel16.Controls.Add(btnNewRecipe, 1, 0);
            uiTableLayoutPanel16.Controls.Add(ipRecipe, 0, 0);
            uiTableLayoutPanel16.Dock = DockStyle.Fill;
            uiTableLayoutPanel16.Location = new Point(3, 3);
            uiTableLayoutPanel16.Name = "uiTableLayoutPanel16";
            uiTableLayoutPanel16.RowCount = 1;
            uiTableLayoutPanel16.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel16.Size = new Size(359, 52);
            uiTableLayoutPanel16.TabIndex = 2;
            uiTableLayoutPanel16.TagString = null;
            // 
            // btnNewRecipe
            // 
            btnNewRecipe.Cursor = Cursors.Hand;
            btnNewRecipe.Dock = DockStyle.Fill;
            btnNewRecipe.FillColor = Color.FromArgb(0, 192, 0);
            btnNewRecipe.FillHoverColor = Color.FromArgb(0, 192, 0);
            btnNewRecipe.FillPressColor = Color.Green;
            btnNewRecipe.FillSelectedColor = Color.Green;
            btnNewRecipe.Font = new Font("Microsoft Sans Serif", 12F);
            btnNewRecipe.Location = new Point(252, 3);
            btnNewRecipe.MinimumSize = new Size(1, 1);
            btnNewRecipe.Name = "btnNewRecipe";
            btnNewRecipe.Size = new Size(104, 46);
            btnNewRecipe.Symbol = 61543;
            btnNewRecipe.TabIndex = 2;
            btnNewRecipe.Text = "Mới";
            btnNewRecipe.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnNewRecipe.Click += btnNewRecipe_Click;
            // 
            // ipRecipe
            // 
            ipRecipe.DataSource = null;
            ipRecipe.Dock = DockStyle.Fill;
            ipRecipe.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            ipRecipe.FillColor = Color.White;
            ipRecipe.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ipRecipe.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipRecipe.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipRecipe.Location = new Point(2, 2);
            ipRecipe.Margin = new Padding(2);
            ipRecipe.MinimumSize = new Size(63, 0);
            ipRecipe.Name = "ipRecipe";
            ipRecipe.Padding = new Padding(0, 0, 30, 2);
            ipRecipe.Size = new Size(245, 48);
            ipRecipe.SymbolSize = 24;
            ipRecipe.TabIndex = 1;
            ipRecipe.TextAlignment = ContentAlignment.MiddleLeft;
            ipRecipe.Watermark = "";
            ipRecipe.SelectedIndexChanged += ipRecipe_SelectedIndexChanged;
            // 
            // uiTabControl1
            // 
            uiTabControl1.Controls.Add(tabPage1);
            uiTabControl1.Controls.Add(tabPage3);
            uiTabControl1.Dock = DockStyle.Fill;
            uiTabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            uiTabControl1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTabControl1.ItemSize = new Size(150, 40);
            uiTabControl1.Location = new Point(0, 0);
            uiTabControl1.MainPage = "";
            uiTabControl1.Name = "uiTabControl1";
            uiTabControl1.SelectedIndex = 0;
            uiTabControl1.Size = new Size(840, 652);
            uiTabControl1.SizeMode = TabSizeMode.Fixed;
            uiTabControl1.TabIndex = 0;
            uiTabControl1.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            uiTabControl1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // omronplC_Hsl1
            // 
            omronplC_Hsl1.PLC_IP = "127.0.0.1";
            omronplC_Hsl1.PLC_PORT = 9600;
            omronplC_Hsl1.PLC_Ready_DM = "D16";
            omronplC_Hsl1.PLC_STATUS = TTManager.PLCHelpers.OmronPLC_Hsl.PLCStatus.Disconnect;
            omronplC_Hsl1.Ready = 0;
            omronplC_Hsl1.Time_Update = 300;
            // 
            // PLCSetting
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(840, 652);
            Controls.Add(uiTabControl1);
            Name = "PLCSetting";
            Symbol = 561288;
            Text = "Cài PLC";
            Initialize += PLCSetting_Initialize;
            Finalize += PLCSetting_Finalize;
            tabPage3.ResumeLayout(false);
            uiTableLayoutPanel10.ResumeLayout(false);
            uiTableLayoutPanel11.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiTitlePanel3.ResumeLayout(false);
            uiTableLayoutPanel22.ResumeLayout(false);
            uiTableLayoutPanel23.ResumeLayout(false);
            uiTableLayoutPanel16.ResumeLayout(false);
            uiTabControl1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private TabPage tabPage3;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel10;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel11;
        private Sunny.UI.UINumPadTextBox uiNumPadTextBox5;
        private Sunny.UI.UINumPadTextBox ipCPLPort;
        private Sunny.UI.UIPanel uiPanel11;
        private Sunny.UI.UINumPadTextBox ipValueCust;
        private Sunny.UI.UITextBox opValueCus;
        private Sunny.UI.UITextBox ipCPLCIP;
        private Sunny.UI.UISymbolButton uiSymbolButton1;
        private Sunny.UI.UISymbolButton uiSymbolButton2;
        private TabPage tabPage1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel3;
        private Sunny.UI.UINumPadTextBox ipDelayTriger;
        private Sunny.UI.UINumPadTextBox ipDelayReject;
        private Sunny.UI.UINumPadTextBox ipRejectStreng;
        private Sunny.UI.UITextBox opDelayTriger;
        private Sunny.UI.UITextBox opDelayReject;
        private Sunny.UI.UITextBox opRejectStreng;
        private Sunny.UI.UITitlePanel uiTitlePanel3;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel22;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel23;
        private Sunny.UI.UISymbolButton btnSave;
        private Sunny.UI.UISymbolButton btnUndo;
        private Sunny.UI.UISymbolButton btnDelete;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel16;
        private Sunny.UI.UISymbolButton btnNewRecipe;
        private Sunny.UI.UIComboBox ipRecipe;
        private Sunny.UI.UITabControl uiTabControl1;
        private TTManager.PLCHelpers.OmronPLC_Hsl omronplC_Hsl1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
    }
}