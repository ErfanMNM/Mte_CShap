using System.Text.Json.Serialization;

namespace MTVS.Models
{
    public class AppConfig
    {
        [JsonPropertyName("Appwrite")]
        public AppwriteConfig? Appwrite { get; set; }

        [JsonPropertyName("Client")]
        public ClientConfig? Client { get; set; }

        [JsonPropertyName("Security")]
        public SecurityConfig? Security { get; set; }
    }

    public class AppwriteConfig
    {
        [JsonPropertyName("Endpoint")]
        public string? Endpoint { get; set; }

        [JsonPropertyName("ProjectId")]
        public string? ProjectId { get; set; }

        [JsonPropertyName("ApiKey")]
        public string? ApiKey { get; set; }
    }

    public class ClientConfig
    {
        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("Product")]
        public string? Product { get; set; }

        [JsonPropertyName("Site")]
        public string? Site { get; set; }

        [JsonPropertyName("Tenant")]
        public string? Tenant { get; set; }

        [JsonPropertyName("UpdateChannel")]
        public string? UpdateChannel { get; set; } = "stable";

        [JsonPropertyName("InstallPath")]
        public string? InstallPath { get; set; }

        [JsonPropertyName("BackupPath")]
        public string? BackupPath { get; set; }

        [JsonPropertyName("CheckIntervalMinutes")]
        public int CheckIntervalMinutes { get; set; } = 60;
    }

    public class SecurityConfig
    {
        [JsonPropertyName("PublicKeyPath")]
        public string? PublicKeyPath { get; set; }

        [JsonPropertyName("VerifySignature")]
        public bool VerifySignature { get; set; } = true;
    }
}

