using TApp.Services;
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
            // Verify license trước khi chạy
            var licenseVerifier = new LicenseVerifier();
            var (isValid, license, errorMessage) = licenseVerifier.VerifyLicense();

            if (!isValid)
            {
                var message = $"License không hợp lệ!\n\n{errorMessage}\n\nVui lòng liên hệ admin để được cấp license.";
                MessageBox.Show(message, "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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