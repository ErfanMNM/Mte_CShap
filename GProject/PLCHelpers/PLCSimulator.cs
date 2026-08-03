using System.Net;
using System.Net.Sockets;
using Serilog;

namespace GProject.PLCHelpers;

/// <summary>
/// PLC Simulator - creates a virtual PLC server using TCP sockets.
/// Responds to Omron FINS UDP protocol over TCP for testing without real hardware PLC.
/// </summary>
public class PLCSimulator : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    private readonly List<TcpClient> _clients = new();
    private readonly object _clientsLock = new();

    private readonly short[] _dmRegisters = new short[5000];
    private readonly object _registerLock = new();

    private int _totalCount = 0;
    private int _passCount = 0;
    private int _failCount = 0;

    public bool IsRunning { get; private set; }
    public event EventHandler<string>? OnLog;

    public PLCSimulator()
    {
        // Initialize default values for common registers
        _dmRegisters[16] = 0;  // PLC_Ready_DM_C1 = D16
        _dmRegisters[30] = 0;  // PLC_Total_Count_DM_C1 = D30
        _dmRegisters[34] = 0;  // PLC_Pass_Count_DM_C1 = D34
        _dmRegisters[32] = 0;  // PLC_Fail_Count_DM_C1 = D32
        _dmRegisters[42] = 0;   // PLC_ORDERQTY_DM = D42
    }

    public void Start(int port = 9600)
    {
        if (IsRunning) return;

        try
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            IsRunning = true;

            _acceptTask = Task.Run(() => AcceptClientsAsync(_cts.Token));
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
            _cts?.Cancel();
            _listener?.Stop();

            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    try { client.Close(); } catch { }
                }
                _clients.Clear();
            }

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
        lock (_registerLock)
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
        lock (_registerLock)
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
        lock (_registerLock)
        {
            if (address >= 0 && address < _dmRegisters.Length)
            {
                _dmRegisters[address] = value;
                OnLog?.Invoke(this, $"Set D{address} = {value}");
            }
        }
    }

    public short GetRegister(short address)
    {
        lock (_registerLock)
        {
            if (address >= 0 && address < _dmRegisters.Length)
                return _dmRegisters[address];
            return 0;
        }
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener != null)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(ct);
                lock (_clientsLock)
                {
                    _clients.Add(client);
                }

                _ = Task.Run(() => HandleClientAsync(client, ct));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[PLC Simulator] Accept client error");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        string clientInfo = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        OnLog?.Invoke(this, $"Client connected: {clientInfo}");

        try
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            while (!ct.IsCancellationRequested && client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (bytesRead == 0) break;

                // Process FINS UDP packet (wrapped in TCP)
                var response = ProcessFinsPacket(buffer, bytesRead);
                if (response != null && response.Length > 0)
                {
                    await stream.WriteAsync(response, 0, response.Length, ct);
                }
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke(this, $"Client error: {ex.Message}");
        }
        finally
        {
            lock (_clientsLock)
            {
                _clients.Remove(client);
            }
            client.Close();
            OnLog?.Invoke(this, $"Client disconnected: {clientInfo}");
        }
    }

    private byte[]? ProcessFinsPacket(byte[] data, int length)
    {
        // FINS/UDP over TCP format (simplified for Omron)
        // ICF(1) + RSV(1) + GCT(1) + DNA(1) + DA1(1) + DA2(1) + SNA(1) + SA1(1) + SA2(1) + SID(1) + MRC(2) + SRC(2) + [DATA]

        if (length < 14) return null;

        // Check if it's a FINS command (ICF = 0x80 or 0x00)
        if (data[0] != 0x80 && data[0] != 0x00) return null;

        byte MRC = data[12];
        byte SRC = data[13];

        // Read DM area command (FINS 01 01)
        if (MRC == 0x01 && SRC == 0x01)
        {
            return HandleReadDM(data, length);
        }

        // Write DM area command (FINS 01 02)
        if (MRC == 0x01 && SRC == 0x02)
        {
            return HandleWriteDM(data, length);
        }

        // Unsupported command - return error response
        return CreateErrorResponse(data, 0x00, 0x01);
    }

    private byte[] HandleReadDM(byte[] data, int length)
    {
        // FINS Read DM format:
        // Header (14) + Memory area code (1) + Address (2) + Bit/Word (1) + Word count (2)

        if (length < 20) return CreateErrorResponse(data, 0x00, 0x01);

        byte memoryArea = data[14]; // 0x82 = DM area
        if (memoryArea != 0x82) return CreateErrorResponse(data, 0x00, 0x01);

        // Parse address (big-endian)
        short address = (short)((data[15] << 8) | data[16]);

        // Word count (big-endian)
        short wordCount = (short)((data[18] << 8) | data[19]);
        if (wordCount <= 0 || wordCount > 100) wordCount = 1;

        // Build response
        // Response format: Header (14) + Response code (2) + Data (wordCount * 2)
        var response = new byte[16 + wordCount * 2];

        // Copy original header
        Array.Copy(data, 0, response, 0, 14);

        // Response code: 00 00 = Normal completion
        response[14] = 0x00;
        response[15] = 0x00;

        lock (_registerLock)
        {
            for (int i = 0; i < wordCount; i++)
            {
                short value = (address + i < _dmRegisters.Length) ? _dmRegisters[address + i] : (short)0;
                response[16 + i * 2] = (byte)(value >> 8);      // High byte
                response[16 + i * 2 + 1] = (byte)(value & 0xFF); // Low byte
            }
        }

        OnLog?.Invoke(this, $"READ D{address} x{wordCount}");
        return response;
    }

    private byte[] HandleWriteDM(byte[] data, int length)
    {
        // FINS Write DM format:
        // Header (14) + Memory area code (1) + Address (2) + Bit/Word (1) + Word count (2) + Data (wordCount * 2)

        if (length < 20) return CreateErrorResponse(data, 0x00, 0x01);

        byte memoryArea = data[14];
        if (memoryArea != 0x82) return CreateErrorResponse(data, 0x00, 0x01);

        short address = (short)((data[15] << 8) | data[16]);
        short wordCount = (short)((data[18] << 8) | data[19]);

        if (wordCount <= 0 || length < 20 + wordCount * 2)
            return CreateErrorResponse(data, 0x00, 0x01);

        lock (_registerLock)
        {
            for (int i = 0; i < wordCount; i++)
            {
                short value = (short)((data[20 + i * 2] << 8) | data[20 + i * 2 + 1]);
                if (address + i < _dmRegisters.Length)
                {
                    _dmRegisters[address + i] = value;
                }
            }
        }

        // Success response
        var response = new byte[16];
        Array.Copy(data, 0, response, 0, 14);
        response[14] = 0x00;
        response[15] = 0x00;

        OnLog?.Invoke(this, $"WRITE D{address} x{wordCount}");
        return response;
    }

    private byte[] CreateErrorResponse(byte[] original, byte mrc, byte src)
    {
        var response = new byte[16];
        Array.Copy(original, 0, response, 0, 14);
        response[14] = mrc;
        response[15] = src;
        return response;
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}
