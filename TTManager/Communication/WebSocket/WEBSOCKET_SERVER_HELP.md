# Huong dan dung WebSocketServerHelper

Class nam tai:

```
TTManager/Communication/WebSocket/WebSocketServerHelper.cs
```

Namespace:

```csharp
using TTManager.Communication.WebSocket;
```

## 1. Tao server

```csharp
var server = new WebSocketServerHelper();
```

Hoac cau hinh port va path ngay khi tao:

```csharp
var server = new WebSocketServerHelper(9000, "/machine");
```

Co the cau hinh buffer size (mac dinh 4096 bytes):

```csharp
var server = new WebSocketServerHelper(9000, "/machine", 8192);
```

Cau hinh thuoc tinh:

```csharp
server.Port = 9000;
server.Path = "/machine";
```

## 2. Bat callback

**Quan trong:** Callback tu dong chay tren dung thread cua caller (UI thread neu dang o WinForms).

```csharp
server.WebSocketServerCallback += (state, data) =>
{
    // Khong can Invoke/BeginInvoke nua vi da tu dong post ve UI thread
    listBox1.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} [{state}] {data}");
};
```

Trang thai:

- `Listening`: server da bat va dang lang nghe.
- `Connected`: co client moi ket noi.
- `Received`: nhan duoc message tu client.
- `Disconnected`: client ngat ket noi hoac server da tat.
- `Error`: xay ra loi.

## 3. Bat server (Start)

```csharp
server.Start();
```

Sau khi goi `Start()`, server se lang nghe tai `http://localhost:{Port}/{Path}/`.

Neu server dang chay, `Start()` se tu dong goi `Stop()` roi bat lai.

**Luu y:** `Start()` se throw exception neu that bai (vi du: port bi chiem).

## 4. Tat server (Stop)

```csharp
server.Stop();
```

Ngat tat ca client dang ket noi va dong listener.

## 5. Nhan message (callback)

Khi co message den, callback se nhan duoc chuoi voi dinh dang:

```
[ClientId] Noi dung message
```

Vi du:

```csharp
server.WebSocketServerCallback += (state, data) =>
{
    if (state == WebSocketServerState.Received)
    {
        // data = "[3f14b2c1-...] Hello from client"
        // Tu dong ve UI thread, khong can Invoke
    }
};
```

## 6. Gui message den client cu the

```csharp
bool ok = await server.SendToClientAsync(clientId, "traloi tu server");
```

Tra ve `true` neu gui thanh cong, `false` neu client khong con ket noi.

## 7. Broadcast message

Gui message den TAT CA client dang ket noi:

```csharp
await server.BroadcastAsync("Thong bao den moi client");
```

Hoac gui binary:

```csharp
byte[] data = Encoding.UTF8.GetBytes("binary data");
await server.BroadcastAsync(data);
```

## 8. Quan ly client

Lay danh sach client dang ket noi:

```csharp
IReadOnlyCollection<Guid> clients = server.GetConnectedClients();
foreach (Guid id in clients)
{
    Console.WriteLine(id);
}
```

So luong client:

```csharp
int count = server.ClientCount;
```

Ngat ket noi mot client cu the (async):

```csharp
await server.DisconnectClientAsync(clientId);
```

Hoac sync version:

```csharp
server.DisconnectClient(clientId);
```

## 9. Vi du WinForms day du

```csharp
using TTManager.Communication.WebSocket;

public partial class MainForm : Form
{
    private WebSocketServerHelper? _server;

    public MainForm()
    {
        InitializeComponent();
    }

    private void Form_Load(object sender, EventArgs e)
    {
        try
        {
            _server = new WebSocketServerHelper(8080, "/ws");
            _server.WebSocketServerCallback += Server_WebSocketServerCallback;
            _server.Start();
            lblStatus.Text = "Server running";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start server: {ex.Message}");
        }
    }

    private void Server_WebSocketServerCallback(WebSocketServerState state, string data)
    {
        // Tu dong chay tren UI thread - khong can Invoke!
        listBox1.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} [{state}] {data}");
    }

    private async void btnBroadcast_Click(object sender, EventArgs e)
    {
        if (_server == null) return;
        await _server.BroadcastAsync(txtMessage.Text);
        txtMessage.Clear();
    }

    private void Form_FormClosing(object sender, FormClosingEventArgs e)
    {
        _server?.Dispose();
    }
}
```

## 10. Loi thuong gap

### Server khong the bat tren port

```text
Failed to start server: Access denied
```

Kiem tra port da bi chiem chua chua:

```powershell
netstat -ano | findstr :8080
```

Doi sang port khac:

```csharp
server.Port = 9090;
server.Start();
```

### HttpListener can quyen admin tren Windows

Tren Windows, `HttpListener` can dang ky URL namespace. Neu khong co quyen, co the loi:

```text
Access denied
```

Giai phap: chay ung dung voi quyen admin, hoac dam bao uws dien dan.

### Client khong the ket noi

Kiem tra URL client:

```
ws://localhost:8080/ws/
```

Neu client chay tren may khac, thay `localhost` bang dia chi IP cua server.

### Client bi ngat ket noi

Server tu dong loai bo client chet khi broadcast. Callback se thong bao:

```
Disconnected: Removed 1 dead client(s) | Active: 0
```

### Su dung khi nao nen chon WebSocket thay vi REST API

- Giao tiep real-time, 2 chieu (server co the day data den client bat ky luc nao).
- Can nhanh, latency thap (WebSocket khong co HTTP overhead).
- Client la trinh duyet web, mobile app, hoac IoT device.
- Neu chi can request-response don gian, dung `RestApiClientHelper` van la uu tien.

## 11. Thread Safety

Thư viện đã được cải tiến với:

- **Callback thread-safe**: Tự động post về UI thread nếu có SynchronizationContext.
- **Client management thread-safe**: Sử dụng `ConcurrentDictionary`.
- **Dispose-safe**: Kiểm tra `_disposed` trước khi thực hiện.
- **Graceful shutdown**: Đóng socket đúng cách trước khi dispose.
- **Timeout cho close**: Tránh deadlock khi đóng socket.
- **Nullable reference**: Hỗ trợ đầy đủ nullable reference types.
