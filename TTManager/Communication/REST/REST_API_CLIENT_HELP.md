# Hướng dẫn dùng RestApiClientHelper

Class nằm tại:

```text
TTManager/Communication/REST/Rest_Api_Client_Helper.cs
```

Namespace:

```csharp
using TTManager.Communication.REST;
```

## 1. Tạo client

```csharp
var api = new RestApiClientHelper("https://jsonplaceholder.typicode.com");
```

Hoặc dùng URL đầy đủ khi gọi:

```csharp
var api = new RestApiClientHelper();
RestApiResponse res = await api.GetAsync("https://example.com/api/status");
```

## 2. Bắt callback

```csharp
api.RestApiClientCallback += (state, data) =>
{
    Console.WriteLine($"{state}: {data}");
};
```

Trạng thái:

- `Connected`: client sẵn sàng.
- `Sent`: đã gửi request.
- `Received`: nhận response thành công HTTP 2xx.
- `Error`: lỗi hoặc HTTP không phải 2xx.
- `Disconnected`: đã dispose client.

## 3. GET

```csharp
RestApiResponse response = await api.GetAsync("/posts/1");

if (response.Success)
{
    Console.WriteLine(response.Content);
}
else
{
    Console.WriteLine($"GET failed: {response.StatusCode} {response.ReasonPhrase}");
}
```

Deserialize JSON trực tiếp:

```csharp
PostDto? post = await api.GetJsonAsync<PostDto>("/posts/1");
```

## 4. POST JSON

```csharp
var body = new
{
    title = "hello",
    body = "test content",
    userId = 1
};

RestApiResponse response = await api.PostAsync("/posts", body);
```

Nếu muốn gửi JSON string sẵn:

```csharp
string json = "{\"title\":\"hello\",\"userId\":1}";
RestApiResponse response = await api.PostAsync("/posts", json);
```

Deserialize JSON response:

```csharp
PostDto? created = await api.PostJsonAsync<PostDto>("/posts", body);
```

## 5. PUT/PATCH/DELETE

```csharp
await api.PutAsync("/posts/1", new { title = "updated" });
await api.PatchAsync("/posts/1", new { title = "patched" });
await api.DeleteAsync("/posts/1");
```

DELETE có body nếu API yêu cầu:

```csharp
await api.DeleteAsync("/items/1", new { reason = "test" });
```

## 6. Header mặc định

Header áp dụng cho mọi request:

```csharp
api.DefaultHeaders["X-Device-Id"] = "MACHINE-01";
api.DefaultHeaders["X-App-Name"] = "TTManager";
```

Header riêng cho 1 request:

```csharp
var headers = new Dictionary<string, string>
{
    ["X-Request-Id"] = Guid.NewGuid().ToString()
};

RestApiResponse response = await api.GetAsync("/status", headers);
```

## 7. Bearer token

```csharp
api.BearerToken = "your_token_here";
RestApiResponse response = await api.GetAsync("/secure/data");
```

Helper tự gửi header:

```text
Authorization: Bearer your_token_here
```

## 8. Basic auth

```csharp
api.BasicUsername = "admin";
api.BasicPassword = "123456";

RestApiResponse response = await api.GetAsync("/secure/data");
```

Nếu có cả `BearerToken` và `BasicUsername`, helper ưu tiên `BearerToken`.

## 9. Timeout

Mặc định timeout 30 giây.

```csharp
api.TimeoutSeconds = 10;
```

Nếu quá thời gian, response trả:

```csharp
response.Success == false
response.StatusCode == 0
response.ReasonPhrase == "Request timeout after 10 seconds"
```

## 10. Ví dụ WinForms

```csharp
private RestApiClientHelper? _api;

private void Form_Load(object sender, EventArgs e)
{
    _api = new RestApiClientHelper("https://jsonplaceholder.typicode.com");
    _api.RestApiClientCallback += (state, data) =>
    {
        BeginInvoke(() => txtLog.AppendText($"{state}: {data}{Environment.NewLine}"));
    };
}

private async void btnGet_Click(object sender, EventArgs e)
{
    if (_api == null) return;

    RestApiResponse response = await _api.GetAsync("/posts/1");
    txtResponse.Text = response.Content;
}

private async void btnPost_Click(object sender, EventArgs e)
{
    if (_api == null) return;

    var body = new { title = "abc", body = "hello", userId = 1 };
    RestApiResponse response = await _api.PostAsync("/posts", body);
    txtResponse.Text = response.Content;
}

private void Form_FormClosing(object sender, FormClosingEventArgs e)
{
    _api?.Dispose();
}
```

## 11. DTO mẫu

```csharp
public sealed class PostDto
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
```

Dùng:

```csharp
PostDto? post = await api.GetJsonAsync<PostDto>("/posts/1");
```

## 12. Lỗi thường gặp

### BaseUrl rỗng

```text
BaseUrl is empty and endpoint is not absolute.
```

Cách sửa:

```csharp
api.BaseUrl = "https://example.com";
```

Hoặc truyền URL đầy đủ:

```csharp
await api.GetAsync("https://example.com/api/status");
```

### HTTP 401 Unauthorized

Kiểm tra token/basic auth:

```csharp
api.BearerToken = "token";
```

### HTTP 415 Unsupported Media Type

Helper gửi body với `Content-Type: application/json`.
Nếu API cần `form-data` hoặc `x-www-form-urlencoded`, cần mở rộng thêm hàm riêng.

### SSL certificate lỗi

Nếu server dùng certificate self-signed, cần cấu hình `HttpClientHandler` riêng. Helper hiện chưa bypass certificate vì không an toàn.
