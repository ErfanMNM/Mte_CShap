namespace VNQR
{
    partial class VNQRMainForm
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
            listBox1 = new ListBox();
            mainWK = new System.ComponentModel.BackgroundWorker();
            updateWK = new System.ComponentModel.BackgroundWorker();
            uiLabel1 = new Sunny.UI.UILabel();
            opAppStatus = new Sunny.UI.UILabel();
            uiNavMenu1 = new Sunny.UI.UINavMenu();
            uiTabControl1 = new Sunny.UI.UITabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            uiTabControl2 = new Sunny.UI.UITabControl();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            uiTabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            uiTabControl2.SuspendLayout();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(279, 3);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(944, 564);
            listBox1.TabIndex = 0;
            // 
            // mainWK
            // 
            mainWK.WorkerSupportsCancellation = true;
            mainWK.DoWork += mainWK_DoWork;
            // 
            // updateWK
            // 
            updateWK.WorkerSupportsCancellation = true;
            updateWK.DoWork += updateWK_DoWork;
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(3, 9);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(128, 23);
            uiLabel1.TabIndex = 1;
            uiLabel1.Text = "Trạng Thái App";
            // 
            // opAppStatus
            // 
            opAppStatus.Font = new Font("Microsoft Sans Serif", 12F);
            opAppStatus.ForeColor = Color.FromArgb(48, 48, 48);
            opAppStatus.Location = new Point(128, 9);
            opAppStatus.Name = "opAppStatus";
            opAppStatus.Size = new Size(100, 23);
            opAppStatus.TabIndex = 1;
            opAppStatus.Text = "uiLabel1";
            // 
            // uiNavMenu1
            // 
            uiNavMenu1.BorderStyle = BorderStyle.None;
            uiNavMenu1.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            uiNavMenu1.Font = new Font("Microsoft Sans Serif", 12F);
            uiNavMenu1.FullRowSelect = true;
            uiNavMenu1.HotTracking = true;
            uiNavMenu1.ItemHeight = 50;
            uiNavMenu1.Location = new Point(3, 3);
            uiNavMenu1.Name = "uiNavMenu1";
            uiNavMenu1.ShowLines = false;
            uiNavMenu1.ShowPlusMinus = false;
            uiNavMenu1.ShowRootLines = false;
            uiNavMenu1.Size = new Size(173, 571);
            uiNavMenu1.TabIndex = 2;
            uiNavMenu1.TipsFont = new Font("Microsoft Sans Serif", 9F);
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
            uiTabControl1.Size = new Size(1226, 617);
            uiTabControl1.SizeMode = TabSizeMode.Fixed;
            uiTabControl1.TabIndex = 3;
            uiTabControl1.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            uiTabControl1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(uiLabel1);
            tabPage1.Controls.Add(listBox1);
            tabPage1.Controls.Add(opAppStatus);
            tabPage1.Location = new Point(0, 40);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(1226, 577);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(uiTabControl2);
            tabPage2.Controls.Add(uiNavMenu1);
            tabPage2.Location = new Point(0, 40);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(1226, 577);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // uiTabControl2
            // 
            uiTabControl2.Controls.Add(tabPage3);
            uiTabControl2.Controls.Add(tabPage4);
            uiTabControl2.DrawMode = TabDrawMode.OwnerDrawFixed;
            uiTabControl2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTabControl2.ItemSize = new Size(0, 1);
            uiTabControl2.Location = new Point(182, 3);
            uiTabControl2.MainPage = "";
            uiTabControl2.Name = "uiTabControl2";
            uiTabControl2.SelectedIndex = 0;
            uiTabControl2.Size = new Size(1041, 571);
            uiTabControl2.SizeMode = TabSizeMode.Fixed;
            uiTabControl2.TabIndex = 3;
            uiTabControl2.TabUnSelectedForeColor = Color.FromArgb(240, 240, 240);
            uiTabControl2.TabVisible = false;
            uiTabControl2.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(0, 0);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(1041, 571);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "tabPage3";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(0, 40);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(200, 60);
            tabPage4.TabIndex = 1;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // VNQRMainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1226, 652);
            Controls.Add(uiTabControl1);
            Name = "VNQRMainForm";
            Text = "Form1";
            ZoomScaleRect = new Rectangle(15, 15, 1216, 550);
            Load += VNQRMainForm_Load;
            uiTabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            uiTabControl2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox1;
        private TTManager.PLCHelpers.OmronPLC_Hsl omronplC_Hsl1;
        private System.ComponentModel.BackgroundWorker mainWK;
        private System.ComponentModel.BackgroundWorker updateWK;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel opAppStatus;
        private Sunny.UI.UINavMenu uiNavMenu1;
        private Sunny.UI.UITabControl uiTabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Sunny.UI.UITabControl uiTabControl2;
        private TabPage tabPage3;
        private TabPage tabPage4;
    }
}
