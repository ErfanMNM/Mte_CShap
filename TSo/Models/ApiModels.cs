namespace TSo.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "OK")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(string message = "OK")
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message)
        => new() { Success = false, Message = message };
}

public class QRActivateRequest
{
    public string QRContent { get; set; } = string.Empty;
}

public class QRActivateResponse
{
    public bool Success { get; set; }
    public e_Production_Status Status { get; set; }
    public string QRContent { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? RecordId { get; set; }
}

public class BatchChangeRequest
{
    public string BatchCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
}

public class BatchListResponse
{
    public List<BatchListItem> Items { get; set; } = new();
}

public class BatchListItem
{
    public string BatchCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
}

public class DashboardResponse
{
    public BatchInfo CurrentBatch { get; set; } = new();
    public ProductionCounters CameraCounters { get; set; } = new();
    public PLCCounters PLCCounters { get; set; } = new();
    public DeviceStatus DeviceStatus { get; set; } = new();
    public SystemStatus SystemStatus { get; set; } = new();
    public int ProductionPerHour { get; set; }
    public int ActiveCodesTotal { get; set; }
}

public class QRSearchResponse
{
    public List<QRProductRecord> Records { get; set; } = new();
    public bool IsActive { get; set; }
    public int TotalCount { get; set; }
}

public class ActivityLog
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string LogType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string? Code { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ActivityLogResponse
{
    public List<ActivityLog> Logs { get; set; } = new();
    public int TotalCount { get; set; }
}
