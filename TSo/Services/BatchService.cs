using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using TSo.Configs;
using TSo.Database;
using TSo.Models;

namespace TSo.Services;

public class BatchService
{
    private readonly Logger _logger;
    private readonly AppConfigs _config;

    public BatchService(Logger logger, AppConfigs config)
    {
        _logger = logger;
        _config = config;
    }

    public BatchInfo? GetCurrentBatch()
    {
        var last = BatchHistoryHelper.GetLatest(AppConfigs.BatchHistoryDbPath);
        if (last != null)
        {
            GlobalState.CurrentBatch = new BatchInfo
            {
                BatchCode = last.BatchCode,
                Barcode = last.Barcode,
                LineName = _config.Line_Name ?? "Unknown"
            };
            return GlobalState.CurrentBatch;
        }

        GlobalState.CurrentBatch = new BatchInfo
        {
            BatchCode = "NNN",
            Barcode = "000",
            LineName = _config.Line_Name ?? "Unknown"
        };
        return null;
    }

    public BatchInfo ChangeBatch(string batchCode, string barcode, string username)
    {
        var batch = new BatchInfo
        {
            BatchCode = batchCode,
            Barcode = barcode,
            LineName = _config.Line_Name ?? "Unknown",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = username
        };

        BatchHistoryHelper.AddHistory(
            AppConfigs.BatchHistoryDbPath,
            batchCode,
            barcode,
            username,
            DateTime.UtcNow
        );

        GlobalState.CurrentBatch = batch;
        GlobalState.CameraCounters = new ProductionCounters();

        _logger.Log(username, LogType.UserAction, $"Batch changed",
            $"{{'BatchCode':'{batchCode}','Barcode':'{barcode}'}}", "UA-BATCH-01");

        return batch;
    }

    public BatchListResponse GetBatchListFromExcel()
    {
        var response = new BatchListResponse();

        if (string.IsNullOrEmpty(_config.production_list_path) || !File.Exists(_config.production_list_path))
        {
            _logger.Log("System", LogType.Warning, "Production list Excel file not found",
                $"{{'Path':'{_config.production_list_path}'}}");
            return response;
        }

        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var stream = File.Open(_config.production_list_path!, FileMode.Open, FileAccess.Read);
            using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);

            var headers = new List<string>();
            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    headers.Add(reader.GetValue(i)?.ToString() ?? "");
            }

            var itemCodeIdx = headers.FindIndex(h => h.Contains("Item code", StringComparison.OrdinalIgnoreCase));
            var barcodeIdx = headers.FindIndex(h => h.Contains("Barcode", StringComparison.OrdinalIgnoreCase));

            while (reader.Read())
            {
                var itemCode = itemCodeIdx >= 0 ? reader.GetValue(itemCodeIdx)?.ToString()?.Trim() : null;
                var barcode = barcodeIdx >= 0 ? reader.GetValue(barcodeIdx)?.ToString()?.Trim() : null;

                if (!string.IsNullOrEmpty(itemCode))
                {
                    response.Items.Add(new BatchListItem
                    {
                        BatchCode = itemCode,
                        Barcode = barcode ?? "000"
                    });
                }
            }

            _logger.Log("System", LogType.Info, $"Loaded {response.Items.Count} batches from Excel");
        }
        catch (Exception ex)
        {
            _logger.Log("System", LogType.Error, "Failed to load batch list from Excel", ex.Message);
        }

        return response;
    }

    public string? GetBarcodeFromExcel(string batchCode)
    {
        if (string.IsNullOrEmpty(_config.production_list_path) || !File.Exists(_config.production_list_path))
            return null;

        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var shortCode = batchCode.Split("-")[0];

            using var stream = File.Open(_config.production_list_path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);

            var headers = new List<string>();
            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    headers.Add(reader.GetValue(i)?.ToString() ?? "");
            }

            var itemCodeIdx = headers.FindIndex(h => h.Contains("Item code", StringComparison.OrdinalIgnoreCase));
            var barcodeIdx = headers.FindIndex(h => h.Contains("Barcode", StringComparison.OrdinalIgnoreCase));

            while (reader.Read())
            {
                var itemCode = itemCodeIdx >= 0 ? reader.GetValue(itemCodeIdx)?.ToString()?.Trim() : null;
                if (itemCode == shortCode)
                {
                    var barcode = barcodeIdx >= 0 ? reader.GetValue(barcodeIdx)?.ToString()?.Trim() : null;
                    return barcode ?? "000";
                }
            }
        }
        catch { }

        return null;
    }

    public bool ValidateBatchCode(string batchCode)
    {
        return IsDateValid(batchCode) && IsLineValid(batchCode) && HasThreeDashes(batchCode);
    }

    private static bool IsDateValid(string batchId)
    {
        return Regex.IsMatch(batchId, @"\d{6}");
    }

    private static bool IsLineValid(string batchId)
    {
        var lineNum = Regex.Match(batchId, @"TOL\d+").Value;
        return !string.IsNullOrEmpty(lineNum);
    }

    private static bool HasThreeDashes(string batchId)
    {
        return batchId.Split('-').Length - 1 >= 3;
    }

    public List<BatchInfo> GetBatchHistory(int limit = 50)
    {
        return BatchHistoryHelper.GetHistory(AppConfigs.BatchHistoryDbPath, limit);
    }
}
