import { useEffect, useRef, useState, useCallback } from "react";
import type {
  CameraCodeStatusMessage,
  CameraEventMessage,
  CameraSnapshot,
  ProductionStatus,
} from "../types/camera";
import { useDeviceStore } from "../store/useDeviceStore";

interface UseCameraSocketOptions {
  url: string;
  onEvent?: (msg: CameraEventMessage) => void;
  reconnectIntervalMs?: number;
  maxReconnectAttempts?: number;
  /** If true, this hook will update the centralized device store.
   *  Set to false if using multiple instances (e.g., for logs only). */
  syncToStore?: boolean;
}

interface CameraLogEntry {
  id: string;
  time: Date;
  msg: CameraEventMessage;
}

// Snapshot gọn về mã vừa được ProductionStateMachine xử lý xong — dùng để render badge "TỐT/TRÙNG/LỖI".
export interface LastScan {
  code: string;
  status: ProductionStatus;
  plcSent: boolean;
  cartonCode: string | null;
  cartonId: number | null;
  at: string;
}

const initialSnapshot: CameraSnapshot = {
  camera: { state: "Reconnecting", lastCode: "", lastAt: null },
  connected: false,
  clientCount: 0,
  lastEventAt: null,
};

export function useCameraSocket({
  url,
  onEvent,
  reconnectIntervalMs = 3000,
  maxReconnectAttempts = 10,
  syncToStore = true,
}: UseCameraSocketOptions) {
  const [snapshot, setSnapshot] = useState<CameraSnapshot>(initialSnapshot);
  const [logs, setLogs] = useState<CameraLogEntry[]>([]);
  const [lastScan, setLastScan] = useState<LastScan | null>(null);
  const wsRef = useRef<WebSocket | null>(null);
  const attemptsRef = useRef(0);
  const retryRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const manualRef = useRef(false);
  const onEventRef = useRef(onEvent);
  onEventRef.current = onEvent;

  // Centralized store reference (stable, doesn't cause re-renders in hooks)
  const storeSetCamera = useDeviceStore((s) => s.setCamera);

  const pushLog = useCallback((msg: CameraEventMessage) => {
    setLogs((prev) =>
      [
        {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          time: new Date(),
          msg,
        },
        ...prev,
      ].slice(0, 100)
    );
  }, []);

  const connect = useCallback(() => {
    try {
      const ws = new WebSocket(url);
      wsRef.current = ws;

      ws.onopen = () => {
        attemptsRef.current = 0;
        const newSnapshot = { ...initialSnapshot, connected: true };
        setSnapshot(newSnapshot);
        if (syncToStore) storeSetCamera(newSnapshot);
      };

      ws.onclose = () => {
        const newSnapshot = {
          ...initialSnapshot,
          connected: false,
          camera: { ...initialSnapshot.camera, state: "Disconnected" },
        };
        setSnapshot(newSnapshot);
        if (syncToStore) storeSetCamera(newSnapshot);
        if (!manualRef.current && attemptsRef.current < maxReconnectAttempts) {
          const delay = reconnectIntervalMs * Math.pow(1.5, attemptsRef.current);
          retryRef.current = setTimeout(() => {
            attemptsRef.current++;
            connect();
          }, delay);
        }
      };

      ws.onerror = () => {
        // errors are surfaced through onclose + UI state
      };

      ws.onmessage = (ev) => {
        try {
          const data = JSON.parse(ev.data as string);
          if (!data || !data.camera || !data.state) return;

          // Event "CodeScanned" — lưu status mã cho badge TỐT/TRÙNG/LỖI.
          // Vẫn đẩy vào logs để tab THÔNG BÁO CHUNG hiển thị.
          if (data.state === "CodeScanned") {
            const codeMsg = data as CameraCodeStatusMessage;
            setLastScan({
              code: codeMsg.data ?? "",
              status: codeMsg.status,
              plcSent: !!codeMsg.plcSent,
              cartonCode: codeMsg.cartonCode ?? null,
              cartonId: codeMsg.cartonId ?? null,
              at: codeMsg.at,
            });
            const eventMsg: CameraEventMessage = {
              camera: codeMsg.camera,
              state: "CodeScanned",
              data: codeMsg.data,
              at: codeMsg.at,
            };
            pushLog(eventMsg);
            setSnapshot((s) => {
              const newSnapshot = {
                ...s,
                lastEventAt: codeMsg.at,
                camera: {
                  state: "CodeScanned" as const,
                  lastCode: codeMsg.data || s.camera.lastCode,
                  lastAt: codeMsg.at,
                },
              };
              if (syncToStore) storeSetCamera(newSnapshot);
              return newSnapshot;
            });
            onEventRef.current?.(eventMsg);
            return;
          }

          const eventMsg = data as CameraEventMessage;
          pushLog(eventMsg);
          setSnapshot((s) => {
            const newSnapshot = {
              ...s,
              lastEventAt: eventMsg.at,
              camera: {
                state: eventMsg.state,
                lastCode:
                  eventMsg.state === "Received"
                    ? eventMsg.data
                    : s.camera.lastCode,
                lastAt: eventMsg.at,
              },
            };
            if (syncToStore) storeSetCamera(newSnapshot);
            return newSnapshot;
          });
          onEventRef.current?.(eventMsg);
        } catch {
          // ignore non-JSON frames
        }
      };
    } catch {
      // swallow - next reconnect attempt will retry
    }
  }, [url, reconnectIntervalMs, maxReconnectAttempts, pushLog, syncToStore, storeSetCamera]);

  useEffect(() => {
    connect();
    return () => {
      manualRef.current = true;
      if (retryRef.current) clearTimeout(retryRef.current);
      wsRef.current?.close(1000, "unmount");
    };
  }, [connect]);

  const reconnect = useCallback(() => {
    manualRef.current = true;
    if (retryRef.current) clearTimeout(retryRef.current);
    wsRef.current?.close(1000, "manual");
    manualRef.current = false;
    attemptsRef.current = 0;
    const resetSnapshot = { ...initialSnapshot };
    setSnapshot(resetSnapshot);
    setLastScan(null);
    if (syncToStore) storeSetCamera(resetSnapshot);
    connect();
  }, [connect, syncToStore, storeSetCamera]);

  return { snapshot, logs, reconnect, lastScan };
}
