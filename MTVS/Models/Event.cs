using System.Text.Json.Serialization;

namespace MTVS.Models
{
    public class Event
    {
        [JsonPropertyName("$id")]
        public string? Id { get; set; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; } // check, download, backup_ok, install_ok, health_fail, rollback, error

        [JsonPropertyName("product")]
        public string? Product { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }
    }
}

