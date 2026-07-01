using System;
using System.Collections.Generic;
using TTManager.Omron;

namespace VNQR.Infrastructure
{
    /// <summary>
    /// Globals - Biến toàn cục cho ứng dụng VNQR
    /// Dùng: GV.OrderNo, GV.AppState, GV.CameraState_Active, etc.
    /// </summary>
    public static class GV
    {
        // ===== App Info =====
        public static string AppName => "VNQR";

        // ===== App State =====
        public static e_ProductionState AppState { get; set; } = e_ProductionState.Checking;

        // ===== Camera States =====
        public static eOmronCameraState CameraState_Active { get; set; } = eOmronCameraState.Disconnected;
        public static eOmronCameraState CameraState_Package { get; set; } = eOmronCameraState.Disconnected;

        // ===== Current User =====
        public static string CurrentUser { get; set; } = "System";

        // ===== Current PO (Production Order) =====
        public static string OrderNo { get; set; } = string.Empty;
        public static string Gtin { get; set; } = string.Empty;
        public static string ProductionDate { get; set; } = string.Empty;
        public static string ProductName { get; set; } = string.Empty;
        public static int OrderQty { get; set; } = 0;
        public static int CartonCapacity { get; set; } = 24;

        // ===== Production Counters =====
        public static int TotalCount { get; set; } = 0;
        public static int PassCount { get; set; } = 0;
        public static int FailCount { get; set; } = 0;
        public static int DuplicateCount { get; set; } = 0;
        public static int CartonCount { get; set; } = 0;
        public static int CartonClosedCount { get; set; } = 0;

        // ===== Current Carton =====
        public static int CurrentCartonId { get; set; } = 0;
        public static string CurrentCartonCode { get; set; } = string.Empty;
        public static int ItemsInCurrentCarton { get; set; } = 0;

        // ===== Dictionaries for fast lookup (like MASAN) =====
        public static Dictionary<string, CodeInfo> Dictionary_Code_Data { get; set; } = new();
        public static Dictionary<string, CodeInfo> Dictionary_Code_Package_Data { get; set; } = new();
        public static Dictionary<int, CartonInfo> Dictionary_Carton_Data { get; set; } = new();

        // ===== Convenience Methods =====
        public static void Reset()
        {
            AppState = e_ProductionState.Checking;
            OrderNo = string.Empty;
            Gtin = string.Empty;
            ProductionDate = string.Empty;
            ProductName = string.Empty;
            OrderQty = 0;
            TotalCount = 0;
            PassCount = 0;
            FailCount = 0;
            DuplicateCount = 0;
            CartonCount = 0;
            CartonClosedCount = 0;
            CurrentCartonId = 0;
            CurrentCartonCode = string.Empty;
            ItemsInCurrentCarton = 0;
            Dictionary_Code_Data.Clear();
            Dictionary_Code_Package_Data.Clear();
            Dictionary_Carton_Data.Clear();
        }

        public static bool HasPO => !string.IsNullOrEmpty(OrderNo);
        public static double ProgressPercent => OrderQty > 0 ? (double)PassCount / OrderQty * 100 : 0;

        // ===== Code Info (for Dictionary lookup) =====
        public class CodeInfo
        {
            public string Code { get; set; } = "";
            public string OrderNo { get; set; } = "";
            public int Status { get; set; } = 0; // 0 = unused, 1 = activated (used at camera active)
            public string CartonCode { get; set; } = "0";
            public string ActivateDate { get; set; } = "0";
            public string ProductionDate { get; set; } = "0";
            public string ActivateUser { get; set; } = "";
            public string PackingDate { get; set; } = "0";
            public bool IsPacked { get; set; } = false;
        }

        // ===== Carton Info (for Dictionary lookup) =====
        public class CartonInfo
        {
            public int Id { get; set; } = 0;
            public string CartonCode { get; set; } = "0";
            public string StartDatetime { get; set; } = "0";
            public string CompletedDatetime { get; set; } = "0";
            public string ActivateUser { get; set; } = "";
            public string ProductionDate { get; set; } = "0";
            public bool IsStarted => StartDatetime != "0";
            public bool IsCompleted => CompletedDatetime != "0";
        }
    }

    public enum e_ProductionState
    {
        Checking,
        NoSelectedPO,
        Start,
        Loading,
        Ready,
        Running,
        Saving,
        Checking_Queue,
        Completed,
        Error,
        Idle,
        Pause,
        Waiting_Stop,
        ThieuSanPham
    }

    public enum e_CodeStatus
    {
        Unused = 0,
        Used = 1
    }

    public enum e_CartonStatus
    {
        Open = 0,
        Closed = 1,
        Cancelled = -1
    }

    public enum e_Production_Status
    {
        Pass,
        Fail,
        Duplicate,
        ReadFail,
        Error
    }
}
