using Glib.Omron;
using GProject.DataPoolHelper;
using GProject.Auth;
using GProject.Production;
using GProject.ProductionOrderHelpers;
using Serilog;

namespace GProject
{
    public class Program
    {
    private static OmronCodeReader? _CR_Active;
    private static OmronCodeReader? _CR_Package;
    private static PLCMonitor? _plcMonitor;
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

            // Initialize PO databases
            GProduction.Initialize();
            Log.Information("  PO databases initialized.");

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
                Log.Information("  Production:      http://localhost:9999/api/production/state");
                Log.Information("");
                Log.Information("  Waiting for requests...");
                Log.Information("========================================");
                Log.Information("");

                // Khởi động State Machine loop
                ProductionStateMachine? stateMachine = ProductionStateMachine.Instance;
                stateMachine.Start();
                Log.Information("[Main] ProductionStateMachine started. Initial state: {State}",
                    stateMachine.CurrentState);

                // Khởi tạo 2 camera (active + package) - hardcoded cho demo
                _CR_Active = new OmronCodeReader(OmronCodeReader.e_CodeReaderModel.V430, "127.0.0.1", 9001);
                _CR_Package = new OmronCodeReader(OmronCodeReader.e_CodeReaderModel.V430, "127.0.0.1", 9002);

                _CR_Active.ClientCallback += (state, data) => OnCameraEvent("active", state, data);
                _CR_Package.ClientCallback += (state, data) => OnCameraEvent("package", state, data);

                _CR_Active.Connect();
                _CR_Package.Connect();
                Log.Information("[Main] Cameras initialized: active=127.0.0.1:9001, package=127.0.0.1:9002");

                // Khoi tao PLC Monitor (Omron FINS/UDP) - heartbeat + counter polling.
                // Broadcasts connection state to FE via /ws/plc.
                _plcMonitor = new PLCMonitor();
                _plcMonitor.StateChanged += (_, args) =>
                {
                    var (state, message) = args;
                    var stateStr = state switch
                    {
                        PLCConnectionState.Connected => "Connected",
                        PLCConnectionState.Reconnecting => "Reconnecting",
                        _ => "Disconnected",
                    };
                    Log.Information("[PLC] {State} - {Message}", stateStr, message);
                    _ = PLCHub.Instance.BroadcastStateAsync(stateStr, message);
                };
                _plcMonitor.Start();

                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GProject terminated unexpectedly");
            }
            finally
            {
                // Dừng camera
                try { _CR_Active?.Disconnect(); }
                catch (Exception ex) { Log.Warning(ex, "[Main] Lỗi khi dừng camera active"); }
                try { _CR_Package?.Disconnect(); }
                catch (Exception ex) { Log.Warning(ex, "[Main] Lỗi khi dừng camera package"); }

                // Dừng PLC monitor
                try { _plcMonitor?.Dispose(); }
                catch (Exception ex) { Log.Warning(ex, "[Main] Lỗi khi dừng PLCMonitor"); }

                // Dừng state machine
                try { await ProductionStateMachine.Instance.StopAsync(); }
                catch (Exception ex) { Log.Warning(ex, "[Main] Lỗi khi dừng StateMachine"); }

                Log.Information("GProject stopped.");
                Log.CloseAndFlush();
            }
        }

        private static void OnCameraEvent(string camera, eOmronCodeReaderState state, string data)
        {
            Log.Information("[Camera:{Camera}] [{State}] {Data}", camera, state, data);
            _ = CameraHub.Instance.BroadcastAsync(camera, state, data);
        }
    }
}
