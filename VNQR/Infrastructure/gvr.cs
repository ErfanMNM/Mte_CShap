using System;
using System.Collections.Generic;
using System.Text;
using TTManager.Omron;

namespace VNQR.Infrastructure
{
    public static class gvr
    {
        public static string AppName = "VNQR";
        public static eOmronCameraState CameraState = eOmronCameraState.Disconnected;
        public static e_AppState AppState = e_AppState.Idle;
    }

    public enum e_AppState
    {
        Idle,
        Running,
        Error,
        NotUsed
    }
}
