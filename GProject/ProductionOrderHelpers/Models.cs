using System.Data;

namespace GProject.ProductionOrderHelpers
{
    /// <summary>
    /// Kết quả trả về từ các hàm xử lý
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public DataTable? Data { get; set; }
        public int Count { get; set; }

        public Result() { }

        public Result(bool isSuccess, string message, int count = 0, DataTable? data = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Count = count;
        }

        public static Result Success(string message = "Thành công.", int count = 0, DataTable? data = null)
            => new(true, message, count, data);

        public static Result Fail(string message)
            => new(false, message);

        public static Result FromDataTable(DataTable? table, string successMsg = "Thành công.", string failMsg = "Không có dữ liệu.")
            => table != null && table.Rows.Count > 0
                ? Success(successMsg, table.Rows.Count, table)
                : Fail(failMsg);
    }

    /// <summary>
    /// Thông tin Production Order
    /// </summary>
    public class POInfo
    {
        public string OrderNo { get; set; } = "";
        public string Site { get; set; } = "";
        public string Factory { get; set; } = "";
        public string ProductionLine { get; set; } = "";
        public string ProductionDate { get; set; } = "";
        public string Shift { get; set; } = "";
        public int OrderQty { get; set; }
        public int CartonCapacity { get; set; } = 24;
        public string LotNumber { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string Gtin { get; set; } = "";
        public string CustomerOrderNo { get; set; } = "";
        public string Uom { get; set; } = "PCS";
        public string CreatedTime { get; set; } = "";
        public string ModifiedTime { get; set; } = "";

        /// <summary>
        /// Tạo POInfo từ DataRow
        /// </summary>
        public static POInfo FromDataRow(DataRow row)
        {
            return new POInfo
            {
                OrderNo = row["orderNo"]?.ToString() ?? "",
                Site = row["site"]?.ToString() ?? "",
                Factory = row["factory"]?.ToString() ?? "",
                ProductionLine = row["productionLine"]?.ToString() ?? "",
                ProductionDate = row["productionDate"]?.ToString() ?? "",
                Shift = row["shift"]?.ToString() ?? "",
                OrderQty = Convert.ToInt32(row["orderQty"] ?? 0),
                CartonCapacity = Convert.ToInt32(row["cartonCapacity"] ?? 24),
                LotNumber = row["lotNumber"]?.ToString() ?? "",
                ProductCode = row["productCode"]?.ToString() ?? "",
                ProductName = row["productName"]?.ToString() ?? "",
                Gtin = row["gtin"]?.ToString() ?? "",
                CustomerOrderNo = row["customerOrderNo"]?.ToString() ?? "",
                Uom = row["uom"]?.ToString() ?? "PCS",
                CreatedTime = row["CreatedTime"]?.ToString() ?? "",
                ModifiedTime = row["ModifiedTime"]?.ToString() ?? "",
            };
        }

        /// <summary>
        /// Chuyển POInfo thành Dictionary để insert/update
        /// </summary>
        public Dictionary<string, object?> ToDictionary()
        {
            return new Dictionary<string, object?>
            {
                ["orderNo"] = OrderNo,
                ["site"] = Site,
                ["factory"] = Factory,
                ["productionLine"] = ProductionLine,
                ["productionDate"] = ProductionDate,
                ["shift"] = Shift,
                ["orderQty"] = OrderQty,
                ["cartonCapacity"] = CartonCapacity,
                ["lotNumber"] = LotNumber,
                ["productCode"] = ProductCode,
                ["productName"] = ProductName,
                ["gtin"] = Gtin,
                ["customerOrderNo"] = CustomerOrderNo,
                ["uom"] = Uom,
                ["CreatedTime"] = CreatedTime,
                ["ModifiedTime"] = ModifiedTime
            };
        }
    }

    /// <summary>
    /// Thông tin mã sản phẩm
    /// </summary>
    public class CodeData
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string CartonCode { get; set; } = "0";
        public int Status { get; set; }
        public string ActivateDate { get; set; } = "0";
        public string ProductionDate { get; set; } = "0";
        public string ActivateUser { get; set; } = "";
        public string PackingDate { get; set; } = "0";
        public string SendStatus { get; set; } = "Pending";
        public string ReceiveStatus { get; set; } = "Pending";

        public static CodeData FromDataRow(DataRow row)
        {
            return new CodeData
            {
                Id = Convert.ToInt32(row["Id"] ?? 0),
                Code = row["Code"]?.ToString() ?? "",
                CartonCode = row["cartonCode"]?.ToString() ?? "0",
                Status = Convert.ToInt32(row["Status"] ?? 0),
                ActivateDate = row["ActivateDate"]?.ToString() ?? "0",
                ProductionDate = row["ProductionDate"]?.ToString() ?? "0",
                ActivateUser = row["ActivateUser"]?.ToString() ?? "",
                PackingDate = row["PackingDate"]?.ToString() ?? "0",
                SendStatus = row["Send_Status"]?.ToString() ?? "Pending",
                ReceiveStatus = row["Recive_Status"]?.ToString() ?? "Pending",
            };
        }
    }

    /// <summary>
    /// Dữ liệu bản ghi từ camera
    /// </summary>
    public class RecordData
    {
        public string Code { get; set; } = "FAIL";
        public string CartonCode { get; set; } = "0";
        public string Status { get; set; } = "0";
        public string PLCStatus { get; set; } = "FAIL";
        public string ActivateDate { get; set; } = "0";
        public string ActivateUser { get; set; } = "";
        public string PackingDate { get; set; } = "0";
        public string PackingUser { get; set; } = "";
        public string ProductionDate { get; set; } = "0";
    }

    /// <summary>
    /// Thông tin thùng carton
    /// </summary>
    public class CartonData
    {
        public int Id { get; set; }
        public string CartonCode { get; set; } = "0";
        public string StartDatetime { get; set; } = "0";
        public string CompletedDatetime { get; set; } = "0";
        public string ActivateUser { get; set; } = "";
        public string ProductionDate { get; set; } = "0";

        public static CartonData FromDataRow(DataRow row)
        {
            return new CartonData
            {
                Id = Convert.ToInt32(row["Id"] ?? 0),
                CartonCode = row["cartonCode"]?.ToString() ?? "0",
                StartDatetime = row["Start_Datetime"]?.ToString() ?? "0",
                CompletedDatetime = row["Completed_Datetime"]?.ToString() ?? "0",
                ActivateUser = row["ActivateUser"]?.ToString() ?? "",
                ProductionDate = row["ProductionDate"]?.ToString() ?? "0",
            };
        }
    }

    /// <summary>
    /// Bộ đếm sản phẩm
    /// </summary>
    public class ProductCounter
    {
        // Tổng cuối cùng (single source of truth)
        public int PassTotal { get; set; }
        public int FailTotal { get; set; }

        // Chi tiết (breakdown cho UI/debug)
        public int NotFoundCount { get; set; }
        public int ReadFailCount { get; set; }
        public int FormatFailCount { get; set; }
        public int ErrorCount { get; set; }
        public int TimeoutCount { get; set; }

        // Tách riêng — Duplicate tính vào FailTotal (PLC FAIL cho mã trùng lặp)
        public int DuplicateCount { get; set; }

        public int TotalCount { get; set; }
        public int TotalCartonCount { get; set; }
        public int ActivatedCartonCount { get; set; }
        public int ErrorCartonCount { get; set; }
        public int CartonID { get; set; } = 1;
        public int CartonCapacity { get; set; } = 24;
        public string CartonCode { get; set; } = "";

        public void Reset()
        {
            PassTotal = 0;
            FailTotal = 0;
            NotFoundCount = 0;
            ReadFailCount = 0;
            FormatFailCount = 0;
            ErrorCount = 0;
            TimeoutCount = 0;
            DuplicateCount = 0;
            TotalCount = 0;
            TotalCartonCount = 0;
            ActivatedCartonCount = 0;
            ErrorCartonCount = 0;
            CartonID = 1;
            CartonCode = "";
            CartonCapacity = 24;
        }
    }

    /// <summary>
    /// Bộ đếm AWS Send
    /// </summary>
    public class AWSSendCounter
    {
        public int PendingCount { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }

        public void Reset()
        {
            PendingCount = 0;
            SentCount = 0;
            FailedCount = 0;
        }
    }

    /// <summary>
    /// Bộ đếm AWS Receive
    /// </summary>
    public class AWSReceiveCounter
    {
        public int WaitingCount { get; set; }
        public int ReceivedCount { get; set; }

        public void Reset()
        {
            WaitingCount = 0;
            ReceivedCount = 0;
        }
    }

    /// <summary>
    /// Thông tin code trong Dictionary (cho lookup nhanh)
    /// </summary>
    public class CodeInfo
    {
        public string Code { get; set; } = "";
        public string OrderNo { get; set; } = "";
        public int Status { get; set; }
        public string CartonCode { get; set; } = "0";
        public string ActivateDate { get; set; } = "0";
        public string ProductionDate { get; set; } = "0";
        public string ActivateUser { get; set; } = "";
        public string PackingDate { get; set; } = "0";
        public bool IsPacked => CartonCode != "0";
    }

    /// <summary>
    /// Thông tin carton trong Dictionary (cho lookup nhanh)
    /// </summary>
    public class CartonInfo
    {
        public int Id { get; set; }
        public string CartonCode { get; set; } = "0";
        public string StartDatetime { get; set; } = "0";
        public string CompletedDatetime { get; set; } = "0";
        public string ActivateUser { get; set; } = "";
        public string ProductionDate { get; set; } = "0";
        public bool IsClosed => CompletedDatetime != "0";
        public bool IsOpen => StartDatetime != "0" && CompletedDatetime == "0";
    }

    /// <summary>
    /// Item trong queue ghi CodeUpdate (update Status=1 + cartonCode vào {orderNo}.db)
    /// </summary>
    public class CodeUpdateItem
    {
        public string OrderNo { get; set; } = "";
        public string Code { get; set; } = "";
        public string ActivateDate { get; set; } = "";
        public string ActivateUser { get; set; } = "";
        public string PackingDate { get; set; } = "";
        public string CartonCode { get; set; } = "0";
        public string ProductionDate { get; set; } = "";
    }

    /// <summary>
    /// Loại update cho Carton (hiện tại chỉ có Complete)
    /// </summary>
    public enum CartonUpdateType
    {
        Complete
    }

    /// <summary>
    /// Item trong queue ghi CartonUpdate (chỉ khi đủ 24 mã trong thùng)
    /// </summary>
    public class CartonUpdateItem
    {
        public CartonUpdateType Type { get; set; }
        public string OrderNo { get; set; } = "";
        public int CartonId { get; set; }
        public string ActivateUser { get; set; } = "";
    }

    /// <summary>
    /// Thông tin lịch sử PO
    /// </summary>
    public class POHistoryData
    {
        public int Id { get; set; }
        public string PO { get; set; } = "";
        public string ProductionDate { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string Status { get; set; } = "";
        public string UserName { get; set; } = "";

        public static POHistoryData FromDataRow(DataRow row)
        {
            return new POHistoryData
            {
                Id = Convert.ToInt32(row["ID"] ?? 0),
                PO = row["PO"]?.ToString() ?? "",
                ProductionDate = row["ProductionDate"]?.ToString() ?? "",
                StartTime = row["StartTime"]?.ToString() ?? "",
                EndTime = row["EndTime"]?.ToString() ?? "",
                Status = row["Status"]?.ToString() ?? "",
                UserName = row["UserName"]?.ToString() ?? ""
            };
        }
    }
}
