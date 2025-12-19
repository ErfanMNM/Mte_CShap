using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DemoApp.Models;

namespace DemoApp.Services
{
    /// <summary>
    /// Quản lý license: tạo, ký, verify bằng RSA
    /// </summary>
    public class LicenseManager
    {
        private const int KeySize = 2048;
        private readonly string _privateKeyPath;
        private readonly string _publicKeyPath;

        public LicenseManager()
        {
            var keysDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TAppLicense",
                "Keys"
            );
            Directory.CreateDirectory(keysDir);

            _privateKeyPath = Path.Combine(keysDir, "private.key");
            _publicKeyPath = Path.Combine(keysDir, "public.key");
        }

        /// <summary>
        /// Tạo cặp khóa RSA mới (chỉ làm 1 lần)
        /// </summary>
        public void GenerateKeyPair()
        {
            using (var rsa = RSA.Create(KeySize))
            {
                // Lưu private key (dùng để ký license)
                var privateKey = rsa.ExportRSAPrivateKey();
                File.WriteAllBytes(_privateKeyPath, privateKey);

                // Lưu public key (dùng để verify license)
                var publicKey = rsa.ExportRSAPublicKey();
                File.WriteAllBytes(_publicKeyPath, publicKey);
            }
        }

        /// <summary>
        /// Kiểm tra đã có key pair chưa
        /// </summary>
        public bool HasKeyPair()
        {
            return File.Exists(_privateKeyPath) && File.Exists(_publicKeyPath);
        }

        /// <summary>
        /// Tạo và ký license
        /// </summary>
        public string GenerateLicense(LicenseModel license)
        {
            if (!HasKeyPair())
            {
                throw new InvalidOperationException("Chưa có key pair. Vui lòng tạo key pair trước!");
            }

            // Tạo LicenseKey nếu chưa có
            if (string.IsNullOrWhiteSpace(license.LicenseKey))
            {
                license.LicenseKey = GenerateLicenseKey();
            }

            // Serialize license thành JSON
            var json = JsonSerializer.Serialize(license, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Ký bằng private key
            using (var rsa = RSA.Create(KeySize))
            {
                var privateKey = File.ReadAllBytes(_privateKeyPath);
                rsa.ImportRSAPrivateKey(privateKey, out _);

                var dataBytes = Encoding.UTF8.GetBytes(json);
                var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                
                // Tạo license file content: JSON + Signature (base64)
                var licenseContent = new
                {
                    Data = json,
                    Signature = Convert.ToBase64String(signature)
                };

                return JsonSerializer.Serialize(licenseContent, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
        }

        /// <summary>
        /// Verify license từ file content
        /// </summary>
        public (bool IsValid, LicenseModel? License, string ErrorMessage) VerifyLicense(string licenseContent)
        {
            try
            {
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
                if (!File.Exists(_publicKeyPath))
                {
                    return (false, null, "Không tìm thấy public key để verify!");
                }

                using (var rsa = RSA.Create(KeySize))
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

                return (true, license, "License hợp lệ!");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi verify license: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu license vào file
        /// </summary>
        public void SaveLicenseToFile(string licenseContent, string filePath)
        {
            File.WriteAllText(filePath, licenseContent, Encoding.UTF8);
        }

        /// <summary>
        /// Đọc license từ file
        /// </summary>
        public string LoadLicenseFromFile(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// Export public key để đưa cho TApp
        /// </summary>
        public string ExportPublicKey()
        {
            if (!File.Exists(_publicKeyPath))
            {
                throw new FileNotFoundException("Chưa có public key!");
            }

            var publicKey = File.ReadAllBytes(_publicKeyPath);
            return Convert.ToBase64String(publicKey);
        }

        /// <summary>
        /// Tạo LicenseKey ngẫu nhiên
        /// </summary>
        private string GenerateLicenseKey()
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var key = new StringBuilder();
            
            for (int i = 0; i < 4; i++)
            {
                if (i > 0) key.Append("-");
                for (int j = 0; j < 4; j++)
                {
                    key.Append(chars[random.Next(chars.Length)]);
                }
            }
            
            return key.ToString();
        }
    }
}

