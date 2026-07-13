import React from "react";
import {
  Factory,
  LayoutDashboard,
  Cpu,
  Package,
  Database,
  BarChart2,
  Users,
  Settings,
  Activity,
  Server,
  Wifi,
  Camera,
  QrCode,
  CheckCircle2,
  RefreshCw,
  PlugZap,
} from "lucide-react";

const BackdropCard: React.FC<{ children: React.ReactNode; className?: string }> = ({
  children,
  className = "",
}) => (
  <div
    className={`bg-white rounded-3xl border border-slate-200/60 shadow-sm overflow-hidden flex flex-col ${className}`}
  >
    {children}
  </div>
);

const BackdropCardHeader: React.FC<{
  title: string;
  icon: React.ElementType;
}> = ({ title, icon: Icon }) => (
  <div className="bg-slate-50/80 border-b border-slate-100 flex items-center gap-2 px-4 xl:px-6 py-3 xl:py-3.5 shrink-0 rounded-t-3xl">
    <Icon className="w-4 h-4 text-blue-600" />
    <h2 className="text-[11px] xl:text-[13px] font-bold tracking-wide uppercase text-slate-800">
      {title}
    </h2>
  </div>
);

const NAV_ITEMS = [
  { id: "monitor", title: "Giám sát SCADA", icon: LayoutDashboard, active: true },
  { id: "production", title: "Điều khiển SX", icon: Factory },
  { id: "plcsetting", title: "PLC Setting", icon: Cpu },
  { id: "batches", title: "Lệnh sản xuất", icon: Package },
  { id: "datapool", title: "Quản lý DataPool", icon: Database },
  { id: "history", title: "Báo cáo sản xuất", icon: BarChart2 },
  { id: "users", title: "Quản lý tài khoản", icon: Users },
  { id: "settings", title: "Cấu hình hệ thống", icon: Settings },
];

const DEVICES = [
  {
    label: "CAMERA",
    status: "ONLINE",
    icon: Camera,
    bg: "bg-green-50 border-green-200",
    text: "text-green-800",
    iconBg: "bg-gradient-to-br from-green-500 to-emerald-600 shadow-green-500/30",
  },
  {
    label: "PLC OMRON",
    status: "ĐANG KẾT NỐI",
    icon: Cpu,
    bg: "bg-amber-50 border-amber-200",
    text: "text-amber-800",
    iconBg: "bg-gradient-to-br from-amber-400 to-orange-500 shadow-amber-500/30",
  },
  {
    label: "BACKEND",
    status: "ONLINE",
    icon: Server,
    bg: "bg-green-50 border-green-200",
    text: "text-green-800",
    iconBg: "bg-gradient-to-br from-green-500 to-emerald-600 shadow-green-500/30",
  },
  {
    label: "HỆ THỐNG",
    status: "READY",
    icon: Wifi,
    bg: "bg-blue-50 border-blue-200",
    text: "text-blue-800",
    iconBg: "bg-gradient-to-br from-blue-400 to-indigo-600 shadow-blue-500/30",
  },
];

const LOG_ROWS = [
  { id: "—", time: "08:42:15", state: "Connected", tone: "bg-blue-100 text-blue-700", code: "CAMERA-01", carton: "—" },
  { id: "—", time: "08:42:08", state: "Received", tone: "bg-green-100 text-green-700", code: "CAMERA-01", carton: "CARTON-12345" },
  { id: "—", time: "08:41:55", state: "Received", tone: "bg-green-100 text-green-700", code: "CAMERA-01", carton: "CARTON-12344" },
  { id: "—", time: "08:41:42", state: "Connected", tone: "bg-blue-100 text-blue-700", code: "CAMERA-01", carton: "—" },
  { id: "—", time: "08:41:30", state: "Received", tone: "bg-green-100 text-green-700", code: "CAMERA-01", carton: "CARTON-12343" },
  { id: "—", time: "08:41:18", state: "Duplicate", tone: "bg-amber-100 text-amber-700", code: "CAMERA-01", carton: "CARTON-12342" },
];

const LoginBackdrop: React.FC = () => {
  return (
    <div className="fixed inset-0 flex h-screen bg-[#F6F8FA] overflow-hidden font-sans text-slate-900 select-none">
      {/* SIDEBAR */}
      <aside className="w-64 xl:w-72 bg-white border-r border-slate-200/60 flex flex-col shrink-0">
        <div className="h-20 flex items-center px-6 xl:px-8 border-b border-slate-100/50 shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center shadow-lg shadow-blue-600/20">
              <Factory className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="text-base xl:text-lg font-black tracking-tighter text-slate-800 leading-none">
                MASAN-SERIALIZATION
              </h1>
              <span className="text-[10px] font-bold text-slate-400 tracking-widest uppercase">
                Admin Panel
              </span>
            </div>
          </div>
        </div>

        <nav className="flex-1 overflow-y-auto px-4 py-6 flex flex-col gap-1.5">
          <div className="text-xs font-bold text-slate-400 tracking-widest uppercase px-4 mb-2">
            Điều khiển trung tâm
          </div>
          {NAV_ITEMS.map((item) => (
            <div
              key={item.id}
              className={`flex items-center gap-3.5 px-4 py-3.5 rounded-2xl font-semibold text-sm ${
                item.active ? "bg-blue-50 text-blue-700" : "text-slate-600"
              }`}
            >
              <item.icon
                className={`w-5 h-5 ${item.active ? "text-blue-600" : "text-slate-400"}`}
              />
              {item.title}
            </div>
          ))}
        </nav>

        <div className="p-4 border-t border-slate-100 shrink-0">
          <div className="flex items-center gap-3 bg-slate-50 p-3 rounded-2xl border border-slate-100">
            <div className="w-10 h-10 rounded-xl bg-indigo-100 text-indigo-700 flex items-center justify-center font-bold text-sm shrink-0">
              GU
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-bold text-slate-800 truncate">Guest</p>
              <p className="text-xs font-medium text-slate-500 truncate">
                Chưa đăng nhập
              </p>
            </div>
          </div>
        </div>
      </aside>

      {/* MAIN */}
      <main className="flex-1 overflow-hidden p-4 lg:p-6 2xl:p-8 flex flex-col min-h-0">
        <div className="flex flex-col xl:flex-row gap-4 xl:gap-5 h-full min-h-0 pb-1">
          {/* LEFT COLUMN */}
          <div className="flex flex-col gap-4 xl:gap-5 w-full xl:w-[60%] h-full min-h-0">
            {/* Result card */}
            <BackdropCard className="shrink-0">
              <BackdropCardHeader title="KẾT QUẢ VỪA KIỂM" icon={Activity} />
              <div className="p-4 2xl:p-5 flex gap-3 2xl:gap-5 h-28 2xl:h-36">
                <div className="w-1/3 rounded-2xl text-white flex flex-col items-center justify-center bg-gradient-to-br from-green-500 to-emerald-600 shadow-lg shadow-green-500/20 ring-1 ring-white/20">
                  <CheckCircle2 className="w-8 h-8 2xl:w-10 2xl:h-10 mb-1" strokeWidth={2.5} />
                  <span className="text-2xl 2xl:text-3xl font-black tracking-widest">
                    TỐT
                  </span>
                </div>
                <div className="flex-1 bg-slate-50 rounded-2xl border border-slate-200/80 p-3 flex flex-col relative overflow-hidden">
                  <div className="text-[10px] 2xl:text-xs font-black text-slate-400 uppercase tracking-widest mb-1 2xl:mb-2">
                    Sự kiện camera gần nhất
                  </div>
                  <div className="text-sm 2xl:text-lg font-mono font-medium text-slate-800 leading-tight">
                    Đang chờ dữ liệu từ camera...
                  </div>
                </div>
              </div>
            </BackdropCard>

            {/* Log table card */}
            <BackdropCard className="flex-1 min-h-0">
              <div className="bg-slate-50/50 border-b border-slate-100 flex p-1.5 gap-1.5 shrink-0 px-2 pt-2 rounded-t-3xl">
                <div className="flex-1 py-2.5 px-4 text-xs font-bold rounded-xl bg-blue-50 text-blue-700 flex items-center justify-center gap-2">
                  <Activity className="w-4 h-4" /> THÔNG BÁO CHUNG
                </div>
                <div className="flex-1 py-2.5 px-4 text-xs font-bold rounded-xl text-slate-500 flex items-center justify-center gap-2">
                  <Cpu className="w-4 h-4" /> NHẬT KÝ PLC
                </div>
                <div className="flex-1 py-2.5 px-4 text-xs font-bold rounded-xl text-slate-500 flex items-center justify-center gap-2">
                  <Activity className="w-4 h-4" /> KIỂM TRA LỖI
                </div>
                <div className="flex-1 py-2.5 px-4 text-xs font-bold rounded-xl text-slate-500 flex items-center justify-center gap-2">
                  <QrCode className="w-4 h-4" /> LỊCH SỬ CAMERA
                </div>
              </div>
              <div className="flex-1 overflow-hidden bg-white">
                <table className="w-full text-sm text-left">
                  <thead className="text-[10px] uppercase text-slate-400 bg-white sticky top-0 border-b border-slate-100">
                    <tr>
                      <th className="px-5 py-3 font-bold tracking-wider">ID</th>
                      <th className="px-5 py-3 font-bold tracking-wider">Thời gian</th>
                      <th className="px-5 py-3 font-bold tracking-wider">Trạng thái</th>
                      <th className="px-5 py-3 font-bold tracking-wider">Mã</th>
                      <th className="px-5 py-3 font-bold tracking-wider">Carton</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100/80">
                    {LOG_ROWS.map((row, i) => (
                      <tr key={i} className="hover:bg-slate-50/50">
                        <td className="px-5 py-2.5 font-mono text-[11px] text-slate-500">
                          {row.id}
                        </td>
                        <td className="px-5 py-2.5 font-mono text-[11px] text-slate-500">
                          {row.time}
                        </td>
                        <td className="px-5 py-2.5">
                          <span
                            className={`px-2.5 py-1 rounded-full text-[10px] font-bold tracking-wide uppercase ${row.tone}`}
                          >
                            {row.state}
                          </span>
                        </td>
                        <td className="px-5 py-2.5 text-sm text-slate-700 font-mono">
                          {row.code}
                        </td>
                        <td className="px-5 py-2.5 text-sm text-slate-700 font-mono">
                          {row.carton}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </BackdropCard>
          </div>

          {/* RIGHT COLUMN */}
          <div className="flex flex-col gap-4 xl:gap-5 w-full xl:w-[40%] h-full pb-1">
            {/* Device status */}
            <BackdropCard className="flex-1">
              <BackdropCardHeader title="TRẠNG THÁI THIẾT BỊ" icon={Server} />
              <div className="p-2 grid grid-cols-2 gap-1.5 2xl:gap-2 flex-1">
                {DEVICES.map((d) => (
                  <div
                    key={d.label}
                    className={`rounded-xl border p-2 flex items-center gap-2 ${d.bg}`}
                  >
                    <div
                      className={`w-8 h-8 rounded-lg flex items-center justify-center shadow-md ${d.iconBg}`}
                    >
                      <d.icon className="w-3.5 h-3.5 text-white" strokeWidth={2.5} />
                    </div>
                    <div className="min-w-0 flex-1">
                      <div className={`text-[10px] font-black uppercase tracking-wider ${d.text}`}>
                        {d.label}
                      </div>
                      <div className={`text-[9px] font-bold ${d.text}`}>{d.status}</div>
                    </div>
                  </div>
                ))}
              </div>
            </BackdropCard>

            {/* WS connection */}
            <BackdropCard className="flex-1">
              <BackdropCardHeader
                title="KẾT NỐI WEBSOCKET"
                icon={Wifi}
              />
              <div className="p-3 xl:p-4 flex flex-col gap-3 flex-1">
                <div className="grid grid-cols-2 gap-2">
                  <div className="bg-slate-50 rounded-xl border border-slate-100 px-3 py-2 flex items-center justify-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
                    <span className="text-xs font-bold text-slate-600">Đã kết nối</span>
                  </div>
                  <div className="bg-slate-50 rounded-xl border border-slate-100 px-3 py-2 flex items-center justify-center gap-2">
                    <PlugZap className="w-4 h-4 text-blue-600" />
                    <span className="text-xs font-bold text-slate-600">Camera WS</span>
                  </div>
                  <div className="bg-slate-50 rounded-xl border border-slate-100 px-3 py-2 flex items-center justify-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
                    <span className="text-xs font-bold text-slate-600">PLC OK</span>
                  </div>
                  <div className="bg-slate-50 rounded-xl border border-slate-100 px-3 py-2 flex items-center justify-center gap-2">
                    <PlugZap className="w-4 h-4 text-emerald-600" />
                    <span className="text-xs font-bold text-slate-600">PLC WS</span>
                  </div>
                </div>
                <div className="bg-slate-50 rounded-xl border border-slate-100 px-3 py-2">
                  <div className="text-[10px] font-bold text-slate-400 uppercase mb-1">
                    Camera Endpoint
                  </div>
                  <div className="text-xs font-mono text-slate-700 break-all">
                    ws://localhost:9999/ws/camera
                  </div>
                </div>
                <button className="flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-xl text-xs font-bold transition-colors">
                  <RefreshCw className="w-3.5 h-3.5" /> Thử lại kết nối
                </button>
              </div>
            </BackdropCard>
          </div>
        </div>
      </main>
    </div>
  );
};

export default LoginBackdrop;
