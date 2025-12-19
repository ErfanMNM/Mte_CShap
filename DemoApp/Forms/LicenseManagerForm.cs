using System;
using System.IO;
using System.Windows.Forms;
using DemoApp.Models;
using DemoApp.Services;

namespace DemoApp.Forms
{
    public partial class LicenseManagerForm : Form
    {
        private readonly LicenseManager _licenseManager;
        private LicenseModel? _currentLicense;

        public LicenseManagerForm()
        {
            InitializeComponent();
            _licenseManager = new LicenseManager();
            LoadUI();
        }

        private void LoadUI()
        {
            this.Text = "License Manager - TApp";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tab Control
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new System.Drawing.Point(10, 10)
            };

            // Tab 1: Key Management
            var tabKeys = new TabPage("🔑 Quản lý Keys");
            tabKeys.Controls.Add(CreateKeyManagementPanel());
            tabControl.TabPages.Add(tabKeys);

            // Tab 2: Generate License
            var tabGenerate = new TabPage("📝 Tạo License");
            tabGenerate.Controls.Add(CreateGenerateLicensePanel());
            tabControl.TabPages.Add(tabGenerate);

            // Tab 3: Verify License
            var tabVerify = new TabPage("✅ Verify License");
            tabVerify.Controls.Add(CreateVerifyLicensePanel());
            tabControl.TabPages.Add(tabVerify);

            this.Controls.Add(tabControl);
        }

        private Panel CreateKeyManagementPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblTitle = new Label
            {
                Text = "Quản lý RSA Key Pair",
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };

            var lblStatus = new Label
            {
                Text = "Trạng thái: " + (_licenseManager.HasKeyPair() ? "✅ Đã có keys" : "❌ Chưa có keys"),
                AutoSize = true,
                Location = new System.Drawing.Point(10, 50)
            };

            var btnGenerateKeys = new Button
            {
                Text = "Tạo Key Pair Mới",
                Size = new System.Drawing.Size(200, 35),
                Location = new System.Drawing.Point(10, 80)
            };
            btnGenerateKeys.Click += (s, e) =>
            {
                try
                {
                    if (_licenseManager.HasKeyPair())
                    {
                        var result = MessageBox.Show(
                            "Đã có key pair. Tạo mới sẽ ghi đè keys cũ!\nBạn có chắc chắn?",
                            "Xác nhận",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        if (result != DialogResult.Yes) return;
                    }

                    _licenseManager.GenerateKeyPair();
                    lblStatus.Text = "Trạng thái: ✅ Đã có keys";
                    MessageBox.Show("Tạo key pair thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var btnExportPublicKey = new Button
            {
                Text = "Export Public Key",
                Size = new System.Drawing.Size(200, 35),
                Location = new System.Drawing.Point(10, 125)
            };
            btnExportPublicKey.Click += (s, e) =>
            {
                try
                {
                    if (!_licenseManager.HasKeyPair())
                    {
                        MessageBox.Show("Chưa có key pair!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var publicKey = _licenseManager.ExportPublicKey();
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                        FileName = "public_key.txt"
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveDialog.FileName, publicKey);
                        MessageBox.Show("Export public key thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            panel.Controls.AddRange(new Control[] { lblTitle, lblStatus, btnGenerateKeys, btnExportPublicKey });
            return panel;
        }

        private Panel CreateGenerateLicensePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            int y = 10;
            int labelWidth = 150;
            int textBoxWidth = 300;
            int spacing = 35;

            // Company Name
            var lblCompany = new Label { Text = "Tên công ty:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var txtCompany = new TextBox { Location = new System.Drawing.Point(170, y), Width = textBoxWidth };
            panel.Controls.AddRange(new Control[] { lblCompany, txtCompany });
            y += spacing;

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var txtEmail = new TextBox { Location = new System.Drawing.Point(170, y), Width = textBoxWidth };
            panel.Controls.AddRange(new Control[] { lblEmail, txtEmail });
            y += spacing;

            // Phone
            var lblPhone = new Label { Text = "Số điện thoại:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var txtPhone = new TextBox { Location = new System.Drawing.Point(170, y), Width = textBoxWidth };
            panel.Controls.AddRange(new Control[] { lblPhone, txtPhone });
            y += spacing;

            // Start Date
            var lblStartDate = new Label { Text = "Ngày bắt đầu:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var dtpStartDate = new DateTimePicker { Location = new System.Drawing.Point(170, y), Width = textBoxWidth, Value = DateTime.Now };
            panel.Controls.AddRange(new Control[] { lblStartDate, dtpStartDate });
            y += spacing;

            // Expiry Date
            var lblExpiryDate = new Label { Text = "Ngày hết hạn:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var dtpExpiryDate = new DateTimePicker { Location = new System.Drawing.Point(170, y), Width = textBoxWidth, Value = DateTime.Now.AddYears(1) };
            panel.Controls.AddRange(new Control[] { lblExpiryDate, dtpExpiryDate });
            y += spacing;

            // Max Machines
            var lblMaxMachines = new Label { Text = "Số máy tối đa:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var numMaxMachines = new NumericUpDown { Location = new System.Drawing.Point(170, y), Width = textBoxWidth, Minimum = 1, Maximum = 100, Value = 1 };
            panel.Controls.AddRange(new Control[] { lblMaxMachines, numMaxMachines });
            y += spacing;

            // Features
            var lblFeatures = new Label { Text = "Tính năng:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var txtFeatures = new TextBox { Location = new System.Drawing.Point(170, y), Width = textBoxWidth, Text = "Camera,Printer,Cloud,PLC" };
            var lblFeaturesHint = new Label
            {
                Text = "(Ví dụ: Camera,Printer,Cloud,PLC - phân cách bằng dấu phẩy)",
                Location = new System.Drawing.Point(170, y + 25),
                Width = textBoxWidth,
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Segoe UI", 8)
            };
            panel.Controls.AddRange(new Control[] { lblFeatures, txtFeatures, lblFeaturesHint });
            y += spacing + 20;

            // Notes
            var lblNotes = new Label { Text = "Ghi chú:", Location = new System.Drawing.Point(10, y), Width = labelWidth };
            var txtNotes = new TextBox { Location = new System.Drawing.Point(170, y), Width = textBoxWidth, Height = 60, Multiline = true };
            panel.Controls.AddRange(new Control[] { lblNotes, txtNotes });
            y += 80;

            // Generate Button
            var btnGenerate = new Button
            {
                Text = "🎫 Tạo License",
                Size = new System.Drawing.Size(200, 40),
                Location = new System.Drawing.Point(10, y),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += (s, e) =>
            {
                try
                {
                    if (!_licenseManager.HasKeyPair())
                    {
                        MessageBox.Show("Chưa có key pair! Vui lòng tạo key pair trước.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var license = new LicenseModel
                    {
                        CompanyName = txtCompany.Text,
                        Email = txtEmail.Text,
                        Phone = txtPhone.Text,
                        StartDate = dtpStartDate.Value,
                        ExpiryDate = dtpExpiryDate.Value,
                        MaxMachines = (int)numMaxMachines.Value,
                        Features = txtFeatures.Text,
                        Notes = txtNotes.Text
                    };

                    var licenseContent = _licenseManager.GenerateLicense(license);

                    // Save to file
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "License files (*.lic)|*.lic|All files (*.*)|*.*",
                        FileName = $"license_{license.LicenseKey}.lic"
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _licenseManager.SaveLicenseToFile(licenseContent, saveDialog.FileName);
                        MessageBox.Show($"Tạo license thành công!\n\nLicense Key: {license.LicenseKey}\nFile: {saveDialog.FileName}", 
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            panel.Controls.Add(btnGenerate);

            return panel;
        }

        private Panel CreateVerifyLicensePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblTitle = new Label
            {
                Text = "Verify License",
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };

            var btnBrowse = new Button
            {
                Text = "Chọn file license...",
                Size = new System.Drawing.Size(200, 35),
                Location = new System.Drawing.Point(10, 50)
            };

            var txtLicensePath = new TextBox
            {
                Location = new System.Drawing.Point(220, 50),
                Width = 400,
                ReadOnly = true
            };

            var txtResult = new TextBox
            {
                Location = new System.Drawing.Point(10, 100),
                Width = 750,
                Height = 300,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            btnBrowse.Click += (s, e) =>
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "License files (*.lic)|*.lic|All files (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    txtLicensePath.Text = openDialog.FileName;
                    try
                    {
                        var licenseContent = _licenseManager.LoadLicenseFromFile(openDialog.FileName);
                        var (isValid, license, errorMessage) = _licenseManager.VerifyLicense(licenseContent);

                        var result = new System.Text.StringBuilder();
                        result.AppendLine("=== KẾT QUẢ VERIFY LICENSE ===");
                        result.AppendLine();
                        result.AppendLine($"Trạng thái: {(isValid ? "✅ HỢP LỆ" : "❌ KHÔNG HỢP LỆ")}");
                        result.AppendLine($"Thông báo: {errorMessage}");
                        result.AppendLine();

                        if (license != null)
                        {
                            result.AppendLine("=== THÔNG TIN LICENSE ===");
                            result.AppendLine($"License Key: {license.LicenseKey}");
                            result.AppendLine($"Công ty: {license.CompanyName}");
                            result.AppendLine($"Email: {license.Email}");
                            result.AppendLine($"SĐT: {license.Phone}");
                            result.AppendLine($"Ngày bắt đầu: {license.StartDate:dd/MM/yyyy}");
                            result.AppendLine($"Ngày hết hạn: {license.ExpiryDate:dd/MM/yyyy}");
                            result.AppendLine($"Số ngày còn lại: {license.DaysRemaining()}");
                            result.AppendLine($"Số máy tối đa: {license.MaxMachines}");
                            result.AppendLine($"Tính năng: {license.Features}");
                            result.AppendLine($"Ghi chú: {license.Notes}");
                        }

                        txtResult.Text = result.ToString();
                        txtResult.ForeColor = isValid ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                    }
                    catch (Exception ex)
                    {
                        txtResult.Text = $"Lỗi: {ex.Message}";
                        txtResult.ForeColor = System.Drawing.Color.Red;
                    }
                }
            };

            panel.Controls.AddRange(new Control[] { lblTitle, btnBrowse, txtLicensePath, txtResult });
            return panel;
        }
    }
}

