namespace TSo.Models;

public class UserData
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Salt { get; set; }
    public string? Role { get; set; }
    public string? Key2FA { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TwoFACode { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public string? TwoFAQrUrl { get; set; }
}

public class SessionInfo
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
