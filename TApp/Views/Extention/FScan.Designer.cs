namespace TApp.Views.Extention
{
    partial class FScan
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
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            btnFind = new Sunny.UI.UISymbolButton();
            ipQRContent = new Sunny.UI.UITextBox();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            opConsole = new Sunny.UI.UIListBox();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            opInfoTable = new Sunny.UI.UIDataGridView();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            opTypePanel = new Sunny.UI.UITitlePanel();
            opType = new Sunny.UI.UILabel();
            opStatus = new Sunny.UI.UISymbolLabel();
            opTimePanel = new Sunny.UI.UITitlePanel();
            opTime = new Sunny.UI.UILabel();
            uiLabel1 = new Sunny.UI.UILabel();
            WK_Find = new System.ComponentModel.BackgroundWorker();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opInfoTable).BeginInit();
            uiTableLayoutPanel4.SuspendLayout();
            opTypePanel.SuspendLayout();
            opTimePanel.SuspendLayout();
            SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel1, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel3, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 21.3549328F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 78.6450653F));
            uiTableLayoutPanel1.Size = new Size(874, 679);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel2);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 2);
            uiTitlePanel1.Margin = new Padding(2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(870, 140);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Nhập mã vào ô phía dưới hoặc quét mã bằng tay cầm";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85.13825F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.8617516F));
            uiTableLayoutPanel2.Controls.Add(btnFind, 1, 0);
            uiTableLayoutPanel2.Controls.Add(ipQRContent, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(1, 35);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(868, 104);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // btnFind
            // 
            btnFind.Dock = DockStyle.Fill;
            btnFind.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnFind.Location = new Point(742, 3);
            btnFind.MinimumSize = new Size(1, 1);
            btnFind.Name = "btnFind";
            btnFind.Size = new Size(123, 98);
            btnFind.Symbol = 561487;
            btnFind.SymbolSize = 40;
            btnFind.TabIndex = 0;
            btnFind.Text = "KIỂM TRA";
            btnFind.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnFind.Click += btnFind_Click;
            // 
            // ipQRContent
            // 
            ipQRContent.Dock = DockStyle.Fill;
            ipQRContent.Font = new Font("Microsoft Sans Serif", 12F);
            ipQRContent.Location = new Point(4, 5);
            ipQRContent.Margin = new Padding(4, 5, 4, 5);
            ipQRContent.MinimumSize = new Size(1, 16);
            ipQRContent.Name = "ipQRContent";
            ipQRContent.Padding = new Padding(5);
            ipQRContent.ShowText = false;
            ipQRContent.Size = new Size(731, 94);
            ipQRContent.TabIndex = 1;
            ipQRContent.TextAlignment = ContentAlignment.MiddleLeft;
            ipQRContent.Watermark = "";
            ipQRContent.DoubleClick += ipQRContent_DoubleClick;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 1;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            uiTableLayoutPanel3.Controls.Add(opConsole, 0, 2);
            uiTableLayoutPanel3.Controls.Add(uiTitlePanel2, 0, 1);
            uiTableLayoutPanel3.Controls.Add(uiTableLayoutPanel4, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(2, 146);
            uiTableLayoutPanel3.Margin = new Padding(2);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 4;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 73F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 148F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
            uiTableLayoutPanel3.Size = new Size(870, 531);
            uiTableLayoutPanel3.TabIndex = 1;
            uiTableLayoutPanel3.TagString = null;
            // 
            // opConsole
            // 
            opConsole.Dock = DockStyle.Fill;
            opConsole.Font = new Font("Microsoft Sans Serif", 12F);
            opConsole.HoverColor = Color.FromArgb(155, 200, 255);
            opConsole.ItemSelectForeColor = Color.White;
            opConsole.Location = new Point(2, 342);
            opConsole.Margin = new Padding(2);
            opConsole.MinimumSize = new Size(1, 1);
            opConsole.Name = "opConsole";
            opConsole.Padding = new Padding(2);
            opConsole.ShowText = false;
            opConsole.Size = new Size(866, 144);
            opConsole.TabIndex = 1;
            opConsole.Text = "uiListBox1";
            // 
            // uiTitlePanel2
            // 
            uiTitlePanel2.Controls.Add(opInfoTable);
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel2.Location = new Point(2, 75);
            uiTitlePanel2.Margin = new Padding(2);
            uiTitlePanel2.MinimumSize = new Size(1, 1);
            uiTitlePanel2.Name = "uiTitlePanel2";
            uiTitlePanel2.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel2.ShowText = false;
            uiTitlePanel2.Size = new Size(866, 263);
            uiTitlePanel2.TabIndex = 3;
            uiTitlePanel2.Text = "Bảng thông tin";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel2.TitleColor = Color.FromArgb(0, 192, 192);
            // 
            // opInfoTable
            // 
            dataGridViewCellStyle6.BackColor = Color.FromArgb(235, 243, 255);
            opInfoTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            opInfoTable.BackgroundColor = Color.White;
            opInfoTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle7.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle7.ForeColor = Color.White;
            dataGridViewCellStyle7.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            opInfoTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            opInfoTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = SystemColors.Window;
            dataGridViewCellStyle8.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle8.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle8.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = DataGridViewTriState.False;
            opInfoTable.DefaultCellStyle = dataGridViewCellStyle8;
            opInfoTable.Dock = DockStyle.Fill;
            opInfoTable.EnableHeadersVisualStyles = false;
            opInfoTable.Font = new Font("Microsoft Sans Serif", 12F);
            opInfoTable.GridColor = Color.FromArgb(80, 160, 255);
            opInfoTable.Location = new Point(1, 35);
            opInfoTable.Margin = new Padding(2);
            opInfoTable.Name = "opInfoTable";
            dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle9.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle9.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle9.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle9.SelectionForeColor = Color.White;
            dataGridViewCellStyle9.WrapMode = DataGridViewTriState.True;
            opInfoTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            dataGridViewCellStyle10.BackColor = Color.White;
            dataGridViewCellStyle10.Font = new Font("Microsoft Sans Serif", 12F);
            opInfoTable.RowsDefaultCellStyle = dataGridViewCellStyle10;
            opInfoTable.SelectedIndex = -1;
            opInfoTable.Size = new Size(864, 227);
            opInfoTable.StripeOddColor = Color.FromArgb(235, 243, 255);
            opInfoTable.TabIndex = 1;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.ColumnCount = 3;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62.7809F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.2191F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 153F));
            uiTableLayoutPanel4.Controls.Add(opTypePanel, 2, 0);
            uiTableLayoutPanel4.Controls.Add(opStatus, 0, 0);
            uiTableLayoutPanel4.Controls.Add(opTimePanel, 1, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(2, 2);
            uiTableLayoutPanel4.Margin = new Padding(2);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 1;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.Size = new Size(866, 69);
            uiTableLayoutPanel4.TabIndex = 4;
            uiTableLayoutPanel4.TagString = null;
            // 
            // opTypePanel
            // 
            opTypePanel.Controls.Add(opType);
            opTypePanel.Dock = DockStyle.Fill;
            opTypePanel.Font = new Font("Microsoft Sans Serif", 12F);
            opTypePanel.Location = new Point(714, 2);
            opTypePanel.Margin = new Padding(2);
            opTypePanel.MinimumSize = new Size(1, 1);
            opTypePanel.Name = "opTypePanel";
            opTypePanel.Padding = new Padding(1, 25, 1, 1);
            opTypePanel.ShowText = false;
            opTypePanel.Size = new Size(150, 65);
            opTypePanel.TabIndex = 5;
            opTypePanel.Text = "Loại kích hoạt";
            opTypePanel.TextAlignment = ContentAlignment.MiddleCenter;
            opTypePanel.TitleColor = Color.Green;
            opTypePanel.TitleHeight = 25;
            // 
            // opType
            // 
            opType.Dock = DockStyle.Fill;
            opType.Font = new Font("Microsoft Sans Serif", 12F);
            opType.ForeColor = Color.FromArgb(48, 48, 48);
            opType.Location = new Point(1, 25);
            opType.Name = "opType";
            opType.Size = new Size(148, 39);
            opType.TabIndex = 0;
            opType.Text = "Tự Động";
            opType.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // opStatus
            // 
            opStatus.BackColor = Color.Gainsboro;
            opStatus.Dock = DockStyle.Fill;
            opStatus.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            opStatus.ForeColor = Color.Black;
            opStatus.Location = new Point(3, 3);
            opStatus.MinimumSize = new Size(1, 1);
            opStatus.Name = "opStatus";
            opStatus.Size = new Size(441, 63);
            opStatus.Symbol = 61761;
            opStatus.SymbolColor = Color.Black;
            opStatus.SymbolSize = 50;
            opStatus.TabIndex = 3;
            opStatus.Text = "ĐANG TÌM KIẾM ...";
            // 
            // opTimePanel
            // 
            opTimePanel.Controls.Add(opTime);
            opTimePanel.Controls.Add(uiLabel1);
            opTimePanel.Dock = DockStyle.Fill;
            opTimePanel.Font = new Font("Microsoft Sans Serif", 12F);
            opTimePanel.Location = new Point(449, 2);
            opTimePanel.Margin = new Padding(2);
            opTimePanel.MinimumSize = new Size(1, 1);
            opTimePanel.Name = "opTimePanel";
            opTimePanel.Padding = new Padding(1, 25, 1, 1);
            opTimePanel.ShowText = false;
            opTimePanel.Size = new Size(261, 65);
            opTimePanel.TabIndex = 4;
            opTimePanel.Text = "Thời gian kích hoạt";
            opTimePanel.TextAlignment = ContentAlignment.MiddleCenter;
            opTimePanel.TitleColor = Color.Green;
            opTimePanel.TitleHeight = 25;
            // 
            // opTime
            // 
            opTime.Dock = DockStyle.Fill;
            opTime.Font = new Font("Microsoft Sans Serif", 12F);
            opTime.ForeColor = Color.FromArgb(48, 48, 48);
            opTime.Location = new Point(1, 25);
            opTime.Name = "opTime";
            opTime.Size = new Size(259, 39);
            opTime.TabIndex = 0;
            opTime.Text = "2025-11-24 00:01:02.123+0700";
            opTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(58, 43);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(8, 8);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "uiLabel1";
            // 
            // WK_Find
            // 
            WK_Find.WorkerReportsProgress = true;
            WK_Find.DoWork += WK_Find_DoWork;
            // 
            // FScan
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(874, 679);
            Controls.Add(uiTableLayoutPanel1);
            Name = "FScan";
            Text = "Quét QR";
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opInfoTable).EndInit();
            uiTableLayoutPanel4.ResumeLayout(false);
            opTypePanel.ResumeLayout(false);
            opTimePanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnFind;
        private Sunny.UI.UITextBox ipQRContent;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UIListBox opConsole;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Sunny.UI.UIDataGridView opInfoTable;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UITitlePanel opTypePanel;
        private Sunny.UI.UILabel opType;
        private Sunny.UI.UISymbolLabel opStatus;
        private Sunny.UI.UITitlePanel opTimePanel;
        private Sunny.UI.UILabel opTime;
        private Sunny.UI.UILabel uiLabel1;
        private System.ComponentModel.BackgroundWorker WK_Find;
    }
}