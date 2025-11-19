namespace TTManager.Auth
{
    partial class uc_UserSetting
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
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            btnSave = new Sunny.UI.UISymbolButton();
            opPanel2FA = new Sunny.UI.UIPanel();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            uiSymbolLabel3 = new Sunny.UI.UISymbolLabel();
            ipOTP = new Sunny.UI.UINumPadTextBox();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            ipComfirmNewPassword = new Sunny.UI.UITextBox();
            ipNewPassword = new Sunny.UI.UITextBox();
            ipOldPassword = new Sunny.UI.UITextBox();
            ipUserName = new Sunny.UI.UITextBox();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiPanel3 = new Sunny.UI.UIPanel();
            uiPanel4 = new Sunny.UI.UIPanel();
            uiPanel5 = new Sunny.UI.UIPanel();
            uiPanel6 = new Sunny.UI.UIPanel();
            opRole = new Sunny.UI.UITextBox();
            uiPanel7 = new Sunny.UI.UIPanel();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            opPanel2FA.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
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
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(492, 367);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Điều Chỉnh Thông Tin Tài Khoản";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.BackColor = Color.Transparent;
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel3, 0, 1);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 0);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(1, 35);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 78.57143F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 21.42857F));
            uiTableLayoutPanel1.Size = new Size(490, 331);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 2;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67.35537F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32.64463F));
            uiTableLayoutPanel3.Controls.Add(btnSave, 1, 0);
            uiTableLayoutPanel3.Controls.Add(opPanel2FA, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Font = new Font("Consolas", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            uiTableLayoutPanel3.Location = new Point(3, 263);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 1;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel3.Size = new Size(484, 65);
            uiTableLayoutPanel3.TabIndex = 7;
            uiTableLayoutPanel3.TagString = null;
            // 
            // btnSave
            // 
            btnSave.Cursor = Cursors.Hand;
            btnSave.Dock = DockStyle.Fill;
            btnSave.Font = new Font("Tahoma", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSave.Location = new Point(328, 3);
            btnSave.MinimumSize = new Size(1, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(153, 59);
            btnSave.Symbol = 61639;
            btnSave.SymbolSize = 29;
            btnSave.TabIndex = 5;
            btnSave.Text = "Lưu lại";
            btnSave.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSave.Click += btnSave_Click;
            // 
            // opPanel2FA
            // 
            opPanel2FA.Controls.Add(uiTableLayoutPanel5);
            opPanel2FA.Dock = DockStyle.Fill;
            opPanel2FA.Font = new Font("Microsoft Sans Serif", 12F);
            opPanel2FA.Location = new Point(4, 5);
            opPanel2FA.Margin = new Padding(4, 5, 4, 5);
            opPanel2FA.MinimumSize = new Size(1, 1);
            opPanel2FA.Name = "opPanel2FA";
            opPanel2FA.RectColor = Color.FromArgb(255, 128, 0);
            opPanel2FA.RectSize = 2;
            opPanel2FA.Size = new Size(317, 55);
            opPanel2FA.TabIndex = 6;
            opPanel2FA.Text = null;
            opPanel2FA.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.BackColor = Color.Transparent;
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52.3659325F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47.6340675F));
            uiTableLayoutPanel5.Controls.Add(uiSymbolLabel3, 0, 0);
            uiTableLayoutPanel5.Controls.Add(ipOTP, 1, 0);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(0, 0);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 1;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel5.Size = new Size(317, 55);
            uiTableLayoutPanel5.TabIndex = 13;
            uiTableLayoutPanel5.TagString = null;
            // 
            // uiSymbolLabel3
            // 
            uiSymbolLabel3.Dock = DockStyle.Fill;
            uiSymbolLabel3.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            uiSymbolLabel3.Location = new Point(3, 3);
            uiSymbolLabel3.MinimumSize = new Size(1, 1);
            uiSymbolLabel3.Name = "uiSymbolLabel3";
            uiSymbolLabel3.Size = new Size(160, 49);
            uiSymbolLabel3.Symbol = 57454;
            uiSymbolLabel3.SymbolColor = Color.FromArgb(0, 192, 192);
            uiSymbolLabel3.SymbolSize = 25;
            uiSymbolLabel3.TabIndex = 6;
            uiSymbolLabel3.Text = " Mã xác thực";
            // 
            // ipOTP
            // 
            ipOTP.DecimalPlaces = 0;
            ipOTP.Dock = DockStyle.Fill;
            ipOTP.FillColor = Color.White;
            ipOTP.Font = new Font("Microsoft Sans Serif", 12F);
            ipOTP.Location = new Point(170, 5);
            ipOTP.Margin = new Padding(4, 5, 4, 5);
            ipOTP.Maximum = 999999D;
            ipOTP.Minimum = 0D;
            ipOTP.MinimumSize = new Size(63, 0);
            ipOTP.Name = "ipOTP";
            ipOTP.NumPadType = Sunny.UI.NumPadType.Integer;
            ipOTP.Padding = new Padding(0, 0, 30, 2);
            ipOTP.Size = new Size(143, 45);
            ipOTP.SymbolDropDown = 557532;
            ipOTP.SymbolNormal = 557532;
            ipOTP.SymbolSize = 30;
            ipOTP.TabIndex = 7;
            ipOTP.Text = "111111";
            ipOTP.TextAlignment = ContentAlignment.MiddleLeft;
            ipOTP.Watermark = "";
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.BackColor = Color.Transparent;
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35.33058F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64.66942F));
            uiTableLayoutPanel2.Controls.Add(ipComfirmNewPassword, 1, 3);
            uiTableLayoutPanel2.Controls.Add(ipNewPassword, 1, 2);
            uiTableLayoutPanel2.Controls.Add(ipOldPassword, 1, 1);
            uiTableLayoutPanel2.Controls.Add(ipUserName, 1, 0);
            uiTableLayoutPanel2.Controls.Add(uiPanel2, 0, 1);
            uiTableLayoutPanel2.Controls.Add(uiPanel1, 0, 0);
            uiTableLayoutPanel2.Controls.Add(uiPanel3, 0, 2);
            uiTableLayoutPanel2.Controls.Add(uiPanel4, 0, 3);
            uiTableLayoutPanel2.Controls.Add(uiPanel5, 0, 4);
            uiTableLayoutPanel2.Controls.Add(uiPanel6, 0, 5);
            uiTableLayoutPanel2.Controls.Add(opRole, 1, 5);
            uiTableLayoutPanel2.Controls.Add(uiPanel7, 1, 4);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(3, 3);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 6;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66667F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66667F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66667F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66667F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66667F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66667F));
            uiTableLayoutPanel2.Size = new Size(484, 254);
            uiTableLayoutPanel2.TabIndex = 1;
            uiTableLayoutPanel2.TagString = null;
            // 
            // ipComfirmNewPassword
            // 
            ipComfirmNewPassword.Cursor = Cursors.IBeam;
            ipComfirmNewPassword.Dock = DockStyle.Fill;
            ipComfirmNewPassword.Font = new Font("Microsoft Sans Serif", 12F);
            ipComfirmNewPassword.Location = new Point(173, 128);
            ipComfirmNewPassword.Margin = new Padding(2);
            ipComfirmNewPassword.MinimumSize = new Size(1, 16);
            ipComfirmNewPassword.Name = "ipComfirmNewPassword";
            ipComfirmNewPassword.Padding = new Padding(5);
            ipComfirmNewPassword.PasswordChar = '*';
            ipComfirmNewPassword.ShowText = false;
            ipComfirmNewPassword.Size = new Size(309, 38);
            ipComfirmNewPassword.TabIndex = 10;
            ipComfirmNewPassword.Text = "uiTextBox6";
            ipComfirmNewPassword.TextAlignment = ContentAlignment.MiddleLeft;
            ipComfirmNewPassword.Watermark = "";
            ipComfirmNewPassword.DoubleClick += ipComfirmNewPassword_DoubleClick;
            // 
            // ipNewPassword
            // 
            ipNewPassword.Cursor = Cursors.IBeam;
            ipNewPassword.Dock = DockStyle.Fill;
            ipNewPassword.Font = new Font("Microsoft Sans Serif", 12F);
            ipNewPassword.Location = new Point(173, 86);
            ipNewPassword.Margin = new Padding(2);
            ipNewPassword.MinimumSize = new Size(1, 16);
            ipNewPassword.Name = "ipNewPassword";
            ipNewPassword.Padding = new Padding(5);
            ipNewPassword.PasswordChar = '*';
            ipNewPassword.ShowText = false;
            ipNewPassword.Size = new Size(309, 38);
            ipNewPassword.TabIndex = 9;
            ipNewPassword.Text = "uiTextBox5";
            ipNewPassword.TextAlignment = ContentAlignment.MiddleLeft;
            ipNewPassword.Watermark = "";
            ipNewPassword.DoubleClick += ipNewPassword_DoubleClick;
            // 
            // ipOldPassword
            // 
            ipOldPassword.Cursor = Cursors.IBeam;
            ipOldPassword.Dock = DockStyle.Fill;
            ipOldPassword.Font = new Font("Microsoft Sans Serif", 12F);
            ipOldPassword.Location = new Point(173, 44);
            ipOldPassword.Margin = new Padding(2);
            ipOldPassword.MinimumSize = new Size(1, 16);
            ipOldPassword.Name = "ipOldPassword";
            ipOldPassword.Padding = new Padding(5);
            ipOldPassword.PasswordChar = '*';
            ipOldPassword.ShowText = false;
            ipOldPassword.Size = new Size(309, 38);
            ipOldPassword.TabIndex = 8;
            ipOldPassword.Text = "uiTextBox4";
            ipOldPassword.TextAlignment = ContentAlignment.MiddleLeft;
            ipOldPassword.Watermark = "";
            ipOldPassword.DoubleClick += ipOldPassword_DoubleClick;
            // 
            // ipUserName
            // 
            ipUserName.Cursor = Cursors.IBeam;
            ipUserName.Dock = DockStyle.Fill;
            ipUserName.Font = new Font("Microsoft Sans Serif", 12F);
            ipUserName.Location = new Point(173, 2);
            ipUserName.Margin = new Padding(2);
            ipUserName.MinimumSize = new Size(1, 16);
            ipUserName.Name = "ipUserName";
            ipUserName.Padding = new Padding(5);
            ipUserName.ReadOnly = true;
            ipUserName.ShowText = false;
            ipUserName.Size = new Size(309, 38);
            ipUserName.TabIndex = 7;
            ipUserName.Text = "-";
            ipUserName.TextAlignment = ContentAlignment.MiddleLeft;
            ipUserName.Watermark = "";
            // 
            // uiPanel2
            // 
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel2.Location = new Point(2, 44);
            uiPanel2.Margin = new Padding(2);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.Size = new Size(167, 38);
            uiPanel2.TabIndex = 1;
            uiPanel2.Text = "Mật khẩu cũ";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel1
            // 
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel1.Location = new Point(2, 2);
            uiPanel1.Margin = new Padding(2);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.Radius = 2;
            uiPanel1.Size = new Size(167, 38);
            uiPanel1.TabIndex = 0;
            uiPanel1.Text = "Tên tài khoản";
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel3
            // 
            uiPanel3.Dock = DockStyle.Fill;
            uiPanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel3.Location = new Point(2, 86);
            uiPanel3.Margin = new Padding(2);
            uiPanel3.MinimumSize = new Size(1, 1);
            uiPanel3.Name = "uiPanel3";
            uiPanel3.Size = new Size(167, 38);
            uiPanel3.TabIndex = 2;
            uiPanel3.Text = "Mật khẩu mới";
            uiPanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel4
            // 
            uiPanel4.Dock = DockStyle.Fill;
            uiPanel4.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel4.Location = new Point(2, 128);
            uiPanel4.Margin = new Padding(2);
            uiPanel4.MinimumSize = new Size(1, 1);
            uiPanel4.Name = "uiPanel4";
            uiPanel4.Size = new Size(167, 38);
            uiPanel4.TabIndex = 3;
            uiPanel4.Text = "Nhập lại khẩu mới";
            uiPanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel5
            // 
            uiPanel5.Dock = DockStyle.Fill;
            uiPanel5.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel5.Location = new Point(2, 170);
            uiPanel5.Margin = new Padding(2);
            uiPanel5.MinimumSize = new Size(1, 1);
            uiPanel5.Name = "uiPanel5";
            uiPanel5.Size = new Size(167, 38);
            uiPanel5.TabIndex = 4;
            uiPanel5.Text = "QR 2FA";
            uiPanel5.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel6
            // 
            uiPanel6.Dock = DockStyle.Fill;
            uiPanel6.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel6.Location = new Point(2, 212);
            uiPanel6.Margin = new Padding(2);
            uiPanel6.MinimumSize = new Size(1, 1);
            uiPanel6.Name = "uiPanel6";
            uiPanel6.Size = new Size(167, 40);
            uiPanel6.TabIndex = 5;
            uiPanel6.Text = "Cấp bậc";
            uiPanel6.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opRole
            // 
            opRole.Cursor = Cursors.IBeam;
            opRole.Dock = DockStyle.Fill;
            opRole.Font = new Font("Microsoft Sans Serif", 12F);
            opRole.Location = new Point(173, 212);
            opRole.Margin = new Padding(2);
            opRole.MinimumSize = new Size(1, 16);
            opRole.Name = "opRole";
            opRole.Padding = new Padding(5);
            opRole.ReadOnly = true;
            opRole.ShowText = false;
            opRole.Size = new Size(309, 40);
            opRole.TabIndex = 6;
            opRole.Text = "uiTextBox1";
            opRole.TextAlignment = ContentAlignment.MiddleLeft;
            opRole.Watermark = "";
            // 
            // uiPanel7
            // 
            uiPanel7.Dock = DockStyle.Fill;
            uiPanel7.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel7.Location = new Point(175, 173);
            uiPanel7.Margin = new Padding(4, 5, 4, 5);
            uiPanel7.MinimumSize = new Size(1, 1);
            uiPanel7.Name = "uiPanel7";
            uiPanel7.Size = new Size(305, 32);
            uiPanel7.TabIndex = 11;
            uiPanel7.Text = "Hiện Mã 2FA";
            uiPanel7.TextAlignment = ContentAlignment.MiddleCenter;
            uiPanel7.Click += uiPanel7_Click;
            // 
            // uc_UserSetting
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(uiTitlePanel1);
            Name = "uc_UserSetting";
            Size = new Size(492, 367);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            opPanel2FA.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UIPanel uiPanel3;
        private Sunny.UI.UIPanel uiPanel4;
        private Sunny.UI.UIPanel uiPanel5;
        private Sunny.UI.UIPanel uiPanel6;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UISymbolButton btnSave;
        private Sunny.UI.UIPanel opPanel2FA;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UISymbolLabel uiSymbolLabel3;
        private Sunny.UI.UINumPadTextBox ipOTP;
        private Sunny.UI.UITextBox ipComfirmNewPassword;
        private Sunny.UI.UITextBox ipNewPassword;
        private Sunny.UI.UITextBox ipOldPassword;
        private Sunny.UI.UITextBox ipUserName;
        private Sunny.UI.UITextBox opRole;
        private Sunny.UI.UIPanel uiPanel7;
    }
}
