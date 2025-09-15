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
            uiTabControl1 = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            opAppConfig = new Sunny.UI.UIPanel();
            tabPage2 = new TabPage();
            btnSaveConfig = new Sunny.UI.UISymbolButton();
            uiTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
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
            uiTabControl1.Location = new Point(0, 35);
            uiTabControl1.MainPage = "";
            uiTabControl1.Name = "uiTabControl1";
            uiTabControl1.SelectedIndex = 0;
            uiTabControl1.Size = new Size(992, 627);
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
            tabPage1.Size = new Size(992, 587);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Setting";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.AutoSize = true;
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(opAppConfig, 0, 0);
            uiTableLayoutPanel1.Controls.Add(btnSaveConfig, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Margin = new Padding(2);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 91.82283F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 8.177172F));
            uiTableLayoutPanel1.Size = new Size(992, 587);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // opAppConfig
            // 
            opAppConfig.AutoSize = true;
            opAppConfig.Dock = DockStyle.Fill;
            opAppConfig.Font = new Font("Microsoft Sans Serif", 12F);
            opAppConfig.Location = new Point(0, 0);
            opAppConfig.Margin = new Padding(0);
            opAppConfig.MinimumSize = new Size(1, 1);
            opAppConfig.Name = "opAppConfig";
            opAppConfig.Size = new Size(992, 539);
            opAppConfig.TabIndex = 1;
            opAppConfig.Text = null;
            opAppConfig.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(200, 60);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Người dùng";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Font = new Font("Microsoft Sans Serif", 12F);
            btnSaveConfig.Location = new Point(3, 542);
            btnSaveConfig.MinimumSize = new Size(1, 1);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(986, 42);
            btnSaveConfig.TabIndex = 2;
            btnSaveConfig.Text = "Lưu Lại";
            btnSaveConfig.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // PAppSetting
            // 
            AllowShowTitle = true;
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = true;
            ClientSize = new Size(992, 662);
            Controls.Add(uiTabControl1);
            Name = "PAppSetting";
            Padding = new Padding(0, 35, 0, 0);
            ShowTitle = true;
            Symbol = 559576;
            Text = "Cài Đặt";
            TitleFillColor = Color.FromArgb(0, 192, 192);
            uiTabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl uiTabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UIPanel opAppConfig;
        private Sunny.UI.UISymbolButton btnSaveConfig;
    }
}