# 🔓 Hướng dẫn Bypass Security Checks khi Debug

## 📋 Tổng quan

Khi develop và debug trong Visual Studio, bạn cần bypass các security checks để có thể debug bình thường. Có 3 cách để bypass:

## 🚀 Cách 1: DEBUG Mode (Tự động)

**Cách đơn giản nhất**: Build ở **DEBUG mode**, tất cả security checks sẽ tự động được bypass.

```csharp
#if DEBUG
    // Security checks tự động bypass
#endif
```

**Cách sử dụng:**
1. Trong Visual Studio, chọn **Debug** configuration
2. Build và chạy → Security checks tự động bypass
3. Không cần làm gì thêm!

## 🔧 Cách 2: AppConfigs (Cấu hình)

Thêm vào file config `App.ini`:

```ini
[AppConfigs]
Security_Bypass_Enabled=true
```

**Cách sử dụng:**
1. Mở file: `%LocalAppData%\TApp\Configs\App.ini`
2. Thêm dòng: `Security_Bypass_Enabled=true`
3. Lưu file và chạy app

**Lưu ý:** ⚠️ **KHÔNG BAO GIỜ** set `true` trong production!

## 🌍 Cách 3: Environment Variable

Set environment variable trước khi chạy:

**Windows PowerShell:**
```powershell
$env:TAPP_BYPASS_SECURITY="1"
.\TApp.exe
```

**Windows CMD:**
```cmd
set TAPP_BYPASS_SECURITY=1
TApp.exe
```

**Visual Studio:**
1. Right-click project → **Properties**
2. **Debug** → **Environment variables**
3. Thêm: `TAPP_BYPASS_SECURITY=1`

## 📝 License Bypass (DEBUG Mode)

Trong **DEBUG mode**, nếu license không hợp lệ, bạn sẽ được hỏi:

```
License không hợp lệ!
Bạn có muốn BYPASS license check để debug không?
[Yes] [No]
```

Chọn **Yes** để tiếp tục debug.

## ⚠️ Lưu ý quan trọng

1. ✅ **DEBUG mode**: An toàn, tự động bypass
2. ⚠️ **AppConfigs**: Chỉ dùng khi test, **NHỚ TẮT** trước khi release
3. ⚠️ **Environment Variable**: Chỉ dùng khi test local

## 🎯 Khuyến nghị

**Khi Develop:**
- ✅ Dùng **DEBUG mode** (tự động bypass)
- ✅ Hoặc set `Security_Bypass_Enabled=true` trong config

**Khi Release:**
- ❌ **TẮT** tất cả bypass flags
- ❌ **KHÔNG** set `Security_Bypass_Enabled=true` trong production
- ❌ Build ở **RELEASE mode**

## 🔍 Kiểm tra Bypass Status

Khi bypass được kích hoạt, bạn sẽ thấy message trong Debug Output:

```
⚠️ DEBUG MODE: Security checks are BYPASSED
```

hoặc

```
⚠️ WARNING: Security checks are BYPASSED via config/env!
```

## 🐛 Troubleshooting

### Vẫn bị block khi debug?

1. Kiểm tra bạn đang build ở **DEBUG mode** (không phải Release)
2. Kiểm tra `AppConfigs.Current.Security_Bypass_Enabled` có = true không
3. Kiểm tra environment variable `TAPP_BYPASS_SECURITY`

### Muốn test security checks?

1. Build ở **RELEASE mode**
2. Đảm bảo `Security_Bypass_Enabled=false`
3. Không set environment variable

## ✅ Checklist trước khi Release

- [ ] Build ở **RELEASE mode**
- [ ] `Security_Bypass_Enabled=false` trong config
- [ ] Không set `TAPP_BYPASS_SECURITY` environment variable
- [ ] Test security checks hoạt động đúng
- [ ] Test license verification hoạt động đúng

---

**Happy Debugging! 🎉**

