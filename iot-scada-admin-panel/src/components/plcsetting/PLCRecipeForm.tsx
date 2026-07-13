import React, { useEffect, useRef, useState } from "react";
import {
  Save,
  RefreshCw,
  Loader2,
  CheckCircle2,
  AlertCircle,
  Cpu,
  Plus,
  X,
  Trash2,
  Star,
  RotateCcw,
  Eye,
  Keyboard as KeyboardIcon,
  CircleDot,
} from "lucide-react";
import plcApi from "../../services/plcApi";
import type { PLCRecipe } from "../../types/plc";
import { useVirtualKeyboard } from "../../App";
import RecipeRegistersEditor from "./RecipeRegistersEditor";

const EMPTY_RECIPE: PLCRecipe = {
  id: 0,
  recipeName: "",
  delayCamera: 1000,
  delayReject: 2000,
  rejectStreng: 20,
  isActive: false,
};

interface Toast {
  type: "ok" | "err";
  text: string;
}

interface LiveValues {
  delayCamera: number | null;
  delayReject: number | null;
  rejectStreng: number | null;
  at: number | null;
  error?: string;
}

const POLL_INTERVAL_MS = 3000;

export const PLCRecipeForm: React.FC = () => {
  const { openKeyboard, isOpen } = useVirtualKeyboard();
  const [allRecipes, setAllRecipes] = useState<PLCRecipe[]>([]);
  const [recipe, setRecipe] = useState<PLCRecipe>(EMPTY_RECIPE);
  const [live, setLive] = useState<LiveValues>({
    delayCamera: null,
    delayReject: null,
    rejectStreng: null,
    at: null,
  });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [settingActive, setSettingActive] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [newName, setNewName] = useState("");
  const [toast, setToast] = useState<Toast | null>(null);
  const pollRef = useRef<number | null>(null);

  const showToast = (type: Toast["type"], text: string) => {
    setToast({ type, text });
    setTimeout(() => setToast(null), 4000);
  };

  const loadAll = async () => {
    setLoading(true);
    try {
      const list = await plcApi.getAllRecipes();
      setAllRecipes(list);
      const active = list.find((r) => r.isActive) || list[0];
      if (active) setRecipe({ ...active });
    } catch (err: any) {
      showToast("err", err?.message || "Không tải được danh sách recipe.");
    } finally {
      setLoading(false);
    }
  };

  // Live polling giá trị từ PLC
  const pollLive = async () => {
    try {
      const r = await plcApi.getRecipeFromPlc();
      if (r) {
        setLive({
          delayCamera: r.delayCamera,
          delayReject: r.delayReject,
          rejectStreng: r.rejectStreng,
          at: Date.now(),
          error: undefined,
        });
      }
    } catch (err: any) {
      setLive((prev) => ({
        ...prev,
        at: Date.now(),
        error: err?.message || "Không đọc được PLC",
      }));
    }
  };

  useEffect(() => {
    loadAll();
    pollLive();
    pollRef.current = window.setInterval(pollLive, POLL_INTERVAL_MS);
    return () => {
      if (pollRef.current) window.clearInterval(pollRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const save = async () => {
    if (!recipe.recipeName.trim()) {
      showToast("err", "Vui lòng nhập tên recipe.");
      return;
    }
    if (
      recipe.delayCamera < 0 ||
      recipe.delayReject < 0 ||
      recipe.rejectStreng < 0 ||
      recipe.rejectStreng > 100
    ) {
      showToast("err", "RejectStreng phải 0-100. Delay phải >= 0.");
      return;
    }
    setSaving(true);
    try {
      await plcApi.saveRecipe({
        ...recipe,
        recipeName: recipe.recipeName.trim(),
        id: recipe.id || 0,
      });
      showToast("ok", "Đã lưu recipe.");
      await loadAll();
    } catch (err: any) {
      showToast("err", err?.message || "Lưu thất bại.");
    } finally {
      setSaving(false);
    }
  };

  const setActive = async () => {
    if (!recipe.id) {
      showToast("err", "Hãy lưu recipe trước khi đặt active.");
      return;
    }
    setSettingActive(true);
    try {
      await plcApi.setActiveRecipe(recipe.id);
      showToast("ok", "Đã đặt active và ghi xuống PLC.");
      await loadAll();
      pollLive();
    } catch (err: any) {
      showToast("err", err?.message || "Đặt active thất bại.");
    } finally {
      setSettingActive(false);
    }
  };

  const deleteRecipe = async () => {
    if (!recipe.id) return;
    if (
      !confirm(
        `Xóa recipe "${recipe.recipeName}"? Hành động này không thể hoàn tác.`,
      )
    )
      return;
    setDeleting(true);
    try {
      await plcApi.deleteRecipe(recipe.id);
      showToast("ok", "Đã xóa.");
      setRecipe(EMPTY_RECIPE);
      await loadAll();
    } catch (err: any) {
      showToast("err", err?.message || "Xóa thất bại.");
    } finally {
      setDeleting(false);
    }
  };

  const createRecipe = async () => {
    if (!newName.trim()) {
      showToast("err", "Nhập tên recipe.");
      return;
    }
    setSaving(true);
    try {
      const r = await plcApi.saveRecipe({
        id: 0,
        recipeName: newName.trim(),
        delayCamera: recipe.delayCamera,
        delayReject: recipe.delayReject,
        rejectStreng: recipe.rejectStreng,
        isActive: false,
      });
      showToast("ok", `Đã tạo recipe "${newName}".`);
      setRecipe(r.data || { ...EMPTY_RECIPE, recipeName: newName.trim() });
      setNewName("");
      setShowCreate(false);
      await loadAll();
    } catch (err: any) {
      showToast("err", err?.message || "Tạo recipe thất bại.");
    } finally {
      setSaving(false);
    }
  };

  const selectRecipe = (id: number) => {
    const found = allRecipes.find((r) => r.id === id);
    if (found) setRecipe({ ...found });
  };

  const isActive = recipe.isActive;
  const isDefault = recipe.recipeName?.toLowerCase() === "default";
  const canDelete = recipe.id && !isDefault && !isActive;

  return (
    <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden flex flex-col h-full">
      {/* Header */}
      <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between shrink-0">
        <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
          <Cpu className="w-4 h-4 text-indigo-600" /> Cấu hình PLC Recipe
          {isActive && (
            <span className="ml-1 text-[10px] font-bold bg-green-100 text-green-700 px-2 py-0.5 rounded-full border border-green-200 flex items-center gap-1">
              <Star className="w-3 h-3" /> ACTIVE
            </span>
          )}
        </h2>
        <div className="flex items-center gap-1.5">
          <button
            onClick={() => {
              loadAll();
              pollLive();
            }}
            disabled={loading}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:text-slate-700 hover:bg-slate-100 rounded-xl border border-slate-200 transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? "animate-spin" : ""}`} />
            Làm mới
          </button>
          <button
            onClick={save}
            disabled={saving}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl transition-colors disabled:opacity-50"
          >
            {saving ? (
              <Loader2 className="w-3.5 h-3.5 animate-spin" />
            ) : (
              <Save className="w-3.5 h-3.5" />
            )}
            Lưu vào DB
          </button>
        </div>
      </div>

      {/* Toast */}
      {toast && (
        <div
          className={`mx-4 mt-3 flex items-center gap-2 px-4 py-2.5 rounded-xl border shrink-0 ${
            toast.type === "ok"
              ? "bg-green-50 border-green-200 text-green-800"
              : "bg-red-50 border-red-200 text-red-800"
          }`}
        >
          {toast.type === "ok" ? (
            <CheckCircle2 className="w-4 h-4 shrink-0" />
          ) : (
            <AlertCircle className="w-4 h-4 shrink-0" />
          )}
          <span className="text-sm font-semibold">{toast.text}</span>
        </div>
      )}

      {/* Body */}
      <div className="p-4 xl:p-6 flex-1 overflow-auto space-y-3">
        {/* Live PLC status */}
        <div className="flex items-center gap-2 text-[10px] uppercase tracking-wider font-bold text-slate-500">
          <CircleDot
            className={`w-3.5 h-3.5 ${live.error ? "text-red-500" : "text-green-500"}`}
          />
          <span>PLC Live</span>
          <span
            className={`font-mono normal-case text-slate-600 ${live.error ? "text-red-600" : ""}`}
          >
            {live.error
              ? `Mất kết nối: ${live.error}`
              : live.at
                ? `Cập nhật ${Math.max(1, Math.floor((Date.now() - live.at) / 1000))}s trước`
                : "Chờ đọc..."}
          </span>
          <button
            onClick={pollLive}
            className="ml-1 p-1 hover:bg-slate-100 rounded-lg text-slate-500"
            title="Đọc ngay"
          >
            <Eye className="w-3 h-3" />
          </button>
        </div>

        {/* Recipe dropdown */}
        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-1.5">
            Chọn Recipe
          </label>
          <div className="flex gap-2">
            <select
              value={recipe.id ?? ""}
              onChange={(e) => selectRecipe(Number(e.target.value))}
              className="flex-1 bg-slate-50 border border-slate-200 rounded-xl px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            >
              <option value="">-- Chọn recipe --</option>
              {allRecipes.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.recipeName} {r.isActive ? "★ ACTIVE" : ""}
                </option>
              ))}
            </select>
            <button
              onClick={() => setShowCreate(true)}
              className="flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-indigo-600 hover:text-indigo-700 hover:bg-indigo-50 rounded-xl border border-indigo-200 transition-colors"
            >
              <Plus className="w-3.5 h-3.5" /> Tạo mới
            </button>
          </div>
          {showCreate && (
            <div className="mt-2 flex items-center gap-2 bg-indigo-50 border border-indigo-200 rounded-xl px-3 py-2">
              <input
                type="text"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
                placeholder="Tên recipe (VD: MyRecipe)"
                className="flex-1 bg-white border border-indigo-200 rounded-lg px-3 py-1.5 text-sm outline-none focus:ring-1 focus:ring-indigo-500"
                onKeyDown={(e) => e.key === "Enter" && createRecipe()}
                autoFocus
              />
              <button
                onClick={createRecipe}
                disabled={saving}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-indigo-600 text-white text-xs font-bold rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50"
              >
                {saving ? <Loader2 className="w-3 h-3 animate-spin" /> : <Plus className="w-3 h-3" />}
                Tạo
              </button>
              <button
                onClick={() => {
                  setShowCreate(false);
                  setNewName("");
                }}
                className="p-1 hover:bg-indigo-100 rounded-lg"
              >
                <X className="w-4 h-4 text-indigo-600" />
              </button>
            </div>
          )}
        </div>

        {/* Name input */}
        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-1.5">
            Tên Recipe
          </label>
          <input
            type="text"
            value={recipe.recipeName}
            onChange={(e) => setRecipe({ ...recipe, recipeName: e.target.value })}
            onFocus={() =>
              openKeyboard(recipe.recipeName, "default", (v) =>
                setRecipe((prev) => ({ ...prev, recipeName: v })),
              )
            }
            disabled={isDefault}
            className="w-full bg-slate-50 border border-slate-200 rounded-xl px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 disabled:opacity-60 disabled:cursor-not-allowed"
          />
          {isDefault && (
            <p className="text-[10px] text-amber-600 mt-1">
              Recipe "Default" không thể đổi tên.
            </p>
          )}
        </div>

        {/* 3 number fields - dual value (Setting + Live from PLC) */}
        <div className="grid grid-cols-1 gap-2">
          <DualNumberField
            label="DelayCamera"
            unit="ms"
            settingValue={recipe.delayCamera}
            liveValue={live.delayCamera}
            color="blue"
            min={0}
            onChangeSetting={(v) =>
              setRecipe((prev) => ({ ...prev, delayCamera: v }))
            }
            openKb={(cur, apply) =>
              openKeyboard(String(cur), "numeric", (s) =>
                apply(Number.isFinite(Number(s)) ? Number(s) : 0),
              )
            }
            kbOpen={isOpen}
          />
          <DualNumberField
            label="DelayReject"
            unit="ms"
            settingValue={recipe.delayReject}
            liveValue={live.delayReject}
            color="amber"
            min={0}
            onChangeSetting={(v) =>
              setRecipe((prev) => ({ ...prev, delayReject: v }))
            }
            openKb={(cur, apply) =>
              openKeyboard(String(cur), "numeric", (s) =>
                apply(Number.isFinite(Number(s)) ? Number(s) : 0),
              )
            }
            kbOpen={isOpen}
          />
          <DualNumberField
            label="RejectStreng"
            unit="0-100"
            settingValue={recipe.rejectStreng}
            liveValue={live.rejectStreng}
            color="purple"
            min={0}
            max={100}
            onChangeSetting={(v) =>
              setRecipe((prev) => ({ ...prev, rejectStreng: v }))
            }
            openKb={(cur, apply) =>
              openKeyboard(String(cur), "numeric", (s) =>
                apply(Number.isFinite(Number(s)) ? Number(s) : 0),
              )
            }
            kbOpen={isOpen}
          />
        </div>

        {/* Custom Registers (per recipe) */}
        <div className="border-t border-slate-100 pt-4 mt-2">
          <RecipeRegistersEditor recipeId={recipe.id ?? null} />
        </div>

        {/* Bottom actions */}
        <div className="pt-4 border-t border-slate-100 flex items-center justify-between flex-wrap gap-2">
          <div className="flex items-center gap-1.5 text-[11px] text-slate-500">
            {recipe.id
              ? `ID: ${recipe.id}`
              : "Recipe mới — bấm Lưu để tạo"}
          </div>
          <div className="flex items-center gap-1.5">
            <button
              onClick={() => setRecipe({ ...EMPTY_RECIPE })}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl border border-slate-200 transition-colors"
              title="Reset form"
            >
              <RotateCcw className="w-3.5 h-3.5" /> Reset
            </button>
            <button
              onClick={setActive}
              disabled={!recipe.id || isActive || settingActive}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold bg-green-600 hover:bg-green-700 text-white rounded-xl transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {settingActive ? (
                <Loader2 className="w-3.5 h-3.5 animate-spin" />
              ) : (
                <Star className="w-3.5 h-3.5" />
              )}
              Đặt active & ghi PLC
            </button>
            <button
              onClick={deleteRecipe}
              disabled={!canDelete || deleting}
              className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold bg-red-600 hover:bg-red-700 text-white rounded-xl transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
              title={
                isDefault
                  ? "Không thể xóa Default"
                  : isActive
                    ? "Không thể xóa recipe active"
                    : "Xóa recipe"
              }
            >
              {deleting ? (
                <Loader2 className="w-3.5 h-3.5 animate-spin" />
              ) : (
                <Trash2 className="w-3.5 h-3.5" />
              )}
              Xóa
            </button>
          </div>
        </div>

        {recipe.id && (
          <p className="text-[10px] text-slate-400">
            Tạo bởi <span className="font-semibold">{recipe.createdBy || "Operator"}</span> • Cập nhật:{" "}
            <span className="font-mono">
              {recipe.updatedAt
                ? new Date(recipe.updatedAt).toLocaleString("vi-VN")
                : "—"}
            </span>
          </p>
        )}
      </div>
    </div>
  );
};

export default PLCRecipeForm;
interface DualNumberFieldProps {
  label: string;
  unit: string;
  settingValue: number;
  liveValue: number | null;
  color: "blue" | "amber" | "purple";
  min?: number;
  max?: number;
  onChangeSetting: (v: number) => void;
  openKb: (
    cur: number,
    apply: (v: number) => void,
  ) => void;
  kbOpen: boolean;
}

const COLOR_RING: Record<DualNumberFieldProps["color"], string> = {
  blue: "focus-within:ring-blue-500/40 border-blue-200 bg-blue-50/40",
  amber: "focus-within:ring-amber-500/40 border-amber-200 bg-amber-50/40",
  purple: "focus-within:ring-purple-500/40 border-purple-200 bg-purple-50/40",
};
const COLOR_TEXT: Record<DualNumberFieldProps["color"], string> = {
  blue: "text-blue-700",
  amber: "text-amber-700",
  purple: "text-purple-700",
};
const COLOR_BUTTON: Record<DualNumberFieldProps["color"], string> = {
  blue: "bg-blue-600 hover:bg-blue-700",
  amber: "bg-amber-600 hover:bg-amber-700",
  purple: "bg-purple-600 hover:bg-purple-700",
};

const DualNumberField: React.FC<DualNumberFieldProps> = ({
  label,
  unit,
  settingValue,
  liveValue,
  color,
  min,
  max,
  onChangeSetting,
  openKb,
  kbOpen,
}) => {
  const handleKb = () => openKb(settingValue, onChangeSetting);

  // Đánh dấu chênh lệch giữa setting vs live
  const differs =
    liveValue !== null &&
    liveValue !== undefined &&
    settingValue !== liveValue;

  const matchColor = differs ? "text-amber-600" : "text-green-600";
  const matchBg = differs ? "bg-amber-50 border-amber-200" : "bg-green-50 border-green-200";

  return (
    <div className={`rounded-xl border ${COLOR_RING[color]} p-2 ring-1 ring-transparent transition-all`}>
      {/* Header */}
      <div className="flex items-center justify-between mb-1">
        <label className={`text-[11px] font-bold ${COLOR_TEXT[color]} flex items-center gap-1.5`}>
          <span>{label}</span>
          <span className="text-[9px] font-mono text-slate-400 font-normal">({unit})</span>
        </label>
        {liveValue !== null && liveValue !== undefined && (
          <span
            className={`text-[9px] font-bold uppercase tracking-wider px-1.5 py-0 rounded-full border ${matchBg} ${matchColor}`}
            title={differs ? "Giá trị cài đặt khác giá trị PLC" : "Khớp"}
          >
            {differs ? "Lệch" : "Khớp"}
          </span>
        )}
      </div>

      {/* Two columns */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-1.5">
        {/* Setting */}
        <button
          type="button"
          onClick={handleKb}
          className={`text-left rounded-md px-2 py-1 border-2 transition-all ${
            kbOpen
              ? "border-indigo-500 bg-indigo-50 ring-2 ring-indigo-500/30"
              : "border-slate-200 bg-white hover:border-slate-300"
          }`}
        >
          <div className="flex items-center justify-between gap-1 mb-0">
            <span className="text-[8px] font-bold uppercase tracking-wider text-slate-500">
              Cài đặt
            </span>
            <KeyboardIcon className="w-2.5 h-2.5 text-slate-400" />
          </div>
          <div className="text-base font-black text-slate-800 font-mono leading-tight">
            {settingValue}
          </div>
        </button>

        {/* Live from PLC */}
        <div className="rounded-md px-2 py-1 border-2 border-slate-200 bg-slate-50">
          <div className="flex items-center justify-between gap-1 mb-0">
            <span className="text-[8px] font-bold uppercase tracking-wider text-slate-500">
              Đọc từ PLC
            </span>
            <Eye className="w-2.5 h-2.5 text-slate-400" />
          </div>
          <div
            className={`text-base font-black font-mono leading-tight ${
              liveValue === null ? "text-slate-300" : "text-slate-700"
            }`}
          >
            {liveValue === null ? "—" : liveValue}
          </div>
          {min !== undefined || max !== undefined ? (
            <div className="text-[8px] text-slate-400 mt-0">
              {min !== undefined ? `min ${min}` : ""}
              {min !== undefined && max !== undefined ? " • " : ""}
              {max !== undefined ? `max ${max}` : ""}
            </div>
          ) : null}
        </div>
      </div>

      {/* Sync hint */}
      {differs && liveValue !== null && (
        <div className="mt-1 flex items-center justify-between gap-1 text-[9px] text-amber-700 bg-amber-50 border border-amber-200 rounded-md px-2 py-1">
          <span>
            Đã lưu DB nhưng chưa ghi xuống PLC. Bấm{" "}
            <span className="font-bold">"Đặt active & ghi PLC"</span> hoặc "Lưu vào DB" (nếu đang active).
          </span>
        </div>
      )}

      {/* Quick adjust row */}
      <div className="mt-1 flex items-center justify-between gap-1">
        <div className="flex items-center gap-0.5">
          <button
            type="button"
            onClick={() =>
              onChangeSetting(Math.max(min ?? -Infinity, settingValue - 10))
            }
            className={`text-[10px] font-bold w-7 h-6 rounded text-white ${COLOR_BUTTON[color]} transition-colors`}
            title="Giảm 10"
          >
            -10
          </button>
          <button
            type="button"
            onClick={() => onChangeSetting(settingValue - 1)}
            className="text-[11px] font-bold w-6 h-6 rounded bg-white border border-slate-300 hover:bg-slate-100 transition-colors"
            title="Giảm 1"
          >
            -
          </button>
          <button
            type="button"
            onClick={() => onChangeSetting(settingValue + 1)}
            className="text-[11px] font-bold w-6 h-6 rounded bg-white border border-slate-300 hover:bg-slate-100 transition-colors"
            title="Tăng 1"
          >
            +
          </button>
          <button
            type="button"
            onClick={() =>
              onChangeSetting(
                Math.min(max ?? Infinity, settingValue + 10),
              )
            }
            className={`text-[10px] font-bold w-7 h-6 rounded text-white ${COLOR_BUTTON[color]} transition-colors`}
            title="Tăng 10"
          >
            +10
          </button>
        </div>
        <button
          type="button"
          onClick={handleKb}
          className="flex items-center gap-0.5 text-[9px] font-semibold text-slate-500 hover:text-indigo-600 transition-colors"
        >
          <KeyboardIcon className="w-2.5 h-2.5" /> Bàn phím số
        </button>
      </div>
    </div>
  );
};

