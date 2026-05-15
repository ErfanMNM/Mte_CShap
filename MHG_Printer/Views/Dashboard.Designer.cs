namespace MHG_Printer.Views
{
    partial class Dashboard
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
            uiTextBox1 = new Sunny.UI.UITextBox();
            SuspendLayout();
            // 
            // uiTextBox1
            // 
            uiTextBox1.Font = new Font("Microsoft Sans Serif", 12F);
            uiTextBox1.Location = new Point(182, 143);
            uiTextBox1.Margin = new Padding(4, 5, 4, 5);
            uiTextBox1.MinimumSize = new Size(1, 16);
            uiTextBox1.Name = "uiTextBox1";
            uiTextBox1.Padding = new Padding(5);
            uiTextBox1.ShowText = false;
            uiTextBox1.Size = new Size(659, 246);
            uiTextBox1.TabIndex = 0;
            uiTextBox1.Text = "uiTextBox1";
            uiTextBox1.TextAlignment = ContentAlignment.MiddleLeft;
            uiTextBox1.Watermark = "";
            // 
            // Dashboard
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(874, 679);
            Controls.Add(uiTextBox1);
            Name = "Dashboard";
            Text = "Bảng chính";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITextBox uiTextBox1;
    }
}
