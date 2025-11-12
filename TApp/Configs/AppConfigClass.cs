using Sunny.UI;

namespace TApp.Configs
{

    //đường dẫn đến file cấu hình trong Appdata\TApp



    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {

        public bool AppHideEnable { get; set; }
        public bool AppTwoFA_Enabled { get; set; }
        public bool AppStartWithWindows { get; set; }
        public bool TCP_AutoStart { get; set; }
        public int TCP_Port { get; set; } 
        public string ? PLC_IP { get; set; }
        public int PLC_Port { get; set; }
        public bool PLC_Auto_Connect { get; set; }
        public string ? Camera_01_IP { get; set; }
        public int Camera_01_Port { get; set; }

        public int PLC_Time_Refresh { get; set; }

        public string ? Camera_02_IP { get; set; }
        public int Camera_02_Port { get; set; }

        public string ? Line_Name { get; set; }

        public bool PLC_Test_Mode { get; set; }

        public string ? production_list_path { get; set; }
        public string? credentialPLCAddressPath { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            AppHideEnable = true;
            AppStartWithWindows = false;
            PLC_IP = "192.168.250.1";
            PLC_Port = 9600;
            TCP_Port = 51236;
            TCP_AutoStart = false;
            PLC_Time_Refresh = 1000;
            
            PLC_Auto_Connect = false;
            Camera_01_IP = "127.0.0.1";
            Camera_01_Port = 50001;
            Line_Name = "Line 3";
            PLC_Test_Mode = true;
            production_list_path = "D:/Masan/DBProductList.xlsx";
            credentialPLCAddressPath = "D:/Masan/a.json";
        }
    }
}
