import { useEffect, useRef, useState, useCallback } from "react";
import type { PLCSnapshot, PLCState, PLCMessage } from "../types/plc";

interface UsePLCWebSocketOptions {
  url: string;
  reconnectIntervalMs?: number;
  maxReconnectAttempts?: number;
}

const initialSnapshot: PLCSnapshot = {
  state: "Disconnected",
  connected: false,
  ip: null,
  port: null,
  message: null,
  lastEventAt: null,
};

export function usePLCWebSocket({
  url,
  reconnectIntervalMs = 3000,
  maxReconnectAttempts = 5,
}: UsePLCWebSocketOptions) {
  const [snapshot, setSnapshot] = useState<PLCSnapshot>(initialSnapshot);
  const [logs, setLogs] = useState<PLCMessage[]>([]);
  const wsRef = useRef<WebSocket | null>(null);
  const attemptsRef = useRef(0);
  const retryRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const manualRef = useRef(false);

  const pushLog = useCallback((msg: PLCMessage) => {
    setLogs((prev) => [msg, ...prev].slice(0, 50));
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
        // surfaced through onclose
      };

      ws.onmessage = (ev) => {
        try {
          const data = JSON.parse(ev.data as string) as PLCMessage;
          if (!data.state) return;
          pushLog(data);
          setSnapshot((s) => ({
            ...s,
            state: data.state as PLCState,
            message: data.message ?? null,
            ip: data.ip ?? s.ip,
            port: data.port ?? s.port,
            lastEventAt: data.at,
          }));
        } catch {
          // ignore
        }
      };
    } catch {
      // swallow
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
