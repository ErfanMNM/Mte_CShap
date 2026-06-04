using System.ComponentModel;
using TSo;
using TSo.Configs;
using TSo.Api;
using TSo.Database;
using TSo.Services;

Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    Console.WriteLine("Shutting down...");
    Environment.Exit(0);
};

Console.WriteLine("=== TSo QR Activation Server ===");
Console.WriteLine($"Data folder: {AppConfigs.QRDatadbPath}");

GlobalState.Config.Load();
Console.WriteLine($"Server URL: {GlobalState.Config.Server_Host}");
Console.WriteLine($"PLC IP: {GlobalState.Config.PLC_IP}:{GlobalState.Config.PLC_Port} (Test: {GlobalState.Config.PLC_Test_Mode})");
Console.WriteLine($"Camera: {GlobalState.Config.Camera_01_IP}:{GlobalState.Config.Camera_01_Port}");
Console.WriteLine($"Scanner: {GlobalState.Config.Handheld_COM_Port}");
Console.WriteLine($"Line: {GlobalState.Config.Line_Name}");

Directory.CreateDirectory(Path.GetDirectoryName(AppConfigs.QRDatadbPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(AppConfigs.UsersDbPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(AppConfigs.LogPath)!);

QRDatabaseHelper.Init();
UserDatabaseHelper.Init(AppConfigs.UsersDbPath);

if (!UserDatabaseHelper.GetUserList(AppConfigs.UsersDbPath).Any())
{
    UserDatabaseHelper.AddUser("admin", "admin123", "Admin", AppConfigs.UsersDbPath);
    Console.WriteLine("Default admin user created: admin / admin123");
}

var logger = new Logger(AppConfigs.LogPath);

var authService = new AuthService(logger);
var qrService = new QRService(logger);
var plcService = new PLCService(logger, GlobalState.Config);
var cameraService = new CameraService(logger, GlobalState.Config);
var batchService = new BatchService(logger, GlobalState.Config);
var scannerService = new ScannerService(logger, GlobalState.Config);

qrService.OnQRActivated += record =>
{
    plcService.SendRejectResult(0);
};

cameraService.OnDataReceived += data =>
{
    if (!GlobalState.SystemStatus.IsDeactive && GlobalState.IsAuthenticated)
    {
        qrService.ProcessCameraQR(data, GlobalState.CurrentSession?.Username);
    }
};

scannerService.OnDataReceived += data =>
{
    if (!GlobalState.SystemStatus.IsDeactive && GlobalState.IsAuthenticated)
    {
        qrService.ProcessManualAdd(data, GlobalState.CurrentSession?.Username ?? "Scanner");
    }
};

plcService.Connect();
cameraService.Start();
scannerService.Connect();

batchService.GetCurrentBatch();
qrService.ReloadActiveSet();

Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Services initialized");

var server = new TSoApiServer(
    authService, qrService, plcService,
    cameraService, batchService, scannerService, logger,
    GlobalState.Config.Server_Host);

server.Start();
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] API server running at {GlobalState.Config.Server_Host}");

var wkDequeue = new BackgroundWorker();
wkDequeue.DoWork += (s, e) =>
{
    int tick = 0;
    while (!wkDequeue.CancellationPending)
    {
        try
        {
            while (GlobalState.QueueRecord.TryDequeue(out var record))
            {
                QRDatabaseHelper.AddOrActivateCode(record, AppConfigs.QRDatadbPath);
            }

            while (GlobalState.QueueActive.TryDequeue(out var activeRecord))
            {
                QRDatabaseHelper.AddActiveCodeUnique(activeRecord, AppConfigs.ActiveUniqueDbPath);
            }

            if (GlobalState.Config.Production_Speed_Mode == 1)
            {
                tick++;
                if (tick >= 20)
                {
                    tick = 0;
                    UpdateProductionPerHour();
                }
            }
        }
        catch { }

        Thread.Sleep(50);
    }
};
wkDequeue.RunWorkerAsync();

Console.WriteLine("Press Ctrl+C to stop.");

await Task.Delay(Timeout.Infinite);

void UpdateProductionPerHour()
{
    if (GlobalState.Config.Production_Speed_Mode != 1) return;

    var resetTimeout = GlobalState.Config.Production_Speed_Reset_Timeout;
    if (resetTimeout <= 0) resetTimeout = 30;

    long currentMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    long lastMs = GlobalState.LastProductTimestampMs;

    if (lastMs > 0)
    {
        double diffSeconds = (currentMs - lastMs) / 1000.0;
        if (diffSeconds > resetTimeout)
        {
            GlobalState.ProductionPerHour = 0;
        }
    }
}
