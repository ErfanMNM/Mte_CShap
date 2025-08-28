namespace TApp
{
    partial class MainForm
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
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            NavMenu = new Sunny.UI.UINavMenu();
            MainTab = new Sunny.UI.UITabControl();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 1);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Margin = new Padding(0);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 3;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 6.77165365F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 87.55905F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 5.51181126F));
            uiTableLayoutPanel1.Size = new Size(1137, 635);
            uiTableLayoutPanel1.TabIndex = 2;
            uiTableLayoutPanel1.TagString = null;
            // 
            // NavMenu
            // 
            NavMenu.BackColor = Color.Teal;
            NavMenu.BorderStyle = BorderStyle.None;
            NavMenu.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            NavMenu.Font = new Font("Microsoft Sans Serif", 12F);
            NavMenu.FullRowSelect = true;
            NavMenu.HotTracking = true;
            NavMenu.ItemHeight = 50;
            NavMenu.Location = new Point(2, 2);
            NavMenu.Margin = new Padding(2);
            NavMenu.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            NavMenu.Name = "NavMenu";
            NavMenu.ShowLines = false;
            NavMenu.ShowPlusMinus = false;
            NavMenu.ShowRootLines = false;
            NavMenu.Size = new Size(179, 550);
            NavMenu.TabIndex = 1;
            NavMenu.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // MainTab
            // 
            MainTab.DrawMode = TabDrawMode.OwnerDrawFixed;
            MainTab.FillColor = Color.FromArgb(192, 255, 255);
            MainTab.Font = new Font("Microsoft Sans Serif", 12F);
            MainTab.ItemSize = new Size(150, 40);
            MainTab.Location = new Point(185, 2);
            MainTab.MainPage = "";
            MainTab.Margin = new Padding(2);
            MainTab.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            MainTab.Name = "MainTab";
            MainTab.SelectedIndex = 0;
            MainTab.Size = new Size(948, 550);
            MainTab.SizeMode = TabSizeMode.Fixed;
            MainTab.TabBackColor = Color.FromArgb(192, 255, 255);
            MainTab.TabIndex = 0;
            MainTab.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            MainTab.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.094986F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 83.905014F));
            uiTableLayoutPanel2.Controls.Add(MainTab, 1, 0);
            uiTableLayoutPanel2.Controls.Add(NavMenu, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(0, 43);
            uiTableLayoutPanel2.Margin = new Padding(0);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(1137, 556);
            uiTableLayoutPanel2.TabIndex = 2;
            uiTableLayoutPanel2.TagString = null;
            // 
            // MainForm
            // 
            AllowShowTitle = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1137, 635);
            Controls.Add(uiTableLayoutPanel1);
            Name = "MainForm";
            Padding = new Padding(0);
            ShowTitle = false;
            Text = "TApp";
            ZoomScaleRect = new Rectangle(15, 15, 852, 482);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UITabControl MainTab;
        private Sunny.UI.UINavMenu NavMenu;
    }
}