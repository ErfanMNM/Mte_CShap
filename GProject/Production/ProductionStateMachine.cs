using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using GProject.ProductionOrderHelpers;
using GProject.DataPoolHelper;
using GProject.IoT;
using GProject.PLCHelpers;
using Glib.Omron;
using Glib.PLCHelpers;
using Serilog;
using GProject.Infrastructure;

namespace GProject.Production
{
    /// <summary>
    /// Vòng lặp State Machine kiểm soát trạng thái sản xuất của ứng dụng.
    /// Tham khảo mẫu từ FDashboard.cs - Process_Production_State() chạy mỗi 100ms.
    ///
    /// Luồng trạng thái (theo plan.md):
    /// NeedLogin -> Checking -> Editing/LoadPO -> CheckPO -> LoadPO -> Ready
    ///            -> PushDataToDic -> Running <-> Paused -> CheckAfterCompleted -> Completed
    /// </summary>
    public class ProductionStateMachine
    {
        private static readonly Lazy<ProductionStateMachine> _instance = new(() => new ProductionStateMachine());
        public static ProductionStateMachine Instance => _instance.Value;

        // #region agent log
        private static readonly object AgentDebugLogLock = new();

        private static void AgentDebugLog(
            string hypothesisId,
            string message,
            object data,
            [System.Runtime.CompilerServices.CallerFilePath] string file = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            try
            {
                var payload = new
                {
                    sessionId = "7016d0",
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = $"{Path.GetFileName(file)}:{line}",
                    message,
                    data,
                    runId = "pre-fix",
                    hypothesisId
                };
                string entry = System.Text.Json.JsonSerializer.Serialize(payload) + Environment.NewLine;
                lock (AgentDebugLogLock)
                {
                    File.AppendAllText(@"C:\Users\THUC\source\repos\Mte_CShap\debug-7016d0.log", entry);
                }
            }
            catch
            {
            }
        }
        // #endregion

        private readonly object _stateLock = new();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private e_ProductionState _currentState = e_ProductionState.NeedLogin;
        private e_ProductionState _previousState = e_ProductionState.NeedLogin;

        // Background DB writer (3 queue + 1 consumer thread duy nhất)
        public ConcurrentQueue<RecordData> RecordQueue { get; } = new();
        public ConcurrentQueue<CodeUpdateItem> CodeUpdateQueue { get; } = new();
        public ConcurrentQueue<CartonUpdateItem> CartonUpdateQueue { get; } = new();

        public ConcurrentDictionary<int, CartonInfo > Dictionary_Cartons { get; set; } = new();

        private CancellationTokenSource? _writerCts;
        private Task? _writerTask;

        // Dữ liệu PO hiện tại trong RAM
        public static POInfo? ProductionData { get; set; }

        // Dictionary lưu mã sản phẩm cho lookup nhanh
        public ConcurrentDictionary<string, CodeInfo> Dictionary_Codes { get; } = new();

        // Dictionary lưu thông tin carton
        

        // Bộ đếm
        public ProductCounter ActiveCounter { get; private set; } = new();

        /// <summary>
        /// Cập nhật ActiveCounter.TotalCount từ PLC.
        /// </summary>
        public void UpdateActiveCounterTotal(int totalCount)
        {
            lock (_stateLock)
            {
                if (totalCount > ActiveCounter.TotalCount)
                    ActiveCounter.TotalCount = totalCount;
            }
        }
        public ProductCounter PackageCounter { get; private set; } = new();

        // Cờ trạng thái thiết bị
        public bool IsAppReady { get; set; }
        public bool IsDeviceReady { get; set; }
        public string LastWarning { get; set; } = "";

        // Trạng thái kết nối thiết bị - đọc từ Global (single source of truth)
        public bool IsPLCConnected => Global.PLC_STATUS == OmronPLC_Hsl.PLCStatus.Connected;
        public bool IsCameraConnected => Global.Camera_STATUS == eOmronCodeReaderState.Connected;
        private string _deviceDisconnectReason = "";

        // Cờ đánh dấu đang xử lý mã từ camera (để tránh race condition khi camera gửi liên tục)
        private volatile bool _isProcessingCameraCode = false;
        public bool IsProcessingCameraCode => _isProcessingCameraCode;
        public void SetProcessingCameraCode(bool value) => _isProcessingCameraCode = value;

        // Cấu hình
        public int CartonWarning { get; set; } = 3;
        public int CartonOffset { get; set; } = 1;

        // AWS IoT
        private AWSIoTClient? _awsClient;

        /// <summary>Khởi tạo AWS IoT client</summary>
        public void InitAWS(AWSIoTConfig config)
        {
            if (!config.Enabled)
            {
                Log.Information("[AWS] AWS IoT disabled");
                return;
            }

            try
            {
                _awsClient = new AWSIoTClient(config.ToAWSConfig());
                _awsClient.OnStatusChanged += (s, e) =>
                {
                    Log.Information("[AWS] Status: {Status} - {Message}", e.Status, e.Message);
                };
                _awsClient.OnMessageReceived += (s, e) =>
                {
                    Log.Information("[AWS] Received on {Topic}: {Payload}", e.Topic, e.Payload);
                    HandleAWSMessage(e.Topic, e.Payload);
                };
                _awsClient.OnMessageSent += (s, e) =>
                {
                    if (e.Success)
                        Log.Debug("[AWS] Sent to {Topic}: {Payload}", e.Topic, e.Payload);
                    else
                        Log.Warning("[AWS] Send failed to {Topic}: {Message}", e.Topic, e.Message);
                };
                Log.Information("[AWS] AWS IoT client initialized with endpoint: {Endpoint}", config.Endpoint);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AWS] Failed to initialize AWS IoT client");
            }
        }

        /// <summary>Kết nối AWS IoT</summary>
        public async Task<bool> ConnectAWSAsync()
        {
            if (_awsClient == null)
            {
                Log.Warning("[AWS] AWS client not initialized");
                return false;
            }

            var success = await _awsClient.ConnectAsync();
            if (success)
            {
                // Subscribe to response and command topics
                var config = G.AWS;
                await _awsClient.SubscribeAsync(config.ResponseTopic);
                await _awsClient.SubscribeAsync(config.CommandTopic);
                Log.Information("[AWS] Connected and subscribed to topics");
            }
            return success;
        }

        /// <summary>Ngắt kết nối AWS</summary>
        public async Task DisconnectAWSAsync()
        {
            if (_awsClient != null)
            {
                await _awsClient.DisconnectAsync();
            }
        }

        /// <summary>Gửi dữ liệu mã lên AWS</summary>
        public void SendCodeToAWS(string code, string status, string activateDate, string cartonCode)
        {
            if (_awsClient == null || !_awsClient.IsConnected)
            {
                Log.Debug("[AWS] Skipping send - not connected");
                return;
            }

            if (ProductionData == null)
            {
                Log.Debug("[AWS] Skipping send - no ProductionData");
                return;
            }

            var payload = new AWSSendPayload
            {
                message_id = $"{Guid.NewGuid():N}-{ProductionData.OrderNo}-{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}",
                orderNo = ProductionData.OrderNo,
                uniqueCode = code,
                gtin = ProductionData.Gtin ?? "",
                cartonCode = cartonCode,
                status = status == "Pass" ? 1 : 0,
                activate_datetime = activateDate,
                production_date = ProductionData.ProductionDate ?? "",
                thing_name = G.AWS.ThingName
            };

            _awsClient.PublishPayload(G.AWS.PublishTopic, payload);
            Log.Debug("[AWS] Queued code {Code} for send", code);
        }

        /// <summary>Gửi thông tin thùng hoàn thành lên AWS</summary>
        public void SendCartonToAWS(string cartonCode, string cartonId, int itemCount, string completedDatetime, string user)
        {
            if (_awsClient == null || !_awsClient.IsConnected)
            {
                Log.Debug("[AWS] Skipping carton send - not connected");
                return;
            }

            if (ProductionData == null) return;

            var payload = new AWSCartonPayload
            {
                message_id = $"{Guid.NewGuid():N}-{ProductionData.OrderNo}-CARTON-{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}",
                orderNo = ProductionData.OrderNo,
                cartonCode = cartonCode,
                cartonId = cartonId,
                itemCount = itemCount,
                completed_datetime = completedDatetime,
                activateUser = user,
                thing_name = G.AWS.ThingName
            };

            _awsClient.PublishCartonPayload("CZ/carton", payload);
            Log.Debug("[AWS] Queued carton {CartonCode} for send", cartonCode);
        }

        /// <summary>Gửi thông tin PO started/completed lên AWS</summary>
        public void SendPOEventToAWS(string eventType)
        {
            if (_awsClient == null || !_awsClient.IsConnected)
            {
                Log.Debug("[AWS] Skipping PO event - not connected");
                return;
            }

            if (ProductionData == null) return;

            var payload = new AWSPOPayload
            {
                message_id = $"{Guid.NewGuid():N}-{ProductionData.OrderNo}-{eventType.ToUpper()}-{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}",
                orderNo = ProductionData.OrderNo,
                event_type = eventType,
                gtin = ProductionData.Gtin ?? "",
                orderQty = ProductionData.OrderQty,
                productionDate = ProductionData.ProductionDate ?? "",
                line = ProductionData.ProductionLine ?? "",
                user = G.AWS.ThingName,
                thing_name = G.AWS.ThingName
            };

            _awsClient.PublishPOPayload($"CZ/po/{eventType}", payload);
            Log.Information("[AWS] Queued PO {EventType} for order {OrderNo}", eventType, ProductionData.OrderNo);
        }

        /// <summary>Xử lý message nhận từ AWS</summary>
        private void HandleAWSMessage(string topic, string payload)
        {
            try
            {
                if (topic.Contains("/command"))
                {
                    // Xử lý command từ cloud
                    Log.Information("[AWS] Received command: {Payload}", payload);
                    // TODO: Implement command handling (e.g., start/stop production)
                }
                else if (topic.Contains("/response"))
                {
                    // Xử lý response
                    var response = System.Text.Json.JsonSerializer.Deserialize<AWSResponse>(payload);
                    if (response != null)
                    {
                        Log.Information("[AWS] Response for {MessageId}: {Status}", response.message_id, response.status);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[AWS] Error handling message: {Error}", ex.Message);
            }
        }

        /// <summary>Trạng thái AWS IoT</summary>
        public AWSIoTStatus AWSStatus => _awsClient?.Status ?? AWSIoTStatus.Disconnected;

        /// <summary>Internal reference to AWS client for API access</summary>
        public AWSIoTClient? AWSClientInternal => _awsClient;

        /// <summary>Số message đang chờ gửi</summary>
        public int AWSPendingCount => _awsClient?.PendingCount ?? 0;

        public e_ProductionState CurrentState
        {
            get { lock (_stateLock) return _currentState; }
        }

        public e_ProductionState PreviousState
        {
            get { lock (_stateLock) return _previousState; }
        }

        private ProductionStateMachine()
        {
            // Device status được đọc từ Global, không cần khởi tạo ở đây
        }

        /// <summary>
        /// Bắt đầu vòng lặp state machine chạy nền
        /// </summary>
        public void Start()
        {
            if (_loopTask != null && !_loopTask.IsCompleted)
            {
                Log.Warning("[StateMachine] Already running");
                return;
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _loopTask = Task.Run(() => RunLoop(token), token);
            StartDbWriter();
            Log.Information("[StateMachine] Started");
        }

        /// <summary>
        /// Dừng vòng lặp
        /// </summary>
        public async Task StopAsync()
        {
            _cts?.Cancel();
            if (_loopTask != null)
            {
                try { await _loopTask; }
                catch (OperationCanceledException) { }
            }
            await StopDbWriterAsync();
            Log.Information("[StateMachine] Stopped");
        }

        /// <summary>
        /// Chuyển trạng thái (thread-safe) + broadcast qua WebSocket
        /// </summary>
        public void SetState(e_ProductionState newState, string? reason = null)
        {
            lock (_stateLock)
            {
                if (_currentState == newState)
                {
                    Log.Debug("[StateMachine] SetState no-op: {State} already set (reason: {Reason})",
                        newState, reason ?? "no reason");
                    return;
                }
                _previousState = _currentState;
                _currentState = newState;
            }
            Log.Information("[StateMachine] {Prev} -> {New} ({Reason})",
                _previousState, newState, reason ?? "no reason");

            // #region agent log
            if (newState is e_ProductionState.Running or e_ProductionState.WaitCartonCode)
            {
                int cartonId = ActiveCounter.CartonID;
                AgentDebugLog("H2,H3", "Relevant production state transition", new
                {
                    previousState = _previousState.ToString(),
                    newState = newState.ToString(),
                    reason = reason ?? "no reason",
                    cartonId,
                    packedCount = PackageCounter.PassTotal,
                    cartonCapacity = ActiveCounter.CartonCapacity,
                    currentCartonHasCode = Dictionary_Cartons.TryGetValue(cartonId, out var currentCarton)
                        && !string.IsNullOrEmpty(currentCarton.CartonCode)
                        && currentCarton.CartonCode != "0",
                    nextCartonHasCode = Dictionary_Cartons.TryGetValue(cartonId + 1, out var nextCarton)
                        && !string.IsNullOrEmpty(nextCarton.CartonCode)
                        && nextCarton.CartonCode != "0"
                });
            }
            // #endregion
        }

        /// <summary>
        /// Reset về trạng thái đầu khi logout
        /// </summary>
        public void ResetForLogout()
        {
            ProductionData = null;
            Dictionary_Codes.Clear();
           Dictionary_Cartons.Clear();
            ActiveCounter = new ProductCounter();
            PackageCounter = new ProductCounter();

            // Xoá 3 queue tránh giữ dữ liệu của PO cũ
            while (RecordQueue.TryDequeue(out _)) { }
            while (CodeUpdateQueue.TryDequeue(out _)) { }
            while (CartonUpdateQueue.TryDequeue(out _)) { }

            SetState(e_ProductionState.NeedLogin, "logout");
        }

        /// <summary>
        /// Cập nhật ProductionDate cho PO hiện tại
        /// Chỉ cho phép khi đang ở trạng thái Ready
        /// </summary>
        /// <param name="newDate">Ngày sản xuất mới (format: yyyy-MM-dd hoặc yyyy-MM-dd HH:mm:ss)</param>
        /// <param name="userName">Người dùng thực hiện</param>
        /// <returns>true nếu cập nhật thành công, false nếu không thành công</returns>
        public (bool success, string message) UpdateProductionDate(string newDate, string userName)
        {
            if (CurrentState != e_ProductionState.Ready)
            {
                return (false, $"Không thể sửa ngày SX từ trạng thái {CurrentState}. Cần ở trạng thái Ready.");
            }

            if (ProductionData == null)
            {
                return (false, "Chưa chọn PO.");
            }

            if (string.IsNullOrWhiteSpace(newDate))
            {
                return (false, "ProductionDate không được trống.");
            }

            // Validate date format
            if (!DateTime.TryParse(newDate, out var parsedDate))
            {
                return (false, "Định dạng ngày không hợp lệ. Vui lòng nhập theo format yyyy-MM-dd.");
            }

            string oldDate = ProductionData.ProductionDate;
            ProductionData.ProductionDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

            // Ghi log thay đổi
            GProduction.POHistoryManager.RecordProductionDateChange(
ProductionData.OrderNo, oldDate, ProductionData.ProductionDate, userName);

            Log.Information("[StateMachine] ProductionDate updated from '{OldDate}' to '{NewDate}' by {User}",
                oldDate, ProductionData.ProductionDate, userName);

            return (true, $"Đã cập nhật ngày SX thành {ProductionData.ProductionDate}");
        }

        /// <summary>
        /// Đồng bộ PO Database với DataPool khi bắt đầu Run PO
        /// - Load codesInPool CHỈ lấy Status=0 (chưa dùng) để tránh đẩy mã đã used vào PO
        /// - Mã ĐÃ ACTIVATE trong PO (Status=1) => Đánh dấu Status=1 trong Pool (phục hồi sau restart)
        /// - Mã có trong Pool (Status=0) nhưng không trong PO => Thêm vào PO (Status=0)
        /// - Kiểm tra số mã còn lại trong Pool (Status=0) >= orderQty
        /// </summary>
        public (bool success, string message, int availableCodes) SyncPoolWithPO()
        {
            if (ProductionData == null)
                return (false, "Chưa chọn PO.", 0);

            string orderNo = ProductionData.OrderNo;
            string gtin = ProductionData.Gtin;
            int orderQty = ProductionData.OrderQty;

            if (string.IsNullOrWhiteSpace(gtin))
                return (false, "PO không có GTIN.", 0);

            try
            {
                string dbPoolPath = GProject.ProductionOrderHelpers.Config.GetDataPoolPath(gtin);
                string dbPOPath = GProject.ProductionOrderHelpers.Config.GetPODBPath(orderNo);

                // Load mã từ PO Database
                HashSet<string> codesInPO = new(StringComparer.OrdinalIgnoreCase);
                using (var conPO = new SqliteConnection($"Data Source={dbPOPath}"))
                {
                    conPO.Open();
                    using var cmd = new SqliteCommand("SELECT Code FROM UniqueCodes;", conPO);
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) codesInPO.Add(rd.GetString(0));
                }

                // Load mã từ DataPool - CHỈ lấy Status=0 (chưa dùng) để tránh đẩy mã đã used vào PO
                HashSet<string> codesInPool = new(StringComparer.OrdinalIgnoreCase);
                using (var conPool = new SqliteConnection($"Data Source={dbPoolPath}"))
                {
                    conPool.Open();
                    using var cmd = new SqliteCommand("SELECT Code FROM Codes WHERE Status = 0;", conPool);
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) codesInPool.Add(rd.GetString(0));
                }

                // Mã ĐÃ ACTIVATE trong PO (Status=1) => Đánh dấu Status=1 trong Pool.
                // Chỉ áp dụng cho mã đã activate thật, tránh đánh Used oan khi PO mới nạp mã chưa chạy
                // (trước đây đánh cho MỌI mã có trong PO - sai).
                using (var conPO2 = new SqliteConnection($"Data Source={dbPOPath}"))
                {
                    conPO2.Open();
                    using var cmd = new SqliteCommand("SELECT Code FROM UniqueCodes WHERE Status = 1;", conPO2);
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var code = rd.GetString(0);
                        if (codesInPool.Contains(code))
                            DataPoolStatic.MarkUsedSimple(gtin, code);
                    }
                }

                // Mã có trong Pool (Status=0) nhưng không trong PO -> Thêm vào PO (Status=0)
                int addedCount = 0;
                if (codesInPool.Count > 0)
                {
                    using var conPO3 = new SqliteConnection($"Data Source={dbPOPath}");
                    conPO3.Open();
                    using var tx = conPO3.BeginTransaction();

                    foreach (var code in codesInPool)
                    {
                        if (codesInPO.Contains(code)) continue;

                        using var cmd = conPO3.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = @"INSERT OR IGNORE INTO UniqueCodes (Code, Status, ProductionDate)
                                           VALUES ($code, 0, $prodDate)";
                        cmd.Parameters.AddWithValue("$code", code);
                        cmd.Parameters.AddWithValue("$prodDate", ProductionData.ProductionDate ?? "");
                        cmd.ExecuteNonQuery();
                        addedCount++;
                    }
                    tx.Commit();
                }

                // Đếm số mã còn lại trong Pool (Status=0)
                int availableInPool = 0;
                using (var conPool2 = new SqliteConnection($"Data Source={dbPoolPath}"))
                {
                    conPool2.Open();
                    using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Codes WHERE Status = 0;", conPool2);
                    availableInPool = Convert.ToInt32(cmd.ExecuteScalar());
                }

                Log.Information("[SyncPoolWithPO] Sync completed: added={Added}, availableInPool={Available}, orderQty={OrderQty}",
                    addedCount, availableInPool, orderQty);

                // Kiểm tra số mã có đủ không
                if (availableInPool < orderQty)
                {
                    LastWarning = $"Khong du ma: Pool con {availableInPool}, can {orderQty}";
                    return (false, LastWarning, availableInPool);
                }

                return (true, $"Sync thanh cong. Them {addedCount} ma vao PO.", availableInPool);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[SyncPoolWithPO] Error syncing PO with DataPool");
                return (false, $"Loi sync: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Thử lại kiểm tra và chạy sản xuất sau khi đã thêm mã vào pool
        /// Chỉ hoạt động khi đang ở trạng thái InsufficientCodes
        /// </summary>
        public (bool success, string message, int availableCodes) RetryRunProduction()
        {
            if (CurrentState != e_ProductionState.InsufficientCodes)
                return (false, $"Chi co the retry khi o trang thai InsufficientCodes. State hien tai: {CurrentState}", 0);

            // Gọi lại SyncPoolWithPO để kiểm tra
            var result = SyncPoolWithPO();
            if (result.success)
            {
                // Đủ mã -> Quay về PushingToDic để bắt đầu
                SetState(e_ProductionState.PushingToDic, "Du ma sau khi retry");
                Log.Information("[RetryRunProduction] Retry success, transitioning to PushingToDic");
            }
            return result;
        }

        /// <summary>
        /// Khởi động background consumer thread ghi DB tuần tự (1 thread duy nhất)
        /// Ưu tiên: Record (audit) > CodeUpdate > CartonUpdate
        /// </summary>
        private void StartDbWriter()
        {
            if (_writerTask != null && !_writerTask.IsCompleted)
                return;

            _writerCts = new CancellationTokenSource();
            var token = _writerCts.Token;
            _writerTask = Task.Run(() => DbWriterLoop(token), token);
            Log.Information("[StateMachine] DB writer started");
        }

        /// <summary>
        /// Dừng background consumer thread và drain queue để không mất audit/DB.
        /// </summary>
        private async Task StopDbWriterAsync()
        {
            // Drain an toàn: không cancel nhánh writer cho tới khi tất cả queue rỗng
            // hoặc hết timeout, nhưng KHÔNG chờ nếu writer đã chết (tránh shutdown kẹt).
            if (_writerTask != null && !_writerTask.IsCompleted)
            {
                var deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline
                       && (RecordQueue.Count > 0 || CodeUpdateQueue.Count > 0 || CartonUpdateQueue.Count > 0))
                {
                    await Task.Delay(20);
                }
            }

            try { _writerCts?.Cancel(); } catch { }
            if (_writerTask != null)
            {
                try { await _writerTask; }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Log.Error(ex, "[StateMachine] Lỗi khi stop DB writer"); }
            }
            Log.Information("[StateMachine] DB writer stopped");
        }

        /// <summary>
        /// Đếm tổng item còn lại trong 3 queue — dùng để log drain status khi shutdown.
        /// </summary>
        private int TotalQueueDepth()
            => RecordQueue.Count + CodeUpdateQueue.Count + CartonUpdateQueue.Count;

        /// <summary>
        /// Thử lại một thao tác SQLite với backoff ngắn, tránh SQLite "database is locked"
        /// hoặc I/O tạm thời làm rớt 1 item audit.
        /// </summary>
        private static async Task RetrySqliteAsync(Func<Task> op, int attempts = 3, int delayMs = 50)
        {
            Exception? last = null;
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    await op();
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (i == attempts - 1) break;
                    try { await Task.Delay(delayMs); } catch { }
                }
            }
            throw last ?? new Exception("retry exhausted");
        }

        /// <summary>
        /// Vòng lặp ghi DB duy nhất, lần lượt drain Record -> CodeUpdate -> CartonUpdate
        /// </summary>
        private async Task DbWriterLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (ProductionData == null)
                    {
                        await Task.Delay(20, ct);
                        continue;
                    }

                    // Ưu tiên ghi Record trước (audit quan trọng nhất)
                    if (RecordQueue.TryDequeue(out var rec))
                    {
                        var orderNo = ProductionData?.OrderNo ?? "";
                        try
                        {
                            await RetrySqliteAsync(() =>
                            {
                                var r = GProduction.PORecordHelper.Record(orderNo, rec);
                                if (!r.IsSuccess) throw new InvalidOperationException(r.Message);
                                return Task.CompletedTask;
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Warning("[DB Writer] Record lỗi (sau retry): {Msg}", ex.Message);
                        }
                    }
                    else if (CodeUpdateQueue.TryDequeue(out var codeUpd))
                    {
                        try
                        {
                            await RetrySqliteAsync(() =>
                            {
                                var r = GProduction.PORecordHelper.UpdateCodeStatusAndCarton(
                                    codeUpd.OrderNo, codeUpd.Code, codeUpd.ActivateDate, codeUpd.ActivateUser,
                                    codeUpd.PackingDate, codeUpd.CartonCode, codeUpd.ProductionDate);
                                if (!r.IsSuccess) throw new InvalidOperationException(r.Message);
                                return Task.CompletedTask;
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Warning("[DB Writer] UpdateCode lỗi (sau retry): {Msg}", ex.Message);
                        }
                    }
                    else if (CartonUpdateQueue.TryDequeue(out var cartUpd))
                    {
                        //đọc ngược lại db chính xem trong db id thùng đó có thực sự có số lượng sản phẩm đúng hay không.
                        //Nếu sai dừng lại và báo lỗi => Thêm trạng thái lỗi Kiểm chéo đóng gói
                        //Kiểm tra lại luôn thùng trước đó để dừng lại kịp thời trước khi lỗi đi quá xa.
                        if (cartUpd.Type == CartonUpdateType.Complete)
                        {
                            bool completeOk = false;
                            string completeErr = "";
                            try
                            {
                                await RetrySqliteAsync(() =>
                                {
                                    var r = GProduction.POCarton.CompleteCarton(
                                        cartUpd.OrderNo, cartUpd.CartonId, cartUpd.ActivateUser);
                                    if (!r.IsSuccess) throw new InvalidOperationException(r.Message);
                                    return Task.CompletedTask;
                                });
                                completeOk = true;
                            }
                            catch (Exception ex)
                            {
                                completeErr = ex.Message;
                                Log.Warning("[DB Writer] CompleteCarton lỗi (sau retry): {Msg}", ex.Message);
                            }

                            if (completeOk)
                            {
                                // Lấy thông tin thùng để gửi AWS
                                if (Dictionary_Cartons.TryGetValue(cartUpd.CartonId, out var carton))
                                {
                                    SendCartonToAWS(
                                        carton.CartonCode,
                                        cartUpd.CartonId.ToString(),
                                        ActiveCounter.CartonCapacity,
                                        carton.CompletedDatetime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        cartUpd.ActivateUser
                                    );
                                }
                            }
                            else
                            {
                                Log.Error("[DB Writer] BỎ QUA StartCarton kế tiếp vì CompleteCarton thất bại ({Err})", completeErr);
                                continue;
                            }

                            // Sau khi đóng thùng hiện tại, tự động start thùng kế tiếp nếu có
                            try
                            {
                                await RetrySqliteAsync(() =>
                                {
                                    var sr = GProduction.POCarton.StartCarton(
                                        cartUpd.OrderNo, cartUpd.CartonId + 1, cartUpd.ActivateUser);
                                    if (!sr.IsSuccess) throw new InvalidOperationException(sr.Message);
                                    return Task.CompletedTask;
                                });
                            }
                            catch (Exception ex)
                            {
                                Log.Warning("[DB Writer] StartCarton kế tiếp lỗi (sau retry): {Msg}", ex.Message);
                            }
                        }
                    }
                    else
                    {
                        // Không có item nào, sleep ngắn rồi lặp lại
                        await Task.Delay(5, ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[DB Writer] Lỗi không mong đợi");
                    await Task.Delay(50, ct);
                }
            }
        }

        /// <summary>
        /// Xử lý 1 mã nhận được từ camera - pipeline 11 nhánh khớp với MASAN FDashboard.CameraSub_Process.
        /// Trả về CameraReadResult để Caller broadcast CodeScanned + push history.
        /// Trả về null khi state không phải Running (không phát badge scan cho FE).
        /// </summary>
        public CameraReadResult? HandleCodeFromCamera(string? rawCode, string camera = "camera")
        {
            if (string.IsNullOrWhiteSpace(rawCode))
            {
                return RejectAndEmit(camera, "", e_Production_Status.ReadFail, "empty payload");
            }

            rawCode = rawCode.Trim();
            rawCode = rawCode.Replace("\r", "").Replace("\n", "");
            string[] parts = rawCode.Split('|');
            string code = parts[0] ?? "";
            string status = parts.Length > 1 ? (parts[1] ?? "") : "NO_READ";
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var at = DateTime.UtcNow;
            var cam = string.IsNullOrEmpty(camera) ? "camera" : camera;

            // 1) Parse format
            if (parts.Length != 2 || string.IsNullOrEmpty(code))
            {
                ActiveCounter.TotalCount++;
                ActiveCounter.ErrorCount++;
                ActiveCounter.FailTotal++;
                ActiveCounter.FormatFailCount++;
                RecordQueue.Enqueue(new RecordData
                {
                    Code = code,
                    Status = "FormatError",
                    PLCStatus = "FAIL",
                    ActivateDate = now,
                    ActivateUser = "<API>",
                    ProductionDate = ProductionData?.ProductionDate ?? "0",
                });
                Log.Warning("[Camera] FormatError payload: {Raw}", rawCode);
                return WriteAndEmit(camera, code, e_Production_Status.FormatError, null, null, at);
            }

            // 2) status tu camera
            if (!string.Equals(status, "OK", StringComparison.Ordinal))
            {
                ActiveCounter.TotalCount++;
                if (string.Equals(status, "REJECT", StringComparison.OrdinalIgnoreCase))
                {
                    ActiveCounter.FormatFailCount++;
                    ActiveCounter.FailTotal++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "FormatFail",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = ProductionData?.ProductionDate ?? "0",
                    });
                    Log.Warning("[Camera] REJECT: {Code}", code);
                    return WriteAndEmit(camera, code, e_Production_Status.FormatError, null, null, at);
                }
                if (string.Equals(status, "NO_READ", StringComparison.OrdinalIgnoreCase))
                {
                    ActiveCounter.ReadFailCount++;
                    ActiveCounter.FailTotal++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "ReadFail",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = ProductionData?.ProductionDate ?? "0",
                    });
                    Log.Warning("[Camera] NO_READ code={Code}", code);
                    return WriteAndEmit(camera, code, e_Production_Status.ReadFail, null, null, at);
                }
                ActiveCounter.ErrorCount++;
                ActiveCounter.FailTotal++;
                RecordQueue.Enqueue(new RecordData
                {
                    Code = code,
                    Status = "Error",
                    PLCStatus = "FAIL",
                    ActivateDate = now,
                    ActivateUser = "<API>",
                    ProductionDate = ProductionData?.ProductionDate ?? "0",
                });
                Log.Warning("[Camera] Unknown status '{Status}' code={Code}", status, code);
                return WriteAndEmit(camera, code, e_Production_Status.Error, null, null, at);
            }

            // 3) status="OK" -> lookup + activate
            ActiveCounter.TotalCount++;

            string currentCartonCode;
            string orderNoSnapshot;
            string productionDateSnapshot;
            int activeCartonId;
            CodeInfo infoSnapshot;

            lock (_stateLock)
            {
                orderNoSnapshot = ProductionData?.OrderNo ?? "";
                productionDateSnapshot = ProductionData?.ProductionDate ?? "0";

                // 4) NotFound
                if (!Dictionary_Codes.TryGetValue(code, out var infoFound))
                {
                    ActiveCounter.NotFoundCount++;
                    ActiveCounter.FailTotal++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "NotFound",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = productionDateSnapshot,
                    });
                    Log.Warning("[Camera] NotFound: {Code}", code);
                    return WriteAndEmit(camera, code, e_Production_Status.NotFound, null, ActiveCounter.CartonID, at);
                }

                // 5) Duplicate -> PLC Fail (0), van audit + FE badge TRUNG.
                if (infoFound.Status == 1)
                {
                    ActiveCounter.DuplicateCount++;
                    ActiveCounter.FailTotal++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        CartonCode = infoFound.CartonCode,
                        Status = "Duplicate",
                        PLCStatus = "FAIL",
                        ActivateDate = infoFound.ActivateDate,
                        ActivateUser = infoFound.ActivateUser,
                        PackingDate = infoFound.PackingDate,
                        ProductionDate = productionDateSnapshot,
                    });
                    Log.Warning("[Camera] Duplicate: {Code} carton={Carton}", code, infoFound.CartonCode);
                    return WriteAndEmit(camera, code, e_Production_Status.Duplicate, infoFound.CartonCode, ActiveCounter.CartonID, at);
                }

                // 6) Thung hien tai chua co ma -> Error + WaitCartonCode
                if (!Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var carton)
                    || string.IsNullOrEmpty(carton.CartonCode) || carton.CartonCode == "0")
                {
                    ActiveCounter.FailTotal++;
                    ActiveCounter.ErrorCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "Error",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = productionDateSnapshot,
                    });
                    LastWarning = $"Thung hien tai (ID={ActiveCounter.CartonID}) chua co ma";
                    Log.Warning("[Camera] Reject {Code}: {Warning}", code, LastWarning);
                    SetState(e_ProductionState.WaitCartonCode, LastWarning);
                    return WriteAndEmit(camera, code, e_Production_Status.Error, null, ActiveCounter.CartonID, at);
                }

                currentCartonCode = carton.CartonCode;
                activeCartonId = ActiveCounter.CartonID;
                infoSnapshot = infoFound;
            }

            // 7) Ghi lane xuong PLC NGAY ngoai lock
            short plcValue = MapResultForPLC(e_Production_Status.Pass, activeCartonId);
            bool plcWriteOk = WritePlcLane(plcValue);

            if (!plcWriteOk)
            {
                lock (_stateLock)
                {
                    ActiveCounter.FailTotal++;
                    ActiveCounter.ErrorCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        CartonCode = currentCartonCode,
                        Status = "Error",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = productionDateSnapshot,
                    });
                }
                Log.Warning("[Camera] PLC write fail cho {Code}", code);
                return new CameraReadResult(cam, code, e_Production_Status.Error, false,
                    currentCartonCode, activeCartonId, at);
            }

            // 8) Cho PLC ACK V2
            var ack = e_TimeoutCheckResult.Pass;//WaitPlcAckV2(plcValue, code, camera);
            if (ack != e_TimeoutCheckResult.Pass)
            {
                lock (_stateLock)
                {
                    ActiveCounter.TimeoutCount++;
                    ActiveCounter.FailTotal++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        CartonCode = currentCartonCode,
                        Status = "Timeout",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = productionDateSnapshot,
                    });
                }
                Log.Warning("[Camera] ACK fail ({Result}) cho {Code}", ack, code);
                return new CameraReadResult(cam, code, e_Production_Status.Timeout, true,
                    currentCartonCode, activeCartonId, at);
            }

            // 9) ACK Pass -> commit Dictionary + DB + DataPool + carton
            bool cartonRolloverError = false;
            string rolloverErrorMsg = "";

            lock (_stateLock)
            {
                infoSnapshot.Status = 1;
                infoSnapshot.ActivateDate = now;
                infoSnapshot.ActivateUser = "<API>";
                infoSnapshot.CartonCode = currentCartonCode;
                infoSnapshot.PackingDate = now;
                infoSnapshot.ProductionDate = productionDateSnapshot;
                Dictionary_Codes[code] = infoSnapshot;

                RecordQueue.Enqueue(new RecordData
                {
                    Code = code,
                    CartonCode = currentCartonCode,
                    Status = "Pass",
                    PLCStatus = "PASS",
                    ActivateDate = now,
                    ActivateUser = "<API>",
                    PackingDate = now,
                    PackingUser = "<API>",
                    ProductionDate = productionDateSnapshot,
                });

                CodeUpdateQueue.Enqueue(new CodeUpdateItem
                {
                    OrderNo = orderNoSnapshot,
                    Code = code,
                    ActivateDate = now,
                    ActivateUser = "<API>",
                    PackingDate = now,
                    CartonCode = currentCartonCode,
                    ProductionDate = productionDateSnapshot,
                });

                ActiveCounter.PassTotal++;
                PackageCounter.PassTotal++;
                ActiveCounter.CartonCode = currentCartonCode;

                // #region agent log
                if (PackageCounter.PassTotal >= ActiveCounter.CartonCapacity - Math.Max(CartonOffset, 1))
                {
                    AgentDebugLog("H3,H5", "Camera pass committed near carton boundary", new
                    {
                        assignedCartonId = activeCartonId,
                        activeCartonId = ActiveCounter.CartonID,
                        packedCount = PackageCounter.PassTotal,
                        cartonCapacity = ActiveCounter.CartonCapacity,
                        cartonOffset = CartonOffset,
                        activePassTotal = ActiveCounter.PassTotal,
                        totalCount = ActiveCounter.TotalCount
                    });
                }
                // #endregion

                if (!string.IsNullOrEmpty(ProductionData?.Gtin))
                {
                    try { DataPoolStatic.MarkUsedSimple(ProductionData.Gtin, code); }
                    catch (Exception ex) { Log.Warning(ex, "[Camera] DataPool MarkUsed fail cho {Code}", code); }
                }

                if (PackageCounter.PassTotal >= ActiveCounter.CartonCapacity
                    && !string.IsNullOrEmpty(orderNoSnapshot))
                {
                    int currentCartonID = ActiveCounter.CartonID;
                    CartonUpdateQueue.Enqueue(new CartonUpdateItem
                    {
                        Type = CartonUpdateType.Complete,
                        OrderNo = orderNoSnapshot,
                        CartonId = currentCartonID,
                        ActivateUser = "<API>",
                    });

                    if (Dictionary_Cartons.TryGetValue(currentCartonID, out var closedCarton))
                        closedCarton.CompletedDatetime = now;

                    int nextCartonId = currentCartonID + 1;
                    ActiveCounter.CartonID = nextCartonId;
                    PackageCounter.PassTotal = 0;

                    if (Dictionary_Cartons.TryGetValue(nextCartonId, out var nextCarton))
                    {
                        ActiveCounter.CartonCode = nextCarton.CartonCode;
                        if (nextCarton.CartonCode == "0" || string.IsNullOrEmpty(nextCarton.CartonCode))
                        {
                            cartonRolloverError = true;
                            rolloverErrorMsg = $"Thung ke tiep (ID={nextCartonId}) chua co ma";
                        }
                    }
                    else
                    {
                        cartonRolloverError = true;
                        rolloverErrorMsg = $"Khong tim thay thung ke tiep (ID={nextCartonId})";
                    }
                }

                if (ProductionData != null && ActiveCounter.PassTotal >= ProductionData.OrderQty)
                {
                    SetState(e_ProductionState.WaitingStop, $"orderQty {ProductionData.OrderQty} reached");
                }
            }

            if (cartonRolloverError)
            {
                LastWarning = rolloverErrorMsg;
                SetState(e_ProductionState.WaitCartonCode, rolloverErrorMsg);
                Log.Warning("[Camera] {Msg}", rolloverErrorMsg);
            }

            return new CameraReadResult(cam, code, e_Production_Status.Pass, true,
                currentCartonCode, activeCartonId, at);
        }

        /// <summary>
        /// Trả về e_Production_Status cho mã vừa quét — dùng khi cần tra cứu
        /// trạng thái đã xử lý gần nhất (mặc định NotFound nếu chưa có kết quả).
        /// </summary>
        public e_Production_Status PeekLastCodeStatus(string code)
        {
            if (string.IsNullOrEmpty(code)) return e_Production_Status.ReadFail;
            if (Dictionary_Codes.TryGetValue(code, out var info))
            {
                if (info.Status == 1) return e_Production_Status.Duplicate;
                return e_Production_Status.Pass;
            }
            return e_Production_Status.NotFound;
        }

        /// <summary>
        /// Vòng lặp chính - mỗi 100ms gọi ProcessState() tương tự FDashboard
        /// </summary>
        private void RunLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    ProcessState();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[StateMachine] Lỗi trong vòng lặp");
                    SetState(e_ProductionState.Error, $"loop error: {ex.Message}");
                }

                try { Task.Delay(10, token).Wait(token); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>
        /// Xử lý state hiện tại - tương tự Process_Production_State() trong FDashboard.cs
        /// </summary>
        private void ProcessState()
        {
            
            switch (CurrentState)
            {
                case e_ProductionState.NeedLogin:
                    // Chờ user đăng nhập qua API
                    // Khi login thành công -> chuyển sang Checking
                    break;

                case e_ProductionState.Checking:
                    ProcessCheckingState();
                    break;

                case e_ProductionState.Editing:
                    // Chờ user chọn PO qua API
                    break;

                case e_ProductionState.NoSelectedPO:
                    // Chờ user chọn PO
                    break;

                case e_ProductionState.CheckPO:
                    ProcessCheckPOState();
                    break;

                case e_ProductionState.LoadPO:
                    ProcessLoadPOState();
                    break;

                case e_ProductionState.Ready:
                    // Sẵn sàng - chờ lệnh start
                    CheckDeviceStatus();
                    break;

                case e_ProductionState.PushingToDic:
                    ProcessPushToDicState();
                    break;

                case e_ProductionState.Running:
                    CheckDeviceStatus();
                    ProcessRunningState();
                    break;

                case e_ProductionState.Paused:
                    CheckDeviceStatus();
                    ProcessPauseState();
                    break;
                case e_ProductionState.WaitCartonCode:
                    CheckDeviceStatus();
                    // Đang chờ user nhập mã thùng mới qua API
                    ProcessPauseState();
                    break;

                case e_ProductionState.InsufficientCodes:
                    // Không đủ mã trong pool - chờ user thêm mã vào pool hoặc quay lại Ready
                    ProcessPauseState();
                    break;

                case e_ProductionState.WaitingStop:
                    // Đang chờ PLC dừng
                    break;

                case e_ProductionState.CheckAfterCompleted:
                    ProcessCheckAfterCompleted();
                    break;

                case e_ProductionState.Completed:
                    // Đã hoàn thành - chờ reset
                    break;

                case e_ProductionState.DeviceError:
                    TryReconnectDevices();
                    break;
                case e_ProductionState.Error:
                    // Đã lỗi - chờ xử lý
                    break;
            }
        }




        /// <summary>
        /// Tự động reconnect và resume khi thiết bị kết nối lại
        /// </summary>
        private void TryReconnectDevices()
        {
            bool plcConnected = Global.PLC_STATUS == OmronPLC_Hsl.PLCStatus.Connected;
            bool cameraConnected = Global.Camera_STATUS == eOmronCodeReaderState.Connected;
            
            if (plcConnected && cameraConnected)
            {
                    SetState(PreviousState, "devices reconnected");
                    //SetState(e_ProductionState.Ready, "devices reconnected");

            }
        }


        private void CheckDeviceStatus()
        {
            bool plcConnected = Global.PLC_STATUS == OmronPLC_Hsl.PLCStatus.Connected;
            bool cameraConnected = Global.Camera_STATUS == eOmronCodeReaderState.Connected;
            if (!plcConnected || !cameraConnected)
            {
                SetState(e_ProductionState.DeviceError, "device disconnected");
            }
        }

        /// <summary>
        /// Checking: kiểm tra PO gần nhất trong POHistory.db
        /// - Nếu có PO đang chạy dở -> LoadPO
        /// - Nếu PO đã hoàn thành -> Editing
        /// - Nếu chưa có PO nào -> Editing
        /// </summary>
        private void ProcessCheckingState()
        {
            try
            {
                var lastRunning = GProduction.POHistoryManager.GetLastRunningPO();
                if (lastRunning != null)
                {
                    // Có PO đang chạy dở -> load lại
                    var po = GProduction.POLoader.GetByOrderNo(lastRunning.PO);
                    if (po.IsSuccess && po.Data != null && po.Data.Rows.Count > 0)
                    {
                        ProductionData = POInfo.FromDataRow(po.Data.Rows[0]);
                        SetState(e_ProductionState.LoadPO, $"continue PO {ProductionData.OrderNo}");
                        return;
                    }
                }
                // Chưa có PO nào hoặc PO đã hoàn thành -> vào Editing
                SetState(e_ProductionState.Editing, "no running PO");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[StateMachine] Lỗi Checking");
                SetState(e_ProductionState.Error, ex.Message);
            }
        }

        /// <summary>
        /// CheckPO: kiểm tra và tạo DB cho PO
        /// </summary>
        private void ProcessCheckPOState()
        {
            try
            {
                if (ProductionData == null)
                {
                    SetState(e_ProductionState.NoSelectedPO, "ProductionData null");
                    return;
                }

                var status = GProduction.POCreator.CheckPODatabaseStatus(
                    ProductionData.OrderNo, ProductionData.OrderQty,
                    ProductionData.CartonCapacity > 0 ? ProductionData.CartonCapacity : 24);

                if (!status.IsFullyInitialized)
                {
                    // Tạo DB và load codes
                    var result = GProduction.POCreator.EnsurePODatabaseReady(
                        ProductionData.OrderNo,
                        ProductionData.Gtin,
                        ProductionData.OrderQty,
                        ProductionData.CartonCapacity > 0 ? ProductionData.CartonCapacity : 24,
                        autoLoadCodes: true);

                    if (!result.success)
                    {
                        SetState(e_ProductionState.Error, $"CheckPO failed: {result.message}");
                        return;
                    }
                }
                SetState(e_ProductionState.LoadPO, "DB ready");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[StateMachine] Lỗi CheckPO");
                SetState(e_ProductionState.Error, ex.Message);
            }
        }

        /// <summary>
        /// LoadPO: load codes và cartons vào Dictionary, reset counter
        /// </summary>
        private void ProcessLoadPOState()
        {
            try
            {
                if (ProductionData == null)
                {
                    SetState(e_ProductionState.NoSelectedPO, "ProductionData null");
                    return;
                }

                Dictionary_Codes.Clear();
                Dictionary_Cartons.Clear();
                ActiveCounter = new ProductCounter
                {
                    CartonCapacity = ProductionData.CartonCapacity > 0 ? ProductionData.CartonCapacity : 24
                };
                PackageCounter = new ProductCounter();

                // Loader chỉ nhận Dictionary<string, CodeInfo> nên load vào tạm rồi copy sang ConcurrentDictionary
                var tempCodes = new Dictionary<string, CodeInfo>();
                GProduction.CodeDictionaryLoader.LoadAllCodesToDictionary(
                    ProductionData.OrderNo, tempCodes);
                foreach (var kv in tempCodes)
                {
                    Dictionary_Codes[kv.Key] = kv.Value;
                }

                // Load cartons từ DB để sẵn sàng cho state Running (sẽ reload đầy đủ trong PushToDic)
                var cartonsResult = GProduction.POCarton.GetAll(ProductionData.OrderNo);
                if (cartonsResult.IsSuccess && cartonsResult.Data != null)
                {
                    foreach (System.Data.DataRow row in cartonsResult.Data.Rows)
                    {
                        var info = new CartonInfo
                        {
                            Id = Convert.ToInt32(row["Id"] ?? 0),
                            CartonCode = row["cartonCode"]?.ToString() ?? "0",
                            StartDatetime = row["Start_Datetime"]?.ToString() ?? "0",
                            CompletedDatetime = row["Completed_Datetime"]?.ToString() ?? "0"
                        };
                        Dictionary_Cartons[info.Id] = info;
                    }
                }

                SetState(e_ProductionState.Ready, $"PO {ProductionData.OrderNo} loaded");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[StateMachine] Lỗi LoadPO");
                SetState(e_ProductionState.Error, ex.Message);
            }
        }

        /// <summary>
        /// PushDataToDic: load codes và cartons vào Dictionary (giống Camera_Processing trong FDashboard)
        /// </summary>
        private void ProcessPushToDicState()
        {
            try
            {
                if (ProductionData == null)
                {
                    SetState(e_ProductionState.NoSelectedPO, "ProductionData null");
                    return;
                }

                var tempCodes = new Dictionary<string, CodeInfo>();
                GProduction.CodeDictionaryLoader.LoadAllCodesToDictionary(
                    ProductionData.OrderNo, tempCodes);

                Dictionary_Codes.Clear();
                foreach (var kv in tempCodes)
                {
                    Dictionary_Codes[kv.Key] = kv.Value;
                }
                Log.Information("[StateMachine] Dictionary_Codes loaded: {Count} codes (PO={OrderNo})",
                    Dictionary_Codes.Count, ProductionData.OrderNo);

                var cartonsResult = GProduction.POCarton.GetAll(ProductionData.OrderNo);
                if (cartonsResult.IsSuccess && cartonsResult.Data != null && cartonsResult.Data.Rows.Count > 0)
                {
                    Dictionary_Cartons.Clear();
                    foreach (System.Data.DataRow row in cartonsResult.Data.Rows)
                    {
                        var info = new CartonInfo
                        {
                            Id = Convert.ToInt32(row["Id"] ?? 0),
                            CartonCode = row["cartonCode"]?.ToString() ?? "0",
                            StartDatetime = row["Start_Datetime"]?.ToString() ?? "0",
                            CompletedDatetime = row["Completed_Datetime"]?.ToString() ?? "0"
                        };
                        Dictionary_Cartons[info.Id] = info;
                    }

                    // Reload trạng thái đang chạy từ DB để phục hồi khi restart (mất điện)
                    // Ưu tiên thùng đã pack có sản phẩm nhiều nhất, không dùng StartDatetime
                    string? lastCartonCode = GProduction.PORecordHelper.GetLastPackedCartonCode(ProductionData.OrderNo);

                    if (!string.IsNullOrEmpty(lastCartonCode))
                    {
                        var lastCarton = Dictionary_Cartons.Values
                            .FirstOrDefault(c => c.CartonCode == lastCartonCode);

                        if (lastCarton != null)
                        {
                            int packedCount = GProduction.PORecordHelper.GetCodeCountInCarton(
                                ProductionData.OrderNo, lastCartonCode);

                            if (packedCount >= ProductionData.CartonCapacity)
                            {
                                ActiveCounter.CartonID = lastCarton.Id + 1;
                                ActiveCounter.CartonCode = "";
                            }
                            else
                            {
                                ActiveCounter.CartonID = lastCarton.Id;
                                ActiveCounter.CartonCode = lastCartonCode;
                            }
                        }
                        else
                        {
                            ActiveCounter.CartonID = 1;
                            ActiveCounter.CartonCode = "";
                        }
                    }
                    else
                    {
                        ActiveCounter.CartonID = 1;
                        ActiveCounter.CartonCode = "";
                    }

                    // 2) Tính lại PackageCounter.PassTotal = số mã đã pack trong carton hiện tại
                    if (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var curCarton)
                        && curCarton.CartonCode != "0")
                    {
                        PackageCounter.PassTotal = GProduction.PORecordHelper.GetCodeCountInCarton(
                            ProductionData.OrderNo, curCarton.CartonCode);
                    }
                    else
                    {
                        PackageCounter.PassTotal = 0;
                    }

                    // 3) Tính lại ActiveCounter.PassTotal = tổng mã đã active trong PO
                    ActiveCounter.PassTotal = GProduction.PORecordHelper.GetActiveCount(ProductionData.OrderNo);

                    // 3.5) Rehydrate các counter Fail/FormatError/Timeout/... từ bảng Records
                    var fromRecord = GProduction.PORecordHelper.GetCountersFromRecordDB(ProductionData.OrderNo);
                    ActiveCounter.FormatFailCount = fromRecord.FormatFailCount;
                    ActiveCounter.ReadFailCount   = fromRecord.ReadFailCount;
                    ActiveCounter.TimeoutCount    = fromRecord.TimeoutCount;
                    ActiveCounter.NotFoundCount   = fromRecord.NotFoundCount;
                    ActiveCounter.DuplicateCount  = fromRecord.DuplicateCount;
                    ActiveCounter.ErrorCount      = fromRecord.ErrorCount;
                    ActiveCounter.FailTotal       = fromRecord.FailTotal;
                    if (fromRecord.TotalCount > ActiveCounter.TotalCount)
                        ActiveCounter.TotalCount = fromRecord.TotalCount;

                    // 4) Đồng bộ PO với DataPool và kiểm tra đủ mã
                    var syncResult = SyncPoolWithPO();
                    if (!syncResult.success)
                    {
                        Log.Warning("[StateMachine] SyncPoolWithPO failed: {Message}", syncResult.message);
                        SetState(e_ProductionState.InsufficientCodes, syncResult.message);
                        return;
                    }

                    Log.Information("[StateMachine] Reload state: cartonID={CartonId}, cartonCode={CartonCode}, packed={Packed}, totalActive={Active}",
                        ActiveCounter.CartonID, ActiveCounter.CartonCode, PackageCounter.PassTotal, ActiveCounter.PassTotal);
                }
                else
                {
                    // Carton DB thiếu hoặc rỗng -> auto-recover trước khi chuyển sang Running.
                    // Trước đây nhánh này bị bỏ qua khiến Dictionary rỗng -> Running tick -> Ready loop.
                    Log.Warning("[StateMachine] POCarton.GetAll failed/rong cho {OrderNo}, attempting auto-recover...",
                        ProductionData.OrderNo);

                    int cartonCap = ProductionData.CartonCapacity > 0 ? ProductionData.CartonCapacity : 24;
                    var recoverResult = GProduction.POCreator.EnsurePODatabaseReady(
                        ProductionData.OrderNo,
                        ProductionData.Gtin,
                        ProductionData.OrderQty,
                        cartonCap,
                        autoLoadCodes: true);

                    if (!recoverResult.success)
                    {
                        SetState(e_ProductionState.Error,
                            $"Cannot recover PO DB: {recoverResult.message}");
                        return;
                    }

                    // Retry sau auto-recover
                    cartonsResult = GProduction.POCarton.GetAll(ProductionData.OrderNo);
                    if (!cartonsResult.IsSuccess || cartonsResult.Data == null || cartonsResult.Data.Rows.Count == 0)
                    {
                        SetState(e_ProductionState.Error,
                            $"Carton DB not loadable sau auto-recover. File: {ProductionOrderHelpers.Config.GetCartonPath(ProductionData.OrderNo)}");
                        return;
                    }

                    Dictionary_Cartons.Clear();
                    foreach (System.Data.DataRow row in cartonsResult.Data.Rows)
                    {
                        var info = new CartonInfo
                        {
                            Id = Convert.ToInt32(row["Id"] ?? 0),
                            CartonCode = row["cartonCode"]?.ToString() ?? "0",
                            StartDatetime = row["Start_Datetime"]?.ToString() ?? "0",
                            CompletedDatetime = row["Completed_Datetime"]?.ToString() ?? "0"
                        };
                        Dictionary_Cartons[info.Id] = info;
                    }

                    // Nạp lại Dictionary_Codes cùng cách nhánh chính để tránh giới hạn 100
                    var tempCodesRecover = new Dictionary<string, CodeInfo>();
                    GProduction.CodeDictionaryLoader.LoadAllCodesToDictionary(
                        ProductionData.OrderNo, tempCodesRecover);

                    Dictionary_Codes.Clear();
                    foreach (var kv in tempCodesRecover)
                    {
                        Dictionary_Codes[kv.Key] = kv.Value;
                    }
                    Log.Information("[StateMachine] Auto-recover: Dictionary_Codes loaded {Count} codes",
                        Dictionary_Codes.Count);

                    var lastRunningAfterRecover = Dictionary_Cartons.Values
                        .Where(c => c.StartDatetime != "0" && c.CompletedDatetime == "0")
                        .OrderBy(c => c.Id)
                        .FirstOrDefault();

                    if (lastRunningAfterRecover != null)
                    {
                        ActiveCounter.CartonID = lastRunningAfterRecover.Id;
                        ActiveCounter.CartonCode = lastRunningAfterRecover.CartonCode;
                    }
                    else
                    {
                        var firstOpenAfterRecover = Dictionary_Cartons.Values
                            .Where(c => c.CompletedDatetime == "0")
                            .OrderBy(c => c.Id)
                            .FirstOrDefault();
                        ActiveCounter.CartonID = firstOpenAfterRecover?.Id ?? 1;
                        ActiveCounter.CartonCode = firstOpenAfterRecover?.CartonCode ?? "";
                    }

                    PackageCounter.PassTotal = (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var curCartonAfter)
                            && curCartonAfter.CartonCode != "0")
                        ? GProduction.PORecordHelper.GetCodeCountInCarton(
                            ProductionData.OrderNo, curCartonAfter.CartonCode)
                        : 0;

                    ActiveCounter.PassTotal = GProduction.PORecordHelper.GetActiveCount(ProductionData.OrderNo);

                    // Rehydrate các counter Fail/FormatError/Timeout/... từ bảng Records (auto-recover path)
                    var fromRecordAfter = GProduction.PORecordHelper.GetCountersFromRecordDB(ProductionData.OrderNo);
                    ActiveCounter.FormatFailCount = fromRecordAfter.FormatFailCount;
                    ActiveCounter.ReadFailCount   = fromRecordAfter.ReadFailCount;
                    ActiveCounter.TimeoutCount    = fromRecordAfter.TimeoutCount;
                    ActiveCounter.NotFoundCount   = fromRecordAfter.NotFoundCount;
                    ActiveCounter.DuplicateCount  = fromRecordAfter.DuplicateCount;
                    ActiveCounter.ErrorCount      = fromRecordAfter.ErrorCount;
                    ActiveCounter.FailTotal       = fromRecordAfter.FailTotal;
                    if (fromRecordAfter.TotalCount > ActiveCounter.TotalCount)
                        ActiveCounter.TotalCount = fromRecordAfter.TotalCount;

                    var syncResultAfter = SyncPoolWithPO();
                    if (!syncResultAfter.success)
                    {
                        Log.Warning("[StateMachine] SyncPoolWithPO failed (after recover): {Message}", syncResultAfter.message);
                        SetState(e_ProductionState.InsufficientCodes, syncResultAfter.message);
                        return;
                    }

                    Log.Information("[StateMachine] Auto-recover OK: cartonID={CartonId}, cartonCode={CartonCode}, packed={Packed}, totalActive={Active}",
                        ActiveCounter.CartonID, ActiveCounter.CartonCode, PackageCounter.PassTotal, ActiveCounter.PassTotal);
                }

                SetState(e_ProductionState.Running, "data pushed to dic");
                // Gửi PO started event lên AWS
                SendPOEventToAWS("start");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[StateMachine] Lỗi PushToDic");
                SetState(e_ProductionState.Error, ex.Message);
            }
        }

        /// <summary>
        /// Running: kiểm tra carton hiện tại và carton sắp tới
        /// - PROACTIVE: Nếu thùng sắp đầy VÀ thùng kế tiếp đã có mã -> AUTO ROLLOVER
        /// - Nếu thùng sắp đầy VÀ thùng kế tiếp chưa có mã -> WaitCartonCode
        /// - Nếu thùng hiện tại chưa có mã -> WaitCartonCode
        /// </summary>
        private void ProcessRunningState()
        {
            if (ProductionData == null)
            {
                SetState(e_ProductionState.NoSelectedPO, "ProductionData null in Running");
                return;
            }

            int currentCartonId = ActiveCounter.CartonID;
            int packedCount = PackageCounter.PassTotal;

            // Kiểm tra thùng hiện tại
            if (Dictionary_Cartons.TryGetValue(currentCartonId, out var currentCarton))
            {
                if (currentCarton.CartonCode == "0")
                {
                    LastWarning = "Thùng hiện tại chưa có mã";
                    SetState(e_ProductionState.WaitCartonCode, LastWarning);
                    return;
                }
            }
            else
            {
                Log.Error("[StateMachine] cartonID {CartonId} not in Dictionary_Cartons. DB inconsistent.",
                    currentCartonId);
                SetState(e_ProductionState.Error,
                    $"Carton '{currentCartonId}' missing in dict. Carton DB có thể chưa được load đúng.");
                return;
            }

            // Nhánh ② KHÔNG tự đóng thùng/proactive rollover nữa — chỉ nhánh ① (HandleCodeFromCamera)
            // mới có quyền đóng thùng khi PackageCounter.PassTotal >= CartonCapacity.
            // Ở đây chỉ phát cảnh báo sớm nếu thùng sắp đầy mà thùng kế chưa có mã.

            // Cảnh báo sớm (khi còn 2-3 sản phẩm nữa là đầy)
            int warningThreshold = ActiveCounter.CartonCapacity - CartonWarning;
            if (packedCount >= warningThreshold)
            {
                if (Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var nextCarton)
                    && (nextCarton.CartonCode == "0" || string.IsNullOrEmpty(nextCarton.CartonCode)))
                {
                    LastWarning = "Cảnh báo: thùng sắp tới chưa có mã";
                    Log.Warning("[StateMachine] {Warning}", LastWarning);
                }
            }
        }

        /// <summary>
        /// Pause/WaitCartonCode: chờ thùng có mã rồi quay lại Running
        /// - Auto rollover nếu thùng kế tiếp đã có mã và thùng hiện tại đã đầy
        /// </summary>
        private void ProcessPauseState()
        {
            if (ProductionData == null)
            {
                SetState(e_ProductionState.NoSelectedPO, "ProductionData null in Pause");
                return;
            }

            int currentCartonId = ActiveCounter.CartonID;

            // Sync ActiveCounter.CartonCode nếu cần
            if (Dictionary_Cartons.TryGetValue(currentCartonId, out var syncCarton))
            {
                if (!string.IsNullOrEmpty(syncCarton.CartonCode)
                    && syncCarton.CartonCode != "0"
                    && ActiveCounter.CartonCode != syncCarton.CartonCode)
                {
                    ActiveCounter.CartonCode = syncCarton.CartonCode;
                    Log.Information("[StateMachine] Synced ActiveCounter.CartonCode = {Code}", syncCarton.CartonCode);
                }
            }

            // THÙNG HIỆN TẠI đã có mã -> về Running (CHECK TRƯỚC)
            if (Dictionary_Cartons.TryGetValue(currentCartonId, out var currentCarton)
                && !string.IsNullOrEmpty(currentCarton.CartonCode)
                && currentCarton.CartonCode != "0")
            {
                // #region agent log
                AgentDebugLog("H2,H4", "Pause sees current carton code and resumes", new
                {
                    currentCartonId,
                    packedCount = PackageCounter.PassTotal,
                    cartonCapacity = ActiveCounter.CartonCapacity,
                    cartonOffset = CartonOffset,
                    currentCartonHasCode = true,
                    nextCartonExists = Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var debugNextCarton),
                    nextCartonHasCode = debugNextCarton != null
                        && !string.IsNullOrEmpty(debugNextCarton.CartonCode)
                        && debugNextCarton.CartonCode != "0",
                    lastWarning = LastWarning
                });
                // #endregion

                //int nextThreshold = ActiveCounter.CartonCapacity - CartonOffset;
                //if (PackageCounter.PassTotal >= nextThreshold)
                //{
                //    if(Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var nextCarton))
                //}
                    SetState(e_ProductionState.Running, "Thung hien tai da co ma");
                return;
            }

            // THÙNG KẾ TIẾP đã có mã + thùng HIỆN TẠI đã ĐẦY -> AUTO ROLLOVER
            // CHỈ rollover khi thùng hiện tại đã đầy, tránh bug khi thùng hiện tại chưa có mã
            if (PackageCounter.PassTotal >= ActiveCounter.CartonCapacity
                && Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var nextCarton)
                && !string.IsNullOrEmpty(nextCarton.CartonCode)
                && nextCarton.CartonCode != "0")
            {
                Log.Information("[StateMachine] Auto rollover in ProcessPauseState - next carton ready, current full");

                // 1. Đóng thùng hiện tại
                CartonUpdateQueue.Enqueue(new CartonUpdateItem
                {
                    Type = CartonUpdateType.Complete,
                    OrderNo = ProductionData.OrderNo,
                    CartonId = currentCartonId,
                    ActivateUser = "<AUTO>"
                });

                if (Dictionary_Cartons.TryGetValue(currentCartonId, out var closingCarton))
                    closingCarton.CompletedDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 2. Chuyển sang thùng mới
                ActiveCounter.CartonID = currentCartonId + 1;
                ActiveCounter.CartonCode = nextCarton.CartonCode;
                PackageCounter.PassTotal = 0;

                // 3. Về Running
                SetState(e_ProductionState.Running, "Auto rollover, thung ke tiep da co ma");
                return;
            }
        }

        /// <summary>
        /// CheckAfterCompleted: kiểm tra đã hoàn tất PO chưa
        /// </summary>
        private void ProcessCheckAfterCompleted()
        {
            try
            {
                if (ProductionData == null)
                {
                    SetState(e_ProductionState.NoSelectedPO, "ProductionData null");
                    return;
                }

                int totalCartons = Dictionary_Cartons.Count;
                int closedCartons = Dictionary_Cartons.Values.Count(c => c.CompletedDatetime != "0");

                if (totalCartons > 0 && closedCartons >= totalCartons)
                {
                    GProduction.POHistoryManager.RecordEnd(ProductionData.OrderNo);
                    SetState(e_ProductionState.Completed, "all cartons closed");
                    // Gửi PO completed event lên AWS
                    SendPOEventToAWS("complete");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[StateMachine] Lỗi CheckAfterCompleted");
                SetState(e_ProductionState.Error, ex.Message);
            }
        }

        // ===== PLC Response + Timeout V2 =====

        public enum e_TimeoutCheckResult
        {
            Pass,           // PLC confirm OK (D200==expected && D202 in {1,2})
            Timeout,        // PLC confirm reject hoặc poll hết timeout hoặc ID nhảy
            PlcUnavailable, // _plcMonitor null
            Error,          // exception
        }

        /// <summary>Map status + carton id sang giá trị ghi xuống PLC DM.</summary>
        private static short MapResultForPLC(e_Production_Status status, int cartonId)
        {
            return status switch
            {
                e_Production_Status.Pass => (short)(cartonId % 2 == 1 ? 1 : 2),
                _ => 0
            };
        }

/// <summary>
        /// Helper dùng chung: ghi kết quả xuống PLC (0 cho Fail, 1/2 cho Pass theo cartonId)
        /// rồi trả về CameraReadResult tương ứng. Dùng cho mọi nhánh Fail trong pipeline.
        /// </summary>
        private CameraReadResult WriteAndEmit(string camera, string code, e_Production_Status status,
            string? cartonCode, int? cartonId, DateTime at)
        {
            bool plcSent = WritePlcLane(MapResultForPLC(status, cartonId ?? 0));
            return new CameraReadResult(camera, code, status, plcSent, cartonCode, cartonId, at);
        }

        /// <summary>
        /// Shortcut cho nhánh Reject không cần biết carton — luôn map về 0.
        /// </summary>
        private CameraReadResult RejectAndEmit(string camera, string code,
            e_Production_Status status, string reason)
        {
            Log.Warning("[Camera] RejectAndEmit {Status}: {Code} ({Reason})", status, code, reason);
            return WriteAndEmit(camera, code, status, null, null, DateTime.UtcNow);
        }

        /// <summary>
        /// Ghi 1 giá trị xuống thanh ghi PLC_Reject_DM_C1. Trả về true nếu ghi thành công.
        /// Đây là điểm ghi DUY NHẤT xuống PLC — mọi nhánh trong pipeline đều đi qua helper này.
        /// </summary>
        private bool WritePlcLane(short value)
        {
            try
            {
                var plc = Global.omronPLC;
                if (plc?.plc == null)
                {
                    Log.Warning("[PLC] omronPLC null, skip write {Val}", value);
                    return false;
                }
                string address;
                try { address = PLCAddressWithGoogleSheetHelper.Get("PLC_Reject_DM_C1"); }
                catch { address = "D200"; }

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

        /// <summary>
        /// Cơ chế ACK V2: snapshot CurrentID/CurrentStatus trước khi ghi, ghi lane xuống PLC,
        /// poll cho đến khi ID/Status thay đổi đúng như đã gửi, hoặc timeout.
        /// Mặc định: timeout 500ms, poll 10ms (cấu hình qua AppConfig).
        /// </summary>
        private e_TimeoutCheckResult WaitPlcAckV2(int expectedLane, string code, string camera)
        {
            try
            {
                var plc = Global.omronPLC;
                if (plc?.plc == null) return e_TimeoutCheckResult.PlcUnavailable;

                int timeoutMs = _ackTimeoutMs;
                int pollMs = _ackPollIntervalMs;
                if (timeoutMs <= 0) timeoutMs = 500;
                if (pollMs <= 0) pollMs = 10;

                string idAddr, statusAddr;
                try { idAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_CurrentID_DM_C2"); }
                catch { idAddr = "D200"; }
                try { statusAddr = PLCAddressWithGoogleSheetHelper.Get("PLC_CurrentStatus_DM_C2"); }
                catch { statusAddr = "D202"; }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                int? prevId = null;
                int? prevStatus = null;
                try
                {
                    var r1 = plc.plc.ReadInt16(idAddr, 1);
                    var r2 = plc.plc.ReadInt16(statusAddr, 1);
                    if (r1.IsSuccess && r2.IsSuccess && r1.Content != null && r2.Content != null
                        && r1.Content.Length > 0 && r2.Content.Length > 0)
                    {
                        prevId = r1.Content[0];
                        prevStatus = r2.Content[0];
                    }
                }
                catch { }

                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    System.Threading.Thread.Sleep(pollMs);

                    int currentId, currentStatus;
                    try
                    {
                        var r1 = plc.plc.ReadInt16(idAddr, 1);
                        var r2 = plc.plc.ReadInt16(statusAddr, 1);
                        if (!r1.IsSuccess || !r2.IsSuccess
                            || r1.Content == null || r2.Content == null
                            || r1.Content.Length == 0 || r2.Content.Length == 0)
                        {
                            continue;
                        }
                        currentId = r1.Content[0];
                        currentStatus = r2.Content[0];
                    }
                    catch
                    {
                        continue;
                    }

                    if (prevId.HasValue && prevStatus.HasValue
                        && currentId == prevId.Value && currentStatus == prevStatus.Value)
                    {
                        continue;
                    }

                    if (currentStatus == expectedLane)
                    {
                        Log.Debug("[Camera:{Camera}] ACK Pass code={Code} lane={Lane} after {Ms}ms",
                            camera, code, expectedLane, sw.ElapsedMilliseconds);
                        return e_TimeoutCheckResult.Pass;
                    }

                    if (prevId.HasValue && prevStatus.HasValue
                        && (currentId != prevId.Value || currentStatus != prevStatus.Value))
                    {
                        Log.Warning("[Camera:{Camera}] ACK Reject code={Code} status={Status} (expected={Lane})",
                            camera, code, currentStatus, expectedLane);
                        return e_TimeoutCheckResult.Timeout;
                    }
                }

                Log.Warning("[Camera:{Camera}] ACK Timeout code={Code} after {Ms}ms",
                    camera, code, sw.ElapsedMilliseconds);
                return e_TimeoutCheckResult.Timeout;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[Camera] WaitPlcAckV2 threw");
                return e_TimeoutCheckResult.Error;
            }
        }

        /// <summary>
        /// Ghi audit record cho mã bị reject do gate bận / state không cho phép.
        /// Mã này KHÔNG vào lookup, KHÔNG kích hoạt, chỉ audit + đẩy FE.
        /// </summary>
        public void RecordBusyRejection(CameraReadResult result, bool plcSent)
        {
            string status = result.Status switch
            {
                e_Production_Status.FormatError => "FormatFail",
                e_Production_Status.ReadFail => "ReadFail",
                e_Production_Status.Duplicate => "Duplicate",
                e_Production_Status.NotFound => "NotFound",
                e_Production_Status.Error => "Error",
                e_Production_Status.Timeout => "Timeout",
                _ => "Error",
            };
            ActiveCounter.FailTotal++;
            switch (result.Status)
            {
                case e_Production_Status.FormatError:
                    ActiveCounter.FormatFailCount++;
                    break;
                case e_Production_Status.ReadFail:
                    ActiveCounter.ReadFailCount++;
                    break;
                case e_Production_Status.NotFound:
                    ActiveCounter.NotFoundCount++;
                    break;
                case e_Production_Status.Duplicate:
                    ActiveCounter.DuplicateCount++;
                    break;
                case e_Production_Status.Error:
                    ActiveCounter.ErrorCount++;
                    break;
                case e_Production_Status.Timeout:
                    ActiveCounter.TimeoutCount++;
                    break;
            }

            RecordQueue.Enqueue(new RecordData
            {
                Code = result.Code,
                Status = status,
                PLCStatus = plcSent ? "PASS" : "FAIL",
                ActivateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ActivateUser = "<API>",
                ProductionDate = ProductionData?.ProductionDate ?? "0",
            });
        }

        private int _ackTimeoutMs = 500;
        private int _ackPollIntervalMs = 10;

        /// <summary>Apply PLC ACK timeout/poll config từ AppConfig.</summary>
        public void ApplyAckConfig(int timeoutMs, int pollIntervalMs)
        {
            if (timeoutMs > 0) _ackTimeoutMs = timeoutMs;
            if (pollIntervalMs > 0) _ackPollIntervalMs = pollIntervalMs;
            Log.Information("[StateMachine] ACK V2 timeout={TimeoutMs}ms poll={PollMs}ms",
                _ackTimeoutMs, _ackPollIntervalMs);
        }
    }
}
