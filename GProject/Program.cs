using Glib.Omron;
using GProject.DataPoolHelper;

namespace GProject
{
    public class Program
    {
        private OmronCodeReader? _CR_Active;
        private OmronCodeReader? _CR_Package;
        private static GProjectApiServer? _apiServer;

        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  GProject - Starting...");
            Console.WriteLine("========================================");

            DataPoolStatic.DataPath = DataPool.DefaultDataPath;

            _apiServer = new GProjectApiServer(9999, "0.0.0.0", (src, msg) =>
            {
                Console.WriteLine($"[{src}] {msg}");
            });

            try
            {
                await _apiServer.StartAsync();
                Console.WriteLine();
                Console.WriteLine("  REST API Server: http://localhost:9999");
                Console.WriteLine("  Health Check:    http://localhost:9999/api/health");
                Console.WriteLine("  DataPool API:    http://localhost:9999/api/datapool/pools");
                Console.WriteLine();
                Console.WriteLine("  Waiting for requests...");
                Console.WriteLine("========================================");
                Console.WriteLine();

                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to start API server: {ex.Message}");
            }

            Console.WriteLine("GProject stopped.");
        }
    }
}
