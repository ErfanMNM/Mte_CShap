using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TApp.Models;

namespace TApp.Services
{
    /// <summary>
    /// Verify license trong TApp (offline-first)
    /// </summary>
    public class LicenseVerifier
    {
        private const string LicenseFileName = "license.lic";
        private readonly string _licensePath;
        private readonly string _publicKeyPath;
        private LicenseModel? _cachedLicense;

        public LicenseVerifier()
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TApp"
            );
            Directory.CreateDirectory(appDataDir);

            _licensePath = Path.Combine(appDataDir, LicenseFileName);
            _publicKeyPath = Path.Combine(appDataDir, "public.key");
        }

        /// <summary>
        /// Import public key từ base64 string
        /// </summary>
        public void ImportPublicKey(string publicKeyBase64)
        {
            try
            {
                var publicKey = Convert.FromBase64String(publicKeyBase64);
                File.WriteAllBytes(_publicKeyPath, publicKey);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể import public key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verify license từ file
        /// </summary>
        public (bool IsValid, LicenseModel? License, string ErrorMessage) VerifyLicense()
        {
            // Kiểm tra file license có tồn tại không
            if (!File.Exists(_licensePath))
            {
                return (false, null, "Không tìm thấy file license!");
            }

            // Kiểm tra public key có tồn tại không
            if (!File.Exists(_publicKeyPath))
            {
                return (false, null, "Không tìm thấy public key! Vui lòng import public key trước.");
            }

            try
            {
                // Đọc license content
                var licenseContent = File.ReadAllText(_licensePath, Encoding.UTF8);

                // Parse license content
                var licenseObj = JsonSerializer.Deserialize<JsonElement>(licenseContent);
                
                if (!licenseObj.TryGetProperty("Data", out var dataElement) ||
                    !licenseObj.TryGetProperty("Signature", out var signatureElement))
                {
                    return (false, null, "License file không đúng định dạng!");
                }

                var data = dataElement.GetString() ?? string.Empty;
                var signatureBase64 = signatureElement.GetString() ?? string.Empty;

                // Verify signature bằng public key
                using (var rsa = RSA.Create(2048))
                {
                    var publicKey = File.ReadAllBytes(_publicKeyPath);
                    rsa.ImportRSAPublicKey(publicKey, out _);

                    var dataBytes = Encoding.UTF8.GetBytes(data);
                    var signature = Convert.FromBase64String(signatureBase64);

                    bool isValid = rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    if (!isValid)
                    {
                        return (false, null, "License không hợp lệ! Chữ ký không đúng.");
                    }
                }

                // Deserialize license model
                var license = JsonSerializer.Deserialize<LicenseModel>(data);
                if (license == null)
                {
                    return (false, null, "Không thể parse license data!");
                }

                // Kiểm tra thời gian
                if (!license.IsValid())
                {
                    return (false, license, $"License đã hết hạn! (Hết hạn: {license.ExpiryDate:dd/MM/yyyy})");
                }

                // Cache license
                _cachedLicense = license;

                return (true, license, "License hợp lệ!");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi verify license: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra tính năng có được kích hoạt không
        /// </summary>
        public bool HasFeature(string feature)
        {
            if (_cachedLicense == null)
            {
                var (isValid, license, _) = VerifyLicense();
                if (!isValid || license == null)
                    return false;
                _cachedLicense = license;
            }

            return _cachedLicense.HasFeature(feature);
        }

        /// <summary>
        /// Lấy license đã cache
        /// </summary>
        public LicenseModel? GetCachedLicense()
        {
            if (_cachedLicense == null)
            {
                var (isValid, license, _) = VerifyLicense();
                if (isValid && license != null)
                {
                    _cachedLicense = license;
                }
            }
            return _cachedLicense;
        }

        /// <summary>
        /// Kiểm tra license có tồn tại không
        /// </summary>
        public bool LicenseExists()
        {
            return File.Exists(_licensePath);
        }

        /// <summary>
        /// Số ngày còn lại
        /// </summary>
        public int DaysRemaining()
        {
            var license = GetCachedLicense();
            if (license == null)
                return 0;
            return license.DaysRemaining();
        }
    }
}

