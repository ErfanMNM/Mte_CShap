using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace GProject.Auth;

public class CurrentUser
{
    public string SessionId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}

public static class AuthMiddleware
{
    public static readonly string SessionCookieName = "gauth_session";
    public static readonly string UserContextKey = "CurrentUser";

    public static CurrentUser? GetCurrentUser(HttpContext context)
    {
        if (context.Items.TryGetValue(UserContextKey, out var user) && user is CurrentUser currentUser)
            return currentUser;
        return null;
    }

    public static void SetCurrentUser(HttpContext context, CurrentUser user)
    {
        context.Items[UserContextKey] = user;
    }

    public static async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip auth for certain paths
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/api/auth/login") || path == "/api/health" || path.StartsWith("/ws/"))
        {
            await next(context);
            return;
        }

        // Check session cookie
        if (context.Request.Cookies.TryGetValue(SessionCookieName, out var sessionId) && !string.IsNullOrEmpty(sessionId))
        {
            var session = AuthService.ValidateSession(sessionId);
            if (session != null)
            {
                var currentUser = new CurrentUser
                {
                    SessionId = session.Id,
                    UserId = session.UserId,
                    Username = session.Username,
                    Role = session.Role,
                    ExpiresAt = DateTime.Parse(session.ExpiresAt)
                };
                SetCurrentUser(context, currentUser);
            }
        }

        await next(context);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    public string[] Roles { get; }

    public AuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public bool IsAuthorized(CurrentUser? user)
    {
        if (user == null) return false;
        if (Roles.Length == 0) return true;
        return AuthService.HasRole(user.Role, Roles);
    }
}

public static class AuthHelper
{
    public static bool HasPermission(string role, string permission)
    {
        var permissions = GetRolePermissions(role);
        return permissions.Contains(permission);
    }

    public static string[] GetRolePermissions(string role)
    {
        return role.ToLower() switch
        {
            "sadmin" => new[] { "view", "add", "edit", "import", "manage_pools", "manage_users", "view_history" },
            "administrator" => new[] { "view", "add", "edit", "import", "manage_pools", "view_history" },
            "operator" => new[] { "view", "add", "edit", "import" },
            "viewer" => new[] { "view" },
            _ => Array.Empty<string>()
        };
    }

    public static IResult Unauthorized(string message = "Unauthorized")
    {
        return Results.Json(new { success = false, message }, statusCode: 401);
    }

    public static IResult Forbidden(string message = "Forbidden")
    {
        return Results.Json(new { success = false, message }, statusCode: 403);
    }
}
