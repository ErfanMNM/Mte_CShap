using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace VNQR.DataPool
{
    public class DataPool
    {
        public static string dataPath = $"C:/VNQR/Databases";//Link mặc định bể chứa dữ liệu
    }

    //một bể dữ liệu là 1 file sqlite, trong đó có 1 bảng dữ liệu chính chứa các cột: ID, Code, Status, orderNo, CreateTime, CreateID, Note ( Code là mã code, Status là trạng thái mã 0 = chưa dùng, 1= đã dùng, orderNo là mã lệnh sản xuất, CreateTime là thời gian tạo, CreateID là mã của phiếu tạo, Note là ghi chú). Sqlite chạy WAL mode.

    //<tên bể>.vnqrdb
    //nhập dữ liệu mới vào bể chứa dữ liệu : có các cách nhập liệu sau:
    //1. Nhập liệu thủ công: người dùng nhập liệu trực tiếp vào bể chứa dữ liệu thông qua giao diện người dùng (cho phép nhập Code, Status, orderNo, CreateID, Note, các cột còn lại sẽ được hệ thống tự động điền vào) => Từng cái 1
    //2. Nhập liệu tự động từ đầu đọc mã: hệ thống nhập liệu tự động từ đầu đọc mã code của camera, sau đó lưu vào bể chứa dữ liệu. (cho phép nhập Code, orderNo, CreateID, Note, các cột còn lại sẽ được hệ thống tự động điền vào) => tự động đánh dấu là đã dùng với mã chưa từng tồn tại. => nếu gặp mã đã tồn tại mà chưa dùng thì tự động đánh dấu là đã dùng để khỏi phải update á. còn nếu gặp mã đã tồn tại và đã dùng thì trả về lỗi và không lưu vào bể chứa dữ liệu. => Từng cái 1
    //3. Nhập liệu từ file: người dùng có thể nhập liệu từ file csv vào bể chứa dữ liệu. hàm sẽ nhận địa chỉ file, tên user, cột Code và Note (chỉ cho phép nhập Code và Note, các cột còn lại sẽ được hệ thống tự động điền vào) =>  sẽ tạo một file sqlite có tên  = CreateID chứa thông tin phiên nhập bao gồm CreateID, UserName, CreateTime, Note, ImportMethod, ImportSource, ImportCount.. => Nhập hàng loạt.

    //orderNo có thể để trống nếu mã chưa dùng, nhưng nếu mã đã dùng thì orderNo phải có giá trị để phân biệt các lô sản xuất khác nhau. CreateID là mã của phiếu tạo, nếu được thêm tay thì ghi là User:<Tên User>, nếu add bằng camera thì ghi Reader, nhưng nếu có phiếu tạo thì CreateID phải có giá trị để phân biệt các phiếu tạo khác nhau. Note là ghi chú, có thể để trống nếu không có ghi chú, nhưng nếu có ghi chú thì Note phải có giá trị để phân biệt các ghi chú khác nhau.

    public enum e_ImportMethod
    {
        Manual,
        Reader,
        File
    }

    public enum e_CodeStatus
    {
        Unused = 0,
        Used = 1
    }

    public class TResult
    {
        public bool issuccess { get; set; }
        public string message { get; set; }
        public DataTable data { get; set; }
        public int count { get; set; }

        // PascalCase aliases
        public bool IsSuccess => issuccess;
        public string Message => message;
        public int Count => count;

        public TResult(bool issuccess, string message, int count = 0, DataTable data = null)
        {
            this.issuccess = issuccess;
            this.message = message;
            this.data = data;
            this.count = count;
        }
    }

    // ================== POOL HELPER ==================
    // Helper chung để tạo / mở 1 bể dữ liệu và file phiếu tạo.
    public static class PoolHelper
    {
        // Lấy đường dẫn file .vnqrdb theo tên bể
        public static string GetPoolPath(string poolName)
        {
            if (string.IsNullOrWhiteSpace(poolName))
                throw new ArgumentException("Tên bể dữ liệu không được trống.", nameof(poolName));
            return Path.Combine(DataPool.dataPath, poolName + ".vnqrdb");
        }

        // Lấy đường dẫn file phiếu tạo (nằm trong thư mục con Phieu)
        public static string GetPhieuPath(string createID)
        {
            if (string.IsNullOrWhiteSpace(createID))
                throw new ArgumentException("CreateID không được trống.", nameof(createID));
            return Path.Combine(DataPool.dataPath, "Phieu", createID + ".vnqrdb");
        }

        // Đảm bảo folder tồn tại + tạo file sqlite (nếu chưa có) + tạo bảng Codes (nếu chưa có) + bật WAL.
        public static void EnsurePool(string poolName)
        {
            string dbPath = GetPoolPath(poolName);
            string folder = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            const string sql = @"
                CREATE TABLE IF NOT EXISTS Codes (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT NOT NULL UNIQUE,
                    Status INTEGER NOT NULL DEFAULT 0,
                    orderNo TEXT NOT NULL DEFAULT '',
                    CreateTime TEXT NOT NULL,
                    CreateID TEXT NOT NULL,
                    Note TEXT NOT NULL DEFAULT ''
                );
                CREATE INDEX IF NOT EXISTS IDX_Codes_Status ON Codes(Status);
                CREATE INDEX IF NOT EXISTS IDX_Codes_orderNo ON Codes(orderNo);
                CREATE INDEX IF NOT EXISTS IDX_Codes_CreateID ON Codes(CreateID);
                PRAGMA journal_mode=WAL;
            ";

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Đảm bảo file phiếu tạo tồn tại + tạo bảng PhieuTao + bật WAL.
        public static void EnsurePhieu(string createID)
        {
            string dbPath = GetPhieuPath(createID);
            string folder = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            const string sql = @"
                CREATE TABLE IF NOT EXISTS PhieuTao (
                    CreateID TEXT PRIMARY KEY,
                    UserName TEXT NOT NULL,
                    CreateTime TEXT NOT NULL,
                    Note TEXT NOT NULL DEFAULT '',
                    ImportMethod TEXT NOT NULL,
                    ImportSource TEXT NOT NULL DEFAULT '',
                    ImportCount INTEGER NOT NULL DEFAULT 0
                );
                PRAGMA journal_mode=WAL;
            ";

            using (var con = new SQLiteConnection($"Data Source={dbPath}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    // ================== IMPORT ==================
    // Gồm 3 phương thức nhập liệu tương ứng với 3 cách trong comment.
    public static class Import
    {
        // 1. Nhập liệu thủ công (từng cái 1).
        // Người dùng truyền vào: Code, Status (mặc định 0), orderNo, CreateID (mặc định "User:<userName>"), Note, userName.
        // Hệ thống tự điền: ID, CreateTime.
        public static TResult Manual(
            string poolName,
            string code,
            int? status = null,
            string orderNo = "",
            string createID = "",
            string note = "",
            string userName = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName))
                    return new TResult(false, "Tên bể dữ liệu không được trống.");
                if (string.IsNullOrWhiteSpace(code))
                    return new TResult(false, "Mã code không được trống.");

                PoolHelper.EnsurePool(poolName);

                string finalCreateID = string.IsNullOrWhiteSpace(createID)
                    ? $"User:{userName}"
                    : createID;
                int finalStatus = status ?? (int)e_CodeStatus.Unused;
                string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Mã đã tồn tại -> báo lỗi (nhập tay từng cái 1).
                if (CodeExists(poolName, code))
                    return new TResult(false, $"Mã '{code}' đã tồn tại trong bể.");

                // Validate: mã đã dùng bắt buộc phải có orderNo.
                if (finalStatus == (int)e_CodeStatus.Used && string.IsNullOrWhiteSpace(orderNo))
                    return new TResult(false, "Mã đã dùng phải có orderNo.");

                using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
                {
                    con.Open();
                    const string sql = @"
                        INSERT INTO Codes (Code, Status, orderNo, CreateTime, CreateID, Note)
                        VALUES (@Code, @Status, @orderNo, @CreateTime, @CreateID, @Note);";
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Code", code.Trim());
                        cmd.Parameters.AddWithValue("@Status", finalStatus);
                        cmd.Parameters.AddWithValue("@orderNo", orderNo ?? "");
                        cmd.Parameters.AddWithValue("@CreateTime", createTime);
                        cmd.Parameters.AddWithValue("@CreateID", finalCreateID);
                        cmd.Parameters.AddWithValue("@Note", note ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }

                return new TResult(true, "Nhập mã thủ công thành công.", 1);
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi khi nhập mã thủ công: {ex.Message}");
            }
        }

        // 2. Nhập liệu tự động từ đầu đọc mã (từng cái 1).
        // Người dùng truyền vào: Code, orderNo, CreateID (mặc định "Reader"), Note.
        // Quy tắc:
        //  - Mã chưa từng tồn tại -> insert mới, Status = 1 (đã dùng).
        //  - Mã đã tồn tại nhưng Status = 0 (chưa dùng) -> update Status = 1.
        //  - Mã đã tồn tại và Status = 1 (đã dùng) -> trả lỗi, không lưu.
        public static TResult FromReader(
            string poolName,
            string code,
            string orderNo,
            string createID = "Reader",
            string note = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName))
                    return new TResult(false, "Tên bể dữ liệu không được trống.");
                if (string.IsNullOrWhiteSpace(code))
                    return new TResult(false, "Mã code không được trống.");
                if (string.IsNullOrWhiteSpace(orderNo))
                    return new TResult(false, "Mã từ camera phải có orderNo.");

                PoolHelper.EnsurePool(poolName);

                var existing = GetByCode(poolName, code);
                if (existing.issuccess && existing.data != null && existing.data.Rows.Count > 0)
                {
                    int currentStatus = Convert.ToInt32(existing.data.Rows[0]["Status"]);
                    if (currentStatus == (int)e_CodeStatus.Used)
                        return new TResult(false, $"Mã '{code}' đã được sử dụng trước đó.");

                    // Chưa dùng -> update lên đã dùng.
                    bool ok = Updater.UpdateStatus(poolName, code, (int)e_CodeStatus.Used, orderNo, createID, note);
                    if (ok) return new TResult(true, "Đã cập nhật mã chưa dùng thành đã dùng.", 1);
                    return new TResult(false, "Không thể cập nhật trạng thái mã.");
                }

                // Mã chưa tồn tại -> insert mới với Status = Used.
                string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
                {
                    con.Open();
                    const string sql = @"
                        INSERT INTO Codes (Code, Status, orderNo, CreateTime, CreateID, Note)
                        VALUES (@Code, @Status, @orderNo, @CreateTime, @CreateID, @Note);";
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Code", code.Trim());
                        cmd.Parameters.AddWithValue("@Status", (int)e_CodeStatus.Used);
                        cmd.Parameters.AddWithValue("@orderNo", orderNo);
                        cmd.Parameters.AddWithValue("@CreateTime", createTime);
                        cmd.Parameters.AddWithValue("@CreateID", createID);
                        cmd.Parameters.AddWithValue("@Note", note ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }

                return new TResult(true, "Đã thêm mã mới và đánh dấu đã dùng.", 1);
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi khi nhập mã từ camera: {ex.Message}");
            }
        }

        // 3. Nhập liệu từ file CSV (nhập hàng loạt).
        // Người dùng truyền vào: đường dẫn file csv, tên user, tên cột Code, tên cột Note (có thể bỏ trống),
        // CreateID (mã phiếu), note phiếu.
        // Hệ thống tự điền cho từng dòng: ID, Status = 0, orderNo = "", CreateTime, CreateID.
        // Đồng thời tạo file sqlite <CreateID>.vnqrdb trong thư mục Phieu/ chứa thông tin phiên nhập.
        public static TResult FromFile(
            string poolName,
            string csvPath,
            string userName,
            string createID,
            string codeColumn,
            string noteColumn = "",
            string note = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName))
                    return new TResult(false, "Tên bể dữ liệu không được trống.");
                if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                    return new TResult(false, $"Không tìm thấy file CSV: {csvPath}");
                if (string.IsNullOrWhiteSpace(createID))
                    return new TResult(false, "CreateID không được trống.");
                if (string.IsNullOrWhiteSpace(codeColumn))
                    return new TResult(false, "Tên cột Code không được trống.");

                PoolHelper.EnsurePool(poolName);
                PoolHelper.EnsurePhieu(createID);

                string createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var lines = File.ReadAllLines(csvPath);
                if (lines.Length < 2)
                    return new TResult(false, "File CSV trống hoặc không có dữ liệu.");

                var header = SplitCsvLine(lines[0]);
                int codeIdx = Array.IndexOf(header, codeColumn);
                if (codeIdx < 0)
                    return new TResult(false, $"Không tìm thấy cột '{codeColumn}' trong file CSV.");

                int? noteIdx = string.IsNullOrWhiteSpace(noteColumn)
                    ? null
                    : Array.IndexOf(header, noteColumn);
                if (!string.IsNullOrWhiteSpace(noteColumn) && (noteIdx == null || noteIdx < 0))
                    return new TResult(false, $"Không tìm thấy cột '{noteColumn}' trong file CSV.");

                int successCount = 0;
                int skipCount = 0;

                using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        const string insertSql = @"
                            INSERT OR IGNORE INTO Codes (Code, Status, orderNo, CreateTime, CreateID, Note)
                            VALUES (@Code, @Status, @orderNo, @CreateTime, @CreateID, @Note);";

                        for (int i = 1; i < lines.Length; i++)
                        {
                            var row = SplitCsvLine(lines[i]);
                            if (row.Length <= codeIdx) continue;
                            string code = row[codeIdx].Trim();
                            if (string.IsNullOrWhiteSpace(code)) continue;

                            string rowNote = (noteIdx.HasValue && row.Length > noteIdx.Value)
                                ? row[noteIdx.Value].Trim()
                                : "";

                            using (var cmd = new SQLiteCommand(insertSql, con, tx))
                            {
                                cmd.Parameters.AddWithValue("@Code", code);
                                cmd.Parameters.AddWithValue("@Status", (int)e_CodeStatus.Unused);
                                cmd.Parameters.AddWithValue("@orderNo", "");
                                cmd.Parameters.AddWithValue("@CreateTime", createTime);
                                cmd.Parameters.AddWithValue("@CreateID", createID);
                                cmd.Parameters.AddWithValue("@Note", rowNote);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0) successCount++;
                                else skipCount++;
                            }
                        }
                        tx.Commit();
                    }
                }

                // Ghi phiếu tạo (ghi đè nếu CreateID đã tồn tại).
                using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPhieuPath(createID)}"))
                {
                    con.Open();
                    const string sql = @"
                        INSERT OR REPLACE INTO PhieuTao
                        (CreateID, UserName, CreateTime, Note, ImportMethod, ImportSource, ImportCount)
                        VALUES (@CreateID, @UserName, @CreateTime, @Note, @ImportMethod, @ImportSource, @ImportCount);";
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@CreateID", createID);
                        cmd.Parameters.AddWithValue("@UserName", userName ?? "");
                        cmd.Parameters.AddWithValue("@CreateTime", createTime);
                        cmd.Parameters.AddWithValue("@Note", note ?? "");
                        cmd.Parameters.AddWithValue("@ImportMethod", e_ImportMethod.File.ToString());
                        cmd.Parameters.AddWithValue("@ImportSource", csvPath);
                        cmd.Parameters.AddWithValue("@ImportCount", successCount);
                        cmd.ExecuteNonQuery();
                    }
                }

                return new TResult(true,
                    $"Nhập từ file hoàn tất: {successCount} thêm mới, {skipCount} bị bỏ qua (trùng mã).",
                    successCount);
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi khi nhập từ file: {ex.Message}");
            }
        }

        // ================== HELPER QUERY ==================
        public static bool CodeExists(string poolName, string code)
        {
            PoolHelper.EnsurePool(poolName);
            using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT COUNT(1) FROM Codes WHERE Code = @Code;", con))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public static TResult GetByCode(string poolName, string code)
        {
            try
            {
                PoolHelper.EnsurePool(poolName);
                using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
                {
                    con.Open();
                    const string sql = "SELECT * FROM Codes WHERE Code = @Code LIMIT 1;";
                    using (var da = new SQLiteDataAdapter(sql, con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@Code", code);
                        var table = new DataTable();
                        da.Fill(table);
                        if (table.Rows.Count > 0)
                            return new TResult(true, "Lấy thông tin mã thành công.", 1, table);
                        return new TResult(false, "Không tìm thấy mã.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi truy vấn: {ex.Message}");
            }
        }

        // Bộ tách CSV tối thiểu: hỗ trợ trường được bao bởi dấu nháy kép "..." (có escape "" bên trong).
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuote = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuote && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuote = !inQuote;
                    }
                }
                else if (c == ',' && !inQuote)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result.ToArray();
        }
    }

    // ================== UPDATER ==================
    // Các hàm cập nhật dữ liệu đã có trong bể.
    public static class Updater
    {
        // Cập nhật Status + Reason kiểu mới (kèm orderNo / CreateID / Note tùy chọn).
        // Trả về true nếu có dòng bị ảnh hưởng, false nếu không tìm thấy Code.
        public static bool UpdateStatus(
            string poolName,
            string code,
            int newStatus,
            string orderNo = "",
            string createID = "",
            string note = "")
        {
            PoolHelper.EnsurePool(poolName);
            using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
            {
                con.Open();
                // Chỉ ghi đè khi giá trị truyền vào khác rỗng; nếu rỗng thì giữ nguyên giá trị cũ.
                const string sql = @"
                    UPDATE Codes
                    SET Status = @Status,
                        orderNo = COALESCE(NULLIF(@orderNo, ''), orderNo),
                        CreateID = COALESCE(NULLIF(@CreateID, ''), CreateID),
                        Note = COALESCE(NULLIF(@Note, ''), Note)
                    WHERE Code = @Code;";
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@orderNo", orderNo ?? "");
                    cmd.Parameters.AddWithValue("@CreateID", createID ?? "");
                    cmd.Parameters.AddWithValue("@Note", note ?? "");
                    cmd.Parameters.AddWithValue("@Code", code);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Cập nhật mềm: chỉ ghi đè những cột có giá trị truyền vào (null = bỏ qua).
        public static bool Update(
            string poolName,
            string code,
            int? status = null,
            string orderNo = null,
            string createID = null,
            string note = null)
        {
            if (string.IsNullOrWhiteSpace(poolName) || string.IsNullOrWhiteSpace(code))
                return false;

            var sets = new List<string>();
            var parameters = new Dictionary<string, object>
            {
                { "@Code", code }
            };

            if (status.HasValue)
            {
                sets.Add("Status = @Status");
                parameters.Add("@Status", status.Value);
            }
            if (orderNo != null)
            {
                sets.Add("orderNo = @orderNo");
                parameters.Add("@orderNo", orderNo);
            }
            if (createID != null)
            {
                sets.Add("CreateID = @CreateID");
                parameters.Add("@CreateID", createID);
            }
            if (note != null)
            {
                sets.Add("Note = @Note");
                parameters.Add("@Note", note);
            }

            if (sets.Count == 0) return false;

            string sql = $"UPDATE Codes SET {string.Join(", ", sets)} WHERE Code = @Code;";

            PoolHelper.EnsurePool(poolName);
            using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Đánh dấu mã đã dùng, kèm orderNo bắt buộc.
        public static bool MarkUsed(string poolName, string code, string orderNo, string createID = "", string note = "")
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return false;
            return UpdateStatus(poolName, code, (int)e_CodeStatus.Used, orderNo, createID, note);
        }

        // Đánh dấu mã chưa dùng lại (reset). orderNo sẽ bị xóa về rỗng nếu truyền "".
        public static bool MarkUnused(string poolName, string code)
        {
            PoolHelper.EnsurePool(poolName);
            using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
            {
                con.Open();
                const string sql = @"
                    UPDATE Codes
                    SET Status = 0,
                        orderNo = ''
                    WHERE Code = @Code;";
                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Xóa cứng 1 mã khỏi bể.
        public static bool Delete(string poolName, string code)
        {
            PoolHelper.EnsurePool(poolName);
            using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(
                    "DELETE FROM Codes WHERE Code = @Code;", con))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    // ================== QUERY ==================
    // Các hàm truy vấn danh sách codes trong một bể.
    public static class Query
    {
        // Lấy toàn bộ codes trong bể (có thể lọc theo status).
        public static TResult GetAllCodes(string poolName, int? status = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(poolName))
                    return new TResult(false, "Tên bể dữ liệu không được trống.");

                PoolHelper.EnsurePool(poolName);

                using (var con = new SQLiteConnection($"Data Source={PoolHelper.GetPoolPath(poolName)}"))
                {
                    con.Open();
                    string sql = status.HasValue
                        ? "SELECT * FROM Codes WHERE Status = @Status ORDER BY ID ASC;"
                        : "SELECT * FROM Codes ORDER BY ID ASC;";
                    using (var da = new SQLiteDataAdapter(sql, con))
                    {
                        if (status.HasValue)
                            da.SelectCommand.Parameters.AddWithValue("@Status", status.Value);
                        var table = new DataTable();
                        da.Fill(table);
                        return new TResult(true, "Lấy danh sách mã thành công.", table.Rows.Count, table);
                    }
                }
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi truy vấn: {ex.Message}");
            }
        }
    }

    // ================== LISTER ==================
    // Liệt kê các bể dữ liệu hiện có trong thư mục DataPool.
    public static class Lister
    {
        public static TResult ListPools()
        {
            try
            {
                if (!Directory.Exists(DataPool.dataPath))
                    return new TResult(true, "Thư mục DataPool chưa tồn tại.", 0, new DataTable());

                var table = new DataTable();
                table.Columns.Add("name", typeof(string));
                table.Columns.Add("fileName", typeof(string));
                table.Columns.Add("size", typeof(long));

                var files = Directory.GetFiles(DataPool.dataPath, "*.vnqrdb");
                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    table.Rows.Add(
                        Path.GetFileNameWithoutExtension(file),
                        info.Name,
                        info.Length);
                }

                return new TResult(true, "Liệt kê bể dữ liệu thành công.", table.Rows.Count, table);
            }
            catch (Exception ex)
            {
                return new TResult(false, $"Lỗi liệt kê bể: {ex.Message}");
            }
        }
    }
}
