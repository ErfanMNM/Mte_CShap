namespace GProject.ProductionOrderHelpers
{
    #region Request Models

    /// <summary>POST /api/carton/scan</summary>
    public class CartonScanRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("machineName")]
        public string MachineName { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("cartonCode")]
        public string CartonCode { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("scannedAt")]
        public string ScannedAt { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("mode")]
        public string Mode { get; set; } = "scan";

        /// <summary>
        /// Lane của PDA: "PDA01" / "PDA02" / "01" / "02". Chỉ bắt buộc khi mode=assign.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("lane")]
        public string Lane { get; set; } = "";

        /// <summary>
        /// Carton ID muốn gán mã. Nếu null hoặc 0, hệ thống sẽ tự tìm thùng chưa có mã đầu tiên.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("cartonId")]
        public int? CartonId { get; set; }
    }

    #endregion

    #region Response Models

    /// <summary>Response cho POST /api/carton/scan</summary>
    public class CartonScanResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; } = "";

        /// <summary>OK / WARN / ERR</summary>
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = "ERR";

        [System.Text.Json.Serialization.JsonPropertyName("cartonIndex")]
        public int CartonIndex { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("orderNo")]
        public string OrderNo { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("productCount")]
        public int ProductCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("activateDate")]
        public string ActivateDate { get; set; } = "";
    }

    /// <summary>Response cho GET /api/carton/{cartonCode}/info</summary>
    public class CartonInfoResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("cartonCode")]
        public string CartonCode { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("cartonIndex")]
        public int CartonIndex { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("activateDate")]
        public string ActivateDate { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("activateUser")]
        public string ActivateUser { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("productCount")]
        public int ProductCount { get; set; }

        /// <summary>OK / WARN / ERR</summary>
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; } = "ERR";

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    /// <summary>Response cho GET /api/carton/current-po</summary>
    public class CurrentPOResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("orderNo")]
        public string OrderNo { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("productName")]
        public string ProductName { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("orderQty")]
        public int OrderQty { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("state")]
        public string State { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    #endregion

    #region Internal Models

    /// <summary>Dùng nội bộ trong CartonWriteQueue</summary>
    public enum CartonWriteType { ScanCarton, StartCarton, CompleteCarton, ResetCarton, AssignCarton }

    /// <summary>Task gửi vào queue để serialize các thao tác write</summary>
    public class CartonWriteTask
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public CartonWriteType Type { get; init; }
        public string OrderNo { get; init; } = "";
        public string CartonCode { get; init; } = "";
        public int? CartonId { get; init; }
        public string MachineName { get; init; } = "";
        public string ScannedAt { get; init; } = "";
        public string Mode { get; init; } = "scan";
        internal TaskCompletionSource<CartonWriteResult> Completion { get; } = new();
        public Task<CartonWriteResult> Task => Completion.Task;
    }

    /// <summary>Kết quả trả về từ queue consumer</summary>
    public class CartonWriteResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        /// <summary>OK / WARN / ERR</summary>
        public string Status { get; init; } = "ERR";
        public int CartonIndex { get; init; }
        public int ProductCount { get; init; }
        public string ActivateDate { get; init; } = "";
    }

    /// <summary>Thông tin chi tiết thùng — dùng cho GetCartonInfo</summary>
    public class CartonDetailInfo
    {
        public bool Success { get; set; }
        public string CartonCode { get; set; } = "";
        public int CartonIndex { get; set; }
        public string StartDatetime { get; set; } = "0";
        public string ActivateUser { get; set; } = "";
        public int ProductCount { get; set; }
        public string Status { get; set; } = "ERR";
        public string Message { get; set; } = "";

        public CartonDetailInfo WithError(string msg) { Message = msg; return this; }
    }

    /// <summary>Thông tin scan thùng cuối cùng</summary>
    public class CartonScanInfo
    {
        public string CartonCode { get; set; } = "";
        public string StartDatetime { get; set; } = "";
        public string ActivateUser { get; set; } = "";
        public int CartonIndex { get; set; }
        public string ScanAt { get; set; } = "";
        public string Mode { get; set; } = "";
        public string Result { get; set; } = "";
    }

    #endregion
}
