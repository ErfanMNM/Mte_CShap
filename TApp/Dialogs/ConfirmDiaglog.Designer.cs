namespace TApp.Dialogs
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
            uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            uiTableLayoutPanel5 = new Sunny.UI.UITableLayoutPanel();
            opNoti = new Sunny.UI.UIRichTextBox();
            uiComboBox1 = new Sunny.UI.UIComboBox();
            uiNumPadTextBox1 = new Sunny.UI.UINumPadTextBox();
            uiTitlePanel1.SuspendLayout();
            uiTableLayoutPanel1.SuspendLayout();
            uiTableLayoutPanel5.SuspendLayout();
            SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiTableLayoutPanel1);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(0, 0);
            uiTitlePanel1.Margin = new Padding(4, 5, 4, 5);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(416, 282);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "Bảng thông tin";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
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
            uiTableLayoutPanel1.Location = new Point(1, 35);
            uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            uiTableLayoutPanel1.RowCount = 3;
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 139F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 53.2710266F));
            uiTableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 46.7289734F));
            uiTableLayoutPanel1.Size = new Size(414, 246);
            uiTableLayoutPanel1.TabIndex = 0;
            uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel2
            // 
            uiTableLayoutPanel2.ColumnCount = 3;
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 123F));
            uiTableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 117F));
            uiTableLayoutPanel2.Dock = DockStyle.Fill;
            uiTableLayoutPanel2.Location = new Point(2, 198);
            uiTableLayoutPanel2.Margin = new Padding(2);
            uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            uiTableLayoutPanel2.RowCount = 1;
            uiTableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel2.Size = new Size(410, 46);
            uiTableLayoutPanel2.TabIndex = 0;
            uiTableLayoutPanel2.TagString = null;
            // 
            // uiTableLayoutPanel5
            // 
            uiTableLayoutPanel5.ColumnCount = 2;
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            uiTableLayoutPanel5.Controls.Add(uiComboBox1, 0, 0);
            uiTableLayoutPanel5.Controls.Add(uiNumPadTextBox1, 1, 0);
            uiTableLayoutPanel5.Dock = DockStyle.Fill;
            uiTableLayoutPanel5.Location = new Point(2, 141);
            uiTableLayoutPanel5.Margin = new Padding(2);
            uiTableLayoutPanel5.Name = "uiTableLayoutPanel5";
            uiTableLayoutPanel5.RowCount = 1;
            uiTableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            uiTableLayoutPanel5.Size = new Size(410, 53);
            uiTableLayoutPanel5.TabIndex = 2;
            uiTableLayoutPanel5.TagString = null;
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
            opNoti.Size = new Size(410, 135);
            opNoti.TabIndex = 3;
            opNoti.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiComboBox1
            // 
            uiComboBox1.DataSource = null;
            uiComboBox1.Dock = DockStyle.Fill;
            uiComboBox1.FillColor = Color.White;
            uiComboBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiComboBox1.ItemHoverColor = Color.FromArgb(155, 200, 255);
            uiComboBox1.ItemSelectForeColor = Color.FromArgb(235, 243, 255);
            uiComboBox1.Location = new Point(2, 2);
            uiComboBox1.Margin = new Padding(2);
            uiComboBox1.MinimumSize = new Size(63, 0);
            uiComboBox1.Name = "uiComboBox1";
            uiComboBox1.Padding = new Padding(0, 0, 30, 2);
            uiComboBox1.Size = new Size(206, 49);
            uiComboBox1.SymbolSize = 24;
            uiComboBox1.TabIndex = 0;
            uiComboBox1.Text = "UserX";
            uiComboBox1.TextAlignment = ContentAlignment.MiddleLeft;
            uiComboBox1.Watermark = "";
            // 
            // uiNumPadTextBox1
            // 
            uiNumPadTextBox1.Dock = DockStyle.Fill;
            uiNumPadTextBox1.FillColor = Color.White;
            uiNumPadTextBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiNumPadTextBox1.Location = new Point(212, 2);
            uiNumPadTextBox1.Margin = new Padding(2);
            uiNumPadTextBox1.Maximum = 999999D;
            uiNumPadTextBox1.MaxLength = 18;
            uiNumPadTextBox1.Minimum = 0D;
            uiNumPadTextBox1.MinimumSize = new Size(63, 0);
            uiNumPadTextBox1.Name = "uiNumPadTextBox1";
            uiNumPadTextBox1.NumPadType = Sunny.UI.NumPadType.IDNumber;
            uiNumPadTextBox1.Padding = new Padding(0, 0, 30, 2);
            uiNumPadTextBox1.Size = new Size(196, 49);
            uiNumPadTextBox1.SymbolNormal = 557532;
            uiNumPadTextBox1.SymbolSize = 24;
            uiNumPadTextBox1.TabIndex = 1;
            uiNumPadTextBox1.TextAlignment = ContentAlignment.MiddleLeft;
            uiNumPadTextBox1.Watermark = "";
            // 
            // ConfirmDiaglog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(416, 282);
            Controls.Add(uiTitlePanel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ConfirmDiaglog";
            StartPosition = FormStartPosition.CenterScreen;
            uiTitlePanel1.ResumeLayout(false);
            uiTableLayoutPanel1.ResumeLayout(false);
            uiTableLayoutPanel5.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel5;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UIRichTextBox opNoti;
        private Sunny.UI.UIComboBox uiComboBox1;
        private Sunny.UI.UINumPadTextBox uiNumPadTextBox1;
    }
}