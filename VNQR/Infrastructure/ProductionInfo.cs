using System;
using System.Text.Json;
using TTManager.Omron;
using VNQR.Helpers;
using VNQR.Infrastructure;

namespace VNQR.Infrastructure
{
    /// <summary>
    /// Singleton in-memory chứa dữ liệu PO đang chạy.
    /// Thay thế việc truy vấn DB nhiều lần bằng counter trong RAM.
    /// Tương tự Globals.ProductionData ở MASA-SERIALIZATION.
    /// </summary>
    public sealed class ProductionInfo
    {
        #region Singleton Instance

        public static ProductionInfo? Current { get; private set; }

        #endregion

        #region PO Metadata

        public string OrderNo { get; private set; } = "";
        public string Gtin { get; private set; } = "";
        public string ProductionDate { get; private set; } = "";
        public string UserName { get; private set; } = "";
        public string Shift { get; private set; } = "";
        public string ProductionLine { get; private set; } = "";
        public string Site { get; private set; } = "";
        public string Factory { get; private set; } = "";
        public string ProductName { get; private set; } = "";
        public string ProductCode { get; private set; } = "";
        public int OrderQty { get; private set; } = 0;
        public string LotNumber { get; private set; } = "";
        public string Uom { get; private set; } = "PCS";
        public DateTime StartTime { get; private set; } = DateTime.Now;

        #endregion

        #region In-Memory Counters

        public int ActiveCount { get; private set; }
        public int PackedCount { get; private set; }
        public int PassCount { get; private set; }
        public int FailCount { get; private set; }
        public int DuplicateCount { get; private set; }

        public int RemainCount => OrderQty - ActiveCount;
        public int ActivePercent => OrderQty > 0 ? (int)Math.Round(ActiveCount * 100.0 / OrderQty) : 0;
        public int PackedPercent => OrderQty > 0 ? (int)Math.Round(PackedCount * 100.0 / OrderQty) : 0;

        #endregion

        #region Current Carton Tracking

        public int CurrentCartonId { get; private set; }
        public string CurrentCartonCode { get; private set; } = "";
        public int CartonCapacity { get; private set; }
        public int ItemsInCurrentCarton { get; private set; }

        #endregion

        #region Last Event Tracking

        public string LastActivatedCode { get; private set; } = "";
        public DateTime LastActivatedTime { get; private set; }
        public string LastPackedCode { get; private set; } = "";
        public DateTime LastPackedTime { get; private set; }

        #endregion

        #region WebSocket Broadcast Support

        public event Action? OnUpdated;

        #endregion

        #region Lifecycle Methods

        private ProductionInfo() { }

        /// <summary>
        /// Load PO vào RAM từ POInfo. Gọi khi bắt đầu/resume PO.
        /// </summary>
        public static void Load(po.POInfo info, string userName = "")
        {
            LoadFromPOInfo(info);
            if (!string.IsNullOrEmpty(userName))
                Current!.UserName = userName;

            Current!.StartTime = DateTime.Now;
            GV.AppState = e_ProductionState.Running;

            Current.OnUpdated?.Invoke();
        }

        /// <summary>
        /// Load từ một dòng DataRow (ví dụ khi resume từ POHistory).
        /// </summary>
        public static void LoadFromDataRow(System.Data.DataRow row, string userName = "")
        {
            Current = new ProductionInfo
            {
                OrderNo = row["PO"]?.ToString() ?? "",
                ProductionDate = row["ProductionDate"]?.ToString() ?? "",
                UserName = string.IsNullOrEmpty(userName) ? row["UserName"]?.ToString() ?? "" : userName,
                Gtin = row["gtin"]?.ToString() ?? "",
                ProductName = row["productName"]?.ToString() ?? "",
                ProductCode = row["productCode"]?.ToString() ?? "",
                OrderQty = Convert.ToInt32(row["orderQty"] ?? 0),
                Shift = row["shift"]?.ToString() ?? "",
                ProductionLine = row["productionLine"]?.ToString() ?? "",
                Site = row["site"]?.ToString() ?? "",
                Factory = row["factory"]?.ToString() ?? "",
                LotNumber = row["lotNumber"]?.ToString() ?? "",
                Uom = row["uom"]?.ToString() ?? "PCS",
                StartTime = DateTime.Now
            };

            GV.AppState = e_ProductionState.Running;

            Current.OnUpdated?.Invoke();
        }

        private static void LoadFromPOInfo(po.POInfo info)
        {
            Current = new ProductionInfo
            {
                OrderNo = info.OrderNo,
                Gtin = info.Gtin,
                ProductionDate = info.ProductionDate,
                UserName = info.CustomerOrderNo ?? "",
                Shift = info.Shift,
                ProductionLine = info.ProductionLine,
                Site = info.Site,
                Factory = info.Factory,
                ProductName = info.ProductName,
                ProductCode = info.ProductCode,
                OrderQty = info.OrderQty,
                LotNumber = info.LotNumber,
                Uom = info.Uom,
                StartTime = DateTime.Now
            };
        }

        /// <summary>
        /// Unload PO khỏi RAM. Gọi khi PO hoàn thành hoặc dừng.
        /// </summary>
        public static void Unload()
        {
            if (Current != null)
            {
                Current.OnUpdated = null;
                Current = null;
            }
            GV.AppState = e_ProductionState.Idle;
        }

        /// <summary>
        /// Reset toàn bộ counter về 0 (giữ nguyên metadata PO).
        /// </summary>
        public static void ResetCounters()
        {
            if (Current == null) return;
            Current.ActiveCount = 0;
            Current.PackedCount = 0;
            Current.PassCount = 0;
            Current.FailCount = 0;
            Current.DuplicateCount = 0;
            Current.CurrentCartonId = 0;
            Current.CurrentCartonCode = "";
            Current.ItemsInCurrentCarton = 0;
            Current.LastActivatedCode = "";
            Current.LastPackedCode = "";
        }

        #endregion

        #region Counter Increment Methods

        public static void IncrementActive(string code = "")
        {
            if (Current == null) return;
            Current.ActiveCount++;
            if (!string.IsNullOrEmpty(code))
            {
                Current.LastActivatedCode = code;
                Current.LastActivatedTime = DateTime.Now;
            }
            Current.OnUpdated?.Invoke();
        }

        public static void DecrementActive()
        {
            if (Current == null || Current.ActiveCount <= 0) return;
            Current.ActiveCount--;
            Current.OnUpdated?.Invoke();
        }

        public static void IncrementPacked(string code = "")
        {
            if (Current == null) return;
            Current.PackedCount++;
            if (!string.IsNullOrEmpty(code))
            {
                Current.LastPackedCode = code;
                Current.LastPackedTime = DateTime.Now;
            }
            Current.ItemsInCurrentCarton++;
            Current.OnUpdated?.Invoke();
        }

        public static void DecrementPacked()
        {
            if (Current == null || Current.PackedCount <= 0) return;
            Current.PackedCount--;
            Current.OnUpdated?.Invoke();
        }

        public static void IncrementPass()
        {
            if (Current == null) return;
            Current.PassCount++;
            Current.OnUpdated?.Invoke();
        }

        public static void IncrementFail()
        {
            if (Current == null) return;
            Current.FailCount++;
            Current.OnUpdated?.Invoke();
        }

        public static void IncrementDuplicate()
        {
            if (Current == null) return;
            Current.DuplicateCount++;
            Current.OnUpdated?.Invoke();
        }

        public static void SetCurrentCarton(int cartonId, string cartonCode, int capacity)
        {
            if (Current == null) return;
            Current.CurrentCartonId = cartonId;
            Current.CurrentCartonCode = cartonCode;
            Current.CartonCapacity = capacity;
            Current.ItemsInCurrentCarton = 0;
            Current.OnUpdated?.Invoke();
        }

        public static void CompleteCurrentCarton()
        {
            if (Current == null) return;
            Current.ItemsInCurrentCarton = Current.CartonCapacity;
            Current.OnUpdated?.Invoke();
        }

        #endregion

        #region JSON Snapshot

        /// <summary>
        /// Tạo JSON snapshot dùng cho WebSocket broadcast.
        /// </summary>
        public static string BroadcastSnapshot()
        {
            if (Current == null) return "";

            var snapshot = new
            {
                orderNo = Current.OrderNo,
                gtin = Current.Gtin,
                productionDate = Current.ProductionDate,
                userName = Current.UserName,
                shift = Current.Shift,
                productionLine = Current.ProductionLine,
                orderQty = Current.OrderQty,
                activeCount = Current.ActiveCount,
                packedCount = Current.PackedCount,
                passCount = Current.PassCount,
                failCount = Current.FailCount,
                duplicateCount = Current.DuplicateCount,
                remainCount = Current.RemainCount,
                activePercent = Current.ActivePercent,
                packedPercent = Current.PackedPercent,
                currentCarton = new
                {
                    id = Current.CurrentCartonId,
                    code = Current.CurrentCartonCode,
                    capacity = Current.CartonCapacity,
                    items = Current.ItemsInCurrentCarton
                },
                lastActivatedCode = Current.LastActivatedCode,
                lastPackedCode = Current.LastPackedCode,
                appState = GV.AppState.ToString(),
                timestamp = DateTime.Now
            };

            return JsonSerializer.Serialize(snapshot);
        }

        #endregion
    }
}
