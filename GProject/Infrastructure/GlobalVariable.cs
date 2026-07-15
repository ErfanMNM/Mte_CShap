using GProject.ProductionOrderHelpers;
using System.Collections.Concurrent;

namespace GProject.Infrastructure
{
    //public class GlobalVariable
    //{
    //    public static ConcurrentDictionary<int, CartonInfo> Dictionary_Cartons { get; set; } = new();
    //}

    /// <summary>
    /// Global configuration cho GProject
    /// </summary>
    public static class G
    {
        /// <summary>
        /// AWS IoT Configuration
        /// </summary>
        public static AWSIoTConfig AWS { get; set; } = new();

        /// <summary>
        /// AWS IoT Client instance
        /// </summary>
        public static GProject.IoT.AWSIoTClient? AWSClient { get; set; }
    }

    /// <summary>
    /// AWS IoT Configuration - load từ config hoặc env
    /// </summary>
    public class AWSIoTConfig
    {
        public bool Enabled { get; set; } = false;
        public bool DevMode { get; set; } = false;
        public string Endpoint { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string RootCAPath { get; set; } = "";
        public string ClientCertPath { get; set; } = "";
        public string ClientCertPassword { get; set; } = "";
        public bool AutoSend { get; set; } = false;
        public string ThingName { get; set; } = "GProject";

        /// <summary>
        /// Topic prefix cho publish
        /// </summary>
        public string PublishTopic => DevMode ? "CZ/dataDev" : "CZ/data";

        /// <summary>
        /// Topic subscribe response
        /// </summary>
        public string ResponseTopic => $"CZ/{ClientId}/response";

        /// <summary>
        /// Topic subscribe command
        /// </summary>
        public string CommandTopic => $"CZ/{ClientId}/command";

        /// <summary>
        /// Convert to IoT.AWSConfig
        /// </summary>
        public IoT.AWSConfig ToAWSConfig()
        {
            return new IoT.AWSConfig
            {
                Enabled = Enabled,
                DevMode = DevMode,
                Endpoint = Endpoint,
                ClientId = ClientId,
                RootCAPath = RootCAPath,
                ClientCertPath = ClientCertPath,
                ClientCertPassword = ClientCertPassword,
                AutoSend = AutoSend,
                ThingName = ThingName
            };
        }
    }
}
