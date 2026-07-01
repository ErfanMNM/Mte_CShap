using Glib.Omron;
using GProject.DataPoolHelper;
using GProject.Auth;
using Serilog;

namespace GProject
{
    public class Program
    {
        private OmronCodeReader? _CR_Active;
        private OmronCodeReader? _CR_Package;
        private static GProjectApiServer? _apiServer;

        static async Task Main(string[] args)
        {
            // Configure Serilog file logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    @"C:\GProject\Logs\gproject-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30
                )
                .CreateLogger();

            Log.Information("========================================");
            Log.Information("  GProject - Starting...");
            Log.Information("========================================");

            // Initialize Auth database
            AuthDb.EnsureCreated();
            Log.Information("  Auth database initialized.");

            DataPoolStatic.DataPath = DataPool.DefaultDataPath;

            _apiServer = new GProjectApiServer(9999, "0.0.0.0", (src, msg) =>
            {
                Log.Information("[{Source}] {Message}", src, msg);
            });

            try
            {
                await _apiServer.StartAsync();
                Log.Information("");
                Log.Information("  REST API Server: http://localhost:9999");
                Log.Information("  Health Check:    http://localhost:9999/api/health");
                Log.Information("  DataPool API:    http://localhost:9999/api/datapool/pools");
                Log.Information("");
                Log.Information("  Waiting for requests...");
                Log.Information("========================================");
                Log.Information("");

                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GProject terminated unexpectedly");
            }
            finally
            {
                Log.Information("GProject stopped.");
                Log.CloseAndFlush();
            }
        }
    }
}
