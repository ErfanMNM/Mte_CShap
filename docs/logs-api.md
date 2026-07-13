# Logs API — Tài liệu cho Frontend

API đọc log hệ thống từ backend `GProject` (.NET). Mọi log được Serilog ghi song song vào:

1. **File text**: `C:\GProject\Logs\gproject-YYYYMMDD.log` (giữ 30 ngày, dành cho debug/dev).
2. **SQLite**: `C:\GProject\Logs\gproject.db` (giữ vĩnh viễn, dùng cho API này).

API đọc từ SQLite với filter + phân trang.

> **Auth**: Tất cả endpoint yêu cầu user đã login với role **SAdmin**. User khác (Administrator, Operator, Viewer) sẽ nhận `403 Forbidden`.

Base URL: `http://<host>:9999`

---

## 1. Authentication

API dùng cookie session giống các endpoint khác. Cookie tên: `gauth_session`.

```bash
# Login trước
curl -c cookies.txt -X POST http://localhost:9999/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"sadmin","password":"SAdmin@123"}'

# Sau đó gọi API có kèm cookies
curl -b cookies.txt 'http://localhost:9999/api/logs?pageSize=10'
```

Trên trình duyệt / React, FE đã login là xong — cookie tự gửi kèm nếu dùng `credentials: 'include'`.

### Mặc định SAdmin
- Username: `sadmin`
- Password: `SAdmin@123`

---

## 2. Endpoints

### 2.1 `GET /api/logs` — Query + Phân trang

Trả danh sách log entries có filter + phân trang.

#### Query string

| Param | Type | Default | Mô tả |
|---|---|---|---|
| `from` | ISO 8601 UTC | 24h trước | Lọc từ timestamp (inclusive) |
| `to` | ISO 8601 UTC | `now` | Lọc đến timestamp (inclusive) |
| `level` | string | (all) | Lọc level: `Verbose` / `Debug` / `Information` / `Warning` / `Error` / `Fatal` |
| `tag` | string | (none) | Lọc tag — khớp `[Tag]` ở đầu message (case-insensitive, contains) |
| `q` | string | (none) | Tìm trong message (LIKE %q%) |
| `page` | int ≥ 1 | `1` | Trang hiện tại |
| `pageSize` | int 1–500 | `50` | Số entry mỗi trang |
| `sort` | `asc` / `desc` | `desc` | Thứ tự theo `timestamp` (tie-break theo `id`) |

Ví dụ:

```bash
# 50 lỗi gần nhất trong 7 ngày
curl -b cookies.txt \
  'http://localhost:9999/api/logs?level=Error&from=2026-07-07T00:00:00Z&pageSize=50'

# Tìm log của StateMachine chứa "POtest2"
curl -b cookies.txt \
  'http://localhost:9999/api/logs?tag=StateMachine&q=POtest2'
```

#### Response (200)

```json
{
  "success": true,
  "message": "OK",
  "data": {
    "items": [
      {
        "id": 44,
        "timestamp": "2026-07-13T18:13:24.0400611Z",
        "level": "Information",
        "levelName": "Information",
        "message": "[StateMachine] LoadPO -> Ready (\"PO POtest2 loaded\")",
        "messageTemplate": "[StateMachine] {StateTransition} ({Reason})",
        "exception": null,
        "properties": "{\"StateTransition\":\"LoadPO -> Ready\",\"Reason\":\"PO POtest2 loaded\"}",
        "sourceContext": null,
        "machineName": "DESKTOP-NQ67QOR",
        "threadId": 13
      }
    ],
    "page": 1,
    "pageSize": 50,
    "totalCount": 44,
    "totalPages": 1,
    "hasNext": false,
    "hasPrev": false
  }
}
```

#### Field giải thích

| Field | Type | Mô tả |
|---|---|---|
| `id` | long | Primary key, dùng để fetch detail qua `/api/logs/{id}` |
| `timestamp` | ISO 8601 UTC | Thời điểm log được ghi |
| `level` | string | Tên level (`Information`, `Error`, ...) |
| `levelName` | string | Giống `level`, giữ cho FE linh hoạt |
| `message` | string | Message đã render (template đã thay property) |
| `messageTemplate` | string? | Template gốc, ví dụ `[Auth] Logout error for session {SessionId}` |
| `exception` | string? | Full stack trace, chỉ có ở log có exception |
| `properties` | string? | JSON string chứa các property của Serilog. **Cần `JSON.parse()` phía FE** |
| `sourceContext` | string? | Tên class/context sinh ra log |
| `machineName` | string? | Hostname server |
| `threadId` | int? | Thread ID |

#### Error responses

| Status | Body | Nguyên nhân |
|---|---|---|
| `401` | `{"success":false,"message":"Unauthorized"}` | Chưa login |
| `403` | `{"success":false,"message":"Forbidden: requires SAdmin role."}` | Không phải SAdmin |
| `500` | `{"success":false,"message":"<error>"}` | Lỗi server (DB không tồn tại, query sai,...) |

---

### 2.2 `GET /api/logs/levels` — Danh sách level distinct

Trả các level name có trong DB để FE build filter dropdown.

```bash
curl -b cookies.txt 'http://localhost:9999/api/logs/levels'
```

#### Response

```json
{
  "success": true,
  "message": "OK",
  "data": ["Debug", "Error", "Information", "Warning"]
}
```

---

### 2.3 `GET /api/logs/tags` — Danh sách tag distinct

Extract `[Tag]` ở đầu mỗi rendered message, trả unique list để FE build filter dropdown. Quét tối đa **5000 message gần nhất** để tiết kiệm.

```bash
curl -b cookies.txt 'http://localhost:9999/api/logs/tags'
```

#### Response

```json
{
  "success": true,
  "message": "OK",
  "data": ["Auth", "Main", "PLC", "PLCMonitor", "StateMachine", "WebSocket"]
}
```

> **Lưu ý**: Một số tag có dấu nháy do Serilog escape JSON property (ví dụ `"GProjectApiServer"`). Đây là đặc thù format của code hiện tại; nếu muốn clean hơn có thể filter FE-side bằng cách bỏ ký tự `"`.

---

### 2.4 `GET /api/logs/{id}` — Chi tiết 1 entry

Trả full record bao gồm `exception` (full stack trace) và `properties` (full JSON). Dùng khi FE click vào 1 row để mở modal chi tiết.

```bash
curl -b cookies.txt 'http://localhost:9999/api/logs/32'
```

#### Response (200)

```json
{
  "success": true,
  "message": "OK",
  "data": {
    "id": 32,
    "timestamp": "2026-07-13T18:13:09.9837128Z",
    "level": "Error",
    "levelName": "Error",
    "message": "[Auth] Logout error for session \"25e384b8-379e-45ca-b93e-7d45f7afe338\"",
    "messageTemplate": "[Auth] Logout error for session {SessionId}",
    "exception": "Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"ORDER\": syntax error'.\r\n   at Microsoft.Data.Sqlite.SqliteException.ThrowExceptionForRC(...)\r\n   ...",
    "properties": "{\"SessionId\":\"25e384b8-379e-45ca-b93e-7d45f7afe338\"}",
    "sourceContext": null,
    "machineName": "DESKTOP-NQ67QOR",
    "threadId": 13
  }
}
```

#### Error responses

| Status | Mô tả |
|---|---|
| `404` | `{"success":false,"message":"Log entry not found."}` |
| `401` / `403` | Như trên |

---

## 3. Convention "Tag"

Tag là phần `[Tag]` ở đầu message, được extract bằng regex `^\[([^\[\]:]+)\]`. Tag giúp FE filter nhanh theo nguồn log.

Tag **KHÔNG** match nếu:
- Trong tag có ký tự `:` (ví dụ `[Camera:camera]`).
- Trong tag có ký tự `[` hoặc `]` (ví dụ `[Auth[Admin]]`).
- Message không bắt đầu bằng `[Tag]`.

Danh sách tag hiện có trong code (cập nhật theo thực tế):

| Tag | Nguồn |
|---|---|
| `Main` | `Program.cs` (khởi động) |
| `Auth` | `AuthService.cs` |
| `PLC` / `PLCMonitor` | `PLCMonitor.cs` |
| `Camera` | `OnCameraEvent` trong `Program.cs` |
| `StateMachine` | `ProductionStateMachine.cs` |
| `GProjectApiServer` / `POApiServer` | REST API server |
| `WebSocket` | WebSocket handlers |
| `Device` | Device state change events |
| ... và tất cả `[Source]` xuất hiện trong code qua `Log.Information("[{Source}] {Message}", src, msg)` |

---

## 4. Pagination guide

### Cách FE nên dùng

#### 4.1. Phân trang truyền thống (xem lịch sử)

```js
const PAGE_SIZE = 50;

async function fetchLogsPage(page = 1) {
  const r = await fetch(
    `/api/logs?page=${page}&pageSize=${PAGE_SIZE}` +
    `&from=2026-07-13T00:00:00Z&level=Error`,
    { credentials: 'include' }
  );
  const json = await r.json();
  return json.data; // { items, page, pageSize, totalCount, totalPages, hasNext, hasPrev }
}
```

UI controls:
- Nút "Trang trước" enable khi `hasPrev === true`
- Nút "Trang sau" enable khi `hasNext === true`
- Hiển thị "Trang X / Y" với `page` và `totalPages`

#### 4.2. Live tail (polling — khuyến nghị)

Để hiển thị log realtime, FE nên giữ 1 timestamp "last seen" và poll mỗi 5–10s.

```js
const POLL_INTERVAL_MS = 10_000;

async function pollNewLogs() {
  const lastSeen = localStorage.getItem('lastLogTimestamp') || new Date().toISOString();
  const r = await fetch(
    `/api/logs?from=${encodeURIComponent(lastSeen)}&pageSize=200&sort=asc`,
    { credentials: 'include' }
  );
  const { data } = await r.json();

  if (data.items.length > 0) {
    data.items.forEach(addToUi); // append vào bảng
    // Cập nhật lastSeen bằng timestamp cuối cùng
    localStorage.setItem('lastLogTimestamp', data.items[data.items.length - 1].timestamp);
  }
}

setInterval(pollNewLogs, POLL_INTERVAL_MS);
```

Lưu ý:
- Dùng `sort=asc` để đảm bảo entries đến theo thứ tự thời gian.
- `pageSize=200` để tránh miss entries nếu burst nhiều log giữa 2 poll.
- Khi user refresh, reset `lastSeen = now`.

#### 4.3. Cursor alternative (nếu cần scale lớn hơn)

Phân trang theo offset (mặc định) sẽ chậm dần khi `page` lớn. Nếu FE cần deep paging, recommend dùng filter `from/to` kèm `sort=desc` và giảm `pageSize`:

```js
async function loadOlderThan(timestamp) {
  return fetch(
    `/api/logs?to=${encodeURIComponent(timestamp)}&pageSize=50&sort=desc`,
    { credentials: 'include' }
  );
}
```

---

## 5. Sample frontend code

### React + fetch

```tsx
import { useEffect, useState } from 'react';

type LogEntry = {
  id: number;
  timestamp: string;
  level: string;
  message: string;
  exception?: string;
  properties?: string;
};

type LogsPage = {
  items: LogEntry[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
};

export function LogsTable() {
  const [page, setPage] = useState<LogsPage | null>(null);
  const [filters, setFilters] = useState({ level: '', tag: '', q: '' });

  async function load(p = 1) {
    const params = new URLSearchParams({
      page: String(p),
      pageSize: '50',
      sort: 'desc',
    });
    if (filters.level) params.set('level', filters.level);
    if (filters.tag) params.set('tag', filters.tag);
    if (filters.q) params.set('q', filters.q);

    const r = await fetch(`/api/logs?${params}`, { credentials: 'include' });
    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    const json = await r.json();
    setPage(json.data);
  }

  useEffect(() => { load(1); }, [filters]);

  return (
    <div>
      <Filters value={filters} onChange={setFilters} />
      <table>
        <thead>
          <tr>
            <th>Time</th><th>Level</th><th>Message</th><th></th>
          </tr>
        </thead>
        <tbody>
          {page?.items.map((it) => (
            <tr key={it.id}>
              <td>{new Date(it.timestamp).toLocaleString()}</td>
              <td><LevelBadge level={it.level} /></td>
              <td><code>{it.message}</code></td>
              <td><button onClick={() => openDetail(it.id)}>Detail</button></td>
            </tr>
          ))}
        </tbody>
      </table>
      <Pagination page={page} onChange={load} />
    </div>
  );
}
```

### Parse `properties` field

Vì `properties` là JSON string, FE cần parse:

```js
function getProperty(entry, key) {
  if (!entry.properties) return undefined;
  try {
    const obj = JSON.parse(entry.properties);
    return obj[key];
  } catch { return undefined; }
}

// Ví dụ
const sessionId = getProperty(entry, 'SessionId');
const userName = getProperty(entry, 'UserName');
```

---

## 6. Limits & performance

- **Max pageSize**: 500. Server tự cap nếu FE gửi lớn hơn.
- **Default range**: 24h nếu FE không truyền `from/to`. Tránh query toàn DB.
- **Tag filter**: tag là substring search in-memory (do tag nằm trong message). Đủ nhanh với `pageSize <= 500`. Nếu cần filter ở scale lớn, đề xuất dùng `q` (LIKE) thay thế.
- **Levels/Tags list**: `tags` chỉ quét 5000 message gần nhất. Tag cũ hơn có thể không xuất hiện trong dropdown nhưng vẫn filter được qua `tag=...` nếu biết chính xác.

---

## 7. Environment notes

- **DB file**: `C:\GProject\Logs\gproject.db` (Windows-only path vì app đang chạy trên Windows).
- **Nếu DB chưa tồn tại**: server vẫn respond 200 với `totalCount: 0`. Đợi Serilog flush batch (mỗi 1s) hoặc restart app để tạo DB.
- **WAL mode**: DB dùng SQLite WAL — concurrent read OK, không block log writer.
- **Retention**: Không tự prune. Nếu DB phình quá lớn, có thể thêm cleanup job riêng (chưa có).
