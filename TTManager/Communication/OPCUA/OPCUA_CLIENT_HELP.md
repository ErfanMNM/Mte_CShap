# Hướng dẫn dùng thư viện OPC UA Client

File này hướng dẫn dùng class `OpcUaClientHelper` trong code C#.

Class nằm tại:

```text
TTManager/Communication/OPCUA/OpcUa_Client_Helper.cs
```

Namespace:

```csharp
using TTManager.Communication.OPCUA;
```

## 1. Chuẩn bị project

Project cần package:

```xml
<PackageReference Include="nauful-LibUA-core" Version="1.0.39" />
```

Nếu dùng từ project khác, cần reference project chứa file `OpcUaClientHelper`, hoặc copy thư mục `Communication/OPCUA` qua project cần dùng.

## 2. Tạo client

```csharp
using TTManager.Communication.OPCUA;

var opcClient = new OpcUaClientHelper("opc.tcp://127.0.0.1:4840");
```

Endpoint hợp lệ có dạng:

```text
opc.tcp://127.0.0.1:4840
opc.tcp://192.168.1.10:4840
opc.tcp://192.168.1.20:4840/UA/Server
```

## 3. Bắt sự kiện callback

```csharp
opcClient.OpcUaClientCallback += (state, data) =>
{
    Console.WriteLine($"{state}: {data}");
};
```

Các trạng thái:

```csharp
OpcUaClientState.Connected
OpcUaClientState.Disconnected
OpcUaClientState.Received
OpcUaClientState.Error
```

Ý nghĩa:

- `Connected`: kết nối thành công.
- `Disconnected`: đã ngắt kết nối hoặc server đóng kết nối.
- `Received`: đọc được dữ liệu từ node.
- `Error`: lỗi connect/read/write.

## 4. Kết nối server

```csharp
await opcClient.ConnectAsync();
```

Kiểm tra trạng thái:

```csharp
if (opcClient.Connected)
{
    Console.WriteLine("OPC UA connected");
}
```

Mặc định helper dùng:

```text
Anonymous login
MessageSecurityMode.None
SecurityPolicy.None
```

## 5. Đọc node

Đọc `DataValue` đầy đủ:

```csharp
DataValue? dataValue = await opcClient.ReadAsync("ns=2;s=Tag1");

if (dataValue != null)
{
    Console.WriteLine(dataValue.Value);
}
```

Đọc value trực tiếp:

```csharp
object? value = await opcClient.ReadValueAsync("ns=2;s=Tag1");
Console.WriteLine(value);
```

Ví dụ `NodeId`:

```text
ns=2;s=Tag1
ns=2;s=PLC.Temperature
ns=3;i=1001
```

## 6. Ghi node

Ghi số nguyên:

```csharp
bool ok = await opcClient.WriteAsync("ns=2;s=Tag1", 123);
```

Ghi chuỗi:

```csharp
bool ok = await opcClient.WriteAsync("ns=2;s=ProductCode", "ABC123");
```

Ghi số thực:

```csharp
bool ok = await opcClient.WriteAsync("ns=2;s=Temperature", 36.5);
```

Ghi boolean:

```csharp
bool ok = await opcClient.WriteAsync("ns=2;s=Start", true);
```

Kiểm tra kết quả:

```csharp
if (!ok)
{
    Console.WriteLine("Write failed");
}
```

## 7. Ngắt kết nối

```csharp
await opcClient.DisconnectAsync();
```

Nếu dùng lâu trong app, nên gọi `Dispose()` khi đóng form/service:

```csharp
opcClient.Dispose();
```

## 8. Ví dụ đầy đủ trong Console/App

```csharp
using TTManager.Communication.OPCUA;

var opcClient = new OpcUaClientHelper("opc.tcp://127.0.0.1:4840");

opcClient.OpcUaClientCallback += (state, data) =>
{
    Console.WriteLine($"[{state}] {data}");
};

await opcClient.ConnectAsync();

if (opcClient.Connected)
{
    object? oldValue = await opcClient.ReadValueAsync("ns=2;s=Tag1");
    Console.WriteLine($"Old value: {oldValue}");

    bool writeOk = await opcClient.WriteAsync("ns=2;s=Tag1", 456);
    Console.WriteLine($"Write OK: {writeOk}");

    object? newValue = await opcClient.ReadValueAsync("ns=2;s=Tag1");
    Console.WriteLine($"New value: {newValue}");
}

await opcClient.DisconnectAsync();
opcClient.Dispose();
```

## 9. Ví dụ dùng trong WinForms

Khai báo field trong form:

```csharp
private OpcUaClientHelper? _opcClient;
```

Connect button:

```csharp
private async void btnConnect_Click(object sender, EventArgs e)
{
    _opcClient = new OpcUaClientHelper(txtEndpoint.Text.Trim());

    _opcClient.OpcUaClientCallback += (state, data) =>
    {
        BeginInvoke(() => txtLog.AppendText($"{state}: {data}{Environment.NewLine}"));
    };

    await _opcClient.ConnectAsync();
}
```

Read button:

```csharp
private async void btnRead_Click(object sender, EventArgs e)
{
    if (_opcClient == null || !_opcClient.Connected) return;

    object? value = await _opcClient.ReadValueAsync(txtNodeId.Text.Trim());
    txtReadValue.Text = value?.ToString() ?? string.Empty;
}
```

Write button:

```csharp
private async void btnWrite_Click(object sender, EventArgs e)
{
    if (_opcClient == null || !_opcClient.Connected) return;

    bool ok = await _opcClient.WriteAsync(txtNodeId.Text.Trim(), txtWriteValue.Text);
    txtLog.AppendText($"Write: {ok}{Environment.NewLine}");
}
```

Close form:

```csharp
private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
{
    if (_opcClient != null)
    {
        await _opcClient.DisconnectAsync();
        _opcClient.Dispose();
    }
}
```

## 10. Lỗi thường gặp

### Sai endpoint

Lỗi thường thấy:

```text
Connection failed: Invalid endpoint url
EndpointUrl must use opc.tcp scheme.
```

Cách sửa: endpoint phải bắt đầu bằng `opc.tcp://`.

### Sai NodeId

Lỗi thường thấy:

```text
Read failed: Invalid node id
Write failed: Invalid node id
```

Cách sửa: dùng đúng format `ns=...;s=...` hoặc `ns=...;i=...`.

### Server không cho ghi

Lỗi thường thấy:

```text
Write failed: BadUserAccessDenied
Write failed: BadNotWritable
```

Cách sửa: kiểm tra quyền ghi, tài khoản, hoặc cấu hình node trên server.

### Sai kiểu dữ liệu

Ví dụ node là `Int32` nhưng truyền chuỗi:

```csharp
await opcClient.WriteAsync("ns=2;s=Tag1", "123");
```

Nên truyền đúng kiểu:

```csharp
await opcClient.WriteAsync("ns=2;s=Tag1", 123);
```

## 11. Giới hạn hiện tại

`OpcUaClientHelper` hiện hỗ trợ:

- Connect anonymous.
- Security `None`.
- Read value.
- Write value.
- Callback trạng thái.
- Disconnect/Dispose.

Chưa hỗ trợ sẵn:

- Username/password.
- Certificate login.
- Security `Sign` hoặc `SignAndEncrypt`.
- Subscribe/monitor realtime.
- Browse node tree.

Nếu cần, có thể mở rộng thêm các hàm này trong `OpcUaClientHelper`.
