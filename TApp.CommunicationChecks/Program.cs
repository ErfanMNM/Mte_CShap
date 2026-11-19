using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Models;
using TApp.Views.Settings;
using TApp.Views.Dashboard;
using TTManager.Datalogic;

internal static class Program
{
    private const string ValidCode = "TEST-BARCODE-00000001";
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);

    [STAThread]
    private static int Main()
    {
        // Không chạy MainForm/Start, không đọc cấu hình thật hoặc mở kết nối nhà máy.
        WindowsFormsSynchronizationContext.AutoInstall = false;
        SynchronizationContext.SetSynchronizationContext(null);
        Console.OutputEncoding = Encoding.UTF8;

        (string Name, Action Check)[] checks =
        [
            ("Pass/Fail gửi đúng MP5/MP6, không CR/LF", CheckExactCommands),
            ("Lệnh CC320 tùy ý gửi nguyên văn và từ chối lệnh rỗng", CheckCustomCC320Command),
            ("Chưa kết nối trả false", CheckDisconnectedSend),
            ("Gửi Pass thành công mới kích hoạt mã", CheckPassActivation),
            ("Gửi Pass lỗi lưu Error và không kích hoạt mã", CheckFailedPass),
            ("Mọi loại Fail đều gửi MP6", CheckFailureStatuses),
            ("Sản phẩm bận gửi Timeout sau sản phẩm trước", CheckBusyOrdering),
            ("Không mất bản tin lúc worker vừa xử lý xong", CheckWorkerCompletionRace),
            ("Không gửi lệnh đã chờ khóa sang phiên kết nối mới", CheckConnectionVersionAfterWait),
            ("Không gửi kết quả camera cũ sau kết nối lại", CheckQueuedResultAfterReconnect),
            ("Tắt CC320 vẫn ghi PLC qua UDP", CheckPlcRouting),
            ("Thời gian camera đến CC320 bao gồm chờ hàng đợi", CheckTimingIncludesQueueWait),
            ("Listbox giữ số đo riêng cho từng code", CheckTimingSnapshotsOnListbox),
            ("Gửi lỗi xóa số đo Global và không gắn thời gian thành công", CheckFailedSendTiming),
            ("Gửi đồng thời dữ liệu lớn không thiếu hoặc xen byte", CheckConcurrentTransport),
            ("Recipe PLC tương thích file cũ và lưu danh sách D", CheckPlcSettingsPersistence),
            ("Trang PLC chỉ còn tab đầu với bảng tham số mới", CheckPlcSettingsPage)
        ];

        int failed = 0;
        foreach (var (name, check) in checks)
        {
            ResetProduction();
            try
            {
                check();
                Console.WriteLine($"ĐẠT: {name}");
            }
            catch (Exception ex)
            {
                failed++;
                Console.Error.WriteLine($"LỖI: {name}\n{ex}");
            }
        }

        Console.WriteLine($"Kết quả: {checks.Length - failed}/{checks.Length} kiểm tra đạt.");
        return failed == 0 ? 0 : 1;
    }

    private static void ResetProduction()
    {
        AppConfigs.Current.CC320_Enabled = true;
        AppConfigs.Current.Data_Mode = "normal";
        AppConfigs.Current.Production_Speed_Mode = 0;
        AppConfigs.Current.Old_Camera_job_Enabled = false;
        GlobalVarialbles.Logger = null;
        GlobalVarialbles.CameraToCC320ElapsedMs = -1;
        GlobalVarialbles.CurrentUser.Username = "CC320-check";
        FD_Globals.productionData = new ProductionData { Barcode = "TEST-BARCODE", BatchCode = "CHECK" };
        FD_Globals.ActiveSet = new HashSet<string>();
        FD_Globals.QueueActive = new ConcurrentQueue<QRProductRecord>();
        FD_Globals.QueueRecord = new ConcurrentQueue<QRProductRecord>();
    }

    private static void CheckExactCommands()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        Assert(dashboard.Send_Result_To_PLC(e_PLC_Result.Pass), "Không gửi được Pass.");
        Assert(dashboard.Send_Result_To_PLC(e_PLC_Result.Fail), "Không gửi được Fail.");
        Assert(dashboard.Send_Result_To_PLC(e_PLC_Result.Pass), "Không gửi được Pass thứ hai.");
        AssertBytes(connection.ReadExactly(9), "MP5MP6MP5");
        connection.AssertNoExtraBytes();
    }

    private static void CheckCustomCC320Command()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);

        var empty = dashboard.SendCustomCC320CommandAsync("   ").WaitAsync(CheckTimeout).GetAwaiter().GetResult();
        Assert(!empty.Success, "Lệnh rỗng phải bị từ chối.");
        Assert(!string.IsNullOrWhiteSpace(empty.Message), "Lệnh rỗng phải trả về thông báo lý do.");
        connection.AssertNoExtraBytes();

        var sent = dashboard.SendCustomCC320CommandAsync("RR2,500").WaitAsync(CheckTimeout).GetAwaiter().GetResult();
        Assert(sent.Success, "Lệnh tùy ý phải gửi được khi CC320 đã kết nối.");
        AssertBytes(connection.ReadExactly(7), "RR2,500");
        connection.AssertNoExtraBytes();

        using var disconnected = CreateDashboard(new DatalogicCamera("127.0.0.1", 1));
        var failed = disconnected.SendCustomCC320CommandAsync("MP5").WaitAsync(CheckTimeout).GetAwaiter().GetResult();
        Assert(!failed.Success, "Chưa kết nối phải trả về thất bại.");
        Assert(failed.Message.Contains("CC320"), "Thông báo lỗi phải nêu rõ CC320.");
    }

    private static void CheckDisconnectedSend()
    {
        using var dashboard = CreateDashboard(new DatalogicCamera("127.0.0.1", 1));
        Assert(!dashboard.Send_Result_To_PLC(e_PLC_Result.Pass), "Chưa kết nối phải trả false khi gửi Pass.");
        Assert(!dashboard.Send_Result_To_PLC(e_PLC_Result.Fail), "Chưa kết nối phải trả false khi gửi Fail.");
    }

    private static void CheckPassActivation()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        InvokePrivate(dashboard, "Camera_ProcessData", ValidCode);
        AssertBytes(connection.ReadExactly(3), "MP5");
        Assert(FD_Globals.ActiveSet.SetEquals([ValidCode]), "Mã Pass phải có trong ActiveSet.");
        Assert(FD_Globals.QueueActive.Count == 1, "Mã Pass phải được đưa vào QueueActive một lần.");
        Assert(FD_Globals.QueueRecord.Single().Status == e_Production_Status.Pass, "Phải lưu trạng thái Pass.");
        Assert(FD_Globals.productionData.productCameraCounter.Pass == 1, "Bộ đếm Pass phải tăng một.");
        connection.AssertNoExtraBytes();
    }

    private static void CheckFailedPass()
    {
        using var dashboard = CreateDashboard(new DatalogicCamera("127.0.0.1", 1));
        InvokePrivate(dashboard, "Camera_ProcessData", ValidCode);
        Assert(FD_Globals.ActiveSet.Count == 0, "Gửi lỗi không được đánh dấu mã đã kích hoạt.");
        Assert(FD_Globals.QueueActive.IsEmpty, "Gửi lỗi không được enqueue QueueActive.");
        var record = FD_Globals.QueueRecord.Single();
        Assert(record.Status == e_Production_Status.Error, "Gửi lỗi phải lưu Error thay vì Pass.");
        Assert(record.Reason.Contains("CC320") && record.Reason.Contains("Pass"), "Bản ghi cần lý do lỗi gửi.");
        Assert(FD_Globals.productionData.productCameraCounter.Pass == 0, "Gửi lỗi không tăng Pass.");
        Assert(FD_Globals.productionData.productCameraCounter.Error == 1, "Gửi lỗi phải tăng Error.");
    }

    private static void CheckFailureStatuses()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        e_Production_Status[] statuses =
        [
            e_Production_Status.ReadFail, e_Production_Status.Duplicate,
            e_Production_Status.FormatError, e_Production_Status.Timeout
        ];
        foreach (var status in statuses)
        {
            Assert((bool)InvokePrivate(dashboard, "SendInspectionResult", status, "CHECK")!, $"Không gửi được {status}.");
        }
        AssertBytes(connection.ReadExactly(3 * statuses.Length), string.Concat(statuses.Select(_ => "MP6")));
        Assert(FD_Globals.QueueRecord.Select(r => r.Status).SequenceEqual(statuses), "Phải giữ nguyên phân loại lỗi kiểm tra QR.");
        connection.AssertNoExtraBytes();
    }

    private static void CheckBusyOrdering()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        // Khóa chỉ để giữ worker chưa dequeue A trong khi callback B đến.
        lock (GetField<object>(dashboard, "_cameraQueueLock"))
        {
            InvokePrivate(dashboard, "HandleCameraDataReceived", ValidCode);
            InvokePrivate(dashboard, "HandleCameraDataReceived", "NEXT-PRODUCT");
        }
        AssertBytes(connection.ReadExactly(6), "MP5MP6");
        WaitForWorker(dashboard, 2);
        Assert(FD_Globals.QueueRecord.Select(r => r.Status).SequenceEqual([e_Production_Status.Pass, e_Production_Status.Timeout]),
            "Kết quả phải theo thứ tự A Pass, B Timeout.");
        connection.AssertNoExtraBytes();
    }

    private static void CheckWorkerCompletionRace()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        using var drained = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var worker = GetField<BackgroundWorker>(dashboard, "WK_Camera");
        int invocations = 0;
        DoWorkEventHandler holdBeforeCompletion = (_, _) =>
        {
            if (Interlocked.Increment(ref invocations) != 1) return;
            drained.Set();
            Assert(release.Wait(CheckTimeout), "Không nhận được tín hiệu hoàn tất worker.");
        };
        worker.DoWork += holdBeforeCompletion;
        try
        {
            InvokePrivate(dashboard, "HandleCameraDataReceived", ValidCode);
            Assert(drained.Wait(CheckTimeout), "Worker chưa drain hàng đợi.");
            // DoWork chính đã hết queue nhưng IsBusy vẫn true, mô phỏng khe hoàn tất.
            InvokePrivate(dashboard, "HandleCameraDataReceived", "LATE-PRODUCT");
            release.Set();
            AssertBytes(connection.ReadExactly(6), "MP5MP6");
            WaitForWorker(dashboard, 2);
            Assert(FD_Globals.QueueRecord.Last().Status == e_Production_Status.Timeout, "Bản tin cuối phải được xử lý.");
            connection.AssertNoExtraBytes();
        }
        finally
        {
            release.Set();
            worker.DoWork -= holdBeforeCompletion;
        }
    }

    private static void CheckConnectionVersionAfterWait()
    {
        using var connection = new LoopbackConnection();
        var sendLock = GetField<SemaphoreSlim>(connection.Camera, "_sendLock");
        long originalVersion = connection.Camera.ConnectionVersion;
        Task<bool> pending;
        sendLock.Wait();
        try
        {
            pending = connection.Camera.SendAsync("MP5", originalVersion);
            Assert(!pending.IsCompleted, "Lệnh phải chờ khóa gửi.");
            // Mô phỏng transport vừa chuyển sang một phiên kết nối khác.
            SetField(connection.Camera, "_connectionVersion", originalVersion + 1);
        }
        finally
        {
            sendLock.Release();
        }

        Assert(!pending.WaitAsync(CheckTimeout).GetAwaiter().GetResult(), "Lệnh của phiên cũ phải bị từ chối sau khi chờ khóa.");
        connection.AssertNoExtraBytes();
        Assert(connection.Camera.SendAsync("MP6", originalVersion + 1).WaitAsync(CheckTimeout).GetAwaiter().GetResult(),
            "Lệnh thuộc phiên hiện tại phải gửi được.");
        AssertBytes(connection.ReadExactly(3), "MP6");
        connection.AssertNoExtraBytes();
    }

    private static void CheckQueuedResultAfterReconnect()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        lock (GetField<object>(dashboard, "_cameraQueueLock"))
        {
            InvokePrivate(dashboard, "HandleCameraDataReceived", ValidCode);
            // Bản tin đã giữ phiên cũ nhưng worker chưa lấy ra xử lý.
            SetField(connection.Camera, "_connectionVersion", connection.Camera.ConnectionVersion + 1);
        }
        WaitForWorker(dashboard, 1);
        connection.AssertNoExtraBytes();
        Assert(FD_Globals.QueueRecord.Single().Status == e_Production_Status.Error,
            "Kết quả không còn hợp lệ sau reconnect phải lưu Error.");
        Assert(FD_Globals.ActiveSet.Count == 0 && FD_Globals.QueueActive.IsEmpty,
            "Kết quả cũ không được kích hoạt mã.");
    }

    private static void CheckPlcRouting()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        using var timeout = new CancellationTokenSource(CheckTimeout);
        int port = ((IPEndPoint)server.Client.LocalEndPoint!).Port;
        var plc = GetField<TTManager.PLCHelpers.OmronPLC_Hsl>(dashboard, "omronPLC_Hsl1").plc;
        plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeUdpNet("127.0.0.1", port)
        {
            ReceiveTimeOut = 2000,
            SleepTime = 0,
            IsPersistentConnection = true
        };
        var addresses = (Dictionary<string, string>)typeof(PLCAddressWithGoogleSheetHelper)
            .GetField("_addressMap", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        addresses["PLC_Reject_DM"] = "D200";
        AppConfigs.Current.CC320_Enabled = false;

        // Chỉ phản hồi hai lệnh ghi FINS/UDP cần cho check, không chạy PLC thật.
        var receive = Task.Run(async () =>
        {
            var values = new List<byte[]>();
            for (int i = 0; i < 2; i++)
            {
                var request = await server.ReceiveAsync(timeout.Token).ConfigureAwait(false);
                Assert(request.Buffer.Length == 20, "Lệnh ghi PLC phải gồm một word.");
                Assert(request.Buffer.AsSpan(10, 8).SequenceEqual(new byte[] { 1, 2, 0x82, 0, 200, 0, 0, 1 }),
                    "Phải ghi một word vào DM D200.");
                values.Add(request.Buffer[^2..]);
                var response = new byte[14];
                request.Buffer.AsSpan(0, 12).CopyTo(response);
                response[0] = 0xC0;
                request.Buffer.AsSpan(6, 3).CopyTo(response.AsSpan(3, 3));
                request.Buffer.AsSpan(3, 3).CopyTo(response.AsSpan(6, 3));
                await server.SendAsync(response, request.RemoteEndPoint, timeout.Token).ConfigureAwait(false);
            }
            return values;
        });

        GlobalVarialbles.CameraToCC320ElapsedMs = 123.456;
        Assert((bool)InvokePrivate(dashboard, "SendInspectionResult", e_Production_Status.Pass, ValidCode,
            null, Stopwatch.GetTimestamp())!, "Nhánh PLC không gửi được Pass.");
        Assert((bool)InvokePrivate(dashboard, "SendInspectionResult", e_Production_Status.ReadFail, "FAIL",
            null, Stopwatch.GetTimestamp())!, "Nhánh PLC không gửi được Fail.");
        var values = receive.WaitAsync(CheckTimeout).GetAwaiter().GetResult();
        Assert(values[0].AsSpan().SequenceEqual(new byte[] { 0, 1 }), "PLC Pass phải giữ giá trị 1.");
        Assert(values[1].AsSpan().SequenceEqual(new byte[] { 0, 0 }), "PLC Fail phải giữ giá trị 0.");
        Assert(GlobalVarialbles.CameraToCC320ElapsedMs == -1, "Nhánh PLC không được giữ số đo CC320 cũ.");
        Assert(FD_Globals.QueueRecord.All(record => record.CameraToCC320ElapsedMs == null),
            "Bản ghi PLC không có thời gian gửi CC320.");
        connection.AssertNoExtraBytes();
    }

    private static void CheckTimingIncludesQueueWait()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        double queuedMs;
        lock (GetField<object>(dashboard, "_cameraQueueLock"))
        {
            InvokePrivate(dashboard, "HandleCameraDataReceived", ValidCode);
            // Giữ worker để chứng minh mốc đo bắt đầu lúc nhận, không phải lúc dequeue/gửi.
            long queuedAt = Stopwatch.GetTimestamp();
            Thread.Sleep(60);
            queuedMs = Stopwatch.GetElapsedTime(queuedAt).TotalMilliseconds;
        }
        AssertBytes(connection.ReadExactly(3), "MP5");
        WaitForWorker(dashboard, 1);
        var record = FD_Globals.QueueRecord.Single();
        Assert(record.CameraToCC320ElapsedMs >= queuedMs,
            $"Số đo phải bao gồm ít nhất {queuedMs:F3} ms chờ worker.");
        Assert(GlobalVarialbles.CameraToCC320ElapsedMs == record.CameraToCC320ElapsedMs,
            "Global phải bằng số đo của kết quả vừa gửi.");
        connection.AssertNoExtraBytes();
    }

    private static void CheckTimingSnapshotsOnListbox()
    {
        using var connection = new LoopbackConnection();
        using var dashboard = CreateDashboard(connection.Camera);
        long firstReceivedAt = Stopwatch.GetTimestamp();
        Thread.Sleep(20);
        Assert((bool)InvokePrivate(dashboard, "SendInspectionResult", e_Production_Status.Pass, ValidCode,
            null, firstReceivedAt)!, "Không gửi được kết quả có số đo thứ nhất.");
        var first = FD_Globals.QueueRecord.Single();
        double? firstElapsedMs = first.CameraToCC320ElapsedMs;

        long secondReceivedAt = Stopwatch.GetTimestamp();
        Thread.Sleep(70);
        Assert((bool)InvokePrivate(dashboard, "SendInspectionResult", e_Production_Status.ReadFail, "FAIL",
            null, secondReceivedAt)!, "Không gửi được kết quả có số đo thứ hai.");
        var second = FD_Globals.QueueRecord.Last();
        AssertBytes(connection.ReadExactly(6), "MP5MP6");
        Assert(firstElapsedMs.HasValue && second.CameraToCC320ElapsedMs.HasValue,
            "Cả hai bản ghi CC320 đều phải có số đo.");
        double firstMeasuredMs = firstElapsedMs ?? throw new InvalidOperationException("Thiếu số đo code thứ nhất.");
        double secondMeasuredMs = second.CameraToCC320ElapsedMs ?? throw new InvalidOperationException("Thiếu số đo code thứ hai.");
        Assert(first.CameraToCC320ElapsedMs == firstElapsedMs, "Số đo cũ không được thay đổi khi code mới được gửi.");
        Assert(GlobalVarialbles.CameraToCC320ElapsedMs == second.CameraToCC320ElapsedMs,
            "Global phải cập nhật theo code vừa gửi sau cùng.");

        // Đổi Global trước khi render để bắt lỗi dùng nhầm số đo mới nhất cho mọi dòng.
        GlobalVarialbles.CameraToCC320ElapsedMs = 98765.432;
        InvokePrivate(dashboard, "Render_Production_Result", first);
        InvokePrivate(dashboard, "Render_Production_Result", second);
        var view = GetField<Sunny.UI.UIListBox>(dashboard, "opView");
        string firstText = view.Items[1].ToString()!;
        string secondText = view.Items[0].ToString()!;
        Assert(firstText.StartsWith($"[CC320: {firstMeasuredMs:F3} ms]") && firstText.Contains(ValidCode),
            "Dòng code thứ nhất phải hiển thị số đo riêng của nó phía trước.");
        Assert(secondText.StartsWith($"[CC320: {secondMeasuredMs:F3} ms]") && secondText.Contains("FAIL"),
            "Dòng code thứ hai phải hiển thị số đo riêng của nó phía trước.");
        Assert(!firstText.Contains("98765") && !secondText.Contains("98765"),
            "Listbox không được lấy số đo Global tại thời điểm render.");
        connection.AssertNoExtraBytes();
    }

    private static void CheckFailedSendTiming()
    {
        using var dashboard = CreateDashboard(new DatalogicCamera("127.0.0.1", 1));
        GlobalVarialbles.CameraToCC320ElapsedMs = 123.456;
        InvokePrivate(dashboard, "HandleCameraDataReceived", ValidCode);
        WaitForWorker(dashboard, 1);
        var record = FD_Globals.QueueRecord.Single();
        Assert(GlobalVarialbles.CameraToCC320ElapsedMs == -1, "Gửi lỗi phải xóa số đo Global cũ.");
        Assert(record.Status == e_Production_Status.Error && record.CameraToCC320ElapsedMs == null,
            "Gửi lỗi không được hiển thị số đo của một lần gửi thành công.");
        InvokePrivate(dashboard, "Render_Production_Result", record);
        var view = GetField<Sunny.UI.UIListBox>(dashboard, "opView");
        Assert(!view.Items[0].ToString()!.StartsWith("[CC320:"),
            "Bản ghi gửi lỗi không được gắn thời gian CC320 giả.");
    }

    private static void CheckConcurrentTransport()
    {
        using var connection = new LoopbackConnection();
        connection.Client.SendBufferSize = 1024;
        string first = new string('A', 256 * 1024);
        string second = new string('B', 256 * 1024);
        var receive = Task.Run(() => connection.ReadExactly(first.Length + second.Length + 3));
        Task<bool>[] sends =
        [
            connection.Camera.SendAsync(first),
            connection.Camera.SendAsync(second),
            connection.Camera.SendAsync("MP6")
        ];
        bool[] sent = Task.WhenAll(sends).WaitAsync(CheckTimeout).GetAwaiter().GetResult();
        Assert(sent.All(value => value), "Gửi đồng thời phải thành công.");
        AssertBytes(receive.WaitAsync(CheckTimeout).GetAwaiter().GetResult(), first + second + "MP6");
        connection.AssertNoExtraBytes();
    }

    private static void CheckPlcSettingsPersistence()
    {
        string directory = Path.Combine(Path.GetTempPath(), "TApp-PLC-check-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new PLCSettingsStore(directory);
            var parameters = store.LoadParameters("D100");
            Assert(parameters.Select(x => x.Address).SequenceEqual(["D100", "D102", "D104"]),
                "Danh sách khởi tạo phải dành hai word cho mỗi tham số Int32.");
            store.SaveParameters(parameters);
            Assert(PLCSettingsStore.NormalizeAddress("D100.3") == "D100.03", "Địa chỉ bit một chữ số phải được chuẩn hóa hai chữ số.");
            Assert(PLCSettingsStore.ValidateAddress("D100.16", PlcValueType.Bit) != null, "Bit thứ 16 phải bị từ chối.");

            File.WriteAllText(Path.Combine(directory, "Legacy.rplc"),
                "{\"DelayCamera\":\"123\",\"DelayReject\":\"456\",\"RejectStreng\":\"7\"}");
            var legacy = store.LoadRecipe("Legacy", parameters);
            Assert(legacy.Values["D100"] == "123" && legacy.Values["D102"] == "456" && legacy.Values["D104"] == "7",
                "Recipe cũ phải nạp được ba giá trị legacy vào địa chỉ tương ứng.");

            Assert(store.CreateRecipe("Copy", legacy, parameters) == null, "Tạo recipe mới phải thành công.");
            var copy = store.LoadRecipe("Copy", parameters);
            Assert(copy.Values["D102"] == "456", "Recipe mới phải giữ nguyên giá trị được sao chép.");

            var overlap = new[]
            {
                new PlcParameterDefinition { Name = "Value", Address = "D100", ValueType = PlcValueType.Int32 },
                new PlcParameterDefinition { Name = "Bit", Address = "D101.00", ValueType = PlcValueType.Bit }
            };
            bool overlapRejected = false;
            try { store.SaveParameters(overlap); }
            catch (ArgumentException) { overlapRejected = true; }
            Assert(overlapRejected, "Danh sách phải chặn bit nằm trong word thứ hai của Int32.");

            bool readyWordRejected = false;
            try
            {
                store.SaveParameters([new PlcParameterDefinition { Name = "Ready", Address = "D16", ValueType = PlcValueType.Int32 }]);
            }
            catch (ArgumentException) { readyWordRejected = true; }
            Assert(readyWordRejected, "D16 phải được dành riêng cho trạng thái Ready của PLC.");

            bool duplicateBitRejected = false;
            try
            {
                store.SaveParameters([
                    new PlcParameterDefinition { Name = "BitA", Address = "D100.3", ValueType = PlcValueType.Bit },
                    new PlcParameterDefinition { Name = "BitB", Address = "D100.03", ValueType = PlcValueType.Bit }
                ]);
            }
            catch (ArgumentException) { duplicateBitRejected = true; }
            Assert(duplicateBitRejected, "Cùng bit phải bị nhận diện trùng dù cách viết khác nhau.");

            Assert(store.DeleteRecipe("Copy"), "Recipe không mặc định phải xóa được.");
            Assert(!store.DeleteRecipe("Default"), "Recipe mặc định không được xóa.");

            var audit = new PLCRecipeAuditLog(directory);
            audit.Record("SELECT", "Legacy", "D100=123", "test");
            Assert(audit.GetLastSelectedRecipeName() == "Legacy", "Audit phải nhớ recipe được chọn gần nhất.");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static void CheckPlcSettingsPage()
    {
        using var page = new PLCSetting();
        var tabs = GetField<Sunny.UI.UITabControl>(page, "uiTabControl1");
        var grid = GetField<System.Windows.Forms.DataGridView>(page, "_grid");
        Assert(tabs.TabPages.Count == 1, "Trang PLC phải chỉ giữ lại tab đầu.");
        Assert(tabs.TabPages[0].Text == "Camera Trước", "Tab còn lại phải là tab Camera Trước.");
        Assert(grid.Columns.Count == 8, "Bảng D phải có đủ cột tên, PLC, input, address, type, quyền, mô tả và trạng thái.");
    }

    private static FDashboard CreateDashboard(DatalogicCamera camera)
    {
        var dashboard = new FDashboard();
        Assert(!dashboard.IsHandleCreated, "Bộ kiểm tra không được tạo cửa sổ Dashboard.");
        SetField(dashboard, "_cc320", camera);
        return dashboard;
    }

    private static void WaitForWorker(FDashboard dashboard, int expectedRecords)
    {
        var worker = GetField<BackgroundWorker>(dashboard, "WK_Camera");
        var watch = Stopwatch.StartNew();
        while (worker.IsBusy || FD_Globals.QueueRecord.Count < expectedRecords)
        {
            if (watch.Elapsed > CheckTimeout) throw new TimeoutException("Hết thời gian chờ xử lý kết quả camera.");
            Thread.Sleep(5);
        }
    }

    private static object? InvokePrivate(object target, string methodName, params object?[] arguments)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(target.GetType().Name, methodName);
        object?[] completeArguments = method.GetParameters().Select((_, index) =>
            index < arguments.Length ? arguments[index] : Type.Missing).ToArray();
        try
        {
            return method.Invoke(target, completeArguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static T GetField<T>(object target, string name) =>
        (T)(target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target)
            ?? throw new MissingFieldException(target.GetType().Name, name));

    private static void SetField(object target, string name, object? value) =>
        (target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(target.GetType().Name, name)).SetValue(target, value);

    private static void AssertBytes(byte[] actual, string expected) =>
        Assert(actual.AsSpan().SequenceEqual(Encoding.UTF8.GetBytes(expected)), "Dữ liệu TCP nhận được không đúng byte hoặc thứ tự.");

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class LoopbackConnection : IDisposable
    {
        private readonly Socket _peer;
        public Socket Client { get; }
        public DatalogicCamera Camera { get; }

        public LoopbackConnection()
        {
            using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen(1);
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            Client.Connect(listener.LocalEndPoint!);
            _peer = listener.Accept();
            Camera = new DatalogicCamera("127.0.0.1", ((IPEndPoint)listener.LocalEndPoint!).Port);
            // Chỉ thay khâu kết nối; toàn bộ SendAsync và dispatcher dùng mã production.
            SetField(Camera, "_client", Client);
            SetConnected(true);
        }

        public byte[] ReadExactly(int length)
        {
            byte[] buffer = new byte[length];
            using var timeout = new CancellationTokenSource(CheckTimeout);
            int received = 0;
            while (received < length)
            {
                int count = _peer.ReceiveAsync(buffer.AsMemory(received), SocketFlags.None, timeout.Token)
                    .AsTask().GetAwaiter().GetResult();
                Assert(count > 0, "Kết nối đóng trước khi nhận đủ byte.");
                received += count;
            }
            return buffer;
        }

        public void AssertNoExtraBytes() =>
            Assert(!_peer.Poll(50_000, SelectMode.SelectRead), "Có byte thừa sau lệnh TCP.");

        private void SetConnected(bool connected) =>
            typeof(DatalogicCamera).GetProperty(nameof(DatalogicCamera.Connected))!.SetValue(Camera, connected);

        public void Dispose()
        {
            // Không gọi Disconnect vì component hiện tự bật reconnect trong hàm đó.
            SetConnected(false);
            SetField(Camera, "_client", null);
            Client.Dispose();
            _peer.Dispose();
        }
    }
}
