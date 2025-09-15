using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp.Configs
{

    //đường dẫn đến file cấu hình trong Appdata\TApp



    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {

        public bool AppHideEnable { get; set; }
        public bool AppStartWithWindows { get; set; }

        public bool TCP_AutoStart { get; set; } = false;

        public int TCP_Port { get; set; } = 51236;

        public string PLC_IP { get; set; } = string.Empty;
        public int PLC_Port { get; set; } = 102;
        public bool PLC_Auto_Connect { get; set; } = false;

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
        }
    }
}
