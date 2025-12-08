namespace TApp.Views.Extention
{
    partial class FExtention
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            tab2 = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel7 = new Sunny.UI.UITitlePanel();
            opLastTimeUpload = new Sunny.UI.UISymbolLabel();
            uiSymbolLabel3 = new Sunny.UI.UISymbolLabel();
            uiTitlePanel3 = new Sunny.UI.UITitlePanel();
            opLastUploadFileName = new Sunny.UI.UISymbolLabel();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            opC1 = new Sunny.UI.UISymbolLabel();
            opNextUploadTime = new Sunny.UI.UISymbolLabel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            btnCloudHis = new Sunny.UI.UISymbolButton();
            btnERPCheck = new Sunny.UI.UISymbolButton();
            opData = new Sunny.UI.UIDataGridView();
            opConsole = new Sunny.UI.UIListBox();
            tabPage3 = new TabPage();
            uiTableLayoutPanel6 = new Sunny.UI.UITableLayoutPanel();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiTitlePanel4 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel7 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel5 = new Sunny.UI.UITitlePanel();
            uiListBox1 = new Sunny.UI.UIListBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            erP_Google1 = new TTManager.Masan.ERP_Google(components);
            backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            WK_IOT_SCADA = new System.ComponentModel.BackgroundWorker();
            tab2.SuspendLayout();
            tabPage1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            uiTitlePanel7.SuspendLayout();
            uiTitlePanel3.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opData).BeginInit();
            tabPage3.SuspendLayout();
            uiTableLayoutPanel6.SuspendLayout();
            uiTitlePanel4.SuspendLayout();
            uiTitlePanel5.SuspendLayout();
            SuspendLayout();
            // 
            // tab2
            // 
            tab2.Controls.Add(tabPage1);
            tab2.Controls.Add(tabPage3);
            tab2.Dock = DockStyle.Fill;
            tab2.DrawMode = TabDrawMode.OwnerDrawFixed;
            tab2.Font = new Font("Microsoft Sans Serif", 12F);
            tab2.ItemSize = new Size(150, 40);
            tab2.Location = new Point(0, 0);
            tab2.MainPage = "";
            tab2.Name = "tab2";
            tab2.SelectedIndex = 0;
            tab2.Size = new Size(874, 679);
            tab2.SizeMode = TabSizeMode.Fixed;
            tab2.TabIndex = 0;
            tab2.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            tab2.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(uiTableLayoutPanel1);
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(874, 639);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Sao lưu và ERP";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel4, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel1, 0, 3);
            uiTableLayoutPanel1.Controls.Add(opData, 0, 1);
            uiTableLayoutPanel1.Controls.Add(opConsole, 0, 2);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Margin = new Padding(2);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 4;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 87F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 216F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 93F));
            uiTableLayoutPanel1.Size = new Size(874, 639);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.ColumnCount = 3;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 318F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 242F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel7, 1, 0);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel3, 2, 0);
            uiTableLayoutPanel4.Controls.Add(uiTitlePanel2, 0, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(2, 2);
            uiTableLayoutPanel4.Margin = new Padding(2);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 1;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.Size = new Size(870, 83);
            uiTableLayoutPanel4.TabIndex = 4;
            uiTableLayoutPanel4.TagString = null;
            // 
            // uiTitlePanel7
            // 
            uiTitlePanel7.Controls.Add(opLastTimeUpload);
            uiTitlePanel7.Controls.Add(uiSymbolLabel3);
            uiTitlePanel7.Dock = DockStyle.Fill;
            uiTitlePanel7.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel7.Location = new Point(320, 2);
            uiTitlePanel7.Margin = new Padding(2);
            uiTitlePanel7.MinimumSize = new Size(1, 1);
            uiTitlePanel7.Name = "uiTitlePanel7";
            uiTitlePanel7.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel7.ShowText = false;
            uiTitlePanel7.Size = new Size(238, 79);
            uiTitlePanel7.TabIndex = 4;
            uiTitlePanel7.Text = "Thời gian vừa tải lên";
            uiTitlePanel7.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opLastTimeUpload
            // 
            opLastTimeUpload.Dock = DockStyle.Fill;
            opLastTimeUpload.Font = new Font("Microsoft Sans Serif", 12F);
            opLastTimeUpload.Location = new Point(1, 35);
            opLastTimeUpload.MinimumSize = new Size(1, 1);
            opLastTimeUpload.Name = "opLastTimeUpload";
            opLastTimeUpload.Size = new Size(236, 43);
            opLastTimeUpload.Symbol = 559480;
            opLastTimeUpload.TabIndex = 2;
            opLastTimeUpload.Text = "2025-11-29 23:23:23.999";
            // 
            // uiSymbolLabel3
            // 
            uiSymbolLabel3.Dock = DockStyle.Fill;
            uiSymbolLabel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel3.Location = new Point(1, 35);
            uiSymbolLabel3.MinimumSize = new Size(1, 1);
            uiSymbolLabel3.Name = "uiSymbolLabel3";
            uiSymbolLabel3.Size = new Size(236, 43);
            uiSymbolLabel3.TabIndex = 1;
            uiSymbolLabel3.Text = "2025-11-29 23:23:23.999";
            // 
            // uiTitlePanel3
            // 
            uiTitlePanel3.Controls.Add(opLastUploadFileName);
            uiTitlePanel3.Dock = DockStyle.Fill;
            uiTitlePanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel3.Location = new Point(562, 2);
            uiTitlePanel3.Margin = new Padding(2);
            uiTitlePanel3.MinimumSize = new Size(1, 1);
            uiTitlePanel3.Name = "uiTitlePanel3";
            uiTitlePanel3.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel3.ShowText = false;
            uiTitlePanel3.Size = new Size(306, 79);
            uiTitlePanel3.TabIndex = 1;
            uiTitlePanel3.Text = "Tên tệp vừa tải lên";
            uiTitlePanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opLastUploadFileName
            // 
            opLastUploadFileName.Dock = DockStyle.Fill;
            opLastUploadFileName.Font = new Font("Microsoft Sans Serif", 12F);
            opLastUploadFileName.Location = new Point(1, 35);
            opLastUploadFileName.MinimumSize = new Size(1, 1);
            opLastUploadFileName.Name = "opLastUploadFileName";
            opLastUploadFileName.Size = new Size(304, 43);
            opLastUploadFileName.TabIndex = 2;
            opLastUploadFileName.Text = "Line 3_";
            // 
            // uiTitlePanel2
            // 
            uiTitlePanel2.Controls.Add(uiTableLayoutPanel5);
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel2.Location = new Point(2, 2);
            uiTitlePanel2.Margin = new Padding(2);
            uiTitlePanel2.MinimumSize = new Size(1, 1);
            uiTitlePanel2.Name = "uiTitlePanel2";
            uiTitlePanel2.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel2.ShowText = false;
            uiTitlePanel2.Size = new Size(314, 79);
            uiTitlePanel2.TabIndex = 0;
            uiTitlePanel2.Text = "Thời gian chuẩn bị tải lên";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72.82609F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.173914F));
            uiTableLayoutPanel5.Controls.Add(opC1, 1, 0);
            uiTableLayoutPanel5.Controls.Add(opNextUploadTime, 0, 0);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(1, 35);
            uiTableLayoutPanel5.Margin = new Padding(2);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 1;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.Size = new Size(312, 43);
            uiTableLayoutPanel5.TabIndex = 0;
            uiTableLayoutPanel5.TagString = null;
            // 
            // opC1
            // 
            opC1.Dock = DockStyle.Fill;
            opC1.Font = new Font("Microsoft Sans Serif", 12F);
            opC1.Location = new Point(230, 3);
            opC1.MinimumSize = new Size(1, 1);
            opC1.Name = "opC1";
            opC1.Size = new Size(79, 37);
            opC1.Symbol = 557747;
            opC1.TabIndex = 1;
            opC1.Text = "300";
            // 
            // opNextUploadTime
            // 
            opNextUploadTime.Dock = DockStyle.Fill;
            opNextUploadTime.Font = new Font("Microsoft Sans Serif", 12F);
            opNextUploadTime.Location = new Point(3, 3);
            opNextUploadTime.MinimumSize = new Size(1, 1);
            opNextUploadTime.Name = "opNextUploadTime";
            opNextUploadTime.Size = new Size(221, 37);
            opNextUploadTime.Symbol = 261463;
            opNextUploadTime.TabIndex = 0;
            opNextUploadTime.Text = "2025-11-29T23:23:23.999";
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel2);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 548);
            uiTitlePanel1.Margin = new Padding(2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(870, 89);
            uiTitlePanel1.TabIndex = 1;
            uiTitlePanel1.Text = "Chức Năng";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 3;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 571F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.84175F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49.15825F));
            uiTableLayoutPanel2.Controls.Add(btnCloudHis, 1, 0);
            uiTableLayoutPanel2.Controls.Add(btnERPCheck, 2, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(1, 35);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(868, 53);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // btnCloudHis
            // 
            btnCloudHis.Dock = DockStyle.Fill;
            btnCloudHis.FillColor = Color.FromArgb(0, 192, 192);
            btnCloudHis.Font = new Font("Microsoft Sans Serif", 12F);
            btnCloudHis.Location = new Point(573, 2);
            btnCloudHis.Margin = new Padding(2);
            btnCloudHis.MinimumSize = new Size(1, 1);
            btnCloudHis.Name = "btnCloudHis";
            btnCloudHis.Size = new Size(147, 49);
            btnCloudHis.Symbol = 560250;
            btnCloudHis.TabIndex = 2;
            btnCloudHis.Text = "Lịch sử tải lên";
            btnCloudHis.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnCloudHis.Click += btnCloudHis_Click;
            // 
            // btnERPCheck
            // 
            btnERPCheck.Dock = DockStyle.Fill;
            btnERPCheck.FillColor = Color.FromArgb(0, 192, 192);
            btnERPCheck.Font = new Font("Microsoft Sans Serif", 12F);
            btnERPCheck.Location = new Point(724, 2);
            btnERPCheck.Margin = new Padding(2);
            btnERPCheck.MinimumSize = new Size(1, 1);
            btnERPCheck.Name = "btnERPCheck";
            btnERPCheck.Size = new Size(142, 49);
            btnERPCheck.Symbol = 561637;
            btnERPCheck.TabIndex = 0;
            btnERPCheck.Text = "Kiểm tra ERP";
            btnERPCheck.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnERPCheck.Click += btnERPCheck_Click;
            // 
            // opData
            // 
            opData.AllowUserToAddRows = false;
            opData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            opData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            opData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            opData.BackgroundColor = Color.White;
            opData.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            opData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            opData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            opData.DefaultCellStyle = dataGridViewCellStyle3;
            opData.Dock = DockStyle.Fill;
            opData.EnableHeadersVisualStyles = false;
            opData.Font = new Font("Microsoft Sans Serif", 12F);
            opData.GridColor = Color.FromArgb(80, 160, 255);
            opData.Location = new Point(3, 90);
            opData.Name = "opData";
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            opData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 12F);
            opData.RowsDefaultCellStyle = dataGridViewCellStyle5;
            opData.SelectedIndex = -1;
            opData.Size = new Size(868, 237);
            opData.StripeOddColor = Color.FromArgb(235, 243, 255);
            opData.TabIndex = 2;
            // 
            // opConsole
            // 
            opConsole.Dock = DockStyle.Fill;
            opConsole.Font = new Font("Microsoft Sans Serif", 12F);
            opConsole.HoverColor = Color.FromArgb(155, 200, 255);
            opConsole.ItemSelectForeColor = Color.White;
            opConsole.Location = new Point(2, 332);
            opConsole.Margin = new Padding(2);
            opConsole.MinimumSize = new Size(1, 1);
            opConsole.Name = "opConsole";
            opConsole.Padding = new Padding(2);
            opConsole.ShowText = false;
            opConsole.Size = new Size(870, 212);
            opConsole.TabIndex = 3;
            opConsole.Text = "uiListBox1";
            opConsole.DoubleClick += opConsole_DoubleClick;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(uiTableLayoutPanel6);
            tabPage3.Location = new Point(0, 40);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(200, 60);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Kiểm tra hệ thống";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // uiTableLayoutPanel6
            // 
            uiTableLayoutPanel6.ColumnCount = 1;
            uiTableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel6.Controls.Add(uiPanel2, 0, 0);
            uiTableLayoutPanel6.Controls.Add(uiTitlePanel4, 0, 2);
            uiTableLayoutPanel6.Controls.Add(uiTitlePanel5, 0, 1);
            uiTableLayoutPanel6.Dock = DockStyle.Fill;
            uiTableLayoutPanel6.Location = new Point(0, 0);
            uiTableLayoutPanel6.Margin = new Padding(2);
            uiTableLayoutPanel6.Name = "uiTableLayoutPanel6";
            uiTableLayoutPanel6.RowCount = 3;
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 176F));
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            uiTableLayoutPanel6.Size = new Size(200, 60);
            uiTableLayoutPanel6.TabIndex = 1;
            uiTableLayoutPanel6.TagString = null;
            // 
            // uiPanel2
            // 
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel2.Location = new Point(2, 2);
            uiPanel2.Margin = new Padding(2);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.Size = new Size(196, 50);
            uiPanel2.TabIndex = 0;
            uiPanel2.Text = "Trang này cung cấp các tính năng nâng cao để kiểm tra lỗi của thiết bị";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel4
            // 
            uiTitlePanel4.Controls.Add(uiTableLayoutPanel7);
            uiTitlePanel4.Dock = DockStyle.Fill;
            uiTitlePanel4.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel4.Location = new Point(2, -114);
            uiTitlePanel4.Margin = new Padding(2);
            uiTitlePanel4.MinimumSize = new Size(1, 1);
            uiTitlePanel4.Name = "uiTitlePanel4";
            uiTitlePanel4.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel4.ShowText = false;
            uiTitlePanel4.Size = new Size(196, 172);
            uiTitlePanel4.TabIndex = 1;
            uiTitlePanel4.Text = "Chức Năng";
            uiTitlePanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel7
            // 
            uiTableLayoutPanel7.ColumnCount = 5;
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 152F));
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 171F));
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176F));
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 182F));
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            uiTableLayoutPanel7.Dock = DockStyle.Fill;
            uiTableLayoutPanel7.Location = new Point(1, 35);
            uiTableLayoutPanel7.Margin = new Padding(2);
            uiTableLayoutPanel7.Name = "uiTableLayoutPanel7";
            uiTableLayoutPanel7.RowCount = 2;
            uiTableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel7.Size = new Size(194, 136);
            uiTableLayoutPanel7.TabIndex = 0;
            uiTableLayoutPanel7.TagString = null;
            // 
            // uiTitlePanel5
            // 
            uiTitlePanel5.Controls.Add(uiListBox1);
            uiTitlePanel5.Dock = DockStyle.Fill;
            uiTitlePanel5.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel5.Location = new Point(2, 56);
            uiTitlePanel5.Margin = new Padding(2);
            uiTitlePanel5.MinimumSize = new Size(1, 1);
            uiTitlePanel5.Name = "uiTitlePanel5";
            uiTitlePanel5.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel5.ShowText = false;
            uiTitlePanel5.Size = new Size(196, 1);
            uiTitlePanel5.TabIndex = 2;
            uiTitlePanel5.Text = "Bảng Thông Báo";
            uiTitlePanel5.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiListBox1
            // 
            uiListBox1.Dock = DockStyle.Fill;
            uiListBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiListBox1.HoverColor = Color.FromArgb(155, 200, 255);
            uiListBox1.ItemSelectForeColor = Color.White;
            uiListBox1.Location = new Point(1, 35);
            uiListBox1.Margin = new Padding(2);
            uiListBox1.MinimumSize = new Size(1, 1);
            uiListBox1.Name = "uiListBox1";
            uiListBox1.Padding = new Padding(2);
            uiListBox1.ShowText = false;
            uiListBox1.Size = new Size(194, 1);
            uiListBox1.TabIndex = 4;
            uiListBox1.Text = "uiListBox1";
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            // 
            // erP_Google1
            // 
            erP_Google1.credentialPath = "C:\\Masan_Sales-268504-8f6f3a1f4f7e.json";
            erP_Google1.DatasetID = "FactoryIntegration";
            erP_Google1.LineName = "DL01";
            erP_Google1.ORG_CODE = "MIP";
            erP_Google1.ProjectID = "sales-268504";
            erP_Google1.SUB_INV = "110-101-1001";
            erP_Google1.TableID = "BatchProduction";
            // 
            // backgroundWorker2
            // 
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.DoWork += backgroundWorker2_DoWork;
            // 
            // WK_IOT_SCADA
            // 
            WK_IOT_SCADA.WorkerSupportsCancellation = true;
            WK_IOT_SCADA.DoWork += WK_IOT_SCADA_DoWork;
            // 
            // FExtention
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(874, 679);
            Controls.Add(tab2);
            Name = "FExtention";
            Symbol = 559515;
            Text = "Chức Năng";
            Initialize += FExtention_Initialize;
            tab2.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            uiTitlePanel7.ResumeLayout(false);
            uiTitlePanel3.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opData).EndInit();
            tabPage3.ResumeLayout(false);
            uiTableLayoutPanel6.ResumeLayout(false);
            uiTitlePanel4.ResumeLayout(false);
            uiTitlePanel5.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl tab2;
        private TabPage tabPage1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnERPCheck;
        private Sunny.UI.UIDataGridView opData;
        private Sunny.UI.UIListBox opConsole;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private TTManager.Masan.ERP_Google erP_Google1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private TabPage tabPage3;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel6;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UITitlePanel uiTitlePanel7;
        private Sunny.UI.UISymbolLabel opLastTimeUpload;
        private Sunny.UI.UISymbolLabel uiSymbolLabel3;
        private Sunny.UI.UITitlePanel uiTitlePanel3;
        private Sunny.UI.UISymbolLabel opLastUploadFileName;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UISymbolLabel opC1;
        private Sunny.UI.UISymbolLabel opNextUploadTime;
        private Sunny.UI.UISymbolButton btnCloudHis;
        private Sunny.UI.UITitlePanel uiTitlePanel4;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel7;
        private Sunny.UI.UITitlePanel uiTitlePanel5;
        private Sunny.UI.UIListBox uiListBox1;
        private System.ComponentModel.BackgroundWorker WK_IOT_SCADA;
    }
}