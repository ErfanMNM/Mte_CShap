namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Cấu hình đường dẫn và SQL schemas cho Production Order
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Thư mục gốc lưu trữ PODatabases
        /// </summary>
        public static string BasePath { get; set; } = "C:/GProject/PODatabases";

        /// <summary>
        /// Tên file danh sách PO
        /// </summary>
        public const string POListFileName = "PO_List.db";

        /// <summary>
        /// Tên file lịch sử PO
        /// </summary>
        private const string POHistoryFileName = "POHistory.db";

        /// <summary>
        /// Lấy đường dẫn PO_List.db, tự động tạo thư mục nếu chưa có
        /// </summary>
        public static string GetPOListPath()
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);
            return Path.Combine(BasePath, POListFileName);
        }

        /// <summary>
        /// Lấy đường dẫn POHistory.db
        /// </summary>
        public static string GetPOHistoryPath()
            => Path.Combine(BasePath, POHistoryFileName);

        /// <summary>
        /// Lấy đường dẫn thư mục theo cấu trúc yyyy-MM/gtin/
        /// </summary>
        public static string GetBasePath(string gtin, string productionDate)
        {
            if (string.IsNullOrEmpty(gtin) || string.IsNullOrEmpty(productionDate))
                return BasePath;

            string monthFolder;
            try
            {
                DateTime dt = DateTime.Parse(productionDate);
                monthFolder = dt.ToString("yyyy-MM");
            }
            catch
            {
                monthFolder = DateTime.Now.ToString("yyyy-MM");
            }

            string path = Path.Combine(BasePath, monthFolder, gtin);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Lấy đường dẫn thư mục từ orderNo (cần query PO_List trước)
        /// </summary>
        public static string GetBasePathByOrderNo(string orderNo)
        {
            var r = POLoader.GetByOrderNo(orderNo);
            if (!r.IsSuccess || r.Data == null || r.Data.Rows.Count == 0)
                return BasePath;

            var row = r.Data.Rows[0];
            string gtin = row["Gtin"]?.ToString() ?? "";
            string prodDate = row["ProductionDate"]?.ToString() ?? "";
            return GetBasePath(gtin, prodDate);
        }

        // ================== SQL SCHEMAS ==================

        /// <summary>
        /// Schema cho bảng PO trong PO_List.db
        /// </summary>
        public const string SQL_CREATE_PO_LIST = @"
            CREATE TABLE IF NOT EXISTS PO (
                orderNo         TEXT NOT NULL PRIMARY KEY,
                site            TEXT NOT NULL DEFAULT '',
                factory         TEXT NOT NULL DEFAULT '',
                productionLine  TEXT NOT NULL DEFAULT '',
                productionDate  TEXT NOT NULL DEFAULT '',
                shift           TEXT NOT NULL DEFAULT '',
                orderQty        INTEGER NOT NULL DEFAULT 0,
                lotNumber       TEXT NOT NULL DEFAULT '',
                productCode     TEXT NOT NULL DEFAULT '',
                productName     TEXT NOT NULL DEFAULT '',
                gtin            TEXT NOT NULL DEFAULT '',
                customerOrderNo TEXT NOT NULL DEFAULT '',
                uom             TEXT NOT NULL DEFAULT 'PCS',
                CreatedTime     TEXT NOT NULL DEFAULT '',
                ModifiedTime    TEXT NOT NULL DEFAULT ''
            );
            CREATE INDEX IF NOT EXISTS IDX_PO_gtin ON PO(gtin);
            PRAGMA journal_mode=WAL;
        ";

        /// <summary>
        /// Schema cho bảng UniqueCodes trong {orderNo}.db
        /// </summary>
        public const string SQL_CREATE_PO_CODES = @"
            CREATE TABLE IF NOT EXISTS UniqueCodes (
                ID             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Code           TEXT NOT NULL UNIQUE,
                cartonCode     TEXT NOT NULL DEFAULT '0',
                Status         INTEGER NOT NULL DEFAULT 0,
                ActivateDate   TEXT NOT NULL DEFAULT '0',
                ProductionDate TEXT NOT NULL DEFAULT '0',
                ActivateUser   TEXT NOT NULL DEFAULT '',
                PackingDate    TEXT NOT NULL DEFAULT '0',
                Send_Status    TEXT NOT NULL DEFAULT 'Pending',
                Recive_Status  TEXT NOT NULL DEFAULT 'Pending'
            );
            CREATE INDEX IF NOT EXISTS IDX_UC_Status ON UniqueCodes(Status);
            CREATE INDEX IF NOT EXISTS IDX_UC_cartonCode ON UniqueCodes(cartonCode);
            CREATE UNIQUE INDEX IF NOT EXISTS IDX_UC_Code ON UniqueCodes(Code);
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA cache_size=10000;
            PRAGMA temp_store=memory;
        ";

        /// <summary>
        /// Schema cho bảng Records trong Record_{orderNo}.db (chung cho cả activate và packing)
        /// </summary>
        public const string SQL_CREATE_RECORD = @"
            CREATE TABLE IF NOT EXISTS Records (
                ID             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Code           TEXT NOT NULL DEFAULT 'FAIL',
                cartonCode     TEXT NOT NULL DEFAULT '0',
                Status         TEXT NOT NULL DEFAULT '0',
                PLC_Status     TEXT NOT NULL DEFAULT 'FAIL',
                ActivateDate   TEXT NOT NULL DEFAULT '0',
                ActivateUser   TEXT NOT NULL DEFAULT '',
                PackingDate    TEXT NOT NULL DEFAULT '0',
                PackingUser    TEXT NOT NULL DEFAULT '',
                ProductionDate TEXT NOT NULL DEFAULT '0'
            );
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA cache_size=10000;
            PRAGMA temp_store=memory;
        ";

        /// <summary>
        /// Schema cho bảng Carton trong Carton_{orderNo}.db
        /// </summary>
        public const string SQL_CREATE_CARTON = @"
            CREATE TABLE IF NOT EXISTS Carton (
                ID                  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                cartonCode          TEXT NOT NULL DEFAULT '0',
                Start_Datetime      TEXT NOT NULL DEFAULT '0',
                Completed_Datetime   TEXT NOT NULL DEFAULT '0',
                ActivateUser        TEXT NOT NULL DEFAULT '',
                ProductionDate      TEXT NOT NULL DEFAULT '0'
            );
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA cache_size=10000;
            PRAGMA temp_store=memory;
        ";

        /// <summary>
        /// Schema cho bảng CartonCode trong Carton_{orderNo}.db — log mỗi lần scan thùng
        /// </summary>
        public const string SQL_CREATE_CARTON_CODE = @"
            CREATE TABLE IF NOT EXISTS CartonCode (
                ID            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                MachineName   TEXT NOT NULL,
                CartonCode    TEXT NOT NULL,
                CartonIndex   INTEGER NOT NULL DEFAULT 0,
                ScanAt        TEXT NOT NULL,
                Mode          TEXT NOT NULL DEFAULT 'scan',
                Result        TEXT NOT NULL DEFAULT ''
            );
            CREATE INDEX IF NOT EXISTS IDX_CC_MachineName ON CartonCode(MachineName);
            CREATE INDEX IF NOT EXISTS IDX_CC_CartonCode ON CartonCode(CartonCode);
            PRAGMA journal_mode=WAL;
        ";

        /// <summary>
        /// Schema cho bảng POHistory
        /// </summary>
        public const string SQL_CREATE_PO_HISTORY = @"
            CREATE TABLE IF NOT EXISTS POHistory (
                ID             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                PO             TEXT NOT NULL,
                ProductionDate TEXT NOT NULL DEFAULT '',
                StartTime      TEXT NOT NULL DEFAULT '',
                EndTime        TEXT NOT NULL DEFAULT '',
                Status         TEXT NOT NULL DEFAULT '',
                UserName       TEXT NOT NULL DEFAULT ''
            );
            CREATE INDEX IF NOT EXISTS IDX_PH_PO ON POHistory(PO);
            CREATE INDEX IF NOT EXISTS IDX_PH_Status ON POHistory(Status);
            PRAGMA journal_mode=WAL;
        ";

        // ================== DATABASE PATH HELPERS ==================

        /// <summary>
        /// Đảm bảo database tồn tại với schema cho trước
        /// </summary>
        private static void EnsureDB(string dbPath, string sqlSchema)
        {
            string folder = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using var con = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sqlSchema;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Đảm bảo PO_List.db tồn tại
        /// </summary>
        public static void EnsurePOList() => EnsureDB(GetPOListPath(), SQL_CREATE_PO_LIST);

        /// <summary>
        /// Đảm bảo POHistory.db tồn tại
        /// </summary>
        public static void EnsurePOHistory() => EnsureDB(GetPOHistoryPath(), SQL_CREATE_PO_HISTORY);

        /// <summary>
        /// Lấy đường dẫn file database PO (UniqueCodes)
        /// </summary>
        public static string GetPODBPath(string orderNo)
            => Path.Combine(GetBasePathByOrderNo(orderNo), $"{orderNo}.db");

        /// <summary>
        /// Lấy đường dẫn file Record
        /// </summary>
        public static string GetRecordPath(string orderNo)
            => Path.Combine(GetBasePathByOrderNo(orderNo), $"Record_{orderNo}.db");

        /// <summary>
        /// Lấy đường dẫn file Carton
        /// </summary>
        public static string GetCartonPath(string orderNo)
            => Path.Combine(GetBasePathByOrderNo(orderNo), $"Carton_{orderNo}.db");

        /// <summary>
        /// Lấy đường dẫn DataPool theo GTIN
        /// </summary>
        public static string GetDataPoolPath(string gtin)
            => Path.Combine(GProject.DataPoolHelper.DataPoolStatic.DataPath, $"{gtin}.db");
    }
}
