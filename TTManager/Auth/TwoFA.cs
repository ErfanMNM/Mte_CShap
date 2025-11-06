using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TTManager.Auth
{
    public static class TwoFAHelper
    {
        public static string GenerateSecret(int length = 20)
        {
            byte[] buffer = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return Base32.ToBase32(buffer);
        }

        public static string GenerateOTP(string base32Secret, int digits = 6, int timeStep = 30, DateTime? time = null)
        {
            if (digits < 4 || digits > 8)
                throw new ArgumentException("Digits must be between 4 and 8.");

            byte[] key = Base32.FromBase32(base32Secret);
            long unixTime = (long)((time ?? DateTime.UtcNow) - new DateTime(1970, 1, 1)).TotalSeconds;
            long counter = unixTime / timeStep;

            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                byte[] hash = hmac.ComputeHash(counterBytes);
                int offset = hash[hash.Length - 1] & 0x0F;
                int binaryCode = ((hash[offset] & 0x7F) << 24) |
                                 ((hash[offset + 1] & 0xFF) << 16) |
                                 ((hash[offset + 2] & 0xFF) << 8) |
                                 (hash[offset + 3] & 0xFF);

                int otp = binaryCode % (int)Math.Pow(10, digits);
                return otp.ToString(new string('0', digits));
            }
        }

        public static bool VerifyOTP(string base32Secret, string userCode, int digits = 6, int timeStep = 30, int window = 1)
        {
            for (int i = -window; i <= window; i++)
            {
                DateTime time = DateTime.UtcNow.AddSeconds(i * timeStep);
                string generatedCode = GenerateOTP(base32Secret, digits, timeStep, time);
                if (generatedCode == userCode)
                    return true;
            }
            return false;
        }

        public static string GenerateQrCodeUri(string secret, string accountName, string issuer)
        {
            string label = HttpUtility.UrlEncode($"{issuer}:{accountName}");
            string issuerEncoded = HttpUtility.UrlEncode(issuer);
            return $"otpauth://totp/{label}?secret={secret}&issuer={issuerEncoded}&digits=6";
        }
    }

}
