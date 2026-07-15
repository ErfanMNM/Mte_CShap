namespace GProject.IoT
{
    /// <summary>
    /// Payload gửi lên AWS IoT Core khi có mã được scan/activate
    /// </summary>
    public class AWSSendPayload
    {
        public string message_id { get; set; } = "";
        public string orderNo { get; set; } = "";
        public string uniqueCode { get; set; } = "";
        public string gtin { get; set; } = "";
        public string cartonCode { get; set; } = "";
        public int status { get; set; }
        public string activate_datetime { get; set; } = "";
        public string production_date { get; set; } = "";
        public string thing_name { get; set; } = "";
    }

    /// <summary>
    /// Response từ AWS IoT Core
    /// </summary>
    public class AWSResponse
    {
        public string status { get; set; } = "";
        public string message_id { get; set; } = "";
        public string error_message { get; set; } = "";
    }

    /// <summary>
    /// Payload gửi khi hoàn thành một thùng
    /// </summary>
    public class AWSCartonPayload
    {
        public string message_id { get; set; } = "";
        public string orderNo { get; set; } = "";
        public string cartonCode { get; set; } = "";
        public string cartonId { get; set; } = "";
        public int itemCount { get; set; }
        public string completed_datetime { get; set; } = "";
        public string activateUser { get; set; } = "";
        public string thing_name { get; set; } = "";
    }

    /// <summary>
    /// Payload gửi khi PO started/completed
    /// </summary>
    public class AWSPOPayload
    {
        public string message_id { get; set; } = "";
        public string orderNo { get; set; } = "";
        public string event_type { get; set; } = ""; // "start" or "complete"
        public string gtin { get; set; } = "";
        public int orderQty { get; set; }
        public string productionDate { get; set; } = "";
        public string line { get; set; } = "";
        public string user { get; set; } = "";
        public string thing_name { get; set; } = "";
    }

    /// <summary>
    /// Cấu hình AWS IoT
    /// </summary>
    public class AWSConfig
    {
        public bool Enabled { get; set; } = false;
        public bool DevMode { get; set; } = false;
        public string Endpoint { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string RootCAPath { get; set; } = "";
        public string ClientCertPath { get; set; } = "";
        public string ClientCertPassword { get; set; } = "";
        public bool AutoSend { get; set; } = false;
        public string ThingName { get; set; } = "";
    }

    /// <summary>
    /// Trạng thái kết nối AWS IoT
    /// </summary>
    public enum AWSIoTStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Error,
        Subscribed,
        Unsubscribed,
        Published,
        Unpublished
    }

    /// <summary>
    /// Item trong queue gửi AWS
    /// </summary>
    public class AWSSendItem
    {
        public string Topic { get; set; } = "";
        public string Payload { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
    }
}
