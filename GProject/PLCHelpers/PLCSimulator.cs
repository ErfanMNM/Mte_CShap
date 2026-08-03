using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GProject.PLCHelpers;

/// <summary>
/// PLC giả lập sử dụng raw TCP sockets.
/// Hỗ trợ giao thức FINS/TCP để tương thích với OmronPLC_Hsl.
/// </summary>
public class PLCSimulator : IDisposable
{
    private TcpListener? _listener;
    private readonly List<TcpClient> _clients = new();
    private readonly object _clientsLock = new();
    private readonly short[] _dmRegisters = new short[5000];
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    
    // Counter simulation
    private int _totalCount = 0;
    private int _passCount = 0;
    private int _failCount = 0;

    public bool IsRunning { get; private set; }
    public int Port { get; private set; } = 9600;

    public event EventHandler<string>? OnLog;

    /// <summary>
    /// Khởi động PLC giả lập trên port chỉ định.
    /// </summary>
    public void Start(int port = 9600)
    {
        if (IsRunning)
        {
            Log.Warning("[PLCSim] Already running on port {Port}", Port);
            return;
        }

        Port = port;

        try
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            
            // Start accepting clients
            _acceptTask = Task.Run(async () => AcceptClientsAsync(_cts.Token));

            IsRunning = true;

            // Khởi tạo một số giá trị mặc định
            InitializeDefaultValues();

            Log.Information("[PLCSim] Simulation started on port {Port}", Port);
            OnLog?.Invoke(this, $"PLC Simulator started on port {Port}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[PLCSim] Failed to start simulation on port {Port}", Port);
            Stop();
            throw;
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
                Log.Debug("[PLCSim] Client connected from {Endpoint}", client.Client.RemoteEndPoint);
                _ = Task.Run(() => HandleClientAsync(client, ct), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Warning("[PLCSim] Accept error: {Error}", ex.Message);
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];

            while (!ct.IsCancellationRequested && client.Connected)
            {
                if (stream.DataAvailable)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (bytesRead > 0)
                    {
                        var request = new byte[bytesRead];
                        Array.Copy(buffer, request, bytesRead);
                        
                        Log.Debug("[PLCSim] Received {Bytes} bytes", bytesRead);
                        
                        var response = HandleRequest(request);
                        if (response.Length > 0)
                        {
                            await stream.WriteAsync(response, 0, response.Length, ct);
                            Log.Debug("[PLCSim] Sent {Bytes} bytes", response.Length);
                        }
                    }
                }
                else
                {
                    await Task.Delay(10, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            Log.Warning("[PLCSim] Client error: {Error}", ex.Message);
        }
        finally
        {
            lock (_clientsLock)
            {
                _clients.Remove(client);
            }
            client.Close();
            Log.Debug("[PLCSim] Client disconnected");
        }
    }

    /// <summary>
    /// Dừng PLC giả lập.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) return;

        try
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener = null;

            lock (_clientsLock)
            {
                foreach (var client in _clients)
                {
                    try { client.Close(); } catch { }
                }
                _clients.Clear();
            }

            IsRunning = false;
            
            Log.Information("[PLCSim] Simulation stopped");
            OnLog?.Invoke(this, "PLC Simulator stopped");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[PLCSim] Error stopping simulation");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Khởi tạo giá trị mặc định cho các thanh ghi quan trọng.
    /// </summary>
    private void InitializeDefaultValues()
    {
        // PLC_Ready_DM_C1 = D16 → Set ready = 1
        _dmRegisters[16] = 1;
        
        // Counter addresses
        // PLC_Total_Count_DM_C1 = D30
        _dmRegisters[30] = 0;
        // PLC_Fail_Count_DM_C1 = D32
        _dmRegisters[32] = 0;
        // PLC_Pass_Count_DM_C1 = D34
        _dmRegisters[34] = 0;
        // PLC_Timeout_Count_DM_C1 = D38
        _dmRegisters[38] = 0;
        // PLC_Line_Speed_DM_C1 = D28
        _dmRegisters[28] = 100;
        
        // Recipe registers (cho camera pipeline)
        // PLC_Delay_Camera_DM_C1 = D0
        _dmRegisters[0] = 1000;
        // PLC_Delay_Reject_DM_C1 = D2
        _dmRegisters[2] = 2000;
        // PLC_Reject_Strength_DM_C1 = D4
        _dmRegisters[4] = 20;
        
        // Status registers
        // PLC_CurrentID_DM_C1 = D2000
        _dmRegisters[2000] = 0;
        // PLC_CurrentStatus_DM_C1 = D2002
        _dmRegisters[2002] = 0;
        
        Log.Information("[PLCSim] Default values initialized");
    }

    /// <summary>
    /// Xử lý FINS TCP request từ OmronPLC_Hsl.
    /// FINS/TCP header format:
    /// - Header: "FINS" (4 bytes) + Length (4 bytes)
    /// - Command: ICF RSV GCT DNA DA1 DA2 SNA SA1 SA2 SID (10 bytes)
    /// - Data: varies by command
    /// </summary>
    private byte[] HandleRequest(byte[] request)
    {
        if (request == null || request.Length < 14)
            return Array.Empty<byte>();

        try
        {
            // Check for FINS/TCP header "FINS"
            if (request[0] != 0x46 || request[1] != 0x49 || request[2] != 0x4E || request[3] != 0x53)
            {
                // Try Modbus TCP format (for compatibility)
                return HandleModbusRequest(request);
            }

            // Get data length (bytes 4-7, big-endian)
            int length = (request[4] << 24) | (request[5] << 16) | (request[6] << 8) | request[7];

            if (request.Length < 14)
                return CreateFinsErrorResponse(0x0001);

            // FINS command at offset 12-13
            byte cmd1 = request[12];
            byte cmd2 = request[13];

            // Read memory area (command 0x0101)
            if (cmd1 == 0x01 && cmd2 == 0x01)
            {
                return HandleFinsReadMemoryArea(request);
            }
            // Write memory area (command 0x0102)
            else if (cmd1 == 0x01 && cmd2 == 0x02)
            {
                return HandleFinsWriteMemoryArea(request);
            }
            // Connection test (command 0x0000)
            else if (cmd1 == 0x00 && cmd2 == 0x00)
            {
                return CreateFinsConnectionTestResponse(request);
            }

            return CreateFinsErrorResponse(0x0004);
        }
        catch (Exception ex)
        {
            Log.Warning("[PLCSim] HandleRequest error: {Error}", ex.Message);
            return CreateFinsErrorResponse(0x0004);
        }
    }

    private byte[] HandleFinsReadMemoryArea(byte[] request)
    {
        try
        {
            if (request.Length < 26)
                return CreateFinsErrorResponse(0x0001);

            // Memory area code (DM = 0x82) at offset 15
            byte areaCode = request[15];
            
            // Address (2 bytes, big-endian) at offset 16-17
            int address = (request[16] << 8) | request[17];
            
            // Bit offset (usually 0 for word access) at offset 18
            byte bitOffset = request[18];
            
            // Word count (2 bytes, big-endian) at offset 20-21
            int wordCount = (request[20] << 8) | request[21];

            Log.Debug("[PLCSim] FINS Read: Area={AreaCode:X2}, Address={Addr}, Count={Count}", 
                areaCode, address, wordCount);

            lock (_dmRegisters)
            {
                // Build FINS response
                var response = new List<byte>();
                
                // FINS header
                response.AddRange(new byte[] { 0x46, 0x49, 0x4E, 0x53 }); // "FINS"
                
                // Response data length = 2 (response code) + wordCount * 2
                int responseDataLen = 2 + wordCount * 2;
                response.Add((byte)((responseDataLen >> 24) & 0xFF));
                response.Add((byte)((responseDataLen >> 16) & 0xFF));
                response.Add((byte)((responseDataLen >> 8) & 0xFF));
                response.Add((byte)(responseDataLen & 0xFF));
                
                // Response code (0000 = success)
                response.AddRange(new byte[] { 0x00, 0x00 });
                
                // Copy response header from request (client info)
                if (request.Length >= 24)
                {
                    response.Add(request[9]);  // SNA
                    response.Add(request[10]); // SA1
                    response.Add(request[11]); // SA2
                    response.Add(request[14]); // SID
                }
                
                // Data
                for (int i = 0; i < wordCount && (address + i) < _dmRegisters.Length; i++)
                {
                    short value = _dmRegisters[address + i];
                    response.Add((byte)((value >> 8) & 0xFF)); // High byte
                    response.Add((byte)(value & 0xFF));       // Low byte
                }

                return response.ToArray();
            }
        }
        catch (Exception ex)
        {
            Log.Warning("[PLCSim] HandleFinsReadMemoryArea error: {Error}", ex.Message);
            return CreateFinsErrorResponse(0x0004);
        }
    }

    private byte[] HandleFinsWriteMemoryArea(byte[] request)
    {
        try
        {
            if (request.Length < 26)
                return CreateFinsErrorResponse(0x0001);

            // Memory area code (DM = 0x82) at offset 15
            byte areaCode = request[15];
            
            // Address (2 bytes, big-endian) at offset 16-17
            int address = (request[16] << 8) | request[17];
            
            // Bit offset at offset 18
            byte bitOffset = request[18];
            
            // Word count (2 bytes, big-endian) at offset 20-21
            int wordCount = (request[20] << 8) | request[21];

            Log.Debug("[PLCSim] FINS Write: Area={AreaCode:X2}, Address={Addr}, Count={Count}", 
                areaCode, address, wordCount);

            lock (_dmRegisters)
            {
                // Data starts at offset 24
                int dataOffset = 24;
                for (int i = 0; i < wordCount && (address + i) < _dmRegisters.Length && (dataOffset + i * 2 + 1) < request.Length; i++)
                {
                    short value = (short)((request[dataOffset + i * 2] << 8) | request[dataOffset + i * 2 + 1]);
                    _dmRegisters[address + i] = value;
                    
                    Log.Debug("[PLCSim] Wrote D{Addr} = {Value}", address + i, value);
                }

                // Build success response
                var response = new List<byte>();
                response.AddRange(new byte[] { 0x46, 0x49, 0x4E, 0x53 }); // "FINS"
                
                // Response data length = 6 (response code + header copy)
                response.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x06 });
                
                // Response code = success
                response.AddRange(new byte[] { 0x00, 0x00 });
                
                // Copy header from request
                if (request.Length >= 26)
                {
                    response.Add(request[9]);  // SNA
                    response.Add(request[10]); // SA1
                    response.Add(request[11]); // SA2
                    response.Add(request[14]); // SID
                }

                return response.ToArray();
            }
        }
        catch (Exception ex)
        {
            Log.Warning("[PLCSim] HandleFinsWriteMemoryArea error: {Error}", ex.Message);
            return CreateFinsErrorResponse(0x0004);
        }
    }

    private byte[] HandleModbusRequest(byte[] request)
    {
        // Simple Modbus TCP support for compatibility
        // Modbus header: Transaction ID (2) + Protocol ID (2) + Length (2) + Unit ID (1) + Function code (1) + Data
        
        if (request.Length < 8)
            return Array.Empty<byte>();

        // Function code 0x03 = Read Holding Registers
        if (request[7] == 0x03)
        {
            int address = (request[8] << 8) | request[9];
            int count = (request[10] << 8) | request[11];

            lock (_dmRegisters)
            {
                var response = new List<byte>();
                // Header
                response.Add(request[0]);
                response.Add(request[1]);
                response.Add(request[2]);
                response.Add(request[3]);
                // Length (will set later)
                int lenPos = response.Count;
                response.Add(0);
                response.Add(0);
                // Unit ID
                response.Add(request[6]);
                // Function code
                response.Add(0x03);
                // Byte count
                response.Add((byte)(count * 2));
                
                // Data
                for (int i = 0; i < count && (address + i) < _dmRegisters.Length; i++)
                {
                    short value = _dmRegisters[address + i];
                    response.Add((byte)((value >> 8) & 0xFF));
                    response.Add((byte)(value & 0xFF));
                }

                // Set length
                int length = response.Count - 6;
                response[lenPos] = (byte)((length >> 8) & 0xFF);
                response[lenPos + 1] = (byte)(length & 0xFF);

                return response.ToArray();
            }
        }
        // Function code 0x06 = Write Single Register
        else if (request[7] == 0x06)
        {
            int address = (request[8] << 8) | request[9];
            short value = (short)((request[10] << 8) | request[11]);
            
            lock (_dmRegisters)
            {
                if (address < _dmRegisters.Length)
                    _dmRegisters[address] = value;
            }

            // Echo back the request as success response
            return request;
        }

        return Array.Empty<byte>();
    }

    private byte[] CreateFinsConnectionTestResponse(byte[] request)
    {
        // Echo back with zero response code
        if (request.Length >= 14)
        {
            var response = new List<byte>();
            response.AddRange(new byte[] { 0x46, 0x49, 0x4E, 0x53 }); // "FINS"
            response.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x0A }); // Length = 10
            response.AddRange(new byte[] { 0x00, 0x00 }); // Response code = success
            // Copy header
            response.Add(request[9]);
            response.Add(request[10]);
            response.Add(request[11]);
            response.Add(request[14]);
            // 4 bytes of zeros (connection test data)
            response.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            
            return response.ToArray();
        }
        return Array.Empty<byte>();
    }

    private byte[] CreateFinsErrorResponse(byte errorCode)
    {
        var response = new List<byte>();
        response.AddRange(new byte[] { 0x46, 0x49, 0x4E, 0x53 }); // "FINS"
        response.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x02 }); // Length = 2
        response.Add((byte)((errorCode >> 8) & 0xFF));
        response.Add((byte)(errorCode & 0xFF));
        return response.ToArray();
    }

    /// <summary>
    /// Đọc giá trị từ thanh ghi DM.
    /// </summary>
    public short ReadDM(int address)
    {
        lock (_dmRegisters)
        {
            return address >= 0 && address < _dmRegisters.Length 
                ? _dmRegisters[address] 
                : (short)0;
        }
    }

    /// <summary>
    /// Ghi giá trị vào thanh ghi DM.
    /// </summary>
    public void WriteDM(int address, short value)
    {
        lock (_dmRegisters)
        {
            if (address >= 0 && address < _dmRegisters.Length)
            {
                _dmRegisters[address] = value;
                Log.Debug("[PLCSim] WriteDM D{Addr} = {Value}", address, value);
            }
        }
    }

    /// <summary>
    /// Reset tất cả counters về 0.
    /// </summary>
    public void ResetCounters()
    {
        lock (_dmRegisters)
        {
            _totalCount = 0;
            _passCount = 0;
            _failCount = 0;
            
            _dmRegisters[30] = 0; // Total
            _dmRegisters[32] = 0; // Fail
            _dmRegisters[34] = 0; // Pass
            
            Log.Information("[PLCSim] Counters reset");
            OnLog?.Invoke(this, "Counters reset to 0");
        }
    }

    /// <summary>
    /// Tăng pass counter.
    /// </summary>
    public void IncrementPass()
    {
        lock (_dmRegisters)
        {
            _totalCount++;
            _passCount++;
            _dmRegisters[30] = (short)_totalCount;
            _dmRegisters[34] = (short)_passCount;
        }
    }

    /// <summary>
    /// Tăng fail counter.
    /// </summary>
    public void IncrementFail()
    {
        lock (_dmRegisters)
        {
            _totalCount++;
            _failCount++;
            _dmRegisters[30] = (short)_totalCount;
            _dmRegisters[32] = (short)_failCount;
        }
    }

    /// <summary>
    /// Set trạng thái ready của PLC.
    /// </summary>
    public void SetReady(bool ready)
    {
        lock (_dmRegisters)
        {
            // PLC_Ready_DM_C1 = D16
            _dmRegisters[16] = ready ? (short)1 : (short)0;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
