using TTManager.Communication;
using TSo.Configs;

namespace TSo.Services;

public enum ScannerStatus { Disconnected, Connected, Error }

public class ScannerService : IDisposable
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;
    private SerialClientHelper? _scanner;

    private ScannerStatus _status = ScannerStatus.Disconnected;

    public event Action<ScannerStatus>? OnStatusChanged;
    public event Action<string>? OnDataReceived;

    public ScannerStatus Status => _status;
    public bool IsConnected => _status == ScannerStatus.Connected;

    public ScannerService(Logger logger, AppConfigs config)
    {
        _logger = logger;
        _config = config;
    }

    private void Scanner_Callback(SerialClientState state, string data)
    {
        switch (state)
        {
            case SerialClientState.Connected:
                SetStatus(ScannerStatus.Connected);
                _logger.Log("System", LogType.DeviceAction, "Scanner connected",
                    $"{{'Port':'{_config.Handheld_COM_Port}'}}", "INFO-SCAN-01");
                break;

            case SerialClientState.Disconnected:
                SetStatus(ScannerStatus.Disconnected);
                _logger.Log("System", LogType.Warning, "Scanner disconnected", data, "ERR-SCAN-01");
                break;

            case SerialClientState.Received when !string.IsNullOrWhiteSpace(data):
                ProcessScannerData(data);
                break;

            case SerialClientState.Error:
                SetStatus(ScannerStatus.Error);
                _logger.Log("System", LogType.Error, "Scanner error", data, "ERR-SCAN-02");
                break;
        }
    }

    private void ProcessScannerData(string rawData)
    {
        var lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            _logger.Log("Scanner", LogType.DeviceAction, $"Scanner data: {trimmed}");
            OnDataReceived?.Invoke(trimmed);
        }
    }

    public bool Connect()
    {
        if (string.IsNullOrEmpty(_config.Handheld_COM_Port))
        {
            _logger.Log("System", LogType.Warning, "Scanner COM port not configured");
            return false;
        }

        try
        {
            Disconnect();

            _scanner = new SerialClientHelper(_config.Handheld_COM_Port, 9600);
            _scanner.SerialClientCallback += Scanner_Callback;
            _scanner.Connect();

            return _scanner.Connected;
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, "Scanner connection failed",
                $"{{'Port':'{_config.Handheld_COM_Port}','Error':'{ex.Message}'}}", "ERR-SCAN-03");
            SetStatus(ScannerStatus.Error);
            return false;
        }
    }

    public void Disconnect()
    {
        if (_scanner != null)
        {
            _scanner.SerialClientCallback -= Scanner_Callback;
            _scanner.Disconnect();
            _scanner = null;
        }
        SetStatus(ScannerStatus.Disconnected);
    }

    private void SetStatus(ScannerStatus status)
    {
        if (_status == status) return;
        _status = status;
        GlobalState.DeviceStatus.ScannerStatus = status.ToString();
        OnStatusChanged?.Invoke(status);
    }

    public void Dispose()
    {
        Disconnect();
    }
}
