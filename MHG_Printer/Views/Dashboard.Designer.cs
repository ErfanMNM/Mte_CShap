namespace MHG_Printer.Views
{
    partial class Dashboard
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
            uiLabel1 = new Sunny.UI.UILabel();
            SuspendLayout();
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("Microsoft Sans Serif", 12F);
            uiLabel1.ForeColor = Color.FromArgb(48, 48, 48);
            uiLabel1.Location = new Point(324, 187);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(378, 185);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "Đây là Dashboard";
            // 
            // Dashboard
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1015, 621);
            Controls.Add(uiLabel1);
            Name = "Dashboard";
            Text = "Dashboard";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UILabel uiLabel1;
    }
}