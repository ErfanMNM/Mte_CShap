---
name: DataPoolHelper Implementation
overview: Tạo DataPoolHelper trong GProject/Helper/ với cấu trúc giống VNQR nhưng dễ sử dụng hơn, theo pattern ProductionOrder trong MasanSerialization
todos:
  - id: data-pool-core
    content: Tạo DataPool.cs - Core class với instance properties và methods
    status: completed
  - id: data-pool-helper
    content: Tạo DataPoolHelper.cs - Static helper methods (Import, Query, Updater)
    status: completed
  - id: data-pool-manager
    content: Tạo DataPoolManager.cs - Static manager cho multiple pools
    status: completed
  - id: build-test
    content: Build và test compilation
    status: completed
isProject: false
---

## Tạo DataPoolHelper cho GProject

### Cấu trúc thư mục
```
GProject/Helper/DataPoolHelper/
├── DataPool.cs           // Core class - mỗi instance là 1 bể dữ liệu
├── DataPoolManager.cs    // Quản lý nhiều pool, lưu static instances
└── DataPoolHelper.cs     // Static helper methods (Import, Query, Updater)
```

### Pattern sử dụng (giống ProductionOrder)
```csharp
// Khai báo trong class chứa (Form, Service, etc.)
public static DataPool DataPool_s1 { get; set; } = new DataPool();

// Cấu hình
DataPool_s1.PoolName = "abc";  // Tên bể dữ liệu
DataPool_s1.DataPath = "C:\\GProject\\DataPool"; // Đường dẫn (mặc định)

// Sử dụng
DataPool_s1.Add("CODE123");                    // Thêm mã từ reader (auto-mark used)
DataPool_s1.AddManual("CODE456");              // Thêm mã thủ công
DataPool_s1.MarkUsed("CODE123", "LOT001");      // Đánh dấu đã dùng
DataPool_s1.MarkUnused("CODE123");              // Reset về chưa dùng
var unused = DataPool_s1.GetUnused();           // Lấy mã chưa dùng
```

### Core Features (từ VNQR)
1. **Import** - 3 cách nhập liệu:
   - `Manual(code)` - Nhập tay từng cái
   - `FromReader(code)` - Từ camera/reader, auto-mark used
   - `FromFile(csvPath)` - Nhập hàng loạt từ CSV

2. **Query** - Truy vấn:
   - `GetAll()` - Tất cả mã
   - `GetByCode(code)` - Tìm theo mã
   - `GetUnused()` - Mã chưa dùng
   - `GetUsed()` - Mã đã dùng

3. **Update** - Cập nhật:
   - `MarkUsed(code, batchID)` - Đánh dấu đã dùng
   - `MarkUnused(code)` - Reset về chưa dùng
   - `Delete(code)` - Xóa mã

4. **Lister** - Liệt kê:
   - `ListAllPools()` - Danh sách tất cả bể

### Tạo files

1. **DataPool.cs** - Class core với Properties:
   - `PoolName` - Tên bể
   - `DataPath` - Đường dẫn (default: C:\GProject\DataPool)
   - Instance methods: Add, AddManual, MarkUsed, MarkUnused, GetAll, GetUnused, GetByCode, Delete

2. **DataPoolHelper.cs** - Static class với:
   - `Import.Manual(poolName, code, ...)`
   - `Import.FromReader(poolName, code, batchID, ...)`
   - `Import.FromFile(poolName, csvPath, ...)`
   - `Query.GetAll(poolName)`
   - `Lister.ListAll()`

3. **DataPoolManager.cs** - Static manager:
   - Quản lý dictionary các DataPool instances
   - `GetOrCreate(poolName)` - Lấy hoặc tạo mới
   - Tự động tạo folder nếu chưa có