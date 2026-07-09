import { useEffect, useRef, useState, useCallback } from "react";
import type { ProductionStateResponse } from "../types/production";
import { useDeviceStore } from "../store/useDeviceStore";

interface UseProductionWebSocketOptions {
  url: string;
  reconnectIntervalMs?: number;
  maxReconnectAttempts?: number;
  /** If true, this hook will update the centralized device store.
   *  Set to false if using multiple instances (e.g., for logs only). */
  syncToStore?: boolean;
}

const initialSnapshot: ProductionStateResponse = {
  success: false,
  state: "Unknown",
  previousState: "Unknown",
  orderNo: "",
  productName: "",
  orderQty: 0,
  activeCounter: { PassCount: 0, FailCount: 0, DuplicateCount: 0, CartonID: 0, CartonCode: "" },
  codesCount: 0,
  cartonsCount: 0,
  lastWarning: "",
  isAppReady: false,
  isDeviceReady: false,
};

export function useProductionWebSocket({
  url,
  reconnectIntervalMs = 3000,
  maxReconnectAttempts = 10,
  syncToStore = true,
}: UseProductionWebSocketOptions) {
  const [snapshot, setSnapshot] = useState<ProductionStateResponse>(initialSnapshot);
  const [connected, setConnected] = useState(false);
  const wsRef = useRef<WebSocket | null>(null);
  const attemptsRef = useRef(0);
  const retryRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const manualRef = useRef(false);

  // Centralized store references (stable, doesn't cause re-renders in hooks)
  const storeSetProduction = useDeviceStore((s) => s.setProduction);
  const storeSetProductionConnected = useDeviceStore((s) => s.setProductionConnected);

  const connect = useCallback(() => {
    try {
      const ws = new WebSocket(url);
      wsRef.current = ws;

      ws.onopen = () => {
        attemptsRef.current = 0;
        setConnected(true);
        if (syncToStore) storeSetProductionConnected(true);
      };

      ws.onclose = () => {
        setConnected(false);
        if (syncToStore) storeSetProductionConnected(false);
        if (!manualRef.current && attemptsRef.current < maxReconnectAttempts) {
          const delay = reconnectIntervalMs * Math.pow(1.5, attemptsRef.current);
          retryRef.current = setTimeout(() => {
            attemptsRef.current++;
            connect();
          }, delay);
        }
      };

      ws.onerror = () => {
        // errors surface through onclose
      };

      ws.onmessage = (ev) => {
        try {
          const data = JSON.parse(ev.data as string) as ProductionStateResponse;
          if (data.state) {
            setSnapshot(data);
            if (syncToStore) storeSetProduction(data);
          }
        } catch {
          // ignore non-JSON frames
        }
      };
    } catch {
      // swallow — next reconnect attempt will retry
    }
  }, [url, reconnectIntervalMs, maxReconnectAttempts, syncToStore, storeSetProduction, storeSetProductionConnected]);

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
    setConnected(false);
    if (syncToStore) {
      storeSetProduction(null);
      storeSetProductionConnected(false);
    }
    connect();
  }, [connect, syncToStore, storeSetProduction, storeSetProductionConnected]);

  return { snapshot, connected, reconnect };
}
