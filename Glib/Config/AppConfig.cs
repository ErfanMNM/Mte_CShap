namespace Glib.Config;

/// <summary>
/// Root configuration model for application settings.
/// </summary>
public class AppConfig
{
    // PLC Settings
    public string PlcIp { get; set; } = "192.168.1.1";
    public int PlcPort { get; set; } = 9600;

    // Camera Settings
    public string CameraIp { get; set; } = "192.168.1.100";
    public int CameraPort { get; set; } = 5000;

    // Application Settings
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Sets all properties to their default values.
    /// </summary>
    public void SetDefault()
    {
        PlcIp = "192.168.1.1";
        PlcPort = 9600;

        CameraIp = "192.168.1.100";
        CameraPort = 5000;

        AutoStart = true;
    }
}
