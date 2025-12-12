namespace TApp.Views.Auth
{
    partial class FDeactive
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
            btnReactivate = new Sunny.UI.UISymbolButton();
            uiLabel2 = new Sunny.UI.UILabel();
            SuspendLayout();
            // 
            // uiLabel1
            // 
            uiLabel1.Font = new Font("Tahoma", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            uiLabel1.ForeColor = Color.Red;
            uiLabel1.Location = new Point(141, 150);
            uiLabel1.Name = "uiLabel1";
            uiLabel1.Size = new Size(632, 60);
            uiLabel1.TabIndex = 0;
            uiLabel1.Text = "HỆ THỐNG ĐANG BỊ VÔ HIỆU HÓA";
            uiLabel1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnReactivate
            // 
            btnReactivate.Font = new Font("Microsoft YaHei", 14F);
            btnReactivate.Location = new Point(227, 320);
            btnReactivate.MinimumSize = new Size(1, 1);
            btnReactivate.Name = "btnReactivate";
            btnReactivate.Size = new Size(453, 100);
            btnReactivate.Symbol = 61528;
            btnReactivate.TabIndex = 2;
            btnReactivate.Text = "KÍCH HOẠT LẠI";
            btnReactivate.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnReactivate.Click += btnReactivate_Click;
            // 
            // uiLabel2
            // 
            uiLabel2.Font = new Font("Microsoft YaHei", 14F);
            uiLabel2.ForeColor = Color.Orange;
            uiLabel2.Location = new Point(204, 234);
            uiLabel2.Name = "uiLabel2";
            uiLabel2.Size = new Size(500, 40);
            uiLabel2.TabIndex = 1;
            uiLabel2.Text = "Mọi hoạt động đã bị dừng. Vui lòng liên hệ quản trị viên.";
            uiLabel2.TextAlign = ContentAlignment.MiddleCenter;
            uiLabel2.Click += uiLabel2_Click;
            // 
            // FDeactive
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(900, 636);
            Controls.Add(btnReactivate);
            Controls.Add(uiLabel2);
            Controls.Add(uiLabel1);
            Name = "FDeactive";
            Text = "Vô Hiệu Hóa";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UISymbolButton btnReactivate;
        private Sunny.UI.UILabel uiLabel2;
    }
}

