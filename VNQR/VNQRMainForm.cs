using TTManager.Omron;
using TTManager.PLCHelpers;

namespace VNQR
{
    public partial class VNQRMainForm : Form
    {
        #region Fields
        private OmronCamera? omronCamera;
        private OmronPLC_Hsl.PLCStatus? PLC_Status = OmronPLC_Hsl.PLCStatus.Disconnect;

        #endregion
        public VNQRMainForm()
        {
            InitializeComponent();
        }

        private void Init()
        {
            omronCamera = new OmronCamera(OmronCamera.CameraModel.V430);

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
    }
}
