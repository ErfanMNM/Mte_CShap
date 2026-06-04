using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using TSo.Database;
using TSo.Models;
using TSo.Configs;

namespace TSo.Services;

public class AuthService
{
    private readonly Logger _logger;
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private readonly TimeSpan _sessionDuration = TimeSpan.FromHours(8);

    public AuthService(Logger logger)
    {
        _logger = logger;
    }

    public LoginResponse Login(LoginRequest request)
    {
        var dbFile = AppConfigs.UsersDbPath;

        if (!File.Exists(dbFile))
        {
            UserDatabaseHelper.Init(dbFile);
            if (UserDatabaseHelper.AddUser("admin", "admin123", "Admin", dbFile))
            {
                _logger.Log("System", LogType.System, "Created default admin user");
            }
        }

        var user = UserDatabaseHelper.GetUserByUsername(request.Username, dbFile);
        if (user == null)
        {
            _logger.Log(request.Username, LogType.Error, "Login failed - user not found");
            return new LoginResponse { Success = false, Message = "Invalid username or password" };
        }

        var isValid = UserDatabaseHelper.ValidateCredentials(request.Username, request.Password, dbFile);
        if (!isValid)
        {
            _logger.Log(request.Username, LogType.Error, "Login failed - invalid password");
            return new LoginResponse { Success = false, Message = "Invalid username or password" };
        }

        if (GlobalState.Config.AppTwoFA_Enabled && !string.IsNullOrEmpty(user.Key2FA))
        {
            if (string.IsNullOrEmpty(request.TwoFACode))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "2FA code required",
                    Username = user.Username,
                    Role = user.Role
                };
            }

            var is2FAValid = UserDatabaseHelper.Validate2FA(request.Username, request.TwoFACode, dbFile);
            if (!is2FAValid)
            {
                _logger.Log(request.Username, LogType.Error, "Login failed - invalid 2FA code");
                return new LoginResponse { Success = false, Message = "Invalid 2FA code" };
            }
        }

        var session = CreateSession(user.Username, user.Role ?? "Operator");
        GlobalState.Sessions[session.Token] = session;
        GlobalState.CurrentSession = session;
        GlobalState.IsAuthenticated = true;

        _logger.Log(request.Username, LogType.UserAction, "Login successful",
            $"{{'Role':'{user.Role}'}}", "UA-LOGIN-01");

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            Token = session.Token,
            Username = user.Username,
            Role = user.Role
        };
    }

    public bool Logout(string token)
    {
        if (GlobalState.Sessions.TryRemove(token, out var session))
        {
            _logger.Log(session.Username, LogType.UserAction, "Logout successful");
            GlobalState.IsAuthenticated = false;
            GlobalState.CurrentSession = null;
            return true;
        }
        return false;
    }

    public SessionInfo? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        if (GlobalState.Sessions.TryGetValue(token, out var session))
        {
            if (session.ExpiresAt > DateTime.UtcNow)
            {
                return session;
            }
            GlobalState.Sessions.TryRemove(token, out _);
        }

        return null;
    }

    public string? Get2FAQrUrl(string username)
    {
        var usersDir = Path.GetDirectoryName(AppConfigs.UsersDbPath)!;
        var dbFile = Path.Combine(usersDir, "users.database");

        var secret = UserDatabaseHelper.Get2FAKey(username, dbFile);
        if (string.IsNullOrEmpty(secret))
        {
            secret = TwoFAGenerator.GenerateSecret();
            UserDatabaseHelper.Set2FAKey(username, secret, dbFile);
        }

        return $"otpauth://totp/TSo:{username}?secret={secret}&issuer=TSo&digits=6&period=30";
    }

    private SessionInfo CreateSession(string username, string role)
    {
        var token = GenerateToken();
        return new SessionInfo
        {
            Token = token,
            Username = username,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_sessionDuration)
        };
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public List<SessionInfo> GetActiveSessions()
    {
        return GlobalState.Sessions.Values
            .Where(s => s.ExpiresAt > DateTime.UtcNow)
            .ToList();
    }
}
