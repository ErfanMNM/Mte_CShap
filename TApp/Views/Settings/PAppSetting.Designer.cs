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
            opAppConfig = new Sunny.UI.UIPanel();
            tabPage2 = new TabPage();
            uiTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
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
            tabPage1.Controls.Add(opAppConfig);
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(992, 587);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Setting";
            tabPage1.UseVisualStyleBackColor = true;
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
            opAppConfig.Size = new Size(992, 587);
            opAppConfig.TabIndex = 0;
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
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl uiTabControl1;
        private TabPage tabPage1;
        private Sunny.UI.UIPanel opAppConfig;
        private TabPage tabPage2;
    }
}