using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using VNQR.Infrastructure;

namespace VNQR.Helpers
{
    /// <summary>
    /// po - Production Order Helper
    /// 
    /// CƠ CHẾ:
    /// - Codes được nạp từ DataPool (&lt;gtin&gt;.vnqrdb) vào &lt;orderNo&gt;.db khi khởi tạo PO.
    /// - Codes theo GTIN (nhiều PO cùng GTIN dùng chung mã nguồn)
    /// - Mỗi PO có database riêng để tracking (records, cartons...)
    /// - Cấu trúc thư mục: &lt;baseDataPath&gt;/yyyy-MM/gtin/
    ///   + Ví dụ: C:/VNQR/PODatabases/2025-01/8931234567890/PO001.db
    /// 
    /// SỬ DỤNG:
    /// - po.Config.BasePath = "C:/VNQR/PODatabases";
    /// - po.POLoader.GetAll();
    /// - po.POCreator.InitPO("PO001");
    /// - po.POActivator.Record("PO001", data);
    /// - po.POPacking.Record("PO001", data);
    /// 
    /// HOẶC dùng singleton-like qua GV:
    /// - GV.Production.POLoader.GetAll();
    /// - GV.Production.POCreator.InitPO("PO001");
    /// </summary>
    public class po
    {
        // ================== CONFIG ==================
        public static class Config
        {
            public static string BasePath = "C:/VNQR/PODatabases";
            public static string POListFileName = "PO_List.db";

            public static string GetPOListPath()
            {
                if (!Directory.Exists(BasePath))
                    Directory.CreateDirectory(BasePath);
                return Path.Combine(BasePath, POListFileName);
            }

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

            public static string GetBasePathByOrderNo(string orderNo)
            {
                var r = POLoader.GetByOrderNo(orderNo);
                if (!r.IsSuccess || r.Data == null || r.Data.Rows.Count == 0)
                    return BasePath;

                var row = r.Data.Rows[0];
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

            public static void EnsurePOList() => EnsureDB(GetPOListPath(), SQL_CREATE_PO_LIST);

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
        public class Result
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = "";
            public DataTable? Data { get; set; }
            public int Count { get; set; }

            // Backward compatibility aliases
            public bool issuccess => IsSuccess;
            public DataTable? data => Data;

            public Result() { }
            public Result(bool isSuccess, string message, int count = 0, DataTable? data = null)
            {
                IsSuccess = isSuccess;
                Message = message;
                Data = data;
                Count = count;
            }

            public static Result Success(string message = "Thành công.", int count = 0, DataTable? data = null)
                => new(true, message, count, data);

            public static Result Fail(string message)
                => new(false, message);

            public static Result FromDataTable(DataTable? table, string successMsg = "Thành công.", string failMsg = "Không có dữ liệu.")
                => table != null && table.Rows.Count > 0
                    ? Success(successMsg, table.Rows.Count, table)
                    : Fail(failMsg);
        }

        public class POInfo
        {
            public string OrderNo { get; set; } = "";
            public string Site { get; set; } = "";
            public string Factory { get; set; } = "";
            public string ProductionLine { get; set; } = "";
            public string ProductionDate { get; set; } = "";
            public string Shift { get; set; } = "";
            public int OrderQty { get; set; } = 0;
            public string LotNumber { get; set; } = "";
            public string ProductCode { get; set; } = "";
            public string ProductName { get; set; } = "";
            public string Gtin { get; set; } = "";
            public string CustomerOrderNo { get; set; } = "";
            public string Uom { get; set; } = "";
            public string CreatedTime { get; set; } = "";
            public string ModifiedTime { get; set; } = "";

            public static POInfo FromDataRow(DataRow row)
            {
                return new POInfo
                {
                    OrderNo = row["orderNo"]?.ToString() ?? "",
                    Site = row["site"]?.ToString() ?? "",
                    Factory = row["factory"]?.ToString() ?? "",
                    ProductionLine = row["productionLine"]?.ToString() ?? "",
                    ProductionDate = row["productionDate"]?.ToString() ?? "",
                    Shift = row["shift"]?.ToString() ?? "",
                    OrderQty = Convert.ToInt32(row["orderQty"] ?? 0),
                    LotNumber = row["lotNumber"]?.ToString() ?? "",
                    ProductCode = row["productCode"]?.ToString() ?? "",
                    ProductName = row["productName"]?.ToString() ?? "",
                    Gtin = row["gtin"]?.ToString() ?? "",
                    CustomerOrderNo = row["customerOrderNo"]?.ToString() ?? "",
                    Uom = row["uom"]?.ToString() ?? "",
                    CreatedTime = row["CreatedTime"]?.ToString() ?? "",
                    ModifiedTime = row["ModifiedTime"]?.ToString() ?? "",
                };
            }

            public DataRow ToDataRow(DataTable table)
            {
                var row = table.NewRow();
                row["orderNo"] = OrderNo;
                row["site"] = Site;
                row["factory"] = Factory;
                row["productionLine"] = ProductionLine;
                row["productionDate"] = ProductionDate;
                row["shift"] = Shift;
                row["orderQty"] = OrderQty;
                row["lotNumber"] = LotNumber;
                row["productCode"] = ProductCode;
                row["productName"] = ProductName;
                row["gtin"] = Gtin;
                row["customerOrderNo"] = CustomerOrderNo;
                row["uom"] = Uom;
                row["CreatedTime"] = CreatedTime;
                row["ModifiedTime"] = ModifiedTime;
                return row;
            }
        }

        public class CodeData
        {
            public int Id { get; set; }
            public string Code { get; set; } = "";
            public string CartonCode { get; set; } = "0";
            public int Status { get; set; } = 0;
            public string ActivateDate { get; set; } = "0";
            public string ProductionDate { get; set; } = "0";
            public string ActivateUser { get; set; } = "";
            public string PackingDate { get; set; } = "0";
            public string SendStatus { get; set; } = "Pending";
            public string ReceiveStatus { get; set; } = "Pending";

            public static CodeData FromDataRow(DataRow row)
            {
                return new CodeData
                {
                    Id = Convert.ToInt32(row["Id"] ?? 0),
                    Code = row["Code"]?.ToString() ?? "",
                    CartonCode = row["cartonCode"]?.ToString() ?? "0",
                    Status = Convert.ToInt32(row["Status"] ?? 0),
                    ActivateDate = row["ActivateDate"]?.ToString() ?? "0",
                    ProductionDate = row["ProductionDate"]?.ToString() ?? "0",
                    ActivateUser = row["ActivateUser"]?.ToString() ?? "",
                    PackingDate = row["PackingDate"]?.ToString() ?? "0",
                    SendStatus = row["Send_Status"]?.ToString() ?? "Pending",
                    ReceiveStatus = row["Recive_Status"]?.ToString() ?? "Pending",
                };
            }
        }

        public class RecordData
        {
            public string Code { get; set; } = "FAIL";
            public string CartonCode { get; set; } = "0";
            public string Status { get; set; } = "0";
            public string PLCStatus { get; set; } = "FAIL";
            public string ActivateDate { get; set; } = "0";
            public string ActivateUser { get; set; } = "";
            public string PackingDate { get; set; } = "0";
            public string PackingUser { get; set; } = "";
            public string ProductionDate { get; set; } = "0";
        }

        public class CartonData
        {
            public int Id { get; set; } = 0;
            public string CartonCode { get; set; } = "0";
            public string StartDatetime { get; set; } = "0";
            public string CompletedDatetime { get; set; } = "0";
            public string ActivateUser { get; set; } = "";
            public string ProductionDate { get; set; } = "0";

            public static CartonData FromDataRow(DataRow row)
            {
                return new CartonData
                {
                    Id = Convert.ToInt32(row["Id"] ?? 0),
                    CartonCode = row["cartonCode"]?.ToString() ?? "0",
                    StartDatetime = row["Start_Datetime"]?.ToString() ?? "0",
                    CompletedDatetime = row["Completed_Datetime"]?.ToString() ?? "0",
                    ActivateUser = row["ActivateUser"]?.ToString() ?? "",
                    ProductionDate = row["ProductionDate"]?.ToString() ?? "0",
                };
            }
        }

        public enum e_CartonStatus { Open = 0, Closed = 1, Cancelled = -1 }
        public enum e_PLCStatus { PASS, FAIL, ERROR, TIMEOUT, READFAIL, FORMATERROR }
        public enum e_CodeStatus { Unused = 0, Used = 1 }
        public enum e_AWSSendStatus { Pending, Sent, Failed }
        public enum e_AWSReceiveStatus
        {
            Waiting = 0, Pending = 1, Sent = 200, Error = 2,
            Error404 = 404, Error500 = 500, Error400 = 400,
            Error401 = 401, Error403 = 403, Error408 = 408, Error409 = 409
        }
        #endregion

        // ================== PO LOADER (CRUD PO_List) ==================
        #region POLoader
        public static class POLoader
        {
            public static void EnsurePOList() => Config.EnsurePOList();

            public static Result GetAll()
            {
                try
                {
                    EnsurePOList();
                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    const string sql = "SELECT * FROM PO ORDER BY orderNo";
                    using var da = new SQLiteDataAdapter(sql, con);
                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, "Lấy danh sách PO thành công.", "Không có PO nào.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi lấy danh sách PO: {ex.Message}");
                }
            }

            public static Result GetByOrderNo(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return Result.Fail("orderNo không được trống.");

                    EnsurePOList();
                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    const string sql = "SELECT * FROM PO WHERE orderNo = @orderNo LIMIT 1";
                    using var da = new SQLiteDataAdapter(sql, con);
                    da.SelectCommand.Parameters.AddWithValue("@orderNo", orderNo);
                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, "Lấy thông tin PO thành công.", $"Không tìm thấy PO: {orderNo}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi lấy PO: {ex.Message}");
                }
            }

            public static Result GetByGTIN(string gtin)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(gtin))
                        return Result.Fail("gtin không được trống.");

                    EnsurePOList();
                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    const string sql = "SELECT * FROM PO WHERE gtin = @gtin ORDER BY orderNo";
                    using var da = new SQLiteDataAdapter(sql, con);
                    da.SelectCommand.Parameters.AddWithValue("@gtin", gtin);
                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, $"Tìm thấy {table.Rows.Count} PO.", $"Không tìm thấy PO với gtin: {gtin}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi lấy PO: {ex.Message}");
                }
            }

            public static Result Create(POInfo po)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(po.OrderNo))
                        return Result.Fail("orderNo không được trống.");
                    if (po.OrderQty <= 24)
                        return Result.Fail("orderQty phải > 24.");

                    EnsurePOList();
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    const string sql = @"
                        INSERT INTO PO (orderNo, site, factory, productionLine, productionDate,
                                       shift, orderQty, lotNumber, productCode, productName,
                                       gtin, customerOrderNo, uom, CreatedTime, ModifiedTime)
                        VALUES (@orderNo, @site, @factory, @productionLine, @productionDate,
                                @shift, @orderQty, @lotNumber, @productCode, @productName,
                                @gtin, @customerOrderNo, @uom, @CreatedTime, @ModifiedTime);";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@orderNo", po.OrderNo);
                    cmd.Parameters.AddWithValue("@site", po.Site ?? "");
                    cmd.Parameters.AddWithValue("@factory", po.Factory ?? "");
                    cmd.Parameters.AddWithValue("@productionLine", po.ProductionLine ?? "");
                    cmd.Parameters.AddWithValue("@productionDate", po.ProductionDate ?? "");
                    cmd.Parameters.AddWithValue("@shift", po.Shift ?? "");
                    cmd.Parameters.AddWithValue("@orderQty", po.OrderQty);
                    cmd.Parameters.AddWithValue("@lotNumber", po.LotNumber ?? "");
                    cmd.Parameters.AddWithValue("@productCode", po.ProductCode ?? "");
                    cmd.Parameters.AddWithValue("@productName", po.ProductName ?? "");
                    cmd.Parameters.AddWithValue("@gtin", po.Gtin ?? "");
                    cmd.Parameters.AddWithValue("@customerOrderNo", po.CustomerOrderNo ?? "");
                    cmd.Parameters.AddWithValue("@uom", po.Uom ?? "");
                    cmd.Parameters.AddWithValue("@CreatedTime", now);
                    cmd.Parameters.AddWithValue("@ModifiedTime", now);
                    cmd.ExecuteNonQuery();
                    return Result.Success($"Tạo PO '{po.OrderNo}' thành công.");
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint_PrimaryKey)
                {
                    return Result.Fail($"PO '{po.OrderNo}' đã tồn tại.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi tạo PO: {ex.Message}");
                }
            }

            public static Result Update(string orderNo, POInfo po)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return Result.Fail("orderNo không được trống.");

                    EnsurePOList();
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    const string sql = @"
                        UPDATE PO SET
                            site = @site, factory = @factory, productionLine = @productionLine,
                            productionDate = @productionDate, shift = @shift, orderQty = @orderQty,
                            lotNumber = @lotNumber, productCode = @productCode, productName = @productName,
                            gtin = @gtin, customerOrderNo = @customerOrderNo, uom = @uom,
                            ModifiedTime = @ModifiedTime
                        WHERE orderNo = @orderNo;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@orderNo", orderNo);
                    cmd.Parameters.AddWithValue("@site", po.Site ?? "");
                    cmd.Parameters.AddWithValue("@factory", po.Factory ?? "");
                    cmd.Parameters.AddWithValue("@productionLine", po.ProductionLine ?? "");
                    cmd.Parameters.AddWithValue("@productionDate", po.ProductionDate ?? "");
                    cmd.Parameters.AddWithValue("@shift", po.Shift ?? "");
                    cmd.Parameters.AddWithValue("@orderQty", po.OrderQty);
                    cmd.Parameters.AddWithValue("@lotNumber", po.LotNumber ?? "");
                    cmd.Parameters.AddWithValue("@productCode", po.ProductCode ?? "");
                    cmd.Parameters.AddWithValue("@productName", po.ProductName ?? "");
                    cmd.Parameters.AddWithValue("@gtin", po.Gtin ?? "");
                    cmd.Parameters.AddWithValue("@customerOrderNo", po.CustomerOrderNo ?? "");
                    cmd.Parameters.AddWithValue("@uom", po.Uom ?? "");
                    cmd.Parameters.AddWithValue("@ModifiedTime", now);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Cập nhật PO '{orderNo}' thành công.")
                        : Result.Fail($"Không tìm thấy PO: {orderNo}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi cập nhật PO: {ex.Message}");
                }
            }

            public static Result Delete(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return Result.Fail("orderNo không được trống.");

                    EnsurePOList();
                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    using var cmd = new SQLiteCommand("DELETE FROM PO WHERE orderNo = @orderNo;", con);
                    cmd.Parameters.AddWithValue("@orderNo", orderNo);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0)
                        return Result.Fail($"Không tìm thấy PO: {orderNo}");

                    DeletePOFiles(orderNo);
                    return Result.Success($"Xóa PO '{orderNo}' thành công.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi xóa PO: {ex.Message}");
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

            public static bool Exists(string orderNo)
            {
                try
                {
                    EnsurePOList();
                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    using var cmd = new SQLiteCommand("SELECT COUNT(1) FROM PO WHERE orderNo = @orderNo;", con);
                    cmd.Parameters.AddWithValue("@orderNo", orderNo);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }

            // ================== Query UniqueCodes trong PO ==================
            public static Result GetCodes(string orderNo, int? status = null, string? cartonCode = null)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Database PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    string sql = "SELECT * FROM UniqueCodes WHERE 1=1";
                    if (status.HasValue) sql += " AND Status = @Status";
                    if (!string.IsNullOrWhiteSpace(cartonCode)) sql += " AND cartonCode = @cartonCode";
                    sql += " ORDER BY ID";

                    using var da = new SQLiteDataAdapter(sql, con);
                    if (status.HasValue) da.SelectCommand.Parameters.AddWithValue("@Status", status.Value);
                    if (!string.IsNullOrWhiteSpace(cartonCode)) da.SelectCommand.Parameters.AddWithValue("@cartonCode", cartonCode);

                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, $"Lấy {table.Rows.Count} mã thành công.", "Không tìm thấy mã nào.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi truy vấn codes: {ex.Message}");
                }
            }

            public static Result GetCodeByCode(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Database PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "SELECT * FROM UniqueCodes WHERE Code = @Code LIMIT 1";
                    using var da = new SQLiteDataAdapter(sql, con);
                    da.SelectCommand.Parameters.AddWithValue("@Code", code);
                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, "Lấy thông tin mã thành công.", $"Không tìm thấy mã: {code}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi truy vấn: {ex.Message}");
                }
            }

            public static bool CodeExists(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return false;
                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    using var cmd = new SQLiteCommand("SELECT COUNT(1) FROM UniqueCodes WHERE Code = @Code;", con);
                    cmd.Parameters.AddWithValue("@Code", code);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }

            public static int GetCodeCount(string orderNo, int? status = null)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;
                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    string sql = "SELECT COUNT(*) FROM UniqueCodes";
                    if (status.HasValue) sql += " WHERE Status = @Status";
                    using var cmd = new SQLiteCommand(sql, con);
                    if (status.HasValue) cmd.Parameters.AddWithValue("@Status", status.Value);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { return 0; }
            }

            public static (bool success, string message, int loadedCount) LoadCodesFromDataPool(
                string orderNo, string gtin, int? limitQty = null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.", 0);
                    if (string.IsNullOrWhiteSpace(gtin))
                        return (false, "gtin không được trống.", 0);

                    var poInfo = GetByOrderNo(orderNo);
                    if (!poInfo.IsSuccess || poInfo.Data == null || poInfo.Data.Rows.Count == 0)
                        return (false, $"PO '{orderNo}' không tồn tại.", 0);

                    int orderQty = Convert.ToInt32(poInfo.Data.Rows[0]["orderQty"]);
                    if (orderQty <= 24)
                        return (false, "orderQty phải > 24.", 0);

                    string dbPoolPath = Config.GetDataPoolPath(gtin);
                    string dbPOPath = Config.GetPODBPath(orderNo);

                    if (!File.Exists(dbPOPath))
                        POCreator.InitPO(orderNo);

                    if (!File.Exists(dbPoolPath))
                        return (false, $"DataPool '{gtin}.vnqrdb' không tồn tại. Hãy nhập mã vào DataPool trước.", 0);

                    int loadedCount = 0;

                    // Lấy mã Status=0 từ DataPool
                    List<string> availableCodes = new();
                    using (var conPool = new SQLiteConnection($"Data Source={dbPoolPath}"))
                    {
                        conPool.Open();
                        string sql = "SELECT Code FROM Codes WHERE Status = 0";
                        if (limitQty.HasValue) sql += $" LIMIT {limitQty.Value}";
                        using var cmd = new SQLiteCommand(sql, conPool);
                        using var rd = cmd.ExecuteReader();
                        while (rd.Read()) availableCodes.Add(rd.GetString(0));
                    }

                    if (availableCodes.Count < orderQty)
                        return (false, $"Không đủ mã! Cần: {orderQty} | Còn: {availableCodes.Count}", 0);

                    // Lấy mã đã có trong PO
                    HashSet<string> existingCodes = new(StringComparer.OrdinalIgnoreCase);
                    using (var conPO = new SQLiteConnection($"Data Source={dbPOPath}"))
                    {
                        conPO.Open();
                        using var cmd = new SQLiteCommand("SELECT Code FROM UniqueCodes;", conPO);
                        using var rd = cmd.ExecuteReader();
                        while (rd.Read()) existingCodes.Add(rd.GetString(0));
                    }

                    // Insert vào PO + update DataPool
                    using var conPool2 = new SQLiteConnection($"Data Source={dbPoolPath}");
                    using var conPO2 = new SQLiteConnection($"Data Source={dbPOPath}");
                    conPool2.Open();
                    conPO2.Open();
                    using var tx = conPO2.BeginTransaction();

                    const string insertPO = @"INSERT OR IGNORE INTO UniqueCodes (Code, Status, ProductionDate) VALUES (@Code, 0, @ProductionDate);";
                    const string updatePool = "UPDATE Codes SET Status = 1 WHERE Code = @Code;";

                    for (int i = 0; i < orderQty && i < availableCodes.Count; i++)
                    {
                        string code = availableCodes[i];
                        if (existingCodes.Contains(code)) continue;

                        using (var cmdInsert = new SQLiteCommand(insertPO, conPO2, tx))
                        {
                            cmdInsert.Parameters.AddWithValue("@Code", code);
                            cmdInsert.Parameters.AddWithValue("@ProductionDate", poInfo.Data.Rows[0]["productionDate"]?.ToString() ?? "");
                            cmdInsert.ExecuteNonQuery();
                            loadedCount++;
                        }
                        using (var cmdUpdate = new SQLiteCommand(updatePool, conPool2))
                        {
                            cmdUpdate.Parameters.AddWithValue("@Code", code);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return (true, $"Nạp {loadedCount} mã thành công.", loadedCount);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi nạp mã: {ex.Message}", 0);
                }
            }
        }
        #endregion

        // ================== PO CREATOR ==================
        #region POCreator
        public static class POCreator
        {
            public static Result InitPO(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return Result.Fail("orderNo không được trống.");

                    string basePath = Config.GetBasePathByOrderNo(orderNo);
                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    (string dbPath, string schema)[] schemas = {
                        (Config.GetPODBPath(orderNo), Config.SQL_CREATE_PO_CODES),
                        (Config.GetRecordActivePath(orderNo), Config.SQL_CREATE_RECORD_ACTIVE),
                        (Config.GetRecordPackingPath(orderNo), Config.SQL_CREATE_RECORD_PACKING),
                        (Config.GetCartonPath(orderNo), Config.SQL_CREATE_CARTON)
                    };

                    foreach (var (dbPath, schema) in schemas)
                    {
                        if (!File.Exists(dbPath))
                        {
                            using var con = new SQLiteConnection($"Data Source={dbPath}");
                            con.Open();
                            using var cmd = new SQLiteCommand(schema, con);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return Result.Success($"Khởi tạo PO '{orderNo}' thành công.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi khởi tạo PO: {ex.Message}");
                }
            }

            public static (bool success, string message, int createdCount) CreateEmptyCartons(string orderNo, int count)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return (false, $"Carton DB chưa tồn tại.", 0);

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    using var tx = con.BeginTransaction();
                    const string sql = "INSERT INTO Carton (cartonCode, Start_Datetime, Completed_Datetime, ActivateUser, ProductionDate) VALUES ('0', '0', '0', 'System', '0');";
                    for (int i = 0; i < count; i++)
                    {
                        using var cmd = new SQLiteCommand(sql, con, tx);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
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
            public static Result Record(string orderNo, RecordData data)
            {
                try
                {
                    string dbPath = Config.GetRecordActivePath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Record_Active '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"INSERT INTO Records (Code, cartonCode, Status, PLC_Status, ActivateDate, ActivateUser, ProductionDate)
                                         VALUES (@Code, @cartonCode, @Status, @PLC_Status, @ActivateDate, @ActivateUser, @ProductionDate);";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Code", data.Code ?? "FAIL");
                    cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                    cmd.Parameters.AddWithValue("@Status", data.Status ?? "0");
                    cmd.Parameters.AddWithValue("@PLC_Status", data.PLCStatus ?? "FAIL");
                    cmd.Parameters.AddWithValue("@ActivateDate", data.ActivateDate ?? "0");
                    cmd.Parameters.AddWithValue("@ActivateUser", data.ActivateUser ?? "");
                    cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                    cmd.ExecuteNonQuery();

                    ProductionInfo.IncrementActive(data.Code);
                    return Result.Success($"Ghi record Active '{data.Code}' thành công.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi ghi Record Active: {ex.Message}");
                }
            }

            public static Result ActivateCode(string orderNo, string code, string activateDate, string activateUser, string productionDate)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE UniqueCodes SET Status = 1, ActivateDate = COALESCE(NULLIF(@ActivateDate, ''), ActivateDate),
                                         ActivateUser = COALESCE(NULLIF(@ActivateUser, ''), ActivateUser),
                                         ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                                         WHERE Code = @Code;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@ActivateDate", activateDate ?? "");
                    cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                    cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Activate code '{code}' thành công.")
                        : Result.Fail($"Không tìm thấy mã: {code}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi activate code: {ex.Message}");
                }
            }

            public static Result DeactivateCode(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE UniqueCodes SET Status = 0, ActivateDate = '0', ActivateUser = '', cartonCode = '0' WHERE Code = @Code;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Code", code);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Deactivate code '{code}' thành công.")
                        : Result.Fail($"Không tìm thấy mã: {code}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi deactivate code: {ex.Message}");
                }
            }

            public static int GetActiveCount(string orderNo) => POLoader.GetCodeCount(orderNo, status: 1);
        }
        #endregion

        // ================== PO PACKING (Camera Packing) ==================
        #region POPacking
        public static class POPacking
        {
            public static Result Record(string orderNo, RecordData data)
            {
                try
                {
                    string dbPath = Config.GetRecordPackingPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Record_Packing '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"INSERT INTO Records (Code, cartonCode, Status, PLC_Status, PackingDate, PackingUser, ProductionDate)
                                         VALUES (@Code, @cartonCode, @Status, @PLC_Status, @PackingDate, @PackingUser, @ProductionDate);";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Code", data.Code ?? "FAIL");
                    cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                    cmd.Parameters.AddWithValue("@Status", data.Status ?? "0");
                    cmd.Parameters.AddWithValue("@PLC_Status", data.PLCStatus ?? "FAIL");
                    cmd.Parameters.AddWithValue("@PackingDate", data.PackingDate ?? "0");
                    cmd.Parameters.AddWithValue("@PackingUser", data.PackingUser ?? "");
                    cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                    cmd.ExecuteNonQuery();

                    ProductionInfo.IncrementPacked(data.Code);
                    return Result.Success($"Ghi record Packing '{data.Code}' thành công.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi ghi Record Packing: {ex.Message}");
                }
            }

            public static Result PackCode(string orderNo, string code, string cartonCode, string packingDate, string packingUser, string productionDate)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE UniqueCodes SET cartonCode = @cartonCode,
                                         PackingDate = COALESCE(NULLIF(@PackingDate, ''), PackingDate),
                                         ProductionDate = COALESCE(NULLIF(@ProductionDate, ''), ProductionDate)
                                         WHERE Code = @Code;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@cartonCode", cartonCode ?? "0");
                    cmd.Parameters.AddWithValue("@PackingDate", packingDate ?? "");
                    cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Pack code '{code}' vào thùng '{cartonCode}' thành công.")
                        : Result.Fail($"Không tìm thấy mã: {code}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi pack code: {ex.Message}");
                }
            }

            public static Result UnpackCode(string orderNo, string code)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"PO DB '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "UPDATE UniqueCodes SET cartonCode = '0', PackingDate = '0' WHERE Code = @Code;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@Code", code);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Unpack code '{code}' thành công.")
                        : Result.Fail($"Không tìm thấy mã: {code}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi unpack code: {ex.Message}");
                }
            }

            public static int GetPackedCount(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;
                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    using var cmd = new SQLiteCommand("SELECT COUNT(*) FROM UniqueCodes WHERE cartonCode <> '0';", con);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { return 0; }
            }
        }
        #endregion

        // ================== PO CARTON ==================
        #region POCarton
        public static class POCarton
        {
            public static Result Create(string orderNo, CartonData data)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"INSERT INTO Carton (cartonCode, Start_Datetime, Completed_Datetime, ActivateUser, ProductionDate)
                                         VALUES (@cartonCode, @Start_Datetime, @Completed_Datetime, @ActivateUser, @ProductionDate);";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                    cmd.Parameters.AddWithValue("@Start_Datetime", data.StartDatetime ?? "0");
                    cmd.Parameters.AddWithValue("@Completed_Datetime", data.CompletedDatetime ?? "0");
                    cmd.Parameters.AddWithValue("@ActivateUser", data.ActivateUser ?? "");
                    cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                    cmd.ExecuteNonQuery();
                    return Result.Success($"Tạo thùng '{data.CartonCode}' thành công.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi tạo carton: {ex.Message}");
                }
            }

            public static Result Update(string orderNo, CartonData data)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE Carton SET cartonCode = @cartonCode, Start_Datetime = @Start_Datetime,
                                         Completed_Datetime = @Completed_Datetime, ActivateUser = @ActivateUser, ProductionDate = @ProductionDate
                                         WHERE ID = @ID;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", data.Id);
                    cmd.Parameters.AddWithValue("@cartonCode", data.CartonCode ?? "0");
                    cmd.Parameters.AddWithValue("@Start_Datetime", data.StartDatetime ?? "0");
                    cmd.Parameters.AddWithValue("@Completed_Datetime", data.CompletedDatetime ?? "0");
                    cmd.Parameters.AddWithValue("@ActivateUser", data.ActivateUser ?? "");
                    cmd.Parameters.AddWithValue("@ProductionDate", data.ProductionDate ?? "0");
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Cập nhật carton ID '{data.Id}' thành công.")
                        : Result.Fail($"Không tìm thấy carton ID: {data.Id}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi cập nhật carton: {ex.Message}");
                }
            }

            public static Result Delete(string orderNo, int cartonId)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "DELETE FROM Carton WHERE ID = @ID;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", cartonId);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Xóa carton ID '{cartonId}' thành công.")
                        : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi xóa carton: {ex.Message}");
                }
            }

            public static Result GetAll(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "SELECT * FROM Carton ORDER BY ID";
                    using var da = new SQLiteDataAdapter(sql, con);
                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, $"Lấy {table.Rows.Count} thùng thành công.", "Không có thùng nào.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi lấy carton: {ex.Message}");
                }
            }

            public static Result GetByCartonCode(string orderNo, string cartonCode)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail("Carton DB không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "SELECT * FROM Carton WHERE cartonCode = @cartonCode LIMIT 1";
                    using var da = new SQLiteDataAdapter(sql, con);
                    da.SelectCommand.Parameters.AddWithValue("@cartonCode", cartonCode);
                    var table = new DataTable();
                    da.Fill(table);
                    return Result.FromDataTable(table, "Lấy thùng thành công.", $"Không tìm thấy thùng: {cartonCode}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi: {ex.Message}");
                }
            }

            public static Result StartCarton(string orderNo, int cartonId, string activateUser)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE Carton SET Start_Datetime = @Start_Datetime, ActivateUser = @ActivateUser WHERE ID = @ID;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", cartonId);
                    cmd.Parameters.AddWithValue("@Start_Datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Start carton ID '{cartonId}' thành công.")
                        : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi start carton: {ex.Message}");
                }
            }

            public static Result CompleteCarton(string orderNo, int cartonId, string activateUser)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE Carton SET Completed_Datetime = @Completed_Datetime, ActivateUser = @ActivateUser WHERE ID = @ID;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", cartonId);
                    cmd.Parameters.AddWithValue("@Completed_Datetime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@ActivateUser", activateUser ?? "");
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Complete carton ID '{cartonId}' thành công.")
                        : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi complete carton: {ex.Message}");
                }
            }

            public static Result ResetCarton(string orderNo, int cartonId)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return Result.Fail($"Carton DB của PO '{orderNo}' không tồn tại.");

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = @"UPDATE Carton SET cartonCode = '0', Start_Datetime = '0', Completed_Datetime = '0' WHERE ID = @ID;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@ID", cartonId);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0
                        ? Result.Success($"Reset carton ID '{cartonId}' thành công.")
                        : Result.Fail($"Không tìm thấy carton ID: {cartonId}");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi reset carton: {ex.Message}");
                }
            }

            public static int GetTotalCartonCount(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;
                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    using var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Carton;", con);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { return 0; }
            }

            public static int GetClosedCartonCount(string orderNo)
            {
                try
                {
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath)) return 0;
                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    using var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Carton WHERE Completed_Datetime <> '0';", con);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { return 0; }
            }
        }
        #endregion

        // ================== PO HISTORY MANAGER ==================
        #region POHistoryManager
        public static class POHistoryManager
        {
            private const string HISTORY_DB_NAME = "POHistory.db";

            public static string GetHistoryDbPath()
                => Path.Combine(Config.BasePath, HISTORY_DB_NAME);

            public static void EnsureHistoryDB()
            {
                string folder = Path.GetDirectoryName(GetHistoryDbPath()) ?? Config.BasePath;
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                const string sql = @"
                    CREATE TABLE IF NOT EXISTS POHistory (
                        ID             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        PO             TEXT NOT NULL,
                        ProductionDate TEXT NOT NULL DEFAULT '',
                        StartTime      TEXT NOT NULL DEFAULT '',
                        EndTime        TEXT NOT NULL DEFAULT '',
                        Status         TEXT NOT NULL DEFAULT '',
                        UserName       TEXT NOT NULL DEFAULT ''
                    );
                    CREATE INDEX IF NOT EXISTS IDX_PH_PO      ON POHistory(PO);
                    CREATE INDEX IF NOT EXISTS IDX_PH_Status   ON POHistory(Status);
                    PRAGMA journal_mode=WAL;
                ";
                using var con = new SQLiteConnection($"Data Source={GetHistoryDbPath()}");
                con.Open();
                using var cmd = new SQLiteCommand(sql, con);
                cmd.ExecuteNonQuery();
            }

            public static Result RecordStart(string orderNo, string productionDate, string userName)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return Result.Fail("orderNo không được trống.");

                    EnsureHistoryDB();
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using var con = new SQLiteConnection($"Data Source={GetHistoryDbPath()}");
                    con.Open();
                    const string sql = @"INSERT INTO POHistory (PO, ProductionDate, StartTime, EndTime, Status, UserName)
                                         VALUES (@PO, @ProductionDate, @StartTime, @EndTime, @Status, @UserName);";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@PO", orderNo);
                    cmd.Parameters.AddWithValue("@ProductionDate", productionDate ?? "");
                    cmd.Parameters.AddWithValue("@StartTime", now);
                    cmd.Parameters.AddWithValue("@EndTime", "");
                    cmd.Parameters.AddWithValue("@Status", "Running");
                    cmd.Parameters.AddWithValue("@UserName", userName ?? "");
                    cmd.ExecuteNonQuery();
                    return Result.Success($"Đã ghi bắt đầu PO '{orderNo}'.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi ghi Start PO: {ex.Message}");
                }
            }

            public static Result RecordEnd(string orderNo)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return Result.Fail("orderNo không được trống.");

                    EnsureHistoryDB();
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    using var con = new SQLiteConnection($"Data Source={GetHistoryDbPath()}");
                    con.Open();
                    const string sql = "UPDATE POHistory SET EndTime = @EndTime, Status = @Status WHERE PO = @PO AND EndTime = '';";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@PO", orderNo);
                    cmd.Parameters.AddWithValue("@EndTime", now);
                    cmd.Parameters.AddWithValue("@Status", "Completed");
                    cmd.ExecuteNonQuery();
                    return Result.Success($"Đã ghi kết thúc PO '{orderNo}'.");
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Lỗi khi ghi End PO: {ex.Message}");
                }
            }

            public static DataTable? GetLastRunningPO()
            {
                try
                {
                    EnsureHistoryDB();
                    using var con = new SQLiteConnection($"Data Source={GetHistoryDbPath()}");
                    con.Open();
                    const string sql = @"SELECT * FROM POHistory WHERE EndTime = '' AND Status = 'Running' ORDER BY ID DESC LIMIT 1;";
                    using var da = new SQLiteDataAdapter(sql, con);
                    var table = new DataTable();
                    da.Fill(table);
                    return table.Rows.Count > 0 ? table : null;
                }
                catch { return null; }
            }

            public static DataTable? GetLastPO()
            {
                try
                {
                    EnsureHistoryDB();
                    using var con = new SQLiteConnection($"Data Source={GetHistoryDbPath()}");
                    con.Open();
                    const string sql = "SELECT * FROM POHistory ORDER BY ID DESC LIMIT 1;";
                    using var da = new SQLiteDataAdapter(sql, con);
                    var table = new DataTable();
                    da.Fill(table);
                    return table.Rows.Count > 0 ? table : null;
                }
                catch { return null; }
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
                    if (string.IsNullOrWhiteSpace(orderNo)) return false;
                    using var con = new SQLiteConnection($"Data Source={Config.GetPOListPath()}");
                    con.Open();
                    const string sql = "UPDATE PO SET productionDate = @newDate, ModifiedTime = @ModifiedTime WHERE orderNo = @orderNo;";
                    using var cmd = new SQLiteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@orderNo", orderNo);
                    cmd.Parameters.AddWithValue("@newDate", newDate ?? "");
                    cmd.Parameters.AddWithValue("@ModifiedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }
        #endregion

        // ================== CODE DICTIONARY LOADER ==================
        #region CodeDictionaryLoader
        /// <summary>
        /// Load codes từ PO database vào Dictionary để lookup nhanh
        /// </summary>
        public static class CodeDictionaryLoader
        {
            /// <summary>
            /// Load toàn bộ codes từ PO vào Dictionary
            /// </summary>
            public static (bool success, string message, int count) LoadAllCodesToDictionary(
                string orderNo,
                Dictionary<string, GV.CodeInfo> dictionary)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.", 0);

                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return (false, $"Database PO '{orderNo}' không tồn tại.", 0);

                    dictionary.Clear();
                    int count = 0;

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "SELECT * FROM UniqueCodes;";
                    using var cmd = new SQLiteCommand(sql, con);
                    using var rd = cmd.ExecuteReader();

                    while (rd.Read())
                    {
                        var info = new GV.CodeInfo
                        {
                            Code = rd["Code"]?.ToString() ?? "",
                            OrderNo = orderNo,
                            Status = Convert.ToInt32(rd["Status"] ?? 0),
                            CartonCode = rd["cartonCode"]?.ToString() ?? "0",
                            ActivateDate = rd["ActivateDate"]?.ToString() ?? "0",
                            ProductionDate = rd["ProductionDate"]?.ToString() ?? "0",
                            ActivateUser = rd["ActivateUser"]?.ToString() ?? "",
                            PackingDate = rd["PackingDate"]?.ToString() ?? "0",
                            IsPacked = rd["cartonCode"]?.ToString() != "0"
                        };

                        if (!string.IsNullOrEmpty(info.Code))
                        {
                            dictionary[info.Code] = info;
                            count++;
                        }
                    }

                    return (true, $"Đã load {count} codes vào Dictionary.", count);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi load codes: {ex.Message}", 0);
                }
            }

            /// <summary>
            /// Load chỉ codes chưa activate (Status=0) vào Dictionary
            /// </summary>
            public static (bool success, string message, int count) LoadUnusedCodesToDictionary(
                string orderNo,
                Dictionary<string, GV.CodeInfo> dictionary)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.", 0);

                    string dbPath = Config.GetPODBPath(orderNo);
                    if (!File.Exists(dbPath))
                        return (false, $"Database PO '{orderNo}' không tồn tại.", 0);

                    dictionary.Clear();
                    int count = 0;

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "SELECT * FROM UniqueCodes WHERE Status = 0;";
                    using var cmd = new SQLiteCommand(sql, con);
                    using var rd = cmd.ExecuteReader();

                    while (rd.Read())
                    {
                        var info = new GV.CodeInfo
                        {
                            Code = rd["Code"]?.ToString() ?? "",
                            OrderNo = orderNo,
                            Status = 0,
                            CartonCode = "0",
                            ActivateDate = "0",
                            ProductionDate = rd["ProductionDate"]?.ToString() ?? "0",
                            ActivateUser = "",
                            PackingDate = "0",
                            IsPacked = false
                        };

                        if (!string.IsNullOrEmpty(info.Code))
                        {
                            dictionary[info.Code] = info;
                            count++;
                        }
                    }

                    return (true, $"Đã load {count} codes chưa sử dụng vào Dictionary.", count);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi load codes: {ex.Message}", 0);
                }
            }

            /// <summary>
            /// Cập nhật status của code trong database sau khi activate
            /// </summary>
            public static Result ActivateCodeInDatabase(string orderNo, string code,
                string activateDate, string activateUser, string productionDate)
            {
                return POActivator.ActivateCode(orderNo, code, activateDate, activateUser, productionDate);
            }

            /// <summary>
            /// Cập nhật status của code trong database sau khi pack
            /// </summary>
            public static Result PackCodeInDatabase(string orderNo, string code,
                string cartonCode, string packingDate, string packingUser, string productionDate)
            {
                return POPacking.PackCode(orderNo, code, cartonCode, packingDate, packingUser, productionDate);
            }
        }
        #endregion

        // ================== CARTON DICTIONARY LOADER ==================
        #region CartonDictionaryLoader
        /// <summary>
        /// Load cartons từ Carton database vào Dictionary để lookup nhanh
        /// </summary>
        public static class CartonDictionaryLoader
        {
            /// <summary>
            /// Load toàn bộ cartons từ PO vào Dictionary
            /// </summary>
            public static (bool success, string message, int count) LoadAllCartonsToDictionary(
                string orderNo,
                Dictionary<int, GV.CartonInfo> dictionary)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(orderNo))
                        return (false, "orderNo không được trống.", 0);

                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                        return (false, $"Carton DB của PO '{orderNo}' không tồn tại.", 0);

                    dictionary.Clear();
                    int count = 0;

                    using var con = new SQLiteConnection($"Data Source={dbPath}");
                    con.Open();
                    const string sql = "SELECT * FROM Carton ORDER BY ID;";
                    using var cmd = new SQLiteCommand(sql, con);
                    using var rd = cmd.ExecuteReader();

                    while (rd.Read())
                    {
                        var info = new GV.CartonInfo
                        {
                            Id = Convert.ToInt32(rd["ID"] ?? 0),
                            CartonCode = rd["cartonCode"]?.ToString() ?? "0",
                            StartDatetime = rd["Start_Datetime"]?.ToString() ?? "0",
                            CompletedDatetime = rd["Completed_Datetime"]?.ToString() ?? "0",
                            ActivateUser = rd["ActivateUser"]?.ToString() ?? "",
                            ProductionDate = rd["ProductionDate"]?.ToString() ?? "0"
                        };

                        dictionary[info.Id] = info;
                        count++;
                    }

                    return (true, $"Đã load {count} cartons vào Dictionary.", count);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi load cartons: {ex.Message}", 0);
                }
            }

            /// <summary>
            /// Tạo N cartons trống và load vào Dictionary
            /// </summary>
            public static (bool success, string message, int count) CreateAndLoadCartons(
                string orderNo, int count, Dictionary<int, GV.CartonInfo> dictionary)
            {
                try
                {
                    // Ensure carton DB exists
                    string dbPath = Config.GetCartonPath(orderNo);
                    if (!File.Exists(dbPath))
                    {
                        var initResult = POCreator.InitPO(orderNo);
                        if (!initResult.IsSuccess)
                            return (false, initResult.Message, 0);
                    }

                    // Create empty cartons
                    var createResult = POCreator.CreateEmptyCartons(orderNo, count);
                    if (!createResult.success)
                        return (false, createResult.message, 0);

                    // Load lại vào dictionary
                    return LoadAllCartonsToDictionary(orderNo, dictionary);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi tạo cartons: {ex.Message}", 0);
                }
            }

            /// <summary>
            /// Start một carton (bắt đầu đóng gói)
            /// </summary>
            public static Result StartCartonInDatabase(string orderNo, int cartonId, string activateUser)
            {
                return POCarton.StartCarton(orderNo, cartonId, activateUser);
            }

            /// <summary>
            /// Complete một carton (hoàn thành đóng gói)
            /// </summary>
            public static Result CompleteCartonInDatabase(string orderNo, int cartonId, string activateUser)
            {
                return POCarton.CompleteCarton(orderNo, cartonId, activateUser);
            }

            /// <summary>
            /// Reset carton về trạng thái trống
            /// </summary>
            public static Result ResetCartonInDatabase(string orderNo, int cartonId)
            {
                return POCarton.ResetCarton(orderNo, cartonId);
            }
        }
        #endregion

        // ================== PRODUCTION ORDER WRAPPER ==================
        // Lưu ý: Các class bên trong (POLoader, POCreator, etc.) đều là static.
        // Để dùng: po.POLoader.GetAll()
        // Wrapper này giữ lại để tương thích backward compatibility
    }
}
