Xây dựng hệ thống quản lý lệnh sản xuất.
## Tổng quan
Production Order (viết tắt PO) quản lý toàn bộ vòng đời PO: tạo PO, nạp mã từ DataPool, tracking CodeReader Active/Packing (đếm counter Pass/Fail/ReadFail/TimeOut/Error/NotFound/Duplicate/), và quản lý thùng carton.

Kiến trúc toàn bộ hệ thống 

**Cấu trúc lưu trữ:** `<baseDataPath>/yyyy-MM/gtin/`

yyyy-MM dựa vào thời gian Po được tạo.

```
├── PO_List.db                    # Danh sách PO
├── POHistory.db                  # Lịch sử chạy PO
└── yyyy-MM/
    └── {gtin}/
        ├── {orderNo}.db          # UniqueCodes (mã sản phẩm) đây là nơi chính lưu thành quả chạy.
        ├── Record_Active_{orderNo}.db   # Bản ghi CodeReader active
        ├── Record_Packing_{orderNo}.db   # Bản ghi CodeReader packing
        └── Carton_{orderNo}.db   # Thông tin thùng

Một Po gồm các thông số sau:

public string orderNo { get; set; } = "-";
public string site { get; set; } = "-";
public string factory { get; set; } = "-";
public string productionLine { get; set; } = "-";
public string productionDate { get; set; } = "-";
public string shift { get; set; } = "-";
public string orderQty { get; set; } = "-";
public string lotNumber { get; set; } = "-";
public string productCode { get; set; } = "-";
public string productName { get; set; } = "-";
public string gtin { get; set; } = "-";
public string customerOrderNo { get; set; } = "-";
public string uom { get; set; } = "-";
public string cartonSize { get; set; } = "-";
public string totalCZCode { get; set; } = "-";
public Product_Counter Active_Counter { get; set; } = new Product_Counter(); => Của đầu đọc Active
public Product_Counter Package_Counter { get; set; } = new Product_Counter(); => Của đầu đọc