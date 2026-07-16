using Glib.PLCHelpers;
using HslCommunication;

namespace GProject;

/// <summary>
/// Wrapper PLC nhẹ thay cho PLCMonitor/PLCHub đã bị xóa. Mọi endpoint recipe/register
/// đều đi qua <see cref="Global.omronPLC"/> nên backend chỉ có DUY NHẤT một kết nối PLC
/// — camera pipeline và API endpoints cùng chia sẻ instance này.
/// </summary>
public class PLCMonitorLite
{
    public static readonly PLCMonitorLite Instance = new();

    /// <summary>Trạng thái kết nối, giữ tương thích với FE API cũ.</summary>
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
            return Global.PLC_STATUS == OmronPLC_Hsl.PLCStatus.Connected
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

    /// <summary>Recipe dùng 3 ô DM cố định (giống project cũ).</summary>
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
                return new RecipeResult { Success = false, Error = "PLC chưa khởi tạo." };
            }

            short[] values = new short[3];
            var r0 = plc.ReadInt16(RecipeCameraAddr, 1);
            if (!r0.IsSuccess || r0.Content == null || r0.Content.Length == 0)
                return new RecipeResult { Success = false, Error = $"Đọc {RecipeCameraAddr} thất bại: {r0.Message}" };
            values[0] = r0.Content[0];

            var r1 = plc.ReadInt16(RecipeRejectAddr, 1);
            if (!r1.IsSuccess || r1.Content == null || r1.Content.Length == 0)
                return new RecipeResult { Success = false, Error = $"Đọc {RecipeRejectAddr} thất bại: {r1.Message}" };
            values[1] = r1.Content[0];

            var r2 = plc.ReadInt16(RecipeStrengthAddr, 1);
            if (!r2.IsSuccess || r2.Content == null || r2.Content.Length == 0)
                return new RecipeResult { Success = false, Error = $"Đọc {RecipeStrengthAddr} thất bại: {r2.Message}" };
            values[2] = r2.Content[0];

            return new RecipeResult { Success = true, Value = values };
        }
        catch (Exception ex)
        {
            return new RecipeResult { Success = false, Error = $"Lỗi đọc recipe: {ex.Message}" };
        }
    }

    public string WriteRecipe(int delayCamera, int delayReject, int rejectStrength)
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null) return "PLC chưa khởi tạo.";

            var w0 = plc.Write(RecipeCameraAddr, (short)delayCamera);
            if (!w0.IsSuccess) return $"Ghi {RecipeCameraAddr} thất bại: {w0.Message}";

            var w1 = plc.Write(RecipeRejectAddr, (short)delayReject);
            if (!w1.IsSuccess) return $"Ghi {RecipeRejectAddr} thất bại: {w1.Message}";

            var w2 = plc.Write(RecipeStrengthAddr, (short)rejectStrength);
            if (!w2.IsSuccess) return $"Ghi {RecipeStrengthAddr} thất bại: {w2.Message}";

            return "";
        }
        catch (Exception ex)
        {
            return $"Lỗi ghi recipe: {ex.Message}";
        }
    }

    public RegisterResult ReadRegister(string address, string dataType)
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null) return new RegisterResult { Success = false, Error = "PLC chưa khởi tạo." };

            var r = plc.ReadInt16(address, 1);
            if (!r.IsSuccess || r.Content == null || r.Content.Length == 0)
                return new RegisterResult { Success = false, Error = $"Đọc {address} thất bại: {r.Message}" };

            return new RegisterResult { Success = true, Value = r.Content[0].ToString() };
        }
        catch (Exception ex)
        {
            return new RegisterResult { Success = false, Error = $"Lỗi đọc thanh ghi: {ex.Message}" };
        }
    }

    public string WriteRegister(string address, string dataType, string value)
    {
        try
        {
            var plc = Global.omronPLC?.plc;
            if (plc == null) return "PLC chưa khởi tạo.";

            if (!short.TryParse(value, out var v))
                return $"Giá trị '{value}' không hợp lệ (cần số nguyên).";

            var w = plc.Write(address, v);
            return w.IsSuccess ? "" : $"Ghi {address} thất bại: {w.Message}";
        }
        catch (Exception ex)
        {
            return $"Lỗi ghi thanh ghi: {ex.Message}";
        }
    }
}