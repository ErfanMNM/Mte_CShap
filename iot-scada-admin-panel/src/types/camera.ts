// TypeScript interfaces for camera WebSocket events from GProject backend
// Matches CameraHub.BroadcastAsync payload in GProject/CameraHub.cs

export type CameraName = "camera";

export type CameraState =
  | "Connected"
  | "Disconnected"
  | "Received"
  | "Reconnecting";

export interface CameraEventMessage {
  camera: CameraName;
  state: CameraState;
  data: string;
  at: string; // ISO UTC timestamp from server
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
