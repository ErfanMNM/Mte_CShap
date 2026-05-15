namespace MHG_Printer.Infrastructure
{
    public enum e_LogType
    {
        System,
        UserAction,
        Error
    }

    public enum e_AppState
    {
        Initializing,
        Ready,
        Editing,
        Error,
        Stopped,
        Deactive
    }

    public enum e_AppRenderState
    {
        LOGIN,
        ACTIVE,
        DEACTIVE
    }

    public static class GlobalVariables
    {
        public static TTManager.Auth.UserData CurrentUser { get; set; } = new TTManager.Auth.UserData
        {
            Username = string.Empty,
            Role = string.Empty
        };

        public static TTManager.Audit.LogHelper<e_LogType>? Logger { get; set; }
        public static e_AppState CurrentAppState { get; set; } = e_AppState.Initializing;
        public static e_AppRenderState AppRenderState { get; set; } = e_AppRenderState.LOGIN;
    }
}
