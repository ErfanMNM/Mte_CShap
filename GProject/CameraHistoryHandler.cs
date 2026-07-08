using Microsoft.AspNetCore.Http;

namespace GProject;

/// <summary>
/// REST handler cho /api/camera-history — trả về ring buffer các mã camera đã quét gần nhất.
/// Đăng ký qua GProjectApiServer.StartAsync với _app.MapGet("/api/camera-history", ...).
/// </summary>
public static class CameraHistoryHandler
{
    public static IResult HandleGet(HttpContext context)
    {
        try
        {
            int limit = 200;
            if (context.Request.Query.TryGetValue("limit", out var limitVal) && int.TryParse(limitVal.ToString(), out int l))
                limit = l;
            if (limit <= 0) limit = 200;
            if (limit > 1000) limit = 1000;

            var items = CameraHub.Instance.GetHistory(limit);
            return Results.Json(new
            {
                success = true,
                count = items.Count,
                items
            });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "[CameraHistory] Error getting history");
            return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
        }
    }
}
