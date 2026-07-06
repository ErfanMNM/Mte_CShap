// ErrorBoundary: a React class component.
//
// NOTE: this project does NOT install @types/react, so React.Component /
// Component base class members (state, setState, props) are not visible to
// TypeScript. We declare the shape explicitly and cast `this` where needed
// so the file type-checks without adding a new dependency.

import React from "react";
import { AlertTriangle, RefreshCw, LayoutDashboard } from "lucide-react";

interface ErrorBoundaryProps {
  children: React.ReactNode;
  fallback?: (err: Error, reset: () => void) => React.ReactNode;
  // Allow React's reserved `key` prop without making it part of the public API.
  key?: unknown;
}

interface ErrorBoundaryState {
  error: Error | null;
}

interface ErrorBoundaryInstance {
  state: ErrorBoundaryState;
  props: ErrorBoundaryProps;
  setState(updater: Partial<ErrorBoundaryState> | ((prev: ErrorBoundaryState) => Partial<ErrorBoundaryState>)): void;
  reset(): void;
  goHome(): void;
}

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo): void {
    // eslint-disable-next-line no-console
    console.error("[ErrorBoundary] Caught error:", error, info.componentStack);
  }

  reset(): void {
    (this as unknown as ErrorBoundaryInstance).setState({ error: null });
  }

  goHome(): void {
    const url = new URL(window.location.href);
    url.searchParams.set("panel", "monitor");
    window.location.href = url.toString();
  }

  render() {
    const self = this as unknown as ErrorBoundaryInstance;
    const error = self.state.error;
    if (!error) return self.props.children;

    if (self.props.fallback) {
      return self.props.fallback(error, () => (self as unknown as { reset(): void }).reset());
    }

    return (
      <div className="flex items-center justify-center h-full w-full p-6 animate-in fade-in duration-300">
        <div className="max-w-lg w-full bg-white rounded-3xl border border-red-200 shadow-sm overflow-hidden">
          <div className="bg-red-50 border-b border-red-200 px-6 py-4 flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-red-100 flex items-center justify-center shrink-0">
              <AlertTriangle className="w-5 h-5 text-red-600" strokeWidth={2.5} />
            </div>
            <div className="min-w-0">
              <h2 className="text-base font-bold text-red-800 tracking-tight">
                Đã xảy ra lỗi
              </h2>
              <p className="text-xs text-red-600 mt-0.5">
                Panel này gặp sự cố. Bạn có thể thử lại hoặc quay về trang Giám sát SCADA.
              </p>
            </div>
          </div>

          <div className="p-6 flex flex-col gap-4">
            <div className="rounded-xl border border-slate-200 bg-slate-50 p-3">
              <div className="text-[10px] font-bold uppercase tracking-wider text-slate-500 mb-1">
                Chi tiết lỗi
              </div>
              <div className="text-xs font-mono text-slate-700 break-all leading-relaxed">
                {error.message || error.name || "Unknown error"}
              </div>
            </div>

            <div className="flex items-center gap-2 flex-wrap">
              <button
                onClick={() => self.reset()}
                className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold bg-blue-600 hover:bg-blue-700 text-white shadow-lg shadow-blue-500/20 transition-colors"
              >
                <RefreshCw className="w-4 h-4" />
                Thử lại
              </button>
              <button
                onClick={() => self.goHome()}
                className="flex items-center gap-2 px-4 py-2 rounded-xl text-sm font-bold bg-slate-100 hover:bg-slate-200 text-slate-700 transition-colors"
              >
                <LayoutDashboard className="w-4 h-4" />
                Về trang Giám sát SCADA
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }
}

export default ErrorBoundary;