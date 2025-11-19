using Sunny.UI;

namespace TApp.Configs
{

    //đường dẫn đến file cấu hình trong Appdata\TApp



    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {

        public bool AppHideEnable { get; set; }
        public bool AppTwoFA_Enabled { get; set; }
        public string ? PLC_IP { get; set; }
        public int PLC_Port { get; set; }

        public string ? Camera_01_IP { get; set; }
        public int Camera_01_Port { get; set; }

        public int PLC_Time_Refresh { get; set; }

        public string ? Line_Name { get; set; }

        public bool PLC_Test_Mode { get; set; }

        public string ? production_list_path { get; set; }
        public string? credentialPLCAddressPath { get; set; }
        public string? credentialERPPath { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            AppHideEnable = true;

            PLC_IP = "192.168.250.1";
            PLC_Port = 9600;

            PLC_Time_Refresh = 1000;
            Camera_01_IP = "127.0.0.1";
            Camera_01_Port = 50001;
            Line_Name = "Line 3";
            PLC_Test_Mode = true;
            production_list_path = "D:/Masan/DBProductList.xlsx";
            credentialPLCAddressPath = "D:/Masan/a.json";
            credentialERPPath = "C:/Users/DANOMT/Downloads/Masan/sales-268504-20a4b06ea0fb.json";
        }
    }
}
