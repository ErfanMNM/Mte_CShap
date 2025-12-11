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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            uiPanel4 = new Sunny.UI.UIPanel();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            opAppClock = new Sunny.UI.UILabel();
            MainTab = new Sunny.UI.UITabControl();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            uiPanel3 = new Sunny.UI.UIPanel();
            NavMenu = new Sunny.UI.UINavMenu();
            opUser = new Sunny.UI.UISymbolLabel();
            btnHome = new Sunny.UI.UISymbolButton();
            headNav = new Sunny.UI.UINavBar();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiImageButton1 = new Sunny.UI.UIImageButton();
            WK1 = new System.ComponentModel.BackgroundWorker();
            clock = new System.ComponentModel.BackgroundWorker();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            uiPanel4.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            headNav.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)uiImageButton1).BeginInit();
            SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 1);
            uiTableLayoutPanel1.Controls.Add(headNav, 0, 0);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Margin = new Padding(0);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Size = new Size(1028, 768);
            uiTableLayoutPanel1.TabIndex = 2;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Controls.Add(uiTableLayoutPanel3, 1, 0);
            uiTableLayoutPanel2.Controls.Add(uiTableLayoutPanel5, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(0, 45);
            uiTableLayoutPanel2.Margin = new Padding(0);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(1028, 723);
            uiTableLayoutPanel2.TabIndex = 2;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.ColumnCount = 1;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.Controls.Add(uiPanel4, 0, 1);
            uiTableLayoutPanel3.Controls.Add(MainTab, 0, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Fill;
            uiTableLayoutPanel3.Location = new Point(150, 0);
            uiTableLayoutPanel3.Margin = new Padding(0);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 2;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            uiTableLayoutPanel3.Size = new Size(878, 723);
            uiTableLayoutPanel3.TabIndex = 2;
            uiTableLayoutPanel3.TagString = null;
            // 
            // uiPanel4
            // 
            uiPanel4.Controls.Add(uiTableLayoutPanel4);
            uiPanel4.Dock = DockStyle.Fill;
            uiPanel4.FillColor = Color.Gainsboro;
            uiPanel4.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiPanel4.ForeColor = Color.MediumBlue;
            uiPanel4.Location = new Point(0, 683);
            uiPanel4.Margin = new Padding(0);
            uiPanel4.MinimumSize = new Size(1, 1);
            uiPanel4.Name = "uiPanel4";
            uiPanel4.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            uiPanel4.RectColor = Color.Blue;
            uiPanel4.Size = new Size(878, 40);
            uiPanel4.TabIndex = 5;
            uiPanel4.Text = null;
            uiPanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel4
            // 
            uiTableLayoutPanel4.BackColor = Color.PaleTurquoise;
            uiTableLayoutPanel4.ColumnCount = 2;
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 157F));
            uiTableLayoutPanel4.Controls.Add(opAppClock, 1, 0);
            uiTableLayoutPanel4.Dock = DockStyle.Fill;
            uiTableLayoutPanel4.Location = new Point(0, 0);
            uiTableLayoutPanel4.Margin = new Padding(0);
            uiTableLayoutPanel4.Name = "uiTableLayoutPanel4";
            uiTableLayoutPanel4.RowCount = 1;
            uiTableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel4.Size = new Size(878, 40);
            uiTableLayoutPanel4.TabIndex = 0;
            uiTableLayoutPanel4.TagString = null;
            // 
            // opAppClock
            // 
            opAppClock.Dock = DockStyle.Fill;
            opAppClock.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opAppClock.ForeColor = Color.FromArgb(0, 0, 192);
            opAppClock.Location = new Point(724, 0);
            opAppClock.Name = "opAppClock";
            opAppClock.Size = new Size(151, 40);
            opAppClock.TabIndex = 0;
            opAppClock.Text = "2025-11-11 11:11:11.111 +007";
            opAppClock.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainTab
            // 
            MainTab.Dock = DockStyle.Fill;
            MainTab.DrawMode = TabDrawMode.OwnerDrawFixed;
            MainTab.Font = new Font("Microsoft Sans Serif", 12F);
            MainTab.ItemSize = new Size(0, 1);
            MainTab.Location = new Point(2, 2);
            MainTab.MainPage = "";
            MainTab.Margin = new Padding(2);
            MainTab.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            MainTab.Name = "MainTab";
            MainTab.SelectedIndex = 0;
            MainTab.Size = new Size(874, 679);
            MainTab.SizeMode = TabSizeMode.Fixed;
            MainTab.TabBackColor = Color.FromArgb(243, 249, 255);
            MainTab.TabIndex = 1;
            MainTab.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            MainTab.TabVisible = false;
            MainTab.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 1;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel5.Controls.Add(uiPanel3, 0, 3);
            uiTableLayoutPanel5.Controls.Add(NavMenu, 0, 0);
            uiTableLayoutPanel5.Controls.Add(opUser, 0, 2);
            uiTableLayoutPanel5.Controls.Add(btnHome, 0, 1);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(0, 0);
            uiTableLayoutPanel5.Margin = new Padding(0);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 4;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            uiTableLayoutPanel5.Size = new Size(150, 723);
            uiTableLayoutPanel5.TabIndex = 3;
            uiTableLayoutPanel5.TagString = null;
            // 
            // uiPanel3
            // 
            uiPanel3.BackColor = Color.FromArgb(0, 192, 192);
            uiPanel3.Dock = DockStyle.Fill;
            uiPanel3.FillColor = Color.FromArgb(0, 192, 192);
            uiPanel3.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiPanel3.ForeColor = Color.MediumBlue;
            uiPanel3.Location = new Point(0, 682);
            uiPanel3.Margin = new Padding(0);
            uiPanel3.MinimumSize = new Size(1, 1);
            uiPanel3.Name = "uiPanel3";
            uiPanel3.RectColor = Color.Blue;
            uiPanel3.Size = new Size(150, 41);
            uiPanel3.TabIndex = 4;
            uiPanel3.Text = "MASAN v12.11.5.2";
            uiPanel3.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // NavMenu
            // 
            NavMenu.BorderStyle = BorderStyle.None;
            NavMenu.Dock = DockStyle.Fill;
            NavMenu.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            NavMenu.Font = new Font("Microsoft Sans Serif", 12F);
            NavMenu.FullRowSelect = true;
            NavMenu.HotTracking = true;
            NavMenu.ItemHeight = 50;
            NavMenu.Location = new Point(3, 3);
            NavMenu.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            NavMenu.Name = "NavMenu";
            NavMenu.ShowLines = false;
            NavMenu.ShowPlusMinus = false;
            NavMenu.ShowRootLines = false;
            NavMenu.Size = new Size(144, 584);
            NavMenu.TabIndex = 2;
            NavMenu.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // opUser
            // 
            opUser.Dock = DockStyle.Fill;
            opUser.Font = new Font("Microsoft Sans Serif", 12F);
            opUser.Location = new Point(3, 641);
            opUser.MinimumSize = new Size(1, 1);
            opUser.Name = "opUser";
            opUser.Size = new Size(144, 38);
            opUser.Symbol = 62142;
            opUser.TabIndex = 3;
            opUser.Text = "Admin";
            // 
            // btnHome
            // 
            btnHome.Dock = DockStyle.Fill;
            btnHome.Font = new Font("Microsoft Sans Serif", 12F);
            btnHome.Location = new Point(3, 593);
            btnHome.MinimumSize = new Size(1, 1);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(144, 42);
            btnHome.Symbol = 61461;
            btnHome.TabIndex = 5;
            btnHome.Text = "Về Trang Chủ";
            btnHome.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnHome.Click += btnHome_Click;
            // 
            // headNav
            // 
            headNav.BackColor = Color.LightSteelBlue;
            headNav.Controls.Add(uiPanel2);
            headNav.Controls.Add(uiPanel1);
            headNav.Controls.Add(uiImageButton1);
            headNav.Dock = DockStyle.Top;
            headNav.DropMenuFont = new Font("Microsoft Sans Serif", 12F);
            headNav.Font = new Font("Microsoft Sans Serif", 12F);
            headNav.ForeColor = Color.White;
            headNav.Location = new Point(0, 0);
            headNav.Margin = new Padding(0);
            headNav.MenuHoverColor = Color.Transparent;
            headNav.MenuSelectedColor = Color.Transparent;
            headNav.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            headNav.Name = "headNav";
            headNav.NodeInterval = 0;
            headNav.SelectedForeColor = Color.White;
            headNav.SelectedHighColor = Color.White;
            headNav.SelectedHighColorSize = 0;
            headNav.ShowItemsArrow = false;
            headNav.Size = new Size(1028, 45);
            headNav.TabIndex = 3;
            headNav.MenuItemClick += headNav_MenuItemClick;
            // 
            // uiPanel2
            // 
            uiPanel2.FillColor = Color.Transparent;
            uiPanel2.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiPanel2.ForeColor = Color.MediumBlue;
            uiPanel2.Location = new Point(247, 0);
            uiPanel2.Margin = new Padding(4, 5, 4, 5);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.RectColor = Color.Transparent;
            uiPanel2.Size = new Size(623, 45);
            uiPanel2.TabIndex = 2;
            uiPanel2.Text = "PHẦN MỀM KÍCH HOẠT MÃ QR";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel1
            // 
            uiPanel1.FillColor = Color.Transparent;
            uiPanel1.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiPanel1.ForeColor = Color.MediumBlue;
            uiPanel1.Location = new Point(58, 0);
            uiPanel1.Margin = new Padding(4, 5, 4, 5);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.RectColor = Color.Transparent;
            uiPanel1.Size = new Size(181, 45);
            uiPanel1.TabIndex = 1;
            uiPanel1.Text = "Tân Tiến Hightech";
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiImageButton1
            // 
            uiImageButton1.Font = new Font("Microsoft Sans Serif", 12F);
            uiImageButton1.Image = (Image)resources.GetObject("uiImageButton1.Image");
            uiImageButton1.Location = new Point(-1, 0);
            uiImageButton1.Name = "uiImageButton1";
            uiImageButton1.Size = new Size(52, 45);
            uiImageButton1.SizeMode = PictureBoxSizeMode.StretchImage;
            uiImageButton1.TabIndex = 0;
            uiImageButton1.TabStop = false;
            uiImageButton1.Text = null;
            // 
            // WK1
            // 
            WK1.WorkerSupportsCancellation = true;
            WK1.DoWork += WK1_DoWork;
            // 
            // clock
            // 
            clock.WorkerSupportsCancellation = true;
            clock.DoWork += clock_DoWork;
            // 
            // MainForm
            // 
            AllowShowTitle = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1028, 768);
            Controls.Add(uiTableLayoutPanel1);
            MaximumSize = new Size(1028, 768);
            Name = "MainForm";
            Padding = new Padding(0);
            ShowTitle = false;
            Text = "TApp";
            WindowState = FormWindowState.Minimized;
            ZoomScaleRect = new Rectangle(15, 15, 852, 482);
            Resize += MainForm_Resize;
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel3.ResumeLayout(false);
            uiPanel4.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            headNav.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)uiImageButton1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
        private Sunny.UI.UITabControl MainTab;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UINavBar headNav;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UIImageButton uiImageButton1;
        private Sunny.UI.UINavMenu NavMenu;
        private System.ComponentModel.BackgroundWorker WK1;
        private Sunny.UI.UISymbolLabel opUser;
        private Sunny.UI.UIPanel uiPanel3;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel4;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UILabel opAppClock;
        private System.ComponentModel.BackgroundWorker clock;
        private Sunny.UI.UISymbolButton btnHome;
    }
}