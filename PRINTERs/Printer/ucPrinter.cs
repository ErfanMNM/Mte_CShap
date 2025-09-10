using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LocateExtentions;
using System.Reflection;
using System.Threading;
using System.IO;

namespace Printer
{
    public partial class ucPrinter : UserControl
    {
        public ucPrinter()
        {
            InitializeComponent();
        }
        aSyncClient client;
        public bool IsConnected = false;
        public bool IsPrinting = false;
        public void LOAD()
        {
            client = new aSyncClient();
            client.IP = "192.168.0.32";
            //client.IP = "127.0.0.1";
            client.ClientCallBack += Client_ClientCallBack;
            PrinterData.DataChanged += PrinterData_DataChanged;
            client.LOAD();
            timer1.Interval = 2000;
            timer1.Start();

            try {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    listBox1.Items.AddRange(lines);
                }
            } catch { }

        }
        int step = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (step == 0)
            {
                client.Send("MON");
            } 
        }
        private void PrinterData_DataChanged(PrinterProperty property, string oldValue, string newValue)
        {
            this.SafeInvoke(new Action(() =>
            {
                switch (property)
                {
                    case PrinterProperty.Monitor:
                        break;
                    case PrinterProperty.Index:
                        lblIndex.Text = newValue;
                        break;
                    case PrinterProperty.Speed:
                        lblBuffer.Text = $"Tốc độ: {newValue} mét / phút";
                        break;
                    case PrinterProperty.PrinterStatus:                       
                        if (newValue.ToLower().Contains("start"))
                        {
                            btnPrint.BackColor = Color.Red;
                            btnPrint.Text = "Dừng in";
                            Logs("Máy in đang bật!");
                            IsPrinting = true;
                        }
                        else if (newValue.ToLower().Contains("stop"))
                        {
                            btnPrint.BackColor = Color.Green;
                            btnPrint.Text = "Bắt đầu in";
                            Logs("Máy in đã tắt!");
                            IsPrinting = false;
                        }
                        else if (newValue.ToLower().Contains("printing"))
                        {
                            btnPrint.BackColor = Color.Red;
                            btnPrint.Text = "Dừng in";
                            Logs("Máy in đang in...");
                            IsPrinting = true;
                        }
                        else if (string.IsNullOrEmpty(newValue))
                        {
                            btnPrint.BackColor = Color.Red;
                            btnPrint.Text = "Dừng in";
                            Logs("Máy in đang đợi data...");
                            SendData();
                            IsPrinting = true;
                        }
                        break;
                    case PrinterProperty.PrintedPages:
                        break;
                    case PrinterProperty.PrintheadStatus:
                        break;
                    case PrinterProperty.InkVolume:
                        lblInk.Text = $"Mực : {newValue}";
                        break;
                    case PrinterProperty.InkType:
                        break;
                    case PrinterProperty.DatabaseIndex:
                        break;
                    case PrinterProperty.DateTime:
                        break;
                    case PrinterProperty.TemplateName:
                        lblJob.Text = newValue;
                        break;
                    default:
                        break;
                }
            }));
        }

        private void Client_ClientCallBack(enumClient eAE, string s)
        {
            switch (eAE)
            {
                case enumClient.CONNECTED:
                    IsConnected=true;
                    this.SafeInvoke(new Action(() =>
                    {
                        label3.Text = "Kết nối máy in thành công";
                        Logs("Kết nối máy in thành công");
                    }));
                    break;
                case enumClient.DISCONNECTED:
                    IsConnected = false;
                    this.SafeInvoke(new Action(() =>
                    {
                        label3.Text = "Kết nối máy in thất bại";
                        Logs("Kết nối máy in thất bại");
                    }));
                    break;
                case enumClient.RECEIVED:
                    if (s.Contains("MON"))
                    {
                        PrinterData.ParseAndSetData(s);
                    }
                    else if (s.Contains("RSFP"))
                    {
                        ParsedPrinterInfo a = ParsedPrinterInfo.ParseSpecialData(s, out int count);
                        this.SafeInvoke(new Action(() =>
                        {
                            lblCounter.Text = $"{a.PrintedPage}";
                            lblCountPrinter.Text = $"{a.TotalPDData}";
                        }));
                        while (count > 0)
                        {
                            Send1Data();
                            count--;
                        }
                    }
                    else if(s.Contains("STAR;SYSN;"))
                    {
                        string ss = PrinterErrorParser.ParseErrorCode(s);
                        if(ss != null)
                        {
                            this.SafeInvoke(new Action(() =>
                            {
                                Logs(ss);
                            }));
                        }
                    }                   
                    break;
                default:
                    break;
            }
        }
        private async void btnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;

            if (client != null && client.Connected)
            {
                if (btnPrint.BackColor == Color.Green)
                {
                    client.Send("START");
                }
                else
                {
                    client.Send("STOP");
                }
                await Task.Delay(5000); // Không chặn UI

                btnPrint.Enabled = true;
            }
        }
        private void Send1Data()
        {
            if (client != null && client.Connected)
            {
                if (DATA.Count > 0)
                {
                    string data = DATA[0];
                    DATA.RemoveAt(0);
                    client.Send($"DATA;{data};{index}");
                    index++;
                    System.Threading.Thread.Sleep(10);
                }
                if (DATA.Count == 0)
                {
                    this.SafeInvoke(new Action(() =>
                    {
                        Logs("Hết data!");
                    }));
                }
                lblTotal.SafeInvoke(new Action(() =>
                {
                    lblTotal.Text = DATA.Count.ToString();
                }));
            }
        }
        long index = 0;
        private void SendData()
        {
            if (client != null && client.Connected)
            {
                index = 1;
                int i = 0;
                while (DATA.Count > 0)
                {
                    string data = DATA[0];
                    DATA.RemoveAt(0);
                    client.Send($"DATA;{data};{index}");
                    Thread.Sleep(10);
                    index++;
                    i++;
                    if (i >= 20) break;
                }
                this.SafeInvoke(new Action(() => { Logs($"Đã gửi {i} data!"); }));
                if (DATA.Count == 0)
                {
                    this.SafeInvoke(new Action(() =>
                    {
                        Logs("Hết data!");
                    }));
                }
                lblTotal.SafeInvoke(new Action(() =>
                {
                    lblTotal.Text = DATA.Count.ToString();
                }));
            }
        }
        List<string> DATA = new List<string>();
        long Total = 0;
        public void AddData(List<string> data)
        {
            index = 1;
            DATA.Clear();
            DATA.AddRange(data);
            Total = DATA.Count;
            this.SafeInvoke(new Action(() => { Logs("Đã thêm dữ liệu. Đang khởi động lại máy in..."); }));
            if (client != null && client.Connected)
            {
                client.Send("STOP");
                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    client.Send("START");
                });
            }
        }
        private void Logs(string message)
        {
            listBox1.SafeInvoke(new Action(() =>
            {
                // Kiểm tra số lượng item trong ListBox
                if (listBox1.Items.Count >= 100)
                {
                    // Xóa item cuối cùng
                    listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
                }

                // Thêm item mới vào đầu
                listBox1.Items.Insert(0, $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - {message}");
            }));
            try { File.WriteAllLines(filePath, listBox1.Items.Cast<string>()); } catch { }
        }
        string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Log.txt");

        int count = 0;
        private void button3_Click(object sender, EventArgs e)
        {
            Logs($"Số lần đã in: {lblCounter.Text}. Dữ liệu trên máy in: {lblCountPrinter.Text}. Dữ liệu còn lại: {lblTotal.Text}. {lblInk.Text}. {lblBuffer.Text}");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ListBox lstReceive1 = sender as ListBox;
            if (lstReceive1.SelectedItem != null) // Kiểm tra có item nào được chọn không
            {
                string selectedText = lstReceive1.SelectedItem.ToString();
                MessageBox.Show(selectedText, "Chi tiết dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Logs("------------------------------------------------------------");
        }
    }

    public enum PrinterProperty
    {
        Monitor,
        Index,
        Speed,
        PrinterStatus,
        PrintedPages,
        PrintheadStatus,
        InkVolume,
        InkType,
        DatabaseIndex,
        DateTime,
        TemplateName
    }
    public static class ControlExtensions
    {
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }


    public static class PrinterData
    {
        public static string Monitor { get; set; } = "";
        public static int Index { get; set; } = 0;
        public static double Speed { get; set; } = 0;
        public static string PrinterStatus { get; set; } = "-1";
        public static int PrintedPages { get; set; } = 0;
        public static string PrintheadStatus { get; set; } = "";
        public static string InkVolume { get; set; } = "";
        public static string InkType { get; set; } = "";
        public static int DatabaseIndex { get; set; } = 0;
        public static string DateTime { get; set; } = "";
        public static string TemplateName { get; set; } = "";

        public delegate void DataChangedEventHandler(PrinterProperty property, string oldValue, string newValue);
        public static event DataChangedEventHandler DataChanged;

        public static void OnDataChanged(PrinterProperty property, string oldValue, string newValue)
        {
            DataChanged?.Invoke(property, oldValue, newValue);
        }

        public static void ParseAndSetData(string rawData)
        {
            string cleanedData = rawData.TrimStart('\u0002').TrimEnd('\u0003');
            string[] parts = cleanedData.Split(';');

            try
            {
                if (parts.Length >= 1 && parts[0] != Monitor)
                {
                    OnDataChanged(PrinterProperty.Monitor, Monitor, parts[0]);
                    Monitor = parts[0];
                }

                if (parts.Length >= 2 && int.TryParse(parts[1], out int newIndex) && newIndex != Index)
                {
                    OnDataChanged(PrinterProperty.Index, Index.ToString(), newIndex.ToString());
                    Index = newIndex;
                }

                if (parts.Length >= 3 && double.TryParse(parts[2], out double rawSpeed))
                {
                    double newSpeed = rawSpeed / 1000;
                    if (newSpeed != Speed)
                    {
                        OnDataChanged(PrinterProperty.Speed, Speed.ToString(), newSpeed.ToString());
                        Speed = newSpeed;
                    }
                }

                if (parts.Length >= 4 && parts[3] != PrinterStatus)
                {
                    OnDataChanged(PrinterProperty.PrinterStatus, PrinterStatus, parts[3]);
                    PrinterStatus = parts[3];
                }

                if (parts.Length >= 5 && int.TryParse(parts[4], out int newPrintedPages) && newPrintedPages != PrintedPages)
                {
                    OnDataChanged(PrinterProperty.PrintedPages, PrintedPages.ToString(), newPrintedPages.ToString());
                    PrintedPages = newPrintedPages;
                }

                if (parts.Length >= 6 && parts[5] != PrintheadStatus)
                {
                    OnDataChanged(PrinterProperty.PrintheadStatus, PrintheadStatus, parts[5]);
                    PrintheadStatus = parts[5];
                }

                if (parts.Length >= 7 && parts[6] != InkVolume)
                {
                    string[] parts3 = parts[6].Split('\u001d');
                    if (parts3.Length == 2)
                    {
                        // Chuyển đổi sang số thực
                        if (double.TryParse(parts3[0], out double volume)&& double.TryParse(parts3[1], out double volume2))
                        {
                            OnDataChanged(PrinterProperty.InkVolume, InkVolume, $"[ {(volume/1000):F2} / 42 ml ] [ {(volume2 / 1000):F2} / 42 ml ]");
                        }
                    }                    
                    InkVolume = parts[6];
                }

                if (parts.Length >= 8 && parts[7] != InkType)
                {
                    OnDataChanged(PrinterProperty.InkType, InkType, parts[7]);
                    InkType = parts[7];
                }

                if (parts.Length >= 9 && int.TryParse(parts[8], out int newDatabaseIndex) && newDatabaseIndex != DatabaseIndex)
                {
                    OnDataChanged(PrinterProperty.DatabaseIndex, DatabaseIndex.ToString(), newDatabaseIndex.ToString());
                    DatabaseIndex = newDatabaseIndex;
                }

                if (parts.Length >= 10 && parts[9] != DateTime)
                {
                    OnDataChanged(PrinterProperty.DateTime, DateTime, parts[9]);
                    DateTime = parts[9];
                }

                if (parts.Length >= 11 && parts[10] != TemplateName)
                {
                    OnDataChanged(PrinterProperty.TemplateName, TemplateName, parts[10]);
                    TemplateName = parts[10];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing data: " + ex.Message);
            }
        }

    }
    public class ParsedPrinterInfo
    {
        public long PrintedPage { get; set; } = -1;
        public long TotalPDData { get; set; } = -1;
        public string Fields { get; set; } = "";

        public static ParsedPrinterInfo ParseSpecialData(string rawData, out int sendcout)
        {
            sendcout = 0;
            var result = new ParsedPrinterInfo();

            // 1. Loại bỏ STX và ETX
            string cleaned = rawData.TrimStart('\u0002').TrimEnd('\u0003');

            // 2. Tách các phần bằng dấu ';'
            string[] parts = cleaned.Split(';');

            if (parts.Length < 5) return result;

            // 3. Tách PrintedPage và TotalPDData
            string[] pageInfo = parts[1].Split('/');
            if (pageInfo.Length == 2 &&
                long.TryParse(pageInfo[0], out long printedPage) &&
                long.TryParse(pageInfo[1], out long totalPDData))
            {
                result.PrintedPage = printedPage;
                result.TotalPDData = totalPDData;
            }

            string[] parts2 = parts[1].Split('/');
            if (parts2.Length == 2)
            {
                if(int.TryParse(parts[4], out int tem0)&& int.TryParse(parts2[1], out int tem1))
                {
                    sendcout = 20 - tem1 + tem0;
                    if (sendcout > 0)
                    {

                    }
                }
               
            }

            // 4. Lấy các field sau "DATA"
            parts.Skip(3);

            return result;
        }

    }
    public static class PrinterErrorParser
    {
        private static readonly Dictionary<string, string> ErrorMessages = new Dictionary<string, string>
    {
        {"000", "Không xác định"},
        {"001", "Mở mẫu in thất bại (không tồn tại, đang mở mẫu khác,...)"},
        {"002", "Trang bắt đầu hoặc kết thúc không hợp lệ"},
        {"003", "Chưa chọn đầu in"},
        {"004", "Vượt giới hạn tốc độ"},
        {"005", "Đầu in bị ngắt kết nối"},
        {"006", "Không nhận diện được đầu in"},
        {"007", "Không có hộp mực"},
        {"008", "Hộp mực không hợp lệ"},
        {"009", "Hết mực"},
        {"010", "Hộp mực bị khóa"},
        {"011", "Phiên bản không hợp lệ"},
        {"012", "Đầu in không chính xác"},
        {"013", "Bắt đầu xử lý in"},
        {"014", "Giá trị vòng lặp không hợp lệ"},
        {"015", "Mực yếu (sắp hết mực)"}
    };

        public static string ParseErrorCode(string rawData)
        {
            // 1. Loại bỏ STX và ETX
            string cleaned = rawData.TrimStart('\u0002').TrimEnd('\u0003');

            // 2. Kiểm tra định dạng
            if (cleaned.StartsWith("STAR;SYSN;"))
            {
                string[] parts = cleaned.Split(';');
                if (parts.Length == 3)
                {
                    string errorCode = parts[2];
                    if (ErrorMessages.TryGetValue(errorCode, out string message))
                    {
                        return $"Mã lỗi {errorCode}: {message}";
                    }
                    else
                    {
                        return $"Mã lỗi không xác định: {errorCode}";
                    }
                }
            }

            return null; // Không phải định dạng lỗi
        }
    }

}
