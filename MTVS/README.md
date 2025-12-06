# MTVS - Version Manager

Ứng dụng quản lý phiên bản sử dụng Appwrite làm backend.

## Tính năng

- ✅ Kiểm tra phiên bản mới từ Appwrite
- ✅ Tải và cài đặt cập nhật (chỉ khi người dùng click)
- ✅ Tự động backup trước khi cập nhật
- ✅ Khôi phục (rollback) từ backup nếu có lỗi
- ✅ Health check sau khi cập nhật
- ✅ Hỗ trợ hooks (pre-install, post-install, healthcheck)
- ✅ Quản lý nhiều loại component (exe, service, config, etc.)
- ✅ Hỗ trợ channels (stable/beta/dev)
- ✅ Báo cáo events về server

## Cài đặt

### 1. Cấu hình Appwrite

Xem file `AppwriteConfig.txt` để biết cách thiết lập:
- Database collections
- Storage buckets
- API keys

### 2. Cấu hình ứng dụng

1. Copy `AppConfig.json.example` thành `AppConfig.json`
2. Điền thông tin Appwrite:
   - Endpoint
   - ProjectId
   - ApiKey
3. Cấu hình Client:
   - Product: Tên sản phẩm
   - Site: Tên site/chi nhánh
   - Tenant: Tên tenant/khách hàng
   - UpdateChannel: "stable", "beta", hoặc "dev"
   - InstallPath: Đường dẫn cài đặt ứng dụng
   - BackupPath: Đường dẫn lưu backup

### 3. Build và chạy

```bash
dotnet build
dotnet run
```

## Sử dụng

1. **Kiểm tra cập nhật**: Click nút "Kiểm tra cập nhật"
2. **Cập nhật**: Nếu có phiên bản mới, click "Cập nhật"
3. **Khôi phục**: Nếu cần, click "Khôi phục" để rollback về bản backup

## Cấu trúc Manifest

Manifest mô tả cách cài đặt update:

```json
{
  "version": "1.2.3",
  "product": "MTVS",
  "channel": "stable",
  "components": [
    {
      "type": "exe",
      "name": "MTVS.exe",
      "sourcePath": "MTVS.exe",
      "targetPath": "MTVS.exe",
      "preserve": [],
      "hooks": {
        "preInstall": "stop_service.bat",
        "postInstall": "start_service.bat",
        "healthcheck": "healthcheck.exe"
      }
    }
  ],
  "backup": {
    "paths": ["appsettings.json", "Data"],
    "exclude": ["*.log", "temp"]
  },
  "policy": {
    "requireUserClick": true,
    "maintenanceWindow": {
      "start": "02:00",
      "end": "04:00"
    }
  }
}
```

## Quy trình Release

1. Build ứng dụng
2. Tạo manifest.json
3. Ký số artifact (SHA256)
4. Upload artifact và manifest lên Appwrite Storage
5. Tạo release record trong Database collection "releases"
6. Client sẽ tự động phát hiện và cho phép update

## Bảo mật

- Sử dụng signed URLs cho tất cả download
- Verify hash trước khi cài đặt
- Backup được mã hóa trước khi upload (tùy chọn)
- Events được log để audit

## Troubleshooting

### Lỗi kết nối Appwrite
- Kiểm tra Endpoint, ProjectId, ApiKey trong AppConfig.json
- Kiểm tra network/firewall

### Lỗi download
- Kiểm tra signed URL có hợp lệ không
- Kiểm tra quyền Storage bucket

### Lỗi cài đặt
- Kiểm tra quyền ghi vào InstallPath
- Kiểm tra disk space
- Xem log trong textBoxLog

## License

Xem LICENSE.txt

