import React, { useState, useEffect } from "react";
import { Cpu, Power, RefreshCw } from "lucide-react";
import { plcApi } from "../../services/plcApi";

const PLCSimulation: React.FC = () => {
  const [enabled, setEnabled] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchStatus();
  }, []);

  const fetchStatus = async () => {
    try {
      const data = await plcApi.getSimulationStatus();
      setEnabled(data.simulation);
    } catch (err) {
      console.error("Failed to fetch simulation status:", err);
    }
  };

  const handleToggle = async () => {
    setLoading(true);
    setError(null);
    try {
      const newState = !enabled;
      await plcApi.toggleSimulation(newState);
      setEnabled(newState);
    } catch (err: any) {
      setError(err.message || "Failed to toggle simulation");
      console.error("Toggle simulation error:", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-slate-200 p-5 flex flex-col gap-4">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className={`w-9 h-9 rounded-lg flex items-center justify-center transition-colors ${
          enabled 
            ? "bg-green-100" 
            : "bg-slate-100"
        }`}>
          <Cpu className={`w-4 h-4 ${enabled ? "text-green-600" : "text-slate-500"}`} />
        </div>
        <div>
          <h3 className="text-sm font-semibold text-slate-800">
            PLC Simulation
          </h3>
          <p className="text-xs text-slate-500">
            {enabled ? "127.0.0.1:9600" : "Disabled"}
          </p>
        </div>
      </div>

      {/* Toggle Button */}
      <div className="flex items-center justify-between">
        <div className="flex flex-col gap-1">
          <span className="text-sm font-medium text-slate-700">
            Enable Simulation
          </span>
          <span className="text-xs text-slate-500">
            Connect to local PLC simulator (127.0.0.1:9600)
          </span>
        </div>
        <button
          onClick={handleToggle}
          disabled={loading}
          className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
            enabled ? "bg-green-500" : "bg-slate-300"
          } ${loading ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}`}
        >
          {loading ? (
            <RefreshCw className="w-4 h-4 text-white animate-spin mx-auto" />
          ) : (
            <span
              className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-sm transition-transform ${
                enabled ? "translate-x-6" : "translate-x-1"
              }`}
            />
          )}
        </button>
      </div>

      {/* Status Indicator */}
      <div className={`flex items-center gap-2 px-3 py-2 rounded-lg ${
        enabled ? "bg-green-50" : "bg-slate-50"
      }`}>
        <div className={`w-2 h-2 rounded-full ${
          enabled ? "bg-green-500 animate-pulse" : "bg-slate-400"
        }`} />
        <span className={`text-xs font-medium ${
          enabled ? "text-green-700" : "text-slate-500"
        }`}>
          {enabled ? "Simulation Running" : "Simulation Off"}
        </span>
      </div>

      {/* Error Message */}
      {error && (
        <div className="px-3 py-2 rounded-lg bg-red-50 border border-red-200">
          <span className="text-xs text-red-600">{error}</span>
        </div>
      )}

      {/* Info */}
      <div className="text-xs text-slate-500 p-3 rounded-lg bg-blue-50 border border-blue-100">
        <strong>Note:</strong> When simulation is enabled, the app connects to a local 
        PLC simulator on localhost:9600 instead of the real PLC. Useful for testing 
        without hardware.
      </div>
    </div>
  );
};

export default PLCSimulation;
