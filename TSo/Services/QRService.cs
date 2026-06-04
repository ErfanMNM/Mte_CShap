using TSo.Database;
using TSo.Models;
using TSo.Configs;

namespace TSo.Services;

public class QRService
{
    private readonly Logger _logger;

    public event Action<QRProductRecord>? OnQRActivated;

    public QRService(Logger logger)
    {
        _logger = logger;
    }

    public QRValidationResult ProcessCameraQR(string rawData, string? username = null)
    {
        rawData = rawData.Trim().Replace("\r", "").Replace("\n", "");

        if (string.Equals(rawData, "FAIL", StringComparison.OrdinalIgnoreCase))
        {
            var result = new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.ReadFail,
                QRContent = rawData,
                Reason = "Camera read failure"
            };
            IncrementCounter(e_Production_Status.ReadFail);
            return result;
        }

        if (GlobalState.ActiveSet.Contains(rawData))
        {
            var result = new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.Duplicate,
                QRContent = rawData,
                Reason = "Duplicate QR code"
            };
            IncrementCounter(e_Production_Status.Duplicate);
            return result;
        }

        if (!IsValidQRContent(rawData))
        {
            var result = new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.FormatError,
                QRContent = rawData,
                Reason = "Invalid QR format"
            };
            IncrementCounter(e_Production_Status.FormatError);
            return result;
        }

        GlobalState.ActiveSet.Add(rawData);
        IncrementCounter(e_Production_Status.Pass);

        var record = CreateRecord(rawData, e_Production_Status.Pass, username ?? "Camera");
        var id = QRDatabaseHelper.AddOrActivateCode(record, AppConfigs.QRDatadbPath);
        record.Id = id;

        GlobalState.QueueActive.Enqueue(record);

        _logger.Log(username ?? "Camera", LogType.DeviceAction, $"QR activated: {rawData}",
            $"{{'Status':'Pass','BatchCode':'{record.BatchCode}'}}", "UA-QR-01");

        OnQRActivated?.Invoke(record);

        return new QRValidationResult
        {
            Success = true,
            Status = e_Production_Status.Pass,
            QRContent = rawData,
            Reason = "Activated",
            RecordId = id
        };
    }

    public QRValidationResult ProcessManualAdd(string qrContent, string username)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
        {
            return new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.Error,
                Reason = "Empty QR content"
            };
        }

        qrContent = qrContent.Trim();

        if (GlobalState.ActiveSet.Contains(qrContent))
        {
            return new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.Duplicate,
                QRContent = qrContent,
                Reason = "QR code already exists"
            };
        }

        if (qrContent.Length < 16)
        {
            return new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.FormatError,
                QRContent = qrContent,
                Reason = "QR code too short (min 16 chars)"
            };
        }

        if (!qrContent.Contains(GlobalState.CurrentBatch.Barcode))
        {
            return new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.FormatError,
                QRContent = qrContent,
                Reason = "QR code does not contain current barcode"
            };
        }

        if (string.IsNullOrWhiteSpace(GlobalState.CurrentBatch.BatchCode) ||
            GlobalState.CurrentBatch.BatchCode == "NNN")
        {
            return new QRValidationResult
            {
                Success = false,
                Status = e_Production_Status.Error,
                QRContent = qrContent,
                Reason = "No batch selected"
            };
        }

        GlobalState.ActiveSet.Add(qrContent);

        var record = CreateRecord(qrContent, e_Production_Status.Pass, $"{username} (Manual)");
        record.Reason = "Manual Add";
        var id = QRDatabaseHelper.AddOrActivateCode(record, AppConfigs.QRDatadbPath);
        record.Id = id;

        GlobalState.QueueActive.Enqueue(record);
        IncrementCounter(e_Production_Status.Pass);

        _logger.Log(username, LogType.UserAction, $"Manual QR add: {qrContent}",
            $"{{'BatchCode':'{record.BatchCode}'}}", "UA-QR-MANUAL-01");

        OnQRActivated?.Invoke(record);

        return new QRValidationResult
        {
            Success = true,
            Status = e_Production_Status.Pass,
            QRContent = qrContent,
            Reason = "Manually added",
            RecordId = id
        };
    }

    public QRSearchResponse Search(string qrContent)
    {
        var records = QRDatabaseHelper.GetByQRContent(qrContent);
        var activeRecords = QRDatabaseHelper.GetActiveByQRContent(qrContent);

        return new QRSearchResponse
        {
            Records = records,
            IsActive = activeRecords.Count > 0,
            TotalCount = records.Count
        };
    }

    public bool DeactivateQR(string qrContent, string reason, string username)
    {
        var success = QRDatabaseHelper.DeactivateCode(qrContent, reason, username);
        if (success)
        {
            GlobalState.ActiveSet.Remove(qrContent);
            _logger.Log(username, LogType.UserAction, $"QR deactivated: {qrContent}",
                $"{{'Reason':'{reason}'}}", "UA-QR-DEACTIVATE-01");
        }
        return success;
    }

    public void ReloadActiveSet()
    {
        var set = QRDatabaseHelper.LoadActiveToHashSet();
        GlobalState.ActiveSet.Clear();
        foreach (var item in set)
            GlobalState.ActiveSet.Add(item);
    }

    private bool IsValidQRContent(string data)
    {
        if (data.Length < 16) return false;
        if (string.IsNullOrEmpty(GlobalState.CurrentBatch.Barcode)) return true;
        return data.Contains(GlobalState.CurrentBatch.Barcode);
    }

    private QRProductRecord CreateRecord(string qrContent, e_Production_Status status, string username)
    {
        var now = DateTime.UtcNow;
        return new QRProductRecord
        {
            QRContent = qrContent,
            Status = status,
            BatchCode = GlobalState.CurrentBatch.BatchCode,
            Barcode = GlobalState.CurrentBatch.Barcode,
            UserName = username,
            TimeStampActive = now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            TimeUnixActive = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
            ProductionDatetime = now.ToString("yyyy-MM-dd HH:mm:ss"),
            Reason = ""
        };
    }

    private void IncrementCounter(e_Production_Status status)
    {
        var counters = GlobalState.CameraCounters;
        counters.Total++;

        switch (status)
        {
            case e_Production_Status.Pass: counters.Pass++; break;
            case e_Production_Status.Duplicate: counters.Duplicate++; break;
            case e_Production_Status.ReadFail: counters.ReadFail++; break;
            case e_Production_Status.Timeout: counters.Timeout++; break;
            case e_Production_Status.Error: counters.Error++; break;
            case e_Production_Status.FormatError: counters.FormatError++; break;
        }
    }
}
