import React, { useState, useEffect, useCallback } from "react";
import {
  Database,
  RefreshCw,
  Search,
  X,
  Check,
  AlertCircle,
  Tag,
  ChevronDown,
  ChevronRight,
  Plus,
  Upload,
  FileText,
  Loader2,
  CheckCircle2,
} from "lucide-react";
import datapoolApi from "../../services/datapoolApi";
import type { DataPoolInfo, DataPoolCode } from "../../types/datapool";

interface PoolWithStats extends DataPoolInfo {
  totalCodes: number;
  availableCodes: number;
  usedCodes: number;
  loaded?: boolean;
}

/* =========================================
   MODAL COMPONENT
   ========================================= */

type ModalMode = "create" | "addcode" | "import" | null;

const Modal: React.FC<{
  mode: ModalMode;
  onClose: () => void;
  children: React.ReactNode;
  title: string;
}> = ({ mode, onClose, children, title }) => {
  if (!mode) return null;
  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm animate-in fade-in duration-200"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="bg-white rounded-3xl shadow-2xl w-full max-w-lg mx-4 animate-in zoom-in-95 slide-in-from-bottom-4 duration-300">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h3 className="text-base font-bold text-slate-800">{title}</h3>
          <button
            onClick={onClose}
            className="p-2 hover:bg-slate-100 rounded-xl transition-colors text-slate-400 hover:text-slate-600"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
        <div className="p-6">{children}</div>
      </div>
    </div>
  );
};

/* =========================================
   CREATE POOL FORM
   ========================================= */

const CreatePoolForm: React.FC<{
  onSuccess: () => void;
  onClose: () => void;
}> = ({ onSuccess, onClose }) => {
  const [poolName, setPoolName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!poolName.trim()) {
      setError("Tên pool không được trống.");
      return;
    }
    setLoading(true);
    setError("");
    setSuccess("");
    try {
      await datapoolApi.createPool(poolName.trim());
      setSuccess(`Pool '${poolName.trim()}' đã được tạo thành công!`);
      setTimeout(() => {
        onSuccess();
        onClose();
      }, 1000);
    } catch (err: any) {
      setError(err?.response?.data?.message || err.message || "Đã xảy ra lỗi.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">
          Tên Pool (GTIN)
        </label>
        <input
          type="text"
          value={poolName}
          onChange={(e) => setPoolName(e.target.value)}
          placeholder="VD: A001, 8934588012345"
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
          autoFocus
        />
        <p className="text-[11px] text-slate-400 mt-1">
          Tên pool thường là GTIN của sản phẩm. File .vnqrdb sẽ được tạo trong C:/VNQR/Databases/
        </p>
      </div>
      {error && (
        <div className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" />
          {error}
        </div>
      )}
      {success && (
        <div className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-green-50 border border-green-200 text-green-700 text-sm">
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          {success}
        </div>
      )}
      <div className="flex justify-end gap-3 pt-2">
        <button
          type="button"
          onClick={onClose}
          className="px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={loading}
          className="flex items-center gap-2 px-5 py-2 text-sm font-bold text-white bg-purple-600 hover:bg-purple-700 disabled:opacity-50 rounded-xl transition-colors"
        >
          {loading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Plus className="w-4 h-4" />}
          Tạo Pool
        </button>
      </div>
    </form>
  );
};

/* =========================================
   ADD CODE FORM
   ========================================= */

const AddCodeForm: React.FC<{
  pools: PoolWithStats[];
  onSuccess: () => void;
  onClose: () => void;
}> = ({ pools, onSuccess, onClose }) => {
  const [poolName, setPoolName] = useState(pools[0]?.name || "");
  const [code, setCode] = useState("");
  const [batchID, setBatchID] = useState("");
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!poolName.trim()) { setError("Chọn pool."); return; }
    if (!code.trim()) { setError("Mã không được trống."); return; }
    setLoading(true);
    setError("");
    setSuccess("");
    try {
      await datapoolApi.addCode({
        poolName: poolName.trim(),
        code: code.trim(),
        status: 0,
        batchID: batchID.trim(),
        note: note.trim(),
      });
      setSuccess("Mã đã được thêm thành công!");
      setCode("");
      setBatchID("");
      setNote("");
      setTimeout(() => {
        onSuccess();
        onClose();
      }, 1000);
    } catch (err: any) {
      setError(err?.response?.data?.message || err.message || "Đã xảy ra lỗi.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Pool</label>
        <select
          value={poolName}
          onChange={(e) => setPoolName(e.target.value)}
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all appearance-none"
        >
          <option value="">-- Chọn pool --</option>
          {pools.map((p) => (
            <option key={p.name} value={p.name}>{p.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Mã QR / Code</label>
        <input
          type="text"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          placeholder="VD: QR-0001"
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all font-mono"
          autoFocus
        />
      </div>
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Batch ID <span className="font-normal text-slate-400">(tùy chọn)</span></label>
        <input
          type="text"
          value={batchID}
          onChange={(e) => setBatchID(e.target.value)}
          placeholder="VD: BATCH-2026-001"
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
        />
      </div>
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Ghi chú <span className="font-normal text-slate-400">(tùy chọn)</span></label>
        <input
          type="text"
          value={note}
          onChange={(e) => setNote(e.target.value)}
          placeholder="VD: Nhập tay"
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
        />
      </div>
      {error && (
        <div className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" />
          {error}
        </div>
      )}
      {success && (
        <div className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-green-50 border border-green-200 text-green-700 text-sm">
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          {success}
        </div>
      )}
      <div className="flex justify-end gap-3 pt-2">
        <button
          type="button"
          onClick={onClose}
          className="px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={loading}
          className="flex items-center gap-2 px-5 py-2 text-sm font-bold text-white bg-purple-600 hover:bg-purple-700 disabled:opacity-50 rounded-xl transition-colors"
        >
          {loading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Plus className="w-4 h-4" />}
          Thêm mã
        </button>
      </div>
    </form>
  );
};

/* =========================================
   IMPORT CSV FORM
   ========================================= */

interface ParsedCSV {
  headers: string[];
  rows: string[][];
  totalRows: number;
}

const ImportCSVForm: React.FC<{
  pools: PoolWithStats[];
  onSuccess: () => void;
  onClose: () => void;
}> = ({ pools, onSuccess, onClose }) => {
  const [poolName, setPoolName] = useState(pools[0]?.name || "");
  const [createID, setCreateID] = useState(`IMPORT-${new Date().toISOString().slice(0, 10).replace(/-/g, "")}-001`);
  const [parsedCSV, setParsedCSV] = useState<ParsedCSV | null>(null);
  const [codeColumn, setCodeColumn] = useState("Code");
  const [noteColumn, setNoteColumn] = useState("");
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const parseCSV = (text: string): ParsedCSV => {
    const lines = text.split(/\r?\n/).filter(line => line.trim());
    if (lines.length < 2) {
      throw new Error("File CSV phải có ít nhất 1 header và 1 dòng dữ liệu.");
    }
    const headers = splitCSVLine(lines[0]);
    const rows = lines.slice(1).map(line => splitCSVLine(line));
    return { headers, rows, totalRows: rows.length };
  };

  const splitCSVLine = (line: string): string[] => {
    const result: string[] = [];
    let current = "";
    let inQuotes = false;
    for (let i = 0; i < line.length; i++) {
      const char = line[i];
      if (char === '"') {
        if (inQuotes && line[i + 1] === '"') {
          current += '"';
          i++;
        } else {
          inQuotes = !inQuotes;
        }
      } else if (char === ',' && !inQuotes) {
        result.push(current.trim());
        current = "";
      } else {
        current += char;
      }
    }
    result.push(current.trim());
    return result;
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const text = event.target?.result as string;
        const parsed = parseCSV(text);
        setParsedCSV(parsed);

        // Auto-detect Code column
        const codeIdx = parsed.headers.findIndex(h =>
          h.toLowerCase().includes("code") ||
          h.toLowerCase().includes("gtin") ||
          h.toLowerCase().includes("qr")
        );
        if (codeIdx >= 0) setCodeColumn(parsed.headers[codeIdx]);

        // Auto-detect Note column
        const noteIdx = parsed.headers.findIndex(h =>
          h.toLowerCase().includes("note") ||
          h.toLowerCase().includes("ghi chú") ||
          h.toLowerCase().includes("desc")
        );
        if (noteIdx >= 0) setNoteColumn(parsed.headers[noteIdx]);

        setError("");
      } catch (err: any) {
        setError(err.message || "Lỗi khi đọc file CSV.");
        setParsedCSV(null);
      }
    };
    reader.readAsText(file);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!poolName.trim()) { setError("Chọn pool."); return; }
    if (!createID.trim()) { setError("CreateID không được trống."); return; }
    if (!parsedCSV) { setError("Vui lòng chọn file CSV."); return; }
    if (!codeColumn.trim()) { setError("Chọn cột mã."); return; }

    setLoading(true);
    setError("");
    setSuccess("");
    try {
      // Build CSV content from parsed data
      const headerRow = [codeColumn, noteColumn].filter(Boolean).join(",");
      const dataRows = parsedCSV.rows
        .map(row => {
          const codeIdx = parsedCSV.headers.indexOf(codeColumn);
          const noteIdx = noteColumn ? parsedCSV.headers.indexOf(noteColumn) : -1;
          const code = codeIdx >= 0 ? row[codeIdx] || "" : "";
          const noteVal = noteIdx >= 0 ? row[noteIdx] || "" : "";
          if (!code.trim()) return null;
          return `"${code.trim()}"${noteColumn ? `,"${noteVal.trim()}"` : ""}`;
        })
        .filter(Boolean)
        .join("\n");

      const csvContent = headerRow + "\n" + dataRows;

      // Send to backend via FormData (or we can update backend to accept JSON with CSV content)
      await datapoolApi.importCSVFromContent({
        poolName: poolName.trim(),
        csvContent,
        createID: createID.trim(),
        userName: "admin",
        note: note.trim(),
      });

      setSuccess(`Nhập ${parsedCSV.totalRows} mã thành công!`);
      setTimeout(() => {
        onSuccess();
        onClose();
      }, 1500);
    } catch (err: any) {
      setError(err?.response?.data?.message || err.message || "Đã xảy ra lỗi.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Pool</label>
        <select
          value={poolName}
          onChange={(e) => setPoolName(e.target.value)}
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all appearance-none"
        >
          <option value="">-- Chọn pool --</option>
          {pools.map((p) => (
            <option key={p.name} value={p.name}>{p.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Create ID</label>
        <input
          type="text"
          value={createID}
          onChange={(e) => setCreateID(e.target.value)}
          placeholder="VD: IMPORT-20260625-001"
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all font-mono"
        />
        <p className="text-[11px] text-slate-400 mt-1">Mã định danh phiên nhập (unique).</p>
      </div>

      {/* File Upload */}
      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">File CSV</label>
        <label className="flex items-center justify-center gap-2 w-full px-4 py-4 border-2 border-dashed border-slate-300 rounded-xl cursor-pointer hover:border-purple-400 hover:bg-purple-50/30 transition-all">
          <Upload className="w-5 h-5 text-slate-400" />
          <span className="text-sm text-slate-600">
            {parsedCSV ? `Đã chọn: ${parsedCSV.totalRows} dòng` : "Nhấn để chọn file CSV"}
          </span>
          <input
            type="file"
            accept=".csv"
            onChange={handleFileSelect}
            className="hidden"
          />
        </label>
      </div>

      {/* Column Selection */}
      {parsedCSV && (
        <div className="bg-slate-50 rounded-xl p-3 border border-slate-200">
          <div className="text-xs font-semibold text-slate-600 mb-2">
            Xem trước ({parsedCSV.totalRows} dòng)
          </div>
          {/* Preview table */}
          <div className="overflow-x-auto max-h-32 rounded-lg border border-slate-200">
            <table className="w-full text-xs">
              <thead className="bg-slate-100 sticky top-0">
                <tr>
                  {parsedCSV.headers.map((h, i) => (
                    <th key={i} className="px-2 py-1.5 text-left font-semibold text-slate-600 whitespace-nowrap border-b border-slate-200">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {parsedCSV.rows.slice(0, 3).map((row, ri) => (
                  <tr key={ri} className="hover:bg-white">
                    {row.map((cell, ci) => (
                      <td key={ci} className="px-2 py-1 text-slate-500 whitespace-nowrap border-b border-slate-100 last:border-0">
                        {cell || "—"}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {parsedCSV.totalRows > 3 && (
            <div className="text-[10px] text-slate-400 mt-1 text-center">
              ... và {parsedCSV.totalRows - 3} dòng khác
            </div>
          )}

          {/* Column selectors */}
          <div className="grid grid-cols-2 gap-3 mt-3">
            <div>
              <label className="block text-[11px] font-semibold text-slate-600 mb-1">Cột Mã <span className="text-red-500">*</span></label>
              <select
                value={codeColumn}
                onChange={(e) => setCodeColumn(e.target.value)}
                className="w-full bg-white border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-800 outline-none focus:ring-1 focus:ring-purple-500"
              >
                <option value="">-- Chọn cột --</option>
                {parsedCSV.headers.map((h) => (
                  <option key={h} value={h}>{h}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-[11px] font-semibold text-slate-600 mb-1">Cột Ghi chú</label>
              <select
                value={noteColumn}
                onChange={(e) => setNoteColumn(e.target.value)}
                className="w-full bg-white border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-800 outline-none focus:ring-1 focus:ring-purple-500"
              >
                <option value="">-- Không chọn --</option>
                {parsedCSV.headers.map((h) => (
                  <option key={h} value={h}>{h}</option>
                ))}
              </select>
            </div>
          </div>
        </div>
      )}

      <div>
        <label className="block text-sm font-semibold text-slate-700 mb-1.5">Ghi chú phiên nhập <span className="font-normal text-slate-400">(tùy chọn)</span></label>
        <input
          type="text"
          value={note}
          onChange={(e) => setNote(e.target.value)}
          placeholder="VD: Nhập từ khách hàng A"
          className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
        />
      </div>
      {error && (
        <div className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" />
          {error}
        </div>
      )}
      {success && (
        <div className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-green-50 border border-green-200 text-green-700 text-sm">
          <CheckCircle2 className="w-4 h-4 shrink-0" />
          {success}
        </div>
      )}
      <div className="flex justify-end gap-3 pt-2">
        <button
          type="button"
          onClick={onClose}
          className="px-4 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={loading || !parsedCSV}
          className="flex items-center gap-2 px-5 py-2 text-sm font-bold text-white bg-purple-600 hover:bg-purple-700 disabled:opacity-50 rounded-xl transition-colors"
        >
          {loading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Upload className="w-4 h-4" />}
          Nhập CSV
        </button>
      </div>
    </form>
  );
};

/* =========================================
   MAIN DATAPOOL VIEW
   ========================================= */

const DataPoolView: React.FC = () => {
  const [pools, setPools] = useState<PoolWithStats[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingPool, setIsLoadingPool] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [expandedPool, setExpandedPool] = useState<string | null>(null);
  const [poolCodes, setPoolCodes] = useState<Record<string, DataPoolCode[]>>({});
  const [poolSearchTerms, setPoolSearchTerms] = useState<Record<string, string>>({});
  const [poolSearchResults, setPoolSearchResults] = useState<Record<string, boolean>>({});
  const [modalMode, setModalMode] = useState<ModalMode>(null);

  // Load pool list with stats
  const fetchPools = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const poolList = await datapoolApi.getPoolsWithStats();
      setPools(poolList.map((p) => ({
        ...p,
        totalCodes: p.totalCodes ?? 0,
        availableCodes: p.availableCodes ?? 0,
        usedCodes: p.usedCodes ?? 0,
        loaded: true,
      })));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load pools");
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    fetchPools();
  }, [fetchPools]);

  // Toggle expand pool to show codes
  const toggleExpand = async (poolName: string) => {
    if (expandedPool === poolName) {
      setExpandedPool(null);
      return;
    }
    setExpandedPool(poolName);
    setPoolSearchTerms(prev => ({ ...prev, [poolName]: "" }));
    setPoolSearchResults(prev => ({ ...prev, [poolName]: false }));
  };

  // Search codes in pool via API
  const searchPoolCodes = async (poolName: string, keyword: string) => {
    if (!keyword.trim()) return;
    setIsLoadingPool(poolName);
    setPoolSearchResults(prev => ({ ...prev, [poolName]: true }));
    try {
      const codes = await datapoolApi.searchCodes(poolName, keyword);
      setPoolCodes(prev => ({ ...prev, [poolName]: codes }));
    } catch {
      setPoolCodes(prev => ({ ...prev, [poolName]: [] }));
    } finally {
      setIsLoadingPool(null);
    }
  };

  const filteredPools = pools.filter(p =>
    !searchTerm || p.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const statusColor = (available: number, total: number) => {
    if (total === 0) return "bg-slate-100 text-slate-500";
    const ratio = available / total;
    if (ratio > 0.5) return "bg-green-100 text-green-700";
    if (ratio > 0.2) return "bg-amber-100 text-amber-700";
    return "bg-red-100 text-red-700";
  };

  return (
    <div className="flex flex-col gap-4 h-full min-h-0 w-full animate-in fade-in duration-500 overflow-auto scrollbar-hide pb-6">
      {/* Header */}
      <div className="flex items-center justify-between shrink-0 px-1 flex-wrap gap-3">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-purple-600 flex items-center justify-center shadow-lg shadow-purple-600/20">
            <Database className="w-5 h-5 text-white" />
          </div>
          <div>
            <h1 className="text-xl 2xl:text-2xl font-bold text-slate-800 tracking-tight">
              Quản lý DataPool
            </h1>
            <p className="text-xs text-slate-500 mt-0.5">
              Tạo pool, nhập mã và theo dõi kho mã QR
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setModalMode("create")}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-purple-600 hover:text-purple-700 hover:bg-purple-50 rounded-xl border border-purple-200 transition-colors"
            title="Tạo Pool mới"
          >
            <Plus className="w-3.5 h-3.5" />
            Tạo Pool
          </button>
          <button
            onClick={() => setModalMode("addcode")}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-purple-600 hover:text-purple-700 hover:bg-purple-50 rounded-xl border border-purple-200 transition-colors"
            title="Nhập code đơn lẻ"
          >
            <FileText className="w-3.5 h-3.5" />
            Nhập Code
          </button>
          <button
            onClick={() => setModalMode("import")}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-purple-600 hover:text-purple-700 hover:bg-purple-50 rounded-xl border border-purple-200 transition-colors"
            title="Import CSV"
          >
            <Upload className="w-3.5 h-3.5" />
            Import CSV
          </button>
          <button
            onClick={fetchPools}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:text-slate-700 hover:bg-slate-100 rounded-xl border border-slate-200 transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? "animate-spin" : ""}`} />
            Làm mới
          </button>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="flex items-center gap-3 px-4 py-3 rounded-2xl border bg-red-50 border-red-200 text-red-800 shrink-0 animate-in slide-in-from-top-2">
          <AlertCircle className="w-5 h-5 shrink-0" />
          <span className="text-sm font-semibold">{error}</span>
        </div>
      )}

      {/* Summary Stats */}
      {pools.length > 0 && (
        <div className="grid grid-cols-4 gap-3 shrink-0">
          <div className="bg-white rounded-2xl border border-slate-200/60 p-3 text-center">
            <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Số Pool</div>
            <div className="text-2xl font-black text-purple-600">{pools.length}</div>
          </div>
          <div className="bg-white rounded-2xl border border-slate-200/60 p-3 text-center">
            <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Tổng Mã</div>
            <div className="text-2xl font-black text-slate-800">
              {pools.reduce((sum, p) => sum + p.totalCodes, 0).toLocaleString()}
            </div>
          </div>
          <div className="bg-white rounded-2xl border border-slate-200/60 p-3 text-center">
            <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Sẵn Sàng</div>
            <div className="text-2xl font-black text-green-600">
              {pools.reduce((sum, p) => sum + p.availableCodes, 0).toLocaleString()}
            </div>
          </div>
          <div className="bg-white rounded-2xl border border-slate-200/60 p-3 text-center">
            <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">Đã Dùng</div>
            <div className="text-2xl font-black text-blue-600">
              {pools.reduce((sum, p) => sum + p.usedCodes, 0).toLocaleString()}
            </div>
          </div>
        </div>
      )}

      {/* Search */}
      <div className="flex items-center gap-2 bg-white rounded-2xl border border-slate-200 px-3 py-2 shadow-sm shrink-0">
        <Search className="w-4 h-4 text-slate-400" />
        <input
          type="text"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          placeholder="Tìm kiếm theo tên pool..."
          className="flex-1 bg-transparent outline-none text-sm text-slate-700 placeholder:text-slate-400"
        />
        {searchTerm && (
          <button onClick={() => setSearchTerm("")} className="p-1 hover:bg-slate-100 rounded-lg transition-colors">
            <X className="w-3.5 h-3.5 text-slate-400" />
          </button>
        )}
      </div>

      {/* Pool List */}
      <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden">
        <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between">
          <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
            <Database className="w-4 h-4 text-purple-600" /> Danh sách Pool
            <span className="ml-1 text-[10px] font-bold bg-purple-100 text-purple-700 px-2 py-0.5 rounded-full">
              {filteredPools.length}
            </span>
          </h2>
        </div>

        <div className="divide-y divide-slate-100/80">
          {isLoading ? (
            <div className="px-5 py-10 text-center text-slate-400">
              <RefreshCw className="w-6 h-6 mx-auto mb-2 animate-spin" />
              <span className="text-sm">Đang tải danh sách pool...</span>
            </div>
          ) : filteredPools.length === 0 ? (
            <div className="px-5 py-10 text-center text-slate-400">
              <Database className="w-10 h-10 mx-auto mb-3 opacity-30" />
              <span className="text-sm">
                {searchTerm ? "Không tìm thấy pool phù hợp" : "Chưa có pool nào — bấm 'Tạo Pool' để bắt đầu"}
              </span>
            </div>
          ) : (
            filteredPools.map((pool) => (
              <div key={pool.name}>
                {/* Pool Row */}
                <div
                  className="flex items-center gap-3 px-4 xl:px-6 py-3 hover:bg-slate-50/50 transition-colors cursor-pointer group"
                  onClick={() => toggleExpand(pool.name)}
                >
                  {/* Expand arrow */}
                  <div className="shrink-0 text-slate-400">
                    {isLoadingPool === pool.name ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : expandedPool === pool.name ? (
                      <ChevronDown className="w-4 h-4" />
                    ) : (
                      <ChevronRight className="w-4 h-4" />
                    )}
                  </div>

                  {/* Pool icon */}
                  <div className="w-8 h-8 rounded-lg bg-purple-100 flex items-center justify-center shrink-0">
                    <Database className="w-4 h-4 text-purple-600" />
                  </div>

                  {/* Pool info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-sm font-bold text-slate-800">{pool.name}</span>
                      {pool.loaded && pool.totalCodes > 0 && (
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold tracking-wide ${statusColor(pool.availableCodes, pool.totalCodes)}`}>
                          {pool.availableCodes === 0 ? "HẾT MÃ" : `${pool.availableCodes} còn`}
                        </span>
                      )}
                    </div>
                    <div className="text-[11px] text-slate-400 mt-0.5">
                      {pool.fileName} &bull; {(pool.size / 1024).toFixed(1)} KB
                    </div>
                  </div>

                  {/* Stats */}
                  {pool.loaded && (
                    <div className="flex items-center gap-3 shrink-0">
                      <div className="text-right">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-400">Còn</div>
                        <div className="text-sm font-black text-green-600">{pool.availableCodes.toLocaleString()}</div>
                      </div>
                      <div className="text-right">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-400">Đã dùng</div>
                        <div className="text-sm font-black text-slate-500">{pool.usedCodes.toLocaleString()}</div>
                      </div>
                      <div className="text-right">
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-400">Tổng</div>
                        <div className="text-sm font-black text-slate-700">{pool.totalCodes.toLocaleString()}</div>
                      </div>
                      {/* Progress bar */}
                      <div className="w-20 h-2 bg-slate-100 rounded-full overflow-hidden shrink-0">
                        <div
                          className={`h-full rounded-full transition-all ${
                            pool.availableCodes === 0 ? "bg-red-500" :
                            pool.availableCodes / pool.totalCodes > 0.5 ? "bg-green-500" :
                            pool.availableCodes / pool.totalCodes > 0.2 ? "bg-amber-500" : "bg-red-500"
                          }`}
                          style={{ width: `${pool.totalCodes > 0 ? (pool.availableCodes / pool.totalCodes) * 100 : 0}%` }}
                        />
                      </div>
                    </div>
                  )}
                </div>

                {/* Expanded: Code List */}
                {expandedPool === pool.name && (
                  <div className="bg-slate-50/50 border-t border-slate-100 px-4 xl:px-6 py-3">
                    {/* Search codes */}
                    <div className="flex items-center gap-2 mb-3">
                      <div className="flex items-center gap-2 bg-white rounded-xl border border-slate-200 px-3 py-1.5 flex-1">
                        <Search className="w-3.5 h-3.5 text-slate-400" />
                        <input
                          type="text"
                          placeholder="Tìm kiếm mã (Enter để tìm)..."
                          className="flex-1 bg-transparent outline-none text-xs text-slate-700 placeholder:text-slate-400"
                          value={poolSearchTerms[pool.name] || ""}
                          onChange={(e) => setPoolSearchTerms(prev => ({ ...prev, [pool.name]: e.target.value }))}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              searchPoolCodes(pool.name, poolSearchTerms[pool.name] || "");
                            }
                          }}
                        />
                        {poolSearchTerms[pool.name] && (
                          <button
                            onClick={() => {
                              setPoolSearchTerms(prev => ({ ...prev, [pool.name]: "" }));
                              setPoolCodes(prev => ({ ...prev, [pool.name]: [] }));
                              setPoolSearchResults(prev => ({ ...prev, [pool.name]: false }));
                            }}
                            className="p-0.5 hover:bg-slate-100 rounded"
                          >
                            <X className="w-3 h-3 text-slate-400" />
                          </button>
                        )}
                      </div>
                      {poolSearchTerms[pool.name] && (
                        <button
                          onClick={() => searchPoolCodes(pool.name, poolSearchTerms[pool.name] || "")}
                          className="px-3 py-1.5 bg-purple-600 text-white text-xs font-semibold rounded-xl hover:bg-purple-700 transition-colors"
                        >
                          Tìm
                        </button>
                      )}
                    </div>

                    {isLoadingPool === pool.name ? (
                      <div className="text-center text-slate-400 py-4">
                        <RefreshCw className="w-5 h-5 mx-auto animate-spin" />
                      </div>
                    ) : poolSearchResults[pool.name] === true && poolCodes[pool.name] ? (
                      <>
                        <div className="text-[10px] font-bold uppercase tracking-wider text-slate-400 mb-2 flex items-center gap-2">
                          <Tag className="w-3 h-3" />
                          Kết quả tìm kiếm: {poolCodes[pool.name].length} mã
                        </div>
                        <div className="bg-white rounded-xl border border-slate-200/80 overflow-hidden">
                          <table className="w-full text-xs">
                            <thead>
                              <tr className="text-[10px] uppercase text-slate-400 bg-slate-50 border-b border-slate-100">
                                <th className="px-3 py-2 font-bold text-left">Mã</th>
                                <th className="px-3 py-2 font-bold text-center">Trạng thái</th>
                                <th className="px-3 py-2 font-bold text-left">Batch</th>
                                <th className="px-3 py-2 font-bold text-left">Ghi chú</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                              {poolCodes[pool.name].map((code) => (
                                <tr key={code.code} className="hover:bg-slate-50/50">
                                  <td className="px-3 py-2 font-mono font-semibold text-slate-700">{code.code}</td>
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
                                  <td className="px-3 py-2 text-slate-500 truncate max-w-[200px]">{code.note || "—"}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                          {poolCodes[pool.name].length === 0 && (
                            <div className="px-3 py-4 text-center text-xs text-slate-400">
                              Không tìm thấy mã nào chứa "{poolSearchTerms[pool.name]}"
                            </div>
                          )}
                        </div>
                      </>
                    ) : (
                      <div className="text-center text-slate-400 py-4 text-sm">
                        Nhập từ khóa và bấm "Tìm" để tìm mã trong pool
                      </div>
                    )}
                  </div>
                )}
              </div>
            ))
          )}
        </div>
      </div>

      {/* Modals */}
      <Modal
        mode={modalMode}
        onClose={() => setModalMode(null)}
        title={
          modalMode === "create" ? "Tạo Pool mới" :
          modalMode === "addcode" ? "Nhập Code đơn lẻ" :
          modalMode === "import" ? "Import CSV" : ""
        }
      >
        {modalMode === "create" && (
          <CreatePoolForm
            onSuccess={fetchPools}
            onClose={() => setModalMode(null)}
          />
        )}
        {modalMode === "addcode" && (
          <AddCodeForm
            pools={pools}
            onSuccess={fetchPools}
            onClose={() => setModalMode(null)}
          />
        )}
        {modalMode === "import" && (
          <ImportCSVForm
            pools={pools}
            onSuccess={fetchPools}
            onClose={() => setModalMode(null)}
          />
        )}
      </Modal>
    </div>
  );
};

export default DataPoolView;
