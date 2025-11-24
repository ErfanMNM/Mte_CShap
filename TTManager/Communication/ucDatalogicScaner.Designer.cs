namespace TTManager.Communication
{
    partial class ucDatalogicScaner
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
            opPanelStt = new Sunny.UI.UIPanel();
            opStatus = new Sunny.UI.UISymbolLabel();
            opPanelStt.SuspendLayout();
            SuspendLayout();
            // 
            // opPanelStt
            // 
            opPanelStt.Controls.Add(opStatus);
            opPanelStt.Dock = DockStyle.Fill;
            opPanelStt.FillColor = Color.FromArgb(255, 192, 128);
            opPanelStt.Font = new Font("Microsoft Sans Serif", 12F);
            opPanelStt.Location = new Point(0, 0);
            opPanelStt.Margin = new Padding(4, 5, 4, 5);
            opPanelStt.MinimumSize = new Size(1, 1);
            opPanelStt.Name = "opPanelStt";
            opPanelStt.RectColor = Color.Red;
            opPanelStt.Size = new Size(203, 67);
            opPanelStt.TabIndex = 0;
            opPanelStt.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // opStatus
            // 
            opStatus.BackColor = Color.Transparent;
            opStatus.Dock = DockStyle.Fill;
            opStatus.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            opStatus.ForeColor = Color.Red;
            opStatus.Location = new Point(0, 0);
            opStatus.MinimumSize = new Size(1, 1);
            opStatus.Name = "opStatus";
            opStatus.Size = new Size(203, 67);
            opStatus.Symbol = 361735;
            opStatus.SymbolColor = Color.Red;
            opStatus.TabIndex = 0;
            opStatus.Text = "Mất kết nối";
            // 
            // ucDatalogicScaner
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(opPanelStt);
            Name = "ucDatalogicScaner";
            Size = new Size(203, 67);
            opPanelStt.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UIPanel opPanelStt;
        private Sunny.UI.UISymbolLabel opStatus;
    }
}
