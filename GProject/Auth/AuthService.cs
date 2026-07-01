using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Serilog;

namespace GProject.Auth;

public class User
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    
    [JsonIgnore]
    public string PasswordHash { get; set; } = "";
    
    public string DisplayName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = "";
    public string? CreatedBy { get; set; }
}

public class LoginHistoryEntry
{
    public string Id { get; set; } = "";
    public string? UserId { get; set; }
    public string Username { get; set; } = "";
    public string LoginTime { get; set; } = "";
    public string? LogoutTime { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}

public class Session
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string ExpiresAt { get; set; } = "";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public static class AuthService
{
    private const int SessionDurationHours = 8;

    public static (User? user, string? sessionId) Login(string username, string password, string? ipAddress, string? userAgent)
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, PasswordHash, DisplayName, Role, IsActive, CreatedAt FROM Users WHERE Username = @username AND IsActive = 1";
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                LogLoginHistory(null, username, false, "User not found or inactive", ipAddress, userAgent);
                Log.Warning("[Auth] Login failed: user not found or inactive - {Username}", username);
                return (null, null);
            }

            var user = new User
            {
                Id = reader.GetString(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                DisplayName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Role = reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1,
                CreatedAt = reader.GetString(6)
            };
            reader.Close();

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                LogLoginHistory(user.Id, username, false, "Invalid password", ipAddress, userAgent);
                Log.Warning("[Auth] Login failed: invalid password - {Username}", username);
                return (null, null);
            }

            // Create session
            var sessionId = CreateSession(user, ipAddress, userAgent);
            LogLoginHistory(user.Id, username, true, null, ipAddress, userAgent);
            Log.Information("[Auth] User logged in: {Username} ({Role})", username, user.Role);

            return (user, sessionId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Login error for {Username}", username);
            return (null, null);
        }
    }

    public static void Logout(string sessionId)
    {
        try
        {
            using var con = AuthDb.GetConnection();

            // Get session info before deleting
            var getCmd = con.CreateCommand();
            getCmd.CommandText = "SELECT UserId, Username FROM Sessions WHERE Id = @id";
            getCmd.Parameters.AddWithValue("@id", sessionId);
            var reader = getCmd.ExecuteReader();

            if (reader.Read())
            {
                var userId = reader.GetString(0);
                var username = reader.GetString(1);
                reader.Close();

                // Update login history with logout time
                var updateCmd = con.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE LoginHistory
                    SET LogoutTime = @logoutTime
                    WHERE UserId = @userId AND LogoutTime IS NULL AND Success = 1
                    ORDER BY LoginTime DESC LIMIT 1";
                updateCmd.Parameters.AddWithValue("@logoutTime", DateTime.UtcNow.ToString("o"));
                updateCmd.Parameters.AddWithValue("@userId", userId);
                updateCmd.ExecuteNonQuery();

                Log.Information("[Auth] User logged out: {Username}", username);
            }
            else
            {
                reader.Close();
            }

            // Delete session
            var delCmd = con.CreateCommand();
            delCmd.CommandText = "DELETE FROM Sessions WHERE Id = @id";
            delCmd.Parameters.AddWithValue("@id", sessionId);
            delCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Logout error for session {SessionId}", sessionId);
        }
    }

    public static Session? ValidateSession(string sessionId)
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, UserId, Username, Role, CreatedAt, ExpiresAt, IpAddress, UserAgent
                FROM Sessions
                WHERE Id = @id AND ExpiresAt > @now
            ";
            cmd.Parameters.AddWithValue("@id", sessionId);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Session
            {
                Id = reader.GetString(0),
                UserId = reader.GetString(1),
                Username = reader.GetString(2),
                Role = reader.GetString(3),
                CreatedAt = reader.GetString(4),
                ExpiresAt = reader.GetString(5),
                IpAddress = reader.IsDBNull(6) ? null : reader.GetString(6),
                UserAgent = reader.IsDBNull(7) ? null : reader.GetString(7)
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Session validation error");
            return null;
        }
    }

    public static List<User> GetAllUsers()
    {
        var users = new List<User>();
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, DisplayName, Role, IsActive, CreatedAt, CreatedBy FROM Users ORDER BY Username";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetString(0),
                    Username = reader.GetString(1),
                    DisplayName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Role = reader.GetString(3),
                    IsActive = reader.GetInt32(4) == 1,
                    CreatedAt = reader.GetString(5),
                    CreatedBy = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error getting users");
        }
        return users;
    }

    public static bool CreateUser(string username, string password, string displayName, string role, string createdBy)
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (Id, Username, PasswordHash, DisplayName, Role, IsActive, CreatedAt, CreatedBy)
                VALUES (@id, @username, @hash, @displayName, @role, 1, @createdAt, @createdBy)
            ";
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(password));
            cmd.Parameters.AddWithValue("@displayName", displayName);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@createdBy", createdBy);
            cmd.ExecuteNonQuery();

            Log.Information("[Auth] User created: {Username} ({Role}) by {CreatedBy}", username, role, createdBy);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error creating user {Username}", username);
            return false;
        }
    }

    public static bool UpdateUser(string id, string? password, string? displayName, string? role, bool? isActive)
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var parts = new List<string>();
            var cmd = con.CreateCommand();

            if (password != null)
            {
                parts.Add("PasswordHash = @hash");
                cmd.Parameters.AddWithValue("@hash", BCrypt.Net.BCrypt.HashPassword(password));
            }
            if (displayName != null)
            {
                parts.Add("DisplayName = @displayName");
                cmd.Parameters.AddWithValue("@displayName", displayName);
            }
            if (role != null)
            {
                parts.Add("Role = @role");
                cmd.Parameters.AddWithValue("@role", role);
            }
            if (isActive.HasValue)
            {
                parts.Add("IsActive = @isActive");
                cmd.Parameters.AddWithValue("@isActive", isActive.Value ? 1 : 0);
            }

            if (parts.Count == 0) return true;

            cmd.CommandText = $"UPDATE Users SET {string.Join(", ", parts)} WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            Log.Information("[Auth] User updated: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error updating user {Id}", id);
            return false;
        }
    }

    public static bool DeleteUser(string id)
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Users WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            Log.Information("[Auth] User deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error deleting user {Id}", id);
            return false;
        }
    }

    public static List<LoginHistoryEntry> GetLoginHistory(int limit = 100)
    {
        var history = new List<LoginHistoryEntry>();
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = $"SELECT Id, UserId, Username, LoginTime, LogoutTime, IpAddress, UserAgent, Success, FailureReason FROM LoginHistory ORDER BY LoginTime DESC LIMIT {limit}";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new LoginHistoryEntry
                {
                    Id = reader.GetString(0),
                    UserId = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Username = reader.GetString(2),
                    LoginTime = reader.GetString(3),
                    LogoutTime = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IpAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
                    UserAgent = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Success = reader.GetInt32(7) == 1,
                    FailureReason = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error getting login history");
        }
        return history;
    }

    public static void CleanupExpiredSessions()
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Sessions WHERE ExpiresAt < @now";
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
            var deleted = cmd.ExecuteNonQuery();
            if (deleted > 0) Log.Information("[Auth] Cleaned up {Count} expired sessions", deleted);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error cleaning up sessions");
        }
    }

    private static string CreateSession(User user, string? ipAddress, string? userAgent)
    {
        var sessionId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(SessionDurationHours);

        using var con = AuthDb.GetConnection();
        var cmd = con.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Sessions (Id, UserId, Username, Role, CreatedAt, ExpiresAt, IpAddress, UserAgent)
            VALUES (@id, @userId, @username, @role, @createdAt, @expiresAt, @ipAddress, @userAgent)
        ";
        cmd.Parameters.AddWithValue("@id", sessionId);
        cmd.Parameters.AddWithValue("@userId", user.Id);
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@role", user.Role);
        cmd.Parameters.AddWithValue("@createdAt", now.ToString("o"));
        cmd.Parameters.AddWithValue("@expiresAt", expiresAt.ToString("o"));
        cmd.Parameters.AddWithValue("@ipAddress", ipAddress ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@userAgent", userAgent ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();

        return sessionId;
    }

    private static void LogLoginHistory(string? userId, string username, bool success, string? failureReason, string? ipAddress, string? userAgent)
    {
        try
        {
            using var con = AuthDb.GetConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO LoginHistory (Id, UserId, Username, LoginTime, IpAddress, UserAgent, Success, FailureReason)
                VALUES (@id, @userId, @username, @loginTime, @ipAddress, @userAgent, @success, @failureReason)
            ";
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@userId", userId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@loginTime", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@ipAddress", ipAddress ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@userAgent", userAgent ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@success", success ? 1 : 0);
            cmd.Parameters.AddWithValue("@failureReason", failureReason ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Auth] Error logging login history");
        }
    }

    public static bool HasRole(string userRole, params string[] allowedRoles)
    {
        foreach (var role in allowedRoles)
        {
            if (string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
