// TypeScript interfaces for camera WebSocket events from GProject backend
// Matches CameraHub.BroadcastAsync + BroadcastCodeStatus payload in GProject/CameraHub.cs

export type CameraName = "camera";

// Connection states (OmronCodeReader.eOmronCodeReaderState) + new "CodeScanned" event.
export type CameraState =
  | "Connected"
  | "Disconnected"
  | "Received"
  | "Reconnecting"
  | "CodeScanned";

// Trạng thái mã khi camera quét — khớp với enum e_Production_Status trong GProject/CameraHub.cs
export type ProductionStatus =
  | "Pass"
  | "Fail"
  | "Duplicate"
  | "NotFound"
  | "FormatError"
  | "Error"
  | "ReadFail"
  | "Timeout";

export interface CameraEventMessage {
  camera: CameraName;
  state: CameraState;
  data: string;
  at: string; // ISO UTC timestamp from server
}

// Event phát khi camera quét được 1 mã và đã được ProductionStateMachine xử lý
// (state = "CodeScanned"). FE dùng status để render badge TỐT/TRÙNG/KHÔNG ĐỌC/LỖI.
export interface CameraCodeStatusMessage {
  camera: CameraName;
  state: "CodeScanned";
  data: string;
  status: ProductionStatus;
  plcSent: boolean;
  cartonCode: string | null;
  cartonId: number | null;
  at: string;
}

export interface CameraChannelSnapshot {
  state: CameraState;
  lastCode: string;
  lastAt: string | null;
}

export interface CameraSnapshot {
  camera: CameraChannelSnapshot;
  connected: boolean;
  clientCount: number;
  lastEventAt: string | null;
}

// Mục trong /api/camera-history — ring buffer phía server (200 entry gần nhất).
export interface CameraHistoryItem {
  id: number;
  at: string;
  code: string;
  status: ProductionStatus;
  plcSent: boolean;
  cartonCode: string | null;
  cartonId: number | null;
}

export interface CameraHistoryResponse {
  success: boolean;
  count: number;
  items: CameraHistoryItem[];
}
