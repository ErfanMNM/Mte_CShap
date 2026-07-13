# API Changes for FE Team - InsufficientCodes & Retry

## 1. Trạng thái mới: `InsufficientCodes`

### Mô tả
Khi bắt đầu Run PO, hệ thống sẽ đồng bộ PO Database với DataPool. Nếu số mã còn lại trong Pool (Status=0) nhỏ hơn `orderQty`, hệ thống sẽ chuyển sang trạng thái `InsufficientCodes`.

### State Diagram

```
                    +-------------------+
                    | InsufficientCodes |
                    +-------------------+
                             |
              User them ma vao Pool
                             |
                             v
                    +-------------------+
                    | /api/production/  |
                    |     retry        |
                    +-------------------+
                             |
                    +-------+-------+
                    |               |
                  Du ma          Khong du
                    |               |
                    v               v
              PushingToDic    InsufficientCodes
                    |
                    v
                 Running
```

### FE cần xử lý

1. **Khi nhận được state = `InsufficientCodes`**:
   - Hiển thị thông báo lỗi rõ ràng cho người dùng
   - Hiển thị số mã còn thiếu: `orderQty - availableCodes`
   - Disable nút Start/Run
   - Enable nút "Thử lại" (Retry)

2. **Khi người dùng click nút "Thử lại"**:
   - Gọi `POST /api/production/retry`
   - Nếu `success = true`: State sẽ tự động chuyển sang `PushingToDic` -> `Running`
   - Nếu `success = false`: Tiếp tục hiển thị lỗi `InsufficientCodes`

---

## 2. API Endpoint: Retry

### `POST /api/production/retry`

Kiểm tra lại số mã trong pool sau khi đã thêm mã.

**Chỉ hoạt động khi**: State hiện tại = `InsufficientCodes`

#### Request
```http
POST /api/production/retry
Content-Type: application/json
```

#### Response Success (200)
```json
{
  "success": true,
  "message": "Sync thanh cong. Them X ma vao PO.",
  "availableCodes": 150,
  "orderQty": 100,
  "neededCodes": 0
}
```

#### Response Failure (400)
```json
{
  "success": false,
  "message": "Chi co the retry khi o trang thai InsufficientCodes. State hien tai: Running",
  "availableCodes": 0,
  "orderQty": 100,
  "neededCodes": 100
}
```

#### Response Error (500)
```json
{
  "success": false,
  "message": "Loi: [error details]",
  "availableCodes": 0,
  "orderQty": 0,
  "neededCodes": 0
}
```

---

## 3. Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | true nếu đủ mã và có thể tiếp tục |
| `message` | string | Thông báo chi tiết |
| `availableCodes` | int | Số mã còn lại trong Pool (Status=0) |
| `orderQty` | int | Số mã cần cho PO |
| `neededCodes` | int | Số mã còn thiếu (orderQty - availableCodes) |

---

## 4. FE Implementation Example

```typescript
// Khi nhận state = "InsufficientCodes" từ /api/production/status hoặc /api/devices/status
const handleInsufficientCodes = async () => {
  try {
    const response = await fetch('/api/production/retry', {
      method: 'POST',
    });
    const data = await response.json();
    
    if (data.success) {
      // Đủ mã, production sẽ tự động tiếp tục
      // State sẽ chuyển: InsufficientCodes -> PushingToDic -> Running
      console.log('Du ma roi, bat dau san xuat');
    } else {
      // Vẫn không đủ mã
      alert(`Khong du ma!\nCon thieu: ${data.neededCodes} ma\nVui long them ma vao pool`);
    }
  } catch (error) {
    console.error('Loi retry:', error);
  }
};
```

---

## 5. UI Recommendation

```
+------------------------------------------+
|         LOI: KHONG DU MA                |
+------------------------------------------+
|                                          |
|  Pool con: 50 ma                         |
|  Can: 100 ma                             |
|  Con thieu: 50 ma                        |
|                                          |
|  [Them ma vao Pool]  [Thu lai]           |
|                                          |
+------------------------------------------+
```

- **Nút "Thêm mã vào Pool"**: Điều hướng sang màn hình DataPool để import thêm mã
- **Nút "Thử lại"**: Gọi API retry để kiểm tra lại sau khi đã thêm mã

---

## 6. Check API Status

Để biết trạng thái hiện tại, FE có thể poll:

```http
GET /api/production/status
```

Response:
```json
{
  "success": true,
  "state": "InsufficientCodes",
  "orderQty": 100,
  ...
}
```

Hoặc:
```http
GET /api/devices/status
```

Response:
```json
{
  "production": {
    "state": "InsufficientCodes",
    "orderQty": 100,
    ...
  }
}
```

---

## Revision History

| Date | Version | Description |
|------|---------|-------------|
| 2026-07-13 | 1.0 | Initial document |
