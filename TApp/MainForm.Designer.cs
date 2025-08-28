namespace TApp
{
    partial class MainForm
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
            uiSymbolButton1 = new Sunny.UI.UISymbolButton();
            SuspendLayout();
            // 
            // uiSymbolButton1
            // 
            uiSymbolButton1.Font = new Font("Microsoft Sans Serif", 12F);
            uiSymbolButton1.Location = new Point(12, 12);
            uiSymbolButton1.MinimumSize = new Size(1, 1);
            uiSymbolButton1.Name = "uiSymbolButton1";
            uiSymbolButton1.Size = new Size(171, 42);
            uiSymbolButton1.Symbol = 361234;
            uiSymbolButton1.TabIndex = 0;
            uiSymbolButton1.Text = "uiSymbolButton1";
            uiSymbolButton1.TipsFont = new Font("Microsoft Sans Serif", 9F);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(809, 649);
            Controls.Add(uiSymbolButton1);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UISymbolButton uiSymbolButton1;
    }
}