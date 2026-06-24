import { useState, useEffect, useCallback, useRef } from "react";

export interface DeviceStatus {
  type: "device_status";
  timestamp: string;
  camera: {
    state: string;
    ip: string;
  };
  plc: {
    state: string;
    connected: boolean;
  };
  app: {
    state: string;
  };
}

export interface LogEntry {
  id: string;
  time: Date;
  message: string;
  source: "camera" | "plc" | "ws" | "app" | "system";
  type: "info" | "warning" | "error" | "success";
}

export interface UseWebSocketOptions {
  url: string;
  reconnectInterval?: number;
  maxReconnectAttempts?: number;
  onMessage?: (data: DeviceStatus) => void;
  onLog?: (entry: LogEntry) => void;
}

export interface UseWebSocketReturn {
  isConnected: boolean;
  clientCount: number;
  lastMessage: DeviceStatus | null;
  logs: LogEntry[];
  sendMessage: (message: string) => void;
  reconnect: () => void;
}

const createLogEntry = (
  message: string,
  source: LogEntry["source"],
  type: LogEntry["type"] = "info"
): LogEntry => ({
  id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
  time: new Date(),
  message,
  source,
  type,
});

export function useWebSocket({
  url,
  reconnectInterval = 3000,
  maxReconnectAttempts = 10,
  onMessage,
  onLog,
}: UseWebSocketOptions): UseWebSocketReturn {
  const [isConnected, setIsConnected] = useState(false);
  const [clientCount, setClientCount] = useState(0);
  const [lastMessage, setLastMessage] = useState<DeviceStatus | null>(null);
  const [logs, setLogs] = useState<LogEntry[]>([]);

  const wsRef = useRef<WebSocket | null>(null);
  const reconnectAttemptsRef = useRef(0);
  const reconnectTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isManualCloseRef = useRef(false);

  const addLog = useCallback(
    (entry: LogEntry) => {
      setLogs((prev) => [entry, ...prev].slice(0, 100));
      onLog?.(entry);
    },
    [onLog]
  );

  const connect = useCallback(() => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      return;
    }

    try {
      wsRef.current = new WebSocket(url);

      wsRef.current.onopen = () => {
        setIsConnected(true);
        reconnectAttemptsRef.current = 0;
        addLog(
          createLogEntry(`Đã kết nối WebSocket: ${url}`, "ws", "success")
        );
      };

      wsRef.current.onclose = (event) => {
        setIsConnected(false);
        setClientCount(0);

        if (!isManualCloseRef.current) {
          addLog(
            createLogEntry(
              `WebSocket đóng: ${event.reason || "Không rõ lý do"}`,
              "ws",
              "warning"
            )
          );

          if (reconnectAttemptsRef.current < maxReconnectAttempts) {
            const delay = reconnectInterval * Math.pow(1.5, reconnectAttemptsRef.current);
            addLog(
              createLogEntry(
                `Thử kết nối lại sau ${Math.round(delay / 1000)}s... (${reconnectAttemptsRef.current + 1}/${maxReconnectAttempts})`,
                "ws",
                "info"
              )
            );

            reconnectTimeoutRef.current = setTimeout(() => {
              reconnectAttemptsRef.current++;
              connect();
            }, delay);
          } else {
            addLog(
              createLogEntry(
                `Không thể kết nối sau ${maxReconnectAttempts} lần thử`,
                "ws",
                "error"
              )
            );
          }
        }
      };

      wsRef.current.onerror = (error) => {
        console.error("WebSocket error:", error);
        addLog(createLogEntry("Lỗi kết nối WebSocket", "ws", "error"));
      };

      wsRef.current.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data) as DeviceStatus;

          if (data.type === "device_status") {
            setLastMessage(data);
            onMessage?.(data);

            if (data.camera) {
              const cameraState =
                data.camera.state === "Connected"
                  ? "success"
                  : data.camera.state === "Disconnected"
                    ? "error"
                    : "warning";
              addLog(
                createLogEntry(
                  `Camera: ${data.camera.state}`,
                  "camera",
                  cameraState
                )
              );
            }

            if (data.plc) {
              const plcState = data.plc.connected
                ? "success"
                : "error";
              addLog(
                createLogEntry(
                  `PLC: ${data.plc.connected ? "Kết nối" : "Mất kết nối"}`,
                  "plc",
                  plcState
                )
              );
            }
          }

          if (event.data.includes("Client connected")) {
            setClientCount((prev) => prev + 1);
            const match = event.data.match(/Total:\s*(\d+)/);
            if (match) {
              setClientCount(parseInt(match[1], 10));
            }
          }

          if (event.data.includes("Client removed") || event.data.includes("dead client")) {
            setClientCount((prev) => Math.max(0, prev - 1));
          }
        } catch {
          if (typeof event.data === "string" && !event.data.startsWith("{")) {
            addLog(createLogEntry(event.data, "ws", "info"));
          }
        }
      };
    } catch (error) {
      addLog(
        createLogEntry(`Không thể khởi tạo WebSocket: ${error}`, "ws", "error")
      );
    }
  }, [url, reconnectInterval, maxReconnectAttempts, addLog, onMessage]);

  const disconnect = useCallback(() => {
    isManualCloseRef.current = true;

    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
      reconnectTimeoutRef.current = null;
    }

    if (wsRef.current) {
      wsRef.current.close(1000, "Manual disconnect");
      wsRef.current = null;
    }

    setIsConnected(false);
    addLog(createLogEntry("Đã ngắt kết nối WebSocket", "ws", "info"));
  }, [addLog]);

  const sendMessage = useCallback((message: string) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(message);
      addLog(createLogEntry(`Gửi: ${message}`, "ws", "info"));
    } else {
      addLog(
        createLogEntry("Không thể gửi: WebSocket chưa kết nối", "ws", "warning")
      );
    }
  }, [addLog]);

  const reconnect = useCallback(() => {
    disconnect();
    isManualCloseRef.current = false;
    reconnectAttemptsRef.current = 0;
    connect();
  }, [disconnect, connect]);

  useEffect(() => {
    connect();
    return () => {
      disconnect();
    };
  }, [connect, disconnect]);

  return {
    isConnected,
    clientCount,
    lastMessage,
    logs,
    sendMessage,
    reconnect,
  };
}
