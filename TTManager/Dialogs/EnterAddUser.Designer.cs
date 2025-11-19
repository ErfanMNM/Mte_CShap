namespace TTManager.Diaglogs
{
    partial class EnterAddUser
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
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiPanel3 = new Sunny.UI.UIPanel();
            ipRole = new Sunny.UI.UIComboBox();
            btnAdd = new Sunny.UI.UISymbolButton();
            btnCancel = new Sunny.UI.UISymbolButton();
            uiTableLayoutPanel6 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel9 = new Sunny.UI.UITableLayoutPanel();
            ipPassword = new Sunny.UI.UITextBox();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiTableLayoutPanel7 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel8 = new Sunny.UI.UITableLayoutPanel();
            ipUsername = new Sunny.UI.UITextBox();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel6.SuspendLayout();
            uiTableLayoutPanel9.SuspendLayout();
            uiTableLayoutPanel7.SuspendLayout();
            uiTableLayoutPanel8.SuspendLayout();
            SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.BackColor = SystemColors.ControlLightLight;
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel1);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.FillColor = Color.FromArgb(224, 224, 224);
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(0, 0);
            uiTitlePanel1.Margin = new Padding(5, 6, 5, 6);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 45, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(449, 255);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Tạo người dùng mới";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel1.TitleHeight = 45;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 1);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel6, 0, 0);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(1, 45);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 72.72727F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 27.272728F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            uiTableLayoutPanel1.Size = new Size(447, 209);
            uiTableLayoutPanel1.TabIndex = 2;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 4;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25.2631588F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32.0300751F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24.8307F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18.2844238F));
            uiTableLayoutPanel2.Controls.Add(uiPanel3, 0, 0);
            uiTableLayoutPanel2.Controls.Add(ipRole, 1, 0);
            uiTableLayoutPanel2.Controls.Add(btnAdd, 2, 0);
            uiTableLayoutPanel2.Controls.Add(btnCancel, 3, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(2, 154);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(443, 53);
            uiTableLayoutPanel2.TabIndex = 11;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiPanel3
            // 
            uiPanel3.Dock = DockStyle.Fill;
            uiPanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel3.Location = new Point(2, 2);
            uiPanel3.Margin = new Padding(2);
            uiPanel3.MinimumSize = new Size(1, 1);
            uiPanel3.Name = "uiPanel3";
            uiPanel3.Size = new Size(107, 49);
            uiPanel3.TabIndex = 13;
            uiPanel3.Text = "Chức vụ";
            uiPanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // ipRole
            // 
            ipRole.DataSource = null;
            ipRole.Dock = DockStyle.Fill;
            ipRole.FillColor = Color.White;
            ipRole.Font = new Font("Microsoft Sans Serif", 12F);
            ipRole.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipRole.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipRole.Location = new Point(113, 2);
            ipRole.Margin = new Padding(2);
            ipRole.MinimumSize = new Size(63, 0);
            ipRole.Name = "ipRole";
            ipRole.Padding = new Padding(0, 0, 30, 2);
            ipRole.Size = new Size(137, 49);
            ipRole.SymbolSize = 24;
            ipRole.TabIndex = 14;
            ipRole.Text = "uiComboBox1";
            ipRole.TextAlignment = ContentAlignment.MiddleLeft;
            ipRole.Watermark = "";
            // 
            // btnAdd
            // 
            btnAdd.Dock = DockStyle.Fill;
            btnAdd.Font = new Font("Microsoft Sans Serif", 12F);
            btnAdd.Location = new Point(255, 3);
            btnAdd.MinimumSize = new Size(1, 1);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(103, 47);
            btnAdd.Symbol = 61452;
            btnAdd.TabIndex = 15;
            btnAdd.Text = "Đồng ý";
            btnAdd.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnAdd.Click += uiSymbolButton1_Click;
            // 
            // btnCancel
            // 
            btnCancel.Dock = DockStyle.Fill;
            btnCancel.FillColor = Color.FromArgb(255, 128, 0);
            btnCancel.Font = new Font("Microsoft Sans Serif", 12F);
            btnCancel.Location = new Point(364, 3);
            btnCancel.MinimumSize = new Size(1, 1);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(76, 47);
            btnCancel.Symbol = 61453;
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Hủy";
            btnCancel.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnCancel.Click += Close_Click;
            // 
            // uiTableLayoutPanel6
            // 
            uiTableLayoutPanel6.ColumnCount = 1;
            uiTableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel6.Controls.Add(uiTableLayoutPanel9, 0, 1);
            uiTableLayoutPanel6.Controls.Add(uiTableLayoutPanel7, 0, 0);
            uiTableLayoutPanel6.Dock = DockStyle.Fill;
            uiTableLayoutPanel6.Location = new Point(3, 3);
            uiTableLayoutPanel6.Name = "uiTableLayoutPanel6";
            uiTableLayoutPanel6.RowCount = 2;
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            uiTableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            uiTableLayoutPanel6.Size = new Size(441, 146);
            uiTableLayoutPanel6.TabIndex = 6;
            uiTableLayoutPanel6.TagString = null;
            // 
            // uiTableLayoutPanel9
            // 
            uiTableLayoutPanel9.ColumnCount = 2;
            uiTableLayoutPanel9.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25.0379372F));
            uiTableLayoutPanel9.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 74.96207F));
            uiTableLayoutPanel9.Controls.Add(ipPassword, 1, 0);
            uiTableLayoutPanel9.Controls.Add(uiPanel2, 0, 0);
            uiTableLayoutPanel9.Dock = DockStyle.Fill;
            uiTableLayoutPanel9.Location = new Point(2, 75);
            uiTableLayoutPanel9.Margin = new Padding(2);
            uiTableLayoutPanel9.Name = "uiTableLayoutPanel9";
            uiTableLayoutPanel9.RowCount = 1;
            uiTableLayoutPanel9.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel9.Size = new Size(437, 69);
            uiTableLayoutPanel9.TabIndex = 10;
            uiTableLayoutPanel9.TagString = null;
            // 
            // ipPassword
            // 
            ipPassword.Cursor = Cursors.IBeam;
            ipPassword.Dock = DockStyle.Fill;
            ipPassword.Font = new Font("Microsoft Sans Serif", 12F);
            ipPassword.Location = new Point(111, 2);
            ipPassword.Margin = new Padding(2);
            ipPassword.MinimumSize = new Size(1, 16);
            ipPassword.Name = "ipPassword";
            ipPassword.Padding = new Padding(5);
            ipPassword.ShowText = false;
            ipPassword.Size = new Size(324, 65);
            ipPassword.TabIndex = 12;
            ipPassword.TextAlignment = ContentAlignment.MiddleLeft;
            ipPassword.Watermark = "";
            ipPassword.DoubleClick += ipPassword_DoubleClick;
            // 
            // uiPanel2
            // 
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel2.Location = new Point(2, 2);
            uiPanel2.Margin = new Padding(2);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.Size = new Size(105, 65);
            uiPanel2.TabIndex = 10;
            uiPanel2.Text = "Mật khẩu";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel7
            // 
            uiTableLayoutPanel7.ColumnCount = 1;
            uiTableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 84.06504F));
            uiTableLayoutPanel7.Controls.Add(uiTableLayoutPanel8, 0, 0);
            uiTableLayoutPanel7.Dock = DockStyle.Fill;
            uiTableLayoutPanel7.Location = new Point(2, 2);
            uiTableLayoutPanel7.Margin = new Padding(2);
            uiTableLayoutPanel7.Name = "uiTableLayoutPanel7";
            uiTableLayoutPanel7.RowCount = 1;
            uiTableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel7.Size = new Size(437, 69);
            uiTableLayoutPanel7.TabIndex = 7;
            uiTableLayoutPanel7.TagString = null;
            // 
            // uiTableLayoutPanel8
            // 
            uiTableLayoutPanel8.ColumnCount = 2;
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85.3026F));
            uiTableLayoutPanel8.Controls.Add(ipUsername, 1, 0);
            uiTableLayoutPanel8.Controls.Add(uiPanel1, 0, 0);
            uiTableLayoutPanel8.Dock = DockStyle.Fill;
            uiTableLayoutPanel8.Location = new Point(2, 2);
            uiTableLayoutPanel8.Margin = new Padding(2);
            uiTableLayoutPanel8.Name = "uiTableLayoutPanel8";
            uiTableLayoutPanel8.RowCount = 1;
            uiTableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel8.Size = new Size(433, 65);
            uiTableLayoutPanel8.TabIndex = 9;
            uiTableLayoutPanel8.TagString = null;
            // 
            // ipUsername
            // 
            ipUsername.Cursor = Cursors.IBeam;
            ipUsername.Dock = DockStyle.Fill;
            ipUsername.Font = new Font("Microsoft Sans Serif", 12F);
            ipUsername.Location = new Point(112, 2);
            ipUsername.Margin = new Padding(2);
            ipUsername.MinimumSize = new Size(1, 16);
            ipUsername.Name = "ipUsername";
            ipUsername.Padding = new Padding(5);
            ipUsername.ShowText = false;
            ipUsername.Size = new Size(319, 61);
            ipUsername.TabIndex = 12;
            ipUsername.TextAlignment = ContentAlignment.MiddleLeft;
            ipUsername.Watermark = "";
            ipUsername.DoubleClick += ipUsername_DoubleClick;
            // 
            // uiPanel1
            // 
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiPanel1.Location = new Point(2, 2);
            uiPanel1.Margin = new Padding(2);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.Size = new Size(106, 61);
            uiPanel1.TabIndex = 10;
            uiPanel1.Text = "Tên tài khoản";
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // EnterAddUser
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(449, 255);
            Controls.Add(uiTitlePanel1);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "EnterAddUser";
            Text = "Entertext";
            Load += Entertext_Load;
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel6.ResumeLayout(false);
            uiTableLayoutPanel9.ResumeLayout(false);
            uiTableLayoutPanel7.ResumeLayout(false);
            uiTableLayoutPanel8.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel6;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel7;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel8;
        private Sunny.UI.UITextBox ipUsername;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel9;
        private Sunny.UI.UITextBox ipPassword;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel3;
        private Sunny.UI.UIComboBox ipRole;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnAdd;
        private Sunny.UI.UISymbolButton btnCancel;
    }
}