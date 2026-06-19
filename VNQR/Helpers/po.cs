using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace VNQR.Helpers
{
    // po = Production Order. Quản lý PO gồm 4 file SQLite trong thư mục yyyy-MM/gtin/:
    //   - <PO>.db                  — bảng UniqueCodes chứa codes cho PO này
    //   - Record_Active_<PO>.db    — tracking sản phẩm đi qua camera Active
    //   - Record_Packing_<PO>.db   — tracking sản phẩm đi qua camera Packing
    //   - Carton_<PO>.db           — tracking thùng carton
    // Và 1 file PO_List.db chung trong baseDataPath quản lý danh sách PO.
    // Codes được nạp từ DataPool (<gtin>.vnqrdb) vào <PO>.db khi khởi tạo PO.

    public class po
    {
        // ================== CONFIG ==================
        public static class Config
        {
            public static string baseDataPath = "C:/VNQR/PODatabases";
            public static string poListFileName = "PO_List.db";

            // Lấy đường dẫn PO_List.db
            public static string GetPOListPath()
            {
                if (!Directory.Exists(baseDataPath))
                    Directory.CreateDirectory(baseDataPath);
                return Path.Combine(baseDataPath, poListFileName);
            }

            // Lấy base path: <baseDataPath>/yyyy-MM/gtin/
            public static string GetBasePath(string gtin, string productionDate)
            {
                if (string.IsNullOrEmpty(gtin) || string.IsNullOrEmpty(productionDate))
                    return baseDataPath;

                string monthFolder = "";
                try
                {
                    DateTime dt = DateTime.Parse(productionDate);
                    monthFolder = dt.ToString("yyyy-MM");
                }
                catch
                {
                    monthFolder = DateTime.Now.ToString("yyyy-MM");
                }

                string path = Path.Combine(baseDataPath, monthFolder, gtin);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }

            // Lấy base path khi chỉ có orderNo (tự lookup từ PO_List)
            public static string GetBasePathByOrderNo(string orderNo)
            {
                var r = POLoader.GetByOrderNo(orderNo);
                if (!r.issuccess || r.data == null || r.data.Rows.Count == 0)
                    return baseDataPath;

                var row = r.data.Rows[0];
                string gtin = row["gtin"]?.ToString() ?? "";
                string prodDate = row["productionDate"]?.ToString() ?? "";
                return GetBasePath(gtin, prodDate);
            }

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
                    uom             TEXT NOT NULL DEFAULT '',
                    CreatedTime     TEXT NOT NULL DEFAULT '',
                    ModifiedTime    TEXT NOT NULL DEFAULT ''
                );
                CREATE INDEX IF NOT EXISTS IDX_PO_gtin ON PO(gtin);
                PRAGMA journal_mode=WAL;
            ";

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
                CREATE INDEX IF NOT EXISTS IDX_UC_Status      ON UniqueCodes(Status);
                CREATE INDEX IF NOT EXISTS IDX_UC_cartonCode  ON UniqueCodes(cartonCode);
                CREATE UNIQUE INDEX IF NOT EXISTS IDX_UC_Code ON UniqueCodes(Code);
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA cache_size=10000;
                PRAGMA temp_store=memory;
            ";

            public const string SQL_CREATE_RECORD_ACTIVE = @"
                CREATE TABLE IF NOT EXISTS Records (
                    ID             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Code           TEXT NOT NULL DEFAULT 'FAIL',
                    cartonCode     TEXT NOT NULL DEFAULT '0',
                    Status         TEXT NOT NULL DEFAULT '0',
                    PLC_Status     TEXT NOT NULL DEFAULT 'FAIL',
                    ActivateDate   TEXT NOT NULL DEFAULT '0',
                    ActivateUser   TEXT NOT NULL DEFAULT '',
                    ProductionDate TEXT NOT NULL DEFAULT '0'
                );
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA cache_size=10000;
                PRAGMA temp_store=memory;
            ";

            public const string SQL_CREATE_RECORD_PACKING = @"
                CREATE TABLE IF NOT EXISTS Records (
                    ID             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Code           TEXT NOT NULL DEFAULT 'FAIL',
                    cartonCode     TEXT NOT NULL DEFAULT '0',
                    Status         TEXT NOT NULL DEFAULT '0',
                    PLC_Status     TEXT NOT NULL DEFAULT 'FAIL',
                    PackingDate    TEXT NOT NULL DEFAULT '0',
                    PackingUser    TEXT NOT NULL DEFAULT '',
                    ProductionDate TEXT NOT NULL DEFAULT '0'
                );
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA cache_size=10000;
                PRAGMA temp_store=memory;
            ";

            public const string SQL_CREATE_CARTON = @"
                CREATE TABLE IF NOT EXISTS Carton (
                    ID                  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    cartonCode          TEXT NOT NULL DEFAULT '0',
                    Start_Datetime      TEXT NOT NULL DEFAULT '0',
                    Completed_Datetime  TEXT NOT NULL DEFAULT '0',
                    ActivateUser        TEXT NOT NULL DEFAULT '',
                    ProductionDate      TEXT NOT NULL DEFAULT '0'
                );
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA cache_size=10000;
                PRAGMA temp_store=memory;
            ";

            // Tạo DB + bảng nếu chưa tồn tại
            private static void EnsureDB(string dbPath, string sqlSchema)
            {
                string folder = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(sqlSchema, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            public static void EnsurePOList()
            {
                EnsureDB(GetPOListPath(), SQL_CREATE_PO_LIST);
            }

            public static string GetPODBPath(string orderNo)
                => Path.Combine(GetBasePathByOrderNo(orderNo), $"{orderNo}.db");

            public static string GetRecordActivePath(string orderNo)
                => Path.Combine(GetBasePathByOrderNo(orderNo), $"Record_Active_{orderNo}.db");

            public static string GetRecordPackingPath(string orderNo)
                => Path.Combine(GetBasePathByOrderNo(orderNo), $"Record_Packing_{orderNo}.db");

            public static string GetCartonPath(string orderNo)
                => Path.Combine(GetBasePathByOrderNo(orderNo), $"Carton_{orderNo}.db");

            public static string GetDataPoolPath(string gtin)
                => Path.Combine(VNQR.DataPool.DataPool.dataPath, $"{gtin}.vnqrdb");
        }

        // ================== MODELS ==================
        #region Models
        public class TResult
        {
            public bool issuccess { get; set; }
            public string message { get; set; }
            public DataTable data { get; set; }
            public int count { get; set; }

            public TResult(bool issuccess, string message, int count = 0, DataTable data = null)
            {
                this.issuccess = issuccess;
                this.message = message;
                this.data = data;
                this.count = count;
            }
        }

        public class POInfo
        {
            public string orderNo { get; set; } = "";
            public string site { get; set; } = "";
            public string factory { get; set; } = "";
            public string productionLine { get; set; } = "";
            public string productionDate { get; set; } = "";
            public string shift { get; set; } = "";
            public int orderQty { get; set; } = 0;
            public string lotNumber { get; set; } = "";
            public string productCode { get; set; } = "";
            public string productName { get; set; } = "";
            public string gtin { get; set; } = "";
            public string customerOrderNo { get; set; } = "";
            public string uom { get; set; } = "";
            public string CreatedTime { get; set; } = "";
            public string ModifiedTime { get; set; } = "";
        }

        public class POCodeData
        {
            public int id { get; set; }
            public string code { get; set; } = "";
            public string cartonCode { get; set; } = "0";
            public int status { get; set; } = 0;
            public string activateDate { get; set; } = "0";
            public string productionDate { get; set; } = "0";
            public string activateUser { get; set; } = "";
            public string packingDate { get; set; } = "0";
            public string sendStatus { get; set; } = "Pending";
            public string receiveStatus { get; set; } = "Pending";
        }

        public class PORecordData
        {
            public string code { get; set; } = "FAIL";
            public string cartonCode { get; set; } = "0";
            public string status { get; set; } = "0";
            public string plcStatus { get; set; } = "FAIL";
            public string activateDate { get; set; } = "0";
            public string activateUser { get; set; } = "";
            public string packingDate { get; set; } = "0";
            public string packingUser { get; set; } = "";
            public string productionDate { get; set; } = "0";
        }

        public class POCartonData
        {
            public int id { get; set; } = 0;
            public string cartonCode { get; set; } = "0";
            public string startDatetime { get; set; } = "0";
            public string completedDatetime { get; set; } = "0";
            public string activateUser { get; set; } = "";
            public string productionDate { get; set; } = "0";
        }

        public enum e_Carton_Status { Open = 0, Closed = 1, Cancelled = -1 }

        public enum e_PLC_Status
        {
            PASS,
            FAIL,
            ERROR,
            TIMEOUT,
            READFAIL,
            FORMATERROR
        }

        public enum e_Code_Status { Unused = 0, Used = 1 }

        public enum e_AWS_Send_Status { Pending, Sent, Failed }

        public enum e_AWS_Recive_Status
        {
            Waiting = 0,
            Pending = 1,
            Sent = 200,
            Error = 2,
            Error_404 = 404,
            Error_500 = 500,
            Error_400 = 400,
            Error_401 = 401,
            Error_403 = 403,
            Error_408 = 408,
            Error_409 = 409
        }
        #endregion

        // ================== PO LOADER (CRUD PO_List) ==================
        #region POLoader
        public static class POLoader
        {
            // Đảm bảo PO_List tồn tại
            public static void EnsurePOList() => Config.EnsurePOList();

            // Lấy tất cả PO
            public static TResult GetAll()
            {
                try
                {
                    EnsurePOList();
                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        const string sql = "SELECT * FROM PO ORDER BY orderNo";
                        var da = new SQLiteDataAdapter(sql, con);
                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, "Lấy danh sách PO thành công.", table.Rows.Count, table)
                            : new TResult(false, "Không có PO nào.");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi khi lấy danh sách PO: {ex.Message}");
                }
            }

            // Lấy PO theo orderNo
            public static TResult GetByOrderNo(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return new TResult(false, "orderNo không được trống.");
                    EnsurePOList();
                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        const string sql = "SELECT * FROM PO WHERE orderNo = @orderNo LIMIT 1";
                        var da = new SQLiteDataAdapter(sql, con);
                        da.SelectCommand.Parameters.AddWithValue("@orderNo", orderNo);
                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, "Lấy thông tin PO thành công.", 1, table)
                            : new TResult(false, $"Không tìm thấy PO: {orderNo}");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi khi lấy PO: {ex.Message}");
                }
            }

            // Lấy PO theo gtin
            public static TResult GetByGTIN(string gtin)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(gtin))
                        return new TResult(false, "gtin không được trống.");
                    EnsurePOList();
                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        const string sql = "SELECT * FROM PO WHERE gtin = @gtin ORDER BY orderNo";
                        var da = new SQLiteDataAdapter(sql, con);
                        da.SelectCommand.Parameters.AddWithValue("@gtin", gtin);
                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, $"Tìm thấy {table.Rows.Count} PO với gtin: {gtin}", table.Rows.Count, table)
                            : new TResult(false, $"Không tìm thấy PO với gtin: {gtin}");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi khi lấy PO: {ex.Message}");
                }
            }

            // Tạo PO mới
            public static (bool success, string message) Create(POInfo po)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(po.orderNo))
                        return (false, "orderNo không được trống.");
                    if (po.orderQty <= 24)
                        return (false, "orderQty phải > 24.");

                    EnsurePOList();

                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        const string sql = @"
                            INSERT INTO PO (orderNo, site, factory, productionLine, productionDate,
                                           shift, orderQty, lotNumber, productCode, productName,
                                           gtin, customerOrderNo, uom, CreatedTime, ModifiedTime)
                            VALUES (@orderNo, @site, @factory, @productionLine, @productionDate,
                                    @shift, @orderQty, @lotNumber, @productCode, @productName,
                                    @gtin, @customerOrderNo, @uom, @CreatedTime, @ModifiedTime);";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@orderNo", po.orderNo);
                            cmd.Parameters.AddWithValue("@site", po.site ?? "");
                            cmd.Parameters.AddWithValue("@factory", po.factory ?? "");
                            cmd.Parameters.AddWithValue("@productionLine", po.productionLine ?? "");
                            cmd.Parameters.AddWithValue("@productionDate", po.productionDate ?? "");
                            cmd.Parameters.AddWithValue("@shift", po.shift ?? "");
                            cmd.Parameters.AddWithValue("@orderQty", po.orderQty);
                            cmd.Parameters.AddWithValue("@lotNumber", po.lotNumber ?? "");
                            cmd.Parameters.AddWithValue("@productCode", po.productCode ?? "");
                            cmd.Parameters.AddWithValue("@productName", po.productName ?? "");
                            cmd.Parameters.AddWithValue("@gtin", po.gtin ?? "");
                            cmd.Parameters.AddWithValue("@customerOrderNo", po.customerOrderNo ?? "");
                            cmd.Parameters.AddWithValue("@uom", po.uom ?? "");
                            cmd.Parameters.AddWithValue("@CreatedTime", now);
                            cmd.Parameters.AddWithValue("@ModifiedTime", now);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return (true, $"Tạo PO '{po.orderNo}' thành công.");
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint_PrimaryKey)
                {
                    return (false, $"PO '{po.orderNo}' đã tồn tại.");
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi tạo PO: {ex.Message}");
                }
            }

            // Cập nhật PO
            public static (bool success, string message) Update(string orderNo, POInfo po)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.");

                    EnsurePOList();
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE PO SET
                                site            = @site,
                                factory         = @factory,
                                productionLine  = @productionLine,
                                productionDate  = @productionDate,
                                shift           = @shift,
                                orderQty        = @orderQty,
                                lotNumber       = @lotNumber,
                                productCode     = @productCode,
                                productName     = @productName,
                                gtin            = @gtin,
                                customerOrderNo = @customerOrderNo,
                                uom             = @uom,
                                ModifiedTime    = @ModifiedTime
                            WHERE orderNo = @orderNo;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@orderNo", orderNo);
                            cmd.Parameters.AddWithValue("@site", po.site ?? "");
                            cmd.Parameters.AddWithValue("@factory", po.factory ?? "");
                            cmd.Parameters.AddWithValue("@productionLine", po.productionLine ?? "");
                            cmd.Parameters.AddWithValue("@productionDate", po.productionDate ?? "");
                            cmd.Parameters.AddWithValue("@shift", po.shift ?? "");
                            cmd.Parameters.AddWithValue("@orderQty", po.orderQty);
                            cmd.Parameters.AddWithValue("@lotNumber", po.lotNumber ?? "");
                            cmd.Parameters.AddWithValue("@productCode", po.productCode ?? "");
                            cmd.Parameters.AddWithValue("@productName", po.productName ?? "");
                            cmd.Parameters.AddWithValue("@gtin", po.gtin ?? "");
                            cmd.Parameters.AddWithValue("@customerOrderNo", po.customerOrderNo ?? "");
                            cmd.Parameters.AddWithValue("@uom", po.uom ?? "");
                            cmd.Parameters.AddWithValue("@ModifiedTime", now);
                            int rows = cmd.ExecuteNonQuery();
                            return rows > 0
                                ? (true, $"Cập nhật PO '{orderNo}' thành công.")
                                : (false, $"Không tìm thấy PO: {orderNo}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi cập nhật PO: {ex.Message}");
                }
            }

            // Xóa PO + toàn bộ file DB của PO
            public static (bool success, string message) Delete(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.");

                    EnsurePOList();

                    // Xóa khỏi PO_List
                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "DELETE FROM PO WHERE orderNo = @orderNo;", con))
                        {
                            cmd.Parameters.AddWithValue("@orderNo", orderNo);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                return (false, $"Không tìm thấy PO: {orderNo}");
                        }
                    }

                    // Xóa các file DB của PO
                    DeletePOFiles(orderNo);
                    return (true, $"Xóa PO '{orderNo}' thành công.");
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi xóa PO: {ex.Message}");
                }
            }

            private static void DeletePOFiles(string orderNo)
            {
                string[] paths = {
                    Config.GetPODBPath(orderNo),
                    Config.GetRecordActivePath(orderNo),
                    Config.GetRecordPackingPath(orderNo),
                    Config.GetCartonPath(orderNo)
                };
                foreach (var p in paths)
                    if (File.Exists(p))
                        File.Delete(p);
            }

            // Kiểm tra PO tồn tại
            public static bool Exists(string orderNo)
            {
                try
                {
                    EnsurePOList();
                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "SELECT COUNT(1) FROM PO WHERE orderNo = @orderNo;", con))
                        {
                            cmd.Parameters.AddWithValue("@orderNo", orderNo);
                            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                        }
                    }
                }
                catch { return false; }
            }

            // ================== Query UniqueCodes trong PO ==================

            public static TResult GetCodes(string orderNo, int? status = null, string cartonCode = null)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return new TResult(false, $"Database PO '{orderNo}' không tồn tại.");

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        string sql = "SELECT * FROM UniqueCodes WHERE 1=1";
                        if (status.HasValue) sql += " AND Status = @Status";
                        if (!string.IsNullOrWhiteSpace(cartonCode)) sql += " AND cartonCode = @cartonCode";
                        sql += " ORDER BY ID";

                        var da = new SQLiteDataAdapter(sql, con);
                        if (status.HasValue) da.SelectCommand.Parameters.AddWithValue("@Status", status.Value);
                        if (!string.IsNullOrWhiteSpace(cartonCode)) da.SelectCommand.Parameters.AddWithValue("@cartonCode", cartonCode);

                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, $"Lấy {table.Rows.Count} mã thành công.", table.Rows.Count, table)
                            : new TResult(false, "Không tìm thấy mã nào.");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi truy vấn codes: {ex.Message}");
                }
            }

            public static TResult GetCodeByCode(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return new TResult(false, $"Database PO '{orderNo}' không tồn tại.");

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = "SELECT * FROM UniqueCodes WHERE Code = @Code LIMIT 1";
                        var da = new SQLiteDataAdapter(sql, con);
                        da.SelectCommand.Parameters.AddWithValue("@Code", code);
                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, "Lấy thông tin mã thành công.", 1, table)
                            : new TResult(false, $"Không tìm thấy mã: {code}");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi truy vấn: {ex.Message}");
                }
            }

            public static bool CodeExists(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return false;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "SELECT COUNT(1) FROM UniqueCodes WHERE Code = @Code;", con))
                        {
                            cmd.Parameters.AddWithValue("@Code", code);
                            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                        }
                    }
                }
                catch { return false; }
            }

            public static int GetCodeCount(string orderNo, int? status = null)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        string sql = "SELECT COUNT(*) FROM UniqueCodes";
                        if (status.HasValue) sql += " WHERE Status = @Status";

                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            if (status.HasValue) cmd.Parameters.AddWithValue("@Status", status.Value);
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                catch { return 0; }
            }

            // ================== Load codes từ DataPool ==================
            // Lấy mã Status=0 từ <gtin>.vnqrdb (DataPool), insert vào <orderNo>.db (PO).
            // Đồng thời update DataPool: Status=1 để đánh dấu đã lấy.
            public static (bool success, string message, int loadedCount) LoadCodesFromDataPool(
                string orderNo,
                string gtin,
                int? limitQty = null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.", 0);
                    if (string.IsNullOrWhiteSpace(gtin))
                        return (false, "gtin không được trống.", 0);

                    // Validate PO tồn tại
                    var poInfo = GetByOrderNo(orderNo);
                    if (!poInfo.issuccess || poInfo.data == null || poInfo.data.Rows.Count == 0)
                        return (false, $"PO '{orderNo}' không tồn tại.", 0);

                    int orderQty = Convert.ToInt32(poInfo.data.Rows[0]["orderQty"]);
                    if (orderQty <= 24)
                        return (false, "orderQty phải > 24.", 0);

                    string dbPoolPath = Config.GetDataPoolPath(gtin);
                    string dbPOPath = Config.GetPODBPath(orderNo);

                    // Đảm bảo PO db tồn tại
                    if (!File.Exists(dbPOPath))
                        POCreator.InitPO(orderNo);

                    // Kiểm tra DataPool có tồn tại không
                    if (!File.Exists(dbPoolPath))
                        return (false, $"DataPool '{gtin}.vnqrdb' không tồn tại. Hãy nhập mã vào DataPool trước.", 0);

                    int loadedCount = 0;

                    // Lấy mã Status=0 từ DataPool
                    List<string> availableCodes = new List<string>();
                    using (var conPool = new SQLiteConnection($"Data Source={dbPoolPath}"))
                    {
                        conPool.Open();
                        string sql = "SELECT Code FROM Codes WHERE Status = 0";
                        if (limitQty.HasValue) sql += $" LIMIT {limitQty.Value}";

                        using (var cmd = new SQLiteCommand(sql, conPool))
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                availableCodes.Add(rd.GetString(0));
                            }
                        }
                    }

                    // Kiểm tra đủ mã không
                    if (availableCodes.Count < orderQty)
                    {
                        return (false,
                            $"Không đủ mã! Cần: {orderQty} | Còn trong DataPool: {availableCodes.Count}",
                            0);
                    }

                    // Lấy những mã đã có trong PO (tránh insert trùng)
                    HashSet<string> existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var conPO = new SQLiteConnection($"Data Source={dbPOPath}"))
                    {
                        conPO.Open();
                        using (var cmd = new SQLiteCommand("SELECT Code FROM UniqueCodes;", conPO))
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read()) existingCodes.Add(rd.GetString(0));
                        }
                    }

                    // Insert vào PO + update DataPool
                    using (var conPool = new SQLiteConnection($"Data Source={dbPoolPath}"))
                    using (var conPO = new SQLiteConnection($"Data Source={dbPOPath}"))
                    {
                        conPool.Open();
                        conPO.Open();

                        using (var txPO = conPO.BeginTransaction())
                        {
                            const string insertPO = @"
                                INSERT OR IGNORE INTO UniqueCodes (Code, Status, ProductionDate)
                                VALUES (@Code, 0, @ProductionDate);";
                            const string updatePool = @"
                                UPDATE Codes SET Status = 1 WHERE Code = @Code;";

                            for (int i = 0; i < orderQty && i < availableCodes.Count; i++)
                            {
                                string code = availableCodes[i];
                                if (existingCodes.Contains(code)) continue;

                                // Insert vào PO
                                using (var cmdInsert = new SQLiteCommand(insertPO, conPO, txPO))
                                {
                                    cmdInsert.Parameters.AddWithValue("@Code", code);
                                    cmdInsert.Parameters.AddWithValue("@ProductionDate",
                                        poInfo.data.Rows[0]["productionDate"]?.ToString() ?? "");
                                    cmdInsert.ExecuteNonQuery();
                                    loadedCount++;
                                }

                                // Update DataPool: đánh dấu đã lấy
                                using (var cmdUpdate = new SQLiteCommand(updatePool, conPool))
                                {
                                    cmdUpdate.Parameters.AddWithValue("@Code", code);
                                    cmdUpdate.ExecuteNonQuery();
                                }
                            }
                            txPO.Commit();
                        }
                    }

                    return (true, $"Nạp {loadedCount} mã từ DataPool '{gtin}' vào PO '{orderNo}' thành công.", loadedCount);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi nạp mã từ DataPool: {ex.Message}", 0);
                }
            }
        }
        #endregion

        // ================== PO CREATOR ==================
        #region POCreator
        public static class POCreator
        {
            // Tạo 4 file .db cho PO nếu chưa tồn tại
            public static (bool success, string message) InitPO(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.");

                    // Đảm bảo base path tồn tại
                    string basePath = Config.GetBasePathByOrderNo(orderNo);
                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    (string dbPath, string schema)[] schemas = {
                        (Config.GetPODBPath(orderNo), Config.SQL_CREATE_PO_CODES),
                        (Config.GetRecordActivePath(orderNo), Config.SQL_CREATE_RECORD_ACTIVE),
                        (Config.GetRecordPackingPath(orderNo), Config.SQL_CREATE_RECORD_PACKING),
                        (Config.GetCartonPath(orderNo), Config.SQL_CREATE_CARTON)
                    };

                    foreach (var item in schemas)
                    {
                        string dbPath = item.dbPath;
                        string schema = item.schema;
                        if (!File.Exists(dbPath))
                        {
                            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                            {
                                con.Open();
                                using (var cmd = new SQLiteCommand(schema, con))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    return (true, $"Khởi tạo PO '{orderNo}' thành công.");
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi khởi tạo PO: {ex.Message}");
                }
            }

            // Tạo thêm N thùng trống cho PO
            public static (bool success, string message, int createdCount) CreateEmptyCartons(
                string orderNo, int count)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return (false, $"Carton DB của PO '{orderNo}' chưa tồn tại.", 0);

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        using (var tx = con.BeginTransaction())
                        {
                            const string sql = @"
                                INSERT INTO Carton (cartonCode, Start_Datetime, Completed_Datetime, ActivateUser, ProductionDate)
                                VALUES ('0', '0', '0', 'System', '0');";
                            for (int i = 0; i < count; i++)
                            {
                                using (var cmd = new SQLiteCommand(sql, con, tx))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            tx.Commit();
                        }
                    }
                    return (true, $"Tạo {count} thùng trống thành công.", count);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi tạo thùng: {ex.Message}", 0);
                }
            }
        }
        #endregion

        // ================== PO ACTIVATOR (Camera Active) ==================
        #region POActivator
        public static class POActivator
        {
            // Ghi 1 record vào Record_Active_<orderNo>.db
            public static void Record(string orderNo, PORecordData data)
            {
                try
                {
                    string dbPath = Config.GetRecordActivePath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            INSERT INTO Records (Code, cartonCode, Status, PLC_Status,
                                                ActivateDate, ActivateUser, ProductionDate)
                            VALUES (@Code, @cartonCode, @Status, @PLC_Status,
                                    @ActivateDate, @ActivateUser, @ProductionDate);";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@Code", data.code ?? "FAIL");
                            cmd.Parameters.AddWithValue("@cartonCode", data.cartonCode ?? "0");
                            cmd.Parameters.AddWithValue("@Status", data.status ?? "0");
                            cmd.Parameters.AddWithValue("@PLC_Status", data.plcStatus ?? "FAIL");
                            cmd.Parameters.AddWithValue("@ActivateDate", data.activateDate ?? "0");
                            cmd.Parameters.AddWithValue("@ActivateUser", data.activateUser ?? "");
                            cmd.Parameters.AddWithValue("@ProductionDate", data.productionDate ?? "0");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POActivator.Record] Lỗi: {ex.Message}");
                }
            }

            // Đánh dấu code đã active trong UniqueCodes
            public static void ActivateCode(string orderNo, string code,
                string activateDate, string activateUser, string productionDate)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE UniqueCodes
                            SET Status = 1,
                                ActivateDate   = COALESCE(NULLIF(@ActivateDate, ''), ActivateDate),
                                ActivateUser   = COALESCE(NULLIF(@ActivateUser, ''), ActivateUser),
                                ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                            WHERE Code = @Code;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@Code", code);
                            cmd.Parameters.AddWithValue("@ActivateDate", activateDate ?? "");
                            cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                            cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POActivator.ActivateCode] Lỗi: {ex.Message}");
                }
            }

            // Reset code về chưa active (Status=0)
            public static void DeactivateCode(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE UniqueCodes
                            SET Status = 0,
                                ActivateDate   = '0',
                                ActivateUser   = '',
                                cartonCode     = '0'
                            WHERE Code = @Code;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@Code", code);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POActivator.DeactivateCode] Lỗi: {ex.Message}");
                }
            }

            // Đếm số mã đã active
            public static int GetActiveCount(string orderNo)
            {
                return POLoader.GetCodeCount(orderNo, status: 1);
            }
        }
        #endregion

        // ================== PO PACKING (Camera Packing) ==================
        #region POPacking
        public static class POPacking
        {
            // Ghi 1 record vào Record_Packing_<orderNo>.db
            public static void Record(string orderNo, PORecordData data)
            {
                try
                {
                    string dbPath = Config.GetRecordPackingPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            INSERT INTO Records (Code, cartonCode, Status, PLC_Status,
                                                PackingDate, PackingUser, ProductionDate)
                            VALUES (@Code, @cartonCode, @Status, @PLC_Status,
                                    @PackingDate, @PackingUser, @ProductionDate);";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@Code", data.code ?? "FAIL");
                            cmd.Parameters.AddWithValue("@cartonCode", data.cartonCode ?? "0");
                            cmd.Parameters.AddWithValue("@Status", data.status ?? "0");
                            cmd.Parameters.AddWithValue("@PLC_Status", data.plcStatus ?? "FAIL");
                            cmd.Parameters.AddWithValue("@PackingDate", data.packingDate ?? "0");
                            cmd.Parameters.AddWithValue("@PackingUser", data.packingUser ?? "");
                            cmd.Parameters.AddWithValue("@ProductionDate", data.productionDate ?? "0");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POPacking.Record] Lỗi: {ex.Message}");
                }
            }

            // Đánh dấu code đã đóng gói vào thùng
            public static void PackCode(string orderNo, string code, string cartonCode,
                string packingDate, string packingUser, string productionDate)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE UniqueCodes
                            SET cartonCode   = @cartonCode,
                                PackingDate  = COALESCE(NULLIF(@PackingDate, ''), PackingDate),
                                ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                            WHERE Code = @Code;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@Code", code);
                            cmd.Parameters.AddWithValue("@cartonCode", cartonCode ?? "0");
                            cmd.Parameters.AddWithValue("@PackingDate", packingDate ?? "");
                            cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POPacking.PackCode] Lỗi: {ex.Message}");
                }
            }

            // Gỡ code khỏi thùng (xóa cartonCode)
            public static void UnpackCode(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE UniqueCodes
                            SET cartonCode = '0',
                                PackingDate = '0'
                            WHERE Code = @Code;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@Code", code);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POPacking.UnpackCode] Lỗi: {ex.Message}");
                }
            }

            // Đếm số mã đã đóng gói (có cartonCode != '0')
            public static int GetPackedCount(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "SELECT COUNT(*) FROM UniqueCodes WHERE cartonCode <> '0';", con))
                        {
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                catch { return 0; }
            }
        }
        #endregion

        // ================== PO CARTON ==================
        #region POCarton
        public static class POCarton
        {
            public static void Create(string orderNo, POCartonData data)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            INSERT INTO Carton (cartonCode, Start_Datetime, Completed_Datetime,
                                                ActivateUser, ProductionDate)
                            VALUES (@cartonCode, @Start_Datetime, @Completed_Datetime,
                                    @ActivateUser, @ProductionDate);";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@cartonCode", data.cartonCode ?? "0");
                            cmd.Parameters.AddWithValue("@Start_Datetime", data.startDatetime ?? "0");
                            cmd.Parameters.AddWithValue("@Completed_Datetime", data.completedDatetime ?? "0");
                            cmd.Parameters.AddWithValue("@ActivateUser", data.activateUser ?? "");
                            cmd.Parameters.AddWithValue("@ProductionDate", data.productionDate ?? "0");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POCarton.Create] Lỗi: {ex.Message}");
                }
            }

            public static void Update(string orderNo, POCartonData data)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE Carton
                            SET cartonCode         = @cartonCode,
                                Start_Datetime     = @Start_Datetime,
                                Completed_Datetime = @Completed_Datetime,
                                ActivateUser       = @ActivateUser,
                                ProductionDate     = @ProductionDate
                            WHERE ID = @ID;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", data.id);
                            cmd.Parameters.AddWithValue("@cartonCode", data.cartonCode ?? "0");
                            cmd.Parameters.AddWithValue("@Start_Datetime", data.startDatetime ?? "0");
                            cmd.Parameters.AddWithValue("@Completed_Datetime", data.completedDatetime ?? "0");
                            cmd.Parameters.AddWithValue("@ActivateUser", data.activateUser ?? "");
                            cmd.Parameters.AddWithValue("@ProductionDate", data.productionDate ?? "0");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POCarton.Update] Lỗi: {ex.Message}");
                }
            }

            public static void Delete(string orderNo, int cartonId)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "DELETE FROM Carton WHERE ID = @ID;", con))
                        {
                            cmd.Parameters.AddWithValue("@ID", cartonId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POCarton.Delete] Lỗi: {ex.Message}");
                }
            }

            public static TResult GetAll(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return new TResult(false, $"Carton DB của PO '{orderNo}' không tồn tại.");

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = "SELECT * FROM Carton ORDER BY ID";
                        var da = new SQLiteDataAdapter(sql, con);
                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, $"Lấy {table.Rows.Count} thùng thành công.", table.Rows.Count, table)
                            : new TResult(false, "Không có thùng nào.");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi khi lấy carton: {ex.Message}");
                }
            }

            public static TResult GetByCartonCode(string orderNo, string cartonCode)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return new TResult(false, $"Carton DB không tồn tại.");

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = "SELECT * FROM Carton WHERE cartonCode = @cartonCode LIMIT 1";
                        var da = new SQLiteDataAdapter(sql, con);
                        da.SelectCommand.Parameters.AddWithValue("@cartonCode", cartonCode);
                        var table = new DataTable();
                        da.Fill(table);
                        return table.Rows.Count > 0
                            ? new TResult(true, "Lấy thùng thành công.", 1, table)
                            : new TResult(false, $"Không tìm thấy thùng: {cartonCode}");
                    }
                }
                catch (Exception ex)
                {
                    return new TResult(false, $"Lỗi: {ex.Message}");
                }
            }

            public static void StartCarton(string orderNo, int cartonId, string activateUser)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE Carton
                            SET Start_Datetime = @Start_Datetime,
                                ActivateUser   = @ActivateUser
                            WHERE ID = @ID;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", cartonId);
                            cmd.Parameters.AddWithValue("@Start_Datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POCarton.StartCarton] Lỗi: {ex.Message}");
                }
            }

            public static void CompleteCarton(string orderNo, int cartonId, string activateUser)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE Carton
                            SET Completed_Datetime = @Completed_Datetime,
                                ActivateUser        = @ActivateUser
                            WHERE ID = @ID;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", cartonId);
                            cmd.Parameters.AddWithValue("@Completed_Datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POCarton.CompleteCarton] Lỗi: {ex.Message}");
                }
            }

            public static void ResetCarton(string orderNo, int cartonId)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return;

                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE Carton
                            SET cartonCode         = '0',
                                Start_Datetime     = '0',
                                Completed_Datetime = '0'
                            WHERE ID = @ID;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@ID", cartonId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POCarton.ResetCarton] Lỗi: {ex.Message}");
                }
            }

            public static int GetTotalCartonCount(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;
                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "SELECT COUNT(*) FROM Carton;", con))
                        {
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                catch { return 0; }
            }

            public static int GetClosedCartonCount(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;
                    using (var con = new SQLiteConnection($"Data Source={dbPath}"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "SELECT COUNT(*) FROM Carton WHERE Completed_Datetime <> '0';", con))
                        {
                            return Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                catch { return 0; }
            }
        }
        #endregion

        // ================== PO UPDATER ==================
        #region POUpdater
        public static class POUpdater
        {
            public static bool UpdateProductionDate(string orderNo, string newDate)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return false;

                    using (var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}"))
                    {
                        con.Open();
                        const string sql = @"
                            UPDATE PO
                            SET productionDate = @newDate,
                                ModifiedTime   = @ModifiedTime
                            WHERE orderNo = @orderNo;";
                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.Parameters.AddWithValue("@orderNo", orderNo);
                            cmd.Parameters.AddWithValue("@newDate", newDate ?? "");
                            cmd.Parameters.AddWithValue("@ModifiedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            return cmd.ExecuteNonQuery() > 0;
                        }
                    }
                }
                catch { return false; }
            }
        }
        #endregion
    }
}
