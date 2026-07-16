import React, { useState, useEffect, useMemo } from "react";
import {
  CheckCircle2,
  AlertCircle,
  Wifi,
  WifiOff,
  Server,
  Monitor,
  Settings,
  Cpu,
  RotateCcw,
  Plus,
  Trash2,
  AlertTriangle,
  Play,
  Pause,
  ChevronDown,
  Activity,
  History,
  QrCode,
  LayoutDashboard,
  Users,
  Search,
  Bell,
  Menu,
  Factory,
  Database,
  BarChart2,
  Keyboard as KeyboardIcon,
  RefreshCw,
  PlugZap,
  Package,
  LogOut,
  ScrollText,
} from "lucide-react";

import ReactECharts from "echarts-for-react";
import { KeyboardProvider, useVirtualKeyboard } from "./hooks/useVirtualKeyboard";
import { useCameraSocket } from "./hooks/useCameraSocket";
import { useDevicePolling } from "./hooks/useDevicePolling";
import { useDeviceStore } from "./store/useDeviceStore";
import POManagerView from "./components/pomanager/POManagerView";
import DataPoolView from "./components/datapool/DataPoolView";
import ProductionView from "./components/production/ProductionView";
import { PLCSettingsView } from "./components/plcsetting/PLCSettingsView";
import LogsView from "./components/logs/LogsView";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import { LoginScreen } from "./components/LoginScreen";
import { ErrorBoundary } from "./components/ErrorBoundary";
import ConnectionLostDialog from "./components/ConnectionLostDialog";
import type {
  CameraHistoryItem,
  CameraHistoryResponse,
} from "./types/camera";

/* =========================================
   COMPONENTS - SHARED SCADA UI
   ========================================= */

const Card = ({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) => (
  <div
    className={`bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden flex flex-col ${className}`}
  >
    {children}
  </div>
);

const CardHeader = ({
  title,
  icon: Icon,
  action,
}: {
  title: string;
  icon?: React.ElementType;
  action?: React.ReactNode;
}) => (
  <div className="bg-slate-50/80 border-b border-slate-100 flex items-center justify-between px-4 xl:px-6 py-3.5 xl:py-4 shrink-0 rounded-t-3xl">
    <div className="flex items-center gap-2 text-slate-800">
      {Icon && <Icon className="w-4 h-4 text-blue-600" />}
      <h2 className="text-[11px] xl:text-[13px] font-bold tracking-wide uppercase">
        {title}
      </h2>
    </div>
    {action && <div>{action}</div>}
  </div>
);

const StatBox = ({
  label,
  value,
  type = "default",
}: {
  label: string;
  value: string | number;
  type?: "default" | "success" | "error" | "primary";
}) => {
  const colors = {
    default: "bg-slate-50 border-slate-200/80 text-slate-800",
    primary: "bg-blue-50/50 border-blue-200/60 text-blue-800",
    success: "bg-green-50/50 border-green-200/60 text-green-800",
    error: "bg-red-50/50 border-red-200/60 text-red-800",
  };

  return (
    <div
      className={`flex flex-col items-center justify-center p-1.5 xl:p-2 2xl:p-3 rounded-xl xl:rounded-[14px] border ${colors[type]} min-w-0 overflow-hidden`}
    >
      <span className="text-[9px] lg:text-[10px] 2xl:text-[11px] font-bold uppercase tracking-wider opacity-70 mb-0.5 xl:mb-1 text-center truncate w-full px-0.5">
        {label}
      </span>
      <span className="text-sm lg:text-[15px] xl:text-base 2xl:text-lg font-black tracking-tight leading-none text-center truncate w-full px-0.5">
        {value}
      </span>
    </div>
  );
};

const NetworkStrengthIndicator = ({ strength }: { strength: number }) => {
  const getConfig = (level: number) => {
    if (level >= 4) return { label: "MẠNH", bg: "bg-green-50 border-green-100", dot: "bg-green-500", bars: 5, barColor: "bg-green-500" };
    if (level >= 3) return { label: "TỐT", bg: "bg-emerald-50 border-emerald-100", dot: "bg-emerald-500", bars: 4, barColor: "bg-emerald-500" };
    if (level >= 2) return { label: "KHÁ", bg: "bg-amber-50 border-amber-100", dot: "bg-amber-500", bars: 3, barColor: "bg-amber-500" };
    if (level >= 1) return { label: "YẾU", bg: "bg-orange-50 border-orange-100", dot: "bg-orange-500", bars: 2, barColor: "bg-orange-500" };
    return { label: "KÉM", bg: "bg-red-50 border-red-100", dot: "bg-red-500", bars: 1, barColor: "bg-red-500" };
  };

  const config = getConfig(strength);

  return (
    <div className={`flex items-center justify-between px-2 py-1.5 xl:py-2 2xl:py-2.5 rounded-xl border ${config.bg} transition-colors min-w-0`}>
      <div className="flex items-center gap-1.5 xl:gap-2 min-w-0">
        <div className="p-1 rounded-md bg-white shadow-sm ring-1 ring-slate-900/5 hidden sm:block shrink-0">
          <Wifi className="w-3 h-3 xl:w-3.5 xl:h-3.5 text-slate-600" strokeWidth={2.5} />
        </div>
        <div className="min-w-0">
          <span className="text-[9px] xl:text-[10px] 2xl:text-xs font-bold tracking-wide truncate block">TỐC ĐỘ MẠNG</span>
          <div className="flex items-end gap-[3px] mt-0.5">
            {[1, 2, 3, 4, 5].map((bar) => (
              <div
                key={bar}
                className={`w-1.5 rounded-sm transition-colors ${
                  bar <= config.bars ? config.barColor : "bg-slate-200"
                }`}
                style={{ height: `${6 + bar * 2}px` }}
              />
            ))}
          </div>
        </div>
      </div>
      <div className="flex items-center shrink-0 pl-1">
        <div className={`w-1.5 h-1.5 xl:w-2 xl:h-2 rounded-full ${config.dot}`} />
      </div>
    </div>
  );
};

const DeviceIndicator = ({
  label,
  subLabel,
  icon: Icon,
  status,
  onRetry,
  showRetrySpinner,
}: {
  label: string;
  subLabel?: string;
  icon: React.ElementType;
  status: "connected" | "error" | "offline" | "warning" | "connecting";
  onRetry?: () => void;
  showRetrySpinner?: boolean;
}) => {
  const isOk = status === "connected";
  const isWarn = status === "warning" || status === "connecting";
  const isBad = status === "error" || status === "offline";

  const containerCls = isOk
    ? "bg-green-50 border-green-200"
    : isWarn
      ? "bg-amber-50 border-amber-200"
      : "bg-red-50 border-red-200";

  const iconBgCls = isOk
    ? "bg-gradient-to-br from-green-500 to-emerald-600 shadow-md shadow-green-500/30"
    : isWarn
      ? "bg-gradient-to-br from-amber-400 to-orange-500 shadow-md shadow-amber-500/30"
      : "bg-gradient-to-br from-red-500 to-rose-600 shadow-md shadow-red-500/30";

  const labelCls = isOk ? "text-green-800" : isWarn ? "text-amber-800" : "text-red-800";
  const subCls = isOk ? "text-green-700" : isWarn ? "text-amber-700" : "text-red-700";
  const badgeCls = isOk
    ? "bg-green-100 text-green-700 border-green-200"
    : isWarn
      ? "bg-amber-100 text-amber-700 border-amber-200"
      : "bg-red-100 text-red-700 border-red-200";
  const dotCls = isOk ? "bg-green-500" : isWarn ? "bg-amber-500" : "bg-red-500";
  const statusText = isOk ? "ONLINE" : isWarn ? "ĐANG KẾT NỐI" : "OFFLINE";

  return (
    <div
      className={`rounded-xl border p-2 flex items-center gap-2 transition-all duration-300 ${containerCls}`}
    >
      <div className="relative shrink-0">
        <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${iconBgCls}`}>
          <Icon className="w-3.5 h-3.5 text-white" strokeWidth={2.5} />
        </div>
        <div className="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-white flex items-center justify-center">
          <span className={`w-1.5 h-1.5 rounded-full ${dotCls} ${!isOk ? "animate-pulse" : ""}`} />
        </div>
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-1.5">
          <span className={`text-[10px] font-black uppercase tracking-wider ${labelCls}`}>
            {label}
          </span>
          <span className={`text-[8px] font-bold px-1.5 py-0.5 rounded border ${badgeCls}`}>
            {statusText}
          </span>
        </div>
        {subLabel && (
          <span className={`text-[10px] ${subCls} font-mono truncate block mt-0.5`}>
            {subLabel}
          </span>
        )}
      </div>
      {showRetrySpinner && status === "error" && onRetry && (
        <button
          type="button"
          onClick={onRetry}
          aria-label="Thử lại"
          title="Thử lại"
          className="shrink-0 p-1.5 rounded-lg hover:bg-red-100 transition-colors"
        >
          <RefreshCw className="w-3.5 h-3.5 text-red-600 animate-spin" />
        </button>
      )}
    </div>
  );
};

const AppStateIndicator = ({ state }: { state: string }) => {
  const stateConfig: Record<
    string,
    { label: string; bg: string; text: string; dot: string; badge: string }
  > = {
    NeedLogin: {
      label: "NEED LOGIN",
      bg: "bg-slate-50 border-slate-200",
      text: "text-slate-700",
      dot: "bg-slate-400",
      badge: "bg-slate-100 text-slate-600 border-slate-200",
    },
    NoSelectedPO: {
      label: "NO PO",
      bg: "bg-slate-50 border-slate-200",
      text: "text-slate-700",
      dot: "bg-slate-400",
      badge: "bg-slate-100 text-slate-600 border-slate-200",
    },
    Editing: {
      label: "EDITING",
      bg: "bg-slate-50 border-slate-200",
      text: "text-slate-700",
      dot: "bg-slate-400",
      badge: "bg-slate-100 text-slate-600 border-slate-200",
    },
    CheckingPO: {
      label: "CHECKING PO",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    Checking: {
      label: "CHECKING",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    CheckPO: {
      label: "CHECK PO",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    LoadPO: {
      label: "LOAD PO",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    Ready: {
      label: "READY",
      bg: "bg-green-50 border-green-200",
      text: "text-green-700",
      dot: "bg-green-500",
      badge: "bg-green-100 text-green-700 border-green-200",
    },
    PushingToDic: {
      label: "LOADING DIC",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    Running: {
      label: "RUNNING",
      bg: "bg-green-50 border-green-200",
      text: "text-green-700",
      dot: "bg-green-500",
      badge: "bg-green-100 text-green-700 border-green-200",
    },
    Paused: {
      label: "PAUSED",
      bg: "bg-amber-50 border-amber-200",
      text: "text-amber-700",
      dot: "bg-amber-500",
      badge: "bg-amber-100 text-amber-700 border-amber-200",
    },
    CheckingQueue: {
      label: "CHECK QUEUE",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    Saving: {
      label: "SAVING",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    WaitingStop: {
      label: "WAITING STOP",
      bg: "bg-amber-50 border-amber-200",
      text: "text-amber-700",
      dot: "bg-amber-500 animate-pulse",
      badge: "bg-amber-100 text-amber-700 border-amber-200",
    },
    CheckAfterCompleted: {
      label: "CHECK DONE",
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      dot: "bg-blue-500 animate-pulse",
      badge: "bg-blue-100 text-blue-700 border-blue-200",
    },
    Completed: {
      label: "COMPLETED",
      bg: "bg-green-50 border-green-200",
      text: "text-green-700",
      dot: "bg-green-500",
      badge: "bg-green-100 text-green-700 border-green-200",
    },
    DeviceError: {
      label: "DEVICE ERROR",
      bg: "bg-red-50 border-red-200",
      text: "text-red-700",
      dot: "bg-red-500 animate-pulse",
      badge: "bg-red-100 text-red-700 border-red-200",
    },
    Error: {
      label: "ERROR",
      bg: "bg-red-50 border-red-200",
      text: "text-red-700",
      dot: "bg-red-500 animate-pulse",
      badge: "bg-red-100 text-red-700 border-red-200",
    },
    Unknown: {
      label: "IDLE",
      bg: "bg-slate-50 border-slate-200",
      text: "text-slate-700",
      dot: "bg-slate-400",
      badge: "bg-slate-100 text-slate-600 border-slate-200",
    },
  };

  const config =
    stateConfig[state] ||
    stateConfig.Unknown;

  const isOk = ["Ready", "Running", "Completed"].includes(state);
  const isBad = ["DeviceError", "Error"].includes(state);
  const iconBgCls = isOk
    ? "bg-gradient-to-br from-green-500 to-emerald-600 shadow-md shadow-green-500/30"
    : isBad
      ? "bg-gradient-to-br from-red-500 to-rose-600 shadow-md shadow-red-500/30"
      : "bg-gradient-to-br from-blue-400 to-indigo-600 shadow-md shadow-blue-500/30";

  return (
    <div className={`rounded-xl border p-2 flex items-center gap-2 transition-all duration-300 ${config.bg}`}>
      <div className="relative shrink-0">
        <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${iconBgCls}`}>
          <Wifi className="w-3.5 h-3.5 text-white" strokeWidth={2.5} />
        </div>
        <div className="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full border-2 border-white flex items-center justify-center">
          <span className={`w-1.5 h-1.5 rounded-full ${config.dot}`} />
        </div>
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-1.5">
          <span className={`text-[10px] font-black uppercase tracking-wider ${config.text}`}>
            HỆ THỐNG
          </span>
          <span className={`text-[8px] font-bold px-1.5 py-0.5 rounded border ${config.badge}`}>
            {config.label}
          </span>
        </div>
      </div>
    </div>
  );
};

/* =========================================
   SCADA MONITOR VIEW (The Dashboard)
   ========================================= */

const ScadaMonitorView = () => {
  const [activeTab, setActiveTab] = useState<
    "log" | "error" | "plc" | "history"
  >("log");
  const [cameraHistory, setCameraHistory] = useState<CameraHistoryItem[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);

  // Camera WebSocket — kept only for real-time log table & onEvent callback
  const wsUrl =
    import.meta.env.VITE_CAMERA_WS_URL || "ws://localhost:9999/ws/camera";

  const { logs, reconnect: reconnectCamera } = useCameraSocket({
    url: wsUrl,
    syncToStore: true, // Sync to store so lastScan persists across tab switches
  });

  // PLC & Production — REST polling via /api/devices/status every 2000ms
  useDevicePolling({ intervalMs: 2000 });

  // Get device status from centralized store
  const cameraStatus = useDeviceStore((s) => s.camera);
  const plcStatus = useDeviceStore((s) => s.plc);
  const appStatus = useDeviceStore((s) => s.production);
  const productionConnected = useDeviceStore((s) => s.productionConnected);
  const apiStatus = useDeviceStore((s) => s.apiStatus);

  // Get lastScan from store (persists across tab switches)
  const lastScan = cameraStatus.camera.lastScan;

  // ====== Helpers ======

  // Camera status mapping (using store data)
  const getCameraStatus = (): "connected" | "error" | "warning" | "connecting" => {
    const { camera, connected } = cameraStatus;
    const s = camera.state?.toLowerCase();
    if (s === "reconnecting") return "connecting";
    if (s === "disconnected" || s === "deactive") return "error";
    if (!connected) return "error";
    if (s === "connected" || s === "received" || s === "codescanned") return "connected";
    return "connected";
  };

  // PLC status mapping (using store data)
  const getPlcStatus = (): "connected" | "error" | "warning" | "connecting" => {
    const { connected, state } = plcStatus;
    const s = state?.toLowerCase();
    if (!connected && !state) return "connecting";
    if (s === "disconnected") return "error";
    if (s === "reconnecting") return "connecting";
    if (connected) return "connected";
    return "connecting";
  };

  // Application status mapping (using store data)
  // Bind DeviceIndicator "Backend" trực tiếp vào appStatus.state (chuỗi API trả về).
  // Trả "error" khi /api/devices/status fail liên tiếp, nếu không thì map theo state code.
  const getAppStatus = (): "connected" | "error" | "warning" | "connecting" => {
    if (apiStatus.error) return "error";
    const state = appStatus?.state;
    if (!productionConnected) return "connecting";
    if (!state || state === "Unknown") return "connecting";
    const apiStateMap: Record<string, "connected" | "error" | "warning"> = {
      Running: "connected",
      Ready: "connected",
      Paused: "warning",
      Completed: "connected",
      Error: "error",
      DeviceError: "error",
      NeedLogin: "warning",
      NoSelectedPO: "warning",
      Editing: "warning",
    };
    return apiStateMap[state] ?? "warning";
  };

  const cameraState = cameraStatus.camera.state;

  const formatLastCode = (lastCode: string, lastAt: string | null) => {
    if (!lastCode) return "Chưa quét";
    if (!lastAt) return lastCode;
    return `${lastCode} @ ${new Date(lastAt).toLocaleTimeString("vi-VN")}`;
  };

  const cameraSubLabel = (state: string | undefined) => {
    if (!state) return "Đang kết nối";
    const labelMap: Record<string, string> = {
      Connected: "Đã kết nối",
      Reconnecting: "Đang kết nối lại",
      Disconnected: "Mất kết nối",
      Received: "Đang hoạt động",
    };
    return labelMap[state] ?? state;
  };

  // App state label from store
  const appStateLabel = apiStatus.error
    ? `Lỗi kết nối${apiStatus.statusCode ? ` (HTTP ${apiStatus.statusCode})` : ""}`
    : !productionConnected
      ? "Mất kết nối WS"
      : !appStatus?.state || appStatus.state === "Unknown"
        ? "Idle"
        : appStatus.state;

  // ====== Scan status (badge TỐT) ======
  const scanBadge = useMemo(() => {
    if (!lastScan) {
      return {
        label: cameraState === "Disconnected" ? "WAIT" : "TỐT",
        subLabel: cameraStatus.lastEventAt
          ? new Date(cameraStatus.lastEventAt).toLocaleString("vi-VN")
          : "Đang chờ...",
        code: cameraStatus.camera.lastCode || "",
        tone: "wait" as "ok" | "warn" | "err" | "wait",
      };
    }
    const status = lastScan.status;
    if (status === "Pass")
      return {
        label: "TỐT",
        subLabel: new Date(lastScan.at).toLocaleString("vi-VN"),
        code: lastScan.code,
        tone: "ok" as const,
      };
    if (status === "Duplicate")
      return {
        label: "TRÙNG",
        subLabel: new Date(lastScan.at).toLocaleString("vi-VN"),
        code: lastScan.code,
        tone: "warn" as const,
      };
    if (status === "ReadFail" || status === "Timeout")
      return {
        label: "KHÔNG ĐỌC",
        subLabel: new Date(lastScan.at).toLocaleString("vi-VN"),
        code: lastScan.code || "",
        tone: "err" as const,
      };
    if (status === "NotFound")
      return {
        label: "KHÔNG CÓ",
        subLabel: new Date(lastScan.at).toLocaleString("vi-VN"),
        code: lastScan.code,
        tone: "err" as const,
      };
    if (status === "Error")
      return {
        label: "LỖI",
        subLabel: new Date(lastScan.at).toLocaleString("vi-VN"),
        code: lastScan.code,
        tone: "err" as const,
      };
    return {
      label: status,
      subLabel: new Date(lastScan.at).toLocaleString("vi-VN"),
      code: lastScan.code,
      tone: "warn" as const,
    };
  }, [lastScan, cameraState, cameraStatus.lastEventAt, cameraStatus.camera.lastCode]);

  // ====== Lịch sử camera (fetch khi mở tab) ======
  useEffect(() => {
    if (activeTab !== "history") return;
    setHistoryLoading(true);
    const apiBase =
      (import.meta.env.VITE_API_BASE_URL as string | undefined) ??
      "http://localhost:9999";
    fetch(`${apiBase}/api/camera-history?limit=200`)
      .then((r) => (r.ok ? r.json() : Promise.reject(r.statusText)))
      .then((d: CameraHistoryResponse) => setCameraHistory(d.items ?? []))
      .catch(() => setCameraHistory([]))
      .finally(() => setHistoryLoading(false));
  }, [activeTab]);

  const statusBadgeClass = (status: string) => {
    switch (status) {
      case "Pass":
        return "bg-green-100 text-green-700";
      case "Duplicate":
        return "bg-amber-100 text-amber-700";
      case "ReadFail":
      case "Timeout":
      case "NotFound":
      case "Error":
        return "bg-red-100 text-red-700";
      default:
        return "bg-slate-100 text-slate-700";
    }
  };

  return (
    <div className="flex flex-col gap-3 xl:gap-4 h-full min-h-0 w-full animate-in fade-in duration-500">
      {/* Main Dashboard Layout */}
      <div className="flex flex-col xl:flex-row gap-4 xl:gap-5 h-full min-h-0 pb-1">
        {/* LEFT COLUMN */}
        <div className="flex flex-col gap-4 xl:gap-5 w-full xl:w-[60%] h-full min-h-0">
          {/* Kết quả vừa kiểm */}
          <Card className="shrink-0">
            <CardHeader title="KẾT QUẢ VỪA KIỂM" icon={Activity} />
            <div className="p-3 2xl:p-5 flex flex-col sm:flex-row gap-3 2xl:gap-5 h-[110px] 2xl:h-40">
              <div
                className={`w-full sm:w-[35%] rounded-xl 2xl:rounded-2xl text-white flex flex-col items-center justify-center p-2 2xl:p-4 shadow-lg ring-1 ring-white/20 ${
                  scanBadge.tone === "err"
                    ? "bg-gradient-to-br from-red-500 to-rose-600 shadow-red-500/20"
                    : scanBadge.tone === "warn"
                      ? "bg-gradient-to-br from-amber-500 to-orange-600 shadow-amber-500/20"
                      : scanBadge.tone === "wait"
                        ? "bg-gradient-to-br from-slate-400 to-slate-500 shadow-slate-400/20"
                        : "bg-gradient-to-br from-green-500 to-emerald-600 shadow-green-500/20"
                }`}
              >
                <CheckCircle2
                  className="w-10 h-10 2xl:w-12 2xl:h-12 mb-1 2xl:mb-2"
                  strokeWidth={2.5}
                />
                <span className="text-2xl 2xl:text-3xl font-black tracking-widest leading-none">
                  {scanBadge.label}
                </span>
              </div>

              <div className="flex-1 bg-slate-50 rounded-xl 2xl:rounded-2xl border border-slate-200/80 p-3 flex flex-col relative overflow-hidden text-left h-full">
                <div className="absolute top-2 right-2 p-2 2xl:p-3 opacity-[0.03] pointer-events-none">
                  <QrCode className="w-24 h-24 2xl:w-32 2xl:h-32" />
                </div>
                <div className="text-[10px] 2xl:text-xs font-black text-slate-400 uppercase tracking-widest mb-1 2xl:mb-2 flex items-center gap-1">
                  Sự kiện camera gần nhất
                </div>
                <div className="text-sm 2xl:text-lg font-mono font-medium text-slate-800 break-all leading-tight pr-8">
                  {scanBadge.subLabel}
                </div>
                <div className="mt-auto text-xs flex items-center gap-2 flex-wrap">
                  {scanBadge.code && (
                    <span className="px-2 py-0.5 2xl:px-3 2xl:py-1 rounded-full text-[10px] 2xl:text-[11px] font-bold tracking-wide bg-blue-100/80 text-blue-700 font-mono">
                      {scanBadge.code}
                    </span>
                  )}
                </div>
              </div>
            </div>
          </Card>

          {/* Bảng thông báo */}
          <Card className="flex-1 min-h-0 flex flex-col">
            <div className="bg-slate-50/50 border-b border-slate-100 flex p-1.5 gap-1.5 shrink-0 px-2 pt-2 rounded-t-3xl">
              <button
                onClick={() => setActiveTab("log")}
                className={`flex-1 py-2.5 2xl:py-3 px-4 text-xs 2xl:text-sm font-bold rounded-xl transition-all flex items-center justify-center gap-2 ${activeTab === "log" ? "bg-blue-50 text-blue-700" : "text-slate-500 hover:text-slate-700 hover:bg-slate-50"}`}
              >
                <History className="w-4 h-4" /> THÔNG BÁO CHUNG
              </button>
              <button
                onClick={() => setActiveTab("plc")}
                className={`flex-1 py-2.5 2xl:py-3 px-4 text-xs 2xl:text-sm font-bold rounded-xl transition-all flex items-center justify-center gap-2 ${activeTab === "plc" ? "bg-emerald-50 text-emerald-700" : "text-slate-500 hover:text-slate-700 hover:bg-slate-50"}`}
              >
                <Cpu className="w-4 h-4" /> NHẬT KÝ PLC
              </button>
              <button
                onClick={() => setActiveTab("error")}
                className={`flex-1 py-2.5 2xl:py-3 px-4 text-xs 2xl:text-sm font-bold rounded-xl transition-all flex items-center justify-center gap-2 ${activeTab === "error" ? "bg-red-50 text-red-700" : "text-slate-500 hover:text-slate-700 hover:bg-slate-50"}`}
              >
                <AlertTriangle className="w-4 h-4" /> KIỂM TRA LỖI
              </button>
              <button
                onClick={() => setActiveTab("history")}
                className={`flex-1 py-2.5 2xl:py-3 px-4 text-xs 2xl:text-sm font-bold rounded-xl transition-all flex items-center justify-center gap-2 ${activeTab === "history" ? "bg-violet-50 text-violet-700" : "text-slate-500 hover:text-slate-700 hover:bg-slate-50"}`}
              >
                <QrCode className="w-4 h-4" /> LỊCH SỬ CAMERA
              </button>
            </div>
            <div className="flex-1 overflow-auto bg-white p-0">
                <table className="w-full text-sm text-left relative">
                <thead className="text-[10px] 2xl:text-xs text-slate-400 uppercase bg-white sticky top-0 border-b border-slate-100 z-10 w-full backdrop-blur">
                  <tr>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      ID
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Thời gian
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Trạng thái
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Mã
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Carton
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100/80">
                  {activeTab === "plc" ? (
                    <tr>
                      <td colSpan={5} className="px-5 2xl:px-6 py-8">
                        <div className="flex flex-col gap-3">
                          <div className="flex items-center gap-2">
                            <span className={`w-2.5 h-2.5 rounded-full ${plcStatus.connected ? "bg-emerald-500" : "bg-red-500 animate-pulse"}`} />
                            <span className={`text-sm font-semibold ${plcStatus.connected ? "text-emerald-700" : "text-red-700"}`}>
                              PLC OMRON — {plcStatus.state ?? "Unknown"}
                            </span>
                          </div>
                          <div className="grid grid-cols-2 gap-3">
                            <div className="bg-slate-50 rounded-xl border border-slate-100 px-4 py-3">
                              <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Trạng thái</div>
                              <div className={`text-sm font-semibold ${plcStatus.connected ? "text-emerald-700" : "text-red-700"}`}>
                                {plcStatus.connected ? "Đã kết nối" : "Mất kết nối"}
                              </div>
                            </div>
                            <div className="bg-slate-50 rounded-xl border border-slate-100 px-4 py-3">
                              <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Kết nối WS</div>
                              <div className={`text-sm font-semibold ${plcStatus.clientCount > 0 ? "text-emerald-700" : "text-slate-400"}`}>
                                {plcStatus.clientCount > 0 ? `${plcStatus.clientCount} client(s)` : "Không có client"}
                              </div>
                            </div>
                            <div className="bg-slate-50 rounded-xl border border-slate-100 px-4 py-3">
                              <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">IP</div>
                              <div className="text-sm font-mono font-medium text-slate-700">
                                {plcStatus.ip ?? "—"}
                              </div>
                            </div>
                            <div className="bg-slate-50 rounded-xl border border-slate-100 px-4 py-3">
                              <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Port</div>
                              <div className="text-sm font-mono font-medium text-slate-700">
                                {plcStatus.port ?? "—"}
                              </div>
                            </div>
                          </div>
                          <div className="text-xs text-slate-400 text-center">
                            Trạng thái PLC được cập nhật qua REST polling mỗi 2000ms
                          </div>
                        </div>
                      </td>
                    </tr>
                  ) : activeTab === "history" ? (
                    historyLoading ? (
                      <tr>
                        <td colSpan={5} className="px-5 2xl:px-6 py-8 text-center text-slate-400 text-sm">
                          Đang tải lịch sử...
                        </td>
                      </tr>
                    ) : cameraHistory.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="px-5 2xl:px-6 py-8 text-center text-slate-400 text-sm">
                          Chưa có lịch sử quét nào.
                        </td>
                      </tr>
                    ) : (
                      cameraHistory.map((entry) => (
                        <tr
                          key={entry.id}
                          className="hover:bg-slate-50/50 transition-colors"
                        >
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 font-mono text-[11px] 2xl:text-xs font-semibold text-slate-500 whitespace-nowrap">
                            #{entry.id}
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 font-mono text-[11px] 2xl:text-xs font-semibold text-slate-500 whitespace-nowrap">
                            {new Date(entry.at).toLocaleString("vi-VN")}
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5">
                            <span
                              className={`px-2.5 2xl:px-3 py-1 rounded-full text-[10px] 2xl:text-[11px] font-bold tracking-wide uppercase ${statusBadgeClass(entry.status)}`}
                            >
                              {entry.status}
                            </span>
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 text-xs 2xl:text-sm text-slate-700 font-medium font-mono break-all">
                            {entry.code || "—"}
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 text-xs 2xl:text-sm text-slate-700 font-medium font-mono">
                            {entry.cartonCode ?? "—"}
                          </td>
                        </tr>
                      ))
                    )
                  ) : logs.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="px-5 2xl:px-6 py-8 text-center text-slate-400 text-sm">
                        Đang chờ dữ liệu từ camera...
                      </td>
                    </tr>
                  ) : (
                    logs.map((log) => {
                      const isReceived = log.msg.state === "Received";
                      const isError = log.msg.state === "Disconnected";
                      const stateBadgeColor = isReceived
                        ? "bg-green-100 text-green-700"
                        : isError
                          ? "bg-red-100 text-red-700"
                          : log.msg.state === "Connected"
                            ? "bg-blue-100 text-blue-700"
                            : "bg-amber-100 text-amber-700";
                      return (
                        <tr
                          key={log.id}
                          className="hover:bg-slate-50/50 transition-colors"
                        >
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 font-mono text-[11px] 2xl:text-xs font-semibold text-slate-500 whitespace-nowrap">
                            —
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 font-mono text-[11px] 2xl:text-xs font-semibold text-slate-500 whitespace-nowrap">
                            {new Date(log.time).toLocaleTimeString("vi-VN")}
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5">
                            <span className={`px-2.5 2xl:px-3 py-1 rounded-full text-[10px] 2xl:text-[11px] font-bold tracking-wide uppercase ${stateBadgeColor}`}>
                              {log.msg.state}
                            </span>
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 text-xs 2xl:text-sm text-slate-700 font-medium font-mono break-all">
                            {log.msg.camera.toUpperCase()}
                          </td>
                          <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 text-xs 2xl:text-sm text-slate-700 font-medium font-mono break-all">
                            {log.msg.data || "-"}
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        </div>

        {/* RIGHT COLUMN */}
        <div className="flex flex-col gap-4 xl:gap-5 w-full xl:w-[40%] h-full pb-1">
          <Card className="flex-[1] shadow-sm flex flex-col min-h-0">
            <CardHeader title="TRẠNG THÁI THIẾT BỊ" icon={Server} />
            <div className="p-1.5 xl:p-2 2xl:p-2.5 grid grid-cols-2 gap-1 xl:gap-1.5 2xl:gap-2 flex-1 min-h-0">
              <DeviceIndicator
                icon={Monitor}
                label="CAMERA"
                subLabel={
                  cameraState && ["Reconnecting", "Disconnected", "Deactive"].includes(cameraState)
                    ? cameraSubLabel(cameraState)
                    : formatLastCode(cameraStatus.camera.lastCode, cameraStatus.camera.lastAt)
                }
                status={getCameraStatus()}
              />
              <DeviceIndicator
                icon={Cpu}
                label="PLC OMRON"
                subLabel={cameraSubLabel(plcStatus.state)}
                status={getPlcStatus()}
              />
              <DeviceIndicator
                icon={Settings}
                label="Backend"
                subLabel={appStateLabel}
                status={getAppStatus()}
                showRetrySpinner={apiStatus.error}
                onRetry={() => useDeviceStore.getState().manualPoll()}
              />
              <AppStateIndicator
                state={productionConnected ? appStatus?.state || "Unknown" : "Unknown"}
              />
            </div>
          </Card>

          <Card className="flex-[1.2] shadow-sm flex flex-col min-h-0">
            <CardHeader
              title="KẾT NỐI WEBSOCKET"
              icon={Wifi}
              action={
                <div className="flex items-center gap-2">
                  <span
                    className={`flex items-center gap-1.5 text-xs font-semibold ${
                      cameraStatus.connected ? "text-green-700" : "text-red-700"
                    }`}
                  >
                    <span
                      className={`w-2 h-2 rounded-full ${
                        cameraStatus.connected
                          ? "bg-green-500"
                          : "bg-red-500 animate-pulse"
                      }`}
                    />
                    {cameraStatus.connected ? "Đã kết nối" : "Mất kết nối"}
                  </span>
                  <button
                    onClick={reconnectCamera}
                    className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-lg transition-colors"
                    title="Kết nối lại camera"
                  >
                    <RefreshCw
                      className={`w-3.5 h-3.5 ${!cameraStatus.connected ? "animate-spin" : ""}`}
                    />
                    Camera
                  </button>
                  <button
                    onClick={() => window.location.reload()}
                    className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 rounded-lg transition-colors"
                    title="Tải lại trang (PLC state cập nhật qua REST polling)"
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                    PLC
                  </button>
                </div>
              }
            />
            <div className="p-3 xl:p-4 2xl:p-5 flex flex-col justify-between gap-3 flex-1 overflow-auto">
              <div className="grid grid-cols-2 gap-3">
                <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3 flex items-center justify-center gap-2">
                  <div className={`w-2 h-2 rounded-full ${cameraStatus.connected ? "bg-green-500 animate-pulse" : "bg-red-500"}`} />
                  <span className="text-xs font-bold text-slate-600">
                    {cameraSubLabel(cameraState)}
                  </span>
                </div>
                <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3 flex items-center justify-center gap-2">
                  <PlugZap className="w-4 h-4 text-blue-600" />
                  <span className="text-xs font-bold text-slate-600">
                    Camera WS
                  </span>
                </div>
                <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3 flex items-center justify-center gap-2">
                  <div className={`w-2 h-2 rounded-full ${plcStatus.connected ? "bg-emerald-500 animate-pulse" : "bg-red-500"}`} />
                  <span className="text-xs font-bold text-slate-600">
                    {cameraSubLabel(plcStatus.state)}
                  </span>
                </div>
                <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3 flex items-center justify-center gap-2">
                  <PlugZap className="w-4 h-4 text-emerald-600" />
                  <span className="text-xs font-bold text-slate-600">
                    PLC WS
                  </span>
                </div>
              </div>

              <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3">
                <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Camera Endpoint</div>
                <div className="text-xs font-mono text-slate-700 break-all">{wsUrl}</div>
              </div>

              <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3">
                <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Devices Status API</div>
                <div className="text-xs font-mono text-slate-700 break-all">
                  {`${import.meta.env.VITE_API_BASE_URL || "http://localhost:9999"}/api/devices/status`}
                </div>
              </div>

              <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3">
                <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">PLC target</div>
                <div className="text-xs font-mono text-slate-700 break-all">
                  {plcStatus.ip ? `${plcStatus.ip}:${plcStatus.port}` : "—"}
                </div>
              </div>

              <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3">
                <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Thời gian nhận cuối</div>
                <div className="text-xs font-mono text-slate-700">
                  {cameraStatus.lastEventAt ? new Date(cameraStatus.lastEventAt).toLocaleString("vi-VN") : "Chưa có dữ liệu"}
                </div>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
};

/* =========================================
   PRODUCTION REPORT VIEW
   ========================================= */

const ProductionReportView = () => {
  const option1 = {
    title: {
      text: "Sản lượng 7 ngày gần nhất",
      left: "center",
      textStyle: { color: "#334155", fontSize: 15, fontWeight: "bold" },
    },
    tooltip: { trigger: "axis" },
    grid: { left: "5%", right: "5%", bottom: "10%", containLabel: true },
    xAxis: {
      type: "category",
      data: ["Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN"],
      axisLabel: { color: "#64748b" },
      axisLine: { lineStyle: { color: "#cbd5e1" } },
    },
    yAxis: {
      type: "value",
      axisLabel: { color: "#64748b" },
      splitLine: { lineStyle: { color: "#f1f5f9" } },
    },
    series: [
      {
        data: [1200, 1300, 1250, 1400, 1100, 1150, 1280],
        type: "bar",
        itemStyle: { color: "#3b82f6", borderRadius: [4, 4, 0, 0] },
        barWidth: "40%",
      },
    ],
  };

  const option2 = {
    title: {
      text: "Tỷ lệ Lỗi / Đạt (Hôm nay)",
      left: "center",
      textStyle: { color: "#334155", fontSize: 15, fontWeight: "bold" },
    },
    tooltip: { trigger: "item" },
    legend: { bottom: 10, textStyle: { color: "#475569" } },
    series: [
      {
        name: "Sản lượng",
        type: "pie",
        radius: ["40%", "65%"],
        center: ["50%", "50%"],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 8,
          borderColor: "#fff",
          borderWidth: 2,
        },
        label: { show: false },
        data: [
          {
            value: 12435,
            name: "Sản phẩm tốt",
            itemStyle: { color: "#10b981" },
          },
          { value: 245, name: "Sản phẩm lỗi", itemStyle: { color: "#ef4444" } },
        ],
      },
    ],
  };

  const option3 = {
    title: {
      text: "Tốc độ sản xuất trong ca (sp/phút)",
      left: "center",
      textStyle: { color: "#334155", fontSize: 15, fontWeight: "bold" },
    },
    tooltip: { trigger: "axis" },
    grid: { left: "3%", right: "4%", bottom: "3%", containLabel: true },
    xAxis: {
      type: "category",
      boundaryGap: false,
      data: ["08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00"],
      axisLabel: { color: "#64748b" },
      axisLine: { lineStyle: { color: "#cbd5e1" } },
    },
    yAxis: {
      type: "value",
      axisLabel: { color: "#64748b" },
      splitLine: { lineStyle: { color: "#f1f5f9" } },
    },
    series: [
      {
        name: "Tốc độ (sp/p)",
        type: "line",
        smooth: true,
        data: [110, 115, 120, 118, 112, 119, 122],
        areaStyle: {
          color: {
            type: "linear",
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [
              { offset: 0, color: "rgba(59, 130, 246, 0.4)" },
              { offset: 1, color: "rgba(59, 130, 246, 0.05)" },
            ],
          },
        },
        lineStyle: { width: 3 },
        itemStyle: { color: "#3b82f6" },
      },
    ],
  };

  return (
    <div className="flex flex-col gap-4 h-full min-h-0 w-full animate-in fade-in duration-500 overflow-auto scrollbar-hide pb-6">
      <div className="flex items-center justify-between shrink-0 px-1">
        <h1 className="text-xl 2xl:text-2xl font-bold text-slate-800 tracking-tight">
          Báo cáo sản xuất
        </h1>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 2xl:gap-5 min-h-0">
        <Card className="p-4 2xl:p-5">
          <ReactECharts
            option={option1}
            style={{ height: "320px", width: "100%" }}
          />
        </Card>
        <Card className="p-4 2xl:p-5">
          <ReactECharts
            option={option2}
            style={{ height: "320px", width: "100%" }}
          />
        </Card>
        <Card className="p-4 2xl:p-5 xl:col-span-2">
          <ReactECharts
            option={option3}
            style={{ height: "360px", width: "100%" }}
          />
        </Card>
      </div>
    </div>
  );
};

/* =========================================
   SETTINGS VIEW
   ========================================= */

const SettingsView = () => {
  const [config, setConfig] = useState({
    plcIp: "192.168.1.100",
    plcPort: 502,
    enableCamera: true,
    enableScanner: false,
    timeout: 5000,
    apiUrl: "http://tso.th.io.vn",
    theme: "light",
    language: "vi",
    autoSaveLogs: true,
  });

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const target = e.target as HTMLInputElement;
    const value = target.type === "checkbox" ? target.checked : target.value;
    const name = target.name;
    setConfig((prev) => ({ ...prev, [name]: value }));
  };

  const ToggleSwitch = ({
    label,
    name,
    checked,
  }: {
    label: string;
    name: string;
    checked: boolean;
  }) => (
    <div className="flex items-center justify-between py-3 border-b border-slate-100 last:border-0">
      <span className="text-sm font-semibold text-slate-700">{label}</span>
      <label className="relative inline-flex items-center cursor-pointer mb-0">
        <input
          type="checkbox"
          name={name}
          checked={checked}
          onChange={handleChange}
          className="sr-only peer"
        />
        <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
      </label>
    </div>
  );

  const { openKeyboard } = useVirtualKeyboard();

  const TextInput = ({
    label,
    name,
    value,
    type = "text",
  }: {
    label: string;
    name: string;
    value: string | number;
    type?: string;
  }) => (
    <div className="flex flex-col py-3 border-b border-slate-100 last:border-0 gap-1.5">
      <label className="text-sm font-semibold text-slate-700">{label}</label>
      <div className="relative">
        <input
          type={type}
          name={name}
          value={value}
          onChange={handleChange}
          className="bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 pr-10 outline-none transition-colors"
        />
        <button
          type="button"
          onClick={() => {
            openKeyboard(
              String(value),
              type === "number" ? "numeric" : "default",
              (newVal) => {
                handleChange({ target: { name, value: newVal, type } } as any);
              },
            );
          }}
          className="absolute right-2 top-1/2 -translate-y-1/2 p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
          title="Virtual Keyboard"
        >
          <KeyboardIcon className="w-5 h-5" />
        </button>
      </div>
    </div>
  );

  const SelectInput = ({
    label,
    name,
    value,
    options,
  }: {
    label: string;
    name: string;
    value: string;
    options: { value: string; label: string }[];
  }) => (
    <div className="flex flex-col py-3 border-b border-slate-100 last:border-0 gap-1.5">
      <label className="text-sm font-semibold text-slate-700">{label}</label>
      <div className="relative">
        <select
          name={name}
          value={value}
          onChange={handleChange}
          className="bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 appearance-none outline-none transition-colors"
        >
          {options.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
        <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />
      </div>
    </div>
  );

  return (
    <div className="flex flex-col gap-4 h-full min-h-0 w-full animate-in fade-in duration-500 overflow-auto scrollbar-hide pb-6">
      <div className="flex items-center justify-between shrink-0 px-1">
        <h1 className="text-xl 2xl:text-2xl font-bold text-slate-800 tracking-tight">
          Cấu hình hệ thống
        </h1>
        <button className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl text-sm font-bold transition-colors">
          <CheckCircle2 className="w-4 h-4" /> Lưu cấu hình
        </button>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 2xl:gap-5 min-h-0 items-start">
        {/* SCADA Settings */}
        <Card className="p-0">
          <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 xl:py-4 rounded-t-3xl">
            <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
              <Cpu className="w-4 h-4 text-blue-600" /> Cấu hình SCADA & Thiết
              bị
            </h2>
          </div>
          <div className="p-4 xl:p-6 flex flex-col">
            <TextInput
              label="Địa chỉ IP PLC"
              name="plcIp"
              value={config.plcIp}
            />
            <TextInput
              label="Cổng kết nối PLC (Port)"
              name="plcPort"
              value={config.plcPort}
              type="number"
            />
            <ToggleSwitch
              label="Bật kiểm tra Camera"
              name="enableCamera"
              checked={config.enableCamera}
            />
            <ToggleSwitch
              label="Bật kết nối Scanner"
              name="enableScanner"
              checked={config.enableScanner}
            />
            <TextInput
              label="Độ trễ Timeout (ms)"
              name="timeout"
              value={config.timeout}
              type="number"
            />
          </div>
        </Card>

        <div className="flex flex-col gap-4 2xl:gap-5">
          {/* API / Network */}
          <Card className="p-0">
            <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 xl:py-4 rounded-t-3xl">
              <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                <Wifi className="w-4 h-4 text-blue-600" /> Kết nối mạng & Máy
                chủ
              </h2>
            </div>
            <div className="p-4 xl:p-6 flex flex-col">
              <TextInput
                label="API Endpoint URL"
                name="apiUrl"
                value={config.apiUrl}
              />
            </div>
          </Card>

          {/* App Preferences */}
          <Card className="p-0">
            <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 xl:py-4 rounded-t-3xl">
              <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                <Settings className="w-4 h-4 text-blue-600" /> Tùy chọn ứng dụng
              </h2>
            </div>
            <div className="p-4 xl:p-6 flex flex-col">
              <SelectInput
                label="Giao diện sử dụng"
                name="theme"
                value={config.theme}
                options={[
                  { label: "Sáng (Light)", value: "light" },
                  { label: "Tối (Dark)", value: "dark" },
                  { label: "Tự động (Auto)", value: "auto" },
                ]}
              />
              <SelectInput
                label="Ngôn ngữ"
                name="language"
                value={config.language}
                options={[
                  { label: "Tiếng Việt", value: "vi" },
                  { label: "English", value: "en" },
                ]}
              />
              <ToggleSwitch
                label="Tự động sao lưu nhật ký (Logs)"
                name="autoSaveLogs"
                checked={config.autoSaveLogs}
              />
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
};

/* =========================================
   PLACEHOLDER VIEWS
   ========================================= */

const PlaceholderView = ({ title }: { title: string }) => (
  <div className="flex-1 h-full flex items-center justify-center flex-col animate-in fade-in duration-500">
    <div className="w-24 h-24 mb-6 rounded-3xl bg-slate-100 flex items-center justify-center">
      <Settings className="w-10 h-10 text-slate-300" strokeWidth={1.5} />
    </div>
    <h2 className="text-2xl font-bold tracking-tight text-slate-800">
      {title}
    </h2>
    <p className="text-slate-500 mt-2">
      Tính năng này đang được lên kế hoạch phát triển ở phiên bản tiếp theo.
    </p>
  </div>
);

/* =========================================
   MAIN ADMIN LAYOUT
   ========================================= */

const AdminPanelContent = ({ user, onLogout }: { user: any; onLogout: () => void }) => {
  const [activeRoute, setActiveRoute] = useState(() => {
    const params = new URLSearchParams(window.location.search);
    return params.get("panel") || "monitor";
  });
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const { isOpen } = useVirtualKeyboard();

  const baseNavigation = [
    { id: "monitor", title: "Giám sát SCADA", icon: LayoutDashboard },
    { id: "production", title: "Điều khiển SX", icon: Factory },
    { id: "plcsetting", title: "PLC Setting", icon: Cpu },
    { id: "batches", title: "Lệnh sản xuất", icon: Package },
    { id: "datapool", title: "Quản lý DataPool", icon: Database },
    { id: "history", title: "Báo cáo sản xuất", icon: BarChart2 },
    { id: "users", title: "Quản lý tài khoản", icon: Users },
    { id: "settings", title: "Cấu hình hệ thống", icon: Settings },
  ];

  const isSAdmin = user?.role === "SAdmin";
  const navigation = isSAdmin
    ? [
        ...baseNavigation.slice(0, 6),
        { id: "logs", title: "Logs hệ thống", icon: ScrollText },
        ...baseNavigation.slice(6),
      ]
    : baseNavigation;

  return (
    <div className="flex h-screen bg-[#F6F8FA] overflow-hidden font-sans text-slate-900">
    {/* SIDEBAR (Google 2026 Style - Pill shapes, floating feel) */}
      <aside
        className={`fixed lg:static top-0 left-0 h-full w-72 bg-white border-r border-slate-200/60 z-50 transform transition-transform duration-300 ease-in-out lg:translate-x-0 ${mobileMenuOpen ? "translate-x-0" : "-translate-x-full"} flex flex-col`}
      >
        {/* Logo Section */}
        <div className="h-20 flex items-center px-8 border-b border-slate-100/50 shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center shadow-lg shadow-blue-600/20">
              <Factory className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="text-lg font-black tracking-tighter text-slate-800 leading-none">
                MASAN-SERIALIZATION
              </h1>
              <span className="text-[10px] font-bold text-slate-400 tracking-widest uppercase">
                Admin Panel
              </span>
            </div>
          </div>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 overflow-y-auto px-4 py-6 flex flex-col gap-1.5">
          <div className="text-xs font-bold text-slate-400 tracking-widest uppercase px-4 mb-2 mt-4 first:mt-0">
            Điều khiển trung tâm
          </div>

          {navigation.map((item) => {
            const isActive = activeRoute === item.id;
            return (
              <button
                key={item.id}
                onClick={() => {
                  setActiveRoute(item.id);
                  setMobileMenuOpen(false);
                  const url = new URL(window.location.href);
                  url.searchParams.set("panel", item.id);
                  window.history.pushState({}, "", url.toString());
                }}
                className={`flex items-center gap-3.5 px-4 py-3.5 rounded-2xl font-semibold text-sm transition-all duration-200 group ${
                  isActive
                    ? "bg-blue-50 text-blue-700"
                    : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                }`}
              >
                <item.icon
                  className={`w-5 h-5 transition-colors ${isActive ? "text-blue-600" : "text-slate-400 group-hover:text-slate-600"}`}
                />
                {item.title}
              </button>
            );
          })}
        </nav>

        {/* Bottom User Area */}
        <div className="p-4 border-t border-slate-100 shrink-0">
          <div className="flex items-center gap-3 bg-slate-50 p-3 rounded-2xl border border-slate-100">
            <div className="w-10 h-10 rounded-xl bg-indigo-100 text-indigo-700 flex items-center justify-center font-bold text-sm shrink-0">
              {user?.username?.substring(0, 2).toUpperCase() || "AD"}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-bold text-slate-800 truncate">
                {user?.displayName || user?.username || "User"}
              </p>
              <p className="text-xs font-medium text-slate-500 truncate">
                {user?.role || "Guest"}
              </p>
            </div>
            <button
              onClick={onLogout}
              className="p-2 rounded-xl text-slate-400 hover:text-red-600 hover:bg-red-50 transition-colors"
              title="Logout"
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        </div>
      </aside>

      {/* Mobile Sidebar Overlay */}
      {mobileMenuOpen && (
        <div
          className="fixed inset-0 bg-slate-900/20 backdrop-blur-sm z-40 lg:hidden"
          onClick={() => setMobileMenuOpen(false)}
        />
      )}

      {/* MAIN CONTENT AREA */}
      <main className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <div className="lg:hidden p-4 shrink-0 flex items-center bg-white border-b border-slate-200">
          <button
            className="p-2 -ml-2 rounded-xl text-slate-500 hover:bg-slate-100"
            onClick={() => setMobileMenuOpen(true)}
          >
            <Menu className="w-5 h-5" />
          </button>
          <span className="ml-2 font-bold text-slate-800">Menu</span>
        </div>

        {/* PAGE CONTENT CONTAINER */}
        <div
          className={`flex-1 overflow-hidden p-4 lg:p-6 2xl:p-8 bg-[#F6F8FA] transition-all duration-300 flex flex-col min-h-0 ${isOpen ? "pb-[380px] md:pb-[420px]" : ""}`}
        >
          <ErrorBoundary key={activeRoute}>
            {activeRoute === "monitor" && <ScadaMonitorView />}
            {activeRoute === "production" && <ProductionView />}
            {activeRoute === "history" && <ProductionReportView />}
            {activeRoute === "settings" && <SettingsView />}
            {activeRoute === "batches" && <POManagerView />}
            {activeRoute === "datapool" && <DataPoolView />}
            {activeRoute === "plcsetting" && <PLCSettingsView />}
            {activeRoute === "logs" &&
              (isSAdmin ? (
                <LogsView />
              ) : (
                <PlaceholderView title="Logs hệ thống — không có quyền" />
              ))}
            {activeRoute !== "monitor" &&
              activeRoute !== "production" &&
              activeRoute !== "history" &&
              activeRoute !== "settings" &&
              activeRoute !== "batches" &&
              activeRoute !== "datapool" &&
              activeRoute !== "plcsetting" &&
              activeRoute !== "logs" && (
                <PlaceholderView
                  title={
                    navigation.find((n) => n.id === activeRoute)?.title ||
                    "Chức năng"
                  }
                />
              )}
          </ErrorBoundary>
        </div>
      </main>

      <style>{`
        .scrollbar-hide::-webkit-scrollbar {
            display: none;
        }
        .scrollbar-hide {
            -ms-overflow-style: none;
            scrollbar-width: none;
        }
      `}</style>
    </div>
  );
};

export default function AdminPanel() {
  return (
    <AuthProvider>
      <KeyboardProvider>
        <AdminPanelWithAuth />
      </KeyboardProvider>
    </AuthProvider>
  );
}

const AdminPanelWithAuth = () => {
  const { isAuthenticated, isLoading, user, logout } = useAuth();

  // Read production state from centralized store (synced by useDevicePolling)
  const prodState = useDeviceStore((s) => s.production?.state ?? "Unknown");
  const prodConnected = useDeviceStore((s) => s.productionConnected);

  // Auto logout when production state = NeedLogin, but only if user has been
  // authenticated for more than 5 seconds. This avoids a race where the very
  // first device poll (triggered right after login) returns the stale
  // "NeedLogin" state from the PLC and kicks the user back to the login screen.
  useEffect(() => {
    if (prodState !== "NeedLogin" || !isAuthenticated) return;
    const loginTsRaw = localStorage.getItem("gauth_login_ts");
    const loginTs = loginTsRaw ? Number(loginTsRaw) : 0;
    if (!loginTs) return;
    const elapsed = Date.now() - loginTs;
    if (elapsed < 5000) return;
    logout();
  }, [prodState, isAuthenticated, logout]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen bg-[#F6F8FA]">
        <div className="text-center">
          <div className="w-12 h-12 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-slate-600 font-medium">Loading...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <LoginScreen />;
  }

  return (
    <ErrorBoundary>
      <ConnectionLostDialog />
      <AdminPanelContent user={user} onLogout={logout} />
    </ErrorBoundary>
  );
};
