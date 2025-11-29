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
            DataGridViewCellStyle dataGridViewCellStyle31 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle32 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle33 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle34 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle35 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle26 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle27 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle28 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle29 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle30 = new DataGridViewCellStyle();
            uiTabControl1 = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiSymbolButton2 = new Sunny.UI.UISymbolButton();
            uiSymbolButton1 = new Sunny.UI.UISymbolButton();
            btnERPCheck = new Sunny.UI.UISymbolButton();
            opData = new Sunny.UI.UIDataGridView();
            opConsole = new Sunny.UI.UIListBox();
            tabPage2 = new TabPage();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            erP_Google1 = new TTManager.Masan.ERP_Google(components);
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            uiTitlePanel3 = new Sunny.UI.UITitlePanel();
            uiTitlePanel6 = new Sunny.UI.UITitlePanel();
            uiTitlePanel7 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            uiSymbolLabel1 = new Sunny.UI.UISymbolLabel();
            uiSymbolLabel2 = new Sunny.UI.UISymbolLabel();
            uiSymbolLabel3 = new Sunny.UI.UISymbolLabel();
            uiSymbolLabel4 = new Sunny.UI.UISymbolLabel();
            uiSymbolLabel5 = new Sunny.UI.UISymbolLabel();
            uiDataGridView1 = new Sunny.UI.UIDataGridView();
            uiTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)opData).BeginInit();
            tabPage2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            uiTitlePanel3.SuspendLayout();
            uiTitlePanel6.SuspendLayout();
            uiTitlePanel7.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)uiDataGridView1).BeginInit();
            SuspendLayout();
            // 
            // uiTabControl1
            // 
            uiTabControl1.Controls.Add(tabPage1);
            uiTabControl1.Controls.Add(tabPage2);
            uiTabControl1.Dock = DockStyle.Fill;
            uiTabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            uiTabControl1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTabControl1.ItemSize = new Size(150, 40);
            uiTabControl1.Location = new Point(0, 0);
            uiTabControl1.MainPage = "";
            uiTabControl1.Name = "uiTabControl1";
            uiTabControl1.SelectedIndex = 0;
            uiTabControl1.Size = new Size(874, 679);
            uiTabControl1.SizeMode = TabSizeMode.Fixed;
            uiTabControl1.TabIndex = 0;
            uiTabControl1.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            uiTabControl1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(uiTableLayoutPanel1);
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(874, 639);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Dữ liệu và ERP";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiPanel1, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel1, 0, 3);
            uiTableLayoutPanel1.Controls.Add(opData, 0, 1);
            uiTableLayoutPanel1.Controls.Add(opConsole, 0, 2);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Margin = new Padding(2);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 4;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 122F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 87F));
            uiTableLayoutPanel1.Size = new Size(874, 639);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiPanel1
            // 
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel1.Location = new Point(2, 2);
            uiPanel1.Margin = new Padding(2);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.Size = new Size(870, 50);
            uiPanel1.TabIndex = 0;
            uiPanel1.Text = "Trang này cung cấp cho vận hành hệ thống các công cụ quản lý hệ cơ sở dữ liệu và liên lạc với ERP";
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel2);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 554);
            uiTitlePanel1.Margin = new Padding(2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(870, 83);
            uiTitlePanel1.TabIndex = 1;
            uiTitlePanel1.Text = "Chức Năng";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 3;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 558F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Controls.Add(uiSymbolButton2, 1, 0);
            uiTableLayoutPanel2.Controls.Add(uiSymbolButton1, 0, 0);
            uiTableLayoutPanel2.Controls.Add(btnERPCheck, 2, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(1, 35);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(868, 47);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiSymbolButton2
            // 
            uiSymbolButton2.Dock = DockStyle.Fill;
            uiSymbolButton2.FillColor = Color.FromArgb(0, 192, 192);
            uiSymbolButton2.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton2.Location = new Point(560, 2);
            uiSymbolButton2.Margin = new Padding(2);
            uiSymbolButton2.MinimumSize = new Size(1, 1);
            uiSymbolButton2.Name = "uiSymbolButton2";
            uiSymbolButton2.Size = new Size(151, 43);
            uiSymbolButton2.TabIndex = 2;
            uiSymbolButton2.Text = "Lịch sử tải lên";
            uiSymbolButton2.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // uiSymbolButton1
            // 
            uiSymbolButton1.Dock = DockStyle.Fill;
            uiSymbolButton1.FillColor = Color.FromArgb(0, 192, 192);
            uiSymbolButton1.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton1.Location = new Point(2, 2);
            uiSymbolButton1.Margin = new Padding(2);
            uiSymbolButton1.MinimumSize = new Size(1, 1);
            uiSymbolButton1.Name = "uiSymbolButton1";
            uiSymbolButton1.Size = new Size(554, 43);
            uiSymbolButton1.TabIndex = 1;
            uiSymbolButton1.Text = "Trạng thái OPC UA";
            uiSymbolButton1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // btnERPCheck
            // 
            btnERPCheck.Dock = DockStyle.Fill;
            btnERPCheck.FillColor = Color.FromArgb(0, 192, 192);
            btnERPCheck.Font = new Font("Microsoft Sans Serif", 12F);
            btnERPCheck.Location = new Point(715, 2);
            btnERPCheck.Margin = new Padding(2);
            btnERPCheck.MinimumSize = new Size(1, 1);
            btnERPCheck.Name = "btnERPCheck";
            btnERPCheck.Size = new Size(151, 43);
            btnERPCheck.TabIndex = 0;
            btnERPCheck.Text = "Kiểm tra ERP";
            btnERPCheck.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnERPCheck.Click += btnERPCheck_Click;
            // 
            // opData
            // 
            opData.AllowUserToAddRows = false;
            opData.AllowUserToDeleteRows = false;
            dataGridViewCellStyle31.BackColor = Color.FromArgb(235, 243, 255);
            opData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle31;
            opData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            opData.BackgroundColor = Color.White;
            opData.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle32.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle32.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle32.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle32.ForeColor = Color.White;
            dataGridViewCellStyle32.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle32.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle32.WrapMode = DataGridViewTriState.True;
            opData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle32;
            opData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle33.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle33.BackColor = SystemColors.Window;
            dataGridViewCellStyle33.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle33.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle33.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle33.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle33.WrapMode = DataGridViewTriState.False;
            opData.DefaultCellStyle = dataGridViewCellStyle33;
            opData.Dock = DockStyle.Fill;
            opData.EnableHeadersVisualStyles = false;
            opData.Font = new Font("Microsoft Sans Serif", 12F);
            opData.GridColor = Color.FromArgb(80, 160, 255);
            opData.Location = new Point(3, 57);
            opData.Name = "opData";
            dataGridViewCellStyle34.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle34.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle34.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle34.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle34.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle34.SelectionForeColor = Color.White;
            dataGridViewCellStyle34.WrapMode = DataGridViewTriState.True;
            opData.RowHeadersDefaultCellStyle = dataGridViewCellStyle34;
            dataGridViewCellStyle35.BackColor = Color.White;
            dataGridViewCellStyle35.Font = new Font("Microsoft Sans Serif", 12F);
            opData.RowsDefaultCellStyle = dataGridViewCellStyle35;
            opData.SelectedIndex = -1;
            opData.Size = new Size(868, 370);
            opData.StripeOddColor = Color.FromArgb(235, 243, 255);
            opData.TabIndex = 2;
            // 
            // opConsole
            // 
            opConsole.Dock = DockStyle.Fill;
            opConsole.Font = new Font("Microsoft Sans Serif", 12F);
            opConsole.HoverColor = Color.FromArgb(155, 200, 255);
            opConsole.ItemSelectForeColor = Color.White;
            opConsole.Location = new Point(2, 432);
            opConsole.Margin = new Padding(2);
            opConsole.MinimumSize = new Size(1, 1);
            opConsole.Name = "opConsole";
            opConsole.Padding = new Padding(2);
            opConsole.ShowText = false;
            opConsole.Size = new Size(870, 118);
            opConsole.TabIndex = 3;
            opConsole.Text = "uiListBox1";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(uiTableLayoutPanel3);
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(874, 639);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Dữ liệu đám mây";
            tabPage2.UseVisualStyleBackColor = true;
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
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 1;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel3.Controls.Add(uiTitlePanel6, 0, 1);
            uiTableLayoutPanel3.Controls.Add(uiTableLayoutPanel4, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(0, 0);
            uiTableLayoutPanel3.Margin = new Padding(2);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 2;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 16.27543F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 83.72457F));
            uiTableLayoutPanel3.Size = new Size(874, 639);
            uiTableLayoutPanel3.TabIndex = 0;
            uiTableLayoutPanel3.TagString = null;
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
            uiTableLayoutPanel4.Size = new Size(870, 100);
            uiTableLayoutPanel4.TabIndex = 0;
            uiTableLayoutPanel4.TagString = null;
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
            uiTitlePanel2.Size = new Size(314, 96);
            uiTitlePanel2.TabIndex = 0;
            uiTitlePanel2.Text = "Thời gian chuẩn bị tải lên";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel3
            // 
            uiTitlePanel3.Controls.Add(uiSymbolLabel5);
            uiTitlePanel3.Dock = DockStyle.Fill;
            uiTitlePanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel3.Location = new Point(562, 2);
            uiTitlePanel3.Margin = new Padding(2);
            uiTitlePanel3.MinimumSize = new Size(1, 1);
            uiTitlePanel3.Name = "uiTitlePanel3";
            uiTitlePanel3.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel3.ShowText = false;
            uiTitlePanel3.Size = new Size(306, 96);
            uiTitlePanel3.TabIndex = 1;
            uiTitlePanel3.Text = "Tên tệp vừa tải lên";
            uiTitlePanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel6
            // 
            uiTitlePanel6.Controls.Add(uiDataGridView1);
            uiTitlePanel6.Dock = DockStyle.Fill;
            uiTitlePanel6.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel6.Location = new Point(2, 106);
            uiTitlePanel6.Margin = new Padding(2);
            uiTitlePanel6.MinimumSize = new Size(1, 1);
            uiTitlePanel6.Name = "uiTitlePanel6";
            uiTitlePanel6.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel6.ShowText = false;
            uiTitlePanel6.Size = new Size(870, 531);
            uiTitlePanel6.TabIndex = 3;
            uiTitlePanel6.Text = "Lịch sử 100 lần tải lên gần nhất";
            uiTitlePanel6.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel7
            // 
            uiTitlePanel7.Controls.Add(uiSymbolLabel4);
            uiTitlePanel7.Controls.Add(uiSymbolLabel3);
            uiTitlePanel7.Dock = DockStyle.Fill;
            uiTitlePanel7.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel7.Location = new Point(320, 2);
            uiTitlePanel7.Margin = new Padding(2);
            uiTitlePanel7.MinimumSize = new Size(1, 1);
            uiTitlePanel7.Name = "uiTitlePanel7";
            uiTitlePanel7.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel7.ShowText = false;
            uiTitlePanel7.Size = new Size(238, 96);
            uiTitlePanel7.TabIndex = 4;
            uiTitlePanel7.Text = "Thời gian vừa tải lên";
            uiTitlePanel7.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72.82609F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.173914F));
            uiTableLayoutPanel5.Controls.Add(uiSymbolLabel2, 1, 0);
            uiTableLayoutPanel5.Controls.Add(uiSymbolLabel1, 0, 0);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(1, 35);
            uiTableLayoutPanel5.Margin = new Padding(2);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 1;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.Size = new Size(312, 60);
            uiTableLayoutPanel5.TabIndex = 0;
            uiTableLayoutPanel5.TagString = null;
            // 
            // uiSymbolLabel1
            // 
            uiSymbolLabel1.Dock = DockStyle.Fill;
            uiSymbolLabel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel1.Location = new Point(3, 3);
            uiSymbolLabel1.MinimumSize = new Size(1, 1);
            uiSymbolLabel1.Name = "uiSymbolLabel1";
            uiSymbolLabel1.Size = new Size(221, 54);
            uiSymbolLabel1.TabIndex = 0;
            uiSymbolLabel1.Text = "2025-11-29T23:23:23.999";
            // 
            // uiSymbolLabel2
            // 
            uiSymbolLabel2.Dock = DockStyle.Fill;
            uiSymbolLabel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel2.Location = new Point(230, 3);
            uiSymbolLabel2.MinimumSize = new Size(1, 1);
            uiSymbolLabel2.Name = "uiSymbolLabel2";
            uiSymbolLabel2.Size = new Size(79, 54);
            uiSymbolLabel2.TabIndex = 1;
            uiSymbolLabel2.Text = "300";
            // 
            // uiSymbolLabel3
            // 
            uiSymbolLabel3.Dock = DockStyle.Fill;
            uiSymbolLabel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel3.Location = new Point(1, 35);
            uiSymbolLabel3.MinimumSize = new Size(1, 1);
            uiSymbolLabel3.Name = "uiSymbolLabel3";
            uiSymbolLabel3.Size = new Size(236, 60);
            uiSymbolLabel3.TabIndex = 1;
            uiSymbolLabel3.Text = "2025-11-29 23:23:23.999";
            // 
            // uiSymbolLabel4
            // 
            uiSymbolLabel4.Dock = DockStyle.Fill;
            uiSymbolLabel4.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel4.Location = new Point(1, 35);
            uiSymbolLabel4.MinimumSize = new Size(1, 1);
            uiSymbolLabel4.Name = "uiSymbolLabel4";
            uiSymbolLabel4.Size = new Size(236, 60);
            uiSymbolLabel4.TabIndex = 2;
            uiSymbolLabel4.Text = "2025-11-29 23:23:23.999";
            // 
            // uiSymbolLabel5
            // 
            uiSymbolLabel5.Dock = DockStyle.Fill;
            uiSymbolLabel5.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel5.Location = new Point(1, 35);
            uiSymbolLabel5.MinimumSize = new Size(1, 1);
            uiSymbolLabel5.Name = "uiSymbolLabel5";
            uiSymbolLabel5.Size = new Size(304, 60);
            uiSymbolLabel5.TabIndex = 2;
            uiSymbolLabel5.Text = "Line 3_";
            // 
            // uiDataGridView1
            // 
            dataGridViewCellStyle26.BackColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle26;
            uiDataGridView1.BackgroundColor = Color.White;
            uiDataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle27.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle27.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle27.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle27.ForeColor = Color.White;
            dataGridViewCellStyle27.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle27.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle27.WrapMode = DataGridViewTriState.True;
            uiDataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle27;
            uiDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle28.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle28.BackColor = SystemColors.Window;
            dataGridViewCellStyle28.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle28.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle28.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle28.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle28.WrapMode = DataGridViewTriState.False;
            uiDataGridView1.DefaultCellStyle = dataGridViewCellStyle28;
            uiDataGridView1.Dock = DockStyle.Fill;
            uiDataGridView1.EnableHeadersVisualStyles = false;
            uiDataGridView1.Font = new Font("Microsoft Sans Serif", 12F);
            uiDataGridView1.GridColor = Color.FromArgb(80, 160, 255);
            uiDataGridView1.Location = new Point(1, 35);
            uiDataGridView1.Name = "uiDataGridView1";
            dataGridViewCellStyle29.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle29.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle29.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle29.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle29.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle29.SelectionForeColor = Color.White;
            dataGridViewCellStyle29.WrapMode = DataGridViewTriState.True;
            uiDataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle29;
            dataGridViewCellStyle30.BackColor = Color.White;
            dataGridViewCellStyle30.Font = new Font("Microsoft Sans Serif", 12F);
            uiDataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle30;
            uiDataGridView1.SelectedIndex = -1;
            uiDataGridView1.Size = new Size(868, 495);
            uiDataGridView1.StripeOddColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.TabIndex = 0;
            // 
            // FExtention
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(874, 679);
            Controls.Add(uiTabControl1);
            Name = "FExtention";
            Text = "FExtention";
            Initialize += FExtention_Initialize;
            uiTabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)opData).EndInit();
            tabPage2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            uiTitlePanel3.ResumeLayout(false);
            uiTitlePanel6.ResumeLayout(false);
            uiTitlePanel7.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)uiDataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl uiTabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnERPCheck;
        private Sunny.UI.UIDataGridView opData;
        private Sunny.UI.UIListBox opConsole;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Sunny.UI.UISymbolButton uiSymbolButton2;
        private Sunny.UI.UISymbolButton uiSymbolButton1;
        private TTManager.Masan.ERP_Google erP_Google1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UITitlePanel uiTitlePanel6;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UITitlePanel uiTitlePanel3;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Sunny.UI.UITitlePanel uiTitlePanel7;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UISymbolLabel uiSymbolLabel3;
        private Sunny.UI.UISymbolLabel uiSymbolLabel2;
        private Sunny.UI.UISymbolLabel uiSymbolLabel1;
        private Sunny.UI.UIDataGridView uiDataGridView1;
        private Sunny.UI.UISymbolLabel uiSymbolLabel4;
        private Sunny.UI.UISymbolLabel uiSymbolLabel5;
    }
}