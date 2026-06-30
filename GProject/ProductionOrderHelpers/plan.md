Xây dựng hệ thống quản lý lệnh sản xuất.
## Tổng quan
Production Order (viết tắt PO) quản lý toàn bộ vòng đời PO: tạo PO, nạp mã từ DataPool, tracking CodeReader Active/Packing (đếm counter Pass/Fail/ReadFail/TimeOut/Error/NotFound/Duplicate/), và quản lý thùng carton.

Kiến trúc toàn bộ hệ thống luồng hoạt động như sau: (nói rõ để dễ thiết kế)

=> Nạp datapool để có bể dữ liệu => tạo po mới => chạy po => hoàn tất

Khi bắt đầu bật app sẽ ở trạng thái NeedLogin => đăng nhập thành công => Chuyển trạng thái Checking : Kiểm tra po chạy gần nhất ở file POHistory.db : nếu chưa có po nào chạy gần nhất, hoặc po chạy gần nhất có số lượng sản phẩm đã đóng thùng (package = orderQty) thì app chuyển trạng thái => Editing (hoặc nếu có po dang dở thì LoadPO) : Trạng thái chờ để người dùng chọn po mới => người dùng chọn po xong chuyển sang => CheckPO: nếu po chưa có file db thì tạo mới (theo cấu trúc bên dưới) => LoadPO reset biến po trong ram (ví dụ: public static ProductionOrder ProductionData) sau đó nhét thông tin po vào biến, lấy thông tin counter từ trong file db => Ready : đây là trạng thái sẵn sàng sản xuất nhưng chưa bắt đầu sản xuất => Khi người dùng gọi api start thì chuyển sang => PushDataToDic => Load toàn bộ dữ liệu Code vào Dictionary để khi đầu đọc trả mã về truy vấn và đánh dấu nhanh (xem mục 1 bên dưới để biết thêm), Load toàn bộ dữ liệu các thùng đã chạy để biết đang chạy đến thùng bao nhiêu (dựa vào Record_Packing_{orderNo}.db và Carton_{orderNo}.db) => Sau khi hoàn tất chuyển sang Running: ở trạng thái running luôn luôn kiểm tra xem trạng thái thiết bị nếu có lỗi là nhảy sang DeviceError, kiểm tra xem thùng hiện tại có mã hay chưa nếu chưa hoặc nếu số sản phẩm đã đóng >= Thông số CartonWarning trong config mà thùng sắp tới vẫn chưa có mã chuyển sang Pause đợi người dùng quét thùng.

Ở trạng thái running có 2 đầu đọc ,1 để active code và loại các sản phẩm lỗi từ sớm, 2 là để phân thùng (Phần đầu đọc này sẽ code sau nhưng nó sẽ liên quan đến cơ chế  Ghi dữ liệu cập nhật các data)
- `Update_Active_Status()` - Cập nhật trạng thái kích hoạt mã => của file db chính => Cập nhật mã đã dùng trong bể dữ liệu luôn.
- `Insert_Record_Active()` - Ghi bản ghi đầu active
- `Insert_Record_Package()` - Ghi bản ghi đầu package
- `Insert_Carton()` - Ghi thông tin thùng
- `Update_Carton()` - Cập nhật thùng

Sau đủ 24 sản phẩm thì đánh mã thùng vào dic và vào db.


Đầu tiên sẽ đánh dấu trong Dictionary chính trước => thêm vào hàng chờ và xử lý các record lưu vào db ở luồng riêng để làm sao luồng từ khi đầu đọc Recive mã => Kiểm tra => đánh dấu => Gửi trả cho PLC là nhanh nhất. 

1.1 một class mẫu của dictionary 

public class ProductionCodeData
    {
        public string Code { get; set; } // Mã sản phẩm
        public int codeID { get; set; } // cột ID trong csdl chính {orderNo}.db 
        public string cartonCode { get; set; } // Mã code thùng
        public string Activate_User { get; set; } // Mã id thùng trong csdl
        public string Main_Camera_Status { get; set; } // 0: Chưa kích hoạt,Active là 1, reject là -1
        public string Sub_Camera_Status { get; set; } // 0: Chưa kích hoạt từ camera phụ, 1: Đã kích hoạt, -1: Lỗi
        public string Sub_Camera_Activate_Datetime { get; set; } // 0: Chưa kích hoạt,Active là 1, reject là -1
        public string Activate_Datetime { get; set; } // Thời gian kích hoạt
        public string Production_Datetime { get; set; } // Thời gian sản xuất
    }

**Cấu trúc lưu trữ:** `<baseDataPath>/yyyy-MM/gtin/`

yyyy-MM dựa vào thời gian Po được tạo.

```
├── PO_List.db                    # Danh sách PO
├── POHistory.db                  # Lịch sử chạy PO
└── yyyy-MM/
    └── {gtin}/
        ├── {orderNo}.db          # UniqueCodes (mã sản phẩm) đây là nơi chính lưu thành quả chạy
        ├── Record_Active_{orderNo}.db   # Bản ghi CodeReader active
        ├── Record_Packing_{orderNo}.db   # Bản ghi CodeReader packing
        └── Carton_{orderNo}.db   # Thông tin thùng

==> Khi tạo file dữ liệu ở bước CheckPO 

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

=> {orderNo}.db là dữ liệu gồm các mã chưa dùng trong bể insert vào từ đầu luôn

Counter Classes
- passCount, failCount, duplicateCount
- readfailCount, notfoundCount, errorCount
- totalCount, totalCartonCount
- activatedCartonCount, errorCartonCount
- cartonID, carton_Packing_Code, carton_Packing_ID


- **WAL Mode** cho SQLite để tăng hiệu suất
- **PRAGMA synchronous = NORMAL** 
- **PRAGMA cache_size = 1000000**
- **PRAGMA temp_store = memory**
- Sử dụng **Transaction** cho batch insert