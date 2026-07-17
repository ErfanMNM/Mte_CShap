import React, { useState } from "react";
import { X, ScrollText, AlertOctagon } from "lucide-react";
import type { LogEntry } from "../../types/logs";
import { parseLogProperties } from "../../types/logs";

interface LogDetailModalProps {
  entry: LogEntry | null;
  onClose: () => void;
}

function formatTimestamp(ts: string): string {
  try {
    return new Date(ts).toLocaleString();
  } catch {
    return ts;
  }
}

export const LogDetailModal: React.FC<LogDetailModalProps> = ({
  entry,
  onClose,
}) => {
  if (!entry) return null;
  const props = parseLogProperties(entry);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-sm"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="bg-white rounded-3xl shadow-2xl w-full max-w-3xl mx-4 max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 shrink-0">
          <div className="flex items-center gap-2">
            <ScrollText className="w-5 h-5 text-blue-600" />
            <h3 className="text-base font-bold text-slate-800">
              Chi tiết log entry #{entry.id}
            </h3>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-slate-100 rounded-xl transition-colors text-slate-400 hover:text-slate-600"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="px-6 py-5 overflow-y-auto">
          <table className="w-full text-sm">
            <tbody>
              <Row label="ID" value={String(entry.id)} />
              <Row label="Timestamp" value={formatTimestamp(entry.timestamp)} />
              <Row label="Level" value={entry.level || entry.levelName || ""} />
              <Row
                label="Source"
                value={entry.sourceContext ?? "—"}
              />
              <Row
                label="Machine"
                value={entry.machineName ?? "—"}
              />
              <Row
                label="Thread"
                value={entry.threadId != null ? String(entry.threadId) : "—"}
              />
              <Row
                label="Message Template"
                value={entry.messageTemplate ?? "—"}
                mono
              />
            </tbody>
          </table>

          <div className="mt-5">
            <h4 className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-2">
              Message
            </h4>
            <pre className="text-xs font-mono bg-slate-50 border border-slate-200 rounded-xl p-3 overflow-x-auto whitespace-pre-wrap break-words text-slate-800">
              {entry.message}
            </pre>
          </div>

          {entry.exception ? (
            <div className="mt-5">
              <h4 className="text-xs font-bold uppercase tracking-wider text-red-600 mb-2 flex items-center gap-1.5">
                <AlertOctagon className="w-3.5 h-3.5" />
                Exception
              </h4>
              <pre className="text-xs font-mono bg-red-50 border border-red-200 rounded-xl p-3 max-h-72 overflow-auto text-red-900 whitespace-pre-wrap break-words">
                {entry.exception}
              </pre>
            </div>
          ) : null}

          <div className="mt-5">
            <h4 className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-2">
              Properties
            </h4>
            {props ? (
              <pre className="text-xs font-mono bg-slate-900 text-slate-100 rounded-xl p-3 overflow-x-auto">
                {JSON.stringify(props, null, 2)}
              </pre>
            ) : (
              <div className="text-xs text-slate-400 italic">Không có.</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

const Row: React.FC<{ label: string; value: string; mono?: boolean }> = ({
  label,
  value,
  mono,
}) => (
  <tr className="border-b border-slate-100 last:border-0">
    <td className="py-2 pr-4 align-top w-44 text-[11px] font-bold uppercase tracking-wider text-slate-500">
      {label}
    </td>
    <td
      className={`py-2 text-sm text-slate-800 break-words ${
        mono ? "font-mono text-xs" : ""
      }`}
    >
      {value || "—"}
    </td>
  </tr>
);

export default LogDetailModal;