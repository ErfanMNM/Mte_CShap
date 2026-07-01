
using Sunny.UI;
using System.Reflection;
using TApp.Configs;
using TApp.Infrastructure;
using TApp.Views.Dashboard;
using TTManager.Audit;
using TTManager.Auth;
using TTManager.Diaglogs;
using static TApp.Views.Dashboard.FDashboard;

namespace TApp.Views.Settings
{
    /// <summary>
    /// UI page for displaying and editing application settings.
    /// Builds dynamic controls from <see cref="AppConfigs"/> and binds values.
    /// </summary>
    public partial class PAppSetting : UIPage
    {
        #region Properties
        LogHelper<e_LogType> PSLogger { get; set; }
        #endregion

        #region Fields
        private Dictionary<string, Control> _configControls = new Dictionary<string, Control>();
        private Dictionary<string, PropertyInfo> _configProperties = new Dictionary<string, PropertyInfo>();
        private List<UIGroupBox> _groupBoxes = new List<UIGroupBox>();
        private UITableLayoutPanel _outerTable;
        private const int ColumnGap = 10;
        private const int RowGap = 10;
        private const int ItemRowHeight = 38;
        private const int LabelColumnWidth = 280;
        private const int ControlColumnMinWidth = 200;
        // Trạng thái load của uc_UserManager1 để nút Reload có thể toggle
        private bool _isUserManagerLoaded = true;
        #endregion

        #region Constructor
        public PAppSetting()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes dynamic controls and loads current configuration values.
        /// </summary>
        public void START()
        {
            string logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MTE", "Logs", "Pages", "PSlog.ptl"
            );
            PSLogger = new LogHelper<e_LogType>(logPath);
            GenerateConfigControls();
            LoadCurrentConfig();
        }
        #endregion

        #region UI Building
        private void GenerateConfigControls()
        {
            var configType = typeof(AppConfigs);
            var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite &&
                           !p.Name.Equals("Current", StringComparison.OrdinalIgnoreCase) &&
                           p.DeclaringType == configType)
                .ToList();

            // Clear existing dynamic controls
            tabPage1.Controls.Clear();
            _configControls.Clear();
            _configProperties.Clear();
            _groupBoxes.Clear();

            // Group properties by category
            var categories = GroupPropertiesByCategory(properties).ToList();

            // Outer 2-column TableLayoutPanel
            var outerTable = new UITableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Padding = new Padding(10, 10, 10, 10)
            };
            outerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            outerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            outerTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tabPage1.Controls.Add(outerTable);

            // Left column: StackPanel-like flow for odd categories
            var leftPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0, 0, ColumnGap, 0)
            };

            // Right column: StackPanel-like flow for even categories
            var rightPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0)
            };

            outerTable.Controls.Add(leftPanel, 0, 0);
            outerTable.Controls.Add(rightPanel, 1, 0);

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                var targetPanel = (i % 2 == 0) ? leftPanel : rightPanel;
                CreateGroupBoxForCategory(targetPanel, category);
            }
        }

        private void CreateGroupBoxForCategory(FlowLayoutPanel parent, KeyValuePair<string, List<PropertyInfo>> category)
        {
            int itemCount = category.Value.Count;
            int boxHeight = 40 + (itemCount * ItemRowHeight);

            var groupBox = new UIGroupBox()
            {
                Text = category.Key,
                Width = 380,
                Height = boxHeight,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                FillColor = Color.FromArgb(255, 255, 255),
                RectColor = Color.FromArgb(189, 195, 199),
                Radius = 8,
                RectSize = 1,
                AutoSize = false,
                Margin = new Padding(0, 0, 0, RowGap)
            };
            parent.Controls.Add(groupBox);
            _groupBoxes.Add(groupBox);

            var itemsTable = new UITableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = itemCount,
                AutoSize = false,
                Padding = new Padding(10, 10, 10, 10)
            };
            itemsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumnWidth));
            itemsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < itemCount; i++)
                itemsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, ItemRowHeight));

            groupBox.Controls.Add(itemsTable);

            int row = 0;
            foreach (var property in category.Value)
            {
                _configProperties[property.Name] = property;

                var label = new UILabel()
                {
                    Text = GetDisplayName(property.Name),
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 0, 0)
                };

                Control? control = CreateControlForProperty(property);
                if (control != null)
                {
                    // Wrap switch/numpad in a panel to center it while keeping Dock=Fill
                    if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(int))
                    {
                        var wrapper = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(0) };
                        control.Dock = DockStyle.None;
                        control.Size = GetControlSize(property.PropertyType);
                        control.Location = new Point(
                            wrapper.Width - control.Width,
                            (wrapper.Height - control.Height) / 2);
                        wrapper.Layout += (s, e) =>
                        {
                            if (control.Width > 0)
                                control.Location = new Point(
                                    wrapper.Width - control.Width,
                                    (wrapper.Height - control.Height) / 2);
                        };
                        wrapper.Controls.Add(control);
                        _configControls[property.Name] = wrapper;
                        itemsTable.Controls.Add(wrapper, 1, row);
                    }
                    else
                    {
                        control.Dock = DockStyle.Fill;
                        _configControls[property.Name] = control;
                        itemsTable.Controls.Add(control, 1, row);
                    }
                }

                itemsTable.Controls.Add(label, 0, row);
                row++;
            }
        }

        private void tabPage1_Resize(object sender, EventArgs e)
        {
            // Recalculate group box widths based on current column width
            if (tabPage1.Width <= 0) return;

            int availableWidth = (tabPage1.Width / 2) - ColumnGap - 20;
            foreach (var gb in _groupBoxes)
            {
                gb.Width = Math.Max(availableWidth, 300);
            }
        }

        private void tabPage2_Resize(object sender, EventArgs e)
        {
            uc_UserSetting1?.PerformLayout();
            uc_UserManager1?.PerformLayout();
        }

        private Dictionary<string, List<PropertyInfo>> GroupPropertiesByCategory(List<PropertyInfo> properties)
        {
            var categories = new Dictionary<string, List<PropertyInfo>>();

            foreach (var property in properties)
            {
                string category = GetPropertyCategory(property.Name);
                if (!categories.ContainsKey(category))
                {
                    categories[category] = new List<PropertyInfo>();
                }
                categories[category].Add(property);
            }

            return categories;
        }

        private string GetPropertyCategory(string propertyName)
        {
            if (propertyName.Contains("Secret"))
                return "🔐 Bảo mật";
            if (propertyName.Contains("App"))
                return "⚙️ Cấu hình ứng dụng";
            if (propertyName.Contains("Camera"))
                return "📹 Máy quét QR";
            if (propertyName.Contains("Hardware"))
                return "🔌 Phần cứng";
            if (propertyName.Contains("Cloud"))
                return "☁️ Đám Mây";
            if (propertyName.Contains("ERP"))
                return "🗄️ Cấu hình ERP";
            if (propertyName.Contains("PLC"))
                return "📦 Cấu hình PLC";
            if (propertyName.Contains("TCP"))
                return "🔧 Cấu hình truyền thông";
            if (propertyName.Contains("AWS"))
                return "☁️ Cấu hình AWS";
            return "⚙️ Cài đặt chung";
        }

        private string GetDisplayName(string propertyName)
        {
            // Convert property names to user-friendly display names
            var displayNames = new Dictionary<string, string>()
            {
                { "AppHideEnable", "Ẩn ứng dụng khi tắt" },
                { "AppStartWithWindows", "Khởi động cùng Windows" },
                { "AppTwoFA_Enabled", "Bật xác thực 2 bước" },
                { "TCP_Port", "Cổng TCP" },
                { "PLC_IP", "Địa chỉ IP PLC" },
                { "PLC_Port", "Cổng PLC" },
                { "PLC_Time_Refresh", "Thời gian làm mới PLC (ms)" },
                { "PLC_Test_Mode", "Chế độ thử nghiệm PLC" },
                { "Description", "Description" },
                { "AWS_Credential_Path", "Đường dẫn AWS Credential" },
                { "Camera_01_IP", "IP Máy Scan" },
                { "Camera_01_Port", "Cổng Máy Scan" },
                { "Data_Mode", "Chế độ dữ liệu" },
                { "Line_Name", "Tên dây chuyền" },
                { "Handheld_COM_Port", "Cổng COM Thiết bị cầm tay" },
                { "production_list_path", "Đường dẫn danh sách sản xuất" },
                { "credentialPLCAddressPath", "Đường dẫn thông tin đăng nhập PLC" },
                { "credentialERPPath", "Đường dẫn thông tin đăng nhập ERP" },
                { "ERP_Sub_Inv", "ERP Sub Inventory" },
                { "ERP_Org_Code", "ERP Organization Code" },
                { "ERP_DatasetID", "ERP Dataset ID" },
                { "ERP_TableID", "ERP Table ID" },
                { "ERP_ProjectID", "ERP Project ID" },
                { "Cloud_Connection_Enabled", "Bật kết nối đám mây" },
                { "Cloud_Refresh_Interval_Minute", "Khoảng thời gian làm mới đám mây (phút)" },
                { "Cloud_Upload_Enabled", "Bật tải lên đám mây" },
                { "Local_Backup_Enabled", "Bật sao lưu cục bộ" },
                { "Backup_Folder_Path", "Thư mục sao lưu (để trống dùng mặc định)" }
            };

            return displayNames.ContainsKey(propertyName) ? displayNames[propertyName] : propertyName;
        }

        private Size GetControlSize(Type propertyType)
        {
            if (propertyType == typeof(bool))
                return new Size(60, 30);
            else if (propertyType == typeof(int))
                return new Size(120, 30);
            else
                return new Size(200, 30);
        }

        private Control? CreateControlForProperty(PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(bool))
            {
                var uiSwitch = new UISwitch()
                {
                    Name = $"sw_{property.Name}",
                    ActiveText = "Bật",
                    InActiveText = "Tắt",
                    Size = new Size(60, 30)
                };
                return uiSwitch;
            }
            else if (propertyType == typeof(int))
            {
                var numPadTextBox = new UINumPadTextBox()
                {
                    Name = $"numpad_{property.Name}",
                    FillColor = Color.White,
                    RectColor = Color.FromArgb(189, 195, 199),
                    Radius = 8,
                    Font = new Font("Segoe UI", 10F),
                    Maximum = property.Name.ToLower().Contains("port") ? 65535 : int.MaxValue,
                    Minimum = 0,
                    Watermark = "2-click: numpad | Ctrl+2-click: keyboard"
                };

                // UINumPadTextBox tự động có numpad dialog khi double click
                // Thêm option cho bàn phím chữ bằng Ctrl+Double Click
                numPadTextBox.MouseDoubleClick += (s, e) =>
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        ShowVirtualKeyboard(numPadTextBox, property.Name);
                    }
                };

                return numPadTextBox;
            }
            else if (propertyType == typeof(string))
            {
                var textBox = new UITextBox()
                {
                    Name = $"txt_{property.Name}",
                    FillColor = Color.White,
                    RectColor = Color.FromArgb(189, 195, 199),
                    Radius = 8,
                    Font = new Font("Segoe UI", 10F)
                };

                // Thêm tooltip để hướng dẫn người dùng
                textBox.Watermark = "Double-click để mở bàn phím ảo";

                // Special handling for password fields
                if (property.Name.ToLower().Contains("password"))
                {
                    textBox.PasswordChar = '●';
                }

                // Special handling for path fields
                if (property.Name.ToLower().Contains("path"))
                {
                    textBox.ReadOnly = true;
                    textBox.BackColor = Color.FromArgb(248, 248, 248);

                    // Add browse button
                    var browseBtn = new UIButton()
                    {
                        Text = "📁",
                        Dock = DockStyle.Right,
                        Width = 36,
                        Font = new Font("Segoe UI", 10F),
                        Radius = 8,
                        FillColor = Color.FromArgb(108, 117, 125),
                        FillHoverColor = Color.FromArgb(134, 142, 150),
                        FillPressColor = Color.FromArgb(73, 80, 87),
                        RectSize = 0,
                        ForeColor = Color.White
                    };

                    string propName = property.Name;
                    browseBtn.Click += (s, e) => BrowseForFile(textBox, propName);

                    // Path textbox cũng có thể dùng bàn phím ảo cho việc edit
                    textBox.DoubleClick += (s, e) => ShowVirtualKeyboard(textBox, propName);

                    var container = new Panel()
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(0)
                    };
                    textBox.Dock = DockStyle.Fill;
                    container.Controls.Add(textBox);
                    container.Controls.Add(browseBtn);

                    return container;
                }

                // Thêm double-click event để hiện bàn phím ảo cho textbox thường
                textBox.DoubleClick += (s, e) => ShowVirtualKeyboard(textBox, property.Name);

                return textBox;
            }

            return null;
        }
        #endregion

        #region Data Binding
        private void LoadCurrentConfig()
        {
            var config = AppConfigs.Current;

            foreach (var kvp in _configControls)
            {
                var propertyName = kvp.Key;
                var control = kvp.Value;
                var property = _configProperties[propertyName];

                try
                {
                    var value = property.GetValue(config);
                    SetControlValue(control, value);
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    System.Diagnostics.Debug.WriteLine($"Error loading value for {propertyName}: {ex.Message}");
                }
            }
        }

        private void SetControlValue(Control control, object value)
        {
            if (control is UISwitch uiSwitch && value is bool boolValue)
            {
                uiSwitch.Active = boolValue;
            }
            else if (control is UINumPadTextBox numPadTextBox && value is int intValue)
            {
                numPadTextBox.Text = intValue.ToString();
            }
            else if (control is UITextBox numTextBox && value is int intValue2 && numTextBox.Name.StartsWith("txt_") && !numTextBox.Name.Contains("password") && !numTextBox.Name.Contains("path") && !numTextBox.Name.Contains("host") && !numTextBox.Name.Contains("client") && !numTextBox.Name.Contains("CA") && !numTextBox.Name.Contains("COM"))
            {
                numTextBox.Text = intValue2.ToString();
            }
            else if (control is UITextBox textBox && value is string stringValue)
            {
                textBox.Text = stringValue ?? string.Empty;
            }
            else if (control is Panel panel)
            {
                // Handle wrapper panels for bool/int controls
                var innerSwitch = panel.Controls.OfType<UISwitch>().FirstOrDefault();
                if (innerSwitch != null && value is bool boolVal)
                {
                    innerSwitch.Active = boolVal;
                    return;
                }
                var innerNumPad = panel.Controls.OfType<UINumPadTextBox>().FirstOrDefault();
                if (innerNumPad != null && value is int intVal)
                {
                    innerNumPad.Text = intVal.ToString();
                    return;
                }
                // Handle path controls with browse button
                var textBoxInPanel = panel.Controls.OfType<UITextBox>().FirstOrDefault();
                if (textBoxInPanel != null && value is string strVal)
                {
                    textBoxInPanel.Text = strVal ?? string.Empty;
                }
            }
        }

        private object GetControlValue(Control control, Type targetType)
        {
            if (control is UISwitch uiSwitch && targetType == typeof(bool))
            {
                return uiSwitch.Active;
            }
            else if (control is UINumPadTextBox numPadTextBox && targetType == typeof(int))
            {
                if (int.TryParse(numPadTextBox.Text, out int result))
                    return result;
                return 0;
            }
            else if (control is UITextBox numTextBox && targetType == typeof(int) && numTextBox.Name.StartsWith("txt_") && !numTextBox.Name.Contains("password") && !numTextBox.Name.Contains("path") && !numTextBox.Name.Contains("host") && !numTextBox.Name.Contains("client") && !numTextBox.Name.Contains("CA") && !numTextBox.Name.Contains("COM"))
            {
                if (int.TryParse(numTextBox.Text, out int result))
                    return result;
                return 0;
            }
            else if (control is UITextBox textBox && targetType == typeof(string))
            {
                return textBox.Text;
            }
            else if (control is Panel panel)
            {
                // Check for wrapper panels containing UISwitch or UINumPadTextBox
                var innerSwitch = panel.Controls.OfType<UISwitch>().FirstOrDefault();
                if (innerSwitch != null && targetType == typeof(bool))
                    return innerSwitch.Active;

                var innerNumPad = panel.Controls.OfType<UINumPadTextBox>().FirstOrDefault();
                if (innerNumPad != null && targetType == typeof(int))
                {
                    if (int.TryParse(innerNumPad.Text, out int result))
                        return result;
                    return 0;
                }

                // Path controls with browse button
                var textBoxInPanel = panel.Controls.OfType<UITextBox>().FirstOrDefault();
                if (textBoxInPanel != null && targetType == typeof(string))
                    return textBoxInPanel.Text ?? string.Empty;
            }

            return null;
        }
        #endregion

        #region File Dialog
        private void BrowseForFile(UITextBox textBox, string propertyName)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                if (propertyName.ToLower().Contains("pem") || propertyName.ToLower().Contains("rootca"))
                {
                    openFileDialog.Filter = "PEM files (*.pem)|*.pem|All files (*.*)|*.*";
                    openFileDialog.Title = "Chọn file Root CA";
                }
                else if (propertyName.ToLower().Contains("pfx"))
                {
                    openFileDialog.Filter = "PFX files (*.pfx)|*.pfx|All files (*.*)|*.*";
                    openFileDialog.Title = "Chọn file Client Certificate";
                }
                else
                {
                    openFileDialog.Filter = "All files (*.*)|*.*";
                    openFileDialog.Title = "Chọn file";
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox.Text = openFileDialog.FileName;
                }
            }
        }
        #endregion

        #region Virtual Keyboard
        private void ShowVirtualKeyboard(Control textControl, string propertyName)
        {
            try
            {
                var displayName = GetDisplayName(propertyName);
                var isPassword = propertyName.ToLower().Contains("password");

                string currentText = "";
                if (textControl is UITextBox textBox)
                {
                    currentText = textBox.Text;
                }
                else if (textControl is UINumPadTextBox numPadBox)
                {
                    currentText = numPadBox.Text;
                }

                var keyboard = new Entertext()
                {
                    TileText = $"Nhập giá trị cho {displayName}",
                    TextValue = currentText,
                    IsPassword = isPassword
                };

                if (keyboard.ShowDialog() == DialogResult.OK)
                {
                    if (textControl is UITextBox tb)
                    {
                        // Nếu là path field và readonly, cần bỏ readonly tạm thời để update
                        if (tb.ReadOnly)
                        {
                            tb.ReadOnly = false;
                            tb.Text = keyboard.TextValue;
                            tb.ReadOnly = true;
                        }
                        else
                        {
                            tb.Text = keyboard.TextValue;
                        }
                    }
                    else if (textControl is UINumPadTextBox npb)
                    {
                        npb.Text = keyboard.TextValue;
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorTip($"Lỗi hiện thị bàn phím: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers - Buttons
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var config = AppConfigs.Current;

                // Cập nhật config từ UI động
                foreach (var kvp in _configControls)
                {
                    var propertyName = kvp.Key;
                    var control = kvp.Value;
                    var property = _configProperties[propertyName];

                    try
                    {
                        var value = GetControlValue(control, property.PropertyType);
                        if (value != null)
                        {
                            property.SetValue(config, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ShowErrorTip($"Lỗi cập nhật {propertyName}: {ex.Message}");
                        PSLogger?.LogAsync(
                            GlobalVarialbles.CurrentUser.Username,
                            e_LogType.Error,
                            "Lỗi cập nhật cài đặt",
                            $"{{'Property':'{propertyName}','Error':'{ex.Message}'}}",
                            "ERR-APPSETTING-01"
                        );
                        return;
                    }
                }

                // Lưu config
                config.Save();

                // Ghi log lưu cài đặt thành công
                PSLogger?.LogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.UserAction,
                    "Lưu cài đặt ứng dụng thành công",
                    "",
                    "UA-APPSETTING-02"
                );

                this.ShowSuccessTip("Cài đặt đã được lưu thành công!");
            }
            catch (Exception ex)
            {
                this.ShowErrorTip($"Lỗi lưu cài đặt: {ex.Message}");
                PSLogger?.LogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Error,
                    "Lỗi lưu cài đặt ứng dụng",
                    ex.Message,
                    "ERR-APPSETTING-02"
                );
            }
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            var result = this.ShowAskDialog("Bạn có chắc muốn khôi phục cài đặt mặc định không?");
            if (result)
            {
                try
                {
                    AppConfigs.Current.SetDefault();
                    LoadCurrentConfig();

                    // Ghi log khôi phục mặc định
                    PSLogger?.LogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.UserAction,
                        "Khôi phục cài đặt mặc định",
                        "",
                        "UA-APPSETTING-03"
                    );

                    this.ShowSuccessTip("Đã khôi phục cài đặt mặc định!");
                }
                catch (Exception ex)
                {
                    this.ShowErrorTip($"Lỗi khôi phục cài đặt: {ex.Message}");
                    PSLogger?.LogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "Lỗi khôi phục cài đặt mặc định",
                        ex.Message,
                        "ERR-APPSETTING-03"
                    );
                }
            }
            else
            {
                // Ghi log người dùng hủy khôi phục
                PSLogger?.LogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.UserAction,
                    "Hủy khôi phục cài đặt mặc định",
                    "",
                    "UA-APPSETTING-04"
                );
            }
        }

        /// <summary>
        /// Nút Reload dùng để hủy / tạo lại uc_UserManager1.
        /// Lần nhấn thứ nhất: remove + dispose control.
        /// Lần nhấn tiếp theo: tạo lại control và add vào tabPage2.
        /// </summary>
        private void btnReload_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isUserManagerLoaded && uc_UserManager1 != null && !uc_UserManager1.IsDisposed)
                {
                    // Hủy UserManager hiện tại
                    uc_UserManager1.OnAction -= uc_UserManager1_OnAction;
                    if (tabPage2.Controls.Contains(uc_UserManager1))
                    {
                        tabPage2.Controls.Remove(uc_UserManager1);
                    }
                    uc_UserManager1.Dispose();
                    uc_UserManager1 = null;

                    _isUserManagerLoaded = false;
                    // Nếu có button reload trong designer, có thể đổi text cho dễ hiểu
                    if (sender is UISymbolButton btn)
                    {
                        btn.Text = "Tải lại UserManager";
                    }
                }
                else
                {
                    // Tạo lại UserManager mới và add vào tab Người dùng
                    uc_UserManager1 = new uc_UserManager
                    {
                        Name = "uc_UserManager1",
                        Dock = DockStyle.Right,
                        Font = new Font("Microsoft Sans Serif", 12F),
                        IS2FAEnabled = false,
                        MinimumSize = new Size(1, 1),
                        Text = "uc_UserManager1",
                        TextAlignment = ContentAlignment.MiddleCenter,
                        CurrentUserName = GlobalVarialbles.CurrentUser.Username
                    };

                    // Quyền: chỉ Admin mới được thao tác
                    uc_UserManager1.Enabled = GlobalVarialbles.CurrentUser.Role == "Admin";

                    uc_UserManager1.OnAction += uc_UserManager1_OnAction;
                    tabPage2.Controls.Add(uc_UserManager1);
                    uc_UserManager1.INIT();

                    _isUserManagerLoaded = true;
                    if (sender is UISymbolButton btn)
                    {
                        btn.Text = "Hủy UserManager";
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorTip($"Lỗi xử lý Reload UserManager: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers - User Controls
        private void uc_UserSetting1_OnUserAction(object sender, LoginActionEventArgs e)
        {
            uiListBox1.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {e.Message}");
        }

        private void uc_UserManager1_OnAction(object sender, LoginActionEventArgs e)
        {
            PSLogger.LogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Info, "Sự kiện : " + e.Message, "", "UM01");
            uiListBox1.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {e.Message}");
        }
        #endregion

        #region Event Handlers - Page Lifecycle
        private void PAppSetting_Initialize(object sender, EventArgs e)
        {
            // Ghi log mở trang cài đặt
            PSLogger?.LogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Mở trang cài đặt ứng dụng",
                "",
                "UA-APPSETTING-01"
            );

            this.uiListBox1.Items.Clear();
            uc_UserSetting1.CurrentUserName = GlobalVarialbles.CurrentUser.Username;
            uc_UserSetting1.INIT();

            if (GlobalVarialbles.CurrentUser.Role != "Admin")
            {
                btnDefault.Enabled = false;
                btnSave.Enabled = false;
                if (uc_UserManager1 != null && !uc_UserManager1.IsDisposed)
                    uc_UserManager1.Enabled = false;
            }
            else
            {
                btnDefault.Enabled = true;
                btnSave.Enabled = true;
                if (uc_UserManager1 != null && !uc_UserManager1.IsDisposed)
                    uc_UserManager1.Enabled = true;
            }

            if (uc_UserManager1 != null && !uc_UserManager1.IsDisposed)
            {
                uc_UserManager1.CurrentUserName = GlobalVarialbles.CurrentUser.Username;
                uc_UserManager1.INIT();
            }
        }
        #endregion
    }
}
