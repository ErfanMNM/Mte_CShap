import React from "react";
import { AlertTriangle, RefreshCw, WifiOff, Monitor, Cpu, Server } from "lucide-react";
import { useDeviceStore } from "../store/useDeviceStore";

const FAIL_THRESHOLD = 5;
const POLLING_INTERVAL_MS = 2000;

const formatElapsed = (count: number): string => {
  const seconds = count * (POLLING_INTERVAL_MS / 1000);
  if (seconds < 60) return `${seconds} giây`;
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins}m ${secs}s`;
};

const ConnectionLostDialog: React.FC = () => {
  const apiStatus = useDeviceStore((s) => s.apiStatus);
  const failCount = useDeviceStore((s) => s.failCount);
  const manualPoll = useDeviceStore((s) => s.manualPoll);

  if (!apiStatus.error || failCount < FAIL_THRESHOLD) return null;

  const handleRetry = async () => {
    await manualPoll();
  };

  const elapsed = formatElapsed(failCount);
  const retryIn = Math.max(0, FAIL_THRESHOLD - (failCount % FAIL_THRESHOLD));

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-3xl shadow-2xl border border-red-200 w-full max-w-md mx-4 overflow-hidden">
        {/* Header */}
        <div className="bg-red-50 border-b border-red-200 px-6 py-5 flex items-start gap-4">
          <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-red-500 to-rose-600 flex items-center justify-center shrink-0 shadow-lg shadow-red-500/30">
            <AlertTriangle className="w-6 h-6 text-white" strokeWidth={2.5} />
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-lg font-bold text-red-800 tracking-tight">
              Mất kết nối Backend
            </h2>
            <p className="text-sm text-red-600 mt-1 leading-relaxed">
              Không thể kết nối đến server. Vui lòng kiểm tra mạng hoặc liên hệ admin.
            </p>
          </div>
        </div>

        {/* Body - Connection Status Table */}
        <div className="px-6 py-5">
          <div className="rounded-xl border border-slate-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200">
                  <th className="px-4 py-2.5 text-left text-[11px] font-bold uppercase tracking-wider text-slate-500">
                    Thành phần
                  </th>
                  <th className="px-4 py-2.5 text-left text-[11px] font-bold uppercase tracking-wider text-slate-500">
                    Trạng thái
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                <tr className="bg-white">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Server className="w-4 h-4 text-slate-500" />
                      <span className="font-semibold text-slate-700">Backend API</span>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <span className="inline-flex items-center gap-1.5 text-red-600 font-bold text-xs">
                      <span className="w-1.5 h-1.5 rounded-full bg-red-500 animate-pulse" />
                      OFFLINE
                    </span>
                  </td>
                </tr>
                <tr className="bg-slate-50/50">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Monitor className="w-4 h-4 text-slate-500" />
                      <span className="font-semibold text-slate-700">Camera</span>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <span className="inline-flex items-center gap-1.5 text-amber-600 font-bold text-xs">
                      <span className="w-1.5 h-1.5 rounded-full bg-amber-500" />
                      KHÔNG RÕ
                    </span>
                  </td>
                </tr>
                <tr className="bg-white">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Cpu className="w-4 h-4 text-slate-500" />
                      <span className="font-semibold text-slate-700">PLC OMRON</span>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <span className="inline-flex items-center gap-1.5 text-amber-600 font-bold text-xs">
                      <span className="w-1.5 h-1.5 rounded-full bg-amber-500" />
                      KHÔNG RÕ
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Error details */}
          <div className="mt-4 rounded-xl bg-slate-50 border border-slate-200 p-3">
            <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1.5">
              Chi tiết lỗi
            </div>
            <div className="text-xs font-mono text-slate-700 leading-relaxed">
              {apiStatus.statusCode
                ? `HTTP ${apiStatus.statusCode}`
                : apiStatus.message === "network"
                  ? "Lỗi mạng (timeout / DNS / refused)"
                  : apiStatus.message || "Unknown"}
            </div>
          </div>

          {/* Status row */}
          <div className="mt-3 flex items-center justify-between text-xs text-slate-500">
            <div className="flex items-center gap-1.5">
              <WifiOff className="w-3.5 h-3.5" />
              <span>Mất kết nối: <strong className="text-slate-700">{elapsed}</strong></span>
            </div>
            <div>
              Lần thử lại: <strong className="text-slate-700">{failCount}</strong>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="px-6 py-4 bg-slate-50 border-t border-slate-200 flex items-center gap-3">
          <button
            onClick={handleRetry}
            className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold bg-blue-600 hover:bg-blue-700 text-white shadow-lg shadow-blue-500/20 transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
            Thử lại ngay
          </button>
          <div className="text-[11px] text-slate-400 text-center">
            Tự động thử lại sau<br />{retryIn * (POLLING_INTERVAL_MS / 1000)}s
          </div>
        </div>
      </div>
    </div>
  );
};

export default ConnectionLostDialog;
