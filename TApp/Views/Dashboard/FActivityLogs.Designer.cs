namespace TApp.Views.Dashboard
{
    partial class FActivityLogs
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            WK_AutoLog = new System.ComponentModel.BackgroundWorker();
            WK_Getlogs = new System.ComponentModel.BackgroundWorker();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            opDataG = new Sunny.UI.UIDataGridView();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            btnGetAll = new Sunny.UI.UISymbolButton();
            ipDateFrom = new Sunny.UI.UIDatePicker();
            btnExportCsv = new Sunny.UI.UISymbolButton();
            btnExportPDF = new Sunny.UI.UISymbolButton();
            btnGetLogs = new Sunny.UI.UISymbolButton();
            ipLogType = new Sunny.UI.UIComboBox();
            ipDateTo = new Sunny.UI.UIDatePicker();
            btnRefresh = new Sunny.UI.UISymbolButton();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            ipSize = new Sunny.UI.UIComboBox();
            uiPagination1 = new Sunny.UI.UIPagination();
            opTotalCount = new Sunny.UI.UIPanel();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opDataG).BeginInit();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // WK_AutoLog
            // 
            WK_AutoLog.WorkerReportsProgress = true;
            WK_AutoLog.WorkerSupportsCancellation = true;
            WK_AutoLog.DoWork += WK_AutoLog_DoWork;
            // 
            // WK_Getlogs
            // 
            WK_Getlogs.WorkerReportsProgress = true;
            WK_Getlogs.WorkerSupportsCancellation = true;
            WK_Getlogs.DoWork += WK_Getlogs_DoWork;
            WK_Getlogs.RunWorkerCompleted += WK_Getlogs_RunWorkerCompleted;
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel1);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 20F);
            uiTitlePanel1.Location = new Point(0, 0);
            uiTitlePanel1.Margin = new Padding(4, 5, 4, 5);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 50, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(840, 674);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "NHẬT KÝ HOẠT ĐỘNG NGƯỜI DÙNG";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel1.TitleHeight = 50;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(opDataG, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(1, 50);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 80.41734F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 19.58266F));
            uiTableLayoutPanel1.Size = new Size(838, 623);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // opDataG
            // 
            opDataG.AllowUserToAddRows = false;
            opDataG.AllowUserToDeleteRows = false;
            opDataG.AllowUserToOrderColumns = true;
            opDataG.AllowUserToResizeColumns = false;
            opDataG.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            opDataG.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            opDataG.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            opDataG.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            opDataG.BackgroundColor = Color.White;
            opDataG.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            opDataG.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            opDataG.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            opDataG.DefaultCellStyle = dataGridViewCellStyle3;
            opDataG.Dock = DockStyle.Fill;
            opDataG.EnableHeadersVisualStyles = false;
            opDataG.Font = new Font("Microsoft Sans Serif", 12F);
            opDataG.GridColor = Color.FromArgb(80, 160, 255);
            opDataG.Location = new Point(3, 3);
            opDataG.Name = "opDataG";
            opDataG.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            opDataG.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 12F);
            opDataG.RowsDefaultCellStyle = dataGridViewCellStyle5;
            opDataG.ScrollMode = Sunny.UI.UIDataGridView.UIDataGridViewScrollMode.Page;
            opDataG.SelectedIndex = -1;
            opDataG.Size = new Size(832, 495);
            opDataG.StripeOddColor = Color.FromArgb(235, 243, 255);
            opDataG.TabIndex = 0;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 1;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Controls.Add(uiTableLayoutPanel4, 0, 1);
            uiTableLayoutPanel2.Controls.Add(uiTableLayoutPanel3, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(3, 504);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 2;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 44.82759F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 55.17241F));
            uiTableLayoutPanel2.Size = new Size(832, 116);
            uiTableLayoutPanel2.TabIndex = 1;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.ColumnCount = 8;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            uiTableLayoutPanel4.Controls.Add(btnGetAll, 7, 0);
            uiTableLayoutPanel4.Controls.Add(ipDateFrom, 3, 0);
            uiTableLayoutPanel4.Controls.Add(btnExportCsv, 1, 0);
            uiTableLayoutPanel4.Controls.Add(btnExportPDF, 0, 0);
            uiTableLayoutPanel4.Controls.Add(btnGetLogs, 6, 0);
            uiTableLayoutPanel4.Controls.Add(ipLogType, 5, 0);
            uiTableLayoutPanel4.Controls.Add(ipDateTo, 4, 0);
            uiTableLayoutPanel4.Controls.Add(btnRefresh, 2, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(3, 55);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 1;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.Size = new Size(826, 58);
            uiTableLayoutPanel4.TabIndex = 1;
            uiTableLayoutPanel4.TagString = null;
            // 
            // btnGetAll
            // 
            btnGetAll.Cursor = Cursors.Hand;
            btnGetAll.Dock = DockStyle.Fill;
            btnGetAll.FillColor = Color.Aquamarine;
            btnGetAll.Font = new Font("Microsoft Sans Serif", 12F);
            btnGetAll.Location = new Point(739, 3);
            btnGetAll.MinimumSize = new Size(1, 1);
            btnGetAll.Name = "btnGetAll";
            btnGetAll.RectColor = Color.Blue;
            btnGetAll.RectSize = 2;
            btnGetAll.Size = new Size(84, 52);
            btnGetAll.Symbol = 559775;
            btnGetAll.SymbolColor = Color.MediumBlue;
            btnGetAll.SymbolSize = 30;
            btnGetAll.TabIndex = 7;
            btnGetAll.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnGetAll.Click += btnGetAll_Click;
            // 
            // ipDateFrom
            // 
            ipDateFrom.DateCultureInfo = new System.Globalization.CultureInfo("");
            ipDateFrom.Dock = DockStyle.Fill;
            ipDateFrom.FillColor = Color.White;
            ipDateFrom.Font = new Font("Microsoft Sans Serif", 12F);
            ipDateFrom.Location = new Point(182, 2);
            ipDateFrom.Margin = new Padding(2);
            ipDateFrom.MaxLength = 10;
            ipDateFrom.MinimumSize = new Size(63, 0);
            ipDateFrom.Name = "ipDateFrom";
            ipDateFrom.Padding = new Padding(0, 0, 30, 2);
            ipDateFrom.RectColor = Color.Blue;
            ipDateFrom.RectSize = 2;
            ipDateFrom.Size = new Size(116, 54);
            ipDateFrom.SymbolDropDown = 61555;
            ipDateFrom.SymbolNormal = 61555;
            ipDateFrom.SymbolSize = 24;
            ipDateFrom.TabIndex = 3;
            ipDateFrom.Text = "2025-01-01";
            ipDateFrom.TextAlignment = ContentAlignment.MiddleLeft;
            ipDateFrom.Value = new DateTime(2025, 1, 1, 0, 0, 0, 0);
            ipDateFrom.Watermark = "";
            // 
            // btnExportCsv
            // 
            btnExportCsv.Cursor = Cursors.Hand;
            btnExportCsv.Dock = DockStyle.Fill;
            btnExportCsv.FillColor = Color.WhiteSmoke;
            btnExportCsv.Font = new Font("Microsoft Sans Serif", 12F);
            btnExportCsv.Location = new Point(62, 2);
            btnExportCsv.Margin = new Padding(2);
            btnExportCsv.MinimumSize = new Size(1, 1);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.RectColor = Color.Blue;
            btnExportCsv.RectSize = 2;
            btnExportCsv.Size = new Size(56, 54);
            btnExportCsv.Symbol = 363197;
            btnExportCsv.SymbolColor = Color.Green;
            btnExportCsv.SymbolSize = 50;
            btnExportCsv.TabIndex = 1;
            btnExportCsv.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // btnExportPDF
            // 
            btnExportPDF.Cursor = Cursors.Hand;
            btnExportPDF.Dock = DockStyle.Fill;
            btnExportPDF.FillColor = Color.WhiteSmoke;
            btnExportPDF.Font = new Font("Microsoft Sans Serif", 12F);
            btnExportPDF.Location = new Point(2, 2);
            btnExportPDF.Margin = new Padding(2);
            btnExportPDF.MinimumSize = new Size(1, 1);
            btnExportPDF.Name = "btnExportPDF";
            btnExportPDF.RectColor = Color.Blue;
            btnExportPDF.RectSize = 2;
            btnExportPDF.Size = new Size(56, 54);
            btnExportPDF.Symbol = 261889;
            btnExportPDF.SymbolColor = Color.FromArgb(255, 128, 0);
            btnExportPDF.SymbolSize = 50;
            btnExportPDF.TabIndex = 0;
            btnExportPDF.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnExportPDF.Click += btnExportPDF_Click;
            // 
            // btnGetLogs
            // 
            btnGetLogs.Cursor = Cursors.Hand;
            btnGetLogs.Dock = DockStyle.Fill;
            btnGetLogs.FillColor = Color.Aquamarine;
            btnGetLogs.Font = new Font("Microsoft Sans Serif", 12F);
            btnGetLogs.Location = new Point(659, 3);
            btnGetLogs.MinimumSize = new Size(1, 1);
            btnGetLogs.Name = "btnGetLogs";
            btnGetLogs.RectColor = Color.Blue;
            btnGetLogs.RectSize = 2;
            btnGetLogs.Size = new Size(74, 52);
            btnGetLogs.Symbol = 61473;
            btnGetLogs.SymbolColor = Color.MediumBlue;
            btnGetLogs.SymbolSize = 30;
            btnGetLogs.TabIndex = 6;
            btnGetLogs.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnGetLogs.Click += btnGetLogs_Click;
            // 
            // ipLogType
            // 
            ipLogType.DataSource = null;
            ipLogType.Dock = DockStyle.Fill;
            ipLogType.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            ipLogType.FillColor = Color.White;
            ipLogType.Font = new Font("Microsoft Sans Serif", 12F);
            ipLogType.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipLogType.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipLogType.Location = new Point(438, 2);
            ipLogType.Margin = new Padding(2);
            ipLogType.MaxDropDownItems = 16;
            ipLogType.MinimumSize = new Size(63, 0);
            ipLogType.Name = "ipLogType";
            ipLogType.Padding = new Padding(0, 0, 30, 2);
            ipLogType.RectColor = Color.Blue;
            ipLogType.RectSize = 2;
            ipLogType.Size = new Size(216, 54);
            ipLogType.SymbolSize = 24;
            ipLogType.TabIndex = 5;
            ipLogType.TextAlignment = ContentAlignment.MiddleLeft;
            ipLogType.Watermark = "";
            // 
            // ipDateTo
            // 
            ipDateTo.DateCultureInfo = new System.Globalization.CultureInfo("");
            ipDateTo.Dock = DockStyle.Fill;
            ipDateTo.FillColor = Color.White;
            ipDateTo.Font = new Font("Microsoft Sans Serif", 12F);
            ipDateTo.Location = new Point(302, 2);
            ipDateTo.Margin = new Padding(2);
            ipDateTo.MaxLength = 10;
            ipDateTo.MinimumSize = new Size(63, 0);
            ipDateTo.Name = "ipDateTo";
            ipDateTo.Padding = new Padding(0, 0, 30, 2);
            ipDateTo.RectColor = Color.Blue;
            ipDateTo.RectSize = 2;
            ipDateTo.Size = new Size(132, 54);
            ipDateTo.SymbolDropDown = 61555;
            ipDateTo.SymbolNormal = 61555;
            ipDateTo.SymbolSize = 24;
            ipDateTo.TabIndex = 4;
            ipDateTo.Text = "2025-01-01";
            ipDateTo.TextAlignment = ContentAlignment.MiddleLeft;
            ipDateTo.Value = new DateTime(2025, 1, 1, 0, 0, 0, 0);
            ipDateTo.Watermark = "";
            // 
            // btnRefresh
            // 
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Dock = DockStyle.Fill;
            btnRefresh.FillColor = Color.WhiteSmoke;
            btnRefresh.Font = new Font("Microsoft Sans Serif", 12F);
            btnRefresh.Location = new Point(122, 2);
            btnRefresh.Margin = new Padding(2);
            btnRefresh.MinimumSize = new Size(1, 1);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.RectColor = Color.Blue;
            btnRefresh.RectSize = 2;
            btnRefresh.Size = new Size(56, 54);
            btnRefresh.Symbol = 61473;
            btnRefresh.SymbolColor = Color.Blue;
            btnRefresh.SymbolSize = 50;
            btnRefresh.TabIndex = 2;
            btnRefresh.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnRefresh.Click += btnRefresh_Click;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 3;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 93F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
            uiTableLayoutPanel3.Controls.Add(ipSize, 1, 0);
            uiTableLayoutPanel3.Controls.Add(uiPagination1, 0, 0);
            uiTableLayoutPanel3.Controls.Add(opTotalCount, 2, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(3, 3);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 1;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel3.Size = new Size(826, 46);
            uiTableLayoutPanel3.TabIndex = 0;
            uiTableLayoutPanel3.TagString = null;
            // 
            // ipSize
            // 
            ipSize.DataSource = null;
            ipSize.Dock = DockStyle.Fill;
            ipSize.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            ipSize.FillColor = Color.White;
            ipSize.Font = new Font("Microsoft Sans Serif", 12F);
            ipSize.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipSize.Items.AddRange(new object[] { "10", "20", "50", "100", "500", "1000" });
            ipSize.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipSize.Location = new Point(605, 5);
            ipSize.Margin = new Padding(4, 5, 4, 5);
            ipSize.MinimumSize = new Size(63, 0);
            ipSize.Name = "ipSize";
            ipSize.Padding = new Padding(0, 0, 30, 2);
            ipSize.Size = new Size(85, 36);
            ipSize.SymbolSize = 24;
            ipSize.TabIndex = 1;
            ipSize.Text = "50";
            ipSize.TextAlignment = ContentAlignment.MiddleLeft;
            ipSize.Watermark = "";
            ipSize.SelectedIndexChanged += ipSize_SelectedIndexChanged;
            // 
            // uiPagination1
            // 
            uiPagination1.ButtonFillSelectedColor = Color.FromArgb(64, 128, 204);
            uiPagination1.ButtonStyleInherited = false;
            uiPagination1.Dock = DockStyle.Fill;
            uiPagination1.Font = new Font("Microsoft Sans Serif", 12F);
            uiPagination1.Location = new Point(4, 5);
            uiPagination1.Margin = new Padding(4, 5, 4, 5);
            uiPagination1.MinimumSize = new Size(1, 1);
            uiPagination1.Name = "uiPagination1";
            uiPagination1.PageSize = 50;
            uiPagination1.RectSides = ToolStripStatusLabelBorderSides.None;
            uiPagination1.ShowText = false;
            uiPagination1.Size = new Size(593, 36);
            uiPagination1.TabIndex = 0;
            uiPagination1.Text = "uiPagination1";
            uiPagination1.TextAlignment = ContentAlignment.MiddleCenter;
            uiPagination1.TotalCount = 100;
            uiPagination1.PageChanged += uiPagination1_PageChanged;
            // 
            // opTotalCount
            // 
            opTotalCount.Dock = DockStyle.Fill;
            opTotalCount.Font = new Font("Microsoft Sans Serif", 12F);
            opTotalCount.Location = new Point(698, 5);
            opTotalCount.Margin = new Padding(4, 5, 4, 5);
            opTotalCount.MinimumSize = new Size(1, 1);
            opTotalCount.Name = "opTotalCount";
            opTotalCount.Size = new Size(124, 36);
            opTotalCount.TabIndex = 2;
            opTotalCount.Text = "0";
            opTotalCount.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // FActivityLogs
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(840, 674);
            Controls.Add(uiTitlePanel1);
            Name = "FActivityLogs";
            Symbol = 57591;
            Text = "Nhật ký";
            Initialize += FActivityLogs_Initialize;
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opDataG).EndInit();
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker WK_AutoLog;
        private System.ComponentModel.BackgroundWorker WK_Getlogs;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UIDataGridView opDataG;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UISymbolButton btnGetAll;
        private Sunny.UI.UIDatePicker ipDateFrom;
        private Sunny.UI.UISymbolButton btnExportCsv;
        private Sunny.UI.UISymbolButton btnExportPDF;
        private Sunny.UI.UISymbolButton btnGetLogs;
        private Sunny.UI.UIComboBox ipLogType;
        private Sunny.UI.UIDatePicker ipDateTo;
        private Sunny.UI.UISymbolButton btnRefresh;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UIComboBox ipSize;
        private Sunny.UI.UIPagination uiPagination1;
        private Sunny.UI.UIPanel opTotalCount;
    }
}
