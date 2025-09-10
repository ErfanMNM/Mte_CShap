namespace Printer
{
    partial class Form1
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
            this.ucPrinter1 = new Printer.ucPrinter();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ucPrinter1
            // 
            this.ucPrinter1.Location = new System.Drawing.Point(0, 0);
            this.ucPrinter1.Name = "ucPrinter1";
            this.ucPrinter1.Size = new System.Drawing.Size(800, 450);
            this.ucPrinter1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 437);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(203, 38);
            this.button1.TabIndex = 1;
            this.button1.Text = "them data";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 524);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ucPrinter1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ucPrinter ucPrinter1;
        private System.Windows.Forms.Button button1;
    }
}