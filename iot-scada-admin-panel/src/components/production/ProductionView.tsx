import React, { useState, useEffect, useCallback, useRef, useMemo } from "react";
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
  Box,
  TrendingUp,
  Monitor,
  Cpu,
  Wifi,
  WifiOff,
  Edit3,
  Calendar,
  FileText,
  Building2,
  Factory,
  ChevronLeft,
  ChevronRight,
  Lock,
} from "lucide-react";
import productionApi from "../../services/productionApi";
import poApi from "../../services/poApi";
import { useCameraSocket } from "../../hooks/useCameraSocket";
import { useDevicePolling } from "../../hooks/useDevicePolling";
import { useDeviceStore } from "../../store/useDeviceStore";
import { handleActiveCodeScanned } from "../../services/cameraApi";
import type { ProductionStatusResponse, RetryRunResponse } from "../../types/production";
import type { POInfo, POListItem, POCarton } from "../../types/po";

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
  const [isRetrying, setIsRetrying] = useState(false);
  const [retryResult, setRetryResult] = useState<RetryRunResponse | null>(null);

  // PO detail state
  const [poDetail, setPoDetail] = useState<POInfo | null>(null);
  const [isLoadingPODetail, setIsLoadingPODetail] = useState(false);

  // PO card active tab (stats | detail)
  const [poCardTab, setPoCardTab] = useState<"stats" | "detail">("stats");

  // Cartons list state (for prev/current/next carton display)
  const [cartons, setCartons] = useState<POCarton[]>([]);

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
    NOTIF_ERR_RETRY: "Không thể thử lại kiểm tra mã.",
    NOTIF_ERR_RETRY_INVALID_STATE: "Chỉ có thể thử lại khi ở trạng thái InsufficientCodes.",
    // Success keys
    NOTIF_PO_LIST_RELOADED: "Đã tải lại danh sách PO ({count} mục).",
    NOTIF_PO_STARTED: "Đã bắt đầu sản xuất: {po}.",
    NOTIF_STOPPED: "Đã dừng sản xuất.",
    NOTIF_RESET: "Đã reset sản xuất.",
    NOTIF_CHANGE_PO_SUCCESS: "Đã chuyển sang chế độ chỉnh sửa. Vui lòng chọn PO và nhấn Cài Lệnh.",
    NOTIF_PO_SET: "Đã cài PO: {po}.",
    NOTIF_DATE_UPDATED: "Đã cập nhật ngày sản xuất.",
    NOTIF_RETRY_SUCCESS: "Đã kiểm tra mã - đủ để chạy sản xuất!",
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

  // Auto-select PO when backend has a running PO and PO list is loaded
  useEffect(() => {
    if (!status || poList.length === 0) return;
    if (status.hasPO && status.orderNo && selectedPO !== status.orderNo) {
      const exists = poList.some((po) => po.orderNo === status.orderNo);
      if (exists) {
        setSelectedPO(status.orderNo);
      }
    }
  }, [status, poList, selectedPO]);

  useEffect(() => {
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

  // REST polling for device status (Camera, PLC, Production state)
  useDevicePolling({ intervalMs: 2000 });

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

  // ────── Retry Production (InsufficientCodes) ──────
  const handleRetryProduction = async () => {
    if (!isInsufficientCodes) {
      setError("NOTIF_ERR_RETRY_INVALID_STATE");
      return;
    }
    setIsRetrying(true);
    setError(null);
    setSuccess(null);
    setRetryResult(null);
    try {
      const result = await productionApi.retryProduction();
      setRetryResult(result);
      if (result.success) {
        setSuccess("NOTIF_RETRY_SUCCESS");
        await fetchStatus();
      } else {
        setError(`Không đủ mã! Còn thiếu: ${result.neededCodes} mã`);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "NOTIF_ERR_RETRY";
      setError(msg);
    } finally {
      setIsRetrying(false);
    }
  };

  // ────── PO Detail ──────

  const fetchPODetail = useCallback(async (orderNo: string) => {
    setIsLoadingPODetail(true);
    try {
      const data = await poApi.getPO(orderNo);
      setPoDetail(data);
    } catch {
      setPoDetail(null);
    } finally {
      setIsLoadingPODetail(false);
    }
  }, []);

  // Reset detail + reload when current PO changes
  useEffect(() => {
    if (!status?.orderNo) {
      setPoDetail(null);
      return;
    }
    if (poDetail?.orderNo !== status.orderNo) {
      fetchPODetail(status.orderNo);
    }
  }, [status?.orderNo, poDetail?.orderNo, fetchPODetail]);

  // ────── Cartons list ──────
  const fetchCartons = useCallback(async (orderNo: string) => {
    try {
      const data = await poApi.getCartons(orderNo);
      setCartons(data || []);
    } catch {
      setCartons([]);
    }
  }, []);

  // Reset cartons when PO changes
  useEffect(() => {
    if (!status?.orderNo) {
      setCartons([]);
      return;
    }
    fetchCartons(status.orderNo);
  }, [status?.orderNo, fetchCartons]);

  // Derive prev / current / next cartons from the list and status
  const derivedCartons = useMemo(() => {
    const currentId = status?.currentCartonId ?? 0;
    const sorted = [...cartons].sort((a, b) => a.id - b.id);
    // Current: match by id; otherwise fallback to status data
    const current =
      sorted.find((c) => c.id === currentId && currentId > 0) ||
      (currentId > 0
        ? {
            id: currentId,
            cartonCode: status?.currentCartonCode || "",
            status: "Open",
          }
        : null);

    // Prev: highest id that is "Closed" AND < currentId
    const prevClosed =
      sorted
        .filter((c) => c.status === "Closed" && (!current || c.id < current.id))
        .sort((a, b) => b.id - a.id)[0] || null;
    // Fallback: only synthesize prev if current.id > 1 (otherwise there is no previous carton)
    const prev =
      prevClosed ||
      (current && current.id > 1
        ? {
            id: current.id - 1,
            cartonCode: "",
            status: "Prev",
          }
        : null);

    // Next: lowest id that is "Empty" AND > currentId
    const nextEmpty =
      sorted
        .filter((c) => c.status === "Empty" && (!current || c.id > current.id))
        .sort((a, b) => a.id - b.id)[0] || null;
    // Fallback: if current exists but no empty carton in list, synthesize current + 1
    const next =
      nextEmpty ||
      (current
        ? {
            id: current.id + 1,
            cartonCode: "",
            status: "Next",
          }
        : nextEmpty);
    return { prev, current, next };
  }, [cartons, status?.currentCartonId, status?.currentCartonCode]);

  const formatDetailDate = (dateStr?: string) => {
    if (!dateStr || dateStr === "0" || dateStr === "null") return "—";
    try {
      return new Date(dateStr).toLocaleString("vi-VN");
    } catch {
      return dateStr;
    }
  };

  const isRunning = status?.state === "Running";
  const isEditing = status?.state === "Editing";
  const isReady = status?.state === "Ready";
  const isIdle = status?.state === "NoSelectedPO";
  const isInsufficientCodes = status?.state === "InsufficientCodes";
  const canStart = !isRunning && status?.hasPO === true && !!selectedPO && !isInsufficientCodes;
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
    InsufficientCodes: { label: "KHÔNG ĐỦ MÃ", bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500 animate-pulse", icon: "" },
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

                {/* Cài Lệnh - cài đặt PO từ Editing */}
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

                {/* Retry - khi InsufficientCodes */}
                {isInsufficientCodes && (
                  <button
                    onClick={handleRetryProduction}
                    disabled={isRetrying}
                    className="flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all bg-amber-500 hover:bg-amber-600 text-white shadow-lg shadow-amber-500/20"
                  >
                    {isRetrying ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : (
                      <RefreshCw className="w-4 h-4" />
                    )}
                    {isRetrying ? "Đang kiểm tra..." : "Thử lại"}
                  </button>
                )}
              </div>

              {/* Info text */}
              <div className="text-xs text-slate-400 flex items-center gap-2">
                <AlertCircle className="w-3.5 h-3.5" />
                {isInsufficientCodes 
                  ? `Không đủ mã! Pool còn ${retryResult?.availableCodes || 0} mã, cần ${retryResult?.orderQty || status?.orderQty || 0} mã. Vui lòng thêm mã vào pool rồi nhấn "Thử lại".` 
                  : isEditing 
                    ? "Chọn PO → Nhấn Cài Lệnh để thiết lập. Start để bắt đầu sản xuất." 
                    : "Chọn PO → Start để bắt đầu. Đổi PO để thay đổi lệnh sản xuất."}
              </div>

              {/* InsufficientCodes Warning Banner */}
              {isInsufficientCodes && (
                <div className="flex items-center gap-3 p-4 rounded-xl bg-red-50 border border-red-200">
                  <AlertTriangle className="w-6 h-6 text-red-600 shrink-0" />
                  <div className="flex-1">
                    <div className="text-sm font-bold text-red-700">
                      Không đủ mã để chạy sản xuất
                    </div>
                    <div className="text-xs text-red-600 mt-0.5">
                      Pool còn: <span className="font-semibold">{retryResult?.availableCodes || 0}</span> mã |
                      Cần: <span className="font-semibold">{retryResult?.orderQty || status?.orderQty || 0}</span> mã |
                      Còn thiếu: <span className="font-semibold">{retryResult?.neededCodes || Math.max(0, (retryResult?.orderQty || status?.orderQty || 0) - (retryResult?.availableCodes || 0))}</span> mã
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* PO Info Card (tabs: Thông tin PO | Chi tiết PO) */}
          {status?.hasPO && (
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                <div className="flex items-center justify-between gap-2 flex-wrap">
                  <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                    <TrendingUp className="w-4 h-4 text-blue-600" /> Thông tin PO
                    <span className="ml-1 font-mono text-blue-700 normal-case tracking-normal text-xs">
                      ({status.orderNo})
                    </span>
                  </h2>
                  <button
                    onClick={() => {
                      if (poCardTab === "stats") {
                        fetchStatus();
                      } else if (status?.orderNo) {
                        fetchPODetail(status.orderNo);
                      }
                    }}
                    disabled={poCardTab === "stats" ? isRefreshingPO : isLoadingPODetail}
                    className={`p-1.5 rounded-lg transition-all ${
                      (poCardTab === "stats" ? isRefreshingPO : isLoadingPODetail)
                        ? 'bg-blue-100 text-blue-500 cursor-not-allowed'
                        : 'bg-slate-100 hover:bg-slate-200 text-slate-600 hover:text-slate-800'
                    }`}
                    title={poCardTab === "stats" ? "Tải lại thông tin" : "Tải lại chi tiết PO"}
                  >
                    <RefreshCw className={`w-4 h-4 ${(poCardTab === "stats" ? isRefreshingPO : isLoadingPODetail) ? 'animate-spin' : ''}`} />
                  </button>
                </div>
                {/* Tab bar */}
                <div className="flex items-center gap-1 mt-2 w-fit">
                  <button
                    onClick={() => setPoCardTab("stats")}
                    className={`flex items-center gap-1.5 px-3 py-1 text-[11px] font-bold rounded-md transition-all ${
                      poCardTab === "stats"
                        ? "bg-blue-50 text-blue-700 shadow-sm"
                        : "text-slate-500 hover:text-slate-800 hover:bg-slate-50"
                    }`}
                  >
                    <TrendingUp className="w-3.5 h-3.5" /> Thông tin PO
                  </button>
                  <button
                    onClick={() => setPoCardTab("detail")}
                    className={`flex items-center gap-1.5 px-3 py-1 text-[11px] font-bold rounded-md transition-all ${
                      poCardTab === "detail"
                        ? "bg-blue-50 text-blue-700 shadow-sm"
                        : "text-slate-500 hover:text-slate-800 hover:bg-slate-50"
                    }`}
                  >
                    <FileText className="w-3.5 h-3.5" /> Chi tiết PO
                  </button>
                </div>
              </div>
              <div className="p-4 xl:p-6">
                {poCardTab === "stats" ? (
                  <>
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
                  </>
                ) : isLoadingPODetail && !poDetail ? (
                  <div className="flex items-center gap-3 text-slate-500 py-6 justify-center">
                    <RefreshCw className="w-5 h-5 animate-spin" />
                    <span className="text-sm font-medium">Đang tải chi tiết PO...</span>
                  </div>
                ) : poDetail ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-x-6">
                    <DetailRow label="Mã đơn hàng (Order No.)" value={poDetail.orderNo} mono />
                    <DetailRow label="Số lượng đặt (Order Qty)" value={poDetail.orderQty?.toLocaleString()} />
                    <DetailRow label="Quy cách thùng (Carton Capacity)" value={poDetail.cartonCapacity?.toLocaleString()} icon={Box} />
                    <DetailRow label="GTIN" value={poDetail.gtin} mono />
                    <DetailRow label="Tên sản phẩm (Product Name)" value={poDetail.productName} />
                    <DetailRow label="Mã sản phẩm (Product Code)" value={poDetail.productCode} />
                    <DetailRow label="Số lô (Lot Number)" value={poDetail.lotNumber} />
                    <DetailRow label="Site" value={poDetail.site} icon={Building2} />
                    <DetailRow label="Nhà máy (Factory)" value={poDetail.factory} icon={Factory} />
                    <DetailRow label="Dây chuyền (Production Line)" value={poDetail.productionLine} />
                    <DetailRow label="Ca (Shift)" value={poDetail.shift} />
                    <DetailRow label="Ngày sản xuất (Production Date)" value={poDetail.productionDate} icon={Calendar} />
                    <DetailRow label="Mã đơn hàng KH (Customer Order No.)" value={poDetail.customerOrderNo} />
                    <DetailRow label="Đơn vị (UOM)" value={poDetail.uom} />
                    <DetailRow label="Thời gian tạo (Created Time)" value={formatDetailDate(poDetail.createdTime)} />
                    <DetailRow label="Thời gian sửa (Modified Time)" value={formatDetailDate(poDetail.modifiedTime)} />
                  </div>
                ) : (
                  <div className="text-center text-slate-400 py-6">
                    <FileText className="w-8 h-8 mx-auto mb-2 opacity-30" />
                    <span className="text-sm">Không tải được chi tiết PO</span>
                  </div>
                )}
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
                <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-2">
                  Thùng trước / hiện tại / sắp
                </div>
                <div className="grid grid-cols-3 gap-2">
                  {/* Prev */}
                  <div className={`rounded-lg border p-2 flex flex-col gap-1 ${
                    derivedCartons.prev
                      ? derivedCartons.prev.status === "Closed"
                        ? "border-slate-200 bg-white"
                        : "border-dashed border-slate-300 bg-slate-50/50"
                      : "border-dashed border-slate-200 bg-slate-50/50"
                  }`}>
                    <div className="flex items-center gap-1 text-[9px] font-bold uppercase tracking-wider text-slate-500">
                      <ChevronLeft className="w-3 h-3" /> Trước
                    </div>
                    {derivedCartons.prev ? (
                      <>
                        <div className={`text-sm font-black font-mono ${
                          derivedCartons.prev.status === "Closed" ? "text-slate-700" : "text-slate-400"
                        }`}>
                          #{derivedCartons.prev.id}
                        </div>
                        <div className={`text-[10px] font-mono truncate ${
                          derivedCartons.prev.status === "Closed" ? "text-slate-500" : "text-slate-400 italic"
                        }`}>
                          {derivedCartons.prev.cartonCode && derivedCartons.prev.cartonCode !== "0"
                            ? derivedCartons.prev.cartonCode
                            : "—"}
                        </div>
                        <div className={`flex items-center gap-1 text-[9px] font-bold ${
                          derivedCartons.prev.status === "Closed" ? "text-green-700" : "text-slate-400"
                        }`}>
                          <Lock className="w-2.5 h-2.5" />
                          {derivedCartons.prev.status === "Closed" ? "Đã đóng" : "Sẽ tạo"}
                        </div>
                      </>
                    ) : (
                      <div className="text-[10px] text-slate-400 italic">Chưa có</div>
                    )}
                  </div>

                  {/* Current (highlighted) */}
                  <div className={`rounded-lg border-2 p-2 flex flex-col gap-1 ${
                    derivedCartons.current
                      ? "border-blue-500 bg-blue-50 shadow-sm shadow-blue-500/10"
                      : "border-dashed border-slate-300 bg-slate-50/50"
                  }`}>
                    <div className="flex items-center gap-1 text-[9px] font-bold uppercase tracking-wider text-blue-700">
                      <Box className="w-3 h-3" /> Hiện tại
                    </div>
                    {derivedCartons.current ? (
                      <>
                        <div className="text-base font-black text-blue-700 font-mono">
                          #{derivedCartons.current.id}
                        </div>
                        <div className="text-[10px] font-mono text-slate-700 truncate">
                          {derivedCartons.current.cartonCode && derivedCartons.current.cartonCode !== "0"
                            ? derivedCartons.current.cartonCode
                            : "—"}
                        </div>
                        <div className="text-[9px] font-bold text-blue-700">
                          {status?.itemsInCarton || 0}/{status?.cartonCapacity || 24}
                        </div>
                      </>
                    ) : (
                      <div className="text-[10px] text-slate-400 italic">—</div>
                    )}
                  </div>

                  {/* Next */}
                  <div className={`rounded-lg border p-2 flex flex-col gap-1 ${
                    derivedCartons.next
                      ? derivedCartons.next.status === "Empty"
                        ? "border-slate-200 bg-white"
                        : "border-dashed border-slate-300 bg-slate-50/50"
                      : "border-dashed border-slate-200 bg-slate-50/50"
                  }`}>
                    <div className="flex items-center gap-1 text-[9px] font-bold uppercase tracking-wider text-slate-500 justify-end">
                      Sắp <ChevronRight className="w-3 h-3" />
                    </div>
                    {derivedCartons.next ? (
                      <>
                        <div className={`text-sm font-black font-mono ${
                          derivedCartons.next.status === "Empty" ? "text-slate-700" : "text-slate-400"
                        }`}>
                          #{derivedCartons.next.id}
                        </div>
                        <div className={`text-[10px] font-mono truncate ${
                          derivedCartons.next.status === "Empty" ? "text-slate-500" : "text-slate-400 italic"
                        }`}>
                          {derivedCartons.next.cartonCode && derivedCartons.next.cartonCode !== "0"
                            ? derivedCartons.next.cartonCode
                            : "—"}
                        </div>
                        <div className={`text-[9px] font-bold ${
                          derivedCartons.next.status === "Empty" ? "text-slate-500" : "text-slate-400"
                        }`}>
                          {derivedCartons.next.status === "Empty" ? "Trống" : "Sẽ tạo"}
                        </div>
                      </>
                    ) : (
                      <div className="text-[10px] text-slate-400 italic">Chưa có</div>
                    )}
                  </div>
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
                <Monitor className={`w-4 h-4 ${
                  (cameraStatus.connected || plcStatus.connected)
                    ? "text-green-600"
                    : "text-red-600"
                }`} />
                Trạng thái Camera
                <span className="ml-auto flex items-center gap-1.5">
                  {cameraStatus.connected ? (
                    <Wifi className="w-3.5 h-3.5 text-green-600" />
                  ) : (
                    <WifiOff className="w-3.5 h-3.5 text-red-600 animate-pulse" />
                  )}
                  {plcStatus.connected ? (
                    <Cpu className="w-3.5 h-3.5 text-green-600" />
                  ) : (
                    <Cpu className="w-3.5 h-3.5 text-red-600 animate-pulse" />
                  )}
                </span>
              </h2>
            </div>
            <div className="p-3 xl:p-4 flex flex-col gap-2.5">
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
              {/* WS banner chips */}
              <div className="flex items-center gap-1.5 mt-0.5">
                <div
                  className={`flex-1 flex items-center gap-1.5 px-2 py-1.5 rounded-lg border ${
                    cameraStatus.connected
                      ? "bg-green-50 border-green-200"
                      : "bg-red-50 border-red-200"
                  }`}
                >
                  <Wifi
                    className={`w-3 h-3 ${
                      cameraStatus.connected ? "text-green-600" : "text-red-600"
                    } ${cameraStatus.connected ? "" : "animate-pulse"}`}
                  />
                  <span
                    className={`text-[9px] font-bold uppercase tracking-wider ${
                      cameraStatus.connected ? "text-green-700" : "text-red-700"
                    }`}
                  >
                    CAM WS
                  </span>
                  <span
                    className={`ml-auto text-[9px] font-black ${
                      cameraStatus.connected ? "text-green-600" : "text-red-600"
                    }`}
                  >
                    {cameraStatus.connected ? "ONLINE" : "OFFLINE"}
                  </span>
                </div>
                <div
                  className={`flex-1 flex items-center gap-1.5 px-2 py-1.5 rounded-lg border ${
                    plcStatus.connected
                      ? "bg-green-50 border-green-200"
                      : "bg-red-50 border-red-200"
                  }`}
                >
                  <Cpu
                    className={`w-3 h-3 ${
                      plcStatus.connected ? "text-green-600" : "text-red-600"
                    } ${plcStatus.connected ? "" : "animate-pulse"}`}
                  />
                  <span
                    className={`text-[9px] font-bold uppercase tracking-wider ${
                      plcStatus.connected ? "text-green-700" : "text-red-700"
                    }`}
                  >
                    PLC WS
                  </span>
                  <span
                    className={`ml-auto text-[9px] font-black ${
                      plcStatus.connected ? "text-green-600" : "text-red-600"
                    }`}
                  >
                    {plcStatus.connected ? "ONLINE" : "OFFLINE"}
                  </span>
                </div>
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

interface DetailRowProps {
  label: string;
  value?: string | number;
  mono?: boolean;
  icon?: React.ElementType;
}

const DetailRow: React.FC<DetailRowProps> = ({ label, value, mono, icon: Icon }) => (
  <div className="flex items-start justify-between py-2.5 border-b border-slate-100 last:border-0 gap-3">
    <span className="text-xs font-bold uppercase tracking-wider text-slate-500 flex items-center gap-1.5 shrink-0">
      {Icon && <Icon className="w-3.5 h-3.5" />} {label}
    </span>
    <span className={`text-sm font-semibold text-slate-800 text-right break-words ${mono ? "font-mono" : ""}`}>
      {value || "—"}
    </span>
  </div>
);

const MiniDeviceIndicator: React.FC<{
  icon: React.ElementType;
  label: string;
  subLabel?: string;
  status: CamUiStatus;
}> = ({ icon: Icon, label, subLabel, status }) => {
  const isOk = status === "ok";
  const isWarn = status === "warning";
  const isBad = status === "error" || status === "offline";
  const containerCls = isOk
    ? "bg-green-50 border-green-300"
    : isWarn
      ? "bg-amber-50 border-amber-300"
      : "bg-red-50 border-red-300";

  const iconBgCls = isOk
    ? "bg-gradient-to-br from-green-500 to-emerald-600 shadow-md shadow-green-500/30"
    : isWarn
      ? "bg-gradient-to-br from-amber-400 to-orange-500 shadow-md shadow-amber-500/30"
      : "bg-gradient-to-br from-red-500 to-rose-600 shadow-md shadow-red-500/30";

  const labelCls = isOk ? "text-green-800" : isWarn ? "text-amber-800" : "text-red-800";
  const subCls = isOk ? "text-green-700" : isWarn ? "text-amber-700" : "text-red-700";
  const badgeCls = isOk
    ? "bg-green-100 text-green-700 border-green-300"
    : isWarn
      ? "bg-amber-100 text-amber-700 border-amber-300"
      : "bg-red-100 text-red-700 border-red-300";
  const dotCls = isOk ? "bg-green-500" : isWarn ? "bg-amber-500" : "bg-red-500";
  const statusText = isOk ? "ONLINE" : isWarn ? "CẢNH BÁO" : "OFFLINE";

  return (
    <div
      className={`rounded-xl border p-2.5 flex items-center gap-2.5 transition-all duration-300 ${containerCls}`}
    >
      <div className="relative shrink-0">
        <div
          className={`w-9 h-9 rounded-lg flex items-center justify-center ${iconBgCls}`}
        >
          <Icon className="w-4 h-4 text-white" strokeWidth={2.5} />
        </div>
        <div className="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-white flex items-center justify-center">
          <span
            className={`w-1.5 h-1.5 rounded-full ${dotCls} ${!isOk ? "animate-pulse" : ""}`}
          />
        </div>
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-1.5">
          <span className={`text-[10px] font-black uppercase tracking-wider ${labelCls}`}>
            {label}
          </span>
          <span
            className={`text-[8px] font-bold px-1.5 py-0.5 rounded border ${badgeCls}`}
          >
            {statusText}
          </span>
        </div>
        {subLabel && (
          <span className={`text-[10px] ${subCls} font-mono truncate block mt-0.5`}>
            {subLabel}
          </span>
        )}
      </div>
    </div>
  );
};

export default ProductionView;
