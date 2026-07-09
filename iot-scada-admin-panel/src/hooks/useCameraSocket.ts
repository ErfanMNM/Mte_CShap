import { useEffect, useRef, useState, useCallback } from "react";
import type {
  CameraCodeStatusMessage,
  CameraEventMessage,
  CameraSnapshot,
  ProductionStatus,
} from "../types/camera";

interface UseCameraSocketOptions {
  url: string;
  onEvent?: (msg: CameraEventMessage) => void;
  reconnectIntervalMs?: number;
  maxReconnectAttempts?: number;
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
        setSnapshot((s) => ({ ...s, connected: true }));
      };

      ws.onclose = () => {
        setSnapshot((s) => ({
          ...s,
          connected: false,
          camera: { ...s.camera, state: "Disconnected" },
        }));
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
            setSnapshot((s) => ({
              ...s,
              lastEventAt: codeMsg.at,
              [codeMsg.camera]: {
                state: "CodeScanned",
                lastCode: codeMsg.data || s[codeMsg.camera].lastCode,
                lastAt: codeMsg.at,
              },
            }));
            onEventRef.current?.(eventMsg);
            return;
          }

          const eventMsg = data as CameraEventMessage;
          pushLog(eventMsg);
          setSnapshot((s) => ({
            ...s,
            lastEventAt: eventMsg.at,
            [eventMsg.camera]: {
              state: eventMsg.state,
              lastCode:
                eventMsg.state === "Received"
                  ? eventMsg.data
                  : s[eventMsg.camera].lastCode,
              lastAt: eventMsg.at,
            },
          }));
          onEventRef.current?.(eventMsg);
        } catch {
          // ignore non-JSON frames
        }
      };
    } catch {
      // swallow - next reconnect attempt will retry
    }
  }, [url, reconnectIntervalMs, maxReconnectAttempts, pushLog]);

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
    setSnapshot(initialSnapshot);
    setLastScan(null);
    connect();
  }, [connect]);

  return { snapshot, logs, reconnect, lastScan };
}
