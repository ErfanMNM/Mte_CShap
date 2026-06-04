using System.IO.Ports;
using System.Text;
using TSo.Configs;
using TSo.Models;

namespace TSo.Services;

public enum ScannerStatus { Disconnected, Connected, Error }

public class ScannerService : IDisposable
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;
    private SerialPort? _port;
    private CancellationTokenSource? _cts;
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

            _port = new SerialPort(_config.Handheld_COM_Port, 9600, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                NewLine = "\r"
            };

            _port.DataReceived += Port_DataReceived;
            _port.Open();

            SetStatus(ScannerStatus.Connected);
            _logger.Log("System", LogType.DeviceAction, "Scanner connected",
                $"{{'Port':'{_config.Handheld_COM_Port}'}}", "INFO-SCAN-01");
            return true;
        }
        catch (Exception ex)
        {
            SetStatus(ScannerStatus.Error);
            _logger.Log("System", LogType.Error, "Scanner connection failed",
                $"{{'Port':'{_config.Handheld_COM_Port}','Error':'{ex.Message}'}}", "ERR-SCAN-01");
            return false;
        }
    }

    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            var data = _port!.ReadExisting();
            var lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    _logger.Log("Scanner", LogType.DeviceAction, $"Scanner data: {trimmed}");
                    OnDataReceived?.Invoke(trimmed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, "Scanner read error", ex.Message);
        }
    }

    public void Disconnect()
    {
        if (_port != null)
        {
            try
            {
                _port.DataReceived -= Port_DataReceived;
                if (_port.IsOpen)
                    _port.Close();
                _port.Dispose();
            }
            catch { }
            _port = null;
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
        _cts?.Dispose();
    }
}
