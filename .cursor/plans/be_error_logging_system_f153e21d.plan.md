---
name: BE Error Logging System
overview: Thêm hệ thống log file cho backend để theo dõi lỗi với thời gian, giờ, phút chính xác.
todos:
  - id: add-serilog
    content: Thêm Serilog.Sinks.File package vào GProject.csproj
    status: completed
  - id: setup-serilog
    content: Cấu hình Serilog logging trong Program.cs
    status: completed
  - id: update-apiserver-logging
    content: Thay Console.WriteLine bằng Log.Error/Warning trong GProjectApiServer.cs
    status: completed
  - id: add-datapool-logging
    content: Thêm logging vào DataPoolHelper.cs khi có exception
    status: completed
  - id: add-exception-handler
    content: Thêm global exception handler middleware
    status: completed
  - id: build-test
    content: Build và test
    status: completed
isProject: false
---

## Phân tích

Hiện tại BE chỉ log ra console (`Console.WriteLine`, custom `Log()` method), không có log file. Muốn theo dõi lỗi khi nào xảy ra cần:
1. Log file có timestamp
2. Phân biệt ERROR vs INFO
3. Xem log được dễ dàng

## Kế hoạch thực hiện

### 1. Thêm NuGet package Serilog.Sinks.File

```xml
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

File: `GProject/GProject.csproj`

### 2. Cập nhật Program.cs - cấu hình Serilog file logging

```csharp
// Thêm vào đầu file
using Serilog;

// Trong InitAsync()
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        @"C:\GProject\Logs\gproject-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30  // giữ 30 ngày log
    )
    .CreateLogger();

try
{
    Log.Information("GProject starting...");
    // ... existing code
    Log.Information("GProject started successfully on port {Port}", port);
}
catch (Exception ex)
{
    Log.Fatal(ex, "GProject terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### 3. Cập nhật GProjectApiServer.cs

- Inject `ILogger<GProjectApiServer>` hoặc dùng Serilog `Log` trực tiếp
- Thay `Console.WriteLine()` bằng `Log.Warning()`, `Log.Error()`
- Log request errors

### 4. Cập nhật DataPoolHelper.cs

- Inject logger hoặc dùng Serilog static
- Log khi có exception xảy ra

### 5. Thêm middleware exception handler

Global exception handling để catch tất cả unhandled errors:

```csharp
// Trong GProjectApiServer
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        Log.Error(ex, "Unhandled exception in request {Path}", context.Request.Path);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { success = false, message = "Internal server error" });
    });
});
```

## File cần sửa

1. `GProject/GProject.csproj` - thêm package reference
2. `GProject/Program.cs` - cấu hình Serilog
3. `GProject/GProjectApiServer.cs` - dùng Serilog thay Console
4. `GProject/DataPoolHelper/DataPoolHelper.cs` - thêm logging khi có exception

## Log output mẫu

```
2026-06-25 14:30:45.123 [ERR] Lỗi khi nhập mã từ file: File not found
2026-06-25 14:31:02.456 [INF] API request: GET /api/datapool/pools
2026-06-25 14:32:10.789 [ERR] Unhandled exception in request /api/datapool/add: ...
```

## Đọc log

Log file nằm ở: `C:\GProject\Logs\gproject-2026.06.25.log`

Có thể mở bằng Notepad hoặc dùng PowerShell:
```powershell
Get-Content "C:\GProject\Logs\gproject-*.log" -Tail 50
```