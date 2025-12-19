# 📜 Hệ thống License Management cho TApp

## 🎯 Tổng quan

Hệ thống license management được thiết kế theo kiến trúc **OFFLINE-FIRST, ONLINE-OPTIONAL** với các đặc điểm:

- ✅ **Offline Verification**: License được verify ngay khi khởi động, không cần internet
- ✅ **RSA Digital Signature**: Sử dụng RSA 2048-bit để ký và verify license
- ✅ **Feature-based**: Kiểm soát từng tính năng (Camera, Printer, Cloud, PLC)
- ✅ **Expiry Management**: Quản lý thời hạn license tự động
- ✅ **Secure**: Private key chỉ có trong DemoApp, public key được embed vào TApp

## 🏗️ Kiến trúc

```
┌────────────┐
│ License Tool│  (DemoApp - offline)
│             │
│ - Generate │
│   Key Pair │
│ - Create   │
│   License  │
│ - Sign RSA │
└─────┬──────┘
      │  ký RSA
License File (.lic)
      │
┌─────▼─────────┐
│   TApp .NET 8  │
│────────────────│
│ 1. Verify      │  ✅ BẮT BUỘC
│    offline     │
│ 2. Unlock      │
│    feature     │
│ 3. Cache       │
│    license     │
│────────────────│
│ Nếu có Internet│
│ → Call API     │  (optional)
│ → Check revoke │
│ → Gia hạn      │
└───────────────┘
```

## 📦 Cấu trúc Files

### DemoApp (License Management Tool)

```
DemoApp/
├── Models/
│   └── LicenseModel.cs          # Model chứa thông tin license
├── Services/
│   └── LicenseManager.cs        # Quản lý: tạo key, ký license, verify
└── Forms/
    └── LicenseManagerForm.cs    # UI quản lý license
```

### TApp (Application)

```
TApp/
├── Models/
│   └── LicenseModel.cs          # Model license (giống DemoApp)
├── Services/
│   └── LicenseVerifier.cs      # Verify license khi khởi động
└── Helpers/
    └── LicenseHelper.cs         # Helper để check features
```

## 🚀 Hướng dẫn sử dụng

### Bước 1: Tạo Key Pair (Chỉ làm 1 lần)

1. Mở **DemoApp**
2. Vào tab **"🔑 Quản lý Keys"**
3. Click **"Tạo Key Pair Mới"**
4. Keys sẽ được lưu tại:
   ```
   %AppData%\TAppLicense\Keys\
   ├── private.key  (DÙNG ĐỂ KÝ LICENSE - BẢO MẬT!)
   └── public.key   (ĐƯA CHO TAPP ĐỂ VERIFY)
   ```

### Bước 2: Export Public Key

1. Trong tab **"🔑 Quản lý Keys"**
2. Click **"Export Public Key"**
3. Lưu file `public_key.txt`
4. **Import vào TApp** (xem bước 3)

### Bước 3: Import Public Key vào TApp

Có 2 cách:

#### Cách 1: Copy file vào thư mục TApp
```
%LocalAppData%\TApp\public.key
```

#### Cách 2: Sử dụng LicenseVerifier.ImportPublicKey()
```csharp
var verifier = new LicenseVerifier();
verifier.ImportPublicKey(publicKeyBase64);
```

### Bước 4: Tạo License

1. Vào tab **"📝 Tạo License"**
2. Điền thông tin:
   - **Tên công ty**: Tên khách hàng
   - **Email**: Email liên hệ
   - **Số điện thoại**: SĐT
   - **Ngày bắt đầu**: Ngày license có hiệu lực
   - **Ngày hết hạn**: Ngày license hết hạn
   - **Số máy tối đa**: Số máy được phép sử dụng
   - **Tính năng**: Danh sách tính năng (phân cách bằng dấu phẩy)
     - Ví dụ: `Camera,Printer,Cloud,PLC`
   - **Ghi chú**: Ghi chú thêm
3. Click **"🎫 Tạo License"**
4. Lưu file `.lic` vào vị trí mong muốn

### Bước 5: Cài đặt License cho TApp

1. Copy file `.lic` vào:
   ```
   %LocalAppData%\TApp\license.lic
   ```
2. Đảm bảo `public.key` đã được import (bước 3)
3. Khởi động TApp - license sẽ được verify tự động

### Bước 6: Verify License (Kiểm tra)

1. Mở **DemoApp**
2. Vào tab **"✅ Verify License"**
3. Click **"Chọn file license..."**
4. Chọn file `.lic` để verify
5. Xem kết quả verify

## 🔐 Bảo mật

### ⚠️ QUAN TRỌNG

- **PRIVATE KEY**: Chỉ lưu trong DemoApp, **KHÔNG BAO GIỜ** chia sẻ hoặc đưa cho khách hàng
- **PUBLIC KEY**: Có thể chia sẻ, được embed vào TApp để verify
- **License File**: Có thể chia sẻ cho khách hàng

### Best Practices

1. ✅ Backup private key ở nơi an toàn
2. ✅ Mỗi license nên có LicenseKey duy nhất
3. ✅ Kiểm tra license trước khi gửi cho khách hàng
4. ✅ Lưu trữ thông tin license (company, expiry) để quản lý

## 💻 Sử dụng trong Code

### Trong TApp - Kiểm tra tính năng

```csharp
using TApp.Helpers;

// Kiểm tra tính năng Camera
if (LicenseHelper.HasCameraFeature())
{
    // Enable camera functionality
}

// Kiểm tra tính năng Printer
if (LicenseHelper.HasPrinterFeature())
{
    // Enable printer functionality
}

// Kiểm tra tính năng Cloud
if (LicenseHelper.HasCloudFeature())
{
    // Enable cloud upload
}

// Kiểm tra tính năng PLC
if (LicenseHelper.HasPLCFeature())
{
    // Enable PLC communication
}

// Lấy số ngày còn lại
int daysLeft = LicenseHelper.GetDaysRemaining();
if (daysLeft <= 30)
{
    // Show warning
}

// Lấy thông tin license
var license = LicenseHelper.GetLicense();
if (license != null)
{
    string companyName = license.CompanyName;
    DateTime expiryDate = license.ExpiryDate;
}
```

### Verify License khi khởi động

License đã được verify tự động trong `TApp/Program.cs`:

```csharp
var licenseVerifier = new LicenseVerifier();
var (isValid, license, errorMessage) = licenseVerifier.VerifyLicense();

if (!isValid)
{
    MessageBox.Show($"License không hợp lệ!\n\n{errorMessage}");
    return; // Exit application
}
```

## 📋 Format License File

License file (`.lic`) là JSON với cấu trúc:

```json
{
  "Data": "{...license model JSON...}",
  "Signature": "base64_rsa_signature"
}
```

License Model JSON:
```json
{
  "CompanyName": "ABC Company",
  "Email": "contact@abc.com",
  "Phone": "0123456789",
  "StartDate": "2024-01-01T00:00:00",
  "ExpiryDate": "2025-01-01T00:00:00",
  "MaxMachines": 1,
  "Features": "Camera,Printer,Cloud,PLC",
  "LicenseKey": "ABCD-1234-EFGH-5678",
  "CreatedDate": "2024-01-01T00:00:00",
  "Notes": "License for production"
}
```

## 🔄 Online Verification (Optional - Tương lai)

Có thể mở rộng thêm:

1. **License API**: Server để quản lý license online
2. **Revoke Check**: Kiểm tra license có bị revoke không
3. **Auto Renewal**: Tự động gia hạn license
4. **Usage Tracking**: Theo dõi số máy đang sử dụng

## 🐛 Troubleshooting

### Lỗi: "Không tìm thấy file license!"
- Kiểm tra file `license.lic` có trong `%LocalAppData%\TApp\` không
- Đảm bảo file không bị đổi tên

### Lỗi: "Không tìm thấy public key!"
- Import public key vào `%LocalAppData%\TApp\public.key`
- Hoặc sử dụng `LicenseVerifier.ImportPublicKey()`

### Lỗi: "License không hợp lệ! Chữ ký không đúng."
- License file bị chỉnh sửa hoặc hỏng
- Tạo lại license mới

### Lỗi: "License đã hết hạn!"
- License đã quá ngày ExpiryDate
- Tạo license mới với ExpiryDate mới

## 📝 Notes

- License được verify **offline** - không cần internet
- License được **cache** sau khi verify thành công
- Mỗi license có **LicenseKey** duy nhất (tự động generate)
- **Features** là danh sách tính năng được kích hoạt (phân cách bằng dấu phẩy)

## 🎉 Hoàn thành!

Hệ thống license đã sẵn sàng sử dụng. Chúc bạn thành công! 🚀

