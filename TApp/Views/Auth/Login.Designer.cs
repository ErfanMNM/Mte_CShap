namespace TApp.Views.Auth
{
    partial class Login
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
            ucLogin1 = new TTManager.Auth.ucLogin();
            SuspendLayout();
            // 
            // ucLogin1
            // 
            //ucLogin1.data_file_path = "C:\\Users\\THUC\\AppData\\Local\\TanTien\\Users\\users.database";
            ucLogin1.IS2FAEnabled = true;
            ucLogin1.Location = new Point(107, 159);
            ucLogin1.Name = "ucLogin1";
            ucLogin1.Size = new Size(623, 315);
            ucLogin1.TabIndex = 0;
            ucLogin1.OnLoginAction += ucLogin1_OnLoginAction;
            // 
            // Login
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(892, 635);
            Controls.Add(ucLogin1);
            Name = "Login";
            Text = "Đăng Nhập";
            ZoomScaleRect = new Rectangle(15, 15, 889, 507);
            ResumeLayout(false);
        }

        #endregion

        private TTManager.Auth.ucLogin ucLogin1;
    }
}