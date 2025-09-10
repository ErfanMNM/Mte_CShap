using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum PrintButtonState
{
    PrintingStopped = 0,         // Printing Stopped – idle
    ConfiguringPrinters = 1,      // Configuring printer(s)
    Printing = 2,                 // Printing
    UnknownState3 = 3,            // Không có mô tả rõ ràng
    UnknownState4 =4,            // Không có mô tả rõ ràng
    PrinterOfflineOrNoJob = 5,    // Printer off-line, or no job is loaded.
    PausedReadyOrStop =6         // Paused: Ready to Print or Stop.
}

public class XXX
{
    public bool Connect = false;
    public int Count = 0;
}
public class PrinterStatus
{
    public static string GetPrintButtonDescription(PrintButtonState state)
    {
        switch (state)
        {
            case PrintButtonState.PrintingStopped:
                return "Printing Stopped – idle";
            case PrintButtonState.ConfiguringPrinters:
                return "Configuring printer(s).";
            case PrintButtonState.Printing:
                return "Printing";
            case PrintButtonState.UnknownState3:
                return "Unknown State (3)";
            case PrintButtonState.UnknownState4:
                return "Unknown State (4)";
            case PrintButtonState.PrinterOfflineOrNoJob:
                return "Printer off-line, or no job is loaded.";
            case PrintButtonState.PausedReadyOrStop:
                return "Paused: Ready to Print or Stop.";
            default:
                return "Unknown State";
        }
    }

    // 🔹 Biến static (Dùng chung cho toàn bộ chương trình)
    public static XXX G_IsClient { get; set; }
    public static bool G_Pringting { get; set; }
    public static int G_Speed { get; set; }
    public static string G_SpeedUnits { get; set; }
    public static int G_PiecesPerHour { get; set; }
    public static int G_LineCounter { get; set; }
    public static int G_JobCounter { get; set; }
    public static int G_SimpleCounter { get; set; }
    public static int G_PCBuffers { get; set; }
    public static int G_PrinterBuffers { get; set; }
    public static string G_InkTimeToEmpty { get; set; }
    public static double G_InkVolumeToEmpty { get; set; }
    public static string G_InkLowAlarmActive { get; set; }
    public static string G_InkEmptyAlarmActive { get; set; }
    public static string G_PrinterStatusL1 { get; set; }
    public static string G_PrinterStatusL2 { get; set; }
    public static string G_PrinterStatusL3 { get; set; }
    public static string G_PrinterStatusL4 { get; set; }
    public static string G_PrinterStatusL5 { get; set; }

    // 🔹 Biến instance (Thuộc tính của từng object PrinterStatus)
    public string Printing { get; set; }
    public int Speed { get; set; }
    public string SpeedUnits { get; set; }
    public int PiecesPerHour { get; set; }
    public int LineCounter { get; set; }
    public int JobCounter { get; set; }
    public int SimpleCounter { get; set; }
    public int PCBuffers { get; set; }
    public int PrinterBuffers { get; set; }
    public string InkTimeToEmpty { get; set; }
    public double InkVolumeToEmpty { get; set; }
    public string InkLowAlarmActive { get; set; }
    public string InkEmptyAlarmActive { get; set; }
    public string PrinterStatusL1 { get; set; }
    public string PrinterStatusL2 { get; set; }
    public string PrinterStatusL3 { get; set; }
    public string PrinterStatusL4 { get; set; }
    public string PrinterStatusL5 { get; set; }

    // 🔹 Constructor mặc định
    public PrinterStatus() { }

    // 🔹 Hàm phân tích dữ liệu nhận được từ server
    public static PrinterStatus Parse(string data)
    {
        PrinterStatus status = new PrinterStatus();
        string[] keyValuePairs = data.Split(',');

        foreach (string pair in keyValuePairs)
        {
            string[] parts = pair.Split('=');
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();

                // Gán giá trị vào biến instance và biến static
                switch (key)
                {
                    case "Printing":
                        status.Printing = value;
                        G_Pringting = value.ToLower() == "yes"; // Cập nhật biến static
                        break;
                    case "Speed":
                        status.Speed = int.TryParse(value, out int speed) ? speed : 0;
                        G_Speed = status.Speed;
                        break;
                    case "SpeedUnits":
                        status.SpeedUnits = value;
                        G_SpeedUnits = value;
                        break;
                    case "PiecesPerHour":
                        status.PiecesPerHour = int.TryParse(value, out int pph) ? pph : 0;
                        G_PiecesPerHour = status.PiecesPerHour;
                        break;
                    case "LineCounter":
                        status.LineCounter = int.TryParse(value, out int lc) ? lc : 0;
                        G_LineCounter = status.LineCounter;
                        break;
                    case "JobCounter":
                        status.JobCounter = int.TryParse(value, out int jc) ? jc : 0;
                        G_JobCounter = status.JobCounter;
                        break;
                    case "SimpleCounter":
                        status.SimpleCounter = int.TryParse(value, out int sc) ? sc : 0;
                        G_SimpleCounter = status.SimpleCounter;
                        break;
                    case "PCBuffers":
                        status.PCBuffers = int.TryParse(value, out int pcb) ? pcb : 0;
                        G_PCBuffers = status.PCBuffers;
                        break;
                    case "PrinterBuffers":
                        status.PrinterBuffers = int.TryParse(value, out int pb) ? pb : 0;
                        G_PrinterBuffers = status.PrinterBuffers;
                        break;
                    case "InkTimeToEmpty":
                        status.InkTimeToEmpty = value;
                        G_InkTimeToEmpty = value;
                        break;
                    case "InkVolumeToEmpty":
                        status.InkVolumeToEmpty = double.TryParse(value, out double ivte) ? ivte : 0.0;
                        G_InkVolumeToEmpty = status.InkVolumeToEmpty;
                        break;
                    case "InkLowAlarmActive":
                        status.InkLowAlarmActive = value;
                        G_InkLowAlarmActive = value;
                        break;
                    case "InkEmptyAlarmActive":
                        status.InkEmptyAlarmActive = value;
                        G_InkEmptyAlarmActive = value;
                        break;
                    case "PrinterStatusL1":
                        status.PrinterStatusL1 = value;
                        G_PrinterStatusL1 = value;
                        break;
                    case "PrinterStatusL2":
                        status.PrinterStatusL2 = value;
                        G_PrinterStatusL2 = value;
                        break;
                    case "PrinterStatusL3":
                        status.PrinterStatusL3 = value;
                        G_PrinterStatusL3 = value;
                        break;
                    case "PrinterStatusL4":
                        status.PrinterStatusL4 = value;
                        G_PrinterStatusL4 = value;
                        break;
                    case "PrinterStatusL5":
                        status.PrinterStatusL5 = value;
                        G_PrinterStatusL5 = value;
                        break;
                }
            }
        }

        return status;
    }

    // 🔹 Hàm hiển thị thông tin
    public override string ToString()
    {
        return $"LPrinting={Printing}, Speed={Speed}, SpeedUnits={SpeedUnits}, PiecesPerHour={PiecesPerHour}, " +
               $"LineCounter={LineCounter}, JobCounter={JobCounter}, SimpleCounter={SimpleCounter}, " +
               $"PCBuffers={PCBuffers}, PrinterBuffers={PrinterBuffers}, InkTimeToEmpty={InkTimeToEmpty}, " +
               $"InkVolumeToEmpty={InkVolumeToEmpty}, InkLowAlarmActive={InkLowAlarmActive}, " +
               $"InkEmptyAlarmActive={InkEmptyAlarmActive}, PrinterStatusL1={PrinterStatusL1}, " +
               $"PrinterStatusL2={PrinterStatusL2}, PrinterStatusL3={PrinterStatusL3}, " +
               $"PrinterStatusL4={PrinterStatusL4}, PrinterStatusL5={PrinterStatusL5}";
    }
}

