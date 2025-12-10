using Sunny.UI;
using TTManager.Masan;

namespace TApp.Configs
{

    //đường dẫn đến file cấu hình trong Appdata\TApp

    public enum DataMode
    {
        Normal,
        Test,
        Hard
    }


    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {

        public bool AppHideEnable { get; set; }
        public bool AppTwoFA_Enabled { get; set; }

        public string Data_Mode { get; set; }

        public string AWS_Credential_Path { get; set; }
        public string ? PLC_IP { get; set; }
        public int PLC_Port { get; set; }

        public string ? Camera_01_IP { get; set; }
        public int Camera_01_Port { get; set; }

        public int PLC_Time_Refresh { get; set; }

        public string ? Line_Name { get; set; }

        public bool PLC_Test_Mode { get; set; }

        public string ? Handheld_COM_Port { get; set; }

        public string ? production_list_path { get; set; }
        public string? credentialPLCAddressPath { get; set; }
        public string? credentialERPPath { get; set; }

        public string? ERP_Sub_Inv { get; set; }
        public string? ERP_Org_Code { get; set; }
        public string? ERP_DatasetID { get; set; }
        public string? ERP_TableID { get; set; }
        public string? ERP_ProjectID { get; set; }

        public bool Cloud_Connection_Enabled { get; set; }
        public int Cloud_Refresh_Interval_Minute { get; set; }
        public bool Cloud_Upload_Enabled { get; set; }
        public bool Local_Backup_Enabled { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            AppHideEnable = true;

            PLC_IP = "192.168.250.1";
            PLC_Port = 9600;

            Handheld_COM_Port = "COM3";

            PLC_Time_Refresh = 1000;
            Camera_01_IP = "127.0.0.1";
            Camera_01_Port = 50001;
            Line_Name = "Line 3";
            PLC_Test_Mode = true;
            production_list_path = "C:/MASANQR/Configs/DBProductList.xlsx";
            credentialPLCAddressPath = "C:/MASANQR/Configs/GoogleSheet.json";
            credentialERPPath = "C:/MASANQR/Configs/sales-268504-20a4b06ea0fb.json";
            AWS_Credential_Path = "C:/MASANQR/Configs/aws_credentials.json";
            Data_Mode = "normal";
            ERP_DatasetID = "FactoryIntegration";
            ERP_TableID = "BatchProduction";
            ERP_Sub_Inv = "W05";
            ERP_Org_Code = "MIP";
            ERP_ProjectID = "sales-268504";
            Cloud_Connection_Enabled = false;
            Cloud_Refresh_Interval_Minute = 60;
            Cloud_Upload_Enabled = true;
            Local_Backup_Enabled = true;
        }
    }
}
