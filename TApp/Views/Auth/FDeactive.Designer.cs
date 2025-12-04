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
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.btnReactivate = new Sunny.UI.UISymbolButton();
            this.SuspendLayout();
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("Microsoft YaHei", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.uiLabel1.ForeColor = System.Drawing.Color.Red;
            this.uiLabel1.Location = new System.Drawing.Point(200, 150);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(500, 60);
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "HỆ THỐNG ĐANG BỊ VÔ HIỆU HÓA";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("Microsoft YaHei", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.uiLabel2.ForeColor = System.Drawing.Color.Orange;
            this.uiLabel2.Location = new System.Drawing.Point(200, 230);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(500, 40);
            this.uiLabel2.TabIndex = 1;
            this.uiLabel2.Text = "Mọi hoạt động đã bị dừng. Vui lòng liên hệ quản trị viên.";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnReactivate
            // 
            this.btnReactivate.Font = new System.Drawing.Font("Microsoft YaHei", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnReactivate.Location = new System.Drawing.Point(350, 320);
            this.btnReactivate.Name = "btnReactivate";
            this.btnReactivate.Size = new System.Drawing.Size(200, 50);
            this.btnReactivate.Symbol = 61528;
            this.btnReactivate.TabIndex = 2;
            this.btnReactivate.Text = "KÍCH HOẠT LẠI";
            this.btnReactivate.Click += new System.EventHandler(this.btnReactivate_Click);
            // 
            // FDeactive
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(900, 636);
            this.Controls.Add(this.btnReactivate);
            this.Controls.Add(this.uiLabel2);
            this.Controls.Add(this.uiLabel1);
            this.Name = "FDeactive";
            this.Text = "Vô Hiệu Hóa";
            this.ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UISymbolButton btnReactivate;
    }
}

