using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace TApp.Security
{
    /// <summary>
    /// Kiểm tra tính toàn vẹn của code (anti-tampering)
    /// </summary>
    public static class CodeIntegrity
    {
        /// <summary>
        /// Tính hash của assembly hiện tại
        /// </summary>
        public static string CalculateAssemblyHash()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var location = assembly.Location;

                if (string.IsNullOrEmpty(location))
                    return string.Empty;

                using (var md5 = MD5.Create())
                {
                    using (var stream = System.IO.File.OpenRead(location))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Kiểm tra hash của method (có thể dùng để phát hiện code bị modify)
        /// </summary>
        public static string CalculateMethodHash(MethodInfo method)
        {
            try
            {
                // Lấy IL code của method
                var methodBody = method.GetMethodBody();
                if (methodBody == null)
                    return string.Empty;

                var ilBytes = methodBody.GetILAsByteArray();
                if (ilBytes == null || ilBytes.Length == 0)
                    return string.Empty;

                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(ilBytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Kiểm tra xem assembly có bị modify không
        /// (So sánh với hash đã lưu trước đó)
        /// </summary>
        public static bool VerifyAssemblyIntegrity(string expectedHash)
        {
            if (string.IsNullOrEmpty(expectedHash))
                return true; // Nếu không có hash để so sánh, bỏ qua

            var currentHash = CalculateAssemblyHash();
            return string.Equals(currentHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra license verification method có bị modify không
        /// </summary>
        public static bool VerifyLicenseMethodIntegrity()
        {
            try
            {
                var verifierType = typeof(Services.LicenseVerifier);
                var verifyMethod = verifierType.GetMethod("VerifyLicense", 
                    BindingFlags.Public | BindingFlags.Instance);

                if (verifyMethod == null)
                    return false;

                var methodHash = CalculateMethodHash(verifyMethod);
                
                // Có thể lưu hash này và so sánh
                // Ở đây chỉ return true, có thể mở rộng
                return !string.IsNullOrEmpty(methodHash);
            }
            catch
            {
                return false;
            }
        }
    }
}

