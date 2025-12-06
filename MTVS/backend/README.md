# MTVS Backend - Node.js + MySQL + Google Drive + Prisma

Backend API cho hệ thống quản lý phiên bản MTVS sử dụng Node.js, MySQL, Prisma ORM và Google Drive.

## Tính năng

- ✅ Quản lý releases (phiên bản)
- ✅ Quản lý clients (máy khách)
- ✅ Quản lý backups với mật khẩu
- ✅ Ghi log events
- ✅ Lưu trữ file trên Google Drive với service account
- ✅ Hỗ trợ file nén có mật khẩu (ZIP, RAR)
- ✅ **Prisma ORM** - Type-safe database client

## Yêu cầu

- Node.js >= 16.x
- MySQL >= 5.7
- Google Cloud Service Account với quyền Google Drive API

## Cài đặt

### 1. Cài đặt dependencies

```bash
cd backend
npm install
```

### 2. Cấu hình Environment

Tạo file `.env` (xem `ENV_SETUP.md`):

```env
# Database (cho Prisma)
DATABASE_URL="mysql://user:password@localhost:3306/mtvs_db"

# Hoặc dùng biến riêng lẻ
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=your_password
DB_NAME=mtvs_db

# Google Drive
GOOGLE_DRIVE_SERVICE_ACCOUNT_PATH=./config/service-account-key.json
GOOGLE_DRIVE_FOLDER_ID=your_folder_id

# Server
PORT=3000
NODE_ENV=development
```

### 3. Setup Database với Prisma

**Cách 1: Tự động (khuyến nghị)**
```bash
npm run setup-db
```

**Cách 2: Thủ công**
```bash
# Generate Prisma Client
npm run prisma:generate

# Tạo migration và apply
npm run prisma:migrate
```

### 4. Setup Google Drive

Xem hướng dẫn chi tiết trong `GOOGLE_DRIVE_SETUP.md`

### 5. Chạy server

Development:
```bash
npm run dev
```

Production:
```bash
npm start
```

## Prisma Commands

```bash
# Generate Prisma Client
npm run prisma:generate

# Tạo migration mới
npm run prisma:migrate

# Deploy migrations (production)
npm run prisma:deploy

# Mở Prisma Studio (GUI)
npm run prisma:studio
```

Xem `PRISMA_GUIDE.md` để biết chi tiết về Prisma.

## API Endpoints

### Releases

- `GET /api/releases/check` - Kiểm tra update mới
- `GET /api/releases` - Lấy danh sách releases
- `GET /api/releases/:id` - Lấy thông tin release
- `POST /api/releases` - Tạo release mới (upload file)

### Clients

- `POST /api/clients/register` - Đăng ký/cập nhật client
- `GET /api/clients/:clientId` - Lấy thông tin client
- `GET /api/clients` - Lấy danh sách clients
- `PUT /api/clients/:clientId/status` - Cập nhật trạng thái

### Backups

- `POST /api/backups` - Upload backup
- `GET /api/backups/:clientId` - Lấy danh sách backups
- `GET /api/backups/:clientId/:backupId` - Lấy thông tin backup
- `DELETE /api/backups/:backupId` - Xóa backup

### Events

- `POST /api/events` - Ghi event
- `GET /api/events/:clientId` - Lấy events của client
- `GET /api/events` - Lấy tất cả events

## Ví dụ sử dụng

### Tạo release mới

```bash
curl -X POST http://localhost:3000/api/releases \
  -F "file=@release.zip" \
  -F "product=MTVS" \
  -F "version=1.2.3" \
  -F "channel=stable" \
  -F "os=windows" \
  -F "arch=x64" \
  -F "filePassword=my_password" \
  -F "changelog=New features"
```

### Kiểm tra update

```bash
curl "http://localhost:3000/api/releases/check?product=MTVS&channel=stable&currentVersion=1.2.2&os=windows&arch=x64"
```

### Upload backup

```bash
curl -X POST http://localhost:3000/api/backups \
  -F "file=@backup.zip" \
  -F "clientId=client-123" \
  -F "product=MTVS" \
  -F "version=1.2.3" \
  -F "filePassword=backup_password"
```

## Cấu trúc thư mục

```
backend/
├── config/
│   └── database.js          # Prisma client config
├── prisma/
│   ├── schema.prisma        # Prisma schema
│   └── migrations/         # Migration files
├── routes/
│   ├── releases.js
│   ├── clients.js
│   ├── backups.js
│   └── events.js
├── services/
│   └── googleDriveService.js
├── scripts/
│   └── setup-database.js
├── uploads/                 # Temp uploads
├── .env
├── package.json
├── server.js
└── README.md
```

## Lưu ý

- File upload sẽ được lưu tạm trong `uploads/` và tự động xóa sau khi upload lên Google Drive
- Mật khẩu file được lưu trong database và description của file trên Google Drive
- Sử dụng HTTPS trong production
- Backup API key và service account key cẩn thận
- Không commit file `.env` và `service-account-key.json` vào git
- **Prisma Client** tự động generate sau khi chạy migrations

## Troubleshooting

### Lỗi kết nối MySQL
- Kiểm tra MySQL đang chạy
- Kiểm tra `DATABASE_URL` trong `.env`
- Kiểm tra quyền user MySQL

### Lỗi Prisma Client chưa generate
```bash
npm run prisma:generate
```

### Lỗi Google Drive
- Kiểm tra service account key file
- Kiểm tra Google Drive API đã được bật
- Kiểm tra folder đã được share với service account email

### Lỗi upload file
- Kiểm tra kích thước file không vượt quá `MAX_FILE_SIZE`
- Kiểm tra quyền ghi trong thư mục `uploads/`

## Tài liệu tham khảo

- [Prisma Guide](./PRISMA_GUIDE.md) - Hướng dẫn chi tiết về Prisma
- [Quick Start](./QUICK_START.md) - Hướng dẫn nhanh
- [Google Drive Setup](./GOOGLE_DRIVE_SETUP.md) - Setup Google Drive
- [Migration Guide](../MIGRATION_GUIDE.md) - Migration từ Appwrite

