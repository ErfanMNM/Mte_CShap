# Kiểm tra gửi kết quả CC320

Chạy trên Windows có .NET 8 Desktop Runtime và .NET SDK:

```powershell
dotnet run --project TApp.CommunicationChecks/TApp.CommunicationChecks.csproj
```

Bộ kiểm tra dùng TCP/UDP loopback trên cổng tạm, kiểm tra byte thực tế nhận được từ `DatalogicCamera.SendAsync` và luồng gửi của `FDashboard`. Không gọi `MainForm`, `FDashboard.Start`, không đọc cấu hình nhà máy hoặc kết nối PLC/CC320 thật. Khâu thiết lập socket TCP được thay bằng reflection để tránh chạy vòng tự kết nối của component; phần gửi, hàng đợi và xử lý kết quả đều dùng mã production. Nhánh PLC dùng bộ phản hồi FINS/UDP tối thiểu chỉ cho hai lệnh ghi một word.

Các tình huống: ánh xạ `MP5`/`MP6` nguyên chuỗi, chưa kết nối, kích hoạt mã sau gửi thành công, lưu lỗi khi gửi thất bại, phân loại Fail, thứ tự kết quả khi worker bận, khe hoàn tất worker, từ chối lệnh cũ sau khi phiên kết nối đổi trong lúc chờ khóa hoặc chờ worker, chuyển về PLC khi tắt CC320, và dữ liệu lớn gửi đồng thời. Chương trình trả exit code `1` nếu có kiểm tra thất bại.

Phần đo thời gian kiểm tra thêm mốc bắt đầu lúc nhận camera (bao gồm thời gian chờ worker), cập nhật `GlobalVarialbles.CameraToCC320ElapsedMs`, và số đo riêng của từng code trên listbox. Bộ kiểm tra cố ý giữ worker khoảng 60 ms để phát hiện việc bỏ sót thời gian hàng đợi; không áp ngưỡng thời gian tối đa phụ thuộc tải máy. Gửi lỗi và nhánh PLC không được hiển thị số đo CC320 cũ. Thời gian đo kết thúc khi gửi xong qua socket, không xác nhận thời điểm CC320 nhận hoặc thực thi lệnh.
