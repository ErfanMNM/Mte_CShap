using Sunny.UI;

namespace VNQR.Configs
{
    [ConfigFile("Configs\\App.ini")]
    public class AppConfigs : IniConfig<AppConfigs>
    {
        public string? Camera_01_IP { get; set; }
        public int Camera_01_Port { get; set; }

        public override void SetDefault()
        {
            base.SetDefault();
            Camera_01_IP = "127.0.0.1";
            Camera_01_Port = 49211;
        }
    }
}
