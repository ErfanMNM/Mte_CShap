# Hệ Thống Quản Lý Sản Xuất & Kích Hoạt Mã QR

[![Version](https://img.shields.io/badge/version-PMM--01-blue.svg)](documents/Tai%20lieu%20van%20hanh%202.pdf)
[![License](https://img.shields.io/badge/license-Proprietary-red.svg)](LICENSE.txt)

Hệ thống quản lý sản xuất và kích hoạt mã QR tự động cho các chuyên sản xuất tương ớt, tích hợp với PLC Omron, đầu đọc mã vạch Datalogic và máy quét cầm tay.

## 📋 Mục Lục

- [Giới Thiệu](#giới-thiệu)
- [Tính Năng](#tính-năng)
- [Yêu Cầu Hệ Thống](#yêu-cầu-hệ-thống)
- [Cài Đặt](#cài-đặt)
- [Hướng Dẫn Sử Dụng](#hướng-dẫn-sử-dụng)
- [Cấu Hình](#cấu-hình)
- [Khắc Phục Sự Cố](#khắc-phục-sự-cố)
- [Hỗ Trợ](#hỗ-trợ)

## 🎯 Giới Thiệu

Hệ thống được thiết kế để tự động hóa quy trình kích hoạt mã QR trong quá trình sản xuất, tích hợp với các thiết bị phần cứng như PLC, camera quét mã và máy quét cầm tay. Hệ thống hỗ trợ quản lý lô sản xuất, theo dõi trạng thái kích hoạt, và đồng bộ dữ liệu với hệ thống ERP.

### Tầm Quan Trọng

- Cung cấp hướng dẫn chi tiết về cách vận hành và quản lý lô sản xuất
- Giúp người vận hành hiểu rõ nguyên lý hoạt động của hệ thống
- Hạn chế các lỗi do vận hành sai, đảm bảo an toàn cho dữ liệu và thiết bị
- Tối ưu hóa hiệu suất hệ thống, giảm thiểu thời gian chết và sai sót

## ✨ Tính Năng

### Tính Năng Chính

- ✅ **Tự động kích hoạt mã QR** từ camera quét
- ✅ **Quản lý lô sản xuất** (Batch Management)
- ✅ **Tra cứu mã QR** trong database
- ✅ **Thêm mã thủ công** khi cần thiết
- ✅ **Đồng bộ với PLC Omron** để đọc counter
- ✅ **Tích hợp máy quét cầm tay** Datalogic
- ✅ **Backup dữ liệu tự động** (Local & Cloud)
- ✅ **Quản lý người dùng** với xác thực 2FA
- ✅ **Nhật ký hoạt động** chi tiết
- ✅ **Cấu hình Recipe PLC** linh hoạt

### Tính Năng Cloud & Backup

Hệ thống hỗ trợ nhiều chế độ backup:

| Cloud Connection | Cloud Upload | Local Backup | Kết Quả |
|-----------------|--------------|--------------|----------|
| ❌ | ❌ | ❌ | Không chạy gì cả |
| ✅ | ✅ | ✅ | Upload cloud + Backup local |
| ✅ | ✅ | ❌ | Upload cloud, không backup local |
| ✅ | ❌ | ✅ | Chỉ backup local |
| ✅ | ❌ | ❌ | False |

## 🖥️ Yêu Cầu Hệ Thống

### Phần Cứng

- **Máy tính điều khiển**: Windows 10/11, RAM tối thiểu 4GB
- **PLC Omron**: Hỗ trợ giao thức FINS/TCP
- **Đầu đọc mã vạch Datalogic**: Camera quét QR tự động
- **Máy quét cầm tay** (Handheld Scanner): Datalogic, kết nối qua COM Port

### Phần Mềm

- **.NET Framework 8.0** hoặc cao hơn
- **SQLite** hoặc **SQL Server** (tùy cấu hình)
- **Sunny UI Framework** (UI Library)

## 🚀 Cài Đặt

### Bước 1: Kiểm Tra Trước Khi Cài Đặt

- [ ] Kiểm tra kết nối mạng
- [ ] Kiểm tra kết nối PLC
- [ ] Kiểm tra kết nối Camera
- [ ] Kiểm tra cổng COM cho máy quét cầm tay
- [ ] Đảm bảo có quyền Admin để cài đặt

### Bước 2: Cài Đặt Phần Mềm

1. Giải nén file cài đặt
2. Chạy file `TApp.exe` với quyền Administrator
3. Làm theo hướng dẫn trong wizard cài đặt
4. Khởi động lại máy tính nếu được yêu cầu

### Bước 3: Cấu Hình Ban Đầu

1. Mở ứng dụng
2. Đăng nhập với tài khoản Admin
3. Truy cập **Settings** → **App Settings**
4. Cấu hình các tham số:
   - IP và Port PLC
   - IP và Port Camera
   - Cổng COM cho máy quét cầm tay
   - Đường dẫn backup
   - Thông tin kết nối Cloud (nếu có)

## 📖 Hướng Dẫn Sử Dụng

### Quy Trình Khởi Động Hệ Thống

#### 1. Kiểm Tra Trước Khi Khởi Động

- Kiểm tra nguồn điện cho tất cả thiết bị
- Kiểm tra kết nối cáp mạng và COM
- Kiểm tra trạng thái PLC và Camera

#### 2. Bật Nguồn

1. Bật nguồn máy tính
2. Bật nguồn PLC
3. Bật nguồn Camera
4. Đợi các thiết bị khởi động hoàn tất

#### 3. Khởi Động Ứng Dụng

1. Mở ứng dụng `TApp.exe`
2. Đợi hệ thống khởi tạo
3. Kiểm tra trạng thái kết nối trên màn hình chính

#### 4. Đăng Nhập

1. Nhập tên người dùng và mật khẩu
2. Nhập mã xác thực 2FA (nếu được bật)
3. Click **Đăng nhập**

#### 5. Đổi Lô Sản Xuất

1. Trên màn hình chính, click **Đổi Lô**
2. Nhập thông tin lô mới:
   - **Batch Code**: Mã lô sản xuất
   - **Barcode**: Mã vạch sản phẩm
   - **Line Name**: Tên dây chuyền
3. Click **Xác nhận**
4. Kiểm tra thông tin hiển thị trên màn hình

### Sử Dụng Máy Quét Cầm Tay

#### Quét và Tra Cứu Mã QR

1. Mở trang **Scan** (Quét/Tra cứu)
2. Quét mã QR bằng máy quét cầm tay
3. Hệ thống tự động tra cứu và hiển thị thông tin:
   - Trạng thái kích hoạt
   - Thông tin lô sản xuất
   - Thời gian kích hoạt
   - Lịch sử xử lý

#### Thêm Mã Thủ Công

1. Mở trang **Add Code** (Thêm mã thủ công)
2. Nhập hoặc quét mã QR cần thêm
3. Kiểm tra thông tin hiển thị
4. Click **Thêm** để thêm vào hệ thống
5. Kiểm tra kết quả trên bảng danh sách

### Quản Lý Người Dùng

1. Truy cập **Settings** → **User Management**
2. **Tạo người dùng mới**:
   - Click **Thêm người dùng**
   - Nhập thông tin: Username, Password, Role
   - Lưu thông tin
3. **Đổi mật khẩu**:
   - Chọn người dùng
   - Click **Đổi mật khẩu**
   - Nhập mật khẩu mới
4. **Lấy mã xác thực 2FA**:
   - Chọn người dùng
   - Click **Lấy mã 2FA**
   - Quét mã QR bằng ứng dụng xác thực

## ⚙️ Cấu Hình

### Cấu Hình Recipe PLC

1. Truy cập **Settings** → **Recipe PLC**
2. Cấu hình các tham số:
   - **DM Address**: Địa chỉ bộ nhớ PLC
   - **Recipe Values**: Các giá trị recipe
   - **Monitor Camera**: Xem trạng thái camera
3. Lưu cấu hình
4. Xem lịch sử thay đổi nếu cần

### Cấu Hình Ứng Dụng

#### Các Tham Số Quan Trọng

**Cấu hình PLC:**
- `PLC_IP`: Địa chỉ IP của PLC
- `PLC_Port`: Cổng kết nối PLC
- `PLC_Time_Refresh`: Thời gian làm mới (ms)

**Cấu hình Camera:**
- `Camera_01_IP`: Địa chỉ IP máy quét QR
- `Camera_01_Port`: Cổng kết nối camera

**Cấu hình Máy Quét Cầm Tay:**
- `Handheld_COM_Port`: Cổng COM của máy quét

**Cấu hình Cloud:**
- `Cloud_Connection_Enabled`: Bật/tắt kết nối đám mây
- `Cloud_Upload_Enabled`: Bật/tắt upload lên cloud
- `Cloud_Refresh_Interval_Minute`: Khoảng thời gian làm mới (phút)

**Cấu hình Backup:**
- `Local_Backup_Enabled`: Bật/tắt backup cục bộ
- `production_list_path`: Đường dẫn danh sách sản xuất
- `credentialPLCAddressPath`: Đường dẫn thông tin đăng nhập PLC
- `credentialERPPath`: Đường dẫn thông tin đăng nhập ERP

**Cấu hình ERP:**
- `ERP_Sub_Inv`: ERP Sub Inventory
- `ERP_Org_Code`: ERP Organization Code
- `ERP_DatasetID`: ERP Dataset ID
- `ERP_TableID`: ERP Table ID
- `ERP_ProjectID`: ERP Project ID

**Cấu hình AWS:**
- `AWS_Credential_Path`: Đường dẫn AWS Credential

**Cấu hình Ứng Dụng:**
- `AppHideEnable`: Ẩn ứng dụng khi tắt
- `AppStartWithWindows`: Khởi động cùng Windows
- `AppTwoFA_Enabled`: Bật xác thực 2 bước
- `Data_Mode`: Chế độ dữ liệu
- `Line_Name`: Tên dây chuyền

## 🔧 Khắc Phục Sự Cố

### Các Lỗi Thường Gặp

#### 1. Mất Kết Nối PLC

**Triệu chứng:**
- Không đọc được counter từ PLC
- Hiển thị lỗi "PLC Connection Failed"

**Giải pháp:**
1. Kiểm tra cáp mạng kết nối PLC
2. Kiểm tra IP và Port trong Settings
3. Kiểm tra trạng thái PLC (đèn báo)
4. Khởi động lại PLC nếu cần
5. Kiểm tra firewall và antivirus

#### 2. Camera Không Quét Được Mã

**Triệu chứng:**
- Camera không phát hiện mã QR
- Mã QR bị mờ hoặc không đọc được

**Giải pháp:**
1. Kiểm tra kết nối mạng camera
2. Kiểm tra IP và Port camera trong Settings
3. Làm sạch ống kính camera
4. Kiểm tra ánh sáng và góc quét
5. Kiểm tra chất lượng in mã QR

#### 3. Máy Quét Cầm Tay Không Hoạt Động

**Triệu chứng:**
- Không quét được mã bằng máy quét cầm tay
- Không có phản hồi khi quét

**Giải pháp:**
1. Kiểm tra pin máy quét
2. Kiểm tra cổng COM trong Settings
3. Kiểm tra driver COM Port
4. Thử kết nối lại máy quét
5. Khởi động lại ứng dụng

#### 4. Hệ Thống Bị Treo

**Triệu chứng:**
- Ứng dụng không phản hồi
- Màn hình đóng băng

**Giải pháp:**
1. Đợi 30 giây để hệ thống tự phục hồi
2. Nếu vẫn treo, đóng ứng dụng bằng Task Manager
3. Kiểm tra log file để xem lỗi
4. Khởi động lại máy tính nếu cần
5. Liên hệ bộ phận IT

#### 5. Mất Điện

**Quy trình ứng phó:**
1. Hệ thống tự động lưu dữ liệu trước khi tắt
2. Sau khi có điện, khởi động lại hệ thống
3. Kiểm tra dữ liệu đã được lưu
4. Tiếp tục sản xuất với lô hiện tại

#### 6. Lỗi Database

**Triệu chứng:**
- Không lưu được dữ liệu
- Lỗi truy xuất database

**Giải pháp:**
1. Kiểm tra dung lượng ổ cứng
2. Kiểm tra quyền truy cập database
3. Kiểm tra file database có bị hỏng không
4. Restore từ backup nếu cần
5. Liên hệ bộ phận IT

### Quy Trình Ứng Phó Sự Cố

1. **Xác định vấn đề**: Quan sát triệu chứng và thông báo lỗi
2. **Kiểm tra cơ bản**: Kiểm tra kết nối, nguồn điện, cáp
3. **Tham khảo tài liệu**: Xem phần khắc phục sự cố
4. **Thử giải pháp**: Áp dụng các bước khắc phục
5. **Ghi nhận**: Ghi lại lỗi và cách xử lý
6. **Báo cáo**: Liên hệ IT nếu không giải quyết được

## 📞 Hỗ Trợ

### Thông Tin Liên Hệ

**Công ty TNHH TM DV 5X Kỹ Thuật Cao Tân Tiến**

- **Địa chỉ**: 51/2 Trường Chinh, Phường Bảy Hiền, TP. Hồ Chí Minh
- **Email**: info@tantienhitech.com
- **Website**: www.tantienhitech.com
- **Điện thoại**: (028) 2253 4098
- **Di động**: 0876 00 01 00 (Mr. Thức)

### Lưu Ý Khi Liên Hệ Hỗ Trợ

- Chuẩn bị thông tin về lỗi gặp phải
- Chụp ảnh màn hình lỗi (nếu có)
- Ghi lại thời gian xảy ra lỗi
- Mô tả các bước đã thử để khắc phục

## 📚 Tài Liệu Tham Khảo

- [Tài liệu vận hành chi tiết](documents/Tai%20lieu%20van%20hanh%202.pdf)
- [Hướng dẫn sử dụng nhanh](TApp/HUONG_DAN_SU_DUNG.md)

## 🔒 An Toàn

### Quy Tắc Chung

- ✅ Luôn đăng xuất khi không sử dụng
- ✅ Không chia sẻ thông tin đăng nhập
- ✅ Backup dữ liệu định kỳ
- ✅ Báo cáo ngay khi phát hiện bất thường
- ✅ Tuân thủ quy trình vận hành

### Quy Trình Ứng Phó Sự Cố

- **Mất điện**: Hệ thống tự động lưu dữ liệu
- **Hệ thống treo**: Đợi 30s, sau đó đóng bằng Task Manager
- **Mất kết nối PLC/Camera**: Kiểm tra cáp và cấu hình

## 📝 Changelog

### Version PMM-01 (Tháng 12/2025)

- Phiên bản đầu tiên
- Tích hợp PLC Omron
- Tích hợp Camera Datalogic
- Hỗ trợ máy quét cầm tay
- Quản lý lô sản xuất
- Backup tự động (Local & Cloud)
- Xác thực 2FA

## 📄 License

Proprietary - Tất cả quyền được bảo lưu.

---

**Lưu ý**: Tài liệu này được biên soạn dựa trên phiên bản phần mềm hiện tại. Có thể có sự thay đổi trong các phiên bản sau.

**Lần cập nhật cuối**: Tháng 12 năm 2025

---

<div align="center">

**Chúc bạn vận hành hiệu quả!** 🚀

</div>
