using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TTManager.PDA
{
    /// <summary>
    /// Dữ liệu PDA gửi về khi quét mã Code.
    /// </summary>
    public class ScanData
    {
        public string Code { get; set; } = "";
        public DateTime Time { get; set; }
        public string PdaName { get; set; } = "";
    }

    /// <summary>
    /// Phản hồi từ server khi PDA gửi scan thành công.
    /// </summary>
    public class ScanResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Lớp chứa dữ liệu nhận khi PDA gửi POST request.
    /// Tách riêng để tránh confusion giữa request payload và domain model.
    /// </summary>
    public class ScanRequestPayload
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";
        [JsonPropertyName("pdaName")]
        public string PdaName { get; set; } = "";
    }

    /// <summary>
    /// API Server cho phép PDA gửi mã Code quét được về cho TTManager.
    /// </summary>
    public class ApiServer
    {
        private readonly Action<ScanData> _onScan;
        private readonly ILogger? _logger;
        private WebApplication? _app;
        private readonly int _port;
        private readonly string _host;

        /// <summary>
        /// Số lượng scan chưa xử lý (dùng để hiển thị badge trên UI).
        /// </summary>
        public int PendingCount => _pendingQueue.Count;

        private readonly ConcurrentQueue<ScanData> _pendingQueue = new();

        /// <summary>
        /// Tạo ApiServer với callback được gọi mỗi khi PDA gửi mã về.
        /// </summary>
        /// <param name="onScan">Callback xử lý mã Code, được gọi trên thread của request.</param>
        /// <param name="port">Port lắng nghe (mặc định 6969).</param>
        /// <param name="host">Host bind (mặc định 0.0.0.0).</param>
        /// <param name="logger">ILogger để ghi log. Nếu null, dùng Console.</param>
        public ApiServer(Action<ScanData> onScan, int port = 6969, string host = "0.0.0.0", ILogger? logger = null)
        {
            _onScan = onScan ?? throw new ArgumentNullException(nameof(onScan));
            _port = port;
            _host = host;
            _logger = logger;
        }

        /// <summary>
        /// Khởi động API Server một cách bất đồng bộ.
        /// Server chạy trên background thread.
        /// </summary>
        public async Task StartAsync()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost
                .UseUrls($"http://{_host}:{_port}")
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });

            _app = builder.Build();

            // Giao diện web cho PDA
            _app.MapGet("/", HandleIndex);

            // API endpoints
            _app.MapPost("/api/scan", HandleScan);
            _app.MapGet("/api/health", HandleHealth);
            _app.MapGet("/api/stats", HandleStats);

            Log("ApiServer", $"Started on http://{_host}:{_port}");
            Log("ApiServer", $"PDA UI available at http://{GetLocalIP()}:{_port}");
            await _app.StartAsync();
        }

        private string GetLocalIP()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return _host;
        }

        /// <summary>
        /// Dừng API Server và giải phóng tài nguyên.
        /// </summary>
        public async Task StopAsync()
        {
            if (_app != null)
            {
                Log("ApiServer", "Stopping...");
                await _app.StopAsync();
                _app = null;
                Log("ApiServer", "Stopped.");
            }
        }

        /// <summary>
        /// Lấy và xóa scan đầu tiên trong queue (FIFO).
        /// </summary>
        /// <returns>Dữ liệu scan, hoặc null nếu queue rỗng.</returns>
        public ScanData? DequeueScan()
        {
            return _pendingQueue.TryDequeue(out var scan) ? scan : null;
        }

        private IResult HandleIndex(HttpContext context)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            return Results.Text(PdaWebUI.Html, "text/html; charset=utf-8");
        }

        private IResult HandleScan(HttpContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = reader.ReadToEndAsync().GetAwaiter().GetResult();

                Log("ApiServer", $"Received scan payload: {body}");

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var payload = System.Text.Json.JsonSerializer.Deserialize<ScanRequestPayload>(body, options);

                if (payload == null || string.IsNullOrWhiteSpace(payload.Code))
                {
                    return Results.BadRequest(new ScanResponse
                    {
                        Success = false,
                        Message = "Invalid payload: 'Code' is required."
                    });
                }

                var scan = new ScanData
                {
                    Code = payload.Code.Trim(),
                    PdaName = string.IsNullOrWhiteSpace(payload.PdaName) ? "Unknown" : payload.PdaName.Trim(),
                    Time = DateTime.Now
                };

                _pendingQueue.Enqueue(scan);

                try
                {
                    _onScan(scan);
                }
                catch (Exception ex)
                {
                    Log("ApiServer", $"Callback error: {ex.Message}");
                    return Results.Json(new ScanResponse
                    {
                        Success = false,
                        Message = $"Callback error: {ex.Message}"
                    }, statusCode: 500);
                }

                Log("ApiServer", $"Scan queued: [{scan.PdaName}] {scan.Code}");

                return Results.Json(new ScanResponse
                {
                    Success = true,
                    Message = "Scan received."
                });
            }
            catch (Exception ex)
            {
                Log("ApiServer", $"Error handling scan: {ex.Message}");
                return Results.Json(new ScanResponse
                {
                    Success = false,
                    Message = $"Server error: {ex.Message}"
                }, statusCode: 500);
            }
        }

        private IResult HandleHealth(HttpContext context)
        {
            return Results.Json(new
            {
                Status = "OK",
                Timestamp = DateTime.Now,
                PendingScans = PendingCount,
                Uptime = (DateTime.Now - ProcessStartTime).TotalSeconds
            });
        }

        private IResult HandleStats(HttpContext context)
        {
            return Results.Json(new
            {
                PendingCount,
                QueueSize = _pendingQueue.Count
            });
        }

        private DateTime ProcessStartTime { get; } = DateTime.Now;

        private void Log(string source, string message)
        {
            var logLine = $"[{DateTime.Now:HH:mm:ss}] [{source}] {message}";
            if (_logger != null)
                _logger.LogInformation(logLine);
            else
                Console.WriteLine(logLine);
        }
    }

    /// <summary>
    /// Giao diện web hiện đại cho PDA — mã HTML/CSS/JS embed trong assembly.
    /// PDA truy cập http://&lt;IP&gt;:6969/ để sử dụng.
    /// </summary>
    internal static class PdaWebUI
    {
        public const string Html = """
<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no, maximum-scale=1.0">
  <title>PDA Scanner</title>
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
  <style>
    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

    :root {
      --primary: #4F46E5;
      --primary-dark: #4338CA;
      --primary-light: #EEF2FF;
      --success: #059669;
      --success-light: #D1FAE5;
      --error: #DC2626;
      --error-light: #FEE2E2;
      --warning: #D97706;
      --warning-light: #FEF3C7;
      --bg: #F8FAFC;
      --surface: #FFFFFF;
      --border: #E2E8F0;
      --text: #1E293B;
      --text-secondary: #64748B;
      --radius: 12px;
      --shadow: 0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06);
      --shadow-lg: 0 10px 15px -3px rgba(0,0,0,0.1), 0 4px 6px -2px rgba(0,0,0,0.05);
    }

    body {
      font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
      background: var(--bg);
      color: var(--text);
      min-height: 100dvh;
      display: flex;
      flex-direction: column;
    }

    /* ── Header ────────────────────────────────────────────── */
    .header {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white;
      padding: 20px 16px 16px;
      position: sticky;
      top: 0;
      z-index: 100;
    }

    .header-top {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 12px;
    }

    .app-title {
      font-size: 20px;
      font-weight: 700;
      letter-spacing: -0.3px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .app-title svg { width: 24px; height: 24px; }

    .status-badge {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 12px;
      font-weight: 600;
      background: rgba(255,255,255,0.2);
      padding: 4px 10px;
      border-radius: 20px;
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #4ADE80;
      animation: pulse-dot 2s infinite;
    }

    .status-dot.offline { background: #F87171; animation: none; }

    @keyframes pulse-dot {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.7; transform: scale(1.2); }
    }

    .header-stats {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 8px;
    }

    .stat-card {
      background: rgba(255,255,255,0.15);
      border-radius: var(--radius);
      padding: 10px;
      text-align: center;
      backdrop-filter: blur(4px);
    }

    .stat-value {
      font-size: 22px;
      font-weight: 700;
      line-height: 1;
      margin-bottom: 2px;
    }

    .stat-label {
      font-size: 10px;
      opacity: 0.85;
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    /* ── Main area ──────────────────────────────────────────── */
    .main {
      flex: 1;
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 16px;
      max-width: 480px;
      margin: 0 auto;
      width: 100%;
    }

    /* ── Scanner card ───────────────────────────────────────── */
    .scanner-card {
      background: var(--surface);
      border-radius: var(--radius);
      box-shadow: var(--shadow);
      overflow: hidden;
      border: 1px solid var(--border);
    }

    .scanner-header {
      padding: 14px 16px 10px;
      border-bottom: 1px solid var(--border);
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .scanner-title {
      font-size: 13px;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .scan-count {
      background: var(--primary-light);
      color: var(--primary);
      font-size: 11px;
      font-weight: 700;
      padding: 2px 8px;
      border-radius: 20px;
    }

    /* Manual input */
    .input-group {
      padding: 16px;
      border-bottom: 1px solid var(--border);
    }

    .input-row {
      display: flex;
      gap: 8px;
    }

    .code-input {
      flex: 1;
      padding: 14px 16px;
      font-size: 18px;
      font-family: 'Inter', monospace;
      font-weight: 600;
      border: 2px solid var(--border);
      border-radius: var(--radius);
      outline: none;
      transition: border-color 0.2s, box-shadow 0.2s;
      background: var(--bg);
      letter-spacing: 1px;
    }

    .code-input:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
    }

    .send-btn {
      padding: 14px 20px;
      background: var(--primary);
      color: white;
      border: none;
      border-radius: var(--radius);
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.15s, transform 0.1s;
      display: flex;
      align-items: center;
      gap: 6px;
      white-space: nowrap;
    }

    .send-btn:hover { background: var(--primary-dark); }
    .send-btn:active { transform: scale(0.97); }
    .send-btn:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }

    .send-btn svg { width: 18px; height: 18px; }

    /* Quick action buttons */
    .quick-actions {
      padding: 12px 16px;
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .quick-btn {
      padding: 8px 14px;
      border: 1.5px solid var(--border);
      border-radius: 20px;
      background: var(--surface);
      font-size: 13px;
      font-weight: 500;
      color: var(--text);
      cursor: pointer;
      transition: all 0.15s;
    }

    .quick-btn:hover {
      border-color: var(--primary);
      color: var(--primary);
      background: var(--primary-light);
    }

    .quick-btn:active { transform: scale(0.96); }

    /* ── Feedback toast ─────────────────────────────────────── */
    #feedback {
      position: fixed;
      top: 80px;
      left: 50%;
      transform: translateX(-50%) translateY(-20px);
      padding: 12px 20px;
      border-radius: var(--radius);
      font-size: 14px;
      font-weight: 600;
      box-shadow: var(--shadow-lg);
      opacity: 0;
      pointer-events: none;
      transition: opacity 0.25s, transform 0.25s;
      z-index: 999;
      white-space: nowrap;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    #feedback.show {
      opacity: 1;
      transform: translateX(-50%) translateY(0);
    }

    #feedback.success { background: var(--success); color: white; }
    #feedback.error { background: var(--error); color: white; }

    /* ── Recent scans ───────────────────────────────────────── */
    .section-title {
      font-size: 13px;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      padding: 0 4px;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .clear-btn {
      font-size: 12px;
      color: var(--error);
      background: none;
      border: none;
      cursor: pointer;
      font-weight: 600;
      font-family: inherit;
    }

    .scan-list {
      background: var(--surface);
      border-radius: var(--radius);
      box-shadow: var(--shadow);
      border: 1px solid var(--border);
      overflow: hidden;
      max-height: 340px;
      overflow-y: auto;
    }

    .scan-list::-webkit-scrollbar { width: 4px; }
    .scan-list::-webkit-scrollbar-track { background: transparent; }
    .scan-list::-webkit-scrollbar-thumb { background: var(--border); border-radius: 4px; }

    .scan-item {
      padding: 12px 16px;
      border-bottom: 1px solid var(--border);
      display: flex;
      align-items: center;
      gap: 12px;
      animation: slide-in 0.3s ease;
      transition: background 0.15s;
    }

    .scan-item:last-child { border-bottom: none; }
    .scan-item:hover { background: var(--bg); }

    @keyframes slide-in {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .scan-icon {
      width: 36px;
      height: 36px;
      border-radius: 10px;
      background: var(--primary-light);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .scan-icon svg { width: 18px; height: 18px; color: var(--primary); }

    .scan-info { flex: 1; min-width: 0; }

    .scan-code {
      font-size: 15px;
      font-weight: 700;
      font-family: 'Inter', monospace;
      color: var(--text);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      letter-spacing: 0.5px;
    }

    .scan-meta {
      font-size: 11px;
      color: var(--text-secondary);
      margin-top: 2px;
      display: flex;
      gap: 8px;
    }

    .scan-time { font-weight: 500; }
    .scan-pda { color: var(--primary); font-weight: 600; }

    .scan-check {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: var(--success-light);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .scan-check svg { width: 14px; height: 14px; color: var(--success); }

    .empty-state {
      padding: 40px 20px;
      text-align: center;
      color: var(--text-secondary);
    }

    .empty-state svg {
      width: 48px;
      height: 48px;
      margin: 0 auto 12px;
      opacity: 0.3;
    }

    .empty-state p { font-size: 14px; font-weight: 500; }
    .empty-state span { font-size: 12px; display: block; margin-top: 4px; opacity: 0.7; }

    /* ── PDA Name setup ─────────────────────────────────────── */
    .pda-name-section {
      background: var(--surface);
      border-radius: var(--radius);
      box-shadow: var(--shadow);
      border: 1px solid var(--border);
      padding: 14px 16px;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .pda-name-label {
      font-size: 12px;
      font-weight: 600;
      color: var(--text-secondary);
      white-space: nowrap;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .pda-name-input {
      flex: 1;
      padding: 8px 12px;
      border: 1.5px solid var(--border);
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      font-family: inherit;
      background: var(--bg);
      outline: none;
      transition: border-color 0.2s;
    }

    .pda-name-input:focus { border-color: var(--primary); }

    /* ── Footer ──────────────────────────────────────────────── */
    .footer {
      text-align: center;
      padding: 12px;
      font-size: 11px;
      color: var(--text-secondary);
      opacity: 0.6;
    }
  </style>
</head>
<body>

  <!-- Header -->
  <div class="header">
    <div class="header-top">
      <div class="app-title">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <rect x="2" y="3" width="20" height="14" rx="2"/><path d="M8 21h8M12 17v4"/>
          <path d="M9 8l2 2 4-4"/>
        </svg>
        PDA Scanner
      </div>
      <div class="status-badge">
        <div class="status-dot" id="serverDot"></div>
        <span id="serverStatus">Checking...</span>
      </div>
    </div>
    <div class="header-stats">
      <div class="stat-card">
        <div class="stat-value" id="totalScans">0</div>
        <div class="stat-label">Tổng</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" id="todayScans">0</div>
        <div class="stat-label">Hôm nay</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" id="last24h">0</div>
        <div class="stat-label">24h qua</div>
      </div>
    </div>
  </div>

  <!-- Main -->
  <div class="main">

    <!-- PDA Name -->
    <div class="pda-name-section">
      <div class="pda-name-label">PDA Name</div>
      <input class="pda-name-input" type="text" id="pdaNameInput" placeholder="VD: PDA-KHO-01" maxlength="50">
    </div>

    <!-- Scanner -->
    <div class="scanner-card">
      <div class="scanner-header">
        <div class="scanner-title">Mã Code</div>
        <div class="scan-count" id="scanCount">0 lần quét</div>
      </div>

      <div class="input-group">
        <div class="input-row">
          <input
            class="code-input"
            type="text"
            id="codeInput"
            placeholder="Nhập hoặc quét mã..."
            autocomplete="off"
            autocorrect="off"
            autocapitalize="off"
            spellcheck="false"
          >
          <button class="send-btn" id="sendBtn" onclick="sendScan()">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
              <path d="M5 12h14M12 5l7 7-7 7"/>
            </svg>
            Gửi
          </button>
        </div>
      </div>

      <div class="quick-actions">
        <button class="quick-btn" onclick="setInput('QC-OK')">QC-OK</button>
        <button class="quick-btn" onclick="setInput('QC-NG')">QC-NG</button>
        <button class="quick-btn" onclick="setInput('START')">START</button>
        <button class="quick-btn" onclick="setInput('STOP')">STOP</button>
        <button class="quick-btn" onclick="clearInput()">Xoá</button>
      </div>
    </div>

    <!-- Recent Scans -->
    <div class="section-title">
      Lịch sử quét
      <button class="clear-btn" onclick="clearHistory()">Xoá</button>
    </div>

    <div class="scan-list" id="scanList">
      <div class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
          <rect x="3" y="4" width="18" height="16" rx="2"/>
          <path d="M7 8h10M7 12h6"/>
        </svg>
        <p>Chưa có mã nào</p>
        <span>Nhập mã và nhấn Gửi để bắt đầu</span>
      </div>
    </div>

  </div>

  <div class="footer">TTManager PDA Interface</div>

  <!-- Feedback toast -->
  <div id="feedback"></div>

  <script>
    const codeInput = document.getElementById('codeInput');
    const sendBtn = document.getElementById('sendBtn');
    const pdaNameInput = document.getElementById('pdaNameInput');
    const scanList = document.getElementById('scanList');
    const feedback = document.getElementById('feedback');
    const scanCountEl = document.getElementById('scanCount');
    const totalScansEl = document.getElementById('totalScans');
    const todayScansEl = document.getElementById('todayScans');
    const last24hEl = document.getElementById('last24h');
    const serverDot = document.getElementById('serverDot');
    const serverStatus = document.getElementById('serverStatus');

    let totalScans = 0;
    let todayScans = 0;
    let last24hScans = 0;
    let feedbackTimer = null;

    // Load saved PDA name
    pdaNameInput.value = localStorage.getItem('pdaName') || '';

    pdaNameInput.addEventListener('change', () => {
      localStorage.setItem('pdaName', pdaNameInput.value);
    });

    // Auto-focus input on load
    codeInput.focus();

    // Enter key to send
    codeInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        sendScan();
      }
    });

    function setInput(val) {
      codeInput.value = val;
      codeInput.focus();
    }

    function clearInput() {
      codeInput.value = '';
      codeInput.focus();
    }

    async function sendScan() {
      const code = codeInput.value.trim();
      if (!code) {
        showFeedback('Vui lòng nhập mã Code!', 'error');
        codeInput.focus();
        return;
      }

      sendBtn.disabled = true;

      try {
        const res = await fetch('/api/scan', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            code: code,
            pdaName: pdaNameInput.value.trim() || 'PDA'
          })
        });

        const data = await res.json();

        if (data.success) {
          showFeedback(`Da gui: ${code}`, 'success');
          addScanItem(code, pdaNameInput.value.trim() || 'PDA');
          totalScans++;
          todayScans++;
          last24hScans++;
          updateStats();
          clearInput();
        } else {
          showFeedback(data.message || 'Loi gui!', 'error');
        }
      } catch (err) {
        showFeedback('Khong the ket noi server!', 'error');
        console.error(err);
      } finally {
        sendBtn.disabled = false;
      }
    }

    function addScanItem(code, pdaName) {
      // Remove empty state if present
      const empty = scanList.querySelector('.empty-state');
      if (empty) empty.remove();

      const now = new Date();
      const timeStr = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

      const item = document.createElement('div');
      item.className = 'scan-item';
      item.innerHTML = `
        <div class="scan-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 7V5a2 2 0 0 1 2-2h2"/>
            <path d="M17 3h2a2 2 0 0 1 2 2v2"/>
            <path d="M21 17v2a2 2 0 0 1-2 2h-2"/>
            <path d="M7 21H5a2 2 0 0 1-2-2v-2"/>
            <path d="M7 8h10M7 12h7"/>
          </svg>
        </div>
        <div class="scan-info">
          <div class="scan-code">${escapeHtml(code)}</div>
          <div class="scan-meta">
            <span class="scan-time">${timeStr}</span>
            <span class="scan-pda">${escapeHtml(pdaName)}</span>
          </div>
        </div>
        <div class="scan-check">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
            <path d="M20 6L9 17l-5-5"/>
          </svg>
        </div>
      `;

      scanList.insertBefore(item, scanList.firstChild);

      // Keep max 50 items
      const items = scanList.querySelectorAll('.scan-item');
      if (items.length > 50) items[items.length - 1].remove();

      scanCountEl.textContent = items.length + ' lan quet';
    }

    function updateStats() {
      totalScansEl.textContent = totalScans;
      todayScansEl.textContent = todayScans;
      last24hEl.textContent = last24hScans;
    }

    function clearHistory() {
      scanList.innerHTML = `
        <div class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <rect x="3" y="4" width="18" height="16" rx="2"/>
            <path d="M7 8h10M7 12h6"/>
          </svg>
          <p>Chua co ma nao</p>
          <span>Nhap ma va nhan Gui de bat dau</span>
        </div>
      `;
      totalScans = 0;
      todayScans = 0;
      last24hScans = 0;
      scanCountEl.textContent = '0 lan quet';
      updateStats();
    }

    function showFeedback(msg, type) {
      feedback.textContent = msg;
      feedback.className = type + ' show';
      if (feedbackTimer) clearTimeout(feedbackTimer);
      feedbackTimer = setTimeout(() => { feedback.className = ''; }, 2200);
    }

    function escapeHtml(str) {
      const div = document.createElement('div');
      div.textContent = str;
      return div.innerHTML;
    }

    // Check server health periodically
    async function checkHealth() {
      try {
        const res = await fetch('/api/health');
        if (res.ok) {
          serverDot.classList.remove('offline');
          serverStatus.textContent = 'Online';
        } else {
          serverDot.classList.add('offline');
          serverStatus.textContent = 'Offline';
        }
      } catch {
        serverDot.classList.add('offline');
        serverStatus.textContent = 'Offline';
      }
    }

    setInterval(checkHealth, 10000);
    checkHealth();

    // Reset today counter at midnight
    function checkDayChange() {
      const today = new Date().toDateString();
      if (localStorage.getItem('scanDate') !== today) {
        todayScans = 0;
        localStorage.setItem('scanDate', today);
        updateStats();
      }
    }
    checkDayChange();
    setInterval(checkDayChange, 60000);
  </script>

</body>
</html>
""";
    }
}
