using MTs.Datalogic;
using Sunny.UI;
using TApp.Configs;

namespace TApp.Views.Dashboard
{
    public partial class FDashboard : UIPage
    {
        private DatalogicCamera ? _datalogicCamera;
        public FDashboard()
        {
            InitializeComponent();
        }

        public void Start()
        {
            InitializeDevices();
        }

        private void InitializeDevices()
        {
            _datalogicCamera = new DatalogicCamera(AppConfigs.Current.Camera_IP, AppConfigs.Current.Camera_Port);
            _datalogicCamera.ClientCallback += DatalogicCamera_ClientCallback;
        }

        private void DatalogicCamera_ClientCallback(eDatalogicCameraState state, string data)
        {
            switch (state)
            {
                case eDatalogicCameraState.Connected:
                    break;
                case eDatalogicCameraState.Disconnected:
                    break;
                case eDatalogicCameraState.Received:
                    break;
                case eDatalogicCameraState.Reconnecting:
                    break;
            }
        }

        private void Camera_ProcessData(string data)
        {
            // Xử lý dữ liệu nhận được từ camera Datalogic

        }
    }
}
