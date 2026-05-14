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
            SuspendLayout();
            // 
            // M2_MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1024, 768);
            Font = new Font("Tahoma", 10F);
            Name = "M2_MainForm";
            Text = "M2 - OPC UA Client Test";
            ZoomScaleRect = new Rectangle(15, 15, 800, 450);
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
    }
}
