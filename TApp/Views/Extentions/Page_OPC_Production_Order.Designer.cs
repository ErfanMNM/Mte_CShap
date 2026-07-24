namespace TApp.Views
{
    partial class Page_OPC_Production_Order
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
            uiListBox1 = new Sunny.UI.UIListBox();
            uiSymbolButton3 = new Sunny.UI.UISymbolButton();
            uiLabel1 = new Sunny.UI.UILabel();
            uiLabel2 = new Sunny.UI.UILabel();
            opPOItem = new Sunny.UI.UILabel();
            opPOLot = new Sunny.UI.UILabel();
            ipPOItem = new Sunny.UI.UITextBox();
            ipPOLot = new Sunny.UI.UITextBox();
            SuspendLayout();
            // 
            // uiListBox1
            // 
            uiListBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiListBox1.HoverColor = Color.FromArgb(155, 200, 255);
            uiListBox1.ItemSelectForeColor = Color.White;
            uiListBox1.Location = new Point(13, 72);
            uiListBox1.Margin = new Padding(4, 5, 4, 5);
            uiListBox1.MinimumSize = new Size(1, 1);
            uiListBox1.Name = "uiListBox1";
            uiListBox1.Padding = new Padding(2);
            uiListBox1.ShowText = false;
            uiListBox1.Size = new Size(1065, 545);
            uiListBox1.TabIndex = 0;
            uiListBox1.Text = "uiListBox1";
            // 
            // uiSymbolButton3
            // 
            uiSymbolButton3.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton3.Location = new Point(559, 12);
            uiSymbolButton3.MinimumSize = new Size(1, 1);
            uiSymbolButton3.Name = "uiSymbolButton3";
            uiSymbolButton3.Size = new Size(157, 58);
            uiSymbolButton3.TabIndex = 3;
            uiSymbolButton3.Text = "Gửi test";
            uiSymbolButton3.TipsFont = new Font("Microsoft Sans Serif", 9F);
            uiSymbolButton3.Click += uiSymbolButton3_Click;
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(13, 12);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(100, 23);
            uiLabel1.TabIndex = 4;
            uiLabel1.Text = "POItem";
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("Microsoft Sans Serif", 12F);
            uiLabel2.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel2.Location = new Point(13, 41);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(100, 23);
            uiLabel2.TabIndex = 5;
            uiLabel2.Text = "POLot";
            // 
            // opPOItem
            // 
            opPOItem.Font = new Font("Microsoft Sans Serif", 12F);
            opPOItem.ForeColor = Color.FromArgb(48, 48, 48);
            opPOItem.Location = new Point(102, 12);
            opPOItem.Name = "opPOItem";
            opPOItem.Size = new Size(100, 23);
            opPOItem.TabIndex = 4;
            opPOItem.Text = "POItem";
            // 
            // opPOLot
            // 
            opPOLot.Font = new Font("Microsoft Sans Serif", 12F);
            opPOLot.ForeColor = Color.FromArgb(48, 48, 48);
            opPOLot.Location = new Point(102, 41);
            opPOLot.Name = "opPOLot";
            opPOLot.Size = new Size(100, 23);
            opPOLot.TabIndex = 5;
            opPOLot.Text = "POLot";
            // 
            // ipPOItem
            // 
            ipPOItem.Font = new Font("Microsoft Sans Serif", 12F);
            ipPOItem.Location = new Point(723, 12);
            ipPOItem.Margin = new Padding(4, 5, 4, 5);
            ipPOItem.MinimumSize = new Size(1, 16);
            ipPOItem.Name = "ipPOItem";
            ipPOItem.Padding = new Padding(5);
            ipPOItem.ShowText = false;
            ipPOItem.Size = new Size(355, 29);
            ipPOItem.TabIndex = 6;
            ipPOItem.Text = "ITEM12345-12345";
            ipPOItem.TextAlignment = ContentAlignment.MiddleLeft;
            ipPOItem.Watermark = "";
            // 
            // ipPOLot
            // 
            ipPOLot.Font = new Font("Microsoft Sans Serif", 12F);
            ipPOLot.Location = new Point(723, 41);
            ipPOLot.Margin = new Padding(4, 5, 4, 5);
            ipPOLot.MinimumSize = new Size(1, 16);
            ipPOLot.Name = "ipPOLot";
            ipPOLot.Padding = new Padding(5);
            ipPOLot.ShowText = false;
            ipPOLot.Size = new Size(355, 29);
            ipPOLot.TabIndex = 7;
            ipPOLot.Text = "LOT-99-66-69";
            ipPOLot.TextAlignment = ContentAlignment.MiddleLeft;
            ipPOLot.Watermark = "";
            // 
            // Page_OPC_Production_Order
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1091, 631);
            Controls.Add(ipPOLot);
            Controls.Add(ipPOItem);
            Controls.Add(opPOLot);
            Controls.Add(opPOItem);
            Controls.Add(uiLabel2);
            Controls.Add(uiLabel1);
            Controls.Add(uiSymbolButton3);
            Controls.Add(uiListBox1);
            Name = "Page_OPC_Production_Order";
            Text = "Page_OPC_Production_Order";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIListBox uiListBox1;
        private Sunny.UI.UISymbolButton uiSymbolButton3;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UILabel opPOItem;
        private Sunny.UI.UILabel opPOLot;
        private Sunny.UI.UITextBox ipPOItem;
        private Sunny.UI.UITextBox ipPOLot;
    }
}