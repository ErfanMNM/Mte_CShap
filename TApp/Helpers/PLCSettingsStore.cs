using Newtonsoft.Json;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace TApp.Helpers
{
    public enum PlcValueType
    {
        Int32,
        Bit
    }

    public enum PlcAccessMode
    {
        ReadOnly,
        WriteOnly,
        ReadWrite
    }

    public sealed class PlcParameterDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public PlcValueType ValueType { get; set; } = PlcValueType.Int32;
        public PlcAccessMode AccessMode { get; set; } = PlcAccessMode.ReadWrite;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public PlcParameterDefinition Clone() => new()
        {
            Name = Name,
            Address = Address,
            ValueType = ValueType,
            AccessMode = AccessMode,
            Description = Description,
            SortOrder = SortOrder
        };
    }

    /// <summary>
    /// Schema recipe v2. Ba thuộc tính legacy được giữ lại để các file .rplc cũ
    /// và phiên bản TApp trước vẫn đọc được ba tham số mặc định.
    /// </summary>
    public sealed class PlcRecipeDocument
    {
        public int SchemaVersion { get; set; } = 2;
        public string DelayCamera { get; set; } = "0";
        public string DelayReject { get; set; } = "0";
        public string RejectStreng { get; set; } = "0";
        public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public PlcRecipeDocument Clone()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in Values ?? new Dictionary<string, string>()) values[pair.Key] = pair.Value;
            return new PlcRecipeDocument
            {
                SchemaVersion = SchemaVersion,
                DelayCamera = DelayCamera,
                DelayReject = DelayReject,
                RejectStreng = RejectStreng,
                Values = values
            };
        }
    }

    /// <summary>
    /// Lưu metadata danh sách D và recipe vào thư mục cục bộ của TApp.
    /// </summary>
    public sealed class PLCSettingsStore
    {
        private static readonly Regex Int32AddressRegex = new(@"^D\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex BitAddressRegex = new(@"^D\d+\.(?:[0-9]|1[0-5])$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string _directory;
        private readonly string _parameterPath;

        public PLCSettingsStore(string directory = "PLC_RECIPEs")
        {
            _directory = string.IsNullOrWhiteSpace(directory) ? "PLC_RECIPEs" : directory;
            _parameterPath = Path.Combine(_directory, "PLC_D_LIST.json");
        }

        public IReadOnlyList<PlcParameterDefinition> LoadParameters(string baseAddress)
        {
            EnsureDirectory();

            if (File.Exists(_parameterPath))
            {
                List<PlcParameterDefinition>? values;
                try
                {
                    var json = File.ReadAllText(_parameterPath);
                    values = JsonConvert.DeserializeObject<List<PlcParameterDefinition>>(json);
                }
                catch (Exception ex) when (ex is IOException or JsonException)
                {
                    throw new InvalidDataException($"Không đọc được danh sách PLC '{_parameterPath}': {ex.Message}", ex);
                }
                if (values == null)
                    throw new InvalidDataException($"Danh sách PLC '{_parameterPath}' rỗng hoặc không hợp lệ.");
                ValidateParameters(values);
                return values.OrderBy(x => x.SortOrder).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
            }

            var defaults = CreateDefaultParameters(baseAddress);
            SaveParameters(defaults);
            return defaults;
        }

        public void SaveParameters(IEnumerable<PlcParameterDefinition> parameters)
        {
            var values = parameters?.Select(x => x.Clone()).ToList()
                ?? throw new ArgumentNullException(nameof(parameters));
            ValidateParameters(values);
            WriteJsonAtomically(_parameterPath, values);
        }

        public IReadOnlyList<string> LoadRecipeNames()
        {
            EnsureDirectory();
            var names = Directory.GetFiles(_directory, "*.rplc", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (names.Count == 0) names.Add("Default");
            return names;
        }

        public PlcRecipeDocument LoadRecipe(string name, IReadOnlyList<PlcParameterDefinition> parameters,
            PlcRecipeDocument? fallback = null)
        {
            var safeName = NormalizeRecipeName(name);
            var path = RecipePath(safeName);
            PlcRecipeDocument document;

            if (!File.Exists(path))
            {
                document = fallback?.Clone() ?? new PlcRecipeDocument();
            }
            else
            {
                try
                {
                    document = JsonConvert.DeserializeObject<PlcRecipeDocument>(File.ReadAllText(path))
                        ?? new PlcRecipeDocument();
                }
                catch (Exception ex) when (ex is IOException or JsonException)
                {
                    throw new InvalidDataException($"Không đọc được recipe '{safeName}': {ex.Message}", ex);
                }
            }

            document.SchemaVersion = Math.Max(document.SchemaVersion, 2);
            document.Values = NormalizeRecipeValues(document.Values);
            MapLegacyValues(document, parameters);
            return document;
        }

        public void SaveRecipe(string name, PlcRecipeDocument recipe,
            IReadOnlyList<PlcParameterDefinition>? parameters = null)
        {
            if (recipe is null) throw new ArgumentNullException(nameof(recipe));
            var safeName = NormalizeRecipeName(name);
            var document = recipe.Clone();
            document.SchemaVersion = 2;
            document.Values = NormalizeRecipeValues(document.Values);
            if (parameters is { Count: > 0 }) MapLegacyValues(document, parameters);
            WriteJsonAtomically(RecipePath(safeName), document);
        }

        public string? CreateRecipe(string name, PlcRecipeDocument? selectedRecipe,
            IReadOnlyList<PlcParameterDefinition>? parameters = null)
        {
            string safeName;
            try
            {
                safeName = NormalizeRecipeName(name);
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }

            if (File.Exists(RecipePath(safeName)))
                return "Tên recipe đã tồn tại, vui lòng chọn tên khác.";

            SaveRecipe(safeName, selectedRecipe?.Clone() ?? new PlcRecipeDocument(), parameters);
            return null;
        }

        public bool DeleteRecipe(string name)
        {
            var safeName = NormalizeRecipeName(name);
            if (safeName.Equals("Default", StringComparison.OrdinalIgnoreCase)) return false;

            var path = RecipePath(safeName);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        public static string? ValidateAddress(string address, PlcValueType valueType)
        {
            if (string.IsNullOrWhiteSpace(address)) return "Địa chỉ D không được để trống.";
            var normalized = address.Trim().ToUpperInvariant();
            bool valid = valueType == PlcValueType.Int32
                ? Int32AddressRegex.IsMatch(normalized)
                : BitAddressRegex.IsMatch(normalized);
            if (!valid) return valueType == PlcValueType.Int32
                ? "Địa chỉ Int32 phải có dạng D100."
                : "Địa chỉ Bit phải có dạng D100.00 đến D100.15.";
            string wordText = normalized[1..].Split('.')[0];
            if (!int.TryParse(wordText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int word)
                || word > (valueType == PlcValueType.Int32 ? 32766 : 32767))
                return "Số word D nằm ngoài phạm vi hỗ trợ.";
            return null;
        }

        public static string NormalizeAddress(string address)
        {
            string normalized = address.Trim().ToUpperInvariant();
            var match = Regex.Match(normalized, @"^D(?<word>\d+)(?:\.(?<bit>\d{1,2}))?$");
            if (!match.Success || !int.TryParse(match.Groups["word"].Value, out int word)) return normalized;
            if (!match.Groups["bit"].Success) return $"D{word}";
            return int.TryParse(match.Groups["bit"].Value, out int bit) && bit <= 15
                ? $"D{word}.{bit:D2}"
                : normalized;
        }

        public static string? ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Tên tham số không được để trống.";
            if (name.Trim().Length > 80) return "Tên tham số không được dài quá 80 ký tự.";
            return null;
        }

        private static List<PlcParameterDefinition> CreateDefaultParameters(string baseAddress)
        {
            string normalized = NormalizeAddress(baseAddress);
            if (!Int32AddressRegex.IsMatch(normalized)
                || !int.TryParse(normalized[1..], out int parsedStart)
                || parsedStart > 32763) normalized = "D100";
            int start = int.Parse(normalized[1..]);

            return new List<PlcParameterDefinition>
            {
                new() { Name = "DelayCamera", Address = $"D{start}", Description = "Độ trễ camera", SortOrder = 0 },
                new() { Name = "DelayReject", Address = $"D{start + 2}", Description = "Độ trễ reject", SortOrder = 1 },
                new() { Name = "RejectStrength", Address = $"D{start + 4}", Description = "Độ rộng xung reject", SortOrder = 2 }
            };
        }

        private static void MapLegacyValues(PlcRecipeDocument document, IReadOnlyList<PlcParameterDefinition> parameters)
        {
            if (parameters.Count == 0) return;
            document.Values ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var legacy = new[] { document.DelayCamera, document.DelayReject, document.RejectStreng };
            for (int i = 0; i < Math.Min(3, parameters.Count); i++)
            {
                string address = NormalizeAddress(parameters[i].Address);
                if (!document.Values.ContainsKey(address) || document.Values[address] is null)
                    document.Values[address] = legacy[i] ?? "0";
            }

            if (parameters.Count > 0 && document.Values.TryGetValue(NormalizeAddress(parameters[0].Address), out var delayCamera))
                document.DelayCamera = delayCamera;
            if (parameters.Count > 1 && document.Values.TryGetValue(NormalizeAddress(parameters[1].Address), out var delayReject))
                document.DelayReject = delayReject;
            if (parameters.Count > 2 && document.Values.TryGetValue(NormalizeAddress(parameters[2].Address), out var rejectStrength))
                document.RejectStreng = rejectStrength;
        }

        private static Dictionary<string, string> NormalizeRecipeValues(IDictionary<string, string>? values)
        {
            var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (values == null) return normalized;
            foreach (var pair in values)
                if (!string.IsNullOrWhiteSpace(pair.Key)) normalized[NormalizeAddress(pair.Key)] = pair.Value ?? "0";
            return normalized;
        }

        public static void ValidateParameterDefinitions(IEnumerable<PlcParameterDefinition> parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            ValidateParameters(parameters.Select(x => x.Clone()).ToList());
        }

        private static void ValidateParameters(IReadOnlyList<PlcParameterDefinition> values)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parsedAddresses = new List<(PlcParameterDefinition Parameter, int FirstWord, int LastWord, int? Bit)>();
            foreach (var item in values)
            {
                if (!Enum.IsDefined(item.ValueType)) throw new ArgumentException($"Kiểu dữ liệu không hợp lệ: {item.ValueType}");
                if (!Enum.IsDefined(item.AccessMode)) throw new ArgumentException($"Quyền truy cập không hợp lệ: {item.AccessMode}");
                var nameError = ValidateName(item.Name);
                if (nameError != null) throw new ArgumentException(nameError);

                var addressError = ValidateAddress(item.Address, item.ValueType);
                if (addressError != null) throw new ArgumentException(addressError);

                item.Name = item.Name.Trim();
                item.Address = NormalizeAddress(item.Address);
                if (!names.Add(item.Name)) throw new ArgumentException($"Tên tham số bị trùng: {item.Name}");
                if (!addresses.Add(item.Address)) throw new ArgumentException($"Địa chỉ D bị trùng: {item.Address}");

                var match = Regex.Match(item.Address, @"^D(?<word>\d+)(?:\.(?<bit>\d+))?$", RegexOptions.IgnoreCase);
                int word = int.Parse(match.Groups["word"].Value, System.Globalization.CultureInfo.InvariantCulture);
                int? bit = match.Groups["bit"].Success
                    ? int.Parse(match.Groups["bit"].Value, System.Globalization.CultureInfo.InvariantCulture)
                    : null;
                parsedAddresses.Add((item, word, word + (bit.HasValue ? 0 : 1), bit));
            }

            foreach (var address in parsedAddresses)
                if (address.FirstWord <= 16 && address.LastWord >= 16)
                    throw new ArgumentException($"Địa chỉ {address.Parameter.Address} chồng lên D16 đang dành cho trạng thái Ready của PLC.");

            for (int i = 0; i < parsedAddresses.Count; i++)
            for (int j = i + 1; j < parsedAddresses.Count; j++)
            {
                var left = parsedAddresses[i];
                var right = parsedAddresses[j];
                bool sameWord = left.FirstWord == right.FirstWord;
                if (sameWord && left.Bit.HasValue && right.Bit.HasValue) continue;
                if (left.FirstWord <= right.LastWord && right.FirstWord <= left.LastWord)
                    throw new ArgumentException($"Địa chỉ {left.Parameter.Address} và {right.Parameter.Address} bị chồng vùng nhớ D.");
            }
        }

        private string RecipePath(string name) => Path.Combine(_directory, $"{name}.rplc");

        private string NormalizeRecipeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên recipe không được để trống.");
            var value = name.Trim();
            if (value.Length > 80 || value is "." or ".." || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Tên recipe không hợp lệ.");
            return value;
        }

        private void EnsureDirectory() => Directory.CreateDirectory(_directory);

        private void WriteJsonAtomically(string path, object value)
        {
            EnsureDirectory();
            string tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllText(tempPath, JsonConvert.SerializeObject(value, Formatting.Indented));
                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, null);
                }
                else
                {
                    File.Move(tempPath, path);
                }
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
    }

    /// <summary>Giữ lịch sử recipe của phiên bản PLC settings trước.</summary>
    public sealed class PLCRecipeAuditLog
    {
        private readonly string _path;

        public PLCRecipeAuditLog(string directory = "PLC_RECIPEs")
        {
            Directory.CreateDirectory(directory);
            _path = Path.Combine(directory, "log.rlplc");
        }

        public string? GetLastSelectedRecipeName()
        {
            using var connection = OpenConnection();
            using var command = new SQLiteCommand("SELECT RecipeName FROM Log WHERE Action = @Action ORDER BY ID DESC LIMIT 1;", connection);
            command.Parameters.AddWithValue("@Action", "SELECT");
            return command.ExecuteScalar()?.ToString();
        }

        public void Record(string action, string recipeName, string recipeValue, string? userName)
        {
            using var connection = OpenConnection();
            using var command = new SQLiteCommand(
                "INSERT INTO Log (RecipeName, RecipeValue, Action, Timestamp, UserName) VALUES (@RecipeName, @RecipeValue, @Action, @Timestamp, @UserName);",
                connection);
            command.Parameters.AddWithValue("@RecipeName", recipeName);
            command.Parameters.AddWithValue("@RecipeValue", recipeValue);
            command.Parameters.AddWithValue("@Action", action);
            command.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            command.Parameters.AddWithValue("@UserName", userName ?? string.Empty);
            command.ExecuteNonQuery();
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection($"Data Source={_path};Version=3;");
            connection.Open();
            using var command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Log (ID INTEGER NOT NULL UNIQUE, RecipeName TEXT, RecipeValue TEXT, Action TEXT, Timestamp TEXT, UserName TEXT, PRIMARY KEY(ID AUTOINCREMENT));",
                connection);
            command.ExecuteNonQuery();
            return connection;
        }
    }
}
