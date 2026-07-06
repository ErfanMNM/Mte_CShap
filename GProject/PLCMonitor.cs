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

        var consecutiveFailures = 0;

        while (!ct.IsCancellationRequested && _running)
        {
            try
            {
                // 1) Write Ready flag (heartbeat)
                var writeResult = _plc.Write(readyDm, (short)1);

                bool writeOk = writeResult.IsSuccess;
                bool readOk = false;
                int[]? counters = null;
                int deactive = 0;

                //gửi ok thì có nghĩa là PLC đang online.
                if (writeOk)
                {
                    consecutiveFailures = 0;
                    EmitState(PLCConnectionState.Connected, $"PLC online @ {_ip}:{_port};");
                }
                else
                {
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
