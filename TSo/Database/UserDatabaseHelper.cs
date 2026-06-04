using System.Data.SQLite;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TSo.Models;

namespace TSo.Database;

public static class UserDatabaseHelper
{
    private const string CreateTableSql = @"
        CREATE TABLE IF NOT EXISTS users (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            Password TEXT NOT NULL,
            Salt TEXT NOT NULL,
            Role TEXT NOT NULL DEFAULT 'Operator',
            Key2FA TEXT,
            CreatedAt TEXT NOT NULL
        );
        PRAGMA journal_mode=WAL;
    ";

    public static void Init(string dbPath)
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();
        using var cmd = new SQLiteCommand(CreateTableSql, con);
        cmd.ExecuteNonQuery();
    }

    public static UserData? GetUserByUsername(string username, string dbPath)
    {
        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = "SELECT ID, Username, Password, Salt, Role, Key2FA FROM users WHERE Username = @username LIMIT 1";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@username", username);

        using var rd = cmd.ExecuteReader();
        if (rd.Read())
        {
            return new UserData
            {
                Username = rd.GetString(1),
                Password = rd.GetString(2),
                Salt = rd.GetString(3),
                Role = rd.GetString(4),
                Key2FA = rd.IsDBNull(5) ? null : rd.GetString(5)
            };
        }

        return null;
    }

    public static bool ValidateCredentials(string username, string password, string dbPath)
    {
        var user = GetUserByUsername(username, dbPath);
        if (user == null || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Salt))
            return false;

        var hashed = HashPassword(password, user.Salt!);
        return user.Password == hashed;
    }

    public static bool Validate2FA(string username, string code, string dbPath, int window = 1)
    {
        var user = GetUserByUsername(username, dbPath);
        if (user == null || string.IsNullOrEmpty(user.Key2FA))
            return false;

        return TwoFAVerifier.VerifyOTP(user.Key2FA, code, 6, 30, window);
    }

    public static bool AddUser(string username, string password, string role, string dbPath)
    {
        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var salt = Guid.NewGuid().ToString();
        var hashed = HashPassword(password, salt);
        var secret2FA = TwoFAGenerator.GenerateSecret();
        var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        var sql = @"INSERT INTO users (Username, Password, Salt, Role, Key2FA, CreatedAt)
                    VALUES (@username, @password, @salt, @role, @key2fa, @createdAt)";

        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@password", hashed);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@role", role);
        cmd.Parameters.AddWithValue("@key2fa", secret2FA);
        cmd.Parameters.AddWithValue("@createdAt", createdAt);

        try
        {
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (SQLiteException)
        {
            return false;
        }
    }

    public static bool UpdatePassword(string username, string newPassword, string dbPath)
    {
        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var salt = Guid.NewGuid().ToString();
        var hashed = HashPassword(newPassword, salt);

        var sql = "UPDATE users SET Password = @password, Salt = @salt WHERE Username = @username";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@password", hashed);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@username", username);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool Set2FAKey(string username, string key2FA, string dbPath)
    {
        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = "UPDATE users SET Key2FA = @key2fa WHERE Username = @username";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@key2fa", key2FA);
        cmd.Parameters.AddWithValue("@username", username);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static string? Get2FAKey(string username, string dbPath)
    {
        var user = GetUserByUsername(username, dbPath);
        return user?.Key2FA;
    }

    public static List<string> GetUserList(string dbPath)
    {
        var users = new List<string>();
        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        using var cmd = new SQLiteCommand("SELECT Username FROM users", con);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            users.Add(rd.GetString(0));
        }

        return users;
    }

    public static bool DeleteUser(string username, string dbPath)
    {
        using var con = new SQLiteConnection($"Data Source={dbPath}");
        con.Open();

        var sql = "DELETE FROM users WHERE Username = @username";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@username", username);

        return cmd.ExecuteNonQuery() > 0;
    }

    private static string HashPassword(string password, string salt)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + salt);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public static class TwoFAGenerator
{
    public static string GenerateSecret(int length = 20)
    {
        var secret = new byte[length];
        Random.Shared.NextBytes(secret);
        return Base32Encode(secret);
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var sb = new StringBuilder();
        int buffer = 0, bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                bitsLeft -= 5;
                sb.Append(alphabet[(buffer >> bitsLeft) & 31]);
            }
        }

        if (bitsLeft > 0)
            sb.Append(alphabet[(buffer << (5 - bitsLeft)) & 31]);

        return sb.ToString();
    }
}

public static class TwoFAVerifier
{
    public static bool VerifyOTP(string secret, string code, int digits = 6, int period = 30, int window = 1)
    {
        try
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
                return false;

            code = code.Trim().Replace(" ", "");
            if (code.Length != digits || !code.All(char.IsDigit))
                return false;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            for (int i = -window; i <= window; i++)
            {
                var expectedCode = GetHotp(secret, (currentTime / period) + i, digits);
                if (string.Equals(code, expectedCode, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string GetHotp(string secret, long counter, int digits)
    {
        var key = Base32Decode(secret);
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        using var hmac = new System.Security.Cryptography.HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[hash.Length - 1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                  | ((hash[offset + 1] & 0xFF) << 16)
                  | ((hash[offset + 2] & 0xFF) << 8)
                  | (hash[offset + 3] & 0xFF);

        var otp = binary % (int)Math.Pow(10, digits);
        return otp.ToString().PadLeft(digits, '0');
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        input = input.ToUpperInvariant().Replace(" ", "").Replace("-", "");
        var output = new List<byte>();
        int buffer = 0, bitsLeft = 0;

        foreach (var c in input)
        {
            if (c == '=') continue;
            var value = alphabet.IndexOf(c);
            if (value < 0) continue;

            buffer = (buffer << 5) | value;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                output.Add((byte)(buffer >> bitsLeft));
            }
        }

        return output.ToArray();
    }
}
