import React from "react";
import { Cpu } from "lucide-react";
import CameraIframe from "./CameraIframe";
import PLCRecipeForm from "./PLCRecipeForm";

const PLCSettingsView: React.FC = () => {
  return (
    <div className="flex flex-col gap-4 h-full min-h-0 w-full animate-in fade-in duration-500 overflow-auto scrollbar-hide pb-6">
      {/* Header */}
      <div className="flex items-center gap-3 px-1 shrink-0">
        <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center shadow-lg shadow-blue-500/20">
          <Cpu className="w-5 h-5 text-white" />
        </div>
        <div>
          <h1 className="text-xl 2xl:text-2xl font-bold text-slate-800 tracking-tight">
            PLC Setting
          </h1>
          <p className="text-xs text-slate-500 mt-0.5">
            Camera viewer & cấu hình recipe DelayCamera / DelayReject /
            RejectStreng
          </p>
        </div>
      </div>

      {/* 2-column layout */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 2xl:gap-5 items-stretch">
        <CameraIframe />
        <PLCRecipeForm />
      </div>
    </div>
  );
};

export default PLCSettingsView;
