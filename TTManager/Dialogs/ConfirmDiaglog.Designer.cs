namespace TTManager.Dialogs
{
    partial class ConfirmDiaglog
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
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            opNoti = new Sunny.UI.UIRichTextBox();
            uiTitlePanel2 = new Sunny.UI.UITitlePanel();
            uiTitlePanel3 = new Sunny.UI.UITitlePanel();
            ipTwoFA = new Sunny.UI.UINumPadTextBox();
            ipUserName = new Sunny.UI.UIComboBox();
            btnExit = new Sunny.UI.UISymbolButton();
            btnOK = new Sunny.UI.UISymbolButton();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            uiTableLayoutPanel2.SuspendLayout();
            uiTitlePanel2.SuspendLayout();
            uiTitlePanel3.SuspendLayout();
            SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel1);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiTitlePanel1.Location = new Point(0, 0);
            uiTitlePanel1.Margin = new Padding(4, 5, 4, 5);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 45, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(495, 338);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "THÔNG BÁO";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel1.TitleHeight = 45;
            // 
            // uiTableLayoutPanel1
            // 
            uiTableLayoutPanel1.BackColor = Color.Transparent;
            uiTableLayoutPanel1.ColumnCount = 1;
            uiTableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel5, 0, 1);
            uiTableLayoutPanel1.Controls.Add(uiTableLayoutPanel2, 0, 2);
            uiTableLayoutPanel1.Controls.Add(opNoti, 0, 0);
            uiTableLayoutPanel1.Dock = DockStyle.Fill;
            uiTableLayoutPanel1.Location = new Point(1, 45);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 3;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 139F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 62.7451F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 37.2549F));
            uiTableLayoutPanel1.Size = new Size(493, 292);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 177F));
            uiTableLayoutPanel5.Controls.Add(uiTitlePanel3, 1, 0);
            uiTableLayoutPanel5.Controls.Add(uiTitlePanel2, 0, 0);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(2, 141);
            uiTableLayoutPanel5.Margin = new Padding(2);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 1;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel5.Size = new Size(489, 92);
            uiTableLayoutPanel5.TabIndex = 2;
            uiTableLayoutPanel5.TagString = null;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 3;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 123F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 117F));
            uiTableLayoutPanel2.Controls.Add(btnExit, 2, 0);
            uiTableLayoutPanel2.Controls.Add(btnOK, 1, 0);
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(2, 237);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(489, 53);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // opNoti
            // 
            opNoti.Dock = DockStyle.Fill;
            opNoti.FillColor = Color.White;
            opNoti.Font = new Font("Microsoft Sans Serif", 12F);
            opNoti.Location = new Point(2, 2);
            opNoti.Margin = new Padding(2);
            opNoti.MinimumSize = new Size(1, 1);
            opNoti.Name = "opNoti";
            opNoti.Padding = new Padding(2);
            opNoti.ShowText = false;
            opNoti.Size = new Size(489, 135);
            opNoti.TabIndex = 3;
            opNoti.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiTitlePanel2
            // 
            uiTitlePanel2.Controls.Add(ipUserName);
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel2.Location = new Point(2, 2);
            uiTitlePanel2.Margin = new Padding(2);
            uiTitlePanel2.MinimumSize = new Size(1, 1);
            uiTitlePanel2.Name = "uiTitlePanel2";
            uiTitlePanel2.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel2.RectColor = Color.FromArgb(0, 192, 192);
            uiTitlePanel2.ShowText = false;
            uiTitlePanel2.Size = new Size(308, 88);
            uiTitlePanel2.TabIndex = 2;
            uiTitlePanel2.Text = "Chọn người dùng";
            uiTitlePanel2.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel2.TitleColor = Color.FromArgb(0, 192, 192);
            // 
            // uiTitlePanel3
            // 
            uiTitlePanel3.Controls.Add(ipTwoFA);
            uiTitlePanel3.Dock = DockStyle.Fill;
            uiTitlePanel3.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel3.Location = new Point(314, 2);
            uiTitlePanel3.Margin = new Padding(2);
            uiTitlePanel3.MinimumSize = new Size(1, 1);
            uiTitlePanel3.Name = "uiTitlePanel3";
            uiTitlePanel3.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel3.RectColor = Color.FromArgb(0, 192, 192);
            uiTitlePanel3.ShowText = false;
            uiTitlePanel3.Size = new Size(173, 88);
            uiTitlePanel3.TabIndex = 3;
            uiTitlePanel3.Text = "Mã xác thực";
            uiTitlePanel3.TextAlignment = ContentAlignment.MiddleCenter;
            uiTitlePanel3.TitleColor = Color.FromArgb(0, 192, 192);
            // 
            // ipTwoFA
            // 
            ipTwoFA.Dock = DockStyle.Fill;
            ipTwoFA.FillColor = Color.White;
            ipTwoFA.Font = new Font("Microsoft Sans Serif", 12F);
            ipTwoFA.Location = new Point(1, 35);
            ipTwoFA.Margin = new Padding(2);
            ipTwoFA.MinimumSize = new Size(63, 0);
            ipTwoFA.Name = "ipTwoFA";
            ipTwoFA.Padding = new Padding(0, 0, 30, 2);
            ipTwoFA.Size = new Size(171, 52);
            ipTwoFA.SymbolNormal = 557532;
            ipTwoFA.SymbolSize = 24;
            ipTwoFA.TabIndex = 0;
            ipTwoFA.TextAlignment = ContentAlignment.MiddleLeft;
            ipTwoFA.Watermark = "";
            // 
            // ipUserName
            // 
            ipUserName.DataSource = null;
            ipUserName.Dock = DockStyle.Fill;
            ipUserName.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            ipUserName.FillColor = Color.White;
            ipUserName.Font = new Font("Microsoft Sans Serif", 12F);
            ipUserName.ItemHoverColor = Color.FromArgb(155, 200, 255);
            ipUserName.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            ipUserName.Location = new Point(1, 35);
            ipUserName.Margin = new Padding(2);
            ipUserName.MinimumSize = new Size(63, 0);
            ipUserName.Name = "ipUserName";
            ipUserName.Padding = new Padding(0, 0, 30, 2);
            ipUserName.Size = new Size(306, 52);
            ipUserName.SymbolSize = 24;
            ipUserName.TabIndex = 0;
            ipUserName.TextAlignment = ContentAlignment.MiddleLeft;
            ipUserName.Watermark = "";
            // 
            // btnExit
            // 
            btnExit.Dock = DockStyle.Fill;
            btnExit.FillColor = Color.FromArgb(192, 64, 0);
            btnExit.Font = new Font("Microsoft Sans Serif", 12F);
            btnExit.Location = new Point(375, 3);
            btnExit.MinimumSize = new Size(1, 1);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(111, 47);
            btnExit.Symbol = 61453;
            btnExit.TabIndex = 0;
            btnExit.Text = "Thoát";
            btnExit.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnExit.Click += uiSymbolButton1_Click;
            // 
            // btnOK
            // 
            btnOK.Dock = DockStyle.Fill;
            btnOK.FillColor = Color.FromArgb(0, 192, 0);
            btnOK.Font = new Font("Microsoft Sans Serif", 12F);
            btnOK.Location = new Point(252, 3);
            btnOK.MinimumSize = new Size(1, 1);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(117, 47);
            btnOK.Symbol = 61452;
            btnOK.TabIndex = 0;
            btnOK.Text = "Đồng ý";
            btnOK.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnOK.Click += uiSymbolButton1_Click;
            // 
            // ConfirmDiaglog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(495, 338);
            Controls.Add(uiTitlePanel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ConfirmDiaglog";
            StartPosition = FormStartPosition.CenterScreen;
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            uiTableLayoutPanel2.ResumeLayout(false);
            uiTitlePanel2.ResumeLayout(false);
            uiTitlePanel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UIRichTextBox opNoti;
        private Sunny.UI.UITitlePanel uiTitlePanel2;
        private Sunny.UI.UITitlePanel uiTitlePanel3;
        private Sunny.UI.UINumPadTextBox ipTwoFA;
        private Sunny.UI.UIComboBox ipUserName;
        private Sunny.UI.UISymbolButton btnExit;
        private Sunny.UI.UISymbolButton btnOK;
    }
}