

Bây giờ tôi đã nghiên cứu kỹ toàn bộ dự án. Dưới đây là bản tổng kết chi tiết.

---

# MASAN-SERIALIZATION - Bản Nghiên Cứu Toàn Diện

## 1. Tổng Quan Dự Án

**MASAN-SERIALIZATION** là ứng dụng Windows Forms (.NET Framework 4.8) phục vụ hệ thống **track & trace (theo dõi - truy xuất)** cho **Masan Group**. Mục tiêu: gắn mã duy nhất (serialization) cho từng sản phẩm, đóng thùng, kích hoạt lên cloud (AWS IoT), và quản lý toàn bộ quy trình sản xuất theo tiêu chuẩn.

### 1.1 Tech Stack

| Layer | Công nghệ |
|---|---|
| UI Framework | SunnyUI 3.8.6 |
| Database | SQLite (System.Data.SQLite 1.0.119) |
| PLC Communication | HslCommunication 12.3.2 (Omron FINS) |
| Cloud (IoT) | MQTTnet 4.1.3 → AWS IoT Core |
| Barcode/QR | ZXing.Net 0.16.10 |
| Google Sheets | Google.Apis 1.70.0 (tải PLC addresses) |
| Embedded Browser | WebView2 |
| Auth/Logging | Thư viện shared `SpT` (SPMS1) |
| ORM | EntityFramework 6.5.1 |
| JSON | Newtonsoft.Json 13.0.3 |

### 1.2 Cấu trúc thư mục chính

```
MASAN-SERIALIZATION/
├── FMain.cs                   # Main form (MDI container, state machine)
├── Infrastructure/
│   └── Globals.cs             # Singleton state: PO data, PLC instances, AWS, queues
├── Production/
│   └── ProductionHelper.cs    # Core business logic: ~2400 dòng
├── Helpers/
│   ├── DatabaseHelper.cs      # Tìm kiếm code trên tất cả PO databases
│   ├── QRDatabaseHelper.cs    # Quản lý bể QR tại C:\MASAN\QRDatabase.db
│   └── ThieuSanPhamHelper.cs # Cảnh báo thiếu sản phẩm (blink + PLC alarm)
├── Configs/
│   ├── IniConfigs.cs          # MSC.ini - toàn bộ settings
│   └── PLCDMA.cs              # Tải PLC addresses từ Google Sheets
├── Enums/
│   ├── eState.cs              # App/Camera states
│   ├── eResult.cs             # Warning types
│   └── SystemLog.cs           # Log types
├── Views/
│   ├── Dashboards/
│   │   ├── FDashboard.cs      # Dashboard chính (~1000+ dòng)
│   │   └── PCartonDashboard.cs # Quản lý đóng thùng
│   ├── ProductionInfo/
│   │   └── PPOInfo.cs         # Trang chọn & quản lý PO (~1600 dòng)
│   ├── AWS/
│   │   ├── PAwsIot.cs         # AWS IoT Core MQTT integration
│   │   └── PAws.cs            # AWS settings page
│   ├── Database/              # Các tool tra cứu code
│   ├── SCADA/                 # PStatictis - thống kê (ECharts)
│   ├── Settings/              # Cài đặt app, PLC recipe
│   ├── Login/                 # Đăng nhập (PLogin)
│   └── Reports/               # Báo cáo sản xuất
├── Dialogs/
│   ├── POM.cs                 # 2FA POM dialog
│   └── Entertext.cs           # Virtual keyboard
├── Utils/
│   ├── ErrorCodes.cs          # Mã lỗi định dạng [Module]-[Category]-[Number]
│   ├── ErrorMigrationHelper.cs # Migration mã lỗi cũ
│   ├── Extension.cs            # Extension methods cho UI
│   └── LogExtensions.cs        # Structured logging
└── Docs/
    └── Timeout_Bottle_Mechanism.md # Tài liệu camera timeout
```

---

## 2. Kiến trúc Dữ Liệu

### 2.1 Nguồn dữ liệu MES (Input)

```
C:\MasanSerialization_v2\
├── Server_Service\
│   ├── po1.db                 # SQLite - danh sách tất cả PO
│   └── data\                  # PO metadata (JSON)
│       ├── PO001.json         # Thông tin chi tiết PO001
│       └── PO002.json
└── codes_json\                # Mã CZ (Unique codes)
    └── GTIN_893xxxx.json      # Lưu theo GTIN, KHÔNG theo PO
```

Mỗi file `*.json` chứa thông tin PO: `orderNo`, `site`, `factory`, `productionLine`, `productionDate`, `shift`, `orderQty`, `lotNumber`, `productCode`, `productName`, `gtin`, `customerOrderNo`, `uom`.

Mã CZ được lưu **theo GTIN** (nhiều PO cùng GTIN dùng chung bộ mã). Cấu trúc JSON:

```json
{
  "blocks": {
    "0": { "codes": [{ "code": "...", "createdAt": "...", "blockNo": 0 }, ...] },
    "1": { "codes": [...] }
  }
}
```

### 2.2 PODatabases (Output - dữ liệu chạy máy)

```
C:/MasanSerialization_v2/PODatabases/
├── 2025-01/              # Theo năm-tháng sản xuất
│   └── 8931234567890/    # Theo GTIN
│       ├── PO001.db      # UniqueCodes - DS mã (trạng thái active/trùng/gửi AWS)
│       ├── Record_PO001.db      # Records - mã đã quét (Camera Main)
│       ├── Record_CameraSub_PO001.db  # Camera Sub records
│       ├── carton_PO001.db      # Carton - thùng đóng
│       ├── Send_AWS_Record_PO001.db   # AWS send log
│       └── Recive_AWS_Record_PO001.db # AWS receive log
```

### 2.3 Các bảng SQLite chính

| Bảng | Database | Mục đích |
|---|---|---|
| `UniqueCodes` | `{orderNo}.db` | Mã CZ - trạng thái kích hoạt, gửi AWS, trùng |
| `Records` | `Record_{orderNo}.db` | Mã Camera Main đã quét |
| `Records_CameraSub` | `Record_CameraSub_{orderNo}.db` | Mã Camera Sub đã quét + cartonID |
| `Carton` | `carton_{orderNo}.db` | Thông tin thùng |
| `PO` | `POLog.db` | Lịch sử Create/Update/Delete PO |
| `QRProducts` | `C:\MASAN\QRDatabase.db` | Bể QR product (active/deactive) |

### 2.4 Global State (`Globals.cs` + `Globals_Database`)

- **`Globals.ProductionData`** (kiểu `ProductionOrder`) — lưu toàn bộ thông tin PO hiện tại, counters.
- **`Globals_Database.Dictionary_ProductionCode_Data`** — Dictionary in-memory để lookup nhanh mã sản phẩm.
- **`Globals_Database.Dictionary_ProductionCarton_Data`** — Dictionary carton.
- **Queues**: `Update_Product_To_SQLite_Queue`, `Insert_Product_To_Record_Queue`, `Insert_Product_To_Record_CS_Queue`, `Update_Product_To_Record_Carton_Queue`, `Activate_Carton`, `aWS_Send_Datas`, `aWS_Recive_Datas`.

---

## 3. Quy Trình Sản Xuất Chi Tiết

Dưới đây là flow end-to-end từ lúc MES tạo đơn hàng đến khi hoàn thành và gửi AWS.

### Giai đoạn 1: Tạo & Chuẩn Bị PO

```
MES Server (bên ngoài)
    │
    ├── Xuất PO metadata → C:\MasanSerialization_v2\Server_Service\data\{orderNo}.json
    │
    └── Xuất mã CZ (theo GTIN) → C:\MasanSerialization_v2\Server_Service\codes_json\GTIN_{gtin}.json
```

**Trên ứng dụng (PPOInfo.cs):**

```
1. Mở giao diện PPOInfo
   └── Load danh sách PO từ folder data\ (*.json)
       └── Loại bỏ PO đã Deleted/Complete
       └── Lưu vào ComboBox (ipOrderNO)

2. Người dùng chọn PO trong ComboBox
   ├── ipOrderNO_SelectedIndexChanged()
   ├── Gọi getfromMES.ProductionOrder_Detail(orderNo) → đọc JSON
   └── Render thông tin: orderQty, productName, GTIN, factory, line...

3. Nhấn "Lưu PO" (btnPO_Click)
   ├── Validate: PO chưa bị Deleted, chưa Completed, đủ mã CZ
   ├── Gọi Check_Database_File(orderNo, orderQty)
   │   ├── Tạo folder theo cấu trúc: dataPath/yyyy-MM/GTIN/
   │   ├── Tạo {orderNo}.db → UniqueCodes (lọc mã đã used ở PO khác cùng GTIN)
   │   ├── Tạo Record_{orderNo}.db → Records
   │   ├── Tạo Record_CameraSub_{orderNo}.db
   │   ├── Tạo carton_{orderNo}.db → pre-create N thùng = orderQty/24
   │   └── Ghi vào POLog.db: Action='Create'
   └── Gọi Save_PO() → ghi productionDate vào POLog.db

4. State: NoSelectedPO → Start → Loading → Saving → Ready
```

### Giai đoạn 2: Chạy Sản Xuất

```
Người dùng nhấn "Chạy sản xuất" (btnRUN)
    │
    ├── Kiểm tra APP_Ready + Device_Ready
    ├── Gọi PrepareProductionData():
    │   ├── Get_Records_CameraSub() → đếm số mã đã scan
    │   ├── Reset counters
    │   └── Tính cartonID hiện tại (thùng đang đóng)
    │
    ├── Nếu totalCount == 0 → Pushing_new_PO_to_PLC
    │   └── Đẩy toàn bộ mã xuống PLC
    │
    └── Nếu totalCount > 0 → Pushing_continue_PO_to_PLC
        └── Đẩy tiếp mã còn lại (resume)

State: Ready → Running
```

### Giai đoạn 3: Main Process Loop (FDashboard)

```
BackgroundWorker chạy liên tục (mỗi 100ms):
│
├── PLC Communication
│   ├── Đọc counter từ PLC (CameraMain_PLC_Counter, CameraSub_PLC_Counter)
│   ├── Gửi tín hiệu reject/pass cho Camera Main
│   └── PLC Duo Mode: hỗ trợ 2 PLC song song
│
├── Camera Main Processing
│   ├── Camera Main đọc mã trên sản phẩm ( conveyor line)
│   ├── Kiểm tra trùng với Dictionary_ProductionCode_Data
│   ├── Nếu trùng: reject hoặc cảnh báo (tùy cấu hình CameraMain_DuplicateReject_Enabled)
│   ├── Nếu không trùng:
│   │   ├── Lookup mã trong UniqueCodes DB
│   │   ├── Kiểm tra đã active ở PO khác (cùng GTIN) chưa
│   │   ├── Cập nhật Dictionary
│   │   └── Enqueue vào Update_Product_To_SQLite_Queue
│   └── Ghi Record vào Record_{orderNo}.db
│
├── Camera Sub Processing (Secondary verification)
│   ├── Camera Sub đọc mã (sau camera main)
│   ├── Lookup trong Dictionary
│   ├── Kiểm tra trùng (MaBiTrung - mã nằm trong 2 thùng)
│   ├── Enqueue vào Insert_Product_To_Record_CS_Queue
│   └── Khi đủ 24 sản phẩm → tạo thùng mới
│
├── Carton Packing
│   ├── Mỗi thùng chứa cartonPack (mặc định 24) sản phẩm
│   ├── CartonID tăng dần
│   ├── Khi đầy → kích hoạt carton (ghi vào carton_{orderNo}.db)
│   └── Enqueue vào Activate_Carton
│
├── AWS IoT Publishing
│   ├── Background worker kiểm tra hàng đợi aWS_Send_Datas
│   ├── Publish lên topic "CZ/data" (hoặc "CZ/dataDev" nếu Dev mode)
│   ├── Payload: message_id, orderNo, uniqueCode, gtin, cartonCode, status, activate_datetime, production_date, thing_name
│   ├── Subscribe topic "CZ/{clientId}/response"
│   ├── Nhận phản hồi → update Recive_Status
│   └── Ghi log vào Send_AWS_Record / Recive_AWS_Record
│
└── Queue Processing (ghi DB)
    ├── Update_Product_To_SQLite_Queue → UniqueCodes.Status=1
    ├── Insert_Product_To_Record_Queue → Records
    ├── Insert_Product_To_Record_CS_Queue → Records_CameraSub
    ├── Update_Product_To_Record_Carton_Queue → Carton
    └── Activate_Carton → kích hoạt thùng
```

### Giai đoạn 4: Đóng Thùng & Kích Hoạt

```
Conveyor Belt: sản phẩm chạy qua line
    │
    ├── Camera Main: scan mã sản phẩm (lần 1)
    │   └── Kiểm tra trùng, đủ mã
    │
    ├── Camera Sub: scan mã sản phẩm (lần 2 - xác nhận)
    │   └── Gán cartonID hiện tại
    │
    ├── Đếm sản phẩm trong thùng:
    │   └── Khi đạt cartonPack (24) → thùng đầy
    │
    ├── Tạo cartonCode (barcode)
    │   └── Ghi vào carton_{orderNo}.db
    │   └── Enqueue Activate_Carton
    │
    ├── Kích hoạt carton:
    │   ├── Cập nhật carton.status = activated
    │   ├── Update cartonID trong Records_CameraSub
    │   └── Gửi lên AWS IoT
    │
    └── Thùng mới: cartonID++
```

### Giai đoạn 5: AWS IoT Cloud Sync

```
AWS IoT Core
    │
    ├── Publisher: topic "CZ/data"
    │   └── App publishes: message_id, orderNo, uniqueCode, gtin, cartonCode, status, activate_datetime, production_date
    │
    └── Subscriber: topic "CZ/{clientId}/response"
        └── AWS trả về: status, message_id, error_message
            ├── Pending → chờ xử lý
            ├── 200 OK → thành công
            └── 4xx/5xx → lỗi
```

### Giai đoạn 6: Trạng thái đặc biệt

```
ThieuSanPham (Thiếu sản phẩm)
├── Phát hiện khi carton không đủ số lượng
├── Blink red text + bật đèn báo PLC alarm
├── Người dùng nhấn Reset
└── Xử lý:
    ├── TH1: Thùng hiện tại = 0 sp + thùng trước < 24
    │   └── Reset thùng hiện tại, đóng app
    └── TH2: Nghiêm trọng
        └── Cảnh báo, dừng sản xuất, liên hệ nhà cung cấp

MaBiTrung (Mã bị trùng thùng)
├── Phát hiện khi 1 mã xuất hiện trong >1 cartonID
├── Bỏ qua gửi AWS cho mã đó
└── Ghi log cảnh báo

DuSanPham (Đủ sản phẩm)
└── Khi passCount >= orderQty → Completed
```

---

## 4. State Machine (Production States)

```
e_Production_State
├── NoSelectedPO          ← Không có PO được chọn
├── Start                ← App khởi động, kiểm tra PO cuối
├── Checking_PO_Info      ← Kiểm tra PO từ MES
├── Loading              ← Đang tải dữ liệu
├── Saving              ← Lưu PO (tạo DB files)
├── Ready               ← Sẵn sàng chạy sản xuất
├── Pushing_new_PO_to_PLC  ← Đẩy PO mới xuống PLC
├── Pushing_continue_PO_to_PLC ← Resume PO đang chạy dở
├── Running              ← Đang chạy sản xuất
├── Checking_Queue       ← Kiểm tra queue trước khi dừng
├── Waiting_Stop        ← Chờ dừng (đủ số lượng)
├── Completed           ← Hoàn thành
├── Pause               ← Tạm dừng
├── Editing             ← Chỉnh sửa PO
├── Editting_ProductionDate ← Chỉnh ngày sản xuất
├── Camera_Processing   ← Xử lý camera
├── Pushing_to_Dic      ← Đẩy vào Dictionary
├── ThieuSanPham        ← Cảnh báo thiếu sản phẩm
├── KiemTraThieu        ← Kiểm tra thiếu (xử lý TH1/TH2)
├── MaBiTrung           ← Phát hiện mã trùng thùng
├── Check_After_Completed
└── Error
```

---

## 5. Cấu Hình (MSC.ini)

File `Configs/MSC.ini` chứa toàn bộ settings:

| Setting | Mặc định | Mô tả |
|---|---|---|
| `cartonPack` | 24 | Số sản phẩm/thùng |
| `cartonOfset` | 2 | Offset |
| `cartonWarning` | 5 | Ngưỡng cảnh báo |
| `Camera_Main_IP` | 127.0.0.1 | IP Camera Main |
| `Camera_Sub_IP` | 127.0.0.1 | IP Camera Sub |
| `PLC_Duo_Mode` | false | Dual PLC |
| `PLC_Test_Mode` | true | Test PLC |
| `AWS_ENA` | true | Bật AWS |
| `Auto_Send_AWS` | false | Tự động gửi AWS |
| `TwoFA_Enabled` | false | 2FA authentication |
| `CameraMain_DuplicateReject_Enabled` | true | Đá sản phẩm trùng ở Camera Main |
| `CameraSub_Timeout_Enabled` | true | Camera Sub timeout |
| `CameraSub_Timeout_Ms` | 500 | Timeout ms |
| `Check_Db_Old_Active` | false | Kiểm tra trùng với DB cũ |
| `Check_Db_Old_Bypass` | false | Bypass kiểm tra |
| `cartonScanerMode` | 0 | 0=Manual, 1=Auto |
| `cartonScanerTCP_IP` | 192.168.250.14 | TCP scanner IP |
| `host` | aws...amazonaws.com | AWS IoT endpoint |
| `clientId` | MIPWP501 | AWS Client ID |

---

## 6. Mã Lỗi Hệ Thống

Format: `[Module]-[Category]-[Number]`

| Module | Ý nghĩa |
|---|---|
| `EM-*` | Main application (khởi tạo, xử lý chung) |
| `PP-*` | Production process (quản lý PO, trạng thái) |
| `DA-*` | Dashboard (PLC giao tiếp, thiết bị) |
| `DB-*` | Database (đọc/ghi PO, records) |
| `PC-*` | Carton dashboard |
| `ST-*` | Statistics |
| `EA-*` | Critical emergency (DB corruption) |
| `CF-*` | Configuration |

---

## 7. Các Tính Năng Đặc Biệt

### 7.1 Dual Camera (Main + Sub)
- **Camera Main**: Quét mã đầu tiên trên conveyor. Kiểm tra trùng, lookup trong dictionary, ghi vào UniqueCodes.
- **Camera Sub**: Quét thứ hai để xác nhận. Gán `cartonID`, kiểm tra `MaBiTrung`.

### 7.2 Dual PLC (Duo Mode)
- Hỗ trợ 2 PLC Omron song song (PLC_Instance, PLC_Instance_02).
- Giao tiếp qua HslCommunication (FINS protocol).
- PLC addresses được load từ Google Sheets (fallback: local `plc_addresses.json`).

### 7.3 QR Database (`C:\MASAN\QRDatabase.db`)
- Bể QR product độc lập.
- Supports: Add/Activate, Deactivate, Lookup, BatchCode tracking.
- Backup and recreate functionality.

### 7.4 2FA Authentication
- Dialog `POM.cs` yêu cầu xác thực 2 yếu tố cho thao tác nhạy cảm.
- Toggle bật/tắt qua `TwoFA_Enabled` config.

### 7.5 Thống kê ECharts (PStatictis)
- Biểu đồ cột: sản lượng theo giờ.
- Biểu đồ tròn: Pass/Fail.
- Charts: AWS sync status.

### 7.6 Production Report
- Form `ProductionReportForm` thể hiện:
  - Sản lượng theo đơn hàng
  - Tình trạng đóng thùng
  - Trạng thái sync MES
  - Phân tích theo ngày sản xuất

---

## 8. Data Flow Tổng Hợp

```
┌─────────────────────────────────────────────────────────────────┐
│                         MES SERVER SERVICE                        │
│  C:\MasanSerialization_v2\Server_Service\                        │
│  ├── po1.db               ← Danh sách PO                        │
│  ├── data/{PO}.json       ← PO metadata                         │
│  └── codes_json/GTIN_*.json ← Mã CZ (theo GTIN)               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    APP: TẠO / CHỌN PO                          │
│  PPOInfo.Start()                                                 │
│  ├── Load PO list từ data/*.json                               │
│  ├── Check_Database_File → tạo folder yyyy-MM/GTIN/             │
│  ├── Tạo {PO}.db, Record_*.db, carton_*.db                    │
│  ├── Insert mã CZ (đã lọc used codes)                         │
│  └── Save_PO() → ghi POLog.db                                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    APP: CHẠY SẢN XUẤT                          │
│  FDashboard BackgroundWorker (100ms loop)                        │
│  ├── PLC ←→ Đọc counter, gửi reject/pass                      │
│  ├── Camera Main ──→ Quét mã ──→ Kiểm tra trùng ──→ Dictionary│
│  ├── Camera Sub ────→ Quét mã ──→ Gán cartonID ──→ Records_CS │
│  ├── Carton Packing ──→ Đếm 24 sản phẩm ──→ Tạo cartonCode    │
│  └── AWS IoT ─────────→ Publish mã ──→ Subscribe response     │
└────────────────────────────┬────────────────────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ PODatabases/ │    │   PLC        │    │ AWS IoT Core │
│ yyyy-MM/GTIN │    │   (Omron)    │    │   (MQTT)     │
│ ├── {PO}.db  │    │              │    │              │
│ ├── Record_*.db    │              │    │              │
│ ├── Record_CS_*.db │              │    │              │
│ └── carton_*.db    │              │    │              │
└──────────────┘    └──────────────┘    └──────────────┘
```

---

## 9. Các Enum Quan Trọng

```csharp
e_Production_Status:
  Pass=1, Fail=-1, Duplicate=-3, ReadFail=-2, NotFound=-4, Error=-5

e_AWS_Send_Status:
  Pending, Sent, Failed

e_AWS_Recive_Status:
  Waiting=0, Pending, Sent=200, Error=2,
  Error_400, Error_401, Error_403, Error_404, Error_408, Error_409, Error_500

e_awsIot_status:
  Disconnected, Connecting, Connected, Subscribed,
  Unsubscribed, Published, Unpublished, Error

e_Camera_State:
  DISCONNECTED, CONNECTED, RECONNECTING
```

---

## 10. Key Takeaways

1. **Lõi serialization**: Mỗi sản phẩm được gắn 1 mã duy nhất từ MES, scan bởi Camera Main + Camera Sub, track qua Dictionary in-memory + SQLite.

2. **Cơ chế chống trùng**: Kiểm tra trùng ở 3 level — Dictionary (in-memory), UniqueCodes DB (local), và cross-PO (GTIN). Nếu 1 mã xuất hiện trong 2 thùng → `MaBiTrung`, bỏ qua gửi AWS.

3. **Đóng thùng**: Mỗi thùng 24 sản phẩm. Khi đầy → tạo `cartonCode` (barcode) → kích hoạt thùng → gửi AWS. CartonID tự động tăng.

4. **GTIN-based codes**: Nhiều PO cùng GTIN dùng chung 1 bộ mã. Khi tạo DB mới, lọc bỏ các mã đã `Activated` ở PO khác cùng GTIN.

5. **AWS IoT là bước cuối**: Sau khi sản phẩm được scan + carton đầy + kích hoạt → publish lên AWS IoT Core → chờ response. Nếu timeout hoặc lỗi → retry.

6. **Thiếu sản phẩm (ThieuSanPham)**: Cơ chế an toàn — nếu thùng đầu tiên thiếu → reset thùng + đóng app; nếu nghiêm trọng → cảnh báo + dừng sản xuất + liên hệ nhà cung cấp.

7. **Dual Mode**: Hỗ trợ 2 PLC (Duo Mode) và 2 camera. Cấu hình linh hoạt qua `MSC.ini`.

8. **Path structure mới**: `C:/MasanSerialization_v2/PODatabases/yyyy-MM/GTIN/` — tách theo tháng sản xuất và GTIN thay vì theo PO để tiết kiệm storage khi nhiều PO cùng sản phẩm.