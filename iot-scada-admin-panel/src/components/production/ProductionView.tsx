import React, { useState, useEffect, useCallback, useRef } from "react";
import {
  Play,
  Square,
  RotateCcw,
  RefreshCw,
  AlertCircle,
  AlertTriangle,
  Check,
  Package,
  CheckCircle2,
  XCircle,
  Copy,
  Box,
  TrendingUp,
  Monitor,
  Cpu,
  Wifi,
  WifiOff,
  Edit3,
} from "lucide-react";
import productionApi from "../../services/productionApi";
import poApi from "../../services/poApi";
import { useCameraSocket } from "../../hooks/useCameraSocket";
import { useDeviceStore } from "../../store/useDeviceStore";
import { handleActiveCodeScanned } from "../../services/cameraApi";
import type { ProductionStatusResponse } from "../../types/production";
import type { POListItem } from "../../types/po";

type CamUiStatus = "ok" | "error" | "offline" | "warning";

// Device status mapping using centralized store
const getCamStatusFromStore = (state: string | undefined, connected: boolean): CamUiStatus => {
  if (!connected) return "offline";
  if (state === "Disconnected") return "offline";
  return "ok";
};

const getPlcStatusFromStore = (state: string | undefined, connected: boolean): CamUiStatus => {
  if (!connected) return "offline";
  if (!state) return "warning";
  if (state === "Disconnected" || state === "Reconnecting") return "error";
  return "ok";
};

const ProductionView: React.FC = () => {
  const [status, setStatus] = useState<ProductionStatusResponse | null>(null);
  const [poList, setPoList] = useState<POListItem[]>([]);
  const [selectedPO, setSelectedPO] = useState<string>("");
  const [isLoading, setIsLoading] = useState(false);
  const [isStarting, setIsStarting] = useState(false);
  const [isStopping, setIsStopping] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [isChangingPO, setIsChangingPO] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [healthOk, setHealthOk] = useState(false);

  const statusRef = useRef<ProductionStatusResponse | null>(null);
  useEffect(() => {
    statusRef.current = status;
  }, [status]);

  const fetchStatus = useCallback(async () => {
    try {
      const data = await productionApi.getStatus();
      setStatus(data);
      setHealthOk(true);
    } catch {
      setHealthOk(false);
      // Khong hien thi error message khi mat ket noi
    }
  }, []);

  const fetchPOList = useCallback(async () => {
    try {
      const list = await poApi.getAllPOs();
      setPoList(list);
    } catch {
      // ignore
    }
  }, []);

  useEffect(() => {
    fetchStatus();
    fetchPOList();
    const interval = setInterval(fetchStatus, 1000);
    return () => clearInterval(interval);
  }, [fetchStatus, fetchPOList]);

  // Auto-dismiss messages
  useEffect(() => {
    if (success) {
      const t = setTimeout(() => setSuccess(null), 4000);
      return () => clearTimeout(t);
    }
  }, [success]);

  useEffect(() => {
    if (error) {
      const t = setTimeout(() => setError(null), 6000);
      return () => clearTimeout(t);
    }
  }, [error]);

  // Get device status from centralized store
  const cameraStatus = useDeviceStore((s) => s.camera);
  const plcStatus = useDeviceStore((s) => s.plc);
  const productionStatus = useDeviceStore((s) => s.production);

  // Camera WebSocket - auto addFromReader khi nhận Received từ camera (logs only, store sync handled by App.tsx)
  const cameraWsUrl =
    import.meta.env.VITE_CAMERA_WS_URL || "ws://localhost:9999/ws/camera";
  const { lastScan } = useCameraSocket({
    url: cameraWsUrl,
    onEvent: async (msg) => {
      if (msg.state !== "Received" || msg.camera !== "camera") return;
      if (!msg.data) return;

      const currentOrderNo = statusRef.current?.orderNo ?? "";
      const batchID = currentOrderNo || "default";
      try {
        await handleActiveCodeScanned(msg.data, "camera", batchID);
        await fetchStatus();
        setSuccess(`Đã quét: ${msg.data}`);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Lỗi xử lý code quét"
        );
      }
    },
    syncToStore: false, // Store sync handled by App.tsx hooks
  });

// PLC state from store (updated via REST polling)
// Treat healthOk as the production connection signal: BE down => request fails => banner red.
const productionConnected = healthOk;

  const handleStartProduction = async () => {
    if (!selectedPO) {
      setError("Vui lòng chọn một PO để bắt đầu.");
      return;
    }
    setIsStarting(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.startProduction({
        orderNo: selectedPO,
        userName: "Frontend",
      });
      if (result.success) {
        setSuccess(`Đã bắt đầu sản xuất: ${selectedPO}`);
        await fetchStatus();
      } else {
        setError(result.message || "Không thể bắt đầu sản xuất.");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi khi bắt đầu.");
    } finally {
      setIsStarting(false);
    }
  };

  const handleStopProduction = async () => {
    setIsStopping(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.stopProduction("Frontend");
      if (result.success) {
        setSuccess("Đã dừng sản xuất.");
        await fetchStatus();
      } else {
        setError(result.message || "Không thể dừng sản xuất.");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi khi dừng.");
    } finally {
      setIsStopping(false);
    }
  };

  const handleResetProduction = async () => {
    setIsResetting(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.resetProduction("Frontend");
      if (result.success) {
        setSuccess("Đã reset sản xuất.");
        setSelectedPO("");
        await fetchStatus();
      } else {
        setError(result.message || "Không thể reset sản xuất.");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi khi reset.");
    } finally {
      setIsResetting(false);
    }
  };

  const handleChangePO = async () => {
    setIsChangingPO(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.selectPO("");
      if (result.success) {
        setSuccess("Đã chuyển sang chế độ chỉnh sửa. Vui lòng chọn PO và nhấn Cài Lệnh.");
      } else {
        setError(result.message || "Không thể đổi PO.");
        setIsChangingPO(false);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi khi đổi PO.");
      setIsChangingPO(false);
    }
  };

  const handleSetPO = async () => {
    if (!selectedPO) {
      setError("Vui lòng chọn một PO để cài lệnh.");
      return;
    }
    setIsChangingPO(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.setPO(selectedPO);
      if (result.success) {
        setSuccess(`Đã cài PO: ${selectedPO}`);
        await fetchStatus();
      } else {
        setError(result.message || "Không thể cài lệnh.");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Lỗi khi cài lệnh.");
    } finally {
      setIsChangingPO(false);
    }
  };

  const isRunning = status?.state === "Running";
  const isEditing = status?.state === "Editing";
  const isReady = status?.state === "Ready";
  const isIdle = status?.state === "NoSelectedPO";
  const canStart = !isRunning && status?.hasPO === false && !!selectedPO;
  const canStop = isRunning;
  const canReset = status?.hasPO === true;

  const stateConfig: Record<string, { label: string; bg: string; text: string; dot: string; icon: string }> = {
    NeedLogin: { label: "NEED LOGIN", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
    Checking: { label: "CHECKING", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500 animate-pulse", icon: "" },
    NoSelectedPO: { label: "NO PO", bg: "bg-slate-50", text: "text-slate-600", dot: "bg-slate-400", icon: "" },
    Editing: { label: "EDITING", bg: "bg-amber-50", text: "text-amber-700", dot: "bg-amber-500", icon: "" },
    CheckPO: { label: "CHECK PO", bg: "bg-indigo-50", text: "text-indigo-700", dot: "bg-indigo-500 animate-pulse", icon: "" },
    LoadPO: { label: "LOAD PO", bg: "bg-indigo-50", text: "text-indigo-700", dot: "bg-indigo-500 animate-pulse", icon: "" },
    Ready: { label: "READY", bg: "bg-green-50", text: "text-green-700", dot: "bg-green-500", icon: "" },
    PushingToDic: { label: "PUSHING TO DIC", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500 animate-pulse", icon: "" },
    Running: { label: "RUNNING", bg: "bg-green-50", text: "text-green-800", dot: "bg-green-500 animate-pulse", icon: "" },
    Paused: { label: "PAUSED", bg: "bg-amber-50", text: "text-amber-800", dot: "bg-amber-500", icon: "" },
    WaitingStop: { label: "WAITING STOP", bg: "bg-orange-50", text: "text-orange-700", dot: "bg-orange-500 animate-pulse", icon: "" },
    CheckAfterCompleted: { label: "CHECKING", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500 animate-pulse", icon: "" },
    Completed: { label: "COMPLETED", bg: "bg-emerald-50", text: "text-emerald-700", dot: "bg-emerald-500", icon: "" },
    DeviceError: { label: "DEVICE ERROR", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
    Error: { label: "ERROR", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
    CheckingQueue: { label: "CHECKING QUEUE", bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500 animate-pulse", icon: "" },
    Saving: { label: "SAVING", bg: "bg-purple-50", text: "text-purple-700", dot: "bg-purple-500 animate-pulse", icon: "" },
  };

  const state = status?.state || "Checking";
  const cfg = stateConfig[state] || { label: state, bg: "bg-slate-50", text: "text-slate-700", dot: "bg-slate-500", icon: "" };

  // Override colors and label when WS is disconnected
  const effectiveCfg = !productionConnected
    ? { ...cfg, bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", label: "Mất Kết Nối" }
    : cfg;

  return (
    <div className="flex flex-col gap-4 h-full min-h-0 w-full animate-in fade-in duration-500 overflow-auto scrollbar-hide pb-6">
      {/* Header */}
      <div className="flex items-center justify-between shrink-0 px-1 flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-green-500 to-emerald-600 flex items-center justify-center shadow-lg shadow-green-500/20">
            <TrendingUp className="w-5 h-5 text-white" />
          </div>
          <div>
            <h1 className="text-xl 2xl:text-2xl font-bold text-slate-800 tracking-tight">
              Điều khiển Sản xuất
            </h1>
            <p className="text-xs text-slate-500 mt-0.5">
              Bắt đầu / Dừng / Reset production từ React Frontend
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <div
            className={`flex items-center gap-2 px-3 py-1.5 rounded-xl border text-xs font-semibold ${
              healthOk
                ? "bg-green-50 border-green-200 text-green-700"
                : "bg-red-50 border-red-200 text-red-700"
            }`}
          >
            <div
              className={`w-2 h-2 rounded-full ${
                healthOk
                  ? "bg-green-500 shadow-[0_0_6px_rgba(34,197,94,0.6)]"
                  : "bg-red-500 animate-pulse"
              }`}
            />
            {healthOk ? "API Online" : "API Offline"}
          </div>
          <button
            onClick={() => { fetchStatus(); fetchPOList(); }}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-xl border border-blue-100 transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? "animate-spin" : ""}`} />
            Làm mới
          </button>
        </div>
      </div>

      {/* Toast Notifications */}
      {(error || success) && (
        <div
          className={`flex items-center gap-3 px-4 py-3 rounded-2xl border shrink-0 animate-in slide-in-from-top-2 ${
            error
              ? "bg-red-50 border-red-200 text-red-800"
              : "bg-green-50 border-green-200 text-green-800"
          }`}
        >
          {error ? (
            <AlertCircle className="w-5 h-5 shrink-0" />
          ) : (
            <Check className="w-5 h-5 shrink-0" />
          )}
          <span className="text-sm font-semibold flex-1">{error || success}</span>
          <button
            onClick={() => { setError(null); setSuccess(null); }}
            className="p-1 hover:bg-white/50 rounded-lg transition-colors"
          >
            <XCircle className="w-4 h-4" />
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        {/* Control Panel */}
        <div className="xl:col-span-2 flex flex-col gap-4">
          {/* State Banner */}
          <div className={`rounded-2xl border p-4 ${effectiveCfg.bg}`}>
            <div className="flex items-center gap-3 flex-wrap">
              <div className={`w-3 h-3 rounded-full ${effectiveCfg.dot}`} />
              <span className={`text-sm font-black tracking-wider ${effectiveCfg.text}`}>
                {effectiveCfg.label}
              </span>
              {status?.hasPO && (
                <span className="ml-2 text-xs font-semibold text-slate-500">
                  • {status.orderNo}
                </span>
              )}
              {/* lastWarning indicator */}
              {productionStatus?.lastWarning && (
                <span className="ml-2 flex items-center gap-1 px-2 py-0.5 bg-amber-100 text-amber-700 text-xs font-semibold rounded-full">
                  <AlertTriangle className="w-3 h-3" />
                  {productionStatus.lastWarning}
                </span>
              )}
              {/* WebSocket status */}
              <span className="ml-auto flex items-center gap-1.5">
                {!productionConnected && (
                  <span className="px-2 py-0.5 bg-red-100 text-red-700 text-xs font-bold rounded-full animate-pulse">
                    Mất Kết Nối
                  </span>
                )}
                {productionConnected ? (
                  <Wifi className="w-3.5 h-3.5 text-green-500" />
                ) : (
                  <WifiOff className="w-3.5 h-3.5 text-red-500" />
                )}
                <span className={productionConnected ? "text-green-600" : "text-red-500"}>
                  WS {productionConnected ? "Online" : "Offline"}
                </span>
              </span>
            </div>
          </div>

          {/* PO Selection + Controls */}
          <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
            <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
              <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                <Package className="w-4 h-4 text-green-600" /> Chọn PO &amp; Điều khiển
              </h2>
            </div>
            <div className="p-4 xl:p-6 flex flex-col gap-4">
              {/* PO Select */}
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold uppercase tracking-wider text-slate-600">
                  Chọn Lệnh sản xuất (PO)
                </label>
                <select
                  value={selectedPO}
                  onChange={(e) => setSelectedPO(e.target.value)}
                  disabled={!isEditing}
                  className="w-full bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-xl p-2.5 outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors disabled:opacity-50"
                >
                  <option value="">-- Chọn PO --</option>
                  {poList.map((po) => (
                    <option key={po.orderNo} value={po.orderNo}>
                      {po.orderNo} - {po.productName || "(no name)"} ({po.orderQty} pcs)
                    </option>
                  ))}
                </select>
              </div>

              {/* Action Buttons */}
              <div className="flex items-center gap-3 flex-wrap">
                {/* Start */}
                <button
                  onClick={handleStartProduction}
                  disabled={isStarting || isStopping || isResetting || !selectedPO || isRunning || isEditing}
                  className={`flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all ${
                    isStarting || !selectedPO || isRunning || isEditing
                      ? "bg-slate-100 text-slate-400 cursor-not-allowed"
                      : "bg-green-600 hover:bg-green-700 text-white shadow-lg shadow-green-500/20"
                  }`}
                >
                  {isStarting ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    <Play className="w-4 h-4" />
                  )}
                  {isStarting ? "Đang bắt đầu..." : "Bắt đầu (Start)"}
                </button>

                {/* Stop */}
                <button
                  onClick={handleStopProduction}
                  disabled={isStarting || isStopping || isResetting || !isRunning}
                  className={`flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all ${
                    !isRunning
                      ? "bg-slate-100 text-slate-400 cursor-not-allowed"
                      : "bg-red-600 hover:bg-red-700 text-white shadow-lg shadow-red-500/20"
                  }`}
                >
                  {isStopping ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    <Square className="w-4 h-4" />
                  )}
                  {isStopping ? "Đang dừng..." : "Dừng (Stop)"}
                </button>

                {/* Reset */}
                <button
                  onClick={handleResetProduction}
                  disabled={true}
                  className="flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all bg-slate-100 text-slate-400 cursor-not-allowed"
                >
                  {isResetting ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    <RotateCcw className="w-4 h-4" />
                  )}
                  {isResetting ? "Đang reset..." : "Reset PO"}
                </button>

                {/* Change PO - doi PO tu Ready sang Editing */}
                <button
                  onClick={handleChangePO}
                  disabled={isStarting || isStopping || isResetting || status?.totalCount !== 0 || !isReady}
                  className={`flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all ${
                    status?.totalCount !== 0 || !isReady
                      ? "bg-slate-100 text-slate-400 cursor-not-allowed"
                      : "bg-orange-500 hover:bg-orange-600 text-white shadow-lg shadow-orange-500/20"
                  }`}
                >
                  <Edit3 className="w-4 h-4" />
                  Đổi PO
                </button>

                {/* Cai Lenh - cai dat PO tu Editing */}
                <button
                  onClick={handleSetPO}
                  disabled={isStarting || isStopping || isResetting || !isEditing || !selectedPO}
                  className={`flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all ${
                    !isEditing || !selectedPO
                      ? "bg-slate-100 text-slate-400 cursor-not-allowed"
                      : "bg-blue-600 hover:bg-blue-700 text-white shadow-lg shadow-blue-500/20"
                  }`}
                >
                  <CheckCircle2 className="w-4 h-4" />
                  Cài Lệnh
                </button>
              </div>

              {/* Info text */}
              <div className="text-xs text-slate-400 flex items-center gap-2">
                <AlertCircle className="w-3.5 h-3.5" />
                {isEditing ? "Chọn PO → Nhấn Cài Lệnh để thiết lập. Start để bắt đầu sản xuất." : "Chọn PO → Start để bắt đầu. Đổi PO để thay đổi lệnh sản xuất."}
              </div>
            </div>
          </div>

          {/* Live Stats */}
          {status?.hasPO && (
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <TrendingUp className="w-4 h-4 text-blue-600" /> Thông tin PO hiện tại
                </h2>
              </div>
              <div className="p-4 xl:p-6">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
                  <StatCard label="Mã PO" value={status.orderNo || "-"} mono />
                  <StatCard label="Sản phẩm" value={status.productName || "-"} />
                  <StatCard label="SL Order" value={status.orderQty.toLocaleString()} />
                  <StatCard label="Tiến độ" value={`${status.progressPercent.toFixed(1)}%`} color="blue" />
                </div>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                  <StatCard label="Tổng đếm" value={status.totalCount.toLocaleString()} color="slate" />
                  <StatCard label="Pass ✓" value={status.passCount.toLocaleString()} color="green" />
                  <StatCard label="Fail ✗" value={status.failCount.toLocaleString()} color="red" />
                  <StatCard label="Trùng ⚡" value={status.duplicateCount.toLocaleString()} color="amber" />
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Right Column: Carton Info */}
        <div className="flex flex-col gap-4">
          {/* Carton Status */}
          <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
            <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
              <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                <Box className="w-4 h-4 text-purple-600" /> Thông tin Thùng
              </h2>
            </div>
            <div className="p-4 xl:p-6 flex flex-col gap-3">
              <div className="grid grid-cols-2 gap-3">
                <StatCard label="Tổng thùng" value={status?.cartonCount?.toString() || "0"} />
                <StatCard label="Đã đóng" value={status?.cartonClosedCount?.toString() || "0"} color="green" />
              </div>
              <div className="rounded-xl border border-slate-200 bg-slate-50 p-3">
                <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
                  Thùng hiện tại
                </div>
                <div className="text-lg font-black text-slate-800 font-mono">
                  #{status?.currentCartonId || 0}
                </div>
                <div className="text-xs font-semibold text-slate-600 mt-0.5 font-mono">
                  {status?.currentCartonCode || "—"}
                </div>
              </div>
              <div className="rounded-xl border border-slate-200 bg-slate-50 p-3">
                <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-2">
                  Items trong thùng
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-2xl font-black text-slate-800">
                    {status?.itemsInCarton || 0}
                  </span>
                  <span className="text-sm text-slate-400">/</span>
                  <span className="text-lg font-bold text-slate-500">
                    {status?.cartonCapacity || 24}
                  </span>
                </div>
                {/* Progress bar */}
                <div className="mt-2 h-2 bg-slate-200 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-gradient-to-r from-blue-500 to-purple-500 rounded-full transition-all duration-500"
                    style={{
                      width: `${Math.min(
                        ((status?.itemsInCarton || 0) / (status?.cartonCapacity || 24)) * 100,
                        100
                      )}%`,
                    }}
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Camera Status */}
          <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
            <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
              <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                <Monitor className="w-4 h-4 text-indigo-600" /> Trạng thái Camera
              </h2>
            </div>
            <div className="p-3 xl:p-4 flex flex-col gap-2">
              <MiniDeviceIndicator
                icon={Monitor}
                label="CAMERA"
                subLabel={
                  cameraStatus.camera.lastCode
                    ? `${cameraStatus.camera.lastCode} @ ${new Date(
                        cameraStatus.camera.lastAt || Date.now()
                      ).toLocaleTimeString("vi-VN")}`
                    : "-"
                }
                status={getCamStatusFromStore(cameraStatus.camera.state, cameraStatus.connected)}
              />
              <MiniDeviceIndicator
                icon={Cpu}
                label="PLC"
                subLabel={
                  plcStatus.ip
                    ? `${plcStatus.ip}:${plcStatus.port ?? ""}${plcStatus.message ? ` - ${plcStatus.message}` : ""}`
                    : plcStatus.connected
                      ? "online"
                      : "offline"
                }
                status={getPlcStatusFromStore(plcStatus.state, plcStatus.connected)}
              />
              <div className="mt-1 text-[10px] text-slate-400 font-mono break-all">
                WS: cam {cameraStatus.connected ? "online" : "offline"} • plc {plcStatus.connected ? "online" : "offline"}
              </div>
            </div>
          </div>

          {/* Quick Reference */}
          <div className="bg-gradient-to-br from-slate-800 to-slate-900 rounded-2xl p-4 text-white">
            <h3 className="text-xs font-bold uppercase tracking-wider opacity-60 mb-3">
              API Endpoints
            </h3>
            <div className="space-y-2 text-xs font-mono">
              <div className="flex items-start gap-2">
                <span className="text-green-400 shrink-0">POST</span>
                <span className="opacity-80">/api/production/start</span>
              </div>
              <div className="flex items-start gap-2">
                <span className="text-red-400 shrink-0">POST</span>
                <span className="opacity-80">/api/production/stop</span>
              </div>
              <div className="flex items-start gap-2">
                <span className="text-amber-400 shrink-0">POST</span>
                <span className="opacity-80">/api/production/reset</span>
              </div>
              <div className="flex items-start gap-2">
                <span className="text-blue-400 shrink-0">GET</span>
                <span className="opacity-80">/api/production/status</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// Helper stat card component
interface StatCardProps {
  label: string;
  value: string | number;
  mono?: boolean;
  color?: "default" | "green" | "red" | "blue" | "amber" | "slate";
}

const StatCard: React.FC<StatCardProps> = ({ label, value, mono, color = "default" }) => {
  const colorMap = {
    default: "bg-slate-50 border-slate-200 text-slate-800",
    green: "bg-green-50 border-green-200 text-green-800",
    red: "bg-red-50 border-red-200 text-red-800",
    blue: "bg-blue-50 border-blue-200 text-blue-800",
    amber: "bg-amber-50 border-amber-200 text-amber-800",
    slate: "bg-slate-50 border-slate-200 text-slate-800",
  };

  return (
    <div className={`rounded-xl border p-3 text-center ${colorMap[color]}`}>
      <div className="text-[9px] font-bold uppercase tracking-wider opacity-60 mb-1">
        {label}
      </div>
      <div className={`text-lg font-black tracking-tight ${mono ? "font-mono" : ""}`}>
        {value}
      </div>
    </div>
  );
};

const MiniDeviceIndicator: React.FC<{
  icon: React.ElementType;
  label: string;
  subLabel?: string;
  status: CamUiStatus;
}> = ({ icon: Icon, label, subLabel, status }) => {
  const styles: Record<CamUiStatus, string> = {
    ok: "bg-green-50 text-green-700 border-green-100",
    error: "bg-red-50 text-red-700 border-red-100",
    warning: "bg-amber-50 text-amber-700 border-amber-100",
    offline: "bg-slate-50 text-slate-600 border-slate-100",
  };
  const dotStyles: Record<CamUiStatus, string> = {
    ok: "bg-green-500",
    error: "bg-red-500 animate-pulse",
    warning: "bg-amber-500",
    offline: "bg-slate-400",
  };
  return (
    <div
      className={`flex items-center justify-between px-2.5 py-2 rounded-xl border ${styles[status]} min-w-0`}
    >
      <div className="flex items-center gap-2 min-w-0 flex-1">
        <Icon className="w-3.5 h-3.5 shrink-0" strokeWidth={2.5} />
        <div className="min-w-0 flex-1">
          <span className="text-[10px] font-bold tracking-wide uppercase block">
            {label}
          </span>
          {subLabel && (
            <span className="text-[9px] font-medium text-slate-500 font-mono tracking-wide truncate block">
              {subLabel}
            </span>
          )}
        </div>
      </div>
      <div className={`w-2 h-2 rounded-full shrink-0 ${dotStyles[status]}`} />
    </div>
  );
};

export default ProductionView;
