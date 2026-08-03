---
name: PLC Simulation Feature
overview: Thêm tính năng PLC giả lập (simulation) cho GProject, cho phép bật/tắt kết nối tới PLC ảo localhost:9600 thay vì PLC thật, hỗ trợ debug và test mà không cần phần cứng PLC.
todos:
  - id: add-appconfig-prop
    content: Add PLC_Simulation property to AppConfig.cs
    status: completed
  - id: create-simulator-class
    content: Create PLCSimulator.cs using TCP sockets
    status: completed
  - id: add-global-vars
    content: Add plcSimulator and UsePlcSimulation to GlobalVariable.cs
    status: completed
  - id: update-program
    content: Update Program.cs to initialize simulator when enabled
    status: completed
  - id: add-api-endpoint
    content: Add /api/plc/simulation endpoint to GProjectApiServer.cs
    status: completed
isProject: false
---

## Thêm tính năng PLC Giả lập (Simulation)

### 1. Thêm property vào AppConfig

**File:** `GProject/Configs/AppConfig.cs`

Thêm property boolean để bật/tắt simulation:

```csharp
// PLC Settings
public string? PLC_IP { get; set; }
public int PLC_Port { get; set; }
public bool PLC_Simulation { get; set; } = false;  // ← THÊM MỚI
```

---

### 2. Tạo class PLCSimulator

**File:** `GProject/PLCHelpers/PLCSimulator.cs` (mới)

Sử dụng `HslCommunication.Core.Net.NetSimplifyServer` để tạo PLC server ảo:

```csharp
public class PLCSimulator
{
    private NetSimplifyServer? _server;
    
    // Các DM registers ảo
    private short[] _dmRegisters = new short[5000];
    
    // Counter simulation
    private int _totalCount = 0;
    private int _passCount = 0;
    private int _failCount = 0;
    
    public bool IsRunning { get; private set; }
    public void Start(int port = 9600);
    public void Stop();
    public void ResetCounters();
    
    // Ghi log mỗi lần có read/write để debug
    public event EventHandler<string>? OnLog;
}
```

---

### 3. Thêm singleton Global

**File:** `GProject/Infrastructure/GlobalVariable.cs`

Thêm:

```csharp
public static class Global
{
    public static OmronPLC_Hsl? omronPLC;
    public static PLCSimulator? plcSimulator;  // ← THÊM
    public static bool UsePlcSimulation = false;  // ← THÊM
    // ...
}
```

---

### 4. Sửa Program.cs - khởi tạo Simulator

**File:** `GProject/Program.cs`

Sau khi load config, thêm logic khởi tạo simulator:

```csharp
// Sau dòng: _config = ConfigStorage.Load<AppConfig>() ?? new AppConfig();
if (_config.PLC_Simulation)
{
    Global.UsePlcSimulation = true;
    Global.plcSimulator = new PLCSimulator();
    Global.plcSimulator.Start(_config.PLC_Port > 0 ? _config.PLC_Port : 9600);
    Log.Information("[PLC] Simulation mode ENABLED on port {Port}", _config.PLC_Port);
}
else
{
    Global.UsePlcSimulation = false;
    // Khởi tạo PLC thật như cũ...
}
```

---

### 5. Thêm API endpoint để bật/tắt simulation

**File:** `GProject/GProjectApiServer.cs`

```csharp
// POST /api/plc/simulation
app.MapPost("/api/plc/simulation", (bool enable) => {
    if (enable)
    {
        Global.plcSimulator ??= new PLCSimulator();
        Global.plcSimulator.Start(9600);
        Global.UsePlcSimulation = true;
    }
    else
    {
        Global.plcSimulator?.Stop();
        Global.UsePlcSimulation = false;
    }
    return Results.Ok(new { simulation = Global.UsePlcSimulation });
});
```

---

### 6. Thêm UI switch trong Frontend

**File:** `FrontEnd-Six/src/components/plcsetting/PLCSettingsView.tsx`

Thêm switch button:

```tsx
<div className="plc-setting-section">
  <h3>PLC Simulation</h3>
  <label className="switch-label">
    <span>Enable Simulation (127.0.0.1:9600)</span>
    <input
      type="checkbox"
      checked={plcSettings.simulation}
      onChange={(e) => updatePlcSettings({ simulation: e.target.checked })}
    />
  </label>
  <p className="hint">Khi bật, kết nối tới PLC ảo localhost thay vì PLC thật</p>
</div>
```

---

### 7. Thêm API client function

**File:** `FrontEnd-Six/src/services/plcApi.ts`

```typescript
export const togglePlcSimulation = async (enable: boolean): Promise<void> => {
  await api.post('/api/plc/simulation', { enable });
};
```

---

### Files cần tạo mới

| File | Mô tả |
|------|-------|
| `GProject/PLCHelpers/PLCSimulator.cs` | Class PLC giả lập dùng HslCommunication |

### Files cần sửa

| File | Thay đổi |
|------|----------|
| `GProject/Configs/AppConfig.cs` | Thêm `PLC_Simulation` |
| `GProject/Infrastructure/GlobalVariable.cs` | Thêm `plcSimulator`, `UsePlcSimulation` |
| `GProject/Program.cs` | Logic khởi tạo simulator |
| `GProject/GProjectApiServer.cs` | Thêm endpoint `/api/plc/simulation` |
| `FrontEnd-Six/src/components/plcsetting/PLCSettingsView.tsx` | Thêm UI switch |
| `FrontEnd-Six/src/services/plcApi.ts` | Thêm `togglePlcSimulation` |
| `FrontEnd-Six/src/types/plc.ts` | Thêm type cho simulation state |