using System.Text.Json.Serialization;

namespace MTVS.Models
{
    public class Backup
    {
        [JsonPropertyName("$id")]
        public string? Id { get; set; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("backupId")]
        public string? BackupId { get; set; }

        [JsonPropertyName("storageFileId")]
        public string? StorageFileId { get; set; }

        [JsonPropertyName("product")]
        public string? Product { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("checksum")]
        public string? Checksum { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("encrypted")]
        public bool Encrypted { get; set; }

        [JsonPropertyName("metadata")]
        public string? Metadata { get; set; }
    }
}

