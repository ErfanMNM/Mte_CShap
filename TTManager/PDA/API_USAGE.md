# API Server - Hướng Dẫn Sử Dụng

## Tổng Quan

API Server cho phép các thiết bị PDA gửi mã Code đã quét về cho ứng dụng TTManager thông qua HTTP POST, hoặc dùng **giao diện web trực tiếp** trên trình duyệt. Server chạy trên port **6969**.

## Kiến Trúc

```
┌─────────┐   POST /api/scan    ┌──────────────┐   callback    ┌───────────────┐
│  PDA 1  │ ──────────────────▶│              │──────────────▶│ VNQRMainForm │
├─────────┤  http://IP:6969/    │  ApiServer   │               └───────────────┘
│  PDA 2  │ ◀── web UI ────────│  (port 6969) │  event        ┌───────────────┐
├─────────┤                    │              │──────────────▶│ PdaScanManager│
│  PDA N  │ ──────────────────▶│              │               └───────────────┘
└─────────┘                    └──────────────┘
                                    │
                              GET / (web UI)
                                    │
                              ┌─────┴─────┐
                              │ PDA Browser│
                              └───────────┘
```

- **ApiServer**: Web server ASP.NET Core chạy trên background thread, nhận request từ PDA.
- **PdaWebUI**: Giao diện web hiện đại embed trong assembly, truy cập qua trình duyệt.
- **PdaScanManager**: Quản lý lifecycle của server, cung cấp event và queue để UI tương tác.
- **VNQRMainForm**: WinForms form, lắng nghe scan qua event hoặc polling timer.

## Giao Diện Web Cho PDA

PDA có thể truy cập trực tiếp giao diện web bằng trình duyệt:

```
http://<IP_MAY_CHAY_TTMANAGER>:6969/
```

Ví dụ: nếu máy TTManager có IP `192.168.1.100`, PDA mở trình duyệt truy cập:

```
http://192.168.1.100:6969/
```

### Tính năng giao diện

- **Header** hiển thị số lượng scan: tổng, hôm nay, 24h qua
- **Trạng thái server** online/offline tự động kiểm tra mỗi 10 giây
- **Input mã Code** — nhập tay hoặc dùng bàn phím PDA
- **Nút Gửi** — gửi mã về server ngay lập tức
- **Phím tắt nhanh** — QC-OK, QC-NG, START, STOP
- **Lịch sử scan** — hiển thị các mã đã gửi gần nhất
- **Tên PDA** — lưu vào localStorage, không cần nhập lại

### Nhập mã thủ công

1. Mở trình duyệt trên PDA, truy cập `http://<IP>:6969/`
2. Nhập tên PDA (ví dụ: `PDA-KHO-01`) — chỉ cần làm 1 lần
3. Nhập mã Code vào ô, nhấn **Gửi** hoặc phím **Enter**
4. Mã được gửi về TTManager, feedback xác nhận hiển thị ngay

### Screenshot giao diện

```
┌──────────────────────────────────────┐
│  [=]  PDA Scanner        ● Online    │
│  ┌────────┬────────┬────────┐       │
│  │   42   │   12   │   38   │       │
│  │  Tong  │Hom nay │ 24h qua│       │
│  └────────┴────────┴────────┘       │
├──────────────────────────────────────┤
│  PDA Name: [PDA-KHO-01            ]  │
├──────────────────────────────────────┤
│  Ma Code                             │
│  ┌─────────────────────────┬──────┐  │
│  │ ABC123456789            │ Gui │  │
│  └─────────────────────────┴──────┘  │
│  [QC-OK] [QC-NG] [START] [STOP]     │
├──────────────────────────────────────┤
│  Lich su quet              [Xoa]     │
│  ┌──────────────────────────────┐    │
│  │ [=] ABC123456 @ 11:25:30    │    │
│  │     PDA-KHO-01              │    │
│  │ [=] DEF987654 @ 11:24:58    │    │
│  │     PDA-KHO-01              │    │
│  └──────────────────────────────┘    │
└──────────────────────────────────────┘
```

## Các Endpoint

### GET /

Trả về giao diện web cho PDA.

```http
GET http://<IP_MAY_CHAY_TTMANAGER>:6969/
```

### POST /api/scan

Nhận mã Code từ PDA gửi về.

**Request:**

```http
POST http://<IP_MAY_CHAY_TTMANAGER>:6969/api/scan
Content-Type: application/json

{
  "code": "1234567890",
  "pdaName": "PDA-KHO-01"
}
```

| Field     | Kiểu   | Bắt buộc | Mô tả                              |
|-----------|--------|----------|-------------------------------------|
| `code`    | string | Có       | Mã Code được quét (barcode/QR)     |
| `pdaName` | string | Không    | Tên thiết bị PDA (mặc định: `Unknown`) |

**Response thành công:**

```json
{
  "success": true,
  "message": "Scan received."
}
```

**Response lỗi:**

```json
{
  "success": false,
  "message": "Invalid payload: 'Code' is required."
}
```

### GET /api/health

Kiểm tra trạng thái server.

```http
GET http://<IP_MAY_CHAY_TTMANAGER>:6969/api/health
```

**Response:**

```json
{
  "status": "OK",
  "timestamp": "2026-06-23T11:25:00",
  "pendingScans": 3,
  "uptime": 120.5
}
```

### GET /api/stats

Thống kê nhanh.

```http
GET http://<IP_MAY_CHAY_TTMANAGER>:6969/api/stats
```

## Ví Dụ Gửi Từ PDA

### curl

```bash
curl -X POST http://192.168.1.100:6969/api/scan \
  -H "Content-Type: application/json" \
  -d '{"code":"ABC123456","pdaName":"PDA-LINE-01"}'
```

### JavaScript (trong ứng dụng PDA)

```javascript
async function sendScan(code, pdaName) {
    const response = await fetch('http://192.168.1.100:6969/api/scan', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            code: code,
            pdaName: pdaName || 'PDA-DEFAULT'
        })
    });
    const result = await response.json();
    console.log(result.message);
}
```

### Python

```python
import requests

url = "http://192.168.1.100:6969/api/scan"
payload = {"code": "ABC123456", "pdaName": "PDA-KHO-02"}

response = requests.post(url, json=payload)
print(response.json())
```

## Sử Dụng Trong Code

### Khởi động server (trong `TTMain.cs`)

```csharp
private readonly PdaScanManager _pdaManager = new();

// Trong Form_Load
await _pdaManager.StartAsync();
```

### Nhận scan qua event

```csharp
_pdaManager.OnScanReceived += scan => {
    // scan.Code      — mã vừa quét
    // scan.PdaName   — tên PDA
    // scan.Time      — thời gian quét
    Debug.WriteLine($"[{scan.PdaName}] {scan.Code}");
};
```

### Hoặc nhận scan qua polling

```csharp
private readonly Timer _scanPollTimer = new Timer { Interval = 500 };

private void PollScanQueue(object? sender, EventArgs e)
{
    while (true)
    {
        var scan = _pdaManager.DequeueScan();
        if (scan == null) break;

        // Xử lý scan
        Debug.WriteLine($"[{scan.PdaName}] {scan.Code}");
    }
}
```

### Dừng server (trong `Form_Closing`)

```csharp
protected override async void OnFormClosing(FormClosingEventArgs e)
{
    await _pdaManager.StopAsync();
    base.OnFormClosing(e);
}
```

## Cấu Hình Port

```csharp
// Port mặc định là 6969, có thể đổi:
await _pdaManager.StartAsync(port: 8080);
```

## Lưu Ý Quan Trọng

1. **Tường lửa**: Đảm bảo port 6969 (hoặc port bạn chọn) được mở trên máy chạy TTManager.
2. **Mạng LAN**: PDA và máy TTManager phải cùng mạng LAN/WiFi. Dùng IP nội bộ (VD: `192.168.x.x`), không dùng `localhost`.
3. **Thread safety**: Event `OnScanReceived` được gọi trên background thread của ASP.NET Core. Nếu cập nhật UI, dùng `Invoke()`:

   ```csharp
   _pdaManager.OnScanReceived += scan => {
       this.Invoke(() => {
           listBox1.Items.Add($"[{scan.PdaName}] {scan.Code}");
       });
   };
   ```
4. **Giao diện web**: PDA mở trình duyệt truy cập `http://<IP>:6969/` để sử dụng giao diện thay vì gọi API thủ công.
5. **Cùng máy**: Nếu PDA chạy trên cùng máy với TTManager, dùng `http://127.0.0.1:6969/api/scan` hoặc mở trình duyệt truy cập `http://127.0.0.1:6969/`.
6. **Font chữ**: Giao diện sử dụng Google Font `Inter`, cần có kết nối internet để tải font. Nếu PDA không có internet, font sẽ fallback về sans-serif mặc định.
