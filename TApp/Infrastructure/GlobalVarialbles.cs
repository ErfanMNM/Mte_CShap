using MTs.Auditrails;
using TApp.Helpers;
using TApp.Views.Dashboard;
using TTManager.Auth;

namespace TApp.Infrastructure
{

    public enum CameraStatus
    {
        Disconnected = 0,
        Connected = 1,
        Error = 2,
        Reconnecting = 3,
    }

    public enum e_AppState
    {
        Initializing = 0,
        Ready = 1,
        Stopped = 2,
        Error = 3,
        
        Editing = 4,
        DeviceError = 5,

    }

    public static class GlobalVarialbles
    {
        public static UserData CurrentUser { get; set; } = new UserData();
        public static e_AppState CurrentAppState { get; set; } = e_AppState.Initializing;

        public static LogHelper<e_LogType>? Logger;
    }

}
