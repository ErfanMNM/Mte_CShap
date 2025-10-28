using Sunny.UI;

namespace TApp.Configs
{

    //đường dẫn đến file cấu hình trong Appdata\TApp



    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {

        public bool AppHideEnable { get; set; }
        public bool AppStartWithWindows { get; set; }
        public bool TCP_AutoStart { get; set; }
        public int TCP_Port { get; set; } = 51236;
        public string ? PLC_IP { get; set; }
        public int PLC_Port { get; set; }
        public bool PLC_Auto_Connect { get; set; }
        public string ? Camera_IP { get; set; }
        public int Camera_Port { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            AppHideEnable = true;
            AppStartWithWindows = false;
            PLC_IP = "192.168.250.12";
            TCP_Port = 51236;
            TCP_AutoStart = false;
            PLC_Port = 102;
            PLC_Auto_Connect = false;
            Camera_IP = "192.69.0.1";
            Camera_Port = 50001;
        }
    }
}
