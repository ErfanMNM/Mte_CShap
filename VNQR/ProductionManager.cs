using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using VNQR.Helpers;
using VNQR.Infrastructure;

namespace VNQR
{
    /// <summary>
    /// ProductionManager - Quản lý luồng sản xuất chính
    /// 
    /// CƠ CHẾ:
    /// 1. User chọn PO từ SelectPOForm
    /// 2. Load codes từ DataPool vào PO database
    /// 3. Load codes vào Dictionary để lookup nhanh
    /// 4. Chạy sản xuất:
    ///    - Camera Active: quét mã -> kiểm tra trùng -> activate -> gửi PLC OK
    ///    - Camera Package: quét mã -> kiểm tra đã activate -> pack vào thùng
    /// 5. Khi thùng đầy -> tự động chuyển thùng mới
    /// 6. Khi hết mã -> hoàn thành PO
    /// </summary>
    public static class ProductionManager
    {
        // ===== Events =====
        public static event Action<string>? OnLog;
        public static event Action? OnStateChanged;
        public static event Action? OnCountersUpdated;

        // ===== Private Fields =====
        private static CancellationTokenSource? _cancellationTokenSource;
        private static bool _isProcessing = false;

        // ===== State Machine =====
        public static void SetState(e_ProductionState newState)
        {
            if (GV.AppState != newState)
            {
                GV.AppState = newState;
                OnStateChanged?.Invoke();
            }
        }

        // ===== PO Selection =====
        /// <summary>
        /// Lấy danh sách PO từ database
        /// </summary>
        public static po.Result GetPOList()
        {
            return po.POLoader.GetAll();
        }

        /// <summary>
        /// Chọn và khởi tạo PO
        /// </summary>
        public static async Task<(bool success, string message)> SelectAndInitializePO(string orderNo, string userName)
        {
            try
            {
                Log("[PO] Bắt đầu khởi tạo PO...");

                // 1. Lấy thông tin PO
                var poResult = po.POLoader.GetByOrderNo(orderNo);
                if (!poResult.IsSuccess || poResult.Data == null || poResult.Data.Rows.Count == 0)
                    return (false, $"Không tìm thấy PO: {orderNo}");

                var poRow = poResult.Data.Rows[0];

                // 2. Set thông tin vào GV
                GV.OrderNo = orderNo;
                GV.Gtin = poRow["gtin"]?.ToString() ?? "";
                GV.ProductionDate = poRow["productionDate"]?.ToString() ?? "";
                GV.ProductName = poRow["productName"]?.ToString() ?? "";
                GV.OrderQty = Convert.ToInt32(poRow["orderQty"] ?? 0);
                GV.CurrentUser = userName;
                GV.TotalCount = 0;
                GV.PassCount = 0;
                GV.FailCount = 0;
                GV.DuplicateCount = 0;
                GV.CartonCount = 0;
                GV.CartonClosedCount = 0;
                GV.CurrentCartonId = 0;
                GV.CurrentCartonCode = "";
                GV.ItemsInCurrentCarton = 0;

                // 3. Clear dictionaries
                GV.Dictionary_Code_Data.Clear();
                GV.Dictionary_Code_Package_Data.Clear();
                GV.Dictionary_Carton_Data.Clear();

                Log($"[PO] Thông tin: GTIN={GV.Gtin}, Qty={GV.OrderQty}");

                // 4. Load codes từ DataPool vào PO database
                Log("[PO] Đang nạp codes từ DataPool...");
                var loadResult = po.POLoader.LoadCodesFromDataPool(GV.OrderNo, GV.Gtin);
                if (!loadResult.success)
                {
                    Log($"[PO] Cảnh báo: {loadResult.message}");
                    // Tiếp tục nếu đã có codes trong PO
                }
                else
                {
                    Log($"[PO] {loadResult.message}");
                }

                // 5. Tạo PO databases (UniqueCodes, Record_Active, Record_Packing, Carton)
                Log("[PO] Khởi tạo databases...");
                var initResult = po.POCreator.InitPO(GV.OrderNo);
                if (!initResult.IsSuccess)
                {
                    Log($"[PO] Lỗi khởi tạo: {initResult.Message}");
                    return (false, initResult.Message);
                }

                // 6. Tính số thùng cần thiết
                int cartonCount = (GV.OrderQty + GV.CartonCapacity - 1) / GV.CartonCapacity;
                GV.CartonCount = cartonCount;

                // 7. Tạo cartons
                Log($"[PO] Tạo {cartonCount} thùng...");
                var cartonResult = po.POCreator.CreateEmptyCartons(GV.OrderNo, cartonCount);
                if (!cartonResult.success)
                {
                    Log($"[PO] Cảnh báo tạo thùng: {cartonResult.message}");
                }

                // 8. Load codes vào Dictionary
                Log("[PO] Load codes vao Dictionary...");
                var dictResult = po.CodeDictionaryLoader.LoadAllCodesToDictionary(GV.OrderNo, GV.Dictionary_Code_Data);
                if (!dictResult.success)
                {
                    Log($"[PO] Lỗi load Dictionary: {dictResult.message}");
                    return (false, dictResult.message);
                }
                Log($"[PO] Đã load {dictResult.count} codes vào Dictionary.");

                // Copy sang Dictionary cho Camera Package
                foreach (var kvp in GV.Dictionary_Code_Data)
                {
                    GV.Dictionary_Code_Package_Data[kvp.Key] = kvp.Value;
                }

                // 9. Load cartons vào Dictionary
                Log("[PO] Load cartons vào Dictionary...");
                var cartonDictResult = po.CartonDictionaryLoader.LoadAllCartonsToDictionary(GV.OrderNo, GV.Dictionary_Carton_Data);
                if (!cartonDictResult.success)
                {
                    Log($"[PO] Cảnh báo load cartons: {cartonDictResult.message}");
                }

                // 10. Tính toán carton ban đầu
                CalculateInitialCarton();

                // 11. Ghi lịch sử bắt đầu
                po.POHistoryManager.RecordStart(GV.OrderNo, GV.ProductionDate, GV.CurrentUser);

                Log($"[PO] Khởi tạo hoàn tất. Bắt đầu sản xuất...");
                SetState(e_ProductionState.Running);

                return (true, "Khởi tạo PO thành công.");
            }
            catch (Exception ex)
            {
                Log($"[PO] Lỗi: {ex.Message}");
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Tính toán carton ban đầu dựa trên số codes đã activate
        /// </summary>
        private static void CalculateInitialCarton()
        {
            int totalActivated = 0;
            foreach (var kvp in GV.Dictionary_Code_Data)
            {
                if (kvp.Value.Status == 1) // Đã activate
                    totalActivated++;
            }

            GV.PassCount = totalActivated;

            if (totalActivated == 0)
            {
                GV.CurrentCartonId = 1;
                GV.CurrentCartonCode = "";
                GV.ItemsInCurrentCarton = 0;
            }
            else
            {
                // Tính số thùng đã hoàn thành và thùng hiện tại
                int fullCartons = totalActivated / GV.CartonCapacity;
                int remainder = totalActivated % GV.CartonCapacity;

                GV.CartonClosedCount = fullCartons;
                GV.CurrentCartonId = fullCartons + 1;
                GV.ItemsInCurrentCarton = remainder;

                // Tìm carton code hiện tại
                if (GV.Dictionary_Carton_Data.TryGetValue(GV.CurrentCartonId, out var carton))
                {
                    GV.CurrentCartonCode = carton.CartonCode;
                }
            }
        }

        // ===== Camera Active Processing =====
        /// <summary>
        /// Xử lý mã từ Camera Active
        /// </summary>
        public static (bool success, e_Production_Status status, string message) ProcessCameraActiveCode(string code)
        {
            if (GV.AppState != e_ProductionState.Running)
            {
                return (false, e_Production_Status.Error, "Sản xuất chưa chạy.");
            }

            if (string.IsNullOrWhiteSpace(code) || code == "FAIL")
            {
                GV.FailCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.ReadFail, "Không đọc được mã.");
            }

            // Lookup trong Dictionary
            if (!GV.Dictionary_Code_Data.TryGetValue(code, out var codeInfo))
            {
                GV.FailCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.Error, "Mã không tồn tại trong PO.");
            }

            // Kiểm tra đã activate chưa
            if (codeInfo.Status == 1)
            {
                GV.DuplicateCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.Duplicate, "Mã đã được sử dụng.");
            }

            // Activate thành công
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Cập nhật Dictionary
            codeInfo.Status = 1;
            codeInfo.ActivateDate = now;
            codeInfo.ActivateUser = GV.CurrentUser;
            codeInfo.ProductionDate = GV.ProductionDate;

            // Cập nhật Database (async)
            Task.Run(() =>
            {
                po.CodeDictionaryLoader.ActivateCodeInDatabase(GV.OrderNo, code, now, GV.CurrentUser, GV.ProductionDate);
                po.POActivator.Record(GV.OrderNo, new po.RecordData
                {
                    Code = code,
                    Status = "1",
                    PLCStatus = "PASS",
                    ActivateDate = now,
                    ActivateUser = GV.CurrentUser,
                    ProductionDate = GV.ProductionDate
                });
            });

            GV.PassCount++;
            GV.TotalCount++;
            OnCountersUpdated?.Invoke();

            Log($"[Active] OK: {code} | Tổng: {GV.TotalCount} | OK: {GV.PassCount}");

            return (true, e_Production_Status.Pass, "OK");
        }

        // ===== Camera Package Processing =====
        /// <summary>
        /// Xử lý mã từ Camera Package
        /// </summary>
        public static (bool success, e_Production_Status status, string message) ProcessCameraPackageCode(string code)
        {
            if (GV.AppState != e_ProductionState.Running)
            {
                return (false, e_Production_Status.Error, "Sản xuất chưa chạy.");
            }

            if (string.IsNullOrWhiteSpace(code) || code == "FAIL")
            {
                GV.FailCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.ReadFail, "Không đọc được mã.");
            }

            // Lookup trong Dictionary
            if (!GV.Dictionary_Code_Package_Data.TryGetValue(code, out var codeInfo))
            {
                GV.FailCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.Error, "Mã không tồn tại trong PO.");
            }

            // Kiểm tra đã activate chưa
            if (codeInfo.Status != 1)
            {
                GV.FailCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.Fail, "Mã chưa được activate.");
            }

            // Kiểm tra đã pack chưa
            if (codeInfo.IsPacked)
            {
                GV.DuplicateCount++;
                GV.TotalCount++;
                OnCountersUpdated?.Invoke();
                return (false, e_Production_Status.Duplicate, "Mã đã được đóng gói.");
            }

            // Kiểm tra thùng hiện tại
            if (!ValidateCurrentCarton())
            {
                return (false, e_Production_Status.Error, "Chưa có mã thùng.");
            }

            // Pack vào thùng
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Cập nhật Dictionary
            codeInfo.CartonCode = GV.CurrentCartonCode;
            codeInfo.PackingDate = now;
            codeInfo.IsPacked = true;

            // Cập nhật Database
            Task.Run(() =>
            {
                po.CodeDictionaryLoader.PackCodeInDatabase(GV.OrderNo, code, GV.CurrentCartonCode, now, GV.CurrentUser, GV.ProductionDate);
                po.POPacking.Record(GV.OrderNo, new po.RecordData
                {
                    Code = code,
                    CartonCode = GV.CurrentCartonCode,
                    Status = "1",
                    PLCStatus = "PASS",
                    PackingDate = now,
                    PackingUser = GV.CurrentUser,
                    ProductionDate = GV.ProductionDate
                });
            });

            GV.ItemsInCurrentCarton++;
            GV.TotalCount++;
            OnCountersUpdated?.Invoke();

            Log($"[Pack] OK: {code} -> Thùng {GV.CurrentCartonCode} ({GV.ItemsInCurrentCarton}/{GV.CartonCapacity})");

            // Kiểm tra thùng đầy
            if (GV.ItemsInCurrentCarton >= GV.CartonCapacity)
            {
                CompleteCurrentCarton();
            }

            // Kiểm tra hoàn thành PO
            CheckPOCompletion();

            return (true, e_Production_Status.Pass, "OK");
        }

        /// <summary>
        /// Kiểm tra thùng hiện tại có hợp lệ không
        /// </summary>
        private static bool ValidateCurrentCarton()
        {
            if (GV.CurrentCartonId <= 0)
            {
                Log("[Pack] Lỗi: Chưa có thùng.");
                return false;
            }

            if (GV.CurrentCartonCode == "0" || string.IsNullOrEmpty(GV.CurrentCartonCode))
            {
                Log("[Pack] Lỗi: Thùng chưa có mã.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hoàn thành thùng hiện tại và chuyển thùng mới
        /// </summary>
        private static void CompleteCurrentCarton()
        {
            if (GV.CurrentCartonId <= 0) return;

            Log($"[Carton] Thùng {GV.CurrentCartonCode} đầy! Hoàn thành...");

            // Update Database
            Task.Run(() =>
            {
                po.CartonDictionaryLoader.CompleteCartonInDatabase(GV.OrderNo, GV.CurrentCartonId, GV.CurrentUser);
            });

            GV.CartonClosedCount++;
            GV.CurrentCartonId++;
            GV.ItemsInCurrentCarton = 0;

            // Tạo hoặc lấy thùng mới
            if (GV.Dictionary_Carton_Data.TryGetValue(GV.CurrentCartonId, out var newCarton))
            {
                GV.CurrentCartonCode = newCarton.CartonCode;

                if (string.IsNullOrEmpty(GV.CurrentCartonCode) || GV.CurrentCartonCode == "0")
                {
                    // Thùng chưa có mã - cần quét mã thùng trước
                    Log($"[Carton] Cảnh báo: Thùng #{GV.CurrentCartonId} chưa có mã. Cần quét thùng.");
                }
            }
            else
            {
                GV.CurrentCartonCode = "";
                Log($"[Carton] Cảnh báo: Không tìm thấy thùng #{GV.CurrentCartonId}.");
            }

            OnCountersUpdated?.Invoke();
        }

        /// <summary>
        /// Kiểm tra hoàn thành PO
        /// </summary>
        private static void CheckPOCompletion()
        {
            int totalPacked = 0;
            foreach (var kvp in GV.Dictionary_Code_Data)
            {
                if (kvp.Value.IsPacked)
                    totalPacked++;
            }

            if (totalPacked >= GV.OrderQty)
            {
                Log($"[PO] Hoàn thành! Tổng: {totalPacked}/{GV.OrderQty}");
                StopProduction();
                po.POHistoryManager.RecordEnd(GV.OrderNo);
                SetState(e_ProductionState.Completed);
            }
        }

        // ===== Carton Management =====
        /// <summary>
        /// Bắt đầu thùng (khi có mã thùng)
        /// </summary>
        public static void StartCarton(string cartonCode)
        {
            if (GV.AppState != e_ProductionState.Running) return;

            // Tìm carton ID tương ứng
            foreach (var kvp in GV.Dictionary_Carton_Data)
            {
                if (kvp.Value.CartonCode == cartonCode)
                {
                    GV.CurrentCartonId = kvp.Key;
                    GV.CurrentCartonCode = cartonCode;

                    // Update Database
                    Task.Run(() =>
                    {
                        po.CartonDictionaryLoader.StartCartonInDatabase(GV.OrderNo, GV.CurrentCartonId, GV.CurrentUser);
                    });

                    Log($"[Carton] Bắt đầu thùng: {cartonCode}");
                    return;
                }
            }

            Log($"[Carton] Không tìm thấy thùng: {cartonCode}");
        }

        /// <summary>
        /// Start carton kế tiếp (auto)
        /// </summary>
        public static void StartNextCarton()
        {
            int nextId = GV.CurrentCartonId + 1;
            if (GV.Dictionary_Carton_Data.TryGetValue(nextId, out var nextCarton))
            {
                GV.CurrentCartonId = nextId;
                GV.CurrentCartonCode = nextCarton.CartonCode;
                GV.ItemsInCurrentCarton = 0;

                Task.Run(() =>
                {
                    po.CartonDictionaryLoader.StartCartonInDatabase(GV.OrderNo, GV.CurrentCartonId, GV.CurrentUser);
                });

                Log($"[Carton] Chuyển thùng mới: #{GV.CurrentCartonId} - {GV.CurrentCartonCode}");
                OnCountersUpdated?.Invoke();
            }
        }

        // ===== Start/Stop Production =====
        public static void StartProduction()
        {
            if (GV.AppState == e_ProductionState.Running)
            {
                Log("[System] Đã đang chạy.");
                return;
            }

            if (!GV.HasPO)
            {
                Log("[System] Chưa chọn PO.");
                return;
            }

            SetState(e_ProductionState.Running);
            Log("[System] Bắt đầu sản xuất.");
        }

        public static void StopProduction()
        {
            SetState(e_ProductionState.Idle);
            Log("[System] Dừng sản xuất.");
        }

        // ===== Reset =====
        public static void ResetPO()
        {
            // Stop nếu đang chạy
            StopProduction();

            // Clear
            GV.OrderNo = "";
            GV.Gtin = "";
            GV.ProductionDate = "";
            GV.ProductName = "";
            GV.OrderQty = 0;
            GV.TotalCount = 0;
            GV.PassCount = 0;
            GV.FailCount = 0;
            GV.DuplicateCount = 0;
            GV.CartonCount = 0;
            GV.CartonClosedCount = 0;
            GV.CurrentCartonId = 0;
            GV.CurrentCartonCode = "";
            GV.ItemsInCurrentCarton = 0;

            GV.Dictionary_Code_Data.Clear();
            GV.Dictionary_Code_Package_Data.Clear();
            GV.Dictionary_Carton_Data.Clear();

            SetState(e_ProductionState.NoSelectedPO);
            OnCountersUpdated?.Invoke();
            Log("[System] Reset PO hoàn tất.");
        }

        // ===== Helper =====
        private static void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        // ===== Stats =====
        public static int GetRemainingCodes()
        {
            int unused = 0;
            foreach (var kvp in GV.Dictionary_Code_Data)
            {
                if (kvp.Value.Status == 0)
                    unused++;
            }
            return unused;
        }

        public static int GetPackedCodes()
        {
            int packed = 0;
            foreach (var kvp in GV.Dictionary_Code_Data)
            {
                if (kvp.Value.IsPacked)
                    packed++;
            }
            return packed;
        }
    }
}
