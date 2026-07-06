# Mô Tả Hoạt Động Hệ Thống Phần Mềm - PLC Integration

## 1. Tổng Quan

Hệ thống sử dụng phần mềm giao tiếp với PLC thông qua việc **đọc/ghi giá trị vào bộ nhớ memory** của PLC. Ví dụ minh hoạ sử dụng PLC **Omron**.

---

## 2. Phương Thức Giao Tiếp PLC

| Hành động | Mô tả |
|-----------|-------|
| **Write** | Phần mềm ghi giá trị vào memory (VD: D10) của PLC |
| **Read** | Phần mềm đọc giá trị từ memory của PLC |

> Các thông số cài đặt, counter, và trạng thái được trao đổi qua việc đọc/ghi biến nhớ.

---

## 3. Luồng Hoạt Động Camera Đóng Gói

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        CAMERA ĐÓNG GÓI                                     │
└─────────────────────────────────────────────────────────────────────────────┘

  PLC                                          PHẦN MỀM
   │                                               │
   │  ① Tín hiệu TRIGGER                          │
   │ ─────────────────────────────────────────────►│
   │                                               │
   │  ② DELAY (canh vị trí chụp hợp lý)           │
   │  ◄────────────────────────────────────────────│
   │                                               │
   │  ③ Gửi IO cho Camera chụp                    │
   │ ─────────────────────────────────────────────►│
   │                                               │
   │                                               │ ④ Camera chụp
   │                                               │ ⑤ Xử lý ảnh
   │                                               │
   │                   KẾT QUẢ                     │
   │  ◄────────────────────────────────────────────│
   │                                               │
   │  ⑦ Write D10 = 1 (PASS)                      │
   │     Hoặc: Write D10 = 0 (FAIL)               │
   │                                               │
```

### Chi Tiết Kết Quả

| Kết quả xử lý | Giá trị ghi vào D10 | Hành động PLC |
|---------------|---------------------|---------------|
| **PASS** | `1` | Tiếp tục quy trình |
| **FAIL** | `0` | Loại sản phẩm |
| **TIMEOUT** (>120ms) | `0` | **PLC bắt buộc loại sản phẩm** |

---

## 4. Luồng Hoạt Động Camera Phân Làn Sản Phẩm

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    CAMERA PHÂN LÀN SẢN PHẨM                                │
└─────────────────────────────────────────────────────────────────────────────┘

  PLC                                          PHẦN MỀM
   │                                               │
   │  ① Tín hiệu TRIGGER                          │
   │ ─────────────────────────────────────────────►│
   │                                               │
   │  ② DELAY (canh vị trí chụp hợp lý)           │
   │  ◄────────────────────────────────────────────│
   │                                               │
   │  ③ Gửi IO cho Camera chụp                    │
   │ ─────────────────────────────────────────────►│
   │                                               │
   │                                               │ ④ Camera chụp
   │                                               │ ⑤ Xử lý ảnh
   │                                               │
   │                   KẾT QUẢ                     │
   │  ◄────────────────────────────────────────────│
   │                                               │
   │  ⑦ Write D10 = 1 (LANE A - PASS)             │
   │     Hoặc: Write D10 = 2 (LANE B - PASS)       │
   │     Hoặc: Write D10 = 0 (FAIL)                │
   │                                               │
```

### Chi Tiết Kết Quả

| Kết quả xử lý | Giá trị ghi vào D10 | Hành động PLC |
|---------------|---------------------|---------------|
| **PASS - Làn A** | `1` | Điều hướng sang làn A |
| **PASS - Làn B** | `2` | Điều hướng sang làn B |
| **FAIL** | `0` | Loại sản phẩm |
| **TIMEOUT** (>120ms) | `0` | **PLC bắt buộc loại sản phẩm** |

---

## 5. Xử Lý Timeout

```
┌─────────────────────────────────────────────────────────────────┐
│                        TIMEOUT HANDLING                          │
└─────────────────────────────────────────────────────────────────┘

  Timer 120ms
  ┌────────────────────────┐
  │                        │
  │   ◄──── 120ms ────►   │
  │                        │
  └───────────┬────────────┘
              │
              ▼
    ┌─────────────────────┐
    │ Camera phản hồi?    │
    └─────────┬───────────┘
              │
      ┌───────┴───────┐
      │               │
     CÓ              KHÔNG
      │               │
      ▼               ▼
  Xử lý bình     ┌─────────────────────┐
  thường         │ Write D10 = 0       │
                 │ (FORCE REJECT)      │
                 └─────────────────────┘
```

> **Quan trọng:** Nếu sau **120ms** mà camera không phản hồi, PLC **bắt buộc** phải loại sản phẩm đó ra khỏi dây chuyền.

---

## 6. Thông Số Cài Đặt & Counter

| Thông số | Phương thức | Mục đích |
|----------|-------------|----------|
| Thông số cài đặt | **Read/Write** memory | PLC đọc config từ phần mềm |
| Counter | **Read** memory | Phần mềm đọc số lượng từ PLC |

---

## 7. Tóm Tắt Memory Map

| Memory | Mô tả | Hướng |
|--------|-------|-------|
| D10 | Kết quả kiểm tra camera | PLC ← Phần mềm (Write) |
| Dxx | Thông số cài đặt | PLC ↔ Phần mềm (Read/Write) |
| Dxx | Counter | PLC → Phần mềm (Read) |

---

## 8. Lưu Ý Cho Team Cơ Khí

1. **Trigger signal** cần được gửi từ PLC khi sản phẩm đến vị trí
2. **Delay time** cần được tunning phù hợp với vị trí camera và tốc độ dây chuyền
3. **Timeout 120ms** là cố định - nếu camera không phản hồi trong thời gian này, sản phẩm phải được loại
4. **D10 = 1/2** để phân làn yêu cầu PLC có logic điều hướng tương ứng
