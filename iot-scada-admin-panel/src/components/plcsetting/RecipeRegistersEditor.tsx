import React, { useCallback, useEffect, useState } from "react";
import {
  Plus,
  Trash2,
  Save,
  Loader2,
  Eye,
  Send,
  AlertCircle,
  CheckCircle2,
  X,
  Edit3,
  Settings2,
  ChevronRight,
  ChevronDown,
} from "lucide-react";
import plcApi from "../../services/plcApi";
import type {
  PlcDataType,
  RecipeRegister,
  RecipeRegisterLive,
} from "../../types/plc";
import { useVirtualKeyboard } from "../../hooks/useVirtualKeyboard";

interface Props {
  recipeId: number | null;
  /** Bật/tắt polling live PLC cho các ô này */
  pollIntervalMs?: number;
}

interface Toast {
  type: "ok" | "err";
  text: string;
}

const DATA_TYPES: PlcDataType[] = ["int16", "int32", "float", "string"];
const POLL_INTERVAL = 3000;

const newRow = (order: number): RecipeRegister => ({
  name: "",
  address: `D${400 + order * 10}`,
  dataType: "int32",
  defaultValue: "0",
  unit: "",
  note: "",
  sortOrder: order,
});

const RecipeRegistersEditor: React.FC<Props> = ({ recipeId, pollIntervalMs = POLL_INTERVAL }) => {
  const { openKeyboard } = useVirtualKeyboard();
  const [rows, setRows] = useState<RecipeRegister[]>([]);
  const [live, setLive] = useState<Record<number, RecipeRegisterLive>>({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [reading, setReading] = useState(false);
  const [writing, setWriting] = useState(false);
  const [pollOn, setPollOn] = useState(true);
  const [toast, setToast] = useState<Toast | null>(null);
  const [editingNameId, setEditingNameId] = useState<number | null>(null);
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());

  const toggleRow = (idx: number) => {
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(idx)) next.delete(idx);
      else next.add(idx);
      return next;
    });
  };

  const showToast = (type: Toast["type"], text: string) => {
    setToast({ type, text });
    setTimeout(() => setToast(null), 4000);
  };

  /* ===== Load + auto reload when recipeId changes ===== */
  useEffect(() => {
    if (!recipeId) {
      setRows([]);
      setLive({});
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const list = await plcApi.getRegisters(recipeId);
        if (!cancelled) {
          setRows(list.length ? list : [newRow(0)]);
        }
      } catch (err: any) {
        if (!cancelled) showToast("err", err?.message || "Không tải được registers.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [recipeId]);

  /* ===== Live polling ===== */
  const refreshLive = useCallback(async () => {
    if (!recipeId || rows.length === 0) return;
    try {
      const r = await plcApi.readRegistersFromPlc(recipeId);
      const map: Record<number, RecipeRegisterLive> = {};
      (r.data || []).forEach((d) => (map[d.id] = d));
      setLive(map);
    } catch (err: any) {
      // Surface error on first row so user sees something
      setLive((prev) => ({
        ...prev,
        __error: { id: -1, name: "PLC", address: "", dataType: "int32", ok: false, value: "", error: err?.message || "Lỗi" } as any,
      }));
    }
  }, [recipeId, rows.length]);

  useEffect(() => {
    if (!pollOn || !recipeId || rows.length === 0) return;
    refreshLive();
    const id = window.setInterval(refreshLive, pollIntervalMs);
    return () => window.clearInterval(id);
  }, [pollOn, recipeId, pollIntervalMs, refreshLive, rows.length]);

  /* ===== Row ops ===== */
  const addRow = () => {
    const order = rows.length;
    setRows((r) => [...r, newRow(order)]);
  };

  const removeRow = (idx: number) => {
    setRows((r) => r.filter((_, i) => i !== idx));
  };

  const updateRow = (idx: number, patch: Partial<RecipeRegister>) => {
    setRows((r) =>
      r.map((row, i) => (i === idx ? { ...row, ...patch } : row)),
    );
  };

  /* ===== Save / Read / Write ===== */
  const save = async () => {
    if (!recipeId) {
      showToast("err", "Chọn recipe trước.");
      return;
    }
    // Validate
    for (let i = 0; i < rows.length; i++) {
      const r = rows[i];
      if (!r.name.trim()) {
        showToast("err", `Hàng ${i + 1}: chưa nhập Tên.`);
        return;
      }
      if (!r.address.trim()) {
        showToast("err", `Hàng ${i + 1}: chưa nhập Ô nhớ (address).`);
        return;
      }
    }
    if (rows.length === 0) {
      showToast("err", "Chưa có thanh ghi nào.");
      return;
    }
    setSaving(true);
    try {
      const result = await plcApi.saveRegisters(recipeId, rows);
      setRows(result.data ?? []);
      showToast("ok", `Đã lưu ${result.data?.length ?? 0} thanh ghi vào DB.`);
    } catch (err: any) {
      showToast("err", err?.message || "Lưu thất bại.");
    } finally {
      setSaving(false);
    }
  };

  const readNow = async () => {
    if (!recipeId) return;
    setReading(true);
    try {
      const r = await plcApi.readRegistersFromPlc(recipeId);
      const map: Record<number, RecipeRegisterLive> = {};
      (r.data || []).forEach((d) => (map[d.id] = d));
      setLive(map);
      const okCount = (r.data || []).filter((d) => d.ok).length;
      showToast("ok", `Đọc ${okCount}/${(r.data || []).length} ô từ PLC.`);
    } catch (err: any) {
      showToast("err", err?.message || "Không đọc được PLC.");
    } finally {
      setReading(false);
    }
  };

  const writeToPlc = async () => {
    if (!recipeId || rows.length === 0) return;
    setWriting(true);
    try {
      const values: Record<string, string> = {};
      rows.forEach((r) => {
        if (r.id) values[r.id.toString()] = r.defaultValue;
      });
      const r = await plcApi.writeRegistersToPlc(recipeId, values);
      showToast(
        r.success ? "ok" : "err",
        r.message || (r.success ? "Ghi xong." : "Có lỗi."),
      );
      // Cập nhật ô live ngay
      const map: Record<number, RecipeRegisterLive> = {};
      (r.data || []).forEach((d: any) => (map[d.id] = d));
      setLive(map);
    } catch (err: any) {
      showToast("err", err?.message || "Ghi thất bại.");
    } finally {
      setWriting(false);
    }
  };

  if (!recipeId) {
    return (
      <div className="rounded-2xl border border-slate-200/60 bg-white p-4 text-sm text-slate-500">
        Chọn recipe ở trên trước khi cấu hình các ô đọc/ghi.
      </div>
    );
  }

  return (
    <div className="rounded-2xl border border-slate-200/60 bg-white shadow-sm">
      {/* Header */}
      <div className="bg-gradient-to-r from-slate-50 to-indigo-50/40 border-b border-slate-100 px-4 py-3 flex items-center justify-between gap-2">
        <h3 className="text-[12px] font-bold uppercase tracking-wider text-slate-700 flex items-center gap-2">
          <Settings2 className="w-4 h-4 text-indigo-600" />
          Custom Registers (PLC)
          <span className="text-[10px] font-bold bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded-full">
            {rows.length}
          </span>
        </h3>
        <div className="flex items-center gap-1">
          <label className="flex items-center gap-1 text-[10px] text-slate-500 mr-2 select-none cursor-pointer">
            <input
              type="checkbox"
              checked={pollOn}
              onChange={(e) => setPollOn(e.target.checked)}
              className="w-3 h-3 accent-indigo-600"
            />
            Live
          </label>
          <button
            onClick={readNow}
            disabled={reading}
            className="flex items-center gap-1 px-2 py-1 text-[11px] font-semibold text-slate-700 bg-white hover:bg-slate-50 rounded-lg border border-slate-200 transition-colors"
          >
            {reading ? <Loader2 className="w-3 h-3 animate-spin" /> : <Eye className="w-3 h-3" />}
            Đọc
          </button>
          <button
            onClick={writeToPlc}
            disabled={writing || rows.length === 0}
            className="flex items-center gap-1 px-2 py-1 text-[11px] font-semibold text-white bg-green-600 hover:bg-green-700 rounded-lg transition-colors disabled:opacity-40"
          >
            {writing ? <Loader2 className="w-3 h-3 animate-spin" /> : <Send className="w-3 h-3" />}
            Ghi PLC
          </button>
          <button
            onClick={save}
            disabled={saving}
            className="flex items-center gap-1 px-2 py-1 text-[11px] font-bold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg transition-colors disabled:opacity-40"
          >
            {saving ? <Loader2 className="w-3 h-3 animate-spin" /> : <Save className="w-3 h-3" />}
            Lưu DB
          </button>
        </div>
      </div>

      {/* Toast */}
      {toast && (
        <div
          className={`mx-4 mt-3 flex items-center gap-2 px-3 py-2 rounded-lg border text-xs ${
            toast.type === "ok"
              ? "bg-green-50 border-green-200 text-green-800"
              : "bg-red-50 border-red-200 text-red-800"
          }`}
        >
          {toast.type === "ok" ? <CheckCircle2 className="w-3.5 h-3.5" /> : <AlertCircle className="w-3.5 h-3.5" />}
          <span className="font-semibold">{toast.text}</span>
        </div>
      )}

      {/* Empty state */}
      {rows.length === 0 && (
        <div className="px-4 py-10 text-center text-slate-400">
          <Settings2 className="w-10 h-10 mx-auto mb-3 opacity-30" />
          <p className="text-sm mb-4">Recipe này chưa có ô đọc/ghi nào.</p>
          <button
            onClick={() => setRows([newRow(0)])}
            className="inline-flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-bold rounded-xl"
          >
            <Plus className="w-4 h-4" /> Thêm thanh ghi đầu tiên
          </button>
        </div>
      )}

      {/* Table */}
      {rows.length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="bg-slate-50/70 text-[10px] uppercase tracking-wider text-slate-500 border-b border-slate-100">
                <th className="w-8 px-2 py-2"></th>
                <th className="px-3 py-2 text-left font-bold w-8">#</th>
                <th className="px-3 py-2 text-left font-bold min-w-[180px] xl:min-w-[220px]">
                  Tên
                </th>
                <th className="px-3 py-2 text-left font-bold">Giá trị cài</th>
                <th className="px-3 py-2 text-left font-bold">Đơn vị</th>
                <th className="px-3 py-2 text-center font-bold min-w-[120px]">
                  Đọc từ PLC
                </th>
                <th className="px-3 py-2"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {rows.map((row, idx) => {
                const liveVal = row.id != null ? live[row.id] : undefined;
                const diff =
                  liveVal?.ok &&
                  String(liveVal.value) !== String(row.defaultValue);
                const isExpanded = expandedRows.has(idx);

                return (
                  <React.Fragment key={idx}>
                    <tr className="hover:bg-slate-50/50 transition-colors">
                      {/* Chevron toggle */}
                      <td className="px-2 py-2">
                        <button
                          onClick={() => toggleRow(idx)}
                          className="p-1 rounded-md hover:bg-indigo-50 text-slate-400 hover:text-indigo-600 transition-colors"
                          title={isExpanded ? "Ẩn chi tiết" : "Hiện chi tiết"}
                        >
                          {isExpanded ? (
                            <ChevronDown className="w-3.5 h-3.5" />
                          ) : (
                            <ChevronRight className="w-3.5 h-3.5" />
                          )}
                        </button>
                      </td>

                      {/* Index */}
                      <td className="px-3 py-2 text-slate-400 font-mono">
                        {idx + 1}
                      </td>

                      {/* Name */}
                      <td className="px-3 py-2 min-w-[180px] xl:min-w-[220px]">
                        <CellInput
                          value={row.name}
                          onChange={(v) => updateRow(idx, { name: v })}
                          onOpenKb={() =>
                            openKb(row.name, "default", (v) =>
                              updateRow(idx, { name: v }),
                            )
                          }
                          editing={editingNameId === idx}
                          setEditing={(b) => setEditingNameId(b ? idx : null)}
                          placeholder="VD: DelayA"
                          className="font-semibold text-slate-800"
                        />
                      </td>

                      {/* Default value */}
                      <td className="px-3 py-2">
                        <CellInput
                          value={row.defaultValue}
                          onChange={(v) => updateRow(idx, { defaultValue: v })}
                          onOpenKb={() =>
                            openKb(
                              row.defaultValue,
                              row.dataType === "string" ? "default" : "numeric",
                              (v) => updateRow(idx, { defaultValue: v }),
                            )
                          }
                          className="font-mono font-bold text-indigo-700 text-sm"
                          placeholder="0"
                        />
                      </td>

                      {/* Unit */}
                      <td className="px-3 py-2">
                        <CellInput
                          value={row.unit}
                          onChange={(v) => updateRow(idx, { unit: v })}
                          onOpenKb={() =>
                            openKb(row.unit, "default", (v) =>
                              updateRow(idx, { unit: v }),
                            )
                          }
                          placeholder="ms"
                          className="text-slate-500"
                        />
                      </td>

                      {/* Live */}
                      <td className="px-3 py-2 text-center min-w-[120px]">
                        {liveVal ? (
                          liveVal.ok ? (
                            <div className="flex flex-col items-center">
                              <span
                                className={`font-mono font-bold ${
                                  diff ? "text-amber-600" : "text-green-700"
                                }`}
                              >
                                {liveVal.value || "(trống)"}
                              </span>
                              <span
                                className={`text-[9px] font-bold ${
                                  diff ? "text-amber-500" : "text-green-500"
                                }`}
                              >
                                {diff ? "LỆCH" : "KHỚP"}
                              </span>
                            </div>
                          ) : (
                            <div className="text-red-500 text-[10px]" title={liveVal.error}>
                              ERR
                              <X className="w-3 h-3 inline ml-0.5" />
                            </div>
                          )
                        ) : (
                          <span className="text-slate-300 text-[10px]">—</span>
                        )}
                      </td>

                      {/* Delete */}
                      <td className="px-3 py-2 text-right">
                        <button
                          onClick={() => removeRow(idx)}
                          className="p-1 hover:bg-red-50 text-slate-400 hover:text-red-600 rounded-md transition-colors"
                          title="Xóa hàng"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </td>
                    </tr>

                    {/* Expanded details row */}
                    {isExpanded && (
                      <tr className="bg-slate-50/40 border-b border-slate-100">
                        <td colSpan={7} className="px-3 py-3">
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-3 pl-8">
                            <div>
                              <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
                                Ô nhớ (Address)
                              </label>
                              <CellInput
                                value={row.address}
                                onChange={(v) => updateRow(idx, { address: v })}
                                onOpenKb={() =>
                                  openKb(row.address, "default", (v) =>
                                    updateRow(idx, { address: v }),
                                  )
                                }
                                placeholder="D100"
                                className="font-mono text-slate-600"
                              />
                            </div>
                            <div>
                              <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
                                Kiểu dữ liệu
                              </label>
                              <select
                                value={row.dataType}
                                onChange={(e) =>
                                  updateRow(idx, {
                                    dataType: e.target.value as PlcDataType,
                                  })
                                }
                                className="w-full bg-white border border-slate-200 rounded-md px-2 py-1 text-xs outline-none focus:ring-1 focus:ring-indigo-500"
                              >
                                {DATA_TYPES.map((t) => (
                                  <option key={t} value={t}>
                                    {t}
                                  </option>
                                ))}
                              </select>
                            </div>
                            <div>
                              <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
                                Ghi chú
                              </label>
                              <CellInput
                                value={row.note}
                                onChange={(v) => updateRow(idx, { note: v })}
                                onOpenKb={() =>
                                  openKb(row.note, "default", (v) =>
                                    updateRow(idx, { note: v }),
                                  )
                                }
                                placeholder="..."
                                className="text-slate-500 italic"
                              />
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* Footer */}
      {rows.length > 0 && (
        <div className="px-4 py-2.5 bg-slate-50/60 border-t border-slate-100 flex items-center justify-between">
          <button
            onClick={addRow}
            className="flex items-center gap-1.5 px-3 py-1.5 text-[11px] font-bold text-indigo-600 hover:bg-indigo-50 border border-indigo-200 rounded-lg transition-colors"
          >
            <Plus className="w-3.5 h-3.5" /> Thêm hàng
          </button>
          <span className="text-[10px] text-slate-400">
            Click ô bất kỳ để mở bàn phím cảm ứng
          </span>
        </div>
      )}
    </div>
  );

  function openKb(current: string, layout: "default" | "numeric", apply: (v: string) => void) {
    openKeyboard(current, layout, apply);
  }
};

/* ===== Inline editable cell with keyboard support ===== */
const CellInput: React.FC<{
  value: string;
  onChange: (v: string) => void;
  onOpenKb: () => void;
  placeholder?: string;
  className?: string;
  editing?: boolean;
  setEditing?: (b: boolean) => void;
}> = ({ value, onChange, onOpenKb, placeholder, className, editing, setEditing }) => {
  return (
    <div className="relative flex items-center gap-1 group">
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onFocus={() => setEditing?.(true)}
        onBlur={() => setEditing?.(false)}
        onClick={onOpenKb}
        placeholder={placeholder}
        className={`w-full bg-transparent border-b border-transparent hover:border-slate-200 focus:border-indigo-400 focus:bg-white px-1 py-0.5 text-xs outline-none transition-all ${className || ""}`}
      />
      <button
        type="button"
        onClick={onOpenKb}
        className={`p-0.5 opacity-0 group-hover:opacity-100 hover:bg-slate-200 rounded text-slate-400 hover:text-indigo-600 transition-opacity ${
          editing ? "opacity-100" : ""}`}
        title="Mở bàn phím"
      >
        <Edit3 className="w-3 h-3" />
      </button>
    </div>
  );
};

export default RecipeRegistersEditor;
