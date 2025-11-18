
using System.Data;
using System.Data.SQLite;

using System.Security.Cryptography;
using System.Text;


namespace TTManager.Auth
{
    public enum LoginAction
    {
        Login,
        Logout,
        UpdateProfile,
        DeleteAccount,
        AddAccount, // Thêm hành động thêm tài khoản
        ViewData,//xem dữ liệu
        ChangePassword, // Thêm hành động đổi mật khẩu
        ResetPassword, // Thêm hành động đặt lại mật khẩu
        TwoFactorAuthentication, // Thêm hành động xác thực hai yếu tố
        AdminPrivileges // Thêm hành động quản trị viên
    }

    public enum e_User_Role
    {
        Admin,
        Operator,
        Ghost,
        Worker
    }

    public class LoginActionEventArgs : EventArgs
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
    }
    public class UserData
    {
        public string Username { get; set; } = string.Empty;  // Tên user
        public string ?Password { get; set; }  // Hash password
        public string? Salt { get; set; }      // Salt
        public string? Role { get; set; }      // Quyền
        public string? Key2FA { get; set; }    // Khóa 2FA

        public override string ToString()
        {
            return $"User[Username={Username}, Role={Role}]";
        }

        //lấy danh sách user từ sqlite trong table users cột Username
        public static DataTable GetUserListFromDB(string data_file_path)
        {
            var dataTable = new DataTable();
            using (var db = new SQLiteConnection($"Data Source={data_file_path};Version=3;"))
            {
                db.Open();
                using (var command = new SQLiteCommand("SELECT Username FROM users", db))
                {
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        public static bool DeleteUser(string username, string data_file_path)
        {
            using (var conn = new SQLiteConnection($"Data Source={data_file_path};Version=3;"))
            {
                conn.Open();
                string sql = "DELETE FROM users WHERE Username = @username";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0; // Trả về true nếu xóa thành công
                }
            }
        }

        //lấy user từ sqlite trong table users theo Username
        public static UserData GetUserByUsername(string username, string data_file_path)
        {
            UserData user = null;

            using (var conn = new SQLiteConnection($"Data Source={data_file_path};Version=3;"))
            {
                conn.Open();

                string sql = @"SELECT ID, Username, Password, Salt, Role, Key2FA 
                       FROM users WHERE Username = @username LIMIT 1";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            var row = dt.Rows[0];
                            user = new UserData
                            {
                                Username = row["Username"].ToString(),
                                Password = row["Password"].ToString(),
                                Salt = row["Salt"].ToString(),
                                Role = row["Role"].ToString(),
                                Key2FA = row["Key2FA"].ToString()
                            };
                        }
                    }
                }

                conn.Close();
            }

            return user;
        }

        //kiểm tra xem 2FA có đúng không theo username
        
    }
    public class UserHelper
    {
        public static bool Validate2FA(string username, string code, string data_file_path)
        {
            UserData user = UserData.GetUserByUsername(username, data_file_path);
            if (user == null || string.IsNullOrEmpty(user.Key2FA))
            {
                return false; // Người dùng không tồn tại hoặc không có khóa 2FA
            }
            // Kiểm tra mã 2FA
            return TwoFAHelper.VerifyOTP(user.Key2FA, code);
        }

        public static bool IsAdmin(string username, string data_file_path)
        {
            UserData user = UserData.GetUserByUsername(username, data_file_path);
            return user != null && user.Role == "Admin";
        }
        public static bool ValidateCredentials(string username, string password, string data_file_path)
        {
            // File SQLite đặt cạnh file exe
            string dbFile = data_file_path;
            if (!File.Exists(dbFile))
            {
                //trả về sự kiện
                return false;
            }
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT Password, Salt FROM users WHERE Username = @username";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader.GetString(0);
                            string salt = reader.GetString(1);
                            string hashedPassword = HashPassword(password, salt);
                            return storedPassword == hashedPassword;
                        }
                    }
                }
                conn.Close();
            }
            return false;
        }

        // Hàm HashPassword để hash mật khẩu với salt
        public static string HashPassword(string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool UpdatePassword(string username, string password, string data_file_path)
        {

            // File SQLite đặt cạnh file exe
            string dbFile = data_file_path;
            if (!System.IO.File.Exists(dbFile))
            {
                //trả về sự kiện
                return false;
            }
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string salt = Guid.NewGuid().ToString(); // Tạo salt mới
                string hashedPassword = HashPassword(password, salt);
                string sql = "UPDATE users SET Password = @password, Salt = @salt WHERE Username = @username";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@username", username);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0; // Trả về true nếu cập nhật thành công
                }
            }

        }

        public static bool AddUser(string username, string password, string Role, string data_file_path)
        {

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(Role))
            {
                return false; // Trả về false nếu thông tin không hợp lệ
            }
            //tạo key 2FA chuẩn base32
            string key2FA = TwoFAHelper.GenerateSecret();
            UserData user = new UserData
            {
                Username = username,
                Password = password,
                Role = Role,
                Key2FA = key2FA // Khóa 2FA có thể để trống nếu không sử dụng
            };
            // File SQLite đặt cạnh file exe
            string dbFile = data_file_path;
            if (!System.IO.File.Exists(dbFile))
            {
                //trả về sự kiện
                return false;
            }
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string salt = Guid.NewGuid().ToString(); // Tạo salt mới
                string hashedPassword = HashPassword(user.Password, salt);
                string sql = "INSERT INTO users (Username, Password, Salt, Role, Key2FA) VALUES (@username, @password, @salt, @role, @key2fa)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@key2fa", user.Key2FA);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0; // Trả về true nếu thêm thành công
                }
            }
        }

        
    }
}
