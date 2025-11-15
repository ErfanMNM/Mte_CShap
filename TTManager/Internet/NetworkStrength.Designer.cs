namespace TTManager.Internet
{
    partial class NetworkStrength
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            uiTableLayoutPanel8 = new Sunny.UI.UITableLayoutPanel();
            opInternetStatus = new Sunny.UI.UIPanel();
            opInternetSignal = new Sunny.UI.UISignal();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            uiTableLayoutPanel8.SuspendLayout();
            SuspendLayout();
            // 
            // uiTableLayoutPanel8
            // 
            uiTableLayoutPanel8.ColumnCount = 2;
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 71.962616F));
            uiTableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28.037384F));
            uiTableLayoutPanel8.Controls.Add(opInternetStatus, 0, 0);
            uiTableLayoutPanel8.Controls.Add(opInternetSignal, 1, 0);
            uiTableLayoutPanel8.Dock = DockStyle.Fill;
            uiTableLayoutPanel8.Location = new Point(0, 0);
            uiTableLayoutPanel8.Margin = new Padding(0);
            uiTableLayoutPanel8.Name = "uiTableLayoutPanel8";
            uiTableLayoutPanel8.RowCount = 1;
            uiTableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            uiTableLayoutPanel8.Size = new Size(122, 41);
            uiTableLayoutPanel8.TabIndex = 1;
            uiTableLayoutPanel8.TagString = null;
            // 
            // opInternetStatus
            // 
            opInternetStatus.Dock = DockStyle.Fill;
            opInternetStatus.Font = new Font("Microsoft Sans Serif", 12F);
            opInternetStatus.Location = new Point(2, 2);
            opInternetStatus.Margin = new Padding(2);
            opInternetStatus.MinimumSize = new Size(1, 1);
            opInternetStatus.Name = "opInternetStatus";
            opInternetStatus.RectColor = Color.FromArgb(255, 128, 0);
            opInternetStatus.RectSize = 2;
            opInternetStatus.Size = new Size(83, 37);
            opInternetStatus.TabIndex = 0;
            opInternetStatus.Text = "Không có";
            opInternetStatus.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opInternetSignal
            // 
            opInternetSignal.Dock = DockStyle.Fill;
            opInternetSignal.Font = new Font("Microsoft Sans Serif", 12F);
            opInternetSignal.Level = 2;
            opInternetSignal.LineWidth = 4;
            opInternetSignal.Location = new Point(90, 3);
            opInternetSignal.MinimumSize = new Size(1, 1);
            opInternetSignal.Name = "opInternetSignal";
            opInternetSignal.OffColor = Color.LightGray;
            opInternetSignal.OnColor = Color.Green;
            opInternetSignal.Size = new Size(29, 35);
            opInternetSignal.TabIndex = 3;
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            // 
            // NetworkStrength
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(uiTableLayoutPanel8);
            Name = "NetworkStrength";
            Size = new Size(122, 41);
            uiTableLayoutPanel8.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel8;
        private Sunny.UI.UIPanel opInternetStatus;
        private Sunny.UI.UISignal opInternetSignal;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}
