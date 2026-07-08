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

        private DateTime _lastBroadcast = DateTime.MinValue;
        private static readonly TimeSpan BroadcastThrottle = TimeSpan.FromMilliseconds(500);

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

        // Cờ trạng thái thiết bị
        public bool IsAppReady { get; set; }
        public bool IsDeviceReady { get; set; }
        public string LastWarning { get; set; } = "";

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

        private ProductionStateMachine() { }

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

            // Broadcast ngay lập tức khi state đổi
            _ = BroadcastStateAsync();
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

        private DateTime _lastPeriodicBroadcast = DateTime.MinValue;
        private static readonly TimeSpan PeriodicBroadcastInterval = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Broadcast production state qua ProductionHub WebSocket.
        /// Throttle: chỉ broadcast nếu đã qua >= 500ms kể từ lần cuối.
        /// </summary>
        private async Task BroadcastStateAsync()
        {
            try
            {
                var now = DateTime.Now;
                if ((now - _lastBroadcast) < BroadcastThrottle) return;
                _lastBroadcast = now;

                var sm = ProductionStateMachine.Instance;
                await ProductionHub.Instance.BroadcastStateAsync(
                    sm.CurrentState.ToString(),
                    sm.PreviousState.ToString(),
                    ProductionData?.OrderNo,
                    ProductionData?.ProductName,
                    ProductionData?.OrderQty ?? 0,
                    new
                    {
                        sm.ActiveCounter.PassCount,
                        sm.ActiveCounter.FailCount,
                        sm.ActiveCounter.DuplicateCount,
                        sm.ActiveCounter.CartonID,
                        sm.ActiveCounter.CartonCode
                    },
                    sm.LastWarning,
                    sm.IsAppReady,
                    sm.IsDeviceReady,
                    sm.Dictionary_Codes.Count,
                    sm.Dictionary_Cartons.Count
                );
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[StateMachine] Broadcast failed");
            }
        }

        /// <summary>
        /// Xử lý 1 mã nhận được từ camera - luồng chính của chức năng 1-camera:
        /// 1) Chuẩn hoá mã + loại FAIL
        /// 2) Tra Dictionary_Codes O(1)
        /// 3) Nếu Status=0: update RAM -> enqueue Record + CodeUpdate -> tăng PassCount
        /// 4) Nếu đủ sức chứa carton -> enqueue CartonComplete (start thùng kế tiếp ở consumer)
        /// 5) Nếu Status=1: Duplicate
        /// 6) Nếu không có trong Dictionary: NotFound
        /// </summary>
        public void HandleCodeFromCamera(string? rawCode)
        {
            // Chuẩn hoá mã giống MASAN
            string code = rawCode?.Trim() ?? "";
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (string.IsNullOrEmpty(code))
            {
                lock (_stateLock)
                {
                    ActiveCounter.ReadFailCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = "FAIL",
                        Status = "FAIL",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>"
                    });
                }
                Log.Warning("[Camera] Empty code");
                return;
            }

            // Xử lý marker đặc biệt Omron (<GS>, <RS>, <US>)
            if (code.Contains("<GS>") || code.Contains("<RS>") || code.Contains("<US>"))
            {
                lock (_stateLock)
                {
                    ActiveCounter.ReadFailCount++;
                    RecordQueue.Enqueue(new RecordData
                    {
                        Code = code,
                        Status = "FAIL",
                        PLCStatus = "FAIL",
                        ActivateDate = now,
                        ActivateUser = "<API>"
                    });
                }
                Log.Warning("[Camera] Code contains special marker: {Code}", code);
                return;
            }

            // Chỉ xử lý khi đang chạy
            if (CurrentState != e_ProductionState.Running)
            {
                Log.Debug("[Camera] Bỏ qua mã {Code} vì state={State}", code, CurrentState);
                return;
            }

            if (ProductionData == null)
            {
                Log.Warning("[Camera] Bỏ qua mã {Code} vì chưa chọn PO", code);
                return;
            }

            lock (_stateLock)
            {
                ActiveCounter.TotalCount++;

                // 1) Tra Dictionary
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
                    return;
                }

                // 2) Đã active rồi -> Duplicate
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
                    return;
                }

                // 3) Lấy cartonCode hiện tại
                string currentCartonCode = "0";
                if (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var carton))
                    currentCartonCode = carton.CartonCode;

                // 4) Thùng hiện tại chưa có mã -> Reject
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
                    return;
                }

                // 5) Cập nhật Dictionary ngay trong RAM
                info.Status = 1;
                info.ActivateDate = now;
                info.ActivateUser = "<API>";
                info.CartonCode = currentCartonCode;
                info.PackingDate = now;
                info.ProductionDate = ProductionData.ProductionDate;
                Dictionary_Codes[code] = info;

                // 6) Enqueue ghi Record ngay (audit)
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

                // 7) Enqueue ghi CodeUpdate ngay (update UniqueCodes)
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

                // 8) Tăng bộ đếm
                ActiveCounter.PassCount++;
                PackageCounter.PassCount++;
                ActiveCounter.CartonCode = currentCartonCode;

                // 9) Đủ carton -> enqueue CompleteCarton + StartCarton next (consumer thread sẽ xử lý)
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

                    // Cập nhật RAM: carton đã đóng + chuyển sang carton kế tiếp
                    if (Dictionary_Cartons.TryGetValue(currentCartonID, out var closedCarton))
                    {
                        closedCarton.CompletedDatetime = now;
                    }

                    ActiveCounter.CartonID++;
                    PackageCounter.PassCount = 0;

                    // Lấy cartonCode mới (nếu có)
                    if (Dictionary_Cartons.TryGetValue(ActiveCounter.CartonID, out var nextCarton))
                    {
                        ActiveCounter.CartonCode = nextCarton.CartonCode;

                        // Thùng kế tiếp chưa có mã -> Pause
                        if (nextCarton.CartonCode == "0" || string.IsNullOrEmpty(nextCarton.CartonCode))
                        {
                            LastWarning = $"Thùng kế tiếp (ID={ActiveCounter.CartonID}) chưa có mã";
                            SetState(e_ProductionState.Paused, LastWarning);
                            return;
                        }
                    }
                    else
                    {
                        LastWarning = $"Không tìm thấy thùng kế tiếp (ID={ActiveCounter.CartonID})";
                        SetState(e_ProductionState.Paused, LastWarning);
                        return;
                    }
                }

                // 10) Đạt orderQty -> chuyển WaitingStop
                if (ActiveCounter.PassCount >= ProductionData.OrderQty)
                {
                    SetState(e_ProductionState.WaitingStop, $"orderQty {ProductionData.OrderQty} reached");
                }
            }
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

                // Periodic broadcast mỗi 500ms để update counter
                try
                {
                    var now = DateTime.Now;
                    if ((now - _lastPeriodicBroadcast) >= PeriodicBroadcastInterval)
                    {
                        _lastPeriodicBroadcast = now;
                        _ = BroadcastStateAsync();
                    }
                }
                catch { }
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
                case e_ProductionState.Error:
                    // Đã lỗi - chờ xử lý
                    break;
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
                    ProductionData.OrderNo, ProductionData.OrderQty, ActiveCounter.CartonCapacity);

                if (!status.IsFullyInitialized)
                {
                    // Tạo DB và load codes
                    var result = GProduction.POCreator.EnsurePODatabaseReady(
                        ProductionData.OrderNo,
                        ProductionData.Gtin,
                        ProductionData.OrderQty,
                        ActiveCounter.CartonCapacity,
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
                ActiveCounter = new ProductCounter();
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
    }
}
