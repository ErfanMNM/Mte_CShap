namespace GProject.Config;

/// <summary>
/// Root configuration model for application settings.
/// </summary>
public class AppConfig
{
    // PLC Settings
    public string? PLC_IP { get; set; }
    public int PLC_Port { get; set; }

    //API Settings
    public string? API_HostIP { get; set; } = "0.0.0.0";
    public int API_Port { get; set; }

    // Camera Settings
    public string? Camera_Ip { get; set; }
    public int Camera_Port { get; set; }

    // Application Settings
    public bool AutoStart { get; set; }

    // PLC ACK V2 (camera pipeline) — chờ CurrentID/CurrentStatus confirm lane đã gửi
    public int CameraAckTimeoutMs { get; set; } = 500;
    public int CameraAckPollIntervalMs { get; set; } = 10;

    /// <summary>
    /// Sets all properties to their default values.
    /// </summary>
    public void SetDefault()
    {
        PLC_IP = "192.168.1.1";
        PLC_Port = 9600;

        Camera_Ip = "192.168.1.100";
        Camera_Port = 5000;

        API_HostIP = "127.0.0.1";
        API_Port = 9999;

        AutoStart = true;

        CameraAckTimeoutMs = 500;
        CameraAckPollIntervalMs = 10;
    }
}
