using Glib.PLCHelpers;
using HslCommunication;

namespace GProject;

/// <summary>
/// Lightweight PLC wrapper that replaces the deleted PLCMonitor/PLCHub. Routes all
/// recipe/register endpoints through <see cref="Global.omronPLC"/> so the backend
/// uses a SINGLE shared PLC connection (no parallel connections from camera
/// pipeline + API endpoints).
/// </summary>
public class PLCMonitorLite
{
    public static readonly PLCMonitorLite Instance = new();

    /// <summary>Connection state, kept compatible with the legacy FE API.</summary>
    public enum PLCConnectionState
    {
        Disconnected,
        Connected,
        Reconnecting,
    }

    public PLCConnectionState State
    {
        get
        {
            var plc = Global.omronPLC;
            if (plc == null) return PLCConnectionState.Disconnected;
            return plc.PLC_STATUS == OmronPLC_Hsl.PLCStatus.Connected
                ? PLCConnectionState.Connected
                : PLCConnectionState.Disconnected;
        }
    }

    public class RecipeResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public short[] Value { get; set; } = Array.Empty<short>();
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public string Value { get; set; } = "";
    }

    /// <summary>Recipe values live in 3 fixed DM cells (matches the legacy project).</summary>
    private const string RecipeCameraAddr = "D300";
    private const string RecipeRejectAddr = "D301";
    private const string RecipeStrengthAddr = "D302";

    public RecipeResult ReadRecipe()
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null)
            {
                return new RecipeResult { Success = false, Error = "PLC not initialized." };
            }

            short[] values = new short[3];
            var r0 = plc.ReadInt16(RecipeCameraAddr, 1);
            if (!r0.IsSuccess || r0.Content == null || r0.Content.Length == 0)
                return new RecipeResult { Success = false, Error = $"Read {RecipeCameraAddr}: {r0.Message}" };
            values[0] = r0.Content[0];

            var r1 = plc.ReadInt16(RecipeRejectAddr, 1);
            if (!r1.IsSuccess || r1.Content == null || r1.Content.Length == 0)
                return new RecipeResult { Success = false, Error = $"Read {RecipeRejectAddr}: {r1.Message}" };
            values[1] = r1.Content[0];

            var r2 = plc.ReadInt16(RecipeStrengthAddr, 1);
            if (!r2.IsSuccess || r2.Content == null || r2.Content.Length == 0)
                return new RecipeResult { Success = false, Error = $"Read {RecipeStrengthAddr}: {r2.Message}" };
            values[2] = r2.Content[0];

            return new RecipeResult { Success = true, Value = values };
        }
        catch (Exception ex)
        {
            return new RecipeResult { Success = false, Error = ex.Message };
        }
    }

    public string WriteRecipe(int delayCamera, int delayReject, int rejectStrength)
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null) return "PLC not initialized.";

            var w0 = plc.Write(RecipeCameraAddr, (short)delayCamera);
            if (!w0.IsSuccess) return $"Write {RecipeCameraAddr}: {w0.Message}";

            var w1 = plc.Write(RecipeRejectAddr, (short)delayReject);
            if (!w1.IsSuccess) return $"Write {RecipeRejectAddr}: {w1.Message}";

            var w2 = plc.Write(RecipeStrengthAddr, (short)rejectStrength);
            if (!w2.IsSuccess) return $"Write {RecipeStrengthAddr}: {w2.Message}";

            return "";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public RegisterResult ReadRegister(string address, string dataType)
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null) return new RegisterResult { Success = false, Error = "PLC not initialized." };

            var r = plc.ReadInt16(address, 1);
            if (!r.IsSuccess || r.Content == null || r.Content.Length == 0)
                return new RegisterResult { Success = false, Error = r.Message };

            return new RegisterResult { Success = true, Value = r.Content[0].ToString() };
        }
        catch (Exception ex)
        {
            return new RegisterResult { Success = false, Error = ex.Message };
        }
    }

    public string WriteRegister(string address, string dataType, string value)
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null) return "PLC not initialized.";

            if (!short.TryParse(value, out var v))
                return $"Value '{value}' is invalid (expected integer).";

            var w = plc.Write(address, v);
            return w.IsSuccess ? "" : w.Message;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}