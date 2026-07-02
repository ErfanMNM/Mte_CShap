# ProductionOrderHelpers - Hướng Dẫn Sử Dụng

## Tổng Quan

ProductionOrderHelpers là hệ thống quản lý Production Order (PO) cho GProject, được xây dựng để quản lý toàn bộ vòng đời PO: từ tạo PO, nạp mã từ DataPool, tracking CodeReader Active/Packing, đến quản lý thùng carton.

---

## Cấu Trúc Project

```
GProject/
├── ProductionOrderHelpers/
│   ├── Config.cs                    # Cấu hình đường dẫn, SQL schemas
│   ├── Enums.cs                     # Enumerations
│   ├── Models.cs                    # Data Models
│   ├── SQLiteHelper.cs              # Helper cho SQLite operations
│   ├── POLoader.cs                  # CRUD PO List
│   ├── POCreator.cs                 # Tạo database PO
│   ├── POActivator.cs              # Camera active
│   ├── POPacking.cs                 # Camera packing
│   ├── POCarton.cs                  # Quản lý thùng
│   ├── POHistoryManager.cs          # Lịch sử PO
│   ├── CodeDictionaryLoader.cs      # Load codes vào Dictionary
│   ├── CartonDictionaryLoader.cs     # Load cartons vào Dictionary
│   ├── GProduction.cs               # Main wrapper class
│   └── POApiServer.cs               # REST API endpoints
```

---

## Cấu Trúc Database

### Đường dẫn gốc
```
C:/GProject/PODatabases/
```

### Cấu trúc thư mục
```
C:/GProject/PODatabases/
├── PO_List.db                      # Danh sách tất cả PO
├── POHistory.db                    # Lịch sử chạy PO
└── yyyy-MM/                       # Theo tháng
    └── {gtin}/
        ├── {orderNo}.db           # UniqueCodes - Mã sản phẩm
        ├── Record_Active_{orderNo}.db   # Bản ghi camera active
        ├── Record_Packing_{orderNo}.db  # Bản ghi camera packing
        └── Carton_{orderNo}.db    # Thông tin thùng carton
```

### Bảng PO (PO_List.db)
```sql
CREATE TABLE PO (
    orderNo         TEXT PRIMARY KEY,
    site            TEXT,
    factory         TEXT,
    productionLine  TEXT,
    productionDate TEXT,
    shift           TEXT,
    orderQty        INTEGER,
    lotNumber       TEXT,
    productCode     TEXT,
    productName     TEXT,
    gtin            TEXT,
    customerOrderNo TEXT,
    uom             TEXT DEFAULT 'PCS',
    CreatedTime     TEXT,
    ModifiedTime    TEXT
);
```

### Bảng UniqueCodes ({orderNo}.db)
```sql
CREATE TABLE UniqueCodes (
    ID             INTEGER PRIMARY KEY AUTOINCREMENT,
    Code           TEXT UNIQUE,
    cartonCode     TEXT DEFAULT '0',
    Status         INTEGER DEFAULT 0,     -- 0: Unused, 1: Used
    ActivateDate   TEXT DEFAULT '0',
    ProductionDate TEXT DEFAULT '0',
    ActivateUser   TEXT,
    PackingDate    TEXT DEFAULT '0',
    Send_Status    TEXT DEFAULT 'Pending',
    Recive_Status  TEXT DEFAULT 'Pending'
);
```

### Bảng Carton (Carton_{orderNo}.db)
```sql
CREATE TABLE Carton (
    ID                  INTEGER PRIMARY KEY AUTOINCREMENT,
    cartonCode          TEXT DEFAULT '0',
    Start_Datetime      TEXT DEFAULT '0',
    Completed_Datetime   TEXT DEFAULT '0',
    ActivateUser        TEXT,
    ProductionDate      TEXT DEFAULT '0'
);
```

### Bảng POHistory (POHistory.db)
```sql
CREATE TABLE POHistory (
    ID             INTEGER PRIMARY KEY AUTOINCREMENT,
    PO             TEXT,
    ProductionDate TEXT,
    StartTime      TEXT,
    EndTime        TEXT,
    Status         TEXT,
    UserName       TEXT
);
```

---

## Sử Dụng - Backend (C#)

### 1. Khởi tạo

```csharp
// Import namespace
using GProject.ProductionOrderHelpers;

// Khởi tạo database (gọi 1 lần khi app start)
GProduction.Initialize();
```

### 2. CRUD PO

```csharp
// Tạo PO mới
var po = new POInfo
{
    OrderNo = "PO001",
    Site = "SITE_MASAN",
    Factory = "FACTORY_01",
    ProductionLine = "LINE_1",
    ProductionDate = "2026-07-01",
    Shift = "A",
    OrderQty = 100,
    ProductCode = "SKU001",
    ProductName = "Sản phẩm A",
    Gtin = "1234567890123",
    CustomerOrderNo = "CUST001",
    Uom = "PCS"
};

var result = GProduction.POLoader.Create(po);
if (result.IsSuccess)
{
    Console.WriteLine("Tạo PO thành công!");
}

// Lấy danh sách PO
var allPO = GProduction.POLoader.GetAll();

// Lấy PO theo orderNo
var poDetail = GProduction.POLoader.GetByOrderNo("PO001");

// Kiểm tra PO tồn tại
bool exists = GProduction.POLoader.Exists("PO001");

// Cập nhật PO
po.ProductName = "Sản phẩm A Updated";
GProduction.POLoader.Update("PO001", po);

// Xóa PO
GProduction.POLoader.Delete("PO001");
```

### 3. Tạo Database cho PO

```csharp
// Khởi tạo tất cả database files cho PO
var initResult = GProduction.POCreator.InitPO("PO001");

// Tạo N thùng trống
GProduction.POCreator.CreateEmptyCartons("PO001", 10);

// Tạo số thùng cần thiết (tự động tính theo orderQty)
GProduction.POCreator.CreateRequiredCartons("PO001", 100, 24); // 100 sp / 24 = 5 thùng
```

### 4. Activate Code (Camera Active)

```csharp
// Kích hoạt một mã
var activateResult = GProduction.POActivator.ActivateCode(
    "PO001",           // orderNo
    "CODE123",         // code
    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // activateDate
    "Operator1",       // activateUser
    "2026-07-01"      // productionDate
);

// Hủy kích hoạt
GProduction.POActivator.DeactivateCode("PO001", "CODE123");

// Ghi bản ghi từ camera
var record = new RecordData
{
    Code = "CODE123",
    PLCStatus = "PASS",
    ActivateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    ActivateUser = "Operator1"
};
GProduction.POActivator.Record("PO001", record);

// Đếm mã đã activate
int activeCount = GProduction.POActivator.GetActiveCount("PO001");
int unusedCount = GProduction.POActivator.GetUnusedCount("PO001");
```

### 5. Pack Code (Camera Packing)

```csharp
// Đóng mã vào thùng
var packResult = GProduction.POPacking.PackCode(
    "PO001",           // orderNo
    "CODE123",         // code
    "CTN001",          // cartonCode
    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // packingDate
    "Operator1",       // packingUser
    "2026-07-01"      // productionDate
);

// Gỡ mã khỏi thùng
GProduction.POPacking.UnpackCode("PO001", "CODE123");

// Đếm mã đã đóng thùng
int packedCount = GProduction.POPacking.GetPackedCount("PO001");

// Đếm mã trong một thùng
int countInCarton = GProduction.POPacking.GetCodeCountInCarton("PO001", "CTN001");
```

### 6. Quản lý Carton

```csharp
// Lấy danh sách thùng
var cartons = GProduction.POCarton.GetAll("PO001");

// Bắt đầu đóng thùng (mở thùng)
GProduction.POCarton.StartCarton("PO001", 1, "Operator1");

// Hoàn thành thùng (đóng thùng)
GProduction.POCarton.CompleteCarton("PO001", 1, "Operator1");

// Reset thùng về trạng thái trống
GProduction.POCarton.ResetCarton("PO001", 1);

// Thống kê
int totalCartons = GProduction.POCarton.GetTotalCartonCount("PO001");
int closedCartons = GProduction.POCarton.GetClosedCartonCount("PO001");
```

### 7. Lịch sử PO

```csharp
// Ghi bắt đầu chạy PO
GProduction.POHistoryManager.RecordStart("PO001", "2026-07-01", "Operator1");

// Ghi kết thúc PO
GProduction.POHistoryManager.RecordEnd("PO001");

// Lấy PO đang chạy cuối cùng
var runningPO = GProduction.POHistoryManager.GetLastRunningPO();

// Kiểm tra PO có đang chạy không
bool isRunning = GProduction.POHistoryManager.IsPORunning("PO001");
```

### 8. Sử dụng Dictionary cho lookup nhanh

```csharp
// Dictionary cho codes (O(1) lookup)
var codeDict = new Dictionary<string, CodeInfo>();

// Load tất cả codes vào dictionary
GProduction.CodeDictionaryLoader.LoadAllCodesToDictionary("PO001", codeDict);

// Tìm code nhanh
if (codeDict.TryGetValue("CODE123", out var codeInfo))
{
    Console.WriteLine($"Code: {codeInfo.Code}, Status: {codeInfo.Status}");
}

// Dictionary cho cartons
var cartonDict = new Dictionary<int, CartonInfo>();

GProduction.CartonDictionaryLoader.LoadAllCartonsToDictionary("PO001", cartonDict);

// Tìm thùng tiếp theo chưa đóng
int? nextOpenCartonId = GProduction.CartonDictionaryLoader.FindNextOpenCarton(cartonDict);
```

### 9. Nạp mã từ DataPool

```csharp
// Nạp mã từ DataPool vào PO
var loadResult = GProduction.POLoader.LoadCodesFromDataPool(
    "PO001",           // orderNo
    "1234567890123",   // gtin (DataPool name)
    limitQty: 100     // optional: giới hạn số mã
);

if (loadResult.success)
{
    Console.WriteLine($"Đã nạp {loadResult.loadedCount} mã");
}
else
{
    Console.WriteLine($"Lỗi: {loadResult.message}");
}
```

### 10. Kiểm tra & Setup Database PO

```csharp
// Kiểm tra trạng thái database của PO
var status = GProduction.POCreator.CheckPODatabaseStatus("PO001", orderQty: 100, cartonCapacity: 24);

Console.WriteLine($"PO ready: {status.IsFullyInitialized}");
Console.WriteLine($"Codes: {status.LoadedCodes}/{status.TotalCodes}");
Console.WriteLine($"Cartons: {status.CreatedCartons}/{status.RequiredCartons}");
foreach (var file in status.Files)
{
    Console.WriteLine($"{file.FileName}: {(file.Exists ? "OK" : "MISSING")}");
}

// Tự động setup PO database (tạo file, nạp mã, tạo thùng nếu thiếu)
var setupResult = GProduction.POCreator.EnsurePODatabaseReady(
    "PO001",           // orderNo
    "1234567890123",   // gtin
    100,              // orderQty
    24,               // cartonCapacity
    autoLoadCodes: true
);

if (setupResult.success)
{
    Console.WriteLine($"Setup thành công! Nạp {setupResult.codesLoaded} mã, tạo {setupResult.cartonsCreated} thùng");
}
```

---

## Sử Dụng - REST API

### Base URL
```
http://localhost:9999
```

### Authentication
API sử dụng cookie-based authentication từ Auth middleware.

### Endpoints

#### Health Check
```http
GET /api/health
```
Response:
```json
{
  "status": "OK",
  "timestamp": "2026-07-01T00:00:00"
}
```

#### PO Management

**Tạo PO mới**
```http
POST /api/po
Content-Type: application/json

{
  "orderNo": "PO001",
  "site": "SITE_MASAN",
  "factory": "FACTORY_01",
  "productionLine": "LINE_1",
  "productionDate": "2026-07-01",
  "shift": "A",
  "orderQty": 100,
  "productCode": "SKU001",
  "productName": "Sản phẩm A",
  "gtin": "1234567890123",
  "customerOrderNo": "CUST001",
  "uom": "PCS",
  "userName": "Frontend",
  "autoLoadCodes": true,
  "cartonCapacity": 24
}
```

**Lấy danh sách PO**
```http
GET /api/po/list
```

**Lấy chi tiết PO**
```http
GET /api/po/{orderNo}
```

**Kiểm tra xóa được không**
```http
GET /api/po/{orderNo}/can-delete
```

**Xóa PO**
```http
DELETE /api/po/{orderNo}
```

**Kiểm tra trạng thái Database PO**
```http
GET /api/po/{orderNo}/status
```
Response:
```json
{
  "success": true,
  "orderNo": "PO001",
  "isFullyInitialized": true,
  "files": [
    {"fileName": "PO001.db", "path": "C:/GProject/PODatabases/...", "exists": true, "fileSize": 4096},
    {"fileName": "Record_Active_PO001.db", "exists": true, "fileSize": 4096},
    {"fileName": "Record_Packing_PO001.db", "exists": true, "fileSize": 4096},
    {"fileName": "Carton_PO001.db", "exists": true, "fileSize": 4096}
  ],
  "totalCodes": 100,
  "loadedCodes": 100,
  "requiredCartons": 5,
  "createdCartons": 5,
  "message": "PO ready - all files initialized and data loaded."
}
```

**Tự động Setup Database PO**
```http
POST /api/po/{orderNo}/ensure-ready
Content-Type: application/json

{
  "autoLoadCodes": true,
  "cartonCapacity": 24
}
```
Response:
```json
{
  "success": true,
  "message": "PO ready. Loaded 100 codes, 5 cartons.",
  "codesLoaded": 100,
  "cartonsCreated": 5
}
```

#### Codes

**Lấy danh sách codes**
```http
GET /api/po/{orderNo}/codes?status=0&limit=100
```
- `status`: 0=Unused, 1=Used (optional)
- `cartonCode`: filter theo carton (optional)
- `limit`: số lượng tối đa (default: 100)

**Activate code**
```http
POST /api/po/{orderNo}/activate
Content-Type: application/json

{
  "code": "CODE123",
  "activateDate": "2026-07-01T10:00:00",
  "activateUser": "Operator1",
  "productionDate": "2026-07-01"
}
```

**Pack code vào thùng**
```http
POST /api/po/{orderNo}/pack
Content-Type: application/json

{
  "code": "CODE123",
  "cartonCode": "CTN001",
  "packingDate": "2026-07-01T10:00:00",
  "packingUser": "Operator1",
  "productionDate": "2026-07-01"
}
```

#### Cartons

**Lấy danh sách thùng**
```http
GET /api/po/{orderNo}/cartons
```

**Bắt đầu thùng**
```http
POST /api/po/{orderNo}/cartons/start
Content-Type: application/json

{
  "cartonId": 1,
  "activateUser": "Operator1"
}
```

**Hoàn thành thùng**
```http
POST /api/po/{orderNo}/cartons/complete
Content-Type: application/json

{
  "cartonId": 1,
  "activateUser": "Operator1"
}
```

#### Production Status

**Lấy trạng thái sản xuất**
```http
GET /api/production/status
```

---

## Enumerations

### e_ProductionState
| Value | Mô Tả |
|-------|--------|
| 0 | NoSelectedPO - Chưa chọn PO |
| 1 | Editing - Đang chỉnh sửa |
| 2 | CheckingPO - Đang kiểm tra PO |
| 3 | Ready - Sẵn sàng |
| 4 | Running - Đang chạy |
| 5 | Paused - Tạm dừng |
| 8 | Completed - Hoàn thành |
| 99 | Error - Lỗi |

### e_CodeStatus
| Value | Mô Tả |
|-------|--------|
| 0 | Unused - Chưa sử dụng |
| 1 | Used - Đã sử dụng/activate |

### e_CartonStatus
| Value | Mô Tả |
|-------|--------|
| 0 | Open - Đang mở |
| 1 | Closed - Đã đóng |
| -1 | Cancelled - Đã hủy |

### e_PLCStatus
| Value | Mô Tả |
|-------|--------|
| PASS | Mã hợp lệ |
| FAIL | Mã không hợp lệ |
| ERROR | Lỗi |
| TIMEOUT | Timeout |
| READFAIL | Đọc thất bại |
| FORMATERROR | Định dạng lỗi |

---

## Frontend Integration

### Environment Variables
```env
VITE_PO_API_URL=http://localhost:9999
```

### React Usage
```tsx
import poApi from '@/services/poApi';
import type { POInfo, CreatePORequest } from '@/types/po';

// Tạo PO
const createPO = async () => {
  const request: CreatePORequest = {
    orderNo: "PO001",
    orderQty: 100,
    gtin: "1234567890123",
    // ...
  };
  
  const result = await poApi.createPO(request);
  if (result.success) {
    console.log(`Created ${result.orderNo} with ${result.loadedCodesCount} codes`);
  }
};

// Lấy danh sách PO
const loadPOs = async () => {
  const pos = await poApi.getAllPOs();
  console.log(pos);
};

// Lấy chi tiết PO
const loadDetail = async () => {
  const po = await poApi.getPO("PO001");
  console.log(po.stats); // activeCodes, packedCodes, cartonCount...
};

// Activate code
const activate = async () => {
  const result = await poApi.activateCode("PO001", "CODE123", "Frontend");
  console.log(result.success);
};

// Pack code
const pack = async () => {
  const result = await poApi.packCode("PO001", "CODE123", "CTN001", "Frontend");
  console.log(result.success);
};

// Carton operations
const startCarton = async () => {
  await poApi.startCarton("PO001", 1, "Frontend");
};

const completeCarton = async () => {
  await poApi.completeCarton("PO001", 1, "Frontend");
};
```

---

## Ví dụ Workflow Hoàn Chỉnh

```csharp
// 1. Tạo và khởi tạo PO
var po = new POInfo { OrderNo = "PO001", OrderQty = 100, Gtin = "1234567890123", ... };
GProduction.POLoader.Create(po);
GProduction.POCreator.InitPO("PO001");
GProduction.POCreator.CreateRequiredCartons("PO001", 100, 24);
GProduction.POHistoryManager.RecordStart("PO001", "2026-07-01", "Operator");

// 2. Load codes từ DataPool
GProduction.POLoader.LoadCodesFromDataPool("PO001", "1234567890123");

// 3. Load vào Dictionary để lookup nhanh
var codeDict = new Dictionary<string, CodeInfo>();
GProduction.CodeDictionaryLoader.LoadAllCodesToDictionary("PO001", codeDict);

var cartonDict = new Dictionary<int, CartonInfo>();
GProduction.CartonDictionaryLoader.LoadAllCartonsToDictionary("PO001", cartonDict);

// 4. Khi camera active đọc được code
string scannedCode = ReadCodeFromCamera();
if (codeDict.TryGetValue(scannedCode, out var codeInfo) && codeInfo.Status == 0)
{
    // Activate code
    GProduction.POActivator.ActivateCode("PO001", scannedCode, 
        DateTime.Now.ToString(), "Operator1", "2026-07-01");
    
    // Update dictionary
    codeInfo.Status = 1;
    
    // Tìm thùng tiếp theo
    int? nextCartonId = GProduction.CartonDictionaryLoader.FindNextOpenCarton(cartonDict);
    if (nextCartonId.HasValue)
    {
        // Bắt đầu thùng nếu chưa bắt đầu
        var carton = cartonDict[nextCartonId.Value];
        if (carton.StartDatetime == "0")
        {
            GProduction.POCarton.StartCarton("PO001", nextCartonId.Value, "Operator1");
        }
    }
}

// 5. Khi camera packing đọc được code
GProduction.POPacking.PackCode("PO001", scannedCode, "CTN001", 
    DateTime.Now.ToString(), "Operator1", "2026-07-01");

// 6. Hoàn thành PO
GProduction.POHistoryManager.RecordEnd("PO001");
```

---

## Cấu Hình Nâng Cao

### Thay đổi đường dẫn gốc
```csharp
GProject.ProductionOrderHelpers.Config.BasePath = "D:/MyPOData";
GProduction.Initialize();
```

### Các cấu hình SQLite
- WAL mode enabled
- Synchronous = NORMAL
- Cache size = 10000
- Temp store = memory

---

## Lưu Ý Quan Trọng

1. **OrderQty phải > 24** - Đây là yêu cầu bắt buộc khi tạo PO
2. **Codes không thể xóa** - Nếu đã activate code, PO không thể xóa
3. **Auto-load codes** - Khi tạo PO với `autoLoadCodes: true`, hệ thống sẽ tự động nạp mã từ DataPool tương ứng với GTIN
4. **Dictionary cho lookup nhanh** - Nên sử dụng Dictionary thay vì query database trong vòng lặp production

---

## Version
- Version: 1.1.0
- Last Updated: 2026-07-02
- Changelog:
  - 1.1.0: Thêm CheckPODatabaseStatus() và EnsurePODatabaseReady() để kiểm tra và tự động setup PO database
  - 1.0.0: Phiên bản đầu tiên
