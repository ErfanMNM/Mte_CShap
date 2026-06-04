/*
 * TSo Program.cs — TSo QR Activation Server
 *
 * MỤC LỤC
 *   1. Kiến trúc tổng quan
 *   2. Bootstrapping
 *   3. Khởi tạo services
 *   4. Lắp ráp event handlers
 *   5. Khởi động thiết bị
 *   6. Khởi động background workers
 *
 * LUỒNG DỮ LIỆU (Data Flow)
 *
 *   Camera/Scanner  →  CameraService / ScannerService  →  QRService
 *                                                          │
 *                                          ┌───────────────┴───────────────┐
 *                                          │                               │
 *                                    GlobalState                    QueueRecord
 *                                    (RAM - ActiveSet)                  │
 *                                          │                               │
 *                                          │                    WK_Dequeue (BG Worker)
 *                                          │                               │
 *                                          │                     QRDatabaseHelper
 *                                          │                               │
 *                                    TSoApiServer ◄───── OnQRActivated ──┘
 *                                    (WebSocket push)
 *
 * TRẠNG THÁI HỆ THỐNG (System State Machine)
 *
 *   Login ──(login OK)──► Ready ──(camera/scanner data)──► Processing ──(done)──► Ready
 *     ▲                      │
 *     │                      ├──(logout)──────────────────────┘
 *     │                      │
 *     │                      └──(PLC Deactive DM = 1)──► Deactive ──(reactivate)──► Ready
 *
 * CÁC BACKGROUND WORKERS
 *   • WK_Dequeue : Dequeue QR record từ RAM → ghi xuống SQLite (không block device I/O)
 *   • OmronPLC_Hsl.WK_Update : Ping PLC mỗi PLC_Time_Refresh ms (quản lý bên trong PLCService)
 *   • WsBroadcastLoop : Gửi sự kiện real-time đến FE qua WebSocket (bên trong TSoApiServer)
 */

using System.ComponentModel;
using TSo;
using TSo.Configs;
using TSo.Api;
using TSo.Database;
using TSo.Services;

// ============================================================
// 1. BOOTSTRAPPING — Khởi tạo môi trường trước khi chạy
// ============================================================

// Ctrl+C hoặc Ctrl+Break → thoát graceful thay vì crash
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    Console.WriteLine("Shutting down...");
    Environment.Exit(0);
};

Console.WriteLine("=== TSo QR Activation Server ===");
Console.WriteLine($"Data folder: {AppConfigs.QRDatadbPath}");

// Load cấu hình từ App.ini trong thư mục AppData
// SetDefault() được gọi bên trong Load() để đảm bảo luôn có giá trị hợp lệ
GlobalState.Config.Load();

// In ra thông tin khởi động để operator kiểm tra
Console.WriteLine($"Server URL: {GlobalState.Config.Server_Host}");
Console.WriteLine($"PLC IP: {GlobalState.Config.PLC_IP}:{GlobalState.Config.PLC_Port} (Test: {GlobalState.Config.PLC_Test_Mode})");
Console.WriteLine($"Camera: {GlobalState.Config.Camera_01_IP}:{GlobalState.Config.Camera_01_Port}");
Console.WriteLine($"Scanner: {GlobalState.Config.Handheld_COM_Port}");
Console.WriteLine($"Line: {GlobalState.Config.Line_Name}");

// Tạo thư mục chứa dữ liệu nếu chưa tồn tại
// Path.GetDirectoryName trả về null nếu là root → dùng ! để suppress nullable warning
Directory.CreateDirectory(Path.GetDirectoryName(AppConfigs.QRDatadbPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(AppConfigs.UsersDbPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(AppConfigs.LogPath)!);

// ============================================================
// 2. KHỞI TẠO DATABASE
// ============================================================

// Init() tạo bảng nếu chưa có:
//   • QRProducts    : Lịch sử tất cả QR đã xử lý (Pass/Fail/Duplicate/...)
//   • ActiveUniqueQR : Danh sách QR đang active (dùng HashSet để check duplicate nhanh)
QRDatabaseHelper.Init();

// Khởi tạo database user (chứa account đăng nhập)
UserDatabaseHelper.Init(AppConfigs.UsersDbPath);

// Tạo tài khoản admin mặc định nếu chưa có user nào
// Giá trị: admin / admin123
if (!UserDatabaseHelper.GetUserList(AppConfigs.UsersDbPath).Any())
{
    UserDatabaseHelper.AddUser("admin", "admin123", "Admin", AppConfigs.UsersDbPath);
    Console.WriteLine("Default admin user created: admin / admin123");
}

// ============================================================
// 3. KHỞI TẠO SERVICES
// ============================================================

var logger = new Logger(AppConfigs.LogPath);

// AuthService : Quản lý đăng nhập, session token, 2FA
var authService = new AuthService(logger);

// QRService : Xử lý validate và kích hoạt QR code
//   • ProcessCameraQR : Validate + lưu + broadcast
//   • ProcessManualAdd: Thêm QR thủ công (từ máy quét cầm tay)
var qrService = new QRService(logger);

// PLCService : Giao tiếp Omron PLC qua FINS/UDP
//   • Kết nối đến PLC
//   • Gửi tín hiệu Ready/Reject/Reset/Clear
//   • Poll counter từ PLC mỗi PLC_Time_Refresh ms
var plcService = new PLCService(logger, GlobalState.Config);

// CameraService : Kết nối camera Datalogic qua TCP socket
//   • DatalogicCamera (TTManager) là TCP client, kết nối đến camera (camera là TCP server)
//   • Tự động reconnect + ping host khi mất kết nối
var cameraService = new CameraService(logger, GlobalState.Config);

// BatchService : Quản lý lô sản xuất (đọc/ghi batch từ database)
var batchService = new BatchService(logger, GlobalState.Config);

// ScannerService : Kết nối máy quét cầm tay qua Serial COM
//   • SerialClientHelper (TTManager) quản lý SerialPort
//   • DataReceived event được fire khi có dữ liệu từ máy quét
var scannerService = new ScannerService(logger, GlobalState.Config);

// ============================================================
// 4. LẮP RÁP EVENT HANDLERS — Pipeline xử lý dữ liệu
// ============================================================

// Khi QR được kích hoạt thành công → gửi tín hiệu Pass cho PLC
// PLC sẽ nhận D10 = 0 → cho phép sản phẩm đi qua
// (Fail/duplicate/etc. được xử lý bên trong QRService trước khi gọi event này)
qrService.OnQRActivated += record =>
{
    plcService.SendRejectResult(0);
};

// Khi camera gửi dữ liệu QR:
//   1. Bỏ qua nếu hệ thống đang Deactive (PLC yêu cầu dừng)
//   2. Bỏ qua nếu chưa có ai đăng nhập (tránh spam khi chưa auth)
//   3. Gọi QRService.ProcessCameraQR để validate và kích hoạt
//      QRService sẽ fire OnQRActivated → gửi tín hiệu PLC
cameraService.OnDataReceived += data =>
{
    if (!GlobalState.SystemStatus.IsDeactive && GlobalState.IsAuthenticated)
    {
        qrService.ProcessCameraQR(data, GlobalState.CurrentSession?.Username);
    }
};

// Khi máy quét cầm tay gửi dữ liệu:
//   1. Cùng logic guard như camera (Deactive + Auth check)
//   2. ProcessManualAdd khác ProcessCameraQR ở chỗ:
//      - Tự động ghi trực tiếp vào QRDatabase (không qua queue)
//      - Gắn nhãn nguồn "(Manual)" để phân biệt
//      - Không broadcast qua PLC (operator chủ động thêm)
scannerService.OnDataReceived += data =>
{
    if (!GlobalState.SystemStatus.IsDeactive && GlobalState.IsAuthenticated)
    {
        qrService.ProcessManualAdd(data, GlobalState.CurrentSession?.Username ?? "Scanner");
    }
};

// ============================================================
// 5. KHỞI ĐỘNG THIẾT BỊ
// ============================================================

// PLC: Khởi tạo kết nối FINS/UDP
//   • OmronPLC_Hsl.InitPLC() sẽ tạo persistent UDP pipe
//   • Bên trong InitPLC(), WK_Update BackgroundWorker được start
//   • WK_Update ping PLC mỗi PLC_Time_Refresh ms để:
//       - Gửi tín hiệu Ready (D16 = 1 khi đang chạy)
//       - Phát hiện mất kết nối → fire PLCStatus_OnChange event
plcService.Connect();

// Camera: Bắt đầu TCP client kết nối đến Datalogic camera
//   • DatalogicCamera.Connect() bắt đầu reconnect loop
//   • Ping camera mỗi 3s, nếu ping OK → thử kết nối TCP
//   • Khi nhận dữ liệu → fire ClientCallback → CameraService.Camera_Callback
cameraService.Start();

// Scanner: Mở cổng Serial COM cho máy quét cầm tay
//   • SerialClientHelper.Connect() mở SerialPort với baud 9600
//   • DataReceived event được fire khi có dữ liệu serial
//   • Nếu COM port chưa cấu hình → bỏ qua (chế độ camera-only)
scannerService.Connect();

// ============================================================
// 6. KHỞI TẠO TRẠNG THÁI BAN ĐẦU
// ============================================================

// Lấy lô sản xuất hiện tại từ database
// Nếu không có → mặc định BatchCode="NNN", Barcode="000"
batchService.GetCurrentBatch();

// Load danh sách QR đang active từ SQLite lên RAM (HashSet)
// Mục đích: check duplicate O(1) mà không cần truy vấn DB mỗi lần
// Khi server restart, ActiveSet được repopulate từ ActiveUniqueQR table
qrService.ReloadActiveSet();

Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Services initialized");

// ============================================================
// 7. KHỞI ĐỘNG API SERVER
// ============================================================

var server = new TSoApiServer(
    authService, qrService, plcService,
    cameraService, batchService, scannerService, logger,
    GlobalState.Config.Server_Host);

// TSoApiServer.Start() tạo 3 background tasks:
//   • ServerLoop       : HttpListener chờ request HTTP + WebSocket
//   • ProcessQueueLoop : Dequeue trạng thái hệ thống (deactive check, speed calc)
//   • WsBroadcastLoop   : Gửi sự kiện real-time đến FE qua WebSocket
server.Start();
Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] API server running at {GlobalState.Config.Server_Host}");

// ============================================================
// 8. BACKGROUND WORKER: DEQUEUE — Ghi QR record xuống SQLite
// ============================================================

// Tại sao dùng queue + background worker thay vì ghi trực tiếp?
//   • SQLite write là I/O blocking → nếu ghi trực tiếp trên main thread
//     khi camera gửi dữ liệu liên tục sẽ gây lag hoặc miss data
//   • Queue giải decoupling: producer (QRService) chỉ enqueue,
//     consumer (WK_Dequeue) chạy nền không ảnh hưởng tốc độ nhận diện
//
// Hai queue được dequeue song song:
//   • QueueRecord : QR đã được kích hoạt → ghi vào QRProducts (lịch sử)
//   • QueueActive : QR mới active → ghi vào ActiveUniqueQR (dùng cho reload khi restart)

var wkDequeue = new BackgroundWorker();
wkDequeue.DoWork += (s, e) =>
{
    int tick = 0;

    // Chạy vĩnh viễn cho đến khi CancellationPending
    // Thread.Sleep(50) → ~20 cycles/s là đủ nhanh cho usecase này
    while (!wkDequeue.CancellationPending)
    {
        try
        {
            // Dequeue tất cả QR record đang chờ từ QRService
            // Mỗi record được ghi vào QRProducts table với đầy đủ metadata
            while (GlobalState.QueueRecord.TryDequeue(out var record))
            {
                QRDatabaseHelper.AddOrActivateCode(record, AppConfigs.QRDatadbPath);
            }

            // Dequeue record để ghi vào bảng active unique
            // Bảng này dùng unique index trên QRContent → INSERT OR IGNORE
            while (GlobalState.QueueActive.TryDequeue(out var activeRecord))
            {
                QRDatabaseHelper.AddActiveCodeUnique(activeRecord, AppConfigs.ActiveUniqueDbPath);
            }

            // Cập nhật tốc độ sản xuất mỗi 20 cycles (~1 giây)
            // Production_Speed_Mode = 1: tính từ timestamp sản phẩm gần nhất
            // Production_Speed_Mode = 0: tính từ database (15 phút trước)
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
        catch
        {
            // Lỗi database → bỏ qua cycle này, không crash worker
            // Ví dụ: file locked, disk full, corruption
        }

        Thread.Sleep(50);
    }
};
wkDequeue.RunWorkerAsync();

Console.WriteLine("Press Ctrl+C to stop.");

// ============================================================
// 9. MAIN THREAD BLOCK — Giữ process chạy
// ============================================================

// Task.Delay(Timeout.Infinite) giữ main thread alive
// Ctrl+C sẽ trigger CancelKeyPress → Environment.Exit(0)
await Task.Delay(Timeout.Infinite);

// ============================================================
// 10. HELPER: Tính tốc độ sản xuất / giờ
// ============================================================

/*
 * Production_Speed_Mode = 1 (khuyến nghị cho dây chuyền nhanh)
 *
 * Cơ chế tính:
 *   1. Mỗi sản phẩm được kích hoạt → timestamp được lưu vào ProductTimestampSamples
 *   2. Giữ N mẫu gần nhất (Production_Speed_Sample_Count, default 10)
 *   3. Tốc độ = 3600 / thời_gian_trung_bình_giữa_N_sản_phẩm
 *
 * Reset:
 *   Nếu không có sản phẩm mới trong resetTimeout giây (default 30s)
 *   → ProductionPerHour = 0 (dây chuyền dừng)
 */
void UpdateProductionPerHour()
{
    // Chỉ chạy khi mode = 1 (mode 0 tính từ DB trong ProcessQueueLoop bên TSoApiServer)
    if (GlobalState.Config.Production_Speed_Mode != 1) return;

    // Timeout để reset tốc độ về 0 khi dây chuyền dừng
    var resetTimeout = GlobalState.Config.Production_Speed_Reset_Timeout;
    if (resetTimeout <= 0) resetTimeout = 30;

    long currentMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    long lastMs = GlobalState.LastProductTimestampMs;

    if (lastMs > 0)
    {
        double diffSeconds = (currentMs - lastMs) / 1000.0;
        // Nếu sản phẩm cuối cách đây > resetTimeout giây → dây chuyền dừng
        if (diffSeconds > resetTimeout)
        {
            GlobalState.ProductionPerHour = 0;
        }
    }
}
