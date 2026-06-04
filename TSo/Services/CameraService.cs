using TTManager.Datalogic;
using TSo.Configs;

namespace TSo.Services;

public enum CameraStatus { Disconnected, Connected, Error, Reconnecting }

public class CameraService : IDisposable
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;
    private readonly DatalogicCamera _camera;
    private CameraStatus _status = CameraStatus.Disconnected;

    public event Action<CameraStatus>? OnStatusChanged;
    public event Action<string>? OnDataReceived;

    public CameraStatus Status => _status;
    public bool IsConnected => _status == CameraStatus.Connected;

    public CameraService(Logger logger, AppConfigs config)
    {
        _logger = logger;
        _config = config;
        _camera = new DatalogicCamera(_config.Camera_01_IP ?? "127.0.0.1", _config.Camera_01_Port);
        _camera.ClientCallback += Camera_Callback;
    }

    private void Camera_Callback(eDatalogicCameraState state, string data)
    {
        switch (state)
        {
            case eDatalogicCameraState.Connected:
                SetStatus(CameraStatus.Connected);
                _logger.Log("System", LogType.DeviceAction, "Camera connected",
                    $"{{'IP':'{_config.Camera_01_IP}','Port':'{_config.Camera_01_Port}'}}", "INFO-CAM-01");
                break;

            case eDatalogicCameraState.Disconnected:
                SetStatus(CameraStatus.Disconnected);
                _logger.Log("System", LogType.Warning, "Camera disconnected", data, "ERR-CAM-01");
                break;

            case eDatalogicCameraState.Reconnecting:
                SetStatus(CameraStatus.Reconnecting);
                _logger.Log("System", LogType.Warning, "Camera reconnecting...", data, "WARN-CAM-01");
                break;

            case eDatalogicCameraState.Received when !string.IsNullOrWhiteSpace(data):
                ProcessCameraData(data);
                break;
        }
    }

    private void ProcessCameraData(string rawData)
    {
        var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            _logger.Log("Camera", LogType.Debug, $"Camera data received: {trimmed}");
            OnDataReceived?.Invoke(trimmed);
        }
    }

    public void Start()
    {
        if (string.IsNullOrEmpty(_config.Camera_01_IP))
        {
            _logger.Log("System", LogType.Warning, "Camera IP not configured, skipping camera init");
            return;
        }

        try
        {
            _camera.Connect();
            _logger.Log("System", LogType.System, "Camera service started",
                $"{{'IP':'{_config.Camera_01_IP}','Port':'{_config.Camera_01_Port}'}}", "INFO-CAM-00");
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, "Camera start failed", ex.Message, "ERR-CAM-00");
            SetStatus(CameraStatus.Error);
        }
    }

    private void SetStatus(CameraStatus status)
    {
        if (_status == status) return;
        _status = status;
        GlobalState.DeviceStatus.CameraStatus = status.ToString();
        OnStatusChanged?.Invoke(status);
    }

    public void Stop()
    {
        _camera.Disconnect();
        SetStatus(CameraStatus.Disconnected);
    }

    public void Dispose()
    {
        Stop();
    }
}
