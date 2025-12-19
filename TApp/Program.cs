using TApp.Services;
using TApp.Security;
using TApp.Configs;
using System.Windows.Forms;
using System.Threading;

namespace TApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ========== LOAD CONFIG ==========
            // Load config trước để check bypass flag
            AppConfigs.Current.Load();

            // ========== BYPASS SECURITY CHECKS (DEBUG MODE) ==========
            // Cho phép bypass security checks khi debug
            // Có thể set từ AppConfigs hoặc environment variable
            var bypassFromConfig = AppConfigs.Current.Security_Bypass_Enabled;
            var bypassFromEnv = Environment.GetEnvironmentVariable("TAPP_BYPASS_SECURITY") == "1";
            
#if DEBUG
            // Trong DEBUG mode, tự động bypass
            AntiReverseEngineering.BypassSecurityChecks = true;
            System.Diagnostics.Debug.WriteLine("⚠️ DEBUG MODE: Security checks are BYPASSED");
#else
            // Trong RELEASE mode, check config và env
            AntiReverseEngineering.BypassSecurityChecks = bypassFromConfig || bypassFromEnv;
            
            if (AntiReverseEngineering.BypassSecurityChecks)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ WARNING: Security checks are BYPASSED via config/env!");
            }
#endif

            // ========== ANTI-REVERSE ENGINEERING CHECKS ==========
            // Kiểm tra bảo mật trước khi chạy (sẽ tự động bypass nếu flag được set)
            var securityCheck = AntiReverseEngineering.PerformSecurityChecks();
            
            // Nếu phát hiện reverse engineering, thoát ngay (trừ khi bypass)
            if (!securityCheck.IsSecure && !AntiReverseEngineering.BypassSecurityChecks)
            {
                // Có thể log hoặc gửi thông tin về server
                AntiReverseEngineering.HandleSecurityViolation(securityCheck);
                return;
            }

            // Anti-debugging timing check (sẽ tự động skip nếu bypass)
            AntiReverseEngineering.AntiDebugTimingCheck();

            // ========== LICENSE VERIFICATION ==========
            // Verify license trước khi chạy (có thể bypass trong debug mode)
            var licenseVerifier = new LicenseVerifier();
            var (isValid, license, errorMessage) = licenseVerifier.VerifyLicense();

#if DEBUG
            // Trong DEBUG mode, cho phép chạy không cần license (hoặc có thể bypass)
            if (!isValid && !AntiReverseEngineering.BypassSecurityChecks)
            {
                var bypassLicense = MessageBox.Show(
                    $"License không hợp lệ!\n\n{errorMessage}\n\nBạn có muốn BYPASS license check để debug không?",
                    "License Error (DEBUG MODE)",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                
                if (bypassLicense == DialogResult.No)
                {
                    return;
                }
                // Nếu chọn Yes, tiếp tục chạy
            }
#else
            // Trong RELEASE mode, bắt buộc phải có license hợp lệ
            if (!isValid)
            {
                var message = $"License không hợp lệ!\n\n{errorMessage}\n\nVui lòng liên hệ admin để được cấp license.";
                MessageBox.Show(message, "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kiểm tra integrity của license file (chỉ trong RELEASE)
            var licensePath = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "TApp",
                "license.lic"
            );
            if (!AntiReverseEngineering.CheckLicenseFileIntegrity(licensePath))
            {
                MessageBox.Show("License file đã bị chỉnh sửa hoặc hỏng!", "License Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
#endif

            // Hiển thị thông tin license (optional - có thể bỏ qua)
            if (license != null && license.DaysRemaining() <= 30)
            {
                var warning = $"Cảnh báo: License sẽ hết hạn sau {license.DaysRemaining()} ngày!\nNgày hết hạn: {license.ExpiryDate:dd/MM/yyyy}\n\nVui lòng gia hạn license sớm.";
                MessageBox.Show(warning, "License Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "TApp", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("Phần mềm đang chạy rồi bro!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                //Sunny.UI.StyleManager.Style = Sunny.UI.UIStyle.Dark;
                Application.Run(new MainForm());
            }
        }
    }
}