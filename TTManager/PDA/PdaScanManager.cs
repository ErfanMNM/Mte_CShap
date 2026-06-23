using System.Collections.Concurrent;

namespace TTManager.PDA
{
    /// <summary>
    /// Central manager that owns the ApiServer and exposes scan events
    /// to the WinForms UI via events or polling via <see cref="DequeueScan"/>.
    /// All ApiServer operations should go through here.
    /// </summary>
    public class PdaScanManager
    {
        private ApiServer? _server;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Raised on the background thread whenever a scan is received.
        /// Subscribe to this if you want real-time push notifications.
        /// </summary>
        public event Action<ScanData>? OnScanReceived;

        /// <summary>
        /// Raised whenever the server logs something.
        /// </summary>
        public event Action<string>? OnLog;

        /// <summary>
        /// Whether the API server is currently running.
        /// </summary>
        public bool IsRunning => _server != null;

        /// <summary>
        /// Port the server is listening on.
        /// </summary>
        public int Port => _serverPort;

        private int _serverPort;

        /// <summary>
        /// Number of pending scans in the queue.
        /// </summary>
        public int PendingCount => _server?.PendingCount ?? 0;

        private readonly ConcurrentQueue<ScanData> _scanHistory = new();

        /// <summary>
        /// Start the API server on the given port.
        /// </summary>
        /// <param name="port">Port to listen on (default 6969).</param>
        public async Task StartAsync(int port = 6969)
        {
            if (_server != null)
            {
                Log($"Server already running on port {_serverPort}.");
                return;
            }

            _serverPort = port;
            _cts = new CancellationTokenSource();

            Log($"Starting API Server on port {port}...");

            _server = new ApiServer(OnScanCallback, port);

            try
            {
                await _server.StartAsync();
                Log($"API Server started successfully on http://0.0.0.0:{port}");
                Log("Waiting for scans from PDA devices...");
            }
            catch (Exception ex)
            {
                Log($"Failed to start server: {ex.Message}");
                _server = null;
                throw;
            }
        }

        /// <summary>
        /// Stop the API server.
        /// </summary>
        public async Task StopAsync()
        {
            if (_server == null) return;

            Log("Stopping API Server...");
            _cts?.Cancel();

            await _server.StopAsync();
            _server = null;
            _cts?.Dispose();
            _cts = null;

            Log("API Server stopped.");
        }

        /// <summary>
        /// Pull the next scan from the queue. Use this from a UI timer or background poll.
        /// Returns null if the queue is empty.
        /// </summary>
        public ScanData? DequeueScan()
        {
            return _server?.DequeueScan();
        }

        /// <summary>
        /// Get recent scan history (last N items).
        /// </summary>
        public IEnumerable<ScanData> GetRecentHistory(int count = 50)
        {
            return _scanHistory.TakeLast(count);
        }

        /// <summary>
        /// Clear the scan history.
        /// </summary>
        public void ClearHistory()
        {
            while (_scanHistory.TryDequeue(out _)) { }
        }

        private void OnScanCallback(ScanData scan)
        {
            _scanHistory.Enqueue(scan);
            OnScanReceived?.Invoke(scan);
            Log($"[SCAN] [{scan.PdaName}] {scan.Code} @ {scan.Time:HH:mm:ss}");
        }

        private void Log(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            OnLog?.Invoke(line);
            System.Diagnostics.Debug.WriteLine(line);
        }
    }
}
