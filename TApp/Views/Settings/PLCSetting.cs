using HslCommunication;
using Sunny.UI;
using System.ComponentModel;
using System.Globalization;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Utils;
using TTManager.Diaglogs;

namespace TApp.Views.Settings
{
    public partial class PLCSetting : UIPage
    {
        private readonly PLCSettingsStore _store = new("PLC_RECIPEs");
        private PLCRecipeAuditLog? _audit;
        private readonly SemaphoreSlim _plcIoLock = new(1, 1);
        private readonly Dictionary<string, string> _recipeValues = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _plcValues = new(StringComparer.OrdinalIgnoreCase);
        private DataGridView _grid = null!;
        private ComboBox _recipeCombo = null!;
        private TextBox _recipeName = null!;
        private Label _statusLabel = null!;
        private System.Windows.Forms.Timer? _pollTimer;
        private bool _isPolling;
        private bool _loading;
        private bool _initialized;
        private string _baseAddress = "D100";

        public bool IsPageOpen { get; set; }

        // Giữ API cũ để các màn hình khác có thể đọc trạng thái gần nhất nếu cần.
        public static PLC_Parameter PLC_Parameter_On_PLC { get; set; } = new();

        public PLCSetting()
        {
            InitializeComponent();
            uiTabControl1.TabPages.Remove(tabPage3);
            tabPage3.Dispose();
            uiTableLayoutPanel1.Controls.Remove(uiTitlePanel2);
            tabPage1.Controls.Clear();
            uiTableLayoutPanel1.Dispose();
            tabPage1.Text = "Camera Trước";
            BuildSettingsUi();
            _pollTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            _pollTimer.Tick += async (_, _) =>
            {
                if (_isPolling || !IsPageOpen || !_initialized) return;
                _isPolling = true;
                try { await ReadParametersAsync(null, reportStatus: false); }
                finally { _isPolling = false; }
            };
            Disposed += (_, _) => _pollTimer?.Dispose();
        }

        public void INIT()
        {
            if (_initialized) return;
            try
            {
                try { _audit = new PLCRecipeAuditLog("PLC_RECIPEs"); }
                catch { _audit = null; }
                _baseAddress = PLCAddressWithGoogleSheetHelper.Get("PLC_Delay_Camera_DM_C2");
            }
            catch
            {
                _baseAddress = "D100";
            }

            omronplC_Hsl1.PLC_IP = AppConfigs.Current.PLC_Test_Mode ? "127.0.0.1" : AppConfigs.Current.PLC_IP ?? "127.0.0.1";
            omronplC_Hsl1.PLC_PORT = AppConfigs.Current.PLC_Test_Mode ? 9600 : AppConfigs.Current.PLC_Port;

            try
            {
                var parameters = _store.LoadParameters(_baseAddress);
                LoadRecipes(parameters);
                omronplC_Hsl1.InitPLC();
                _initialized = true;
                SetStatus("Đã nạp cấu hình. Chọn Đọc PLC để lấy giá trị hiện tại.");
                if (IsPageOpen) _pollTimer?.Start();
            }
            catch (Exception ex)
            {
                SetStatus($"Lỗi nạp cấu hình: {ex.Message}");
                this.ShowErrorDialog($"Không thể tải cấu hình PLC: {ex.Message}");
            }
        }

        private void BuildSettingsUi()
        {
            tabPage1.AutoScroll = true;
            uiTitlePanel2.Dock = DockStyle.Fill;
            uiTitlePanel2.Text = "ẢNH TỪ CAMERA";

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(4)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 66));
            root.Controls.Add(uiTitlePanel2, 0, 0);

            var settings = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0, 6, 0, 0)
            };
            settings.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            settings.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            settings.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            settings.Controls.Add(BuildRecipePanel(), 0, 0);
            settings.Controls.Add(BuildParameterPanel(), 0, 1);
            _statusLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            settings.Controls.Add(_statusLabel, 0, 2);
            root.Controls.Add(settings, 0, 1);
            tabPage1.Controls.Add(root);
        }

        private Control BuildRecipePanel()
        {
            var panel = new GroupBox { Text = "Recipe", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

            _recipeCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _recipeCombo.SelectedIndexChanged += RecipeCombo_SelectedIndexChanged;
            _recipeName = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Tên recipe mới" };
            layout.Controls.Add(_recipeCombo, 0, 0);
            layout.Controls.Add(_recipeName, 1, 0);
            layout.Controls.Add(CreateButton("Tạo", CreateRecipe_Click), 2, 0);
            layout.Controls.Add(CreateButton("Lưu giá trị", SaveRecipeValues_Click), 3, 0);
            layout.Controls.Add(CreateButton("Áp dụng recipe", ApplyRecipe_Click), 4, 0);
            layout.Controls.Add(CreateButton("Xóa", DeleteRecipe_Click), 5, 0);
            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildParameterPanel()
        {
            var panel = new GroupBox { Text = "Danh sách ô nhớ D", Dock = DockStyle.Fill, Padding = new Padding(8) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
            toolbar.Controls.Add(CreateButton("+ Tham số", AddParameter_Click));
            toolbar.Controls.Add(CreateButton("Xóa dòng", DeleteParameter_Click));
            toolbar.Controls.Add(CreateButton("Lưu danh sách", SaveParameterList_Click));
            toolbar.Controls.Add(CreateButton("Đọc tất cả", ReadAll_Click));
            toolbar.Controls.Add(CreateButton("Đọc dòng", ReadSelected_Click));
            toolbar.Controls.Add(CreateButton("Ghi dòng", WriteSelected_Click));
            toolbar.Controls.Add(CreateButton("Áp dụng + Lưu", ApplyAndSave_Click));

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                EditMode = DataGridViewEditMode.EditOnEnter
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PlcValue", HeaderText = "PLC", ReadOnly = true, FillWeight = 55 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Input", HeaderText = "Giá trị nhập", FillWeight = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Address", HeaderText = "Địa chỉ", FillWeight = 60 });
            var typeColumn = new DataGridViewComboBoxColumn { Name = "ValueType", HeaderText = "Kiểu", FillWeight = 58, FlatStyle = FlatStyle.Flat };
            typeColumn.Items.AddRange(nameof(PlcValueType.Int32), nameof(PlcValueType.Bit));
            _grid.Columns.Add(typeColumn);
            var accessColumn = new DataGridViewComboBoxColumn { Name = "AccessMode", HeaderText = "Quyền", FillWeight = 75, FlatStyle = FlatStyle.Flat };
            accessColumn.Items.AddRange(nameof(PlcAccessMode.ReadOnly), nameof(PlcAccessMode.WriteOnly), nameof(PlcAccessMode.ReadWrite));
            _grid.Columns.Add(accessColumn);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Mô tả", FillWeight = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Trạng thái", ReadOnly = true, FillWeight = 75 });
            foreach (DataGridViewColumn column in _grid.Columns)
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            _grid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "AccessMode") return;
                _grid.Rows[e.RowIndex].Cells["Input"].ReadOnly =
                    string.Equals(CellText(_grid.Rows[e.RowIndex], "AccessMode"), nameof(PlcAccessMode.ReadOnly), StringComparison.OrdinalIgnoreCase);
            };
            _grid.DataError += (_, e) => { e.ThrowException = false; };

            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(_grid, 0, 1);
            panel.Controls.Add(layout);
            return panel;
        }

        private static Button CreateButton(string text, EventHandler onClick)
        {
            var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(76, 30), Margin = new Padding(2) };
            button.Click += onClick;
            return button;
        }

        private void LoadRecipes(IReadOnlyList<PlcParameterDefinition> parameters)
        {
            _loading = true;
            try
            {
                var names = _store.LoadRecipeNames().ToList();
                if (!names.Contains("Default", StringComparer.OrdinalIgnoreCase)) names.Insert(0, "Default");

                if (!File.Exists(Path.Combine("PLC_RECIPEs", "Default.rplc")))
                {
                    var defaults = CreateInitialRecipe(parameters);
                    _store.SaveRecipe("Default", defaults, parameters);
                }

                _recipeCombo.Items.Clear();
                foreach (var name in names) _recipeCombo.Items.Add(name);
                string preferred = GetLastSelectedRecipeName() ?? "Default";
                _recipeCombo.SelectedItem = names.FirstOrDefault(x => x.Equals(preferred, StringComparison.OrdinalIgnoreCase))
                    ?? names.FirstOrDefault(x => x.Equals("Default", StringComparison.OrdinalIgnoreCase));
                if (_recipeCombo.SelectedIndex < 0 && _recipeCombo.Items.Count > 0) _recipeCombo.SelectedIndex = 0;
                LoadSelectedRecipe(parameters);
            }
            finally
            {
                _loading = false;
            }
        }

        private PlcRecipeDocument CreateInitialRecipe(IReadOnlyList<PlcParameterDefinition> parameters)
        {
            var recipe = new PlcRecipeDocument { DelayCamera = "1000", DelayReject = "2000", RejectStreng = "20" };
            for (int index = 0; index < parameters.Count; index++)
                recipe.Values[PLCSettingsStore.NormalizeAddress(parameters[index].Address)] = index switch { 0 => "1000", 1 => "2000", 2 => "20", _ => "0" };
            return recipe;
        }

        private void LoadSelectedRecipe(IReadOnlyList<PlcParameterDefinition>? parameters = null)
        {
            parameters ??= LoadParameterDefinitionsFromGridOrStore();
            var name = _recipeCombo.SelectedItem?.ToString() ?? "Default";
            var fallback = name.Equals("Default", StringComparison.OrdinalIgnoreCase) ? CreateInitialRecipe(parameters) : new PlcRecipeDocument();
            var recipe = _store.LoadRecipe(name, parameters, fallback);
            _recipeValues.Clear();
            foreach (var pair in recipe.Values) _recipeValues[pair.Key] = pair.Value;
            PopulateGrid(parameters, _recipeValues);
        }

        private IReadOnlyList<PlcParameterDefinition> LoadParameterDefinitionsFromGridOrStore()
        {
            if (_grid.Rows.Count > 0)
            {
                try { return ReadDefinitionsFromGrid(); }
                catch { }
            }
            return _store.LoadParameters(_baseAddress);
        }

        private void PopulateGrid(IReadOnlyList<PlcParameterDefinition> parameters, IReadOnlyDictionary<string, string> values)
        {
            _loading = true;
            try
            {
                _grid.Rows.Clear();
                foreach (var parameter in parameters.OrderBy(p => p.SortOrder))
                {
                    string address = PLCSettingsStore.NormalizeAddress(parameter.Address);
                    string input = values.TryGetValue(address, out var value) ? value : parameter.ValueType == PlcValueType.Bit ? "0" : "0";
                    int rowIndex = _grid.Rows.Add(parameter.Name, _plcValues.GetValueOrDefault(address, ""), input, address,
                        parameter.ValueType.ToString(), parameter.AccessMode.ToString(), parameter.Description, "");
                    _grid.Rows[rowIndex].Tag = parameter.Clone();
                    _grid.Rows[rowIndex].Cells["Input"].ReadOnly = parameter.AccessMode == PlcAccessMode.ReadOnly;
                }
            }
            finally { _loading = false; }
        }

        private List<PlcParameterDefinition> ReadDefinitionsFromGrid()
        {
            _grid.EndEdit();
            var result = new List<PlcParameterDefinition>();
            for (int index = 0; index < _grid.Rows.Count; index++)
            {
                var row = _grid.Rows[index];
                var definition = new PlcParameterDefinition
                {
                    Name = CellText(row, "Name"),
                    Address = CellText(row, "Address"),
                    ValueType = Enum.TryParse(CellText(row, "ValueType"), out PlcValueType valueType) ? valueType : PlcValueType.Int32,
                    AccessMode = Enum.TryParse(CellText(row, "AccessMode"), out PlcAccessMode accessMode) ? accessMode : PlcAccessMode.ReadWrite,
                    Description = CellText(row, "Description"),
                    SortOrder = index
                };
                string? addressError = PLCSettingsStore.ValidateAddress(definition.Address, definition.ValueType);
                string? nameError = PLCSettingsStore.ValidateName(definition.Name);
                if (nameError != null || addressError != null)
                    throw new ArgumentException($"Dòng {index + 1}: {nameError ?? addressError}");
                result.Add(definition);
            }

            var duplicateName = result.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
            if (duplicateName != null) throw new ArgumentException($"Tên tham số bị trùng: {duplicateName.Key}");
            var duplicateAddress = result.GroupBy(x => x.Address, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
            if (duplicateAddress != null) throw new ArgumentException($"Địa chỉ D bị trùng: {duplicateAddress.Key}");
            PLCSettingsStore.ValidateParameterDefinitions(result);
            return result;
        }

        private static string CellText(DataGridViewRow row, string name) => row.Cells[name].Value?.ToString()?.Trim() ?? string.Empty;

        private List<(PlcParameterDefinition Definition, string Value)> ReadInputValues()
        {
            var definitions = ReadDefinitionsFromGrid();
            var result = new List<(PlcParameterDefinition, string)>();
            foreach (var definition in definitions)
            {
                if (definition.AccessMode == PlcAccessMode.ReadOnly) continue;
                string value = CellText(_grid.Rows[definition.SortOrder], "Input");
                if (definition.ValueType == PlcValueType.Int32)
                {
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                        throw new ArgumentException($"Giá trị '{value}' của {definition.Name} phải là số nguyên Int32.");
                }
                else if (!TryParseBit(value, out _))
                {
                    throw new ArgumentException($"Giá trị của {definition.Name} phải là 0/1 hoặc true/false.");
                }
                result.Add((definition, value));
            }
            return result;
        }

        private async void ReadAll_Click(object? sender, EventArgs e)
        {
            await ReadParametersAsync(null, reportStatus: true);
        }

        private async void ReadSelected_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedDefinition();
            if (selected == null) { this.ShowWarningTip("Hãy chọn một dòng tham số."); return; }
            await ReadParametersAsync([selected], reportStatus: true);
        }

        private async Task ReadParametersAsync(IReadOnlyList<PlcParameterDefinition>? selected, bool reportStatus)
        {
            try
            {
                var parameters = selected ?? ReadDefinitionsFromGrid();
                var statuses = new List<string>();
                await _plcIoLock.WaitAsync();
                List<(PlcParameterDefinition Parameter, string? Value, string? Error, bool Skipped)> results;
                try
                {
                    results = await Task.Run(() =>
                    {
                        var readResults = new List<(PlcParameterDefinition, string?, string?, bool)>();
                        foreach (var parameter in parameters)
                        {
                            if (parameter.AccessMode == PlcAccessMode.WriteOnly)
                            {
                                readResults.Add((parameter, null, null, true));
                                continue;
                            }
                            var read = ReadValue(parameter);
                            readResults.Add(read.IsSuccess
                                ? (parameter, read.Content, null, false)
                                : (parameter, null, read.Message, false));
                        }
                        return readResults;
                    });
                }
                finally { _plcIoLock.Release(); }

                foreach (var (parameter, value, error, skipped) in results)
                {
                    if (skipped) { SetRowStatus(parameter, "Chỉ ghi"); continue; }
                    if (error == null && value != null)
                    {
                        _plcValues[PLCSettingsStore.NormalizeAddress(parameter.Address)] = value;
                        UpdateLegacySnapshot(parameter.Name, value);
                        SetPlcValue(parameter, value);
                        SetRowStatus(parameter, "Đã đọc");
                    }
                    else
                    {
                        SetRowStatus(parameter, "Lỗi đọc: " + error);
                        statuses.Add($"{parameter.Name}: {error}");
                    }
                }
                if (reportStatus)
                    SetStatus(statuses.Count == 0 ? "Đọc PLC hoàn tất." : "Một số ô nhớ đọc lỗi: " + string.Join(" | ", statuses));
            }
            catch (Exception ex) { if (reportStatus) SetStatus("Lỗi đọc PLC: " + ex.Message); }
        }

        private async void WriteSelected_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedDefinition();
            if (selected == null) { this.ShowWarningTip("Hãy chọn một dòng tham số."); return; }
            try
            {
                var definitions = ReadDefinitionsFromGrid();
                EnsureParameterListSaved(definitions);
                var value = ReadInputValues().FirstOrDefault(x => x.Definition.Name.Equals(selected.Name, StringComparison.OrdinalIgnoreCase));
                if (value.Definition == null) { this.ShowWarningTip("Tham số đang chọn chỉ có quyền đọc."); return; }
                var failures = await WriteValuesAsync([value]);
                SetStatus(failures.Count == 0 ? $"Đã ghi {selected.Name} xuống PLC." : $"Ghi lỗi: {string.Join(" | ", failures)}");
            }
            catch (Exception ex) { SetStatus("Lỗi ghi PLC: " + ex.Message); }
        }

        private async void ApplyRecipe_Click(object? sender, EventArgs e)
        {
            try
            {
                var definitions = ReadDefinitionsFromGrid();
                EnsureParameterListSaved(definitions);
                var values = definitions
                    .Where(d => d.AccessMode != PlcAccessMode.ReadOnly)
                    .Select(d => (d, _recipeValues.GetValueOrDefault(PLCSettingsStore.NormalizeAddress(d.Address), "0")))
                    .ToList();
                ValidateInputValues(values);
                var failures = await WriteValuesAsync(values);
                if (failures.Count == 0)
                {
                    string name = _recipeCombo.SelectedItem?.ToString() ?? "Default";
                    LogRecipeAudit("APPLY", name, RecipeSummary(_store.LoadRecipe(name, ReadDefinitionsFromGrid())));
                    SetStatus("Đã áp dụng recipe xuống PLC.");
                }
                else SetStatus("Áp dụng chưa hoàn tất: " + string.Join(" | ", failures));
            }
            catch (Exception ex) { SetStatus("Lỗi áp dụng recipe: " + ex.Message); }
        }

        private async void ApplyAndSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var parameters = ReadDefinitionsFromGrid();
                EnsureParameterListSaved(parameters);
                var values = ReadInputValues();
                var failures = await WriteValuesAsync(values);
                if (failures.Count > 0)
                {
                    SetStatus("Chưa lưu recipe vì PLC ghi lỗi: " + string.Join(" | ", failures));
                    return;
                }
                SaveCurrentRecipe(parameters, values);
                SetStatus("Đã áp dụng xuống PLC và lưu recipe.");
            }
            catch (Exception ex) { SetStatus("Lỗi áp dụng/lưu: " + ex.Message); }
        }

        private async Task<List<string>> WriteValuesAsync(IReadOnlyList<(PlcParameterDefinition Definition, string Value)> values)
        {
            var failures = new List<string>();
            await _plcIoLock.WaitAsync();
            List<(PlcParameterDefinition Definition, string Value, string? Error)> results;
            try
            {
                results = await Task.Run(() =>
                {
                    var writeResults = new List<(PlcParameterDefinition, string, string?)>();
                    foreach (var item in values)
                    {
                        if (item.Definition.AccessMode == PlcAccessMode.ReadOnly) continue;
                        var write = WriteValue(item.Definition, item.Value);
                        writeResults.Add((item.Definition, item.Value, write.IsSuccess ? null : write.Message));
                    }
                    return writeResults;
                });
            }
            finally { _plcIoLock.Release(); }
            foreach (var (definition, value, error) in results)
            {
                if (error != null)
                {
                    failures.Add($"{definition.Name}: {error}");
                    SetRowStatus(definition, "Lỗi ghi: " + error);
                }
                else
                {
                    _plcValues[PLCSettingsStore.NormalizeAddress(definition.Address)] = value;
                    SetPlcValue(definition, value);
                    SetRowStatus(definition, "Đã ghi");
                }
            }
            return failures;
        }

        private OperateResult<string> ReadValue(PlcParameterDefinition parameter)
        {
            string address = PLCSettingsStore.NormalizeAddress(parameter.Address);
            if (parameter.ValueType == PlcValueType.Bit)
            {
                OperateResult<bool[]> read;
                lock (omronplC_Hsl1.CommunicationLock)
                    read = omronplC_Hsl1.plc.ReadBool(address, 1);
                return read.IsSuccess
                    ? OperateResult.CreateSuccessResult(read.Content![0] ? "1" : "0")
                    : OperateResult.CreateFailedResult<string>(read);
            }
            OperateResult<int> intRead;
            lock (omronplC_Hsl1.CommunicationLock)
                intRead = omronplC_Hsl1.plc.ReadInt32(address);
            return intRead.IsSuccess
                ? OperateResult.CreateSuccessResult(intRead.Content.ToString(CultureInfo.InvariantCulture))
                : OperateResult.CreateFailedResult<string>(intRead);
        }

        private OperateResult WriteValue(PlcParameterDefinition parameter, string value)
        {
            string address = PLCSettingsStore.NormalizeAddress(parameter.Address);
            if (parameter.ValueType == PlcValueType.Bit)
            {
                TryParseBit(value, out bool bitValue);
                lock (omronplC_Hsl1.CommunicationLock)
                    return omronplC_Hsl1.plc.Write(address, new[] { bitValue });
            }
            int intValue = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            lock (omronplC_Hsl1.CommunicationLock)
                return omronplC_Hsl1.plc.Write(address, intValue);
        }

        private void ValidateInputValues(IReadOnlyList<(PlcParameterDefinition Definition, string Value)> values)
        {
            foreach (var (definition, value) in values)
            {
                if (definition.ValueType == PlcValueType.Int32 && !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    throw new ArgumentException($"Giá trị '{value}' của {definition.Name} phải là số nguyên Int32.");
                if (definition.ValueType == PlcValueType.Bit && !TryParseBit(value, out _))
                    throw new ArgumentException($"Giá trị của {definition.Name} phải là 0/1 hoặc true/false.");
            }
        }

        private static bool TryParseBit(string value, out bool result)
        {
            if (value.Trim() is "1" || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)) { result = true; return true; }
            if (value.Trim() is "0" || value.Trim().Equals("false", StringComparison.OrdinalIgnoreCase)) { result = false; return true; }
            result = false;
            return false;
        }

        private PlcParameterDefinition? GetSelectedDefinition()
        {
            if (_grid.CurrentRow == null) return null;
            try { return ReadDefinitionsFromGrid().FirstOrDefault(x => x.SortOrder == _grid.CurrentRow.Index); }
            catch (Exception ex) { SetStatus(ex.Message); return null; }
        }

        private void SetPlcValue(PlcParameterDefinition parameter, string value)
        {
            int index = FindRow(parameter);
            if (index >= 0) _grid.Rows[index].Cells["PlcValue"].Value = value;
        }

        private static void UpdateLegacySnapshot(string name, string value)
        {
            if (name.Equals("DelayCamera", StringComparison.OrdinalIgnoreCase)) PLC_Parameter_On_PLC.DelayCamera = value;
            else if (name.Equals("DelayReject", StringComparison.OrdinalIgnoreCase)) PLC_Parameter_On_PLC.DelayReject = value;
            else if (name.Equals("RejectStrength", StringComparison.OrdinalIgnoreCase) || name.Equals("RejectStreng", StringComparison.OrdinalIgnoreCase))
                PLC_Parameter_On_PLC.RejectStreng = value;
        }

        private void SetRowStatus(PlcParameterDefinition parameter, string status)
        {
            int index = FindRow(parameter);
            if (index >= 0) _grid.Rows[index].Cells["Status"].Value = status;
        }

        private int FindRow(PlcParameterDefinition parameter)
        {
            for (int i = 0; i < _grid.Rows.Count; i++)
                if (CellText(_grid.Rows[i], "Address").Equals(parameter.Address, StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }

        private void AddParameter_Click(object? sender, EventArgs e)
        {
            int row = _grid.Rows.Add("Tham số mới", "", "0", "D100", nameof(PlcValueType.Int32), nameof(PlcAccessMode.ReadWrite), "", "Chưa lưu");
            _grid.CurrentCell = _grid.Rows[row].Cells["Name"];
            _grid.BeginEdit(true);
        }

        private void DeleteParameter_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) { this.ShowWarningTip("Hãy chọn một dòng tham số."); return; }
            _grid.Rows.Remove(_grid.CurrentRow);
            SetStatus("Đã xóa dòng khỏi danh sách đang sửa. Chọn Lưu danh sách để xác nhận.");
        }

        private void SaveParameterList_Click(object? sender, EventArgs e)
        {
            try
            {
                _grid.EndEdit();
                var parameters = ReadDefinitionsFromGrid();
                _store.SaveParameters(parameters);
                SetStatus("Đã lưu danh sách ô nhớ D.");
            }
            catch (Exception ex) { SetStatus("Không lưu được danh sách: " + ex.Message); }
        }

        private void SaveRecipeValues_Click(object? sender, EventArgs e)
        {
            try
            {
                var parameters = ReadDefinitionsFromGrid();
                EnsureParameterListSaved(parameters);
                SaveCurrentRecipe(parameters, ReadInputValues());
                SetStatus("Đã lưu giá trị recipe trên máy tính. PLC chưa thay đổi.");
            }
            catch (Exception ex) { SetStatus("Không lưu được recipe: " + ex.Message); }
        }

        private void SaveCurrentRecipe(IReadOnlyList<PlcParameterDefinition> parameters,
            IReadOnlyList<(PlcParameterDefinition Definition, string Value)> values)
        {
            var recipe = _store.LoadRecipe(_recipeCombo.SelectedItem?.ToString() ?? "Default", parameters);
            recipe.Values ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (definition, value) in values)
                recipe.Values[PLCSettingsStore.NormalizeAddress(definition.Address)] = value;
            _store.SaveRecipe(_recipeCombo.SelectedItem?.ToString() ?? "Default", recipe, parameters);
            LogRecipeAudit("UPDATE", _recipeCombo.SelectedItem?.ToString() ?? "Default", RecipeSummary(recipe));
            _recipeValues.Clear();
            foreach (var pair in recipe.Values) _recipeValues[pair.Key] = pair.Value;
        }

        private void CreateRecipe_Click(object? sender, EventArgs e)
        {
            try
            {
                var name = _recipeName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name)) { this.ShowWarningTip("Nhập tên recipe mới."); return; }
                var parameters = ReadDefinitionsFromGrid();
                EnsureParameterListSaved(parameters);
                var current = _store.LoadRecipe(_recipeCombo.SelectedItem?.ToString() ?? "Default", parameters);
                string? error = _store.CreateRecipe(name, current, parameters);
                if (error != null) { this.ShowErrorDialog(error); return; }
                LogRecipeAudit("CREATE", name, RecipeSummary(current));
                LogRecipeAudit("SELECT", name, RecipeSummary(current));
                ReloadRecipeList(name);
                _recipeName.Clear();
                SetStatus($"Đã tạo recipe '{name}' từ recipe đang chọn.");
            }
            catch (Exception ex) { SetStatus("Không tạo được recipe: " + ex.Message); }
        }

        private void DeleteRecipe_Click(object? sender, EventArgs e)
        {
            string selected = _recipeCombo.SelectedItem?.ToString() ?? "";
            if (!_store.DeleteRecipe(selected)) { this.ShowWarningTip("Không thể xóa recipe mặc định hoặc recipe không tồn tại."); return; }
            LogRecipeAudit("DELETE", selected, "N/A");
            ReloadRecipeList("Default");
            LogRecipeAudit("SELECT", "Default", RecipeSummary(_store.LoadRecipe("Default", ReadDefinitionsFromGrid())));
            SetStatus($"Đã xóa recipe '{selected}'.");
        }

        private void ReloadRecipeList(string selected)
        {
            _loading = true;
            try
            {
                _recipeCombo.Items.Clear();
                foreach (var name in _store.LoadRecipeNames()) _recipeCombo.Items.Add(name);
                _recipeCombo.SelectedItem = _recipeCombo.Items.Cast<string>().FirstOrDefault(x => x.Equals(selected, StringComparison.OrdinalIgnoreCase));
                if (_recipeCombo.SelectedIndex < 0 && _recipeCombo.Items.Count > 0) _recipeCombo.SelectedIndex = 0;
                LoadSelectedRecipe();
            }
            finally { _loading = false; }
        }

        private void EnsureParameterListSaved(IReadOnlyList<PlcParameterDefinition> current)
        {
            var saved = _store.LoadParameters(_baseAddress);
            bool matches = current.Count == saved.Count;
            for (int i = 0; matches && i < current.Count; i++)
            {
                var left = current[i];
                var right = saved[i];
                matches = left.Name.Equals(right.Name, StringComparison.OrdinalIgnoreCase)
                    && PLCSettingsStore.NormalizeAddress(left.Address).Equals(PLCSettingsStore.NormalizeAddress(right.Address), StringComparison.OrdinalIgnoreCase)
                    && left.ValueType == right.ValueType
                    && left.AccessMode == right.AccessMode
                    && left.Description.Equals(right.Description, StringComparison.Ordinal);
            }
            if (!matches) throw new InvalidOperationException("Hãy bấm 'Lưu danh sách' trước khi ghi PLC hoặc thao tác recipe.");
        }

        private void RecipeCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loading || _recipeCombo.SelectedItem == null) return;
            try
            {
                LoadSelectedRecipe();
                string name = _recipeCombo.SelectedItem.ToString()!;
                LogRecipeAudit("SELECT", name, RecipeSummary(_store.LoadRecipe(name, ReadDefinitionsFromGrid())));
            }
            catch (Exception ex) { SetStatus("Không tải được recipe: " + ex.Message); }
        }

        private static string RecipeSummary(PlcRecipeDocument recipe) =>
            string.Join(",", recipe.Values.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase).Select(x => $"{x.Key}={x.Value}"));

        private string? GetLastSelectedRecipeName()
        {
            try { return _audit?.GetLastSelectedRecipeName(); }
            catch { return null; }
        }

        private void LogRecipeAudit(string action, string recipeName, string value)
        {
            try { _audit?.Record(action, recipeName, value, GlobalVarialbles.CurrentUser.Username); }
            catch { }
        }

        private void PLCSetting_Initialize(object sender, EventArgs e)
        {
            IsPageOpen = true;
            if (!_initialized) INIT();
            else _pollTimer?.Start();
            try { webView21.Source = new Uri($"http://{AppConfigs.Current.Camera_01_IP}/monitor"); }
            catch { }
            GlobalVarialbles.Logger?.LogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Mở trang cài đặt PLC", "", "UA-PLCSETTING-01");
        }

        private void PLCSetting_Finalize(object sender, EventArgs e)
        {
            IsPageOpen = false;
            _pollTimer?.Stop();
            try { webView21.Source = new Uri("about:blank"); }
            catch { }
        }

        private void SetStatus(string message)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(() => SetStatus(message)); }
                catch (InvalidOperationException) { }
                return;
            }
            if (_statusLabel != null) _statusLabel.Text = message;
        }

        // Các handler này còn được Designer tham chiếu bởi bố cục cũ đã bị ẩn.
        private void backgroundWorker1_DoWork(object? sender, DoWorkEventArgs e) { }
        private void backgroundWorker2_DoWork(object? sender, DoWorkEventArgs e) { }
        private void uiSymbolButton1_Click(object? sender, EventArgs e) { }
        private void uiSymbolButton2_Click(object? sender, EventArgs e) { }
    }

    public class PLC_Parameter
    {
        public string DelayCamera { get; set; } = "0";
        public string DelayReject { get; set; } = "0";
        public string RejectStreng { get; set; } = "0";
    }
}
