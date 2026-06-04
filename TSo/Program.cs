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
Console.WriteLine($"PLC IP: {GlobalState.Config.PLC_IP}:{GlobalState.Config.PLC_Port}");
Console.WriteLine($"Camera: {GlobalState.Config.Camera_01_IP}:{GlobalState.Config.Camera_01_Port}");
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
        var result = qrService.ProcessManualAdd(data, GlobalState.CurrentSession?.Username ?? "Scanner");
    }
};

plcService.Connect();
await cameraService.StartAsync();
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
Console.WriteLine("Press Ctrl+C to stop.");

await Task.Delay(Timeout.Infinite);
