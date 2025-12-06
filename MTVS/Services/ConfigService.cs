using System.Text.Json;
using MTVS.Models;

namespace MTVS.Services
{
    public class ConfigService
    {
        private AppConfig? _config;
        private readonly string _configPath;

        public ConfigService(string configPath = "AppConfig.json")
        {
            _configPath = configPath;
            LoadConfig();
        }

        public AppConfig GetConfig()
        {
            if (_config == null)
            {
                throw new InvalidOperationException("Configuration not loaded");
            }
            return _config;
        }

        public void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    // Tạo config mặc định
                    _config = CreateDefaultConfig();
                    SaveConfig();
                }

                // Override với environment variables nếu có
                var apiKey = Environment.GetEnvironmentVariable("APPWRITE_API_KEY");
                if (!string.IsNullOrEmpty(apiKey) && _config?.Appwrite != null)
                {
                    _config.Appwrite.ApiKey = apiKey;
                }

                var projectId = Environment.GetEnvironmentVariable("APPWRITE_PROJECT_ID");
                if (!string.IsNullOrEmpty(projectId) && _config?.Appwrite != null)
                {
                    _config.Appwrite.ProjectId = projectId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        public void SaveConfig()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        private AppConfig CreateDefaultConfig()
        {
            // Tạo ClientId từ MAC address hoặc GUID
            var clientId = GetOrCreateClientId();

            return new AppConfig
            {
                Appwrite = new AppwriteConfig
                {
                    Endpoint = "https://cloud.appwrite.io/v1",
                    ProjectId = "",
                    ApiKey = ""
                },
                Client = new ClientConfig
                {
                    ClientId = clientId,
                    Product = "MTVS",
                    Site = "",
                    Tenant = "",
                    UpdateChannel = "stable",
                    InstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MTVS"),
                    BackupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTVS", "Backups"),
                    CheckIntervalMinutes = 60
                },
                Security = new SecurityConfig
                {
                    PublicKeyPath = "public_key.pem",
                    VerifySignature = true
                }
            };
        }

        private string GetOrCreateClientId()
        {
            var clientIdPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTVS", "client_id.txt");
            var clientIdDir = Path.GetDirectoryName(clientIdPath);
            
            if (!string.IsNullOrEmpty(clientIdDir) && !Directory.Exists(clientIdDir))
            {
                Directory.CreateDirectory(clientIdDir);
            }

            if (File.Exists(clientIdPath))
            {
                return File.ReadAllText(clientIdPath).Trim();
            }

            // Tạo GUID mới
            var newClientId = Guid.NewGuid().ToString();
            File.WriteAllText(clientIdPath, newClientId);
            return newClientId;
        }
    }
}

