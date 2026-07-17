---
name: Fix Bug Restart Carton Selection
overview: Fix bug restart app nhảy sai thùng bằng cách thay đổi logic restore cartonID - sử dụng cartonCode đã pack từ UniqueCodes thay vì dùng StartDatetime.
todos: []
isProject: false
---

## Bug hiện tại

**File:** `GProject/Production/ProductionStateMachine.cs`  
**Lines:** 1417-1436

Code hiện tại tìm `lastRunning` bằng `StartDatetime != '0' AND CompletedDatetime == '0'`, nhưng điều này ưu tiên thùng đã start thay vì thùng chưa chạy.

## Fix theo pattern MASAN

Thay logic restore bằng cách tìm `cartonCode` có sản phẩm nhiều nhất từ `UniqueCodes`, giống như cách MASAN làm ở `PPOInfo.cs`.

---

## Thay đổi 1: Thêm helper method mới

**File:** `GProject/ProductionOrderHelpers/PORecord.cs`

Thêm method `GetLastPackedCartonCode()` để lấy cartonCode có count cao nhất:

```csharp
public static string GetLastPackedCartonCode(string orderNo)
{
    try
    {
        string dbPath = Config.GetPODBPath(orderNo);
        if (!File.Exists(dbPath)) return null;
        using var con = new SqliteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"SELECT cartonCode FROM UniqueCodes 
                           WHERE cartonCode <> '0' 
                           GROUP BY cartonCode 
                           ORDER BY COUNT(*) DESC 
                           LIMIT 1;";
        return cmd.ExecuteScalar()?.ToString();
    }
    catch { return null; }
}
```

---

## Thay đổi 2: Sửa logic restore cartonID

**File:** `GProject/Production/ProductionStateMachine.cs`  
**Lines:** 1414-1436 (thay thế toàn bộ block)

```csharp
// Reload trạng thái đang chạy từ DB để phục hồi khi restart (mất điện)
// Ưu tiên thùng đã pack có sản phẩm nhiều nhất, không dùng StartDatetime
string lastCartonCode = GProduction.PORecordHelper.GetLastPackedCartonCode(ProductionData.OrderNo);

if (!string.IsNullOrEmpty(lastCartonCode))
{
    // Tìm cartonInfo từ Dictionary_Cartons
    var lastCarton = Dictionary_Cartons.Values
        .FirstOrDefault(c => c.CartonCode == lastCartonCode);

    if (lastCarton != null)
    {
        int packedCount = GProduction.PORecordHelper.GetCodeCountInCarton(
            ProductionData.OrderNo, lastCartonCode);

        if (packedCount >= ProductionData.CartonCapacity)
        {
            // Thùng cuối đã đầy → sang thùng mới
            ActiveCounter.CartonID = lastCarton.Id + 1;
            ActiveCounter.CartonCode = "";
        }
        else
        {
            // Tiếp tục thùng cuối
            ActiveCounter.CartonID = lastCarton.Id;
            ActiveCounter.CartonCode = lastCartonCode;
        }
    }
    else
    {
        // Không tìm thấy cartonInfo → fallback về thùng 1
        ActiveCounter.CartonID = 1;
        ActiveCounter.CartonCode = "";
    }
}
else
{
    // Chưa có thùng nào được pack → bắt đầu từ thùng 1
    ActiveCounter.CartonID = 1;
    ActiveCounter.CartonCode = "";
}
```

---

## Thay đổi 3: Export helper method qua GProduction

**File:** `GProject/ProductionOrderHelpers/GProduction.cs`

Thêm:

```csharp
public static string GetLastPackedCartonCode(string orderNo)
    => ProductionOrderHelpers.PORecord.GetLastPackedCartonCode(orderNo);
```

---

## Todo List

1. Thêm `GetLastPackedCartonCode()` vào `PORecord.cs`
2. Export qua `GProduction.cs`
3. Thay thế logic restore ở `ProductionStateMachine.cs` lines 1414-1436
4. Build và test