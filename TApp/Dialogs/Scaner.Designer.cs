namespace TApp.Dialogs
{
    partial class Scaner
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
            components = new System.ComponentModel.Container();
            System.Text.ASCIIEncoding asciiEncodingSealed1 = new System.Text.ASCIIEncoding();
            System.Text.DecoderReplacementFallback decoderReplacementFallback1 = new System.Text.DecoderReplacementFallback();
            System.Text.EncoderReplacementFallback encoderReplacementFallback1 = new System.Text.EncoderReplacementFallback();
            uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            uiRichTextBox1 = new Sunny.UI.UIRichTextBox();
            btnOK = new Sunny.UI.UISymbolButton();
            btnCancel = new Sunny.UI.UISymbolButton();
            pnConnect = new Sunny.UI.UIPanel();
            serialPort1 = new System.IO.Ports.SerialPort(components);
            uiTitlePanel1.SuspendLayout();
            SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            uiTitlePanel1.Controls.Add(uiRichTextBox1);
            uiTitlePanel1.Controls.Add(btnOK);
            uiTitlePanel1.Controls.Add(btnCancel);
            uiTitlePanel1.Controls.Add(pnConnect);
            uiTitlePanel1.Dock = DockStyle.Fill;
            uiTitlePanel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTitlePanel1.Location = new Point(0, 0);
            uiTitlePanel1.Margin = new Padding(4, 5, 4, 5);
            uiTitlePanel1.MinimumSize = new Size(1, 1);
            uiTitlePanel1.Name = "uiTitlePanel1";
            uiTitlePanel1.Padding = new Padding(1, 35, 1, 1);
            uiTitlePanel1.ShowText = false;
            uiTitlePanel1.Size = new Size(583, 183);
            uiTitlePanel1.TabIndex = 0;
            uiTitlePanel1.Text = "SCANER";
            uiTitlePanel1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // uiRichTextBox1
            // 
            uiRichTextBox1.FillColor = Color.White;
            uiRichTextBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiRichTextBox1.Location = new Point(4, 40);
            uiRichTextBox1.Margin = new Padding(4, 5, 4, 5);
            uiRichTextBox1.MinimumSize = new Size(1, 1);
            uiRichTextBox1.Name = "uiRichTextBox1";
            uiRichTextBox1.Padding = new Padding(2);
            uiRichTextBox1.ShowText = false;
            uiRichTextBox1.Size = new Size(576, 79);
            uiRichTextBox1.TabIndex = 3;
            uiRichTextBox1.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // btnOK
            // 
            btnOK.Cursor = Cursors.Hand;
            btnOK.Font = new Font("Microsoft Sans Serif", 12F);
            btnOK.Location = new Point(436, 127);
            btnOK.MinimumSize = new Size(1, 1);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(144, 51);
            btnOK.Symbol = 61452;
            btnOK.TabIndex = 2;
            btnOK.Text = "Lưu lại";
            btnOK.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Font = new Font("Microsoft Sans Serif", 12F);
            btnCancel.Location = new Point(285, 127);
            btnCancel.MinimumSize = new Size(1, 1);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(145, 51);
            btnCancel.Symbol = 61453;
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Thoát";
            btnCancel.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnCancel.Click += btnCancel_Click;
            // 
            // pnConnect
            // 
            pnConnect.Font = new Font("Microsoft Sans Serif", 12F);
            pnConnect.Location = new Point(4, 127);
            pnConnect.Margin = new Padding(4, 5, 4, 5);
            pnConnect.MinimumSize = new Size(1, 1);
            pnConnect.Name = "pnConnect";
            pnConnect.Size = new Size(274, 51);
            pnConnect.TabIndex = 0;
            pnConnect.Text = "Mất kết nối";
            pnConnect.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // serialPort1
            // 
            serialPort1.BaudRate = 9600;
            serialPort1.DataBits = 8;
            serialPort1.DiscardNull = false;
            serialPort1.DtrEnable = false;
            asciiEncodingSealed1.DecoderFallback = decoderReplacementFallback1;
            asciiEncodingSealed1.EncoderFallback = encoderReplacementFallback1;
            serialPort1.Encoding = asciiEncodingSealed1;
            serialPort1.Handshake = System.IO.Ports.Handshake.None;
            serialPort1.NewLine = "\n";
            serialPort1.Parity = System.IO.Ports.Parity.None;
            serialPort1.ParityReplace = 63;
            serialPort1.PortName = "COM1";
            serialPort1.ReadBufferSize = 4096;
            serialPort1.ReadTimeout = -1;
            serialPort1.ReceivedBytesThreshold = 1;
            serialPort1.RtsEnable = false;
            serialPort1.StopBits = System.IO.Ports.StopBits.One;
            serialPort1.WriteBufferSize = 2048;
            serialPort1.WriteTimeout = -1;
            // 
            // Scaner
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(583, 183);
            Controls.Add(uiTitlePanel1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Scaner";
            Text = "Scaner";
            Load += Scaner_Load;
            uiTitlePanel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITitlePanel uiTitlePanel1;
        private Sunny.UI.UIPanel pnConnect;
        private Sunny.UI.UISymbolButton btnOK;
        private Sunny.UI.UISymbolButton btnCancel;
        private Sunny.UI.UIRichTextBox uiRichTextBox1;
        private System.IO.Ports.SerialPort serialPort1;
    }
}