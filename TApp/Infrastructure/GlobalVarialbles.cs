using MTs.Auditrails;
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
        Ready = 1,        // Đang chạy (ACTIVE)
        Stopped = 2,      // Dừng máy - bộ đá vẫn hoạt động
        Deactive = 3,     // Dừng máy, không kiểm và không đá (DEACTIVE)
        Error = 4,        // Lỗi
        Editing = 5       // Đang chỉnh sửa
    }

    public static class GlobalVarialbles
    {
        public static UserData CurrentUser { get; set; } = new UserData();
        public static e_AppState CurrentAppState { get; set; } = e_AppState.Initializing;
        public static LogHelper<e_LogType>? Logger;
    }

}
