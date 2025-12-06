# MTVS Admin Panel

Giao diện web dành cho developer để quản lý các phiên bản và gửi xuống client.

## Truy cập

Sau khi start server, truy cập:
```
http://localhost:3000/
```

## Tính năng

### 1. Releases Management
- ✅ Xem danh sách tất cả releases
- ✅ Tạo release mới (upload file)
- ✅ Xem chi tiết release
- ✅ Xóa release
- ✅ Filter theo product, channel, OS
- ✅ Download release files

### 2. Clients Management
- ✅ Xem danh sách clients đã đăng ký
- ✅ Xem thông tin client (version, OS, status)
- ✅ Filter theo product, site, status
- ✅ Xem last seen time

### 3. Backups Management
- ✅ Xem danh sách backups của client
- ✅ Download backup files
- ✅ Xóa backup
- ✅ Filter theo client ID, product

### 4. Events Log
- ✅ Xem log events từ clients
- ✅ Filter theo client ID, product, event type
- ✅ Xem chi tiết events (check, download, install, errors)

### 5. Statistics
- ✅ Tổng số releases
- ✅ Số clients đang active
- ✅ Tổng số backups
- ✅ Số events trong 24h

## Sử dụng

### Tạo Release mới

1. Click nút **"New Release"** ở góc trên bên phải
2. Điền thông tin:
   - Product: Tên sản phẩm (vd: MTVS)
   - Version: Phiên bản (vd: 1.0.0)
   - Channel: stable/beta/dev
   - OS: windows/linux/macos
   - Architecture: x64/x86/arm64
   - File: Chọn file nén (ZIP/RAR/7Z)
   - File Password: (tùy chọn) Mật khẩu file
   - Changelog: Mô tả thay đổi
3. Click **"Upload Release"**
4. File sẽ được upload lên Google Drive và lưu vào database

### Xem Release Details

1. Click icon **mắt** (👁️) ở cột Actions
2. Xem thông tin chi tiết và download link

### Xóa Release

1. Click icon **thùng rác** (🗑️) ở cột Actions
2. Confirm xóa
3. File sẽ bị xóa khỏi Google Drive và database

### Filter & Search

- Sử dụng các filter ở đầu mỗi trang để lọc dữ liệu
- Click **"Refresh"** để reload dữ liệu

## UI Features

- ✅ Responsive design (mobile-friendly)
- ✅ Modern, clean interface
- ✅ Real-time updates
- ✅ Toast notifications
- ✅ Loading states
- ✅ Error handling

## Security Notes

⚠️ **Lưu ý**: Admin panel hiện tại chưa có authentication. Nên thêm authentication trong production:

1. Thêm login page
2. Sử dụng JWT tokens
3. Protect admin routes
4. Rate limiting cho admin actions

## Customization

### Thay đổi màu sắc

Edit file `public/css/style.css` và thay đổi CSS variables:

```css
:root {
    --primary: #2563eb;
    --success: #10b981;
    --danger: #ef4444;
    /* ... */
}
```

### Thêm tính năng mới

1. Thêm route mới trong `routes/`
2. Thêm UI trong `public/index.html`
3. Thêm logic trong `public/js/app.js`

## Troubleshooting

### Không thấy giao diện

- Kiểm tra server đã start chưa
- Kiểm tra port có đúng không
- Xem console log của browser

### API errors

- Kiểm tra network tab trong browser DevTools
- Xem server logs
- Kiểm tra database connection

### Upload file lỗi

- Kiểm tra kích thước file (max 500MB)
- Kiểm tra Google Drive service account
- Xem server logs để biết lỗi chi tiết

