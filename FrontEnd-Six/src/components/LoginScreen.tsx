import React, { useState, useEffect } from "react";
import {
  LogIn,
  User,
  Lock,
  AlertTriangle,
  Loader2,
  Zap,
  ShieldCheck,
  KeyRound,
  Eye,
  EyeOff,
  Server,
} from "lucide-react";
import { useAuth } from "../contexts/AuthContext";
import LoginBackdrop from "./LoginBackdrop";

const LAST_USERNAME_KEY = "gauth_last_username";

interface DemoAccount {
  username: string;
  password: string;
  role: string;
  tone: "indigo" | "emerald" | "amber" | "violet";
}

const DEMO_ACCOUNTS: DemoAccount[] = [
  { username: "sadmin", password: "SAdmin@123", role: "Super Admin", tone: "indigo" },
  { username: "admin", password: "Admin@123", role: "Administrator", tone: "emerald" },
  { username: "operator", password: "Operator@123", role: "Operator", tone: "amber" },
  { username: "viewer", password: "Viewer@123", role: "Viewer", tone: "violet" },
];

const toneClasses: Record<DemoAccount["tone"], { bg: string; text: string; dot: string }> = {
  indigo: { bg: "bg-indigo-100 text-indigo-700 border-indigo-200", text: "text-indigo-700", dot: "bg-indigo-500" },
  emerald: { bg: "bg-emerald-100 text-emerald-700 border-emerald-200", text: "text-emerald-700", dot: "bg-emerald-500" },
  amber: { bg: "bg-amber-100 text-amber-700 border-amber-200", text: "text-amber-700", dot: "bg-amber-500" },
  violet: { bg: "bg-violet-100 text-violet-700 border-violet-200", text: "text-violet-700", dot: "bg-violet-500" },
};

export function LoginScreen() {
  const { login } = useAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [selectedDemo, setSelectedDemo] = useState<string | null>(null);

  useEffect(() => {
    const savedUsername = localStorage.getItem(LAST_USERNAME_KEY);
    if (savedUsername) {
      setUsername(savedUsername);
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      await login(username, password);
      localStorage.setItem(LAST_USERNAME_KEY, username);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setIsLoading(false);
    }
  };

  const fillDemo = (acc: DemoAccount) => {
    setUsername(acc.username);
    setPassword(acc.password);
    setSelectedDemo(acc.username);
    setError("");
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 overflow-hidden animate-in fade-in duration-300">
      {/* Backdrop: giao diện admin mờ phía sau */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="scale-[0.85] origin-top-left w-[117.6%] h-[117.6%] blur-[2px] opacity-60">
          <LoginBackdrop />
        </div>
      </div>
      {/* Dark overlay tăng độ tương phản cho hộp login */}
      <div className="absolute inset-0 bg-slate-900/30 backdrop-blur-[3px]" />

      {/* Login dialog (giống ConnectionLostDialog) */}
      <div className="relative z-10 bg-white rounded-3xl shadow-2xl border border-blue-200 w-full max-w-md mx-auto overflow-hidden">
        {/* Header */}
        <div className="bg-blue-50 border-b border-blue-200 px-6 py-5 flex items-start gap-4">
          <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center shrink-0 shadow-lg shadow-blue-500/30">
            <LogIn className="w-6 h-6 text-white" strokeWidth={2.5} />
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-lg font-bold text-blue-800 tracking-tight">
              Đăng nhập hệ thống
            </h2>
            <p className="text-sm text-blue-600 mt-1 leading-relaxed">
              Vui lòng nhập thông tin đăng nhập để truy cập vào Admin Panel.
            </p>
          </div>
        </div>

        {/* Body - Login form as a status table */}
        <form id="login-form" onSubmit={handleSubmit} className="px-6 py-5">
          <div className="rounded-xl border border-slate-200 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200">
                  <th className="px-4 py-2.5 text-left text-[11px] font-bold uppercase tracking-wider text-slate-500 w-[42%]">
                    Thông tin
                  </th>
                  <th className="px-4 py-2.5 text-left text-[11px] font-bold uppercase tracking-wider text-slate-500">
                    Giá trị
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {/* Username row */}
                <tr className="bg-white">
                  <td className="px-4 py-3 align-middle">
                    <div className="flex items-center gap-2">
                      <User className="w-4 h-4 text-slate-500" />
                      <span className="font-semibold text-slate-700">Tên đăng nhập</span>
                    </div>
                  </td>
                  <td className="px-4 py-2">
                    <input
                      type="text"
                      value={username}
                      onChange={(e) => {
                        setUsername(e.target.value);
                        setSelectedDemo(null);
                      }}
                      required
                      autoFocus
                      placeholder="Nhập username..."
                      className="w-full bg-transparent border-b border-slate-200 hover:border-slate-300 focus:border-blue-500 focus:bg-blue-50/30 px-1 py-1 text-sm text-slate-800 outline-none transition-all font-mono"
                    />
                  </td>
                </tr>
                {/* Password row */}
                <tr className="bg-slate-50/50">
                  <td className="px-4 py-3 align-middle">
                    <div className="flex items-center gap-2">
                      <Lock className="w-4 h-4 text-slate-500" />
                      <span className="font-semibold text-slate-700">Mật khẩu</span>
                    </div>
                  </td>
                  <td className="px-4 py-2">
                    <div className="flex items-center gap-1">
                      <input
                        type={showPassword ? "text" : "password"}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        placeholder="Nhập mật khẩu..."
                        className="flex-1 min-w-0 bg-transparent border-b border-slate-200 hover:border-slate-300 focus:border-blue-500 focus:bg-blue-50/30 px-1 py-1 text-sm text-slate-800 outline-none transition-all font-mono"
                      />
                      <button
                        type="button"
                        onClick={() => setShowPassword((v) => !v)}
                        className="p-1 rounded-md text-slate-400 hover:text-blue-600 hover:bg-blue-50 transition-colors shrink-0"
                        title={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                      >
                        {showPassword ? (
                          <EyeOff className="w-3.5 h-3.5" />
                        ) : (
                          <Eye className="w-3.5 h-3.5" />
                        )}
                      </button>
                    </div>
                  </td>
                </tr>
                {/* Server row (read-only status hint) */}
                <tr className="bg-white">
                  <td className="px-4 py-3 align-middle">
                    <div className="flex items-center gap-2">
                      <Server className="w-4 h-4 text-slate-500" />
                      <span className="font-semibold text-slate-700">Trạng thái</span>
                    </div>
                  </td>
                  <td className="px-4 py-2">
                    <span className="inline-flex items-center gap-1.5 text-blue-600 font-bold text-xs">
                      <span className="w-1.5 h-1.5 rounded-full bg-blue-500 animate-pulse" />
                      SẴN SÀNG
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Error details */}
          {error && (
            <div className="mt-4 rounded-xl bg-red-50 border border-red-200 p-3">
              <div className="text-[10px] font-bold uppercase tracking-wider text-red-500 mb-1.5 flex items-center gap-1.5">
                <AlertTriangle className="w-3.5 h-3.5" />
                Lỗi đăng nhập
              </div>
              <div className="text-xs font-mono text-red-700 leading-relaxed break-words">
                {error}
              </div>
            </div>
          )}

          {/* Demo accounts table */}
          <div className="mt-4 rounded-xl border border-slate-200 overflow-hidden">
            <div className="bg-slate-50 border-b border-slate-200 px-4 py-2.5 flex items-center justify-between">
              <span className="text-[11px] font-bold uppercase tracking-wider text-slate-500 flex items-center gap-1.5">
                <Zap className="w-3.5 h-3.5 text-amber-500" />
                Tài khoản demo
              </span>
              <span className="text-[10px] text-slate-400 italic">Bấm để điền nhanh</span>
            </div>
            <table className="w-full text-sm">
              <tbody className="divide-y divide-slate-100">
                {DEMO_ACCOUNTS.map((acc, idx) => {
                  const tone = toneClasses[acc.tone];
                  const isSelected = selectedDemo === acc.username;
                  return (
                    <tr
                      key={acc.username}
                      onClick={() => fillDemo(acc)}
                      className={`cursor-pointer transition-colors ${
                        idx % 2 === 0 ? "bg-white" : "bg-slate-50/50"
                      } hover:bg-blue-50/50 ${
                        isSelected ? "!bg-blue-50" : ""
                      }`}
                    >
                      <td className="px-4 py-2.5">
                        <div className="flex items-center gap-2">
                          <span className={`w-1.5 h-1.5 rounded-full ${tone.dot}`} />
                          <span className="font-semibold text-slate-700 font-mono text-xs">
                            {acc.username}
                          </span>
                          <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded border ${tone.bg}`}>
                            {acc.role.toUpperCase()}
                          </span>
                        </div>
                      </td>
                      <td className="px-4 py-2.5 text-right">
                        <span className={`text-[10px] font-bold uppercase tracking-wider ${
                          isSelected ? "text-blue-600" : "text-slate-400"
                        }`}>
                          {isSelected ? "ĐÃ CHỌN" : "DÙNG →"}
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </form>

        {/* Footer */}
        <div className="px-6 py-4 bg-slate-50 border-t border-slate-200 flex items-center gap-3">
          <button
            type="submit"
            form="login-form"
            disabled={isLoading || !username || !password}
            className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold bg-blue-600 hover:bg-blue-700 text-white shadow-lg shadow-blue-500/20 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                Đang xác thực...
              </>
            ) : (
              <>
                <ShieldCheck className="w-4 h-4" />
                Đăng nhập
              </>
            )}
          </button>
          <div className="text-[11px] text-slate-400 text-right">
            <KeyRound className="w-3.5 h-3.5 inline mr-1 -mt-0.5" />
            Phiên bản<br />1.0
          </div>
        </div>
      </div>
    </div>
  );
}