using Glib.Omron;
using Glib.PLCHelpers;
using GProject.Auth;
using GProject.Config;
using GProject.DataPoolHelper;
using GProject.Infrastructure;
using GProject.PLCHelpers;
using GProject.Production;
using GProject.ProductionOrderHelpers;
using HslCommunication;
using Raycoon.Serilog.Sinks.SQLite.Options;
using Serilog;
using System.ComponentModel;

namespace GProject
{
    public class Global
    {
        public static OmronPLC_Hsl? omronPLC;
        public static OmronPLC_Hsl.PLCStatus PLC_STATUS = OmronPLC_Hsl.PLCStatus.Disconnect;
        public static eOmronCodeReaderState Camera_STATUS = eOmronCodeReaderState.Disconnected;
    }
    public class Program
    {
    private static OmronCodeReader? _CR_Camera;
    private static GProjectApiServer? _apiServer;
    public static AppConfig? _config;

        // Single shared gate so only one camera payload can run the pipeline at a time.
        // Replaces BackgroundWorker.IsBusy which was racy across the TCP callback thread.
        private static readonly SemaphoreSlim _cameraGate = new(1, 1);

        /// <summary>Exposes the PLC monitor instance for API endpoints.</summary>
        public static PLCMonitorLite GetPLCMonitor() => PLCMonitorLite.Instance;

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
           //_config.SetDefault();
          // ConfigStorage.Save(_config);

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

                // Khởi tạo và cấu hình PLC TRƯỚC camera — backend chỉ dùng 1 kết nối PLC
                // duy nhất (Global.omronPLC), toàn bộ code (camera pipeline, API) cùng
                // chia sẻ instance này để tránh 2 kết nối song song.
                const string spreadsheetId = "1V2xjY6AA4URrtcwUorQE54Ud5KyI7Ev2hpDPMMcXVTI";
                PLCAddressWithGoogleSheetHelper.FilePath = Path.Combine($@"C:/GProject/Configs/Google.json");
                bool sheetLoaded = PLCAddressWithGoogleSheetHelper.Init(
                   spreadsheetId,"VINA_CF!A1:D100");

                //Log.Information("[PLC] Address map loaded from {Source}, keys: {Count}",
                //    sheetLoaded ? "Google Sheet" : "local cache",
                //    PLCAddressWithGoogleSheetHelper.AddressMap.Count);

                Global.omronPLC = new OmronPLC_Hsl();
                Global.omronPLC.PLC_Ready_DM = PLCAddressWithGoogleSheetHelper.Get("PLC_Ready_DM") ?? "D16";
                Global.omronPLC.PLC_IP = PLCAddressWithGoogleSheetHelper.Get("PLC_IP") ?? "127.0.0.1";
                Global.omronPLC.PLC_PORT = _config.PLC_Port > 0 ? _config.PLC_Port : 9600;
                Global.omronPLC.InitPLC();
                Global.omronPLC.PLCStatus_OnChange += OmronPLC_PLCStatus_OnChange;
                Log.Information("[Main] PLC initialized: {IP}:{Port}", Global.omronPLC.PLC_IP, Global.omronPLC.PLC_PORT);

                // Khởi tạo wrapper PLCMonitorLite cho API recipe/register endpoints.
                // PLCMonitorLite.Instance được khởi tạo lazy qua static readonly.

                // Apply ACK V2 config từ AppConfig xuống state machine.
                ProductionStateMachine.Instance.ApplyAckConfig(
                    _config.CameraAckTimeoutMs, _config.CameraAckPollIntervalMs);

                // Khởi tạo 1 camera duy nhất (vừa active vừa phân làn)
                string cameraIp = _config.Camera_Ip ?? "127.0.0.1";
                int cameraPort = _config.Camera_Port;
                _CR_Camera = new OmronCodeReader(OmronCodeReader.e_CodeReaderModel.V430, cameraIp, cameraPort);
                _CR_Camera.ClientCallback += (state, data) => OnCameraEvent("camera", state, data);
                _CR_Camera.Connect();
                Log.Information("[Main] Camera initialized: camera={IP}:{Port}", cameraIp, cameraPort);

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

        /// <summary>
        /// Camera callback — không bao giờ block thread TCP. Mọi quyết định xử lý phải đi qua
        /// atomic gate + background worker để tránh race giữa check busy và start worker.
        /// - Nếu gate bận / state không cho phép xử lý: ghi 0 xuống PLC NGAY rồi bỏ qua,
        ///   KHÔNG lookup, KHÔNG activate, KHÔNG enqueue DB cho mã này.
        /// - Nếu gate rảnh + state hợp lệ: đẩy payload vào worker tuần tự.
        /// </summary>
        private static void OnCameraEvent(string camera, eOmronCodeReaderState state, string data)
        {
            Log.Information("[Camera:{Camera}] [{State}] {Data}", camera, state, data);

            // Connection-state events: chỉ broadcast + đẩy trạng thái xuống state machine.
            switch (state)
            {
                case eOmronCodeReaderState.Connected:
                    if(Global.Camera_STATUS != eOmronCodeReaderState.Connected)
                    {
                        Global.Camera_STATUS = eOmronCodeReaderState.Connected;
                        _ = CameraHub.Instance.BroadcastAsync(camera, state, data);
                        ProductionStateMachine.Instance.OnDeviceStateChanged("Camera", data);
                    }
                    
                    break;
                case eOmronCodeReaderState.Disconnected:
                    if(Global.Camera_STATUS != eOmronCodeReaderState.Disconnected)
                    {
                        Global.Camera_STATUS = eOmronCodeReaderState.Disconnected;
                        _ = CameraHub.Instance.BroadcastAsync(camera, state, data);
                        ProductionStateMachine.Instance.OnDeviceStateChanged("Camera", data);
                    }

                    break;
                case eOmronCodeReaderState.Reconnecting:
                    if(Global.Camera_STATUS != eOmronCodeReaderState.Reconnecting)
                    {
                        Global.Camera_STATUS = eOmronCodeReaderState.Reconnecting;
                        _ = CameraHub.Instance.BroadcastAsync(camera, state, data);
                        ProductionStateMachine.Instance.OnDeviceStateChanged("Camera", data);
                    }
                    break;

                case eOmronCodeReaderState.Received:
                    HandleCameraReceived(camera, data);
                    break;

                default:
                    break;
            }
        }

        private static void HandleCameraReceived(string camera, string data)
        {
            if (data == null) return;

            var stateMachine = ProductionStateMachine.Instance;
            bool canProcess = stateMachine.CurrentState == e_ProductionState.Running
                              || stateMachine.CurrentState == e_ProductionState.WaitingStop;

            // Atomic try-enter gate. Nếu đang bận (gate không nhận) → ghi 0 xuống PLC NGAY,
            // vẫn phát history + CodeScanned cho FE để dashboard thấy mã bị loại do bận.
            if (!_cameraGate.Wait(0))
            {
                Log.Debug("[Camera:{Camera}] Gate bận, reject mã: {Data}", camera, data);
                EmitBusyResult(camera, data);
                return;
            }

            if (!canProcess)
            {
                _cameraGate.Release();
                Log.Debug("[Camera:{Camera}] State={State}, reject mã: {Data}", camera, stateMachine.CurrentState, data);
                EmitBusyResult(camera, data);
                return;
            }

            // Gate đã giữ — release ngay để các payload kế tiếp có thể ghi 0 NGAY nếu worker
            // chưa xong (gate chỉ đảm bảo không 2 worker chạy pipeline cùng lúc; quyết định
            // "có xử lý hay bận" vẫn dựa trên atomic try-enter ở đây).
            // Lưu ý: để tránh 2 worker chạy song song, ta KHÔNG release ngay — worker sẽ release.
            _ = DispatchCameraPayload(camera, data);
        }

        /// <summary>
        /// Worker xử lý payload camera đã qua gate. Tách riêng để mọi nhánh đều giải phóng gate.
        /// </summary>
        private static async Task DispatchCameraPayload(string camera, string data)
        {
            try
            {
                var result = await Task.Run(
                    () => ProductionStateMachine.Instance.HandleCodeFromCamera(data, camera));

                if (result != null)
                {
                    CameraHub.Instance.RecordHistory(result);
                    await CameraHub.Instance.BroadcastCodeStatus(result);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Camera:{Camera}] Lỗi xử lý payload: {Data}", camera, data);
            }
            finally
            {
                _cameraGate.Release();
            }
        }

        /// <summary>
        /// Phát CodeScanned + ghi history cho mã bị reject vì gate bận hoặc state không cho phép.
        /// PLC phải nhận 0 NGAY; mã này KHÔNG vào lookup/activate/DB queue.
        /// </summary>
        private static void EmitBusyResult(string camera, string data)
        {
            string codePart;
            string statusPart;
            var parts = data.Split('|');
            if (parts.Length == 2)
            {
                codePart = parts[0];
                statusPart = parts[1];
            }
            else
            {
                codePart = data;
                statusPart = "NO_READ";
            }

            var at = DateTime.UtcNow;
            var plcResult = WritePlcLane(0);
            var status = statusPart switch
            {
                "REJECT" => e_Production_Status.FormatError,
                "OK" => e_Production_Status.Error, // gate bận nhưng status OK → báo lỗi chứ không Pass
                _ => e_Production_Status.ReadFail,
            };

            var record = new CameraReadResult(
                camera, codePart, status, plcResult, null, null, at);

            // Vẫn ghi audit để FE thấy mã bị loại + DB record queue có entry.
            ProductionStateMachine.Instance.RecordBusyRejection(record, plcResult);

            CameraHub.Instance.RecordHistory(record);
            _ = CameraHub.Instance.BroadcastCodeStatus(record);
        }

        private static bool WritePlcLane(short value)
        {
            try
            {
                var plc = Global.omronPLC;
                if (plc?.plc == null) return false;
                string address = SafeGetAddress("PLC_Reject_DM_C1");
                var op = plc.plc.Write(address, value);
                if (!op.IsSuccess)
                {
                    Log.Warning("[PLC] Write {Addr}={Val} fail: {Msg}", address, value, op.Message);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[PLC] Write {Val} threw", value);
                return false;
            }
        }

        private static string SafeGetAddress(string key)
        {
            try { return PLCAddressWithGoogleSheetHelper.Get(key); }
            catch { return key switch
            {
                "PLC_Reject_DM_C1" => "D200",
                _ => "D200",
            }; }
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
                    ProductionStateMachine.Instance.OnDeviceStateChanged("PLC", e.Message);
                    break;
                case OmronPLC_Hsl.PLCStatus.Disconnect:
                    if (Global.PLC_STATUS != OmronPLC_Hsl.PLCStatus.Disconnect)
                    {
                        Global.PLC_STATUS = OmronPLC_Hsl.PLCStatus.Disconnect;
                    }
                    Log.Warning("[PLC] Disconnected: {Message}", e.Message);
                    ProductionStateMachine.Instance.OnDeviceStateChanged("PLC", e.Message);
                    break;
                default:
                    if (Global.PLC_STATUS != OmronPLC_Hsl.PLCStatus.Disconnect)
                    {
                        Global.PLC_STATUS = OmronPLC_Hsl.PLCStatus.Disconnect;
                    }
                    break;
            }
        }

    }
}