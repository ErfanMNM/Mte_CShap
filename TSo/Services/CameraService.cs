using System.Net;
using System.Net.Sockets;
using System.Text;
using TSo.Configs;
using TSo.Models;

namespace TSo.Services;

public enum CameraStatus { Disconnected, Connected, Error, Reconnecting }

public class CameraService : IDisposable
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;
    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    private Task? _receiveTask;
    private CameraStatus _status = CameraStatus.Disconnected;

    public event Action<CameraStatus>? OnStatusChanged;
    public event Action<string>? OnDataReceived;

    public CameraStatus Status => _status;
    public bool IsConnected => _status == CameraStatus.Connected;

    public CameraService(Logger logger, AppConfigs config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task StartAsync()
    {
        if (_listener != null) return;

        try
        {
            var ip = IPAddress.Any;
            _listener = new TcpListener(ip, _config.Camera_01_Port);
            _listener.Start();

            _cts = new CancellationTokenSource();
            _logger.Log("System", LogType.System, "Camera listener started",
                $"{{'Port':'{_config.Camera_01_Port}'}}", "INFO-CAM-01");

            _ = AcceptClientsAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, "Camera listener start failed", ex.Message, "ERR-CAM-01");
            SetStatus(CameraStatus.Error);
        }
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener != null)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(ct);
                if (_client != null)
                {
                    try { _client.Close(); } catch { }
                }

                _client = client;
                _stream = _client.GetStream();
                SetStatus(CameraStatus.Connected);

                _logger.Log("System", LogType.DeviceAction, "Camera connected");

                _ = ReceiveDataAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Log("System", LogType.Warning, "Camera accept error", ex.Message);
                SetStatus(CameraStatus.Disconnected);
                await Task.Delay(3000, ct);
            }
        }
    }

    private async Task ReceiveDataAsync(CancellationToken ct)
    {
        var buffer = new byte[1024];
        var sb = new StringBuilder();

        while (!ct.IsCancellationRequested && _client?.Connected == true)
        {
            try
            {
                var bytesRead = await _stream!.ReadAsync(buffer, ct);
                if (bytesRead == 0)
                {
                    HandleDisconnection();
                    break;
                }

                var data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                sb.Append(data);

                var content = sb.ToString();
                int newlineIdx;
                while ((newlineIdx = content.IndexOf('\n')) >= 0)
                {
                    var line = content[..newlineIdx].Trim();
                    content = content[(newlineIdx + 1)..];

                    if (!string.IsNullOrEmpty(line))
                    {
                        _logger.Log("Camera", LogType.Debug, $"Camera data received: {line}");
                        OnDataReceived?.Invoke(line);
                    }
                }
                sb.Clear();
                sb.Append(content);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                HandleDisconnection();
                break;
            }
        }
    }

    private void HandleDisconnection()
    {
        SetStatus(CameraStatus.Disconnected);
        _logger.Log("System", LogType.Warning, "Camera disconnected");

        CloseClient();

        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (_listener != null)
                    _ = AcceptClientsAsync(_cts!.Token);
            });
        }
    }

    private void CloseClient()
    {
        try { _stream?.Close(); } catch { }
        try { _client?.Close(); } catch { }
        _stream = null;
        _client = null;
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
        _cts?.Cancel();
        CloseClient();
        _listener?.Stop();
        _listener = null;
        SetStatus(CameraStatus.Disconnected);
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}
