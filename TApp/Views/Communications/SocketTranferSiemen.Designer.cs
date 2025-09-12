namespace TApp.Views.Communications
{
    partial class SocketTranferSiemen
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
            opShow = new Sunny.UI.UIListBox();
            ipPort = new Sunny.UI.UINumPadTextBox();
            btnOpen = new Sunny.UI.UISymbolButton();
            btnSend = new Sunny.UI.UISymbolButton();
            ipConten = new Sunny.UI.UITextBox();
            ipPLCValue = new Sunny.UI.UITextBox();
            btnSendToPLC = new Sunny.UI.UISymbolButton();
            ipPLCMemory = new Sunny.UI.UITextBox();
            ipPLCPort = new Sunny.UI.UINumPadTextBox();
            ipPLCIP = new Sunny.UI.UIIPTextBox();
            btnConnectPLC = new Sunny.UI.UISymbolButton();
            SuspendLayout();
            // 
            // opShow
            // 
            opShow.Font = new Font("Microsoft Sans Serif", 12F);
            opShow.HoverColor = Color.FromArgb(155, 200, 255);
            opShow.ItemSelectForeColor = Color.White;
            opShow.Location = new Point(13, 62);
            opShow.Margin = new Padding(4, 5, 4, 5);
            opShow.MinimumSize = new Size(1, 1);
            opShow.Name = "opShow";
            opShow.Padding = new Padding(2);
            opShow.ShowText = false;
            opShow.Size = new Size(982, 471);
            opShow.TabIndex = 0;
            opShow.Text = "uiListBox1";
            opShow.DoubleClick += opShow_DoubleClick;
            // 
            // ipPort
            // 
            ipPort.FillColor = Color.White;
            ipPort.Font = new Font("Microsoft Sans Serif", 12F);
            ipPort.Location = new Point(13, 14);
            ipPort.Margin = new Padding(4, 5, 4, 5);
            ipPort.MinimumSize = new Size(63, 0);
            ipPort.Name = "ipPort";
            ipPort.Padding = new Padding(0, 0, 30, 2);
            ipPort.Size = new Size(395, 40);
            ipPort.SymbolSize = 24;
            ipPort.TabIndex = 1;
            ipPort.Text = "51234";
            ipPort.TextAlignment = ContentAlignment.MiddleLeft;
            ipPort.Watermark = "";
            // 
            // btnOpen
            // 
            btnOpen.Font = new Font("Microsoft Sans Serif", 12F);
            btnOpen.Location = new Point(415, 14);
            btnOpen.MinimumSize = new Size(1, 1);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(85, 40);
            btnOpen.TabIndex = 2;
            btnOpen.Text = "Mở";
            btnOpen.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnOpen.Click += btnOpen_Click;
            // 
            // btnSend
            // 
            btnSend.Font = new Font("Microsoft Sans Serif", 12F);
            btnSend.Location = new Point(411, 546);
            btnSend.MinimumSize = new Size(1, 1);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(89, 40);
            btnSend.TabIndex = 3;
            btnSend.Text = "Gửi";
            btnSend.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSend.Click += btnSend_Click;
            // 
            // ipConten
            // 
            ipConten.Font = new Font("Microsoft Sans Serif", 12F);
            ipConten.Location = new Point(13, 546);
            ipConten.Margin = new Padding(4, 5, 4, 5);
            ipConten.MinimumSize = new Size(1, 16);
            ipConten.Name = "ipConten";
            ipConten.Padding = new Padding(5);
            ipConten.ShowText = false;
            ipConten.Size = new Size(391, 38);
            ipConten.TabIndex = 4;
            ipConten.TextAlignment = ContentAlignment.MiddleLeft;
            ipConten.Watermark = "";
            // 
            // ipPLCValue
            // 
            ipPLCValue.DoubleValue = 1234D;
            ipPLCValue.Font = new Font("Microsoft Sans Serif", 12F);
            ipPLCValue.IntValue = 1234;
            ipPLCValue.Location = new Point(702, 546);
            ipPLCValue.Margin = new Padding(4, 5, 4, 5);
            ipPLCValue.MinimumSize = new Size(1, 16);
            ipPLCValue.Name = "ipPLCValue";
            ipPLCValue.Padding = new Padding(5);
            ipPLCValue.ShowText = false;
            ipPLCValue.Size = new Size(197, 38);
            ipPLCValue.TabIndex = 6;
            ipPLCValue.Text = "1234";
            ipPLCValue.TextAlignment = ContentAlignment.MiddleLeft;
            ipPLCValue.Watermark = "";
            // 
            // btnSendToPLC
            // 
            btnSendToPLC.Font = new Font("Microsoft Sans Serif", 12F);
            btnSendToPLC.Location = new Point(906, 544);
            btnSendToPLC.MinimumSize = new Size(1, 1);
            btnSendToPLC.Name = "btnSendToPLC";
            btnSendToPLC.Size = new Size(89, 40);
            btnSendToPLC.TabIndex = 5;
            btnSendToPLC.Text = "Gửi";
            btnSendToPLC.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSendToPLC.Click += btnSendToPLC_Click;
            // 
            // ipPLCMemory
            // 
            ipPLCMemory.Font = new Font("Microsoft Sans Serif", 12F);
            ipPLCMemory.Location = new Point(508, 546);
            ipPLCMemory.Margin = new Padding(4, 5, 4, 5);
            ipPLCMemory.MinimumSize = new Size(1, 16);
            ipPLCMemory.Name = "ipPLCMemory";
            ipPLCMemory.Padding = new Padding(5);
            ipPLCMemory.ShowText = false;
            ipPLCMemory.Size = new Size(186, 38);
            ipPLCMemory.TabIndex = 7;
            ipPLCMemory.Text = "M100";
            ipPLCMemory.TextAlignment = ContentAlignment.MiddleLeft;
            ipPLCMemory.Watermark = "";
            // 
            // ipPLCPort
            // 
            ipPLCPort.FillColor = Color.White;
            ipPLCPort.Font = new Font("Microsoft Sans Serif", 12F);
            ipPLCPort.Location = new Point(793, 14);
            ipPLCPort.Margin = new Padding(4, 5, 4, 5);
            ipPLCPort.MinimumSize = new Size(63, 0);
            ipPLCPort.Name = "ipPLCPort";
            ipPLCPort.Padding = new Padding(0, 0, 30, 2);
            ipPLCPort.Size = new Size(95, 40);
            ipPLCPort.SymbolSize = 24;
            ipPLCPort.TabIndex = 2;
            ipPLCPort.Text = "102";
            ipPLCPort.TextAlignment = ContentAlignment.MiddleLeft;
            ipPLCPort.Watermark = "";
            // 
            // ipPLCIP
            // 
            ipPLCIP.FillColor2 = Color.FromArgb(235, 243, 255);
            ipPLCIP.Font = new Font("Microsoft Sans Serif", 12F);
            ipPLCIP.Location = new Point(508, 14);
            ipPLCIP.Margin = new Padding(4, 5, 4, 5);
            ipPLCIP.MinimumSize = new Size(1, 1);
            ipPLCIP.Name = "ipPLCIP";
            ipPLCIP.Padding = new Padding(1);
            ipPLCIP.ShowText = false;
            ipPLCIP.Size = new Size(277, 40);
            ipPLCIP.TabIndex = 3;
            ipPLCIP.Text = "127.0.0.1";
            ipPLCIP.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnConnectPLC
            // 
            btnConnectPLC.Font = new Font("Microsoft Sans Serif", 12F);
            btnConnectPLC.Location = new Point(895, 14);
            btnConnectPLC.MinimumSize = new Size(1, 1);
            btnConnectPLC.Name = "btnConnectPLC";
            btnConnectPLC.Size = new Size(100, 40);
            btnConnectPLC.TabIndex = 8;
            btnConnectPLC.Text = "Kết Nối";
            btnConnectPLC.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnConnectPLC.Click += btnConnectPLC_Click;
            // 
            // SocketTranferSiemen
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = true;
            ClientSize = new Size(1186, 598);
            Controls.Add(btnConnectPLC);
            Controls.Add(ipPLCIP);
            Controls.Add(ipPLCPort);
            Controls.Add(ipPLCMemory);
            Controls.Add(ipPLCValue);
            Controls.Add(btnSendToPLC);
            Controls.Add(ipConten);
            Controls.Add(btnSend);
            Controls.Add(btnOpen);
            Controls.Add(ipPort);
            Controls.Add(opShow);
            Name = "SocketTranferSiemen";
            Symbol = 361926;
            Text = "SocketTranferSiemen";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIListBox opShow;
        private Sunny.UI.UINumPadTextBox ipPort;
        private Sunny.UI.UISymbolButton btnOpen;
        private Sunny.UI.UISymbolButton btnSend;
        private Sunny.UI.UITextBox ipConten;
        private Sunny.UI.UITextBox ipPLCValue;
        private Sunny.UI.UISymbolButton btnSendToPLC;
        private Sunny.UI.UITextBox ipPLCMemory;
        private Sunny.UI.UINumPadTextBox ipPLCPort;
        private Sunny.UI.UIIPTextBox ipPLCIP;
        private Sunny.UI.UISymbolButton btnConnectPLC;
    }
}