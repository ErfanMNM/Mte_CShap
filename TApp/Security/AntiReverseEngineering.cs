using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace TApp.Security
{
    /// <summary>
    /// Các kỹ thuật chống reverse engineering và debugging
    /// </summary>
    public static class AntiReverseEngineering
    {
        /// <summary>
        /// Flag để bypass security checks (chỉ dùng khi debug)
        /// Có thể set từ AppConfigs hoặc environment variable
        /// </summary>
        public static bool BypassSecurityChecks { get; set; } = false;

        #region Windows API

        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("kernel32.dll")]
        private static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        private const uint GENERIC_READ = 0x80000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        #endregion

        /// <summary>
        /// Kiểm tra và chống debugger
        /// </summary>
        public static bool CheckDebugger()
        {
#if DEBUG
            // Trong DEBUG mode, luôn bypass (cho phép debug)
            return false;
#endif

            // Nếu bypass flag được bật, skip check
            if (BypassSecurityChecks)
            {
                return false;
            }

            // 1. Kiểm tra IsDebuggerPresent
            if (IsDebuggerPresent())
            {
                return true;
            }

            // 2. Kiểm tra Remote Debugger
            bool isRemoteDebugger = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isRemoteDebugger);
            if (isRemoteDebugger)
            {
                return true;
            }

            // 3. Kiểm tra Debugger trong Process
            if (Debugger.IsAttached)
            {
                return true;
            }

            // 4. Kiểm tra Debugger trong Environment
            if (Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING") == "1")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra các công cụ reverse engineering phổ biến
        /// </summary>
        public static bool CheckReverseEngineeringTools()
        {
#if DEBUG
            // Trong DEBUG mode, bypass check
            return false;
#endif

            // Nếu bypass flag được bật, skip check
            if (BypassSecurityChecks)
            {
                return false;
            }

            try
            {
                // Danh sách các process cần kiểm tra
                string[] suspiciousProcesses = {
                    "de4dot", "ilspy", "reflector", "dnspy", "dotpeek",
                    "ida", "ida64", "x64dbg", "x32dbg", "windbg",
                    "ollydbg", "ghidra", "wireshark", "fiddler",
                    "processhacker", "procmon", "apimonitor",
                    "cheatengine", "artmoney", "gameguardian"
                };

                var runningProcesses = Process.GetProcesses()
                    .Select(p => p.ProcessName.ToLower())
                    .ToList();

                foreach (var suspicious in suspiciousProcesses)
                {
                    if (runningProcesses.Contains(suspicious))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                // Nếu có lỗi khi kiểm tra, coi như an toàn
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra Virtual Machine (có thể là sandbox để phân tích)
        /// </summary>
        public static bool CheckVirtualMachine()
        {
#if DEBUG
            // Trong DEBUG mode, bypass check
            return false;
#endif

            // Nếu bypass flag được bật, skip check
            if (BypassSecurityChecks)
            {
                return false;
            }

            try
            {
                // Kiểm tra qua WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                        string model = obj["Model"]?.ToString() ?? "";

                        if (manufacturer.Contains("VMware", StringComparison.OrdinalIgnoreCase) ||
                            manufacturer.Contains("Microsoft Corporation", StringComparison.OrdinalIgnoreCase) ||
                            manufacturer.Contains("Xen", StringComparison.OrdinalIgnoreCase) ||
                            manufacturer.Contains("innotek", StringComparison.OrdinalIgnoreCase) ||
                            model.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase) ||
                            model.Contains("VMware", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                // Kiểm tra registry
                try
                {
                    var vmKeys = new[]
                    {
                        @"HARDWARE\ACPI\DSDT\VBOX__",
                        @"HARDWARE\ACPI\FADT\VBOX__",
                        @"HARDWARE\ACPI\RSDT\VBOX__",
                        @"SYSTEM\ControlSet001\Services\VBoxGuest",
                        @"SYSTEM\ControlSet001\Services\VBoxMouse",
                        @"SYSTEM\ControlSet001\Services\VBoxService",
                        @"SYSTEM\ControlSet001\Services\VBoxSF",
                        @"SYSTEM\ControlSet001\Services\VBoxVideo"
                    };

                    foreach (var key in vmKeys)
                    {
                        if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey(key) != null)
                        {
                            return true;
                        }
                    }
                }
                catch { }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra file có bị modify không (tampering detection)
        /// </summary>
        public static bool CheckFileIntegrity()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var location = assembly.Location;
                
                if (string.IsNullOrEmpty(location))
                    return true; // Nếu không có location, bỏ qua

                var fileInfo = new System.IO.FileInfo(location);
                var creationTime = fileInfo.CreationTime;
                var lastWriteTime = fileInfo.LastWriteTime;

                // Kiểm tra nếu file bị modify gần đây (sau khi build)
                // Có thể lưu hash của file và so sánh
                // Ở đây chỉ kiểm tra cơ bản

                return true; // Tạm thời return true, có thể mở rộng
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Anti-tampering: Kiểm tra xem có process nào đang inject vào không
        /// </summary>
        public static bool CheckProcessInjection()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var modules = currentProcess.Modules.Cast<ProcessModule>();

                // Kiểm tra các module đáng ngờ
                var suspiciousModules = new[]
                {
                    "scylla", "x64dbg", "x32dbg", "cheatengine",
                    "inject", "hook", "dllinject"
                };

                foreach (var module in modules)
                {
                    var moduleName = module.ModuleName?.ToLower() ?? "";
                    if (suspiciousModules.Any(s => moduleName.Contains(s)))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Thực hiện tất cả các kiểm tra bảo mật
        /// </summary>
        public static SecurityCheckResult PerformSecurityChecks()
        {
#if DEBUG
            // Trong DEBUG mode, luôn return secure
            return new SecurityCheckResult
            {
                HasDebugger = false,
                HasReverseEngineeringTools = false,
                IsVirtualMachine = false,
                FileTampered = false,
                HasProcessInjection = false,
                IsSecure = true
            };
#endif

            // Nếu bypass flag được bật, skip tất cả checks
            if (BypassSecurityChecks)
            {
                return new SecurityCheckResult
                {
                    HasDebugger = false,
                    HasReverseEngineeringTools = false,
                    IsVirtualMachine = false,
                    FileTampered = false,
                    HasProcessInjection = false,
                    IsSecure = true
                };
            }

            var result = new SecurityCheckResult();

            result.HasDebugger = CheckDebugger();
            result.HasReverseEngineeringTools = CheckReverseEngineeringTools();
            result.IsVirtualMachine = CheckVirtualMachine();
            result.FileTampered = !CheckFileIntegrity();
            result.HasProcessInjection = CheckProcessInjection();

            result.IsSecure = !result.HasDebugger &&
                            !result.HasReverseEngineeringTools &&
                            !result.FileTampered &&
                            !result.HasProcessInjection;

            return result;
        }

        /// <summary>
        /// Xử lý khi phát hiện reverse engineering
        /// </summary>
        public static void HandleSecurityViolation(SecurityCheckResult result)
        {
            // Log violation (có thể gửi về server)
            var violationInfo = $"Security Violation Detected:\n" +
                              $"Debugger: {result.HasDebugger}\n" +
                              $"RE Tools: {result.HasReverseEngineeringTools}\n" +
                              $"VM: {result.IsVirtualMachine}\n" +
                              $"Tampered: {result.FileTampered}\n" +
                              $"Injection: {result.HasProcessInjection}";

            // Có thể ghi log hoặc gửi về server
            System.Diagnostics.Debug.WriteLine(violationInfo);

            // Thoát ứng dụng
            Environment.Exit(1);
        }

        /// <summary>
        /// Anti-debugging: Delay với timing check
        /// </summary>
        public static void AntiDebugTimingCheck()
        {
#if DEBUG
            // Trong DEBUG mode, skip timing check
            return;
#endif

            // Nếu bypass flag được bật, skip check
            if (BypassSecurityChecks)
            {
                return;
            }

            var start = DateTime.Now;
            Thread.Sleep(100);
            var elapsed = (DateTime.Now - start).TotalMilliseconds;

            // Nếu có debugger, thời gian sẽ khác (do breakpoint)
            if (elapsed > 200)
            {
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Kiểm tra xem có file .lic bị modify không
        /// </summary>
        public static bool CheckLicenseFileIntegrity(string licensePath)
        {
            try
            {
                if (!System.IO.File.Exists(licensePath))
                    return false;

                var fileInfo = new System.IO.FileInfo(licensePath);
                
                // Kiểm tra kích thước file (nếu quá nhỏ hoặc quá lớn thì đáng ngờ)
                if (fileInfo.Length < 100 || fileInfo.Length > 100000)
                {
                    return false;
                }

                // Có thể thêm hash check ở đây
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Kết quả kiểm tra bảo mật
    /// </summary>
    public class SecurityCheckResult
    {
        public bool HasDebugger { get; set; }
        public bool HasReverseEngineeringTools { get; set; }
        public bool IsVirtualMachine { get; set; }
        public bool FileTampered { get; set; }
        public bool HasProcessInjection { get; set; }
        public bool IsSecure { get; set; }
    }
}

