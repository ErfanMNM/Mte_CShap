using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHG_Cartoning.Configs
{
    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {
        public string? PLC_IP { get; set; }
        public int PLC_Port { get; set; }
        public string? Camera_IP { get; set; }
        public int Camera_Port { get; set; }
        public override void SetDefault()
        {

            PLC_IP = "127.0.0.1";
            PLC_Port = 9600;
            Camera_IP = "127.0.0.1";
            Camera_Port = 51236;
        }
    }
}
