namespace TSo.Models;

public enum e_Production_Status
{
    Pass = 0,
    Fail = 1,
    Error = 2,
    Duplicate = 3,
    NotFound = 4,
    Timeout = 5,
    ReadFail = 6,
    FormatError = 7,
    Deactive = 8
}

public class QRProductRecord
{
    public int Id { get; set; }
    public string QRContent { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public e_Production_Status Status { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string TimeStampActive { get; set; } = string.Empty;
    public long TimeUnixActive { get; set; }
    public string ProductionDatetime { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class QRValidationResult
{
    public bool Success { get; set; }
    public e_Production_Status Status { get; set; }
    public string QRContent { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? RecordId { get; set; }
}

public class BatchInfo
{
    public string BatchCode { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class ProductionCounters
{
    public int Total { get; set; }
    public int Pass { get; set; }
    public int Fail => Duplicate + ReadFail + NotFound + Timeout + Error + FormatError;
    public int Duplicate { get; set; }
    public int ReadFail { get; set; }
    public int NotFound { get; set; }
    public int Timeout { get; set; }
    public int Error { get; set; }
    public int FormatError { get; set; }
}

public class PLCCounters
{
    public int Total { get; set; }
    public int Pass { get; set; }
    public int ReadFail { get; set; }
    public int Timeout { get; set; }
    public int Fail => ReadFail + Timeout;
}

public class DeviceStatus
{
    public string PLCStatus { get; set; } = "Disconnected";
    public string CameraStatus { get; set; } = "Disconnected";
    public string ScannerStatus { get; set; } = "Disconnected";
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}

public class SystemStatus
{
    public string State { get; set; } = "Unknown";
    public bool IsDeactive { get; set; }
    public bool IsAuthenticated { get; set; }
    public string CurrentUser { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
