using System.Data;
using Microsoft.Data.Sqlite;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// CRUD operations cho PO List
    /// </summary>
    public static class POLoader
    {
        private static string GetConnectionString(string dbPath) => $"Data Source={dbPath}";

        /// <summary>
        /// Đảm bảo PO_List.db tồn tại
        /// </summary>
        public static void EnsurePOList() => Config.EnsurePOList();

        /// <summary>
        /// Lấy danh sách tất cả PO
        /// </summary>
        public static Result GetAll()
        {
            try
            {
                EnsurePOList();
                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(Config.GetPOListPath()), "SELECT * FROM PO ORDER BY orderNo");
                return Result.FromDataTable(table, "Lấy danh sách PO thành công.", "Không có PO nào.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi lấy danh sách PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy PO theo orderNo
        /// </summary>
        public static Result GetByOrderNo(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                EnsurePOList();
                var table = SQLiteHelper.ExecuteQuery(
                    GetConnectionString(Config.GetPOListPath()),
                    "SELECT * FROM PO WHERE orderNo = @orderNo LIMIT 1",
                    new SqliteParameter("@orderNo", orderNo));
                return Result.FromDataTable(table, "Lấy thông tin PO thành công.", $"Không tìm thấy PO: {orderNo}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi lấy PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách PO theo GTIN
        /// </summary>
        public static Result GetByGTIN(string gtin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gtin))
                    return Result.Fail("gtin không được trống.");

                EnsurePOList();
                var table = SQLiteHelper.ExecuteQuery(
                    GetConnectionString(Config.GetPOListPath()),
                    "SELECT * FROM PO WHERE gtin = @gtin ORDER BY orderNo",
                    new SqliteParameter("@gtin", gtin));
                return Result.FromDataTable(table, $"Tìm thấy {table.Rows.Count} PO.", $"Không tìm thấy PO với gtin: {gtin}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi lấy PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo PO mới
        /// </summary>
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

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(Config.GetPOListPath()), @"
                    INSERT INTO PO (orderNo, site, factory, productionLine, productionDate,
                                   shift, orderQty, lotNumber, productCode, productName,
                                   gtin, customerOrderNo, uom, CreatedTime, ModifiedTime)
                    VALUES (@orderNo, @site, @factory, @productionLine, @productionDate,
                            @shift, @orderQty, @lotNumber, @productCode, @productName,
                            @gtin, @customerOrderNo, @uom, @CreatedTime, @ModifiedTime)",
                    new SqliteParameter("@orderNo", po.OrderNo),
                    new SqliteParameter("@site", po.Site ?? ""),
                    new SqliteParameter("@factory", po.Factory ?? ""),
                    new SqliteParameter("@productionLine", po.ProductionLine ?? ""),
                    new SqliteParameter("@productionDate", po.ProductionDate ?? ""),
                    new SqliteParameter("@shift", po.Shift ?? ""),
                    new SqliteParameter("@orderQty", po.OrderQty),
                    new SqliteParameter("@lotNumber", po.LotNumber ?? ""),
                    new SqliteParameter("@productCode", po.ProductCode ?? ""),
                    new SqliteParameter("@productName", po.ProductName ?? ""),
                    new SqliteParameter("@gtin", po.Gtin ?? ""),
                    new SqliteParameter("@customerOrderNo", po.CustomerOrderNo ?? ""),
                    new SqliteParameter("@uom", po.Uom ?? "PCS"),
                    new SqliteParameter("@CreatedTime", now),
                    new SqliteParameter("@ModifiedTime", now));

                return Result.Success($"Tạo PO '{po.OrderNo}' thành công.");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Result.Fail($"PO '{po.OrderNo}' đã tồn tại.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi tạo PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin PO
        /// </summary>
        public static Result Update(string orderNo, POInfo po)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                EnsurePOList();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(Config.GetPOListPath()), @"
                    UPDATE PO SET
                        site = @site, factory = @factory, productionLine = @productionLine,
                        productionDate = @productionDate, shift = @shift, orderQty = @orderQty,
                        lotNumber = @lotNumber, productCode = @productCode, productName = @productName,
                        gtin = @gtin, customerOrderNo = @customerOrderNo, uom = @uom,
                        ModifiedTime = @ModifiedTime
                    WHERE orderNo = @orderNo",
                    new SqliteParameter("@orderNo", orderNo),
                    new SqliteParameter("@site", po.Site ?? ""),
                    new SqliteParameter("@factory", po.Factory ?? ""),
                    new SqliteParameter("@productionLine", po.ProductionLine ?? ""),
                    new SqliteParameter("@productionDate", po.ProductionDate ?? ""),
                    new SqliteParameter("@shift", po.Shift ?? ""),
                    new SqliteParameter("@orderQty", po.OrderQty),
                    new SqliteParameter("@lotNumber", po.LotNumber ?? ""),
                    new SqliteParameter("@productCode", po.ProductCode ?? ""),
                    new SqliteParameter("@productName", po.ProductName ?? ""),
                    new SqliteParameter("@gtin", po.Gtin ?? ""),
                    new SqliteParameter("@customerOrderNo", po.CustomerOrderNo ?? ""),
                    new SqliteParameter("@uom", po.Uom ?? "PCS"),
                    new SqliteParameter("@ModifiedTime", now));

                return rows > 0
                    ? Result.Success($"Cập nhật PO '{orderNo}' thành công.")
                    : Result.Fail($"Không tìm thấy PO: {orderNo}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi khi cập nhật PO: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa PO và tất cả file liên quan
        /// </summary>
        public static Result Delete(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                    return Result.Fail("orderNo không được trống.");

                EnsurePOList();
                int rows = SQLiteHelper.ExecuteNonQuery(GetConnectionString(Config.GetPOListPath()),
                    "DELETE FROM PO WHERE orderNo = @orderNo",
                    new SqliteParameter("@orderNo", orderNo));

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

        /// <summary>
        /// Kiểm tra PO có tồn tại không
        /// </summary>
        public static bool Exists(string orderNo)
        {
            try
            {
                EnsurePOList();
                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(Config.GetPOListPath()),
                    "SELECT COUNT(1) FROM PO WHERE orderNo = @orderNo",
                    new SqliteParameter("@orderNo", orderNo));
                return Convert.ToInt32(result) > 0;
            }
            catch { return false; }
        }

        // ================== QUERY CODES ==================

        /// <summary>
        /// Lấy danh sách codes trong PO với optional filters
        /// </summary>
        public static Result GetCodes(string orderNo, int? status = null, string? cartonCode = null, int limit = 100, int offset = 0)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Database PO '{orderNo}' không tồn tại.");

                string sql = "SELECT * FROM UniqueCodes WHERE 1=1";
                var parameters = new List<SqliteParameter>();

                if (status.HasValue)
                {
                    sql += " AND Status = @Status";
                    parameters.Add(new SqliteParameter("@Status", status.Value));
                }
                if (!string.IsNullOrWhiteSpace(cartonCode))
                {
                    sql += " AND cartonCode = @cartonCode";
                    parameters.Add(new SqliteParameter("@cartonCode", cartonCode));
                }
                sql += " ORDER BY ID";
                sql += $" LIMIT {limit} OFFSET {offset}";

                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(dbPath), sql, parameters.ToArray());
                return Result.FromDataTable(table, $"Lấy {table.Rows.Count} mã thành công.", "Không tìm thấy mã nào.");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi truy vấn codes: {ex.Message}");
            }
        }

        /// <summary>
        /// Đếm tổng số codes trong PO theo filter
        /// </summary>
        public static int CountCodes(string orderNo, int? status = null, string? cartonCode = null)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath)) return 0;

                string sql = "SELECT COUNT(*) FROM UniqueCodes WHERE 1=1";
                var parameters = new List<SqliteParameter>();

                if (status.HasValue)
                {
                    sql += " AND Status = @Status";
                    parameters.Add(new SqliteParameter("@Status", status.Value));
                }
                if (!string.IsNullOrWhiteSpace(cartonCode))
                {
                    sql += " AND cartonCode = @cartonCode";
                    parameters.Add(new SqliteParameter("@cartonCode", cartonCode));
                }

                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath), sql, parameters.ToArray());
                return Convert.ToInt32(result);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Lấy thông tin một code cụ thể
        /// </summary>
        public static Result GetCodeByCode(string orderNo, string code)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath))
                    return Result.Fail($"Database PO '{orderNo}' không tồn tại.");

                var table = SQLiteHelper.ExecuteQuery(GetConnectionString(dbPath),
                    "SELECT * FROM UniqueCodes WHERE Code = @Code LIMIT 1",
                    new SqliteParameter("@Code", code));
                return Result.FromDataTable(table, "Lấy thông tin mã thành công.", $"Không tìm thấy mã: {code}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"Lỗi truy vấn: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra code có tồn tại trong PO không
        /// </summary>
        public static bool CodeExists(string orderNo, string code)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath)) return false;
                var result = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath),
                    "SELECT COUNT(1) FROM UniqueCodes WHERE Code = @Code",
                    new SqliteParameter("@Code", code));
                return Convert.ToInt32(result) > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Đếm số codes trong PO theo status
        /// </summary>
        public static int GetCodeCount(string orderNo, int? status = null)
        {
            try
            {
                string dbPath = Config.GetPODBPath(orderNo);
                if (!File.Exists(dbPath)) return 0;

                string sql = "SELECT COUNT(*) FROM UniqueCodes";
                if (status.HasValue)
                {
                    sql += " WHERE Status = @Status";
                    var result = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath), sql,
                        new SqliteParameter("@Status", status.Value));
                    return Convert.ToInt32(result);
                }

                var result2 = SQLiteHelper.ExecuteScalar(GetConnectionString(dbPath), sql);
                return Convert.ToInt32(result2);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Nạp mã từ DataPool vào PO
        /// </summary>
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
                    return (false, $"DataPool '{gtin}.vnqrdb' không tồn tại.", 0);

                int loadedCount = 0;

                // Lấy mã Status=0 từ DataPool
                List<string> availableCodes = new();
                using (var conPool = new SqliteConnection($"Data Source={dbPoolPath}"))
                {
                    conPool.Open();
                    string sql = "SELECT Code FROM Codes WHERE Status = 0";
                    if (limitQty.HasValue) sql += $" LIMIT {limitQty.Value}";
                    using var cmd = new SqliteCommand(sql, conPool);
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) availableCodes.Add(rd.GetString(0));
                }

                if (availableCodes.Count < orderQty)
                    return (false, $"Không đủ mã! Cần: {orderQty} | Còn: {availableCodes.Count}", 0);

                // Lấy mã đã có trong PO
                HashSet<string> existingCodes = new(StringComparer.OrdinalIgnoreCase);
                using (var conPO = new SqliteConnection($"Data Source={dbPOPath}"))
                {
                    conPO.Open();
                    using var cmd = new SqliteCommand("SELECT Code FROM UniqueCodes;", conPO);
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) existingCodes.Add(rd.GetString(0));
                }

                // Insert vào PO + update DataPool
                using var conPool2 = new SqliteConnection($"Data Source={dbPoolPath}");
                using var conPO2 = new SqliteConnection($"Data Source={dbPOPath}");
                conPool2.Open();
                conPO2.Open();
                using var tx = conPO2.BeginTransaction();

                const string insertPO = @"INSERT OR IGNORE INTO UniqueCodes (Code, Status, ProductionDate) VALUES (@Code, 0, @ProductionDate);";
                const string updatePool = "UPDATE Codes SET Status = 1 WHERE Code = @Code;";

                for (int i = 0; i < orderQty && i < availableCodes.Count; i++)
                {
                    string code = availableCodes[i];
                    if (existingCodes.Contains(code)) continue;

                    using (var cmdInsert = new SqliteCommand(insertPO, conPO2, tx))
                    {
                        cmdInsert.Parameters.AddWithValue("@Code", code);
                        cmdInsert.Parameters.AddWithValue("@ProductionDate", poInfo.Data.Rows[0]["productionDate"]?.ToString() ?? "");
                        cmdInsert.ExecuteNonQuery();
                        loadedCount++;
                    }
                    using (var cmdUpdate = new SqliteCommand(updatePool, conPool2))
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
}
