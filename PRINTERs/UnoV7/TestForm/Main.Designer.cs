namespace TestForm
{
    partial class Main
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.uc_Nowix1 = new Norwix.uc_Nowix();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.workerCheckData = new System.ComponentModel.BackgroundWorker();
            this.button3 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 238F));
            this.tableLayoutPanel1.Controls.Add(this.uc_Nowix1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1059, 464);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // uc_Nowix1
            // 
            this.uc_Nowix1.BackColor = System.Drawing.Color.White;
            this.uc_Nowix1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uc_Nowix1.ENABLE = true;
            this.uc_Nowix1.Location = new System.Drawing.Point(3, 3);
            this.uc_Nowix1.Name = "uc_Nowix1";
            this.uc_Nowix1.Size = new System.Drawing.Size(815, 458);
            this.uc_Nowix1.TabIndex = 0;
            this.uc_Nowix1.Load += new System.EventHandler(this.uc_Nowix1_Load);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(824, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(232, 458);
            this.panel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(107, 9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 90);
            this.button2.TabIndex = 1;
            this.button2.Text = "Load";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 90);
            this.button1.TabIndex = 0;
            this.button1.Text = "Sendata";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // workerCheckData
            // 
            this.workerCheckData.WorkerSupportsCancellation = true;
            this.workerCheckData.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerCheckData_DoWork);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(3, 105);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(124, 83);
            this.button3.TabIndex = 2;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 464);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Main";
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Norwix.uc_Nowix uc_Nowix1;
        private System.ComponentModel.BackgroundWorker workerCheckData;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        //private System.Windows.Forms.Button button3;
    }
}