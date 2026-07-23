using MHG_Cartoning.Configs;
using Sunny.UI;

namespace MHG_Cartoning.Views
{
    public partial class Page_Settings : UIPage
    {
        public Page_Settings()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void Log(string message)
        {
            Invoke(() => uiListBox1.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}"));
        }

        private void LoadConfig()
        {
            var config = AppConfigs.Current;
            txtPLC_IP.Text = config.PLC_IP ?? "";
            numPLC_Port.Text = config.PLC_Port.ToString();
            txtCamera_IP.Text = config.Camera_IP ?? "";
            numCamera_Port.Text = config.Camera_Port.ToString();
            txtOPC_CA_TCP.Text = config.OPC_CA_TCP ?? "";
            txtOPC_POItem_Node.Text = config.OPC_Cartoning_POItem_Node ?? "";
            txtOPC_POLot_Node.Text = config.OPC_Cartoning_POLot_Node ?? "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var config = AppConfigs.Current;
                config.PLC_IP = txtPLC_IP.Text;
                config.PLC_Port = int.TryParse(numPLC_Port.Text, out int plcp) ? plcp : 9600;
                config.Camera_IP = txtCamera_IP.Text;
                config.Camera_Port = int.TryParse(numCamera_Port.Text, out int camP) ? camP : 51236;
                config.OPC_CA_TCP = txtOPC_CA_TCP.Text;
                config.OPC_Cartoning_POItem_Node = txtOPC_POItem_Node.Text;
                config.OPC_Cartoning_POLot_Node = txtOPC_POLot_Node.Text;

                config.Save();
                Log("Cài đặt đã được lưu thành công!");
                UIMessageTip.ShowOk("Lưu thành công!");
            }
            catch (Exception ex)
            {
                Log($"Lỗi lưu cài đặt: {ex.Message}");
                UIMessageTip.ShowError($"Lỗi: {ex.Message}");
            }
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            var result = this.ShowAskDialog("Bạn có chắc muốn khôi phục cài đặt mặc định không?");
            if (result)
            {
                try
                {
                    AppConfigs.Current.SetDefault();
                    LoadConfig();
                    Log("Đã khôi phục cài đặt mặc định!");
                    UIMessageTip.ShowOk("Khôi phục mặc định thành công!");
                }
                catch (Exception ex)
                {
                    Log($"Lỗi khôi phục: {ex.Message}");
                    UIMessageTip.ShowError($"Lỗi: {ex.Message}");
                }
            }
        }
    }
}
