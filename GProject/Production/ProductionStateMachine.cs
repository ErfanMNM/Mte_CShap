using System.Collections.Concurrent;
using GProject.ProductionOrderHelpers;
using Serilog;

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

        private readonly object _stateLock = new();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private e_ProductionState _currentState = e_ProductionState.NeedLogin;
        private e_ProductionState _previousState = e_ProductionState.NeedLogin;

        // Background DB writer (3 queue + 1 consumer thread duy nhất)
        public ConcurrentQueue<RecordData> RecordQueue { get; } = new();
        public ConcurrentQueue<CodeUpdateItem> CodeUpdateQueue { get; } = new();
        public ConcurrentQueue<CartonUpdateItem> CartonUpdateQueue { get; } = new();

        private CancellationTokenSource? _writerCts;
        private Task? _writerTask;

        // Dữ liệu PO hiện tại trong RAM
        public static POInfo? ProductionData { get; set; }

        // Dictionary lưu mã sản phẩm cho lookup nhanh
        public ConcurrentDictionary<string, CodeInfo> Dictionary_Codes { get; } = new();

        // Dictionary lưu thông tin carton
        public ConcurrentDictionary<int, CartonInfo> Dictionary_Cartons { get; } = new();

        // Bộ đếm
        public ProductCounter ActiveCounter { get; private set; } = new();

        /// <summary>
        /// Cập nhật ActiveCounter.TotalCount từ PLC.
        /// </summary>
        public void UpdateActiveCounterTotal(int totalCount)
        {
            lock (_stateLock)
            {
                ActiveCounter.TotalCount = totalCount;
            }
        }
        public ProductCounter PackageCounter { get; private set; } = new();

        // PLC monitor reference (injected by Program.cs after Start)
        private PLCMonitor? _plcMonitor;

        /// <summary>Program.cs inject sau khi _plcMonitor.Start().</summary>
        public PLCMonitor? PLCMonitor
        {
            set => _plcMonitor = value;
        }

        // Cờ trạng thái thiết bị
        public bool IsAppReady { get; set; }
        public bool IsDeviceReady { get; set; }
        public string LastWarning { get; set; } = "";

        // Trạng thái kết nối thiết bị
        public bool IsPLCConnected { get; private set; }
        public bool IsCameraConnected { get; private set; }
        public string DeviceDisconnectReason { get; private set; } = "";

        // Cấu hình
        public int CartonWarning { get; set; } = 3;
        public int CartonOffset { get; set; } = 1;

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
            IsPLCConnected = true;
            IsCameraConnected = true;
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
                if (_currentState == newState) return;
                _previousState = _currentState;
                _currentState = newState;
            }
            Log.Information("[StateMachine] {Prev} -> {New} ({Reason})",
                _previousState, newState, reason ?? "no reason");
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
        /// Dừng background consumer thread
        /// </summary>
        private async Task StopDbWriterAsync()
        {
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
                        var result = GProduction.PORecordHelper.Record(ProductionData.OrderNo, rec);
                        if (!result.IsSuccess)
                            Log.Warning("[DB Writer] Record lỗi: {Msg}", result.Message);
                    }
                    else if (CodeUpdateQueue.TryDequeue(out var codeUpd))
                    {
                        var result = GProduction.PORecordHelper.UpdateCodeStatusAndCarton(
                            codeUpd.OrderNo, codeUpd.Code, codeUpd.ActivateDate, codeUpd.ActivateUser,
                            codeUpd.PackingDate, codeUpd.CartonCode, codeUpd.ProductionDate);
                        if (!result.IsSuccess)
                            Log.Warning("[DB Writer] UpdateCode lỗi: {Msg}", result.Message);
                    }
                    else if (CartonUpdateQueue.TryDequeue(out var cartUpd))
                    {
                        if (cartUpd.Type == CartonUpdateType.Complete)
                        {
                            var result = GProduction.POCarton.CompleteCarton(
                                cartUpd.OrderNo, cartUpd.CartonId, cartUpd.ActivateUser);
                            if (!result.IsSuccess)
                                Log.Warning("[DB Writer] CompleteCarton lỗi: {Msg}", result.Message);

                            // Sau khi đóng thùng hiện tại, tự động start thùng kế tiếp nếu có
                            var startResult = GProduction.POCarton.StartCarton(
                                cartUpd.OrderNo, cartUpd.CartonId + 1, cartUpd.ActivateUser);
                            if (!startResult.IsSuccess)
                                Log.Warning("[DB Writer] StartCarton kế tiếp lỗi: {Msg}", startResult.Message);
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
            string code = rawCode?.Trim() ?? "";
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var at = DateTime.UtcNow;
            var cam = string.IsNullOrEmpty(camera) ? "camera" : camera;

            // Nhánh 1: mã rỗng -> ReadFail
            if (string.IsNullOrEmpty(code))
            {
                lock (_stateLock)
                {
                    ActiveCounter.ReadFailCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = "FAIL",
                        Status = "ReadFail",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>"
                    });
                }
                Log.Warning("[Camera] Empty code");
                return WithPLCWrite(
                    new CameraReadResult(cam, "", e_Production_Status.ReadFail, false, null, null, at),
                    e_Production_Status.ReadFail, null, code: "");
            }

            // Nhánh 2: marker Omron <GS>/<RS>/<US> -> ReadFail
            if (code.Contains("<GS>") || code.Contains("<RS>") || code.Contains("<US>"))
            {
                lock (_stateLock)
                {
                    ActiveCounter.ReadFailCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "ReadFail",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>"
                    });
                }
                Log.Warning("[Camera] Code contains special marker: {Code}", code);
                return WithPLCWrite(
                    new CameraReadResult(cam, code, e_Production_Status.ReadFail, false, null, null, at),
                    e_Production_Status.ReadFail, null, code: code);
            }

            // Nhánh 3: chỉ xử lý khi đang Running; ngược lại bỏ qua (không phát badge)
            if (CurrentState != e_ProductionState.Running)
            {
                Log.Debug("[Camera] Bỏ qua mã {Code} vì state={State}", code, CurrentState);
                return null;
            }
            if (ProductionData == null)
            {
                Log.Warning("[Camera] Bỏ qua mã {Code} vì chưa chọn PO", code);
                return null;
            }

            // Trim marker ASCII thành ký tự control (giống FDashboard.cs:529)
            code = code.Replace("<GS>", "\u001D")
                       .Replace("<RS>", "\u001E")
                       .Replace("<US>", "\u001F");

            // Biến capture NGOÀI lock (gán giá trị bên trong lock scope 1)
            string currentCartonCode = "0";
            CodeInfo infoSnapshot = default!;
            int activeCartonId = 0;

            lock (_stateLock)
            {
                ActiveCounter.TotalCount++;

                // Nhánh 4: NotFound
                if (!Dictionary_Codes.TryGetValue(code, out var info))
                {
                    ActiveCounter.NotFoundCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "NotFound",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = ProductionData.ProductionDate
                    });
                    Log.Warning("[Camera] NotFound: {Code}", code);
                    return WithPLCWrite(
                        new CameraReadResult(cam, code, e_Production_Status.NotFound, false, null, ActiveCounter.CartonID, at),
                        e_Production_Status.NotFound, ActiveCounter.CartonID, code: code);
                }

                // Nhánh 5: Duplicate (đã active từ trước)
                if (info.Status == 1)
                {
                    ActiveCounter.DuplicateCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        CartonCode = info.CartonCode,
                        Status = "Duplicate",
                        PLCStatus = "PASS",
                        ActivateDate = info.ActivateDate,
                        ActivateUser = info.ActivateUser,
                        PackingDate = info.PackingDate,
                        ProductionDate = ProductionData.ProductionDate
                    });
                    Log.Warning("[Camera] Duplicate: {Code} carton={Carton}", code, info.CartonCode);
                    return WithPLCWrite(
                        new CameraReadResult(cam, code, e_Production_Status.Duplicate, true, info.CartonCode, ActiveCounter.CartonID, at),
                        e_Production_Status.Duplicate, ActiveCounter.CartonID, code: code);
                }

                // 3) Lấy cartonCode hiện tại
                if (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var carton))
                    currentCartonCode = carton.CartonCode;

                // Nhánh 6: thùng hiện tại chưa có mã -> Error + Pause
                if (currentCartonCode == "0" || string.IsNullOrEmpty(currentCartonCode))
                {
                    ActiveCounter.FailCount++;
                    ActiveCounter.ErrorCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "Error",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = ProductionData.ProductionDate
                    });
                    LastWarning = $"Thùng hiện tại (ID={ActiveCounter.CartonID}) chưa có mã";
                    Log.Warning("[Camera] Reject {Code}: {Warning}", code, LastWarning);
                    SetState(e_ProductionState.Paused, LastWarning);
                    return WithPLCWrite(
                        new CameraReadResult(cam, code, e_Production_Status.Error, false, null, ActiveCounter.CartonID, at),
                        e_Production_Status.Error, ActiveCounter.CartonID, code: code);
                }

                // === Snapshot + thoát lock sớm để ghi PLC + check timeout NGOÀI lock ===
                activeCartonId = ActiveCounter.CartonID;
                infoSnapshot = info;
            } // <- đóng lock sớm tại đây

            // === NGOÀI LOCK ===
            // 5) Ghi phân làn xuống PLC NGAY (fire-and-forget, 1/2 theo carton id)
            WritePLCResponse(e_Production_Status.Pass, activeCartonId, code);

            // 6) Poll PLC D200 + D202 để xác nhận (NGOÀI lock, tối đa 5s).
            //var timeoutResult = CheckTimeoutV2(code);
            var timeoutResult = e_TimeoutCheckResult.Pass; // TODO: tạm bỏ check timeout PLC để test nhanh
            if (timeoutResult == e_TimeoutCheckResult.Pass)
            {
                // 7) Re-lock để commit dic + DB + counter
                lock (_stateLock)
                {
                    // Cập nhật Dictionary ngay trong RAM (chỉ khi Pass)
                    infoSnapshot.Status = 1;
                    infoSnapshot.ActivateDate = now;
                    infoSnapshot.ActivateUser = "<API>";
                    infoSnapshot.CartonCode = currentCartonCode;
                    infoSnapshot.PackingDate = now;
                    infoSnapshot.ProductionDate = ProductionData!.ProductionDate;
                    Dictionary_Codes[code] = infoSnapshot;

                    // Enqueue ghi Record (audit)
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
                        ProductionDate = ProductionData.ProductionDate
                    });

                    // Enqueue ghi CodeUpdate
                    CodeUpdateQueue.Enqueue(new CodeUpdateItem
                    {
                        OrderNo = ProductionData.OrderNo,
                        Code = code,
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        PackingDate = now,
                        CartonCode = currentCartonCode,
                        ProductionDate = ProductionData.ProductionDate
                    });

                    // Tăng bộ đếm
                    ActiveCounter.PassCount++;
                    PackageCounter.PassCount++;
                    ActiveCounter.CartonCode = currentCartonCode;

                    // Đủ carton -> enqueue CompleteCarton + next carton
                    if (PackageCounter.PassCount >= ActiveCounter.CartonCapacity)
                    {
                        int currentCartonID = ActiveCounter.CartonID;
                        CartonUpdateQueue.Enqueue(new CartonUpdateItem
                        {
                            Type = CartonUpdateType.Complete,
                            OrderNo = ProductionData.OrderNo,
                            CartonId = currentCartonID,
                            ActivateUser = "<API>"
                        });

                        if (Dictionary_Cartons.TryGetValue(currentCartonID, out var closedCarton))
                            closedCarton.CompletedDatetime = now;

                        ActiveCounter.CartonID++;
                        PackageCounter.PassCount = 0;

                        if (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var nextCarton))
                        {
                            ActiveCounter.CartonCode = nextCarton.CartonCode;

                            if (nextCarton.CartonCode == "0" || string.IsNullOrEmpty(nextCarton.CartonCode))
                            {
                                LastWarning = $"Thùng kế tiếp (ID={ActiveCounter.CartonID}) chưa có mã";
                                SetState(e_ProductionState.Paused, LastWarning);
                                return new CameraReadResult(cam, code, e_Production_Status.Error, false, null, currentCartonID, at);
                            }
                        }
                        else
                        {
                            LastWarning = $"Không tìm thấy thùng kế tiếp (ID={ActiveCounter.CartonID})";
                            SetState(e_ProductionState.Paused, LastWarning);
                            return new CameraReadResult(cam, code, e_Production_Status.Error, false, null, currentCartonID, at);
                        }
                    }

                    // Đạt orderQty -> WaitingStop
                    if (ActiveCounter.PassCount >= ProductionData.OrderQty)
                    {
                        SetState(e_ProductionState.WaitingStop, $"orderQty {ProductionData.OrderQty} reached");
                    }
                }

                return new CameraReadResult(cam, code, e_Production_Status.Pass, true,
                                            currentCartonCode, activeCartonId, at);
            }
            else
            {
                // PLC timeout / reject → KHÔNG update dic, KHÔNG tăng counter.
                // Chỉ ghi DB Status=Timeout để audit (mã còn trong dic với Status=0,
                // lần sau còn quét lại được).
                lock (_stateLock)
                {
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        CartonCode = currentCartonCode,
                        Status = "Timeout",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>",
                        ProductionDate = ProductionData!.ProductionDate
                    });
                }

                return new CameraReadResult(cam, code, e_Production_Status.Timeout, false,
                                            currentCartonCode, activeCartonId, at);
            }
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
                    break;

                case e_ProductionState.PushingToDic:
                    ProcessPushToDicState();
                    break;

                case e_ProductionState.Running:
                    ProcessRunningState();
                    break;

                case e_ProductionState.Paused:
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
                    if (!string.IsNullOrEmpty(DeviceDisconnectReason))
                    {
                        Log.Error("[DeviceError] {Reason}", DeviceDisconnectReason);
                    }
                    TryReconnectDevices();
                    break;
                case e_ProductionState.Error:
                    // Đã lỗi - chờ xử lý
                    break;
            }
        }

        /// <summary>
        /// Xử lý thay đổi trạng thái thiết bị (PLC/Camera)
        /// </summary>
        public void OnDeviceStateChanged(string deviceName, bool isConnected, string message)
        {
            lock (_stateLock)
            {
                if (deviceName == "PLC")
                    IsPLCConnected = isConnected;
                else if (deviceName == "Camera")
                    IsCameraConnected = isConnected;

                if (!isConnected)
                {
                    DeviceDisconnectReason = $"{deviceName}: {message}";
                    Log.Warning("[Device] {Device} disconnected: {Msg}", deviceName, message);

                    if (CurrentState == e_ProductionState.Running ||
                        CurrentState == e_ProductionState.Ready ||
                        CurrentState == e_ProductionState.Paused)
                    {
                        SetState(e_ProductionState.DeviceError, DeviceDisconnectReason);
                    }
                }
                else
                {
                    Log.Information("[Device] {Device} reconnected", deviceName);
                    // Không xóa DeviceDisconnectReason ở đây - để TryReconnectDevices tự quyết định
                    // khi CẢ HAI thiết bị đều connected thì mới clear và resume.
                }
            }
        }

        /// <summary>
        /// Tự động reconnect và resume khi thiết bị kết nối lại
        /// </summary>
        private void TryReconnectDevices()
        {
            if (IsPLCConnected && IsCameraConnected && !string.IsNullOrEmpty(DeviceDisconnectReason))
            {
                Log.Information("[DeviceError] All devices reconnected, resuming...");
                DeviceDisconnectReason = "";

                if (PreviousState == e_ProductionState.Running ||
                    PreviousState == e_ProductionState.Paused)
                {
                    SetState(PreviousState, "devices reconnected");
                }
                else
                {
                    SetState(e_ProductionState.Ready, "devices reconnected");
                }
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

                var codesResult = GProduction.POLoader.GetCodes(ProductionData.OrderNo);
                if (codesResult.IsSuccess && codesResult.Data != null)
                {
                    Dictionary_Codes.Clear();
                    foreach (System.Data.DataRow row in codesResult.Data.Rows)
                    {
                        var info = new CodeInfo
                        {
                            Code = row["Code"]?.ToString() ?? "",
                            OrderNo = ProductionData.OrderNo,
                            Status = Convert.ToInt32(row["Status"] ?? 0),
                            CartonCode = row["cartonCode"]?.ToString() ?? "0",
                            ActivateDate = row["ActivateDate"]?.ToString() ?? "0",
                            ProductionDate = row["ProductionDate"]?.ToString() ?? "0",
                            ActivateUser = row["ActivateUser"]?.ToString() ?? "",
                            PackingDate = row["PackingDate"]?.ToString() ?? "0"
                        };
                        Dictionary_Codes[info.Code] = info;
                    }
                }

                var cartonsResult = GProduction.POCarton.GetAll(ProductionData.OrderNo);
                if (cartonsResult.IsSuccess && cartonsResult.Data != null)
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
                    // 1) Tìm cartonID đang chạy dở (Start_Datetime != '0' AND Completed_Datetime = '0')
                    //    Nếu không có thì lấy carton đầu tiên chưa Completed
                    var lastRunning = Dictionary_Cartons.Values
                        .Where(c => c.StartDatetime != "0" && c.CompletedDatetime == "0")
                        .OrderBy(c => c.Id)
                        .FirstOrDefault();

                    if (lastRunning != null)
                    {
                        ActiveCounter.CartonID = lastRunning.Id;
                        ActiveCounter.CartonCode = lastRunning.CartonCode;
                    }
                    else
                    {
                        // Tìm carton chưa đóng đầu tiên (Completed_Datetime = '0')
                        var firstOpen = Dictionary_Cartons.Values
                            .Where(c => c.CompletedDatetime == "0")
                            .OrderBy(c => c.Id)
                            .FirstOrDefault();
                        ActiveCounter.CartonID = firstOpen?.Id ?? 1;
                        ActiveCounter.CartonCode = firstOpen?.CartonCode ?? "";
                    }

                    // 2) Tính lại PackageCounter.PassCount = số mã đã pack trong carton hiện tại
                    if (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var curCarton)
                        && curCarton.CartonCode != "0")
                    {
                        PackageCounter.PassCount = GProduction.PORecordHelper.GetCodeCountInCarton(
                            ProductionData.OrderNo, curCarton.CartonCode);
                    }
                    else
                    {
                        PackageCounter.PassCount = 0;
                    }

                    // 3) Tính lại ActiveCounter.PassCount = tổng mã đã active trong PO
                    ActiveCounter.PassCount = GProduction.PORecordHelper.GetActiveCount(ProductionData.OrderNo);

                    Log.Information("[StateMachine] Reload state: cartonID={CartonId}, cartonCode={CartonCode}, packed={Packed}, totalActive={Active}",
                        ActiveCounter.CartonID, ActiveCounter.CartonCode, PackageCounter.PassCount, ActiveCounter.PassCount);
                }

                SetState(e_ProductionState.Running, "data pushed to dic");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[StateMachine] Lỗi PushToDic");
                SetState(e_ProductionState.Error, ex.Message);
            }
        }

        /// <summary>
        /// Running: kiểm tra carton hiện tại và carton sắp tới
        /// - Nếu thùng hiện tại chưa có mã -> Pause
        /// - Nếu thùng sắp tới chưa có mã và sắp đầy -> Pause
        /// </summary>
        private void ProcessRunningState()
        {
            if (ProductionData == null)
            {
                SetState(e_ProductionState.NoSelectedPO, "ProductionData null in Running");
                return;
            }

            int currentCartonId = ActiveCounter.CartonID;
            int packedCount = PackageCounter.PassCount;

            // Kiểm tra thùng hiện tại
            if (Dictionary_Cartons.TryGetValue(currentCartonId, out var currentCarton))
            {
                if (currentCarton.CartonCode == "0")
                {
                    LastWarning = "Thùng hiện tại chưa có mã";
                    SetState(e_ProductionState.Paused, LastWarning);
                    return;
                }
            }
            else
            {
                LastWarning = "Thùng hiện tại chưa tồn tại";
                SetState(e_ProductionState.Ready, LastWarning);
                return;
            }

            // Kiểm tra thùng sắp tới
            int nextThreshold = ActiveCounter.CartonCapacity - CartonOffset;
            if (packedCount >= nextThreshold)
            {
                if (Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var nextCarton))
                {
                    if (nextCarton.CartonCode == "0")
                    {
                        LastWarning = "Thùng sắp tới chưa có mã";
                        SetState(e_ProductionState.Paused, LastWarning);
                        return;
                    }
                }
            }

            // Cảnh báo sớm
            int warningThreshold = ActiveCounter.CartonCapacity - CartonWarning;
            if (packedCount >= warningThreshold)
            {
                if (Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var nextCarton)
                    && nextCarton.CartonCode == "0")
                {
                    LastWarning = "Cảnh báo: thùng sắp tới chưa có mã";
                    Log.Warning("[StateMachine] {Warning}", LastWarning);
                }
            }
        }

        /// <summary>
        /// Pause: chờ thùng có mã rồi quay lại Running
        /// </summary>
        private void ProcessPauseState()
        {
            if (ProductionData == null)
            {
                SetState(e_ProductionState.NoSelectedPO, "ProductionData null in Pause");
                return;
            }

            int currentCartonId = ActiveCounter.CartonID;

            // Nếu thùng hiện tại đã có mã -> về Running
            if (Dictionary_Cartons.TryGetValue(currentCartonId, out var carton)
                && carton.CartonCode != "0")
            {
                SetState(e_ProductionState.Running, "carton có mã");
                return;
            }

            // Nếu thùng sắp tới đã có mã -> về Running
            if (Dictionary_Cartons.TryGetValue(currentCartonId + 1, out var nextCarton)
                && nextCarton.CartonCode != "0"
                && PackageCounter.PassCount < (ActiveCounter.CartonCapacity - CartonOffset))
            {
                SetState(e_ProductionState.Running, "next carton có mã");
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

        private void WritePLCResponse(e_Production_Status status, int? cartonId, string code)
        {
            if (status == e_Production_Status.Timeout) return; // PLC tự trả lời, không ghi đè
            int id = cartonId ?? 0;
            short value = MapResultForPLC(status, id);
            try
            {
                _plcMonitor?.WriteResultFireAndForget(value);
                Log.Debug("[Camera] PLC write {Value} code={Code} status={Status} cartonId={Id}",
                          value, code, status, id);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[Camera] WritePLCResponse failed");
            }
        }

        /// <summary>
        /// Ghi 0 xuống PLC khi bỏ qua mã camera (state != Running).
        /// PLC sẽ tự reject mã này, phần mềm KHÔNG xử lý DB / counter.
        /// Dùng khi muốn "thả lại" mã về sau.
        /// </summary>
        public void WritePLCResponseSkip()
        {
            try
            {
                _plcMonitor?.WriteResultFireAndForget(0);
                Log.Debug("[Camera] PLC skip (state != Running) — wrote 0");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[Camera] WritePLCResponseSkip failed");
            }
        }

        private CameraReadResult WithPLCWrite(
            CameraReadResult result,
            e_Production_Status status,
            int? cartonId,
            string code)
        {
            WritePLCResponse(status, cartonId, code);
            return result;
        }

        /// <summary>
        /// Poll D200 (ID) + D202 (status) của PLC để đồng bộ kết quả sau khi ghi phân làn.
        /// Nguyên lý (tham chiếu CameraSubPlcSyncV2Helper.WaitAndResolve):
        ///   - PLC giữ D200 = ID sản phẩm đang xử lý (int32, tăng dần).
        ///   - D202 = status: 5 = idle, 0/1/2 = kết quả (1/2 = pass), 3 = reject.
        ///   - Khi ghi D300 (1 hoặc 2), PLC xử lý rồi cập nhật D202. Nếu D202 đổi
        ///     khỏi giá trị snapshot → đó là kết quả thật. Nếu D200 nhảy (currentId >
        ///     snapshotId) trước khi D202 đổi → ID đã chuyển sang chai khác, tính timeout.
        /// </summary>
        public e_TimeoutCheckResult CheckTimeoutV2(string code, int? timeoutMs = null, int? pollMs = null)
        {
            if (_plcMonitor is null) return e_TimeoutCheckResult.PlcUnavailable;
            if (string.IsNullOrEmpty(code)) return e_TimeoutCheckResult.Error;

            int ms = timeoutMs
                ?? (int.TryParse(Environment.GetEnvironmentVariable("PLC_TIMEOUT_MS"), out var t) ? t : 5000);
            int poll = pollMs
                ?? (int.TryParse(Environment.GetEnvironmentVariable("PLC_TIMEOUT_POLL_MS"), out var p) ? p : 50);

            var idDm = Environment.GetEnvironmentVariable("PLC_TIMEOUT_ID_DM")
                       ?? PLCMonitor.DefaultTimeoutIdDm;
            var statusDm = Environment.GetEnvironmentVariable("PLC_TIMEOUT_STATUS_DM")
                           ?? PLCMonitor.DefaultTimeoutStatusDm;

            try
            {
                var snapIdR = _plcMonitor.ReadInt32Safe(idDm, 1);
                var snapStatusR = _plcMonitor.ReadInt32Safe(statusDm, 1);
                if (!snapIdR.Success || !snapStatusR.Success
                    || snapIdR.Value.Length == 0 || snapStatusR.Value.Length == 0)
                {
                    Log.Warning("[Camera] CheckTimeoutV2: cannot snapshot D200/D202");
                    return e_TimeoutCheckResult.PlcUnavailable;
                }

                int expectedId = snapIdR.Value[0];
                int prevStatus = snapStatusR.Value[0];

                var deadline = DateTime.UtcNow.AddMilliseconds(ms);
                while (DateTime.UtcNow < deadline)
                {
                    var curIdR = _plcMonitor.ReadInt32Safe(idDm, 1);
                    if (!curIdR.Success || curIdR.Value.Length == 0)
                    {
                        Thread.Sleep(poll);
                        continue;
                    }
                    int currentId = curIdR.Value[0];

                    if (currentId > expectedId)
                    {
                        Log.Warning("[Camera] CheckTimeoutV2: ID jumped {Exp}->{Cur} for {Code}",
                                    expectedId, currentId, code);
                        return e_TimeoutCheckResult.Timeout;
                    }

                    if (currentId == expectedId)
                    {
                        var curStatusR = _plcMonitor.ReadInt32Safe(statusDm, 1);
                        if (curStatusR.Success && curStatusR.Value.Length > 0)
                        {
                            int currentStatus = curStatusR.Value[0];
                            if (currentStatus != prevStatus)
                            {
                                if (currentStatus == 1 || currentStatus == 2)
                                    return e_TimeoutCheckResult.Pass;
                                return e_TimeoutCheckResult.Timeout;
                            }
                        }
                    }

                    Thread.Sleep(poll);
                }

                Log.Warning("[Camera] CheckTimeoutV2: timeout {Ms}ms for {Code}", ms, code);
                return e_TimeoutCheckResult.Timeout;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[Camera] CheckTimeoutV2 failed");
                return e_TimeoutCheckResult.Error;
            }
        }
    }
}
