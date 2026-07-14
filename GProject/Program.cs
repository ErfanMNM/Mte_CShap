using Glib.Omron;
using GProject.DataPoolHelper;
using GProject.Auth;
using GProject.Production;
using GProject.ProductionOrderHelpers;
using Raycoon.Serilog.Sinks.SQLite.Options;
using Serilog;
using Serilog.Events;
using GProject.Config;

namespace GProject
{
    public class Program
    {
    private static OmronCodeReader? _CR_Camera;
    private static PLCMonitor? _plcMonitor;
    private static GProjectApiServer? _apiServer;
    public static AppConfig? _config;

        /// <summary>Exposes the PLC monitor instance for API endpoints.</summary>
        public static PLCMonitor? GetPLCMonitor() => _plcMonitor;

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
                .WriteTo.SQLite(@"C:\GProject\Logs\gproject.db", options =>
                {
                    options.TableName = "Logs";
                    options.StoreTimestampInUtc = true;
                    options.StorePropertiesAsJson = true;
                    options.StoreExceptionDetails = true;
                    options.JournalMode = SQLiteJournalMode.Wal;
                    options.SynchronousMode = SQLiteSynchronousMode.Normal;
                    options.BatchSizeLimit = 100;
                    options.BatchPeriod = TimeSpan.FromSeconds(1);
                })
                .CreateLogger();

            Log.Information("========================================");
            Log.Information("  GProject - Starting...");
            Log.Information("========================================");

            //Tải cấu hình
            _config = ConfigStorage.Load<AppConfig>() ?? new AppConfig();
           // _config.SetDefault();
            //ConfigStorage.Save(_config);

            // Initialize Auth database
            AuthDb.EnsureCreated();
            Log.Information("  Auth database initialized.");

            // Initialize PO databases
            GProduction.Initialize();
            Log.Information("  PO databases initialized.");

            // Initialize PLC recipe DB
            PLCRecipeDb.EnsureCreated();
            RecipeRegisterDb.EnsureCreated();
            Log.Information("  PLC recipe & registers database initialized.");


            DataPoolStatic.DataPath = DataPool.DefaultDataPath;

            _apiServer = new GProjectApiServer(_config.API_Port, _config.API_HostIP, (src, msg) =>
            {
                Log.Information("[{Source}] {Message}", src, msg);
            });

            try
            {
                await _apiServer.StartAsync();

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

                //khởi tạo heartbeat monitor cho PLC
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
