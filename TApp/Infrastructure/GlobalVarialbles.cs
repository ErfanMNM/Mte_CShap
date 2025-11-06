using TApp.Helpers;
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
        Running = 1,
        Stopped = 2,
        Error = 3,
        NeedLogin = 4,
        NotLoggedIn = 5

    }

    public static class GlobalVarialbles
    {
        public static UserData CurrentUser { get; set; } = new UserData();
        public static e_AppState CurrentAppState { get; set; } = e_AppState.Initializing;
    }

}
