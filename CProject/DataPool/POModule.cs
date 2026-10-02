using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using static System.Net.Mime.MediaTypeNames;

namespace CProject.Module
{
    public class POInfo
    {
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
        public string packSize { get; set; } = "-";
        public string totalCZCode { get; set; } = "-";
        public Product_Counter Counter { get; set; } = new Product_Counter();

        //lưu danh sách thùng và thông tin của từng thùng để dùng ở đây cho đỡ cấn tùm lum
        public Dictionary<string, CartonInfo> CartonInfo { get; set; } = new Dictionary<string, CartonInfo>();
        public Dictionary<string, ProductInfo> ProductInfo { get; set; } = new Dictionary<string, ProductInfo>();
    }

    public class Product_Counter
    {
        public int totalCount { get; set; } = 0; //total count số lượng sản phẩm
        public int passCount { get; set; } = 0;//pass count số tốt
        public int failCount { get; set; } = 0;//fail count số xấu
        public int duplicateCount { get; set; } = 0; //trùng
        public int readfailCount { get; set; } = 0;
        public int notfoundCount { get; set; } = 0;
        public int errorCount { get; set; } = 0;
        public int formatErrorCount { get; set; } = 0;

    }

    public class CartonInfo
    {
        public string carton_Code { get; set; } = "0";
        public string carton_Start_Time { get; set; } = "0";
        public string carton_End_Time { get; set; } = "0";
    }

    public class ProductInfo
    {
        public string product_Code { get; set; } = "0";
        public string product_Start_Time { get; set; } = "0";
        public string product_End_Time { get; set; } = "0";
    }

    //K1 : Kiểm tra xem file db PO_List.db đã tồn tại hay chưa, nếu chưa thì tạo mới nằm ở : C:/CProject/PoDatabase/PO_List.db
    //    CREATE TABLE "ProductionOrder" (
    //	"ID"	INTEGER NOT NULL UNIQUE,
    //	"orderNo"	TEXT NOT NULL UNIQUE,
    //	"site"	TEXT NOT NULL,
    //	"factory"	TEXT NOT NULL,
    //	"productionLine"	TEXT NOT NULL,
    //	"productionDate"	TEXT NOT NULL,
    //	"shift"	TEXT NOT NULL,
    //	"orderQty"	INTEGER NOT NULL,
    //	"lotNumber"	TEXT NOT NULL,
    //	"productCode"	TEXT NOT NULL,
    //	"productName"	TEXT NOT NULL,
    //	"gtin"	TEXT NOT NULL,
    //	"customerOrderNo"	TEXT NOT NULL,
    //	"uom"	TEXT NOT NULL,
    //	"packSize"	INTEGER NOT NULL DEFAULT 24,
    //	"CreateDate"	TEXT NOT NULL,
    //	"CreateUser"	TEXT NOT NULL,
    //	PRIMARY KEY("ID" AUTOINCREMENT)
    //);
    //K2: Tạo ghi PO mới vào PO_List (Có kiểm tra trùng, tồn tại này nọ nhé)

    //K3: Tạo dữ liệu sản xuất cho từng PO
    //    **Cấu trúc lưu trữ file chạy của PO:** `C:/CProject/PoDatabase/yyyy-MM/gtin/`

    //yyyy-MM dựa vào thời gian Po được tạo (CreateDate)

    //```
    //├── PO_List.db                    # Danh sách PO - và thông tin PO
    //├── POHistory.db                  # Lịch sử chạy PO
    //└── yyyy-MM/
    //    └── {gtin}/
    //        ├── {orderNo}.db          # UniqueCodes (mã sản phẩm) đây là nơi chính lưu thành quả chạy
    //        ├── Record_{orderNo}.db   # Bản ghi CodeReader record chạy, chỗ này để đọc counter ra luôn
    //        └── Carton_{orderNo}.db   # Thông tin thùng 

    //K1.1: Tạo dữ liệu sản xuất cho PO mới vào file {orderNo}.db dựa theo GTIN của PO để lấy dữ liệu từ Pool trong thư mục 
    // C:\CProject\DataPool => Lấy hết tất cả mã code có Status = 0 vào file {orderNo}.db
    // K1.2 Tạo file lưu Record_{orderNo}.db

    //A2: Lấy thông tin PO này nọ

    public class PODatabase
    {
        private readonly string _dbPath = "C:/CProject/PoDatabase/PO_List.db";
        private readonly string _connectionString;

        public PODatabase()
        {
            var dir = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);
            _connectionString = $"Data Source={_dbPath}";
        }

        // K1: Initialize database and table
        public void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS ProductionOrder (
                    ID INTEGER NOT NULL UNIQUE,
                    orderNo TEXT NOT NULL UNIQUE,
                    site TEXT NOT NULL,
                    factory TEXT NOT NULL,
                    productionLine TEXT NOT NULL,
                    productionDate TEXT NOT NULL,
                    shift TEXT NOT NULL,
                    orderQty INTEGER NOT NULL,
                    lotNumber TEXT NOT NULL,
                    productCode TEXT NOT NULL,
                    productName TEXT NOT NULL,
                    gtin TEXT NOT NULL,
                    customerOrderNo TEXT NOT NULL,
                    uom TEXT NOT NULL,
                    packSize INTEGER NOT NULL DEFAULT 24,
                    CreateDate TEXT NOT NULL,
                    CreateUser TEXT NOT NULL,
                    PRIMARY KEY(ID AUTOINCREMENT)
                );";

            using var command = new SqliteCommand(createTableSql, connection);
            command.ExecuteNonQuery();
        }
        // K2: Create new PO record
        public (bool success, string message, int id) CreateProductionOrder(POInfo po, string createUser)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Check if orderNo already exists
            var checkSql = "SELECT COUNT(*) FROM ProductionOrder WHERE orderNo = @orderNo";
            using var checkCmd = new SqliteCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@orderNo", po.orderNo);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (exists)
                return (false, $"OrderNo '{po.orderNo}' already exists", -1);

            var insertSql = @"
                INSERT INTO ProductionOrder 
                (orderNo, site, factory, productionLine, productionDate, shift, 
                 orderQty, lotNumber, productCode, productName, gtin, 
                 customerOrderNo, uom, packSize, CreateDate, CreateUser)
                VALUES (@orderNo, @site, @factory, @productionLine, @productionDate, @shift,
                        @orderQty, @lotNumber, @productCode, @productName, @gtin,
                        @customerOrderNo, @uom, @packSize, @createDate, @createUser);
                SELECT last_insert_rowid();";

            using var insertCmd = new SqliteCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@orderNo", po.orderNo);
            insertCmd.Parameters.AddWithValue("@site", po.site);
            insertCmd.Parameters.AddWithValue("@factory", po.factory);
            insertCmd.Parameters.AddWithValue("@productionLine", po.productionLine);
            insertCmd.Parameters.AddWithValue("@productionDate", po.productionDate);
            insertCmd.Parameters.AddWithValue("@shift", po.shift);
            insertCmd.Parameters.AddWithValue("@orderQty", int.TryParse(po.orderQty, out var qty) ? qty : 0);
            insertCmd.Parameters.AddWithValue("@lotNumber", po.lotNumber);
            insertCmd.Parameters.AddWithValue("@productCode", po.productCode);
            insertCmd.Parameters.AddWithValue("@productName", po.productName);
            insertCmd.Parameters.AddWithValue("@gtin", po.gtin);
            insertCmd.Parameters.AddWithValue("@customerOrderNo", po.customerOrderNo);
            insertCmd.Parameters.AddWithValue("@uom", po.uom);
            insertCmd.Parameters.AddWithValue("@packSize", int.TryParse(po.packSize, out var ps) ? ps : 24);
            insertCmd.Parameters.AddWithValue("@createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.Parameters.AddWithValue("@createUser", createUser);

            var id = Convert.ToInt32(insertCmd.ExecuteScalar());
            return (true, "PO created successfully", id);
        }


    }
}