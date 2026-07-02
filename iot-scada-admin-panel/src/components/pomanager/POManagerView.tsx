import React, { useState, useEffect, useCallback } from "react";
import {
  Package,
  Plus,
  Search,
  RefreshCw,
  X,
  Check,
  AlertCircle,
  List,
  FileText,
  ArrowLeft,
  Calendar,
  Hash,
  Tag,
  Building2,
  Factory,
  Layers,
  User,
  Database,
  Trash2,
  AlertTriangle,
  Play,
  Box,
  Code,
  CheckCircle,
  PackageCheck,
  Clock,
} from "lucide-react";
import poApi from "../../services/poApi";
import type {
  POInfo,
  POListItem,
  CreatePORequest,
  POCode,
  POCarton,
  PODatabaseStatus,
} from "../../types/po";

type TabKey = "list" | "create" | "detail";
type DetailTabKey = "info" | "codes" | "cartons" | "database";

interface POManagerViewProps {
  onNavigate?: (route: string) => void;
}

const initialCreateForm: CreatePORequest = {
  orderNo: "",
  site: "SITE_MASAN",
  factory: "FACTORY_01",
  productionLine: "LINE_1",
  productionDate: new Date().toISOString().split("T")[0],
  shift: "A",
  orderQty: 100,
  lotNumber: "",
  productCode: "",
  productName: "",
  gtin: "",
  customerOrderNo: "",
  uom: "PCS",
  userName: "Frontend",
  autoLoadCodes: true,
  cartonCapacity: 24,
};

const POManagerView: React.FC<POManagerViewProps> = () => {
  const [activeTab, setActiveTab] = useState<TabKey>("list");
  const [detailTab, setDetailTab] = useState<DetailTabKey>("info");
  const [poList, setPoList] = useState<POListItem[]>([]);
  const [filteredList, setFilteredList] = useState<POListItem[]>([]);
  const [selectedPO, setSelectedPO] = useState<POInfo | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [formData, setFormData] = useState<CreatePORequest>(initialCreateForm);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [healthStatus, setHealthStatus] = useState<{
    ok: boolean;
    state?: string;
  } | null>(null);

  // Codes for PO detail
  const [poCodes, setPoCodes] = useState<POCode[]>([]);
  const [isLoadingCodes, setIsLoadingCodes] = useState(false);
  const [codesFilter, setCodesFilter] = useState<"all" | "unused" | "active" | "packed">("all");
  const [codesPage, setCodesPage] = useState(1);
  const [totalCodes, setTotalCodes] = useState(0);
  const CODES_PAGE_SIZE = 100;

  // Cartons for PO detail
  const [poCartons, setPoCartons] = useState<POCarton[]>([]);
  const [isLoadingCartons, setIsLoadingCartons] = useState(false);

  // Database status
  const [dbStatus, setDbStatus] = useState<PODatabaseStatus | null>(null);
  const [isLoadingDbStatus, setIsLoadingDbStatus] = useState(false);
  const [isEnsuringReady, setIsEnsuringReady] = useState(false);

  // Delete PO state
  const [deleteTarget, setDeleteTarget] = useState<POListItem | null>(null);
  const [deleteCheck, setDeleteCheck] = useState<{ canDelete: boolean; reason: string } | null>(null);
  const [isCheckingDelete, setIsCheckingDelete] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Toast auto-dismiss
  useEffect(() => {
    if (success) {
      const t = setTimeout(() => setSuccess(null), 4000);
      return () => clearTimeout(t);
    }
  }, [success]);

  useEffect(() => {
    if (error) {
      const t = setTimeout(() => setError(null), 5000);
      return () => clearTimeout(t);
    }
  }, [error]);

  const checkHealth = useCallback(async () => {
    try {
      const data = await poApi.checkHealth();
      setHealthStatus({ ok: data.status === "OK", state: data.appState });
    } catch {
      setHealthStatus({ ok: false });
    }
  }, []);

  const fetchPOList = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const list = await poApi.getAllPOs();
      setPoList(list);
      setFilteredList(list);
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to load PO list";
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    checkHealth();
    fetchPOList();
  }, [checkHealth, fetchPOList]);

  // Filter list when searchTerm changes
  useEffect(() => {
    if (!searchTerm.trim()) {
      setFilteredList(poList);
      return;
    }
    const term = searchTerm.toLowerCase();
    setFilteredList(
      poList.filter(
        (po) =>
          po.orderNo.toLowerCase().includes(term) ||
          (po.productName || "").toLowerCase().includes(term) ||
          (po.gtin || "").toLowerCase().includes(term),
      ),
    );
  }, [searchTerm, poList]);

  const fetchPODetail = async (orderNo: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await poApi.getPO(orderNo);
      setSelectedPO(data);
      setActiveTab("detail");
      setDetailTab("info");
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to load PO detail";
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchPOCodes = async (orderNo: string, page = 1) => {
    setIsLoadingCodes(true);
    const offset = (page - 1) * CODES_PAGE_SIZE;
    // Chuyển filter sang status number
    let statusNum: number | undefined;
    if (codesFilter === "unused") statusNum = 0;
    else if (codesFilter === "active") statusNum = 1;
    // packed cần filter theo cartonCode, sẽ load all và filter ở frontend
    
    try {
      const result = await poApi.getCodes(orderNo, statusNum, undefined, CODES_PAGE_SIZE, offset);
      setPoCodes(result.data);
      setTotalCodes(result.total);
      setCodesPage(page);
    } catch (err) {
      setPoCodes([]);
      setTotalCodes(0);
    } finally {
      setIsLoadingCodes(false);
    }
  };

  const fetchPOCartons = async (orderNo: string) => {
    setIsLoadingCartons(true);
    try {
      const cartons = await poApi.getCartons(orderNo);
      setPoCartons(cartons);
    } catch (err) {
      setPoCartons([]);
    } finally {
      setIsLoadingCartons(false);
    }
  };

  const fetchPODatabaseStatus = async (orderNo: string) => {
    setIsLoadingDbStatus(true);
    try {
      const status = await poApi.getPOStatus(orderNo);
      setDbStatus(status);
    } catch {
      setDbStatus(null);
    } finally {
      setIsLoadingDbStatus(false);
    }
  };

  const handleEnsureDatabaseReady = async () => {
    if (!selectedPO) return;
    setIsEnsuringReady(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await poApi.ensurePODatabaseReady(
        selectedPO.orderNo,
        true,
        selectedPO.orderQty > 0 ? Math.ceil(selectedPO.orderQty / 24) : 5,
      );
      if (result.success) {
        setSuccess(result.message || "Database ready!");
      } else {
        setError(result.message || "Failed to setup database");
      }
      await fetchPODatabaseStatus(selectedPO.orderNo);
      await fetchPOList();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to setup database");
    } finally {
      setIsEnsuringReady(false);
    }
  };

  const handleDetailTabChange = (tab: DetailTabKey) => {
    setDetailTab(tab);
    if (selectedPO) {
      if (tab === "codes") fetchPOCodes(selectedPO.orderNo, 1);
      if (tab === "cartons") fetchPOCartons(selectedPO.orderNo);
      if (tab === "database") fetchPODatabaseStatus(selectedPO.orderNo);
    }
  };

  const handleCodesPageChange = (page: number) => {
    if (selectedPO) {
      fetchPOCodes(selectedPO.orderNo, page);
    }
  };

  const handleDeleteClick = async (po: POListItem) => {
    setDeleteTarget(po);
    setDeleteCheck(null);
    setIsCheckingDelete(true);
    setIsDeleting(false);
    try {
      const result = await poApi.canDeletePO(po.orderNo);
      setDeleteCheck({ canDelete: result.canDelete, reason: result.reason });
    } catch {
      setDeleteCheck({ canDelete: false, reason: "Cannot check PO status" });
    } finally {
      setIsCheckingDelete(false);
    }
  };

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await poApi.deletePO(deleteTarget.orderNo, "Frontend");
      if (result.success) {
        setSuccess(`Deleted PO "${deleteTarget.orderNo}" successfully.`);
        setDeleteTarget(null);
        await fetchPOList();
      } else {
        setError(result.message || "Failed to delete PO.");
        setDeleteCheck({ canDelete: false, reason: result.reason });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete PO.");
    } finally {
      setIsDeleting(false);
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};
    if (!formData.orderNo.trim()) {
      errors.orderNo = "Order No. is required";
    }
    if (!formData.orderQty || formData.orderQty <= 24) {
      errors.orderQty = "Order Qty must be greater than 24";
    }
    if (!formData.productionDate) {
      errors.productionDate = "Production Date is required";
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleCreatePO = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;

    setIsSaving(true);
    setError(null);
    setSuccess(null);
    try {
      const result = await poApi.createPO(formData);
      if (result.success) {
        setSuccess(
          `Created PO "${result.orderNo}" successfully${
            result.loadedCodesCount
              ? ` - Loaded ${result.loadedCodesCount} codes`
              : ""
          }${result.createdCartonsCount ? ` - ${result.createdCartonsCount} cartons` : ""}.`,
        );
        setFormData({ ...initialCreateForm });
        setActiveTab("list");
        await fetchPOList();
      } else {
        setError(result.message || "Failed to create PO");
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to create PO";
      setError(msg);
    } finally {
      setIsSaving(false);
    }
  };

  const handleFormChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value, type } = e.target;
    const newValue =
      type === "number" ? (value === "" ? 0 : Number(value)) : value;
    setFormData((prev) => ({ ...prev, [name]: newValue }));
    if (formErrors[name]) {
      setFormErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr || dateStr === "0") return "—";
    try {
      return new Date(dateStr).toLocaleString("vi-VN");
    } catch {
      return dateStr;
    }
  };

  const handleCodesFilterChange = (filter: "all" | "unused" | "active" | "packed") => {
    setCodesFilter(filter);
    if (selectedPO) {
      fetchPOCodes(selectedPO.orderNo, 1);
    }
  };

  const getFilteredCodes = () => {
    // Filter đã xử lý ở backend, trả về tất cả codes của trang hiện tại
    return poCodes;
  };

  const getCodeStatusLabel = (code: POCode) => {
    if (code.cartonCode && code.cartonCode !== "0") return { label: "Đã đóng gói (Packed)", class: "bg-purple-100 text-purple-700" };
    if (code.status === 1) return { label: "Đã kích hoạt (Active)", class: "bg-blue-100 text-blue-700" };
    return { label: "Chưa dùng (Unused)", class: "bg-green-100 text-green-700" };
  };

  const getCartonStatusLabel = (carton: POCarton) => {
    if (carton.completedDatetime && carton.completedDatetime !== "0") return { label: "Đã đóng (Closed)", class: "bg-slate-100 text-slate-700" };
    if (carton.startDatetime && carton.startDatetime !== "0") return { label: "Đang mở (Open)", class: "bg-amber-100 text-amber-700" };
    return { label: "Trống (Empty)", class: "bg-slate-50 text-slate-400" };
  };

  // ─────────────────────────────────────────────────────────────
  // Render: Header with status + tabs
  // ─────────────────────────────────────────────────────────────
  return (
    <div className="flex flex-col gap-4 h-full min-h-0 w-full animate-in fade-in duration-500 overflow-auto scrollbar-hide pb-6">
      {/* Header */}
      <div className="flex items-center justify-between shrink-0 px-1 flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center shadow-lg shadow-blue-600/20">
            <Package className="w-5 h-5 text-white" />
          </div>
          <div>
            <h1 className="text-xl 2xl:text-2xl font-bold text-slate-800 tracking-tight">
              Production Order Manager
            </h1>
            <p className="text-xs text-slate-500 mt-0.5">
              Create and manage production orders
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <div
            className={`flex items-center gap-2 px-3 py-1.5 rounded-xl border text-xs font-semibold ${
              healthStatus?.ok
                ? "bg-green-50 border-green-200 text-green-700"
                : "bg-red-50 border-red-200 text-red-700"
            }`}
          >
            <div
              className={`w-2 h-2 rounded-full ${
                healthStatus?.ok
                  ? "bg-green-500 shadow-[0_0_6px_rgba(34,197,94,0.6)]"
                  : "bg-red-500 animate-pulse"
              }`}
            />
            {healthStatus?.ok
              ? `API: ${healthStatus.state || "Online"}`
              : "API: Offline"}
          </div>
          <button
            onClick={() => {
              checkHealth();
              fetchPOList();
            }}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-xl border border-blue-100 transition-colors"
          >
            <RefreshCw
              className={`w-3.5 h-3.5 ${isLoading ? "animate-spin" : ""}`}
            />
            Refresh
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
          <span className="text-sm font-semibold flex-1">
            {error || success}
          </span>
          <button
            onClick={() => {
              setError(null);
              setSuccess(null);
            }}
            className="p-1 hover:bg-white/50 rounded-lg transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Delete Confirm Modal */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-white rounded-3xl shadow-2xl border border-slate-200/60 w-full max-w-md mx-4 animate-in zoom-in-95 duration-200">
            <div className="flex items-center gap-3 px-6 py-4 border-b border-slate-100">
              <div className="w-10 h-10 rounded-xl bg-red-100 flex items-center justify-center shrink-0">
                <AlertTriangle className="w-5 h-5 text-red-600" />
              </div>
              <div>
                <h3 className="text-base font-bold text-slate-800">Delete PO</h3>
                <p className="text-xs text-slate-500 font-medium">{deleteTarget.orderNo}</p>
              </div>
              <button
                onClick={() => { setDeleteTarget(null); setDeleteCheck(null); }}
                className="ml-auto p-2 hover:bg-slate-100 rounded-xl transition-colors"
              >
                <X className="w-4 h-4 text-slate-400" />
              </button>
            </div>

            <div className="p-6">
              {isCheckingDelete ? (
                <div className="flex items-center gap-3 text-slate-600">
                  <RefreshCw className="w-5 h-5 animate-spin" />
                  <span className="text-sm font-medium">Checking PO status...</span>
                </div>
              ) : deleteCheck ? (
                <div className="space-y-3">
                  <div className={`flex items-start gap-3 px-4 py-3 rounded-2xl border ${
                    deleteCheck.canDelete
                      ? "bg-green-50 border-green-200 text-green-800"
                      : "bg-amber-50 border-amber-200 text-amber-800"
                  }`}>
                    <AlertCircle className="w-5 h-5 shrink-0 mt-0.5" />
                    <div>
                      <p className="text-sm font-semibold">
                        {deleteCheck.canDelete ? "Can be deleted" : "Cannot be deleted"}
                      </p>
                      <p className="text-xs mt-0.5 opacity-80">{deleteCheck.reason}</p>
                    </div>
                  </div>

                  {!deleteCheck.canDelete && (
                    <p className="text-xs text-slate-500 text-center">
                      This PO has used codes and cannot be deleted.
                    </p>
                  )}
                </div>
              ) : null}
            </div>

            <div className="flex items-center gap-3 px-6 py-4 border-t border-slate-100 bg-slate-50/50 rounded-b-3xl">
              <button
                onClick={() => { setDeleteTarget(null); setDeleteCheck(null); }}
                className="flex-1 px-4 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmDelete}
                disabled={isDeleting || isCheckingDelete || (deleteCheck && !deleteCheck.canDelete)}
                className={`flex-1 flex items-center justify-center gap-2 px-4 py-2.5 text-sm font-bold rounded-xl transition-colors ${
                  isDeleting || isCheckingDelete || (deleteCheck && !deleteCheck.canDelete)
                    ? "bg-slate-200 text-slate-400 cursor-not-allowed"
                    : "bg-red-600 hover:bg-red-700 text-white"
                }`}
              >
                {isDeleting ? (
                  <>
                    <RefreshCw className="w-4 h-4 animate-spin" />
                    Deleting...
                  </>
                ) : (
                  <>
                    <Trash2 className="w-4 h-4" />
                    Delete PO
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="flex items-center gap-1.5 bg-slate-100/60 p-1.5 rounded-2xl shrink-0 w-full sm:w-fit">
        <button
          onClick={() => setActiveTab("list")}
          className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${
            activeTab === "list"
              ? "bg-white text-blue-700 shadow-sm"
              : "text-slate-600 hover:text-slate-900"
          }`}
        >
          <List className="w-4 h-4" /> PO List
        </button>
        <button
          onClick={() => {
            setFormData({ ...initialCreateForm });
            setFormErrors({});
            setActiveTab("create");
          }}
          className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${
            activeTab === "create"
              ? "bg-white text-blue-700 shadow-sm"
              : "text-slate-600 hover:text-slate-900"
          }`}
        >
          <Plus className="w-4 h-4" /> New PO
        </button>
        {selectedPO && (
          <button
            onClick={() => setActiveTab("detail")}
            className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${
              activeTab === "detail"
                ? "bg-white text-blue-700 shadow-sm"
                : "text-slate-600 hover:text-slate-900"
            }`}
          >
            <FileText className="w-4 h-4" /> Detail
          </button>
        )}
      </div>

      {/* Tab Content: LIST */}
      {activeTab === "list" && (
        <div className="flex flex-col gap-4 animate-in fade-in duration-300">
          {/* Search */}
          <div className="flex items-center gap-2 bg-white rounded-2xl border border-slate-200 px-3 py-2 shadow-sm">
            <Search className="w-4 h-4 text-slate-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Search by order no., product name, or GTIN..."
              className="flex-1 bg-transparent outline-none text-sm text-slate-700 placeholder:text-slate-400"
            />
            {searchTerm && (
              <button
                onClick={() => setSearchTerm("")}
                className="p-1 hover:bg-slate-100 rounded-lg transition-colors"
              >
                <X className="w-3.5 h-3.5 text-slate-400" />
              </button>
            )}
          </div>

          {/* List */}
          <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
            <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
              <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                <Package className="w-4 h-4 text-blue-600" /> PO List
                <span className="ml-2 text-[10px] font-bold bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">
                  {filteredList.length}
                </span>
              </h2>
            </div>
            <div className="overflow-auto max-h-[calc(100vh-380px)]">
              <table className="w-full text-sm text-left">
                <thead className="text-[10px] uppercase text-slate-400 bg-white sticky top-0 border-b border-slate-100 z-10 backdrop-blur">
                  <tr>
                    <th className="px-5 py-3 font-bold tracking-wider">Order No.</th>
                    <th className="px-5 py-3 font-bold tracking-wider">Product Name</th>
                    <th className="px-5 py-3 font-bold tracking-wider">Order Qty</th>
                    <th className="px-5 py-3 font-bold tracking-wider">GTIN</th>
                    <th className="px-5 py-3 font-bold tracking-wider">Production Date</th>
                    <th className="px-5 py-3 font-bold tracking-wider text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100/80">
                  {isLoading ? (
                    <tr>
                      <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                        <RefreshCw className="w-6 h-6 mx-auto mb-2 animate-spin" />
                        Loading data...
                      </td>
                    </tr>
                  ) : filteredList.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="px-5 py-10 text-center text-slate-400">
                        {searchTerm
                          ? "No matching POs found"
                          : "No POs yet. Create a new PO to get started."}
                      </td>
                    </tr>
                  ) : (
                    filteredList.map((po) => (
                      <tr key={po.orderNo} className="hover:bg-slate-50/50 transition-colors">
                        <td className="px-5 py-3 font-mono text-xs font-bold text-slate-800">
                          {po.orderNo}
                        </td>
                        <td className="px-5 py-3 text-slate-700">
                          {po.productName || "—"}
                        </td>
                        <td className="px-5 py-3 font-semibold text-slate-700">
                          {po.orderQty?.toLocaleString() || "—"}
                        </td>
                        <td className="px-5 py-3 font-mono text-xs text-slate-500">
                          {po.gtin || "—"}
                        </td>
                        <td className="px-5 py-3 font-mono text-xs text-slate-600">
                          {po.productionDate || "—"}
                        </td>
                        <td className="px-5 py-3 text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => fetchPODetail(po.orderNo)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                            >
                              <FileText className="w-3.5 h-3.5" /> Detail
                            </button>
                            <button
                              onClick={() => handleDeleteClick(po)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                              title="Delete PO"
                            >
                              <Trash2 className="w-3.5 h-3.5" />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Tab Content: CREATE */}
      {activeTab === "create" && (
        <form
          onSubmit={handleCreatePO}
          className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden animate-in fade-in duration-300"
        >
          <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
            <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
              <Plus className="w-4 h-4 text-blue-600" /> Create New PO
            </h2>
            <button
              type="button"
              onClick={() => setActiveTab("list")}
              className="text-xs font-semibold text-slate-500 hover:text-slate-700 flex items-center gap-1"
            >
              <ArrowLeft className="w-3.5 h-3.5" /> Back
            </button>
          </div>

          <div className="p-4 xl:p-6 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            <FormField label="Order No. *" name="orderNo" value={formData.orderNo} onChange={handleFormChange} error={formErrors.orderNo} icon={Hash} placeholder="e.g. PO0001" />
            <FormField label="GTIN" name="gtin" value={formData.gtin || ""} onChange={handleFormChange} icon={Tag} placeholder="e.g. A001" />
            <FormField label="Order Qty *" name="orderQty" type="number" value={formData.orderQty} onChange={handleFormChange} error={formErrors.orderQty} icon={Layers} />
            <FormField label="Carton Capacity" name="cartonCapacity" type="number" value={formData.cartonCapacity || 24} onChange={handleFormChange} icon={Box} />
            <FormField label="Production Date *" name="productionDate" type="date" value={formData.productionDate || ""} onChange={handleFormChange} error={formErrors.productionDate} icon={Calendar} />
            <SelectField label="Shift" name="shift" value={formData.shift || "A"} onChange={handleFormChange} options={[{ value: "A", label: "Shift A" }, { value: "B", label: "Shift B" }, { value: "C", label: "Shift C" }]} />
            <FormField label="Product Name" name="productName" value={formData.productName || ""} onChange={handleFormChange} icon={Package} />
            <FormField label="Product Code" name="productCode" value={formData.productCode || ""} onChange={handleFormChange} icon={Tag} />
            <FormField label="Lot Number" name="lotNumber" value={formData.lotNumber || ""} onChange={handleFormChange} icon={Hash} />
            <FormField label="Site" name="site" value={formData.site || ""} onChange={handleFormChange} icon={Building2} />
            <FormField label="Factory" name="factory" value={formData.factory || ""} onChange={handleFormChange} icon={Factory} />
            <FormField label="Production Line" name="productionLine" value={formData.productionLine || ""} onChange={handleFormChange} icon={Layers} />
            <FormField label="Customer Order No." name="customerOrderNo" value={formData.customerOrderNo || ""} onChange={handleFormChange} icon={Hash} />
            <SelectField label="UOM" name="uom" value={formData.uom || "PCS"} onChange={handleFormChange} options={[{ value: "PCS", label: "PCS" }, { value: "BOX", label: "BOX" }, { value: "SET", label: "SET" }]} />
            <FormField label="Created By" name="userName" value={formData.userName || ""} onChange={handleFormChange} icon={User} />
          </div>

          <div className="bg-slate-50/80 border-t border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
            <label className="flex items-center gap-2 text-sm font-semibold text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={formData.autoLoadCodes ?? true}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, autoLoadCodes: e.target.checked }))
                }
                className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
              />
              Auto-load codes from DataPool
            </label>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setActiveTab("list")}
                className="px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSaving}
                className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-xl text-sm font-bold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSaving ? <RefreshCw className="w-4 h-4 animate-spin" /> : <Plus className="w-4 h-4" />}
                {isSaving ? "Creating..." : "Create PO"}
              </button>
            </div>
          </div>
        </form>
      )}

      {/* Tab Content: DETAIL */}
      {activeTab === "detail" && selectedPO && (
        <div className="flex flex-col gap-4 animate-in fade-in duration-300">
          <div className="flex items-center gap-3">
            <button
              onClick={() => { setActiveTab("list"); setSelectedPO(null); }}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
            >
              <ArrowLeft className="w-3.5 h-3.5" /> Back to List
            </button>
          </div>

          {/* Detail Tabs */}
          <div className="flex items-center gap-1.5 bg-slate-100/60 p-1.5 rounded-2xl shrink-0 w-fit">
            <button onClick={() => setDetailTab("info")} className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${detailTab === "info" ? "bg-white text-blue-700 shadow-sm" : "text-slate-600 hover:text-slate-900"}`}>
              <FileText className="w-4 h-4" /> Thông tin (Info)
            </button>
            <button onClick={() => handleDetailTabChange("codes")} className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${detailTab === "codes" ? "bg-white text-blue-700 shadow-sm" : "text-slate-600 hover:text-slate-900"}`}>
              <Code className="w-4 h-4" /> Mã (Codes) ({poCodes.length})
            </button>
            <button onClick={() => handleDetailTabChange("cartons")} className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${detailTab === "cartons" ? "bg-white text-blue-700 shadow-sm" : "text-slate-600 hover:text-slate-900"}`}>
              <Box className="w-4 h-4" /> Thùng (Cartons) ({poCartons.length})
            </button>
            <button onClick={() => handleDetailTabChange("database")} className={`flex items-center gap-2 px-4 py-2 text-xs font-bold rounded-xl transition-all ${detailTab === "database" ? "bg-white text-blue-700 shadow-sm" : "text-slate-600 hover:text-slate-900"}`}>
              <Database className="w-4 h-4" /> Cơ sở dữ liệu (Database)
            </button>
          </div>

          {/* INFO TAB */}
          {detailTab === "info" && (
            <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
              {/* Basic Info */}
              <div className="xl:col-span-2 bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
                <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                  <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                    <FileText className="w-4 h-4 text-blue-600" /> Chi tiết PO (PO Detail):{" "}
                    <span className="font-mono text-blue-700">{selectedPO.orderNo}</span>
                  </h2>
                </div>
                <div className="p-4 xl:p-6 grid grid-cols-1 md:grid-cols-2 gap-x-6">
                  <DetailRow label="Mã đơn hàng (Order No.)" value={selectedPO.orderNo} mono />
                  <DetailRow label="Số lượng đặt (Order Qty)" value={selectedPO.orderQty?.toLocaleString()} />
                  <DetailRow label="GTIN" value={selectedPO.gtin} mono />
                  <DetailRow label="Tên sản phẩm (Product Name)" value={selectedPO.productName} />
                  <DetailRow label="Mã sản phẩm (Product Code)" value={selectedPO.productCode} />
                  <DetailRow label="Số lô (Lot Number)" value={selectedPO.lotNumber} />
                  <DetailRow label="Site" value={selectedPO.site} icon={Building2} />
                  <DetailRow label="Nhà máy (Factory)" value={selectedPO.factory} icon={Factory} />
                  <DetailRow label="Dây chuyền (Production Line)" value={selectedPO.productionLine} />
                  <DetailRow label="Ca (Shift)" value={selectedPO.shift} />
                  <DetailRow label="Ngày sản xuất (Production Date)" value={selectedPO.productionDate} icon={Calendar} />
                  <DetailRow label="Mã đơn hàng KH (Customer Order No.)" value={selectedPO.customerOrderNo} />
                  <DetailRow label="Đơn vị (UOM)" value={selectedPO.uom} />
                  <DetailRow label="Thời gian tạo (Created Time)" value={formatDate(selectedPO.createdTime)} />
                  <DetailRow label="Thời gian sửa (Modified Time)" value={formatDate(selectedPO.modifiedTime)} />
                </div>
              </div>

              {/* Stats card */}
              <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
                <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                  <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                    <Layers className="w-4 h-4 text-blue-600" /> Tổng quan (Overview)
                  </h2>
                </div>
                <div className="p-4 xl:p-6 flex flex-col gap-3">
                  <div className="rounded-2xl border border-blue-200/60 bg-blue-50/50 p-4 text-center">
                    <div className="text-[10px] font-bold uppercase tracking-wider text-blue-700 opacity-70 mb-1">Số lượng đặt (Order Qty)</div>
                    <div className="text-3xl font-black text-blue-800 tracking-tight">{selectedPO.orderQty?.toLocaleString() || "—"}</div>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="rounded-xl border border-green-200/60 bg-green-50/50 p-3 text-center">
                      <div className="text-[10px] font-bold uppercase tracking-wider text-green-600 mb-1">Đã kích hoạt (Active)</div>
                      <div className="text-2xl font-black text-green-700">{selectedPO.stats?.activeCodes?.toLocaleString() || 0}</div>
                    </div>
                    <div className="rounded-xl border border-purple-200/60 bg-purple-50/50 p-3 text-center">
                      <div className="text-[10px] font-bold uppercase tracking-wider text-purple-600 mb-1">Đã đóng gói (Packed)</div>
                      <div className="text-2xl font-black text-purple-700">{selectedPO.stats?.packedCodes?.toLocaleString() || 0}</div>
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="rounded-xl border border-slate-200/80 bg-slate-50 p-3 text-center">
                      <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Thùng (Cartons)</div>
                      <div className="text-2xl font-black text-slate-700">{selectedPO.stats?.cartonCount || 0}</div>
                    </div>
                    <div className="rounded-xl border border-slate-200/80 bg-slate-50 p-3 text-center">
                      <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Đã đóng (Closed)</div>
                      <div className="text-2xl font-black text-slate-700">{selectedPO.stats?.closedCartons || 0}</div>
                    </div>
                  </div>
                  <div className="rounded-2xl border border-slate-200/80 bg-slate-50 p-4">
                    <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-2">Tiến độ (Progress)</div>
                    <div className="w-full bg-slate-200 rounded-full h-3 overflow-hidden">
                      <div
                        className="bg-gradient-to-r from-blue-500 to-blue-600 h-3 rounded-full transition-all duration-500"
                        style={{ width: `${Math.min(selectedPO.stats?.progressPercent || 0, 100)}%` }}
                      />
                    </div>
                    <div className="text-center mt-2">
                      <span className="text-lg font-black text-blue-700">{selectedPO.stats?.progressPercent?.toFixed(1) || 0}%</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* CODES TAB */}
          {detailTab === "codes" && (
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between flex-wrap gap-3">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <Code className="w-4 h-4 text-blue-600" /> Mã trong PO (Codes in PO)
                </h2>
                <div className="flex items-center gap-2">
                  <div className="flex bg-slate-100 rounded-lg p-1">
                    {(["all", "unused", "active", "packed"] as const).map((filter) => (
                      <button key={filter} onClick={() => handleCodesFilterChange(filter)}
                        className={`px-3 py-1 text-[10px] font-bold rounded-md transition-colors ${codesFilter === filter ? "bg-white text-blue-700 shadow-sm" : "text-slate-500 hover:text-slate-700"}`}>
                        {filter === "all" ? "Tất cả (All)" : filter === "unused" ? "Chưa dùng (Unused)" : filter === "active" ? "Đã kích hoạt (Active)" : "Đã đóng gói (Packed)"}
                      </button>
                    ))}
                  </div>
                  <button onClick={() => selectedPO && fetchPOCodes(selectedPO.orderNo, codesPage)} className="flex items-center gap-1.5 px-2.5 py-1 text-[10px] font-semibold text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                    <RefreshCw className={`w-3 h-3 ${isLoadingCodes ? "animate-spin" : ""}`} /> Làm mới
                  </button>
                </div>
              </div>
              <div className="overflow-auto max-h-[calc(100vh-380px)]">
                {isLoadingCodes ? (
                  <div className="text-center text-slate-400 py-10">
                    <RefreshCw className="w-6 h-6 mx-auto mb-2 animate-spin" />
                    <span className="text-sm">Đang tải mã...</span>
                  </div>
                ) : getFilteredCodes().length === 0 ? (
                  <div className="text-center text-slate-400 py-10">
                    <Code className="w-8 h-8 mx-auto mb-2 opacity-30" />
                    <span className="text-sm">Không tìm thấy mã nào</span>
                  </div>
                ) : (
                  <table className="w-full text-xs">
                    <thead>
                      <tr className="text-[10px] uppercase text-slate-400 bg-slate-50 border-b border-slate-100">
                        <th className="px-4 py-3 font-bold text-left">Mã (Code)</th>
                        <th className="px-4 py-3 font-bold text-center">Trạng thái (Status)</th>
                        <th className="px-4 py-3 font-bold text-left">Thùng (Carton)</th>
                        <th className="px-4 py-3 font-bold text-left">Ngày kích hoạt (Activate Date)</th>
                        <th className="px-4 py-3 font-bold text-left">Người kích hoạt (Activate User)</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                      {getFilteredCodes().map((code) => {
                        const statusInfo = getCodeStatusLabel(code);
                        return (
                          <tr key={code.id} className="hover:bg-slate-50/50">
                            <td className="px-4 py-2.5 font-mono font-semibold text-slate-700">{code.code}</td>
                            <td className="px-4 py-2.5 text-center">
                              <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${statusInfo.class}`}>{statusInfo.label}</span>
                            </td>
                            <td className="px-4 py-2.5 font-mono text-slate-600">{code.cartonCode && code.cartonCode !== "0" ? code.cartonCode : "—"}</td>
                            <td className="px-4 py-2.5 text-slate-500">{formatDate(code.activateDate)}</td>
                            <td className="px-4 py-2.5 text-slate-500">{code.activateUser || "—"}</td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                )}
              </div>
              {/* Pagination */}
              {totalCodes > CODES_PAGE_SIZE && (
                <div className="flex items-center justify-between px-4 xl:px-6 py-3 border-t border-slate-100 bg-slate-50/50">
                  <span className="text-xs text-slate-500">
                    Hiển thị {(codesPage - 1) * CODES_PAGE_SIZE + 1} - {Math.min(codesPage * CODES_PAGE_SIZE, totalCodes)} / {totalCodes} mã
                  </span>
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => handleCodesPageChange(codesPage - 1)}
                      disabled={codesPage <= 1 || isLoadingCodes}
                      className="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      ← Trước
                    </button>
                    <div className="flex items-center gap-1">
                      {Array.from({ length: Math.min(5, Math.ceil(totalCodes / CODES_PAGE_SIZE)) }, (_, i) => {
                        let pageNum: number;
                        const totalPages = Math.ceil(totalCodes / CODES_PAGE_SIZE);
                        if (totalPages <= 5) {
                          pageNum = i + 1;
                        } else if (codesPage <= 3) {
                          pageNum = i + 1;
                        } else if (codesPage >= totalPages - 2) {
                          pageNum = totalPages - 4 + i;
                        } else {
                          pageNum = codesPage - 2 + i;
                        }
                        return (
                          <button
                            key={pageNum}
                            onClick={() => handleCodesPageChange(pageNum)}
                            disabled={isLoadingCodes}
                            className={`w-8 h-8 text-xs font-bold rounded-lg border transition-colors ${
                              codesPage === pageNum
                                ? "bg-blue-600 text-white border-blue-600"
                                : "border-slate-200 hover:bg-white text-slate-600 disabled:opacity-50"
                            }`}
                          >
                            {pageNum}
                          </button>
                        );
                      })}
                    </div>
                    <button
                      onClick={() => handleCodesPageChange(codesPage + 1)}
                      disabled={codesPage >= Math.ceil(totalCodes / CODES_PAGE_SIZE) || isLoadingCodes}
                      className="px-3 py-1.5 text-xs font-semibold rounded-lg border border-slate-200 hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Sau →
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* CARTONS TAB */}
          {detailTab === "cartons" && (
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <Box className="w-4 h-4 text-blue-600" /> Thùng trong PO (Cartons in PO)
                </h2>
                <button onClick={() => selectedPO && fetchPOCartons(selectedPO.orderNo)} className="flex items-center gap-1.5 px-2.5 py-1 text-[10px] font-semibold text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                  <RefreshCw className={`w-3 h-3 ${isLoadingCartons ? "animate-spin" : ""}`} /> Làm mới
                </button>
              </div>
              <div className="overflow-auto max-h-[calc(100vh-380px)]">
                {isLoadingCartons ? (
                  <div className="text-center text-slate-400 py-10">
                    <RefreshCw className="w-6 h-6 mx-auto mb-2 animate-spin" />
                    <span className="text-sm">Đang tải thùng...</span>
                  </div>
                ) : poCartons.length === 0 ? (
                  <div className="text-center text-slate-400 py-10">
                    <Box className="w-8 h-8 mx-auto mb-2 opacity-30" />
                    <span className="text-sm">Không tìm thấy thùng nào</span>
                  </div>
                ) : (
                  <table className="w-full text-xs">
                    <thead>
                      <tr className="text-[10px] uppercase text-slate-400 bg-slate-50 border-b border-slate-100">
                        <th className="px-4 py-3 font-bold text-center">ID</th>
                        <th className="px-4 py-3 font-bold text-center">Trạng thái (Status)</th>
                        <th className="px-4 py-3 font-bold text-left">Thời gian bắt đầu (Start Time)</th>
                        <th className="px-4 py-3 font-bold text-left">Thời gian hoàn thành (Completed Time)</th>
                        <th className="px-4 py-3 font-bold text-left">Người dùng (User)</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                      {poCartons.map((carton) => {
                        const statusInfo = getCartonStatusLabel(carton);
                        return (
                          <tr key={carton.id} className="hover:bg-slate-50/50">
                            <td className="px-4 py-2.5 text-center font-bold text-slate-700">{carton.id}</td>
                            <td className="px-4 py-2.5 text-center">
                              <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${statusInfo.class}`}>{statusInfo.label}</span>
                            </td>
                            <td className="px-4 py-2.5 text-slate-500">{formatDate(carton.startDatetime)}</td>
                            <td className="px-4 py-2.5 text-slate-500">{formatDate(carton.completedDatetime)}</td>
                            <td className="px-4 py-2.5 text-slate-500">{carton.activateUser || "—"}</td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                )}
              </div>
            </div>
          )}

          {/* DATABASE TAB */}
          {detailTab === "database" && (
            <div className="space-y-4">
              {/* Status Header */}
              <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
                <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
                  <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                    <Database className="w-4 h-4 text-blue-600" /> Trạng thái CSDL (Database Status)
                  </h2>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => selectedPO && fetchPODatabaseStatus(selectedPO.orderNo)}
                      className="flex items-center gap-1.5 px-2.5 py-1 text-[10px] font-semibold text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                    >
                      <RefreshCw className={`w-3 h-3 ${isLoadingDbStatus ? "animate-spin" : ""}`} /> Làm mới
                    </button>
                    <button
                      onClick={handleEnsureDatabaseReady}
                      disabled={isEnsuringReady}
                      className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold rounded-lg transition-colors ${
                        isEnsuringReady
                          ? "bg-slate-200 text-slate-400 cursor-not-allowed"
                          : "bg-green-600 hover:bg-green-700 text-white"
                      }`}
                    >
                      {isEnsuringReady ? (
                        <><RefreshCw className="w-3.5 h-3.5 animate-spin" /> Đang thiết lập...</>
                      ) : (
                        <><CheckCircle className="w-3.5 h-3.5" /> Thiết lập tự động</>
                      )}
                    </button>
                  </div>
                </div>

                <div className="p-4 xl:p-6">
                  {isLoadingDbStatus ? (
                    <div className="flex items-center gap-3 text-slate-500 py-8 justify-center">
                      <RefreshCw className="w-5 h-5 animate-spin" />
                      <span className="text-sm font-medium">Đang tải trạng thái...</span>
                    </div>
                  ) : dbStatus ? (
                    <div className="space-y-4">
                      {/* Status Banner */}
                      <div className={`flex items-center gap-3 px-4 py-3 rounded-2xl border ${
                        dbStatus.isFullyInitialized
                          ? "bg-green-50 border-green-200 text-green-800"
                          : "bg-amber-50 border-amber-200 text-amber-800"
                      }`}>
                        {dbStatus.isFullyInitialized ? (
                          <CheckCircle className="w-5 h-5 shrink-0" />
                        ) : (
                          <AlertCircle className="w-5 h-5 shrink-0" />
                        )}
                        <div className="flex-1">
                          <p className="text-sm font-bold">
                            {dbStatus.isFullyInitialized ? "Cơ sở dữ liệu đã sẵn sàng" : "Cơ sở dữ liệu chưa sẵn sàng"}
                          </p>
                          <p className="text-xs opacity-80">{dbStatus.message}</p>
                        </div>
                      </div>

                      {/* Stats Grid */}
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                        <div className="rounded-xl border border-slate-200/80 bg-slate-50 p-3 text-center">
                          <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Mã đã nạp (Loaded Codes)</div>
                          <div className="text-2xl font-black text-slate-700">
                            {dbStatus.loadedCodes}
                            <span className="text-sm font-normal text-slate-400">/{dbStatus.totalCodes}</span>
                          </div>
                        </div>
                        <div className="rounded-xl border border-slate-200/80 bg-slate-50 p-3 text-center">
                          <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Thùng đã tạo (Created Cartons)</div>
                          <div className="text-2xl font-black text-slate-700">
                            {dbStatus.createdCartons}
                            <span className="text-sm font-normal text-slate-400">/{dbStatus.requiredCartons}</span>
                          </div>
                        </div>
                        <div className="rounded-xl border border-slate-200/80 bg-slate-50 p-3 text-center">
                          <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">File CSDL (DB Files)</div>
                          <div className="text-2xl font-black text-slate-700">
                            {dbStatus.files.filter(f => f.exists).length}
                            <span className="text-sm font-normal text-slate-400">/4</span>
                          </div>
                        </div>
                        <div className="rounded-xl border border-slate-200/80 bg-slate-50 p-3 text-center">
                          <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">File thiếu (Missing Files)</div>
                          <div className="text-2xl font-black text-slate-700">
                            {dbStatus.files.filter(f => !f.exists).length}
                          </div>
                        </div>
                      </div>

                      {/* Progress Bars */}
                      <div className="space-y-3">
                        <div>
                          <div className="flex items-center justify-between text-xs mb-1">
                            <span className="font-semibold text-slate-600">Mã đã nạp (Codes Loaded)</span>
                            <span className="text-slate-500">{dbStatus.totalCodes > 0 ? Math.round(dbStatus.loadedCodes / dbStatus.totalCodes * 100) : 0}%</span>
                          </div>
                          <div className="w-full bg-slate-200 rounded-full h-2.5 overflow-hidden">
                            <div
                              className="bg-blue-500 h-2.5 rounded-full transition-all duration-500"
                              style={{ width: `${dbStatus.totalCodes > 0 ? Math.min(dbStatus.loadedCodes / dbStatus.totalCodes * 100, 100) : 0}%` }}
                            />
                          </div>
                        </div>
                        <div>
                          <div className="flex items-center justify-between text-xs mb-1">
                            <span className="font-semibold text-slate-600">Thùng đã tạo (Cartons Created)</span>
                            <span className="text-slate-500">{dbStatus.requiredCartons > 0 ? Math.round(dbStatus.createdCartons / dbStatus.requiredCartons * 100) : 0}%</span>
                          </div>
                          <div className="w-full bg-slate-200 rounded-full h-2.5 overflow-hidden">
                            <div
                              className="bg-purple-500 h-2.5 rounded-full transition-all duration-500"
                              style={{ width: `${dbStatus.requiredCartons > 0 ? Math.min(dbStatus.createdCartons / dbStatus.requiredCartons * 100, 100) : 0}%` }}
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  ) : (
                    <div className="text-center text-slate-400 py-8">
                      <Database className="w-8 h-8 mx-auto mb-2 opacity-30" />
                      <span className="text-sm">Không có trạng thái CSDL</span>
                    </div>
                  )}
                </div>
              </div>

              {/* Database Files List */}
              {dbStatus && (
                <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
                  <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                    <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                      <FileText className="w-4 h-4 text-blue-600" /> Danh sách file CSDL (Database Files)
                    </h2>
                  </div>
                  <div className="divide-y divide-slate-100">
                    {dbStatus.files.map((file) => (
                      <div key={file.fileName} className="flex items-center gap-4 px-4 xl:px-6 py-3">
                        <div className={`w-8 h-8 rounded-lg flex items-center justify-center shrink-0 ${
                          file.exists ? "bg-green-100" : "bg-red-100"
                        }`}>
                          {file.exists ? (
                            <Check className="w-4 h-4 text-green-600" />
                          ) : (
                            <X className="w-4 h-4 text-red-600" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-semibold text-slate-700 truncate">{file.fileName}</p>
                          <p className="text-xs text-slate-400 truncate">{file.path}</p>
                        </div>
                        <div className="text-right shrink-0">
                          {file.exists ? (
                            <span className="text-xs font-medium text-green-600">
                              {((file.fileSize || 0) / 1024).toFixed(1)} KB
                            </span>
                          ) : (
                            <span className="text-xs font-medium text-red-600">Thiếu (Missing)</span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// ─────────────────────────────────────────────────────────────
// Helper sub-components
// ─────────────────────────────────────────────────────────────

interface FormFieldProps {
  label: string;
  name: string;
  value: string | number;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  error?: string;
  icon?: React.ElementType;
  type?: string;
  placeholder?: string;
}

const FormField: React.FC<FormFieldProps> = ({ label, name, value, onChange, error, icon: Icon, type = "text", placeholder }) => (
  <div className="flex flex-col gap-1.5">
    <label className="text-xs font-bold uppercase tracking-wider text-slate-600">{label}</label>
    <div className="relative">
      {Icon && <Icon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />}
      <input
        type={type} name={name} value={value} onChange={onChange} placeholder={placeholder}
        className={`w-full bg-slate-50 border ${error ? "border-red-300" : "border-slate-200"} text-slate-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block p-2.5 ${Icon ? "pl-10" : ""} outline-none transition-colors`}
      />
    </div>
    {error && <span className="text-xs font-semibold text-red-600 flex items-center gap-1"><AlertCircle className="w-3 h-3" /> {error}</span>}
  </div>
);

interface SelectFieldProps {
  label: string;
  name: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  options: { value: string; label: string }[];
}

const SelectField: React.FC<SelectFieldProps> = ({ label, name, value, onChange, options }) => (
  <div className="flex flex-col gap-1.5">
    <label className="text-xs font-bold uppercase tracking-wider text-slate-600">{label}</label>
    <select
      name={name} value={value} onChange={onChange}
      className="bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 outline-none transition-colors"
    >
      {options.map((opt) => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
    </select>
  </div>
);

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

export default POManagerView;
