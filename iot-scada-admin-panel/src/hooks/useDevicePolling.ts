/**
 * REST polling hook for device status.
 * Polls GET /api/devices/status every `intervalMs` ms and updates the centralized device store.
 *
 * Replaces the 3 WebSocket hooks:
 *   - useCameraSocket (store sync only)
 *   - usePLCWebSocket (store sync only)
 *   - useProductionWebSocket (store sync only)
 *
 * The Camera WebSocket is still used for:
 *   - onEvent callback (immediate code scan notification for ProductionView)
 *   - Log table in ScadaMonitorView
 */
import { useEffect, useRef } from "react";
import { useDeviceStore } from "../store/useDeviceStore";
import type { CameraSnapshot } from "../types/camera";
import type { PLCSnapshot } from "../types/plc";
import type { ProductionStateResponse } from "../types/production";

/** Số lần fail liên tiếp trước khi hiện dialog cảnh báo (polling 2000ms × 5 ≈ 10s). */
const FAIL_THRESHOLD = 5;

export interface UseDevicePollingOptions {
  /** Polling interval in ms. Default: 2000ms. */
  intervalMs?: number;
  /** Enable/disable polling. Default: true. */
  enabled?: boolean;
  /** Base URL override. Default: VITE_API_BASE_URL or localhost:9999. */
  baseUrl?: string;
}

export interface DeviceStatusResponse {
  success: boolean;
  at: string;
  camera: {
    state: string;
    ip: string;
    port: number;
    connected: boolean;
    lastCode: string;
    lastCodeAt: string | null;
    lastEventAt: string | null;
    clientCount: number;
  };
  plc: {
    state: string;
    ip: string | null;
    port: number | null;
    connected: boolean;
    clientCount: number;
  };
  production: {
    state: string;
    previousState: string;
    orderNo: string;
    productName: string;
    productionDate: string;
    orderQty: number;
    activeCounter: {
      PassTotal: number;
      FailTotal: number;
      DuplicateCount: number;
      NotFoundCount: number;
      ReadFailCount: number;
      FormatFailCount: number;
      ErrorCount: number;
      TimeoutCount: number;
      TotalCount: number;
      CartonID: number;
      CartonCode: string;
    };
    cartonCount: number;
    cartonClosedCount: number;
    itemsInCarton: number;
    cartonCapacity: number;
    progressPercent: number;
    hasPO: boolean;
    codesCount: number;
    cartonsCount: number;
    lastWarning: string;
    isAppReady: boolean;
    isDeviceReady: boolean;
    clientCount: number;
  };
}

export function useDevicePolling(options: UseDevicePollingOptions = {}) {
  const { intervalMs = 2000, enabled = true, baseUrl } = options;

  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const setCamera = useDeviceStore((s) => s.setCamera);
  const setPlc = useDeviceStore((s) => s.setPlc);
  const setProduction = useDeviceStore((s) => s.setProduction);
  const setProductionConnected = useDeviceStore((s) => s.setProductionConnected);

  useEffect(() => {
    if (!enabled) return;

    const base = baseUrl
      ?? (import.meta.env.VITE_API_BASE_URL as string | undefined)
      ?? "http://localhost:9999";
    const url = `${base}/api/devices/status`;

    const failCountRef = { current: 0 };

    const fetchStatus = async () => {
      const storeState = useDeviceStore.getState();

      let ok = false;
      let statusCode: number | null = null;
      let message: string | null = null;

      try {
        const res = await fetch(url);
        if (!res.ok) {
          statusCode = res.status;
          message = `HTTP ${res.status}`;
          console.warn("[DevicePolling] HTTP error:", res.status);
        } else {
          const data: DeviceStatusResponse = await res.json();
          if (!data.success) {
            message = "API success=false";
            console.warn("[DevicePolling] API returned success=false");
          } else {
            ok = true;

            // Clear error ONLY on successful response
            storeState.clearApiStatus();
            storeState.resetFailCount();

            // --- Update Camera store ---
            const cameraSnapshot: CameraSnapshot = {
              camera: {
                state: data.camera.state as CameraSnapshot["camera"]["state"],
                lastCode: data.camera.lastCode,
                lastAt: data.camera.lastCodeAt,
              },
              connected: data.camera.connected,
              clientCount: data.camera.clientCount,
              lastEventAt: data.camera.lastEventAt,
            };
            setCamera(cameraSnapshot);

            // --- Update PLC store ---
            const plcSnapshot: PLCSnapshot = {
              state: data.plc.state as PLCSnapshot["state"],
              connected: data.plc.connected,
              ip: data.plc.ip,
              port: data.plc.port,
              message: null,
              lastEventAt: null,
              clientCount: data.plc.clientCount,
            };
            setPlc(plcSnapshot);

            // --- Update Production store ---
            const prodSnapshot: ProductionStateResponse = {
              success: true,
              state: data.production.state,
              previousState: data.production.previousState,
              orderNo: data.production.orderNo,
              productName: data.production.productName,
              productionDate: data.production.productionDate,
              orderQty: data.production.orderQty,
              activeCounter: data.production.activeCounter,
              cartonCount: data.production.cartonCount,
              cartonClosedCount: data.production.cartonClosedCount,
              itemsInCarton: data.production.itemsInCarton,
              cartonCapacity: data.production.cartonCapacity,
              progressPercent: data.production.progressPercent,
              hasPO: data.production.hasPO,
              codesCount: data.production.codesCount,
              cartonsCount: data.production.cartonsCount,
              lastWarning: data.production.lastWarning,
              isAppReady: data.production.isAppReady,
              isDeviceReady: data.production.isDeviceReady,
            };
            setProduction(prodSnapshot);
            setProductionConnected(
              !!data.production.state && data.production.state !== "Unknown"
            );
          }
        }
      } catch (err) {
        message = "network";
        console.warn("[DevicePolling] Fetch error:", err);
      }

      if (ok) {
        failCountRef.current = 0;
        // Store already updated above on success
      } else {
        failCountRef.current += 1;
        storeState.incrementFailCount();
        if (failCountRef.current >= FAIL_THRESHOLD) {
          storeState.setApiStatus({ error: true, statusCode, message });
        }
      }
    };

    // Expose manual trigger so UI (e.g. retry button) can request a poll immediately.
    // The reset above always clears the current error so the spinner visually acknowledges the retry attempt.
    useDeviceStore.setState({ manualPoll: fetchStatus });

    // Poll immediately on mount
    fetchStatus();

    timerRef.current = setInterval(fetchStatus, intervalMs);

    return () => {
      // Detach manual trigger so it doesn't outlive this hook instance (e.g. StrictMode double-invoke).
      useDeviceStore.setState({ manualPoll: async () => {} });
      if (timerRef.current) {
        clearInterval(timerRef.current);
        timerRef.current = null;
      }
    };
  }, [intervalMs, enabled, baseUrl, setCamera, setPlc, setProduction, setProductionConnected]);
}
