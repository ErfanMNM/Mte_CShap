namespace CProject.Views
{
    partial class PageSocketUI
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
            btnOpenClose = new Sunny.UI.UISymbolButton();
            btnSend = new Sunny.UI.UISymbolButton();
            ipCommand = new Sunny.UI.UITextBox();
            opCommand = new Sunny.UI.UIListBox();
            txtTopic = new Sunny.UI.UITextBox();
            SuspendLayout();
            // 
            // btnOpenClose
            // 
            btnOpenClose.Font = new Font("Microsoft Sans Serif", 12F);
            btnOpenClose.Location = new Point(2, 3);
            btnOpenClose.MinimumSize = new Size(1, 1);
            btnOpenClose.Name = "btnOpenClose";
            btnOpenClose.Size = new Size(183, 48);
            btnOpenClose.TabIndex = 0;
            btnOpenClose.Text = "Mở server";
            btnOpenClose.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnOpenClose.Click += btnOpenClose_Click;
            // 
            // btnSend
            // 
            btnSend.Font = new Font("Microsoft Sans Serif", 12F);
            btnSend.Location = new Point(2, 57);
            btnSend.MinimumSize = new Size(1, 1);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(183, 48);
            btnSend.TabIndex = 1;
            btnSend.Text = "Send";
            btnSend.TipsFont = new Font("Microsoft Sans Serif", 9F);
            btnSend.Click += btnSend_Click;
            // 
            // ipCommand
            // 
            ipCommand.Font = new Font("Microsoft Sans Serif", 12F);
            ipCommand.Location = new Point(192, 57);
            ipCommand.Margin = new Padding(4, 5, 4, 5);
            ipCommand.MinimumSize = new Size(1, 16);
            ipCommand.Name = "ipCommand";
            ipCommand.Padding = new Padding(5);
            ipCommand.ShowText = false;
            ipCommand.Size = new Size(913, 48);
            ipCommand.TabIndex = 2;
            ipCommand.Text = "Nhập lệnh broadcast";
            ipCommand.TextAlignment = ContentAlignment.MiddleLeft;
            ipCommand.Watermark = "";
            // 
            // opCommand
            // 
            opCommand.Font = new Font("Microsoft Sans Serif", 12F);
            opCommand.HoverColor = Color.FromArgb(155, 200, 255);
            opCommand.ItemSelectForeColor = Color.White;
            opCommand.Location = new Point(2, 113);
            opCommand.Margin = new Padding(4, 5, 4, 5);
            opCommand.MinimumSize = new Size(1, 1);
            opCommand.Name = "opCommand";
            opCommand.Padding = new Padding(2);
            opCommand.ShowText = false;
            opCommand.Size = new Size(1103, 578);
            opCommand.TabIndex = 5;
            opCommand.Text = "uiListBox1";
            // 
            // txtTopic
            // 
            txtTopic.Font = new Font("Microsoft Sans Serif", 12F);
            txtTopic.Location = new Point(192, 3);
            txtTopic.Margin = new Padding(4, 5, 4, 5);
            txtTopic.MinimumSize = new Size(1, 16);
            txtTopic.Name = "txtTopic";
            txtTopic.Padding = new Padding(5);
            txtTopic.ShowText = false;
            txtTopic.Size = new Size(913, 48);
            txtTopic.TabIndex = 3;
            txtTopic.Text = "topic name";
            txtTopic.TextAlignment = ContentAlignment.MiddleLeft;
            txtTopic.Watermark = "";
            // 
            // PageSocketUI
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1130, 686);
            Controls.Add(txtTopic);
            Controls.Add(opCommand);
            Controls.Add(ipCommand);
            Controls.Add(btnSend);
            Controls.Add(btnOpenClose);
            Name = "PageSocketUI";
            Text = "PageSocketUI";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UISymbolButton btnOpenClose;
        private Sunny.UI.UISymbolButton btnSend;
        private Sunny.UI.UITextBox ipCommand;
        private Sunny.UI.UIListBox opCommand;
        private Sunny.UI.UITextBox txtTopic;
    }
}
