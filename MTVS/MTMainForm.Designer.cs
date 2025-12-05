using Sunny.UI;

namespace MTVS
{
    partial class MTMainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainPanel = new UITableLayoutPanel();
            this.titlePanelCurrent = new UITitlePanel();
            this.panelCurrentInfo = new UITableLayoutPanel();
            this.labelCurrentVersion = new UILabel();
            this.labelCurrentVersionValue = new UISymbolLabel();
            this.labelProduct = new UILabel();
            this.labelProductValue = new UILabel();
            this.labelChannel = new UILabel();
            this.labelChannelValue = new UILabel();
            this.titlePanelUpdate = new UITitlePanel();
            this.panelUpdateInfo = new UITableLayoutPanel();
            this.labelLatestVersion = new UILabel();
            this.labelLatestVersionValue = new UISymbolLabel();
            this.labelChangelog = new UILabel();
            this.textBoxChangelog = new UIRichTextBox();
            this.panelButtons = new UITableLayoutPanel();
            this.buttonCheckUpdate = new UISymbolButton();
            this.buttonUpdate = new UISymbolButton();
            this.buttonRollback = new UISymbolButton();
            this.titlePanelLog = new UITitlePanel();
            this.panelLog = new UITableLayoutPanel();
            this.labelStatus = new UISymbolLabel();
            this.progressBar = new UIProcessBar();
            this.textBoxLog = new UIRichTextBox();
            this.mainPanel.SuspendLayout();
            this.titlePanelCurrent.SuspendLayout();
            this.panelCurrentInfo.SuspendLayout();
            this.titlePanelUpdate.SuspendLayout();
            this.panelUpdateInfo.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.titlePanelLog.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.ColumnCount = 1;
            this.mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.mainPanel.Controls.Add(this.titlePanelCurrent, 0, 0);
            this.mainPanel.Controls.Add(this.titlePanelUpdate, 0, 1);
            this.mainPanel.Controls.Add(this.titlePanelLog, 0, 2);
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 35);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new Padding(10);
            this.mainPanel.RowCount = 3;
            this.mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            this.mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 250F));
            this.mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.mainPanel.Size = new System.Drawing.Size(900, 650);
            this.mainPanel.TabIndex = 0;
            // 
            // titlePanelCurrent
            // 
            this.titlePanelCurrent.Controls.Add(this.panelCurrentInfo);
            this.titlePanelCurrent.Dock = DockStyle.Fill;
            this.titlePanelCurrent.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.titlePanelCurrent.Location = new System.Drawing.Point(13, 13);
            this.titlePanelCurrent.Margin = new Padding(3);
            this.titlePanelCurrent.MinimumSize = new Size(1, 1);
            this.titlePanelCurrent.Name = "titlePanelCurrent";
            this.titlePanelCurrent.Padding = new Padding(0, 35, 0, 0);
            this.titlePanelCurrent.Size = new Size(874, 144);
            this.titlePanelCurrent.TabIndex = 0;
            this.titlePanelCurrent.Text = "Thông tin hiện tại";
            this.titlePanelCurrent.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // panelCurrentInfo
            // 
            this.panelCurrentInfo.ColumnCount = 2;
            this.panelCurrentInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            this.panelCurrentInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.panelCurrentInfo.Controls.Add(this.labelCurrentVersion, 0, 0);
            this.panelCurrentInfo.Controls.Add(this.labelCurrentVersionValue, 1, 0);
            this.panelCurrentInfo.Controls.Add(this.labelProduct, 0, 1);
            this.panelCurrentInfo.Controls.Add(this.labelProductValue, 1, 1);
            this.panelCurrentInfo.Controls.Add(this.labelChannel, 0, 2);
            this.panelCurrentInfo.Controls.Add(this.labelChannelValue, 1, 2);
            this.panelCurrentInfo.Dock = DockStyle.Fill;
            this.panelCurrentInfo.Location = new Point(0, 35);
            this.panelCurrentInfo.Name = "panelCurrentInfo";
            this.panelCurrentInfo.Padding = new Padding(10);
            this.panelCurrentInfo.RowCount = 3;
            this.panelCurrentInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            this.panelCurrentInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            this.panelCurrentInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            this.panelCurrentInfo.Size = new Size(874, 109);
            this.panelCurrentInfo.TabIndex = 0;
            // 
            // labelCurrentVersion
            // 
            this.labelCurrentVersion.Dock = DockStyle.Fill;
            this.labelCurrentVersion.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelCurrentVersion.Location = new Point(13, 13);
            this.labelCurrentVersion.Name = "labelCurrentVersion";
            this.labelCurrentVersion.Size = new Size(144, 27);
            this.labelCurrentVersion.TabIndex = 0;
            this.labelCurrentVersion.Text = "Phiên bản hiện tại:";
            this.labelCurrentVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelCurrentVersionValue
            // 
            this.labelCurrentVersionValue.Dock = DockStyle.Fill;
            this.labelCurrentVersionValue.Font = new System.Drawing.Font("Microsoft YaHei", 10F, FontStyle.Bold);
            this.labelCurrentVersionValue.Location = new Point(163, 13);
            this.labelCurrentVersionValue.Name = "labelCurrentVersionValue";
            this.labelCurrentVersionValue.Size = new Size(698, 27);
            this.labelCurrentVersionValue.Symbol = 61473;
            this.labelCurrentVersionValue.SymbolSize = 24;
            this.labelCurrentVersionValue.TabIndex = 1;
            this.labelCurrentVersionValue.Text = "1.0.0";
            this.labelCurrentVersionValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelProduct
            // 
            this.labelProduct.Dock = DockStyle.Fill;
            this.labelProduct.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelProduct.Location = new Point(13, 53);
            this.labelProduct.Name = "labelProduct";
            this.labelProduct.Size = new Size(144, 27);
            this.labelProduct.TabIndex = 2;
            this.labelProduct.Text = "Sản phẩm:";
            this.labelProduct.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelProductValue
            // 
            this.labelProductValue.Dock = DockStyle.Fill;
            this.labelProductValue.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelProductValue.Location = new Point(163, 53);
            this.labelProductValue.Name = "labelProductValue";
            this.labelProductValue.Size = new Size(698, 27);
            this.labelProductValue.TabIndex = 3;
            this.labelProductValue.Text = "MTVS";
            this.labelProductValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelChannel
            // 
            this.labelChannel.Dock = DockStyle.Fill;
            this.labelChannel.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelChannel.Location = new Point(13, 93);
            this.labelChannel.Name = "labelChannel";
            this.labelChannel.Size = new Size(144, 27);
            this.labelChannel.TabIndex = 4;
            this.labelChannel.Text = "Kênh:";
            this.labelChannel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelChannelValue
            // 
            this.labelChannelValue.Dock = DockStyle.Fill;
            this.labelChannelValue.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelChannelValue.Location = new Point(163, 93);
            this.labelChannelValue.Name = "labelChannelValue";
            this.labelChannelValue.Size = new Size(698, 27);
            this.labelChannelValue.TabIndex = 5;
            this.labelChannelValue.Text = "stable";
            this.labelChannelValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // titlePanelUpdate
            // 
            this.titlePanelUpdate.Controls.Add(this.panelUpdateInfo);
            this.titlePanelUpdate.Dock = DockStyle.Fill;
            this.titlePanelUpdate.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.titlePanelUpdate.Location = new Point(13, 163);
            this.titlePanelUpdate.Margin = new Padding(3);
            this.titlePanelUpdate.MinimumSize = new Size(1, 1);
            this.titlePanelUpdate.Name = "titlePanelUpdate";
            this.titlePanelUpdate.Padding = new Padding(0, 35, 0, 0);
            this.titlePanelUpdate.Size = new Size(874, 244);
            this.titlePanelUpdate.TabIndex = 1;
            this.titlePanelUpdate.Text = "Cập nhật";
            this.titlePanelUpdate.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // panelUpdateInfo
            // 
            this.panelUpdateInfo.ColumnCount = 1;
            this.panelUpdateInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.panelUpdateInfo.Controls.Add(this.labelLatestVersion, 0, 0);
            this.panelUpdateInfo.Controls.Add(this.labelLatestVersionValue, 0, 1);
            this.panelUpdateInfo.Controls.Add(this.labelChangelog, 0, 2);
            this.panelUpdateInfo.Controls.Add(this.textBoxChangelog, 0, 3);
            this.panelUpdateInfo.Controls.Add(this.panelButtons, 0, 4);
            this.panelUpdateInfo.Dock = DockStyle.Fill;
            this.panelUpdateInfo.Location = new Point(0, 35);
            this.panelUpdateInfo.Name = "panelUpdateInfo";
            this.panelUpdateInfo.Padding = new Padding(10);
            this.panelUpdateInfo.RowCount = 5;
            this.panelUpdateInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.panelUpdateInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.panelUpdateInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            this.panelUpdateInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.panelUpdateInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            this.panelUpdateInfo.Size = new Size(874, 209);
            this.panelUpdateInfo.TabIndex = 0;
            // 
            // labelLatestVersion
            // 
            this.labelLatestVersion.Dock = DockStyle.Fill;
            this.labelLatestVersion.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelLatestVersion.Location = new Point(13, 13);
            this.labelLatestVersion.Name = "labelLatestVersion";
            this.labelLatestVersion.Size = new Size(848, 24);
            this.labelLatestVersion.TabIndex = 0;
            this.labelLatestVersion.Text = "Phiên bản mới nhất:";
            this.labelLatestVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelLatestVersionValue
            // 
            this.labelLatestVersionValue.Dock = DockStyle.Fill;
            this.labelLatestVersionValue.Font = new System.Drawing.Font("Microsoft YaHei", 12F, FontStyle.Bold);
            this.labelLatestVersionValue.ForeColor = Color.Green;
            this.labelLatestVersionValue.Location = new Point(13, 47);
            this.labelLatestVersionValue.Name = "labelLatestVersionValue";
            this.labelLatestVersionValue.Size = new Size(848, 24);
            this.labelLatestVersionValue.Symbol = 61533;
            this.labelLatestVersionValue.SymbolSize = 28;
            this.labelLatestVersionValue.TabIndex = 1;
            this.labelLatestVersionValue.Text = "---";
            this.labelLatestVersionValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelChangelog
            // 
            this.labelChangelog.Dock = DockStyle.Fill;
            this.labelChangelog.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelChangelog.Location = new Point(13, 81);
            this.labelChangelog.Name = "labelChangelog";
            this.labelChangelog.Size = new Size(848, 19);
            this.labelChangelog.TabIndex = 2;
            this.labelChangelog.Text = "Thay đổi:";
            this.labelChangelog.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBoxChangelog
            // 
            this.textBoxChangelog.Dock = DockStyle.Fill;
            this.textBoxChangelog.FillColor = Color.White;
            this.textBoxChangelog.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.textBoxChangelog.Location = new Point(13, 104);
            this.textBoxChangelog.Margin = new Padding(3, 3, 3, 10);
            this.textBoxChangelog.Name = "textBoxChangelog";
            this.textBoxChangelog.ReadOnly = true;
            this.textBoxChangelog.Size = new Size(848, 45);
            this.textBoxChangelog.TabIndex = 3;
            // 
            // panelButtons
            // 
            this.panelButtons.ColumnCount = 3;
            this.panelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            this.panelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            this.panelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            this.panelButtons.Controls.Add(this.buttonCheckUpdate, 0, 0);
            this.panelButtons.Controls.Add(this.buttonUpdate, 1, 0);
            this.panelButtons.Controls.Add(this.buttonRollback, 2, 0);
            this.panelButtons.Dock = DockStyle.Fill;
            this.panelButtons.Location = new Point(13, 159);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new Padding(5);
            this.panelButtons.RowCount = 1;
            this.panelButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.panelButtons.Size = new Size(848, 40);
            this.panelButtons.TabIndex = 4;
            // 
            // buttonCheckUpdate
            // 
            this.buttonCheckUpdate.Dock = DockStyle.Fill;
            this.buttonCheckUpdate.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.buttonCheckUpdate.Location = new Point(8, 8);
            this.buttonCheckUpdate.MinimumSize = new Size(1, 1);
            this.buttonCheckUpdate.Name = "buttonCheckUpdate";
            this.buttonCheckUpdate.Size = new Size(272, 24);
            this.buttonCheckUpdate.Symbol = 61473;
            this.buttonCheckUpdate.TabIndex = 0;
            this.buttonCheckUpdate.Text = "Kiểm tra cập nhật";
            this.buttonCheckUpdate.Click += new EventHandler(this.buttonCheckUpdate_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Dock = DockStyle.Fill;
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.FillColor = Color.FromArgb(0, 192, 0);
            this.buttonUpdate.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.buttonUpdate.Location = new Point(286, 8);
            this.buttonUpdate.MinimumSize = new Size(1, 1);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new Size(272, 24);
            this.buttonUpdate.Symbol = 61533;
            this.buttonUpdate.TabIndex = 1;
            this.buttonUpdate.Text = "Cập nhật";
            this.buttonUpdate.Click += new EventHandler(this.buttonUpdate_Click);
            // 
            // buttonRollback
            // 
            this.buttonRollback.Dock = DockStyle.Fill;
            this.buttonRollback.Enabled = false;
            this.buttonRollback.FillColor = Color.FromArgb(255, 128, 0);
            this.buttonRollback.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.buttonRollback.Location = new Point(564, 8);
            this.buttonRollback.MinimumSize = new Size(1, 1);
            this.buttonRollback.Name = "buttonRollback";
            this.buttonRollback.Size = new Size(279, 24);
            this.buttonRollback.Symbol = 563629;
            this.buttonRollback.TabIndex = 2;
            this.buttonRollback.Text = "Khôi phục";
            this.buttonRollback.Click += new EventHandler(this.buttonRollback_Click);
            // 
            // titlePanelLog
            // 
            this.titlePanelLog.Controls.Add(this.panelLog);
            this.titlePanelLog.Dock = DockStyle.Fill;
            this.titlePanelLog.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.titlePanelLog.Location = new Point(13, 413);
            this.titlePanelLog.Margin = new Padding(3);
            this.titlePanelLog.MinimumSize = new Size(1, 1);
            this.titlePanelLog.Name = "titlePanelLog";
            this.titlePanelLog.Padding = new Padding(0, 35, 0, 0);
            this.titlePanelLog.Size = new Size(874, 224);
            this.titlePanelLog.TabIndex = 2;
            this.titlePanelLog.Text = "Nhật ký hoạt động";
            this.titlePanelLog.TextAlignment = ContentAlignment.MiddleCenter;
            // 
            // panelLog
            // 
            this.panelLog.ColumnCount = 1;
            this.panelLog.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.panelLog.Controls.Add(this.labelStatus, 0, 0);
            this.panelLog.Controls.Add(this.progressBar, 0, 1);
            this.panelLog.Controls.Add(this.textBoxLog, 0, 2);
            this.panelLog.Dock = DockStyle.Fill;
            this.panelLog.Location = new Point(0, 35);
            this.panelLog.Name = "panelLog";
            this.panelLog.Padding = new Padding(10);
            this.panelLog.RowCount = 3;
            this.panelLog.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.panelLog.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            this.panelLog.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.panelLog.Size = new Size(874, 189);
            this.panelLog.TabIndex = 0;
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = DockStyle.Fill;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft YaHei", 10F);
            this.labelStatus.Location = new Point(13, 13);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new Size(848, 24);
            this.labelStatus.Symbol = 61528;
            this.labelStatus.SymbolSize = 24;
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Sẵn sàng";
            this.labelStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Dock = DockStyle.Fill;
            this.progressBar.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            this.progressBar.Location = new Point(13, 47);
            this.progressBar.MinimumSize = new Size(1, 1);
            this.progressBar.Name = "progressBar";
            //this.progressBar.s = new Size(848, 19);
            this.progressBar.TabIndex = 1;
            this.progressBar.Text = "";
            this.progressBar.Visible = false;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = DockStyle.Fill;
            this.textBoxLog.FillColor = Color.White;
            this.textBoxLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxLog.Location = new Point(13, 76);
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.Size = new Size(848, 100);
            this.textBoxLog.TabIndex = 2;
            // 
            // MTMainForm
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(900, 685);
            this.Controls.Add(this.mainPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "MTMainForm";
            this.Padding = new Padding(0, 35, 0, 0);
            this.ShowIcon = true;
            this.ShowInTaskbar = true;
            this.Text = "Version Manager - MTVS";
            this.mainPanel.ResumeLayout(false);
            this.titlePanelCurrent.ResumeLayout(false);
            this.panelCurrentInfo.ResumeLayout(false);
            this.titlePanelUpdate.ResumeLayout(false);
            this.panelUpdateInfo.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.titlePanelLog.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private UITableLayoutPanel mainPanel;
        private UITitlePanel titlePanelCurrent;
        private UITableLayoutPanel panelCurrentInfo;
        private UILabel labelCurrentVersion;
        private UISymbolLabel labelCurrentVersionValue;
        private UILabel labelProduct;
        private UILabel labelProductValue;
        private UILabel labelChannel;
        private UILabel labelChannelValue;
        private UITitlePanel titlePanelUpdate;
        private UITableLayoutPanel panelUpdateInfo;
        private UILabel labelLatestVersion;
        private UISymbolLabel labelLatestVersionValue;
        private UILabel labelChangelog;
        private UIRichTextBox textBoxChangelog;
        private UITableLayoutPanel panelButtons;
        private UISymbolButton buttonCheckUpdate;
        private UISymbolButton buttonUpdate;
        private UISymbolButton buttonRollback;
        private UITitlePanel titlePanelLog;
        private UITableLayoutPanel panelLog;
        private UISymbolLabel labelStatus;
        private UIProcessBar progressBar;
        private UIRichTextBox textBoxLog;
    }
}
