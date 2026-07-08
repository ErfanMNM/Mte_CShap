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
    private static OmronCodeReader? _CR_Camera;
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

                // Khởi tạo 1 camera duy nhất (vừa active vừa phân làn)
                _CR_Camera = new OmronCodeReader(OmronCodeReader.e_CodeReaderModel.V430, "127.0.0.1", 9001);
                _CR_Camera.ClientCallback += (state, data) => OnCameraEvent("camera", state, data);
                _CR_Camera.Connect();
                Log.Information("[Main] Camera initialized: camera=127.0.0.1:9001");

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
                    ProductionStateMachine.Instance.OnDeviceStateChanged("PLC", state == PLCConnectionState.Connected, message);
                    _ = PLCHub.Instance.BroadcastStateAsync(stateStr, message);
                };
                _plcMonitor.Start();
                ProductionStateMachine.Instance.PLCMonitor = _plcMonitor;

                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GProject terminated unexpectedly");
            }
            finally
            {
                // Dừng camera
                try { _CR_Camera?.Disconnect(); }
                catch (Exception ex) { Log.Warning(ex, "[Main] Lỗi khi dừng camera"); }

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

            switch (state)
            {
                case eOmronCodeReaderState.Connected:
                case eOmronCodeReaderState.Disconnected:
                case eOmronCodeReaderState.Reconnecting:
                    _ = CameraHub.Instance.BroadcastAsync(camera, state, data);
                    ProductionStateMachine.Instance.OnDeviceStateChanged(
                        "Camera", state == eOmronCodeReaderState.Connected, data);
                    break;

                case eOmronCodeReaderState.Received:
                    // Chỉ xử lý đầy đủ khi đang Running.
                    // Ngược lại: vẫn ghi 0 xuống PLC để PLC biết "bỏ qua" mã này,
                    // KHÔNG ghi DB, KHÔNG tăng counter, KHÔNG broadcast (lát thả lại mã sau).
                    if (ProductionStateMachine.Instance.CurrentState != e_ProductionState.Running)
                    {
                        ProductionStateMachine.Instance.WritePLCResponseSkip();
                        return;
                    }
                    var result = ProductionStateMachine.Instance.HandleCodeFromCamera(data, camera);
                    if (result != null)
                    {
                        CameraHub.Instance.RecordHistory(result);
                        _ = CameraHub.Instance.BroadcastCodeStatus(result);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
