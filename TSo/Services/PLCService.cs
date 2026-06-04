using HslCommunication;
using TTManager.PLCHelpers;
using TSo.Configs;
using System.ComponentModel;

namespace TSo.Services;

public enum PLCStatusEnum { Connected, Disconnect }

public class PLCService : IDisposable
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;
    private readonly OmronPLC_Hsl _omron;
    private readonly BackgroundWorker _wkCounter;
    private PLCStatusEnum _status = PLCStatusEnum.Disconnect;
    private int _readyValue = 0;

    public event Action<PLCStatusEnum>? OnStatusChanged;
    public event Action<int[]>? OnCountersUpdated;

    public PLCStatusEnum Status => _status;
    public bool IsConnected => _status == PLCStatusEnum.Connected;

    public PLCService(Logger logger, AppConfigs config)
    {
        _logger = logger;
        _config = config;

        _omron = new OmronPLC_Hsl();
        _omron.PLCStatus_OnChange += Omron_StatusChanged;

        _wkCounter = new BackgroundWorker();
        _wkCounter.DoWork += WK_Counter_DoWork;
        _wkCounter.WorkerSupportsCancellation = true;
    }

    public void Connect()
    {
        try
        {
            var ip = _config.PLC_Test_Mode ? "127.0.0.1" : (_config.PLC_IP ?? "127.0.0.1");
            var port = _config.PLC_Test_Mode ? 9600 : _config.PLC_Port;

            _omron.PLC_IP = ip;
            _omron.PLC_PORT = port;
            _omron.Time_Update = _config.PLC_Time_Refresh;
            _omron.PLC_Ready_DM = _config.PLC_Ready_DM ?? "D16";

            _omron.InitPLC();

            if (!_wkCounter.IsBusy)
                _wkCounter.RunWorkerAsync();

            _logger.Log("System", LogType.System, "PLC service started",
                $"{{'IP':'{ip}','Port':'{port}','Refresh':'{_config.PLC_Time_Refresh}ms'}}", "INFO-PLC-00");
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, "PLC connection failed", ex.Message, "ERR-PLC-00");
            SetStatus(PLCStatusEnum.Disconnect);
        }
    }

    private void Omron_StatusChanged(object? sender, OmronPLC_Hsl.PLCStatusEventArgs e)
    {
        var newStatus = e.Status switch
        {
            OmronPLC_Hsl.PLCStatus.Connected => PLCStatusEnum.Connected,
            _ => PLCStatusEnum.Disconnect
        };

        if (newStatus != _status)
        {
            SetStatus(newStatus);
            _logger.Log("System",
                newStatus == PLCStatusEnum.Connected ? LogType.Info : LogType.Error,
                newStatus == PLCStatusEnum.Connected ? "PLC connected" : "PLC disconnected",
                e.Message,
                newStatus == PLCStatusEnum.Connected ? "INFO-PLC-01" : "ERR-PLC-01");
        }
    }

    private void WK_Counter_DoWork(object? sender, DoWorkEventArgs e)
    {
        while (!_wkCounter.CancellationPending)
        {
            try
            {
                Thread.Sleep(_config.PLC_Time_Refresh);
                ReadCounters();
            }
            catch (OperationCanceledException) { break; }
            catch { }
        }
    }

    public void SetReady(int value)
    {
        _readyValue = value;
    }

    public bool Write(string address, short value)
    {
        if (_omron.plc == null || _status != PLCStatusEnum.Connected)
            return false;

        try
        {
            var result = _omron.plc.Write(address, value);
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
        if (_omron.plc == null || _status != PLCStatusEnum.Connected)
            return null;

        try
        {
            var addr = _config.PLC_Total_Count_DM ?? "D40";
            var result = _omron.plc.ReadInt32(addr, 5);
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
        if (_omron.plc == null || _status != PLCStatusEnum.Connected)
            return null;

        try
        {
            var addr = _config.PLC_Deactive_DM ?? "D100";
            var result = _omron.plc.ReadInt32(addr);
            if (result.IsSuccess)
                return result.Content;
        }
        catch { }

        return null;
    }

    private void SetStatus(PLCStatusEnum status)
    {
        if (_status == status) return;
        _status = status;
        GlobalState.DeviceStatus.PLCStatus = status.ToString();
        OnStatusChanged?.Invoke(status);
    }

    public void Disconnect()
    {
        if (_wkCounter.IsBusy)
            _wkCounter.CancelAsync();

        _omron.plc?.Dispose();
        SetStatus(PLCStatusEnum.Disconnect);
    }

    public void Dispose()
    {
        Disconnect();
        _wkCounter.Dispose();
    }
}
