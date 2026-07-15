// Pipeline matrix verifier — chạy lại logic PLC lane mapping, payload parsing
// và busy-gate classification giống GProject/Production/ProductionStateMachine.cs
// để smoke-test các nhánh quan trọng mà KHÔNG cần PLC thật hay SQLite.
//
// Usage: dotnet run --project PipelineMatrix

using System.Globalization;

internal static class Program
{
    private enum ProductionStatus { Pass, Fail, Duplicate, NotFound, FormatError, Error, ReadFail, Timeout }

    // Mirror of GProject/Production/ProductionStateMachine.cs::MapResultForPLC
    private static short MapResultForPLC(ProductionStatus status, int cartonId)
        => status switch
        {
            ProductionStatus.Pass => (short)(cartonId % 2 == 1 ? 1 : 2),
            _ => 0,
        };

    // Mirror of HandleCodeFromCamera payload parse + early-return classification
    private static (ProductionStatus status, string code) ClassifyPayload(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (ProductionStatus.ReadFail, "");

        var parts = raw!.Split('|');
        var code = parts[0] ?? "";
        var status = parts.Length > 1 ? (parts[1] ?? "") : "NO_READ";

        if (parts.Length != 2 || string.IsNullOrEmpty(code))
            return (ProductionStatus.FormatError, code);

        if (!string.Equals(status, "OK", StringComparison.Ordinal))
        {
            if (string.Equals(status, "REJECT", StringComparison.OrdinalIgnoreCase))
                return (ProductionStatus.FormatError, code);
            if (string.Equals(status, "NO_READ", StringComparison.OrdinalIgnoreCase))
                return (ProductionStatus.ReadFail, code);
            return (ProductionStatus.Error, code);
        }

        // status = OK — chưa lookup dictionary ở đây, caller sẽ quyết định Pass/NotFound/Duplicate
        return (ProductionStatus.Pass, code);
    }

    private static int _failed;

    private static void Expect<T>(string name, T expected, T actual)
    {
        var pass = EqualityComparer<T>.Default.Equals(expected, actual);
        Console.WriteLine($"  [{(pass ? "PASS" : "FAIL")}] {name}: expected={expected}, actual={actual}");
        if (!pass) _failed++;
    }

    private static int Main()
    {
        Console.WriteLine("=== Pipeline Matrix Verification ===\n");

        Console.WriteLine("[1] MapResultForPLC (PLC lane mapping)");
        Expect("Pass carton#1 → lane 1", (short)1, MapResultForPLC(ProductionStatus.Pass, 1));
        Expect("Pass carton#3 → lane 1", (short)1, MapResultForPLC(ProductionStatus.Pass, 3));
        Expect("Pass carton#2 → lane 2", (short)2, MapResultForPLC(ProductionStatus.Pass, 2));
        Expect("Pass carton#4 → lane 2", (short)2, MapResultForPLC(ProductionStatus.Pass, 4));
        Expect("Pass carton#100 (even) → lane 2", (short)2, MapResultForPLC(ProductionStatus.Pass, 100));
        Expect("Pass carton#999 (odd)  → lane 1", (short)1, MapResultForPLC(ProductionStatus.Pass, 999));

        // Mọi non-Pass đều map 0
        foreach (var s in new[] {
            ProductionStatus.Fail, ProductionStatus.Duplicate, ProductionStatus.NotFound,
            ProductionStatus.FormatError, ProductionStatus.Error, ProductionStatus.ReadFail,
            ProductionStatus.Timeout })
        {
            Expect($"non-Pass ({s}) → 0", (short)0, MapResultForPLC(s, 1));
            Expect($"non-Pass ({s}) → 0 (even carton)", (short)0, MapResultForPLC(s, 2));
        }

        Console.WriteLine("\n[2] Payload classification (early-return branches)");
        Expect("null payload  → ReadFail", ProductionStatus.ReadFail, ClassifyPayload(null).status);
        Expect("empty payload → ReadFail", ProductionStatus.ReadFail, ClassifyPayload("").status);
        Expect("whitespace    → ReadFail", ProductionStatus.ReadFail, ClassifyPayload("   ").status);

        Expect("no delimiter   → FormatError", ProductionStatus.FormatError, ClassifyPayload("ABC123").status);
        Expect("empty code|OK  → FormatError", ProductionStatus.FormatError, ClassifyPayload("|OK").status);
        Expect("3 parts        → FormatError", ProductionStatus.FormatError, ClassifyPayload("A|B|C").status);

        Expect("REJECT         → FormatError", ProductionStatus.FormatError, ClassifyPayload("ABC123|REJECT").status);
        Expect("NO_READ        → ReadFail", ProductionStatus.ReadFail, ClassifyPayload("ABC123|NO_READ").status);
        Expect("unknown status → Error", ProductionStatus.Error, ClassifyPayload("ABC123|WEIRD").status);
        Expect("OK             → Pass (Pass branch)", ProductionStatus.Pass, ClassifyPayload("ABC123|OK").status);

        Console.WriteLine("\n[3] Concurrent gating (atomic SemaphoreSlim pattern)");
        // Mô phỏng gate: chỉ một payload chạy tại 1 thời điểm, payload thứ 2 thấy busy
        var gate = new SemaphoreSlim(1, 1);
        var processedOrder = new List<int>();
        object listLock = new();

        async Task<int> TryProcess(int id, int holdMs)
        {
            if (!await gate.WaitAsync(0))
            {
                Console.WriteLine($"  [INFO] payload #{id} REJECTED (busy)");
                return -1; // busy
            }
            try
            {
                await Task.Delay(holdMs);
                lock (listLock) processedOrder.Add(id);
                return id;
            }
            finally
            {
                gate.Release();
            }
        }

        // Chạy 5 payload "đồng thời", chỉ một được vào; phần còn lại busy -> reject
        var tasks = new[]
        {
            TryProcess(1, 80),
            TryProcess(2, 0),
            TryProcess(3, 0),
            TryProcess(4, 0),
            TryProcess(5, 0),
        };
        Task.WhenAll(tasks).Wait();

        Expect("only one payload processed during gate lock", 1, processedOrder.Count);
        Expect("processed order = #1 (first one wins)", 1, processedOrder[0]);

        Console.WriteLine("\n=== Result ===");
        if (_failed == 0)
        {
            Console.WriteLine("All matrix checks PASSED.");
            return 0;
        }
        Console.WriteLine($"{_failed} check(s) FAILED.");
        return 1;
    }
}