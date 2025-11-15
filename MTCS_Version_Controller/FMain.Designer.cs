namespace MTCS_Version_Controller
{
    partial class FMain
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
            uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiProcessBar1 = new Sunny.UI.UIProcessBar();
            uiTableLayoutPanel1.SuspendLayout();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.Controls.Add(uiTitlePanel1, 0, 0);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(0, 0);
            uiTableLayoutPanel1.Margin = new Padding(0);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 2;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            uiTableLayoutPanel1.Size = new Size(697, 399);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel2);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(2, 0);
            uiTitlePanel1.Margin = new Padding(2, 0, 0, 2);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(695, 341);
            uiTitlePanel1.TabIndex = 1;
            uiTitlePanel1.Text = "Thông tin hiện tại";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 1;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel2.Controls.Add(uiProcessBar1, 0, 1);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(1, 35);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 2;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 83.8488F));
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 16.1512032F));
            uiTableLayoutPanel2.Size = new Size(693, 305);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiProcessBar1
            // 
            uiProcessBar1.FillColor = Color.FromArgb(235, 243, 255);
            uiProcessBar1.Font = new Font("Microsoft Sans Serif", 12F);
            uiProcessBar1.Location = new Point(3, 258);
            uiProcessBar1.MinimumSize = new Size(3, 3);
            uiProcessBar1.Name = "uiProcessBar1";
            uiProcessBar1.Size = new Size(687, 44);
            uiProcessBar1.TabIndex = 0;
            // 
            // FMain
            // 
            AllowShowTitle = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(697, 399);
            Controls.Add(uiTableLayoutPanel1);
            Name = "FMain";
            Padding = new Padding(0);
            ShowIcon = false;
            ShowInTaskbar = false;
            ShowTitle = false;
            Text = "Quản Trị Phiên Bản Hệ Thống";
            ZoomScaleRect = new Rectangle(15, 15, 981, 590);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UIProcessBar uiProcessBar1;
    }
}
