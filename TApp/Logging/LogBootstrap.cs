using System;
using System.IO;
using MTs.Auditrails;

namespace TApp
{
    /// <summary>
    /// Khởi tạo LogHelper cho TApp, lưu tại AppData/TApp/Applogs/App.mtl
    /// </summary>
    public static class LogBootstrap
    {
        private static readonly object _sync = new object();
        private static LogHelper? _logger;

        public static LogHelper Logger
        {
            get
            {
                EnsureInitialized();
                return _logger!;
            }
        }

        /// <summary>
        /// Đảm bảo đã khởi tạo Logger và ghi 1 log mẫu khi lần đầu.
        /// Gọi từ MainForm (vd: trong constructor hoặc sự kiện Load).
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_logger != null) return;

            lock (_sync)
            {
                if (_logger != null) return;

                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dbPath = Path.Combine(appData, "TApp", "Applogs", "App.mtl");

                _logger = new LogHelper(dbPath);

                // Ghi log mẫu
                var user = Environment.UserName;
                _logger.Log(user, "INFO", "App started", "Khởi tạo logger trong MainForm");
            }
        }
    }
}

