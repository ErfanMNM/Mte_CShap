namespace MVSC
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
            trayMenu = new MenuStrip();
            SuspendLayout();
            // 
            // trayMenu
            // 
            trayMenu.Location = new Point(0, 0);
            trayMenu.Name = "trayMenu";
            trayMenu.Size = new Size(803, 24);
            trayMenu.TabIndex = 0;
            trayMenu.Text = "menuStrip1";
            // 
            // FMain
            // 
            AllowShowTitle = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(803, 445);
            Controls.Add(trayMenu);
            MainMenuStrip = trayMenu;
            Name = "FMain";
            Padding = new Padding(0);
            ShowIcon = false;
            ShowInTaskbar = false;
            ShowTitle = false;
            Text = "";
            ZoomScaleRect = new Rectangle(15, 15, 629, 433);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip trayMenu;
    }
}
