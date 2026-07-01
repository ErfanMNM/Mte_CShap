using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GProject.Auth;

public static class AuthDb
{
    private static readonly string AuthFolder = @"C:\GProject\Auth";
    private static readonly string DbPath = @"C:\GProject\Auth\gauth.db";
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    public static string GetConnectionString() => ConnectionString;

    public static void EnsureCreated()
    {
        if (!Directory.Exists(AuthFolder))
            Directory.CreateDirectory(AuthFolder);

        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                DisplayName TEXT,
                Role TEXT NOT NULL,
                IsActive INTEGER DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                CreatedBy TEXT
            );

            CREATE TABLE IF NOT EXISTS LoginHistory (
                Id TEXT PRIMARY KEY,
                UserId TEXT,
                Username TEXT NOT NULL,
                LoginTime TEXT NOT NULL,
                LogoutTime TEXT,
                IpAddress TEXT,
                UserAgent TEXT,
                Success INTEGER DEFAULT 1,
                FailureReason TEXT
            );

            CREATE TABLE IF NOT EXISTS Sessions (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                Username TEXT NOT NULL,
                Role TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                IpAddress TEXT,
                UserAgent TEXT,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
        ";
        cmd.ExecuteNonQuery();

        // Seed default users if none exist
        cmd.CommandText = "SELECT COUNT(*) FROM Users;";
        var count = Convert.ToInt64(cmd.ExecuteScalar());

        if (count == 0)
        {
            SeedDefaultUsers(con);
        }
    }

    private static void SeedDefaultUsers(SqliteConnection con)
    {
        var users = new[]
        {
            ("sadmin", "SAdmin@123", "Super Administrator", "SAdmin"),
            ("admin", "Admin@123", "Administrator", "Administrator"),
            ("operator", "Operator@123", "Operator", "Operator"),
            ("viewer", "Viewer@123", "Viewer", "Viewer"),
        };

        foreach (var (username, password, displayName, role) in users)
        {
            var id = Guid.NewGuid().ToString();
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var now = DateTime.UtcNow.ToString("o");

            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (Id, Username, PasswordHash, DisplayName, Role, IsActive, CreatedAt, CreatedBy)
                VALUES (@id, @username, @hash, @displayName, @role, 1, @createdAt, @createdBy)
            ";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@displayName", displayName);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@createdAt", now);
            cmd.Parameters.AddWithValue("@createdBy", "system");
            cmd.ExecuteNonQuery();
        }
    }

    public static SqliteConnection GetConnection()
    {
        var con = new SqliteConnection(ConnectionString);
        con.Open();
        return con;
    }
}
