import React, { useEffect, useState } from "react";
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
} from "lucide-react";
import plcApi from "../../services/plcApi";
import type { PLCRecipe } from "../../types/plc";
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

export const PLCRecipeForm: React.FC = () => {
  const [allRecipes, setAllRecipes] = useState<PLCRecipe[]>([]);
  const [recipe, setRecipe] = useState<PLCRecipe>(EMPTY_RECIPE);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [settingActive, setSettingActive] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [newName, setNewName] = useState("");
  const [toast, setToast] = useState<Toast | null>(null);

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

  useEffect(() => {
    loadAll();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const save = async () => {
    if (!recipe.recipeName.trim()) {
      showToast("err", "Vui lòng nhập tên recipe.");
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
      <div className="p-4 xl:p-6 flex-1 flex flex-col gap-4 min-h-0">
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
            disabled={isDefault}
            className="w-full bg-slate-50 border border-slate-200 rounded-xl px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 disabled:opacity-60 disabled:cursor-not-allowed"
          />
          {isDefault && (
            <p className="text-[10px] text-amber-600 mt-1">
              Recipe "Default" không thể đổi tên.
            </p>
          )}
        </div>

        {/* Custom Registers (per recipe) - scrollable */}
        <div className="border-t border-slate-100 pt-4 flex-1 min-h-0 overflow-auto scrollbar-hide">
          <RecipeRegistersEditor recipeId={recipe.id ?? null} />
        </div>

        {/* Bottom actions */}
        <div className="pt-4 border-t border-slate-100 flex items-center justify-between gap-3 shrink-0">
          <div className="flex items-center gap-2 text-[11px] text-slate-500">
            {recipe.id ? (
              <>
                <span className="font-mono bg-slate-100 px-2 py-1 rounded-lg">ID: {recipe.id}</span>
                <span className="text-slate-300">|</span>
                <span className="text-slate-400">{recipe.recipeName}</span>
              </>
            ) : (
              <span className="text-amber-600">Recipe mới — bấm Lưu để tạo</span>
            )}
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={deleteRecipe}
              disabled={!canDelete || deleting}
              className="flex items-center gap-1.5 px-3 py-2 text-xs font-bold bg-red-500 hover:bg-red-600 text-white rounded-xl transition-all shadow-sm shadow-red-500/20 disabled:opacity-30 disabled:shadow-none"
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
      </div>
    </div>
  );
};

export default PLCRecipeForm;