using System.Text.Json.Serialization;

namespace MTVS.Models
{
    public class Release
    {
        [JsonPropertyName("$id")]
        public string? Id { get; set; }

        [JsonPropertyName("product")]
        public string? Product { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        [JsonPropertyName("manifestRef")]
        public string? ManifestRef { get; set; }

        [JsonPropertyName("files")]
        public List<string>? Files { get; set; }

        [JsonPropertyName("os")]
        public string? Os { get; set; }

        [JsonPropertyName("arch")]
        public string? Arch { get; set; }

        [JsonPropertyName("minVersion")]
        public string? MinVersion { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("signedHash")]
        public string? SignedHash { get; set; }

        [JsonPropertyName("changelog")]
        public string? Changelog { get; set; }

        [JsonPropertyName("riskLevel")]
        public string? RiskLevel { get; set; }

        [JsonPropertyName("constraints")]
        public string? Constraints { get; set; }
    }

    public class ReleaseCheckResult
    {
        public bool HasUpdate { get; set; }
        public Release? LatestRelease { get; set; }
        public string? ManifestUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? Changelog { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

