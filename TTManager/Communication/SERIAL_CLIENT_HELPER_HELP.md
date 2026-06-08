# Huong dan dung SerialClientHelper

Class nam tai:

```
TTManager/Communication/Serial_Client_Helper.cs
```

Namespace:

```csharp
using TTManager.Communication;
```

## 1. Tao client

```csharp
var serial = new SerialClientHelper("COM3", 9600);
```

Thuoc tinh mac dinh:

```csharp
PortName  = "COM1"
BaudRate  = 9600
Parity    = Parity.None
DataBits  = 8
StopBits  = StopBits.One
```

Thay doi thuoc tinh sau khi tao:

```csharp
serial.PortName = "COM5";
serial.BaudRate = 115200;
serial.Parity = Parity.None;
```

## 2. Bat callback

```csharp
serial.SerialClientCallback += (state, data) =>
{
    Console.WriteLine($"{state}: {data}");
};
```

Trang thai:

- `Connected`: ket noi thanh cong.
- `Received`: nhan duoc du lieu.
- `Disconnected`: mat ket noi.
- `Error`: xay ra loi.

## 3. Ket noi (Connect)

```csharp
serial.Connect();
```

Neu dang ket noi, `Connect()` se tu dong goi `Disconnect()` roi ket noi lai.

Kiem tra trang thai:

```csharp
if (serial.Connected)
{
    Console.WriteLine("Dang ket noi");
}
```

## 4. Ngat ket noi (Disconnect)

```csharp
serial.Disconnect();
```

## 5. Gui du lieu (SendAsync)

```csharp
bool ok = await serial.SendAsync("AT\r\n");
```

Tra ve `true` neu gui thanh cong, `false` neu chua ket noi hoac co loi.

## 6. Vi du WinForms

```csharp
private SerialClientHelper? _serial;

private void Form_Load(object sender, EventArgs e)
{
    _serial = new SerialClientHelper("COM3", 9600);
    _serial.SerialClientCallback += (state, data) =>
    {
        BeginInvoke(() =>
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} [{state}] {data}{Environment.NewLine}");
        });
    };
}

private void btnConnect_Click(object sender, EventArgs e)
{
    _serial?.Connect();
}

private void btnDisconnect_Click(object sender, EventArgs e)
{
    _serial?.Disconnect();
}

private async void btnSend_Click(object sender, EventArgs e)
{
    if (_serial == null || !_serial.Connected) return;
    await _serial.SendAsync(txtInput.Text + "\r\n");
}

private void Form_FormClosing(object sender, FormClosingEventArgs e)
{
    _serial?.Disconnect();
}
```

## 7. Loi thuong gap

### Port khong ton tai

```text
Connection failed: The port 'COM99' does not exist.
```

Kiem tra ten port tren Device Manager. Hoac lay danh sach port:

```csharp
string[] ports = SerialPort.GetPortNames();
foreach (string p in ports)
{
    Console.WriteLine(p);
}
```

### Port dang duoc su dung boi ung dung khac

```text
Connection failed: Access to the port 'COM3' is denied.
```

Dong ung dung khac dang su dung port do, hoac goi `Disconnect()` truoc khi ket noi lai.

### Mat ket noi tu dong

Co the do loi day, thiet bi tat, hoac loi tren duong truyen. Callback se thong bao:

```
Disconnected: Error receiving data: The port is closed.
```

Hoac:

```
Disconnected: Send failed: The port is closed.
```

Thu khong phuc: kiem tra day noi, nguon thiet bi, va cau hinh baud rate.

### Baud rate khong khop

Neu thiet bi nhan du lieu nhung bi loi hoac hien thi sai ky tu, kiem tra:

```csharp
serial.BaudRate = 115200;  // hoac 4800, 19200, ...
```

Dung baud rate dung voi thiet bi.

### Gui du lieu khong nhan phan hoi

Kiem tra:

1. Da ket noi chua: `serial.Connected`
2. Data co ky tu xuong dong `\r\n` hoac `\n` khong (nhieu thiet bi yeu cau)
3. Thiet bi co che do text hay binary (neu la binary, can gui `byte[]` thay vi `string`)

Hien tai `SendAsync` chi ho tro chuoi UTF-8. Neu can gui raw byte array, co the mo rong them:

```csharp
public async Task<bool> SendBytesAsync(byte[] data)
{
    if (data == null || _serialPort == null || !Connected)
        return false;
    try
    {
        await _serialPort.BaseStream.WriteAsync(data, 0, data.Length);
        return true;
    }
    catch (Exception ex)
    {
        HandleDisconnection($"Send bytes failed: {ex.Message}");
        return false;
    }
}
```
