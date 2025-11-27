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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            uiTabControl1 = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            btnERPCheck = new Sunny.UI.UISymbolButton();
            uiDataGridView1 = new Sunny.UI.UIDataGridView();
            uiListBox1 = new Sunny.UI.UIListBox();
            tabPage2 = new TabPage();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            uiSymbolButton1 = new Sunny.UI.UISymbolButton();
            uiSymbolButton2 = new Sunny.UI.UISymbolButton();
            uiTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
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
            uiTableLayoutPanel1.Controls.Add(uiDataGridView1, 0, 1);
            uiTableLayoutPanel1.Controls.Add(uiListBox1, 0, 2);
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
            // uiDataGridView1
            // 
            uiDataGridView1.AllowUserToAddRows = false;
            uiDataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            uiDataGridView1.BackgroundColor = Color.White;
            uiDataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle2.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            uiDataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            uiDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Window;
            dataGridViewCellStyle3.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            uiDataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            uiDataGridView1.Dock = DockStyle.Fill;
            uiDataGridView1.EnableHeadersVisualStyles = false;
            uiDataGridView1.Font = new Font("Microsoft Sans Serif", 12F);
            uiDataGridView1.GridColor = Color.FromArgb(80, 160, 255);
            uiDataGridView1.Location = new Point(3, 57);
            uiDataGridView1.Name = "uiDataGridView1";
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(235, 243, 255);
            dataGridViewCellStyle4.Font = new Font("Microsoft Sans Serif", 12F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(48, 48, 48);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(80, 160, 255);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            uiDataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = Color.White;
            dataGridViewCellStyle5.Font = new Font("Microsoft Sans Serif", 12F);
            uiDataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle5;
            uiDataGridView1.SelectedIndex = -1;
            uiDataGridView1.Size = new Size(868, 370);
            uiDataGridView1.StripeOddColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.TabIndex = 2;
            // 
            // uiListBox1
            // 
            uiListBox1.Dock = DockStyle.Fill;
            uiListBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiListBox1.HoverColor = Color.FromArgb(155, 200, 255);
            uiListBox1.ItemSelectForeColor = Color.White;
            uiListBox1.Location = new Point(2, 432);
            uiListBox1.Margin = new Padding(2);
            uiListBox1.MinimumSize = new Size(1, 1);
            uiListBox1.Name = "uiListBox1";
            uiListBox1.Padding = new Padding(2);
            uiListBox1.ShowText = false;
            uiListBox1.Size = new Size(870, 118);
            uiListBox1.TabIndex = 3;
            uiListBox1.Text = "uiListBox1";
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(200, 60);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Kiểm tra tải lên";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
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
            // FExtention
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(874, 679);
            Controls.Add(uiTabControl1);
            Name = "FExtention";
            Text = "FExtention";
            uiTabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
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
        private Sunny.UI.UIDataGridView uiDataGridView1;
        private Sunny.UI.UIListBox uiListBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Sunny.UI.UISymbolButton uiSymbolButton2;
        private Sunny.UI.UISymbolButton uiSymbolButton1;
    }
}