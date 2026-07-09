import { useEffect, useRef, useState, useCallback } from "react";
import type { PLCSnapshot, PLCState, PLCMessage } from "../types/plc";
import { useDeviceStore } from "../store/useDeviceStore";

interface UsePLCWebSocketOptions {
  url: string;
  reconnectIntervalMs?: number;
  maxReconnectAttempts?: number;
  /** If true, this hook will update the centralized device store.
   *  Set to false if using multiple instances (e.g., for logs only). */
  syncToStore?: boolean;
}

const initialSnapshot: PLCSnapshot = {
  state: undefined,
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
  syncToStore = true,
}: UsePLCWebSocketOptions) {
  const [snapshot, setSnapshot] = useState<PLCSnapshot>(initialSnapshot);
  const [logs, setLogs] = useState<PLCMessage[]>([]);
  const wsRef = useRef<WebSocket | null>(null);
  const attemptsRef = useRef(0);
  const retryRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const manualRef = useRef(false);

  // Centralized store reference (stable, doesn't cause re-renders in hooks)
  const storeSetPlc = useDeviceStore((s) => s.setPlc);

  const pushLog = useCallback((msg: PLCMessage) => {
    setLogs((prev) => [msg, ...prev].slice(0, 50));
  }, []);

  const connect = useCallback(() => {
    try {
      const ws = new WebSocket(url);
      wsRef.current = ws;

      ws.onopen = () => {
        attemptsRef.current = 0;
        const newSnapshot = { ...initialSnapshot, connected: true };
        setSnapshot(newSnapshot);
        if (syncToStore) storeSetPlc(newSnapshot);
      };

      ws.onclose = () => {
        const newSnapshot = { ...initialSnapshot, connected: false };
        setSnapshot(newSnapshot);
        if (syncToStore) storeSetPlc(newSnapshot);
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
          const newSnapshot: PLCSnapshot = {
            state: data.state as PLCState,
            connected: true,
            ip: data.ip ?? null,
            port: data.port ?? null,
            message: data.message ?? null,
            lastEventAt: data.at,
          };
          setSnapshot(newSnapshot);
          if (syncToStore) storeSetPlc(newSnapshot);
        } catch {
          // ignore
        }
      };
    } catch {
      // swallow
    }
  }, [url, reconnectIntervalMs, maxReconnectAttempts, pushLog, syncToStore, storeSetPlc]);

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
    if (syncToStore) storeSetPlc(resetSnapshot);
    connect();
  }, [connect, syncToStore, storeSetPlc]);

  return { snapshot, logs, reconnect };
}
