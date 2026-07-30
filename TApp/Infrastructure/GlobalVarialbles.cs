using TTManager.Audit;
using System.ComponentModel;
using TTManager.Auth;


namespace TApp.Infrastructure
{
    public enum e_LogType
    {
        [Description("Thông tin")]
        Info = 0,

        [Description("Cảnh báo")]
        Warning = 1,

        [Description("Lỗi")]
        Error = 2,

        [Description("Debug")]
        Debug = 3,

        [Description("Hệ thống")]
        System = 4,

        [Description("Người dùng")]
        UserAction = 5,

        [Description("Thiết bị")]
        DeviceAction = 6,

        [Description("Bảo trì")]
        Maintenance = 7,

        [Description("Thay đổi dữ liệu")]
        DataChange = 8,

        Critical = 9
    }
    public enum CameraStatus
    {
        Disconnected = 0,
        Connected = 1,
        Error = 2,
        Reconnecting = 3,
    }

    public enum e_AppState
    {
        Initializing = 0, // Khởi động ứng dụng
        Loading_PO = 1,
        Creating_PO = 2,
        Pushing = 3,
        Device_Error = 4,
        Stopped = 5,
        Switching = 6,
        Running = 7,
        Printer_Error = 8,
        Printer_Pause = 9,

    }

    public static class GlobalVarialbles
    {
        public static UserData CurrentUser { get; set; } = new UserData();
        public static e_AppState CurrentAppState { get; set; } = e_AppState.Stopped;
        public static LogHelper<e_LogType>? Logger;

        public static List<string> Print_Codes { get; set; } = new List<string>();

        public static bool IsPush { get; set; } = true;
    }

}
