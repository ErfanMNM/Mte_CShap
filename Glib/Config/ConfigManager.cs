namespace Glib.Config;

/// <summary>
/// Singleton manager for application configuration.
/// Provides easy access to config values: ConfigManager.Current.PlcIp
/// </summary>
public sealed class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance = new(() => new ConfigManager());
    private AppConfig _config;

    private ConfigManager()
    {
        _config = ConfigStorage.Load<AppConfig>() ?? new AppConfig();
    }

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ConfigManager Current => _instance.Value;

    // PLC Settings
    public string PlcIp
    {
        get => _config.PlcIp;
        set => _config.PlcIp = value;
    }

    public int PlcPort
    {
        get => _config.PlcPort;
        set => _config.PlcPort = value;
    }

    // Camera Settings
    public string CameraIp
    {
        get => _config.CameraIp;
        set => _config.CameraIp = value;
    }

    public int CameraPort
    {
        get => _config.CameraPort;
        set => _config.CameraPort = value;
    }

    // Application Settings
    public bool AutoStart
    {
        get => _config.AutoStart;
        set => _config.AutoStart = value;
    }

    /// <summary>
    /// Gets the underlying configuration object.
    /// </summary>
    public AppConfig Config => _config;

    /// <summary>
    /// Saves the current configuration to disk.
    /// </summary>
    public void Save()
    {
        ConfigStorage.Save(_config);
    }

    /// <summary>
    /// Saves the current configuration to a specific file path.
    /// </summary>
    public void Save(string filePath)
    {
        ConfigStorage.Save(_config, filePath);
    }

    /// <summary>
    /// Reloads configuration from disk, discarding any unsaved changes.
    /// </summary>
    public void Reload()
    {
        _config = ConfigStorage.Load<AppConfig>() ?? new AppConfig();
    }

    /// <summary>
    /// Resets all configuration values to defaults and saves.
    /// </summary>
    public void ResetToDefault()
    {
        _config.SetDefault();
        Save();
    }
}
