---
name: ProductionHelper GProject Plan
overview: "Xây dựng hệ thống quản lý Production Order (PO) cho GProject bao gồm: ProductionHelper class, API endpoints, và tích hợp frontend. Hệ thống quản lý toàn bộ vòng đời PO: tạo PO, nạp mã từ DataPool, tracking CodeReader Active/Packing, và quản lý thùng carton."
todos:
  - id: phase1-core
    content: "Tạo ProductionHelper Phase 1: Config.cs, Enums.cs, Models.cs"
    status: completed
  - id: poloader
    content: Tạo POLoader.cs - CRUD cho PO List
    status: completed
  - id: pocreator
    content: Tạo POCreator.cs - Tạo database PO
    status: completed
  - id: poactivator
    content: Tạo POActivator.cs - Camera active
    status: completed
  - id: popacking
    content: Tạo POPacking.cs - Camera packing
    status: completed
  - id: pocarton
    content: Tạo POCarton.cs - Quản lý thùng
    status: completed
  - id: pohistory
    content: Tạo POHistoryManager.cs - Lịch sử PO
    status: completed
  - id: dict-loaders
    content: Tạo DictionaryLoaders - Code & Carton
    status: completed
  - id: gproduction
    content: Tạo GProduction.cs - Main wrapper
    status: completed
  - id: phase2-api
    content: "Phase 2: Mở rộng API Server với PO endpoints"
    status: completed
  - id: phase3-ui
    content: "Phase 3: Frontend integration (làm sau)"
    status: completed
isProject: false
---

# Kế Hoạch Xây Dựng ProductionHelper Cho GProject

## Tổng Quan
Xây dựng hệ thống quản lý Production Order (PO) cho GProject dựa trên:
- Design document: `GProject/ProductionOrderHelpers/plan.md`
- Reference implementation: `VNQR/Helpers/po.cs`
- MASAN reference: `MASAN-SERIALIZATION/Production/ProductionHelper.cs`

---

## Phase 1: ProductionHelper Class (PRIORITY - Làm trước)

### 1.1 Tạo Cấu Trúc Thư Mục
```
GProject/ProductionOrderHelpers/
├── plan.md              (đã có)
├── codecu.md            (đã có)
├── Config.cs           - Cấu hình đường dẫn, SQL schemas, SQLite settings
├── Models.cs           - POInfo, CodeData, RecordData, CartonData, Counter classes
├── Enums.cs             - e_ProductionState, e_CodeStatus, e_CartonStatus, etc.
├── POLoader.cs          - CRUD cho PO List, query codes
├── POCreator.cs        - Tạo database files cho PO
├── POActivator.cs      - Xử lý camera active (kích hoạt mã)
├── POPacking.cs         - Xử lý camera packing (đóng thùng)
├── POCarton.cs          - Quản lý thùng carton
├── POHistoryManager.cs  - Lịch sử chạy PO
├── CodeDictionaryLoader.cs - Load codes vào Dictionary cho lookup nhanh
├── CartonDictionaryLoader.cs - Load cartons vào Dictionary
└── GProduction.cs      - Main class wrapper (tổng hợp tất cả)
```

### 1.2 Chi Tiết Từng File

#### `Config.cs` - Cấu Hình
- Base path: `C:/GProject/PODatabases`
- Cấu trúc: `<basePath>/yyyy-MM/{gtin}/`
- Các database files:
  - `PO_List.db` - Danh sách PO
  - `POHistory.db` - Lịch sử chạy
  - `{orderNo}.db` - UniqueCodes
  - `Record_Active_{orderNo}.db` - Bản ghi camera active
  - `Record_Packing_{orderNo}.db` - Bản ghi camera packing
  - `Carton_{orderNo}.db` - Thông tin thùng
- SQLite PRAGMA: WAL mode, synchronous=NORMAL, cache_size=10000

#### `Models.cs` - Data Models
```csharp
public class POInfo {
    string OrderNo, Site, Factory, ProductionLine, ProductionDate, Shift,
           LotNumber, ProductCode, ProductName, Gtin, CustomerOrderNo, Uom,
           CreatedTime, ModifiedTime;
    int OrderQty;
}

public class ProductCounter {
    int PassCount, FailCount, DuplicateCount, ReadFailCount, NotFoundCount, ErrorCount,
        TotalCount, TotalCartonCount, ActivatedCartonCount, ErrorCartonCount,
        CartonID, CartonCapacity;
    string CartonCode;
}

public class CodeData {
    int Id;
    string Code, CartonCode, Status, ActivateDate, ProductionDate, ActivateUser, PackingDate;
}
```

#### `Enums.cs` - Enumerations
```csharp
public enum e_ProductionState { Editing, Ready, Running, Paused, Completed, Error }
public enum e_CodeStatus { Unused = 0, Used = 1 }
public enum e_CartonStatus { Open = 0, Closed = 1, Cancelled = -1 }
public enum e_PLCStatus { PASS, FAIL, ERROR, TIMEOUT, READFAIL, FORMATERROR }
```

#### `POLoader.cs` - CRUD PO
- `GetAll()` - Lấy danh sách tất cả PO
- `GetByOrderNo(orderNo)` - Lấy PO theo mã
- `GetByGTIN(gtin)` - Lấy PO theo GTIN
- `Create(poInfo)` - Tạo PO mới
- `Update(orderNo, poInfo)` - Cập nhật PO
- `Delete(orderNo)` - Xóa PO
- `Exists(orderNo)` - Kiểm tra PO tồn tại
- `LoadCodesFromDataPool(orderNo, gtin)` - Nạp mã từ DataPool

#### `POCreator.cs` - Tạo Database PO
- `InitPO(orderNo)` - Khởi tạo 4 database files
- `CreateEmptyCartons(orderNo, count)` - Tạo N thùng trống

#### `POActivator.cs` - Camera Active
- `Record(orderNo, data)` - Ghi bản ghi active
- `ActivateCode(orderNo, code, activateDate, user)` - Kích hoạt mã
- `DeactivateCode(orderNo, code)` - Hủy kích hoạt
- `GetActiveCount(orderNo)` - Đếm mã đã active

#### `POPacking.cs` - Camera Packing
- `Record(orderNo, data)` - Ghi bản ghi packing
- `PackCode(orderNo, code, cartonCode)` - Đóng mã vào thùng
- `UnpackCode(orderNo, code)` - Gỡ mã khỏi thùng
- `GetPackedCount(orderNo)` - Đếm mã đã đóng thùng

#### `POCarton.cs` - Quản Lý Thùng
- `Create/Update/Delete` - CRUD thùng
- `StartCarton(cartonId)` - Bắt đầu đóng thùng
- `CompleteCarton(cartonId)` - Hoàn thành thùng
- `ResetCarton(cartonId)` - Reset thùng
- `GetTotalCartonCount/GetClosedCartonCount` - Thống kê

#### `POHistoryManager.cs` - Lịch Sử
- `RecordStart(orderNo)` - Ghi bắt đầu PO
- `RecordEnd(orderNo)` - Ghi kết thúc PO
- `GetLastRunningPO()` - Lấy PO đang chạy cuối
- `GetLastPO()` - Lấy PO cuối cùng

#### `CodeDictionaryLoader.cs` - Dictionary Codes
- `LoadAllCodesToDictionary(orderNo, dictionary)` - Load tất cả codes
- `LoadUnusedCodesToDictionary(orderNo, dictionary)` - Load codes chưa dùng
- `ActivateCodeInDatabase(orderNo, code, ...)` - Update DB
- `PackCodeInDatabase(orderNo, code, ...)` - Update DB

#### `CartonDictionaryLoader.cs` - Dictionary Cartons
- `LoadAllCartonsToDictionary(orderNo, dictionary)` - Load tất cả thùng
- `CreateAndLoadCartons(orderNo, count, dictionary)` - Tạo và load
- `Start/Complete/ResetCartonInDatabase` - Update DB

#### `GProduction.cs` - Main Wrapper
- Static class đồng nhất, dùng: `GProduction.POLoader.GetAll()`

---

## Phase 2: API Endpoints

### Tạo `GProjectApiServer.cs` (mở rộng từ GProjectApiServer hiện tại)

#### PO Endpoints
- `GET /api/po/list` - Danh sách PO
- `GET /api/po/{orderNo}` - Chi tiết PO
- `POST /api/po` - Tạo PO mới
- `PUT /api/po/{orderNo}` - Cập nhật PO
- `DELETE /api/po/{orderNo}` - Xóa PO
- `GET /api/po/{orderNo}/can-delete` - Kiểm tra xóa được không

#### Production Endpoints
- `GET /api/production/status` - Trạng thái sản xuất
- `POST /api/production/start` - Bắt đầu sản xuất
- `POST /api/production/stop` - Dừng sản xuất
- `POST /api/production/reset` - Reset PO

#### Codes Endpoints
- `GET /api/po/{orderNo}/codes` - Danh sách codes
- `GET /api/po/{orderNo}/codes/{code}` - Chi tiết code
- `POST /api/po/{orderNo}/activate` - Kích hoạt code
- `POST /api/po/{orderNo}/pack` - Đóng code vào thùng

#### Carton Endpoints
- `GET /api/po/{orderNo}/cartons` - Danh sách thùng
- `GET /api/po/{orderNo}/cartons/{id}` - Chi tiết thùng
- `POST /api/po/{orderNo}/cartons/start` - Bắt đầu thùng
- `POST /api/po/{orderNo}/cartons/complete` - Hoàn thành thùng

#### Audit Endpoints
- `GET /api/audit` - Logs hoạt động
- `GET /api/audit/{type}` - Logs theo loại

---

## Phase 3: Frontend Integration (iot-scada-admin-panel)

Sau khi hoàn thành Phase 1 & 2:
- Tạo component PO Management
- Tạo Dashboard Production
- Kết nối API để quản lý PO

---

## Database Schema

### PO_List.db - Bảng PO
```sql
CREATE TABLE PO (
    orderNo TEXT PRIMARY KEY,
    site, factory, productionLine, productionDate, shift,
    orderQty INTEGER, lotNumber, productCode, productName,
    gtin, customerOrderNo, uom,
    CreatedTime, ModifiedTime TEXT
);
CREATE INDEX IDX_PO_gtin ON PO(gtin);
PRAGMA journal_mode=WAL;
```

### {orderNo}.db - UniqueCodes
```sql
CREATE TABLE UniqueCodes (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT UNIQUE,
    cartonCode TEXT DEFAULT '0',
    Status INTEGER DEFAULT 0,
    ActivateDate, ProductionDate TEXT,
    ActivateUser, PackingDate TEXT
);
CREATE INDEX IDX_UC_Status ON UniqueCodes(Status);
CREATE INDEX IDX_UC_cartonCode ON UniqueCodes(cartonCode);
PRAGMA journal_mode=WAL;
```

### Record_Active_{orderNo}.db
```sql
CREATE TABLE Records (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT, cartonCode TEXT, Status TEXT,
    PLC_Status TEXT, ActivateDate, ActivateUser, ProductionDate TEXT
);
PRAGMA journal_mode=WAL;
```

### Carton_{orderNo}.db
```sql
CREATE TABLE Carton (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    cartonCode TEXT, Start_Datetime, Completed_Datetime,
    ActivateUser, ProductionDate TEXT
);
PRAGMA journal_mode=WAL;
```

### POHistory.db
```sql
CREATE TABLE POHistory (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    PO TEXT, ProductionDate, StartTime, EndTime,
    Status TEXT, UserName TEXT
);
CREATE INDEX IDX_PH_Status ON POHistory(Status);
PRAGMA journal_mode=WAL;
```

---

## Thứ Tự Thực Hiện

1. **Config.cs** - Cấu hình đường dẫn, schemas
2. **Enums.cs** - Enumerations
3. **Models.cs** - Data models
4. **POLoader.cs** - CRUD PO List
5. **POCreator.cs** - Tạo database PO
6. **POActivator.cs** - Camera active
7. **POPacking.cs** - Camera packing
8. **POCarton.cs** - Quản lý thùng
9. **POHistoryManager.cs** - Lịch sử
10. **CodeDictionaryLoader.cs** - Dictionary codes
11. **CartonDictionaryLoader.cs** - Dictionary cartons
12. **GProduction.cs** - Wrapper class chính
13. **Mở rộng GProjectApiServer** - Thêm endpoints
14. **Frontend integration** - (làm sau)

---

## Files Cần Tạo Mới

| File | Mô Tả |
|------|--------|
| `GProject/ProductionOrderHelpers/Config.cs` | Cấu hình đường dẫn, SQL schemas |
| `GProject/ProductionOrderHelpers/Models.cs` | POInfo, CodeData, CartonData, Counter |
| `GProject/ProductionOrderHelpers/Enums.cs` | Enumerations |
| `GProject/ProductionOrderHelpers/POLoader.cs` | CRUD PO List |
| `GProject/ProductionOrderHelpers/POCreator.cs` | Tạo database PO |
| `GProject/ProductionOrderHelpers/POActivator.cs` | Camera active |
| `GProject/ProductionOrderHelpers/POPacking.cs` | Camera packing |
| `GProject/ProductionOrderHelpers/POCarton.cs` | Quản lý thùng |
| `GProject/ProductionOrderHelpers/POHistoryManager.cs` | Lịch sử PO |
| `GProject/ProductionOrderHelpers/CodeDictionaryLoader.cs` | Dictionary codes |
| `GProject/ProductionOrderHelpers/CartonDictionaryLoader.cs` | Dictionary cartons |
| `GProject/ProductionOrderHelpers/GProduction.cs` | Main wrapper |

## Files Cần Cập Nhật

| File | Mô Tả |
|------|--------|
| `GProject/GProjectApiServer.cs` | Thêm PO endpoints |
| `GProject/GProject.csproj` | Thêm reference System.Data.SQLite |
| `GProject/Program.cs` | Initialize ProductionHelper |