import React, { useState, useEffect, useRef } from "react";
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
  X as CloseIcon,
  RefreshCw,
  Plug,
  PlugZap,
  Package,
} from "lucide-react";

import ReactECharts from "echarts-for-react";
import Keyboard from "react-simple-keyboard";
import "react-simple-keyboard/build/css/index.css";
import { useWebSocket, DeviceStatus, LogEntry } from "./hooks/useWebSocket";
import POManagerView from "./components/pomanager/POManagerView";
import DataPoolView from "./components/datapool/DataPoolView";
import ProductionView from "./components/production/ProductionView";

type KeyboardLayoutType = "default" | "shift" | "numeric";

interface KeyboardState {
  isOpen: boolean;
  value: string;
  layout: KeyboardLayoutType;
  onChange: (val: string) => void;
}

interface KeyboardContextType {
  openKeyboard: (
    initialValue: string,
    layout: KeyboardLayoutType,
    onChange: (val: string) => void,
  ) => void;
  closeKeyboard: () => void;
  isOpen: boolean;
}

const KeyboardContext = React.createContext<KeyboardContextType | null>(null);

export const useVirtualKeyboard = () => {
  const ctx = React.useContext(KeyboardContext);
  if (!ctx)
    throw new Error("useVirtualKeyboard must be used within KeyboardProvider");
  return ctx;
};

export const KeyboardProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [value, setValue] = useState("");
  const [layout, setLayout] = useState<KeyboardLayoutType>("default");
  const [onChangeRef, setOnChangeRef] = useState<{ fn: (val: string) => void }>(
    { fn: () => {} },
  );
  const keyboardRef = useRef<any>(null);

  const openKeyboard = (
    initialValue: string,
    layoutType: KeyboardLayoutType,
    onChange: (val: string) => void,
  ) => {
    setValue(initialValue);
    setLayout(layoutType);
    setOnChangeRef({ fn: onChange });
    setIsOpen(true);
    if (keyboardRef.current) {
      keyboardRef.current.setInput(initialValue);
    }
  };

  const closeKeyboard = () => {
    setIsOpen(false);
  };

  const onChange = (input: string) => {
    setValue(input);
    onChangeRef.fn(input);
  };

  const handleShift = () => {
    const currentLayout = layout;
    let nextLayout = currentLayout;
    if (currentLayout === "default") nextLayout = "shift";
    else if (currentLayout === "shift") nextLayout = "default";

    if (nextLayout !== currentLayout) {
      setLayout(nextLayout as KeyboardLayoutType);
    }
  };

  const onKeyPress = (button: string) => {
    if (button === "{shift}" || button === "{lock}") handleShift();
    if (button === "{enter}" || button === "{escape}") {
      closeKeyboard();
    }
  };

  return (
    <KeyboardContext.Provider value={{ openKeyboard, closeKeyboard, isOpen }}>
      {children}
      {isOpen && (
        <div className="fixed bottom-0 left-0 right-0 z-[100] p-3 md:p-4 bg-[#d1d5db]/90 backdrop-blur-md border-t border-slate-300 shadow-xl animate-in slide-in-from-bottom-full duration-300">
          <div className="max-w-4xl mx-auto relative">
            <div className="absolute -top-[60px] left-0 right-0 flex justify-between items-center bg-white p-2 px-4 rounded-t-xl md:rounded-xl shadow-lg border border-slate-200 gap-4">
              <input 
                type={layout === 'numeric' ? 'number' : 'text'}
                value={value}
                onChange={(e) => {
                  onChange(e.target.value);
                  keyboardRef.current?.setInput(e.target.value);
                }}
                className="flex-1 text-lg font-medium text-slate-800 bg-transparent outline-none p-1"
                placeholder="Nhập giá trị..."
                autoFocus
              />
              <button
                onClick={closeKeyboard}
                className="bg-slate-100 text-slate-500 p-2 rounded-lg hover:bg-red-50 hover:text-red-600 transition-colors flex items-center justify-center shrink-0"
              >
                <CloseIcon className="w-5 h-5" />
              </button>
            </div>

            <style>{`
              .hg-theme-default {
                background-color: transparent !important;
                border-radius: 0 !important;
                padding: 0 !important;
              }
              .hg-button {
                background: white !important;
                border-radius: 8px !important;
                box-shadow: 0 1px 1px rgba(0,0,0,0.2) !important;
                color: #0f172a !important;
                font-weight: 500 !important;
                height: 48px !important;
                display: flex !important;
                align-items: center !important;
                justify-content: center !important;
                font-size: 18px !important;
                transition: all 0.1s !important;
                border-bottom: 1px solid #94a3b8 !important;
                font-family: inherit !important;
              }
              @media (min-width: 768px) {
                .hg-button {
                  height: 56px !important;
                  font-size: 20px !important;
                  border-radius: 10px !important;
                }
              }
              .hg-button:active {
                background: #e2e8f0 !important;
                transform: translateY(1px) !important;
                box-shadow: none !important;
                border-bottom: none !important;
              }
              .hg-button.hg-standardBtn.hg-button-enter {
                background: #3b82f6 !important;
                color: white !important;
                font-weight: 600 !important;
                border-bottom: 1px solid #2563eb !important;
              }
              .hg-button.hg-standardBtn.hg-button-enter:active {
                background: #2563eb !important;
              }
              .hg-button.hg-standardBtn.hg-button-shift,
              .hg-button.hg-standardBtn.hg-button-bksp {
                background: #cbd5e1 !important;
                border-bottom: 1px solid #94a3b8 !important;
                color: #1e293b !important;
              }
              .hg-button.hg-standardBtn.hg-button-space {
                background: white !important;
              }
              .hg-row {
                margin-bottom: 8px !important;
              }
              .hg-row:last-child {
                margin-bottom: 0 !important;
              }
            `}</style>

            <div className="p-1 sm:p-2 w-full">
              <Keyboard
                keyboardRef={(r) => (keyboardRef.current = r)}
                layoutName={layout}
                onChange={onChange}
                onKeyPress={onKeyPress}
                display={{
                  "{bksp}": "⌫",
                  "{enter}": "Xong",
                  "{shift}": "⇧",
                  "{space}": "Dấu cách",
                  "{tab}": "Tab",
                  "{lock}": "Caps",
                  "{escape}": "Đóng",
                }}
                layout={{
                  default: [
                    "1 2 3 4 5 6 7 8 9 0",
                    "q w e r t y u i o p",
                    "a s d f g h j k l",
                    "{shift} z x c v b n m {bksp}",
                    "{space} {enter}",
                  ],
                  shift: [
                    "! @ # $ % ^ & * ( )",
                    "Q W E R T Y U I O P",
                    "A S D F G H J K L",
                    "{shift} Z X C V B N M {bksp}",
                    "{space} {enter}",
                  ],
                  numeric: ["1 2 3", "4 5 6", "7 8 9", "{bksp} 0 {enter}"],
                }}
              />
            </div>
          </div>
        </div>
      )}
    </KeyboardContext.Provider>
  );
};

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
}: {
  label: string;
  subLabel?: string;
  icon: React.ElementType;
  status: "ok" | "error" | "offline" | "warning";
}) => {
  const styles = {
    ok: "bg-green-50 text-green-700 border-green-100",
    error: "bg-red-50 text-red-700 border-red-100",
    warning: "bg-amber-50 text-amber-700 border-amber-100",
    offline: "bg-slate-50 text-slate-600 border-slate-100",
  };
  const dotStyles = {
    ok: "bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.6)]",
    error: "bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.6)] animate-pulse",
    warning: "bg-amber-500 shadow-[0_0_8px_rgba(245,158,11,0.6)]",
    offline: "bg-slate-400",
  };

  return (
    <div className={`flex items-center justify-between px-2 py-1.5 xl:py-2 2xl:py-2.5 rounded-xl border ${styles[status]} transition-colors min-w-0`}>
      <div className="flex items-center gap-1.5 xl:gap-2 min-w-0">
        <div className="p-1 rounded-md bg-white shadow-sm ring-1 ring-slate-900/5 hidden sm:block shrink-0">
          <Icon className="w-3 h-3 xl:w-3.5 xl:h-3.5 text-slate-600" strokeWidth={2.5} />
        </div>
        <div className="min-w-0">
          <span className="text-[9px] xl:text-[10px] 2xl:text-xs font-bold tracking-wide truncate block">{label}</span>
          {subLabel && (
            <span className="text-[8px] xl:text-[9px] 2xl:text-[10px] font-medium text-slate-500 tracking-wide truncate block">{subLabel}</span>
          )}
        </div>
      </div>
      <div className="flex items-center shrink-0 pl-1">
        <div className={`w-1.5 h-1.5 xl:w-2 xl:h-2 rounded-full ${dotStyles[status]}`} />
      </div>
    </div>
  );
};

const AppStateIndicator = ({ state }: { state: string }) => {
  const stateConfig: Record<string, { label: string; bg: string; text: string; dot: string }> = {
    Checking: { label: "CHECKING", bg: "bg-blue-50 border-blue-100", text: "text-blue-700", dot: "bg-blue-500 animate-pulse" },
    Idle: { label: "IDLE", bg: "bg-slate-50 border-slate-100", text: "text-slate-700", dot: "bg-slate-400" },
    Running: { label: "RUNNING", bg: "bg-green-50 border-green-100", text: "text-green-700", dot: "bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.6)]" },
    Error: { label: "ERROR", bg: "bg-red-50 border-red-100", text: "text-red-700", dot: "bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.6)] animate-pulse" },
    NotUsed: { label: "NOT USED", bg: "bg-slate-50 border-slate-100", text: "text-slate-500", dot: "bg-slate-300" },
  };

  const config = stateConfig[state] || { label: state, bg: "bg-slate-50 border-slate-100", text: "text-slate-700", dot: "bg-slate-400" };

  return (
    <div className={`flex items-center justify-between px-2 py-1.5 xl:py-2 2xl:py-2.5 rounded-xl border ${config.bg} transition-colors min-w-0`}>
      <div className="flex items-center gap-1.5 xl:gap-2 min-w-0">
        <div className="p-1 rounded-md bg-white shadow-sm ring-1 ring-slate-900/5 hidden sm:block shrink-0">
          <Wifi className="w-3 h-3 xl:w-3.5 xl:h-3.5 text-slate-600" strokeWidth={2.5} />
        </div>
        <div className="min-w-0">
          <span className="text-[9px] xl:text-[10px] 2xl:text-xs font-bold tracking-wide truncate block">HỆ THỐNG</span>
          <span className={`text-[10px] xl:text-[11px] 2xl:text-xs font-black tracking-wider truncate block ${config.text}`}>
            {config.label}
          </span>
        </div>
      </div>
      <div className="flex items-center shrink-0 pl-1">
        <div className={`w-1.5 h-1.5 xl:w-2 xl:h-2 rounded-full ${config.dot}`} />
      </div>
    </div>
  );
};

/* =========================================
   SCADA MONITOR VIEW (The Dashboard)
   ========================================= */

const ScadaMonitorView = () => {
  const [activeTab, setActiveTab] = useState<"log" | "error">("log");

  // VNQR WebSocket connection
  const wsUrl = import.meta.env.VITE_VNQR_WS_URL || "ws://localhost:8080/ws";

  const {
    isConnected,
    clientCount,
    lastMessage,
    logs,
    reconnect,
  } = useWebSocket({
    url: wsUrl,
    reconnectInterval: 3000,
    maxReconnectAttempts: 5,
  });

  const dashboardData = lastMessage || {};
  const d = dashboardData;

  const mapStatus = (status: string | undefined) => {
    if (!status) return "offline";
    const s = status.toLowerCase();
    if (s === "connected" || s === "ready" || s === "pass" || s === "ok")
      return "ok";
    if (s === "disconnected" || s === "deactive") return "offline";
    if (s === "error" || s === "fail") return "error";
    return "warning";
  };

  const cameraActiveState = d.camera?.active?.state || "Unknown";
  const cameraPackageState = d.camera?.package?.state || "Unknown";
  const plcState = d.plc?.state || "Unknown";
  const appState = d.app?.state || "Unknown";

  return (
    <div className="flex flex-col gap-3 xl:gap-4 h-full min-h-0 w-full animate-in fade-in duration-500">
      {/* Connection Status Bar */}
      <div className="flex items-center justify-between px-3 py-2 bg-white/80 backdrop-blur-sm rounded-2xl border border-slate-200/60 shadow-sm shrink-0">
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2">
            <div
              className={`w-2.5 h-2.5 rounded-full ${
                isConnected
                  ? "bg-green-500 shadow-[0_0_8px_rgba(34,197,94,0.6)]"
                  : "bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.6)] animate-pulse"
              }`}
            />
            <span className={`text-xs font-bold ${isConnected ? "text-green-700" : "text-red-700"}`}>
              {isConnected ? "Đã kết nối VNQR" : "Mất kết nối VNQR"}
            </span>
          </div>
          <div className="h-4 w-px bg-slate-200" />
          <div className="flex items-center gap-1.5 text-slate-600">
            <PlugZap className="w-3.5 h-3.5" />
            <span className="text-xs font-medium">{clientCount} client(s)</span>
          </div>
          <div className="h-4 w-px bg-slate-200" />
          <div className="text-xs text-slate-500 font-mono">
            {wsUrl.replace("ws://", "").replace("wss://", "")}
          </div>
        </div>
        <button
          onClick={reconnect}
          className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-lg transition-colors"
          title="Kết nối lại"
        >
          <RefreshCw className={`w-3.5 h-3.5 ${!isConnected ? "animate-spin" : ""}`} />
          Kết nối lại
        </button>
      </div>

      {/* Main Dashboard Layout */}
      <div className="flex flex-col xl:flex-row gap-4 xl:gap-5 h-full min-h-0 pb-1">
        {/* LEFT COLUMN */}
        <div className="flex flex-col gap-4 xl:gap-5 w-full xl:w-[60%] h-full min-h-0">
          {/* Kết quả vừa kiểm */}
          <Card className="shrink-0">
            <CardHeader title="KẾT QUẢ VỪA KIỂM" icon={Activity} />
            <div className="p-3 2xl:p-5 flex flex-col sm:flex-row gap-3 2xl:gap-5 h-[110px] 2xl:h-40">
              <div className={`w-full sm:w-[35%] rounded-xl 2xl:rounded-2xl text-white flex flex-col items-center justify-center p-2 2xl:p-4 shadow-lg ring-1 ring-white/20 ${
                appState === "Running"
                  ? "bg-gradient-to-br from-green-500 to-emerald-600 shadow-green-500/20"
                  : appState === "Error"
                    ? "bg-gradient-to-br from-red-500 to-red-600 shadow-red-500/20"
                    : "bg-gradient-to-br from-slate-500 to-slate-600 shadow-slate-500/20"
              }`}>
                <CheckCircle2
                  className="w-10 h-10 2xl:w-12 2xl:h-12 mb-1 2xl:mb-2"
                  strokeWidth={2.5}
                />
                <span className="text-2xl 2xl:text-3xl font-black tracking-widest leading-none">
                  {appState === "Running"
                    ? "TỐT"
                    : appState === "Error"
                      ? "LỖI"
                      : appState === "Idle"
                        ? "SẴN SÀNG"
                        : appState.toUpperCase()}
                </span>
              </div>

              <div className="flex-1 bg-slate-50 rounded-xl 2xl:rounded-2xl border border-slate-200/80 p-3 flex flex-col relative overflow-hidden text-left h-full">
                <div className="absolute top-2 right-2 p-2 2xl:p-3 opacity-[0.03] pointer-events-none">
                  <QrCode className="w-24 h-24 2xl:w-32 2xl:h-32" />
                </div>
                <div className="text-[10px] 2xl:text-xs font-black text-slate-400 uppercase tracking-widest mb-1 2xl:mb-2 flex items-center gap-1">
                  Trạng thái hệ thống
                </div>
                <div className="text-sm 2xl:text-lg font-mono font-medium text-slate-800 break-all leading-tight pr-8">
                  {d.timestamp ? new Date(d.timestamp).toLocaleString("vi-VN") : "Đang chờ..."}
                </div>
                <div className="mt-auto text-xs flex items-center gap-2">
                  <span className={`px-2 py-0.5 2xl:px-3 2xl:py-1 rounded-full text-[10px] 2xl:text-[11px] font-bold tracking-wide ${
                    appState === "Running"
                      ? "bg-green-100/80 text-green-700"
                      : appState === "Error"
                        ? "bg-red-100/80 text-red-700"
                        : "bg-slate-100/80 text-slate-700"
                  }`}>
                    {appState === "Running"
                      ? "ĐANG CHẠY"
                      : appState === "Idle"
                        ? "SẴN SÀNG"
                        : appState === "Error"
                          ? "LỖI"
                          : appState}
                  </span>
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
                onClick={() => setActiveTab("error")}
                className={`flex-1 py-2.5 2xl:py-3 px-4 text-xs 2xl:text-sm font-bold rounded-xl transition-all flex items-center justify-center gap-2 ${activeTab === "error" ? "bg-red-50 text-red-700" : "text-slate-500 hover:text-slate-700 hover:bg-slate-50"}`}
              >
                <AlertTriangle className="w-4 h-4" /> KIỂM TRA LỖI
              </button>
            </div>
            <div className="flex-1 overflow-auto bg-white p-0">
              <table className="w-full text-sm text-left relative">
                <thead className="text-[10px] 2xl:text-xs text-slate-400 uppercase bg-white sticky top-0 border-b border-slate-100 z-10 w-full backdrop-blur">
                  <tr>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Thời gian
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Nguồn
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 font-bold tracking-wider">
                      Nội dung sự kiện
                    </th>
                    <th className="px-5 2xl:px-6 py-3 2xl:py-4 text-right font-bold tracking-wider">
                      Trạng thái
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100/80">
                  {logs.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="px-5 2xl:px-6 py-8 text-center text-slate-400 text-sm">
                        Đang chờ dữ liệu từ VNQR...
                      </td>
                    </tr>
                  ) : (
                    logs.map((log) => (
                      <tr
                        key={log.id}
                        className="hover:bg-slate-50/50 transition-colors"
                      >
                        <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 font-mono text-[11px] 2xl:text-xs font-semibold text-slate-500 whitespace-nowrap">
                          {new Date(log.time).toLocaleTimeString("vi-VN")}
                        </td>
                        <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5">
                          <span className={`px-2 py-0.5 rounded-full text-[10px] 2xl:text-[11px] font-bold tracking-wide ${
                            log.source === "camera"
                              ? "bg-blue-100 text-blue-700"
                              : log.source === "plc"
                                ? "bg-purple-100 text-purple-700"
                                : log.source === "ws"
                                  ? "bg-green-100 text-green-700"
                                  : "bg-slate-100 text-slate-700"
                          }`}>
                            {log.source.toUpperCase()}
                          </span>
                        </td>
                        <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 text-xs 2xl:text-sm text-slate-700 font-medium">
                          {log.message}
                        </td>
                        <td className="px-5 2xl:px-6 py-2.5 2xl:py-3.5 text-right">
                          <span className={`px-2.5 2xl:px-3 py-1 rounded-full text-[10px] 2xl:text-[11px] font-bold tracking-wide uppercase ${
                            log.type === "success"
                              ? "bg-green-100 text-green-700"
                              : log.type === "error"
                                ? "bg-red-100 text-red-700"
                                : log.type === "warning"
                                  ? "bg-amber-100 text-amber-700"
                                  : "bg-blue-100 text-blue-700"
                          }`}>
                            {log.type}
                          </span>
                        </td>
                      </tr>
                    ))
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
            <div className="p-2 xl:p-3 2xl:p-4 grid grid-cols-3 grid-rows-2 gap-1.5 2xl:gap-2 flex-1 min-h-0">
              <DeviceIndicator
                icon={Monitor}
                label="CAMERA ACTIVE"
                subLabel={d.camera?.active?.ip || "-"}
                status={mapStatus(cameraActiveState)}
              />
              <DeviceIndicator
                icon={Monitor}
                label="CAMERA PACKAGE"
                subLabel={d.camera?.package?.ip || "-"}
                status={mapStatus(cameraPackageState)}
              />
              <DeviceIndicator
                icon={Cpu}
                label="PLC"
                subLabel="Omron CP2E"
                status={mapStatus(plcState)}
              />
              <AppStateIndicator state={appState} />
              <NetworkStrengthIndicator strength={d.networkStrength || 0} />
              <DeviceIndicator
                icon={Settings}
                label="VNQR"
                status={isConnected ? "ok" : "error"}
              />
            </div>
          </Card>

          <Card className="flex-[1.2] shadow-sm flex flex-col min-h-0">
            <CardHeader title="KẾT NỐI WEBSOCKET" icon={Wifi} />
            <div className="p-3 xl:p-4 2xl:p-5 flex flex-col justify-between gap-3 flex-1 overflow-auto">
              <div className="grid grid-cols-2 gap-3">
                <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3 flex items-center justify-center gap-2">
                  <div className={`w-2 h-2 rounded-full ${isConnected ? "bg-green-500 animate-pulse" : "bg-red-500"}`} />
                  <span className="text-xs font-bold text-slate-600">
                    {isConnected ? "ONLINE" : "OFFLINE"}
                  </span>
                </div>
                <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3 flex items-center justify-center gap-2">
                  <PlugZap className="w-4 h-4 text-blue-600" />
                  <span className="text-xs font-bold text-slate-600">
                    {clientCount} Client(s)
                  </span>
                </div>
              </div>

              <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3">
                <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Endpoint</div>
                <div className="text-xs font-mono text-slate-700 break-all">{wsUrl}</div>
              </div>

              <div className="bg-slate-50/50 rounded-xl border border-slate-100 px-4 py-3">
                <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">Thời gian nhận cuối</div>
                <div className="text-xs font-mono text-slate-700">
                  {d.timestamp ? new Date(d.timestamp).toLocaleString("vi-VN") : "Chưa có dữ liệu"}
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

const AdminPanelContent = () => {
  const [activeRoute, setActiveRoute] = useState(() => {
    const params = new URLSearchParams(window.location.search);
    return params.get("panel") || "monitor";
  });
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const { isOpen } = useVirtualKeyboard();

  const navigation = [
    { id: "monitor", title: "Giám sát SCADA", icon: LayoutDashboard },
    { id: "production", title: "Điều khiển SX", icon: Factory },
    { id: "devices", title: "Quản lý thiết bị", icon: Cpu },
    { id: "batches", title: "Lệnh sản xuất", icon: Package },
    { id: "datapool", title: "Quản lý DataPool", icon: Database },
    { id: "history", title: "Báo cáo sản xuất", icon: BarChart2 },
    { id: "users", title: "Quản lý tài khoản", icon: Users },
    { id: "settings", title: "Cấu hình hệ thống", icon: Settings },
  ];

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
                IoT.NEXUS
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
              AD
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-bold text-slate-800 truncate">
                Adminstrator
              </p>
              <p className="text-xs font-medium text-slate-500 truncate">
                admin@factory.local
              </p>
            </div>
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
          className={`flex-1 overflow-hidden p-4 lg:p-6 2xl:p-8 bg-[#F6F8FA] transition-all duration-300 ${isOpen ? "pb-[380px] md:pb-[420px]" : ""}`}
        >
          {activeRoute === "monitor" && <ScadaMonitorView />}
          {activeRoute === "production" && <ProductionView />}
          {activeRoute === "history" && <ProductionReportView />}
          {activeRoute === "settings" && <SettingsView />}
          {activeRoute === "batches" && <POManagerView />}
          {activeRoute === "datapool" && <DataPoolView />}
          {activeRoute === "production" && <ProductionView />}
          {activeRoute !== "monitor" &&
            activeRoute !== "production" &&
            activeRoute !== "history" &&
            activeRoute !== "settings" &&
            activeRoute !== "batches" &&
            activeRoute !== "datapool" && (
              <PlaceholderView
                title={
                  navigation.find((n) => n.id === activeRoute)?.title ||
                  "Chức năng"
                }
              />
            )}
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
    <KeyboardProvider>
      <AdminPanelContent />
    </KeyboardProvider>
  );
}
