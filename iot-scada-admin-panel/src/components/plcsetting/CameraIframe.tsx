import React, { useState } from "react";
import {
  Camera,
  RefreshCw,
  ExternalLink,
  X,
  Globe,
} from "lucide-react";

const DEFAULT_URL = "https://example.com";

const normalizeUrl = (raw: string): string | null => {
  const trimmed = raw.trim();
  if (!trimmed) return null;
  if (/^https?:\/\//i.test(trimmed)) return trimmed;
  return `https://${trimmed}`;
};

export const CameraIframe: React.FC = () => {
  const [currentUrl, setCurrentUrl] = useState<string>(DEFAULT_URL);
  const [inputUrl, setInputUrl] = useState<string>(DEFAULT_URL);
  const [iframeKey, setIframeKey] = useState<number>(0);

  const apply = () => {
    const u = normalizeUrl(inputUrl);
    if (!u) return;
    setCurrentUrl(u);
    setIframeKey((k) => k + 1);
  };

  const reset = () => {
    setInputUrl(DEFAULT_URL);
    setCurrentUrl(DEFAULT_URL);
    setIframeKey((k) => k + 1);
  };

  const reload = () => {
    setIframeKey((k) => k + 1);
  };

  return (
    <div className="bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden flex flex-col h-full">
      {/* Header */}
      <div className="bg-slate-50/80 border-b border-slate-100 px-4 xl:px-6 py-3.5 flex items-center justify-between shrink-0">
        <h2 className="text-[13px] font-bold tracking-wide uppercase text-slate-800 flex items-center gap-2">
          <Camera className="w-4 h-4 text-blue-600" /> Camera Viewer
        </h2>
        <div className="flex items-center gap-2">
          <button
            onClick={reload}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:text-slate-700 hover:bg-slate-100 rounded-xl border border-slate-200 transition-colors"
            title="Tải lại iframe"
          >
            <RefreshCw className="w-3.5 h-3.5" /> Reload
          </button>
          <a
            href={currentUrl}
            target="_blank"
            rel="noreferrer"
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-xl border border-blue-200 transition-colors"
          >
            <ExternalLink className="w-3.5 h-3.5" /> Mở tab mới
          </a>
        </div>
      </div>

      {/* URL bar */}
      <div className="px-4 xl:px-6 py-3 border-b border-slate-100 bg-white shrink-0">
        <div className="flex items-center gap-2">
          <div className="flex items-center gap-2 flex-1 bg-slate-50 border border-slate-200 rounded-xl px-3 py-2">
            <Globe className="w-4 h-4 text-slate-400 shrink-0" />
            <input
              type="text"
              value={inputUrl}
              onChange={(e) => setInputUrl(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && apply()}
              placeholder="https://..."
              className="flex-1 bg-transparent outline-none text-sm text-slate-700 placeholder:text-slate-400 font-mono"
            />
            {inputUrl && inputUrl !== DEFAULT_URL && (
              <button
                onClick={() => setInputUrl("")}
                className="p-0.5 hover:bg-slate-200 rounded transition-colors"
              >
                <X className="w-3.5 h-3.5 text-slate-400" />
              </button>
            )}
          </div>
          <button
            onClick={apply}
            className="flex items-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-sm font-bold transition-colors shrink-0"
          >
            <RefreshCw className="w-3.5 h-3.5" /> Apply
          </button>
          <button
            onClick={reset}
            className="flex items-center gap-1.5 px-3 py-2 text-slate-600 hover:bg-slate-100 rounded-xl text-sm font-semibold transition-colors border border-slate-200 shrink-0"
            title="Reset về example.com"
          >
            Reset
          </button>
        </div>
        <div className="mt-2 text-[10px] text-slate-400 font-mono truncate">
          Đang hiển thị: <span className="text-slate-600">{currentUrl}</span>
        </div>
      </div>

      {/* Iframe */}
      <div className="flex-1 min-h-[420px] bg-slate-100 relative">
        <iframe
          key={iframeKey}
          src={currentUrl}
          title="Camera iframe viewer"
          className="w-full h-full min-h-[420px] bg-white"
          sandbox="allow-scripts allow-same-origin allow-forms allow-popups"
          referrerPolicy="no-referrer-when-downgrade"
        />
      </div>
    </div>
  );
};

export default CameraIframe;
