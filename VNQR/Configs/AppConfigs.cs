using Sunny.UI;

namespace VNQR.Configs
{
    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {
        public string? Camera_Active_IP { get; set; }
        public int Camera_Active_Port { get; set; }

        public string? Camera_Package_IP { get; set; }
        public int Camera_Package_Port { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            Camera_Active_IP = "127.0.0.1";
            Camera_Active_Port = 49211;

            Camera_Package_IP = "127.0.0.1";
            Camera_Package_Port = 49212;
        }
    }
}
