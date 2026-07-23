namespace MHG_Cartoning.Views
{
    partial class Page_Settings
    {
        private System.ComponentModel.IContainer components = null;
        private Sunny.UI.UIGroupBox ugbSettings;
        private Sunny.UI.UITableLayoutPanel tblMain;
        private Sunny.UI.UILabel lblPLC_IP;
        private Sunny.UI.UITextBox txtPLC_IP;
        private Sunny.UI.UILabel lblPLC_Port;
        private Sunny.UI.UINumPadTextBox numPLC_Port;
        private Sunny.UI.UILabel lblCamera_IP;
        private Sunny.UI.UITextBox txtCamera_IP;
        private Sunny.UI.UILabel lblCamera_Port;
        private Sunny.UI.UINumPadTextBox numCamera_Port;
        private Sunny.UI.UILabel lblOPC_CA_TCP;
        private Sunny.UI.UITextBox txtOPC_CA_TCP;
        private Sunny.UI.UILabel lblOPC_POItem_Node;
        private Sunny.UI.UITextBox txtOPC_POItem_Node;
        private Sunny.UI.UILabel lblOPC_POLot_Node;
        private Sunny.UI.UITextBox txtOPC_POLot_Node;
        private Sunny.UI.UISymbolButton btnSave;
        private Sunny.UI.UISymbolButton btnDefault;
        private Sunny.UI.UIListBox uiListBox1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.ugbSettings = new Sunny.UI.UIGroupBox();
            this.tblMain = new Sunny.UI.UITableLayoutPanel();
            this.lblPLC_IP = new Sunny.UI.UILabel();
            this.txtPLC_IP = new Sunny.UI.UITextBox();
            this.lblPLC_Port = new Sunny.UI.UILabel();
            this.numPLC_Port = new Sunny.UI.UINumPadTextBox();
            this.lblCamera_IP = new Sunny.UI.UILabel();
            this.txtCamera_IP = new Sunny.UI.UITextBox();
            this.lblCamera_Port = new Sunny.UI.UILabel();
            this.numCamera_Port = new Sunny.UI.UINumPadTextBox();
            this.lblOPC_CA_TCP = new Sunny.UI.UILabel();
            this.txtOPC_CA_TCP = new Sunny.UI.UITextBox();
            this.lblOPC_POItem_Node = new Sunny.UI.UILabel();
            this.txtOPC_POItem_Node = new Sunny.UI.UITextBox();
            this.lblOPC_POLot_Node = new Sunny.UI.UILabel();
            this.txtOPC_POLot_Node = new Sunny.UI.UITextBox();
            this.btnSave = new Sunny.UI.UISymbolButton();
            this.btnDefault = new Sunny.UI.UISymbolButton();
            this.uiListBox1 = new Sunny.UI.UIListBox();
            this.ugbSettings.SuspendLayout();
            this.tblMain.SuspendLayout();
            this.SuspendLayout();

            // ugbSettings
            this.ugbSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.ugbSettings.FillColor = System.Drawing.Color.White;
            this.ugbSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.ugbSettings.Location = new System.Drawing.Point(0, 0);
            this.ugbSettings.Name = "ugbSettings";
            this.ugbSettings.Padding = new System.Windows.Forms.Padding(10);
            this.ugbSettings.Radius = 8;
            this.ugbSettings.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.ugbSettings.Size = new System.Drawing.Size(800, 350);
            this.ugbSettings.TabIndex = 0;
            this.ugbSettings.Text = "Cấu hình ứng dụng";
            this.ugbSettings.Controls.Add(this.tblMain);

            // tblMain
            this.tblMain.AutoSize = true;
            this.tblMain.ColumnCount = 2;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Name = "tblMain";
            this.tblMain.Padding = new System.Windows.Forms.Padding(10);
            this.tblMain.RowCount = 8;
            for (int i = 0; i < 8; i++)
            {
                this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38));
            }
            this.tblMain.TabIndex = 0;

            // Row 0: PLC_IP
            this.lblPLC_IP.AutoSize = false;
            this.lblPLC_IP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPLC_IP.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPLC_IP.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblPLC_IP.Location = new System.Drawing.Point(10, 10);
            this.lblPLC_IP.Name = "lblPLC_IP";
            this.lblPLC_IP.Size = new System.Drawing.Size(200, 38);
            this.lblPLC_IP.TabIndex = 0;
            this.lblPLC_IP.Text = "PLC IP Address";
            this.lblPLC_IP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPLC_IP.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblPLC_IP, 0, 0);

            this.txtPLC_IP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPLC_IP.FillColor = System.Drawing.Color.White;
            this.txtPLC_IP.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPLC_IP.Location = new System.Drawing.Point(210, 10);
            this.txtPLC_IP.Name = "txtPLC_IP";
            this.txtPLC_IP.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.txtPLC_IP.Size = new System.Drawing.Size(570, 30);
            this.txtPLC_IP.TabIndex = 1;
            this.tblMain.Controls.Add(this.txtPLC_IP, 1, 0);

            // Row 1: PLC_Port
            this.lblPLC_Port.AutoSize = false;
            this.lblPLC_Port.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPLC_Port.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPLC_Port.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblPLC_Port.Location = new System.Drawing.Point(10, 48);
            this.lblPLC_Port.Name = "lblPLC_Port";
            this.lblPLC_Port.Size = new System.Drawing.Size(200, 38);
            this.lblPLC_Port.TabIndex = 2;
            this.lblPLC_Port.Text = "PLC Port";
            this.lblPLC_Port.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPLC_Port.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblPLC_Port, 0, 1);

            this.numPLC_Port.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numPLC_Port.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numPLC_Port.FillColor = System.Drawing.Color.White;
            this.numPLC_Port.Location = new System.Drawing.Point(210, 48);
            this.numPLC_Port.Maximum = 65535;
            this.numPLC_Port.Minimum = 0;
            this.numPLC_Port.Name = "numPLC_Port";
            this.numPLC_Port.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.numPLC_Port.Size = new System.Drawing.Size(200, 30);
            this.numPLC_Port.TabIndex = 3;
            this.tblMain.Controls.Add(this.numPLC_Port, 1, 1);

            // Row 2: Camera_IP
            this.lblCamera_IP.AutoSize = false;
            this.lblCamera_IP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCamera_IP.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblCamera_IP.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblCamera_IP.Location = new System.Drawing.Point(10, 86);
            this.lblCamera_IP.Name = "lblCamera_IP";
            this.lblCamera_IP.Size = new System.Drawing.Size(200, 38);
            this.lblCamera_IP.TabIndex = 4;
            this.lblCamera_IP.Text = "Camera IP Address";
            this.lblCamera_IP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCamera_IP.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblCamera_IP, 0, 2);

            this.txtCamera_IP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCamera_IP.FillColor = System.Drawing.Color.White;
            this.txtCamera_IP.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtCamera_IP.Location = new System.Drawing.Point(210, 86);
            this.txtCamera_IP.Name = "txtCamera_IP";
            this.txtCamera_IP.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.txtCamera_IP.Size = new System.Drawing.Size(570, 30);
            this.txtCamera_IP.TabIndex = 5;
            this.tblMain.Controls.Add(this.txtCamera_IP, 1, 2);

            // Row 3: Camera_Port
            this.lblCamera_Port.AutoSize = false;
            this.lblCamera_Port.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCamera_Port.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblCamera_Port.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblCamera_Port.Location = new System.Drawing.Point(10, 124);
            this.lblCamera_Port.Name = "lblCamera_Port";
            this.lblCamera_Port.Size = new System.Drawing.Size(200, 38);
            this.lblCamera_Port.TabIndex = 6;
            this.lblCamera_Port.Text = "Camera Port";
            this.lblCamera_Port.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCamera_Port.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblCamera_Port, 0, 3);

            this.numCamera_Port.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numCamera_Port.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numCamera_Port.FillColor = System.Drawing.Color.White;
            this.numCamera_Port.Location = new System.Drawing.Point(210, 124);
            this.numCamera_Port.Maximum = 65535;
            this.numCamera_Port.Minimum = 0;
            this.numCamera_Port.Name = "numCamera_Port";
            this.numCamera_Port.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.numCamera_Port.Size = new System.Drawing.Size(200, 30);
            this.numCamera_Port.TabIndex = 7;
            this.tblMain.Controls.Add(this.numCamera_Port, 1, 3);

            // Row 4: OPC_CA_TCP
            this.lblOPC_CA_TCP.AutoSize = false;
            this.lblOPC_CA_TCP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOPC_CA_TCP.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOPC_CA_TCP.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblOPC_CA_TCP.Location = new System.Drawing.Point(10, 162);
            this.lblOPC_CA_TCP.Name = "lblOPC_CA_TCP";
            this.lblOPC_CA_TCP.Size = new System.Drawing.Size(200, 38);
            this.lblOPC_CA_TCP.TabIndex = 8;
            this.lblOPC_CA_TCP.Text = "OPC UA Server URL";
            this.lblOPC_CA_TCP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOPC_CA_TCP.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblOPC_CA_TCP, 0, 4);

            this.txtOPC_CA_TCP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOPC_CA_TCP.FillColor = System.Drawing.Color.White;
            this.txtOPC_CA_TCP.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtOPC_CA_TCP.Location = new System.Drawing.Point(210, 162);
            this.txtOPC_CA_TCP.Name = "txtOPC_CA_TCP";
            this.txtOPC_CA_TCP.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.txtOPC_CA_TCP.Size = new System.Drawing.Size(570, 30);
            this.txtOPC_CA_TCP.TabIndex = 9;
            this.tblMain.Controls.Add(this.txtOPC_CA_TCP, 1, 4);

            // Row 5: OPC_POItem_Node
            this.lblOPC_POItem_Node.AutoSize = false;
            this.lblOPC_POItem_Node.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOPC_POItem_Node.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOPC_POItem_Node.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblOPC_POItem_Node.Location = new System.Drawing.Point(10, 200);
            this.lblOPC_POItem_Node.Name = "lblOPC_POItem_Node";
            this.lblOPC_POItem_Node.Size = new System.Drawing.Size(200, 38);
            this.lblOPC_POItem_Node.TabIndex = 10;
            this.lblOPC_POItem_Node.Text = "OPC PO Item Node ID";
            this.lblOPC_POItem_Node.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOPC_POItem_Node.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblOPC_POItem_Node, 0, 5);

            this.txtOPC_POItem_Node.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOPC_POItem_Node.FillColor = System.Drawing.Color.White;
            this.txtOPC_POItem_Node.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtOPC_POItem_Node.Location = new System.Drawing.Point(210, 200);
            this.txtOPC_POItem_Node.Name = "txtOPC_POItem_Node";
            this.txtOPC_POItem_Node.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.txtOPC_POItem_Node.Size = new System.Drawing.Size(570, 30);
            this.txtOPC_POItem_Node.TabIndex = 11;
            this.tblMain.Controls.Add(this.txtOPC_POItem_Node, 1, 5);

            // Row 6: OPC_POLot_Node
            this.lblOPC_POLot_Node.AutoSize = false;
            this.lblOPC_POLot_Node.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOPC_POLot_Node.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOPC_POLot_Node.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblOPC_POLot_Node.Location = new System.Drawing.Point(10, 238);
            this.lblOPC_POLot_Node.Name = "lblOPC_POLot_Node";
            this.lblOPC_POLot_Node.Size = new System.Drawing.Size(200, 38);
            this.lblOPC_POLot_Node.TabIndex = 12;
            this.lblOPC_POLot_Node.Text = "OPC PO Lot Node ID";
            this.lblOPC_POLot_Node.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblOPC_POLot_Node.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.tblMain.Controls.Add(this.lblOPC_POLot_Node, 0, 6);

            this.txtOPC_POLot_Node.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOPC_POLot_Node.FillColor = System.Drawing.Color.White;
            this.txtOPC_POLot_Node.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtOPC_POLot_Node.Location = new System.Drawing.Point(210, 238);
            this.txtOPC_POLot_Node.Name = "txtOPC_POLot_Node";
            this.txtOPC_POLot_Node.RectColor = System.Drawing.Color.FromArgb(189, 195, 199);
            this.txtOPC_POLot_Node.Size = new System.Drawing.Size(570, 30);
            this.txtOPC_POLot_Node.TabIndex = 13;
            this.tblMain.Controls.Add(this.txtOPC_POLot_Node, 1, 6);

            // Row 7: Buttons - span both columns
            this.tblMain.SetColumnSpan(this.btnSave, 2);
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.btnSave.FillColor = System.Drawing.Color.FromArgb(46, 204, 113);
            this.btnSave.FillHoverColor = System.Drawing.Color.FromArgb(57, 219, 127);
            this.btnSave.FillPressColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(10, 276);
            this.btnSave.Name = "btnSave";
            this.btnSave.RectColor = System.Drawing.Color.Transparent;
            this.btnSave.Size = new System.Drawing.Size(100, 35);
            this.btnSave.Symbol = 61442;
            this.btnSave.SymbolSize = 16;
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "Lưu";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.tblMain.Controls.Add(this.btnSave, 0, 7);

            this.btnDefault.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.btnDefault.FillColor = System.Drawing.Color.FromArgb(231, 76, 60);
            this.btnDefault.FillHoverColor = System.Drawing.Color.FromArgb(237, 99, 82);
            this.btnDefault.FillPressColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this.btnDefault.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnDefault.ForeColor = System.Drawing.Color.White;
            this.btnDefault.Location = new System.Drawing.Point(120, 276);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.RectColor = System.Drawing.Color.Transparent;
            this.btnDefault.Size = new System.Drawing.Size(130, 35);
            this.btnDefault.Symbol = 61707;
            this.btnDefault.SymbolSize = 16;
            this.btnDefault.TabIndex = 15;
            this.btnDefault.Text = "Mặc định";
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            this.tblMain.Controls.Add(this.btnDefault, 0, 7);

            // uiListBox1
            this.uiListBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.uiListBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.uiListBox1.ItemHeight = 25;
            this.uiListBox1.Location = new System.Drawing.Point(0, 330);
            this.uiListBox1.Name = "uiListBox1";
            this.uiListBox1.Size = new System.Drawing.Size(800, 120);
            this.uiListBox1.TabIndex = 1;

            // Page_Settings
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uiListBox1);
            this.Controls.Add(this.ugbSettings);
            this.Name = "Page_Settings";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(800, 450);
            this.ugbSettings.ResumeLayout(false);
            this.ugbSettings.PerformLayout();
            this.tblMain.ResumeLayout(false);
            this.tblMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
