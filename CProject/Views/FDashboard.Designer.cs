namespace CProject.Views
{
    partial class FDashboard
    {
        private System.ComponentModel.IContainer components = null;

        private Sunny.UI.UITitlePanel panelRoot;
        private Sunny.UI.UITableLayoutPanel tableRoot;

        private Sunny.UI.UITitlePanel panelSession;
        private Sunny.UI.UITableLayoutPanel tableSession;
        private Sunny.UI.UILabel lblPoolNameSession;
        private Sunny.UI.UITextBox txtPoolNameSession;
        private Sunny.UI.UILabel lblCreateID;
        private Sunny.UI.UITextBox txtCreateID;
        private Sunny.UI.UILabel lblCreatedBy;
        private Sunny.UI.UITextBox txtCreatedBy;
        private Sunny.UI.UISymbolButton btnResetSession;
        private Sunny.UI.UISymbolButton btnLoadSamples;

        private Sunny.UI.UITabControl tabApiGroups;
        private System.Windows.Forms.TabPage tabLifecycle;
        private System.Windows.Forms.TabPage tabWrite;
        private System.Windows.Forms.TabPage tabRead;

        private Sunny.UI.UISymbolButton btnGetPoolPath;
        private Sunny.UI.UISymbolButton btnCreatePool;
        private Sunny.UI.UISymbolButton btnGetPoolInfo;
        private Sunny.UI.UISymbolButton btnGetPoolsPaginated;

        private Sunny.UI.UITextBox txtSingleCode;
        private Sunny.UI.UISymbolButton btnBrowseFile;
        private Sunny.UI.UITextBox txtFilePath;
        private Sunny.UI.UISymbolButton btnAddCodeSingle;
        private Sunny.UI.UISymbolButton btnAddCodeFile;
        private Sunny.UI.UISymbolButton btnUpdateStatus;
        private Sunny.UI.UITextBox txtUpdatePoolCode;
        private Sunny.UI.UITextBox txtUpdateCodeID;
        private Sunny.UI.UIComboBox cboUpdateStatus;

        private Sunny.UI.UISymbolButton btnGetPoolCode;
        private Sunny.UI.UITextBox txtQueryPoolCode;
        private Sunny.UI.UITextBox txtQueryCodeID;
        private Sunny.UI.UISymbolButton btnGetCodesPaginated;
        private Sunny.UI.UIComboBox cboFilterStatus;
        private Sunny.UI.UITextBox txtFilterBatchID;
        private Sunny.UI.UISymbolButton btnGetCodesByStatus;
        private Sunny.UI.UIComboBox cboStatusOnly;
        private Sunny.UI.UISymbolButton btnGetCodeCounts;

        private Sunny.UI.UITitlePanel panelLog;
        private Sunny.UI.UIListBox lstLog;
        private Sunny.UI.UISymbolButton btnClearLog;
        private Sunny.UI.UISymbolButton btnCopyLog;

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
            this.components = new System.ComponentModel.Container();

            this.panelRoot = new Sunny.UI.UITitlePanel();
            this.tableRoot = new Sunny.UI.UITableLayoutPanel();
            this.panelSession = new Sunny.UI.UITitlePanel();
            this.tableSession = new Sunny.UI.UITableLayoutPanel();
            this.lblPoolNameSession = new Sunny.UI.UILabel();
            this.txtPoolNameSession = new Sunny.UI.UITextBox();
            this.lblCreateID = new Sunny.UI.UILabel();
            this.txtCreateID = new Sunny.UI.UITextBox();
            this.lblCreatedBy = new Sunny.UI.UILabel();
            this.txtCreatedBy = new Sunny.UI.UITextBox();
            this.btnResetSession = new Sunny.UI.UISymbolButton();
            this.btnLoadSamples = new Sunny.UI.UISymbolButton();

            this.tabApiGroups = new Sunny.UI.UITabControl();
            this.tabLifecycle = new System.Windows.Forms.TabPage();
            this.tabWrite = new System.Windows.Forms.TabPage();
            this.tabRead = new System.Windows.Forms.TabPage();

            this.btnGetPoolPath = new Sunny.UI.UISymbolButton();
            this.btnCreatePool = new Sunny.UI.UISymbolButton();
            this.btnGetPoolInfo = new Sunny.UI.UISymbolButton();
            this.btnGetPoolsPaginated = new Sunny.UI.UISymbolButton();

            this.txtSingleCode = new Sunny.UI.UITextBox();
            this.btnBrowseFile = new Sunny.UI.UISymbolButton();
            this.txtFilePath = new Sunny.UI.UITextBox();
            this.btnAddCodeSingle = new Sunny.UI.UISymbolButton();
            this.btnAddCodeFile = new Sunny.UI.UISymbolButton();
            this.txtUpdatePoolCode = new Sunny.UI.UITextBox();
            this.txtUpdateCodeID = new Sunny.UI.UITextBox();
            this.cboUpdateStatus = new Sunny.UI.UIComboBox();
            this.btnUpdateStatus = new Sunny.UI.UISymbolButton();

            this.btnGetPoolCode = new Sunny.UI.UISymbolButton();
            this.txtQueryPoolCode = new Sunny.UI.UITextBox();
            this.txtQueryCodeID = new Sunny.UI.UITextBox();
            this.btnGetCodesPaginated = new Sunny.UI.UISymbolButton();
            this.cboFilterStatus = new Sunny.UI.UIComboBox();
            this.txtFilterBatchID = new Sunny.UI.UITextBox();
            this.btnGetCodesByStatus = new Sunny.UI.UISymbolButton();
            this.cboStatusOnly = new Sunny.UI.UIComboBox();
            this.btnGetCodeCounts = new Sunny.UI.UISymbolButton();

            this.panelLog = new Sunny.UI.UITitlePanel();
            this.lstLog = new Sunny.UI.UIListBox();
            this.btnClearLog = new Sunny.UI.UISymbolButton();
            this.btnCopyLog = new Sunny.UI.UISymbolButton();

            this.panelRoot.SuspendLayout();
            this.tableRoot.SuspendLayout();
            this.panelSession.SuspendLayout();
            this.tableSession.SuspendLayout();
            this.tabApiGroups.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.SuspendLayout();

            // panelRoot
            this.panelRoot.Controls.Add(this.tableRoot);
            this.panelRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRoot.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.panelRoot.Padding = new System.Windows.Forms.Padding(1, 32, 1, 1);
            this.panelRoot.Size = new System.Drawing.Size(1100, 760);
            this.panelRoot.Text = "QUẢN LÝ DATA POOL";
            this.panelRoot.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRoot.TitleHeight = 32;

            // tableRoot (4 rows: Session, Tabs, Status hint, Log)
            this.tableRoot.BackColor = System.Drawing.Color.White;
            this.tableRoot.ColumnCount = 1;
            this.tableRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRoot.Controls.Add(this.panelSession, 0, 0);
            this.tableRoot.Controls.Add(this.tabApiGroups, 0, 1);
            this.tableRoot.Controls.Add(this.panelLog, 0, 2);
            this.tableRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRoot.Location = new System.Drawing.Point(1, 32);
            this.tableRoot.Name = "tableRoot";
            this.tableRoot.Padding = new System.Windows.Forms.Padding(8);
            this.tableRoot.RowCount = 3;
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableRoot.Size = new System.Drawing.Size(1098, 727);

            // panelSession
            this.panelSession.Controls.Add(this.tableSession);
            this.panelSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSession.Padding = new System.Windows.Forms.Padding(1, 28, 1, 1);
            this.panelSession.Text = "Thiết lập phiên test";
            this.panelSession.TitleHeight = 28;

            this.tableSession.BackColor = System.Drawing.Color.White;
            this.tableSession.ColumnCount = 6;
            this.tableSession.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableSession.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableSession.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableSession.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableSession.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableSession.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableSession.Controls.Add(this.lblPoolNameSession, 0, 0);
            this.tableSession.Controls.Add(this.txtPoolNameSession, 1, 0);
            this.tableSession.Controls.Add(this.lblCreateID, 2, 0);
            this.tableSession.Controls.Add(this.txtCreateID, 3, 0);
            this.tableSession.Controls.Add(this.btnResetSession, 4, 0);
            this.tableSession.Controls.Add(this.btnLoadSamples, 5, 0);
            this.tableSession.Controls.Add(this.lblCreatedBy, 0, 1);
            this.tableSession.Controls.Add(this.txtCreatedBy, 1, 1);
            this.tableSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableSession.Location = new System.Drawing.Point(1, 28);
            this.tableSession.Name = "tableSession";
            this.tableSession.RowCount = 2;
            this.tableSession.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableSession.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            this.lblPoolNameSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPoolNameSession.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblPoolNameSession.Text = "Pool Name:";
            this.lblPoolNameSession.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtPoolNameSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPoolNameSession.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtPoolNameSession.Text = "TestPool";

            this.lblCreateID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCreateID.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblCreateID.Text = "Create ID:";
            this.lblCreateID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtCreateID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCreateID.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtCreateID.Text = "TESTER";

            this.btnResetSession.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResetSession.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnResetSession.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnResetSession.Text = "Reset";
            this.btnResetSession.Symbol = 61453;
            this.btnResetSession.Click += new System.EventHandler(this.btnResetSession_Click);

            this.btnLoadSamples.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLoadSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLoadSamples.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnLoadSamples.Text = "Load mẫu";
            this.btnLoadSamples.Symbol = 61459;
            this.btnLoadSamples.Click += new System.EventHandler(this.btnLoadSamples_Click);

            this.lblCreatedBy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCreatedBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblCreatedBy.Text = "Created By:";
            this.lblCreatedBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.txtCreatedBy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCreatedBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtCreatedBy.Text = "Admin";

            // tabApiGroups
            this.tabApiGroups.Controls.Add(this.tabLifecycle);
            this.tabApiGroups.Controls.Add(this.tabWrite);
            this.tabApiGroups.Controls.Add(this.tabRead);
            this.tabApiGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabApiGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabApiGroups.Name = "tabApiGroups";

            // tabLifecycle
            this.tabLifecycle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLifecycle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabLifecycle.Name = "tabLifecycle";
            this.tabLifecycle.Text = "Pool lifecycle";
            this.tabLifecycle.Padding = new System.Windows.Forms.Padding(8);
            this.tabLifecycle.Controls.Add(BuildLifecycleLayout());

            // tabWrite
            this.tabWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabWrite.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabWrite.Name = "tabWrite";
            this.tabWrite.Text = "Code write";
            this.tabWrite.Padding = new System.Windows.Forms.Padding(8);
            this.tabWrite.Controls.Add(BuildWriteLayout());

            // tabRead
            this.tabRead.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabRead.Name = "tabRead";
            this.tabRead.Text = "Code read";
            this.tabRead.Padding = new System.Windows.Forms.Padding(8);
            this.tabRead.Controls.Add(BuildReadLayout());

            // panelLog
            this.panelLog.Controls.Add(this.lstLog);
            this.panelLog.Controls.Add(this.btnClearLog);
            this.panelLog.Controls.Add(this.btnCopyLog);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Padding = new System.Windows.Forms.Padding(1, 28, 1, 1);
            this.panelLog.Text = "Log";
            this.panelLog.TitleHeight = 28;

            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);

            this.btnClearLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearLog.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClearLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnClearLog.Size = new System.Drawing.Size(90, 30);
            this.btnClearLog.Text = "Clear";
            this.btnClearLog.Symbol = 61453;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

            this.btnCopyLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCopyLog.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCopyLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCopyLog.Size = new System.Drawing.Size(90, 30);
            this.btnCopyLog.Text = "Copy";
            this.btnCopyLog.Symbol = 61481;
            this.btnCopyLog.Click += new System.EventHandler(this.btnCopyLog_Click);

            // FDashboard
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1100, 760);
            this.Controls.Add(this.panelRoot);
            this.Name = "FDashboard";
            this.Text = "Quản Lý Data Pool";

            this.panelRoot.ResumeLayout(false);
            this.tableRoot.ResumeLayout(false);
            this.panelSession.ResumeLayout(false);
            this.tableSession.ResumeLayout(false);
            this.tabApiGroups.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Sunny.UI.UITableLayoutPanel BuildLifecycleLayout()
        {
            var layout = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4
            };
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            for (int i = 0; i < 4; i++)
            {
                layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            }

            btnGetPoolPath.Text = "GetPoolPath";
            btnGetPoolPath.Symbol = 62438;
            btnGetPoolPath.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetPoolPath.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetPoolPath.Click += new System.EventHandler(this.btnGetPoolPath_Click);

            btnCreatePool.Text = "CreatePool";
            btnCreatePool.Symbol = 57359;
            btnCreatePool.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCreatePool.Dock = System.Windows.Forms.DockStyle.Fill;
            btnCreatePool.Click += new System.EventHandler(this.btnCreatePool_Click);

            btnGetPoolInfo.Text = "GetPoolInfo";
            btnGetPoolInfo.Symbol = 62438;
            btnGetPoolInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetPoolInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetPoolInfo.Click += new System.EventHandler(this.btnGetPoolInfo_Click);

            btnGetPoolsPaginated.Text = "GetPoolsPaginated (trang 1)";
            btnGetPoolsPaginated.Symbol = 61950;
            btnGetPoolsPaginated.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetPoolsPaginated.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetPoolsPaginated.Click += new System.EventHandler(this.btnGetPoolsPaginated_Click);

            layout.Controls.Add(btnGetPoolPath, 0, 0);
            layout.Controls.Add(btnCreatePool, 1, 0);
            layout.Controls.Add(btnGetPoolInfo, 0, 1);
            layout.Controls.Add(btnGetPoolsPaginated, 1, 1);
            return layout;
        }

        private Sunny.UI.UITableLayoutPanel BuildWriteLayout()
        {
            var layout = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));

            var lblSingle = new Sunny.UI.UILabel { Text = "Code:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblFile = new Sunny.UI.UILabel { Text = "File CSV:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblUpdPC = new Sunny.UI.UILabel { Text = "PoolCode:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblUpdID = new Sunny.UI.UILabel { Text = "CodeID:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblStatus = new Sunny.UI.UILabel { Text = "New Status:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

            txtSingleCode.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSingleCode.Text = "CODE001";

            btnBrowseFile.Text = "Chọn file";
            btnBrowseFile.Symbol = 61442;
            btnBrowseFile.Cursor = System.Windows.Forms.Cursors.Hand;
            btnBrowseFile.Dock = System.Windows.Forms.DockStyle.Fill;
            btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);

            txtFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
            txtFilePath.ReadOnly = true;
            txtFilePath.Text = "(chưa chọn)";

            btnAddCodeSingle.Text = "AddCodes(mode=1 single)";
            btnAddCodeSingle.Symbol = 57345;
            btnAddCodeSingle.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddCodeSingle.Dock = System.Windows.Forms.DockStyle.Fill;
            btnAddCodeSingle.Click += new System.EventHandler(this.btnAddCodeSingle_Click);

            btnAddCodeFile.Text = "AddCodes(mode=0 file)";
            btnAddCodeFile.Symbol = 61443;
            btnAddCodeFile.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddCodeFile.Dock = System.Windows.Forms.DockStyle.Fill;
            btnAddCodeFile.Click += new System.EventHandler(this.btnAddCodeFile_Click);

            txtUpdatePoolCode.Dock = System.Windows.Forms.DockStyle.Fill;
            txtUpdateCodeID.Dock = System.Windows.Forms.DockStyle.Fill;

            cboUpdateStatus.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cboUpdateStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            cboUpdateStatus.Items.Clear();
            cboUpdateStatus.Items.Add("(không đổi)");
            cboUpdateStatus.Items.Add("0 - Unused");
            cboUpdateStatus.Items.Add("1 - Used");
            cboUpdateStatus.Items.Add("-1 - Error");
            cboUpdateStatus.SelectedIndex = 0;

            btnUpdateStatus.Text = "UpdateCodeStatus";
            btnUpdateStatus.Symbol = 61733;
            btnUpdateStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            btnUpdateStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            btnUpdateStatus.Click += new System.EventHandler(this.btnUpdateStatus_Click);

            layout.Controls.Add(lblSingle, 0, 0);
            layout.Controls.Add(txtSingleCode, 1, 0);
            layout.Controls.Add(btnAddCodeSingle, 2, 0);
            layout.SetColumnSpan(btnAddCodeSingle, 2);

            layout.Controls.Add(lblFile, 0, 1);
            layout.Controls.Add(btnBrowseFile, 1, 1);
            layout.Controls.Add(txtFilePath, 2, 1);
            layout.Controls.Add(btnAddCodeFile, 3, 1);

            layout.Controls.Add(lblUpdPC, 0, 2);
            layout.Controls.Add(txtUpdatePoolCode, 1, 2);
            layout.Controls.Add(lblUpdID, 2, 2);
            layout.Controls.Add(txtUpdateCodeID, 3, 2);

            // Hàng phụ cho status + button update (panel con Dock=Top)
            var statusRow = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 50,
                ColumnCount = 4
            };
            statusRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            statusRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            statusRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            statusRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            statusRow.Controls.Add(lblStatus, 0, 0);
            statusRow.Controls.Add(cboUpdateStatus, 1, 0);
            statusRow.Controls.Add(btnUpdateStatus, 2, 0);
            statusRow.SetColumnSpan(btnUpdateStatus, 2);

            layout.Controls.Add(statusRow, 0, 3);
            layout.SetColumnSpan(statusRow, 4);
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            return layout;
        }

        private Sunny.UI.UITableLayoutPanel BuildReadLayout()
        {
            var layout = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4
            };
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            for (int i = 0; i < 4; i++)
            {
                layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            }

            var lblQPC = new Sunny.UI.UILabel { Text = "PoolCode:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblFS = new Sunny.UI.UILabel { Text = "Status:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblFB = new Sunny.UI.UILabel { Text = "BatchID:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblSo = new Sunny.UI.UILabel { Text = "Status:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

            txtQueryPoolCode.Dock = System.Windows.Forms.DockStyle.Fill;
            txtQueryCodeID.Dock = System.Windows.Forms.DockStyle.Fill;
            txtFilterBatchID.Dock = System.Windows.Forms.DockStyle.Fill;

            cboFilterStatus.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cboFilterStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            cboFilterStatus.Items.Clear();
            cboFilterStatus.Items.Add("All");
            cboFilterStatus.Items.Add("0");
            cboFilterStatus.Items.Add("1");
            cboFilterStatus.Items.Add("-1");
            cboFilterStatus.SelectedIndex = 0;

            cboStatusOnly.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cboStatusOnly.Dock = System.Windows.Forms.DockStyle.Fill;
            cboStatusOnly.Items.Clear();
            cboStatusOnly.Items.Add("All");
            cboStatusOnly.Items.Add("0");
            cboStatusOnly.Items.Add("1");
            cboStatusOnly.Items.Add("-1");
            cboStatusOnly.SelectedIndex = 0;

            btnGetPoolCode.Text = "GetPoolCode";
            btnGetPoolCode.Symbol = 61733;
            btnGetPoolCode.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetPoolCode.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetPoolCode.Click += new System.EventHandler(this.btnGetPoolCode_Click);

            btnGetCodesPaginated.Text = "GetCodesPaginated (page 1, size 100)";
            btnGetCodesPaginated.Symbol = 61950;
            btnGetCodesPaginated.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetCodesPaginated.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetCodesPaginated.Click += new System.EventHandler(this.btnGetCodesPaginated_Click);

            btnGetCodesByStatus.Text = "GetCodesByStatus";
            btnGetCodesByStatus.Symbol = 61733;
            btnGetCodesByStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetCodesByStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetCodesByStatus.Click += new System.EventHandler(this.btnGetCodesByStatus_Click);

            btnGetCodeCounts.Text = "GetCodeCounts";
            btnGetCodeCounts.Symbol = 57794;
            btnGetCodeCounts.Cursor = System.Windows.Forms.Cursors.Hand;
            btnGetCodeCounts.Dock = System.Windows.Forms.DockStyle.Fill;
            btnGetCodeCounts.Click += new System.EventHandler(this.btnGetCodeCounts_Click);

            layout.Controls.Add(lblQPC, 0, 0);
            layout.Controls.Add(txtQueryPoolCode, 1, 0);
            layout.Controls.Add(btnGetPoolCode, 2, 0);
            layout.SetColumnSpan(btnGetPoolCode, 2);

            layout.Controls.Add(lblFS, 0, 1);
            layout.Controls.Add(cboFilterStatus, 1, 1);
            layout.Controls.Add(lblFB, 2, 1);
            layout.Controls.Add(txtFilterBatchID, 3, 1);

            layout.Controls.Add(btnGetCodesPaginated, 0, 2);
            layout.SetColumnSpan(btnGetCodesPaginated, 4);

            layout.Controls.Add(lblSo, 0, 3);
            layout.Controls.Add(cboStatusOnly, 1, 3);
            layout.Controls.Add(btnGetCodesByStatus, 2, 3);
            layout.Controls.Add(btnGetCodeCounts, 3, 3);

            return layout;
        }

        #endregion
    }
}
