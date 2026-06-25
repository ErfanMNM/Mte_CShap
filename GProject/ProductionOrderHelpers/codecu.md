

Tôi đã đọc và phân tích xong file `ProductionHelper.cs`. Dưới đây là tổng quan chi tiết:

---

## Phân Tích File `ProductionHelper.cs`

### 1. Tổng Quan
- **Kích thước**: ~120KB (~2422 dòng)
- **Namespace**: `MASAN_SERIALIZATION.Production`
- **Ngôn ngữ**: C# (.NET)
- **Database**: SQLite
- **Mục đích**: Quản lý Production Order (PO) và mã serialization cho dây chuyền sản xuất

---

### 2. Cấu Trúc Class Chính

#### **Class `ProductionOrder`** (dòng 32-2323)

| Thành phần | Mô tả |
|------------|-------|
| **Properties** | orderNo, site, factory, productionLine, productionDate, shift, orderQty, lotNumber, gtin, productCode, productName, customerOrderNo, uom, cartonSize |
| **Counter Classes** | `Product_Counter`, `AWS_Send_Counter`, `AWS_Recived_Counter` |
| **DAO Classes** | `GetfromMES`, `GetDataPO`, `PostDB` |

---

### 3. Cơ Chế Lưu Trữ Mã Theo GTIN (Điểm quan trọng)

```
Cấu trúc thư mục: C:/MasanSerialization_v2/PODatabases/yyyy-MM/GTIN/
Ví dụ: 2025-01/8931234567890/
├── PO001.db     (database PO001, chứa codes từ GTIN)
├── PO002.db     (database PO002, cũng chứa codes từ GTIN)
├── Record_*.db  (bản ghi sản xuất)
├── carton_*.db  (thông tin thùng)
└── Send/Recive_AWS_*.db (log gửi/nhận AWS)
```

**Điểm mấu chốt**: Nhiều PO có cùng GTIN sẽ **dùng chung 1 bộ mã**. Khi tạo PO mới, hệ thống lọc bỏ các mã đã được activate ở PO khác cùng GTIN.

---

### 4. Các Inner Classes

#### **A. `GetfromMES`** - Đọc từ MES Server
- `ProductionOrder_List()` - Danh sách PO
- `ProductionOrder_Detail()` - Chi tiết PO từ JSON
- `Get_Unique_Code_MES_Count()` - Đếm mã theo GTIN
- `Get_Unique_Codes_MES()` - Lấy danh sách mã theo GTIN
- `MES_Load_OrderNo_ToComboBox()` - Load danh sách PO lên ComboBox

#### **B. `GetDataPO`** - Đọc từ Database Local
- `getCodeInfo()` - Lấy thông tin mã
- `CheckCodeActivatedInOtherPO()` - Kiểm tra mã đã activate ở PO khác chưa
- `GetAllActivatedCodesFromOtherPOs()` - Lấy tất cả mã đã activate từ các PO cùng GTIN
- `Get_Cartons()` - Lấy danh sách thùng
- `Get_Codes_Send()` / `Get_Codes_Send_Failed()` - Lấy mã đã gửi AWS
- `Is_PO_Deleted()` / `Is_PO_Completed()` - Kiểm tra trạng thái PO

#### **C. `PostDB`** - Ghi dữ liệu
- `Update_Active_Status()` - Cập nhật trạng thái kích hoạt mã
- `Insert_Record()` - Ghi bản ghi sản xuất
- `Insert_Carton()` - Ghi thông tin thùng
- `Update_Carton()` - Cập nhật thùng
- `Reset_Carton_To_Unassigned()` - Reset thùng về chưa đóng
- `Insert_Record_Camera_Sub()` - Ghi bản ghi camera phụ

---

### 5. Counter Classes

**`Product_Counter`**:
```csharp
- passCount, failCount, duplicateCount
- readfailCount, notfoundCount, errorCount
- totalCount, totalCartonCount
- activatedCartonCount, errorCartonCount
- cartonID, carton_Packing_Code, carton_Packing_ID
```

**`AWS_Send_Counter`**: pendingCount, sentCount, failedCount

**`AWS_Recived_Counter`**: waitingCount, recivedCount

---

### 6. Database Tables Được Tạo

| Table | File | Mục đích |
|-------|------|----------|
| `UniqueCodes` | `{orderNo}.db` | Lưu mã serialization (Code, Status, cartonCode, ActivateDate, Send/Recive_Status) |
| `Records` | `Record_{orderNo}.db` | Bản ghi sản xuất (Code, Status, PLC_Status) |
| `Records_CameraSub` | `Record_CameraSub_{orderNo}.db` | Bản ghi camera phụ |
| `Carton` | `carton_{orderNo}.db` | Thông tin thùng đóng gói |
| `Records` (AWS Send) | `Send_AWS_Record_{orderNo}.db` | Log gửi AWS |
| `Records` (AWS Recv) | `Recive_AWS_Record_{orderNo}.db` | Log nhận AWS |
| `PO` | `POLog.db` | Log các action trên PO |

---

### 7. Enums Quan Trọng

```csharp
e_Production_State: NoSelectedPO, Start, Running, Completed, Pause, Error...
e_Production_Status: Pass=1, Fail=-1, Duplicate=-3, ReadFail=-2, NotFound=-4, Error=-5
e_AWS_Send_Status: Pending, Sent, Failed
e_AWS_Recive_Status: Waiting, Sent=200, Error, Error_404, Error_500...
e_Production_Order_Log_Type: Deleted, Completed, Create, Update, UpdateProductionDate, Error
```

---

### 8. Helper Method Quan Trọng

```csharp
GetOrderBasePath(orderNo)  // Tạo đường dẫn theo cấu trúc yyyy-MM/GTIN/
GetPOInfo(orderNo)         // Lấy productionDate và GTIN từ JSON
GetGTIN(orderNo)           // Lấy GTIN từ orderNo
```

---

### 9. Các Bước Khi Tạo Database Mới (trong `Check_Database_File`)

1. Kiểm tra file JSON PO tồn tại
2. Lấy GTIN từ orderNo
3. Tạo thư mục theo cấu trúc `yyyy-MM/GTIN/`
4. Tạo database `{orderNo}.db` với bảng `UniqueCodes`
5. **Lọc mã đã dùng** từ các PO cùng GTIN
6. Insert các mã còn lại vào database
7. Tạo các database phụ (Record, Carton, AWS Send/Receive)

---

### 10. Đặc Điểm Kỹ Thuật

- **WAL Mode** cho SQLite để tăng hiệu suất
- **PRAGMA synchronous = NORMAL** 
- **PRAGMA cache_size = 1000000**
- **PRAGMA temp_store = memory**
- Sử dụng **Transaction** cho batch insert

---

### So Sánh Với Codebase Hiện Tại

Nhìn vào workspace của bạn (`Mte_CShap`), tôi thấy bạn đang xây dựng một hệ thống tương tự trong `GProject/ProductionOrderHelpers/`. File tham khảo này cho thấy:
- Hệ thống MASAN dùng **SQLite** với cấu trúc thư mục theo **tháng/GTIN**
- Nhiều PO cùng GTIN chia sẻ mã → cần cơ chế lọc mã đã dùng
- Có hệ thống **AWS Send/Receive** tracking
- Có **Camera Sub** recording

Bạn muốn tôi phân tích thêm phần nào, hoặc muốn tôi giúp implement tính năng cụ thể nào từ file này vào codebase của bạn không?





Đây là file `po.cs` trong project VNQR của bạn. Tôi sẽ phân tích chi tiết.

---

## Phân Tích File `VNQR/Helpers/po.cs`

### 1. Tổng Quan
- **Kích thước**: ~1687 dòng
- **Namespace**: `VNQR.Helpers`
- **Pattern**: Static classes (singleton-like, không cần khởi tạo instance)
- **Database**: SQLite
- **Cơ chế chia sẻ mã theo GTIN**: Có (giống MASAN)

---

### 2. Cấu Trúc Class

```
po (static class)
├── Config (static nested class)     // Cấu hình đường dẫn, SQL schemas
├── Models (nested classes)          // POInfo, CodeData, RecordData, CartonData, Enums
├── POLoader                        // CRUD cho PO List
├── POCreator                       // Tạo database PO
├── POActivator                     // Active codes (Camera Active)
├── POPacking                       // Pack codes vào carton (Camera Packing)
├── POCarton                        // Quản lý thùng carton
├── POHistoryManager                // Lịch sử chạy PO
├── POUpdater                       // Cập nhật PO
├── CodeDictionaryLoader            // Load codes vào Dictionary (lookup nhanh)
└── CartonDictionaryLoader          // Load cartons vào Dictionary
```

---

### 3. So Sánh VNQR vs MASAN

| Khía cạnh | MASAN (`ProductionHelper.cs`) | VNQR (`po.cs`) |
|-----------|------------------------------|----------------|
| **Pattern** | Instance class với inner classes | Pure static classes |
| **PO List** | SQLite file (`po1.db`) | SQLite file (`PO_List.db`) |
| **GTIN Code Path** | `Server_Service/codes_json/GTIN_{gtin}.json` | `DataPool/{gtin}.vnqrdb` |
| **Code Source** | JSON files từ MES | DataPool SQLite database |
| **Record Tables** | `Records`, `Records_CameraSub` | `Record_Active`, `Record_Packing` (tách riêng) |
| **AWS Tracking** | `Send_AWS_Record`, `Recive_AWS_Record` | Chưa có |
| **Carton Tracking** | `carton_{orderNo}.db` | `Carton_{orderNo}.db` |
| **History** | `POLog.db` | `POHistory.db` |
| **Dictionary** | Không có | Có (`CodeDictionaryLoader`) |

---

### 4. Các Điểm Mạnh Của `po.cs`

#### 4.1 Static Classes - Dễ sử dụng
```csharp
po.Config.BasePath = "C:/VNQR/PODatabases";
po.POLoader.GetAll();
po.POCreator.InitPO("PO001");
po.POActivator.Record("PO001", data);
po.POPacking.Record("PO001", data);
```

#### 4.2 Tách Record Theo Chức Năng
```csharp
Record_Active_{orderNo}.db     // Camera Active (quét mã sản phẩm)
Record_Packing_{orderNo}.db    // Camera Packing (đóng thùng)
```

#### 4.3 Dictionary Loader - Lookup Nhanh
```csharp
// Load codes vào Dictionary để lookup O(1) thay vì query SQLite
CodeDictionaryLoader.LoadAllCodesToDictionary(orderNo, dictionary);
```

#### 4.4 Enums Rõ Ràng
```csharp
public enum e_CartonStatus { Open = 0, Closed = 1, Cancelled = -1 }
public enum e_PLCStatus { PASS, FAIL, ERROR, TIMEOUT, READFAIL, FORMATERROR }
public enum e_CodeStatus { Unused = 0, Used = 1 }
public enum e_AWSSendStatus { Pending, Sent, Failed }
public enum e_AWSReceiveStatus { Waiting, Pending, Sent=200, Error, Error404, ... }
```

---

### 5. Cấu Trúc Database Files

```
C:/VNQR/PODatabases/
├── PO_List.db                    # Danh sách PO
├── POHistory.db                  # Lịch sử chạy PO
└── yyyy-MM/
    └── {gtin}/
        ├── {orderNo}.db          # UniqueCodes (mã sản phẩm)
        ├── Record_Active_{orderNo}.db   # Bản ghi camera active
        ├── Record_Packing_{orderNo}.db   # Bản ghi camera packing
        └── Carton_{orderNo}.db   # Thông tin thùng
```

---

### 6. Database Schemas Chi Tiết

#### 6.1 PO List (`PO_List.db`)
```sql
CREATE TABLE PO (
    orderNo TEXT PRIMARY KEY,
    site, factory, productionLine, productionDate, shift,
    orderQty, lotNumber, productCode, productName,
    gtin, customerOrderNo, uom,
    CreatedTime, ModifiedTime
);
CREATE INDEX IDX_PO_gtin ON PO(gtin);
```

#### 6.2 UniqueCodes (`{orderNo}.db`)
```sql
CREATE TABLE UniqueCodes (
    ID INTEGER PRIMARY KEY,
    Code TEXT UNIQUE,
    cartonCode TEXT DEFAULT '0',
    Status INTEGER DEFAULT 0,        -- 0: Unused, 1: Used
    ActivateDate, ProductionDate,
    ActivateUser,
    PackingDate,
    Send_Status, Recive_Status
);
CREATE INDEX IDX_UC_Status ON UniqueCodes(Status);
CREATE INDEX IDX_UC_cartonCode ON UniqueCodes(cartonCode);
```

#### 6.3 Records
```sql
-- Record_Active (Camera Active)
CREATE TABLE Records (
    Code, cartonCode, Status, PLC_Status,
    ActivateDate, ActivateUser, ProductionDate
);

-- Record_Packing (Camera Packing)  
CREATE TABLE Records (
    Code, cartonCode, Status, PLC_Status,
    PackingDate, PackingUser, ProductionDate
);
```

#### 6.4 Carton (`Carton_{orderNo}.db`)
```sql
CREATE TABLE Carton (
    ID INTEGER PRIMARY KEY,
    cartonCode, Start_Datetime, Completed_Datetime,
    ActivateUser, ProductionDate
);
```

---

### 7. Các Tính Năng Chính

| Class | Phương thức | Mô tả |
|-------|-------------|-------|
| **POLoader** | `GetAll()` | Lấy danh sách PO |
| | `GetByOrderNo(orderNo)` | Lấy PO theo orderNo |
| | `GetByGTIN(gtin)` | Lấy PO theo GTIN |
| | `Create(po)`, `Update()`, `Delete()` | CRUD PO |
| | `LoadCodesFromDataPool()` | Nạp mã từ DataPool vào PO |
| | `GetCodeCount()`, `CodeExists()` | Query codes |
| **POCreator** | `InitPO()` | Tạo 4 database files cho PO |
| | `CreateEmptyCartons()` | Tạo N thùng trống |
| **POActivator** | `Record()` | Ghi bản ghi camera active |
| | `ActivateCode()` | Active một mã |
| | `DeactivateCode()` | Hủy active |
| **POPacking** | `Record()` | Ghi bản ghi camera packing |
| | `PackCode()` | Đóng mã vào thùng |
| | `UnpackCode()` | Gỡ mã khỏi thùng |
| **POCarton** | `Create()`, `Update()`, `Delete()` | CRUD carton |
| | `StartCarton()`, `CompleteCarton()` | Bắt đầu/hoàn thành thùng |
| | `ResetCarton()` | Reset thùng về trống |
| | `GetTotalCartonCount()`, `GetClosedCartonCount()` | Thống kê |
| **POHistoryManager** | `RecordStart()`, `RecordEnd()` | Ghi lịch sử chạy PO |
| **CodeDictionaryLoader** | `LoadAllCodesToDictionary()` | Load codes vào Dictionary |
| **CartonDictionaryLoader** | `LoadAllCartonsToDictionary()` | Load cartons vào Dictionary |

---

### 8. Điểm Khác Biệt Quan Trọng

#### MASAN:
- Lấy codes từ **JSON files** (`GTIN_{gtin}.json`) - server MES
- Lọc codes đã dùng từ **tất cả PO cùng GTIN** trong folder
- Chưa có Dictionary cho lookup nhanh

#### VNQR:
- Lấy codes từ **DataPool SQLite** (`{gtin}.vnqrdb`) - local database
- Khi nạp mã, update **cả DataPool** lẫn PO database
- Có Dictionary Loader cho lookup O(1)
- Tách riêng Record_Active và Record_Packing

---

### 9. Cái Cần Thêm (So Với MASAN)

Dựa trên MASAN, `po.cs` còn thiếu:

| Tính năng | MASAN | VNQR | Ưu tiên |
|------------|-------|------|---------|
| AWS Send/Receive tracking | Có | Chưa | Cao |
| Camera Sub (camera phụ) | Có | Chưa | Cao |
| Kiểm tra mã trùng across PO | Có | Chưa | Cao |
| PO Log (các action) | Có | Chưa | Trung |

---

### 10. Kết Luận

`po.cs` là phiên bản **sạch hơn, modular hơn** so với `ProductionHelper.cs` của MASAN:

**Ưu điểm:**
- Static classes → không cần khởi tạo
- Schema rõ ràng, có indexes
- Dictionary Loader cho performance
- Tách Record theo chức năng (Active vs Packing)

**Cần bổ sung:**
- AWS Send/Receive tracking
- Cross-PO duplicate check
- PO Action logging

Bạn muốn tôi hỗ trợ thêm phần nào, như thêm AWS tracking hay cross-PO duplicate check không?





## So Sánh PO Keys Giữa 2 Files

| STT | Key | MASAN (`ProductionHelper.cs`) | VNQR (`po.cs`) | Ghi chú |
|-----|-----|----------------------------|----------------|---------|
| 1 | orderNo | ✅ | ✅ (OrderNo) | Primary Key |
| 2 | site | ✅ | ✅ (Site) | |
| 3 | factory | ✅ | ✅ (Factory) | |
| 4 | productionLine | ✅ | ✅ (ProductionLine) | |
| 5 | productionDate | ✅ | ✅ (ProductionDate) | |
| 6 | shift | ✅ | ✅ (Shift) | |
| 7 | orderQty | ✅ | ✅ (OrderQty) | |
| 8 | lotNumber | ✅ | ✅ (LotNumber) | |
| 9 | productCode | ✅ | ✅ (ProductCode) | |
| 10 | productName | ✅ | ✅ (ProductName) | |
| 11 | gtin | ✅ | ✅ (Gtin) | Quan trọng - dùng chung mã |
| 12 | customerOrderNo | ✅ | ✅ (CustomerOrderNo) | |
| 13 | uom | ✅ | ✅ (Uom) | Đơn vị (PCS, KG...) |
| 14 | cartonSize | ✅ | ❌ | Kích thước thùng |
| 15 | totalCZCode | ✅ | ❌ | Tổng số mã CZ |
| 16 | counter | ✅ | ❌ | Product_Counter, AWS counters |
| 17 | CreatedTime | ❌ | ✅ | Thời gian tạo PO |
| 18 | ModifiedTime | ❌ | ✅ | Thời gian sửa cuối |

---

### Database Tables Chi Tiết

#### MASAN - `PO` Table (trong `POLog.db`)
```sql
CREATE TABLE PO (
    ID          INTEGER PRIMARY KEY,
    orderNO     TEXT NOT NULL,
    productionDate TEXT NOT NULL,
    Action      TEXT NOT NULL,      -- Create, Update, Delete, Completed
    UserName    TEXT NOT NULL,
    Counter     JSON,              -- Serialized counter object
    Timestamp   TEXT NOT NULL,
    Timeunix    INTEGER NOT NULL
);
```

#### VNQR - `PO` Table (trong `PO_List.db`)
```sql
CREATE TABLE PO (
    orderNo         TEXT PRIMARY KEY,
    site            TEXT DEFAULT '',
    factory         TEXT DEFAULT '',
    productionLine  TEXT DEFAULT '',
    productionDate  TEXT DEFAULT '',
    shift           TEXT DEFAULT '',
    orderQty        INTEGER DEFAULT 0,
    lotNumber       TEXT DEFAULT '',
    productCode     TEXT DEFAULT '',
    productName     TEXT DEFAULT '',
    gtin            TEXT DEFAULT '',
    customerOrderNo TEXT DEFAULT '',
    uom             TEXT DEFAULT '',
    CreatedTime     TEXT DEFAULT '',
    ModifiedTime    TEXT DEFAULT ''
);
CREATE INDEX IDX_PO_gtin ON PO(gtin);
```

---

### Keys Chỉ Có Ở MASAN

| Key | Kiểu | Mô tả |
|-----|------|-------|
| `cartonSize` | string | Kích thước thùng carton |
| `totalCZCode` | string | Tổng số mã CZ từ MES |
| `counter` | object | Chứa: `passCount`, `failCount`, `duplicateCount`, `readfailCount`, `notfoundCount`, `errorCount`, `totalCount`, `totalCartonCount`, `activatedCartonCount`, `errorCartonCount`, `cartonID`, `carton_Packing_Code`, `carton_Packing_ID`, `carton_Packing_Count` |
| `awsSendCounter` | object | `pendingCount`, `sentCount`, `failedCount` |
| `awsRecivedCounter` | object | `waitingCount`, `recivedCount` |

### Keys Chỉ Có Ở VNQR

| Key | Kiểu | Mô tả |
|-----|------|-------|
| `CreatedTime` | string | Thời gian tạo PO (yyyy-MM-dd HH:mm:ss) |
| `ModifiedTime` | string | Thời gian sửa cuối cùng |

---

### Tóm Tắt

- **Chung**: 13 keys cơ bản giống nhau (orderNo, site, factory, gtin, etc.)
- **MASAN thêm**: `cartonSize`, `totalCZCode`, `counter` (3 objects tracking production)
- **VNQR thêm**: `CreatedTime`, `ModifiedTime` (audit trail)

Bạn muốn tôi sync thêm fields nào giữa 2 hệ thống không? Ví dụ thêm `cartonSize` vào VNQR, hoặc thêm `CreatedTime`/`ModifiedTime` vào MASAN?