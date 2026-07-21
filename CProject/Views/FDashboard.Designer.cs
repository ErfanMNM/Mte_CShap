namespace CProject.Views
{
    partial class FDashboard
    {
        private System.ComponentModel.IContainer components = null;

        private Sunny.UI.UITitlePanel panelDataPool;
        private Sunny.UI.UITableLayoutPanel tableLayoutDataPool;
        private Sunny.UI.UITextBox txtPoolName;
        private Sunny.UI.UILabel lblPoolName;
        private Sunny.UI.UITextBox txtCode;
        private Sunny.UI.UILabel lblCode;
        private Sunny.UI.UISymbolButton btnCreatePool;
        private Sunny.UI.UISymbolButton btnAddCodeSingle;
        private Sunny.UI.UISymbolButton btnAddCodeFile;
        private Sunny.UI.UISymbolButton btnGetPoolInfo;
        private Sunny.UI.UIListBox lstResult;
        private Sunny.UI.UITabControl tabControlTest;
        private System.Windows.Forms.TabPage tabPageA1A5;
        private System.Windows.Forms.TabPage tabPageA6A7;
        private Sunny.UI.UILabel lblPoolNameA1;
        private Sunny.UI.UITextBox txtPoolNameA1;
        private Sunny.UI.UILabel lblPoolCodeA1;
        private Sunny.UI.UITextBox txtPoolCodeA1;
        private Sunny.UI.UILabel lblCodeIDA1;
        private Sunny.UI.UITextBox txtCodeIDA1;
        private Sunny.UI.UILabel lblNewStatus;
        private Sunny.UI.UITextBox txtNewStatus;
        private Sunny.UI.UISymbolButton btnA1UpdateStatus;
        private Sunny.UI.UISymbolButton btnA2GetPoolInfo;
        private Sunny.UI.UISymbolButton btnA3GetPoolCode;
        private Sunny.UI.UISymbolButton btnA4GetCodesPaginated;
        private Sunny.UI.UISymbolButton btnA5GetCounts;
        private Sunny.UI.UIDataGridView dgvResultA1A5;
        private Sunny.UI.UILabel lblFilterStatus;
        private Sunny.UI.UIComboBox cboFilterStatus;
        private Sunny.UI.UILabel lblFilterBatchID;
        private Sunny.UI.UITextBox txtFilterBatchID;
        private Sunny.UI.UILabel lblPageInfoA4;
        private Sunny.UI.UISymbolButton btnPrevPage;
        private Sunny.UI.UISymbolButton btnNextPage;
        private Sunny.UI.UILabel lblPoolNameA6;
        private Sunny.UI.UITextBox txtPoolNameA6;
        private Sunny.UI.UILabel lblStatusFilterA6;
        private Sunny.UI.UIComboBox cboStatusFilterA6;
        private Sunny.UI.UISymbolButton btnA6GetCodesByStatus;
        private Sunny.UI.UISymbolButton btnA7GetPoolsPaginated;
        private Sunny.UI.UIDataGridView dgvResultA6A7;
        private Sunny.UI.UILabel lblPageInfoA7;
        private Sunny.UI.UISymbolButton btnPrevPageA7;
        private Sunny.UI.UISymbolButton btnNextPageA7;
        private Sunny.UI.UIPanel panelA1Controls;
        private Sunny.UI.UIPanel panelA2A5Controls;

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
            this.panelDataPool = new Sunny.UI.UITitlePanel();
            this.tableLayoutDataPool = new Sunny.UI.UITableLayoutPanel();
            this.lblPoolName = new Sunny.UI.UILabel();
            this.txtPoolName = new Sunny.UI.UITextBox();
            this.lblCode = new Sunny.UI.UILabel();
            this.txtCode = new Sunny.UI.UITextBox();
            this.btnCreatePool = new Sunny.UI.UISymbolButton();
            this.btnAddCodeSingle = new Sunny.UI.UISymbolButton();
            this.btnAddCodeFile = new Sunny.UI.UISymbolButton();
            this.btnGetPoolInfo = new Sunny.UI.UISymbolButton();
            this.lstResult = new Sunny.UI.UIListBox();
            this.tabControlTest = new Sunny.UI.UITabControl();
            this.tabPageA1A5 = new System.Windows.Forms.TabPage();
            this.panelA2A5Controls = new Sunny.UI.UIPanel();
            this.panelA1Controls = new Sunny.UI.UIPanel();
            this.lblPoolNameA1 = new Sunny.UI.UILabel();
            this.txtPoolNameA1 = new Sunny.UI.UITextBox();
            this.lblPoolCodeA1 = new Sunny.UI.UILabel();
            this.txtPoolCodeA1 = new Sunny.UI.UITextBox();
            this.lblCodeIDA1 = new Sunny.UI.UILabel();
            this.txtCodeIDA1 = new Sunny.UI.UITextBox();
            this.lblNewStatus = new Sunny.UI.UILabel();
            this.txtNewStatus = new Sunny.UI.UITextBox();
            this.btnA1UpdateStatus = new Sunny.UI.UISymbolButton();
            this.btnA2GetPoolInfo = new Sunny.UI.UISymbolButton();
            this.btnA3GetPoolCode = new Sunny.UI.UISymbolButton();
            this.btnA4GetCodesPaginated = new Sunny.UI.UISymbolButton();
            this.btnA5GetCounts = new Sunny.UI.UISymbolButton();
            this.dgvResultA1A5 = new Sunny.UI.UIDataGridView();
            this.lblFilterStatus = new Sunny.UI.UILabel();
            this.cboFilterStatus = new Sunny.UI.UIComboBox();
            this.lblFilterBatchID = new Sunny.UI.UILabel();
            this.txtFilterBatchID = new Sunny.UI.UITextBox();
            this.lblPageInfoA4 = new Sunny.UI.UILabel();
            this.btnPrevPage = new Sunny.UI.UISymbolButton();
            this.btnNextPage = new Sunny.UI.UISymbolButton();
            this.tabPageA6A7 = new System.Windows.Forms.TabPage();
            this.lblPoolNameA6 = new Sunny.UI.UILabel();
            this.txtPoolNameA6 = new Sunny.UI.UITextBox();
            this.lblStatusFilterA6 = new Sunny.UI.UILabel();
            this.cboStatusFilterA6 = new Sunny.UI.UIComboBox();
            this.btnA6GetCodesByStatus = new Sunny.UI.UISymbolButton();
            this.btnA7GetPoolsPaginated = new Sunny.UI.UISymbolButton();
            this.dgvResultA6A7 = new Sunny.UI.UIDataGridView();
            this.lblPageInfoA7 = new Sunny.UI.UILabel();
            this.btnPrevPageA7 = new Sunny.UI.UISymbolButton();
            this.btnNextPageA7 = new Sunny.UI.UISymbolButton();

            this.panelDataPool.SuspendLayout();
            this.tableLayoutDataPool.SuspendLayout();
            this.tabControlTest.SuspendLayout();
            this.tabPageA1A5.SuspendLayout();
            this.tabPageA6A7.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelDataPool
            // 
            this.panelDataPool.Controls.Add(this.tableLayoutDataPool);
            this.panelDataPool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDataPool.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.panelDataPool.Location = new System.Drawing.Point(0, 0);
            this.panelDataPool.Name = "panelDataPool";
            this.panelDataPool.Padding = new System.Windows.Forms.Padding(1, 36, 1, 1);
            this.panelDataPool.Size = new System.Drawing.Size(961, 581);
            this.panelDataPool.TabIndex = 0;
            this.panelDataPool.Text = "QUẢN LÝ DATA POOL";
            this.panelDataPool.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelDataPool.TitleHeight = 36;
            // 
            // tableLayoutDataPool
            // 
            this.tableLayoutDataPool.BackColor = System.Drawing.Color.White;
            this.tableLayoutDataPool.ColumnCount = 4;
            this.tableLayoutDataPool.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutDataPool.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutDataPool.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutDataPool.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutDataPool.Controls.Add(this.lblPoolName, 0, 0);
            this.tableLayoutDataPool.Controls.Add(this.txtPoolName, 0, 1);
            this.tableLayoutDataPool.Controls.Add(this.lblCode, 1, 0);
            this.tableLayoutDataPool.Controls.Add(this.txtCode, 1, 1);
            this.tableLayoutDataPool.Controls.Add(this.btnCreatePool, 0, 2);
            this.tableLayoutDataPool.Controls.Add(this.btnAddCodeSingle, 1, 2);
            this.tableLayoutDataPool.Controls.Add(this.btnAddCodeFile, 2, 2);
            this.tableLayoutDataPool.Controls.Add(this.btnGetPoolInfo, 3, 2);
            this.tableLayoutDataPool.Controls.Add(this.tabControlTest, 0, 3);
            this.tableLayoutDataPool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutDataPool.Location = new System.Drawing.Point(1, 36);
            this.tableLayoutDataPool.Name = "tableLayoutDataPool";
            this.tableLayoutDataPool.Padding = new System.Windows.Forms.Padding(10);
            this.tableLayoutDataPool.RowCount = 4;
            this.tableLayoutDataPool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutDataPool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutDataPool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutDataPool.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutDataPool.Size = new System.Drawing.Size(959, 544);
            this.tableLayoutDataPool.TabIndex = 0;
            // 
            // lblPoolName
            // 
            this.lblPoolName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPoolName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblPoolName.Location = new System.Drawing.Point(13, 10);
            this.lblPoolName.Name = "lblPoolName";
            this.lblPoolName.Size = new System.Drawing.Size(224, 30);
            this.lblPoolName.TabIndex = 0;
            this.lblPoolName.Text = "Tên Pool:";
            this.lblPoolName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPoolName
            // 
            this.txtPoolName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPoolName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtPoolName.Location = new System.Drawing.Point(13, 43);
            this.txtPoolName.Name = "txtPoolName";
            this.txtPoolName.Size = new System.Drawing.Size(224, 25);
            this.txtPoolName.TabIndex = 1;
            this.txtPoolName.Text = "TestPool";
            // 
            // lblCode
            // 
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblCode.Location = new System.Drawing.Point(251, 10);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(224, 30);
            this.lblCode.TabIndex = 2;
            this.lblCode.Text = "Code (cho mode 1):";
            this.lblCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCode
            // 
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtCode.Location = new System.Drawing.Point(251, 43);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(224, 25);
            this.txtCode.TabIndex = 3;
            this.txtCode.Text = "CODE001";
            // 
            // btnCreatePool
            // 
            this.btnCreatePool.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreatePool.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCreatePool.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnCreatePool.Location = new System.Drawing.Point(13, 93);
            this.btnCreatePool.Name = "btnCreatePool";
            this.btnCreatePool.Size = new System.Drawing.Size(224, 44);
            this.btnCreatePool.Symbol = 57359;
            this.btnCreatePool.SymbolSize = 16;
            this.btnCreatePool.TabIndex = 4;
            this.btnCreatePool.Text = "Tạo Pool";
            this.btnCreatePool.Click += new System.EventHandler(this.btnCreatePool_Click);
            // 
            // btnAddCodeSingle
            // 
            this.btnAddCodeSingle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddCodeSingle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddCodeSingle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnAddCodeSingle.Location = new System.Drawing.Point(251, 93);
            this.btnAddCodeSingle.Name = "btnAddCodeSingle";
            this.btnAddCodeSingle.Size = new System.Drawing.Size(224, 44);
            this.btnAddCodeSingle.Symbol = 57345;
            this.btnAddCodeSingle.SymbolSize = 16;
            this.btnAddCodeSingle.TabIndex = 5;
            this.btnAddCodeSingle.Text = "Thêm 1 Code";
            this.btnAddCodeSingle.Click += new System.EventHandler(this.btnAddCodeSingle_Click);
            // 
            // btnAddCodeFile
            // 
            this.btnAddCodeFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddCodeFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddCodeFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnAddCodeFile.Location = new System.Drawing.Point(489, 93);
            this.btnAddCodeFile.Name = "btnAddCodeFile";
            this.btnAddCodeFile.Size = new System.Drawing.Size(224, 44);
            this.btnAddCodeFile.Symbol = 61443;
            this.btnAddCodeFile.SymbolSize = 16;
            this.btnAddCodeFile.TabIndex = 6;
            this.btnAddCodeFile.Text = "Thêm từ File";
            this.btnAddCodeFile.Click += new System.EventHandler(this.btnAddCodeFile_Click);
            // 
            // btnGetPoolInfo
            // 
            this.btnGetPoolInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGetPoolInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGetPoolInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnGetPoolInfo.Location = new System.Drawing.Point(727, 93);
            this.btnGetPoolInfo.Name = "btnGetPoolInfo";
            this.btnGetPoolInfo.Size = new System.Drawing.Size(219, 44);
            this.btnGetPoolInfo.Symbol = 62438;
            this.btnGetPoolInfo.SymbolSize = 16;
            this.btnGetPoolInfo.TabIndex = 7;
            this.btnGetPoolInfo.Text = "Lấy thông tin Pool";
            this.btnGetPoolInfo.Click += new System.EventHandler(this.btnGetPoolInfo_Click);
            // 
            // tabControlTest
            // 
            this.tableLayoutDataPool.SetColumnSpan(this.tabControlTest, 4);
            this.tabControlTest.Controls.Add(this.tabPageA1A5);
            this.tabControlTest.Controls.Add(this.tabPageA6A7);
            this.tabControlTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabControlTest.Location = new System.Drawing.Point(13, 150);
            this.tabControlTest.Name = "tabControlTest";
            this.tabControlTest.Size = new System.Drawing.Size(933, 381);
            this.tabControlTest.TabIndex = 8;
            // 
            // tabPageA1A5
            // 
            this.tabPageA1A5.Controls.Add(this.panelA1Controls);
            this.tabPageA1A5.Controls.Add(this.panelA2A5Controls);
            this.tabPageA1A5.Controls.Add(this.dgvResultA1A5);
            this.tabPageA1A5.Controls.Add(this.lblPageInfoA4);
            this.tabPageA1A5.Controls.Add(this.btnPrevPage);
            this.tabPageA1A5.Controls.Add(this.btnNextPage);
            this.tabPageA1A5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPageA1A5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabPageA1A5.Location = new System.Drawing.Point(4, 36);
            this.tabPageA1A5.Name = "tabPageA1A5";
            this.tabPageA1A5.Size = new System.Drawing.Size(925, 341);
            this.tabPageA1A5.TabIndex = 0;
            this.tabPageA1A5.Text = "Test A1-A5";
            // 
            // panelA1Controls
            // 
            this.panelA1Controls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.panelA1Controls.Controls.Add(this.lblPoolNameA1);
            this.panelA1Controls.Controls.Add(this.txtPoolNameA1);
            this.panelA1Controls.Controls.Add(this.lblPoolCodeA1);
            this.panelA1Controls.Controls.Add(this.txtPoolCodeA1);
            this.panelA1Controls.Controls.Add(this.lblCodeIDA1);
            this.panelA1Controls.Controls.Add(this.txtCodeIDA1);
            this.panelA1Controls.Controls.Add(this.lblNewStatus);
            this.panelA1Controls.Controls.Add(this.txtNewStatus);
            this.panelA1Controls.Controls.Add(this.btnA1UpdateStatus);
            this.panelA1Controls.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelA1Controls.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panelA1Controls.Location = new System.Drawing.Point(0, 0);
            this.panelA1Controls.Name = "panelA1Controls";
            this.panelA1Controls.Padding = new System.Windows.Forms.Padding(5);
            this.panelA1Controls.Size = new System.Drawing.Size(925, 50);
            this.panelA1Controls.TabIndex = 0;
            // 
            // panelA2A5Controls
            // 
            this.panelA2A5Controls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panelA2A5Controls.Controls.Add(this.btnA2GetPoolInfo);
            this.panelA2A5Controls.Controls.Add(this.btnA3GetPoolCode);
            this.panelA2A5Controls.Controls.Add(this.btnA4GetCodesPaginated);
            this.panelA2A5Controls.Controls.Add(this.btnA5GetCounts);
            this.panelA2A5Controls.Controls.Add(this.lblFilterStatus);
            this.panelA2A5Controls.Controls.Add(this.cboFilterStatus);
            this.panelA2A5Controls.Controls.Add(this.lblFilterBatchID);
            this.panelA2A5Controls.Controls.Add(this.txtFilterBatchID);
            this.panelA2A5Controls.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelA2A5Controls.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panelA2A5Controls.Location = new System.Drawing.Point(0, 50);
            this.panelA2A5Controls.Name = "panelA2A5Controls";
            this.panelA2A5Controls.Padding = new System.Windows.Forms.Padding(5);
            this.panelA2A5Controls.Size = new System.Drawing.Size(925, 50);
            this.panelA2A5Controls.TabIndex = 1;
            // 
            // lblPoolNameA1
            // 
            this.lblPoolNameA1.AutoSize = true;
            this.lblPoolNameA1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblPoolNameA1.Location = new System.Drawing.Point(8, 8);
            this.lblPoolNameA1.Name = "lblPoolNameA1";
            this.lblPoolNameA1.Size = new System.Drawing.Size(69, 15);
            this.lblPoolNameA1.TabIndex = 0;
            this.lblPoolNameA1.Text = "Pool Name:";
            // 
            // txtPoolNameA1
            // 
            this.txtPoolNameA1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtPoolNameA1.Location = new System.Drawing.Point(8, 25);
            this.txtPoolNameA1.Name = "txtPoolNameA1";
            this.txtPoolNameA1.Size = new System.Drawing.Size(120, 23);
            this.txtPoolNameA1.TabIndex = 1;
            this.txtPoolNameA1.Text = "TestPool";
            // 
            // lblPoolCodeA1
            // 
            this.lblPoolCodeA1.AutoSize = true;
            this.lblPoolCodeA1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblPoolCodeA1.Location = new System.Drawing.Point(135, 8);
            this.lblPoolCodeA1.Name = "lblPoolCodeA1";
            this.lblPoolCodeA1.Size = new System.Drawing.Size(61, 15);
            this.lblPoolCodeA1.TabIndex = 2;
            this.lblPoolCodeA1.Text = "PoolCode:";
            // 
            // txtPoolCodeA1
            // 
            this.txtPoolCodeA1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtPoolCodeA1.Location = new System.Drawing.Point(135, 25);
            this.txtPoolCodeA1.Name = "txtPoolCodeA1";
            this.txtPoolCodeA1.Size = new System.Drawing.Size(100, 23);
            this.txtPoolCodeA1.TabIndex = 3;
            // 
            // lblCodeIDA1
            // 
            this.lblCodeIDA1.AutoSize = true;
            this.lblCodeIDA1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblCodeIDA1.Location = new System.Drawing.Point(242, 8);
            this.lblCodeIDA1.Name = "lblCodeIDA1";
            this.lblCodeIDA1.Size = new System.Drawing.Size(51, 15);
            this.lblCodeIDA1.TabIndex = 4;
            this.lblCodeIDA1.Text = "Code ID:";
            // 
            // txtCodeIDA1
            // 
            this.txtCodeIDA1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtCodeIDA1.Location = new System.Drawing.Point(242, 25);
            this.txtCodeIDA1.Name = "txtCodeIDA1";
            this.txtCodeIDA1.Size = new System.Drawing.Size(60, 23);
            this.txtCodeIDA1.TabIndex = 5;
            // 
            // lblNewStatus
            // 
            this.lblNewStatus.AutoSize = true;
            this.lblNewStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblNewStatus.Location = new System.Drawing.Point(310, 8);
            this.lblNewStatus.Name = "lblNewStatus";
            this.lblNewStatus.Size = new System.Drawing.Size(71, 15);
            this.lblNewStatus.TabIndex = 6;
            this.lblNewStatus.Text = "NewStatus:";
            // 
            // txtNewStatus
            // 
            this.txtNewStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtNewStatus.Location = new System.Drawing.Point(310, 25);
            this.txtNewStatus.Name = "txtNewStatus";
            this.txtNewStatus.Size = new System.Drawing.Size(60, 23);
            this.txtNewStatus.TabIndex = 7;
            this.txtNewStatus.Text = "0";
            // 
            // btnA1UpdateStatus
            // 
            this.btnA1UpdateStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA1UpdateStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA1UpdateStatus.Location = new System.Drawing.Point(378, 15);
            this.btnA1UpdateStatus.Name = "btnA1UpdateStatus";
            this.btnA1UpdateStatus.Size = new System.Drawing.Size(120, 35);
            this.btnA1UpdateStatus.Symbol = 61733;
            this.btnA1UpdateStatus.SymbolSize = 14;
            this.btnA1UpdateStatus.TabIndex = 8;
            this.btnA1UpdateStatus.Text = "A1: UpdateStatus";
            this.btnA1UpdateStatus.Click += new System.EventHandler(this.btnA1UpdateStatus_Click);
            // 
            // btnA2GetPoolInfo
            // 
            this.btnA2GetPoolInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA2GetPoolInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA2GetPoolInfo.Location = new System.Drawing.Point(8, 8);
            this.btnA2GetPoolInfo.Name = "btnA2GetPoolInfo";
            this.btnA2GetPoolInfo.Size = new System.Drawing.Size(150, 36);
            this.btnA2GetPoolInfo.Symbol = 62438;
            this.btnA2GetPoolInfo.SymbolSize = 14;
            this.btnA2GetPoolInfo.TabIndex = 0;
            this.btnA2GetPoolInfo.Text = "A2: GetPoolInfo";
            this.btnA2GetPoolInfo.Click += new System.EventHandler(this.btnA2GetPoolInfo_Click);
            // 
            // btnA3GetPoolCode
            // 
            this.btnA3GetPoolCode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA3GetPoolCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA3GetPoolCode.Location = new System.Drawing.Point(165, 8);
            this.btnA3GetPoolCode.Name = "btnA3GetPoolCode";
            this.btnA3GetPoolCode.Size = new System.Drawing.Size(150, 36);
            this.btnA3GetPoolCode.Symbol = 61733;
            this.btnA3GetPoolCode.SymbolSize = 14;
            this.btnA3GetPoolCode.TabIndex = 1;
            this.btnA3GetPoolCode.Text = "A3: GetPoolCode";
            this.btnA3GetPoolCode.Click += new System.EventHandler(this.btnA3GetPoolCode_Click);
            // 
            // btnA4GetCodesPaginated
            // 
            this.btnA4GetCodesPaginated.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA4GetCodesPaginated.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA4GetCodesPaginated.Location = new System.Drawing.Point(322, 8);
            this.btnA4GetCodesPaginated.Name = "btnA4GetCodesPaginated";
            this.btnA4GetCodesPaginated.Size = new System.Drawing.Size(180, 36);
            this.btnA4GetCodesPaginated.Symbol = 61950;
            this.btnA4GetCodesPaginated.SymbolSize = 14;
            this.btnA4GetCodesPaginated.TabIndex = 2;
            this.btnA4GetCodesPaginated.Text = "A4: GetCodesPaginated";
            this.btnA4GetCodesPaginated.Click += new System.EventHandler(this.btnA4GetCodesPaginated_Click);
            // 
            // btnA5GetCounts
            // 
            this.btnA5GetCounts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA5GetCounts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA5GetCounts.Location = new System.Drawing.Point(510, 8);
            this.btnA5GetCounts.Name = "btnA5GetCounts";
            this.btnA5GetCounts.Size = new System.Drawing.Size(150, 36);
            this.btnA5GetCounts.Symbol = 57794;
            this.btnA5GetCounts.SymbolSize = 14;
            this.btnA5GetCounts.TabIndex = 3;
            this.btnA5GetCounts.Text = "A5: GetCodeCounts";
            this.btnA5GetCounts.Click += new System.EventHandler(this.btnA5GetCounts_Click);
            // 
            // lblFilterStatus
            // 
            this.lblFilterStatus.AutoSize = true;
            this.lblFilterStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFilterStatus.Location = new System.Drawing.Point(670, 8);
            this.lblFilterStatus.Name = "lblFilterStatus";
            this.lblFilterStatus.Size = new System.Drawing.Size(54, 15);
            this.lblFilterStatus.TabIndex = 4;
            this.lblFilterStatus.Text = "Status:";
            // 
            // cboFilterStatus
            // 
            this.cboFilterStatus.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cboFilterStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cboFilterStatus.Location = new System.Drawing.Point(670, 25);
            this.cboFilterStatus.Name = "cboFilterStatus";
            this.cboFilterStatus.Size = new System.Drawing.Size(100, 23);
            this.cboFilterStatus.TabIndex = 5;
            this.cboFilterStatus.Items.AddRange(new object[] { "All", "0-Unused", "1-Used", "-1-Error" });
            this.cboFilterStatus.SelectedIndex = 0;
            // 
            // lblFilterBatchID
            // 
            this.lblFilterBatchID.AutoSize = true;
            this.lblFilterBatchID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblFilterBatchID.Location = new System.Drawing.Point(785, 8);
            this.lblFilterBatchID.Name = "lblFilterBatchID";
            this.lblFilterBatchID.Size = new System.Drawing.Size(62, 15);
            this.lblFilterBatchID.TabIndex = 6;
            this.lblFilterBatchID.Text = "BatchID:";
            // 
            // txtFilterBatchID
            // 
            this.txtFilterBatchID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtFilterBatchID.Location = new System.Drawing.Point(785, 25);
            this.txtFilterBatchID.Name = "txtFilterBatchID";
            this.txtFilterBatchID.Size = new System.Drawing.Size(120, 23);
            this.txtFilterBatchID.TabIndex = 7;
            // 
            // dgvResultA1A5
            // 
            this.dgvResultA1A5.AllowUserToAddRows = false;
            this.dgvResultA1A5.AllowUserToDeleteRows = false;
            this.dgvResultA1A5.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResultA1A5.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResultA1A5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResultA1A5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dgvResultA1A5.Location = new System.Drawing.Point(0, 100);
            this.dgvResultA1A5.Name = "dgvResultA1A5";
            this.dgvResultA1A5.ReadOnly = true;
            this.dgvResultA1A5.RowHeadersWidth = 51;
            this.dgvResultA1A5.RowTemplate.Height = 24;
            this.dgvResultA1A5.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResultA1A5.Size = new System.Drawing.Size(925, 206);
            this.dgvResultA1A5.TabIndex = 2;
            // 
            // lblPageInfoA4
            // 
            this.lblPageInfoA4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblPageInfoA4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblPageInfoA4.Location = new System.Drawing.Point(0, 306);
            this.lblPageInfoA4.Name = "lblPageInfoA4";
            this.lblPageInfoA4.Size = new System.Drawing.Size(600, 35);
            this.lblPageInfoA4.TabIndex = 3;
            this.lblPageInfoA4.Text = "Page: 1 / 1";
            this.lblPageInfoA4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevPage.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPrevPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnPrevPage.Location = new System.Drawing.Point(825, 306);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(50, 35);
            this.btnPrevPage.Symbol = 360947;
            this.btnPrevPage.SymbolSize = 14;
            this.btnPrevPage.TabIndex = 4;
            this.btnPrevPage.Text = "";
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPage.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNextPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnNextPage.Location = new System.Drawing.Point(875, 306);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(50, 35);
            this.btnNextPage.Symbol = 360868;
            this.btnNextPage.SymbolSize = 14;
            this.btnNextPage.TabIndex = 5;
            this.btnNextPage.Text = "";
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // tabPageA6A7
            // 
            this.tabPageA6A7.Controls.Add(this.lblPoolNameA6);
            this.tabPageA6A7.Controls.Add(this.txtPoolNameA6);
            this.tabPageA6A7.Controls.Add(this.lblStatusFilterA6);
            this.tabPageA6A7.Controls.Add(this.cboStatusFilterA6);
            this.tabPageA6A7.Controls.Add(this.btnA6GetCodesByStatus);
            this.tabPageA6A7.Controls.Add(this.btnA7GetPoolsPaginated);
            this.tabPageA6A7.Controls.Add(this.dgvResultA6A7);
            this.tabPageA6A7.Controls.Add(this.lblPageInfoA7);
            this.tabPageA6A7.Controls.Add(this.btnPrevPageA7);
            this.tabPageA6A7.Controls.Add(this.btnNextPageA7);
            this.tabPageA6A7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPageA6A7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabPageA6A7.Location = new System.Drawing.Point(4, 36);
            this.tabPageA6A7.Name = "tabPageA6A7";
            this.tabPageA6A7.Size = new System.Drawing.Size(925, 341);
            this.tabPageA6A7.TabIndex = 1;
            this.tabPageA6A7.Text = "Test A6-A7";
            // 
            // lblPoolNameA6
            // 
            this.lblPoolNameA6.AutoSize = true;
            this.lblPoolNameA6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblPoolNameA6.Location = new System.Drawing.Point(8, 10);
            this.lblPoolNameA6.Name = "lblPoolNameA6";
            this.lblPoolNameA6.Size = new System.Drawing.Size(69, 15);
            this.lblPoolNameA6.TabIndex = 0;
            this.lblPoolNameA6.Text = "Pool Name:";
            // 
            // txtPoolNameA6
            // 
            this.txtPoolNameA6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtPoolNameA6.Location = new System.Drawing.Point(8, 28);
            this.txtPoolNameA6.Name = "txtPoolNameA6";
            this.txtPoolNameA6.Size = new System.Drawing.Size(150, 23);
            this.txtPoolNameA6.TabIndex = 1;
            this.txtPoolNameA6.Text = "TestPool";
            // 
            // lblStatusFilterA6
            // 
            this.lblStatusFilterA6.AutoSize = true;
            this.lblStatusFilterA6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblStatusFilterA6.Location = new System.Drawing.Point(170, 10);
            this.lblStatusFilterA6.Name = "lblStatusFilterA6";
            this.lblStatusFilterA6.Size = new System.Drawing.Size(54, 15);
            this.lblStatusFilterA6.TabIndex = 2;
            this.lblStatusFilterA6.Text = "Status:";
            // 
            // cboStatusFilterA6
            // 
            this.cboStatusFilterA6.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            this.cboStatusFilterA6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cboStatusFilterA6.Location = new System.Drawing.Point(170, 28);
            this.cboStatusFilterA6.Name = "cboStatusFilterA6";
            this.cboStatusFilterA6.Size = new System.Drawing.Size(100, 23);
            this.cboStatusFilterA6.TabIndex = 3;
            this.cboStatusFilterA6.Items.AddRange(new object[] { "All", "0-Unused", "1-Used", "-1-Error" });
            this.cboStatusFilterA6.SelectedIndex = 0;
            // 
            // btnA6GetCodesByStatus
            // 
            this.btnA6GetCodesByStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA6GetCodesByStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA6GetCodesByStatus.Location = new System.Drawing.Point(285, 18);
            this.btnA6GetCodesByStatus.Name = "btnA6GetCodesByStatus";
            this.btnA6GetCodesByStatus.Size = new System.Drawing.Size(180, 35);
            this.btnA6GetCodesByStatus.Symbol = 61733;
            this.btnA6GetCodesByStatus.SymbolSize = 14;
            this.btnA6GetCodesByStatus.TabIndex = 4;
            this.btnA6GetCodesByStatus.Text = "A6: GetCodesByStatus";
            this.btnA6GetCodesByStatus.Click += new System.EventHandler(this.btnA6GetCodesByStatus_Click);
            // 
            // btnA7GetPoolsPaginated
            // 
            this.btnA7GetPoolsPaginated.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnA7GetPoolsPaginated.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnA7GetPoolsPaginated.Location = new System.Drawing.Point(475, 18);
            this.btnA7GetPoolsPaginated.Name = "btnA7GetPoolsPaginated";
            this.btnA7GetPoolsPaginated.Size = new System.Drawing.Size(180, 35);
            this.btnA7GetPoolsPaginated.Symbol = 61950;
            this.btnA7GetPoolsPaginated.SymbolSize = 14;
            this.btnA7GetPoolsPaginated.TabIndex = 5;
            this.btnA7GetPoolsPaginated.Text = "A7: GetPoolsPaginated";
            this.btnA7GetPoolsPaginated.Click += new System.EventHandler(this.btnA7GetPoolsPaginated_Click);
            // 
            // dgvResultA6A7
            // 
            this.dgvResultA6A7.AllowUserToAddRows = false;
            this.dgvResultA6A7.AllowUserToDeleteRows = false;
            this.dgvResultA6A7.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResultA6A7.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResultA6A7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResultA6A7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.dgvResultA6A7.Location = new System.Drawing.Point(0, 60);
            this.dgvResultA6A7.Name = "dgvResultA6A7";
            this.dgvResultA6A7.ReadOnly = true;
            this.dgvResultA6A7.RowHeadersWidth = 51;
            this.dgvResultA6A7.RowTemplate.Height = 24;
            this.dgvResultA6A7.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResultA6A7.Size = new System.Drawing.Size(925, 246);
            this.dgvResultA6A7.TabIndex = 6;
            // 
            // lblPageInfoA7
            // 
            this.lblPageInfoA7.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblPageInfoA7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblPageInfoA7.Location = new System.Drawing.Point(0, 306);
            this.lblPageInfoA7.Name = "lblPageInfoA7";
            this.lblPageInfoA7.Size = new System.Drawing.Size(600, 35);
            this.lblPageInfoA7.TabIndex = 7;
            this.lblPageInfoA7.Text = "Page: 1 / 1";
            this.lblPageInfoA7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnPrevPageA7
            // 
            this.btnPrevPageA7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevPageA7.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPrevPageA7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnPrevPageA7.Location = new System.Drawing.Point(825, 306);
            this.btnPrevPageA7.Name = "btnPrevPageA7";
            this.btnPrevPageA7.Size = new System.Drawing.Size(50, 35);
            this.btnPrevPageA7.Symbol = 360947;
            this.btnPrevPageA7.SymbolSize = 14;
            this.btnPrevPageA7.TabIndex = 8;
            this.btnPrevPageA7.Text = "";
            this.btnPrevPageA7.Click += new System.EventHandler(this.btnPrevPageA7_Click);
            // 
            // btnNextPageA7
            // 
            this.btnNextPageA7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPageA7.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNextPageA7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnNextPageA7.Location = new System.Drawing.Point(875, 306);
            this.btnNextPageA7.Name = "btnNextPageA7";
            this.btnNextPageA7.Size = new System.Drawing.Size(50, 35);
            this.btnNextPageA7.Symbol = 360868;
            this.btnNextPageA7.SymbolSize = 14;
            this.btnNextPageA7.TabIndex = 9;
            this.btnNextPageA7.Text = "";
            this.btnNextPageA7.Click += new System.EventHandler(this.btnNextPageA7_Click);
            // 
            // FDashboard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(961, 581);
            this.Controls.Add(this.panelDataPool);
            this.Name = "FDashboard";
            this.Text = "Quản Lý Data Pool";
            this.panelDataPool.ResumeLayout(false);
            this.tableLayoutDataPool.ResumeLayout(false);
            this.tabControlTest.ResumeLayout(false);
            this.tabPageA1A5.ResumeLayout(false);
            this.tabPageA6A7.ResumeLayout(false);
            this.tabPageA6A7.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}