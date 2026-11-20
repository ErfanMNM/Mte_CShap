# Thiết lập cập nhật TApp qua Cloudflare

TApp chỉ kiểm tra cập nhật khi người dùng chọn **Kiểm tra cập nhật** trong menu biểu tượng khay hệ thống. Package phải có manifest RSA ký hợp lệ và đúng SHA-256 trước khi updater giải nén. Không cấu hình endpoint/khóa công khai thì thao tác cập nhật sẽ báo chưa sẵn sàng; không bỏ qua xác minh.

## Tài nguyên Cloudflare

1. Tạo bucket R2 private tên `tapp-releases`; không bật URL công khai `r2.dev`.
2. Deploy Worker trong `cloudflare/tapp-updates`. Binding `UPDATES_BUCKET` phải trỏ tới bucket `tapp-releases`. Có thể dùng URL `workers.dev` hoặc gắn custom domain HTTPS.
3. Worker cung cấp `GET /v1/updates/stable?platform=win-x64`, `GET /v1/updates/beta?platform=win-x64` và đường dẫn package `/v1/packages/releases/<version>/win-x64/TApp-win-x64.zip`.
4. Worker đọc manifest và package từ R2 bằng binding; client không nhận R2 credentials.

## Khóa ký release

Chạy trên máy quản trị tin cậy:

```powershell
./scripts/create-update-signing-key.ps1 -OutputDirectory C:/secure/TAppUpdateKey
```

Đặt `tapp-update-private.pem` trong GitHub Actions secret `TAPP_UPDATE_PRIVATE_KEY_PEM`. Đặt nguyên nội dung `TrustedUpdateKey.pem` trong GitHub Actions variable `TAPP_UPDATE_PUBLIC_KEY_PEM`. Không commit hoặc tải khóa riêng lên Cloudflare, repo hay máy người dùng. Khi xoay khóa, phát hành client tin cậy khóa công khai mới trước khi ký các manifest bằng khóa mới.

## GitHub Actions settings

Tạo các repository variables:

- `TAPP_UPDATE_PUBLIC_KEY_PEM`: khóa công khai PEM tương ứng.
- `TAPP_UPDATE_BASE_URL`: URL Worker HTTPS kết thúc bằng `/v1/`, ví dụ `https://updates.example.com/v1/`.
- `CLOUDFLARE_ACCOUNT_ID`: Cloudflare account ID dùng làm R2 S3 endpoint.

Tạo các repository secrets:

- `TAPP_UPDATE_PRIVATE_KEY_PEM`: khóa RSA riêng.
- `TAPP_R2_ACCESS_KEY_ID` và `TAPP_R2_SECRET_ACCESS_KEY`: R2 token giới hạn quyền đọc/ghi object vào bucket phát hành (CI kiểm tra trước khi ghi đè đường dẫn release).

Pipeline phát hành chặn tag đã có package cùng version, upload package theo đường dẫn bất biến trước, rồi mới cập nhật manifest `stable` hoặc `beta` với `Cache-Control: no-store`. Không bật job phát hành trước khi cấu hình Worker, bucket, khóa và các giá trị trên.

## Phát hành và cài thử

Tag bản phát hành theo `vMAJOR.MINOR.PATCH`; tag có suffix SemVer như `v9.1.0-beta.1` vào kênh beta. GitHub Actions build TApp self-contained cho `win-x64`, build updater độc lập, bỏ file cấu hình/DB mẫu khỏi package, ký manifest và tải package lên R2. TApp mặc định kiểm tra kênh stable; có thể chạy bản pilot beta bằng cách đặt biến môi trường `TAPP_UPDATE_CHANNEL=beta` cho tài khoản chạy TApp.

Thử trên máy Windows riêng trước khi promote sang stable. Lần cài đầu cần cài thủ công một bản TApp có updater mới; các phiên bản cũ chưa có nút kiểm tra cập nhật. Thư mục cài phải cho tài khoản người chạy quyền ghi (khuyến nghị thư mục người dùng); nếu cài dưới Program Files, cần bộ cài/elevation riêng. Người vận hành phải dừng sản xuất và bảo đảm thiết bị an toàn trước khi xác nhận cập nhật. Updater sao lưu thư mục cài, giữ lại `Configs`, `Database`, `CONFIG.txt`, và khôi phục file cũ nếu khởi động TApp mới thoát trong 15 giây đầu.

## Kiểm tra cục bộ

```powershell
dotnet run --project TApp.Update.Shared.Tests/TApp.Update.Shared.Tests.csproj --configuration Release
dotnet build TApp/TApp.csproj --configuration Release --runtime win-x64 --self-contained true
dotnet publish TApp.Update.Agent/TApp.Update.Agent.csproj --configuration Release --runtime win-x64 --self-contained true
cd cloudflare/tapp-updates
npm install
npm test
npx wrangler deploy --dry-run
```

Worker được kiểm thử với R2 binding giả lập. Tính năng cập nhật end-to-end cần bucket, URL Worker, khóa ký, một package phát hành và máy Windows thử nghiệm; chưa có bước nào ở repo này tự deploy hoặc phát hành lên Cloudflare.
