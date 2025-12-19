using TApp.Services;

namespace TApp.Helpers
{
    /// <summary>
    /// Helper class để kiểm tra license features trong TApp
    /// </summary>
    public static class LicenseHelper
    {
        private static LicenseVerifier? _verifier;
        private static readonly object _lock = new object();

        /// <summary>
        /// Lấy instance của LicenseVerifier (singleton)
        /// </summary>
        private static LicenseVerifier GetVerifier()
        {
            if (_verifier == null)
            {
                lock (_lock)
                {
                    if (_verifier == null)
                    {
                        _verifier = new LicenseVerifier();
                    }
                }
            }
            return _verifier;
        }

        /// <summary>
        /// Kiểm tra tính năng có được kích hoạt không
        /// </summary>
        public static bool HasFeature(string feature)
        {
            try
            {
                return GetVerifier().HasFeature(feature);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra tính năng Camera
        /// </summary>
        public static bool HasCameraFeature()
        {
            return HasFeature("Camera");
        }

        /// <summary>
        /// Kiểm tra tính năng Printer
        /// </summary>
        public static bool HasPrinterFeature()
        {
            return HasFeature("Printer");
        }

        /// <summary>
        /// Kiểm tra tính năng Cloud
        /// </summary>
        public static bool HasCloudFeature()
        {
            return HasFeature("Cloud");
        }

        /// <summary>
        /// Kiểm tra tính năng PLC
        /// </summary>
        public static bool HasPLCFeature()
        {
            return HasFeature("PLC");
        }

        /// <summary>
        /// Lấy số ngày còn lại của license
        /// </summary>
        public static int GetDaysRemaining()
        {
            try
            {
                return GetVerifier().DaysRemaining();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lấy thông tin license hiện tại
        /// </summary>
        public static Models.LicenseModel? GetLicense()
        {
            try
            {
                return GetVerifier().GetCachedLicense();
            }
            catch
            {
                return null;
            }
        }
    }
}

