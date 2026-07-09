// TypeScript interfaces for PLC WebSocket events from GProject backend
// Matches PLCHub.BroadcastStateAsync payload in GProject/PLCHub.cs

export type PLCState =
  | "Connected"
  | "Disconnected"
  | "Reconnecting";

export interface PLCMessage {
  state: PLCState;
  message: string | null;
  ip: string;
  port: number;
  at: string;
}

export interface PLCSnapshot {
  state: PLCState | undefined;
  connected: boolean;
  ip: string | null;
  port: number | null;
  message: string | null;
  lastEventAt: string | null;
  clientCount: number;
}
