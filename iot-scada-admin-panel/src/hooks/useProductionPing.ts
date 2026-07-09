/**
 * Hook ping production state định kỳ để đảm bảo FE luôn có state mới nhất.
 * Call /api/production/ping mỗi interval, BE sẽ broadcast state ngay lập tức qua WebSocket.
 * 
 * Sử dụng khi WebSocket có thể miss message hoặc cần đảm bảo state sync.
 */
import { useEffect, useRef } from "react";
import { useDeviceStore } from "../store/useDeviceStore";

interface UseProductionPingOptions {
  /** URL của API ping (mặc định: /api/production/ping) */
  apiUrl?: string;
  /** Interval ping (ms), mặc định 5000ms = 5s */
  intervalMs?: number;
  /** Enable/disable ping */
  enabled?: boolean;
}

export function useProductionPing({
  apiUrl,
  intervalMs = 5000,
  enabled = true,
}: UseProductionPingOptions = {}) {
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const storeSetProduction = useDeviceStore((s) => s.setProduction);
  const storeSetProductionConnected = useDeviceStore((s) => s.setProductionConnected);

  useEffect(() => {
    if (!enabled) return;

    const baseUrl = apiUrl?.replace("/api/production/ping", "") 
      ?? (import.meta.env.VITE_API_BASE_URL as string | undefined) 
      ?? "http://localhost:9999";
    const pingUrl = `${baseUrl}/api/production/ping`;

    const ping = async () => {
      try {
        const res = await fetch(pingUrl, { 
          method: "GET",
          headers: { "Content-Type": "application/json" }
        });
        if (res.ok) {
          const data = await res.json();
          // State sẽ được broadcast qua WebSocket và cập nhật store tự động
          console.debug("[ProductionPing] OK:", data.at);
        } else {
          console.warn("[ProductionPing] Failed:", res.status);
        }
      } catch (err) {
        console.warn("[ProductionPing] Error:", err);
        // Nếu ping thất bại nhiều lần, có thể BE đang offline
        // Cập nhật connected = false để UI hiển thị đúng
        if (timerRef.current) {
          // Đếm số lần thất bại để xác định offline
        }
      }
    };

    // Ping ngay lập tức khi enable
    ping();

    // Setup interval
    timerRef.current = setInterval(ping, intervalMs);

    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
        timerRef.current = null;
      }
    };
  }, [apiUrl, intervalMs, enabled, storeSetProduction, storeSetProductionConnected]);
}
