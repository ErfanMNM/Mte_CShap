using System.Collections.Concurrent;
using TSo.Models;
using TSo.Configs;

namespace TSo;

public static class GlobalState
{
    public static readonly AppConfigs Config = new();

    public static readonly ConcurrentHashSet<string> ActiveSet = new();
    public static readonly ConcurrentQueue<QRProductRecord> QueueRecord = new();
    public static readonly ConcurrentQueue<QRProductRecord> QueueActive = new();
    public static readonly ConcurrentDictionary<string, SessionInfo> Sessions = new();

    public static readonly ConcurrentQueue<ActivityLog> ActivityLogs = new();

    public static BatchInfo CurrentBatch { get; set; } = new() { BatchCode = "NNN", Barcode = "000" };
    public static ProductionCounters CameraCounters { get; set; } = new();
    public static PLCCounters PLCCounters { get; set; } = new();
    public static DeviceStatus DeviceStatus { get; set; } = new();
    public static SystemStatus SystemStatus { get; set; } = new() { State = "Initializing" };
    public static int AlarmCount { get; set; } = 0;

    public static bool IsAuthenticated { get; set; } = false;
    public static SessionInfo? CurrentSession { get; set; }

    public static int ProductionPerHour { get; set; } = 0;
    public static long LastProductTimestampMs { get; set; } = 0;
    public static readonly List<long> ProductTimestampSamples = new();

    public static int ActiveCodesTotal => ActiveSet.Count;
}

public class ConcurrentHashSet<T>
{
    private readonly ConcurrentDictionary<T, byte> _dict = new();

    public int Count => _dict.Count;

    public void Add(T item) => _dict.TryAdd(item, 0);
    public bool Contains(T item) => _dict.ContainsKey(item);
    public bool Remove(T item) => _dict.TryRemove(item, out _);
    public void Clear() => _dict.Clear();

    public HashSet<T> ToHashSet() => new(_dict.Keys);
}
