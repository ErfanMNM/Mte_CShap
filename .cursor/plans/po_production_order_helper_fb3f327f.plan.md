---
name: PO Production Order Helper
overview: Xây dựng file po.cs - helper quản lý Production Order với 4 file SQLite (PO_List, PO, Record_Active, Record_Packing, Carton), lấy mã từ DataPool theo GTIN, hỗ trợ 2 camera Active/Packing.
todos: []
isProject: false
---

## Kế hoạch hoàn thành `VNQR/Helpers/po.cs`

### Cấu trúc namespace và class

Giữ nguyên `namespace VNQR.Helpers` và `public class po`. Bên trong chia thành các nested static class theo pattern `DataAccess` → `Ctor` → `Properties` → `Methods`:

```
po
├── Config (static)           — path, pragmas, SQL schemas
├── POLoader (static)         — CRUD cho PO_List.db
├── POCreator (static)       — Khởi tạo PO (tạo DB files + bảng)
├── POLoader (instance)      — Load codes từ DataPool vào PO.db
├── POActivator (static)     — Camera Active: insert Record_Active + update UniqueCodes
├── POPacking (static)        — Camera Packing: insert Record_Packing + update UniqueCodes
├── POCarton (static)        — CRUD thùng carton
├── POUpdater (static)       — Update thông tin PO
├── Helpers (static)         — GetPath, GetPOName, GetBasePath, v.v.
├── Models (nested)          — TResult, enums, data model classes
└── Properties (static)       — path config, counters
```

### 1. Cấu hình — `Config`

```csharp
public static class Config
{
    public static string baseDataPath = "C:/VNQR/PODatabases"; // configurable
    public static string poListFileName = "PO_List.db";         // configurable

    // PRAGMA áp dụng cho mọi DB
    private const string PRAGMA = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=10000; PRAGMA temp_store=memory;";

    // Schema PO_List
    public const string SQL_CREATE_PO_LIST = @"
        CREATE TABLE IF NOT EXISTS PO (
            orderNo          TEXT NOT NULL PRIMARY KEY,
            site             TEXT NOT NULL DEFAULT '',
            factory          TEXT NOT NULL DEFAULT '',
            productionLine   TEXT NOT NULL DEFAULT '',
            productionDate   TEXT NOT NULL DEFAULT '',
            shift            TEXT NOT NULL DEFAULT '',
            orderQty         INTEGER NOT NULL DEFAULT 0,
            lotNumber        TEXT NOT NULL DEFAULT '',
            productCode      TEXT NOT NULL DEFAULT '',
            productName      TEXT NOT NULL DEFAULT '',
            gtin             TEXT NOT NULL DEFAULT '',
            customerOrderNo  TEXT NOT NULL DEFAULT '',
            uom              TEXT NOT NULL DEFAULT '',
            CreatedTime      TEXT NOT NULL DEFAULT '',
            ModifiedTime     TEXT NOT NULL DEFAULT ''
        );
        CREATE INDEX IF NOT EXISTS IDX_PO_gtin ON PO(gtin);
        PRAGMA journal_mode=WAL;
    ";

    // Schema UniqueCodes (trong <PO>.db)
    public const string SQL_CREATE_PO_CODES = @"
        CREATE TABLE IF NOT EXISTS UniqueCodes (
            ID              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Code            TEXT NOT NULL UNIQUE,
            cartonCode      TEXT NOT NULL DEFAULT '0',
            Status          INTEGER NOT NULL DEFAULT 0,
            ActivateDate    TEXT NOT NULL DEFAULT '0',
            ProductionDate  TEXT NOT NULL DEFAULT '0',
            ActivateUser    TEXT NOT NULL DEFAULT '',
            PackingDate     TEXT NOT NULL DEFAULT '0',
            Send_Status     TEXT NOT NULL DEFAULT 'Pending',
            Recive_Status   TEXT NOT NULL DEFAULT 'Pending'
        );
        CREATE INDEX IF NOT EXISTS IDX_UC_Status     ON UniqueCodes(Status);
        CREATE INDEX IF NOT EXISTS IDX_UC_cartonCode ON UniqueCodes(cartonCode);
        CREATE UNIQUE INDEX IF NOT EXISTS IDX_UC_Code ON UniqueCodes(Code);
        PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=10000; PRAGMA temp_store=memory;
    ";

    // Schema Record_Active (trong Record_Active_<PO>.db)
    public const string SQL_CREATE_RECORD_ACTIVE = @"
        CREATE TABLE IF NOT EXISTS Records (
            ID              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Code            TEXT NOT NULL DEFAULT 'FAIL',
            cartonCode      TEXT NOT NULL DEFAULT '0',
            Status          TEXT NOT NULL DEFAULT '0',
            PLC_Status      TEXT NOT NULL DEFAULT 'FAIL',
            ActivateDate    TEXT NOT NULL DEFAULT '0',
            ActivateUser    TEXT NOT NULL DEFAULT '',
            ProductionDate  TEXT NOT NULL DEFAULT '0'
        );
        PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=10000; PRAGMA temp_store=memory;
    ";

    // Schema Record_Packing (trong Record_Packing_<PO>.db)
    public const string SQL_CREATE_RECORD_PACKING = @"
        CREATE TABLE IF NOT EXISTS Records (
            ID              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            Code            TEXT NOT NULL DEFAULT 'FAIL',
            cartonCode      TEXT NOT NULL DEFAULT '0',
            Status          TEXT NOT NULL DEFAULT '0',
            PLC_Status      TEXT NOT NULL DEFAULT 'FAIL',
            PackingDate     TEXT NOT NULL DEFAULT '0',
            PackingUser     TEXT NOT NULL DEFAULT '',
            ProductionDate  TEXT NOT NULL DEFAULT '0'
        );
        PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=10000; PRAGMA temp_store=memory;
    ";

    // Schema Carton (trong Carton_<PO>.db)
    public const string SQL_CREATE_CARTON = @"
        CREATE TABLE IF NOT EXISTS Carton (
            ID                   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            cartonCode           TEXT NOT NULL DEFAULT '0',
            Start_Datetime       TEXT NOT NULL DEFAULT '0',
            Completed_Datetime   TEXT NOT NULL DEFAULT '0',
            ActivateUser         TEXT NOT NULL DEFAULT '',
            ProductionDate       TEXT NOT NULL DEFAULT '0'
        );
        PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA cache_size=10000; PRAGMA temp_store=memory;
    ";
}
```

### 2. Helpers & Path

```csharp
public static class Helpers
{
    // Lấy đường dẫn base: <baseDataPath>/yyyy-MM/gtin/
    public static string GetBasePath(string orderNo, string gtin, string productionDate);
    public static string GetBasePath(string orderNo); // overload — tự lookup gtin + date từ PO_List

    // <baseDataPath>/yyyy-MM/gtin/<PO>.db
    public static string GetPODBPath(string orderNo);

    // <baseDataPath>/yyyy-MM/gtin/Record_Active_<PO>.db
    public static string GetRecordActivePath(string orderNo);

    // <baseDataPath>/yyyy-MM/gtin/Record_Packing_<PO>.db
    public static string GetRecordPackingPath(string orderNo);

    // <baseDataPath>/yyyy-MM/gtin/Carton_<PO>.db
    public static string GetCartonPath(string orderNo);

    // <DataPool.dataPath>/<gtin>.vnqrdb
    public static string GetDataPoolPath(string gtin);
}
```

### 3. Models — `Models`

```csharp
public class POInfo          // model cho 1 PO trong PO_List
public class POCodeData      // model cho 1 dòng UniqueCodes
public class PORecordData    // model cho 1 dòng Record
public class POCartonData    // model cho 1 dòng Carton

public enum e_Carton_Status { Open = 0, Closed = 1, Cancelled = -1 }
public enum e_PLC_Status    { PASS, FAIL, ERROR, TIMEOUT, READFAIL, FORMATERROR }

public class TResult { /* giữ nguyên như DataPool */ }
```

### 4. POLoader (static) — CRUD PO_List

```csharp
public static class POLoader
{
    public static void EnsurePOList();                          // tạo PO_List.db + bảng nếu chưa có

    public static TResult GetAll();                             // SELECT * FROM PO ORDER BY orderNo
    public static TResult GetByOrderNo(string orderNo);        // SELECT * FROM PO WHERE orderNo = @orderNo
    public static TResult GetByGTIN(string gtin);              // SELECT * FROM PO WHERE gtin = @gtin

    public static (bool success, string message) Create(POInfo po); // INSERT INTO PO
    public static (bool success, string message) Update(string orderNo, POInfo po); // UPDATE PO
    public static (bool success, string message) Delete(string orderNo); // DELETE PO + xóa các file DB
    public static bool Exists(string orderNo);
}
```

### 5. POCreator (static) — Khởi tạo PO

```csharp
public static class POCreator
{
    // Bước 2: Tạo 4 file .db cho PO
    public static (bool success, string message) InitPO(string orderNo);

    // Tạo bảng + indexes cho từng file
    private static void EnsureDB(string dbPath, string sqlSchema);
}
```

### 6. POLoader (instance) — Load codes từ DataPool

```csharp
public static class POLoader
{
    // Bước 3: Load codes chưa dùng từ DataPool (GTIN) vào PO.db
    // gtin = poolName → lấy mã Status=0 từ <gtin>.vnqrdb → insert vào UniqueCodes
    // Đồng thời update Status=1 trong DataPool (đánh dấu đã lấy)
    public static (bool success, string message, int loadedCount) LoadCodesFromDataPool(
        string orderNo,
        string gtin,
        int? limitQty = null); // null = lấy hết tất cả mã Status=0

    // Truy vấn UniqueCodes trong PO
    public static TResult GetCodes(string orderNo, int? status = null, string cartonCode = null);
    public static TResult GetCodeByCode(string orderNo, string code);
    public static bool CodeExists(string orderNo, string code);
    public static int GetCodeCount(string orderNo, int? status = null);
}
```

### 7. POActivator (static) — Camera Active

```csharp
public static class POActivator
{
    // Ghi Record vào Record_Active_<PO>.db
    public static void Record(string orderNo, PORecordData data);

    // Cập nhật Status trong UniqueCodes (Status=1 khi active)
    public static void ActivateCode(string orderNo, string code, string activateDate,
                                    string activateUser, string productionDate);

    // Reset code về chưa active (Status=0)
    public static void DeactivateCode(string orderNo, string code);

    // Đếm
    public static int GetActiveCount(string orderNo);
}
```

### 8. POPacking (static) — Camera Packing

```csharp
public static class POPacking
{
    public static void Record(string orderNo, PORecordData data);
    public static void PackCode(string orderNo, string code, string cartonCode,
                                string packingDate, string packingUser, string productionDate);
    public static void UnpackCode(string orderNo, string code);  // gỡ code khỏi thùng
    public static int GetPackedCount(string orderNo);
}
```

### 9. POCarton (static) — CRUD Carton

```csharp
public static class POCarton
{
    public static void Create(string orderNo, POCartonData data);
    public static void Update(string orderNo, POCartonData data);
    public static void Delete(string orderNo, int cartonId);
    public static TResult GetAll(string orderNo);
    public static TResult GetByCode(string orderNo, string cartonCode);
    public static void StartCarton(string orderNo, int cartonId, string activateUser);   // Start_Datetime = now
    public static void CompleteCarton(string orderNo, int cartonId, string activateUser); // Completed_Datetime = now
    public static void ResetCarton(string orderNo, int cartonId);                         // reset về trạng thái mở
}
```

### 10. POUpdater (static) — Update thông tin PO

```csharp
public static class POUpdater
{
    public static bool Update(string orderNo, int? status = null, ...);
    public static bool UpdateProductionDate(string orderNo, string newDate);
}
```

### Các bước thực hiện

1. Viết `Config` + SQL schemas
2. Viết `Helpers` (path functions)
3. Viết `Models` (POInfo, TResult, enums)
4. Viết `POLoader` (CRUD PO_List + query codes)
5. Viết `POCreator` (Init PO)
6. Viết `POLoader.LoadCodesFromDataPool` (bước 3)
7. Viết `POActivator`
8. Viết `POPacking`
9. Viết `POCarton`
10. Viết `POUpdater`
11. Build test

### Ghi chú quan trọng

- Mỗi `EnsureDB` đều idempotent (chỉ tạo bảng khi chưa có).
- Khi load codes từ DataPool, map: `DsataPool.Status=0` → `UniqueCodes.Status=0` và `DataPool.Status=1` → `UniqueCodes.Status=1` (code đã bị lấy trước đó bởi PO khác cùng GTIN).
- `POCarton.Create` không pre-create số thùng mặc định (khác reference) — để caller quyết số lượng thùng.
- Phần Send_AWS / Recive_AWS **chưa làm** (user chọn "làm sau").

