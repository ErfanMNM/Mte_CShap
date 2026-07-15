using HslCommunication;
using HslCommunication.Profinet.Omron;
using GProject.PLCHelpers;
using GProject.ProductionOrderHelpers;
using Serilog;

namespace GProject;

public enum PLCConnectionState
{
    Disconnected,
    Connected,
    Reconnecting,
}

public class PLCMonitor : IDisposable
{
    private readonly string _ip;
    private readonly int _port;
    private readonly int _pollMs;

    private readonly OmronFinsUdp _plc;
    private readonly CancellationTokenSource _cts = new();

    private volatile bool _running;

    public event EventHandler<(PLCConnectionState state, string message)>? StateChanged;

    public PLCMonitorState? LastError { get; private set; }

    public PLCConnectionState State { get; private set; } = PLCConnectionState.Disconnected;

    public const string DefaultResultDm = "D300";
    public const string DefaultTimeoutIdDm = "D200";
    public const string DefaultTimeoutStatusDm = "D202";
    public const string DefaultRecipeDm = "D400"; // 3 int32: DelayCamera, DelayReject, RejectStreng

    public record PlcReadResult(bool Success, int[] Value, string Error);

    public record PlcReadAnyResult(bool Success, GProject.ProductionOrderHelpers.PlcDataType DataType, string Value, string Error);

    public PLCMonitor()
    {
        _ip = Environment.GetEnvironmentVariable("PLC_IP") ?? "127.0.0.1";
        _port = int.TryParse(Environment.GetEnvironmentVariable("PLC_PORT"), out var p) ? p : 9600;
        _pollMs = int.TryParse(Environment.GetEnvironmentVariable("PLC_POLL_MS"), out var ms) && ms > 50 ? ms : 300;

        _plc = new OmronFinsUdp(_ip, _port);
        _plc.SA1 = 1;
        _plc.GCT = 2;
        _plc.DA1 = 0;
        _plc.SID = 0;
        _plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
        _plc.ByteTransform.IsStringReverseByteWord = true;
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        _ = Task.Run(() => PollLoop(_cts.Token));
        Log.Information("[PLCMonitor] Started. Target PLC: {Ip}:{Port} every {Poll}ms", _ip, _port, _pollMs);
    }

    private void PollLoop(CancellationToken ct)
    {
        var readyDm = Resolve("PLC_READY_DM", "PLC_Ready_DM", "D16");
        var counterDm = Resolve("PLC_TOTAL_COUNT_DM", "PLC_Total_Count_DM", "D100");
        var deactiveDm = Resolve("PLC_DEACTIVE_DM", "PLC_Deactive_DM", "D200");

        int consecutiveFailures = 0;

        while (!ct.IsCancellationRequested && _running)
        {
            try
            {
                // 1) Write Ready flag (heartbeat)
                var writeResult = _plc.Write(readyDm, (short)1);

                bool writeOk = writeResult.IsSuccess;

                if (writeOk)
                {
                    consecutiveFailures = 0;
                    EmitState(PLCConnectionState.Connected, $"PLC online @ {_ip}:{_port};");

                    // 2) Doc counter tu PLC
                    try
                    {
                        var readResult = _plc.ReadInt32(counterDm, 1);
                        if (readResult.IsSuccess && readResult.Content.Length > 0)
                        {
                            int totalCount = readResult.Content[0];
                            Production.ProductionStateMachine.Instance.UpdateActiveCounterTotal(totalCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("[PLCMonitor] Failed to read counter: {Ex}", ex.Message);
                    }
                }
                else
                {
                    consecutiveFailures++;
                    EmitState(PLCConnectionState.Disconnected, $"PLC offline ({consecutiveFailures} consecutive fails)");
                }

                Thread.Sleep(_pollMs);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[PLCMonitor] Unexpected error in poll loop");
                Thread.Sleep(_pollMs);
            }
        }

        _running = false;
    }

    private void EmitState(PLCConnectionState newState, string message)
    {
        if (State != newState)
        {
            State = newState;
            Log.Information("[PLCMonitor] State: {State} - {Msg}", newState, message);
            try
            {
                StateChanged?.Invoke(this, (newState, message));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[PLCMonitor] StateChanged handler threw");
            }
        }
    }

    /// <summary>
    /// Ghi response camera (1/2/0) xuống DM 16-bit. Fire-and-forget — KHÔNG await,
    /// phải hoàn tất trong vài chục ms để khớp cycle PLC.
    /// </summary>
    public void WriteResultFireAndForget(short value)
    {
        var dm = Resolve("PLC_RESULT_DM", "PLC_Result_DM", DefaultResultDm);
        try
        {
            var r = _plc.Write(dm, value);
            if (!r.IsSuccess)
                Log.Warning("[PLCMonitor] Write {Dm}={Value} failed: {Err}",
                            dm, value, r.Message);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[PLCMonitor] Write {Dm}={Value} threw", dm, value);
        }
    }

    /// <summary>Đọc 1 vùng int32 từ DM, không throw.</summary>
    public PlcReadResult ReadInt32Safe(string address, ushort length)
    {
        try
        {
            var r = _plc.ReadInt32(address, length);
            return new PlcReadResult(r.IsSuccess,
                                     r.IsSuccess ? r.Content : Array.Empty<int>(),
                                     r.IsSuccess ? "" : r.Message);
        }
        catch (Exception ex)
        {
            return new PlcReadResult(false, Array.Empty<int>(), ex.Message);
        }
    }

    /// <summary>Đọc recipe (3 int32) từ DM. KHÔNG throw. Returns default zeros nếu lỗi.</summary>
    public PlcReadResult ReadRecipe()
    {
        var dm = Resolve("PLC_RECIPE_DM", "PLC_Recipe_DM", DefaultRecipeDm);
        var r = ReadInt32Safe(dm, 3);
        if (!r.Success || r.Value.Length < 3)
            return new PlcReadResult(false, new int[] { -1, -1, -1 }, r.Error);
        return new PlcReadResult(true, r.Value, "");
    }

    /// <summary>Ghi 3 int32 xuống DM recipe. Returns success/error.</summary>
    public string WriteRecipe(int delayCamera, int delayReject, int rejectStreng)
    {
        var dm = Resolve("PLC_RECIPE_DM", "PLC_Recipe_DM", DefaultRecipeDm);
        try
        {
            var r = _plc.Write(dm, new int[] { delayCamera, delayReject, rejectStreng });
            if (!r.IsSuccess) return r.Message ?? "Write failed";
            return "";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>Đọc 1 ô tuỳ biến theo data type. KHÔNG throw.</summary>
    public PlcReadAnyResult ReadRegister(string address, string dataType)
    {
        try
        {
            var t = PlcDataTypeMap.Parse(dataType);
            switch (t)
            {
                case PlcDataType.Int16:
                    {
                        var r = _plc.ReadInt16(address, 1);
                        return new PlcReadAnyResult(r.IsSuccess, PlcDataType.Int16,
                                                    r.IsSuccess ? r.Content[0].ToString() : "",
                                                    r.IsSuccess ? "" : r.Message);
                    }
                case PlcDataType.Int32:
                    {
                        var r = _plc.ReadInt32(address, 1);
                        return new PlcReadAnyResult(r.IsSuccess, PlcDataType.Int32,
                                                    r.IsSuccess ? r.Content[0].ToString() : "",
                                                    r.IsSuccess ? "" : r.Message);
                    }
                case PlcDataType.Float:
                    {
                        var r = _plc.ReadFloat(address, 1);
                        return new PlcReadAnyResult(r.IsSuccess, PlcDataType.Float,
                                                    r.IsSuccess ? r.Content[0].ToString(System.Globalization.CultureInfo.InvariantCulture) : "",
                                                    r.IsSuccess ? "" : r.Message);
                    }
                case PlcDataType.String:
                    {
                        // Đọc tối đa 32 word (64 char) cho string.
                        var r = _plc.ReadString(address, 64);
                        return new PlcReadAnyResult(r.IsSuccess, PlcDataType.String,
                                                    r.IsSuccess ? r.Content : "",
                                                    r.IsSuccess ? "" : r.Message);
                    }
            }
        }
        catch (Exception ex)
        {
            return new PlcReadAnyResult(false, PlcDataType.Int32, "", ex.Message);
        }
        return new PlcReadAnyResult(false, PlcDataType.Int32, "", "Unknown");
    }

    /// <summary>Ghi 1 ô tuỳ biến. Trả về "" nếu OK, message lỗi nếu thất bại.</summary>
    public string WriteRegister(string address, string dataType, string value)
    {
        try
        {
            var t = PlcDataTypeMap.Parse(dataType);
            switch (t)
            {
                case PlcDataType.Int16:
                    {
                        if (!short.TryParse(value, out var s)) return $"Không parse được int16: {value}";
                        var r = _plc.Write(address, s);
                        return r.IsSuccess ? "" : (r.Message ?? "Write failed");
                    }
                case PlcDataType.Int32:
                    {
                        if (!int.TryParse(value, out var s)) return $"Không parse được int32: {value}";
                        var r = _plc.Write(address, s);
                        return r.IsSuccess ? "" : (r.Message ?? "Write failed");
                    }
                case PlcDataType.Float:
                    {
                        if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f))
                            return $"Không parse được float: {value}";
                        var r = _plc.Write(address, f);
                        return r.IsSuccess ? "" : (r.Message ?? "Write failed");
                    }
                case PlcDataType.String:
                    {
                        var r = _plc.Write(address, value ?? "");
                        return r.IsSuccess ? "" : (r.Message ?? "Write failed");
                    }
            }
            return "Unknown type";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public void Stop()
    {
        _running = false;
        try { _cts.Cancel(); } catch { }
    }

    /// <summary>
    /// Ưu tiên env var, sau đó Google Sheet (PLCAddressWithGoogleSheetHelper),
    /// cuối cùng fallback về hard-code. Không throw — đảm bảo poll loop không crash
    /// khi helper chưa Init hoặc sheet thiếu key.
    /// </summary>
    private static string Resolve(string envName, string sheetKey, string fallback)
    {
        var env = Environment.GetEnvironmentVariable(envName);
        if (!string.IsNullOrEmpty(env)) return env;
        try
        {
            return PLCAddressWithGoogleSheetHelper.Get(sheetKey);
        }
        catch (Exception ex)
        {
            Log.Warning("[PLCMonitor] Address key '{Key}' missing in Google Sheet, fallback to {Fallback}: {Ex}",
                        sheetKey, fallback, ex.Message);
            return fallback;
        }
    }

    public void Dispose()
    {
        Stop();
        try { _cts.Dispose(); } catch { }
    }
}

public record PLCMonitorState(DateTime At, string Message, string ReadyDm, string CounterDm, string DeactiveDm);
