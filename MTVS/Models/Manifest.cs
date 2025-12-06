using System.Text.Json.Serialization;

namespace MTVS.Models
{
    public class Manifest
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("product")]
        public string? Product { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        [JsonPropertyName("components")]
        public List<Component>? Components { get; set; }

        [JsonPropertyName("backup")]
        public BackupConfig? Backup { get; set; }

        [JsonPropertyName("policy")]
        public PolicyConfig? Policy { get; set; }
    }

    public class Component
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; } // exe, service, process, config, plugin, etc.

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("sourcePath")]
        public string? SourcePath { get; set; }

        [JsonPropertyName("targetPath")]
        public string? TargetPath { get; set; }

        [JsonPropertyName("preserve")]
        public object? Preserve { get; set; } // true, false, or array of paths

        [JsonPropertyName("hooks")]
        public ComponentHooks? Hooks { get; set; }

        [JsonPropertyName("dependsOn")]
        public List<string>? DependsOn { get; set; }
    }

    public class ComponentHooks
    {
        [JsonPropertyName("preInstall")]
        public string? PreInstall { get; set; }

        [JsonPropertyName("postInstall")]
        public string? PostInstall { get; set; }

        [JsonPropertyName("healthcheck")]
        public string? Healthcheck { get; set; }
    }

    public class BackupConfig
    {
        [JsonPropertyName("paths")]
        public List<string>? Paths { get; set; }

        [JsonPropertyName("exclude")]
        public List<string>? Exclude { get; set; }
    }

    public class PolicyConfig
    {
        [JsonPropertyName("requireUserClick")]
        public bool RequireUserClick { get; set; } = true;

        [JsonPropertyName("maintenanceWindow")]
        public MaintenanceWindow? MaintenanceWindow { get; set; }
    }

    public class MaintenanceWindow
    {
        [JsonPropertyName("start")]
        public string? Start { get; set; }

        [JsonPropertyName("end")]
        public string? End { get; set; }
    }
}

