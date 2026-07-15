using Glib.Omron;
using GProject.DataPoolHelper;
using GProject.Auth;
using GProject.PLCHelpers;
using GProject.Production;
using GProject.ProductionOrderHelpers;
using GProject.Infrastructure;
using Raycoon.Serilog.Sinks.SQLite.Options;
using Serilog;
using Serilog.Events;

namespace GProject
{
    public class Program
    {
        private static OmronCodeReader? _CR_Camera;
        private static PLCMonitor? _plcMonitor;
        private static GProjectApiServer? _apiServer;

        /// <summary>Exposes the PLC monitor instance for API endpoints.</summary>
        public static PLCMonitor? GetPLCMonitor() => _plcMonitor;

        /// <summary>Khởi tạo AWS IoT từ config</summary>
        private static void InitAWS()
        {
            // Load config from environment variables hoặc config file
            var config = new AWSIoTConfig
            {
                Enabled = Environment.GetEnvironmentVariable("AWS_ENABLED")?.ToLower() == "true",
                DevMode = Environment.GetEnvironmentVariable("AWS_DEV_MODE")?.ToLower() == "true",
                Endpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT") ?? "",
                ClientId = Environment.GetEnvironmentVariable("AWS_CLIENT_ID") ?? "GProject",
                RootCAPath = Environment.GetEnvironmentVariable("AWS_ROOT_CA_PATH") ?? "",
                ClientCertPath = Environment.GetEnvironmentVariable("AWS_CLIENT_CERT_PATH") ?? "",
                ClientCertPassword = Environment.GetEnvironmentVariable("AWS_CLIENT_CERT_PASSWORD") ?? "",
                AutoSend = Environment.GetEnvironmentVariable("AWS_AUTO_SEND")?.ToLower() == "true",
                ThingName = Environment.GetEnvironmentVariable("AWS_THING_NAME") ?? "GProject"
            };

            G.AWS = config;

            if (config.Enabled)
            {
                ProductionStateMachine.Instance.InitAWS(config);
                Log.Information("[AWS] AWS IoT initialized. Endpoint: {Endpoint}, ClientId: {ClientId}",
                    config.Endpoint, config.ClientId);

                // Auto connect nếu được bật
                if (config.AutoSend)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(2000); // Đợi app khởi động xong
                        await ProductionStateMachine.Instance.ConnectAWSAsync();
                    });
                }
            }
            else
            {
                Log.Information("[AWS] AWS IoT is disabled");
            }
        }

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
                Log.Information("  Device Status:   http://localhost:9999/api/devices/status");
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

                // Khởi tạo AWS IoT nếu được bật
                InitAWS();

                // Khởi tạo 1 camera duy nhất (vừa active vừa phân làn)
                _CR_Camera = new OmronCodeReader(OmronCodeReader.e_CodeReaderModel.V430, "127.0.0.1", 9001);
                _CR_Camera.ClientCallback += (state, data) => OnCameraEvent("camera", state, data);
                _CR_Camera.Connect();
                Log.Information("[Main] Camera initialized: camera=127.0.0.1:9001");

                // Khoi tao PLC Monitor (Omron FINS/UDP) - heartbeat + counter polling.
                // Broadcasts connection state to FE via /ws/plc.

                // Load map địa chỉ PLC từ Google Sheet tab 'VINA CF' (cùng spreadsheet MasanQR mà TApp dùng).
                // Helper sẽ tự fallback về cache local nếu Sheet không truy cập được.
                const string spreadsheetId = "1V2xjY6AA4URrtcwUorQE54Ud5KyI7Ev2hpDPMMcXVTI";
                bool sheetLoaded = PLCAddressWithGoogleSheetHelper.Init(
                    spreadsheetId,
                    Environment.GetEnvironmentVariable("PLC_GOOGLE_SHEET_RANGE") ?? "VINA CF!A1:D100");
                Log.Information("[PLC] Address map loaded from {Source}, keys: {Count}",
                    sheetLoaded ? "Google Sheet" : "local cache",
                    PLCAddressWithGoogleSheetHelper.AddressMap.Count);

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
