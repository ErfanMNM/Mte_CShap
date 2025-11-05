using TApp.Helpers;

namespace TApp.Infrastructure
{

    public enum CameraStatus
    {
        Disconnected = 0,
        Connected = 1,
        Error = 2,
        Reconnecting = 3,
    }

    public enum AppState
    {
        Initializing = 0,
        Running = 1,
        Stopped = 2,
        Error = 3,
    }

}
