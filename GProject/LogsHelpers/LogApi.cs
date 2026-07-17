using System.Globalization;
using System.Text.Json;
using GProject.Auth;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace GProject.Logs;

/// <summary>
/// HTTP handlers that expose the SQLite log store to FE.
/// All endpoints require SAdmin role (checked via "view_logs" permission).
/// </summary>
public static class LogApi
{
    private const string Permission = "view_logs";

    public static async Task HandleList(HttpContext context)
    {
        if (!RequirePermission(context, out var failure)) { await failure!(context.Response); return; }

        try
        {
            var q = BuildQuery(context.Request.Query);
            var page = LogRepository.Query(q);
            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = "OK",
                data = new
                {
                    items = page.Items,
                    page = page.Page,
                    pageSize = page.PageSize,
                    totalCount = page.TotalCount,
                    totalPages = page.TotalPages,
                    hasNext = page.HasNext,
                    hasPrev = page.HasPrev,
                },
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[LogApi] List failed");
            await WriteError(context, ex);
        }
    }

    public static async Task HandleGet(HttpContext context, long id)
    {
        if (!RequirePermission(context, out var failure)) { await failure!(context.Response); return; }

        try
        {
            var entry = LogRepository.GetById(id);
            if (entry == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new { success = false, message = "Log entry not found." });
                return;
            }
            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = "OK",
                data = entry,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[LogApi] GetById {Id} failed", id);
            await WriteError(context, ex);
        }
    }

    public static async Task HandleListLevels(HttpContext context)
    {
        if (!RequirePermission(context, out var failure)) { await failure!(context.Response); return; }

        try
        {
            var levels = LogRepository.ListDistinctLevels();
            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = "OK",
                data = levels,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[LogApi] ListLevels failed");
            await WriteError(context, ex);
        }
    }

    public static async Task HandleListTags(HttpContext context)
    {
        if (!RequirePermission(context, out var failure)) { await failure!(context.Response); return; }

        try
        {
            var tags = LogRepository.ListDistinctTags();
            await context.Response.WriteAsJsonAsync(new
            {
                success = true,
                message = "OK",
                data = tags,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[LogApi] ListTags failed");
            await WriteError(context, ex);
        }
    }

    private static LogQuery BuildQuery(IQueryCollection qs)
    {
        DateTime? from = ParseIso(qs["from"].ToString());
        DateTime? to = ParseIso(qs["to"].ToString());

        var level = qs["level"].ToString();
        if (string.IsNullOrWhiteSpace(level)) level = null;

        var tag = qs["tag"].ToString();
        if (string.IsNullOrWhiteSpace(tag)) tag = null;

        var q = qs["q"].ToString();
        if (string.IsNullOrWhiteSpace(q)) q = null;

        int page = 1, pageSize = 50;
        int.TryParse(qs["page"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out page);
        int.TryParse(qs["pageSize"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out pageSize);

        var sort = qs["sort"].ToString();
        if (string.IsNullOrWhiteSpace(sort)) sort = "desc";

        return new LogQuery
        {
            From = from,
            To = to,
            LevelName = level,
            TagContains = tag,
            MessageContains = q,
            Page = page,
            PageSize = pageSize,
            Sort = sort,
        };
    }

    private static DateTime? ParseIso(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            return dt.ToUniversalTime();
        return null;
    }

    private static bool RequirePermission(HttpContext context, out Func<HttpResponse, Task>? failure)
    {
        var user = AuthMiddleware.GetCurrentUser(context);
        if (user == null)
        {
            failure = async (r) =>
            {
                r.StatusCode = 401;
                await r.WriteAsJsonAsync(new { success = false, message = "Unauthorized" });
            };
            return false;
        }
        if (!AuthHelper.HasPermission(user.Role, Permission))
        {
            failure = async (r) =>
            {
                r.StatusCode = 403;
                await r.WriteAsJsonAsync(new { success = false, message = "Forbidden: requires SAdmin role." });
            };
            return false;
        }
        failure = null;
        return true;
    }

    private static async Task WriteError(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = ex.Message });
    }
}
