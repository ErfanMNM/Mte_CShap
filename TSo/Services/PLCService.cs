using HslCommunication;
using HslCommunication.Profinet.Omron;
using TSo.Configs;

namespace TSo.Services;

public enum PLCStatus { Connected, Disconnect }

public class PLCService : IDisposable
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;
    private OmronFinsUdp? _plc;
    private PLCStatus _status = PLCStatus.Disconnect;
    private CancellationTokenSource? _cts;
    private Task? _pollingTask;

    public event Action<PLCStatus>? OnStatusChanged;
    public event Action<int[]>? OnCountersUpdated;

    public PLCStatus Status => _status;
    public bool IsConnected => _status == PLCStatus.Connected;

    public PLCService(Logger logger, AppConfigs config)
    {
        _logger = logger;
        _config = config;
    }

    public bool Connect()
    {
        try
        {
            var ip = _config.PLC_Test_Mode ? "127.0.0.1" : _config.PLC_IP;
            var port = _config.PLC_Test_Mode ? 9600 : _config.PLC_Port;

            _plc = new OmronFinsUdp
            {
                IpAddress = ip,
                Port = port
            };
            _plc.PlcType = OmronPlcType.CSCJ;
            _plc.SA1 = 1;
            _plc.GCT = 2;
            _plc.DA1 = 0;
            _plc.SID = 0;
            _plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
            _plc.ByteTransform.IsStringReverseByteWord = true;

            var test = _plc.ReadInt32(_config.PLC_Ready_DM ?? "D16", 1);
            if (test.IsSuccess)
            {
                SetStatus(PLCStatus.Connected);
                _logger.Log("System", LogType.System, "PLC connected",
                    $"{{'IP':'{ip}','Port':'{port}'}}", "INFO-PLC-01");
                StartPolling();
                return true;
            }

            SetStatus(PLCStatus.Disconnect);
            _logger.Log("System", LogType.Error, "PLC connection test failed",
                $"{{'IP':'{ip}','Port':'{port}','Error':'{test.Message}'}}", "ERR-PLC-01");
            return false;
        }
        catch (Exception ex)
        {
            SetStatus(PLCStatus.Disconnect);
            _logger.Log("System", LogType.Error, "PLC connection exception", ex.Message, "ERR-PLC-02");
            return false;
        }
    }

    public void Disconnect()
    {
        _cts?.Cancel();
        _plc?.Dispose();
        _plc = null;
        SetStatus(PLCStatus.Disconnect);
    }

    public bool Write(string address, short value)
    {
        if (_plc == null || _status != PLCStatus.Connected)
            return false;

        try
        {
            var result = _plc.Write(address, value);
            return result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    public bool SendReadySignal(int value)
    {
        var addr = _config.PLC_Ready_DM ?? "D16";
        return Write(addr, (short)value);
    }

    public bool SendRejectResult(int result)
    {
        var addr = _config.PLC_Reject_DM ?? "D10";
        return Write(addr, (short)result);
    }

    public bool ResetCounters()
    {
        var addr = _config.PLC_Reset_Counter_DM ?? "D20";
        return Write(addr, 1);
    }

    public bool ClearErrors()
    {
        var addr = _config.PLC_Clear_DM ?? "D30";
        return Write(addr, 1);
    }

    public int[]? ReadCounters()
    {
        if (_plc == null || _status != PLCStatus.Connected)
            return null;

        try
        {
            var addr = _config.PLC_Total_Count_DM ?? "D40";
            var result = _plc.ReadInt32(addr, 5);
            if (result.IsSuccess)
            {
                var counters = result.Content;
                OnCountersUpdated?.Invoke(counters);
                return counters;
            }
        }
        catch { }

        return null;
    }

    public int? ReadDeactiveState()
    {
        if (_plc == null || _status != PLCStatus.Connected)
            return null;

        try
        {
            var addr = _config.PLC_Deactive_DM ?? "D100";
            var result = _plc.ReadInt32(addr);
            if (result.IsSuccess)
                return result.Content;
        }
        catch { }

        return null;
    }

    private void StartPolling()
    {
        _cts = new CancellationTokenSource();
        _pollingTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_config.PLC_Time_Refresh, _cts.Token);
                    ReadCounters();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch { }
            }
        }, _cts.Token);
    }

    private void SetStatus(PLCStatus status)
    {
        if (_status == status) return;
        _status = status;
        GlobalState.DeviceStatus.PLCStatus = status.ToString();
        OnStatusChanged?.Invoke(status);
    }

    public void Dispose()
    {
        Disconnect();
        _cts?.Dispose();
    }
}
