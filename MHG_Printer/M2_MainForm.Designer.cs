namespace MHG_Printer
{
    partial class M2_MainForm
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
            mainPanel = new Sunny.UI.UIPanel();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            lblEndpoint = new Sunny.UI.UILabel();
            txtEndpoint = new Sunny.UI.UITextBox();
            lblNodeId = new Sunny.UI.UILabel();
            txtNodeId = new Sunny.UI.UITextBox();
            lblWriteValue = new Sunny.UI.UILabel();
            txtWriteValue = new Sunny.UI.UITextBox();
            lblWriteType = new Sunny.UI.UILabel();
            cboWriteType = new Sunny.UI.UIComboBox();
            lblReadValue = new Sunny.UI.UILabel();
            txtReadValue = new Sunny.UI.UITextBox();
            buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            btnConnect = new Sunny.UI.UIButton();
            btnDisconnect = new Sunny.UI.UIButton();
            btnRead = new Sunny.UI.UIButton();
            btnWrite = new Sunny.UI.UIButton();
            lblStatusTitle = new Sunny.UI.UILabel();
            lblStatus = new Sunny.UI.UILabel();
            lblLog = new Sunny.UI.UILabel();
            txtLog = new System.Windows.Forms.TextBox();
            mainPanel.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.Controls.Add(tableLayoutPanel);
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.FillColor = System.Drawing.Color.White;
            mainPanel.Font = new System.Drawing.Font("Microsoft YaHei", 12F);
            mainPanel.Location = new System.Drawing.Point(0, 35);
            mainPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            mainPanel.MinimumSize = new System.Drawing.Size(1, 1);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new System.Windows.Forms.Padding(18, 45, 18, 18);
            mainPanel.RectColor = System.Drawing.Color.FromArgb(80, 160, 255);
            mainPanel.Size = new System.Drawing.Size(980, 585);
            mainPanel.TabIndex = 0;
            mainPanel.Text = "OPC UA CLIENT TEST";
            mainPanel.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.BackColor = System.Drawing.Color.White;
            tableLayoutPanel.ColumnCount = 4;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tableLayoutPanel.Controls.Add(lblEndpoint, 0, 0);
            tableLayoutPanel.Controls.Add(txtEndpoint, 1, 0);
            tableLayoutPanel.Controls.Add(lblNodeId, 0, 1);
            tableLayoutPanel.Controls.Add(txtNodeId, 1, 1);
            tableLayoutPanel.Controls.Add(lblWriteValue, 0, 2);
            tableLayoutPanel.Controls.Add(txtWriteValue, 1, 2);
            tableLayoutPanel.Controls.Add(lblWriteType, 2, 2);
            tableLayoutPanel.Controls.Add(cboWriteType, 3, 2);
            tableLayoutPanel.Controls.Add(lblReadValue, 0, 3);
            tableLayoutPanel.Controls.Add(txtReadValue, 1, 3);
            tableLayoutPanel.Controls.Add(buttonPanel, 1, 4);
            tableLayoutPanel.Controls.Add(lblStatusTitle, 0, 5);
            tableLayoutPanel.Controls.Add(lblStatus, 1, 5);
            tableLayoutPanel.Controls.Add(lblLog, 0, 6);
            tableLayoutPanel.Controls.Add(txtLog, 1, 6);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(18, 45);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 8;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel.Size = new System.Drawing.Size(944, 522);
            tableLayoutPanel.TabIndex = 0;
            tableLayoutPanel.SetColumnSpan(txtEndpoint, 3);
            tableLayoutPanel.SetColumnSpan(txtNodeId, 3);
            tableLayoutPanel.SetColumnSpan(txtReadValue, 3);
            tableLayoutPanel.SetColumnSpan(buttonPanel, 3);
            tableLayoutPanel.SetColumnSpan(lblStatus, 3);
            tableLayoutPanel.SetColumnSpan(txtLog, 3);
            // 
            // labels
            // 
            ConfigureLabel(lblEndpoint, "Endpoint");
            ConfigureLabel(lblNodeId, "NodeId");
            ConfigureLabel(lblWriteValue, "Write Value");
            ConfigureLabel(lblWriteType, "Type");
            ConfigureLabel(lblReadValue, "Read Value");
            ConfigureLabel(lblStatusTitle, "Status");
            ConfigureLabel(lblLog, "Log");
            // 
            // text boxes
            // 
            ConfigureTextBox(txtEndpoint, "opc.tcp://127.0.0.1:4840");
            ConfigureTextBox(txtNodeId, "ns=2;s=Tag1");
            ConfigureTextBox(txtWriteValue, "123");
            ConfigureTextBox(txtReadValue, string.Empty);
            txtReadValue.ReadOnly = true;
            // 
            // cboWriteType
            // 
            cboWriteType.DataSource = null;
            cboWriteType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboWriteType.FillColor = System.Drawing.Color.White;
            cboWriteType.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboWriteType.ItemHoverColor = System.Drawing.Color.FromArgb(155, 200, 255);
            cboWriteType.Items.AddRange(new object[] { "String", "Int32", "Double", "Boolean" });
            cboWriteType.ItemSelectForeColor = System.Drawing.Color.FromArgb(235, 243, 255);
            cboWriteType.Location = new System.Drawing.Point(573, 87);
            cboWriteType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cboWriteType.MinimumSize = new System.Drawing.Size(63, 0);
            cboWriteType.Name = "cboWriteType";
            cboWriteType.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            cboWriteType.Size = new System.Drawing.Size(367, 32);
            cboWriteType.SymbolSize = 24;
            cboWriteType.TabIndex = 4;
            cboWriteType.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            cboWriteType.Watermark = "";
            cboWriteType.SelectedIndex = 1;
            // 
            // buttonPanel
            // 
            buttonPanel.BackColor = System.Drawing.Color.White;
            buttonPanel.Controls.Add(btnConnect);
            buttonPanel.Controls.Add(btnDisconnect);
            buttonPanel.Controls.Add(btnRead);
            buttonPanel.Controls.Add(btnWrite);
            buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            buttonPanel.Location = new System.Drawing.Point(123, 171);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new System.Drawing.Size(818, 36);
            buttonPanel.TabIndex = 6;
            // 
            // buttons
            // 
            ConfigureButton(btnConnect, "Connect");
            ConfigureButton(btnDisconnect, "Disconnect");
            ConfigureButton(btnRead, "Read");
            ConfigureButton(btnWrite, "Write");
            btnDisconnect.Enabled = false;
            btnRead.Enabled = false;
            btnWrite.Enabled = false;
            btnConnect.Click += btnConnect_Click;
            btnDisconnect.Click += btnDisconnect_Click;
            btnRead.Click += btnRead_Click;
            btnWrite.Click += btnWrite_Click;
            // 
            // lblStatus
            // 
            lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblStatus.ForeColor = System.Drawing.Color.Firebrick;
            lblStatus.Location = new System.Drawing.Point(123, 210);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(818, 42);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Disconnected";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLog
            // 
            txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            txtLog.Font = new System.Drawing.Font("Consolas", 10F);
            txtLog.Location = new System.Drawing.Point(123, 255);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLog.Size = new System.Drawing.Size(818, 222);
            txtLog.TabIndex = 12;
            // 
            // M2_MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(980, 620);
            Controls.Add(mainPanel);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            Name = "M2_MainForm";
            Text = "M2 - OPC UA Client Test";
            FormClosing += M2_MainForm_FormClosing;
            mainPanel.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void ConfigureLabel(Sunny.UI.UILabel label, string text)
        {
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        private static void ConfigureTextBox(Sunny.UI.UITextBox textBox, string text)
        {
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            textBox.Location = new System.Drawing.Point(124, 5);
            textBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            textBox.MinimumSize = new System.Drawing.Size(1, 16);
            textBox.Padding = new System.Windows.Forms.Padding(5);
            textBox.ShowText = false;
            textBox.Text = text;
            textBox.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            textBox.Watermark = "";
        }

        private static void ConfigureButton(Sunny.UI.UIButton button, string text)
        {
            button.Font = new System.Drawing.Font("Segoe UI", 10F);
            button.Location = new System.Drawing.Point(3, 3);
            button.MinimumSize = new System.Drawing.Size(1, 1);
            button.Size = new System.Drawing.Size(100, 30);
            button.TabIndex = 0;
            button.Text = text;
            button.TipsFont = new System.Drawing.Font("Microsoft YaHei", 9F);
        }

        #endregion

        private Sunny.UI.UIPanel mainPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private Sunny.UI.UILabel lblEndpoint;
        private Sunny.UI.UITextBox txtEndpoint;
        private Sunny.UI.UILabel lblNodeId;
        private Sunny.UI.UITextBox txtNodeId;
        private Sunny.UI.UILabel lblWriteValue;
        private Sunny.UI.UITextBox txtWriteValue;
        private Sunny.UI.UILabel lblWriteType;
        private Sunny.UI.UIComboBox cboWriteType;
        private Sunny.UI.UILabel lblReadValue;
        private Sunny.UI.UITextBox txtReadValue;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private Sunny.UI.UIButton btnConnect;
        private Sunny.UI.UIButton btnDisconnect;
        private Sunny.UI.UIButton btnRead;
        private Sunny.UI.UIButton btnWrite;
        private Sunny.UI.UILabel lblStatusTitle;
        private Sunny.UI.UILabel lblStatus;
        private Sunny.UI.UILabel lblLog;
        private System.Windows.Forms.TextBox txtLog;
    }
}


