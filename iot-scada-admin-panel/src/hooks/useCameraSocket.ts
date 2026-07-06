import { useEffect, useRef, useState, useCallback } from "react";
import type {
  CameraEventMessage,
  CameraSnapshot,
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
        setSnapshot((s) => ({ ...s, connected: false }));
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
          const data = JSON.parse(ev.data as string) as CameraEventMessage;
          if (!data.camera || !data.state) return;
          pushLog(data);
          setSnapshot((s) => ({
            ...s,
            lastEventAt: data.at,
            [data.camera]: {
              state: data.state,
              lastCode:
                data.state === "Received" ? data.data : s[data.camera].lastCode,
              lastAt: data.at,
            },
          }));
          onEventRef.current?.(data);
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
    connect();
  }, [connect]);

  return { snapshot, logs, reconnect };
}
