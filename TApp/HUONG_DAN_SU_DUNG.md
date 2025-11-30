# HỆ THỐNG QUẢN LÝ SẢN XUẤT & KÍCH HOẠT MÃ QR
**Tài liệu hướng dẫn sử dụng**

**Phiên bản:** 1.0
**Ngày biên soạn:** Tháng 11 năm 2025

---

## GIỚI THIỆU

Tài liệu này được biên soạn nhằm cung cấp đầy đủ thông tin giúp người vận hành sử dụng hệ thống quản lý sản xuất đúng cách, đảm bảo an toàn, đồng thời thực hiện các thao tác hiệu quả. Việc tuân thủ các hướng dẫn trong tài liệu sẽ giúp hệ thống hoạt động ổn định, tránh sai sót dữ liệu và đạt hiệu suất tối ưu.

### 1. Tầm quan trọng của tài liệu

- Cung cấp hướng dẫn chi tiết về cách vận hành, quản lý lô sản xuất và khắc phục sự cố thường gặp
- Giúp người vận hành hiểu rõ nguyên lý hoạt động của hệ thống, từ đó sử dụng đúng quy trình
- Hạn chế các lỗi do vận hành sai, đảm bảo an toàn cho dữ liệu và thiết bị
- Tối ưu hóa hiệu suất hệ thống, giảm thiểu thời gian chết và sai sót

### 2. Lưu ý khi sử dụng tài liệu

- Vui lòng luôn giữ tài liệu này bên cạnh thiết bị để có thể tham khảo khi cần thiết
- Đọc kỹ tài liệu trước khi vận hành để tránh những sự cố không mong muốn
- Nếu phát hiện sai sót hoặc cần bổ sung thêm thông tin, vui lòng liên hệ với bộ phận IT/kỹ thuật

### 3. Hỗ trợ kỹ thuật

Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ trong quá trình sử dụng, vui lòng liên hệ với bộ phận IT hoặc quản lý dự án.

---

## MỤC LỤC

- [GIỚI THIỆU](#giới-thiệu)
- [PHẦN 1: HƯỚNG DẪN AN TOÀN](#phần-1-hướng-dẫn-an-toàn)
  - [1.1. Đối với người vận hành](#11-đối-với-người-vận-hành)
  - [1.2. Đối với quản lý/kỹ thuật viên](#12-đối-với-quản-lýkỹ-thuật-viên)
  - [1.3. Đối với môi trường làm việc](#13-đối-với-môi-trường-làm-việc)
  - [1.4. Quy trình ứng phó sự cố](#14-quy-trình-ứng-phó-sự-cố)
- [PHẦN 2: THIẾT BỊ VÀ GIAO DIỆN](#phần-2-thiết-bị-và-giao-diện)
  - [2.1. Thiết bị phần cứng](#21-thiết-bị-phần-cứng)
  - [2.2. Giới thiệu giao diện phần mềm](#22-giới-thiệu-giao-diện-phần-mềm)
  - [2.3. Cách hệ thống hoạt động](#23-cách-hệ-thống-hoạt-động)
- [PHẦN 3: QUY TRÌNH VẬN HÀNH](#phần-3-quy-trình-vận-hành)
  - [3.1. Quy trình khởi động hệ thống](#31-quy-trình-khởi-động-hệ-thống)
  - [3.2. Quy trình đăng nhập](#32-quy-trình-đăng-nhập)
  - [3.3. Quy trình bắt đầu sản xuất, đổi lô](#33-quy-trình-bắt-đầu-sản-xuất-đổi-lô)
  - [3.4. Quy trình quét và tra cứu mã QR](#34-quy-trình-quét-và-tra-cứu-mã-qr)
  - [3.5. Quy trình thêm mã thủ công](#35-quy-trình-thêm-mã-thủ-công)
  - [3.6. Quy trình xóa lỗi và reset counter](#36-quy-trình-xóa-lỗi-và-reset-counter)
- [PHẦN 4: CẤU HÌNH HỆ THỐNG](#phần-4-cấu-hình-hệ-thống)
  - [4.1. Cấu hình ứng dụng](#41-cấu-hình-ứng-dụng)
  - [4.2. Cấu hình PLC](#42-cấu-hình-plc)
  - [4.3. Quản lý người dùng (Admin)](#43-quản-lý-người-dùng-admin)
    - [4.3.1. Tạo người dùng mới](#431-tạo-người-dùng-mới)
    - [4.3.2. Sửa thông tin người dùng](#432-sửa-thông-tin-người-dùng)
    - [4.3.3. Xóa người dùng](#433-xóa-người-dùng)
    - [4.3.4. Reset mật khẩu người dùng](#434-reset-mật-khẩu-người-dùng)
  - [4.4. Đổi mật khẩu](#44-đổi-mật-khẩu)
  - [4.5. Xem nhật ký hoạt động](#45-xem-nhật-ký-hoạt-động)
- [PHẦN 5: XỬ LÝ SỰ CỐ THƯỜNG GẶP](#phần-5-xử-lý-sự-cố-thường-gặp)

---

# PHẦN 1: HƯỚNG DẪN AN TOÀN

An toàn là yếu tố quan trọng hàng đầu trong quá trình vận hành và bảo trì hệ thống. Người vận hành và kỹ thuật viên cần tuyệt đối tuân thủ các nguyên tắc an toàn dưới đây để tránh các rủi ro về dữ liệu, thiết bị và quy trình sản xuất.

## 1.1. Đối với người vận hành

Người vận hành cần tuân thủ các quy tắc sau để đảm bảo an toàn:

### Quy tắc chung

- **Không tự ý tắt ứng dụng** khi đang trong quá trình sản xuất
- **Không tự ý thay đổi cấu hình hệ thống** nếu không có quyền Admin
- **Tuyệt đối không tắt máy tính** khi hệ thống đang vận hành
- **Luôn kiểm tra trạng thái thiết bị** trước khi bắt đầu ca làm việc
- **Đảm bảo các thiết bị ngoại vi** (PLC, Camera, Scanner) được kết nối ổn định
- **Không can thiệp vào phần cứng** (tháo dây, di chuyển thiết bị) khi hệ thống đang chạy

### Thao tác với dữ liệu

- **Kiểm tra kỹ thông tin lô sản xuất** trước khi xác nhận đổi lô
- **Không thêm mã QR sai định dạng** vào hệ thống
- **Báo cáo ngay** khi phát hiện lỗi dữ liệu hoặc mã trùng lặp bất thường
- **Không xóa counter** khi chưa có sự đồng ý của quản lý

### Sử dụng thiết bị bảo hộ

- Đeo găng tay chống tĩnh điện khi cần tiếp xúc với thiết bị điện tử
- Không sử dụng thiết bị trong môi trường ẩm ướt
- Giữ khu vực làm việc gọn gàng, tránh đổ nước lên thiết bị

## 1.2. Đối với quản lý/kỹ thuật viên

Quản lý và kỹ thuật viên có trách nhiệm cấu hình, bảo trì hệ thống và cần tuân thủ các nguyên tắc sau:

- **Nắm rõ cấu trúc hệ thống** trước khi thao tác hoặc cấu hình
- **Không tự ý thay đổi cấu hình PLC** mà không có sự phê duyệt
- **Backup dữ liệu định kỳ** (xem Phần 5) để phòng trường hợp mất dữ liệu
- **Kiểm tra log thường xuyên** để phát hiện sớm các dấu hiệu bất thường
- **Đảm bảo hệ thống mạng** kết nối ổn định với PLC và Camera
- **Ghi chép lại quá trình bảo trì** để thuận tiện cho việc theo dõi

## 1.3. Đối với môi trường làm việc

- Đảm bảo khu vực làm việc gọn gàng, sạch sẽ để tránh nguy cơ vấp ngã hoặc hư hỏng thiết bị
- Không đặt chất lỏng gần máy tính hoặc thiết bị điện
- Đảm bảo hệ thống thông gió hoạt động tốt để tránh tình trạng quá nhiệt cho thiết bị
- Không để các vật dụng kim loại hoặc dễ cháy gần khu vực thiết bị

## 1.4. Quy trình ứng phó sự cố

### Trong trường hợp mất điện

- Hệ thống sẽ tự động tắt
- Dữ liệu trong queue chưa được ghi vào database có thể bị mất
- Sau khi có điện trở lại, khởi động lại hệ thống theo quy trình chuẩn
- Kiểm tra lại dữ liệu sản xuất cuối cùng

### Trong trường hợp hệ thống bị treo

- Đợi 30 giây để xem hệ thống có tự phục hồi không
- Nếu không, liên hệ ngay với kỹ thuật viên
- Tuyệt đối không force shutdown (giữ nút nguồn)
- Sử dụng Task Manager để force close nếu cần thiết

### Trong trường hợp PLC/Camera mất kết nối

- Kiểm tra đèn báo trạng thái trên dashboard
- Đợi hệ thống tự kết nối lại (30 giây)
- Nếu không kết nối được, kiểm tra dây mạng
- Báo cáo cho kỹ thuật viên nếu vẫn không được

---

# PHẦN 2: THIẾT BỊ VÀ GIAO DIỆN

## 2.1. Thiết bị phần cứng

Hệ thống bao gồm các thiết bị phần cứng sau:

### 2.1.1. Máy tính điều khiển

**Chức năng:**
- Chạy phần mềm quản lý sản xuất
- Kết nối và giao tiếp với PLC, Camera, Scanner
- Lưu trữ database sản xuất
- Hiển thị giao diện HMI cho người vận hành

**Yêu cầu cấu hình:**
- Hệ điều hành: Windows 10/11 (64-bit)
- CPU: Intel Core i5 hoặc tương đương
- RAM: Tối thiểu 4GB
- Ổ cứng: Tối thiểu 100GB trống
- Card mạng: Ethernet RJ45

> **[Chèn hình: Hình ảnh máy tính điều khiển]**

### 2.1.2. PLC Omron

**Chức năng:**
- Điều khiển dây chuyền sản xuất
- Đếm số lượng sản phẩm (tổng, pass, fail)
- Nhận tín hiệu kết quả từ phần mềm (QR OK/NG)
- Gửi tín hiệu điều khiển thiết bị

**Thông số kết nối:**
- IP Address: 192.168.250.1
- Port: 9600
- Protocol: TCP/IP (HSL Communication)
- Refresh Rate: 1000ms (1 giây)

> **[Chèn hình: Hình ảnh PLC Omron]**

### 2.1.3. Camera Datalogic

**Chức năng:**
- Quét mã QR trên sản phẩm
- Gửi dữ liệu QR về phần mềm qua TCP/IP
- Tốc độ quét cao (realtime)

**Thông số kết nối:**
- IP Address: 127.0.0.1 (hoặc IP cụ thể)
- Port: 51236
- Protocol: TCP/IP

> **[Chèn hình: Hình ảnh Camera Datalogic]**

### 2.1.4. Máy quét cầm tay (Handheld Scanner)

**Chức năng:**
- Quét mã QR thủ công khi cần tra cứu
- Thêm mã vào hệ thống khi cần

**Thông số kết nối:**
- Kết nối: Serial COM port
- COM Port: COM3 (mặc định)
- Baud Rate: 9600

> **[Chèn hình: Hình ảnh máy quét cầm tay]**

---

## 2.2. Giới thiệu giao diện phần mềm

### 2.2.1. Màn hình chính (Main Form)

Màn hình chính là giao diện điều hướng chính của ứng dụng, bao gồm:

**Thành phần:**

1. **Header Bar** (thanh trên cùng)
   - Logo ứng dụng
   - Tên người dùng + vai trò (Quản Lý hoặc Vận Hành)
   - Đồng hồ thời gian thực
   - Nút Đăng xuất / Tắt máy

2. **Navigation Menu** (thanh bên trái)
   - Dashboard (Trang chủ)
   - App Settings (Cài đặt ứng dụng) - *Chỉ Admin*
   - Scan (Quét/Tra cứu)
   - Add Code (Thêm mã thủ công)
   - PLC Settings (Cài đặt PLC) - *Chỉ Admin*
   - Activity Logs (Nhật ký hoạt động) - *Chỉ Admin*

> **[Chèn hình: Giao diện màn hình chính với các menu]**

### 2.2.2. Dashboard (Trang chủ sản xuất)

Dashboard là màn hình làm việc chính của người vận hành, hiển thị tất cả thông tin quan trọng về sản xuất.

**Các phần chính:**

#### Phần 1: Thông tin lô sản xuất

| Thành phần | Mô tả |
|------------|-------|
| **Số Lô** | Mã lô sản xuất hiện tại (BatchCode) |
| **Barcode** | Mã vạch sản phẩm |
| **Nút "Đổi Lô"** | Thay đổi thông tin lô sản xuất |
| **Trạng thái** | Chế độ chỉnh sửa/Xác nhận |

> **[Chèn hình: Phần thông tin lô sản xuất]**

#### Phần 2: Trạng thái thiết bị

Hiển thị trạng thái kết nối của các thiết bị:

| Thiết bị | Đèn LED | Ý nghĩa |
|----------|---------|---------|
| **PLC** | 🟢 Xanh | Kết nối bình thường |
| | 🔴 Đỏ | Mất kết nối / Lỗi |
| **Camera** | 🟢 Xanh | Đang hoạt động |
| | 🔴 Đỏ | Không kết nối |
| **Ứng dụng** | 🟢 Sẵn Sàng | Đang sản xuất bình thường |
| | 🔵 Cấu Hình | Đang thay đổi lô |
| | 🔴 Lỗi TB | Có thiết bị lỗi |

> **[Chèn hình: Phần trạng thái thiết bị với LED màu]**

#### Phần 3: Bộ đếm sản xuất

**Bộ đếm PLC:**
- **Tổng (Total):** Tổng số sản phẩm đi qua PLC
- **OK (Pass):** Số sản phẩm QR đọc thành công
- **NG (Fail):** Số sản phẩm lỗi (bao gồm ReadFail + Timeout)

**Bộ đếm Camera:**
- **Tổng:** Tổng số QR camera đọc được
- **Pass:** Số QR hợp lệ
- **Fail:** Số QR lỗi (trùng, sai format, timeout)

**Thống kê sản xuất:**
- **Số mã đang hoạt động:** Số lượng mã unique trong RAM
- **Sản lượng/giờ:** Tốc độ sản xuất trung bình

> **[Chèn hình: Phần bộ đếm với số liệu thực tế]**

#### Phần 4: Các nút điều khiển

| Nút | Tính năng |
|-----|-----------|
| **Xóa đếm PLC** | Reset bộ đếm trên PLC về 0 |
| **Xóa lỗi PLC** | Xóa dữ liệu lỗi trên PLC |
| **Quét** | Chuyển sang trang Scan để tra cứu mã |

> **[Chèn hình: Các nút điều khiển]**

#### Phần 5: Hiển thị kết quả

- **Mã QR cuối cùng:** Hiển thị mã QR vừa được quét
- **Danh sách 50 mã gần nhất:** Listbox hiển thị lịch sử quét

> **[Chèn hình: Phần hiển thị kết quả với listbox]**

---

### 2.2.3. Trang Quét/Tra cứu (Scan)

**Chức năng:**
- Kết nối với máy quét cầm tay
- Quét mã QR để tra cứu thông tin
- Hiển thị kết quả tìm kiếm từ database

**Các thành phần:**
1. Nút "Kết nối Scanner" - Bật/tắt kết nối COM port
2. Ô hiển thị mã đã quét
3. Kết quả tra cứu:
   - QR Content
   - Batch Code
   - Barcode
   - Status (Pass/Fail/Duplicate/...)
   - Timestamp

> **[Chèn hình: Giao diện trang Scan]**

---

### 2.2.4. Trang Thêm Mã Thủ Công (Add Code)

**Chức năng:**
- Thêm mã QR vào hệ thống một cách thủ công
- Kiểm tra định dạng và trùng lặp
- Hiển thị queue đang chờ xử lý

**Các thành phần:**
1. **Ô nhập mã:** TextBox nhập mã QR (Enter để thêm nhanh)
2. **Nút "Thêm":** Thêm mã vào hàng đợi
3. **Trạng thái:**
   - 🟢 Xanh: Thêm thành công
   - 🔴 Đỏ: Lỗi (sai định dạng, chưa có lô)
   - 🟡 Cam: Cảnh báo (mã đã tồn tại)
4. **Bảng Queue:** Hiển thị 50 mã đang chờ xử lý
5. **Console:** Log màu theo thời gian thực

> **[Chèn hình: Giao diện trang Add Code với các trạng thái]**

**Quy tắc định dạng mã QR:**
- Độ dài tối thiểu: 16 ký tự
- Phải chứa mã Barcode của sản phẩm
- Không được trùng với mã đã có trong hệ thống

---

### 2.2.5. Trang Cấu Hình Ứng Dụng (App Settings)

> ⚠️ **Chỉ dành cho Admin**

**Chức năng:**
- Cấu hình thông số kết nối PLC, Camera, Scanner
- Thay đổi chế độ dữ liệu (Normal/Test/Hard)
- Cấu hình tên dây chuyền (Line Name)

**Các tham số có thể chỉnh sửa:**

| Tham số | Mô tả | Ví dụ |
|---------|-------|-------|
| **PLC_IP** | Địa chỉ IP của PLC | 192.168.250.1 |
| **PLC_Port** | Port kết nối PLC | 9600 |
| **Camera_01_IP** | Địa chỉ IP Camera | 127.0.0.1 |
| **Camera_01_Port** | Port Camera | 51236 |
| **Handheld_COM_Port** | COM port máy quét | COM3 |
| **Data_Mode** | Chế độ dữ liệu | normal/test/hard |
| **Line_Name** | Tên dây chuyền | Line 3 |
| **PLC_Time_Refresh** | Tần suất đọc PLC (ms) | 1000 |

> **[Chèn hình: Giao diện App Settings với các tham số]**

**Cách thay đổi:**
1. Nhấn vào ô giá trị cần sửa
2. Nhập giá trị mới
3. Nhấn "Lưu" hoặc Enter
4. Hệ thống sẽ tự động lưu vào file `App.ini`

---

### 2.2.6. Trang Cấu Hình PLC (PLC Settings)

> ⚠️ **Chỉ dành cho Admin**

**Chức năng:**
- Kết nối và đọc thông số từ PLC
- Upload/Download PLC recipes (file .rplc)
- Hiển thị trạng thái PLC realtime

**Các chức năng:**
1. **Kết nối PLC:** Kiểm tra kết nối TCP/IP
2. **Đọc thông số:** Đọc các giá trị DM từ PLC
3. **Upload Recipe:** Tải recipe từ file lên PLC
4. **Download Recipe:** Lưu recipe từ PLC ra file

> **[Chèn hình: Giao diện PLC Settings]**

---

### 2.2.7. Trang Nhật Ký Hoạt Động (Activity Logs)

> ⚠️ **Chỉ dành cho Admin**

**Chức năng:**
- Hiển thị lịch sử tất cả hoạt động trong hệ thống
- Lọc theo loại log, ngày, người dùng
- Export ra file CSV

**Các loại log:**
- Info: Thông tin chung
- Warning: Cảnh báo
- Error: Lỗi hệ thống
- UserAction: Thao tác người dùng
- DeviceAction: Thao tác thiết bị
- DataChange: Thay đổi dữ liệu

**Bộ lọc:**
1. Loại log (dropdown)
2. Từ ngày - Đến ngày (date picker)
3. Người dùng (text search)
4. Phân trang (50 log/trang)

> **[Chèn hình: Giao diện Activity Logs với bộ lọc]**

---

## 2.3. Cách hệ thống hoạt động

### Quy trình xử lý mã QR từ Camera

```
[Sản phẩm đi qua Camera]
         ↓
[Camera quét mã QR] → Gửi dữ liệu về Phần mềm
         ↓
[Kiểm tra định dạng]
    ├── Độ dài < 16 ký tự? → ❌ Lỗi định dạng
    ├── Không chứa Barcode? → ❌ Lỗi định dạng
    └── OK → Tiếp tục
         ↓
[Kiểm tra trùng lặp]
    ├── Mã đã tồn tại trong hệ thống? → ❌ Trùng lặp
    └── Chưa tồn tại → Tiếp tục
         ↓
[Lưu vào Queue]
    ├── QueueActive (RAM)
    ├── QueueRecord (đợi ghi DB)
    └── ActiveSet (HashSet kiểm tra nhanh)
         ↓
[Background Worker ghi vào Database]
    ├── QRProducts table (lịch sử đầy đủ)
    └── ActiveUniqueQR table (chỉ mã unique)
         ↓
[Gửi kết quả về PLC]
    ├── Pass → PLC_Reject_DM = 0
    └── Fail → PLC_Reject_DM = 1
         ↓
[Cập nhật giao diện]
    ├── Counter tăng
    ├── Hiển thị mã cuối cùng
    └── Thêm vào listbox
```

> **[Chèn hình: Sơ đồ quy trình xử lý mã QR]**

---

### Quy trình đọc Counter từ PLC

```
[Background Worker - mỗi 1 giây]
         ↓
[Đọc PLC Counter qua TCP/IP]
    ├── DM[0]: Total Count (Tổng)
    ├── DM[1]: ReadFail Count
    ├── DM[2]: Pass Count
    └── DM[3]: Timeout Count
         ↓
[Cập nhật biến toàn cục]
         ↓
[Tính sản lượng/giờ từ Database]
         ↓
[Hiển thị lên Dashboard]
```

---

### Kiến trúc lưu trữ dữ liệu

**3 tầng lưu trữ:**

1. **RAM (HashSet):** Kiểm tra trùng lặp nhanh
   - `ActiveSet` chứa tất cả mã đang hoạt động
   - Load khi khởi động từ database
   - Tốc độ: O(1) - rất nhanh

2. **Queue (ConcurrentQueue):** Đệm trước khi ghi DB
   - `QueueActive`: Queue mã unique
   - `QueueRecord`: Queue lịch sử đầy đủ
   - Tránh block UI khi ghi DB

3. **Database (SQLite):**
   - `QRProducts`: Lịch sử tất cả mã (kể cả trùng)
   - `ActiveUniqueQR`: Chỉ mã unique, có index
   - `batch_history`: Lịch sử đổi lô

> **[Chèn hình: Sơ đồ kiến trúc 3 tầng]**

---

# PHẦN 3: QUY TRÌNH VẬN HÀNH

## 3.1. Quy trình khởi động hệ thống

### Bước 1: Kiểm tra trước khi khởi động

Trước khi khởi động phần mềm, kiểm tra:

✅ Máy tính đã được bật và kết nối mạng
✅ PLC đã được cấp nguồn và sáng đèn
✅ Camera đã được cấp nguồn
✅ Máy quét cầm tay đã được kết nối (nếu sử dụng)
✅ Dây mạng Ethernet được cắm chắc chắn

### Bước 2: Khởi động ứng dụng

1. Tìm file `TApp.exe` trên Desktop hoặc thư mục cài đặt
2. Double-click để khởi động
3. Đợi ứng dụng load (khoảng 5-10 giây)

> **[Chèn hình: Icon ứng dụng TApp.exe]**

### Bước 3: Kiểm tra khởi tạo

Khi ứng dụng khởi động, hệ thống sẽ tự động:

1. **Tạo thư mục Logs** (nếu chưa có)
2. **Load file cấu hình** (`App.ini`)
3. **Kết nối PLC:** Kiểm tra kết nối TCP/IP đến PLC
4. **Kết nối Camera:** Kiểm tra kết nối Camera
5. **Load Database:** Đọc lịch sử lô sản xuất cuối cùng
6. **Load ActiveSet:** Nạp tất cả mã đang hoạt động vào RAM

**Quan sát màn hình:**
- Nếu thành công → Hiển thị màn hình đăng nhập
- Nếu lỗi → Hiển thị thông báo lỗi (xem Phần 5: Xử lý sự cố)

> **[Chèn hình: Màn hình khởi động/splash screen]**

---

## 3.2. Quy trình đăng nhập

### Bước 1: Nhập thông tin đăng nhập

1. Nhập **Tên đăng nhập** (Username)
2. Nhập **Mật khẩu** (Password)
3. (Tùy chọn) Nếu bật 2FA, nhập **Mã OTP**
4. Nhấn nút **"Đăng nhập"** hoặc phím **Enter**

> **[Chèn hình: Màn hình đăng nhập]**

### Bước 2: Xác thực

Hệ thống sẽ kiểm tra:
- Tên đăng nhập và mật khẩu có đúng không?
- Tài khoản có bị khóa không?
- Mã OTP có hợp lệ không? (nếu bật 2FA)

### Bước 3: Vào hệ thống

**Nếu đăng nhập thành công:**
- Màn hình chính được hiển thị
- Header bar hiển thị tên người dùng + vai trò:
  - **[Quản Lý]** → màu đỏ (Admin)
  - **[Vận Hành]** → màu xanh (Operator)
- Navigation menu hiển thị các chức năng tương ứng với quyền
- Log ghi lại: `UA-LOGIN-01: Đăng nhập thành công`

**Nếu đăng nhập thất bại:**
- Hiển thị thông báo lỗi
- Log ghi lại: `ERR-LOGIN-01: Đăng nhập thất bại`

> **[Chèn hình: Header bar với tên user và vai trò]**

---

## 3.3. Quy trình bắt đầu sản xuất, đổi lô

> ⚠️ **LƯU Ý QUAN TRỌNG:**
> Quy trình đổi lô ảnh hưởng trực tiếp đến dữ liệu sản xuất.
> Vui lòng kiểm tra kỹ thông tin trước khi xác nhận!

### Bước 1: Nhấn nút "Đổi Lô"

1. Tại trang **Dashboard**, tìm nút **"Đổi Lô"** (hoặc "Change Batch")
2. Nhấn vào nút này

> **[Chèn hình: Nút "Đổi Lô" trên Dashboard]**

### Bước 2: Chọn chế độ đổi lô

Hệ thống hỗ trợ 2 chế độ:

#### Chế độ 1: Load từ ERP (Google Sheets)

**Khi nào dùng:** Khi sản phẩm đã có trong hệ thống ERP

**Các bước:**
1. Nút "Đổi Lô" chuyển sang chế độ "Tải ERP"
2. Hệ thống tự động kết nối Google Sheets
3. ComboBox "Số Lô" hiển thị danh sách sản phẩm
4. Chọn sản phẩm từ danh sách
5. Barcode tự động điền

> **[Chèn hình: ComboBox với danh sách ERP]**

**Lưu ý:**
- Cần có kết nối Internet
- Nếu lỗi kết nối → Hiển thị thông báo, ghi log `ERP-F-01`

#### Chế độ 2: Nhập thủ công

**Khi nào dùng:** Khi sản phẩm chưa có trong ERP hoặc ERP lỗi

**Các bước:**

##### **Bước 1: Nhập thông tin lô sản xuất**

1. Nhập **Số Lô** (BatchCode) vào ô "Số Lô"
2. Nhập **Barcode** vào ô "Barcode"
3. Kiểm tra kỹ thông tin đã nhập

> **[Chèn hình: Ô nhập Số Lô và Barcode]**

##### **Bước 2: Xác thực quyền (dành cho Operator)**

> ⚠️ **QUAN TRỌNG: Chỉ áp dụng cho tài khoản Operator**

**Tại sao cần xác thực?**
- Nhập thủ công có thể gây sai sót dữ liệu nghiêm trọng
- Cần có sự giám sát của Admin để đảm bảo thông tin chính xác
- Tránh nhập nhầm Barcode hoặc Batch Code

**Quy trình xác thực:**

Khi **Operator** nhập thủ công, hệ thống sẽ yêu cầu **mã 2FA từ Admin**:

1. Sau khi nhập Số Lô và Barcode, nhấn nút **"Xác nhận"**
2. Hệ thống hiển thị dialog yêu cầu mã xác thực:

```
🔐 YÊU CẦU XÁC THỰC ADMIN

Bạn đang nhập thông tin lô sản xuất thủ công.
Vui lòng liên hệ Admin để lấy mã xác thực.

Số Lô: [BatchCode đã nhập]
Barcode: [Barcode đã nhập]

┌─────────────────────────────┐
│ Nhập mã 2FA:  [_________]  │
└─────────────────────────────┘

[Xác nhận]  [Hủy]
```

3. **Operator** gọi Admin đến
4. **Admin** xem thông tin Số Lô và Barcode trên màn hình
5. **Admin** kiểm tra thông tin có chính xác không
6. Nếu đúng, **Admin** nhập **mã 2FA** (mã xác thực của Admin)
7. Nhấn **"Xác nhận"**

> **[Chèn hình: Dialog yêu cầu mã 2FA]**

**Nếu xác thực thành công:**
- ✅ Thông báo: "Xác thực thành công!"
- Thông tin lô được cập nhật
- Log ghi lại: `UA-F-03: Operator [username] nhập thủ công, xác thực bởi Admin [admin_username]`

**Nếu xác thực thất bại:**
- ❌ Thông báo: "Mã 2FA không đúng. Vui lòng thử lại."
- Thông tin lô KHÔNG được cập nhật
- Operator cần nhập lại hoặc hủy

**Lưu ý:**
- ⚠️ **Chỉ Admin** mới có mã 2FA
- ⚠️ Mã 2FA là mật khẩu của tài khoản Admin
- ⚠️ Admin phải **kiểm tra kỹ** Số Lô và Barcode trước khi nhập mã
- ⚠️ Nếu sai thông tin → Admin từ chối và yêu cầu Operator nhập lại

**Trường hợp đặc biệt:**
- Nếu tài khoản đăng nhập là **Admin** → KHÔNG cần xác thực 2FA
- Admin có thể nhập thủ công trực tiếp mà không cần mã bổ sung

---

##### **Bước 3: Hoàn tất (sau khi xác thực thành công)**

Sau khi xác thực thành công (hoặc Admin tự nhập), hệ thống sẽ:
1. Cập nhật Số Lô và Barcode vào hệ thống
2. Hiển thị thông tin mới trên Dashboard
3. Ghi log hoạt động
4. Sẵn sàng cho sản xuất

> **[Chèn hình: Màn hình sau khi nhập thủ công thành công]**

### Bước 3: Xác nhận đổi lô

1. Sau khi điền đầy đủ thông tin, nhấn nút **"Xác Nhận"**
2. Hệ thống hiển thị hộp thoại xác nhận:

```
Bạn có chắc chắn muốn đổi lô?
Lô mới: [BatchCode]
Barcode: [Barcode]

[Xác nhận]  [Hủy]
```

3. Nhấn **"Xác nhận"** để tiếp tục

> **[Chèn hình: Hộp thoại xác nhận đổi lô]**

### Bước 4: Hệ thống xử lý

Khi xác nhận, hệ thống sẽ:

1. ✅ Cập nhật `BatchCode` và `Barcode` vào biến toàn cục
2. ✅ Ghi lịch sử vào database `batch_history`
3. ✅ Reset bộ đếm Camera về 0
4. ✅ Ghi log: `UA-F-02: Đổi lô thành công`
5. ✅ Cập nhật hiển thị trên Dashboard

**Trạng thái ứng dụng chuyển sang:**
- 🟢 **"Sẵn Sàng"** → Có thể bắt đầu sản xuất

> **[Chèn hình: Dashboard sau khi đổi lô thành công]**

### Bước 5: Kiểm tra lại

Sau khi đổi lô, **BẮT BUỘC** kiểm tra:

✅ Số Lô hiển thị đúng trên Dashboard
✅ Barcode hiển thị đúng
✅ Trạng thái ứng dụng là "Sẵn Sàng" (màu xanh)
✅ Bộ đếm Camera đã reset về 0
✅ PLC và Camera đều kết nối (đèn xanh)

**Nếu có bất kỳ thông tin nào sai → Đổi lại ngay!**

---

## 3.4. Quy trình quét và tra cứu mã QR

### Chức năng

Tra cứu thông tin chi tiết của một mã QR đã được quét vào hệ thống.

### Bước 1: Chuyển sang trang Scan

1. Nhấn vào menu **"Scan"** (hoặc nút "Quét" trên Dashboard)
2. Màn hình chuyển sang trang Scan

> **[Chèn hình: Menu Scan]**

### Bước 2: Kết nối máy quét

**Nếu sử dụng máy quét cầm tay:**

1. Nhấn nút **"Kết nối Scanner"**
2. Hệ thống mở kết nối COM port (COM3 mặc định)
3. Khi kết nối thành công → Nút chuyển sang "Ngắt kết nối"

> **[Chèn hình: Nút "Kết nối Scanner"]**

### Bước 3: Quét mã

**Cách 1: Dùng máy quét cầm tay**
1. Đảm bảo đã kết nối (bước 2)
2. Hướng máy quét vào mã QR
3. Nhấn nút quét trên máy quét
4. Dữ liệu tự động hiển thị

**Cách 2: Nhập thủ công**
1. Nhập mã QR vào ô "Mã QR"
2. Nhấn **Enter** hoặc nút **"Tìm kiếm"**

> **[Chèn hình: Ô nhập mã QR và nút tìm kiếm]**

### Bước 4: Xem kết quả

Hệ thống sẽ tìm kiếm trong database và hiển thị:

**Nếu tìm thấy:**
- QR Content: Nội dung mã QR
- Batch Code: Lô sản xuất
- Barcode: Mã vạch sản phẩm
- Status: Trạng thái (Pass/Fail/Duplicate/...)
- User: Người thao tác
- Timestamp: Thời gian quét

**Nếu không tìm thấy:**
- Hiển thị thông báo: "Không tìm thấy mã QR trong hệ thống"

> **[Chèn hình: Kết quả tra cứu mã QR]**

---

## 3.5. Quy trình thêm mã thủ công

### Khi nào dùng?

- Khi cần bổ sung mã QR vào hệ thống không qua camera
- Khi camera lỡ bỏ sót một số mã
- Khi cần test hệ thống

### Bước 1: Chuyển sang trang Add Code

1. Nhấn vào menu **"Add Code"**
2. Màn hình chuyển sang trang Add Code

> **[Chèn hình: Menu Add Code]**

### Bước 2: Kiểm tra điều kiện

Trước khi thêm mã, đảm bảo:

✅ Đã có lô sản xuất (BatchCode đã được cài đặt)
✅ Đã có Barcode sản phẩm
✅ Trạng thái ứng dụng không phải "Lỗi TB"

**Nếu chưa có lô:**
- Hệ thống sẽ báo lỗi: "Chưa có thông tin lô sản xuất"
- Trạng thái hiển thị: 🔴 Đỏ

> **[Chèn hình: Thông báo lỗi chưa có lô]**

### Bước 3: Nhập mã QR

1. Click vào ô **"Nhập mã QR"**
2. Nhập hoặc paste mã QR
3. Nhấn **Enter** hoặc nút **"Thêm"**

> **[Chèn hình: Ô nhập mã QR]**

### Bước 4: Hệ thống kiểm tra

Hệ thống sẽ kiểm tra các điều kiện:

#### Kiểm tra 1: Độ dài

- Mã phải có độ dài ≥ 16 ký tự
- Nếu ngắn hơn → 🔴 **Lỗi:** "Mã QR quá ngắn (< 16 ký tự)"

#### Kiểm tra 2: Chứa Barcode

- Mã phải chứa Barcode của sản phẩm hiện tại
- Nếu không → 🔴 **Lỗi:** "Mã không chứa Barcode sản phẩm"

#### Kiểm tra 3: Trùng lặp

- Kiểm tra trong `ActiveSet`
- Nếu đã tồn tại → 🟡 **Cảnh báo:** "Mã đã tồn tại (TRÙNG)"

### Bước 5: Xem kết quả

**Nếu thành công:**
- Trạng thái: 🟢 Xanh - "Thêm thành công"
- Mã được thêm vào Queue
- Console log: `[OK] QR: [mã] - Thêm vào queue`
- Bảng Queue cập nhật hiển thị mã mới

**Nếu thất bại:**
- Trạng thái: 🔴 Đỏ hoặc 🟡 Cam - "Lỗi/Cảnh báo"
- Console log: `[FAIL] Lý do lỗi`

> **[Chèn hình: Trạng thái thành công và thất bại]**

### Bước 6: Kiểm tra Queue

Sau khi thêm, kiểm tra bảng **Queue** (bảng dưới cùng):

- Hiển thị 50 mã gần nhất đang chờ xử lý
- Background worker sẽ tự động ghi vào database
- Sau vài giây, mã sẽ biến mất khỏi Queue (đã ghi xong)

> **[Chèn hình: Bảng Queue hiển thị mã đang chờ]**

---

## 3.6. Quy trình xóa lỗi và reset counter

### 3.6.1. Xóa đếm PLC

**Khi nào dùng:**
- Khi bắt đầu ca làm việc mới
- Khi muốn reset số liệu thống kê

**Các bước:**

1. Tại trang **Dashboard**, tìm nút **"Xóa đếm PLC"**
2. Nhấn nút
3. Hệ thống hiển thị xác nhận:

```
Bạn có chắc chắn muốn xóa bộ đếm PLC?
Hành động này sẽ reset:
- Tổng số sản phẩm
- Số Pass
- Số Fail

[Xác nhận]  [Hủy]
```

4. Nhấn **"Xác nhận"**
5. Hệ thống gửi lệnh reset đến PLC
6. Bộ đếm PLC về 0
7. Ghi log: `FD-UA-1: User xóa số đếm`

> **[Chèn hình: Nút "Xóa đếm PLC" và hộp thoại xác nhận]**

**Lưu ý:**
- ⚠️ Hành động này **KHÔNG** xóa dữ liệu trong database
- ⚠️ Chỉ reset bộ đếm hiển thị trên PLC
- ⚠️ Cần có quyền để thực hiện

---

### 3.6.2. Xóa lỗi PLC

**Khi nào dùng:**
- Khi PLC báo lỗi
- Khi cần xóa dữ liệu lỗi tồn đọng

**Các bước:**

1. Tại trang **Dashboard**, tìm nút **"Xóa lỗi PLC"**
2. Nhấn nút
3. Hệ thống gửi lệnh clear error đến PLC
4. PLC xóa dữ liệu lỗi
5. Ghi log: `FD-UA-2: User xóa dữ liệu PLC`

> **[Chèn hình: Nút "Xóa lỗi PLC"]**

---

# PHẦN 4: CẤU HÌNH HỆ THỐNG

> ⚠️ **PHẦN NÀY CHỈ DÀNH CHO ADMIN**

## 4.1. Cấu hình ứng dụng

### Truy cập trang cấu hình

1. Đăng nhập với tài khoản **Admin**
2. Nhấn vào menu **"App Settings"**
3. Màn hình hiển thị danh sách tất cả tham số cấu hình

> **[Chèn hình: Menu App Settings]**

### Các tham số quan trọng

#### Cấu hình PLC

| Tham số | Mô tả | Giá trị mặc định |
|---------|-------|------------------|
| **PLC_IP** | Địa chỉ IP của PLC Omron | 192.168.250.1 |
| **PLC_Port** | Port kết nối PLC | 9600 |
| **PLC_Time_Refresh** | Tần suất đọc PLC (ms) | 1000 |
| **PLC_Test_Mode** | Chế độ test (dùng localhost) | True/False |

#### Cấu hình Camera

| Tham số | Mô tả | Giá trị mặc định |
|---------|-------|------------------|
| **Camera_01_IP** | Địa chỉ IP Camera Datalogic | 127.0.0.1 |
| **Camera_01_Port** | Port kết nối Camera | 51236 |

#### Cấu hình Scanner

| Tham số | Mô tả | Giá trị mặc định |
|---------|-------|------------------|
| **Handheld_COM_Port** | COM port máy quét cầm tay | COM3 |

#### Cấu hình Sản xuất

| Tham số | Mô tả | Giá trị mặc định |
|---------|-------|------------------|
| **Line_Name** | Tên dây chuyền sản xuất | Line 3 |
| **Data_Mode** | Chế độ dữ liệu | normal |

**Các chế độ Data_Mode:**
- `normal`: Ghi cả bảng QRProducts và ActiveUniqueQR
- `test`: Chỉ test, không ghi DB
- `hard`: Ghi trực tiếp, không qua queue

#### Cấu hình Tùy chọn

| Tham số | Mô tả | Giá trị mặc định |
|---------|-------|------------------|
| **AppHideEnable** | Cho phép thu nhỏ vào tray | True |
| **AppTwoFA_Enabled** | Bật xác thực 2 yếu tố | False |

### Cách thay đổi tham số

1. Tìm tham số cần sửa trong danh sách
2. Click vào ô **"Giá trị"**
3. Nhập giá trị mới
4. Nhấn **Enter** hoặc click ra ngoài để lưu
5. Hệ thống tự động ghi vào file `App.ini`

> **[Chèn hình: Chỉnh sửa tham số]**

**Lưu ý:**
- ⚠️ Một số tham số yêu cầu **khởi động lại ứng dụng** để có hiệu lực
- ⚠️ Kiểm tra kỹ trước khi lưu, tham số sai có thể làm hệ thống không hoạt động

### Vị trí file cấu hình

File `App.ini` được lưu tại:
```
C:\Users\[Username]\AppData\Local\TApp\Configs\App.ini
```

> **[Chèn hình: Cấu trúc thư mục chứa file App.ini]**

---

## 4.2. Cấu hình Recipe PLC

> ⚠️ **PHẦN NÀY CHỈ DÀNH CHO ADMIN**

### Giới thiệu

Trang **PLC Settings** dùng để quản lý và cấu hình các **Recipe** (công thức) cho hệ thống PLC. Recipe chứa các thông số điều khiển quan trọng:

- **Delay Camera (Độ trễ kích camera):** Thời gian trễ từ khi cảm biến phát hiện sản phẩm đến khi camera chụp (ms)
- **Delay Reject (Độ trễ loại):** Thời gian trễ từ khi phát hiện lỗi đến khi kích hoạt cơ cấu loại (ms)
- **Reject Streng (Cường độ loại):** Sức mạnh/thời gian kích hoạt xi lanh loại (ms hoặc %)

**Có 2 loại Recipe:**
1. **Recipe thường:** Dùng cho sản phẩm phổ thông
2. **Recipe CS (Customer Specific):** Dùng cho sản phẩm khách hàng đặc biệt

---

### Truy cập trang cấu hình Recipe

1. Đăng nhập với tài khoản **Admin**
2. Nhấn vào menu **"PLC Settings"**
3. Màn hình hiển thị giao diện cấu hình Recipe

> **[Chèn hình: Menu PLC Settings]**

**Giao diện chính bao gồm:**
- **Tab Recipe thường:** Quản lý recipe phổ thông
- **Tab Recipe CS:** Quản lý recipe khách hàng đặc biệt
- **Dropdown chọn Recipe:** Danh sách các recipe đã lưu
- **3 ô nhập tham số:** Delay Camera, Delay Reject, Reject Streng
- **Monitor Camera:** Xem trực tiếp hình ảnh từ camera
- **Nút Lưu:** Lưu recipe vào file và ghi xuống PLC

> **[Chèn hình: Giao diện PLC Settings đầy đủ]**

---

## Các tham số Recipe

### 1. Delay Camera (Độ trễ kích camera)

**Ký hiệu:** `DelayCamera` hoặc `Delay Triger`

**Đơn vị:** Mili giây (ms)

**Ý nghĩa:**
- Thời gian trễ từ khi cảm biến phát hiện sản phẩm đến khi camera bắt đầu chụp
- Đảm bảo sản phẩm đã ở đúng vị trí trước khi chụp

**Giá trị mặc định:** 1000 ms (1 giây)

**Cách điều chỉnh:**
- **Tăng giá trị:** Nếu sản phẩm chưa đến vị trí camera khi chụp (ảnh bị lệch)
- **Giảm giá trị:** Nếu sản phẩm đã qua vị trí camera khi chụp (ảnh bị lệch ngược)

**Ví dụ:**
- Băng tải chạy nhanh → Delay = 500-800 ms
- Băng tải chạy chậm → Delay = 1200-1500 ms

> **[Chèn hình: Minh họa Delay Camera]**

---

### 2. Delay Reject (Độ trễ loại)

**Ký hiệu:** `DelayReject`

**Đơn vị:** Mili giây (ms)

**Ý nghĩa:**
- Thời gian trễ từ khi phát hiện sản phẩm lỗi (QR NG) đến khi kích hoạt xi lanh loại
- Đảm bảo sản phẩm lỗi đã đến đúng vị trí cơ cấu loại

**Giá trị mặc định:** 2000 ms (2 giây)

**Cách điều chỉnh:**
- **Tăng giá trị:** Nếu xi lanh đẩy sớm (chưa có sản phẩm lỗi ở vị trí loại)
- **Giảm giá trị:** Nếu xi lanh đẩy muộn (sản phẩm lỗi đã qua vị trí loại)

**Lưu ý:**
- Phụ thuộc vào khoảng cách giữa camera và vị trí loại
- Phụ thuộc vào tốc độ băng tải

> **[Chèn hình: Minh họa Delay Reject]**

---

### 3. Reject Streng (Cường độ loại)

**Ký hiệu:** `RejectStreng`

**Đơn vị:** Mili giây (ms) hoặc % (tùy cấu hình PLC)

**Ý nghĩa:**
- Thời gian kích hoạt xi lanh loại
- Hoặc cường độ/sức mạnh đẩy của xi lanh (nếu dùng van tỷ lệ)

**Giá trị mặc định:** 20 (ms hoặc %)

**Cách điều chỉnh:**
- **Tăng giá trị:** Nếu xi lanh đẩy yếu, sản phẩm lỗi không bị loại hết
- **Giảm giá trị:** Nếu xi lanh đẩy quá mạnh, làm hỏng sản phẩm hoặc thiết bị

**Lưu ý:**
- Giá trị quá nhỏ → Sản phẩm lỗi không bị loại
- Giá trị quá lớn → Tiêu hao khí nén, gây hư hỏng thiết bị

> **[Chèn hình: Minh họa Reject Streng]**

---

## Quy trình làm việc với Recipe

### Bước 1: Chọn Recipe

1. Tại dropdown **"Chọn Recipe"**, click để xem danh sách
2. Danh sách hiển thị các recipe đã lưu (ví dụ: Default, Recipe1, Recipe_SanPhamA, v.v.)
3. Chọn recipe muốn sử dụng

**Khi chọn recipe:**
- Hệ thống tự động load 3 thông số từ file `.rplc`
- Hiển thị lên các ô nhập liệu

> **[Chèn hình: Dropdown chọn Recipe]**

---

### Bước 2: Xem/Chỉnh sửa tham số

Sau khi chọn recipe, 3 ô nhập liệu sẽ hiển thị giá trị:

| Tham số | Ô nhập | Ví dụ giá trị |
|---------|--------|---------------|
| **Delay Camera** | ipDelayTriger | 1000 |
| **Delay Reject** | ipDelayReject | 2000 |
| **Reject Streng** | ipRejectStreng | 20 |

**Để chỉnh sửa:**
1. Click vào ô cần sửa
2. Nhập giá trị mới (chỉ nhập số)
3. Kiểm tra kỹ giá trị

> **[Chèn hình: Các ô nhập tham số]**

**Lưu ý:**
- ⚠️ Chỉ nhập số nguyên dương
- ⚠️ Không nhập ký tự chữ hoặc ký tự đặc biệt
- ⚠️ Kiểm tra kỹ đơn vị (ms)

---

### Bước 3: Lưu Recipe

Sau khi chỉnh sửa xong, nhấn nút **"Lưu"** (Save):

**Hệ thống sẽ thực hiện:**

1. **Lưu vào file `.rplc`**
   - Vị trí: `PLC_RECIPEs/[TênRecipe].rplc`
   - Định dạng: JSON
   - Ví dụ:
   ```json
   {
     "DelayCamera": "1000",
     "DelayReject": "2000",
     "RejectStreng": "20"
   }
   ```

2. **Ghi xuống PLC**
   - Ghi 3 giá trị vào địa chỉ DM của PLC
   - Địa chỉ: `PLC_Delay_Camera_DM_C2` (từ Google Sheets mapping)
   - PLC nhận được và áp dụng ngay lập tức

3. **Ghi log**
   - Action: UPDATE
   - RecipeName: Tên recipe
   - RecipeValue: "DelayCamera,DelayReject,RejectStreng"
   - Timestamp: Thời gian lưu
   - UserName: Tên Admin đang đăng nhập

**Nếu lưu thành công:**
- ✅ Thông báo: "Lưu cài đặt PLC thành công!"
- Recipe được cập nhật vào file và PLC
- Log ghi lại: `UA-PLCSETTING-02: Lưu recipe [TênRecipe]`

**Nếu lưu thất bại:**
- ❌ Thông báo: "Lỗi khi ghi vào PLC: [Chi tiết lỗi]"
- Có thể do:
  - PLC mất kết nối
  - Giá trị nhập sai định dạng
  - Lỗi quyền ghi file
- Log ghi lại: `ERR-PLCSETTING-01: Lỗi lưu recipe`

> **[Chèn hình: Nút Lưu và thông báo thành công]**

---

### Bước 4: Kiểm tra kết quả

Sau khi lưu, **BẮT BUỘC** phải kiểm tra:

1. **Kiểm tra file:**
   - Mở thư mục `PLC_RECIPEs/`
   - Tìm file `[TênRecipe].rplc`
   - Mở bằng Notepad, kiểm tra giá trị đã đúng chưa

2. **Kiểm tra PLC:**
   - Quan sát vận hành thực tế
   - Camera có chụp đúng vị trí không?
   - Xi lanh loại có đẩy đúng thời điểm không?
   - Sản phẩm lỗi có bị loại hết không?

3. **Điều chỉnh nếu cần:**
   - Nếu chưa chính xác → Quay lại Bước 2, chỉnh lại tham số
   - Lưu lại và kiểm tra đến khi đạt yêu cầu

> **[Chèn hình: Kiểm tra file recipe]**

---

## Tạo Recipe mới

**Khi nào cần tạo recipe mới?**
- Khi sản xuất sản phẩm mới với tốc độ khác
- Khi thay đổi bố trí dây chuyền
- Khi cần lưu nhiều cấu hình khác nhau

**Các bước:**

1. **Nhập tên Recipe mới**
   - Tại dropdown "Chọn Recipe", có thể nhập tên mới
   - Hoặc dùng tính năng "Tạo mới" (nếu có)

2. **Nhập các tham số**
   - DelayCamera
   - DelayReject
   - RejectStreng

3. **Nhấn Lưu**
   - Hệ thống tự động tạo file `.rplc` mới
   - Ghi log action: CREATE
   - Recipe mới xuất hiện trong dropdown

> **[Chèn hình: Tạo Recipe mới]**

**Quy tắc đặt tên Recipe:**
- ✅ Nên: `Recipe_SanPhamA`, `Recipe_Line3_Fast`, `Default`
- ❌ Không nên: Tên có khoảng trắng, ký tự đặc biệt

---

## Quản lý Recipe CS (Customer Specific)

**Recipe CS** dành cho các sản phẩm khách hàng đặc biệt, được lưu riêng trong thư mục `PLC_RECIPEs_CS/`

**Cách sử dụng:**
1. Chuyển sang **Tab Recipe CS**
2. Thao tác tương tự như Recipe thường
3. Lưu vào thư mục riêng

**Lưu ý:**
- Recipe CS và Recipe thường **hoàn toàn độc lập**
- Có thể có cùng tên nhưng khác thư mục
- Dùng cho sản phẩm yêu cầu đặc biệt

> **[Chèn hình: Tab Recipe CS]**

---

## Xem Monitor Camera

Trang PLC Settings có tích hợp **Monitor Camera** để xem trực tiếp hình ảnh:

**Chức năng:**
- Hiển thị WebView kết nối đến Camera
- URL: `http://[Camera_IP]/monitor`
- Xem realtime hình ảnh camera đang chụp

**Mục đích:**
- Kiểm tra vị trí sản phẩm khi chụp
- Điều chỉnh Delay Camera cho chính xác
- Debug khi có vấn đề về ảnh

> **[Chèn hình: Monitor Camera trên trang PLC Settings]**

---

## Lịch sử thay đổi Recipe

Mọi thao tác với Recipe đều được ghi log vào database `log.rlplc`:

**Các loại Action:**
- **CREATE:** Tạo recipe mới
- **UPDATE:** Cập nhật recipe
- **SELECT:** Chọn recipe để sử dụng

**Thông tin log:**
- RecipeName: Tên recipe
- RecipeValue: "DelayCamera,DelayReject,RejectStreng"
- Action: CREATE/UPDATE/SELECT
- Timestamp: Thời gian
- UserName: Tên Admin

**Xem lịch sử:**
- Mở file `PLC_RECIPEs/log.rlplc` bằng SQLite Browser
- Xem bảng `Log`
- Lọc theo RecipeName, Action, hoặc UserName

> **[Chèn hình: Database log recipe]**

---

## Khắc phục sự cố

### Sự cố 1: Không lưu được recipe

**Hiện tượng:**
- Nhấn "Lưu" nhưng báo lỗi
- Thông báo: "Lỗi khi ghi vào PLC"

**Nguyên nhân:**
- PLC mất kết nối
- Giá trị nhập sai (ký tự chữ, số âm)
- Lỗi quyền ghi file

**Cách khắc phục:**
1. Kiểm tra kết nối PLC (xem Dashboard)
2. Kiểm tra giá trị nhập (chỉ số nguyên dương)
3. Kiểm tra quyền ghi thư mục `PLC_RECIPEs/`

---

### Sự cố 2: Camera chụp sai vị trí

**Hiện tượng:**
- Ảnh chụp bị lệch, không có sản phẩm

**Nguyên nhân:**
- DelayCamera chưa phù hợp

**Cách khắc phục:**
1. Vào PLC Settings
2. Mở Monitor Camera để quan sát
3. Điều chỉnh DelayCamera:
   - Ảnh lệch trái (chụp sớm) → Tăng Delay
   - Ảnh lệch phải (chụp muộn) → Giảm Delay
4. Lưu và kiểm tra lại

---

### Sự cố 3: Xi lanh loại không đúng thời điểm

**Hiện tượng:**
- Sản phẩm lỗi không bị loại
- Hoặc sản phẩm tốt bị loại nhầm

**Nguyên nhân:**
- DelayReject chưa phù hợp

**Cách khắc phục:**
1. Quan sát vị trí sản phẩm khi xi lanh kích hoạt
2. Điều chỉnh DelayReject:
   - Xi lanh đẩy sớm → Tăng Delay
   - Xi lanh đẩy muộn → Giảm Delay
3. Lưu và kiểm tra lại

---

### Sự cố 4: Sản phẩm lỗi không bị loại hết

**Hiện tượng:**
- Xi lanh đẩy nhưng sản phẩm không rời khỏi băng tải

**Nguyên nhân:**
- RejectStreng quá nhỏ

**Cách khắc phục:**
1. Tăng giá trị RejectStreng (từ 20 lên 30, 40...)
2. Lưu và kiểm tra
3. Tăng dần đến khi sản phẩm bị loại hết

**Lưu ý:**
- Không tăng quá cao → Tốn khí nén, hỏng thiết bị

---

## Bảng tham khảo giá trị Recipe

| Loại sản phẩm | Delay Camera (ms) | Delay Reject (ms) | Reject Streng |
|---------------|-------------------|-------------------|---------------|
| Chai nhỏ, băng tải nhanh | 500-800 | 1500-1800 | 15-20 |
| Chai trung bình | 800-1200 | 1800-2200 | 20-25 |
| Chai lớn, băng tải chậm | 1200-1500 | 2200-2500 | 25-30 |
| Sản phẩm đặc biệt (CS) | Tùy chỉnh | Tùy chỉnh | Tùy chỉnh |

**Lưu ý:**
- Đây chỉ là tham khảo
- Cần thử nghiệm thực tế để tìm giá trị tối ưu

---

## 4.3. Quản lý người dùng (Admin)

> ⚠️ **PHẦN NÀY CHỈ DÀNH CHO ADMIN**

### Truy cập trang quản lý người dùng

1. Đăng nhập với tài khoản **Admin**
2. Nhấn vào menu **"App Settings"**
3. Chọn tab **"User Management"** (Tab thứ 2)
4. Màn hình hiển thị 2 phần:
   - **Cài đặt cá nhân** (User Setting) - Đổi mật khẩu của chính mình
   - **Quản lý người dùng** (User Manager) - Tạo, sửa, xóa user

> **[Chèn hình: Tab User Management với 2 phần]**

---

### 4.3.1. Tạo người dùng mới

**Chức năng:** Thêm tài khoản người dùng mới vào hệ thống

**Các bước:**

#### Bước 1: Tìm phần "Quản lý người dùng"

- Tại tab **"User Management"**, tìm phần **"User Manager"** (phần dưới)
- Đây là khu vực dành cho Admin quản lý tất cả user

> **[Chèn hình: Phần User Manager]**

#### Bước 2: Nhập thông tin người dùng mới

Điền đầy đủ các thông tin sau:

| Trường | Mô tả | Yêu cầu |
|--------|-------|---------|
| **Username** | Tên đăng nhập | Duy nhất, không trùng |
| **Password** | Mật khẩu | Tối thiểu 6 ký tự |
| **Confirm Password** | Xác nhận mật khẩu | Phải trùng với Password |
| **Full Name** | Họ và tên đầy đủ | Tùy chọn |
| **Email** | Email liên hệ | Tùy chọn |
| **Role** | Vai trò | Admin hoặc Operator |

> **[Chèn hình: Form nhập thông tin user mới]**

#### Bước 3: Chọn vai trò (Role)

**Có 2 vai trò:**

1. **Admin (Quản Lý)**
   - Có toàn quyền trên hệ thống
   - Có thể cấu hình ứng dụng, PLC
   - Có thể quản lý user khác
   - Có thể xem nhật ký hoạt động

2. **Operator (Vận Hành)**
   - Chỉ có quyền vận hành sản xuất
   - Không thể cấu hình hệ thống
   - Không thể quản lý user
   - Không thể xem nhật ký

**Lựa chọn:**
- Chọn **"Admin"** từ dropdown nếu cần cấp quyền quản lý
- Chọn **"Operator"** cho người vận hành thông thường

> **[Chèn hình: Dropdown chọn Role]**

#### Bước 4: Tạo tài khoản

1. Kiểm tra lại tất cả thông tin đã nhập
2. Nhấn nút **"Tạo người dùng"** (Create User)
3. Hệ thống kiểm tra:
   - Username có trùng không?
   - Password và Confirm Password có khớp không?
   - Các trường bắt buộc đã điền chưa?

**Nếu thành công:**
- Hiển thị thông báo: "Tạo người dùng thành công!"
- Tài khoản mới được thêm vào danh sách
- Log ghi lại: `UM01: Tạo user [username] thành công`

**Nếu thất bại:**
- Hiển thị thông báo lỗi cụ thể
- Ví dụ: "Username đã tồn tại", "Mật khẩu không khớp"

> **[Chèn hình: Thông báo tạo user thành công]**

#### Bước 5: Kiểm tra danh sách user

- Sau khi tạo, tài khoản mới sẽ xuất hiện trong **danh sách người dùng**
- Kiểm tra thông tin hiển thị đúng không
- Danh sách hiển thị: Username, Full Name, Role, Email

> **[Chèn hình: Danh sách user với user mới được thêm]**

---

### 4.3.2. Sửa thông tin người dùng

**Chức năng:** Chỉnh sửa thông tin của user đã tồn tại

**Các bước:**

#### Bước 1: Chọn user cần sửa

1. Tại **danh sách người dùng**, tìm user cần chỉnh sửa
2. Click vào dòng user đó để chọn
3. Thông tin user sẽ được load lên form

> **[Chèn hình: Chọn user từ danh sách]**

#### Bước 2: Chỉnh sửa thông tin

Có thể chỉnh sửa:
- ✅ Full Name (Họ tên)
- ✅ Email
- ✅ Role (Vai trò)
- ✅ Password (Đổi mật khẩu)
- ❌ Username (KHÔNG thể đổi)

**Lưu ý:**
- Username là duy nhất và không thể thay đổi
- Nếu muốn đổi Password, nhập mật khẩu mới vào ô "New Password"

> **[Chèn hình: Form chỉnh sửa thông tin user]**

#### Bước 3: Lưu thay đổi

1. Sau khi chỉnh sửa xong, nhấn nút **"Cập nhật"** (Update)
2. Hệ thống xác nhận:
```
Bạn có chắc chắn muốn cập nhật thông tin user [username]?

[Xác nhận]  [Hủy]
```
3. Nhấn **"Xác nhận"**
4. Thông tin được cập nhật vào database
5. Log ghi lại: `UM01: Cập nhật user [username]`

> **[Chèn hình: Dialog xác nhận cập nhật]**

---

### 4.3.3. Xóa người dùng

**Chức năng:** Xóa tài khoản user khỏi hệ thống

**Các bước:**

#### Bước 1: Chọn user cần xóa

1. Tại **danh sách người dùng**, chọn user cần xóa
2. Nhấn nút **"Xóa người dùng"** (Delete User)

> **[Chèn hình: Nút Delete User]**

#### Bước 2: Xác nhận xóa

Hệ thống hiển thị cảnh báo:

```
⚠️ CẢNH BÁO
Bạn có chắc chắn muốn xóa người dùng [username]?
Hành động này KHÔNG THỂ HOÀN TÁC!

[Xóa]  [Hủy]
```

**Lưu ý:**
- ⚠️ Không thể xóa tài khoản đang đăng nhập
- ⚠️ Không thể xóa tài khoản Admin cuối cùng (phải có ít nhất 1 Admin)
- ⚠️ Hành động xóa là vĩnh viễn, không thể khôi phục

> **[Chèn hình: Dialog cảnh báo xóa user]**

#### Bước 3: Hoàn tất

Nếu xác nhận xóa:
- User bị xóa khỏi hệ thống
- User không thể đăng nhập nữa
- Log ghi lại: `UM01: Xóa user [username]`

---

### 4.3.4. Reset mật khẩu người dùng

**Khi nào dùng:** Khi user quên mật khẩu

**Các bước:**

1. Chọn user cần reset mật khẩu từ danh sách
2. Nhấn nút **"Reset Password"**
3. Nhập mật khẩu mới (2 lần để xác nhận)
4. Nhấn **"Xác nhận"**
5. Thông báo cho user mật khẩu mới

> **[Chèn hình: Form reset password]**

**Lưu ý:**
- ⚠️ Chỉ Admin mới có quyền reset password cho user khác
- ⚠️ Mật khẩu mới phải tối thiểu 6 ký tự
- ⚠️ User nên đổi lại mật khẩu ngay sau khi đăng nhập

---

### 4.3.5. Xem log hoạt động của user

Tại phần dưới cùng của màn hình có **ListBox hiển thị log** realtime:

**Các sự kiện được log:**
- Tạo user mới
- Cập nhật thông tin user
- Xóa user
- Reset password
- Đổi password

**Định dạng log:**
```
[10:30:15] Tạo user 'nguyenvana' thành công
[10:31:20] Cập nhật user 'tranthib' - Đổi role thành Admin
[10:32:45] Xóa user 'hoangvanc'
```

> **[Chèn hình: ListBox hiển thị log hoạt động]**

---

## 4.4. Đổi mật khẩu

**Chức năng:** Cho phép người dùng (cả Admin và Operator) đổi mật khẩu của chính mình

### Truy cập trang đổi mật khẩu

1. Đăng nhập vào hệ thống
2. Nhấn vào menu **"App Settings"**
3. Chọn tab **"User Management"**
4. Tìm phần **"Cài đặt cá nhân"** (User Setting) - phần trên

> **[Chèn hình: Phần User Setting]**

---

### Các bước đổi mật khẩu

#### Bước 1: Xác nhận thông tin hiện tại

Phần "User Setting" hiển thị thông tin của user đang đăng nhập:
- Username
- Full Name
- Email
- Role

**Kiểm tra xem đây có phải là tài khoản của bạn không.**

> **[Chèn hình: Thông tin user hiện tại]**

#### Bước 2: Nhập mật khẩu cũ

1. Tìm ô **"Mật khẩu hiện tại"** (Current Password)
2. Nhập mật khẩu đang sử dụng
3. Ký tự sẽ hiển thị dạng ● để bảo mật

> **[Chèn hình: Ô nhập mật khẩu hiện tại]**

**Lưu ý:**
- ⚠️ Phải nhập đúng mật khẩu hiện tại
- ⚠️ Nếu sai mật khẩu → Hệ thống sẽ báo lỗi

#### Bước 3: Nhập mật khẩu mới

1. Tìm ô **"Mật khẩu mới"** (New Password)
2. Nhập mật khẩu mới mong muốn

**Yêu cầu mật khẩu mới:**
- Tối thiểu 6 ký tự
- Nên sử dụng kết hợp chữ HOA, chữ thường, số, ký tự đặc biệt
- Không nên dùng mật khẩu quá đơn giản (123456, password, v.v.)

> **[Chèn hình: Ô nhập mật khẩu mới]**

#### Bước 4: Xác nhận mật khẩu mới

1. Tìm ô **"Xác nhận mật khẩu mới"** (Confirm New Password)
2. Nhập lại mật khẩu mới (phải giống Bước 3)

**Mục đích:** Đảm bảo bạn không nhập nhầm mật khẩu

> **[Chèn hình: Ô xác nhận mật khẩu mới]**

#### Bước 5: Đổi mật khẩu

1. Kiểm tra lại tất cả 3 ô:
   - Mật khẩu hiện tại (đúng không?)
   - Mật khẩu mới (đủ mạnh không?)
   - Xác nhận mật khẩu mới (khớp không?)
2. Nhấn nút **"Đổi mật khẩu"** (Change Password)

> **[Chèn hình: Nút Đổi mật khẩu]**

#### Bước 6: Xác nhận và hoàn tất

Hệ thống kiểm tra:
- ✅ Mật khẩu hiện tại có đúng không?
- ✅ Mật khẩu mới có đủ điều kiện không?
- ✅ Mật khẩu mới và xác nhận có khớp không?

**Nếu tất cả OK:**
```
✅ Đổi mật khẩu thành công!
Vui lòng đăng nhập lại với mật khẩu mới.
```

**Nếu có lỗi:**
- 🔴 "Mật khẩu hiện tại không đúng"
- 🔴 "Mật khẩu mới quá ngắn (tối thiểu 6 ký tự)"
- 🔴 "Mật khẩu mới và xác nhận không khớp"

> **[Chèn hình: Thông báo đổi mật khẩu thành công]**

#### Bước 7: Đăng nhập lại

Sau khi đổi mật khẩu thành công:
1. Hệ thống sẽ tự động đăng xuất
2. Bạn cần đăng nhập lại bằng mật khẩu mới
3. Lưu mật khẩu mới cẩn thận

**Lưu ý:**
- ⚠️ Nếu quên mật khẩu mới, liên hệ Admin để reset
- ⚠️ Không chia sẻ mật khẩu cho người khác
- ⚠️ Nên đổi mật khẩu định kỳ (3-6 tháng/lần)

---

### Các lỗi thường gặp khi đổi mật khẩu

| Lỗi | Nguyên nhân | Cách khắc phục |
|-----|-------------|----------------|
| "Mật khẩu hiện tại không đúng" | Nhập sai mật khẩu cũ | Kiểm tra lại, thử nhập chậm hơn |
| "Mật khẩu mới quá ngắn" | Mật khẩu < 6 ký tự | Nhập mật khẩu dài hơn |
| "Mật khẩu không khớp" | New Password ≠ Confirm | Nhập lại cho khớp |
| "Mật khẩu mới trùng với cũ" | Dùng lại mật khẩu cũ | Đổi sang mật khẩu khác |

---

### Mẹo bảo mật mật khẩu

✅ **NÊN:**
- Dùng mật khẩu dài (ít nhất 8-12 ký tự)
- Kết hợp chữ HOA, chữ thường, số, ký tự đặc biệt
- Ví dụ: `VanHanh@2025`, `Admin#Line3`
- Đổi mật khẩu định kỳ
- Lưu mật khẩu ở nơi an toàn

❌ **KHÔNG NÊN:**
- Dùng mật khẩu quá đơn giản: `123456`, `password`, `admin`
- Dùng tên, ngày sinh, số điện thoại
- Chia sẻ mật khẩu cho người khác
- Ghi mật khẩu trên giấy dán màn hình
- Dùng chung 1 mật khẩu cho nhiều hệ thống

---

## 4.5. Xem nhật ký hoạt động

### Truy cập trang Activity Logs

1. Đăng nhập với tài khoản **Admin**
2. Nhấn vào menu **"Activity Logs"**
3. Màn hình hiển thị danh sách log

> **[Chèn hình: Menu Activity Logs]**

### Các loại log

| Loại | Màu | Mô tả |
|------|-----|-------|
| **Info** | Xanh | Thông tin chung |
| **Warning** | Vàng | Cảnh báo |
| **Error** | Đỏ | Lỗi hệ thống |
| **UserAction** | Xanh dương | Thao tác người dùng |
| **DeviceAction** | Tím | Thao tác thiết bị |
| **DataChange** | Cam | Thay đổi dữ liệu |
| **Critical** | Đỏ đậm | Nghiêm trọng |

### Bộ lọc

#### 1. Lọc theo loại log

1. Click vào dropdown **"Loại log"**
2. Chọn loại cần xem
3. Danh sách tự động cập nhật

> **[Chèn hình: Dropdown loại log]**

#### 2. Lọc theo ngày

1. Chọn **"Từ ngày"** (date picker)
2. Chọn **"Đến ngày"** (date picker)
3. Nhấn nút **"Lọc"**

> **[Chèn hình: Date picker lọc theo ngày]**

#### 3. Lọc theo người dùng

1. Nhập tên người dùng vào ô **"Tìm kiếm"**
2. Nhấn **Enter** hoặc nút **"Tìm"**

### Phân trang

- Hiển thị **50 log/trang**
- Nút **"Trang trước"** / **"Trang sau"**
- Hiển thị: "Trang 1 / 10"

> **[Chèn hình: Thanh phân trang]**

### Export ra CSV

1. Sau khi lọc xong
2. Nhấn nút **"Export CSV"**
3. Chọn vị trí lưu file
4. File CSV được tạo với định dạng:

```csv
Timestamp,LogType,User,Message,Code
2025-11-28 10:30:15.123,UserAction,admin,Đăng nhập thành công,UA-LOGIN-01
2025-11-28 10:31:22.456,UserAction,admin,Đổi lô thành công,UA-F-02
```

> **[Chèn hình: Nút Export CSV và file CSV mẫu]**

---

# PHẦN 5: XỬ LÝ SỰ CỐ THƯỜNG GẶP

## 5.1. Sự cố PLC mất kết nối

**Hiện tượng:**
- LED PLC sáng đỏ
- Trạng thái: "Mất kết nối"

**Nguyên nhân:**
- Dây mạng bị rút
- PLC bị tắt nguồn
- IP/Port cấu hình sai
- Network switch bị lỗi

**Cách xử lý:**

1. **Kiểm tra dây mạng:**
   - Kiểm tra dây Ethernet có cắm chắc không
   - Kiểm tra đèn LED trên card mạng (phải nhấp nháy)

2. **Kiểm tra nguồn PLC:**
   - Đảm bảo PLC đã được cấp nguồn
   - Đèn trên PLC phải sáng

3. **Ping thử PLC:**
   - Mở Command Prompt (cmd)
   - Gõ: `ping 192.168.250.1`
   - Nếu thành công → Vấn đề ở phần mềm
   - Nếu thất bại → Vấn đề ở mạng

4. **Kiểm tra cấu hình IP:**
   - Vào **App Settings**
   - Kiểm tra `PLC_IP` và `PLC_Port`
   - Đảm bảo đúng với thực tế

5. **Khởi động lại kết nối:**
   - Tắt phần mềm
   - Bật lại phần mềm
   - Hệ thống tự kết nối lại

**Nếu vẫn không được → Liên hệ kỹ thuật viên**

---

## 5.2. Sự cố Camera không nhận dữ liệu

**Hiện tượng:**
- LED Camera sáng đỏ
- Không có mã QR nào được quét
- Bộ đếm Camera không tăng

**Nguyên nhân:**
- Camera mất nguồn
- Dây mạng bị rút
- IP/Port cấu hình sai
- Camera bị lỗi phần cứng

**Cách xử lý:**

1. **Kiểm tra nguồn Camera:**
   - Đảm bảo Camera đã được cấp nguồn
   - Đèn LED trên Camera phải sáng

2. **Kiểm tra dây mạng:**
   - Kiểm tra dây Ethernet có cắm chắc không

3. **Ping thử Camera:**
   - Mở Command Prompt (cmd)
   - Gõ: `ping [Camera_IP]`
   - Ví dụ: `ping 127.0.0.1`

4. **Kiểm tra cấu hình:**
   - Vào **App Settings**
   - Kiểm tra `Camera_01_IP` và `Camera_01_Port`

5. **Khởi động lại phần mềm:**
   - Tắt và bật lại phần mềm

**Nếu vẫn không được → Liên hệ kỹ thuật viên**

---

## 5.3. Sự cố mã QR bị trùng lặp

**Hiện tượng:**
- Nhiều mã bị báo "Duplicate"
- Bộ đếm Fail tăng cao

**Nguyên nhân:**
- Sản phẩm đi qua camera nhiều lần
- Băng tải bị tắc, sản phẩm đi chậm
- Camera quét nhầm sản phẩm đã qua

**Cách xử lý:**

1. **Kiểm tra băng tải:**
   - Đảm bảo băng tải chạy đều
   - Không có sản phẩm bị tắc

2. **Kiểm tra tốc độ:**
   - Nếu băng tải chạy quá chậm → Sản phẩm có thể bị quét nhiều lần

3. **Kiểm tra vị trí Camera:**
   - Camera phải đặt đúng vị trí
   - Mỗi sản phẩm chỉ đi qua 1 lần

4. **Nếu cố ý quét lại:**
   - Mã sẽ bị reject (đúng như thiết kế)
   - Hệ thống chỉ chấp nhận mã unique

---

## 5.4. Sự cố mã QR sai định dạng

**Hiện tượng:**
- Nhiều mã bị báo "Format Error"
- Status hiển thị "Lỗi định dạng"

**Nguyên nhân:**
- Mã QR bị mờ, camera đọc sai
- Mã QR không đúng chuẩn (< 16 ký tự)
- Mã không chứa Barcode sản phẩm

**Cách xử lý:**

1. **Kiểm tra chất lượng in:**
   - Đảm bảo mã QR in rõ nét
   - Không bị nhòe, mờ

2. **Kiểm tra Camera:**
   - Lau ống kính Camera
   - Đảm bảo ánh sáng đủ

3. **Kiểm tra định dạng mã:**
   - Mã phải có độ dài ≥ 16 ký tự
   - Mã phải chứa Barcode của sản phẩm

4. **Kiểm tra Barcode:**
   - Vào Dashboard → Xem Barcode hiện tại
   - Đảm bảo đúng với sản phẩm đang chạy

---

## 5.5. Sự cố không thể đổi lô

**Hiện tượng:**
- Nút "Đổi Lô" không hoạt động
- Hiển thị lỗi khi đổi lô

**Nguyên nhân:**
- Không có quyền (Operator có thể bị giới hạn)
- Không kết nối được Google Sheets (nếu dùng ERP)
- Thông tin nhập sai

**Cách xử lý:**

1. **Kiểm tra quyền:**
   - Đảm bảo đăng nhập với tài khoản có quyền

2. **Kiểm tra kết nối Internet:**
   - Nếu dùng chế độ Load ERP → Cần Internet
   - Nếu không có Internet → Dùng chế độ nhập thủ công

3. **Kiểm tra thông tin nhập:**
   - BatchCode không được để trống
   - Barcode phải hợp lệ

4. **Thử khởi động lại phần mềm**

---

## 5.6. Sự cố database bị lỗi

**Hiện tượng:**
- Phần mềm báo lỗi database
- Không lưu được dữ liệu

**Nguyên nhân:**
- File database bị corrupt
- Ổ cứng đầy
- Quyền truy cập bị từ chối

**Cách xử lý:**

1. **Kiểm tra dung lượng ổ cứng:**
   - Đảm bảo ổ C: còn ít nhất 10GB trống

2. **Kiểm tra quyền:**
   - Chạy phần mềm với quyền Administrator

3. **Backup và khôi phục:**
   - Thực hiện backup database (xem bên dưới)
   - Nếu cần, restore từ bản backup cũ

4. **Liên hệ kỹ thuật viên** để kiểm tra file database

---

## 5.7. Backup và khôi phục dữ liệu

### Backup thủ công

**Vị trí database:**
```
C:\MASAN\
  ├── QRDatabase.db          (Database chính)
  ├── ActiveUnique.db        (Database unique)
  └── Database\
      └── Production\
          └── batch_history.db   (Lịch sử lô)
```

**Các bước backup:**

1. Tắt phần mềm TApp
2. Mở File Explorer
3. Duyệt đến `C:\MASAN\`
4. Copy toàn bộ thư mục `MASAN` ra ổ USB hoặc thư mục backup
5. Đặt tên theo ngày: `MASAN_Backup_2025-11-28`

> **[Chèn hình: Thư mục MASAN với các file DB]**

### Khôi phục từ backup

**Các bước:**

1. Tắt phần mềm TApp
2. Mở thư mục backup
3. Copy toàn bộ nội dung vào `C:\MASAN\`
4. Xác nhận ghi đè file
5. Khởi động lại phần mềm

**Lưu ý:**
- ⚠️ Chỉ khôi phục khi cần thiết
- ⚠️ Dữ liệu sau thời điểm backup sẽ bị mất
- ⚠️ Nên backup định kỳ hàng tuần

---

## 5.8. Liên hệ hỗ trợ

Khi gặp sự cố không thể tự xử lý, vui lòng liên hệ:

**Bộ phận IT/Kỹ thuật:**
- Điện thoại: [Số điện thoại]
- Email: [Email hỗ trợ]

**Thông tin cần cung cấp:**
1. Mô tả sự cố chi tiết
2. Thời gian xảy ra sự cố
3. Screenshot màn hình lỗi (nếu có)
4. File log (nếu yêu cầu)

**Vị trí file log:**
```
C:\Users\[Username]\AppData\Local\TApp\Logs\
```

---

# PHỤ LỤC

## A. Các mã lỗi thường gặp

| Mã lỗi | Ý nghĩa | Cách xử lý |
|--------|---------|-----------|
| **UA-LOGIN-01** | Đăng nhập thành công | (Không phải lỗi) |
| **ERR-LOGIN-01** | Đăng nhập thất bại | Kiểm tra username/password |
| **INFO-FDASH-01** | Camera init OK | (Không phải lỗi) |
| **ERR-FDASH-01** | Camera init fail | Kiểm tra kết nối Camera |
| **ERP-F-01** | Tải ERP thất bại | Kiểm tra Internet, Google Sheets |
| **K01** | PLC mất kết nối | Kiểm tra PLC |
| **K02** | Camera mất kết nối | Kiểm tra Camera |

---

## B. Phím tắt

| Phím | Chức năng |
|------|-----------|
| **Enter** | Xác nhận / Thêm mã QR |
| **Esc** | Hủy / Đóng dialog |
| **Ctrl + S** | Lưu cấu hình |

---

## C. Thuật ngữ

| Thuật ngữ | Ý nghĩa |
|-----------|---------|
| **BatchCode** | Mã lô sản xuất |
| **Barcode** | Mã vạch sản phẩm |
| **QR Content** | Nội dung mã QR |
| **ActiveSet** | Tập hợp mã đang hoạt động trong RAM |
| **Queue** | Hàng đợi xử lý |
| **PLC** | Programmable Logic Controller |
| **HMI** | Human Machine Interface |
| **DM** | Data Memory (bộ nhớ PLC) |
| **ERP** | Enterprise Resource Planning |
| **2FA** | Two-Factor Authentication |

---

# KẾT LUẬN

Tài liệu này cung cấp đầy đủ thông tin về cách sử dụng Hệ Thống Quản Lý Sản Xuất & Kích Hoạt Mã QR. Để hệ thống hoạt động tối ưu, vui lòng:

✅ Đọc kỹ và tuân thủ các hướng dẫn an toàn
✅ Thực hiện đúng quy trình vận hành
✅ Kiểm tra thường xuyên trạng thái thiết bị
✅ Backup dữ liệu định kỳ
✅ Báo cáo ngay khi phát hiện bất thường

**Chúc bạn vận hành hiệu quả!**

---

*Tài liệu này được biên soạn dựa trên phiên bản phần mềm hiện tại. Có thể có sự thay đổi trong các phiên bản sau.*

*Lần cập nhật cuối: Tháng 11 năm 2025*


##Phụ lục##
  2. Logic hoạt động mới:

  | Cloud_Connection_Enabled | Cloud_Upload_Enabled | Local_Backup_Enabled | Kết quả                                  |
  |--------------------------|----------------------|----------------------|------------------------------------------|
  | false                    | -                    | -                    | ❌ Không chạy gì cả                       |
  | true                     | true                 | true                 | ✅ Upload cloud + Backup local            |
  | true                     | true                 | false                | ✅ Upload cloud, ❌ Không backup local     |
  | true                     | false                | true                 | ❌ Không upload cloud, ✅ Chỉ backup local |
  | true                     | false                | false                | ⚠️ Export CSV tạm nhưng không lưu gì     |

  3. Chi tiết cơ chế:

  Khi Cloud_Upload_Enabled = true:
  - Upload lên Google Cloud Storage
  - Nếu thành công → Cập nhật TimeUnixQR để tránh upload lại
  - Nếu thất bại → Giữ nguyên TimeUnixQR, lần sau retry

  Khi Cloud_Upload_Enabled = false:
  - Không upload cloud
  - Coi như "thành công" để tiếp tục backup local
  - Log: "Cloud upload bị tắt"
  - Vẫn cập nhật TimeUnixQR để không xử lý lại dữ liệu cũ

  Khi Local_Backup_Enabled = true:
  - Backup file CSV vào C:/MASAN/Backup/
  - Chỉ backup khi upload thành công (hoặc không cần upload)

  File tạm:
  - Luôn xóa file tạm trong C:/MASAN/Temp/ sau khi xử lý xong