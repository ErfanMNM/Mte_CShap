using Sunny.UI;
using System.Reflection.Emit;
using TTManager.Omron;
using TTManager.PLCHelpers;
using VNQR.Configs;
using VNQR.Infrastructure;
using VNQR.Utils;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;

namespace VNQR
{
    public partial class VNQRMainForm : UIForm
    {
        #region Fields
        private OmronCamera? omronCamera;
        private OmronPLC_Hsl.PLCStatus? PLC_Status = OmronPLC_Hsl.PLCStatus.Disconnect;

        #endregion
        public VNQRMainForm()
        {
            InitializeComponent();
            omronCamera = new OmronCamera(OmronCamera.e_CameraModel.V430, AppConfigs.Current.Camera_01_IP, AppConfigs.Current.Camera_01_Port);
            omronCamera.ClientCallback += OmronCamera_ClientCallback;
            omronCamera.Connect();
        }

        private void OmronCamera_ClientCallback(eOmronCameraState state, string data)
        {
            switch (state)
            {
                case eOmronCameraState.Connected:
                    gvr.CameraState = state;
                    break;
                case eOmronCameraState.Disconnected:
                    gvr.CameraState = state;
                    break;
                case eOmronCameraState.Received:
                    gvr.CameraState = state;
                    HandleCameraDataReceived(data);
                    break;
                case eOmronCameraState.Reconnecting:
                    gvr.CameraState = state;
                    break;
            }
        }

        private void HandleCameraDataReceived(string data)
        {
            //nếu app đang ở trạng thái chạy thì mới xử lý dữ liệu từ camera
            if (gvr.AppState != e_AppState.Running) { return; }
            //xử lý dữ liệu từ camera ở đây

        }

        //PLC event handler
        private void omronplC_Hsl1_PLCStatus_OnChange(object sender, OmronPLC_Hsl.PLCStatusEventArgs e)
        {
            PLC_Status = e.Status;
            switch (e.Status)
            {
                case OmronPLC_Hsl.PLCStatus.Connected:
                    break;
                case OmronPLC_Hsl.PLCStatus.Disconnect:
                    break;
            }
        }

        private void VNQRMainForm_Load(object sender, EventArgs e)
        {
            mainWK.RunWorkerAsync();
            updateWK.RunWorkerAsync();
        }

        private void mainWK_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!mainWK.CancellationPending)
            {
                switch (gvr.AppState)
                {
                    case e_AppState.Idle:
                        //khởi tạo camera

                        //load dữ liệu sản xuất cũ => load từ database lịch sử sản xuất xem PO nào đang được dùng => nếu không có thì bỏ qua bước này
                        gvr.AppState = e_AppState.Running;
                        break;
                    case e_AppState.Running:
                        break;
                    case e_AppState.Error:
                        break;
                    case e_AppState.NotUsed:
                        break;
                }
                Thread.Sleep(100);
            }

        }

        private void updateWK_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!updateWK.CancellationPending)
            {
                if (MainFormVariable.listbox.Count > 0)
                {
                    string data = MainFormVariable.listbox.Dequeue();
                    this.InvokeIfRequired(() =>
                    {
                        listBox1.Items.Insert(0, data);
                        if (listBox1.Items.Count > 100)
                        {
                            listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
                        }
                    });
                }
                Thread.Sleep(100);
            }
            
        }

    }


public static class MainFormVariable
    {
        public static Queue<string> listbox = new Queue<string>();
    }
}