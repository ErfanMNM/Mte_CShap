using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace NorwixV2
{
    public class ReciverClient
    {
        public delegate void EventHandler(Enum_ConnectionEventClient e, object obj);
        public event EventHandler ConnectionEventCallBack;
        public delegate void EventHandler2(PrintButtonState e, object obj);
        public event EventHandler2 ButtonEventCallBack;
        BackgroundWorker workerCheck = new BackgroundWorker();
        aSyncClient client;
        public bool Connected = false;
        string ip = "127.0.0.1";
        public string IP { get { return ip; } set { client.IP = ip = value; } }
        public ReciverClient()
        {
            client = new aSyncClient();
            client.IP = IP;
            //client.IP = "127.0.0.1";
            client.Port = 10002;
        }
        public void Start()
        {
            client.ClientCallBack += Client_ClientCallBack;
            client.LOAD();
            workerCheck.DoWork += WorkerCheck_DoWork;
            workerCheck.RunWorkerAsync();
        }

        private void WorkerCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (Connected)
                {
                    PrinterStatus.G_IsClient.Count--;

                    if (PrinterStatus.G_IsClient.Count <= 0)
                    {
                        PrinterStatus.G_IsClient.Connect = false;
                        client.Disconnect();
                    }
                }
                Thread.Sleep(1000);
            }
        }

        public void Send(string data)
        {
            client.Send(data);
        }
        string jobname = "";
        int ink1, ink2, ink3, ink4 = 0;
        int counter = 0;
        int button = 0;
        public int Enable = 5;
        bool reciver = false;
        private void Client_ClientCallBack(enumClient eAE, string _strData)
        {
            if (ConnectionEventCallBack != null)
            {
                switch (eAE)
                {
                    case enumClient.CONNECTED:
                        ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.CONNECTED, _strData);
                        Connected = true;
                        PrinterStatus.G_IsClient.Count = 5;
                        break;
                    case enumClient.DISCONNECTED:
                        ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.DISCONNECTED, _strData);
                        Connected = false;
                        break;
                    case enumClient.RECEIVED:
                        try
                        {
                            reciver = true;
                            PrinterStatus.G_IsClient.Count = 5;
                            ProcessData(_strData);
                        }
                        catch
                        {
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private bool Call(ref string old, string current)
        {
            if (old != current)
            {
                old = current;
                return true;
            }
            return false;
        }
        private bool Call(ref int old, int current)
        {
            if (old != current)
            {
                old = current;
                return true;
            }
            return false;
        }
        // Biến tạm lưu nội dung chưa hoàn chỉnh
        private string temp = "";
        private List<string> parts = new List<string>();
        private List<int> li = new List<int>();
        private string jobname2 = "";

        void ProcessData(string data)
        {
            // Ghép nội dung mới với biến tạm
            temp += data;

            while (!string.IsNullOrEmpty(temp))
            {
                // Tìm vị trí bắt đầu
                int startIndex = temp.IndexOf('\u0002');
                if (startIndex == -1)
                    break; // Không tìm thấy bắt đầu

                // Tìm vị trí kết thúc
                int endIndex = temp.IndexOf('\u0003', startIndex);

                while (endIndex != -1)
                {
                    // Kiểm tra ký tự sau '\u0003'
                    if (endIndex + 1 >= temp.Length || temp[endIndex + 1] == '\u0002')
                    {
                        // Đoạn hoàn chỉnh
                        string part = temp.Substring(startIndex, endIndex - startIndex + 1);

                        //li = new List<int>();
                        //for (int i = 0; i < part.Length; i++)
                        //{
                        //    li.Add((int)part[i]);
                        //}
                        if (!CheckButton(part))
                        {
                            ErrorOrWarning(part);

                            CheckStatus(part);

                            CheckJob(part);

                            CheckInk(part);
                        }

                        // Cập nhật temp (bỏ đoạn đã xử lý)
                        temp = temp.Substring(endIndex + 1);
                        break;
                    }
                    else
                    {
                        // Không hợp lệ, tìm endIndex tiếp theo
                        endIndex = temp.IndexOf('\u0003', endIndex + 1);
                    }
                }

                if (endIndex == -1)
                {
                    // Không tìm thấy endIndex hợp lệ, chờ thêm dữ liệu
                    break;
                }
            }
        }
        void ErrorOrWarning(string part)
        {
            Encoding encoding = Encoding.GetEncoding(28591);
            byte[] byteArray = encoding.GetBytes(part);
            string ss = BitConverter.ToString(byteArray).Replace("-", " ");       

            bool isValid = CheckHexStringFormatError(ss);
            if (isValid)
            {

                // Tìm vị trí bắt đầu và kết thúc đoạn text
                int startIndex1 = part.IndexOf('\u0001') + 1; // Sau ký tự '\u0001'
                int endIndex1 = part.Length - 2;   // Trước ký tự '\u0003'
                if (startIndex1 > 0 && endIndex1 > startIndex1)
                {
                    string text = part.Substring(startIndex1 + 1, endIndex1 - startIndex1);
                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.ERROR_OR_WARNING, text);
                    //parts.Add(text);
                    //Log(text);
                }

            }
            else if (part.Contains("error") || part.Contains("failed") || part.Contains("no data") || part.Contains("fault") || part.Contains("job") || part.Contains("NO"))
            {
                // Tìm vị trí bắt đầu và kết thúc đoạn text
                int startIndex1 = part.IndexOf('\u0001') + 1; // Sau ký tự '\u0001'
                int endIndex1 = part.Length - 2;   // Trước ký tự '\u0003'
                if (startIndex1 > 0 && endIndex1 > startIndex1)
                {
                    string text = part.Substring(startIndex1 + 1, endIndex1 - startIndex1);
                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.ERROR_OR_WARNING, text);
                    //parts.Add(text);
                    //Log(text);
                }
            }
        }
        PrintButtonState ButtonState = (PrintButtonState)(-1);
        private bool CheckButton(string hexString)
        {
            Encoding encoding = Encoding.GetEncoding(28591);
            byte[] byteArray = encoding.GetBytes(hexString);
            hexString = BitConverter.ToString(byteArray).Replace("-", " ");
            string pattern = @"02 05 00 00 00 00 ([0-9A-Fa-f]{2}) 93 ([0-9A-Fa-f]{2}) 03";

            Match match = Regex.Match(hexString, pattern);

            if (match.Success)
            {
                string xx1 = match.Groups[1].Value; // Lấy giá trị xx thứ 1
                string xx2 = match.Groups[2].Value; // Lấy giá trị xx thứ 2

                if (int.TryParse(xx2, out int value) && Enum.IsDefined(typeof(PrintButtonState), value))
                {
                    PrintButtonState state = (PrintButtonState)value;
                    
                    if (ButtonEventCallBack != null && ButtonState != state)
                    {
                        ButtonState = state;
                        ButtonEventCallBack.Invoke(state, PrinterStatus.GetPrintButtonDescription(state));                      
                    }
                   
                }
            }
            return match.Success;
        }
        private PrinterStatus previousStatus = new PrinterStatus();
        private void CheckStatus(string data)
        {
            // Hiển thị dạng chuỗi, loại bỏ ký tự điều khiển                    
            string filteredString = new string(data.Where(c => c >= 32 || c == '\n' || c == '\r').ToArray());
            if (filteredString.Contains("Printing=") && filteredString.Contains("PrinterStatusL5"))
            {
                // Tìm phần bắt đầu bằng "Printing" và lấy toàn bộ chuỗi từ đó trở đi
                string result = GetPrintingData(filteredString);

                PrinterStatus currentStatus = PrinterStatus.Parse(result);

                // Nếu đây là lần đầu tiên kiểm tra, lưu trạng thái và thoát

                // Kiểm tra sự thay đổi
                List<string> changes = GetChanges(previousStatus, currentStatus);

                // Nếu có thay đổi, hiển thị thông báo
                if (changes.Count > 0)
                {
                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.STATUS, currentStatus);
                }

                // Cập nhật trạng thái trước đó
                previousStatus = currentStatus;
            }
        }
        string GetPrintingData(string input)
        {
            int index = input.IndexOf("Printing");
            return index != -1 ? input.Substring(index) : string.Empty;
        }

        // 🔹 Hàm này sẽ so sánh tất cả thuộc tính của PrinterStatus và trả về danh sách thay đổi
        private List<string> GetChanges(PrinterStatus oldStatus, PrinterStatus newStatus)
        {
            List<string> changes = new List<string>();

            CompareProperty(nameof(oldStatus.Printing), oldStatus.Printing, newStatus.Printing, changes);
            CompareProperty(nameof(oldStatus.Speed), oldStatus.Speed, newStatus.Speed, changes);
            CompareProperty(nameof(oldStatus.SpeedUnits), oldStatus.SpeedUnits, newStatus.SpeedUnits, changes);
            CompareProperty(nameof(oldStatus.PiecesPerHour), oldStatus.PiecesPerHour, newStatus.PiecesPerHour, changes);
            CompareProperty(nameof(oldStatus.LineCounter), oldStatus.LineCounter, newStatus.LineCounter, changes);
            CompareProperty(nameof(oldStatus.JobCounter), oldStatus.JobCounter, newStatus.JobCounter, changes);
            CompareProperty(nameof(oldStatus.SimpleCounter), oldStatus.SimpleCounter, newStatus.SimpleCounter, changes);
            CompareProperty(nameof(oldStatus.PCBuffers), oldStatus.PCBuffers, newStatus.PCBuffers, changes);
            CompareProperty(nameof(oldStatus.PrinterBuffers), oldStatus.PrinterBuffers, newStatus.PrinterBuffers, changes);
            CompareProperty(nameof(oldStatus.InkTimeToEmpty), oldStatus.InkTimeToEmpty, newStatus.InkTimeToEmpty, changes);
            CompareProperty(nameof(oldStatus.InkVolumeToEmpty), oldStatus.InkVolumeToEmpty, newStatus.InkVolumeToEmpty, changes);
            CompareProperty(nameof(oldStatus.InkLowAlarmActive), oldStatus.InkLowAlarmActive, newStatus.InkLowAlarmActive, changes);
            CompareProperty(nameof(oldStatus.InkEmptyAlarmActive), oldStatus.InkEmptyAlarmActive, newStatus.InkEmptyAlarmActive, changes);
            CompareProperty(nameof(oldStatus.PrinterStatusL1), oldStatus.PrinterStatusL1, newStatus.PrinterStatusL1, changes);
            CompareProperty(nameof(oldStatus.PrinterStatusL2), oldStatus.PrinterStatusL2, newStatus.PrinterStatusL2, changes);
            CompareProperty(nameof(oldStatus.PrinterStatusL3), oldStatus.PrinterStatusL3, newStatus.PrinterStatusL3, changes);
            CompareProperty(nameof(oldStatus.PrinterStatusL4), oldStatus.PrinterStatusL4, newStatus.PrinterStatusL4, changes);
            CompareProperty(nameof(oldStatus.PrinterStatusL5), oldStatus.PrinterStatusL5, newStatus.PrinterStatusL5, changes);

            return changes;
        }

        // 🔹 Hàm hỗ trợ: So sánh giá trị của một thuộc tính và thêm vào danh sách nếu có thay đổi
        private void CompareProperty<T>(string name, T oldValue, T newValue, List<string> changes)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                changes.Add($"{name}:{newValue}");
            }
        }
        string job = "";
        private void CheckJob(string hexString)
        {
            Encoding encoding = Encoding.GetEncoding(28591);
            byte[] byteArray = encoding.GetBytes(hexString);
            hexString = BitConverter.ToString(byteArray).Replace("-", " ");
            bool isValid = CheckHexStringFormat(hexString);
            CheckSpitDone(hexString);
            return;
            if (isValid)
            {
                string result = HexToString(byteArray,120);
                if (job != result)
                {
                    job = result;
                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.REC_JOB_NAME, job);
                }
            }
          
        }
        private bool CheckHexStringFormat(string hexString)
        {
            // Chuyển đổi hex string thành mảng byte
            string[] data = HexStringToByteArray(hexString);

            // Kiểm tra độ dài tối thiểu
            if (data.Length < 8) return false;

            // Kiểm tra điều kiện: Bắt đầu bằng 02 08 00 00 00 00 xx 78
            return data[0] == "02" &&                
                   data[7] == "78"; // data[6] có thể là bất kỳ giá trị nào
        }
        private bool CheckHexStringFormatError(string hexString)
        {
            // Chuyển đổi hex string thành mảng byte
            string[] data = HexStringToByteArray(hexString);

            // Kiểm tra độ dài tối thiểu
            if (data.Length < 8) return false;

            // Kiểm tra điều kiện: Bắt đầu bằng 02 08 00 00 00 00 xx 78
            return data[0] == "02" &&
                   //data[1] == "81" &&
                   data[2] == "00" &&
                   data[3] == "00" &&
                   data[4] == "00" &&
                   data[5] == "00" &&
                   (data[7] == "94" || data[7] == "B6"); // data[6] có thể là bất kỳ giá trị nào
        }
        private string[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", ""); // Loại bỏ khoảng trắng
            int length = hex.Length;
            string[] bytes = new string[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToString(hex.Substring(i, 2));
            }
            return bytes;
        }
        private string HexToString(byte[] hex, int b)
        {        

            // Tìm vị trí của byte 0x78
            int startIndex = Array.IndexOf(hex, (byte)b);
            if (startIndex == -1 || startIndex + 1 >= hex.Length) return string.Empty;

            // Lấy tất cả byte sau 78
            byte[] filteredBytes = hex.Skip(startIndex + 1).ToArray();

            // Kiểm tra nếu byte cuối là 0x03, loại bỏ nó
            if (filteredBytes.Length > 0 && filteredBytes[filteredBytes.Length - 1] == 0x03)
            {
                Array.Resize(ref filteredBytes, filteredBytes.Length - 1);
            }

            // Chuyển byte array sang chuỗi ASCII
            return Encoding.ASCII.GetString(filteredBytes);
        }
        private void CheckInk(string input)
        {
            if (input.Contains("printerType"))
            {
                // Dùng biểu thức chính quy để tìm tất cả các giá trị GoodNozzles
                var matches = Regex.Matches(input, @"GoodNozzles=(\d+)");
                foreach (Match match in matches)
                {
                    if (int.TryParse(match.Groups[1].Value, out int goodNozzles))
                    {
                        if (goodNozzles > 0)
                        {
                            ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.BUTTON_STATE, "Đang làm sạch đầu in...");
                            Request.IS_INK = true;
                            return;
                        }
                    }
                }
                ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.BUTTON_STATE, "Không tìm thấy mực in.");
                Request.IS_INK_ERROR = true;
            }
        }
        private void CheckSpitDone(string hexString)
        {
            // Chuyển đổi hex string thành mảng byte
            string[] data = HexStringToByteArray(hexString);

            // Kiểm tra độ dài tối thiểu
            if (data.Length < 8) return;

            if(    data[0] == "02" && 
                   data[7] == "13")
            {
                ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.BUTTON_STATE, "Làm sạch đầu in thành công.");
            }
        }
    }
}
