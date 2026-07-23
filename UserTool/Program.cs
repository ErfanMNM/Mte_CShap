using TTManager.Auth;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

const string DefaultDbPath = @"C:\Users\TH\AppData\Local\TanTien\Users\users.database";

Console.Clear();
PrintBanner();

while (true)
{
    PrintMenu();
    var key = Console.ReadKey(true);

    switch (key.Key)
    {
        case ConsoleKey.D1: ListUsers(); break;
        case ConsoleKey.D2: AddUser(); break;
        case ConsoleKey.D3: DeleteUser(); break;
        case ConsoleKey.D4: ChangePassword(); break;
        case ConsoleKey.D5: ResetPassword(); break;
        case ConsoleKey.D6: InitDatabase(); break;
        case ConsoleKey.D0: Environment.Exit(0); break;
        default: break;
    }

    if (key.Key != ConsoleKey.D6)
        Pause();
}

static void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"  ____  _       _     ____                           ");
    Console.WriteLine(@" | __ )(_) ___ | | __/ ___|  ___ ___  _ __   ___  _ __ ");
    Console.WriteLine(@" |  _ \| |/ _ \| |/ /\___ \ / __/ _ \| '_ \ / _ \| '__|");
    Console.WriteLine(@" | |_) | | (_) |   <  ___) | (_| (_) | |_) | (_) | |   ");
    Console.WriteLine(@" |____/|_|\___/|_|\_\|____/ \___\___/| .__/ \___/|_|   ");
    Console.WriteLine(@"                                     |_|                ");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine($"  Database: {DefaultDbPath}");
    Console.WriteLine();
}

static void PrintMenu()
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  ╔════════════════════════════════════════╗");
    Console.WriteLine("  ║      USER MANAGEMENT TOOL              ║");
    Console.WriteLine("  ╠════════════════════════════════════════╣");
    Console.WriteLine("  ║  [1] Xem danh sach nguoi dung         ║");
    Console.WriteLine("  ║  [2] Them nguoi dung moi              ║");
    Console.WriteLine("  ║  [3] Xoa nguoi dung                   ║");
    Console.WriteLine("  ║  [4] Doi mat khau                    ║");
    Console.WriteLine("  ║  [5] Reset mat khau nhanh            ║");
    Console.WriteLine("  ║  [6] Khoi tao database (lan dau)     ║");
    Console.WriteLine("  ║  [0] Thoat                          ║");
    Console.WriteLine("  ╚════════════════════════════════════════╝");
    Console.Write("  Chon: ");
    Console.ResetColor();
}

static void InitDatabase()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--- KHOI TAO DATABASE ---");
    Console.ResetColor();

    string? directory = Path.GetDirectoryName(DefaultDbPath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
        Console.WriteLine($"  [OK] Da tao thu muc: {directory}");
    }

    if (!File.Exists(DefaultDbPath))
    {
        SQLiteConnection.CreateFile(DefaultDbPath);
        Console.WriteLine($"  [OK] Da tao file: {DefaultDbPath}");
    }

    using (var conn = new SQLiteConnection($"Data Source={DefaultDbPath};Version=3;"))
    {
        conn.Open();

        var createTableSql = @"CREATE TABLE IF NOT EXISTS ""users""(
            ""ID""    INTEGER,
            ""Username""  TEXT NOT NULL,
            ""Password""  TEXT NOT NULL,
            ""Salt""  TEXT NOT NULL,
            ""Role""  TEXT NOT NULL,
            ""Key2FA""    TEXT NOT NULL,
            PRIMARY KEY(""ID"" AUTOINCREMENT)
        );";
        using (var cmd = new SQLiteCommand(createTableSql, conn))
        {
            cmd.ExecuteNonQuery();
        }

        using (var pragmaCmd = conn.CreateCommand())
        {
            pragmaCmd.CommandText = "PRAGMA journal_mode=WAL;";
            pragmaCmd.ExecuteNonQuery();
        }

        Console.WriteLine("  [OK] Bang users da san sang");
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  Database khoi tao thanh cong!");
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  Ban co muon tao tai khoan Admin dau tien? (Y/N): ");
    Console.ResetColor();
    var confirm = Console.ReadKey(true);
    if (confirm.Key == ConsoleKey.Y)
    {
        AddUserInternal(conn: null, DefaultDbPath, "Admin");
    }
}

static void ListUsers()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--- DANH SACH NGUOI DUNG ---");
    Console.ResetColor();

    if (!File.Exists(DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Database chua ton tai. Vui long khoi tao (option 6).");
        Console.ResetColor();
        return;
    }

    try
    {
        var users = UserData.GetUserListFromDB(DefaultDbPath);
        if (users.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Chua co nguoi dung nao.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("  {0,-5} {1,-20} {2,-10} {3,-30}", "ID", "Username", "Role", "2FA");
        Console.WriteLine("  {0,-5} {1,-20} {2,-10} {3,-30}", new string('-', 5), new string('-', 20), new string('-', 10), new string('-', 30));
        Console.ResetColor();

        foreach (System.Data.DataRow row in users.Rows)
        {
            string username = row["Username"].ToString() ?? "";
            if (username == "SA") continue;

            var fullUser = UserData.GetUserByUsername(username, DefaultDbPath);
            string role = fullUser?.Role ?? "";
            string has2FA = string.IsNullOrEmpty(fullUser?.Key2FA) ? "Khong" : "Co";
            Console.WriteLine("  {0,-5} {1,-20} {2,-10} {3,-30}", $"#{row["ID"]}", username, role, has2FA);
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Loi: {ex.Message}");
        Console.ResetColor();
    }
}

static void AddUser()
{
    AddUserInternal(conn: null, DefaultDbPath, null);
}

static void AddUserInternal(SQLiteConnection? conn, string dbPath, string? forcedRole)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--- THEM NGUOI DUNG MOI ---");
    Console.ResetColor();

    if (!File.Exists(dbPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Database chua ton tai. Vui long khoi tao (option 6).");
        Console.ResetColor();
        return;
    }

    string username;
    while (true)
    {
        Console.Write("  Username: ");
        username = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(username))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Username khong duoc de trong.");
            Console.ResetColor();
            continue;
        }
        if (username == "SA")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Username 'SA' duoc reserved, vui long chon ten khac.");
            Console.ResetColor();
            continue;
        }
        var existing = UserData.GetUserByUsername(username, dbPath);
        if (existing != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Username da ton tai. Vui long chon ten khac.");
            Console.ResetColor();
            continue;
        }
        break;
    }

    Console.Write("  Password: ");
    string password = ReadPassword();

    string role;
    if (!string.IsNullOrEmpty(forcedRole))
    {
        role = forcedRole;
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("  Chon vai tro:");
        Console.WriteLine("    [1] Admin    - Toan quyen");
        Console.WriteLine("    [2] Operator - Van hanh");
        Console.Write("  Lua chon (1/2): ");

        var roleKey = Console.ReadKey(true);
        Console.WriteLine(roleKey.KeyChar);
        role = roleKey.Key == ConsoleKey.D1 ? "Admin" : "Operator";
    }

    string salt = Guid.NewGuid().ToString();
    string hashedPassword = HashPassword(password, salt);
    string key2FA = TwoFAHelper.GenerateSecret();

    bool closeConn = false;
    if (conn == null)
    {
        conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        conn.Open();
        closeConn = true;
    }

    try
    {
        string sql = "INSERT INTO users (Username, Password, Salt, Role, Key2FA) VALUES (@username, @password, @salt, @role, @key2fa)";
        using (var cmd = new SQLiteCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@key2fa", key2FA);
            cmd.ExecuteNonQuery();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  [OK] Da tao tai khoan thanh cong!");
        Console.ResetColor();
        Console.WriteLine($"  Username : {username}");
        Console.WriteLine($"  Password : {password}");
        Console.WriteLine($"  Role     : {role}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  2FA Key  : {key2FA}");
        Console.WriteLine("  (Su dung key 2FA nay de cau hinh Google Authenticator)");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Loi: {ex.Message}");
        Console.ResetColor();
    }
    finally
    {
        if (closeConn && conn != null)
            conn.Close();
    }
}

static void DeleteUser()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--- XOA NGUOI DUNG ---");
    Console.ResetColor();

    if (!File.Exists(DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Database chua ton tai.");
        Console.ResetColor();
        return;
    }

    var users = UserData.GetUserListFromDB(DefaultDbPath);
    if (users.Rows.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  Chua co nguoi dung nao.");
        Console.ResetColor();
        return;
    }

    Console.WriteLine();
    foreach (System.Data.DataRow row in users.Rows)
    {
        string un = row["Username"].ToString() ?? "";
        if (un == "SA") continue;
        Console.WriteLine($"  [{row["ID"]}] {un}");
    }

    Console.Write("\n  Nhap username can xoa: ");
    var username = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrEmpty(username))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  Da huy.");
        Console.ResetColor();
        return;
    }

    if (username == "SA")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Khong the xoa tai khoan SA.");
        Console.ResetColor();
        return;
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"  Ban co chac chan xoa '{username}'? (Y/N): ");
    Console.ResetColor();
    var confirm = Console.ReadKey(true);
    Console.WriteLine(confirm.KeyChar);

    if (confirm.Key != ConsoleKey.Y)
    {
        Console.WriteLine("  Da huy.");
        return;
    }

    if (UserData.DeleteUser(username, DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [OK] Da xoa tai khoan '{username}'.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Loi: Khong the xoa tai khoan.");
        Console.ResetColor();
    }
}

static void ChangePassword()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--- DOI MAT KHAU ---");
    Console.ResetColor();

    if (!File.Exists(DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Database chua ton tai.");
        Console.ResetColor();
        return;
    }

    Console.Write("  Username: ");
    var username = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrEmpty(username))
    {
        Console.WriteLine("  Da huy.");
        return;
    }

    var user = UserData.GetUserByUsername(username, DefaultDbPath);
    if (user == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Tai khoan '{username}' khong ton tai.");
        Console.ResetColor();
        return;
    }

    Console.Write("  Mat khau hien tai: ");
    var oldPw = ReadPassword();

    if (!UserHelper.ValidateCredentials(username, oldPw, DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  Mat khau hien tai khong dung.");
        Console.ResetColor();
        return;
    }

    Console.Write("\n  Mat khau moi: ");
    var newPw = ReadPassword();

    if (string.IsNullOrEmpty(newPw))
    {
        Console.WriteLine("\n  Mat khau moi khong duoc de trong.");
        return;
    }

    if (UserHelper.UpdatePassword(username, newPw, DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  [OK] Da doi mat khau thanh cong!");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  Loi: Khong the doi mat khau.");
        Console.ResetColor();
    }
}

static void ResetPassword()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n--- RESET MAT KHAU NHANH (cho Admin) ---");
    Console.ResetColor();

    if (!File.Exists(DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Database chua ton tai.");
        Console.ResetColor();
        return;
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  Danh sach nguoi dung hien co:");
    Console.ResetColor();

    var users = UserData.GetUserListFromDB(DefaultDbPath);
    if (users.Rows.Count == 0)
    {
        Console.WriteLine("  Chua co nguoi dung.");
        return;
    }

    foreach (System.Data.DataRow row in users.Rows)
    {
        string un = row["Username"].ToString() ?? "";
        if (un == "SA") continue;
        Console.WriteLine($"  - {un}");
    }

    Console.Write("\n  Username can reset: ");
    var username = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrEmpty(username) || username == "SA")
    {
        Console.WriteLine("  Da huy.");
        return;
    }

    var user = UserData.GetUserByUsername(username, DefaultDbPath);
    if (user == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Tai khoan '{username}' khong ton tai.");
        Console.ResetColor();
        return;
    }

    var newPw = Guid.NewGuid().ToString("N")[..12];

    if (UserHelper.UpdatePassword(username, newPw, DefaultDbPath))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  [OK] Da reset mat khau cho '{username}'!");
        Console.ResetColor();
        Console.WriteLine($"  Mat khau moi: {newPw}");
        Console.WriteLine("  (Vui long doi mat khau sau khi dang nhap.)");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n  Loi: Khong the reset mat khau.");
        Console.ResetColor();
    }
}

static string ReadPassword()
{
    var sb = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }
        if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
        {
            sb.Length--;
            Console.Write("\b \b");
        }
        else if (key.Key != ConsoleKey.Backspace)
        {
            sb.Append(key.KeyChar);
            Console.Write("*");
        }
    }
    return sb.ToString();
}

static string HashPassword(string password, string salt)
{
    using var sha256 = SHA256.Create();
    byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
    byte[] hashBytes = sha256.ComputeHash(combinedBytes);
    return Convert.ToBase64String(hashBytes);
}

static void Pause()
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Nhan phim bat ky de tiep tuc...");
    Console.ResetColor();
    Console.ReadKey(true);
}
