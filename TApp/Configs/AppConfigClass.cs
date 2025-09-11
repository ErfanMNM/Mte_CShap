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

        public string PLC_IP { get; set; } = string.Empty;

        public override void SetDefault()
        {
            base.SetDefault();
            AppHideEnable = true;
            AppStartWithWindows = false;
            PLC_IP = "127.0.0.1";
        }
    }
}
