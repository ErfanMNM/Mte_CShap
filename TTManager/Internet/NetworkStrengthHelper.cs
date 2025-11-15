using System.Net.NetworkInformation;


namespace TTManager.Internet
{
    public static class NetworkStrengthHelper
    {
        /// <summary>
        /// Đo tín hiệu mạng (1 đến 5).
        /// </summary>
        public static async Task<int> GetNetworkStrengthAsync()
        {
            string host = "8.8.8.8";
            int attempts = 5;
            long total = 0;
            int success = 0;

            Ping ping = new Ping();

            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    var reply = await ping.SendPingAsync(host, 800);

                    if (reply.Status == IPStatus.Success)
                    {
                        total += reply.RoundtripTime;
                        success++;
                    }
                }
                catch { }
            }

            if (success == 0)
                return 1; // Không ping được -> mạng rất yếu hoặc mất mạng

            double avg = total / (double)success;

            // Chia 5 cấp
            if (avg < 30) return 5;
            if (avg < 70) return 4;
            if (avg < 120) return 3;
            if (avg < 200) return 2;
            return 1;
        }

        public static int GetNetworkStrength()
        {
            string host = "8.8.8.8";
            int attempts = 3;
            long total = 0;
            int success = 0;

            Ping ping = new Ping();

            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    var reply = ping.Send(host, 800);

                    if (reply.Status == IPStatus.Success)
                    {
                        total += reply.RoundtripTime;
                        success++;
                    }
                }
                catch {
                    return 0;
                }
            }

            if (success == 0)
                return 0; // Không ping được = yếu nhất

            double avg = total / (double)success;

            if (avg < 30) return 5;
            if (avg < 70) return 4;
            if (avg < 120) return 3;
            if (avg < 200) return 2;
            return 1;
        }
    }
}
