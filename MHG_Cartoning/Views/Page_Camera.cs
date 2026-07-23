using MHG_Cartoning.Omron;
using Sunny.UI;
using MHG_Cartoning.Configs;
using MHG_Cartoning.Infrastructure;
using System.ComponentModel;

namespace MHG_Cartoning.Views
{
    public partial class Page_Camera : UIPage
    {
        private OmronCamera? _camera;
        private BackgroundWorker? _backgroundWorker;
        public Page_Camera()
        {
            InitializeComponent();

            _camera = new OmronCamera(OmronCamera.e_CameraModel.VHV5, AppConfigs.Current.Camera_IP??"127.0.0.1", AppConfigs.Current.Camera_Port);
            _camera.ClientCallback += _camera_ClientCallback;
            _camera.Connect();

            _backgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
        }

        private void _backgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            string rawCode = e.Argument as string ?? string.Empty;
            Camera_Procesing(rawCode);
        }

        public void Camera_Procesing(string rawCode)
        {

        }

        private void _camera_ClientCallback(eOmronCameraState state, string data)
        {
            switch (state)
            {
                case eOmronCameraState.Connected:
                    if(Global.CAMERA_STATE != eOmronCameraState.Connected)
                    {
                        Global.CAMERA_STATE = eOmronCameraState.Connected;
                    }
                    break;
                case eOmronCameraState.Disconnected:
                    if(Global.CAMERA_STATE != eOmronCameraState.Disconnected)
                    {
                        Global.CAMERA_STATE = eOmronCameraState.Disconnected;
                    }
                    break;
                case eOmronCameraState.Received:

                    break;
                case eOmronCameraState.Reconnecting:
                    if(Global.CAMERA_STATE != eOmronCameraState.Reconnecting)
                    {
                        Global.CAMERA_STATE = eOmronCameraState.Reconnecting;
                    }
                    break;
            }
        }
    }
}
