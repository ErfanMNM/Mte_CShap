namespace MHG_Printer
{
    partial class M2_MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(M2_MainForm));
            clock = new System.ComponentModel.BackgroundWorker();
            WK1 = new System.ComponentModel.BackgroundWorker();
            uiPanel2 = new Sunny.UI.UIPanel();
            uiPanel1 = new Sunny.UI.UIPanel();
            uiImageButton1 = new Sunny.UI.UIImageButton();
            headNav = new Sunny.UI.UINavBar();
            uiTableLayoutPanel3 = new Sunny.UI.UITableLayoutPanel();
            navTrai = new Sunny.UI.UITableLayoutPanel();
            uiPanel3 = new Sunny.UI.UIPanel();
            NavMenu = new Sunny.UI.UINavMenu();
            opUser = new Sunny.UI.UISymbolLabel();
            btnHome = new Sunny.UI.UISymbolButton();
            opAppClock = new Sunny.UI.UILabel();
            uiTableLayoutPanel4 = new Sunny.UI.UITableLayoutPanel();
            uiPanel4 = new Sunny.UI.UIPanel();
            banggiua = new Sunny.UI.UITableLayoutPanel();
            MainTab = new Sunny.UI.UITabControl();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)uiImageButton1).BeginInit();
            headNav.SuspendLayout();
            uiTableLayoutPanel3.SuspendLayout();
            navTrai.SuspendLayout();
            uiTableLayoutPanel4.SuspendLayout();
            uiPanel4.SuspendLayout();
            banggiua.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // clock
            // 
            clock.WorkerSupportsCancellation = true;
            clock.DoWork += clock_DoWork;
            // 
            // WK1
            // 
            WK1.WorkerSupportsCancellation = true;
            WK1.DoWork += WK1_DoWork;
            // 
            // uiPanel2
            // 
            uiPanel2.Dock = DockStyle.Fill;
            uiPanel2.FillColor = Color.Transparent;
            uiPanel2.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiPanel2.ForeColor = Color.Crimson;
            uiPanel2.Location = new Point(233, 5);
            uiPanel2.Margin = new Padding(4, 5, 4, 5);
            uiPanel2.MinimumSize = new Size(1, 1);
            uiPanel2.Name = "uiPanel2";
            uiPanel2.RectColor = Color.Transparent;
            uiPanel2.Size = new Size(677, 35);
            uiPanel2.TabIndex = 2;
            uiPanel2.Text = "PHẦN MỀM KÍCH HOẠT MÃ QR";
            uiPanel2.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiPanel1
            // 
            uiPanel1.Dock = DockStyle.Fill;
            uiPanel1.FillColor = Color.Transparent;
            uiPanel1.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiPanel1.ForeColor = Color.Crimson;
            uiPanel1.Location = new Point(64, 5);
            uiPanel1.Margin = new Padding(4, 5, 4, 5);
            uiPanel1.MinimumSize = new Size(1, 1);
            uiPanel1.Name = "uiPanel1";
            uiPanel1.RectColor = Color.Transparent;
            uiPanel1.Size = new Size(161, 35);
            uiPanel1.TabIndex = 1;
            uiPanel1.Text = "Tân Tiến Hightech";
            uiPanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiImageButton1
            // 
            uiImageButton1.Dock = DockStyle.Fill;
            uiImageButton1.Font = new Font("Microsoft Sans Serif", 12F);
            uiImageButton1.Image = (Image)resources.GetObject("uiImageButton1.Image");
            uiImageButton1.Location = new Point(0, 0);
            uiImageButton1.Margin = new Padding(0);
            uiImageButton1.Name = "uiImageButton1";
            uiImageButton1.Size = new Size(60, 45);
            uiImageButton1.SizeMode = PictureBoxSizeMode.StretchImage;
            uiImageButton1.TabIndex = 0;
            uiImageButton1.TabStop = false;
            uiImageButton1.Text = null;
            // 
            // headNav
            // 
            headNav.Anchor = AnchorStyles.Right;
            headNav.BackColor = Color.LightSteelBlue;
            headNav.Controls.Add(uiTableLayoutPanel3);
            headNav.DropDownItemAutoHeight = true;
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
            headNav.Size = new Size(1024, 45);
            headNav.TabIndex = 3;
            headNav.MenuItemClick += headNav_MenuItemClick;
            // 
            // uiTableLayoutPanel3
            // 
            uiTableLayoutPanel3.AutoSize = true;
            uiTableLayoutPanel3.BackColor = Color.LightSkyBlue;
            uiTableLayoutPanel3.ColumnCount = 3;
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 169F));
            uiTableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.Controls.Add(uiImageButton1, 0, 0);
            uiTableLayoutPanel3.Controls.Add(uiPanel2, 2, 0);
            uiTableLayoutPanel3.Controls.Add(uiPanel1, 1, 0);
            uiTableLayoutPanel3.Dock = DockStyle.Left;
            uiTableLayoutPanel3.Location = new Point(0, 0);
            uiTableLayoutPanel3.Margin = new Padding(0);
            uiTableLayoutPanel3.Name = "uiTableLayoutPanel3";
            uiTableLayoutPanel3.RowCount = 1;
            uiTableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel3.Size = new Size(914, 45);
            uiTableLayoutPanel3.TabIndex = 3;
            uiTableLayoutPanel3.TagString = null;
            // 
            // navTrai
            // 
            navTrai.ColumnCount = 1;
            navTrai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            navTrai.Controls.Add(uiPanel3, 0, 3);
            navTrai.Controls.Add(NavMenu, 0, 0);
            navTrai.Controls.Add(opUser, 0, 2);
            navTrai.Controls.Add(btnHome, 0, 1);
            navTrai.Dock = DockStyle.Fill;
            navTrai.Location = new Point(2, 2);
            navTrai.Margin = new Padding(2);
            navTrai.Name = "navTrai";
            navTrai.RowCount = 4;
            navTrai.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            navTrai.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            navTrai.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            navTrai.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            navTrai.Size = new Size(149, 719);
            navTrai.TabIndex = 3;
            navTrai.TagString = null;
            // 
            // uiPanel3
            // 
            uiPanel3.BackColor = Color.FromArgb(0, 192, 192);
            uiPanel3.Dock = DockStyle.Fill;
            uiPanel3.FillColor = Color.FromArgb(0, 192, 192);
            uiPanel3.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            uiPanel3.ForeColor = Color.MediumBlue;
            uiPanel3.Location = new Point(0, 678);
            uiPanel3.Margin = new Padding(0);
            uiPanel3.MinimumSize = new Size(1, 1);
            uiPanel3.Name = "uiPanel3";
            uiPanel3.RectColor = Color.Blue;
            uiPanel3.Size = new Size(149, 41);
            uiPanel3.TabIndex = 4;
            uiPanel3.Text = "1.0.0";
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
            NavMenu.Location = new Point(0, 0);
            NavMenu.Margin = new Padding(0);
            NavMenu.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            NavMenu.Name = "NavMenu";
            NavMenu.ShowLines = false;
            NavMenu.ShowPlusMinus = false;
            NavMenu.ShowRootLines = false;
            NavMenu.Size = new Size(149, 586);
            NavMenu.TabIndex = 2;
            NavMenu.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // opUser
            // 
            opUser.Dock = DockStyle.Fill;
            opUser.Font = new Font("Microsoft Sans Serif", 12F);
            opUser.Location = new Point(3, 637);
            opUser.MinimumSize = new Size(1, 1);
            opUser.Name = "opUser";
            opUser.Size = new Size(143, 38);
            opUser.Symbol = 62142;
            opUser.TabIndex = 3;
            opUser.Text = "Admin";
            // 
            // btnHome
            // 
            btnHome.Dock = DockStyle.Fill;
            btnHome.Font = new Font("Microsoft Sans Serif", 12F);
            btnHome.Location = new Point(3, 589);
            btnHome.MinimumSize = new Size(1, 1);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(143, 42);
            btnHome.Symbol = 61461;
            btnHome.TabIndex = 5;
            btnHome.Text = "Về Trang Chủ";
            btnHome.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnHome.Click += btnHome_Click;
            // 
            // opAppClock
            // 
            opAppClock.Dock = DockStyle.Fill;
            opAppClock.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opAppClock.ForeColor = Color.FromArgb(0, 0, 192);
            opAppClock.Location = new Point(717, 0);
            opAppClock.Name = "opAppClock";
            opAppClock.Size = new Size(151, 40);
            opAppClock.TabIndex = 0;
            opAppClock.Text = "2025-11-11 11:11:11.111 +007";
            opAppClock.TextAlign = ContentAlignment.MiddleCenter;
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
            uiTableLayoutPanel4.Size = new Size(871, 40);
            uiTableLayoutPanel4.TabIndex = 0;
            uiTableLayoutPanel4.TagString = null;
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
            uiPanel4.Size = new Size(871, 40);
            uiPanel4.TabIndex = 5;
            uiPanel4.Text = null;
            uiPanel4.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // banggiua
            // 
            banggiua.ColumnCount = 1;
            banggiua.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            banggiua.Controls.Add(uiPanel4, 0, 1);
            banggiua.Controls.Add(MainTab, 0, 0);
            banggiua.Dock = DockStyle.Fill;
            banggiua.Location = new Point(153, 0);
            banggiua.Margin = new Padding(0);
            banggiua.Name = "banggiua";
            banggiua.RowCount = 2;
            banggiua.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            banggiua.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            banggiua.Size = new Size(871, 723);
            banggiua.TabIndex = 2;
            banggiua.TagString = null;
            // 
            // MainTab
            // 
            MainTab.Dock = DockStyle.Fill;
            MainTab.DrawMode = TabDrawMode.OwnerDrawFixed;
            MainTab.Font = new Font("Microsoft Sans Serif", 12F);
            MainTab.ItemSize = new Size(0, 1);
            MainTab.Location = new Point(3, 3);
            MainTab.MainPage = "";
            MainTab.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            MainTab.Name = "MainTab";
            MainTab.SelectedIndex = 0;
            MainTab.Size = new Size(865, 677);
            MainTab.SizeMode = TabSizeMode.Fixed;
            MainTab.TabBackColor = Color.FromArgb(243, 249, 255);
            MainTab.TabIndex = 1;
            MainTab.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            MainTab.TabVisible = false;
            MainTab.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 2;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85F));
            uiTableLayoutPanel2.Controls.Add(banggiua, 1, 0);
            uiTableLayoutPanel2.Controls.Add(navTrai, 0, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(0, 45);
            uiTableLayoutPanel2.Margin = new Padding(0);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(1024, 723);
            uiTableLayoutPanel2.TabIndex = 2;
            uiTableLayoutPanel2.TagString = null;
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
            uiTableLayoutPanel1.Size = new Size(1024, 768);
            uiTableLayoutPanel1.TabIndex = 3;
            uiTableLayoutPanel1.TagString = null;
            // 
            // M2_MainForm
            // 
            AllowShowTitle = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1024, 768);
            Controls.Add(uiTableLayoutPanel1);
            Font = new Font("Tahoma", 10F);
            MaximumSize = new Size(1920, 1080);
            Name = "M2_MainForm";
            Padding = new Padding(0);
            ShowFullScreen = true;
            ShowTitle = false;
            Text = "M2 - OPC UA Client Test";
            ZoomScaleRect = new Rectangle(15, 15, 800, 450);
            ((System.ComponentModel.ISupportInitialize)uiImageButton1).EndInit();
            headNav.ResumeLayout(false);
            headNav.PerformLayout();
            uiTableLayoutPanel3.ResumeLayout(false);
            navTrai.ResumeLayout(false);
            uiTableLayoutPanel4.ResumeLayout(false);
            uiPanel4.ResumeLayout(false);
            banggiua.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void ConfigureLabel(Sunny.UI.UILabel label, string text)
        {
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        private static void ConfigureTextBox(Sunny.UI.UITextBox textBox, string text)
        {
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.Font = new System.Drawing.Font("Tahoma", 10F);
            textBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            textBox.MinimumSize = new System.Drawing.Size(1, 16);
            textBox.Padding = new System.Windows.Forms.Padding(5);
            textBox.ShowText = false;
            textBox.Text = text;
            textBox.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            textBox.Watermark = "";
        }

        private static void ConfigureButton(Sunny.UI.UIButton button, string text)
        {
            button.Font = new System.Drawing.Font("Tahoma", 10F);
            button.Location = new System.Drawing.Point(3, 3);
            button.MinimumSize = new System.Drawing.Size(1, 1);
            button.Size = new System.Drawing.Size(105, 32);
            button.TabIndex = 0;
            button.Text = text;
            button.TipsFont = new System.Drawing.Font("Microsoft YaHei", 9F);
        }

        #endregion

        private System.ComponentModel.BackgroundWorker clock;
        private Sunny.UI.UIPanel uiPanel2;
        private Sunny.UI.UIPanel uiPanel1;
        private Sunny.UI.UIImageButton uiImageButton1;
        private Sunny.UI.UINavBar headNav;
        private System.ComponentModel.BackgroundWorker WK1;
        private Sunny.UI.UITableLayoutPanel navTrai;
        private Sunny.UI.UIPanel uiPanel3;
        private Sunny.UI.UINavMenu NavMenu;
        private Sunny.UI.UISymbolLabel opUser;
        private Sunny.UI.UISymbolButton btnHome;
        private Sunny.UI.UILabel opAppClock;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel4;
        private Sunny.UI.UIPanel uiPanel4;
        private Sunny.UI.UITableLayoutPanel banggiua;
        private Sunny.UI.UITabControl MainTab;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel3;
    }
}
