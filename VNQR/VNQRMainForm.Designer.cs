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
            components = new System.ComponentModel.Container();
            listBox1 = new ListBox();
            omronplC_Hsl1 = new TTManager.PLCHelpers.OmronPLC_Hsl(components);
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(203, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(1001, 529);
            listBox1.TabIndex = 0;
            // 
            // omronplC_Hsl1
            // 
            omronplC_Hsl1.PLC_IP = "127.0.0.1";
            omronplC_Hsl1.PLC_PORT = 9600;
            omronplC_Hsl1.PLC_Ready_DM = "D16";
            omronplC_Hsl1.PLC_STATUS = TTManager.PLCHelpers.OmronPLC_Hsl.PLCStatus.Disconnect;
            omronplC_Hsl1.Ready = 0;
            omronplC_Hsl1.Time_Update = 300;
            omronplC_Hsl1.PLCStatus_OnChange += omronplC_Hsl1_PLCStatus_OnChange;
            // 
            // VNQRMainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1216, 550);
            Controls.Add(listBox1);
            Name = "VNQRMainForm";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox1;
        private TTManager.PLCHelpers.OmronPLC_Hsl omronplC_Hsl1;
    }
}
