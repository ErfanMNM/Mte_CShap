namespace TTManager.Auth
{
    partial class ucLogin
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
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiSymbolLabel1 = new Sunny.UI.UISymbolLabel();
            ipPassword = new Sunny.UI.UITextBox();
            uiSymbolLabel2 = new Sunny.UI.UISymbolLabel();
            ipUserName = new Sunny.UI.UIComboBox();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            btnLogin = new Sunny.UI.UISymbolButton();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            uiSymbolLabel3 = new Sunny.UI.UISymbolLabel();
            ipTwoFA = new Sunny.UI.UINumPadTextBox();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel1);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(0, 0);
            uiTitlePanel1.Margin = new Padding(4, 5, 4, 5);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 50, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(505, 248);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Đăng Nhập";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel1.TitleHeight = 50;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.BackColor = Color.Transparent;
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel3, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(1, 50);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 71F));
            uiTableLayoutPanel1.Size = new Size(503, 197);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.BackColor = Color.Azure;
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.88147F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.11853F));
            uiTableLayoutPanel2.Controls.Add(uiSymbolLabel1, 0, 0);
            uiTableLayoutPanel2.Controls.Add(ipPassword, 1, 1);
            uiTableLayoutPanel2.Controls.Add(uiSymbolLabel2, 0, 1);
            uiTableLayoutPanel2.Controls.Add(ipUserName, 1, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(2, 2);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 2;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Size = new Size(499, 122);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiSymbolLabel1
            // 
            uiSymbolLabel1.BackColor = Color.FromArgb(0, 192, 192);
            uiSymbolLabel1.Dock = DockStyle.Fill;
            uiSymbolLabel1.Font = new Font("Microsoft Sans Serif", 14.25F);
            uiSymbolLabel1.Location = new Point(3, 3);
            uiSymbolLabel1.MinimumSize = new Size(1, 1);
            uiSymbolLabel1.Name = "uiSymbolLabel1";
            uiSymbolLabel1.RectSize = 2;
            uiSymbolLabel1.Size = new Size(138, 55);
            uiSymbolLabel1.Symbol = 62142;
            uiSymbolLabel1.SymbolSize = 40;
            uiSymbolLabel1.TabIndex = 0;
            uiSymbolLabel1.Text = "Tài khoản";
            // 
            // ipPassword
            // 
            ipPassword.Dock = DockStyle.Fill;
            ipPassword.Font = new Font("Microsoft Sans Serif", 12F);
            ipPassword.Location = new Point(146, 63);
            ipPassword.Margin = new Padding(2);
            ipPassword.MinimumSize = new Size(1, 16);
            ipPassword.Name = "ipPassword";
            ipPassword.Padding = new Padding(5);
            ipPassword.PasswordChar = '*';
            ipPassword.ShowText = false;
            ipPassword.Size = new Size(351, 57);
            ipPassword.TabIndex = 1;
            ipPassword.TextAlignment = ContentAlignment.MiddleLeft;
            ipPassword.Watermark = "";
            ipPassword.DoubleClick += ipPassword_DoubleClick;
            // 
            // uiSymbolLabel2
            // 
            uiSymbolLabel2.BackColor = Color.FromArgb(0, 192, 192);
            uiSymbolLabel2.Dock = DockStyle.Fill;
            uiSymbolLabel2.Font = new Font("Microsoft Sans Serif", 14.25F);
            uiSymbolLabel2.Location = new Point(3, 64);
            uiSymbolLabel2.MinimumSize = new Size(1, 1);
            uiSymbolLabel2.Name = "uiSymbolLabel2";
            uiSymbolLabel2.Radius = 20;
            uiSymbolLabel2.RectSize = 2;
            uiSymbolLabel2.Size = new Size(138, 55);
            uiSymbolLabel2.Symbol = 361475;
            uiSymbolLabel2.SymbolSize = 40;
            uiSymbolLabel2.TabIndex = 0;
            uiSymbolLabel2.Text = "Mật khẩu";
            // 
            // ipUserName
            // 
            ipUserName.DataSource = null;
            ipUserName.Dock = DockStyle.Fill;
            ipUserName.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            ipUserName.FillColor = Color.White;
            ipUserName.Font = new Font("Microsoft Sans Serif", 12F);
            ipUserName.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipUserName.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipUserName.Location = new Point(146, 2);
            ipUserName.Margin = new Padding(2);
            ipUserName.MinimumSize = new Size(63, 0);
            ipUserName.Name = "ipUserName";
            ipUserName.Padding = new Padding(0, 0, 30, 2);
            ipUserName.Size = new Size(351, 57);
            ipUserName.SymbolSize = 24;
            ipUserName.TabIndex = 1;
            ipUserName.TextAlignment = ContentAlignment.MiddleLeft;
            ipUserName.Watermark = "";
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.BackColor = Color.CadetBlue;
            uiTableLayoutPanel3.ColumnCount = 2;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68.2243F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31.7757015F));
            uiTableLayoutPanel3.Controls.Add(btnLogin, 1, 0);
            uiTableLayoutPanel3.Controls.Add(uiTableLayoutPanel4, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(3, 129);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 1;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel3.Size = new Size(497, 65);
            uiTableLayoutPanel3.TabIndex = 1;
            uiTableLayoutPanel3.TagString = null;
            // 
            // btnLogin
            // 
            btnLogin.Dock = DockStyle.Fill;
            btnLogin.FillColor = Color.FromArgb(0, 192, 0);
            btnLogin.Font = new Font("Microsoft Sans Serif", 12F);
            btnLogin.Location = new Point(341, 2);
            btnLogin.Margin = new Padding(2);
            btnLogin.MinimumSize = new Size(1, 1);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(154, 61);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "Đăng Nhập";
            btnLogin.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnLogin.Click += btnLogin_Click;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.ColumnCount = 2;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 41.8282547F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58.1717453F));
            uiTableLayoutPanel4.Controls.Add(uiSymbolLabel3, 0, 0);
            uiTableLayoutPanel4.Controls.Add(ipTwoFA, 1, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(2, 2);
            uiTableLayoutPanel4.Margin = new Padding(2);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 1;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel4.Size = new Size(335, 61);
            uiTableLayoutPanel4.TabIndex = 1;
            uiTableLayoutPanel4.TagString = null;
            // 
            // uiSymbolLabel3
            // 
            uiSymbolLabel3.BackColor = Color.FromArgb(0, 192, 192);
            uiSymbolLabel3.Dock = DockStyle.Fill;
            uiSymbolLabel3.Font = new Font("Microsoft Sans Serif", 14.25F);
            uiSymbolLabel3.ForeColor = Color.FromArgb(255, 255, 128);
            uiSymbolLabel3.IsCircle = true;
            uiSymbolLabel3.Location = new Point(3, 3);
            uiSymbolLabel3.MinimumSize = new Size(1, 1);
            uiSymbolLabel3.Name = "uiSymbolLabel3";
            uiSymbolLabel3.Radius = 10;
            uiSymbolLabel3.RectSize = 2;
            uiSymbolLabel3.Size = new Size(134, 55);
            uiSymbolLabel3.Symbol = 57454;
            uiSymbolLabel3.SymbolColor = Color.FromArgb(255, 255, 128);
            uiSymbolLabel3.SymbolSize = 30;
            uiSymbolLabel3.TabIndex = 1;
            uiSymbolLabel3.Click += uiSymbolLabel3_Click;
            // 
            // ipTwoFA
            // 
            ipTwoFA.Dock = DockStyle.Fill;
            ipTwoFA.FillColor = Color.White;
            ipTwoFA.Font = new Font("Microsoft Sans Serif", 12F);
            ipTwoFA.Location = new Point(142, 2);
            ipTwoFA.Margin = new Padding(2);
            ipTwoFA.Maximum = 999999D;
            ipTwoFA.MaxLength = 18;
            ipTwoFA.Minimum = 0D;
            ipTwoFA.MinimumSize = new Size(63, 0);
            ipTwoFA.Name = "ipTwoFA";
            ipTwoFA.NumPadType = Sunny.UI.NumPadType.IDNumber;
            ipTwoFA.Padding = new Padding(0, 0, 30, 2);
            ipTwoFA.Size = new Size(191, 57);
            ipTwoFA.SymbolNormal = 557532;
            ipTwoFA.SymbolSize = 24;
            ipTwoFA.TabIndex = 2;
            ipTwoFA.TextAlignment = ContentAlignment.MiddleLeft;
            ipTwoFA.Watermark = "";
            // 
            // ucLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(uiTitlePanel1);
            Name = "ucLogin";
            Size = new Size(505, 248);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolLabel uiSymbolLabel1;
        private Sunny.UI.UISymbolLabel uiSymbolLabel2;
        private Sunny.UI.UITextBox ipPassword;
        private Sunny.UI.UIComboBox ipUserName;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UISymbolButton btnLogin;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UISymbolLabel uiSymbolLabel3;
        private Sunny.UI.UINumPadTextBox ipTwoFA;
    }
}
