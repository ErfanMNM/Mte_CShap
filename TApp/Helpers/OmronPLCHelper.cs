using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;

namespace TApp.Helpers
{

    public enum e_PLC_Result
    {
        Pass = 0,
        Fail = 1
    }

    public static class PLCAddressWithGoogleSheetHelper
    {
        private static readonly string LocalCachePath = "plc_addresses.json";
        private static readonly Dictionary<string, string> _addressMap = new Dictionary<string, string>();

        public static IReadOnlyDictionary<string, string> AddressMap => _addressMap;

        public static bool Init(string spreadsheetId, string range)
        {
            try
            {
                Dictionary<string,string> onlineData = LoadFromGoogleSheet(spreadsheetId, range);
                ApplyData(onlineData);
                SaveToLocal(onlineData);
                return true;
            }
            catch 
            {
                Dictionary<string,string> localData = LoadFromLocal();
                ApplyData(localData);
                return true;
            }
        }

        public static string Get(string key)
        {
            if (_addressMap.ContainsKey(key))
                return _addressMap[key];
            throw new KeyNotFoundException($"Không tìm thấy địa chỉ PLC cho key: {key}");
        }

        private static  Dictionary<string, string> LoadFromGoogleSheet(string spreadsheetId, string range)
        {

            // Tạo credential từ file JSON và gán scope đọc Google Sheets
            GoogleCredential credential = GoogleCredential.GetApplicationDefault();
            credential = credential.CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PLCAddress"
            });

            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            var result = new Dictionary<string, string>();

            if (response.Values != null && response.Values.Count > 0)
            {
                foreach (var row in response.Values.Skip(1)) // skip header
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
            //tự động tạo file nếu không tồn tại
            if (!Directory.Exists(Path.GetDirectoryName(LocalCachePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LocalCachePath));
            }
            //tạo file nếu không tồn tại

            if (!File.Exists(LocalCachePath))
            {
                File.WriteAllText(LocalCachePath, "{}"); // Tạo file rỗng nếu không tồn tại
            }

            var json = File.ReadAllText(LocalCachePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
    }
}
