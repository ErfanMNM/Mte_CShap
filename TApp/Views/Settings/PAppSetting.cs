
using Sunny.UI;
using System.Reflection;
using TApp.Configs;
using TTManager.Diaglogs;

namespace TApp.Views.Settings
{
    /// <summary>
    /// UI page for displaying and editing application settings.
    /// Builds dynamic controls from <see cref="AppConfigs"/> and binds values.
    /// </summary>
    public partial class PAppSetting : UIPage
    {
        #region Fields
        private Dictionary<string, Control> _configControls = new Dictionary<string, Control>();
        private Dictionary<string, PropertyInfo> _configProperties = new Dictionary<string, PropertyInfo>();
        #endregion

        #region Constructor
        public PAppSetting()
        {
            InitializeComponent();
        }
        #endregion

        /// <summary>
        /// Initializes dynamic controls and loads current configuration values.
        /// </summary>
        public void START()
        {
            GenerateConfigControls();
            LoadCurrentConfig();
        }

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

            // Group properties by category
            var categories = GroupPropertiesByCategory(properties);

            int yPos = 20;
            int groupSpacing = 15;

            foreach (var category in categories)
            {
                // Create category group box
                var groupBox = new UIGroupBox()
                {
                    Text = category.Key,
                    Location = new Point(20, yPos),
                    Size = new Size(740, (category.Value.Count * 50) + 60),
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    FillColor = Color.FromArgb(255, 255, 255),
                    RectColor = Color.FromArgb(189, 195, 199),
                    Radius = 8,
                    RectSize = 1
                };
                tabPage1.Controls.Add(groupBox);

                int itemYPos = 35;

                foreach (var property in category.Value)
                {
                    _configProperties[property.Name] = property;

                    // Create modern card-like container
                    var itemPanel = new UIPanel()
                    {
                        Location = new Point(15, itemYPos),
                        Size = new Size(700, 40),
                        FillColor = Color.White,
                        RectColor = Color.FromArgb(224, 230, 237),
                        Radius = 12,
                        RectSize = 1
                    };
                    groupBox.Controls.Add(itemPanel);

                    // Create label with icon
                    var label = new UILabel()
                    {
                        Text = GetDisplayName(property.Name),
                        Location = new Point(15, 8),
                        Size = new Size(300, 24),
                        Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                        ForeColor = Color.FromArgb(52, 73, 94),
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    itemPanel.Controls.Add(label);

                    // Create control based on property type
                    Control? control = CreateControlForProperty(property);
                    if (control != null)
                    {
                        control.Location = new Point(480, 5);
                        control.Size = GetControlSize(property.PropertyType);
                        control.Font = new Font("Tahoma", 10F);
                        control.Anchor = AnchorStyles.Right | AnchorStyles.Top;

                        itemPanel.Controls.Add(control);
                        _configControls[property.Name] = control;
                    }

                    itemYPos += 45;
                }

                yPos += groupBox.Height + groupSpacing;
            }
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
                return "📹 Máy ảnh";
            if (propertyName.Contains("Hardware"))
                return "🔌 Phần cứng";
            if (propertyName.Contains("Cloud"))
                return "☁️ Đám Mây";
            if (propertyName.Contains("PLC"))
                return "📦 Cấu hình PLC";
            if (propertyName.Contains("TCP"))
                return "🔧 Cấu hình truyền thông";
            return "⚙️ Cài đặt chung";
        }

        private string GetDisplayName(string propertyName)
        {
            // Convert property names to user-friendly display names
            var displayNames = new Dictionary<string, string>()
            {
                { "AppHideEnable", "Ẩn ứng dụng khi tắt" },
                { "AppStartWithWindows", "Khởi động cùng Windows" },
                { "TCP_Port", "Cổng TCP" },
                { "PLC_IP", "Địa chỉ IP PLC" },
                { "Description", "Description" },

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
                    //TextAlign = HorizontalAlignment.Center,
                    //HasMaximum = true,
                    Maximum = property.Name.ToLower().Contains("port") ? 65535 : int.MaxValue,
                    //HasMinimum = true,
                    Minimum = 0,
                    Watermark = "2-click: numpad | Ctrl+2-click: keyboard"
                };

                // UINumPadTextBox tự động có numpad dialog khi double click
                // Thêm thêm option cho bàn phím chữ bằng Ctrl+Double Click
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

                // Thêm double-click event để hiện bàn phím ảo
                textBox.DoubleClick += (s, e) => ShowVirtualKeyboard(textBox, property.Name);

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
                        Size = new Size(30, 30),
                        Location = new Point(170, 0),
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
                        Size = new Size(200, 30)
                    };
                    textBox.Size = new Size(165, 30);
                    container.Controls.Add(textBox);
                    container.Controls.Add(browseBtn);

                    return container;
                }

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
            else if (control is Panel panel && value is string stringValue2)
            {
                // Handle path controls with browse button
                var textBoxInPanel = panel.Controls.OfType<UITextBox>().FirstOrDefault();
                if (textBoxInPanel != null)
                {
                    textBoxInPanel.Text = stringValue2 ?? string.Empty;
                }
            }
        }
        #endregion
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            AppConfigs.Current.Save();
            this.ShowSuccessDialog("Lưu cấu hình cài đặt thành công");
        }
    }
}
