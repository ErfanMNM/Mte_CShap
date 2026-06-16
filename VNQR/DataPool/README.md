# VNQR.DataPool — Hướng dẫn sử dụng

`VNQR.DataPool` là bộ helper thao tác với **bể chứa dữ liệu QR** dưới dạng SQLite (đuôi `.vnqrdb`), chạy ở chế độ **WAL**.

## 1. Cấu trúc thư mục & file

Mặc định `DataPool.dataPath = "C:/VNQR/Databases"`. Có thể đổi trước khi gọi các hàm.

```
C:/VNQR/Databases/
├── <tên bể>.vnqrdb            ← bể dữ liệu (1 file = 1 bảng Codes)
└── Phieu/
    └── <CreateID>.vnqrdb      ← file phiếu tạo (chỉ sinh ra khi nhập từ file CSV)
```

### Bảng `Codes` (trong file `<tên bể>.vnqrdb`)

| Cột         | Kiểu      | Ý nghĩa                                                          |
|-------------|-----------|------------------------------------------------------------------|
| `ID`        | INTEGER PK AUTOINCREMENT | Khóa chính tự tăng                                |
| `Code`      | TEXT UNIQUE | Mã QR                                                            |
| `Status`    | INTEGER   | 0 = chưa dùng, 1 = đã dùng (enum `e_CodeStatus`)                 |
| `BatchID`   | TEXT      | Mã lô sản xuất (bắt buộc khi `Status = 1`)                       |
| `CreateTime`| TEXT      | Thời gian tạo bản ghi (`yyyy-MM-dd HH:mm:ss`)                    |
| `CreateID`  | TEXT      | Mã phiếu tạo: `User:<tên>` khi nhập tay, `Reader` khi từ camera, `<CreateID>` khi nhập từ file |
| `Note`      | TEXT      | Ghi chú                                                          |

### Bảng `PhieuTao` (trong file `<CreateID>.vnqrdb` — chỉ tạo khi nhập từ file)

| Cột            | Kiểu     | Ý nghĩa                                       |
|----------------|----------|-----------------------------------------------|
| `CreateID`     | TEXT PK  | Mã phiếu tạo                                  |
| `UserName`     | TEXT     | Tên user thực hiện nhập                       |
| `CreateTime`   | TEXT     | Thời gian tạo phiên nhập                      |
| `Note`         | TEXT     | Ghi chú phiên nhập                            |
| `ImportMethod` | TEXT     | `Manual` / `Reader` / `File` (enum `e_ImportMethod`) |
| `ImportSource` | TEXT     | Đường dẫn file CSV (khi nhập từ file)         |
| `ImportCount`  | INTEGER  | Số mã thêm mới thành công                     |

---

## 2. Khởi tạo nhanh

```csharp
using VNQR.DataPool;

// (Tùy chọn) Đổi đường dẫn gốc
DataPool.dataPath = @"D:\MyApp\VNQR";

// Tạo pool trước khi dùng (idempotent, gọi được nhiều lần)
PoolHelper.EnsurePool("MainPool");
```

---

## 3. Nhập liệu (class `Import`)

Cả 3 hàm đều trả về `TResult` gồm `issuccess`, `message`, `data`, `count`.

### 3.1. Nhập tay từng mã — `Import.Manual`

Cho phép truyền **Code, Status, BatchID, CreateID, Note, userName**. Hệ thống tự điền `ID` và `CreateTime`.

- Nếu `Code` đã tồn tại → trả lỗi.
- Nếu `Status = Used (1)` mà `BatchID` rỗng → trả lỗi.
- Nếu `createID` rỗng → tự gán `User:<userName>`.

```csharp
var r = Import.Manual(
    poolName: "MainPool",
    code:     "QR-0001",
    status:   (int)e_CodeStatus.Unused,   // mặc định = 0, có thể bỏ qua
    batchID:  "",                          // có thể bỏ trống khi chưa dùng
    createID: "",                          // rỗng -> "User:<userName>"
    note:     "Nhập tay từ form quản lý",
    userName: "an.nv"
);

if (!r.issuccess) MessageBox.Show(r.message);
```

### 3.2. Nhập từ đầu đọc mã (camera) — `Import.FromReader`

Cho phép truyền **Code, BatchID, CreateID, Note** (mặc định `CreateID = "Reader"`). Hệ thống tự điền `CreateTime`.

Quy tắc xử lý:

| Tình trạng mã trong DB                | Hành động                                              |
|---------------------------------------|--------------------------------------------------------|
| Chưa từng tồn tại                     | Insert mới với `Status = Used (1)`                     |
| Đã tồn tại, `Status = Unused (0)`     | Update lên `Status = Used (1)` + cập nhật `BatchID`   |
| Đã tồn tại, `Status = Used (1)`       | Trả lỗi, **không lưu**                                |

```csharp
// Trong callback nhận mã từ camera:
var r = Import.FromReader(
    poolName: "MainPool",
    code:     "QR-0001",
    batchID:  "BATCH-2026-06-17-A",   // bắt buộc
    createID: "Reader",               // mặc định, có thể bỏ qua
    note:     ""
);

if (!r.issuccess && r.message.Contains("đã được sử dụng"))
{
    // mã trùng đã dùng -> báo lỗi
}
```

### 3.3. Nhập từ file CSV — `Import.FromFile`

Truyền **đường dẫn file, tên user, tên cột Code, tên cột Note, CreateID**. Hệ thống tự điền `ID, Status, BatchID, CreateTime, CreateID`. Có thể bỏ trống cột Note nếu CSV không có.

- Mã trùng → bỏ qua (đếm vào `skipCount`), KHÔNG ghi đè.
- Đồng thời tạo/ghi đè file `<CreateID>.vnqrdb` trong thư mục `Phieu/` với thông tin phiên nhập.

```csharp
var r = Import.FromFile(
    poolName:   "MainPool",
    csvPath:    @"C:\data\qr_list.csv",
    userName:   "an.nv",
    createID:   "IMPORT-20260617-001",
    codeColumn: "Code",
    noteColumn: "Note",
    note:       "Nhập danh sách từ khách hàng A"
);

// r.count = số mã thêm mới
// r.message = "Nhập từ file hoàn tất: N thêm mới, M bị bỏ qua (trùng mã)."
```

**Định dạng CSV mẫu:**

```csv
Code,Note
QR-0001,Lô A
QR-0002,Lô A
QR-0003,Lô B
```

> Bộ tách CSV tối thiểu hỗ trợ trường có dấu phẩy nếu được bao bởi nháy kép `"a,b"`, và escape `""` bên trong.

---

## 4. Truy vấn (helper trong `Import`)

```csharp
// Kiểm tra mã đã tồn tại
bool exists = Import.CodeExists("MainPool", "QR-0001");

// Lấy 1 bản ghi theo Code
TResult r = Import.GetByCode("MainPool", "QR-0001");
if (r.issuccess && r.data.Rows.Count > 0)
{
    var row = r.data.Rows[0];
    int status    = Convert.ToInt32(row["Status"]);
    string batch  = row["BatchID"].ToString();
    string note   = row["Note"].ToString();
}
```

---

## 5. Cập nhật (class `Updater`)

Các hàm trả về `true` nếu có dòng bị ảnh hưởng, `false` nếu không tìm thấy mã.

### 5.1. `Updater.UpdateStatus` — Cập nhật trạng thái

- Cột nào truyền **rỗng** → giữ nguyên giá trị cũ.
- `Status` luôn bị ghi đè bằng `newStatus`.

```csharp
// Đánh dấu đã dùng
bool ok = Updater.UpdateStatus(
    poolName: "MainPool",
    code:     "QR-0001",
    newStatus: (int)e_CodeStatus.Used,
    batchID:  "BATCH-2026-06-17-A",
    createID: "Reader",
    note:     ""
);
```

### 5.2. `Updater.Update` — Cập nhật mềm (chỉ ghi cột truyền vào)

Truyền `null` cho cột nào → cột đó **không bị đụng**.

```csharp
// Chỉ đổi Note
Updater.Update("MainPool", "QR-0001", note: "Đã sửa chú thích");

// Đổi nhiều cột cùng lúc
Updater.Update("MainPool", "QR-0001",
    status:   (int)e_CodeStatus.Used,
    batchID:  "BATCH-2026-06-17-A",
    createID: "Reader",
    note:     "Đã cập nhật từ form");
```

### 5.3. `Updater.MarkUsed` / `Updater.MarkUnused` — tiện ích

```csharp
// Đánh dấu đã dùng (BatchID bắt buộc)
Updater.MarkUsed("MainPool", "QR-0001", "BATCH-2026-06-17-A");

// Reset về chưa dùng, đồng thời xóa BatchID
Updater.MarkUnused("MainPool", "QR-0001");
```

### 5.4. `Updater.Delete` — xóa cứng 1 mã

```csharp
bool ok = Updater.Delete("MainPool", "QR-0001");
```

---

## 6. Enum & kiểu dữ liệu

```csharp
public enum e_ImportMethod { Manual, Reader, File }
public enum e_CodeStatus   { Unused = 0, Used = 1 }

public class TResult
{
    public bool     issuccess;
    public string   message;
    public DataTable data;   // null nếu không có
    public int      count;
}
```

---

## 7. Mẫu nhanh dùng trong form

```csharp
// 1) Khởi tạo 1 lần khi form load
PoolHelper.EnsurePool("MainPool");

// 2) Nút "Nhập tay"
private void btnManual_Click(object sender, EventArgs e)
{
    var r = Import.Manual("MainPool", txtCode.Text, userName: "an.nv");
    MessageBox.Show(r.message);
}

// 3) Callback camera
private void OnCameraRead(string code)
{
    var r = Import.FromReader("MainPool", code, "BATCH-CURRENT");
    if (!r.issuccess) ShowError(r.message);
}

// 4) Nút "Sửa note"
private void btnEditNote_Click(object sender, EventArgs e)
{
    Updater.Update("MainPool", txtCode.Text, note: txtNote.Text);
}
```

---

## 8. Lưu ý

- Mỗi lần gọi `EnsurePool` / `EnsurePhieu` đều idempotent — có thể gọi nhiều lần không sao.
- Khi `Status = Used (1)` thì `BatchID` **bắt buộc** phải có giá trị.
- Khi nhập từ file CSV, mã trùng sẽ bị **bỏ qua** (không ghi đè). Nếu cần ghi đè, dùng `Updater.Update` sau khi nhập.
- File phiếu tạo chỉ sinh ra ở bước nhập từ file. Hai cách còn lại (Manual, Reader) **không** tạo file phiếu.
- Mọi kết nối SQLite đều dùng `using` nên tài nguyên được giải phóng tự động. WAL mode cho phép nhiều tiến trình đọc/ghi đồng thời.
