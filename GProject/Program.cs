using Glib.Omron;
using GProject.DataPoolHelper;
using GProject.Auth;
using GProject.PLCHelpers;
using GProject.Production;
using GProject.ProductionOrderHelpers;
using GProject.Infrastructure;
using Raycoon.Serilog.Sinks.SQLite.Options;
using Serilog;
using GProject.Config;
using Glib.PLCHelpers;
using System.ComponentModel;
using HslCommunication;

namespace GProject
{
    public class Global
    {
        public static OmronPLC_Hsl? omronPLC;
        public static OmronPLC_Hsl.PLCStatus PLC_STATUS = OmronPLC_Hsl.PLCStatus.Disconnect;
    }
    public class Program
    {
    private static OmronCodeReader? _CR_Camera;
    private static GProjectApiServer? _apiServer;
    public static AppConfig? _config;
    public static BackgroundWorker? _cameraWK;

        /// <summary>Exposes the PLC monitor instance for API endpoints.</summary>

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

        #region PLC DEVICE CONFIG

        #endregion

        static async Task Main(string[] args)
        {

            //khởi động background service để xử lý camera

            _cameraWK = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _cameraWK.DoWork += _cameraWK_DoWork;

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

                // Khởi tạo AWS IoT nếu được bật
                InitAWS();
                // Khởi tạo 1 camera duy nhất (vừa active vừa phân làn)
                _CR_Camera = new OmronCodeReader(OmronCodeReader.e_CodeReaderModel.V430, "127.0.0.1", 9001);
                _CR_Camera.ClientCallback += (state, data) => OnCameraEvent("camera", state, data);
                _CR_Camera.Connect();
                Log.Information("[Main] Camera initialized: camera=127.0.0.1:9001");

                // Khởi tạo và cấu hình PLC
                const string spreadsheetId = "1V2xjY6AA4URrtcwUorQE54Ud5KyI7Ev2hpDPMMcXVTI";
                bool sheetLoaded = PLCAddressWithGoogleSheetHelper.Init(
                    spreadsheetId,
                    Environment.GetEnvironmentVariable("PLC_GOOGLE_SHEET_RANGE") ?? "VINA CF!A1:D100");
                Log.Information("[PLC] Address map loaded from {Source}, keys: {Count}",
                    sheetLoaded ? "Google Sheet" : "local cache",
                    PLCAddressWithGoogleSheetHelper.AddressMap.Count);

                Global.omronPLC.PLC_Ready_DM = PLCAddressWithGoogleSheetHelper.Get("PLC2_Ready_DM") ?? "D16";
                Global.omronPLC.PLC_IP = _config.PLC_IP ?? "127.0.0.1";
                Global.omronPLC.PLC_PORT = _config.PLC_Port > 0 ? _config.PLC_Port : 9600;
                Global.omronPLC.InitPLC();

                Global.omronPLC.PLCStatus_OnChange += OmronPLC_PLCStatus_OnChange;

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

                // Dừng state machine
                try { await ProductionStateMachine.Instance.StopAsync(); }
                catch (Exception ex) { Log.Warning(ex, "[Main] Lỗi khi dừng StateMachine"); }

                Log.Information("GProject stopped.");
                Log.CloseAndFlush();
            }
        }

        private static void _cameraWK_DoWork(object? sender, DoWorkEventArgs e)
        {
            string rawcode = e.Argument as string ?? "";
            ProductionStateMachine.HandleCodeFromCamera(rawcode);
        }

        private static void OmronPLC_PLCStatus_OnChange(object? sender, OmronPLC_Hsl.PLCStatusEventArgs e)
        {
            switch (e.Status)
            {
                case OmronPLC_Hsl.PLCStatus.Connected:
                    if (Global.PLC_STATUS != OmronPLC_Hsl.PLCStatus.Connected)
                    {
                        Global.PLC_STATUS = OmronPLC_Hsl.PLCStatus.Connected;
                    }
                    Log.Information("[PLC] Connected: {Message}", e.Message);
                    ProductionStateMachine.Instance.OnDeviceStateChanged("PLC", true, e.Message);
                    break;
                case OmronPLC_Hsl.PLCStatus.Disconnect:
                    if (Global.PLC_STATUS != OmronPLC_Hsl.PLCStatus.Disconnect)
                    {
                        Global.PLC_STATUS = OmronPLC_Hsl.PLCStatus.Disconnect;
                    }
                    Log.Warning("[PLC] Disconnected: {Message}", e.Message);
                    ProductionStateMachine.Instance.OnDeviceStateChanged("PLC", false, e.Message);
                    break;
                default:
                    if (Global.PLC_STATUS != OmronPLC_Hsl.PLCStatus.Disconnect)
                    {
                        Global.PLC_STATUS = OmronPLC_Hsl.PLCStatus.Disconnect;
                    }
                    Log.Information("[PLC] Status changed: {Status} - {Message}", e.Status, e.Message);
                    break;
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

                    if (ProductionStateMachine.Instance.CurrentState == e_ProductionState.Running || ProductionStateMachine.Instance.CurrentState == e_ProductionState.WaitingStop)
                    {
                        if (_cameraWK.IsBusy)
                        {
                            Log.Debug("[Camera:{Camera}] Bận xử lý, reject mã: {Data}", camera, data);
                            OperateResult result = Global.omronPLC.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reject_DM"), 0); // Ghi 0 xuống PLC
                        }
                        else
                        {
                            _cameraWK.RunWorkerAsync(data);
                        }
                        
                    }
                    else
                    {
                        //trả trạng thái cho ws (fe)
                        Log.Debug("[Camera:{Camera}] State is not Running or WaitingStop, reject code: {Data}", camera, data);
                        OperateResult result = Global.omronPLC.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Reject_DM"), 0); // Ghi 0 xuống PLC
                    }
                    break;

                default:
                    break;
            }
        }



    }
}
