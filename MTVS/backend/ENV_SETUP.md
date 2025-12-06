# Hướng dẫn cấu hình .env

Tạo file `.env` trong thư mục `backend/` với nội dung sau:

```env
# Server Configuration
PORT=3000
NODE_ENV=development

# Database Configuration (cho Prisma)
# Cách 1: Dùng DATABASE_URL (khuyến nghị)
DATABASE_URL="mysql://user:password@localhost:3306/mtvs_db"

# Cách 2: Dùng biến riêng lẻ (nếu cần)
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=your_password
DB_NAME=mtvs_db

# Google Drive Service Account
GOOGLE_DRIVE_SERVICE_ACCOUNT_PATH=./config/service-account-key.json
GOOGLE_DRIVE_FOLDER_ID=your_google_drive_folder_id

# Security
JWT_SECRET=your_jwt_secret_key_here
API_KEY=your_api_key_here

# File Upload
MAX_FILE_SIZE=524288000
ALLOWED_FILE_TYPES=zip,rar,7z

# Backup Configuration
BACKUP_PASSWORD=your_default_backup_password
ENCRYPT_BACKUPS=true
```

## Giải thích các biến:

- **PORT**: Port chạy server (mặc định 3000)
- **DATABASE_URL**: Connection string cho Prisma (format: `mysql://user:password@host:port/database`)
- **DB_***: Thông tin kết nối MySQL (nếu không dùng DATABASE_URL)
- **GOOGLE_DRIVE_SERVICE_ACCOUNT_PATH**: Đường dẫn đến file JSON key của service account
- **GOOGLE_DRIVE_FOLDER_ID**: ID của folder trên Google Drive để lưu file
- **API_KEY**: API key để bảo mật (tùy chọn)
- **MAX_FILE_SIZE**: Kích thước file tối đa (bytes, mặc định 500MB)
- **BACKUP_PASSWORD**: Mật khẩu mặc định cho backup files

## Lưu ý

- ⚠️ **KHÔNG** commit file `.env` vào git
- ⚠️ File này chứa thông tin nhạy cảm
- ⚠️ Sử dụng `.env.example` làm template (nếu có)

