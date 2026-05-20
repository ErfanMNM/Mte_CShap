namespace TApp.Views.Dashboard
{
    partial class FDashboard
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
            MainPanel = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel15 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel17 = new Sunny.UI.UITitlePanel();
            opProductionSpeed = new Sunny.UI.UIDigitalLabel();
            uiTitlePanel18 = new Sunny.UI.UITitlePanel();
            opTimeout = new Sunny.UI.UIDigitalLabel();
            uiTitlePanel19 = new Sunny.UI.UITitlePanel();
            opBatchCount = new Sunny.UI.UIDigitalLabel();
            pnResult = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            opResopse = new Sunny.UI.UIRichTextBox();
            opResultStatus = new Sunny.UI.UIPanel();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel7 = new Sunny.UI.UITitlePanel();
            opFail = new Sunny.UI.UIDigitalLabel();
            uiTitlePanel6 = new Sunny.UI.UITitlePanel();
            opPassCount = new Sunny.UI.UIDigitalLabel();
            uiTitlePanel5 = new Sunny.UI.UITitlePanel();
            opTotalCount = new Sunny.UI.UIDigitalLabel();
            uiTitlePanel3 = new Sunny.UI.UITitlePanel();
            opNoteCameraView = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            opView = new Sunny.UI.UIListBox();
            tabPage2 = new TabPage();
            uiListBox1 = new Sunny.UI.UIListBox();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel14 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel8 = new Sunny.UI.UITableLayoutPanel();
            ipRejectStreng = new Sunny.UI.UINumPadTextBox();
            ipDelayRject = new Sunny.UI.UINumPadTextBox();
            ipDelayTriger = new Sunny.UI.UINumPadTextBox();
            uiPanel17 = new Sunny.UI.UIPanel();
            uiPanel16 = new Sunny.UI.UIPanel();
            uiPanel15 = new Sunny.UI.UIPanel();
            uiPanel11 = new Sunny.UI.UIPanel();
            uiPanel13 = new Sunny.UI.UIPanel();
            uiPanel14 = new Sunny.UI.UIPanel();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiPanel6 = new Sunny.UI.UIPanel();
            uiPanel7 = new Sunny.UI.UIPanel();
            opRejectStreng = new Sunny.UI.UITextBox();
            uiTableLayoutPanel16 = new Sunny.UI.UITableLayoutPanel();
            uiPanel18 = new Sunny.UI.UIPanel();
            opDelayRject = new Sunny.UI.UITextBox();
            uiTableLayoutPanel17 = new Sunny.UI.UITableLayoutPanel();
            opDelayTriger = new Sunny.UI.UITextBox();
            uiTitlePanel4 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel11 = new Sunny.UI.UITableLayoutPanel();
            opLineName = new Sunny.UI.UITextBox();
            uiPanel12 = new Sunny.UI.UIPanel();
            uiTableLayoutPanel14 = new Sunny.UI.UITableLayoutPanel();
            ipBatchNo = new Sunny.UI.UIComboBox();
            uiPanel8 = new Sunny.UI.UIPanel();
            uiPanel9 = new Sunny.UI.UIPanel();
            uiPanel10 = new Sunny.UI.UIPanel();
            opSCount = new Sunny.UI.UITextBox();
            uiTableLayoutPanel13 = new Sunny.UI.UITableLayoutPanel();
            ipBarcode = new Sunny.UI.UITextBox();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel13 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel10 = new Sunny.UI.UITableLayoutPanel();
            opAppStatus = new Sunny.UI.UIPanel();
            opAppStatusCode = new Sunny.UI.UIDigitalLabel();
            uiTitlePanel12 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel9 = new Sunny.UI.UITableLayoutPanel();
            opServerStatus = new Sunny.UI.UIPanel();
            opServerLed = new Sunny.UI.UILedBulb();
            uiTitlePanel11 = new Sunny.UI.UITitlePanel();
            networkStrength1 = new TTManager.Internet.NetworkStrength();
            uiTitlePanel10 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel7 = new Sunny.UI.UITableLayoutPanel();
            uiPanel4 = new Sunny.UI.UIPanel();
            uiLedBulb3 = new Sunny.UI.UILedBulb();
            uiTitlePanel9 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel6 = new Sunny.UI.UITableLayoutPanel();
            opPLCStatus = new Sunny.UI.UIPanel();
            opPLCLed = new Sunny.UI.UILedBulb();
            uiTitlePanel8 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            opCameraStatus = new Sunny.UI.UIPanel();
            opCameraLed = new Sunny.UI.UILedBulb();
            uiTableLayoutPanel12 = new Sunny.UI.UITableLayoutPanel();
            btnChangeBatch = new Sunny.UI.UISymbolButton();
            btnScan = new Sunny.UI.UISymbolButton();
            uiSymbolButton3 = new Sunny.UI.UISymbolButton();
            btnPLCSetting = new Sunny.UI.UISymbolButton();
            btnClearPLC = new Sunny.UI.UISymbolButton();
            btnResetCounterPLC = new Sunny.UI.UISymbolButton();
            opAlarm = new Sunny.UI.UIPanel();
            omronPLC_Hsl1 = new TTManager.PLCHelpers.OmronPLC_Hsl(components);
            WK_Camera = new System.ComponentModel.BackgroundWorker();
            WK_Render_HMI = new System.ComponentModel.BackgroundWorker();
            erP_Google2 = new TTManager.Masan.ERP_Google(components);
            WK_Dequeue = new System.ComponentModel.BackgroundWorker();
            WK_Load_Counter = new System.ComponentModel.BackgroundWorker();
            MainPanel.SuspendLayout();
            uiTableLayoutPanel15.SuspendLayout();
            uiTitlePanel17.SuspendLayout();
            uiTitlePanel18.SuspendLayout();
            uiTitlePanel19.SuspendLayout();
            pnResult.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiTitlePanel7.SuspendLayout();
            uiTitlePanel6.SuspendLayout();
            uiTitlePanel5.SuspendLayout();
            uiTitlePanel3.SuspendLayout();
            opNoteCameraView.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel14.SuspendLayout();
            uiTableLayoutPanel8.SuspendLayout();
            uiPanel11.SuspendLayout();
            uiPanel13.SuspendLayout();
            uiPanel2.SuspendLayout();
            uiPanel6.SuspendLayout();
            uiTableLayoutPanel16.SuspendLayout();
            uiTableLayoutPanel17.SuspendLayout();
            uiTitlePanel4.SuspendLayout();
            uiTableLayoutPanel11.SuspendLayout();
            uiTableLayoutPanel14.SuspendLayout();
            uiTableLayoutPanel13.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            uiTitlePanel13.SuspendLayout();
            uiTableLayoutPanel10.SuspendLayout();
            uiTitlePanel12.SuspendLayout();
            uiTableLayoutPanel9.SuspendLayout();
            uiTitlePanel11.SuspendLayout();
            uiTitlePanel10.SuspendLayout();
            uiTableLayoutPanel7.SuspendLayout();
            uiTitlePanel9.SuspendLayout();
            uiTableLayoutPanel6.SuspendLayout();
            uiTitlePanel8.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            uiTableLayoutPanel12.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.ColumnCount = 2;
            MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64.30206F));
            MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35.69794F));
            MainPanel.Controls.Add(uiTableLayoutPanel15, 1, 1);
            MainPanel.Controls.Add(pnResult, 0, 0);
            MainPanel.Controls.Add(uiTitlePanel2, 1, 0);
            MainPanel.Controls.Add(uiTitlePanel3, 0, 2);
            MainPanel.Controls.Add(uiTableLayoutPanel1, 1, 2);
            MainPanel.Controls.Add(opAlarm, 0, 1);
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.Location = new Point(0, 0);
            MainPanel.Margin = new Padding(2);
            MainPanel.Name = "MainPanel";
            MainPanel.RowCount = 3;
            MainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 115F));
            MainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
            MainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            MainPanel.Size = new Size(1748, 943);
            MainPanel.TabIndex = 0;
            MainPanel.TagString = null;
            // 
            // uiTableLayoutPanel15
            // 
            uiTableLayoutPanel15.ColumnCount = 3;
            uiTableLayoutPanel15.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel15.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel15.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel15.Controls.Add(uiTitlePanel17, 2, 0);
            uiTableLayoutPanel15.Controls.Add(uiTitlePanel18, 1, 0);
            uiTableLayoutPanel15.Controls.Add(uiTitlePanel19, 0, 0);
            uiTableLayoutPanel15.Dock = DockStyle.Fill;
            uiTableLayoutPanel15.Location = new Point(1127, 118);
            uiTableLayoutPanel15.Name = "uiTableLayoutPanel15";
            uiTableLayoutPanel15.RowCount = 1;
            uiTableLayoutPanel15.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel15.Size = new Size(618, 68);
            uiTableLayoutPanel15.TabIndex = 5;
            uiTableLayoutPanel15.TagString = null;
            // 
            // uiTitlePanel17
            // 
            uiTitlePanel17.BackColor = Color.White;
            uiTitlePanel17.Controls.Add(opProductionSpeed);
            uiTitlePanel17.Dock = DockStyle.Fill;
            uiTitlePanel17.FillColor = Color.White;
            uiTitlePanel17.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel17.Location = new Point(413, 1);
            uiTitlePanel17.Margin = new Padding(1);
            uiTitlePanel17.MinimumSize = new Size(1, 1);
            uiTitlePanel17.Name = "uiTitlePanel17";
            uiTitlePanel17.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel17.RectColor = Color.Silver;
            uiTitlePanel17.ShowText = false;
            uiTitlePanel17.Size = new Size(204, 66);
            uiTitlePanel17.TabIndex = 2;
            uiTitlePanel17.Text = "Tốc độ sản xuất";
            uiTitlePanel17.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel17.TitleColor = Color.Black;
            uiTitlePanel17.TitleHeight = 30;
            // 
            // opProductionSpeed
            // 
            opProductionSpeed.BackColor = Color.White;
            opProductionSpeed.DecimalPlaces = 0;
            opProductionSpeed.DigitalSize = 12;
            opProductionSpeed.Dock = DockStyle.Fill;
            opProductionSpeed.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opProductionSpeed.ForeColor = Color.Black;
            opProductionSpeed.Location = new Point(1, 30);
            opProductionSpeed.Margin = new Padding(2);
            opProductionSpeed.MinimumSize = new Size(1, 1);
            opProductionSpeed.Name = "opProductionSpeed";
            opProductionSpeed.Size = new Size(202, 35);
            opProductionSpeed.TabIndex = 0;
            opProductionSpeed.Text = "-2";
            opProductionSpeed.TextAlign = HorizontalAlignment.Center;
            opProductionSpeed.Value = -2D;
            // 
            // uiTitlePanel18
            // 
            uiTitlePanel18.BackColor = Color.White;
            uiTitlePanel18.Controls.Add(opTimeout);
            uiTitlePanel18.Dock = DockStyle.Fill;
            uiTitlePanel18.FillColor = Color.White;
            uiTitlePanel18.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel18.Location = new Point(207, 1);
            uiTitlePanel18.Margin = new Padding(1);
            uiTitlePanel18.MinimumSize = new Size(1, 1);
            uiTitlePanel18.Name = "uiTitlePanel18";
            uiTitlePanel18.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel18.RectColor = Color.Silver;
            uiTitlePanel18.ShowText = false;
            uiTitlePanel18.Size = new Size(204, 66);
            uiTitlePanel18.TabIndex = 1;
            uiTitlePanel18.Text = "Quá hạn";
            uiTitlePanel18.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel18.TitleColor = SystemColors.MenuText;
            uiTitlePanel18.TitleHeight = 30;
            // 
            // opTimeout
            // 
            opTimeout.BackColor = Color.White;
            opTimeout.DecimalPlaces = 0;
            opTimeout.DigitalSize = 12;
            opTimeout.Dock = DockStyle.Fill;
            opTimeout.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opTimeout.ForeColor = Color.Black;
            opTimeout.Location = new Point(1, 30);
            opTimeout.Margin = new Padding(2);
            opTimeout.MinimumSize = new Size(1, 1);
            opTimeout.Name = "opTimeout";
            opTimeout.Size = new Size(202, 35);
            opTimeout.TabIndex = 0;
            opTimeout.Text = "-2";
            opTimeout.TextAlign = HorizontalAlignment.Center;
            opTimeout.Value = -2D;
            // 
            // uiTitlePanel19
            // 
            uiTitlePanel19.BackColor = Color.White;
            uiTitlePanel19.Controls.Add(opBatchCount);
            uiTitlePanel19.Dock = DockStyle.Fill;
            uiTitlePanel19.FillColor = Color.White;
            uiTitlePanel19.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel19.Location = new Point(1, 1);
            uiTitlePanel19.Margin = new Padding(1);
            uiTitlePanel19.MinimumSize = new Size(1, 1);
            uiTitlePanel19.Name = "uiTitlePanel19";
            uiTitlePanel19.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel19.RectColor = Color.Silver;
            uiTitlePanel19.ShowText = false;
            uiTitlePanel19.Size = new Size(204, 66);
            uiTitlePanel19.TabIndex = 0;
            uiTitlePanel19.Text = "Lô Hiện Tại";
            uiTitlePanel19.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel19.TitleColor = SystemColors.ControlText;
            uiTitlePanel19.TitleHeight = 30;
            // 
            // opBatchCount
            // 
            opBatchCount.BackColor = Color.White;
            opBatchCount.DecimalPlaces = 0;
            opBatchCount.DigitalSize = 12;
            opBatchCount.Dock = DockStyle.Fill;
            opBatchCount.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opBatchCount.ForeColor = Color.Black;
            opBatchCount.Location = new Point(1, 30);
            opBatchCount.Margin = new Padding(2);
            opBatchCount.MinimumSize = new Size(1, 1);
            opBatchCount.Name = "opBatchCount";
            opBatchCount.Size = new Size(202, 35);
            opBatchCount.TabIndex = 0;
            opBatchCount.Text = "-2";
            opBatchCount.TextAlign = HorizontalAlignment.Center;
            opBatchCount.Value = -2D;
            // 
            // pnResult
            // 
            pnResult.Controls.Add(uiTableLayoutPanel2);
            pnResult.Dock = DockStyle.Fill;
            pnResult.Font = new Font("Microsoft Sans Serif", 12F);
            pnResult.Location = new Point(2, 2);
            pnResult.Margin = new Padding(2);
            pnResult.MinimumSize = new Size(1, 1);
            pnResult.Name = "pnResult";
            pnResult.Padding = new Padding(1, 35, 1, 1);
            pnResult.Radius = 2;
            pnResult.ShowText = false;
            pnResult.Size = new Size(1120, 111);
            pnResult.TabIndex = 0;
            pnResult.Text = "KẾT QUẢ VỪA KIỂM";
            pnResult.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17.0840778F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 82.9159241F));
            uiTableLayoutPanel2.Controls.Add(opResopse, 1, 0);
            uiTableLayoutPanel2.Controls.Add(opResultStatus, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(1, 35);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(1118, 75);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // opResopse
            // 
            opResopse.Dock = DockStyle.Fill;
            opResopse.FillColor = Color.White;
            opResopse.Font = new Font("Microsoft Sans Serif", 12F);
            opResopse.Location = new Point(192, 2);
            opResopse.Margin = new Padding(2);
            opResopse.MinimumSize = new Size(1, 1);
            opResopse.Name = "opResopse";
            opResopse.Padding = new Padding(2);
            opResopse.ShowText = false;
            opResopse.Size = new Size(924, 71);
            opResopse.TabIndex = 0;
            opResopse.Text = "#1 Mã QR";
            opResopse.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opResultStatus
            // 
            opResultStatus.Dock = DockStyle.Fill;
            opResultStatus.FillColor = Color.Green;
            opResultStatus.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            opResultStatus.ForeColor = Color.White;
            opResultStatus.Location = new Point(2, 2);
            opResultStatus.Margin = new Padding(2);
            opResultStatus.MinimumSize = new Size(1, 1);
            opResultStatus.Name = "opResultStatus";
            opResultStatus.Size = new Size(186, 71);
            opResultStatus.TabIndex = 1;
            opResultStatus.Text = "TỐT";
            opResultStatus.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel2
            // 
            uiTitlePanel2.Controls.Add(uiTableLayoutPanel3);
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel2.Location = new Point(1126, 2);
            uiTitlePanel2.Margin = new Padding(2);
            uiTitlePanel2.MinimumSize = new Size(1, 1);
            uiTitlePanel2.Name = "uiTitlePanel2";
            uiTitlePanel2.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel2.Radius = 1;
            uiTitlePanel2.ShowText = false;
            uiTitlePanel2.Size = new Size(620, 111);
            uiTitlePanel2.TabIndex = 1;
            uiTitlePanel2.Text = "THỐNG KÊ SỐ LƯỢNG";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 3;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel3.Controls.Add(uiTitlePanel7, 2, 0);
            uiTableLayoutPanel3.Controls.Add(uiTitlePanel6, 1, 0);
            uiTableLayoutPanel3.Controls.Add(uiTitlePanel5, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(1, 35);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 1;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.Size = new Size(618, 75);
            uiTableLayoutPanel3.TabIndex = 0;
            uiTableLayoutPanel3.TagString = null;
            // 
            // uiTitlePanel7
            // 
            uiTitlePanel7.Controls.Add(opFail);
            uiTitlePanel7.Dock = DockStyle.Fill;
            uiTitlePanel7.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel7.Location = new Point(413, 1);
            uiTitlePanel7.Margin = new Padding(1);
            uiTitlePanel7.MinimumSize = new Size(1, 1);
            uiTitlePanel7.Name = "uiTitlePanel7";
            uiTitlePanel7.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel7.RectColor = Color.Silver;
            uiTitlePanel7.ShowText = false;
            uiTitlePanel7.Size = new Size(204, 73);
            uiTitlePanel7.TabIndex = 2;
            uiTitlePanel7.Text = "Số Loại";
            uiTitlePanel7.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel7.TitleColor = Color.FromArgb(192, 0, 0);
            uiTitlePanel7.TitleHeight = 30;
            // 
            // opFail
            // 
            opFail.BackColor = Color.FromArgb(255, 192, 192);
            opFail.DecimalPlaces = 0;
            opFail.DigitalSize = 12;
            opFail.Dock = DockStyle.Fill;
            opFail.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opFail.ForeColor = Color.Black;
            opFail.Location = new Point(1, 30);
            opFail.Margin = new Padding(2);
            opFail.MinimumSize = new Size(1, 1);
            opFail.Name = "opFail";
            opFail.Size = new Size(202, 42);
            opFail.TabIndex = 0;
            opFail.Text = "-2";
            opFail.TextAlign = HorizontalAlignment.Center;
            opFail.Value = -2D;
            opFail.DoubleClick += opFail_DoubleClick;
            // 
            // uiTitlePanel6
            // 
            uiTitlePanel6.Controls.Add(opPassCount);
            uiTitlePanel6.Dock = DockStyle.Fill;
            uiTitlePanel6.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel6.Location = new Point(207, 1);
            uiTitlePanel6.Margin = new Padding(1);
            uiTitlePanel6.MinimumSize = new Size(1, 1);
            uiTitlePanel6.Name = "uiTitlePanel6";
            uiTitlePanel6.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel6.RectColor = Color.Silver;
            uiTitlePanel6.ShowText = false;
            uiTitlePanel6.Size = new Size(204, 73);
            uiTitlePanel6.TabIndex = 1;
            uiTitlePanel6.Text = "Số Tốt";
            uiTitlePanel6.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel6.TitleColor = Color.Green;
            uiTitlePanel6.TitleHeight = 30;
            // 
            // opPassCount
            // 
            opPassCount.BackColor = Color.FromArgb(128, 255, 128);
            opPassCount.DecimalPlaces = 0;
            opPassCount.DigitalSize = 12;
            opPassCount.Dock = DockStyle.Fill;
            opPassCount.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opPassCount.ForeColor = Color.Black;
            opPassCount.Location = new Point(1, 30);
            opPassCount.Margin = new Padding(2);
            opPassCount.MinimumSize = new Size(1, 1);
            opPassCount.Name = "opPassCount";
            opPassCount.Size = new Size(202, 42);
            opPassCount.TabIndex = 0;
            opPassCount.Text = "-2";
            opPassCount.TextAlign = HorizontalAlignment.Center;
            opPassCount.Value = -2D;
            // 
            // uiTitlePanel5
            // 
            uiTitlePanel5.Controls.Add(opTotalCount);
            uiTitlePanel5.Dock = DockStyle.Fill;
            uiTitlePanel5.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel5.Location = new Point(1, 1);
            uiTitlePanel5.Margin = new Padding(1);
            uiTitlePanel5.MinimumSize = new Size(1, 1);
            uiTitlePanel5.Name = "uiTitlePanel5";
            uiTitlePanel5.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel5.RectColor = Color.Silver;
            uiTitlePanel5.ShowText = false;
            uiTitlePanel5.Size = new Size(204, 73);
            uiTitlePanel5.TabIndex = 0;
            uiTitlePanel5.Text = "Tổng số";
            uiTitlePanel5.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel5.TitleColor = Color.Blue;
            uiTitlePanel5.TitleHeight = 30;
            // 
            // opTotalCount
            // 
            opTotalCount.BackColor = Color.FromArgb(192, 255, 255);
            opTotalCount.DecimalPlaces = 0;
            opTotalCount.DigitalSize = 12;
            opTotalCount.Dock = DockStyle.Fill;
            opTotalCount.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opTotalCount.ForeColor = Color.Black;
            opTotalCount.Location = new Point(1, 30);
            opTotalCount.Margin = new Padding(2);
            opTotalCount.MinimumSize = new Size(1, 1);
            opTotalCount.Name = "opTotalCount";
            opTotalCount.Size = new Size(202, 42);
            opTotalCount.TabIndex = 0;
            opTotalCount.TextAlign = HorizontalAlignment.Center;
            opTotalCount.Value = -2D;
            // 
            // uiTitlePanel3
            // 
            uiTitlePanel3.Controls.Add(opNoteCameraView);
            uiTitlePanel3.Dock = DockStyle.Fill;
            uiTitlePanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel3.Location = new Point(2, 191);
            uiTitlePanel3.Margin = new Padding(2);
            uiTitlePanel3.MinimumSize = new Size(1, 1);
            uiTitlePanel3.Name = "uiTitlePanel3";
            uiTitlePanel3.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel3.ShowText = false;
            uiTitlePanel3.Size = new Size(1120, 750);
            uiTitlePanel3.TabIndex = 2;
            uiTitlePanel3.Text = "BẢNG THÔNG BÁO";
            uiTitlePanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opNoteCameraView
            // 
            opNoteCameraView.Controls.Add(tabPage1);
            opNoteCameraView.Controls.Add(tabPage2);
            opNoteCameraView.Dock = DockStyle.Fill;
            opNoteCameraView.DrawMode = TabDrawMode.OwnerDrawFixed;
            opNoteCameraView.Font = new Font("Microsoft Sans Serif", 12F);
            opNoteCameraView.ItemSize = new Size(150, 40);
            opNoteCameraView.Location = new Point(1, 35);
            opNoteCameraView.MainPage = "";
            opNoteCameraView.Name = "opNoteCameraView";
            opNoteCameraView.SelectedIndex = 0;
            opNoteCameraView.Size = new Size(1118, 714);
            opNoteCameraView.SizeMode = TabSizeMode.Fixed;
            opNoteCameraView.TabIndex = 0;
            opNoteCameraView.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            opNoteCameraView.TipsFont = new Font("Microsoft Sans Serif", 9F);
            opNoteCameraView.SelectedIndexChanged += opNoteCameraView_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(opView);
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(1118, 674);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Thông báo";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // opView
            // 
            opView.Dock = DockStyle.Fill;
            opView.Font = new Font("Microsoft Sans Serif", 12F);
            opView.HoverColor = Color.FromArgb(155, 200, 255);
            opView.ItemSelectForeColor = Color.White;
            opView.Location = new Point(0, 0);
            opView.Margin = new Padding(4, 5, 4, 5);
            opView.MinimumSize = new Size(1, 1);
            opView.Name = "opView";
            opView.Padding = new Padding(2);
            opView.ShowText = false;
            opView.Size = new Size(1118, 674);
            opView.TabIndex = 0;
            opView.Text = "uiListBox1";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(uiListBox1);
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(200, 60);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Kiểm tra lỗi";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // uiListBox1
            // 
            uiListBox1.Dock = DockStyle.Fill;
            uiListBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiListBox1.HoverColor = Color.FromArgb(155, 200, 255);
            uiListBox1.ItemSelectForeColor = Color.White;
            uiListBox1.Location = new Point(0, 0);
            uiListBox1.Margin = new Padding(4, 5, 4, 5);
            uiListBox1.MinimumSize = new Size(1, 1);
            uiListBox1.Name = "uiListBox1";
            uiListBox1.Padding = new Padding(2);
            uiListBox1.ShowText = false;
            uiListBox1.Size = new Size(200, 60);
            uiListBox1.TabIndex = 0;
            uiListBox1.Text = "uiListBox1";
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel14, 0, 2);
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel4, 0, 1);
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel1, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel12, 0, 3);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(1127, 192);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 4;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 29.2647057F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.0882339F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 37.64706F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 114F));
            uiTableLayoutPanel1.Size = new Size(618, 748);
            uiTableLayoutPanel1.TabIndex = 3;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTitlePanel14
            // 
            uiTitlePanel14.Controls.Add(uiTableLayoutPanel8);
            uiTitlePanel14.Dock = DockStyle.Fill;
            uiTitlePanel14.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel14.Location = new Point(2, 396);
            uiTitlePanel14.Margin = new Padding(2);
            uiTitlePanel14.MinimumSize = new Size(1, 1);
            uiTitlePanel14.Name = "uiTitlePanel14";
            uiTitlePanel14.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel14.ShowText = false;
            uiTitlePanel14.Size = new Size(614, 234);
            uiTitlePanel14.TabIndex = 7;
            uiTitlePanel14.Text = "THÔNG SỐ PLC";
            uiTitlePanel14.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel8
            // 
            uiTableLayoutPanel8.BackColor = Color.Azure;
            uiTableLayoutPanel8.ColumnCount = 3;
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205F));
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49.87715F));
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.12285F));
            uiTableLayoutPanel8.Controls.Add(ipRejectStreng, 1, 3);
            uiTableLayoutPanel8.Controls.Add(ipDelayRject, 1, 2);
            uiTableLayoutPanel8.Controls.Add(ipDelayTriger, 1, 1);
            uiTableLayoutPanel8.Controls.Add(uiPanel17, 0, 3);
            uiTableLayoutPanel8.Controls.Add(uiPanel16, 0, 2);
            uiTableLayoutPanel8.Controls.Add(uiPanel15, 0, 1);
            uiTableLayoutPanel8.Controls.Add(uiPanel11, 1, 0);
            uiTableLayoutPanel8.Controls.Add(uiPanel2, 0, 0);
            uiTableLayoutPanel8.Controls.Add(opRejectStreng, 2, 3);
            uiTableLayoutPanel8.Controls.Add(uiTableLayoutPanel16, 2, 0);
            uiTableLayoutPanel8.Controls.Add(opDelayRject, 2, 2);
            uiTableLayoutPanel8.Controls.Add(uiTableLayoutPanel17, 2, 1);
            uiTableLayoutPanel8.Dock = DockStyle.Fill;
            uiTableLayoutPanel8.Location = new Point(1, 35);
            uiTableLayoutPanel8.Margin = new Padding(2);
            uiTableLayoutPanel8.Name = "uiTableLayoutPanel8";
            uiTableLayoutPanel8.RowCount = 4;
            uiTableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel8.Size = new Size(612, 198);
            uiTableLayoutPanel8.TabIndex = 0;
            uiTableLayoutPanel8.TagString = null;
            uiTableLayoutPanel8.Paint += uiTableLayoutPanel8_Paint;
            // 
            // ipRejectStreng
            // 
            ipRejectStreng.Dock = DockStyle.Fill;
            ipRejectStreng.FillColor = Color.White;
            ipRejectStreng.Font = new Font("Microsoft Sans Serif", 12F);
            ipRejectStreng.Location = new Point(207, 149);
            ipRejectStreng.Margin = new Padding(2);
            ipRejectStreng.Minimum = 0D;
            ipRejectStreng.MinimumSize = new Size(63, 0);
            ipRejectStreng.Name = "ipRejectStreng";
            ipRejectStreng.NumPadType = Sunny.UI.NumPadType.Integer;
            ipRejectStreng.Padding = new Padding(0, 0, 30, 2);
            ipRejectStreng.Size = new Size(199, 47);
            ipRejectStreng.SymbolDropDown = 557532;
            ipRejectStreng.SymbolNormal = 557532;
            ipRejectStreng.SymbolSize = 30;
            ipRejectStreng.TabIndex = 19;
            ipRejectStreng.TextAlignment = ContentAlignment.MiddleLeft;
            ipRejectStreng.Watermark = "";
            // 
            // ipDelayRject
            // 
            ipDelayRject.Dock = DockStyle.Fill;
            ipDelayRject.FillColor = Color.White;
            ipDelayRject.Font = new Font("Microsoft Sans Serif", 12F);
            ipDelayRject.Location = new Point(207, 100);
            ipDelayRject.Margin = new Padding(2);
            ipDelayRject.Minimum = 0D;
            ipDelayRject.MinimumSize = new Size(63, 0);
            ipDelayRject.Name = "ipDelayRject";
            ipDelayRject.NumPadType = Sunny.UI.NumPadType.Integer;
            ipDelayRject.Padding = new Padding(0, 0, 30, 2);
            ipDelayRject.Size = new Size(199, 45);
            ipDelayRject.SymbolDropDown = 557532;
            ipDelayRject.SymbolNormal = 557532;
            ipDelayRject.SymbolSize = 30;
            ipDelayRject.TabIndex = 18;
            ipDelayRject.TextAlignment = ContentAlignment.MiddleLeft;
            ipDelayRject.Watermark = "";
            // 
            // ipDelayTriger
            // 
            ipDelayTriger.Dock = DockStyle.Fill;
            ipDelayTriger.FillColor = Color.White;
            ipDelayTriger.Font = new Font("Microsoft Sans Serif", 12F);
            ipDelayTriger.Location = new Point(207, 51);
            ipDelayTriger.Margin = new Padding(2);
            ipDelayTriger.Minimum = 0D;
            ipDelayTriger.MinimumSize = new Size(63, 0);
            ipDelayTriger.Name = "ipDelayTriger";
            ipDelayTriger.NumPadType = Sunny.UI.NumPadType.Integer;
            ipDelayTriger.Padding = new Padding(0, 0, 30, 2);
            ipDelayTriger.Size = new Size(199, 45);
            ipDelayTriger.SymbolDropDown = 557532;
            ipDelayTriger.SymbolNormal = 557532;
            ipDelayTriger.SymbolSize = 30;
            ipDelayTriger.TabIndex = 17;
            ipDelayTriger.TextAlignment = ContentAlignment.MiddleLeft;
            ipDelayTriger.Watermark = "";
            // 
            // uiPanel17
            // 
            uiPanel17.Dock = DockStyle.Fill;
            uiPanel17.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel17.Location = new Point(2, 149);
            uiPanel17.Margin = new Padding(2);
            uiPanel17.MinimumSize = new Size(1, 1);
            uiPanel17.Name = "uiPanel17";
            uiPanel17.Radius = 1;
            uiPanel17.Size = new Size(201, 47);
            uiPanel17.TabIndex = 16;
            uiPanel17.Text = "Độ Mạnh Bộ Loại";
            uiPanel17.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel16
            // 
            uiPanel16.Dock = DockStyle.Fill;
            uiPanel16.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel16.Location = new Point(2, 100);
            uiPanel16.Margin = new Padding(2);
            uiPanel16.MinimumSize = new Size(1, 1);
            uiPanel16.Name = "uiPanel16";
            uiPanel16.Size = new Size(201, 45);
            uiPanel16.TabIndex = 15;
            uiPanel16.Text = "Độ Trễ Loại";
            uiPanel16.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel15
            // 
            uiPanel15.Dock = DockStyle.Fill;
            uiPanel15.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel15.Location = new Point(2, 51);
            uiPanel15.Margin = new Padding(2);
            uiPanel15.MinimumSize = new Size(1, 1);
            uiPanel15.Name = "uiPanel15";
            uiPanel15.Size = new Size(201, 45);
            uiPanel15.TabIndex = 10;
            uiPanel15.Text = "Độ Trễ Chụp";
            uiPanel15.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel11
            // 
            uiPanel11.Controls.Add(uiPanel13);
            uiPanel11.Dock = DockStyle.Fill;
            uiPanel11.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel11.Location = new Point(207, 2);
            uiPanel11.Margin = new Padding(2);
            uiPanel11.MinimumSize = new Size(1, 1);
            uiPanel11.Name = "uiPanel11";
            uiPanel11.Size = new Size(199, 45);
            uiPanel11.TabIndex = 9;
            uiPanel11.Text = "Độ Trễ Chụp";
            uiPanel11.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel13
            // 
            uiPanel13.Controls.Add(uiPanel14);
            uiPanel13.Dock = DockStyle.Fill;
            uiPanel13.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel13.Location = new Point(0, 0);
            uiPanel13.Margin = new Padding(2);
            uiPanel13.MinimumSize = new Size(1, 1);
            uiPanel13.Name = "uiPanel13";
            uiPanel13.Size = new Size(199, 45);
            uiPanel13.TabIndex = 2;
            uiPanel13.Text = "Độ Trễ Chụp";
            uiPanel13.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel14
            // 
            uiPanel14.Dock = DockStyle.Fill;
            uiPanel14.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel14.Location = new Point(0, 0);
            uiPanel14.Margin = new Padding(2);
            uiPanel14.MinimumSize = new Size(1, 1);
            uiPanel14.Name = "uiPanel14";
            uiPanel14.Radius = 1;
            uiPanel14.RectColor = Color.Red;
            uiPanel14.Size = new Size(199, 45);
            uiPanel14.TabIndex = 2;
            uiPanel14.Text = "Thông số cài đặt";
            uiPanel14.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel2
            // 
            uiPanel2.Controls.Add(uiPanel6);
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel2.Location = new Point(2, 2);
            uiPanel2.Margin = new Padding(2);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.Size = new Size(201, 45);
            uiPanel2.TabIndex = 8;
            uiPanel2.Text = "Độ Trễ Chụp";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel6
            // 
            uiPanel6.Controls.Add(uiPanel7);
            uiPanel6.Dock = DockStyle.Fill;
            uiPanel6.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel6.Location = new Point(0, 0);
            uiPanel6.Margin = new Padding(2);
            uiPanel6.MinimumSize = new Size(1, 1);
            uiPanel6.Name = "uiPanel6";
            uiPanel6.Size = new Size(201, 45);
            uiPanel6.TabIndex = 1;
            uiPanel6.Text = "Độ Trễ Chụp";
            uiPanel6.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel7
            // 
            uiPanel7.Dock = DockStyle.Fill;
            uiPanel7.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel7.Location = new Point(0, 0);
            uiPanel7.Margin = new Padding(2);
            uiPanel7.MinimumSize = new Size(1, 1);
            uiPanel7.Name = "uiPanel7";
            uiPanel7.RectColor = Color.Red;
            uiPanel7.Size = new Size(201, 45);
            uiPanel7.TabIndex = 1;
            uiPanel7.Text = "Tên";
            uiPanel7.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opRejectStreng
            // 
            opRejectStreng.Dock = DockStyle.Fill;
            opRejectStreng.Font = new Font("Microsoft Sans Serif", 12F);
            opRejectStreng.Location = new Point(410, 149);
            opRejectStreng.Margin = new Padding(2);
            opRejectStreng.MinimumSize = new Size(1, 16);
            opRejectStreng.Name = "opRejectStreng";
            opRejectStreng.Padding = new Padding(5);
            opRejectStreng.ReadOnly = true;
            opRejectStreng.ShowText = false;
            opRejectStreng.Size = new Size(200, 47);
            opRejectStreng.TabIndex = 7;
            opRejectStreng.Text = "-";
            opRejectStreng.TextAlignment = ContentAlignment.MiddleLeft;
            opRejectStreng.Watermark = "";
            // 
            // uiTableLayoutPanel16
            // 
            uiTableLayoutPanel16.ColumnCount = 1;
            uiTableLayoutPanel16.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel16.Controls.Add(uiPanel18, 0, 0);
            uiTableLayoutPanel16.Dock = DockStyle.Fill;
            uiTableLayoutPanel16.Location = new Point(409, 0);
            uiTableLayoutPanel16.Margin = new Padding(1, 0, 0, 0);
            uiTableLayoutPanel16.Name = "uiTableLayoutPanel16";
            uiTableLayoutPanel16.RowCount = 1;
            uiTableLayoutPanel16.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel16.Size = new Size(203, 49);
            uiTableLayoutPanel16.TabIndex = 5;
            uiTableLayoutPanel16.TagString = null;
            // 
            // uiPanel18
            // 
            uiPanel18.Dock = DockStyle.Fill;
            uiPanel18.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel18.Location = new Point(2, 2);
            uiPanel18.Margin = new Padding(2);
            uiPanel18.MinimumSize = new Size(1, 1);
            uiPanel18.Name = "uiPanel18";
            uiPanel18.RectColor = Color.Red;
            uiPanel18.Size = new Size(199, 45);
            uiPanel18.TabIndex = 2;
            uiPanel18.Text = "Thông số dưới PLC";
            uiPanel18.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opDelayRject
            // 
            opDelayRject.Dock = DockStyle.Fill;
            opDelayRject.Font = new Font("Microsoft Sans Serif", 12F);
            opDelayRject.Location = new Point(410, 100);
            opDelayRject.Margin = new Padding(2);
            opDelayRject.MinimumSize = new Size(1, 16);
            opDelayRject.Name = "opDelayRject";
            opDelayRject.Padding = new Padding(5);
            opDelayRject.ReadOnly = true;
            opDelayRject.ShowText = false;
            opDelayRject.Size = new Size(200, 45);
            opDelayRject.TabIndex = 3;
            opDelayRject.Text = "-";
            opDelayRject.TextAlignment = ContentAlignment.MiddleLeft;
            opDelayRject.Watermark = "";
            // 
            // uiTableLayoutPanel17
            // 
            uiTableLayoutPanel17.ColumnCount = 1;
            uiTableLayoutPanel17.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel17.Controls.Add(opDelayTriger, 0, 0);
            uiTableLayoutPanel17.Dock = DockStyle.Fill;
            uiTableLayoutPanel17.Location = new Point(409, 49);
            uiTableLayoutPanel17.Margin = new Padding(1, 0, 0, 0);
            uiTableLayoutPanel17.Name = "uiTableLayoutPanel17";
            uiTableLayoutPanel17.RowCount = 1;
            uiTableLayoutPanel17.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel17.Size = new Size(203, 49);
            uiTableLayoutPanel17.TabIndex = 4;
            uiTableLayoutPanel17.TagString = null;
            // 
            // opDelayTriger
            // 
            opDelayTriger.Dock = DockStyle.Fill;
            opDelayTriger.Enabled = false;
            opDelayTriger.Font = new Font("Microsoft Sans Serif", 12F);
            opDelayTriger.Location = new Point(0, 0);
            opDelayTriger.Margin = new Padding(0);
            opDelayTriger.MinimumSize = new Size(1, 16);
            opDelayTriger.Name = "opDelayTriger";
            opDelayTriger.Padding = new Padding(5);
            opDelayTriger.ReadOnly = true;
            opDelayTriger.ShowText = false;
            opDelayTriger.Size = new Size(203, 49);
            opDelayTriger.TabIndex = 0;
            opDelayTriger.Text = "-";
            opDelayTriger.TextAlignment = ContentAlignment.MiddleLeft;
            opDelayTriger.Watermark = "";
            // 
            // uiTitlePanel4
            // 
            uiTitlePanel4.Controls.Add(uiTableLayoutPanel11);
            uiTitlePanel4.Dock = DockStyle.Fill;
            uiTitlePanel4.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel4.Location = new Point(2, 187);
            uiTitlePanel4.Margin = new Padding(2);
            uiTitlePanel4.MinimumSize = new Size(1, 1);
            uiTitlePanel4.Name = "uiTitlePanel4";
            uiTitlePanel4.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel4.ShowText = false;
            uiTitlePanel4.Size = new Size(614, 205);
            uiTitlePanel4.TabIndex = 4;
            uiTitlePanel4.Text = "THÔNG TIN SẢN XUẤT";
            uiTitlePanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel11
            // 
            uiTableLayoutPanel11.BackColor = Color.Azure;
            uiTableLayoutPanel11.ColumnCount = 2;
            uiTableLayoutPanel11.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32.69231F));
            uiTableLayoutPanel11.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67.30769F));
            uiTableLayoutPanel11.Controls.Add(opLineName, 1, 3);
            uiTableLayoutPanel11.Controls.Add(uiPanel12, 0, 3);
            uiTableLayoutPanel11.Controls.Add(uiTableLayoutPanel14, 1, 0);
            uiTableLayoutPanel11.Controls.Add(uiPanel8, 0, 0);
            uiTableLayoutPanel11.Controls.Add(uiPanel9, 0, 1);
            uiTableLayoutPanel11.Controls.Add(uiPanel10, 0, 2);
            uiTableLayoutPanel11.Controls.Add(opSCount, 1, 2);
            uiTableLayoutPanel11.Controls.Add(uiTableLayoutPanel13, 1, 1);
            uiTableLayoutPanel11.Dock = DockStyle.Fill;
            uiTableLayoutPanel11.Location = new Point(1, 35);
            uiTableLayoutPanel11.Margin = new Padding(2);
            uiTableLayoutPanel11.Name = "uiTableLayoutPanel11";
            uiTableLayoutPanel11.RowCount = 4;
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel11.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            uiTableLayoutPanel11.Size = new Size(612, 169);
            uiTableLayoutPanel11.TabIndex = 0;
            uiTableLayoutPanel11.TagString = null;
            // 
            // opLineName
            // 
            opLineName.Dock = DockStyle.Fill;
            opLineName.Font = new Font("Microsoft Sans Serif", 12F);
            opLineName.Location = new Point(202, 128);
            opLineName.Margin = new Padding(2);
            opLineName.MinimumSize = new Size(1, 16);
            opLineName.Name = "opLineName";
            opLineName.Padding = new Padding(5);
            opLineName.ReadOnly = true;
            opLineName.ShowText = false;
            opLineName.Size = new Size(408, 39);
            opLineName.TabIndex = 7;
            opLineName.Text = "Line 3";
            opLineName.TextAlignment = ContentAlignment.MiddleLeft;
            opLineName.Watermark = "";
            // 
            // uiPanel12
            // 
            uiPanel12.Dock = DockStyle.Fill;
            uiPanel12.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel12.Location = new Point(2, 128);
            uiPanel12.Margin = new Padding(2);
            uiPanel12.MinimumSize = new Size(1, 1);
            uiPanel12.Name = "uiPanel12";
            uiPanel12.Size = new Size(196, 39);
            uiPanel12.TabIndex = 6;
            uiPanel12.Text = "Tên chuyền";
            uiPanel12.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel14
            // 
            uiTableLayoutPanel14.ColumnCount = 1;
            uiTableLayoutPanel14.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel14.Controls.Add(ipBatchNo, 0, 0);
            uiTableLayoutPanel14.Dock = DockStyle.Fill;
            uiTableLayoutPanel14.Location = new Point(201, 0);
            uiTableLayoutPanel14.Margin = new Padding(1, 0, 0, 0);
            uiTableLayoutPanel14.Name = "uiTableLayoutPanel14";
            uiTableLayoutPanel14.RowCount = 1;
            uiTableLayoutPanel14.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel14.Size = new Size(411, 42);
            uiTableLayoutPanel14.TabIndex = 5;
            uiTableLayoutPanel14.TagString = null;
            // 
            // ipBatchNo
            // 
            ipBatchNo.DataSource = null;
            ipBatchNo.Dock = DockStyle.Fill;
            ipBatchNo.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            ipBatchNo.Enabled = false;
            ipBatchNo.FillColor = Color.White;
            ipBatchNo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ipBatchNo.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipBatchNo.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipBatchNo.Location = new Point(2, 2);
            ipBatchNo.Margin = new Padding(2);
            ipBatchNo.MinimumSize = new Size(63, 0);
            ipBatchNo.Name = "ipBatchNo";
            ipBatchNo.Padding = new Padding(0, 0, 30, 2);
            ipBatchNo.Size = new Size(407, 38);
            ipBatchNo.SymbolSize = 24;
            ipBatchNo.TabIndex = 3;
            ipBatchNo.Text = "03OT00363-111125-TOL1-2";
            ipBatchNo.TextAlignment = ContentAlignment.MiddleLeft;
            ipBatchNo.Watermark = "";
            ipBatchNo.SelectedIndexChanged += ipBatchNo_SelectedIndexChanged;
            // 
            // uiPanel8
            // 
            uiPanel8.Dock = DockStyle.Fill;
            uiPanel8.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel8.Location = new Point(2, 2);
            uiPanel8.Margin = new Padding(2);
            uiPanel8.MinimumSize = new Size(1, 1);
            uiPanel8.Name = "uiPanel8";
            uiPanel8.Size = new Size(196, 38);
            uiPanel8.TabIndex = 0;
            uiPanel8.Text = "BATCH NO";
            uiPanel8.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel9
            // 
            uiPanel9.Dock = DockStyle.Fill;
            uiPanel9.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel9.Location = new Point(2, 44);
            uiPanel9.Margin = new Padding(2);
            uiPanel9.MinimumSize = new Size(1, 1);
            uiPanel9.Name = "uiPanel9";
            uiPanel9.Size = new Size(196, 38);
            uiPanel9.TabIndex = 0;
            uiPanel9.Text = "BARCODE";
            uiPanel9.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel10
            // 
            uiPanel10.Dock = DockStyle.Fill;
            uiPanel10.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel10.Location = new Point(2, 86);
            uiPanel10.Margin = new Padding(2);
            uiPanel10.MinimumSize = new Size(1, 1);
            uiPanel10.Name = "uiPanel10";
            uiPanel10.Size = new Size(196, 38);
            uiPanel10.TabIndex = 0;
            uiPanel10.Text = "Sản lượng";
            uiPanel10.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opSCount
            // 
            opSCount.Dock = DockStyle.Fill;
            opSCount.Font = new Font("Microsoft Sans Serif", 12F);
            opSCount.Location = new Point(202, 86);
            opSCount.Margin = new Padding(2);
            opSCount.MinimumSize = new Size(1, 16);
            opSCount.Name = "opSCount";
            opSCount.Padding = new Padding(5);
            opSCount.ReadOnly = true;
            opSCount.ShowText = false;
            opSCount.Size = new Size(408, 38);
            opSCount.TabIndex = 3;
            opSCount.Text = "1234567 - 1243534 -1111";
            opSCount.TextAlignment = ContentAlignment.MiddleLeft;
            opSCount.Watermark = "";
            // 
            // uiTableLayoutPanel13
            // 
            uiTableLayoutPanel13.ColumnCount = 1;
            uiTableLayoutPanel13.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel13.Controls.Add(ipBarcode, 0, 0);
            uiTableLayoutPanel13.Dock = DockStyle.Fill;
            uiTableLayoutPanel13.Location = new Point(201, 42);
            uiTableLayoutPanel13.Margin = new Padding(1, 0, 0, 0);
            uiTableLayoutPanel13.Name = "uiTableLayoutPanel13";
            uiTableLayoutPanel13.RowCount = 1;
            uiTableLayoutPanel13.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel13.Size = new Size(411, 42);
            uiTableLayoutPanel13.TabIndex = 4;
            uiTableLayoutPanel13.TagString = null;
            // 
            // ipBarcode
            // 
            ipBarcode.Dock = DockStyle.Fill;
            ipBarcode.DoubleValue = 123456789D;
            ipBarcode.Enabled = false;
            ipBarcode.Font = new Font("Microsoft Sans Serif", 12F);
            ipBarcode.IntValue = 123456789;
            ipBarcode.Location = new Point(0, 0);
            ipBarcode.Margin = new Padding(0);
            ipBarcode.MinimumSize = new Size(1, 16);
            ipBarcode.Name = "ipBarcode";
            ipBarcode.Padding = new Padding(5);
            ipBarcode.ReadOnly = true;
            ipBarcode.ShowText = false;
            ipBarcode.Size = new Size(411, 42);
            ipBarcode.TabIndex = 0;
            ipBarcode.Text = "0123456789";
            ipBarcode.TextAlignment = ContentAlignment.MiddleLeft;
            ipBarcode.Watermark = "";
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel4);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 2);
            uiTitlePanel1.Margin = new Padding(2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(614, 181);
            uiTitlePanel1.TabIndex = 2;
            uiTitlePanel1.Text = "THÔNG TIN  THIẾT BỊ";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.ColumnCount = 3;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel13, 2, 1);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel12, 1, 1);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel11, 0, 1);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel10, 2, 0);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel9, 1, 0);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel8, 0, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(1, 35);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 2;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.Size = new Size(612, 145);
            uiTableLayoutPanel4.TabIndex = 0;
            uiTableLayoutPanel4.TagString = null;
            // 
            // uiTitlePanel13
            // 
            uiTitlePanel13.Controls.Add(uiTableLayoutPanel10);
            uiTitlePanel13.Dock = DockStyle.Fill;
            uiTitlePanel13.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel13.Location = new Point(409, 73);
            uiTitlePanel13.Margin = new Padding(1);
            uiTitlePanel13.MinimumSize = new Size(1, 1);
            uiTitlePanel13.Name = "uiTitlePanel13";
            uiTitlePanel13.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel13.RectColor = Color.Silver;
            uiTitlePanel13.ShowText = false;
            uiTitlePanel13.Size = new Size(202, 71);
            uiTitlePanel13.TabIndex = 6;
            uiTitlePanel13.Text = "HỆ THỐNG";
            uiTitlePanel13.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel13.TitleColor = Color.Teal;
            uiTitlePanel13.TitleHeight = 30;
            // 
            // uiTableLayoutPanel10
            // 
            uiTableLayoutPanel10.ColumnCount = 2;
            uiTableLayoutPanel10.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.962616F));
            uiTableLayoutPanel10.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.037384F));
            uiTableLayoutPanel10.Controls.Add(opAppStatus, 0, 0);
            uiTableLayoutPanel10.Controls.Add(opAppStatusCode, 1, 0);
            uiTableLayoutPanel10.Dock = DockStyle.Fill;
            uiTableLayoutPanel10.Location = new Point(1, 30);
            uiTableLayoutPanel10.Margin = new Padding(0);
            uiTableLayoutPanel10.Name = "uiTableLayoutPanel10";
            uiTableLayoutPanel10.RowCount = 1;
            uiTableLayoutPanel10.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel10.Size = new Size(200, 40);
            uiTableLayoutPanel10.TabIndex = 0;
            uiTableLayoutPanel10.TagString = null;
            // 
            // opAppStatus
            // 
            opAppStatus.Dock = DockStyle.Fill;
            opAppStatus.Font = new Font("Microsoft Sans Serif", 12F);
            opAppStatus.Location = new Point(2, 2);
            opAppStatus.Margin = new Padding(2);
            opAppStatus.MinimumSize = new Size(1, 1);
            opAppStatus.Name = "opAppStatus";
            opAppStatus.RectColor = Color.FromArgb(255, 128, 0);
            opAppStatus.RectSize = 2;
            opAppStatus.Size = new Size(139, 36);
            opAppStatus.TabIndex = 0;
            opAppStatus.Text = "Tạm dừng";
            opAppStatus.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opAppStatusCode
            // 
            opAppStatusCode.BackColor = Color.Transparent;
            opAppStatusCode.DecimalPlaces = 0;
            opAppStatusCode.DigitalSize = 18;
            opAppStatusCode.Dock = DockStyle.Fill;
            opAppStatusCode.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opAppStatusCode.ForeColor = Color.Blue;
            opAppStatusCode.Location = new Point(144, 1);
            opAppStatusCode.Margin = new Padding(1);
            opAppStatusCode.MinimumSize = new Size(1, 1);
            opAppStatusCode.Name = "opAppStatusCode";
            opAppStatusCode.Size = new Size(55, 38);
            opAppStatusCode.TabIndex = 1;
            opAppStatusCode.Value = 1D;
            // 
            // uiTitlePanel12
            // 
            uiTitlePanel12.Controls.Add(uiTableLayoutPanel9);
            uiTitlePanel12.Dock = DockStyle.Fill;
            uiTitlePanel12.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel12.Location = new Point(205, 73);
            uiTitlePanel12.Margin = new Padding(1);
            uiTitlePanel12.MinimumSize = new Size(1, 1);
            uiTitlePanel12.Name = "uiTitlePanel12";
            uiTitlePanel12.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel12.RectColor = Color.Silver;
            uiTitlePanel12.ShowText = false;
            uiTitlePanel12.Size = new Size(202, 71);
            uiTitlePanel12.TabIndex = 5;
            uiTitlePanel12.Text = "MÁY CHỦ";
            uiTitlePanel12.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel12.TitleColor = Color.Teal;
            uiTitlePanel12.TitleHeight = 30;
            // 
            // uiTableLayoutPanel9
            // 
            uiTableLayoutPanel9.ColumnCount = 2;
            uiTableLayoutPanel9.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.962616F));
            uiTableLayoutPanel9.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.037384F));
            uiTableLayoutPanel9.Controls.Add(opServerStatus, 0, 0);
            uiTableLayoutPanel9.Controls.Add(opServerLed, 1, 0);
            uiTableLayoutPanel9.Dock = DockStyle.Fill;
            uiTableLayoutPanel9.Location = new Point(1, 30);
            uiTableLayoutPanel9.Margin = new Padding(0);
            uiTableLayoutPanel9.Name = "uiTableLayoutPanel9";
            uiTableLayoutPanel9.RowCount = 1;
            uiTableLayoutPanel9.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel9.Size = new Size(200, 40);
            uiTableLayoutPanel9.TabIndex = 0;
            uiTableLayoutPanel9.TagString = null;
            // 
            // opServerStatus
            // 
            opServerStatus.Dock = DockStyle.Fill;
            opServerStatus.Font = new Font("Microsoft Sans Serif", 12F);
            opServerStatus.Location = new Point(2, 2);
            opServerStatus.Margin = new Padding(2);
            opServerStatus.MinimumSize = new Size(1, 1);
            opServerStatus.Name = "opServerStatus";
            opServerStatus.RectColor = Color.FromArgb(0, 192, 0);
            opServerStatus.RectSize = 2;
            opServerStatus.Size = new Size(139, 36);
            opServerStatus.TabIndex = 0;
            opServerStatus.Text = "Kết nối";
            opServerStatus.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opServerLed
            // 
            opServerLed.Dock = DockStyle.Fill;
            opServerLed.Location = new Point(146, 3);
            opServerLed.Name = "opServerLed";
            opServerLed.Size = new Size(51, 34);
            opServerLed.TabIndex = 1;
            opServerLed.Text = "uiLedBulb5";
            // 
            // uiTitlePanel11
            // 
            uiTitlePanel11.Controls.Add(networkStrength1);
            uiTitlePanel11.Dock = DockStyle.Fill;
            uiTitlePanel11.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel11.Location = new Point(1, 73);
            uiTitlePanel11.Margin = new Padding(1);
            uiTitlePanel11.MinimumSize = new Size(1, 1);
            uiTitlePanel11.Name = "uiTitlePanel11";
            uiTitlePanel11.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel11.RectColor = Color.Silver;
            uiTitlePanel11.ShowText = false;
            uiTitlePanel11.Size = new Size(202, 71);
            uiTitlePanel11.TabIndex = 4;
            uiTitlePanel11.Text = "INTERNET";
            uiTitlePanel11.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel11.TitleColor = Color.Teal;
            uiTitlePanel11.TitleHeight = 30;
            // 
            // networkStrength1
            // 
            networkStrength1.BackColor = Color.Transparent;
            networkStrength1.Dock = DockStyle.Fill;
            networkStrength1.Location = new Point(1, 30);
            networkStrength1.Name = "networkStrength1";
            networkStrength1.Size = new Size(200, 40);
            networkStrength1.TabIndex = 0;
            // 
            // uiTitlePanel10
            // 
            uiTitlePanel10.Controls.Add(uiTableLayoutPanel7);
            uiTitlePanel10.Dock = DockStyle.Fill;
            uiTitlePanel10.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel10.Location = new Point(409, 1);
            uiTitlePanel10.Margin = new Padding(1);
            uiTitlePanel10.MinimumSize = new Size(1, 1);
            uiTitlePanel10.Name = "uiTitlePanel10";
            uiTitlePanel10.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel10.RectColor = Color.Silver;
            uiTitlePanel10.ShowText = false;
            uiTitlePanel10.Size = new Size(202, 70);
            uiTitlePanel10.TabIndex = 3;
            uiTitlePanel10.Text = "MÁY IN";
            uiTitlePanel10.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel10.TitleColor = Color.Teal;
            uiTitlePanel10.TitleHeight = 30;
            // 
            // uiTableLayoutPanel7
            // 
            uiTableLayoutPanel7.ColumnCount = 2;
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.962616F));
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.037384F));
            uiTableLayoutPanel7.Controls.Add(uiPanel4, 0, 0);
            uiTableLayoutPanel7.Controls.Add(uiLedBulb3, 1, 0);
            uiTableLayoutPanel7.Dock = DockStyle.Fill;
            uiTableLayoutPanel7.Location = new Point(1, 30);
            uiTableLayoutPanel7.Margin = new Padding(0);
            uiTableLayoutPanel7.Name = "uiTableLayoutPanel7";
            uiTableLayoutPanel7.RowCount = 1;
            uiTableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel7.Size = new Size(200, 39);
            uiTableLayoutPanel7.TabIndex = 0;
            uiTableLayoutPanel7.TagString = null;
            // 
            // uiPanel4
            // 
            uiPanel4.Dock = DockStyle.Fill;
            uiPanel4.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel4.Location = new Point(2, 2);
            uiPanel4.Margin = new Padding(2);
            uiPanel4.MinimumSize = new Size(1, 1);
            uiPanel4.Name = "uiPanel4";
            uiPanel4.RectColor = Color.Silver;
            uiPanel4.RectSize = 2;
            uiPanel4.Size = new Size(139, 35);
            uiPanel4.TabIndex = 0;
            uiPanel4.Text = "K.Dùng";
            uiPanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiLedBulb3
            // 
            uiLedBulb3.Color = Color.FromArgb(224, 224, 224);
            uiLedBulb3.Dock = DockStyle.Fill;
            uiLedBulb3.Location = new Point(146, 3);
            uiLedBulb3.Name = "uiLedBulb3";
            uiLedBulb3.Size = new Size(51, 33);
            uiLedBulb3.TabIndex = 1;
            uiLedBulb3.Text = "uiLedBulb3";
            // 
            // uiTitlePanel9
            // 
            uiTitlePanel9.Controls.Add(uiTableLayoutPanel6);
            uiTitlePanel9.Dock = DockStyle.Fill;
            uiTitlePanel9.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel9.Location = new Point(205, 1);
            uiTitlePanel9.Margin = new Padding(1);
            uiTitlePanel9.MinimumSize = new Size(1, 1);
            uiTitlePanel9.Name = "uiTitlePanel9";
            uiTitlePanel9.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel9.RectColor = Color.Silver;
            uiTitlePanel9.ShowText = false;
            uiTitlePanel9.Size = new Size(202, 70);
            uiTitlePanel9.TabIndex = 2;
            uiTitlePanel9.Text = "PLC";
            uiTitlePanel9.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel9.TitleColor = Color.Teal;
            uiTitlePanel9.TitleHeight = 30;
            // 
            // uiTableLayoutPanel6
            // 
            uiTableLayoutPanel6.ColumnCount = 2;
            uiTableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.962616F));
            uiTableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.037384F));
            uiTableLayoutPanel6.Controls.Add(opPLCStatus, 0, 0);
            uiTableLayoutPanel6.Controls.Add(opPLCLed, 1, 0);
            uiTableLayoutPanel6.Dock = DockStyle.Fill;
            uiTableLayoutPanel6.Location = new Point(1, 30);
            uiTableLayoutPanel6.Margin = new Padding(0);
            uiTableLayoutPanel6.Name = "uiTableLayoutPanel6";
            uiTableLayoutPanel6.RowCount = 1;
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel6.Size = new Size(200, 39);
            uiTableLayoutPanel6.TabIndex = 0;
            uiTableLayoutPanel6.TagString = null;
            // 
            // opPLCStatus
            // 
            opPLCStatus.Dock = DockStyle.Fill;
            opPLCStatus.Font = new Font("Microsoft Sans Serif", 12F);
            opPLCStatus.ForeColor = Color.Red;
            opPLCStatus.Location = new Point(2, 2);
            opPLCStatus.Margin = new Padding(2);
            opPLCStatus.MinimumSize = new Size(1, 1);
            opPLCStatus.Name = "opPLCStatus";
            opPLCStatus.RectColor = Color.Red;
            opPLCStatus.RectSize = 2;
            opPLCStatus.Size = new Size(139, 35);
            opPLCStatus.TabIndex = 0;
            opPLCStatus.Text = "Lỗi K01";
            opPLCStatus.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opPLCLed
            // 
            opPLCLed.Blink = true;
            opPLCLed.Color = Color.Red;
            opPLCLed.Dock = DockStyle.Fill;
            opPLCLed.ForeColor = Color.Red;
            opPLCLed.Location = new Point(146, 3);
            opPLCLed.Name = "opPLCLed";
            opPLCLed.Size = new Size(51, 33);
            opPLCLed.TabIndex = 1;
            opPLCLed.Text = "uiLedBulb2";
            // 
            // uiTitlePanel8
            // 
            uiTitlePanel8.Controls.Add(uiTableLayoutPanel5);
            uiTitlePanel8.Dock = DockStyle.Fill;
            uiTitlePanel8.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel8.Location = new Point(1, 1);
            uiTitlePanel8.Margin = new Padding(1);
            uiTitlePanel8.MinimumSize = new Size(1, 1);
            uiTitlePanel8.Name = "uiTitlePanel8";
            uiTitlePanel8.Padding = new Padding(1, 30, 1, 1);
            uiTitlePanel8.RectColor = Color.Silver;
            uiTitlePanel8.ShowText = false;
            uiTitlePanel8.Size = new Size(202, 70);
            uiTitlePanel8.TabIndex = 1;
            uiTitlePanel8.Text = "CAMERA";
            uiTitlePanel8.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel8.TitleColor = Color.Teal;
            uiTitlePanel8.TitleHeight = 30;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.962616F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.037384F));
            uiTableLayoutPanel5.Controls.Add(opCameraStatus, 0, 0);
            uiTableLayoutPanel5.Controls.Add(opCameraLed, 1, 0);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(1, 30);
            uiTableLayoutPanel5.Margin = new Padding(0);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 1;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.Size = new Size(200, 39);
            uiTableLayoutPanel5.TabIndex = 0;
            uiTableLayoutPanel5.TagString = null;
            // 
            // opCameraStatus
            // 
            opCameraStatus.Dock = DockStyle.Fill;
            opCameraStatus.Font = new Font("Microsoft Sans Serif", 12F);
            opCameraStatus.Location = new Point(2, 2);
            opCameraStatus.Margin = new Padding(2);
            opCameraStatus.MinimumSize = new Size(1, 1);
            opCameraStatus.Name = "opCameraStatus";
            opCameraStatus.RectColor = Color.FromArgb(0, 192, 0);
            opCameraStatus.RectSize = 2;
            opCameraStatus.Size = new Size(139, 35);
            opCameraStatus.TabIndex = 0;
            opCameraStatus.Text = "Kết nối";
            opCameraStatus.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opCameraLed
            // 
            opCameraLed.Dock = DockStyle.Fill;
            opCameraLed.Location = new Point(146, 3);
            opCameraLed.Name = "opCameraLed";
            opCameraLed.Size = new Size(51, 33);
            opCameraLed.TabIndex = 1;
            opCameraLed.Text = "uiLedBulb1";
            // 
            // uiTableLayoutPanel12
            // 
            uiTableLayoutPanel12.ColumnCount = 3;
            uiTableLayoutPanel12.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel12.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel12.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            uiTableLayoutPanel12.Controls.Add(btnChangeBatch, 0, 0);
            uiTableLayoutPanel12.Controls.Add(btnScan, 1, 0);
            uiTableLayoutPanel12.Controls.Add(uiSymbolButton3, 2, 0);
            uiTableLayoutPanel12.Controls.Add(btnPLCSetting, 2, 1);
            uiTableLayoutPanel12.Controls.Add(btnClearPLC, 1, 1);
            uiTableLayoutPanel12.Controls.Add(btnResetCounterPLC, 0, 1);
            uiTableLayoutPanel12.Dock = DockStyle.Fill;
            uiTableLayoutPanel12.Location = new Point(2, 634);
            uiTableLayoutPanel12.Margin = new Padding(2);
            uiTableLayoutPanel12.Name = "uiTableLayoutPanel12";
            uiTableLayoutPanel12.RowCount = 2;
            uiTableLayoutPanel12.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel12.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel12.Size = new Size(614, 112);
            uiTableLayoutPanel12.TabIndex = 6;
            uiTableLayoutPanel12.TagString = null;
            // 
            // btnChangeBatch
            // 
            btnChangeBatch.Dock = DockStyle.Fill;
            btnChangeBatch.FillColor = Color.Teal;
            btnChangeBatch.Font = new Font("Microsoft Sans Serif", 12F);
            btnChangeBatch.Location = new Point(2, 2);
            btnChangeBatch.Margin = new Padding(2);
            btnChangeBatch.MinimumSize = new Size(1, 1);
            btnChangeBatch.Name = "btnChangeBatch";
            btnChangeBatch.Size = new Size(200, 52);
            btnChangeBatch.Symbol = 559202;
            btnChangeBatch.TabIndex = 0;
            btnChangeBatch.Text = "Đổi lô";
            btnChangeBatch.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // btnScan
            // 
            btnScan.Dock = DockStyle.Fill;
            btnScan.Font = new Font("Microsoft Sans Serif", 12F);
            btnScan.Location = new Point(206, 2);
            btnScan.Margin = new Padding(2);
            btnScan.MinimumSize = new Size(1, 1);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(200, 52);
            btnScan.Symbol = 563580;
            btnScan.TabIndex = 0;
            btnScan.Text = "Quét mã";
            btnScan.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // uiSymbolButton3
            // 
            uiSymbolButton3.Dock = DockStyle.Fill;
            uiSymbolButton3.FillColor = Color.Green;
            uiSymbolButton3.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton3.Location = new Point(410, 2);
            uiSymbolButton3.Margin = new Padding(2);
            uiSymbolButton3.MinimumSize = new Size(1, 1);
            uiSymbolButton3.Name = "uiSymbolButton3";
            uiSymbolButton3.Size = new Size(202, 52);
            uiSymbolButton3.Symbol = 557670;
            uiSymbolButton3.TabIndex = 0;
            uiSymbolButton3.Text = "Thêm mã";
            uiSymbolButton3.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // btnPLCSetting
            // 
            btnPLCSetting.Dock = DockStyle.Fill;
            btnPLCSetting.FillColor = Color.FromArgb(255, 128, 255);
            btnPLCSetting.Font = new Font("Microsoft Sans Serif", 12F);
            btnPLCSetting.Location = new Point(410, 58);
            btnPLCSetting.Margin = new Padding(2);
            btnPLCSetting.MinimumSize = new Size(1, 1);
            btnPLCSetting.Name = "btnPLCSetting";
            btnPLCSetting.Size = new Size(202, 52);
            btnPLCSetting.Symbol = 61573;
            btnPLCSetting.TabIndex = 0;
            btnPLCSetting.Text = "Cài PLC";
            btnPLCSetting.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // btnClearPLC
            // 
            btnClearPLC.Dock = DockStyle.Fill;
            btnClearPLC.FillColor = Color.FromArgb(192, 192, 0);
            btnClearPLC.Font = new Font("Microsoft Sans Serif", 12F);
            btnClearPLC.Location = new Point(206, 58);
            btnClearPLC.Margin = new Padding(2);
            btnClearPLC.MinimumSize = new Size(1, 1);
            btnClearPLC.Name = "btnClearPLC";
            btnClearPLC.Size = new Size(200, 52);
            btnClearPLC.Symbol = 561647;
            btnClearPLC.TabIndex = 0;
            btnClearPLC.Text = "Xóa lỗi";
            btnClearPLC.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // btnResetCounterPLC
            // 
            btnResetCounterPLC.Dock = DockStyle.Fill;
            btnResetCounterPLC.FillColor = Color.FromArgb(255, 128, 0);
            btnResetCounterPLC.Font = new Font("Microsoft Sans Serif", 12F);
            btnResetCounterPLC.Location = new Point(2, 58);
            btnResetCounterPLC.Margin = new Padding(2);
            btnResetCounterPLC.MinimumSize = new Size(1, 1);
            btnResetCounterPLC.Name = "btnResetCounterPLC";
            btnResetCounterPLC.Size = new Size(200, 52);
            btnResetCounterPLC.Symbol = 62067;
            btnResetCounterPLC.TabIndex = 0;
            btnResetCounterPLC.Text = "Xóa đếm";
            btnResetCounterPLC.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // opAlarm
            // 
            opAlarm.Dock = DockStyle.Fill;
            opAlarm.Font = new Font("Microsoft Sans Serif", 12F);
            opAlarm.Location = new Point(2, 117);
            opAlarm.Margin = new Padding(2);
            opAlarm.MinimumSize = new Size(1, 1);
            opAlarm.Name = "opAlarm";
            opAlarm.RectSize = 2;
            opAlarm.Size = new Size(1120, 70);
            opAlarm.TabIndex = 6;
            opAlarm.Text = "-";
            opAlarm.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // omronPLC_Hsl1
            // 
            omronPLC_Hsl1.PLC_IP = "127.0.0.1";
            omronPLC_Hsl1.PLC_PORT = 9600;
            omronPLC_Hsl1.PLC_Ready_DM = "D16";
            omronPLC_Hsl1.PLC_STATUS = TTManager.PLCHelpers.OmronPLC_Hsl.PLCStatus.Disconnect;
            omronPLC_Hsl1.Ready = 0;
            omronPLC_Hsl1.Time_Update = 300;
            omronPLC_Hsl1.PLCStatus_OnChange += omronPLC_Hsl1_PLCStatus_OnChange;
            // 
            // WK_Camera
            // 
            WK_Camera.WorkerSupportsCancellation = true;
            WK_Camera.DoWork += WK_Camera_DoWork;
            // 
            // WK_Render_HMI
            // 
            WK_Render_HMI.WorkerSupportsCancellation = true;
            WK_Render_HMI.DoWork += WK_Render_HMI_DoWork;
            // 
            // erP_Google2
            // 
            erP_Google2.credentialPath = "D:\\Masan\\sales-268504-20a4b06ea0fb.json";
            erP_Google2.DatasetID = "FactoryIntegration";
            erP_Google2.LineName = "Line 3";
            erP_Google2.ORG_CODE = "MIP";
            erP_Google2.ProjectID = "sales-268504";
            erP_Google2.SUB_INV = "W05";
            erP_Google2.TableID = "BatchProduction";
            // 
            // WK_Dequeue
            // 
            WK_Dequeue.WorkerSupportsCancellation = true;
            WK_Dequeue.DoWork += WK_Dequeue_DoWork;
            // 
            // WK_Load_Counter
            // 
            WK_Load_Counter.WorkerSupportsCancellation = true;
            WK_Load_Counter.DoWork += WK_Load_PLC_DoWork;
            // 
            // FDashboard
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1748, 943);
            Controls.Add(MainPanel);
            Name = "FDashboard";
            Symbol = 559803;
            Text = "Bảng chính";
            MainPanel.ResumeLayout(false);
            uiTableLayoutPanel15.ResumeLayout(false);
            uiTitlePanel17.ResumeLayout(false);
            uiTitlePanel18.ResumeLayout(false);
            uiTitlePanel19.ResumeLayout(false);
            pnResult.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiTitlePanel7.ResumeLayout(false);
            uiTitlePanel6.ResumeLayout(false);
            uiTitlePanel5.ResumeLayout(false);
            uiTitlePanel3.ResumeLayout(false);
            opNoteCameraView.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel14.ResumeLayout(false);
            uiTableLayoutPanel8.ResumeLayout(false);
            uiPanel11.ResumeLayout(false);
            uiPanel13.ResumeLayout(false);
            uiPanel2.ResumeLayout(false);
            uiPanel6.ResumeLayout(false);
            uiTableLayoutPanel16.ResumeLayout(false);
            uiTableLayoutPanel17.ResumeLayout(false);
            uiTitlePanel4.ResumeLayout(false);
            uiTableLayoutPanel11.ResumeLayout(false);
            uiTableLayoutPanel14.ResumeLayout(false);
            uiTableLayoutPanel13.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            uiTitlePanel13.ResumeLayout(false);
            uiTableLayoutPanel10.ResumeLayout(false);
            uiTitlePanel12.ResumeLayout(false);
            uiTableLayoutPanel9.ResumeLayout(false);
            uiTitlePanel11.ResumeLayout(false);
            uiTitlePanel10.ResumeLayout(false);
            uiTableLayoutPanel7.ResumeLayout(false);
            uiTitlePanel9.ResumeLayout(false);
            uiTableLayoutPanel6.ResumeLayout(false);
            uiTitlePanel8.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            uiTableLayoutPanel12.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITableLayoutPanel MainPanel;
        private Sunny.UI.UITitlePanel pnResult;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Sunny.UI.UITitlePanel uiTitlePanel3;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UIRichTextBox opResopse;
        private Sunny.UI.UIPanel opResultStatus;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UITitlePanel uiTitlePanel5;
        private Sunny.UI.UIDigitalLabel opTotalCount;
        private Sunny.UI.UITitlePanel uiTitlePanel7;
        private Sunny.UI.UIDigitalLabel opFail;
        private Sunny.UI.UITitlePanel uiTitlePanel6;
        private Sunny.UI.UIDigitalLabel opPassCount;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UITitlePanel uiTitlePanel8;
        private Sunny.UI.UITitlePanel uiTitlePanel13;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel10;
        private Sunny.UI.UIPanel opAppStatus;
        private Sunny.UI.UITitlePanel uiTitlePanel12;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel9;
        private Sunny.UI.UIPanel opServerStatus;
        private Sunny.UI.UILedBulb opServerLed;
        private Sunny.UI.UITitlePanel uiTitlePanel11;
        private Sunny.UI.UITitlePanel uiTitlePanel10;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel7;
        private Sunny.UI.UIPanel uiPanel4;
        private Sunny.UI.UILedBulb uiLedBulb3;
        private Sunny.UI.UITitlePanel uiTitlePanel9;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel6;
        private Sunny.UI.UIPanel opPLCStatus;
        private Sunny.UI.UILedBulb opPLCLed;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UIPanel opCameraStatus;
        private Sunny.UI.UILedBulb opCameraLed;
        private Sunny.UI.UIDigitalLabel opAppStatusCode;
        private Sunny.UI.UITitlePanel uiTitlePanel4;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel11;
        private Sunny.UI.UIPanel uiPanel8;
        private Sunny.UI.UIPanel uiPanel9;
        private Sunny.UI.UIPanel uiPanel10;
        private Sunny.UI.UITabControl opNoteCameraView;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Sunny.UI.UITextBox opSCount;
        private Sunny.UI.UITextBox uiTextBox2;
        private Sunny.UI.UIListBox opView;
        private TTManager.PLCHelpers.OmronPLC_Hsl omronPLC_Hsl1;
        private System.ComponentModel.BackgroundWorker WK_Camera;
        private System.ComponentModel.BackgroundWorker WK_Render_HMI;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel13;
        private Sunny.UI.UITextBox ipBarcode;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel14;
        private Sunny.UI.UIComboBox ipBatchNo;
        private TTManager.Masan.ERP_Google erP_Google2;
        private TTManager.Internet.NetworkStrength networkStrength1;
        private System.ComponentModel.BackgroundWorker WK_Dequeue;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel15;
        private Sunny.UI.UITitlePanel uiTitlePanel17;
        private Sunny.UI.UIDigitalLabel opProductionSpeed;
        private Sunny.UI.UITitlePanel uiTitlePanel18;
        private Sunny.UI.UIDigitalLabel opTimeout;
        private Sunny.UI.UITitlePanel uiTitlePanel19;
        private Sunny.UI.UIDigitalLabel opBatchCount;
        private Sunny.UI.UIPanel opAlarm;
        private System.ComponentModel.BackgroundWorker WK_Load_Counter;
        private Sunny.UI.UIListBox uiListBox1;
        private Sunny.UI.UITextBox opLineName;
        private Sunny.UI.UIPanel uiPanel12;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel12;
        private Sunny.UI.UISymbolButton btnChangeBatch;
        private Sunny.UI.UISymbolButton btnScan;
        private Sunny.UI.UISymbolButton uiSymbolButton3;
        private Sunny.UI.UISymbolButton btnPLCSetting;
        private Sunny.UI.UISymbolButton btnClearPLC;
        private Sunny.UI.UISymbolButton btnResetCounterPLC;
        private Sunny.UI.UITitlePanel uiTitlePanel14;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel8;
        private Sunny.UI.UINumPadTextBox ipRejectStreng;
        private Sunny.UI.UINumPadTextBox ipDelayRject;
        private Sunny.UI.UINumPadTextBox ipDelayTriger;
        private Sunny.UI.UIPanel uiPanel17;
        private Sunny.UI.UIPanel uiPanel16;
        private Sunny.UI.UIPanel uiPanel15;
        private Sunny.UI.UIPanel uiPanel11;
        private Sunny.UI.UIPanel uiPanel13;
        private Sunny.UI.UIPanel uiPanel14;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel6;
        private Sunny.UI.UIPanel uiPanel7;
        private Sunny.UI.UITextBox opRejectStreng;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel16;
        private Sunny.UI.UIPanel uiPanel18;
        private Sunny.UI.UITextBox opDelayRject;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel17;
        private Sunny.UI.UITextBox opDelayTriger;
    }
}