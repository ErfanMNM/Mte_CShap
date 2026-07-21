import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  ScrollText,
  RefreshCw,
  Search,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  Activity,
  Users,
  Play,
  Pause,
  AlertCircle,
  Loader2,
  X,
  FileText,
} from "lucide-react";
import logsApi from "../../services/logsApi";
import type {
  LogEntry,
  LogLevel,
  LogsPage,
  LogsQueryParams,
} from "../../types/logs";
import { LogDetailModal } from "./LogDetailModal";

const ALL_LEVELS: LogLevel[] = [
  "Verbose",
  "Debug",
  "Information",
  "Warning",
  "Error",
  "Fatal",
];

const LAST_SEEN_KEY = "gproject_last_log_timestamp";

type TabKey = "backend" | "user";

interface FilterState {
  level: string;
  tag: string;
  q: string;
  from: string;
  to: string;
}

const EMPTY_FILTER: FilterState = {
  level: "",
  tag: "",
  q: "",
  from: "",
  to: "",
};

const USER_ACTIVITY_HINT_TAGS = ["Auth", "POApiServer", "Main", "GProjectApiServer"];

function levelBadge(level: string): { bg: string; text: string; label: string } {
  const v = (level || "").toLowerCase();
  if (v === "error" || v === "fatal")
    return {
      bg: "bg-red-50 border-red-200",
      text: "text-red-700",
      label: level.toUpperCase(),
    };
  if (v === "warning")
    return {
      bg: "bg-amber-50 border-amber-200",
      text: "text-amber-700",
      label: level.toUpperCase(),
    };
  if (v === "information")
    return {
      bg: "bg-blue-50 border-blue-200",
      text: "text-blue-700",
      label: "INFO",
    };
  if (v === "debug")
    return {
      bg: "bg-slate-50 border-slate-200",
      text: "text-slate-600",
      label: "DEBUG",
    };
  return {
    bg: "bg-slate-50 border-slate-200",
    text: "text-slate-500",
    label: level.toUpperCase(),
  };
}

function formatTime(ts: string): string {
  try {
    const d = new Date(ts);
    return d.toLocaleString();
  } catch {
    return ts;
  }
}

function extractTagFromMessage(msg: string): string | null {
  const m = /^\[([^\[\]:]+)\]/.exec(msg);
  return m ? m[1] : null;
}

function stripTag(msg: string): string {
  return msg.replace(/^\[[^\[\]:]+\]\s*/, "");
}

interface FilterBarProps {
  filters: FilterState;
  setFilters: (f: FilterState) => void;
  levels: string[];
  tags: string[];
  onRefresh: () => void;
  loading: boolean;
}

const FilterBar: React.FC<FilterBarProps> = ({
  filters,
  setFilters,
  levels,
  tags,
  onRefresh,
  loading,
}) => {
  const update = (patch: Partial<FilterState>) =>
    setFilters({ ...filters, ...patch });

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-12 gap-3 p-4 bg-slate-50/80 border-b border-slate-100">
      <div className="xl:col-span-3">
        <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
          Tìm trong message
        </label>
        <div className="relative">
          <Search className="w-3.5 h-3.5 absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={filters.q}
            onChange={(e) => update({ q: e.target.value })}
            placeholder="vd: StateMachine, POtest2..."
            className="w-full pl-8 pr-2 py-1.5 text-sm bg-white border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-200 focus:border-blue-400"
          />
        </div>
      </div>

      <div className="xl:col-span-2">
        <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
          Level
        </label>
        <select
          value={filters.level}
          onChange={(e) => update({ level: e.target.value })}
          className="w-full px-2 py-1.5 text-sm bg-white border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-200 focus:border-blue-400"
        >
          <option value="">Tất cả</option>
          {levels.length === 0
            ? ALL_LEVELS.map((lv) => (
                <option key={lv} value={lv}>
                  {lv}
                </option>
              ))
            : levels.map((lv) => (
                <option key={lv} value={lv}>
                  {lv}
                </option>
              ))}
        </select>
      </div>

      <div className="xl:col-span-2">
        <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
          Tag
        </label>
        <select
          value={filters.tag}
          onChange={(e) => update({ tag: e.target.value })}
          className="w-full px-2 py-1.5 text-sm bg-white border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-200 focus:border-blue-400"
        >
          <option value="">Tất cả</option>
          {tags.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </select>
      </div>

      <div className="xl:col-span-2">
        <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
          Từ
        </label>
        <input
          type="datetime-local"
          value={filters.from}
          onChange={(e) => update({ from: e.target.value })}
          className="w-full px-2 py-1.5 text-sm bg-white border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-200 focus:border-blue-400"
        />
      </div>

      <div className="xl:col-span-2">
        <label className="block text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
          Đến
        </label>
        <input
          type="datetime-local"
          value={filters.to}
          onChange={(e) => update({ to: e.target.value })}
          className="w-full px-2 py-1.5 text-sm bg-white border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-200 focus:border-blue-400"
        />
      </div>

      <div className="xl:col-span-1 flex items-end">
        <button
          onClick={onRefresh}
          disabled={loading}
          className="w-full inline-flex items-center justify-center gap-1.5 px-3 py-1.5 text-xs font-bold rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          {loading ? (
            <Loader2 className="w-3.5 h-3.5 animate-spin" />
          ) : (
            <RefreshCw className="w-3.5 h-3.5" />
          )}
          Tải lại
        </button>
      </div>
    </div>
  );
};

interface LogsTableProps {
  entries: LogEntry[];
  onRowClick: (entry: LogEntry) => void;
  loading: boolean;
  highlightNew?: boolean;
  newIds?: Set<number>;
}

const LogsTable: React.FC<LogsTableProps> = ({
  entries,
  onRowClick,
  loading,
  highlightNew,
  newIds,
}) => {
  if (loading && entries.length === 0) {
    return (
      <div className="flex items-center justify-center py-16 text-slate-400">
        <Loader2 className="w-5 h-5 animate-spin mr-2" />
        Đang tải...
      </div>
    );
  }
  if (entries.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16 text-slate-400">
        <FileText className="w-10 h-10 mb-2 text-slate-300" />
        <span className="text-sm">Không có log nào khớp bộ lọc.</span>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-xs">
        <thead className="bg-slate-50 border-b border-slate-200 text-slate-600 sticky top-0 z-[1]">
          <tr>
            <th className="px-3 py-2 text-left font-bold uppercase tracking-wider w-44">
              Thời gian
            </th>
            <th className="px-3 py-2 text-left font-bold uppercase tracking-wider w-24">
              Level
            </th>
            <th className="px-3 py-2 text-left font-bold uppercase tracking-wider w-32">
              Tag
            </th>
            <th className="px-3 py-2 text-left font-bold uppercase tracking-wider">
              Message
            </th>
            <th className="px-3 py-2 text-left font-bold uppercase tracking-wider w-16">
              Thread
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100/80">
          {entries.map((e) => {
            const badge = levelBadge(e.level || e.levelName);
            const tag = extractTagFromMessage(e.message);
            const isNew = highlightNew && newIds?.has(e.id);
            return (
              <tr
                key={e.id}
                onClick={() => onRowClick(e)}
                className={`hover:bg-blue-50/40 cursor-pointer transition-colors ${
                  isNew ? "bg-emerald-50/60" : ""
                }`}
              >
                <td className="px-3 py-2 font-mono text-[11px] text-slate-600 whitespace-nowrap">
                  {formatTime(e.timestamp)}
                </td>
                <td className="px-3 py-2">
                  <span
                    className={`inline-flex items-center px-1.5 py-0.5 rounded-md border text-[10px] font-bold ${badge.bg} ${badge.text}`}
                  >
                    {badge.label}
                  </span>
                </td>
                <td className="px-3 py-2 text-slate-500 font-mono text-[11px]">
                  {tag ?? "—"}
                </td>
                <td className="px-3 py-2 text-slate-800 font-mono text-[11px] break-words max-w-0">
                  {stripTag(e.message)}
                </td>
                <td className="px-3 py-2 text-slate-400 font-mono text-[11px]">
                  {e.threadId ?? "—"}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};

interface PaginationProps {
  page: LogsPage | null;
  onChange: (page: number) => void;
  disabled?: boolean;
}

const Pagination: React.FC<PaginationProps> = ({ page, onChange, disabled }) => {
  if (!page) return null;
  const { page: p, totalPages, totalCount, hasNext, hasPrev } = page;
  return (
    <div className="flex items-center justify-between px-4 py-3 border-t border-slate-100 text-xs">
      <div className="text-slate-500">
        Tổng <span className="font-bold text-slate-800">{totalCount}</span> bản
        ghi · Trang <span className="font-bold text-slate-800">{p}</span> /{" "}
        <span className="font-bold text-slate-800">{totalPages || 1}</span>
      </div>
      <div className="flex items-center gap-1">
        <button
          onClick={() => onChange(1)}
          disabled={disabled || !hasPrev}
          className="p-1.5 rounded-lg hover:bg-slate-100 disabled:opacity-30 disabled:hover:bg-transparent text-slate-600"
          title="Trang đầu"
        >
          <ChevronsLeft className="w-4 h-4" />
        </button>
        <button
          onClick={() => onChange(p - 1)}
          disabled={disabled || !hasPrev}
          className="p-1.5 rounded-lg hover:bg-slate-100 disabled:opacity-30 disabled:hover:bg-transparent text-slate-600"
          title="Trang trước"
        >
          <ChevronLeft className="w-4 h-4" />
        </button>
        <button
          onClick={() => onChange(p + 1)}
          disabled={disabled || !hasNext}
          className="p-1.5 rounded-lg hover:bg-slate-100 disabled:opacity-30 disabled:hover:bg-transparent text-slate-600"
          title="Trang sau"
        >
          <ChevronRight className="w-4 h-4" />
        </button>
        <button
          onClick={() => onChange(totalPages)}
          disabled={disabled || !hasNext}
          className="p-1.5 rounded-lg hover:bg-slate-100 disabled:opacity-30 disabled:hover:bg-transparent text-slate-600"
          title="Trang cuối"
        >
          <ChevronsRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
};

export const LogsView: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabKey>("backend");
  const [filters, setFilters] = useState<FilterState>(EMPTY_FILTER);
  const [pageNumber, setPageNumber] = useState<number>(1);
  const pageSize = 50;

  const [levels, setLevels] = useState<string[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const [pageData, setPageData] = useState<LogsPage | null>(null);
  const [loading, setLoading] = useState(false);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);
  const [selected, setSelected] = useState<LogEntry | null>(null);

  const [tailOn, setTailOn] = useState(false);
  const [tailBuffer, setTailBuffer] = useState<LogEntry[]>([]);
  const [newIds, setNewIds] = useState<Set<number>>(new Set());
  const lastSeenRef = useRef<string | null>(
    typeof window !== "undefined"
      ? window.localStorage.getItem(LAST_SEEN_KEY)
      : null,
  );

  const showFilterBanner =
    tailOn && (filters.q.trim() || filters.tag || filters.level);

  const filtersKey = useMemo(
    () =>
      JSON.stringify({
        level: filters.level,
        tag: filters.tag,
        q: filters.q.trim(),
        from: filters.from,
        to: filters.to,
      }),
    [filters],
  );

  const loadLevelsAndTags = useCallback(async () => {
    try {
      const [lvs, tgs] = await Promise.all([
        logsApi.getLevels(),
        logsApi.getTags(),
      ]);
      setLevels(lvs);
      setTags(
        tgs
          .map((t) => t.replace(/^"|"$/g, "").trim())
          .filter((t) => t.length > 0),
      );
    } catch (err) {
      console.warn("Load levels/tags failed", err);
    }
  }, []);

  useEffect(() => {
    loadLevelsAndTags();
  }, [loadLevelsAndTags]);

  const fetchPage = useCallback(
    async (mode: "page" | "tail", opts?: { from?: string; sort?: "asc" | "desc" }) => {
      setLoading(true);
      setErrorBanner(null);
      try {
        const params: LogsQueryParams = {
          level: filters.level || undefined,
          tag: filters.tag || undefined,
          q: filters.q.trim() || undefined,
          from: opts?.from ?? (filters.from || undefined),
          to: filters.to || undefined,
          pageSize,
        };
        if (mode === "page") {
          params.page = pageNumber;
          params.sort = "desc";
        } else {
          params.sort = opts?.sort ?? "asc";
        }
        const data = await logsApi.query(params);
        if (mode === "page") {
          setPageData(data);
        } else {
          setTailBuffer(data.items);
          if (data.items.length > 0) {
            const last = data.items[data.items.length - 1];
            lastSeenRef.current = last.timestamp;
            window.localStorage.setItem(LAST_SEEN_KEY, last.timestamp);
            setNewIds(new Set(data.items.map((i) => i.id)));
          } else {
            setNewIds(new Set());
          }
        }
      } catch (err: any) {
        const status = err?.response?.status;
        const msg =
          status === 403
            ? "Không có quyền — cần role SAdmin."
            : status === 401
              ? "Phiên đăng nhập đã hết hạn."
              : err?.message || "Không tải được log.";
        setErrorBanner(msg);
      } finally {
        setLoading(false);
      }
    },
    [filters, pageNumber, pageSize],
  );

  useEffect(() => {
    setPageNumber(1);
  }, [filtersKey]);

  useEffect(() => {
    if (tailOn) return;
    fetchPage("page");
  }, [fetchPage, tailOn, pageNumber, filtersKey]);

  useEffect(() => {
    if (!tailOn) return;
    const tick = async () => {
      const from = lastSeenRef.current ?? new Date().toISOString();
      await fetchPage("tail", { from, sort: "asc" });
    };
    tick();
    const id = window.setInterval(tick, 10000);
    return () => window.clearInterval(id);
  }, [tailOn, fetchPage]);

  const handleRefresh = useCallback(() => {
    if (tailOn) return;
    fetchPage("page");
  }, [fetchPage, tailOn]);

  const tailEntries = tailBuffer;
  const visibleEntries = tailOn ? tailEntries : (pageData?.items ?? []);

  const hintTags = useMemo(
    () => USER_ACTIVITY_HINT_TAGS.filter((t) => tags.includes(t)),
    [tags],
  );

  return (
    <div className="flex-1 h-full flex flex-col gap-4 p-4 xl:p-6 overflow-hidden">
      <div className="flex items-center justify-between shrink-0">
        <div>
          <h1 className="text-xl font-bold text-slate-800 flex items-center gap-2">
            <ScrollText className="w-5 h-5 text-blue-600" />
            Logs hệ thống
          </h1>
          <p className="text-xs text-slate-500 mt-0.5">
            Xem log backend (Serilog) và thao tác của user. Yêu cầu role SAdmin.
          </p>
        </div>
        <button
          onClick={() => {
            if (tailOn) {
              setTailBuffer([]);
              setNewIds(new Set());
            }
            setTailOn((v) => !v);
          }}
          className={`inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold rounded-xl border transition-colors ${
            tailOn
              ? "bg-emerald-50 border-emerald-200 text-emerald-700 hover:bg-emerald-100"
              : "bg-white border-slate-200 text-slate-700 hover:bg-slate-50"
          }`}
        >
          {tailOn ? (
            <>
              <Pause className="w-3.5 h-3.5" /> Tạm dừng tail
            </>
          ) : (
            <>
              <Play className="w-3.5 h-3.5" /> Live tail
            </>
          )}
        </button>
      </div>

      {errorBanner ? (
        <div className="shrink-0 flex items-center gap-2 px-4 py-2.5 bg-red-50 border border-red-200 text-red-700 rounded-2xl text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span className="flex-1">{errorBanner}</span>
          <button
            onClick={() => setErrorBanner(null)}
            className="p-1 rounded hover:bg-red-100"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      ) : null}

      {showFilterBanner ? (
        <div className="shrink-0 flex items-center gap-2 px-4 py-2 bg-amber-50 border border-amber-200 text-amber-800 rounded-2xl text-xs">
          <AlertCircle className="w-3.5 h-3.5 shrink-0" />
          <span>
            Tail sẽ bỏ qua filter hiện tại — đang lấy tất cả log mới từ{" "}
            <code className="font-mono">
              {lastSeenRef.current ?? "now"}
            </code>
            .
          </span>
        </div>
      ) : null}

      <div className="flex-1 bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden flex flex-col min-h-0">
        <div className="flex items-center gap-1 p-1.5 bg-slate-50 border-b border-slate-100">
          <button
            onClick={() => setActiveTab("backend")}
            className={`flex-1 inline-flex items-center justify-center gap-1.5 px-3 py-2 text-xs font-bold rounded-xl transition-colors ${
              activeTab === "backend"
                ? "bg-white text-blue-700 shadow-sm ring-1 ring-slate-200"
                : "text-slate-500 hover:text-slate-700 hover:bg-slate-100"
            }`}
          >
            <Activity className="w-3.5 h-3.5" />
            Backend Logs
          </button>
          <button
            onClick={() => setActiveTab("user")}
            className={`flex-1 inline-flex items-center justify-center gap-1.5 px-3 py-2 text-xs font-bold rounded-xl transition-colors ${
              activeTab === "user"
                ? "bg-white text-violet-700 shadow-sm ring-1 ring-slate-200"
                : "text-slate-500 hover:text-slate-700 hover:bg-slate-100"
            }`}
          >
            <Users className="w-3.5 h-3.5" />
            User Activity
          </button>
        </div>

        {activeTab === "user" ? (
          <div className="px-4 py-2 bg-violet-50/60 border-b border-violet-100 text-[11px] text-violet-800 flex flex-wrap items-center gap-2">
            <span className="font-bold">Gợi ý tag:</span>
            {hintTags.length === 0 ? (
              <span className="italic text-violet-600/80">
                (Không có tag Auth/POApiServer trong DB — dùng ô tìm kiếm bên
                dưới.)
              </span>
            ) : (
              hintTags.map((t) => (
                <button
                  key={t}
                  onClick={() =>
                    setFilters({ ...filters, tag: t, level: "Information" })
                  }
                  className="px-2 py-0.5 rounded-md bg-white border border-violet-200 text-violet-700 hover:bg-violet-100 font-mono text-[10px]"
                >
                  {t}
                </button>
              ))
            )}
            <button
              onClick={() =>
                setFilters({
                  ...filters,
                  q: "login|logout|activate|deactivate",
                  level: "Information",
                  tag: "",
                })
              }
              className="px-2 py-0.5 rounded-md bg-white border border-violet-200 text-violet-700 hover:bg-violet-100"
            >
              q=login|logout|...
            </button>
            <button
              onClick={() => setFilters(EMPTY_FILTER)}
              className="ml-auto px-2 py-0.5 rounded-md text-violet-600 hover:bg-violet-100"
            >
              <X className="w-3 h-3 inline -mt-0.5" /> Xóa filter
            </button>
          </div>
        ) : null}

        <FilterBar
          filters={filters}
          setFilters={setFilters}
          levels={levels}
          tags={tags}
          onRefresh={handleRefresh}
          loading={loading}
        />

        <div className="flex-1 overflow-auto min-h-0">
          <LogsTable
            entries={visibleEntries}
            onRowClick={setSelected}
            loading={loading}
            highlightNew={tailOn}
            newIds={newIds}
          />
        </div>

        {tailOn ? (
          <div className="px-4 py-2 border-t border-slate-100 text-xs text-slate-500 flex items-center justify-between bg-emerald-50/40">
            <span>
              <span className="inline-block w-1.5 h-1.5 rounded-full bg-emerald-500 mr-1.5 animate-pulse" />
              Tail đang chạy · poll mỗi 10s · đã thấy{" "}
              <span className="font-bold text-slate-800">
                {tailEntries.length}
              </span>{" "}
              entry kể từ{" "}
              <code className="font-mono text-[10px]">
                {lastSeenRef.current ?? "—"}
              </code>
            </span>
            <button
              onClick={() => {
                setTailBuffer([]);
                setNewIds(new Set());
                lastSeenRef.current = new Date().toISOString();
                window.localStorage.setItem(
                  LAST_SEEN_KEY,
                  lastSeenRef.current,
                );
              }}
              className="text-emerald-700 hover:underline"
            >
              Reset lastSeen
            </button>
          </div>
        ) : (
          <Pagination
            page={pageData}
            onChange={(p) => setPageNumber(p)}
            disabled={loading}
          />
        )}
      </div>

      <LogDetailModal entry={selected} onClose={() => setSelected(null)} />
    </div>
  );
};

export default LogsView;