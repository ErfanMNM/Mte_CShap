# Hướng dẫn Setup Appwrite cho MTVS

## Cách 1: Sử dụng Script tự động (Khuyến nghị)

### Bước 1: Cài đặt Node.js
Đảm bảo bạn đã cài đặt Node.js (version 14 trở lên)

### Bước 2: Cài đặt dependencies
```bash
cd MTVS
npm install
```

### Bước 3: Kiểm tra cấu hình
Mở file `AppConfig.json.example` và đảm bảo đã điền đầy đủ:
- `Endpoint`: URL Appwrite server
- `ProjectId`: ID của project
- `ApiKey`: API Key có quyền admin

### Bước 4: Chạy script setup
```bash
npm run setup
```

Script sẽ tự động:
- ✅ Tạo Database "main" (nếu chưa có)
- ✅ Tạo 5 Collections với đầy đủ attributes và indexes:
  - `releases` - Quản lý các phiên bản release
  - `clients` - Thông tin các client đang sử dụng
  - `rolloutPolicies` - Chính sách rollout
  - `backups` - Thông tin các bản backup
  - `events` - Log các sự kiện
- ✅ Tạo 3 Storage Buckets:
  - `artifacts` - Lưu trữ các file artifact (zip, exe, msi...)
  - `backups` - Lưu trữ các bản backup
  - `manifests` - Lưu trữ các file manifest JSON

## Cách 2: Setup thủ công qua Appwrite Console

Nếu không muốn dùng script, bạn có thể tạo thủ công theo hướng dẫn trong file `AppwriteConfig.txt`

## Lưu ý

1. **API Key**: Cần sử dụng API Key có quyền **admin** (Server/Admin API Key)
2. **Attributes**: Một số loại dữ liệu trong Appwrite được lưu dưới dạng string:
   - Datetime: Lưu dưới dạng ISO string (ví dụ: "2024-01-01T00:00:00Z")
   - Boolean: Lưu dưới dạng string "true" hoặc "false"
   - Integer: Lưu dưới dạng string
3. **Indexing**: Sau khi tạo attributes, cần đợi vài giây để Appwrite index hoàn toàn
4. **Permissions**: Script không tự động set permissions. Bạn cần set permissions thủ công trong Appwrite Console:
   - Collections: Cho phép authenticated users đọc/ghi
   - Storage Buckets: Cho phép authenticated users đọc (artifacts, manifests) hoặc đọc/ghi (backups)

## Troubleshooting

### Lỗi: "Collection already exists"
- Không sao, script sẽ bỏ qua và tiếp tục

### Lỗi: "Attribute already exists"
- Không sao, script sẽ bỏ qua và tiếp tục

### Lỗi: "Invalid API Key"
- Kiểm tra lại API Key trong `AppConfig.json.example`
- Đảm bảo API Key có quyền admin

### Lỗi: "Project not found"
- Kiểm tra lại ProjectId trong `AppConfig.json.example`
- Đảm bảo ProjectId đúng

## Sau khi setup

1. Tạo file `AppConfig.json` từ `AppConfig.json.example`
2. Điền thông tin cấu hình
3. Build và chạy ứng dụng MTVS

