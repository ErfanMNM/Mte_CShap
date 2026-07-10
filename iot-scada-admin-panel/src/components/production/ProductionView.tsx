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
  Calendar,
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
  const [cooldownUntil, setCooldownUntil] = useState<number>(0);
  const [isStarting, setIsStarting] = useState(false);
  const [isStopping, setIsStopping] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [isChangingPO, setIsChangingPO] = useState(false);
  const [isEditingDate, setIsEditingDate] = useState(false);
  const [newProductionDate, setNewProductionDate] = useState("");
  const [isSavingDate, setIsSavingDate] = useState(false);
  const [isRefreshingPO, setIsRefreshingPO] = useState(false);
  const isRefreshingPORef = useRef(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [healthOk, setHealthOk] = useState(false);

  const notifMap: Record<string, string> = {
    // Error keys
    NOTIF_ERR_LOAD_PO: "Không thể tải danh sách PO.",
    NOTIF_ERR_SELECT_PO: "Vui lòng chọn một PO để bắt đầu.",
    NOTIF_ERR_SELECT_PO_SET: "Vui lòng chọn một PO để cài lệnh.",
    NOTIF_ERR_START: "Không thể bắt đầu sản xuất.",
    NOTIF_ERR_START_UNKNOWN: "Lỗi khi bắt đầu.",
    NOTIF_ERR_STOP: "Không thể dừng sản xuất.",
    NOTIF_ERR_STOP_UNKNOWN: "Lỗi khi dừng.",
    NOTIF_ERR_RESET: "Không thể reset sản xuất.",
    NOTIF_ERR_RESET_UNKNOWN: "Lỗi khi reset.",
    NOTIF_ERR_CHANGE_PO: "Không thể đổi PO.",
    NOTIF_ERR_CHANGE_PO_UNKNOWN: "Lỗi khi đổi PO.",
    NOTIF_ERR_SET_PO: "Không thể cài lệnh.",
    NOTIF_ERR_SET_PO_UNKNOWN: "Lỗi khi cài lệnh.",
    NOTIF_ERR_EDIT_DATE_NOT_READY: "Chỉ có thể sửa ngày SX khi đang ở trạng thái Ready.",
    NOTIF_ERR_MISSING_DATE: "Vui lòng nhập ngày sản xuất.",
    NOTIF_ERR_UPDATE_DATE: "Không thể cập nhật ngày SX.",
    NOTIF_ERR_UPDATE_DATE_UNKNOWN: "Lỗi khi lưu ngày SX.",
    NOTIF_ERR_SCAN: "Lỗi xử lý code quét.",
    // Success keys
    NOTIF_PO_LIST_RELOADED: "Đã tải lại danh sách PO ({count} mục).",
    NOTIF_PO_STARTED: "Đã bắt đầu sản xuất: {po}.",
    NOTIF_STOPPED: "Đã dừng sản xuất.",
    NOTIF_RESET: "Đã reset sản xuất.",
    NOTIF_CHANGE_PO_SUCCESS: "Đã chuyển sang chế độ chỉnh sửa. Vui lòng chọn PO và nhấn Cài Lệnh.",
    NOTIF_PO_SET: "Đã cài PO: {po}.",
    NOTIF_DATE_UPDATED: "Đã cập nhật ngày sản xuất.",
  };

  const t = (key: string, params?: Record<string, string | number>) => {
    if (notifMap[key]) {
      let msg = notifMap[key];
      if (params) {
        Object.entries(params).forEach(([k, v]) => {
          msg = msg.replace(`{${k}}`, String(v));
        });
      }
      return msg;
    }
    if (key.startsWith("NOTIF_")) return key;
    return key;
  };

  const statusRef = useRef<ProductionStatusResponse | null>(null);
  useEffect(() => {
    statusRef.current = status;
  }, [status]);

  const fetchStatus = useCallback(async () => {
    if (isRefreshingPORef.current) return;
    isRefreshingPORef.current = true;
    setIsRefreshingPO(true);
    try {
      const data = await productionApi.getStatus();
      setStatus(data);
      setHealthOk(true);
    } catch {
      setHealthOk(false);
    } finally {
      // Debounce: không cho gọi lại trong 2 giây sau khi hoàn thành
      setTimeout(() => {
        isRefreshingPORef.current = false;
        setIsRefreshingPO(false);
      }, 2000);
    }
  }, []);

  const fetchPOList = useCallback(async () => {
    setIsLoading(true);
    try {
      const list = await poApi.getAllPOs();
      setPoList(list);
      setSuccess(t("NOTIF_PO_LIST_RELOADED", { count: list.length }));
    } catch {
      setError("NOTIF_ERR_LOAD_PO");
    } finally {
      setIsLoading(false);
      setCooldownUntil(Date.now() + 2000);
    }
  }, [setSuccess, setError]);

  const handleReloadPOList = useCallback(async () => {
    if (isLoading || Date.now() < cooldownUntil) return;
    await fetchPOList();
  }, [isLoading, cooldownUntil, fetchPOList]);

  const [cooldownLeft, setCooldownLeft] = useState(0);
  const initializedRef = useRef(false);

  useEffect(() => {
    if (cooldownUntil === 0) {
      setCooldownLeft(0);
      return;
    }
    const tick = () => {
      const left = Math.max(0, Math.ceil((cooldownUntil - Date.now()) / 1000));
      setCooldownLeft(left);
    };
    tick();
    const id = setInterval(tick, 250);
    return () => clearInterval(id);
  }, [cooldownUntil]);

  useEffect(() => {
    // Skip if already initialized (handles StrictMode double-invoke)
    if (initializedRef.current) return;
    initializedRef.current = true;

    fetchStatus();
    fetchPOList();
    const interval = setInterval(fetchStatus, 1000);
    return () => {
      clearInterval(interval);
      initializedRef.current = false;
    };
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
        const msg = err instanceof Error ? err.message : "NOTIF_ERR_SCAN";
        setError(msg);
      }
    },
    syncToStore: false, // Store sync handled by App.tsx hooks
  });

// PLC state from store (updated via REST polling)
// Treat healthOk as the production connection signal: BE down => request fails => banner red.
const productionConnected = healthOk;

  const handleStartProduction = async () => {
    if (!selectedPO) {
      setError("NOTIF_ERR_SELECT_PO");
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
        setSuccess(t("NOTIF_PO_STARTED", { po: selectedPO }));
        await fetchStatus();
      } else {
        setError(result.message || t("NOTIF_ERR_START"));
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_START_UNKNOWN";
      setError(msg);
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
        setSuccess("NOTIF_STOPPED");
        await fetchStatus();
      } else {
        setError(`NOTIF_ERR_STOP:${result.message || ""}`);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_STOP_UNKNOWN";
      setError(msg);
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
        setSuccess("NOTIF_RESET");
        setSelectedPO("");
        await fetchStatus();
      } else {
        setError(`NOTIF_ERR_RESET:${result.message || ""}`);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_RESET_UNKNOWN";
      setError(msg);
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
        setSuccess("NOTIF_CHANGE_PO_SUCCESS");
      } else {
        setError(`NOTIF_ERR_CHANGE_PO:${result.message || ""}`);
        setIsChangingPO(false);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_CHANGE_PO_UNKNOWN";
      setError(msg);
      setIsChangingPO(false);
    }
  };

  const handleSetPO = async () => {
    if (!selectedPO) {
      setError("NOTIF_ERR_SELECT_PO_SET");
      return;
    }
    setIsChangingPO(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.setPO(selectedPO);
      if (result.success) {
        setSuccess(t("NOTIF_PO_SET", { po: selectedPO }));
        await fetchStatus();
      } else {
        setError(result.message || t("NOTIF_ERR_SET_PO"));
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_SET_PO_UNKNOWN";
      setError(msg);
    } finally {
      setIsChangingPO(false);
    }
  };

  const handleEditProductionDate = () => {
    if (!isReady) {
      setError("NOTIF_ERR_EDIT_DATE_NOT_READY");
      return;
    }
    if (status?.productionDate) {
      const datePart = status.productionDate.split(' ')[0];
      setNewProductionDate(datePart);
    } else {
      setNewProductionDate(new Date().toISOString().split('T')[0]);
    }
    setIsEditingDate(true);
    setError(null);
    setSuccess(null);
  };

  const handleSaveProductionDate = async () => {
    if (!selectedPO || !newProductionDate) {
      setError("NOTIF_ERR_MISSING_DATE");
      return;
    }
    setIsSavingDate(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await productionApi.updateProductionDate(selectedPO, newProductionDate);
      if (result.success) {
        setSuccess("NOTIF_DATE_UPDATED");
        setIsEditingDate(false);
        await fetchStatus();
      } else {
        setError(`NOTIF_ERR_UPDATE_DATE:${result.message || ""}`);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_UPDATE_DATE_UNKNOWN";
      setError(msg);
    } finally {
      setIsSavingDate(false);
    }
  };

  const handleCancelEditDate = () => {
    setIsEditingDate(false);
    setNewProductionDate("");
    setError(null);
    setSuccess(null);
  };

  const isRunning = status?.state === "Running";
  const isEditing = status?.state === "Editing";
  const isReady = status?.state === "Ready";
  const isIdle = status?.state === "NoSelectedPO";
  const canStart = !isRunning && status?.hasPO === true && !!selectedPO;
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
          <span className="text-sm font-semibold flex-1">{error ? t(error) : t(success || "")}</span>
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
                <div className="flex items-center gap-2">
                  <select
                    value={selectedPO}
                    onChange={(e) => setSelectedPO(e.target.value)}
                    disabled={!isEditing}
                    className="flex-1 bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-xl p-2.5 outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors disabled:opacity-50"
                  >
                    <option value="">-- Chọn PO --</option>
                    {poList.map((po) => (
                      <option key={po.orderNo} value={po.orderNo}>
                        {po.orderNo} - {po.productName || "(no name)"} ({po.orderQty} pcs)
                      </option>
                    ))}
                  </select>
                  <button
                    type="button"
                    onClick={handleReloadPOList}
                    disabled={!isEditing || isLoading || cooldownLeft > 0}
                    title={
                      !isEditing
                        ? "Chỉ khả dụng ở trạng thái Editing"
                        : cooldownLeft > 0
                          ? `Vui lòng đợi ${cooldownLeft}s`
                          : "Tải lại danh sách PO"
                    }
                    className={`flex items-center justify-center w-10 h-10 border rounded-xl transition-all duration-200 active:scale-95 shrink-0 disabled:cursor-not-allowed ${
                      isEditing && !isLoading && cooldownLeft === 0
                        ? "bg-amber-50 hover:bg-amber-100 border-amber-300 hover:border-amber-400 text-amber-700 hover:text-amber-800 hover:scale-105 shadow-sm shadow-amber-500/10"
                        : "bg-slate-100 border-slate-200 text-slate-400"
                    }`}
                  >
                    {isLoading ? (
                      <RefreshCw className="w-4 h-4 animate-spin text-amber-600" />
                    ) : cooldownLeft > 0 ? (
                      <span className="text-xs font-bold tabular-nums">{cooldownLeft}</span>
                    ) : (
                      <RefreshCw className="w-4 h-4" />
                    )}
                  </button>
                </div>
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
                <div className="flex items-center justify-between">
                  <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                    <TrendingUp className="w-4 h-4 text-blue-600" /> Thông tin PO hiện tại
                  </h2>
                  <button
                    onClick={fetchStatus}
                    disabled={isRefreshingPO}
                    className={`p-1.5 rounded-lg transition-all ${
                      isRefreshingPO
                        ? 'bg-blue-100 text-blue-500 cursor-not-allowed'
                        : 'bg-slate-100 hover:bg-slate-200 text-slate-600 hover:text-slate-800'
                    }`}
                    title="Tải lại thông tin"
                  >
                    <RefreshCw className={`w-4 h-4 ${isRefreshingPO ? 'animate-spin' : ''}`} />
                  </button>
                </div>
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

          {/* Production Date Section */}
          {status?.hasPO && isReady && (
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <Calendar className="w-4 h-4 text-blue-600" /> Ngày Sản Xuất
                </h2>
              </div>
              <div className="p-4 xl:p-6">
                {!isEditingDate ? (
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
                        Ngày Sản Xuất hiện tại
                      </div>
                      <div className="text-xl font-black text-slate-800">
                        {status.productionDate ? new Date(status.productionDate).toLocaleDateString('vi-VN', {
                          year: 'numeric', month: '2-digit', day: '2-digit'
                        }) : "—"}
                      </div>
                    </div>
                    <button
                      onClick={handleEditProductionDate}
                      disabled={!isReady || isSavingDate}
                      className={`flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold transition-all ${
                        isReady
                          ? "bg-blue-500 hover:bg-blue-600 text-white shadow-lg shadow-blue-500/20"
                          : "bg-slate-100 text-slate-400 cursor-not-allowed"
                      }`}
                    >
                      <Edit3 className="w-4 h-4" />
                      Sửa Ngày SX
                    </button>
                  </div>
                ) : (
                  <div className="flex flex-col gap-3">
                    <div className="flex items-center gap-3 flex-wrap">
                      <div className="flex-1 min-w-[200px]">
                        <label className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1 block">
                          Chọn ngày mới
                        </label>
                        <input
                          type="date"
                          value={newProductionDate}
                          onChange={(e) => setNewProductionDate(e.target.value)}
                          className="w-full px-3 py-2 border border-blue-300 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
                        />
                      </div>
                      <div className="flex items-end gap-2">
                        <button
                          onClick={handleSaveProductionDate}
                          disabled={isSavingDate || !newProductionDate}
                          className={`flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold transition-all ${
                            isSavingDate || !newProductionDate
                              ? "bg-slate-100 text-slate-400 cursor-not-allowed"
                              : "bg-green-500 hover:bg-green-600 text-white shadow-lg shadow-green-500/20"
                          }`}
                        >
                          <Check className="w-4 h-4" />
                          {isSavingDate ? "Đang lưu..." : "Lưu"}
                        </button>
                        <button
                          onClick={handleCancelEditDate}
                          disabled={isSavingDate}
                          className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold bg-slate-100 hover:bg-slate-200 text-slate-700 transition-all"
                        >
                          <XCircle className="w-4 h-4" />
                          Hủy
                        </button>
                      </div>
                    </div>
                    <p className="text-xs text-amber-600 flex items-center gap-1">
                      <AlertTriangle className="w-3.5 h-3.5" />
                      Chỉ có thể sửa ngày khi đang ở trạng thái Ready. Ngày mới sẽ áp dụng cho các sản phẩm quét sau khi lưu.
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Production Date info when not in Ready state */}
          {status?.hasPO && !isReady && (
            <div className="bg-slate-50 rounded-2xl border border-slate-200 p-4">
              <div className="flex items-center gap-3">
                <Calendar className="w-5 h-5 text-slate-400" />
                <div>
                  <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500">
                    Ngày Sản Xuất
                  </div>
                  <div className="text-sm font-semibold text-slate-700">
                    {status.productionDate ? new Date(status.productionDate).toLocaleDateString('vi-VN', {
                      year: 'numeric', month: '2-digit', day: '2-digit'
                    }) : "—"}
                  </div>
                </div>
                {!isRunning && (
                  <span className="ml-auto text-xs text-slate-400">
                    Sửa ngày khi ở trạng thái Ready
                  </span>
                )}
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
  const config: Record<CamUiStatus, {
    iconBg: string;
    borderColor: string;
    labelColor: string;
    subColor: string;
    dotColor: string;
  }> = {
    ok: {
      iconBg: "bg-gradient-to-br from-green-400 to-emerald-500",
      borderColor: "border-green-200",
      labelColor: "text-green-700",
      subColor: "text-green-600",
      dotColor: "bg-green-500",
    },
    error: {
      iconBg: "bg-gradient-to-br from-red-400 to-rose-500",
      borderColor: "border-red-200",
      labelColor: "text-red-700",
      subColor: "text-red-600",
      dotColor: "bg-red-500",
    },
    warning: {
      iconBg: "bg-gradient-to-br from-amber-400 to-orange-500",
      borderColor: "border-amber-200",
      labelColor: "text-amber-700",
      subColor: "text-amber-600",
      dotColor: "bg-amber-500",
    },
    offline: {
      iconBg: "bg-gradient-to-br from-slate-400 to-slate-500",
      borderColor: "border-slate-200",
      labelColor: "text-slate-600",
      subColor: "text-slate-500",
      dotColor: "bg-slate-400",
    },
  };

  const c = config[status];
  const isError = status === "error";

  return (
    <div className="flex items-center gap-2">
      <div className="relative">
        <div className={`w-7 h-7 rounded-lg bg-gradient-to-br ${c.iconBg} flex items-center justify-center shadow-sm`}>
          <Icon className="w-3.5 h-3.5 text-white" strokeWidth={2} />
        </div>
        <div className={`absolute -bottom-0.5 -right-0.5 w-2 h-2 rounded-full border border-white ${c.dotColor} ${isError ? 'animate-pulse' : ''}`} />
      </div>
      <div className="min-w-0">
        <div className="flex items-center gap-1.5">
          <span className={`text-[9px] font-bold uppercase ${c.labelColor}`}>
            {label}
          </span>
          <span className={`w-1.5 h-1.5 rounded-full ${c.dotColor} ${isError ? 'animate-pulse' : ''}`} />
        </div>
        {subLabel && (
          <span className={`text-[8px] ${c.subColor} font-mono truncate block`}>
            {subLabel}
          </span>
        )}
      </div>
    </div>
  );
};

export default ProductionView;
