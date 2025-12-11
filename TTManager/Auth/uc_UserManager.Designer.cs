namespace TTManager.Auth
{
    partial class uc_UserManager
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

        #region Component Designer generated code

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
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiDataGridView1 = new Sunny.UI.UIDataGridView();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            btnLoad = new Sunny.UI.UISymbolButton();
            btnAddUser = new Sunny.UI.UISymbolButton();
            btnExportCsv = new Sunny.UI.UISymbolButton();
            uiSymbolButton1 = new Sunny.UI.UISymbolButton();
            ipTwoFA = new Sunny.UI.UINumPadTextBox();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            ipPassword = new Sunny.UI.UITextBox();
            uiSymbolLabel1 = new Sunny.UI.UISymbolLabel();
            uiTitlePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)uiDataGridView1).BeginInit();
            uiTableLayoutPanel1.SuspendLayout();
            uiPanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiPanel2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiDataGridView1);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 2);
            uiTitlePanel1.Margin = new Padding(2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(385, 244);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Quản Lý Tài Khoản";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiDataGridView1
            // 
            uiDataGridView1.AllowUserToAddRows = false;
            uiDataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            uiDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            uiDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
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
            uiDataGridView1.Location = new Point(1, 35);
            uiDataGridView1.Margin = new Padding(2);
            uiDataGridView1.Name = "uiDataGridView1";
            uiDataGridView1.ReadOnly = true;
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
            uiDataGridView1.Size = new Size(383, 208);
            uiDataGridView1.StripeOddColor = Color.FromArgb(235, 243, 255);
            uiDataGridView1.TabIndex = 0;
            uiDataGridView1.CellClick += uiDataGridView1_CellClick;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel1, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiPanel1, 0, 2);
            uiTableLayoutPanel1.Controls.Add(uiPanel2, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 3;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 81.0996552F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 18.9003429F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            uiTableLayoutPanel1.Size = new Size(389, 362);
            uiTableLayoutPanel1.TabIndex = 1;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiPanel1
            // 
            uiPanel1.Controls.Add(uiTableLayoutPanel2);
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel1.Location = new Point(2, 307);
            uiPanel1.Margin = new Padding(2);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.RectColor = Color.DodgerBlue;
            uiPanel1.RectSize = 2;
            uiPanel1.Size = new Size(385, 53);
            uiPanel1.TabIndex = 1;
            uiPanel1.Text = null;
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.BackColor = Color.Transparent;
            uiTableLayoutPanel2.ColumnCount = 5;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13.24201F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24.6753254F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.5714283F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.43836F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.59056F));
            uiTableLayoutPanel2.Controls.Add(btnLoad, 4, 0);
            uiTableLayoutPanel2.Controls.Add(btnAddUser, 3, 0);
            uiTableLayoutPanel2.Controls.Add(btnExportCsv, 0, 0);
            uiTableLayoutPanel2.Controls.Add(uiSymbolButton1, 1, 0);
            uiTableLayoutPanel2.Controls.Add(ipTwoFA, 2, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(0, 0);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(385, 53);
            uiTableLayoutPanel2.TabIndex = 3;
            uiTableLayoutPanel2.TagString = null;
            // 
            // btnLoad
            // 
            btnLoad.Cursor = Cursors.Hand;
            btnLoad.Dock = DockStyle.Fill;
            btnLoad.FillColor = Color.WhiteSmoke;
            btnLoad.Font = new Font("Microsoft Sans Serif", 12F);
            btnLoad.Location = new Point(321, 2);
            btnLoad.Margin = new Padding(2);
            btnLoad.MinimumSize = new Size(1, 1);
            btnLoad.Name = "btnLoad";
            btnLoad.Radius = 10;
            btnLoad.RectColor = Color.Blue;
            btnLoad.RectSize = 2;
            btnLoad.Size = new Size(62, 49);
            btnLoad.Symbol = 61473;
            btnLoad.SymbolColor = Color.FromArgb(0, 192, 192);
            btnLoad.SymbolSize = 25;
            btnLoad.TabIndex = 15;
            btnLoad.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnLoad.Click += btnLoad_Click_1;
            // 
            // btnAddUser
            // 
            btnAddUser.Cursor = Cursors.Hand;
            btnAddUser.Dock = DockStyle.Fill;
            btnAddUser.FillColor = Color.WhiteSmoke;
            btnAddUser.Font = new Font("Microsoft Sans Serif", 12F);
            btnAddUser.Location = new Point(258, 2);
            btnAddUser.Margin = new Padding(2);
            btnAddUser.MinimumSize = new Size(1, 1);
            btnAddUser.Name = "btnAddUser";
            btnAddUser.Radius = 10;
            btnAddUser.RectColor = Color.Blue;
            btnAddUser.RectSize = 2;
            btnAddUser.Size = new Size(59, 49);
            btnAddUser.Symbol = 62004;
            btnAddUser.SymbolColor = Color.FromArgb(0, 192, 0);
            btnAddUser.SymbolSize = 25;
            btnAddUser.TabIndex = 14;
            btnAddUser.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnAddUser.Click += btnAddUser_Click;
            // 
            // btnExportCsv
            // 
            btnExportCsv.Cursor = Cursors.Hand;
            btnExportCsv.Dock = DockStyle.Fill;
            btnExportCsv.FillColor = Color.WhiteSmoke;
            btnExportCsv.Font = new Font("Microsoft Sans Serif", 12F);
            btnExportCsv.Location = new Point(2, 2);
            btnExportCsv.Margin = new Padding(2);
            btnExportCsv.MinimumSize = new Size(1, 1);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Radius = 10;
            btnExportCsv.RectColor = Color.Blue;
            btnExportCsv.RectSize = 2;
            btnExportCsv.Size = new Size(47, 49);
            btnExportCsv.Symbol = 363197;
            btnExportCsv.SymbolColor = Color.Green;
            btnExportCsv.SymbolSize = 25;
            btnExportCsv.TabIndex = 12;
            btnExportCsv.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // uiSymbolButton1
            // 
            uiSymbolButton1.Dock = DockStyle.Fill;
            uiSymbolButton1.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton1.Location = new Point(53, 0);
            uiSymbolButton1.Margin = new Padding(2, 0, 2, 0);
            uiSymbolButton1.MinimumSize = new Size(1, 1);
            uiSymbolButton1.Name = "uiSymbolButton1";
            uiSymbolButton1.Radius = 10;
            uiSymbolButton1.Size = new Size(91, 53);
            uiSymbolButton1.Symbol = 57454;
            uiSymbolButton1.TabIndex = 17;
            uiSymbolButton1.Text = "Mã OTP";
            uiSymbolButton1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // ipTwoFA
            // 
            ipTwoFA.Dock = DockStyle.Fill;
            ipTwoFA.FillColor = Color.White;
            ipTwoFA.Font = new Font("Microsoft Sans Serif", 12F);
            ipTwoFA.Location = new Point(148, 2);
            ipTwoFA.Margin = new Padding(2);
            ipTwoFA.MinimumSize = new Size(63, 0);
            ipTwoFA.Name = "ipTwoFA";
            ipTwoFA.Padding = new Padding(0, 0, 30, 2);
            ipTwoFA.Size = new Size(106, 49);
            ipTwoFA.SymbolSize = 24;
            ipTwoFA.TabIndex = 18;
            ipTwoFA.Text = "111111";
            ipTwoFA.TextAlignment = ContentAlignment.MiddleLeft;
            ipTwoFA.Watermark = "";
            // 
            // uiPanel2
            // 
            uiPanel2.Controls.Add(uiTableLayoutPanel3);
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel2.Location = new Point(4, 253);
            uiPanel2.Margin = new Padding(4, 5, 4, 5);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.Size = new Size(381, 47);
            uiPanel2.TabIndex = 2;
            uiPanel2.Text = null;
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.BackColor = Color.Transparent;
            uiTableLayoutPanel3.ColumnCount = 2;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.8713913F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.12861F));
            uiTableLayoutPanel3.Controls.Add(ipPassword, 1, 0);
            uiTableLayoutPanel3.Controls.Add(uiSymbolLabel1, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(0, 0);
            uiTableLayoutPanel3.Margin = new Padding(0);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 1;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel3.Size = new Size(381, 47);
            uiTableLayoutPanel3.TabIndex = 0;
            uiTableLayoutPanel3.TagString = null;
            // 
            // ipPassword
            // 
            ipPassword.Dock = DockStyle.Fill;
            ipPassword.Font = new Font("Microsoft Sans Serif", 12F);
            ipPassword.Location = new Point(112, 2);
            ipPassword.Margin = new Padding(2);
            ipPassword.MinimumSize = new Size(1, 16);
            ipPassword.Name = "ipPassword";
            ipPassword.Padding = new Padding(5);
            ipPassword.PasswordChar = '*';
            ipPassword.ShowText = false;
            ipPassword.Size = new Size(267, 43);
            ipPassword.TabIndex = 0;
            ipPassword.Text = "uiTextBox1";
            ipPassword.TextAlignment = ContentAlignment.MiddleLeft;
            ipPassword.Watermark = "";
            ipPassword.DoubleClick += ipPassword_DoubleClick;
            // 
            // uiSymbolLabel1
            // 
            uiSymbolLabel1.Dock = DockStyle.Fill;
            uiSymbolLabel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolLabel1.Location = new Point(3, 3);
            uiSymbolLabel1.MinimumSize = new Size(1, 1);
            uiSymbolLabel1.Name = "uiSymbolLabel1";
            uiSymbolLabel1.Size = new Size(104, 41);
            uiSymbolLabel1.TabIndex = 1;
            uiSymbolLabel1.Text = "Mật khẩu";
            // 
            // uc_UserManager
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(uiTableLayoutPanel1);
            Name = "uc_UserManager";
            Size = new Size(389, 362);
            uiTitlePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)uiDataGridView1).EndInit();
            uiTableLayoutPanel1.ResumeLayout(false);
            uiPanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiPanel2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UIDataGridView uiDataGridView1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnLoad;
        private Sunny.UI.UISymbolButton btnAddUser;
        private Sunny.UI.UISymbolButton btnExportCsv;
        private Sunny.UI.UISymbolButton btnExportPDF;
        private Sunny.UI.UINumPadTextBox uiNumPadTextBox1;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UITextBox ipPassword;
        private Sunny.UI.UISymbolLabel uiSymbolLabel1;
        private Sunny.UI.UISymbolButton uiSymbolButton1;
        private Sunny.UI.UINumPadTextBox ipTwoFA;
    }
}
