using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;

namespace GProject.PLCHelpers
{
    /// <summary>
    /// Helper tải map địa chỉ PLC từ Google Sheet (cột A → key, cột C → DM address),
    /// cache local xuống đĩa cạnh file credential để dùng khi mất mạng / không truy cập được Sheet.
    /// Pattern mirror từ TApp/Helpers/OmronPLCHelper.cs.
    /// </summary>
    public static class PLCAddressWithGoogleSheetHelper
    {
        public static string FilePath { get; set; } = Path.Combine($@"C:/GProject/Configs/Google.json");

        private static string LocalCachePath =>
            Path.Combine(
                Path.GetDirectoryName(FilePath) ?? AppContext.BaseDirectory,
                "plc_addresses.json");

        private static readonly Dictionary<string, string> _addressMap = new();

        public static IReadOnlyDictionary<string, string> AddressMap => _addressMap;

        public static bool Init(string spreadsheetId, string range)
        {
            try
            {
                Dictionary<string, string> onlineData = LoadFromGoogleSheet(spreadsheetId, range);
                ApplyData(onlineData);
                SaveToLocal(onlineData);
                return true;
            }
            catch
            {
                Dictionary<string, string> localData = LoadFromLocal();
                ApplyData(localData);
                return false;
            }
        }

        public static string Get(string key)
        {
            if (_addressMap.TryGetValue(key, out var v)) return v;
            throw new KeyNotFoundException($"Không tìm thấy địa chỉ PLC cho key: {key}");
        }

        private static Dictionary<string, string> LoadFromGoogleSheet(string spreadsheetId, string range)
        {
            GoogleCredential credential;
            string? folder = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            using (var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            }

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GProject-PLCAddress"
            });

            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            var result = new Dictionary<string, string>();

            if (response.Values != null && response.Values.Count > 0)
            {
                foreach (var row in response.Values.Skip(1))
                {
                    if (row.Count >= 3)
                    {
                        string? name = row[0]?.ToString()?.Trim();
                        string? dm = row[2]?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(dm))
                        {
                            result[name] = dm;
                        }
                    }
                }
            }
            return result;
        }

        private static void ApplyData(Dictionary<string, string> data)
        {
            _addressMap.Clear();
            foreach (var kv in data)
                _addressMap[kv.Key] = kv.Value;
        }

        private static void SaveToLocal(Dictionary<string, string> data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalCachePath, json);
        }

        private static Dictionary<string, string> LoadFromLocal()
        {
            string? folder = Path.GetDirectoryName(LocalCachePath);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(LocalCachePath))
            {
                File.WriteAllText(LocalCachePath, "{}");
            }

            var json = File.ReadAllText(LocalCachePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
    }
}