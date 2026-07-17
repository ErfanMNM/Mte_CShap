namespace ProjectD
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            grpServer = new GroupBox();
            lblClientCount = new Label();
            lblStatus = new Label();
            btnStartStop = new Button();
            txtPort = new TextBox();
            lblPort = new Label();
            grpSqlite = new GroupBox();
            lblRowCount = new Label();
            dgvData = new DataGridView();
            txtColumnName = new TextBox();
            lblColumnName = new Label();
            txtTableName = new TextBox();
            lblTableName = new Label();
            btnLoadData = new Button();
            btnBrowse = new Button();
            txtDbPath = new TextBox();
            lblDbPath = new Label();
            grpSettings = new GroupBox();
            numCount = new NumericUpDown();
            lblCount = new Label();
            numStartIndex = new NumericUpDown();
            lblStartIndex = new Label();
            txtDelayMs = new TextBox();
            lblDelayMs = new Label();
            txtSuffix = new TextBox();
            lblSuffix = new Label();
            txtPrefix = new TextBox();
            lblPrefix = new Label();
            txtEndChar = new TextBox();
            lblEndChar = new Label();
            grpManual = new GroupBox();
            btnSendManualCode = new Button();
            txtManualCode = new TextBox();
            lblManualCode = new Label();
            grpActions = new GroupBox();
            btnClearLog = new Button();
            btnSendOne = new Button();
            btnSendAll = new Button();
            grpLog = new GroupBox();
            txtLog = new TextBox();
            grpServer.SuspendLayout();
            grpSqlite.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            grpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStartIndex).BeginInit();
            grpManual.SuspendLayout();
            grpActions.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpServer
            // 
            grpServer.Controls.Add(lblClientCount);
            grpServer.Controls.Add(lblStatus);
            grpServer.Controls.Add(btnStartStop);
            grpServer.Controls.Add(txtPort);
            grpServer.Controls.Add(lblPort);
            grpServer.Location = new Point(12, 12);
            grpServer.Name = "grpServer";
            grpServer.Size = new Size(420, 80);
            grpServer.TabIndex = 0;
            grpServer.TabStop = false;
            grpServer.Text = "TCP Server";
            // 
            // lblClientCount
            // 
            lblClientCount.AutoSize = true;
            lblClientCount.Location = new Point(15, 55);
            lblClientCount.Name = "lblClientCount";
            lblClientCount.Size = new Size(55, 15);
            lblClientCount.TabIndex = 4;
            lblClientCount.Text = "Clients: 0";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(290, 28);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(51, 15);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Stopped";
            // 
            // btnStartStop
            // 
            btnStartStop.Location = new Point(170, 23);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(100, 27);
            btnStartStop.TabIndex = 2;
            btnStartStop.Text = "Start";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(70, 25);
            txtPort.Name = "txtPort";
            txtPort.PlaceholderText = "9000";
            txtPort.Size = new Size(80, 23);
            txtPort.TabIndex = 1;
            txtPort.Text = "9000";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(15, 28);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(32, 15);
            lblPort.TabIndex = 0;
            lblPort.Text = "Port:";
            // 
            // grpSqlite
            // 
            grpSqlite.Controls.Add(lblRowCount);
            grpSqlite.Controls.Add(dgvData);
            grpSqlite.Controls.Add(txtColumnName);
            grpSqlite.Controls.Add(lblColumnName);
            grpSqlite.Controls.Add(txtTableName);
            grpSqlite.Controls.Add(lblTableName);
            grpSqlite.Controls.Add(btnLoadData);
            grpSqlite.Controls.Add(btnBrowse);
            grpSqlite.Controls.Add(txtDbPath);
            grpSqlite.Controls.Add(lblDbPath);
            grpSqlite.Location = new Point(12, 98);
            grpSqlite.Name = "grpSqlite";
            grpSqlite.Size = new Size(550, 303);
            grpSqlite.TabIndex = 1;
            grpSqlite.TabStop = false;
            grpSqlite.Text = "SQLite Data";
            // 
            // lblRowCount
            // 
            lblRowCount.AutoSize = true;
            lblRowCount.Location = new Point(15, 282);
            lblRowCount.Name = "lblRowCount";
            lblRowCount.Size = new Size(47, 15);
            lblRowCount.TabIndex = 9;
            lblRowCount.Text = "Rows: 0";
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Location = new Point(15, 88);
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dgvData.RowHeadersWidth = 40;
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.Size = new Size(510, 191);
            dgvData.TabIndex = 8;
            // 
            // txtColumnName
            // 
            txtColumnName.Location = new Point(305, 55);
            txtColumnName.Name = "txtColumnName";
            txtColumnName.PlaceholderText = "code";
            txtColumnName.Size = new Size(110, 23);
            txtColumnName.TabIndex = 6;
            txtColumnName.Text = "code";
            // 
            // lblColumnName
            // 
            lblColumnName.AutoSize = true;
            lblColumnName.Location = new Point(225, 58);
            lblColumnName.Name = "lblColumnName";
            lblColumnName.Size = new Size(84, 15);
            lblColumnName.TabIndex = 5;
            lblColumnName.Text = "Code Column:";
            // 
            // txtTableName
            // 
            txtTableName.Location = new Point(95, 55);
            txtTableName.Name = "txtTableName";
            txtTableName.PlaceholderText = "codes";
            txtTableName.Size = new Size(120, 23);
            txtTableName.TabIndex = 4;
            txtTableName.Text = "codes";
            // 
            // lblTableName
            // 
            lblTableName.AutoSize = true;
            lblTableName.Location = new Point(15, 58);
            lblTableName.Name = "lblTableName";
            lblTableName.Size = new Size(73, 15);
            lblTableName.TabIndex = 3;
            lblTableName.Text = "Table Name:";
            // 
            // btnLoadData
            // 
            btnLoadData.Location = new Point(425, 53);
            btnLoadData.Name = "btnLoadData";
            btnLoadData.Size = new Size(100, 27);
            btnLoadData.TabIndex = 7;
            btnLoadData.Text = "Load Data";
            btnLoadData.UseVisualStyleBackColor = true;
            btnLoadData.Click += btnLoadData_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(425, 23);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(30, 27);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtDbPath
            // 
            txtDbPath.Location = new Point(75, 25);
            txtDbPath.Name = "txtDbPath";
            txtDbPath.PlaceholderText = "C:\\path\\to\\database.db";
            txtDbPath.Size = new Size(340, 23);
            txtDbPath.TabIndex = 1;
            // 
            // lblDbPath
            // 
            lblDbPath.AutoSize = true;
            lblDbPath.Location = new Point(15, 28);
            lblDbPath.Name = "lblDbPath";
            lblDbPath.Size = new Size(52, 15);
            lblDbPath.TabIndex = 0;
            lblDbPath.Text = "DB Path:";
            // 
            // grpSettings
            // 
            grpSettings.Controls.Add(numCount);
            grpSettings.Controls.Add(lblCount);
            grpSettings.Controls.Add(numStartIndex);
            grpSettings.Controls.Add(lblStartIndex);
            grpSettings.Controls.Add(txtDelayMs);
            grpSettings.Controls.Add(lblDelayMs);
            grpSettings.Controls.Add(txtSuffix);
            grpSettings.Controls.Add(lblSuffix);
            grpSettings.Controls.Add(txtPrefix);
            grpSettings.Controls.Add(lblPrefix);
            grpSettings.Controls.Add(txtEndChar);
            grpSettings.Controls.Add(lblEndChar);
            grpSettings.Location = new Point(568, 12);
            grpSettings.Name = "grpSettings";
            grpSettings.Size = new Size(320, 180);
            grpSettings.TabIndex = 2;
            grpSettings.TabStop = false;
            grpSettings.Text = "Send Settings";
            // 
            // numCount
            // 
            numCount.Location = new Point(250, 26);
            numCount.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.Size = new Size(60, 23);
            numCount.TabIndex = 3;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.Location = new Point(195, 28);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(43, 15);
            lblCount.TabIndex = 2;
            lblCount.Text = "Count:";
            // 
            // numStartIndex
            // 
            numStartIndex.Location = new Point(100, 26);
            numStartIndex.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numStartIndex.Name = "numStartIndex";
            numStartIndex.Size = new Size(80, 23);
            numStartIndex.TabIndex = 1;
            // 
            // lblStartIndex
            // 
            lblStartIndex.AutoSize = true;
            lblStartIndex.Location = new Point(15, 28);
            lblStartIndex.Name = "lblStartIndex";
            lblStartIndex.Size = new Size(65, 15);
            lblStartIndex.TabIndex = 0;
            lblStartIndex.Text = "Start Index:";
            // 
            // txtDelayMs
            // 
            txtDelayMs.Location = new Point(95, 145);
            txtDelayMs.Name = "txtDelayMs";
            txtDelayMs.PlaceholderText = "120";
            txtDelayMs.Size = new Size(80, 23);
            txtDelayMs.TabIndex = 11;
            txtDelayMs.Text = "120";
            // 
            // lblDelayMs
            // 
            lblDelayMs.AutoSize = true;
            lblDelayMs.Location = new Point(15, 148);
            lblDelayMs.Name = "lblDelayMs";
            lblDelayMs.Size = new Size(66, 15);
            lblDelayMs.TabIndex = 10;
            lblDelayMs.Text = "Delay (ms):";
            // 
            // txtSuffix
            // 
            txtSuffix.Location = new Point(65, 115);
            txtSuffix.Name = "txtSuffix";
            txtSuffix.Size = new Size(245, 23);
            txtSuffix.TabIndex = 9;
            // 
            // lblSuffix
            // 
            lblSuffix.AutoSize = true;
            lblSuffix.Location = new Point(15, 118);
            lblSuffix.Name = "lblSuffix";
            lblSuffix.Size = new Size(39, 15);
            lblSuffix.TabIndex = 8;
            lblSuffix.Text = "Suffix:";
            // 
            // txtPrefix
            // 
            txtPrefix.Location = new Point(65, 85);
            txtPrefix.Name = "txtPrefix";
            txtPrefix.Size = new Size(245, 23);
            txtPrefix.TabIndex = 7;
            // 
            // lblPrefix
            // 
            lblPrefix.AutoSize = true;
            lblPrefix.Location = new Point(15, 88);
            lblPrefix.Name = "lblPrefix";
            lblPrefix.Size = new Size(39, 15);
            lblPrefix.TabIndex = 6;
            lblPrefix.Text = "Prefix:";
            // 
            // txtEndChar
            // 
            txtEndChar.Location = new Point(85, 55);
            txtEndChar.Name = "txtEndChar";
            txtEndChar.PlaceholderText = "\\r\\n";
            txtEndChar.Size = new Size(225, 23);
            txtEndChar.TabIndex = 5;
            txtEndChar.Text = "\\r\\n";
            // 
            // lblEndChar
            // 
            lblEndChar.AutoSize = true;
            lblEndChar.Location = new Point(15, 58);
            lblEndChar.Name = "lblEndChar";
            lblEndChar.Size = new Size(58, 15);
            lblEndChar.TabIndex = 4;
            lblEndChar.Text = "End Char:";
            // 
            // grpManual
            // 
            grpManual.Controls.Add(btnSendManualCode);
            grpManual.Controls.Add(txtManualCode);
            grpManual.Controls.Add(lblManualCode);
            grpManual.Location = new Point(568, 198);
            grpManual.Name = "grpManual";
            grpManual.Size = new Size(320, 90);
            grpManual.TabIndex = 3;
            grpManual.TabStop = false;
            grpManual.Text = "Manual Input";
            // 
            // btnSendManualCode
            // 
            btnSendManualCode.Location = new Point(55, 54);
            btnSendManualCode.Name = "btnSendManualCode";
            btnSendManualCode.Size = new Size(240, 28);
            btnSendManualCode.TabIndex = 2;
            btnSendManualCode.Text = "Send Manual Code";
            btnSendManualCode.UseVisualStyleBackColor = true;
            btnSendManualCode.Click += btnSendManualCode_Click;
            // 
            // txtManualCode
            // 
            txtManualCode.Location = new Point(55, 25);
            txtManualCode.Name = "txtManualCode";
            txtManualCode.PlaceholderText = "Enter code here...";
            txtManualCode.Size = new Size(240, 23);
            txtManualCode.TabIndex = 1;
            // 
            // lblManualCode
            // 
            lblManualCode.AutoSize = true;
            lblManualCode.Location = new Point(15, 28);
            lblManualCode.Name = "lblManualCode";
            lblManualCode.Size = new Size(38, 15);
            lblManualCode.TabIndex = 0;
            lblManualCode.Text = "Code:";
            // 
            // grpActions
            // 
            grpActions.Controls.Add(btnClearLog);
            grpActions.Controls.Add(btnSendOne);
            grpActions.Controls.Add(btnSendAll);
            grpActions.Location = new Point(568, 294);
            grpActions.Name = "grpActions";
            grpActions.Size = new Size(320, 107);
            grpActions.TabIndex = 4;
            grpActions.TabStop = false;
            grpActions.Text = "Actions";
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(15, 76);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(290, 25);
            btnClearLog.TabIndex = 2;
            btnClearLog.Text = "Clear Log";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // btnSendOne
            // 
            btnSendOne.Location = new Point(170, 28);
            btnSendOne.Name = "btnSendOne";
            btnSendOne.Size = new Size(135, 45);
            btnSendOne.TabIndex = 1;
            btnSendOne.Text = "Send Selected";
            btnSendOne.UseVisualStyleBackColor = true;
            btnSendOne.Click += btnSendOne_Click;
            // 
            // btnSendAll
            // 
            btnSendAll.Location = new Point(15, 28);
            btnSendAll.Name = "btnSendAll";
            btnSendAll.Size = new Size(135, 45);
            btnSendAll.TabIndex = 0;
            btnSendAll.Text = "Send All (Batch)";
            btnSendAll.UseVisualStyleBackColor = true;
            btnSendAll.Click += btnSendAll_Click;
            // 
            // grpLog
            // 
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(12, 407);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(876, 247);
            grpLog.TabIndex = 5;
            grpLog.TabStop = false;
            grpLog.Text = "Log";
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.Black;
            txtLog.Dock = DockStyle.Fill;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.Lime;
            txtLog.Location = new Point(3, 19);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(870, 225);
            txtLog.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(898, 669);
            Controls.Add(grpLog);
            Controls.Add(grpActions);
            Controls.Add(grpManual);
            Controls.Add(grpSettings);
            Controls.Add(grpSqlite);
            Controls.Add(grpServer);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TCP Server - SQLite Code Sender";
            FormClosing += MainForm_FormClosing;
            grpServer.ResumeLayout(false);
            grpServer.PerformLayout();
            grpSqlite.ResumeLayout(false);
            grpSqlite.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStartIndex).EndInit();
            grpManual.ResumeLayout(false);
            grpManual.PerformLayout();
            grpActions.ResumeLayout(false);
            grpLog.ResumeLayout(false);
            grpLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpServer;
        private Label lblPort;
        private TextBox txtPort;
        private Button btnStartStop;
        private Label lblStatus;
        private Label lblClientCount;

        private GroupBox grpSqlite;
        private Label lblDbPath;
        private TextBox txtDbPath;
        private Button btnBrowse;
        private Label lblTableName;
        private TextBox txtTableName;
        private Label lblColumnName;
        private TextBox txtColumnName;
        private Button btnLoadData;
        private DataGridView dgvData;
        private Label lblRowCount;

        private GroupBox grpSettings;
        private Label lblStartIndex;
        private NumericUpDown numStartIndex;
        private Label lblCount;
        private NumericUpDown numCount;
        private Label lblEndChar;
        private TextBox txtEndChar;
        private Label lblPrefix;
        private TextBox txtPrefix;
        private Label lblSuffix;
        private TextBox txtSuffix;
        private Label lblDelayMs;
        private TextBox txtDelayMs;

        private GroupBox grpManual;
        private Label lblManualCode;
        private TextBox txtManualCode;
        private Button btnSendManualCode;

        private GroupBox grpActions;
        private Button btnSendAll;
        private Button btnSendOne;
        private Button btnClearLog;

        private GroupBox grpLog;
        private TextBox txtLog;
    }
}
