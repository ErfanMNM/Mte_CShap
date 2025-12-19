using System;
using System.Security.Cryptography;
using System.Text;

namespace TApp.Security
{
    /// <summary>
    /// Mã hóa string để tránh lộ thông tin trong IL code
    /// </summary>
    public static class StringEncryption
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("TAppLicense2024!SecretKeyForStringEncryption12345678");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");

        /// <summary>
        /// Mã hóa string
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch
            {
                return plainText;
            }
        }

        /// <summary>
        /// Giải mã string
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        var cipherBytes = Convert.FromBase64String(cipherText);
                        var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch
            {
                return cipherText;
            }
        }

        /// <summary>
        /// Lấy string đã mã hóa (dùng trong code để tránh lộ string)
        /// </summary>
        public static string GetEncryptedString(string encrypted)
        {
            return Decrypt(encrypted);
        }
    }
}

