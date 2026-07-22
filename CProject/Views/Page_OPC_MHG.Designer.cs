namespace CProject.Views
{
    partial class Page_OPC_MHG
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Sunny.UI.UITitlePanel panelRoot;
        private Sunny.UI.UITableLayoutPanel tableRoot;

        private Sunny.UI.UITitlePanel panelConnection;
        private Sunny.UI.UITableLayoutPanel tableConnection;
        private Sunny.UI.UILabel lblEndpoint;
        private Sunny.UI.UITextBox txtEndpoint;
        private Sunny.UI.UILabel lblTimeout;
        private Sunny.UI.UITextBox txtTimeout;
        private Sunny.UI.UILabel lblStatus;
        private Sunny.UI.UISymbolButton btnConnect;
        private Sunny.UI.UISymbolButton btnDisconnect;
        private Sunny.UI.UISymbolButton btnClearEndpoint;

        private Sunny.UI.UITabControl tabApiGroups;
        private System.Windows.Forms.TabPage tabRead;
        private System.Windows.Forms.TabPage tabWrite;

        private Sunny.UI.UITextBox txtReadNodeId;
        private Sunny.UI.UISymbolButton btnRead;
        private Sunny.UI.UITextBox txtReadValue;
        private Sunny.UI.UITextBox txtReadType;
        private Sunny.UI.UITextBox txtReadStatus;
        private Sunny.UI.UITextBox txtReadTimestamp;
        private Sunny.UI.UITextBox txtReadServerTimestamp;

        private Sunny.UI.UITextBox txtWriteNodeId;
        private Sunny.UI.UIComboBox cboWriteType;
        private Sunny.UI.UITextBox txtWriteValue;
        private Sunny.UI.UISymbolButton btnWrite;

        private Sunny.UI.UITitlePanel panelLog;
        private Sunny.UI.UIListBox lstLog;
        private Sunny.UI.UISymbolButton btnClearLog;
        private Sunny.UI.UISymbolButton btnCopyLog;

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
            this.components = new System.ComponentModel.Container();

            this.panelRoot = new Sunny.UI.UITitlePanel();
            this.tableRoot = new Sunny.UI.UITableLayoutPanel();

            this.panelConnection = new Sunny.UI.UITitlePanel();
            this.tableConnection = new Sunny.UI.UITableLayoutPanel();
            this.lblEndpoint = new Sunny.UI.UILabel();
            this.txtEndpoint = new Sunny.UI.UITextBox();
            this.lblTimeout = new Sunny.UI.UILabel();
            this.txtTimeout = new Sunny.UI.UITextBox();
            this.lblStatus = new Sunny.UI.UILabel();
            this.btnConnect = new Sunny.UI.UISymbolButton();
            this.btnDisconnect = new Sunny.UI.UISymbolButton();
            this.btnClearEndpoint = new Sunny.UI.UISymbolButton();

            this.tabApiGroups = new Sunny.UI.UITabControl();
            this.tabRead = new System.Windows.Forms.TabPage();
            this.tabWrite = new System.Windows.Forms.TabPage();

            this.txtReadNodeId = new Sunny.UI.UITextBox();
            this.btnRead = new Sunny.UI.UISymbolButton();
            this.txtReadValue = new Sunny.UI.UITextBox();
            this.txtReadType = new Sunny.UI.UITextBox();
            this.txtReadStatus = new Sunny.UI.UITextBox();
            this.txtReadTimestamp = new Sunny.UI.UITextBox();
            this.txtReadServerTimestamp = new Sunny.UI.UITextBox();

            this.txtWriteNodeId = new Sunny.UI.UITextBox();
            this.cboWriteType = new Sunny.UI.UIComboBox();
            this.txtWriteValue = new Sunny.UI.UITextBox();
            this.btnWrite = new Sunny.UI.UISymbolButton();

            this.panelLog = new Sunny.UI.UITitlePanel();
            this.lstLog = new Sunny.UI.UIListBox();
            this.btnClearLog = new Sunny.UI.UISymbolButton();
            this.btnCopyLog = new Sunny.UI.UISymbolButton();

            this.panelRoot.SuspendLayout();
            this.tableRoot.SuspendLayout();
            this.panelConnection.SuspendLayout();
            this.tableConnection.SuspendLayout();
            this.tabApiGroups.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.SuspendLayout();

            // panelRoot
            this.panelRoot.Controls.Add(this.tableRoot);
            this.panelRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRoot.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.panelRoot.Padding = new System.Windows.Forms.Padding(1, 32, 1, 1);
            this.panelRoot.Size = new System.Drawing.Size(1100, 760);
            this.panelRoot.Text = "OPC UA CLIENT TEST - MHG";
            this.panelRoot.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRoot.TitleHeight = 32;

            // tableRoot
            this.tableRoot.BackColor = System.Drawing.Color.White;
            this.tableRoot.ColumnCount = 1;
            this.tableRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRoot.Controls.Add(this.panelConnection, 0, 0);
            this.tableRoot.Controls.Add(this.tabApiGroups, 0, 1);
            this.tableRoot.Controls.Add(this.panelLog, 0, 2);
            this.tableRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRoot.Location = new System.Drawing.Point(1, 32);
            this.tableRoot.Name = "tableRoot";
            this.tableRoot.Padding = new System.Windows.Forms.Padding(8);
            this.tableRoot.RowCount = 3;
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableRoot.Size = new System.Drawing.Size(1098, 727);

            // panelConnection
            this.panelConnection.Controls.Add(this.tableConnection);
            this.panelConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConnection.Padding = new System.Windows.Forms.Padding(1, 28, 1, 1);
            this.panelConnection.Text = "Thiết lập kết nối OPC UA";
            this.panelConnection.TitleHeight = 28;

            // tableConnection
            this.tableConnection.BackColor = System.Drawing.Color.White;
            this.tableConnection.ColumnCount = 4;
            this.tableConnection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableConnection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableConnection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableConnection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            this.tableConnection.RowCount = 3;
            this.tableConnection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableConnection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableConnection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tableConnection.Dock = System.Windows.Forms.DockStyle.Fill;

            // lblEndpoint
            this.lblEndpoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEndpoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblEndpoint.Text = "Endpoint:";
            this.lblEndpoint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tableConnection.Controls.Add(this.lblEndpoint, 0, 0);

            // txtEndpoint
            this.txtEndpoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEndpoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtEndpoint.Text = "opc.tcp://DESKTOP-3LR82CB:53530/OPCUA/SimulationServer";
            this.tableConnection.Controls.Add(this.txtEndpoint, 1, 0);

            // btnClearEndpoint
            this.btnClearEndpoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearEndpoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClearEndpoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnClearEndpoint.Size = new System.Drawing.Size(90, 28);
            this.btnClearEndpoint.Text = "Mặc định";
            this.btnClearEndpoint.Symbol = 61453;
            this.btnClearEndpoint.Click += new System.EventHandler(this.btnClearEndpoint_Click);
            this.tableConnection.Controls.Add(this.btnClearEndpoint, 2, 0);

            // btnConnect
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnConnect.Text = "Kết nối";
            this.btnConnect.Symbol = 61475;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            this.tableConnection.Controls.Add(this.btnConnect, 3, 0);

            // lblTimeout
            this.lblTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblTimeout.Text = "Timeout (ms):";
            this.lblTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tableConnection.Controls.Add(this.lblTimeout, 0, 1);

            // txtTimeout
            this.txtTimeout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.txtTimeout.Text = "10000";
            this.tableConnection.SetColumnSpan(this.txtTimeout, 2);
            this.tableConnection.Controls.Add(this.txtTimeout, 1, 1);

            // btnDisconnect
            this.btnDisconnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDisconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDisconnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.btnDisconnect.Text = "Ngắt kết nối";
            this.btnDisconnect.Symbol = 61476;
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            this.tableConnection.Controls.Add(this.btnDisconnect, 3, 1);

            // lblStatus
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Text = "Trạng thái: Đã ngắt kết nối (Anonymous / Security None)";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tableConnection.SetColumnSpan(this.lblStatus, 4);
            this.tableConnection.Controls.Add(this.lblStatus, 0, 2);

            // tabApiGroups
            this.tabApiGroups.Controls.Add(this.tabRead);
            this.tabApiGroups.Controls.Add(this.tabWrite);
            this.tabApiGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabApiGroups.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabApiGroups.Name = "tabApiGroups";

            // tabRead
            this.tabRead.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabRead.Name = "tabRead";
            this.tabRead.Text = "Read";
            this.tabRead.Padding = new System.Windows.Forms.Padding(8);
            this.tabRead.Controls.Add(BuildReadLayout());

            // tabWrite
            this.tabWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabWrite.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabWrite.Name = "tabWrite";
            this.tabWrite.Text = "Write";
            this.tabWrite.Padding = new System.Windows.Forms.Padding(8);
            this.tabWrite.Controls.Add(BuildWriteLayout());

            // panelLog
            this.panelLog.Controls.Add(this.lstLog);
            this.panelLog.Controls.Add(this.btnClearLog);
            this.panelLog.Controls.Add(this.btnCopyLog);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Padding = new System.Windows.Forms.Padding(1, 28, 1, 1);
            this.panelLog.Text = "Nhật ký OPC UA";
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

            // Page_OPC_MHG
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1100, 760);
            this.Controls.Add(this.panelRoot);
            this.Name = "Page_OPC_MHG";
            this.Text = "OPC UA Client - MHG";

            this.panelRoot.ResumeLayout(false);
            this.tableRoot.ResumeLayout(false);
            this.panelConnection.ResumeLayout(false);
            this.tableConnection.ResumeLayout(false);
            this.tabApiGroups.ResumeLayout(false);
            this.panelLog.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Sunny.UI.UITableLayoutPanel BuildReadLayout()
        {
            var layout = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 5
            };
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            for (int i = 0; i < 5; i++)
            {
                layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            }

            var lblNodeId = new Sunny.UI.UILabel { Text = "NodeId:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblValue = new Sunny.UI.UILabel { Text = "Value:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblType = new Sunny.UI.UILabel { Text = "CLR Type:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblStatus = new Sunny.UI.UILabel { Text = "StatusCode:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblTs = new Sunny.UI.UILabel { Text = "SourceTimestamp:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblServerTs = new Sunny.UI.UILabel { Text = "ServerTimestamp:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

            txtReadNodeId.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReadNodeId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtReadNodeId.Text = "ns=2;s=Tag1";
            txtReadNodeId.Watermark = "vd: ns=2;s=Tag1";

            btnRead.Text = "Read";
            btnRead.Symbol = 61733;
            btnRead.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRead.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            btnRead.Dock = System.Windows.Forms.DockStyle.Fill;
            btnRead.Click += new System.EventHandler(this.btnRead_Click);

            txtReadValue.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReadValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtReadValue.ReadOnly = true;

            txtReadType.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReadType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtReadType.ReadOnly = true;

            txtReadStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReadStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtReadStatus.ReadOnly = true;

            txtReadTimestamp.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReadTimestamp.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtReadTimestamp.ReadOnly = true;

            txtReadServerTimestamp.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReadServerTimestamp.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtReadServerTimestamp.ReadOnly = true;

            layout.Controls.Add(lblNodeId, 0, 0);
            layout.Controls.Add(txtReadNodeId, 1, 0);
            layout.Controls.Add(btnRead, 2, 0);

            layout.Controls.Add(lblValue, 0, 1);
            layout.Controls.Add(txtReadValue, 1, 1);
            layout.SetColumnSpan(txtReadValue, 2);

            layout.Controls.Add(lblType, 0, 2);
            layout.Controls.Add(txtReadType, 1, 2);
            layout.SetColumnSpan(txtReadType, 2);

            layout.Controls.Add(lblStatus, 0, 3);
            layout.Controls.Add(txtReadStatus, 1, 3);
            layout.SetColumnSpan(txtReadStatus, 2);

            layout.Controls.Add(lblTs, 0, 4);
            layout.Controls.Add(txtReadTimestamp, 1, 4);
            layout.Controls.Add(lblServerTs, 0, 4);
            // place server timestamp label and field on a side column row 4
            layout.Controls.Add(txtReadServerTimestamp, 2, 4);

            // Re-organize row 4: split into 3 columns properly
            // Clear and rebuild row 4 to avoid duplicate placement
            layout.Controls.Remove(lblTs);
            layout.Controls.Remove(txtReadTimestamp);
            layout.Controls.Remove(lblServerTs);
            layout.Controls.Remove(txtReadServerTimestamp);

            var tsRow = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 4
            };
            tsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            tsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            tsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tsRow.RowCount = 1;
            tsRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tsRow.Controls.Add(lblTs, 0, 0);
            tsRow.Controls.Add(txtReadTimestamp, 1, 0);
            tsRow.Controls.Add(lblServerTs, 2, 0);
            tsRow.Controls.Add(txtReadServerTimestamp, 3, 0);

            layout.Controls.Add(tsRow, 1, 4);
            layout.SetColumnSpan(tsRow, 2);

            return layout;
        }

        private Sunny.UI.UITableLayoutPanel BuildWriteLayout()
        {
            var layout = new Sunny.UI.UITableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4
            };
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            for (int i = 0; i < 4; i++)
            {
                layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            }

            var lblNodeId = new Sunny.UI.UILabel { Text = "NodeId:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblType = new Sunny.UI.UILabel { Text = "Data Type:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            var lblValue = new Sunny.UI.UILabel { Text = "Value:", Dock = System.Windows.Forms.DockStyle.Fill, Font = new System.Drawing.Font("Microsoft Sans Serif", 10F), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

            txtWriteNodeId.Dock = System.Windows.Forms.DockStyle.Fill;
            txtWriteNodeId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtWriteNodeId.Text = "ns=2;s=Tag1";
            txtWriteNodeId.Watermark = "vd: ns=2;s=Tag1";

            cboWriteType.DropDownStyle = Sunny.UI.UIDropDownStyle.DropDownList;
            cboWriteType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboWriteType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            cboWriteType.Items.Clear();
            cboWriteType.Items.Add("String");
            cboWriteType.Items.Add("Boolean");
            cboWriteType.Items.Add("Int32");
            cboWriteType.Items.Add("Double");
            cboWriteType.SelectedIndex = 0;

            txtWriteValue.Dock = System.Windows.Forms.DockStyle.Fill;
            txtWriteValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            txtWriteValue.Text = string.Empty;
            txtWriteValue.Watermark = "giá trị cần ghi";

            btnWrite.Text = "Write";
            btnWrite.Symbol = 57345;
            btnWrite.Cursor = System.Windows.Forms.Cursors.Hand;
            btnWrite.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            btnWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            btnWrite.Click += new System.EventHandler(this.btnWrite_Click);

            layout.Controls.Add(lblNodeId, 0, 0);
            layout.Controls.Add(txtWriteNodeId, 1, 0);
            layout.SetColumnSpan(txtWriteNodeId, 2);

            layout.Controls.Add(lblType, 0, 1);
            layout.Controls.Add(cboWriteType, 1, 1);
            layout.SetColumnSpan(cboWriteType, 2);

            layout.Controls.Add(lblValue, 0, 2);
            layout.Controls.Add(txtWriteValue, 1, 2);
            layout.SetColumnSpan(txtWriteValue, 2);

            layout.Controls.Add(btnWrite, 0, 3);
            layout.SetColumnSpan(btnWrite, 3);

            return layout;
        }

        #endregion
    }
}
