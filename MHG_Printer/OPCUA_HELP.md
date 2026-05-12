# Hướng dẫn test OPC UA Client

File này hướng dẫn dùng màn hình test OPC UA trong `M2_MainForm`.

## 1. Mở màn hình

Chạy project `MHG_Printer`, form chính là `M2_MainForm`.

Màn hình có các vùng chính:

- `Endpoint`: địa chỉ OPC UA Server.
- `NodeId`: mã node cần đọc/ghi.
- `Write Value`: giá trị muốn ghi xuống node.
- `Type`: kiểu dữ liệu khi ghi.
- `Read Value`: giá trị đọc được từ server.
- `Status`: trạng thái kết nối.
- `Log`: lịch sử thao tác và lỗi.

## 2. Kết nối server

Nhập endpoint theo dạng:

```text
opc.tcp://127.0.0.1:4840
```

Ví dụ endpoint khác:

```text
opc.tcp://192.168.1.10:4840
opc.tcp://localhost:49320
opc.tcp://192.168.1.20:4840/UA/Server
```

Nhấn `Connect`.

Khi kết nối thành công:

- `Status` đổi sang `Connected`.
- Nút `Read`, `Write`, `Disconnect` được bật.
- Log hiện `Connected successfully.`

Lưu ý: màn hình hiện dùng kết nối `Anonymous` và security `None`.

## 3. Đọc giá trị node

Nhập `NodeId` theo đúng format OPC UA.

Ví dụ:

```text
ns=2;s=Tag1
ns=2;s=PLC.Temperature
ns=3;i=1001
```

Nhấn `Read`.

Nếu đọc thành công:

- Giá trị hiện ở ô `Read Value`.
- Log hiện dạng `Read OK: ns=2;s=Tag1 = 123`.

Nếu lỗi:

- Log hiện `Read failed: ...`.
- Kiểm tra lại `Endpoint`, `NodeId`, quyền đọc, hoặc server có online không.

## 4. Ghi giá trị node

Nhập `NodeId` cần ghi.

Nhập giá trị vào `Write Value`.

Chọn kiểu trong `Type`:

- `String`: ghi chuỗi.
- `Int32`: ghi số nguyên 32-bit.
- `Double`: ghi số thực.
- `Boolean`: ghi `true` hoặc `false`.

Nhấn `Write`.

Nếu ghi thành công:

```text
Write OK: ns=2;s=Tag1 = 123
```

Nếu lỗi:

```text
Write failed: ...
```

Các lỗi thường gặp:

- Node không cho ghi.
- Sai kiểu dữ liệu.
- Sai `NodeId`.
- Server đã mất kết nối.

## 5. Ngắt kết nối

Nhấn `Disconnect` khi test xong.

Khi đóng form, chương trình tự gọi disconnect.

## 6. File code liên quan

- Giao diện và khai báo control: `M2_MainForm.Designer.cs`
- Event handler và xử lý OPC UA: `M2_MainForm.cs`
- Package OPC UA/SunnyUI: `MHG_Printer.csproj`

## 7. Ví dụ test nhanh

1. Mở OPC UA Server test.
2. Nhập endpoint:

```text
opc.tcp://127.0.0.1:4840
```

3. Nhập node:

```text
ns=2;s=Tag1
```

4. Nhấn `Connect`.
5. Nhấn `Read` để đọc.
6. Nhập `456`, chọn `Int32`, nhấn `Write` để ghi.
7. Nhấn `Read` lại để kiểm tra giá trị mới.

## 8. Ghi chú kỹ thuật

Màn hình đang dùng thư viện `nauful-LibUA-core`.

Luồng kết nối hiện tại:

1. `Connect()`
2. `OpenSecureChannel(MessageSecurityMode.None, SecurityPolicy.None, null)`
3. `CreateSession(...)`
4. `ActivateSession(new UserIdentityAnonymousToken("anonymous"), ...)`

Nếu server yêu cầu username/password hoặc certificate, cần mở rộng thêm phần login/security.
