# VNQR.Helpers.po — Hướng dẫn sử dụng

## Tổng quan

`po` (Production Order) quản lý toàn bộ vòng đời PO: tạo PO, nạp mã từ DataPool, tracking camera Active/Packing, và quản lý thùng carton.

**Cấu trúc lưu trữ:** `<baseDataPath>/yyyy-MM/gtin/`

```
yyyy-MM/gtin/
├── PO_List.db               ← danh sách tất cả PO (1 file chung)
├── PO0001.db                ← bảng UniqueCodes (codes cho PO này)
├── Record_Active_PO0001.db  ← tracking camera Active
├── Record_Packing_PO0001.db ← tracking camera Packing
└── Carton_PO0001.db         ← tracking thùng carton
```

Mỗi PO có 4 file .db riêng. PO_List.db quản lý danh sách tất cả PO.

---

## Cấu hình

```csharp
// Đường dẫn gốc — sửa trước khi dùng
VNQR.Helpers.po.Config.baseDataPath = "D:/MyData/PODatabases";

// Tên file PO_List
VNQR.Helpers.po.Config.poListFileName = "PO_List.db";
```

---

## Luồng sử dụng chuẩn

### Bước 1 — Tạo PO vào PO_List

```csharp
var poInfo = new VNQR.Helpers.po.POInfo
{
    orderNo         = "PO0001",
    site            = "SITE_MASAN",
    factory         = "FACTORY_01",
    productionLine  = "LINE_1",
    productionDate  = "2026-01-11",
    shift           = "A",
    orderQty        = 100,
    lotNumber       = "LOT_2026_001",
    productCode     = "PROD_XYZ",
    productName     = "San pham A",
    gtin            = "A001",
    customerOrderNo = "CUST_PO_001",
    uom             = "PCS"
};

var (success, message) = VNQR.Helpers.po.POLoader.Create(poInfo);
```

### Bước 2 — Khởi tạo database cho PO

```csharp
var (success, message) = VNQR.Helpers.po.POCreator.InitPO("PO0001");
// Tạo 4 file .db: PO0001.db, Record_Active_PO0001.db,
//                  Record_Packing_PO0001.db, Carton_PO0001.db
```

### Bước 3 — Nạp mã từ DataPool

```csharp
// gtin = "A001" → tự map đến <DataPool.dataPath>/A001.vnqrdb
var (success, message, loadedCount) =
    VNQR.Helpers.po.POLoader.LoadCodesFromDataPool("PO0001", "A001");

// Chỉ nạp tối đa N mã
var (success, msg, count) =
    VNQR.Helpers.po.POLoader.LoadCodesFromDataPool("PO0001", "A001", limitQty: 50);
```

### Bước 4 — Tạo thùng carton (tùy chọn)

```csharp
// Tạo N thùng trống trước
var (ok, msg, n) = VNQR.Helpers.po.POCreator.CreateEmptyCartons("PO0001", 10);
```

---

## POLoader — CRUD PO_List & Query Codes

### CRUD PO_List

```csharp
// Lấy tất cả PO
TResult result = POLoader.GetAll();
if (result.issuccess) {
    DataTable table = result.data;
    foreach (DataRow row in table.Rows) {
        Console.WriteLine(row["orderNo"]);
    }
}

// Lấy PO theo orderNo
TResult result = POLoader.GetByOrderNo("PO0001");

// Lấy PO theo gtin (tất cả PO cùng GTIN)
TResult result = POLoader.GetByGTIN("A001");

// Kiểm tra PO tồn tại
bool exists = POLoader.Exists("PO0001");

// Cập nhật PO
po.POInfo updated = new po.POInfo { orderQty = 150, shift = "B" };
var (ok, msg) = POLoader.Update("PO0001", updated);

// Xóa PO (xóa cả PO_List + 4 file .db)
var (ok, msg) = POLoader.Delete("PO0001");
```

### Query Codes trong PO

```csharp
// Lấy tất cả codes
TResult result = POLoader.GetCodes("PO0001");

// Lấy codes theo trạng thái (0 = chưa active, 1 = đã active)
TResult result = POLoader.GetCodes("PO0001", status: 0);

// Lấy codes trong thùng cụ thể
TResult result = POLoader.GetCodes("PO0001", cartonCode: "CARTON_001");

// Tìm code cụ thể
TResult result = POLoader.GetCodeByCode("PO0001", "CODE_ABC123");

// Kiểm tra code tồn tại
bool exists = POLoader.CodeExists("PO0001", "CODE_ABC123");

// Đếm codes
int total = POLoader.GetCodeCount("PO0001");           // tất cả
int active = POLoader.GetCodeCount("PO0001", status: 1); // đã active
```

---

## POActivator — Camera Active

Ghi nhận sản phẩm đi qua camera Active.

```csharp
// Ghi record vào Record_Active_<PO>.db
var record = new po.PORecordData
{
    code          = "CODE_001",
    cartonCode    = "0",
    status        = "1",
    plcStatus     = po.e_PLC_Status.PASS.ToString(),
    activateDate  = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    activateUser  = "Operator1",
    productionDate = "2026-01-11"
};
po.POActivator.Record("PO0001", record);

// Đánh dấu code đã active trong UniqueCodes (Status = 1)
po.POActivator.ActivateCode("PO0001", "CODE_001",
    activateDate: "2026-01-11 08:00:00",
    activateUser: "Operator1",
    productionDate: "2026-01-11");

// Reset code về chưa active
po.POActivator.DeactivateCode("PO0001", "CODE_001");

// Đếm số mã đã active
int activeCount = po.POActivator.GetActiveCount("PO0001");
```

---

## POPacking — Camera Packing

Ghi nhận sản phẩm đi qua camera Packing.

```csharp
// Ghi record vào Record_Packing_<PO>.db
var record = new po.PORecordData
{
    code          = "CODE_001",
    cartonCode    = "CARTON_001",
    status        = "1",
    plcStatus     = po.e_PLC_Status.PASS.ToString(),
    packingDate   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    packingUser   = "Operator2",
    productionDate = "2026-01-11"
};
po.POPacking.Record("PO0001", record);

// Gán code vào thùng trong UniqueCodes
po.POPacking.PackCode("PO0001", "CODE_001",
    cartonCode: "CARTON_001",
    packingDate: "2026-01-11 09:00:00",
    packingUser: "Operator2",
    productionDate: "2026-01-11");

// Gỡ code khỏi thùng
po.POPacking.UnpackCode("PO0001", "CODE_001");

// Đếm số mã đã đóng gói
int packed = po.POPacking.GetPackedCount("PO0001");
```

---

## POCarton — Quản lý thùng

```csharp
// Tạo thùng mới
var carton = new po.POCartonData
{
    cartonCode    = "CARTON_001",
    startDatetime = "2026-01-11 09:00:00",
    activateUser  = "Operator2",
    productionDate = "2026-01-11"
};
po.POCarton.Create("PO0001", carton);

// Lấy danh sách thùng
TResult result = po.POCarton.GetAll("PO0001");
if (result.issuccess) {
    DataTable table = result.data;
}

// Tìm thùng theo cartonCode
TResult result = po.POCarton.GetByCartonCode("PO0001", "CARTON_001");

// Bắt đầu đóng thùng (set Start_Datetime = now)
po.POCarton.StartCarton("PO0001", cartonId: 1, activateUser: "Operator2");

// Hoàn thành thùng (set Completed_Datetime = now)
po.POCarton.CompleteCarton("PO0001", cartonId: 1, activateUser: "Operator2");

// Reset thùng về trạng thái mở
po.POCarton.ResetCarton("PO0001", cartonId: 1);

// Cập nhật thông tin thùng
carton.completedDatetime = "2026-01-11 10:00:00";
po.POCarton.Update("PO0001", carton);

// Xóa thùng
po.POCarton.Delete("PO0001", cartonId: 1);

// Đếm thùng
int total = po.POCarton.GetTotalCartonCount("PO0001");
int closed = po.POCarton.GetClosedCartonCount("PO0001");
```

---

## POCreator — Khởi tạo PO

```csharp
// Tạo 4 file .db cho PO
var (success, message) = po.POCreator.InitPO("PO0001");

// Tạo N thùng trống
var (ok, msg, count) = po.POCreator.CreateEmptyCartons("PO0001", 10);
```

---

## POUpdater — Cập nhật PO

```csharp
// Cập nhật ngày sản xuất
bool ok = po.POUpdater.UpdateProductionDate("PO0001", "2026-01-15");
```

---

## POUpdater — Enums

```csharp
po.e_PLC_Status       // PASS, FAIL, ERROR, TIMEOUT, READFAIL, FORMATERROR
po.e_Carton_Status    // Open=0, Closed=1, Cancelled=-1
po.e_Code_Status      // Unused=0, Used=1
po.e_AWS_Send_Status  // Pending, Sent, Failed
po.e_AWS_Recive_Status// Waiting, Pending, Sent=200, Error, Error_404...
```

---

## Ví dụ: Luồng hoàn chỉnh từ đầu đến cuối

```csharp
// 1. Tạo PO
var poInfo = new po.POInfo {
    orderNo = "PO0001", gtin = "A001", orderQty = 100,
    productionDate = "2026-01-11"
};
po.POLoader.Create(poInfo);

// 2. Khởi tạo DB
po.POCreator.InitPO("PO0001");

// 3. Tạo 5 thùng trống
po.POCreator.CreateEmptyCartons("PO0001", 5);

// 4. Nạp mã từ DataPool
po.POLoader.LoadCodesFromDataPool("PO0001", "A001");

// 5. Khi camera Active đọc được code
var activeRecord = new po.PORecordData {
    code = "CODE_001", plcStatus = "PASS",
    activateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    activateUser = "User1"
};
po.POActivator.Record("PO0001", activeRecord);
po.POActivator.ActivateCode("PO0001", "CODE_001",
    activateDate: activeRecord.activateDate,
    activateUser: activeRecord.activateUser,
    productionDate: "2026-01-11");

// 6. Khi camera Packing đọc được code
var packRecord = new po.PORecordData {
    code = "CODE_001", cartonCode = "CARTON_001",
    packingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    packingUser = "User2"
};
po.POPacking.Record("PO0001", packRecord);
po.POPacking.PackCode("PO0001", "CODE_001",
    cartonCode: "CARTON_001",
    packingDate: packRecord.packingDate,
    packingUser: packRecord.packingUser,
    productionDate: "2026-01-11");

// 7. Hoàn thành thùng
po.POCarton.CompleteCarton("PO0001", cartonId: 1, activateUser: "User2");

// 8. Kiểm tra tiến độ
int activeCount = po.POActivator.GetActiveCount("PO0001");    // số mã đã active
int packedCount = po.POPacking.GetPackedCount("PO0001");      // số mã đã đóng gói
int closedCartons = po.POCarton.GetClosedCartonCount("PO0001"); // số thùng đã đóng
```

---

## POApiServer — REST API Server

API Server cho phép tạo và quản lý PO từ hệ thống bên ngoài qua HTTP.

### Khởi động Server

```csharp
// Cách 1: Dùng ILogger
var logger = serviceProvider.GetRequiredService<ILogger<POApiServer>>();
var apiServer = new VNQR.Helpers.POApiServer(port: 9999, logger: logger);

// Cách 2: Dùng callback log tùy chỉnh
var apiServer = new VNQR.Helpers.POApiServer(
    port: 9999,
    onLog: (source, message) => Console.WriteLine($"[{source}] {message}")
);

// Khởi động (chạy bất đồng bộ trên background thread)
await apiServer.StartAsync();

// Dừng server khi không cần nữa
await apiServer.StopAsync();

// Hoặc dùng using
using var apiServer = new VNQR.Helpers.POApiServer(port: 9999);
await apiServer.StartAsync();
// ... server chạy ...
// Dispose tự động gọi StopAsync
```

### Base URL

```
http://<server_ip>:9999
```

---

### 1. Health Check

Kiểm tra trạng thái server.

**Endpoint:** `GET /api/health`

**Response:**
```json
{
    "status": "OK",
    "timestamp": "2026-01-11T10:30:00",
    "appState": "Running"
}
```

---

### 2. Tạo PO mới

**Endpoint:** `POST /api/po`

**Request Body:**
```json
{
    "orderNo": "PO0001",
    "site": "SITE_MASAN",
    "factory": "FACTORY_01",
    "productionLine": "LINE_1",
    "productionDate": "2026-01-11",
    "shift": "A",
    "orderQty": 100,
    "lotNumber": "LOT_2026_001",
    "productCode": "PROD_XYZ",
    "productName": "Sản phẩm A",
    "gtin": "A001",
    "customerOrderNo": "CUST_PO_001",
    "uom": "PCS",
    "userName": "API",
    "autoLoadCodes": true
}
```

**Trường bắt buộc:**
- `orderNo` — Mã PO
- `orderQty` — Số lượng (phải > 24)

**Trường tùy chọn:**
- `autoLoadCodes` — Tự động nạp mã từ DataPool (mặc định: true)
- `gtin` — Dùng để nạp mã từ DataPool khi autoLoadCodes = true
- Các trường khác có giá trị mặc định

**Response thành công (200):**
```json
{
    "success": true,
    "message": "PO 'PO0001' created successfully.",
    "orderNo": "PO0001",
    "loadedCodesCount": 50
}
```

**Response lỗi:**
```json
{
    "success": false,
    "message": "orderNo is required."
}
```

| HTTP Status | Nguyên nhân |
|-------------|-------------|
| 400 | Thiếu orderNo hoặc orderQty <= 24 |
| 409 | PO đã tồn tại |
| 500 | Lỗi server |

---

### 3. Lấy thông tin PO

**Endpoint:** `GET /api/po/{orderNo}`

**Ví dụ:** `GET /api/po/PO0001`

**Response thành công (200):**
```json
{
    "success": true,
    "data": {
        "orderNo": "PO0001",
        "site": "SITE_MASAN",
        "factory": "FACTORY_01",
        "productionLine": "LINE_1",
        "productionDate": "2026-01-11",
        "shift": "A",
        "orderQty": 100,
        "lotNumber": "LOT_2026_001",
        "productCode": "PROD_XYZ",
        "productName": "Sản phẩm A",
        "gtin": "A001",
        "customerOrderNo": "CUST_PO_001",
        "uom": "PCS",
        "createdTime": "2026-01-11T10:30:00",
        "modifiedTime": "2026-01-11T10:30:00"
    }
}
```

**Response lỗi (404):**
```json
{
    "success": false,
    "message": "PO not found."
}
```

---

### 4. Lấy danh sách tất cả PO

**Endpoint:** `GET /api/po/list/all`

**Response:**
```json
{
    "success": true,
    "count": 3,
    "data": [
        {
            "orderNo": "PO0001",
            "productName": "Sản phẩm A",
            "orderQty": 100,
            "productionDate": "2026-01-11",
            "status": "2026-01-11T10:30:00"
        },
        {
            "orderNo": "PO0002",
            "productName": "Sản phẩm B",
            "orderQty": 200,
            "productionDate": "2026-01-12",
            "status": "2026-01-12T08:00:00"
        }
    ]
}
```

---

### Ví dụ: Gọi API từ curl

```bash
# Health check
curl http://localhost:9999/api/health

# Tạo PO mới
curl -X POST http://localhost:9999/api/po \
  -H "Content-Type: application/json" \
  -d '{
    "orderNo": "PO_API_001",
    "site": "SITE_MASAN",
    "factory": "FACTORY_01",
    "productionLine": "LINE_1",
    "productionDate": "2026-01-11",
    "shift": "A",
    "orderQty": 100,
    "lotNumber": "LOT_001",
    "productCode": "PROD_001",
    "productName": "Sản phẩm Test",
    "gtin": "A001",
    "customerOrderNo": "CUST_001",
    "uom": "PCS",
    "autoLoadCodes": true
  }'

# Lấy thông tin PO
curl http://localhost:9999/api/po/PO_API_001

# Lấy danh sách PO
curl http://localhost:9999/api/po/list/all
```

---

### Ví dụ: Gọi API từ C#

```csharp
using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:9999/") };

// Tạo PO
var request = new
{
    orderNo = "PO_API_001",
    site = "SITE_MASAN",
    factory = "FACTORY_01",
    productionLine = "LINE_1",
    productionDate = "2026-01-11",
    shift = "A",
    orderQty = 100,
    lotNumber = "LOT_001",
    productCode = "PROD_001",
    productName = "Sản phẩm Test",
    gtin = "A001",
    customerOrderNo = "CUST_001",
    uom = "PCS",
    autoLoadCodes = true
};

var response = await httpClient.PostAsJsonAsync("/api/po", request);
var result = await response.Content.ReadFromJsonAsync<VNQR.Helpers.CreatePOResponse>();

if (result.Success)
    Console.WriteLine($"Created: {result.OrderNo}");
else
    Console.WriteLine($"Error: {result.Message}");

---

## DataPool API — REST API cho bể dữ liệu

API Server cho phép quản lý bể dữ liệu (DataPool) từ hệ thống bên ngoài qua HTTP.

**Lưu ý:** DataPool path mặc định: `C:/VNQR/Databases`

---

### 1. Thêm mã thủ công (từng cái 1)

**Endpoint:** `POST /api/datapool/add`

**Request:**
```json
{
    "poolName": "A001",
    "code": "CODE_123456",
    "status": 0,
    "batchID": "",
    "note": "Manual entry",
    "userName": "admin"
}
```

- `status`: 0 = chưa dùng, 1 = đã dùng (mặc định: 0)
- Nếu `status = 1` thì `batchID` bắt buộc phải có giá trị

**Response:**
```json
{
    "success": true,
    "message": "Nhập mã thủ công thành công.",
    "count": 1
}
```

---

### 2. Thêm mã từ đầu đọc (camera)

**Endpoint:** `POST /api/datapool/add-reader`

**Request:**
```json
{
    "poolName": "A001",
    "code": "CODE_789",
    "batchID": "LOT_2026_001",
    "note": "Scanned by camera"
}
```

- Mã chưa tồn tại → thêm mới, Status = 1 (đã dùng)
- Mã đã tồn tại nhưng chưa dùng → cập nhật Status = 1
- Mã đã tồn tại và đã dùng → trả lỗi

**Response:**
```json
{
    "success": true,
    "message": "Đã thêm mã mới và đánh dấu đã dùng.",
    "count": 1
}
```

---

### 3. Nhập mã từ file CSV

**Endpoint:** `POST /api/datapool/import-file`

**Request:**
```json
{
    "poolName": "A001",
    "csvPath": "C:/Data/codes.csv",
    "userName": "admin",
    "createID": "IMPORT_001",
    "codeColumn": "Code",
    "noteColumn": "Description",
    "note": "Batch import from supplier"
}
```

- Tạo file phiếu nhập trong thư mục `Phieu/`
- Status mặc định = 0 (chưa dùng) cho tất cả mã

**Response:**
```json
{
    "success": true,
    "message": "Nhập từ file hoàn tất: 100 thêm mới, 5 bị bỏ qua (trùng mã).",
    "count": 100
}
```

---

### 4. Lấy danh sách tất cả mã trong bể

**Endpoint:** `GET /api/datapool/{poolName}/codes`

**Ví dụ:** `GET /api/datapool/A001/codes`

**Response:**
```json
{
    "success": true,
    "count": 3,
    "data": [
        {
            "id": 1,
            "code": "CODE_123456",
            "status": 0,
            "batchID": "",
            "createTime": "2026-01-11 10:00:00",
            "createID": "User:admin",
            "note": "Manual entry"
        },
        {
            "id": 2,
            "code": "CODE_789",
            "status": 1,
            "batchID": "LOT_2026_001",
            "createTime": "2026-01-11 11:00:00",
            "createID": "Reader",
            "note": "Scanned by camera"
        }
    ]
}
```

---

### 5. Lấy thông tin một mã

**Endpoint:** `GET /api/datapool/{poolName}/code/{code}`

**Ví dụ:** `GET /api/datapool/A001/code/CODE_123456`

**Response:**
```json
{
    "success": true,
    "data": {
        "id": 1,
        "code": "CODE_123456",
        "status": 0,
        "batchID": "",
        "createTime": "2026-01-11 10:00:00",
        "createID": "User:admin",
        "note": "Manual entry"
    }
}
```

---

### 6. Cập nhật trạng thái mã

**Endpoint:** `PUT /api/datapool/{poolName}/code/{code}/status`

**Request:**
```json
{
    "status": 1,
    "batchID": "LOT_2026_002",
    "note": "Updated"
}
```

**Response:**
```json
{
    "success": true,
    "message": "Status updated.",
    "count": 1
}
```

---

### 7. Xóa một mã

**Endpoint:** `DELETE /api/datapool/{poolName}/code/{code}`

**Ví dụ:** `DELETE /api/datapool/A001/code/CODE_123456`

**Response:**
```json
{
    "success": true,
    "message": "Code deleted.",
    "count": 1
}
```

---

### 8. Liệt kê các bể dữ liệu

**Endpoint:** `GET /api/datapool/pools`

**Response:**
```json
{
    "success": true,
    "count": 2,
    "data": [
        {
            "name": "A001",
            "fileName": "A001.vnqrdb",
            "size": 4096
        },
        {
            "name": "A002",
            "fileName": "A002.vnqrdb",
            "size": 8192
        }
    ]
}
```

---

### Ví dụ: Gọi DataPool API từ curl

```bash
# Thêm mã thủ công
curl -X POST http://localhost:9999/api/datapool/add \
  -H "Content-Type: application/json" \
  -d '{
    "poolName": "A001",
    "code": "CODE_123456",
    "status": 0,
    "userName": "admin"
  }'

# Thêm mã từ camera
curl -X POST http://localhost:9999/api/datapool/add-reader \
  -H "Content-Type: application/json" \
  -d '{
    "poolName": "A001",
    "code": "CODE_789",
    "batchID": "LOT_2026_001"
  }'

# Nhập mã từ file CSV
curl -X POST http://localhost:9999/api/datapool/import-file \
  -H "Content-Type: application/json" \
  -d '{
    "poolName": "A001",
    "csvPath": "C:/Data/codes.csv",
    "userName": "admin",
    "createID": "IMPORT_001",
    "codeColumn": "Code"
  }'

# Lấy danh sách mã
curl http://localhost:9999/api/datapool/A001/codes

# Lấy thông tin một mã
curl http://localhost:9999/api/datapool/A001/code/CODE_123456

# Cập nhật trạng thái mã
curl -X PUT http://localhost:9999/api/datapool/A001/code/CODE_123456/status \
  -H "Content-Type: application/json" \
  -d '{"status": 1, "batchID": "LOT_2026_002"}'

# Xóa mã
curl -X DELETE http://localhost:9999/api/datapool/A001/code/CODE_123456

# Liệt kê các bể dữ liệu
curl http://localhost:9999/api/datapool/pools
```
```
