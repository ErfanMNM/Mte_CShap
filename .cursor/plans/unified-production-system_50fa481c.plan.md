---
name: unified-production-system
overview: "Triển khai hệ thống unified production gồm: tối ưu State Machine 10ms, CartonWriteQueue serialize, ProductionHub WebSocket broadcast, sửa API, kết nối PLC counter, và cập nhật Frontend + Android PDA."
todos:
  - id: phase1-infra
    content: "Phase 1: Tối ưu State Machine 10ms + Thêm CartonCode schema"
    status: completed
  - id: phase2-carton
    content: "Phase 2: Carton Backend - Models, Queue, Handlers, Routes"
    status: completed
  - id: phase3-broadcast
    content: "Phase 3: State Broadcast - ProductionHub, WebSocket, SetState broadcast"
    status: completed
  - id: phase4-plc
    content: "Phase 4: PLC Integration - PLCMonitor đọc counter"
    status: completed
  - id: phase5-frontend
    content: "Phase 5: Frontend - Types, API, Hook, ProductionView"
    status: completed
  - id: phase6-android
    content: "Phase 6: Android PDA - ApiClient, MainActivity, Layout"
    status: completed
  - id: phase7-testing
    content: "Phase 7: Testing - Backend API + Frontend WebSocket"
    status: completed
isProject: false
---

# Kế hoạch: Unified Production System

## Mục tiêu
Triển khai hệ thống hoàn chỉnh gồm 2 phần chính:
- **Phần A (Carton PDA):** API quét thùng cho Android PDA + Queue serialization tránh race condition
- **Phần B (State Broadcast):** WebSocket broadcast production state real-time + sửa API + kết nối PLC counter

---

## Phase 1: Infrastructure

### 1. Tối ưu State Machine 100ms -> 10ms
- **File:** `GProject/Production/ProductionStateMachine.cs` line 136
- Đổi `Task.Delay(100, token).Wait(token)` thành `Task.Delay(10, token).Wait(token)`

### 2. Thêm bảng CartonCode vào Schema
- **File:** `GProject/ProductionOrderHelpers/Config.cs`
- Thêm bảng `CartonCode` với các columns: ID, MachineName, CartonCode, CartonIndex, ScanAt, Mode, Result

---

## Phase 2: Carton Backend

### 3. Tạo CartonModels.cs
- **File:** `GProject/ProductionOrderHelpers/CartonModels.cs` (MỚI)
- Các model: `CartonScanRequest`, `CartonScanResponse`, `CartonInfoResponse`, `CurrentPOResponse`

### 4. Tạo CartonWriteQueue.cs
- **File:** `GProject/ProductionOrderHelpers/CartonWriteQueue.cs` (MỚI)
- Singleton `CartonWriteQueue` với `BlockingCollection<CartonWriteTask>`
- Background thread consume tuần tự qua `ConsumeLoop()`
- Các method xử lý: `ProcessScanCarton`, `ProcessStartCarton`, `ProcessCompleteCarton`, `ProcessResetCarton`
- `CartonWriteQueueManager` với `ConcurrentDictionary<string, CartonWriteQueue>` (1 queue per PO)

### 5. Tạo POCartonCode.cs
- **File:** `GProject/ProductionOrderHelpers/POCartonCode.cs` (MỚI)
- `GetCartonInfo()` - lấy thông tin thùng
- `GetLastCarton()` - lấy thùng cuối cùng đã quét
- Các model: `CartonDetailInfo`, `CartonScanInfo`

### 6. Thêm Carton Handlers vào POApiServer.cs
- **File:** `GProject/ProductionOrderHelpers/POApiServer.cs`
- `HandleCartonScan()` - POST /api/carton/scan
- `HandleCartonInfo()` - GET /api/carton/{cartonCode}/info
- `HandleGetCurrentPO()` - GET /api/carton/current-po

### 7. Thêm POCartonCodeHelper vào GProduction.cs
- **File:** `GProject/ProductionOrderHelpers/GProduction.cs`
- Wrapper method `ScanCartonAsync()` gọi `CartonWriteQueueManager.EnqueueAsync()`

### 8. Đăng ký Carton Routes
- **File:** `GProject/GProjectApiServer.cs`
- `GET /api/carton/current-po`
- `POST /api/carton/scan`
- `GET /api/carton/{cartonCode}/info`

---

## Phase 3: State Broadcast Backend

### 9. Tạo ProductionHub.cs
- **File:** `GProject/ProductionHub.cs` (MỚI)
- Singleton pattern như `PLCHub`, `CameraHub`
- `ConcurrentDictionary<Guid, WebSocket>` quản lý clients
- `BroadcastStateAsync()` gửi payload JSON (state, counter, codes, cartons...)

### 10. Cập nhật ProductionStateMachine - Broadcast trong SetState()
- **File:** `GProject/Production/ProductionStateMachine.cs`
- Thêm field `_lastBroadcast` + `BroadcastThrottle` (500ms)
- `SetState()` gọi `BroadcastStateAsync()` ngay khi state đổi
- Throttle để tránh spam

### 11. Thêm /ws/production Endpoint
- **File:** `GProject/GProjectApiServer.cs`
- WebSocket endpoint tại `/ws/production`
- Register/unregister client với `ProductionHub.Instance`

### 12. Sửa HandleGetProductionStatus()
- **File:** `GProject/ProductionOrderHelpers/POApiServer.cs`
- Thay hardcoded `{ state: "Ready", hasPO: false }` bằng dữ liệu thực từ `ProductionStateMachine`
- Trả về: State, HasPO, OrderNo, ProductName, OrderQty, TotalCount, PassCount, FailCount, DuplicateCount, CartonCount, CartonClosedCount, CurrentCartonId, CurrentCartonCode, ItemsInCarton, CartonCapacity, ProgressPercent

---

## Phase 4: PLC Integration

### 13. PLCMonitor đọc counter thực từ PLC
- **File:** `GProject/PLCMonitor.cs`
- Viết lại `PollLoop()` đọc counter từ `D100` (env: `PLC_TOTAL_COUNT_DM`)
- Cập nhật vào `ProductionStateMachine.ActiveCounter`
- Cần thêm property `TotalCount` public vào `ActiveCounter`

---

## Phase 5: Frontend (React)

### 14. Thêm ProductionStateResponse type
- **File:** `iot-scada-admin-panel/src/types/production.ts`
- Interface khớp với backend: state, previousState, orderNo, productName, orderQty, activeCounter, codesCount, cartonsCount, lastWarning, isAppReady, isDeviceReady

### 15. Thêm getProductionState() API
- **File:** `iot-scada-admin-panel/src/services/productionApi.ts`
- Method gọi `/api/production/state`

### 16. Tạo useProductionWebSocket hook
- **File:** `iot-scada-admin-panel/src/hooks/useProductionWebSocket.ts` (MỚI)
- Auto-reconnect với exponential backoff
- State: `snapshot` (ProductionStateResponse), `connected` (boolean)

### 17. Cập nhật ProductionView.tsx
- **File:** `iot-scada-admin-panel/src/components/production/ProductionView.tsx`
- Import và sử dụng `useProductionWebSocket`
- Mở rộng `stateConfig` đủ 16 state với màu sắc phân biệt
- Thêm indicator `lastWarning` khi có cảnh báo
- Thêm WebSocket status indicator (online/offline)

---

## Phase 6: Android PDA

### 18. Cập nhật ApiClient.kt
- Thêm `CartonScanRequest`, `CartonScanResponse`, `CartonInfoResponse`, `CurrentPOResponse`
- Thêm interface methods: `postCartonScan()`, `getCartonInfo()`, `getCurrentPO()`

### 19. Cập nhật MainActivity.kt
- Tab **QUET THUNG** (default): gửi `/api/carton/scan`, hiển thị OK/WARN/ERR
- Tab **KIEM TRA THUNG**: gửi `/api/carton/{cartonCode}/info`, hiển thị activate date, số sản phẩm, người kích hoạt

### 20. Cập nhật activity_main.xml
- Sửa layout hỗ trợ 2 chế độ QUET THUNG / KIEM TRA

---

## Phase 7: Testing

### 21. Test Backend API
- Postman/curl test các endpoint `/api/carton/*` và `/api/production/status`
- Verify WebSocket `/ws/production` nhận được broadcast

### 22. Test Frontend
- Verify ProductionView kết nối WebSocket thành công
- Verify 16 state hiển thị đúng với màu sắc
- Verify counter update real-time

---

## Tổng hợp File Changes

**File MỚI (5 file):**
- `GProject/ProductionOrderHelpers/CartonWriteQueue.cs`
- `GProject/ProductionOrderHelpers/POCartonCode.cs`
- `GProject/ProductionOrderHelpers/CartonModels.cs`
- `GProject/ProductionHub.cs`
- `iot-scada-admin-panel/src/hooks/useProductionWebSocket.ts`

**File SỬA (12 file):**
- `GProject/Production/ProductionStateMachine.cs`
- `GProject/ProductionOrderHelpers/Config.cs`
- `GProject/ProductionOrderHelpers/POApiServer.cs`
- `GProject/ProductionOrderHelpers/GProduction.cs`
- `GProject/GProjectApiServer.cs`
- `GProject/PLCMonitor.cs`
- `iot-scada-admin-panel/src/types/production.ts`
- `iot-scada-admin-panel/src/services/productionApi.ts`
- `iot-scada-admin-panel/src/components/production/ProductionView.tsx`
- `MtePDA/app/.../ApiClient.kt`
- `MtePDA/app/.../MainActivity.kt`
- `MtePDA/app/.../activity_main.xml`
