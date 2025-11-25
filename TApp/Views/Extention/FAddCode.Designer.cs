namespace TApp.Views.Extention
{
    partial class FAddCode
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
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            btnAdd = new Sunny.UI.UISymbolButton();
            ipQRContent = new Sunny.UI.UITextBox();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            opConsole = new Sunny.UI.UIListBox();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            opQueueTable = new Sunny.UI.UIDataGridView();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            opStatusPanel = new Sunny.UI.UITitlePanel();
            opStatsPanel = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            uiLabel2 = new Sunny.UI.UILabel();
            uiLabel3 = new Sunny.UI.UILabel();
            opTotalAdded = new Sunny.UI.UILabel();
            opTotalSuccess = new Sunny.UI.UILabel();
            opQueueCount = new Sunny.UI.UILabel();
            opStatus = new Sunny.UI.UISymbolLabel();
            WK_Add = new System.ComponentModel.BackgroundWorker();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opQueueTable).BeginInit();
            uiTableLayoutPanel4.SuspendLayout();
            opStatusPanel.SuspendLayout();
            opStatsPanel.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
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
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 16.2002945F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 83.7997055F));
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
            uiTitlePanel1.Size = new Size(870, 106);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Nhập mã QR để thêm vào hệ thống";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85.13825F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.8617516F));
            uiTableLayoutPanel2.Controls.Add(btnAdd, 1, 0);
            uiTableLayoutPanel2.Controls.Add(ipQRContent, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(1, 35);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(868, 70);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // btnAdd
            // 
            btnAdd.Dock = DockStyle.Fill;
            btnAdd.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAdd.Location = new Point(742, 3);
            btnAdd.MinimumSize = new Size(1, 1);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(123, 64);
            btnAdd.Symbol = 361543;
            btnAdd.SymbolSize = 40;
            btnAdd.TabIndex = 0;
            btnAdd.Text = "THÊM MÃ";
            btnAdd.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnAdd.Click += btnAdd_Click;
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
            ipQRContent.Size = new Size(731, 60);
            ipQRContent.TabIndex = 1;
            ipQRContent.TextAlignment = ContentAlignment.MiddleLeft;
            ipQRContent.Watermark = "";
            ipQRContent.DoubleClick += ipQRContent_DoubleClick;
            ipQRContent.KeyDown += ipQRContent_KeyDown;
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
            uiTableLayoutPanel3.Location = new Point(2, 112);
            uiTableLayoutPanel3.Margin = new Padding(2);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 4;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 147F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
            uiTableLayoutPanel3.Size = new Size(870, 565);
            uiTableLayoutPanel3.TabIndex = 1;
            uiTableLayoutPanel3.TagString = null;
            // 
            // opConsole
            // 
            opConsole.Dock = DockStyle.Fill;
            opConsole.Font = new Font("Microsoft Sans Serif", 12F);
            opConsole.HoverColor = Color.FromArgb(155, 200, 255);
            opConsole.ItemSelectForeColor = Color.White;
            opConsole.Location = new Point(2, 408);
            opConsole.Margin = new Padding(2);
            opConsole.MinimumSize = new Size(1, 1);
            opConsole.Name = "opConsole";
            opConsole.Padding = new Padding(2);
            opConsole.ShowText = false;
            opConsole.Size = new Size(866, 112);
            opConsole.TabIndex = 1;
            opConsole.Text = "uiListBox1";
            // 
            // uiTitlePanel2
            // 
            uiTitlePanel2.Controls.Add(opQueueTable);
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel2.Location = new Point(2, 149);
            uiTitlePanel2.Margin = new Padding(2);
            uiTitlePanel2.MinimumSize = new Size(1, 1);
            uiTitlePanel2.Name = "uiTitlePanel2";
            uiTitlePanel2.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel2.ShowText = false;
            uiTitlePanel2.Size = new Size(866, 255);
            uiTitlePanel2.TabIndex = 3;
            uiTitlePanel2.Text = "Danh sách mã đang chờ xử lý";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel2.TitleColor = Color.FromArgb(0, 192, 192);
            // 
            // opQueueTable
            // 
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            opQueueTable.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            opQueueTable.BackgroundColor = Color.White;
            opQueueTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            opQueueTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            opQueueTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            opQueueTable.DefaultCellStyle = dataGridViewCellStyle3;
            opQueueTable.Dock = DockStyle.Fill;
            opQueueTable.EnableHeadersVisualStyles = false;
            opQueueTable.Font = new Font("Microsoft Sans Serif", 12F);
            opQueueTable.GridColor = Color.FromArgb(80, 160, 255);
            opQueueTable.Location = new Point(1, 35);
            opQueueTable.Margin = new Padding(2);
            opQueueTable.Name = "opQueueTable";
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            opQueueTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 12F);
            opQueueTable.RowsDefaultCellStyle = dataGridViewCellStyle5;
            opQueueTable.SelectedIndex = -1;
            opQueueTable.Size = new Size(864, 219);
            opQueueTable.StripeOddColor = Color.FromArgb(235, 243, 255);
            opQueueTable.TabIndex = 1;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.ColumnCount = 2;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62.7809F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.2191F));
            uiTableLayoutPanel4.Controls.Add(opStatusPanel, 1, 0);
            uiTableLayoutPanel4.Controls.Add(opStatus, 0, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(2, 2);
            uiTableLayoutPanel4.Margin = new Padding(2);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 1;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.Size = new Size(866, 143);
            uiTableLayoutPanel4.TabIndex = 4;
            uiTableLayoutPanel4.TagString = null;
            // 
            // opStatusPanel
            // 
            opStatusPanel.Controls.Add(opStatsPanel);
            opStatusPanel.Controls.Add(opQueueCount);
            opStatusPanel.Dock = DockStyle.Fill;
            opStatusPanel.Font = new Font("Microsoft Sans Serif", 12F);
            opStatusPanel.Location = new Point(545, 2);
            opStatusPanel.Margin = new Padding(2);
            opStatusPanel.MinimumSize = new Size(1, 1);
            opStatusPanel.Name = "opStatusPanel";
            opStatusPanel.Padding = new Padding(1, 25, 1, 1);
            opStatusPanel.ShowText = false;
            opStatusPanel.Size = new Size(319, 139);
            opStatusPanel.TabIndex = 5;
            opStatusPanel.Text = "Số lượng trong hàng đợi";
            opStatusPanel.TextAlignment = ContentAlignment.MiddleCenter;
            opStatusPanel.TitleColor = Color.Green;
            opStatusPanel.TitleHeight = 25;
            // 
            // opStatsPanel
            // 
            opStatsPanel.Controls.Add(uiTableLayoutPanel5);
            opStatsPanel.Dock = DockStyle.Fill;
            opStatsPanel.Font = new Font("Microsoft Sans Serif", 12F);
            opStatsPanel.Location = new Point(1, 25);
            opStatsPanel.Margin = new Padding(2);
            opStatsPanel.MinimumSize = new Size(1, 1);
            opStatsPanel.Name = "opStatsPanel";
            opStatsPanel.Padding = new Padding(1, 25, 1, 1);
            opStatsPanel.ShowText = false;
            opStatsPanel.Size = new Size(317, 113);
            opStatsPanel.TabIndex = 1;
            opStatsPanel.Text = "Thống kê";
            opStatsPanel.TextAlignment = ContentAlignment.MiddleCenter;
            opStatsPanel.TitleColor = Color.Green;
            opStatsPanel.TitleHeight = 25;
            opStatsPanel.Visible = false;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.Controls.Add(uiLabel2, 0, 0);
            uiTableLayoutPanel5.Controls.Add(uiLabel3, 0, 1);
            uiTableLayoutPanel5.Controls.Add(opTotalAdded, 1, 0);
            uiTableLayoutPanel5.Controls.Add(opTotalSuccess, 1, 1);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(1, 25);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 2;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.Size = new Size(315, 87);
            uiTableLayoutPanel5.TabIndex = 0;
            uiTableLayoutPanel5.TagString = null;
            // 
            // uiLabel2
            // 
            uiLabel2.Dock = DockStyle.Fill;
            uiLabel2.Font = new Font("Microsoft Sans Serif", 10F);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(3, 0);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(151, 43);
            uiLabel2.TabIndex = 0;
            uiLabel2.Text = "Đã thêm:";
            uiLabel2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // uiLabel3
            // 
            uiLabel3.Dock = DockStyle.Fill;
            uiLabel3.Font = new Font("Microsoft Sans Serif", 10F);
            uiLabel3.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel3.Location = new Point(3, 43);
            uiLabel3.Name = "uiLabel3";
            uiLabel3.Size = new Size(151, 44);
            uiLabel3.TabIndex = 1;
            uiLabel3.Text = "Thành công:";
            uiLabel3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // opTotalAdded
            // 
            opTotalAdded.Dock = DockStyle.Fill;
            opTotalAdded.Font = new Font("Microsoft Sans Serif", 10F);
            opTotalAdded.ForeColor = Color.Blue;
            opTotalAdded.Location = new Point(160, 0);
            opTotalAdded.Name = "opTotalAdded";
            opTotalAdded.Size = new Size(152, 43);
            opTotalAdded.TabIndex = 2;
            opTotalAdded.Text = "0";
            opTotalAdded.TextAlign = ContentAlignment.MiddleRight;
            // 
            // opTotalSuccess
            // 
            opTotalSuccess.Dock = DockStyle.Fill;
            opTotalSuccess.Font = new Font("Microsoft Sans Serif", 10F);
            opTotalSuccess.ForeColor = Color.Green;
            opTotalSuccess.Location = new Point(160, 43);
            opTotalSuccess.Name = "opTotalSuccess";
            opTotalSuccess.Size = new Size(152, 44);
            opTotalSuccess.TabIndex = 3;
            opTotalSuccess.Text = "0";
            opTotalSuccess.TextAlign = ContentAlignment.MiddleRight;
            // 
            // opQueueCount
            // 
            opQueueCount.Dock = DockStyle.Fill;
            opQueueCount.Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold);
            opQueueCount.ForeColor = Color.FromArgb(48, 48, 48);
            opQueueCount.Location = new Point(1, 25);
            opQueueCount.Name = "opQueueCount";
            opQueueCount.Size = new Size(317, 113);
            opQueueCount.TabIndex = 0;
            opQueueCount.Text = "0";
            opQueueCount.TextAlign = ContentAlignment.MiddleCenter;
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
            opStatus.Size = new Size(537, 137);
            opStatus.Symbol = 61761;
            opStatus.SymbolColor = Color.Black;
            opStatus.SymbolSize = 50;
            opStatus.TabIndex = 3;
            opStatus.Text = "SẴN SÀNG";
            // 
            // WK_Add
            // 
            WK_Add.WorkerReportsProgress = true;
            WK_Add.DoWork += WK_Add_DoWork;
            // 
            // FAddCode
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(874, 679);
            Controls.Add(uiTableLayoutPanel1);
            Name = "FAddCode";
            Text = "Thêm mã kích hoạt";
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opQueueTable).EndInit();
            uiTableLayoutPanel4.ResumeLayout(false);
            opStatusPanel.ResumeLayout(false);
            opStatsPanel.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnAdd;
        private Sunny.UI.UITextBox ipQRContent;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UIListBox opConsole;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Sunny.UI.UIDataGridView opQueueTable;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UITitlePanel opStatusPanel;
        private Sunny.UI.UILabel opQueueCount;
        private Sunny.UI.UISymbolLabel opStatus;
        private Sunny.UI.UITitlePanel opStatsPanel;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UILabel opTotalAdded;
        private Sunny.UI.UILabel opTotalSuccess;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UILabel uiLabel3;
        private System.ComponentModel.BackgroundWorker WK_Add;
    }
}
