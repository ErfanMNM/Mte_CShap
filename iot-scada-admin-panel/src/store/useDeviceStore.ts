/**
 * Centralized Device Store - Single source of truth for all device states from BE.
 * All WebSocket hooks update this store; components consume from it.
 * 
 * This eliminates scattered state mapping logic and ensures consistent
 * status display across all views (ScadaMonitorView, ProductionView, etc.)
 */
import { create } from "zustand";
import type { CameraSnapshot } from "../types/camera";
import type { PLCSnapshot } from "../types/plc";
import type { ProductionStateResponse } from "../types/production";

// ============================================================================
// Device Connection Status Types
// ============================================================================

export type ConnectionStatus = "connected" | "connecting" | "disconnected";

export interface DeviceStatus {
  status: ConnectionStatus;
  label: string;
  subLabel: string;
  ip?: string | null;
  port?: number | null;
}

// ============================================================================
// Store State
// ============================================================================

interface DeviceStoreState {
  // Raw snapshots from WebSocket hooks
  camera: CameraSnapshot;
  plc: PLCSnapshot;
  production: ProductionStateResponse | null;

  // Connection status
  cameraConnected: boolean;
  plcConnected: boolean;
  productionConnected: boolean;

  // /api/devices/status polling health (tracked separately from per-device flags)
  apiStatus: {
    error: boolean;
    statusCode: number | null;
    message: string | null;
  };
  // Số lần fail liên tiếp hiện tại
  failCount: number;
}

// ============================================================================
// Store Actions
// ============================================================================

interface DeviceStoreActions {
  // Update camera state (called by useCameraSocket)
  setCamera: (snapshot: CameraSnapshot) => void;
  // Update PLC state (called by usePLCWebSocket)
  setPlc: (snapshot: PLCSnapshot) => void;
  // Update production state (called by useProductionWebSocket)
  setProduction: (snapshot: ProductionStateResponse | null) => void;
  // Update production connection status
  setProductionConnected: (connected: boolean) => void;
  // Mark the devices-status API as failed (called after consecutive failures)
  setApiStatus: (status: { error: boolean; statusCode: number | null; message: string | null }) => void;
  // Increment fail count
  incrementFailCount: () => void;
  // Reset fail count to 0 (called on successful poll)
  resetFailCount: () => void;
  // Clear devices-status API error
  clearApiStatus: () => void;
  // Trigger an immediate poll of /api/devices/status (registered by useDevicePolling)
  manualPoll: () => Promise<void>;
  // Reset all state
  reset: () => void;
}

// ============================================================================
// Initial State
// ============================================================================

const initialCameraSnapshot: CameraSnapshot = {
  camera: { state: "Reconnecting", lastCode: "", lastAt: null },
  connected: false,
  clientCount: 0,
  lastEventAt: null,
};

const initialPlcSnapshot: PLCSnapshot = {
  state: undefined,
  connected: false,
  ip: null,
  port: null,
  message: null,
  lastEventAt: null,
  clientCount: 0,
};

const getInitialState = (): DeviceStoreState => ({
  camera: initialCameraSnapshot,
  plc: initialPlcSnapshot,
  production: null,
  cameraConnected: false,
  plcConnected: false,
  productionConnected: false,
  apiStatus: { error: false, statusCode: null, message: null },
  failCount: 0,
});

// ============================================================================
// Store Definition
// ============================================================================

export const useDeviceStore = create<DeviceStoreState & DeviceStoreActions>()(
  (set) => ({
    ...getInitialState(),

    setCamera: (snapshot) =>
      set({
        camera: snapshot,
        cameraConnected: snapshot.connected,
      }),

    setPlc: (snapshot) =>
      set({
        plc: snapshot,
        plcConnected: snapshot.connected,
      }),

    setProduction: (snapshot) =>
      set({ production: snapshot }),

    setProductionConnected: (connected) =>
      set({ productionConnected: connected }),

    setApiStatus: (status) => set({ apiStatus: status }),

    incrementFailCount: () => set((s) => ({ failCount: s.failCount + 1 })),

    resetFailCount: () => set({ failCount: 0 }),

    clearApiStatus: () =>
      set({ apiStatus: { error: false, statusCode: null, message: null } }),

    // Default no-op; useDevicePolling registers the real implementation on mount.
    manualPoll: async () => {},

    reset: () => set(getInitialState()),
  })
);

// ============================================================================
// Derived State Selectors (computed from raw snapshots)
// ============================================================================

/**
 * Camera status with human-readable labels.
 * Call this in components to get display-ready data.
 */
export const useCameraStatus = (): DeviceStatus => {
  const { camera, cameraConnected } = useDeviceStore();
  const { state, lastCode, lastAt } = camera.camera;

  const s = state?.toLowerCase();
  
  // Determine connection status
  let status: ConnectionStatus;
  if (!cameraConnected) {
    status = "connecting";
  } else if (s === "connected" || s === "received" || s === "codescanned") {
    status = "connected";
  } else if (s === "reconnecting" || s === "disconnected" || s === "deactive") {
    status = "disconnected";
  } else {
    status = "connected";
  }

  // Determine labels
  const labelMap: Record<string, string> = {
    connected: "Đã kết nối",
    received: "Đang hoạt động",
    reconnecting: "Đang kết nối lại",
    disconnected: "Mất kết nối",
    codescanned: "Đang hoạt động",
  };

  return {
    status,
    label: "CAMERA",
    subLabel: labelMap[s ?? ""] ?? (cameraConnected ? "Đã kết nối" : "Đang kết nối"),
    ip: null,
    port: null,
  };
};

/**
 * PLC status with human-readable labels.
 * Call this in components to get display-ready data.
 */
export const usePlcStatus = (): DeviceStatus => {
  const { plc } = useDeviceStore();
  const { state, connected, ip, port, message } = plc;

  const s = state?.toLowerCase();

  // Determine connection status
  let status: ConnectionStatus;
  if (!connected && !state) {
    status = "connecting";
  } else if (s === "disconnected") {
    status = "disconnected";
  } else if (s === "reconnecting") {
    status = "connecting";
  } else if (connected) {
    status = "connected";
  } else {
    status = "connecting";
  }

  // Determine labels
  const labelMap: Record<string, string> = {
    connected: "Đã kết nối",
    reconnecting: "Đang kết nối lại",
    disconnected: "Mất kết nối",
  };

  return {
    status,
    label: "PLC OMRON",
    subLabel: labelMap[s ?? ""] ?? message ?? (connected ? "Đã kết nối" : "Đang kết nối"),
    ip,
    port,
  };
};

/**
 * Application (Production) status with human-readable labels.
 */
export interface AppStatus {
  status: ConnectionStatus;
  label: string;
  state: string;
  previousState: string;
  orderNo: string | null;
  productName: string | null;
  orderQty: number;
  lastWarning: string;
  isAppReady: boolean;
  isDeviceReady: boolean;
}

export const useAppStatus = (): AppStatus => {
  const { production, productionConnected } = useDeviceStore();

  let status: ConnectionStatus;
  if (!productionConnected) {
    status = "connecting";
  } else if (!production?.state || production.state === "Unknown") {
    status = "connecting";
  } else if (
    production.state === "Running" ||
    production.state === "Ready" ||
    production.state === "Paused" ||
    production.state === "Completed"
  ) {
    status = "connected";
  } else if (production.state === "Error" || production.state === "DeviceError") {
    status = "disconnected";
  } else {
    status = "connected";
  }

  return {
    status,
    label: "UNG DUNG",
    state: production?.state ?? "Unknown",
    previousState: production?.previousState ?? "Unknown",
    orderNo: production?.orderNo ?? null,
    productName: production?.productName ?? null,
    orderQty: production?.orderQty ?? 0,
    lastWarning: production?.lastWarning ?? "",
    isAppReady: production?.isAppReady ?? false,
    isDeviceReady: production?.isDeviceReady ?? false,
  };
};
