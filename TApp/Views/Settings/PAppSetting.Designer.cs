namespace TApp.Views.Settings
{
    partial class PAppSetting
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
            TTManager.Auth.UserData userData1 = new TTManager.Auth.UserData();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTabControl1 = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            uc_UserManager1 = new TTManager.Auth.uc_UserManager();
            uiListBox1 = new Sunny.UI.UIListBox();
            uc_UserSetting1 = new TTManager.Auth.uc_UserSetting();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            btnSave = new Sunny.UI.UISymbolButton();
            btnDefault = new Sunny.UI.UISymbolButton();
            uiTableLayoutPanel1.SuspendLayout();
            uiTabControl1.SuspendLayout();
            tabPage2.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(uiTabControl1, 0, 0);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 35);
            uiTableLayoutPanel1.Margin = new Padding(2);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 91.77019F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 8.229814F));
            uiTableLayoutPanel1.Size = new Size(874, 644);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTabControl1
            // 
            uiTabControl1.Controls.Add(tabPage1);
            uiTabControl1.Controls.Add(tabPage2);
            uiTabControl1.Dock = DockStyle.Fill;
            uiTabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            uiTabControl1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTabControl1.ItemSize = new Size(150, 40);
            uiTabControl1.Location = new Point(3, 3);
            uiTabControl1.MainPage = "";
            uiTabControl1.Name = "uiTabControl1";
            uiTabControl1.SelectedIndex = 0;
            uiTabControl1.Size = new Size(868, 585);
            uiTabControl1.SizeMode = TabSizeMode.Fixed;
            uiTabControl1.TabIndex = 1;
            uiTabControl1.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            uiTabControl1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // tabPage1
            // 
            tabPage1.AutoScroll = true;
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(868, 545);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Setting";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(uc_UserManager1);
            tabPage2.Controls.Add(uiListBox1);
            tabPage2.Controls.Add(uc_UserSetting1);
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(868, 545);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Người dùng";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // uc_UserManager1
            // 
            uc_UserManager1.CurrentUserName = "";
            uc_UserManager1.Font = new Font("Microsoft Sans Serif", 12F);
            uc_UserManager1.IS2FAEnabled = false;
            uc_UserManager1.Location = new Point(424, 3);
            uc_UserManager1.MinimumSize = new Size(1, 1);
            uc_UserManager1.Name = "uc_UserManager1";
            uc_UserManager1.Size = new Size(440, 366);
            uc_UserManager1.TabIndex = 2;
            uc_UserManager1.Text = "uc_UserManager1";
            uc_UserManager1.TextAlignment = ContentAlignment.MiddleCenter;
            uc_UserManager1.OnAction += uc_UserManager1_OnAction;
            // 
            // uiListBox1
            // 
            uiListBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiListBox1.HoverColor = Color.FromArgb(155, 200, 255);
            uiListBox1.ItemSelectForeColor = Color.White;
            uiListBox1.Location = new Point(4, 369);
            uiListBox1.Margin = new Padding(4, 5, 4, 5);
            uiListBox1.MinimumSize = new Size(1, 1);
            uiListBox1.Name = "uiListBox1";
            uiListBox1.Padding = new Padding(2);
            uiListBox1.ShowText = false;
            uiListBox1.Size = new Size(860, 171);
            uiListBox1.TabIndex = 1;
            uiListBox1.Text = "uiListBox1";
            // 
            // uc_UserSetting1
            // 
            uc_UserSetting1.CurrentUserName = null;
            uc_UserSetting1.Font = new Font("Microsoft Sans Serif", 12F);
            uc_UserSetting1.IS2FAEnabled = false;
            uc_UserSetting1.Location = new Point(3, 3);
            uc_UserSetting1.MinimumSize = new Size(1, 1);
            uc_UserSetting1.Name = "uc_UserSetting1";
            uc_UserSetting1.Size = new Size(415, 366);
            uc_UserSetting1.TabIndex = 0;
            uc_UserSetting1.Text = "uc_UserSetting1";
            uc_UserSetting1.TextAlignment = ContentAlignment.MiddleCenter;
            uc_UserSetting1.TwoFARequired = false;
            userData1.Key2FA = null;
            userData1.Password = null;
            userData1.Role = null;
            userData1.Salt = null;
            userData1.Username = "";
            uc_UserSetting1.userData = userData1;
            uc_UserSetting1.OnUserAction += uc_UserSetting1_OnUserAction;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 3;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            uiTableLayoutPanel2.Controls.Add(btnSave, 2, 0);
            uiTableLayoutPanel2.Controls.Add(btnDefault, 1, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(2, 593);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(870, 49);
            uiTableLayoutPanel2.TabIndex = 2;
            uiTableLayoutPanel2.TagString = null;
            // 
            // btnSave
            // 
            btnSave.Dock = DockStyle.Fill;
            btnSave.FillColor = Color.FromArgb(0, 192, 192);
            btnSave.Font = new Font("Microsoft Sans Serif", 12F);
            btnSave.Location = new Point(773, 3);
            btnSave.MinimumSize = new Size(1, 1);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(94, 43);
            btnSave.Symbol = 61639;
            btnSave.TabIndex = 0;
            btnSave.Text = "Lưu Lại";
            btnSave.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSave.Click += btnSave_Click;
            // 
            // btnDefault
            // 
            btnDefault.Dock = DockStyle.Fill;
            btnDefault.FillColor = Color.FromArgb(255, 192, 128);
            btnDefault.Font = new Font("Microsoft Sans Serif", 12F);
            btnDefault.Location = new Point(658, 3);
            btnDefault.MinimumSize = new Size(1, 1);
            btnDefault.Name = "btnDefault";
            btnDefault.Size = new Size(109, 43);
            btnDefault.Symbol = 61473;
            btnDefault.TabIndex = 1;
            btnDefault.Text = "Khôi phục";
            btnDefault.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnDefault.Click += btnDefault_Click;
            // 
            // PAppSetting
            // 
            AllowShowTitle = true;
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = true;
            ClientSize = new Size(874, 679);
            Controls.Add(uiTableLayoutPanel1);
            Name = "PAppSetting";
            Padding = new Padding(0, 35, 0, 0);
            ShowTitle = true;
            Symbol = 559576;
            Text = "Cài Đặt";
            TitleFillColor = Color.FromArgb(0, 192, 192);
            Initialize += PAppSetting_Initialize;
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTabControl1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITabControl uiTabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UISymbolButton btnSave;
        private Sunny.UI.UISymbolButton btnDefault;
        private TTManager.Auth.uc_UserSetting uc_UserSetting1;
        private Sunny.UI.UIListBox uiListBox1;
        private TTManager.Auth.uc_UserManager uc_UserManager1;
    }
}