namespace CProject
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TabBody = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            MainNavMenu = new Sunny.UI.UINavMenu();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            TabBody.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // TabBody
            // 
            TabBody.Controls.Add(tabPage1);
            TabBody.Controls.Add(tabPage2);
            TabBody.Dock = DockStyle.Fill;
            TabBody.DrawMode = TabDrawMode.OwnerDrawFixed;
            TabBody.Font = new Font("Microsoft Sans Serif", 12F);
            TabBody.ItemSize = new Size(0, 1);
            TabBody.Location = new Point(160, 3);
            TabBody.MainPage = "";
            TabBody.Name = "TabBody";
            TabBody.SelectedIndex = 0;
            TabBody.Size = new Size(893, 573);
            TabBody.SizeMode = TabSizeMode.Fixed;
            TabBody.TabIndex = 0;
            TabBody.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            TabBody.TabVisible = false;
            TabBody.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(0, 0);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(893, 573);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(200, 60);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // MainNavMenu
            // 
            MainNavMenu.BorderStyle = BorderStyle.None;
            MainNavMenu.Dock = DockStyle.Fill;
            MainNavMenu.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            MainNavMenu.Font = new Font("Microsoft Sans Serif", 12F);
            MainNavMenu.FullRowSelect = true;
            MainNavMenu.HotTracking = true;
            MainNavMenu.ItemHeight = 50;
            MainNavMenu.Location = new Point(3, 3);
            MainNavMenu.Name = "MainNavMenu";
            MainNavMenu.ShowLines = false;
            MainNavMenu.ShowPlusMinus = false;
            MainNavMenu.ShowRootLines = false;
            MainNavMenu.Size = new Size(151, 573);
            MainNavMenu.TabIndex = 1;
            MainNavMenu.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 2;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.867424F));
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85.132576F));
            uiTableLayoutPanel1.Controls.Add(MainNavMenu, 0, 0);
            uiTableLayoutPanel1.Controls.Add(TabBody, 1, 0);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 35);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 1;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Size = new Size(1056, 579);
            uiTableLayoutPanel1.TabIndex = 2;
            uiTableLayoutPanel1.TagString = null;
            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1056, 614);
            Controls.Add(uiTableLayoutPanel1);
            Name = "MainForm";
            Text = "Form1";
            ZoomScaleRect = new Rectangle(15, 15, 800, 450);
            TabBody.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl TabBody;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Sunny.UI.UINavMenu MainNavMenu;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
    }
}
