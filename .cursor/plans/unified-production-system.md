---
name: unified-production-system
overview: Xây dựng hệ thống sản xuất hoàn chỉnh: (A) Tối ưu State Machine 100ms->10ms + Queue serialization tránh race condition + API quét thùng cho Android PDA; (B) Broadcast production state qua WebSocket real-time + sửa API endpoint + kết nối PLC counter, đảm bảo Frontend luôn nhận được trạng thái thực của hệ thống.
todos: []
isProject: false
---

# Kế hoạch: Unified Production System

## Bối cảnh

Hệ thống gồm 2 phần chính chạy song song:
- **Phần A (Carton PDA):** API quét thùng cho Android PDA + Queue serialization tránh race condition
- **Phần B (State Broadcast):** WebSocket broadcast production state real-time + sửa API + kết nối PLC counter

## Architecture Diagram

```mermaid
flowchart TD
    subgraph Backend["Backend (GProject)"]
        SM["ProductionStateMachine\nRunLoop (10ms)"]
        SM -->|SetState()| BH["ProductionHub\n/ws/production"]
        SM -->|poll 2s| API["HandleGetProductionStatus()\n/api/production/status"]
        SM -->|PLC read 300ms| PLC["PLCMonitor\nPollLoop"]
        PLC -->|update| AC["ActiveCounter\nPackageCounter"]
        SM -->|use queue| CWQ["CartonWriteQueue\nSingleton per PO"]
        CWQ -->|write| DB["Carton DB\n(Carton + CartonCode)"]
    end

    subgraph AndroidPDA["Android PDA"]
        AP["ApiClient.kt\nCartonScan/Info/CurrentPO"]
    end

    subgraph Frontend["React Frontend"]
        WS["useProductionWebSocket\n/ws/production"]
        POLL["productionApi.getStatus()\n/api/production/status (2s)"]
        PV["ProductionView.tsx\n16 states + warning"]
    end

    BH -->|WebSocket| WS
    API -->|REST| POLL
    AP -->|REST| POST["POST /api/carton/scan\nGET /api/carton/info\nGET /api/carton/current-po"]

    style BH fill:#bbf,color:#000
    style SM fill:#fbb,color:#000
    style CWQ fill:#bbf,color:#000
```

---

## Phần A: Carton PDA API + Queue Serialization

### A.1. Tối ưu State Machine 100ms -> 10ms

**File:** `GProject/Production/ProductionStateMachine.cs` line 136

`ProcessState()` chỉ đọc Dictionary in-memory + set state, không write DB — hoàn toàn an toàn để chạy nhanh. CPU tăng ~10x số lần gọi nhưng mỗi call rất nhẹ (< 1ms). PLCMonitor có loop riêng không bị ảnh hưởng.

```csharp
// Line 136: đổi
try { Task.Delay(10, token).Wait(token); }  // 10ms thay vì 100ms
```

### A.2. Database Schema

**File:** `GProject/ProductionOrderHelpers/Config.cs`

Thêm bảng `CartonCode` vào `SQL_CREATE_CARTON`:

```sql
CREATE TABLE IF NOT EXISTS CartonCode (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    MachineName TEXT NOT NULL,
    CartonCode TEXT NOT NULL,
    CartonIndex INTEGER NOT NULL DEFAULT 0,
    ScanAt TEXT NOT NULL,
    Mode TEXT NOT NULL DEFAULT 'scan',
    Result TEXT NOT NULL DEFAULT ''
);
```

### A.3. CartonWriteQueue — Singleton Queue Serialization

**File:** `GProject/ProductionOrderHelpers/CartonWriteQueue.cs` (MỚI)

Thay vì write trực tiếp vào DB (có race condition), tất cả write đi qua 1 BlockingCollection duy nhất per PO. Background thread consume tuần tự, đảm bảo không bao giờ 2 write cùng lúc vào cùng 1 Carton DB.

```csharp
using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.ProductionOrderHelpers
{
    public enum CartonWriteType { ScanCarton, StartCarton, CompleteCarton, ResetCarton }

    public class CartonWriteTask
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public CartonWriteType Type { get; init; }
        public string OrderNo { get; init; } = "";
        public string CartonCode { get; init; } = "";
        public int? CartonId { get; init; }
        public string MachineName { get; init; } = "";
        public string ScannedAt { get; init; } = "";
        public string Mode { get; init; } = "scan";
        internal TaskCompletionSource<CartonWriteResult> Completion { get; } = new();
        public Task<CartonWriteResult> Task => Completion.Task;
    }

    public class CartonWriteResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public string Status { get; init; } = "ERR";  // OK / WARN / ERR
        public int CartonIndex { get; init; }
        public int ProductCount { get; init; }
        public string ActivateDate { get; init; } = "";
    }

    public class CartonWriteQueue : IDisposable
    {
        private readonly BlockingCollection<CartonWriteTask> _queue = new();
        private readonly Task _consumer;
        private readonly string _dbPath;
        private readonly string _connStr;
        private bool _disposed;

        public CartonWriteQueue(string dbPath)
        {
            _dbPath = dbPath;
            _connStr = $"Data Source={dbPath}";
            _consumer = Task.Factory.StartNew(ConsumeLoop, TaskCreationOptions.LongRunning);
            Log.Information("[CartonWriteQueue] Started for {Path}", dbPath);
        }

        public void Enqueue(CartonWriteTask task)
        {
            if (_disposed) { task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = "Queue disposed" }); return; }
            if (!_queue.TryAdd(task))
                task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = "Queue full" });
        }

        public Task<CartonWriteResult> EnqueueAsync(CartonWriteTask task)
        {
            Enqueue(task);
            return task.Task;
        }

        private void ConsumeLoop()
        {
            foreach (var task in _queue.GetConsumingEnumerable())
            {
                try { ProcessTask(task); }
                catch (Exception ex)
                {
                    Log.Error(ex, "[CartonWriteQueue] ProcessTask failed");
                    task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = ex.Message });
                }
            }
        }

        private void ProcessTask(CartonWriteTask task)
        {
            using var con = new SqliteConnection(_connStr);
            con.Open();
            using var tx = con.BeginTransaction();

            try
            {
                var result = task.Type switch
                {
                    CartonWriteType.ScanCarton => ProcessScanCarton(con, tx, task),
                    CartonWriteType.StartCarton => ProcessStartCarton(con, tx, task),
                    CartonWriteType.CompleteCarton => ProcessCompleteCarton(con, tx, task),
                    CartonWriteType.ResetCarton => ProcessResetCarton(con, tx, task),
                    _ => new CartonWriteResult { Success = false, Message = "Unknown type" }
                };

                if (result.Success) tx.Commit();
                else tx.Rollback();

                task.Completion.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                task.Completion.TrySetResult(new CartonWriteResult { Success = false, Message = ex.Message });
            }
        }

        private CartonWriteResult ProcessScanCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            // 1. Tim thung trong Carton
            using var sel = new SqliteCommand("SELECT ID, Start_Datetime FROM Carton WHERE cartonCode = @cc", con, tx);
            sel.Parameters.AddWithValue("@cc", task.CartonCode);
            using var r = sel.ExecuteReader();
            if (!r.Read())
                return new CartonWriteResult { Success = false, Status = "ERR", Message = $"Khong tim thay thung: {task.CartonCode}" };

            int cartonId = r.GetInt32(0);
            string startDt = r.GetString(1);
            r.Close();

            // 2. Neu chua Start -> bat dau thung
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (startDt == "0")
            {
                using var upd = new SqliteCommand(
                    "UPDATE Carton SET Start_Datetime = @dt, ActivateUser = @user WHERE ID = @id", con, tx);
                upd.Parameters.AddWithValue("@dt", now);
                upd.Parameters.AddWithValue("@user", task.MachineName);
                upd.Parameters.AddWithValue("@id", cartonId);
                upd.ExecuteNonQuery();
            }

            // 3. Tinh CartonIndex = STT thung trong PO
            using var idxCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Carton WHERE ID <= @id", con, tx);
            idxCmd.Parameters.AddWithValue("@id", cartonId);
            int cartonIndex = Convert.ToInt32(idxCmd.ExecuteScalar());

            // 4. Log vao CartonCode
            string status = startDt == "0" ? "OK" : "WARN";
            string result = startDt == "0" ? "Bat dau thanh cong" : "Thung da bat dau";
            using var ins = new SqliteCommand(@"
                INSERT INTO CartonCode (MachineName, CartonCode, CartonIndex, ScanAt, Mode, Result)
                VALUES (@mn, @cc, @ci, @sa, @md, @rs)", con, tx);
            ins.Parameters.AddWithValue("@mn", task.MachineName);
            ins.Parameters.AddWithValue("@cc", task.CartonCode);
            ins.Parameters.AddWithValue("@ci", cartonIndex);
            ins.Parameters.AddWithValue("@sa", task.ScannedAt);
            ins.Parameters.AddWithValue("@md", task.Mode);
            ins.Parameters.AddWithValue("@rs", result);
            ins.ExecuteNonQuery();

            // 5. Neu mode=info -> dem san pham trong thung
            int productCount = 0;
            string activateDate = now;
            if (task.Mode == "info")
            {
                var poDb = Config.GetUniqueCodesPath(task.OrderNo);
                using var cntCon = new SqliteConnection($"Data Source={poDb}");
                cntCon.Open();
                using var cntCmd = new SqliteCommand(
                    "SELECT COUNT(*), MIN(PackingDate) FROM UniqueCodes WHERE cartonCode = @cc AND Status = 1", cntCon);
                cntCmd.Parameters.AddWithValue("@cc", task.CartonCode);
                using var cntR = cntCmd.ExecuteReader();
                if (cntR.Read())
                {
                    productCount = cntR.IsDBNull(0) ? 0 : cntR.GetInt32(0);
                    activateDate = cntR.IsDBNull(1) ? now : cntR.GetString(1);
                }
            }

            return new CartonWriteResult
            {
                Success = true,
                Status = status,
                Message = result,
                CartonIndex = cartonIndex,
                ProductCount = productCount,
                ActivateDate = activateDate
            };
        }

        private CartonWriteResult ProcessStartCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Message = "CartonId required" };

            using var upd = new SqliteCommand(
                "UPDATE Carton SET Start_Datetime = @dt, ActivateUser = @user WHERE ID = @id", con, tx);
            upd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            upd.Parameters.AddWithValue("@user", task.MachineName);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            return rows > 0
                ? new CartonWriteResult { Success = true, Status = "OK", Message = $"Start carton {task.CartonId}" }
                : new CartonWriteResult { Success = false, Message = $"Khong tim thay carton {task.CartonId}" };
        }

        private CartonWriteResult ProcessCompleteCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Message = "CartonId required" };

            using var upd = new SqliteCommand(
                "UPDATE Carton SET Completed_Datetime = @dt, ActivateUser = @user WHERE ID = @id", con, tx);
            upd.Parameters.AddWithValue("@dt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            upd.Parameters.AddWithValue("@user", task.MachineName);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            return rows > 0
                ? new CartonWriteResult { Success = true, Status = "OK", Message = $"Complete carton {task.CartonId}" }
                : new CartonWriteResult { Success = false, Message = $"Khong tim thay carton {task.CartonId}" };
        }

        private CartonWriteResult ProcessResetCarton(SqliteConnection con, SqliteTransaction tx, CartonWriteTask task)
        {
            if (!task.CartonId.HasValue)
                return new CartonWriteResult { Success = false, Message = "CartonId required" };

            using var upd = new SqliteCommand(
                "UPDATE Carton SET cartonCode = '0', Start_Datetime = '0', Completed_Datetime = '0' WHERE ID = @id", con, tx);
            upd.Parameters.AddWithValue("@id", task.CartonId.Value);
            int rows = upd.ExecuteNonQuery();

            return rows > 0
                ? new CartonWriteResult { Success = true, Status = "OK", Message = $"Reset carton {task.CartonId}" }
                : new CartonWriteResult { Success = false, Message = $"Khong tim thay carton {task.CartonId}" };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _queue.CompleteAdding();
            _consumer.Wait(5000);
            _queue.Dispose();
            Log.Information("[CartonWriteQueue] Disposed");
        }
    }

    // === Manager: mot queue per PO ===
    public static class CartonWriteQueueManager
    {
        private static readonly ConcurrentDictionary<string, CartonWriteQueue> _queues = new();

        public static CartonWriteQueue GetOrCreate(string orderNo)
        {
            return _queues.GetOrAdd(orderNo, on =>
            {
                var dbPath = Config.GetCartonPath(on);
                return new CartonWriteQueue(dbPath);
            });
        }

        public static void Remove(string orderNo)
        {
            if (_queues.TryRemove(orderNo, out var q)) q.Dispose();
        }

        public static Task<CartonWriteResult> EnqueueAsync(string orderNo, CartonWriteTask task)
        {
            var q = GetOrCreate(orderNo);
            return q.EnqueueAsync(task);
        }
    }
}
```

### A.4. POCartonCode.cs — Read-only helpers (không queue)

**File:** `GProject/ProductionOrderHelpers/POCartonCode.cs` (MỚI)

```csharp
public static class POCartonCode
{
    private static string GetConnStr(string dbPath) => $"Data Source={dbPath}";

    public static CartonDetailInfo GetCartonInfo(string orderNo, string cartonCode)
    {
        var result = new CartonDetailInfo { CartonCode = cartonCode };
        try
        {
            string dbPath = Config.GetCartonPath(orderNo);
            if (!File.Exists(dbPath)) return result.WithError("Carton DB not found");

            var carton = POCarton.GetByCartonCode(orderNo, cartonCode);
            if (!carton.IsSuccess || carton.Data == null || carton.Data.Rows.Count == 0)
                return result.WithError("Khong tim thay thung");

            var row = carton.Data.Rows[0];
            result.CartonIndex = Convert.ToInt32(row["ID"]);
            result.StartDatetime = row["Start_Datetime"]?.ToString() ?? "0";
            result.ActivateUser = row["ActivateUser"]?.ToString() ?? "";

            int count = GProduction.POLoader.CountCodes(orderNo, status: 1, cartonCode: cartonCode);
            result.ProductCount = count;
            result.Status = result.StartDatetime != "0" ? "OK" : "WARN";
            result.Success = true;
            return result;
        }
        catch (Exception ex) { return result.WithError(ex.Message); }
    }

    public static CartonScanInfo? GetLastCarton(string orderNo, string machineName)
    {
        try
        {
            string dbPath = Config.GetCartonPath(orderNo);
            if (!File.Exists(dbPath)) return null;

            var table = SQLiteHelper.ExecuteQuery(GetConnStr(dbPath), @"
                SELECT c.CartonCode, c.Start_Datetime, c.ActivateUser, cc.CartonIndex, cc.ScanAt, cc.Mode, cc.Result
                FROM CartonCode cc
                JOIN Carton c ON c.cartonCode = cc.CartonCode
                WHERE cc.MachineName = @mn
                ORDER BY cc.ID DESC LIMIT 1",
                new SqliteParameter("@mn", machineName));

            if (table.Rows.Count == 0) return null;
            var row = table.Rows[0];
            return new CartonScanInfo
            {
                CartonCode = row["CartonCode"]?.ToString() ?? "",
                StartDatetime = row["Start_Datetime"]?.ToString() ?? "",
                ActivateUser = row["ActivateUser"]?.ToString() ?? "",
                CartonIndex = Convert.ToInt32(row["CartonIndex"]),
                ScanAt = row["ScanAt"]?.ToString() ?? "",
                Mode = row["Mode"]?.ToString() ?? "",
                Result = row["Result"]?.ToString() ?? ""
            };
        }
        catch { return null; }
    }
}

public class CartonDetailInfo
{
    public bool Success { get; set; }
    public string CartonCode { get; set; } = "";
    public int CartonIndex { get; set; }
    public string StartDatetime { get; set; } = "0";
    public string ActivateUser { get; set; } = "";
    public int ProductCount { get; set; }
    public string Status { get; set; } = "ERR";
    public string Message { get; set; } = "";
    public CartonDetailInfo WithError(string msg) { Message = msg; return this; }
}

public class CartonScanInfo
{
    public string CartonCode { get; set; } = "";
    public string StartDatetime { get; set; } = "";
    public string ActivateUser { get; set; } = "";
    public int CartonIndex { get; set; }
    public string ScanAt { get; set; } = "";
    public string Mode { get; set; } = "";
    public string Result { get; set; } = "";
}
```

### A.5. CartonModels.cs — Request/Response Models

**File:** `GProject/ProductionOrderHelpers/CartonModels.cs` (MỚI)

```csharp
// POST /api/carton/scan
public record CartonScanRequest(string MachineName, string CartonCode, string ScannedAt, string Mode = "scan");

public class CartonScanResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string Status { get; set; } = "ERR";  // OK / WARN / ERR
    public int CartonIndex { get; set; }
    public string OrderNo { get; set; } = "";
}

// GET /api/carton/{cartonCode}/info
public class CartonInfoResponse
{
    public bool Success { get; set; }
    public string CartonCode { get; set; } = "";
    public int CartonIndex { get; set; }
    public string ActivateDate { get; set; } = "";
    public string ActivateUser { get; set; } = "";
    public int ProductCount { get; set; }
    public string Status { get; set; } = "ERR";
    public string Message { get; set; } = "";
}

// GET /api/carton/current-po
public class CurrentPOResponse
{
    public bool Success { get; set; }
    public string OrderNo { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int OrderQty { get; set; }
    public string State { get; set; } = "";
    public string Message { get; set; } = "";
}
```

### A.6. Handlers trong POApiServer.cs

Thêm 3 handlers vào file `POApiServer.cs` (region `#region Carton PDA Handlers`):

```csharp
// POST /api/carton/scan
public static async Task<IResult> HandleCartonScan(HttpContext context)
{
    CartonScanRequest? req = await context.Request.ReadFromJsonAsync<CartonScanRequest>();
    if (req == null || string.IsNullOrWhiteSpace(req.CartonCode))
        return Results.Json(new CartonScanResponse { Success = false, Message = "Invalid request" });

    var sm = ProductionStateMachine.Instance;
    if (sm.CurrentState != e_ProductionState.Running || sm.ProductionData == null)
        return Results.Json(new CartonScanResponse { Success = false, Message = "PO not running" });

    var task = new CartonWriteTask
    {
        Type = CartonWriteType.ScanCarton,
        OrderNo = sm.ProductionData.OrderNo,
        CartonCode = req.CartonCode,
        MachineName = req.MachineName,
        ScannedAt = req.ScannedAt,
        Mode = req.Mode
    };

    var result = await CartonWriteQueueManager.EnqueueAsync(sm.ProductionData.OrderNo, task);
    return Results.Json(new CartonScanResponse
    {
        Success = result.Success,
        Message = result.Message,
        Status = result.Status,
        CartonIndex = result.CartonIndex,
        OrderNo = sm.ProductionData.OrderNo
    });
}

// GET /api/carton/{cartonCode}/info
public static IResult HandleCartonInfo(string cartonCode)
{
    var sm = ProductionStateMachine.Instance;
    if (sm.CurrentState != e_ProductionState.Running || sm.ProductionData == null)
        return Results.Json(new CartonInfoResponse { Success = false, Message = "PO not running" });

    var info = POCartonCode.GetCartonInfo(sm.ProductionData.OrderNo, cartonCode);
    return Results.Json(new CartonInfoResponse
    {
        Success = info.Success,
        CartonCode = info.CartonCode,
        CartonIndex = info.CartonIndex,
        ActivateDate = info.StartDatetime,
        ActivateUser = info.ActivateUser,
        ProductCount = info.ProductCount,
        Status = info.Status,
        Message = info.Message
    });
}

// GET /api/carton/current-po
public static IResult HandleGetCurrentPO()
{
    var sm = ProductionStateMachine.Instance;
    if (sm.ProductionData == null)
        return Results.Json(new CurrentPOResponse { Success = false, Message = "No PO loaded" });

    return Results.Json(new CurrentPOResponse
    {
        Success = true,
        OrderNo = sm.ProductionData.OrderNo,
        ProductName = sm.ProductionData.ProductName,
        OrderQty = sm.ProductionData.OrderQty,
        State = sm.CurrentState.ToString()
    });
}
```

### A.7. GProduction wrapper

Thêm vào `GProject/ProductionOrderHelpers/GProduction.cs`:

```csharp
public static class POCartonCodeHelper
{
    public static Task<CartonWriteResult> ScanCartonAsync(string orderNo, string machineName,
        string cartonCode, string scannedAt, string mode = "scan")
    {
        var task = new CartonWriteTask
        {
            Type = CartonWriteType.ScanCarton,
            OrderNo = orderNo,
            MachineName = machineName,
            CartonCode = cartonCode,
            ScannedAt = scannedAt,
            Mode = mode
        };
        return CartonWriteQueueManager.EnqueueAsync(orderNo, task);
    }
}
```

### A.8. Routes trong GProjectApiServer.cs

Thêm sau các route `/api/po/*`:

```csharp
_app.MapGet("/api/carton/current-po", POApiServer.HandleGetCurrentPO);
_app.MapPost("/api/carton/scan", async ctx => await POApiServer.HandleCartonScan(ctx));
_app.MapGet("/api/carton/{cartonCode}/info", POApiServer.HandleCartonInfo);
```

---

## Phần B: State Broadcast + PLC Counter

### B.1. ProductionHub.cs — WebSocket Hub

**File:** `GProject/ProductionHub.cs` (MỚI)

Singleton pattern giống `PLCHub` và `CameraHub`. Payload gồm: `state`, `previousState`, `orderNo`, `productName`, `orderQty`, `activeCounter`, `lastWarning`, `isAppReady`, `isDeviceReady`, `codesCount`, `cartonsCount`, `timestamp`.

```csharp
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;

namespace GProject;

public class ProductionHub
{
    public static readonly ProductionHub Instance = new();

    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    private readonly object _registerLock = new();

    public void Register(WebSocket ws)
    {
        lock (_registerLock)
        {
            _clients[Guid.NewGuid()] = ws;
        }
    }

    public void Unregister(WebSocket ws)
    {
        lock (_registerLock)
        {
            foreach (var kv in _clients.Where(kv => kv.Value == ws).ToList())
            {
                _clients.TryRemove(kv.Key, out _);
            }
        }
    }

    public int ClientCount => _clients.Count;

    public async Task BroadcastStateAsync(
        string state,
        string? previousState,
        string? orderNo,
        string? productName,
        int orderQty,
        object activeCounter,
        string? lastWarning,
        bool isAppReady,
        bool isDeviceReady,
        int codesCount,
        int cartonsCount)
    {
        var payload = JsonSerializer.Serialize(new
        {
            state,
            previousState,
            orderNo,
            productName,
            orderQty,
            activeCounter,
            lastWarning,
            isAppReady,
            isDeviceReady,
            codesCount,
            cartonsCount,
            at = DateTime.UtcNow
        });
        var bytes = Encoding.UTF8.GetBytes(payload);
        var segment = new ArraySegment<byte>(bytes);

        List<WebSocket> clientsToRemove;
        lock (_registerLock)
        {
            clientsToRemove = _clients.Values.Where(c => c.State != WebSocketState.Open).ToList();
            foreach (var ws in clientsToRemove)
                _clients.TryRemove(FindKey(ws), out _);
        }

        foreach (var ws in clientsToRemove)
        {
            try { await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); }
            catch { }
        }

        if (_clients.IsEmpty) return;

        WebSocket[] openClients;
        lock (_registerLock)
        {
            openClients = _clients.Values.Where(c => c.State == WebSocketState.Open).ToArray();
        }

        foreach (var ws in openClients)
        {
            try
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[ProductionHub] Failed to send — removing stale client");
                lock (_registerLock)
                {
                    _clients.TryRemove(FindKey(ws), out _);
                }
            }
        }
    }

    private Guid FindKey(WebSocket ws)
    {
        lock (_registerLock)
        {
            return _clients.First(kv => kv.Value == ws).Key;
        }
    }
}
```

### B.2. Cập nhật ProductionStateMachine — Broadcast trong SetState()

**File:** `GProject/Production/ProductionStateMachine.cs`

Thêm tham chiếu đến `ProductionHub` và gọi broadcast khi state thay đổi. Throttle: chỉ broadcast khi state thay đổi HOẶC mỗi 500ms để update counter.

```csharp
// Thêm field ở đầu class
private DateTime _lastBroadcast = DateTime.MinValue;
private static readonly TimeSpan BroadcastThrottle = TimeSpan.FromMilliseconds(500);

// Cập nhật SetState() — sau khi đổi state, broadcast ngay lập tức
public void SetState(e_ProductionState newState, string? reason = null)
{
    lock (_stateLock)
    {
        if (_currentState == newState) return;
        _previousState = _currentState;
        _currentState = newState;
    }
    Log.Information("[StateMachine] {Prev} -> {New} ({Reason})",
        _previousState, newState, reason ?? "no reason");

    // Broadcast ngay lập tức khi state đổi
    _ = BroadcastStateAsync();
}

// Thêm method broadcast
private async Task BroadcastStateAsync()
{
    try
    {
        var now = DateTime.Now;
        if ((now - _lastBroadcast) < BroadcastThrottle) return;
        _lastBroadcast = now;

        var sm = ProductionStateMachine.Instance;
        await ProductionHub.Instance.BroadcastStateAsync(
            sm.CurrentState.ToString(),
            sm.PreviousState.ToString(),
            ProductionData?.OrderNo,
            ProductionData?.ProductName,
            ProductionData?.OrderQty ?? 0,
            new
            {
                sm.ActiveCounter.PassCount,
                sm.ActiveCounter.FailCount,
                sm.ActiveCounter.DuplicateCount,
                sm.ActiveCounter.CartonID,
                sm.ActiveCounter.CartonCode
            },
            sm.LastWarning,
            sm.IsAppReady,
            sm.IsDeviceReady,
            sm.Dictionary_Codes.Count,
            sm.Dictionary_Cartons.Count
        );
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "[StateMachine] Broadcast failed");
    }
}
```

### B.3. /ws/production Endpoint

**File:** `GProject/GProjectApiServer.cs`

Thêm route sau `/ws/plc`:

```csharp
_app.Map("/ws/production", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();
    ProductionHub.Instance.Register(ws);
    Log.Information("[WebSocket] Production client connected. Total clients: {Count}", ProductionHub.Instance.ClientCount);

    try
    {
        var buffer = new byte[1024];
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close) break;
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "[WebSocket] Error in Production WebSocket connection.");
    }
    finally
    {
        ProductionHub.Instance.Unregister(ws);
        Log.Information("[WebSocket] Production client disconnected. Total clients: {Count}", ProductionHub.Instance.ClientCount);
    }
});
```

### B.4. Sửa HandleGetProductionStatus()

**File:** `GProject/ProductionOrderHelpers/POApiServer.cs`

Thay hardcoded `{ state: "Ready", hasPO: false }` bằng dữ liệu thực từ `ProductionStateMachine`:

```csharp
public static Task<IResult> HandleGetProductionStatus()
{
    try
    {
        var sm = ProductionStateMachine.Instance;
        int activeCodes = 0, packedCodes = 0;
        string? orderNo = null, productName = null;
        int orderQty = 0, cartonCount = 0, closedCartons = 0;

        if (sm.ProductionData != null)
        {
            orderNo = sm.ProductionData.OrderNo;
            productName = sm.ProductionData.ProductName;
            orderQty = sm.ProductionData.OrderQty;
            activeCodes = GProduction.PORecordHelper.GetActiveCount(orderNo);
            packedCodes = GProduction.PORecordHelper.GetPackedCount(orderNo);
            cartonCount = GProduction.POCarton.GetTotalCartonCount(orderNo);
            closedCartons = GProduction.POCarton.GetClosedCartonCount(orderNo);
        }

        return Task.FromResult(Results.Json(new ProductionStatusResponse
        {
            Success = true,
            State = sm.CurrentState.ToString(),
            HasPO = sm.ProductionData != null,
            OrderNo = orderNo,
            ProductName = productName,
            OrderQty = orderQty,
            TotalCount = sm.ActiveCounter.TotalCount,
            PassCount = sm.ActiveCounter.PassCount,
            FailCount = sm.ActiveCounter.FailCount,
            DuplicateCount = sm.ActiveCounter.DuplicateCount,
            CartonCount = cartonCount,
            CartonClosedCount = closedCartons,
            CurrentCartonId = sm.ActiveCounter.CartonID,
            CurrentCartonCode = sm.ActiveCounter.CartonCode,
            ItemsInCarton = sm.PackageCounter.PassCount,
            CartonCapacity = sm.ActiveCounter.CartonCapacity,
            ProgressPercent = orderQty > 0 ? Math.Round((double)packedCodes / orderQty * 100, 2) : 0
        }));
    }
    catch (Exception ex)
    {
        LogError("POApiServer", $"Error getting production status: {ex.Message}", ex);
        return Task.FromResult(Results.Json(new ProductionStatusResponse { Success = false, Message = ex.Message }, statusCode: 500));
    }
}
```

Đồng thời cập nhật `ProductionStatusResponse` (dòng 54-69) để thêm `currentState`, `previousState`, `lastWarning`, `isAppReady`, `isDeviceReady` nếu FE cần.

### B.5. PLCMonitor — Đọc counter thực từ PLC

**File:** `GProject/PLCMonitor.cs`

Viết lại `PollLoop()` để thực sự đọc counter từ PLC. Biến `counterDm` và `deactiveDm` đã khai báo nhưng chưa dùng.

```csharp
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
                        // Cap nhat vao ProductionStateMachine.ActiveCounter
                        // (se lam sau khi co property public de ghi)
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
        catch (OperationCanceledException) { break; }
        catch (Exception ex)
        {
            Log.Error(ex, "[PLCMonitor] Unexpected error in poll loop");
            Thread.Sleep(_pollMs);
        }
    }

    _running = false;
}
```

Cần thêm property `TotalCount` public vào `ProductionStateMachine.ActiveCounter` để `PLCMonitor` có thể cập nhật.

---

## Phần C: Frontend (React)

### C.1. Types — ProductionStateResponse

**File:** `iot-scada-admin-panel/src/types/production.ts`

Bổ sung `ProductionStateResponse` khớp với backend:

```typescript
export interface ProductionStateResponse {
  success: boolean;
  currentState: string;
  previousState: string;
  orderNo: string;
  productName: string;
  orderQty: number;
  activeCounter: {
    PassCount: number;
    FailCount: number;
    DuplicateCount: number;
    CartonID: number;
    CartonCode: string;
  };
  codesCount: number;
  cartonsCount: number;
  lastWarning: string;
  isAppReady: boolean;
  isDeviceReady: boolean;
}
```

### C.2. API — getProductionState()

**File:** `iot-scada-admin-panel/src/services/productionApi.ts`

Thêm method gọi `/api/production/state`:

```typescript
/** Get full production state from StateMachine */
async getProductionState(): Promise<ProductionStateResponse> {
  const response = await apiClient.get<ProductionStateResponse>(
    "/api/production/state"
  );
  return response.data;
},
```

### C.3. Hook — useProductionWebSocket.ts

**File:** `iot-scada-admin-panel/src/hooks/useProductionWebSocket.ts` (MỚI)

```typescript
import { useEffect, useRef, useState, useCallback } from "react";
import type { ProductionStateResponse } from "../types/production";

interface UseProductionWebSocketOptions {
  url: string;
  reconnectIntervalMs?: number;
  maxReconnectAttempts?: number;
}

const initialSnapshot: ProductionStateResponse = {
  success: false,
  currentState: "Unknown",
  previousState: "Unknown",
  orderNo: "",
  productName: "",
  orderQty: 0,
  activeCounter: { PassCount: 0, FailCount: 0, DuplicateCount: 0, CartonID: 0, CartonCode: "" },
  codesCount: 0,
  cartonsCount: 0,
  lastWarning: "",
  isAppReady: false,
  isDeviceReady: false,
};

export function useProductionWebSocket({
  url,
  reconnectIntervalMs = 3000,
  maxReconnectAttempts = 5,
}: UseProductionWebSocketOptions) {
  const [snapshot, setSnapshot] = useState<ProductionStateResponse>(initialSnapshot);
  const [connected, setConnected] = useState(false);
  const wsRef = useRef<WebSocket | null>(null);
  const attemptsRef = useRef(0);
  const retryRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const manualRef = useRef(false);

  const connect = useCallback(() => {
    try {
      const ws = new WebSocket(url);
      wsRef.current = ws;

      ws.onopen = () => {
        attemptsRef.current = 0;
        setConnected(true);
      };

      ws.onclose = () => {
        setConnected(false);
        if (!manualRef.current && attemptsRef.current < maxReconnectAttempts) {
          const delay = reconnectIntervalMs * Math.pow(1.5, attemptsRef.current);
          retryRef.current = setTimeout(() => {
            attemptsRef.current++;
            connect();
          }, delay);
        }
      };

      ws.onerror = () => { };

      ws.onmessage = (ev) => {
        try {
          const data = JSON.parse(ev.data as string) as ProductionStateResponse;
          if (data.state) setSnapshot(data);
        } catch { }
      };
    } catch { }
  }, [url, reconnectIntervalMs, maxReconnectAttempts]);

  useEffect(() => {
    connect();
    return () => {
      manualRef.current = true;
      if (retryRef.current) clearTimeout(retryRef.current);
      wsRef.current?.close(1000, "unmount");
    };
  }, [connect]);

  return { snapshot, connected };
}
```

### C.4. ProductionView.tsx — Hiển thị 16 state + WebSocket

**File:** `iot-scada-admin-panel/src/components/production/ProductionView.tsx`

1. Import hook: `import { useProductionWebSocket } from "../../hooks/useProductionWebSocket";`

2. Kết nối WS: thêm production WS URL + hook trong component

3. Mở rộng `stateConfig` đủ 16 state:

```typescript
const stateConfig: Record<string, { label: string; bg: string; text: string; dot: string; icon: string }> = {
  NeedLogin: { label: "NEED LOGIN", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
  Checking: { label: "CHECKING", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500", icon: "animate-pulse" },
  NoSelectedPO: { label: "NO PO", bg: "bg-slate-50", text: "text-slate-600", dot: "bg-slate-400", icon: "" },
  Editing: { label: "EDITING", bg: "bg-amber-50", text: "text-amber-700", dot: "bg-amber-500", icon: "" },
  CheckingPO: { label: "CHECK PO", bg: "bg-indigo-50", text: "text-indigo-700", dot: "bg-indigo-500", icon: "animate-pulse" },
  LoadPO: { label: "LOAD PO", bg: "bg-indigo-50", text: "text-indigo-700", dot: "bg-indigo-500", icon: "animate-pulse" },
  Ready: { label: "READY", bg: "bg-green-50", text: "text-green-700", dot: "bg-green-500", icon: "" },
  PushingToDic: { label: "PUSHING", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500 animate-pulse", icon: "" },
  Running: { label: "RUNNING", bg: "bg-green-50", text: "text-green-800", dot: "bg-green-500 animate-pulse", icon: "" },
  Paused: { label: "PAUSED", bg: "bg-amber-50", text: "text-amber-800", dot: "bg-amber-500", icon: "" },
  WaitingStop: { label: "WAITING STOP", bg: "bg-orange-50", text: "text-orange-700", dot: "bg-orange-500", icon: "animate-pulse" },
  CheckAfterCompleted: { label: "CHECKING", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500", icon: "animate-pulse" },
  Completed: { label: "COMPLETED", bg: "bg-emerald-50", text: "text-emerald-700", dot: "bg-emerald-500", icon: "" },
  DeviceError: { label: "DEVICE ERROR", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
  Error: { label: "ERROR", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
};
```

4. Thêm indicator `lastWarning` khi có cảnh báo

5. Thêm WebSocket status indicator (online/offline)

---

## Phần D: Android PDA

### D.1. ApiClient.kt — Thêm endpoint Carton

```kotlin
data class CartonScanRequest(
    val machineName: String,
    val cartonCode: String,
    val scannedAt: String,  // "yyyy-MM-dd HH:mm:ss.fff+07:00"
    val mode: String = "scan"
)

data class CartonScanResponse(
    val success: Boolean,
    val message: String,
    val status: String,     // OK / WARN / ERR
    val cartonIndex: Int,
    val orderNo: String
)

data class CartonInfoResponse(
    val success: Boolean,
    val cartonCode: String,
    val cartonIndex: Int,
    val activateDate: String,
    val activateUser: String,
    val productCount: Int,
    val status: String,
    val message: String
)

data class CurrentPOResponse(
    val success: Boolean,
    val orderNo: String,
    val productName: String,
    val orderQty: Int,
    val state: String,
    val message: String
)

interface PdaApiService {
    @POST("/api/carton/scan")
    suspend fun postCartonScan(@Body request: CartonScanRequest): Response<CartonScanResponse>

    @GET("/api/carton/{cartonCode}/info")
    suspend fun getCartonInfo(@Path("cartonCode") code: String): Response<CartonInfoResponse>

    @GET("/api/carton/current-po")
    suspend fun getCurrentPO(): Response<CurrentPOResponse>
}
```

### D.2. MainActivity.kt — 2 chế độ QUET THUNG / KIEM TRA

- **QUET THUNG** (default): gửi `/api/carton/scan`, hiển thị OK/WARN/ERR
- **KIEM TRA THUNG**: gửi `/api/carton/{cartonCode}/info`, hiển thị activate date, số sản phẩm, người kích hoạt

---

## Tổng hợp file thay đổi

### File MỚI
| File | Mô tả |
|------|--------|
| `GProject/ProductionOrderHelpers/CartonWriteQueue.cs` | Singleton queue serialize carton writes |
| `GProject/ProductionOrderHelpers/POCartonCode.cs` | Read-only helpers (GetCartonInfo, GetLastCarton) |
| `GProject/ProductionOrderHelpers/CartonModels.cs` | Request/response models |
| `GProject/ProductionHub.cs` | WebSocket hub singleton cho production state |
| `iot-scada-admin-panel/src/hooks/useProductionWebSocket.ts` | Hook WebSocket cho /ws/production |

### File SỬA
| File | Thay đổi |
|------|----------|
| `GProject/Production/ProductionStateMachine.cs` | Tick 10ms + ProductionHub broadcast + throttle |
| `GProject/ProductionOrderHelpers/Config.cs` | Thêm bảng CartonCode schema |
| `GProject/ProductionOrderHelpers/POApiServer.cs` | 3 handlers carton + sửa HandleGetProductionStatus |
| `GProject/ProductionOrderHelpers/GProduction.cs` | Thêm POCartonCodeHelper |
| `GProject/GProjectApiServer.cs` | 3 routes /api/carton/* + /ws/production |
| `GProject/PLCMonitor.cs` | Đọc counter từ PLC thực sự |
| `iot-scada-admin-panel/src/types/production.ts` | Thêm ProductionStateResponse type |
| `iot-scada-admin-panel/src/services/productionApi.ts` | Thêm getProductionState() |
| `iot-scada-admin-panel/src/components/production/ProductionView.tsx` | 16 states + WS production |
| `MtePDA/app/.../ApiClient.kt` | Thêm endpoint CartonScan/Info/CurrentPO |
| `MtePDA/app/.../MainActivity.kt` | Thêm tab QUET THUNG + KIEM TRA |
| `MtePDA/app/.../activity_main.xml` | Sửa layout |

---

## Thứ tự thực hiện

```
Phase 1: Infrastructure
  1.  sm-speedup         → Đổi State Machine 100ms -> 10ms
  2.  add-sql-schema     → Thêm bảng CartonCode vào Config.cs

Phase 2: Carton Backend
  3.  create-carton-models       → Tạo CartonModels.cs
  4.  create-carton-write-queue  → Tạo CartonWriteQueue.cs
  5.  create-pocartoncode        → Tạo POCartonCode.cs
  6.  add-cartridge-handlers     → Thêm 3 handlers vào POApiServer.cs
  7.  add-gproduction-wrapper    → Thêm POCartonCodeHelper vào GProduction.cs
  8.  add-carton-routes         → Đăng ký 3 routes /api/carton/*

Phase 3: State Broadcast Backend
  9.  create-production-hub      → Tạo ProductionHub.cs
 10.  sm-broadcast              → Cập nhật SetState() gọi broadcast
 11.  add-ws-production-endpoint → /ws/production endpoint
 12.  fix-status-api            → Sửa HandleGetProductionStatus()

Phase 4: PLC Integration
 13.  plc-counter               → PLCMonitor đọc counter thực

Phase 5: Frontend
 14.  fe-types          → ProductionStateResponse type
 15.  fe-api             → getProductionState() API
 16.  fe-hook             → useProductionWebSocket.ts
 17.  fe-view             → ProductionView 16 states + WS

Phase 6: Android PDA
 18.  update-android-apiclient → ApiClient.kt
 19.  update-android-main       → MainActivity.kt
 20.  update-android-layout     → activity_main.xml

Phase 7: Testing
 21.  test-backend-api  → Postman/curl test backend
 22.  test-frontend      → Frontend kết nối WebSocket
```
