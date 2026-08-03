using HslCommunication;
using HslCommunication.Core.Net;
using Serilog;

namespace GProject.PLCHelpers;

/// <summary>
/// PLC Simulator - creates a virtual PLC server using HslCommunication NetSimplifyServer.
/// Allows testing without real hardware PLC.
/// </summary>
public class PLCSimulator : IDisposable
{
    private NetSimplifyServer? _server;
    private readonly short[] _dmRegisters = new short[5000];
    private readonly object _lock = new();

    private int _totalCount = 0;
    private int _passCount = 0;
    private int _failCount = 0;

    public bool IsRunning { get; private set; }
    public event EventHandler<string>? OnLog;

    public void Start(int port = 9600)
    {
        if (IsRunning) return;

        try
        {
            _server = new NetSimplifyServer();
            _server.OnBytesReceived += HandleBytesReceived;
            _server.ServerStart(port);
            IsRunning = true;

            // Initialize default values
            _dmRegisters[16] = 0;  // PLC_Ready_DM_C1 = D16
            _dmRegisters[30] = 0;  // PLC_Total_Count_DM_C1 = D30
            _dmRegisters[34] = 0;  // PLC_Pass_Count_DM_C1 = D34
            _dmRegisters[32] = 0;  // PLC_Fail_Count_DM_C1 = D32
            _dmRegisters[42] = 0;   // PLC_ORDERQTY_DM = D42

            Log.Information("[PLC Simulator] Started on port {Port}", port);
            OnLog?.Invoke(this, $"Simulator started on port {port}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[PLC Simulator] Failed to start on port {Port}", port);
            IsRunning = false;
            throw;
        }
    }

    public void Stop()
    {
        if (!IsRunning) return;

        try
        {
            _server?.ServerClose();
            _server = null;
            IsRunning = false;
            Log.Information("[PLC Simulator] Stopped");
            OnLog?.Invoke(this, "Simulator stopped");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[PLC Simulator] Error stopping");
        }
    }

    public void ResetCounters()
    {
        lock (_lock)
        {
            _totalCount = 0;
            _passCount = 0;
            _failCount = 0;
            _dmRegisters[30] = 0;
            _dmRegisters[34] = 0;
            _dmRegisters[32] = 0;
            OnLog?.Invoke(this, "Counters reset");
        }
    }

    public void SetCounter(string counterType, int value)
    {
        lock (_lock)
        {
            switch (counterType.ToUpper())
            {
                case "TOTAL":
                    _totalCount = value;
                    _dmRegisters[30] = (short)value;
                    break;
                case "PASS":
                    _passCount = value;
                    _dmRegisters[34] = (short)value;
                    break;
                case "FAIL":
                    _failCount = value;
                    _dmRegisters[32] = (short)value;
                    break;
            }
        }
    }

    public void SetRegister(short address, short value)
    {
        lock (_lock)
        {
            if (address >= 0 && address < _dmRegisters.Length)
            {
                _dmRegisters[address] = value;
                OnLog?.Invoke(this, $"Set D{address} = {value}");
            }
        }
    }

    private void HandleBytesReceived(object? sender, HslCommunication.Core.Net.HslProtocol? protocol)
    {
        if (protocol == null) return;

        try
        {
            // Omron FINS protocol handling
            var data = protocol.ProtocolData;
            if (data == null || data.Length < 10) return;

            // FINS command: Read DM (command code 0x0101)
            // Format: Header + Command + Address + Length
            if (data.Length >= 14)
            {
                var command = (data[8] << 8) | data[9];
                var memoryArea = data[10];

                if (command == 0x0101 && memoryArea == 0x82) // DM area read
                {
                    var addressHigh = data[11];
                    var addressLow = data[12];
                    var address = (addressHigh << 8) | addressLow;
                    var wordCount = data[13];

                    // Read DM values
                    var response = new byte[wordCount * 2 + 14];
                    Array.Copy(data, 0, response, 0, 14); // Copy header
                    response[8] = 0; response[9] = 0; // Success response
                    response[10] = 0x82; // DM area

                    lock (_lock)
                    {
                        for (int i = 0; i < wordCount && (address + i) < _dmRegisters.Length; i++)
                        {
                            var value = _dmRegisters[address + i];
                            response[14 + i * 2] = (byte)(value >> 8);
                            response[14 + i * 2 + 1] = (byte)(value & 0xFF);
                        }
                    }

                    _server?.SendBack(response, response.Length);
                    OnLog?.Invoke(this, $"Read D{address}-{address + wordCount - 1}");
                    return;
                }

                // Write DM (command code 0x0102)
                if (command == 0x0102 && memoryArea == 0x82)
                {
                    var addressHigh = data[11];
                    var addressLow = data[12];
                    var address = (addressHigh << 8) | addressLow;
                    var wordCount = data[13];

                    lock (_lock)
                    {
                        for (int i = 0; i < wordCount && (address + i) < _dmRegisters.Length; i++)
                        {
                            var value = (short)((data[14 + i * 2] << 8) | data[14 + i * 2 + 1]);
                            _dmRegisters[address + i] = value;
                            OnLog?.Invoke(this, $"Write D{address + i} = {value}");

                            // Update counters if needed
                            if (address + i == 30) _totalCount = value;
                            if (address + i == 34) _passCount = value;
                            if (address + i == 32) _failCount = value;
                        }
                    }

                    // Send success response
                    var response = new byte[14];
                    Array.Copy(data, 0, response, 0, 12);
                    response[12] = 0; response[13] = 0; // End code: success
                    _server?.SendBack(response, response.Length);
                    return;
                }
            }

            // Simple response for unknown commands
            if (data.Length >= 12)
            {
                var response = new byte[14];
                Array.Copy(data, 0, response, 0, 12);
                response[12] = 0; response[13] = 0;
                _server?.SendBack(response, response.Length);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[PLC Simulator] Error handling request");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
