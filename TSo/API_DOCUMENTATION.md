# TSo API Documentation

## Base URL

```
http://localhost:5000/
```

All endpoints return JSON. All responses include a `timestamp` field (ISO 8601 UTC).

---

## Authentication

### Login

```
POST /api/auth/login
Content-Type: application/json
```

**Request:**
```json
{
  "username": "admin",
  "password": "admin123",
  "twoFACode": "123456"    // optional, required only if 2FA is enabled
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGci...",
  "username": "admin",
  "role": "Admin",
  "timestamp": "2026-06-04T04:37:00Z"
}
```

**Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid username or password",
  "timestamp": "2026-06-04T04:37:00Z"
}
```

---

### Logout

```
POST /api/auth/logout
Authorization: Bearer <token>
```

**Response:**
```json
{ "success": true, "message": "Logged out", "timestamp": "..." }
```

---

### Get Current Session

```
GET /api/auth/session
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGci...",
    "username": "admin",
    "role": "Admin",
    "createdAt": "2026-06-04T04:37:00Z",
    "expiresAt": "2026-06-04T12:37:00Z"
  },
  "timestamp": "..."
}
```

---

### Get 2FA QR URL

```
GET /api/auth/2fa/qrurl?username=admin
```

**Response:**
```json
{
  "success": true,
  "data": "otpauth://totp/TSo:admin?secret=ABCDEF...",
  "timestamp": "..."
}
```

---

## Dashboard

### Get Full Dashboard

```
GET /api/dashboard
Authorization: Bearer <token>
```

**Response:**
```json
{
  "currentBatch": {
    "batchCode": "260604-123456-TOL1-A",
    "barcode": "TOL001",
    "lineName": "Line 1",
    "createdAt": "2026-06-04T04:30:00Z",
    "createdBy": "admin"
  },
  "cameraCounters": {
    "total": 150,
    "pass": 145,
    "fail": 5,
    "duplicate": 3,
    "readFail": 1,
    "notFound": 0,
    "timeout": 0,
    "error": 1,
    "formatError": 0
  },
  "plcCounters": {
    "total": 150,
    "pass": 150,
    "readFail": 0,
    "timeout": 0,
    "fail": 0
  },
  "deviceStatus": {
    "plcStatus": "Connected",
    "cameraStatus": "Connected",
    "scannerStatus": "Disconnected",
    "lastUpdate": "2026-06-04T04:37:00Z"
  },
  "systemStatus": {
    "state": "Ready",
    "isDeactive": false,
    "isAuthenticated": true,
    "currentUser": "admin",
    "currentRole": "Admin",
    "lastUpdate": "2026-06-04T04:37:00Z"
  },
  "productionPerHour": 240,
  "activeCodesTotal": 150
}
```

---

### Get Counters

```
GET /api/counters
Authorization: Bearer <token>
```

**Response:**
```json
{
  "camera": { "total": 150, "pass": 145, ... },
  "plc": { "total": 150, "pass": 150, ... },
  "productionPerHour": 240,
  "activeCodesTotal": 150
}
```

---

### Get Device Status

```
GET /api/status/devices
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "plcStatus": "Connected",
    "cameraStatus": "Connected",
    "scannerStatus": "Disconnected",
    "lastUpdate": "2026-06-04T04:37:00Z"
  },
  "timestamp": "..."
}
```

---

### Get System Status

```
GET /api/status/system
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "state": "Ready",
    "isDeactive": false,
    "isAuthenticated": true,
    "currentUser": "admin",
    "currentRole": "Admin",
    "lastUpdate": "2026-06-04T04:37:00Z"
  },
  "timestamp": "..."
}
```

---

## QR Codes

### Activate QR (from camera)

```
POST /api/qr/activate
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "qrContent": "ABCD1234567890TOL001"
}
```

**Response:**
```json
{
  "success": true,
  "status": "Pass",
  "qrContent": "ABCD1234567890TOL001",
  "reason": "Activated",
  "recordId": 42
}
```

**Status values:** `Pass`, `Fail`, `Error`, `Duplicate`, `NotFound`, `Timeout`, `ReadFail`, `FormatError`, `Deactive`

---

### Manual Add QR

```
POST /api/qr/manual-add
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "qrContent": "ABCD1234567890TOL001"
}
```

**Response:**
```json
{
  "success": true,
  "status": "Pass",
  "qrContent": "ABCD1234567890TOL001",
  "reason": "Manually added",
  "recordId": 43
}
```

---

### Search QR Code

```
GET /api/qr/search?code=ABCD1234
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "records": [
      {
        "id": 42,
        "qrContent": "ABCD1234567890TOL001",
        "batchCode": "260604-123456-TOL1-A",
        "barcode": "TOL001",
        "status": "Pass",
        "userName": "admin",
        "timeStampActive": "2026-06-04 11:37:00.000",
        "timeUnixActive": 1749020220000,
        "productionDatetime": "2026-06-04 11:37:00",
        "reason": ""
      }
    ],
    "isActive": true,
    "totalCount": 1
  },
  "timestamp": "..."
}
```

---

### Deactivate QR

```
POST /api/qr/deactivate
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "qrContent": "ABCD1234567890TOL001"
}
```

**Query params:** `?reason=Defective product`

**Response:**
```json
{ "success": true, "message": "Deactivated", "timestamp": "..." }
```

---

## Batch Management

### Get Current Batch

```
GET /api/batch/current
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "batchCode": "260604-123456-TOL1-A",
    "barcode": "TOL001",
    "lineName": "Line 1",
    "createdAt": "2026-06-04T04:30:00Z",
    "createdBy": "admin"
  },
  "timestamp": "..."
}
```

---

### Change Batch

```
POST /api/batch/change
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "batchCode": "260604-234567-TOL2-B",
  "barcode": "TOL002"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Batch changed",
  "data": {
    "batchCode": "260604-234567-TOL2-B",
    "barcode": "TOL002",
    "lineName": "Line 1",
    "createdAt": "2026-06-04T04:40:00Z",
    "createdBy": "admin"
  },
  "timestamp": "..."
}
```

---

### Get Batch List (from Excel)

```
GET /api/batch/list
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      { "batchCode": "260604-123456-TOL1-A", "barcode": "TOL001" },
      { "batchCode": "260604-234567-TOL2-B", "barcode": "TOL002" }
    ]
  },
  "timestamp": "..."
}
```

---

### Get Batch History

```
GET /api/batch/history?limit=50
Authorization: Bearer <token>
```

---

## System Control

### Deactivate System (Admin only)

```
POST /api/system/deactivate
Authorization: Bearer <token>  (Admin role required)
```

**Response:**
```json
{ "success": true, "message": "System deactivated", "timestamp": "..." }
```

---

### Reactivate System (Admin only)

```
POST /api/system/reactivate
Authorization: Bearer <token>  (Admin role required)
```

---

### Reset PLC Counters (Admin only)

```
POST /api/system/reset-counters
Authorization: Bearer <token>  (Admin role required)
```

---

### Clear Errors (Admin only)

```
POST /api/system/clear-errors
Authorization: Bearer <token>  (Admin role required)
```

---

## Logs & Config

### Get Activity Logs

```
GET /api/logs?count=100
Authorization: Bearer <token>
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "username": "admin",
      "logType": "UserAction",
      "message": "Login successful",
      "detail": "{'Role':'Admin'}",
      "code": "UA-LOGIN-01",
      "timestamp": "2026-06-04T04:37:00Z"
    }
  ],
  "timestamp": "..."
}
```

---

### Get Config

```
GET /api/config
Authorization: Bearer <token>
```

---

### Save Config (Admin only)

```
POST /api/config
Authorization: Bearer <token>  (Admin role required)
```

---

## WebSocket

Connect to `ws://localhost:5000/` for real-time updates.

**Send:**
```json
{ "type": "ping" }
{ "type": "dashboard" }
{ "type": "counters" }
```

**Receive:**
```json
{ "type": "pong", "timestamp": 1749020220000 }
{ "type": "dashboard", "data": { ... full dashboard object ... } }
{ "type": "counters", "data": { ... counters object ... } }
```

---

## Error Responses

All errors follow this format:

```json
{
  "success": false,
  "message": "Error description",
  "timestamp": "2026-06-04T04:37:00Z"
}
```

**HTTP Status Codes:**
- `200` - Success
- `400` - Bad Request
- `401` - Unauthorized (not authenticated)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `500` - Internal Server Error

---

## Roles

- **Admin** - Full access, can change batch, deactivate system, reset counters, save config
- **Operator** - Can activate/deactivate QR, search, view dashboard, change batch
- **Viewer** - Read-only access to dashboard, logs

---

## Data Folder Structure

```
%LOCALAPPDATA%\TSo\
  Configs\
    App.ini           - Application configuration
  Database\
    QRDatabase.db     - Full QR product records
    ActiveUnique.db   - Currently active unique QR codes
    Production\
      batch_history.db - Batch change history
  Users\
    users.database    - User accounts & 2FA secrets
  Logs\
    ALL\
      TSo.ptl         - Activity log file
```
