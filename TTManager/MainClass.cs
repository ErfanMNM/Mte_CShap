
using System.Text;


namespace TTManager
{
    public static class Base32
    {
        //t?t c? các ký t? h?p l? trong Base32
        private static readonly char[] Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
        //private static readonly char[] Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789abcdef".ToCharArray();
        private static readonly int[] Map = new int[128];

        static Base32()
        {
            for (int i = 0; i < Map.Length; i++) Map[i] = -1;
            for (int i = 0; i < Digits.Length; i++) Map[Digits[i]] = i;
        }

        public static byte[] FromBase32(string base32)
        {
            base32 = base32.TrimEnd('=').ToUpperInvariant();
            int byteCount = base32.Length * 5 / 8;
            byte[] result = new byte[byteCount];

            int buffer = 0, bitsLeft = 0, index = 0;
            foreach (char c in base32)
            {
                if (Map[c] < 0) throw new ArgumentException("Invalid Base32 char: " + c);
                buffer <<= 5;
                buffer |= Map[c] & 31;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    result[index++] = (byte)(buffer >> (bitsLeft - 8));
                    bitsLeft -= 8;
                }
            }
            return result;
        }

        public static string ToBase32(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            int buffer = data[0];
            int next = 1, bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xff;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }
                int index = (buffer >> (bitsLeft - 5)) & 0x1f;
                bitsLeft -= 5;
                sb.Append(Digits[index]);
            }
            return sb.ToString();
        }
    }
}
