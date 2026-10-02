namespace TApp.Views.Extention
{
    partial class frmMain
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
            label1 = new Label();
            lblTag = new Label();
            label2 = new Label();
            groupBox1 = new GroupBox();
            radiobtnPass = new RadioButton();
            ratiobtnFail = new RadioButton();
            chkbTestMode = new CheckBox();
            panel1 = new Panel();
            numericUpDown3 = new NumericUpDown();
            numericUpDown2 = new NumericUpDown();
            numericUpDown4 = new NumericUpDown();
            numericUpDown1 = new NumericUpDown();
            label5 = new Label();
            label4 = new Label();
            label6 = new Label();
            label3 = new Label();
            richTextBox1 = new RichTextBox();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 13);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(106, 16);
            label1.TabIndex = 0;
            label1.Text = "Giá trị tag: ";
            // 
            // lblTag
            // 
            lblTag.BackColor = Color.FromArgb(224, 224, 224);
            lblTag.Location = new Point(80, 7);
            lblTag.Margin = new Padding(4, 0, 4, 0);
            lblTag.Name = "lblTag";
            lblTag.Size = new Size(107, 27);
            lblTag.TabIndex = 0;
            lblTag.Text = "...";
            lblTag.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Top;
            label2.Location = new Point(0, 72);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(157, 16);
            label2.TabIndex = 0;
            label2.Text = "Thông tin mới nhất:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radiobtnPass);
            groupBox1.Controls.Add(ratiobtnFail);
            groupBox1.Controls.Add(chkbTestMode);
            groupBox1.Location = new Point(195, 7);
            groupBox1.Margin = new Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 3, 4, 3);
            groupBox1.Size = new Size(189, 47);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Thử tín hiệu";
            // 
            // radiobtnPass
            // 
            radiobtnPass.AutoSize = true;
            radiobtnPass.Checked = true;
            radiobtnPass.Dock = DockStyle.Right;
            radiobtnPass.Location = new Point(71, 22);
            radiobtnPass.Margin = new Padding(4, 3, 4, 3);
            radiobtnPass.Name = "radiobtnPass";
            radiobtnPass.Size = new Size(57, 22);
            radiobtnPass.TabIndex = 1;
            radiobtnPass.TabStop = true;
            radiobtnPass.Text = "PASS";
            radiobtnPass.UseVisualStyleBackColor = true;
            // 
            // ratiobtnFail
            // 
            ratiobtnFail.AutoSize = true;
            ratiobtnFail.Dock = DockStyle.Right;
            ratiobtnFail.Location = new Point(128, 22);
            ratiobtnFail.Margin = new Padding(4, 3, 4, 3);
            ratiobtnFail.Name = "ratiobtnFail";
            ratiobtnFail.Size = new Size(57, 22);
            ratiobtnFail.TabIndex = 1;
            ratiobtnFail.Text = "FAIL";
            ratiobtnFail.UseVisualStyleBackColor = true;
            // 
            // chkbTestMode
            // 
            chkbTestMode.AutoSize = true;
            chkbTestMode.Dock = DockStyle.Left;
            chkbTestMode.Location = new Point(4, 22);
            chkbTestMode.Margin = new Padding(4, 3, 4, 3);
            chkbTestMode.Name = "chkbTestMode";
            chkbTestMode.Size = new Size(49, 22);
            chkbTestMode.TabIndex = 0;
            chkbTestMode.Text = "BẬT";
            chkbTestMode.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(numericUpDown3);
            panel1.Controls.Add(numericUpDown2);
            panel1.Controls.Add(numericUpDown4);
            panel1.Controls.Add(numericUpDown1);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(groupBox1);
            panel1.Controls.Add(lblTag);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(797, 72);
            panel1.TabIndex = 3;
            // 
            // numericUpDown3
            // 
            numericUpDown3.Location = new Point(602, 25);
            numericUpDown3.Margin = new Padding(4, 3, 4, 3);
            numericUpDown3.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDown3.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new Size(92, 26);
            numericUpDown3.TabIndex = 3;
            numericUpDown3.TextAlign = HorizontalAlignment.Center;
            numericUpDown3.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(503, 25);
            numericUpDown2.Margin = new Padding(4, 3, 4, 3);
            numericUpDown2.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDown2.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(92, 26);
            numericUpDown2.TabIndex = 3;
            numericUpDown2.TextAlign = HorizontalAlignment.Center;
            numericUpDown2.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // numericUpDown4
            // 
            numericUpDown4.Location = new Point(705, 25);
            numericUpDown4.Margin = new Padding(4, 3, 4, 3);
            numericUpDown4.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDown4.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown4.Name = "numericUpDown4";
            numericUpDown4.Size = new Size(87, 26);
            numericUpDown4.TabIndex = 3;
            numericUpDown4.TextAlign = HorizontalAlignment.Center;
            numericUpDown4.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numericUpDown4.ValueChanged += numericUpDown4_ValueChanged;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(404, 25);
            numericUpDown1.Margin = new Padding(4, 3, 4, 3);
            numericUpDown1.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(91, 26);
            numericUpDown1.TabIndex = 3;
            numericUpDown1.TextAlign = HorizontalAlignment.Center;
            numericUpDown1.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(598, 7);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(71, 16);
            label5.TabIndex = 0;
            label5.Text = "STRENGTH";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(499, 7);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(71, 16);
            label4.TabIndex = 0;
            label4.Text = "REJECTOR";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(705, 7);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(87, 16);
            label6.TabIndex = 0;
            label6.Text = "Rate (ms):";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(400, 7);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(63, 16);
            label3.TabIndex = 0;
            label3.Text = "CAMERA:";
            // 
            // richTextBox1
            // 
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 88);
            richTextBox1.Margin = new Padding(4, 3, 4, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(797, 659);
            richTextBox1.TabIndex = 4;
            richTextBox1.Text = "";
            // 
            // frmMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(797, 747);
            Controls.Add(richTextBox1);
            Controls.Add(label2);
            Controls.Add(panel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "frmMain";
            Text = "Form1";
            FormClosing += frmMain_FormClosing;
            Load += frmMain_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown4).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        //private cpn_CC320 cpn_CC3201;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTag;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radiobtnPass;
        private System.Windows.Forms.RadioButton ratiobtnFail;
        private System.Windows.Forms.CheckBox chkbTestMode;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

