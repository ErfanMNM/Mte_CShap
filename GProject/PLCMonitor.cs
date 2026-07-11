using HslCommunication;
using HslCommunication.Profinet.Omron;
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
        var readyDm = Environment.GetEnvironmentVariable("PLC_READY_DM") ?? "D16";
        var counterDm = Environment.GetEnvironmentVariable("PLC_TOTAL_COUNT_DM") ?? "D100";
        var deactiveDm = Environment.GetEnvironmentVariable("PLC_DEACTIVE_DM") ?? "D200";

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
        var dm = Environment.GetEnvironmentVariable("PLC_RESULT_DM") ?? DefaultResultDm;
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
        var dm = Environment.GetEnvironmentVariable("PLC_RECIPE_DM") ?? DefaultRecipeDm;
        var r = ReadInt32Safe(dm, 3);
        if (!r.Success || r.Value.Length < 3)
            return new PlcReadResult(false, new int[] { -1, -1, -1 }, r.Error);
        return new PlcReadResult(true, r.Value, "");
    }

    /// <summary>Ghi 3 int32 xuống DM recipe. Returns success/error.</summary>
    public string WriteRecipe(int delayCamera, int delayReject, int rejectStreng)
    {
        var dm = Environment.GetEnvironmentVariable("PLC_RECIPE_DM") ?? DefaultRecipeDm;
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

    public void Stop()
    {
        _running = false;
        try { _cts.Cancel(); } catch { }
    }

    public void Dispose()
    {
        Stop();
        try { _cts.Dispose(); } catch { }
    }
}

public record PLCMonitorState(DateTime At, string Message, string ReadyDm, string CounterDm, string DeactiveDm);
