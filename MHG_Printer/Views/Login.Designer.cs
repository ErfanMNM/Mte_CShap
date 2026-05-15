namespace MHG_Printer.Views
{
    partial class Login
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
            ucLogin1 = new TTManager.Auth.ucLogin();
            SuspendLayout();
            // 
            // ucLogin1
            // 
            ucLogin1.data_file_path = null;
            ucLogin1.IS2FAEnabled = true;
            ucLogin1.Location = new Point(109, 135);
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
            Initialize += Login_Initialize;
            ResumeLayout(false);
        }

        #endregion

        private TTManager.Auth.ucLogin ucLogin1;
    }
}
