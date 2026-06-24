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
} from "lucide-react";
import poApi from "../../services/poApi";
import datapoolApi from "../../services/datapoolApi";
import type { POInfo, POListItem, CreatePORequest } from "../../types/po";
import type { DataPoolCode } from "../../types/datapool";

type TabKey = "list" | "create" | "detail";

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
  userName: "Admin",
  autoLoadCodes: true,
};

const POManagerView: React.FC<POManagerViewProps> = () => {
  const [activeTab, setActiveTab] = useState<TabKey>("list");
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

  // DataPool codes for PO detail
  const [poolCodes, setPoolCodes] = useState<DataPoolCode[]>([]);
  const [isLoadingPoolCodes, setIsLoadingPoolCodes] = useState(false);
  const [poolCodesError, setPoolCodesError] = useState<string | null>(null);

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
          (po.productName || "").toLowerCase().includes(term),
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
      // Load pool codes if PO has gtin
      if (data.gtin) {
        await loadPOCodes(data.gtin);
      } else {
        setPoolCodes([]);
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to load PO detail";
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const loadPOCodes = async (gtin: string) => {
    setIsLoadingPoolCodes(true);
    setPoolCodesError(null);
    try {
      const codes = await datapoolApi.getCodes(gtin);
      setPoolCodes(codes);
    } catch (err) {
      setPoolCodes([]);
      setPoolCodesError(err instanceof Error ? err.message : "Không thể tải mã từ pool");
    } finally {
      setIsLoadingPoolCodes(false);
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
      setDeleteCheck({ canDelete: false, reason: "Không thể kiểm tra trạng thái PO" });
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
      const result = await poApi.deletePO(deleteTarget.orderNo, "Admin");
      if (result.success) {
        setSuccess(`Đã xóa PO "${deleteTarget.orderNo}" thành công.`);
        setDeleteTarget(null);
        await fetchPOList();
      } else {
        setError(result.message || "Xóa PO thất bại.");
        setDeleteCheck({ canDelete: false, reason: result.reason });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Xóa PO thất bại.");
    } finally {
      setIsDeleting(false);
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};
    if (!formData.orderNo.trim()) {
      errors.orderNo = "Mã PO là bắt buộc";
    }
    if (!formData.orderQty || formData.orderQty <= 24) {
      errors.orderQty = "Số lượng phải lớn hơn 24";
    }
    if (!formData.productionDate) {
      errors.productionDate = "Ngày sản xuất là bắt buộc";
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
          `Tạo PO "${result.orderNo}" thành công${
            result.loadedCodesCount
              ? ` - Đã nạp ${result.loadedCodesCount} mã`
              : ""
          }`,
        );
        setFormData({ ...initialCreateForm });
        setActiveTab("list");
        await fetchPOList();
      } else {
        setError(result.message || "Tạo PO thất bại");
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Tạo PO thất bại";
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
    if (!dateStr) return "—";
    try {
      return new Date(dateStr).toLocaleString("vi-VN");
    } catch {
      return dateStr;
    }
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
              Quản lý Lệnh sản xuất (PO)
            </h1>
            <p className="text-xs text-slate-500 mt-0.5">
              Tạo và theo dõi các lệnh sản xuất từ POApiServer
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
                <h3 className="text-base font-bold text-slate-800">Xóa PO</h3>
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
                  <span className="text-sm font-medium">Đang kiểm tra trạng thái PO...</span>
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
                        {deleteCheck.canDelete ? "Có thể xóa" : "Không thể xóa"}
                      </p>
                      <p className="text-xs mt-0.5 opacity-80">{deleteCheck.reason}</p>
                    </div>
                  </div>

                  {!deleteCheck.canDelete && (
                    <p className="text-xs text-slate-500 text-center">
                      PO đã chạy mã không thể xóa. Vui lòng đóng và thử lại.
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
                Hủy
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
                    Đang xóa...
                  </>
                ) : (
                  <>
                    <Trash2 className="w-4 h-4" />
                    Xóa PO
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
          <List className="w-4 h-4" /> Danh sách PO
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
          <Plus className="w-4 h-4" /> Tạo PO mới
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
            <FileText className="w-4 h-4" /> Chi tiết
          </button>
        )}
      </div>

      {/* Tab Content */}
      {activeTab === "list" && (
        <div className="flex flex-col gap-4 animate-in fade-in duration-300">
          {/* Search */}
          <div className="flex items-center gap-2 bg-white rounded-2xl border border-slate-200 px-3 py-2 shadow-sm">
            <Search className="w-4 h-4 text-slate-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Tìm kiếm theo mã PO hoặc tên sản phẩm..."
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
                <Package className="w-4 h-4 text-blue-600" /> Danh sách PO
                <span className="ml-2 text-[10px] font-bold bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">
                  {filteredList.length}
                </span>
              </h2>
            </div>
            <div className="overflow-auto max-h-[calc(100vh-380px)]">
              <table className="w-full text-sm text-left">
                <thead className="text-[10px] uppercase text-slate-400 bg-white sticky top-0 border-b border-slate-100 z-10 backdrop-blur">
                  <tr>
                    <th className="px-5 py-3 font-bold tracking-wider">
                      Mã PO
                    </th>
                    <th className="px-5 py-3 font-bold tracking-wider">
                      Tên sản phẩm
                    </th>
                    <th className="px-5 py-3 font-bold tracking-wider">
                      Số lượng
                    </th>
                    <th className="px-5 py-3 font-bold tracking-wider">
                      Ngày SX
                    </th>
                    <th className="px-5 py-3 font-bold tracking-wider">
                      Trạng thái
                    </th>
                    <th className="px-5 py-3 font-bold tracking-wider text-right">
                      Hành động
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100/80">
                  {isLoading ? (
                    <tr>
                      <td
                        colSpan={6}
                        className="px-5 py-10 text-center text-slate-400"
                      >
                        <RefreshCw className="w-6 h-6 mx-auto mb-2 animate-spin" />
                        Đang tải dữ liệu...
                      </td>
                    </tr>
                  ) : filteredList.length === 0 ? (
                    <tr>
                      <td
                        colSpan={6}
                        className="px-5 py-10 text-center text-slate-400"
                      >
                        {searchTerm
                          ? "Không tìm thấy PO phù hợp"
                          : "Chưa có PO nào. Tạo PO mới để bắt đầu."}
                      </td>
                    </tr>
                  ) : (
                    filteredList.map((po) => (
                      <tr
                        key={po.orderNo}
                        className="hover:bg-slate-50/50 transition-colors"
                      >
                        <td className="px-5 py-3 font-mono text-xs font-bold text-slate-800">
                          {po.orderNo}
                        </td>
                        <td className="px-5 py-3 text-slate-700">
                          {po.productName || "—"}
                        </td>
                        <td className="px-5 py-3 font-semibold text-slate-700">
                          {po.orderQty?.toLocaleString() || "—"}
                        </td>
                        <td className="px-5 py-3 font-mono text-xs text-slate-600">
                          {po.productionDate || "—"}
                        </td>
                        <td className="px-5 py-3">
                          <span className="px-2 py-0.5 rounded-full text-[10px] font-bold tracking-wide bg-blue-100 text-blue-700">
                            {po.status
                              ? new Date(po.status).toLocaleDateString("vi-VN")
                              : "Mới"}
                          </span>
                        </td>
                        <td className="px-5 py-3 text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => fetchPODetail(po.orderNo)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                            >
                              <FileText className="w-3.5 h-3.5" /> Chi tiết
                            </button>
                            <button
                              onClick={() => handleDeleteClick(po)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                              title="Xóa PO"
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

      {activeTab === "create" && (
        <form
          onSubmit={handleCreatePO}
          className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden animate-in fade-in duration-300"
        >
          <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
            <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
              <Plus className="w-4 h-4 text-blue-600" /> Tạo PO mới
            </h2>
            <button
              type="button"
              onClick={() => setActiveTab("list")}
              className="text-xs font-semibold text-slate-500 hover:text-slate-700 flex items-center gap-1"
            >
              <ArrowLeft className="w-3.5 h-3.5" /> Quay lại
            </button>
          </div>

          <div className="p-4 xl:p-6 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            <FormField
              label="Mã PO *"
              name="orderNo"
              value={formData.orderNo}
              onChange={handleFormChange}
              error={formErrors.orderNo}
              icon={Hash}
              placeholder="VD: PO0001"
            />
            <FormField
              label="GTIN"
              name="gtin"
              value={formData.gtin || ""}
              onChange={handleFormChange}
              icon={Tag}
              placeholder="VD: A001"
            />
            <FormField
              label="Số lượng *"
              name="orderQty"
              type="number"
              value={formData.orderQty}
              onChange={handleFormChange}
              error={formErrors.orderQty}
              icon={Layers}
            />
            <FormField
              label="Ngày sản xuất *"
              name="productionDate"
              type="date"
              value={formData.productionDate || ""}
              onChange={handleFormChange}
              error={formErrors.productionDate}
              icon={Calendar}
            />
            <SelectField
              label="Ca"
              name="shift"
              value={formData.shift || "A"}
              onChange={handleFormChange}
              options={[
                { value: "A", label: "Ca A" },
                { value: "B", label: "Ca B" },
                { value: "C", label: "Ca C" },
              ]}
            />
            <SelectField
              label="Đơn vị (UOM)"
              name="uom"
              value={formData.uom || "PCS"}
              onChange={handleFormChange}
              options={[
                { value: "PCS", label: "PCS (Cái)" },
                { value: "BOX", label: "BOX (Hộp)" },
                { value: "SET", label: "SET (Bộ)" },
              ]}
            />
            <FormField
              label="Tên sản phẩm"
              name="productName"
              value={formData.productName || ""}
              onChange={handleFormChange}
              icon={Package}
            />
            <FormField
              label="Mã sản phẩm"
              name="productCode"
              value={formData.productCode || ""}
              onChange={handleFormChange}
              icon={Tag}
            />
            <FormField
              label="Số Lot"
              name="lotNumber"
              value={formData.lotNumber || ""}
              onChange={handleFormChange}
              icon={Hash}
            />
            <FormField
              label="Site"
              name="site"
              value={formData.site || ""}
              onChange={handleFormChange}
              icon={Building2}
            />
            <FormField
              label="Nhà máy"
              name="factory"
              value={formData.factory || ""}
              onChange={handleFormChange}
              icon={Factory}
            />
            <FormField
              label="Chuyền sản xuất"
              name="productionLine"
              value={formData.productionLine || ""}
              onChange={handleFormChange}
              icon={Layers}
            />
            <FormField
              label="Mã đơn hàng KH"
              name="customerOrderNo"
              value={formData.customerOrderNo || ""}
              onChange={handleFormChange}
              icon={Hash}
            />
            <FormField
              label="Người tạo"
              name="userName"
              value={formData.userName || ""}
              onChange={handleFormChange}
              icon={User}
            />
          </div>

          <div className="bg-slate-50/80 border-t border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
            <label className="flex items-center gap-2 text-sm font-semibold text-slate-700 cursor-pointer">
              <input
                type="checkbox"
                checked={formData.autoLoadCodes ?? true}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    autoLoadCodes: e.target.checked,
                  }))
                }
                className="w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
              />
              Tự động nạp mã từ DataPool
            </label>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setActiveTab("list")}
                className="px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
              >
                Hủy
              </button>
              <button
                type="submit"
                disabled={isSaving}
                className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2 rounded-xl text-sm font-bold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSaving ? (
                  <RefreshCw className="w-4 h-4 animate-spin" />
                ) : (
                  <Plus className="w-4 h-4" />
                )}
                {isSaving ? "Đang tạo..." : "Tạo PO"}
              </button>
            </div>
          </div>
        </form>
      )}

      {activeTab === "detail" && selectedPO && (
        <div className="flex flex-col gap-4 animate-in fade-in duration-300">
          <div className="flex items-center gap-3">
            <button
              onClick={() => setActiveTab("list")}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
            >
              <ArrowLeft className="w-3.5 h-3.5" /> Quay lại danh sách
            </button>
          </div>

          <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
            {/* Basic Info */}
            <div className="xl:col-span-2 bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <FileText className="w-4 h-4 text-blue-600" /> Thông tin PO:{" "}
                  <span className="font-mono text-blue-700">
                    {selectedPO.orderNo}
                  </span>
                </h2>
              </div>
              <div className="p-4 xl:p-6 grid grid-cols-1 md:grid-cols-2 gap-x-6">
                <DetailRow label="Mã PO" value={selectedPO.orderNo} mono />
                <DetailRow label="Số lượng" value={selectedPO.orderQty} />
                <DetailRow label="GTIN" value={selectedPO.gtin} />
                <DetailRow
                  label="Tên sản phẩm"
                  value={selectedPO.productName}
                />
                <DetailRow
                  label="Mã sản phẩm"
                  value={selectedPO.productCode}
                />
                <DetailRow label="Số Lot" value={selectedPO.lotNumber} />
                <DetailRow
                  label="Site"
                  value={selectedPO.site}
                  icon={Building2}
                />
                <DetailRow
                  label="Nhà máy"
                  value={selectedPO.factory}
                  icon={Factory}
                />
                <DetailRow
                  label="Chuyền SX"
                  value={selectedPO.productionLine}
                />
                <DetailRow label="Ca" value={selectedPO.shift} />
                <DetailRow
                  label="Ngày sản xuất"
                  value={selectedPO.productionDate}
                  icon={Calendar}
                />
                <DetailRow
                  label="Mã ĐH khách hàng"
                  value={selectedPO.customerOrderNo}
                />
                <DetailRow label="Đơn vị" value={selectedPO.uom} />
                <DetailRow
                  label="Ngày tạo"
                  value={formatDate(selectedPO.createdTime)}
                />
                <DetailRow
                  label="Cập nhật"
                  value={formatDate(selectedPO.modifiedTime)}
                />
              </div>
            </div>

            {/* Stats card */}
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <Layers className="w-4 h-4 text-blue-600" /> Tổng quan
                </h2>
              </div>
              <div className="p-4 xl:p-6 flex flex-col gap-3">
                <div className="rounded-2xl border border-blue-200/60 bg-blue-50/50 p-4 text-center">
                  <div className="text-[10px] font-bold uppercase tracking-wider text-blue-700 opacity-70 mb-1">
                    Số lượng đặt
                  </div>
                  <div className="text-3xl font-black text-blue-800 tracking-tight">
                    {selectedPO.orderQty?.toLocaleString() || "—"}
                  </div>
                </div>
                <div className="rounded-2xl border border-slate-200/80 bg-slate-50 p-4">
                  <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-2">
                    Trạng thái
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-2.5 h-2.5 rounded-full bg-green-500 shadow-[0_0_6px_rgba(34,197,94,0.6)]" />
                    <span className="text-sm font-bold text-slate-700">
                      PO đang hoạt động
                    </span>
                  </div>
                </div>
                <div className="text-xs text-slate-500 text-center pt-2 border-t border-slate-100">
                  Theo dõi mã và thùng carton trong hệ thống POApiServer
                </div>
              </div>
            </div>
          </div>

          {/* DataPool Section */}
          {selectedPO.gtin && (
            <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
              <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
                <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
                  <Database className="w-4 h-4 text-purple-600" /> DataPool:{" "}
                  <span className="font-mono text-purple-700">{selectedPO.gtin}</span>
                </h2>
                <button
                  onClick={() => selectedPO.gtin && loadPOCodes(selectedPO.gtin)}
                  className="flex items-center gap-1.5 px-2.5 py-1 text-[10px] font-semibold text-purple-600 hover:bg-purple-50 rounded-lg transition-colors"
                >
                  <RefreshCw className={`w-3 h-3 ${isLoadingPoolCodes ? "animate-spin" : ""}`} />
                  Làm mới
                </button>
              </div>
              <div className="p-4 xl:p-6">
                {isLoadingPoolCodes ? (
                  <div className="text-center text-slate-400 py-6">
                    <RefreshCw className="w-6 h-6 mx-auto mb-2 animate-spin" />
                    <span className="text-sm">Đang tải mã từ pool...</span>
                  </div>
                ) : poolCodesError ? (
                  <div className="flex items-center gap-2 text-red-600 text-sm">
                    <AlertCircle className="w-4 h-4" />
                    {poolCodesError}
                  </div>
                ) : poolCodes.length === 0 ? (
                  <div className="text-center text-slate-400 py-6">
                    <Database className="w-8 h-8 mx-auto mb-2 opacity-30" />
                    <span className="text-sm">Chưa có mã nào trong pool này</span>
                  </div>
                ) : (
                  <div className="flex flex-col gap-3">
                    {/* Pool stats summary */}
                    <div className="grid grid-cols-3 gap-3">
                      <div className="rounded-xl border border-green-200/60 bg-green-50/50 p-3 text-center">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-green-600 mb-1">Sẵn sàng</div>
                        <div className="text-2xl font-black text-green-700">
                          {poolCodes.filter(c => c.status === 0).length.toLocaleString()}
                        </div>
                      </div>
                      <div className="rounded-xl border border-slate-200/60 bg-slate-50/50 p-3 text-center">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Đã dùng</div>
                        <div className="text-2xl font-black text-slate-600">
                          {poolCodes.filter(c => c.status !== 0).length.toLocaleString()}
                        </div>
                      </div>
                      <div className="rounded-xl border border-slate-200/60 bg-slate-50/50 p-3 text-center">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Tổng</div>
                        <div className="text-2xl font-black text-slate-700">
                          {poolCodes.length.toLocaleString()}
                        </div>
                      </div>
                    </div>
                    {/* Code list */}
                    <div className="bg-slate-50/50 rounded-xl border border-slate-200/80 overflow-hidden">
                      <table className="w-full text-xs">
                        <thead>
                          <tr className="text-[10px] uppercase text-slate-400 bg-slate-100 border-b border-slate-200">
                            <th className="px-3 py-2 font-bold text-left">Mã</th>
                            <th className="px-3 py-2 font-bold text-center">Trạng thái</th>
                            <th className="px-3 py-2 font-bold text-left">Batch</th>
                            <th className="px-3 py-2 font-bold text-left">Ghi chú</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                          {poolCodes.slice(0, 50).map((code) => (
                            <tr key={code.code} className="hover:bg-white/50">
                              <td className="px-3 py-2 font-mono font-semibold text-slate-700 text-[11px]">{code.code}</td>
                              <td className="px-3 py-2 text-center">
                                <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                                  code.status === 0 ? "bg-green-100 text-green-700" :
                                  code.status === 1 ? "bg-blue-100 text-blue-700" :
                                  code.status === 2 ? "bg-slate-100 text-slate-500" :
                                  "bg-red-100 text-red-700"
                                }`}>
                                  {code.status === 0 ? "Sẵn sàng" :
                                   code.status === 1 ? "Đang dùng" :
                                   code.status === 2 ? "Đã xóa" : "Lỗi"}
                                </span>
                              </td>
                              <td className="px-3 py-2 text-slate-500">{code.batchID || "—"}</td>
                              <td className="px-3 py-2 text-slate-500 truncate max-w-[180px]">{code.note || "—"}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                      {poolCodes.length > 50 && (
                        <div className="px-3 py-2 text-center text-[10px] text-slate-400 bg-slate-100 border-t border-slate-200">
                          Hiển thị 50 / {poolCodes.length} mã
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
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

const FormField: React.FC<FormFieldProps> = ({
  label,
  name,
  value,
  onChange,
  error,
  icon: Icon,
  type = "text",
  placeholder,
}) => (
  <div className="flex flex-col gap-1.5">
    <label className="text-xs font-bold uppercase tracking-wider text-slate-600">
      {label}
    </label>
    <div className="relative">
      {Icon && (
        <Icon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 pointer-events-none" />
      )}
      <input
        type={type}
        name={name}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        className={`w-full bg-slate-50 border ${
          error ? "border-red-300" : "border-slate-200"
        } text-slate-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block p-2.5 ${
          Icon ? "pl-10" : ""
        } outline-none transition-colors`}
      />
    </div>
    {error && (
      <span className="text-xs font-semibold text-red-600 flex items-center gap-1">
        <AlertCircle className="w-3 h-3" /> {error}
      </span>
    )}
  </div>
);

interface SelectFieldProps {
  label: string;
  name: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => void;
  options: { value: string; label: string }[];
}

const SelectField: React.FC<SelectFieldProps> = ({
  label,
  name,
  value,
  onChange,
  options,
}) => (
  <div className="flex flex-col gap-1.5">
    <label className="text-xs font-bold uppercase tracking-wider text-slate-600">
      {label}
    </label>
    <select
      name={name}
      value={value}
      onChange={onChange}
      className="bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-xl focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 outline-none transition-colors"
    >
      {options.map((opt) => (
        <option key={opt.value} value={opt.value}>
          {opt.label}
        </option>
      ))}
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
      {Icon && <Icon className="w-3.5 h-3.5" />}
      {label}
    </span>
    <span
      className={`text-sm font-semibold text-slate-800 text-right break-words ${
        mono ? "font-mono" : ""
      }`}
    >
      {value || "—"}
    </span>
  </div>
);

export default POManagerView;
