using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
public enum Enum_ConnectionEventClient
{
    NONE,
    RECEIVEDATA,
    CONNECTED,
    DISCONNECTED,
    REC_INK_1_LEVEL,
    REC_INK_2_LEVEL,
    REC_INK_3_LEVEL,
    REC_INK_4_LEVEL,
    REC_JOB_NAME,
    BUTTON_STATE,
    DATA_COUNTER,
    GET_BUTTON_STATUS,
    DATA_PRINTED,
    ERROR_OR_WARNING,
    STATUS
}
namespace Norwix
{   
    public class Reciver
    {
        public delegate void EventHandler(Enum_ConnectionEventClient e, object obj);
        public event EventHandler ConnectionEventCallBack;
        aSyncClient client;
        public bool Connected = false;
        public Reciver()
        {
            client = new aSyncClient();
            client.IP = "192.168.0.32";
            //client.IP = "127.0.0.1";
            client.Port = 10002;
        }
        public void Start()
        {
            client.ClientCallBack += Client_ClientCallBack;
            client.LOAD();
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
        private void Client_ClientCallBack(enumClient eAE, string _strData)
        {
            if (ConnectionEventCallBack != null)
            {
                switch (eAE)
                {
                    case enumClient.CONNECTED:
                        ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.CONNECTED, _strData);
                        Connected = true;
                        break;
                    case enumClient.DISCONNECTED:
                        ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.DISCONNECTED, _strData);
                        Connected = false;
                        break;
                    case enumClient.RECEIVED:
                        try
                        {
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

                        li = new List<int>();
                        for (int i = 0; i < part.Length; i++)
                        {
                            li.Add((int)part[i]);
                        }

                        ErrorOrWarning(part);
                      
                        if (li.Count >= 8)
                        {
                            switch (li[6])
                            {
                                case 0:
                                    break;
                                default:
                                    break;
                            }
                            switch (li[7])
                            {

                                case 63:
                                    if (li[1] > 15)
                                    {

                                    }
                                    else
                                    {
                                        switch (li[1])
                                        {
                                            case 6:
                                                if (/*Enable > 0 && */Call(ref button, li[8])) { ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.BUTTON_STATE, li[8]); }
                                                break;
                                            case 8:
                                                if (Call(ref counter, li[8])) ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.DATA_COUNTER, li[8]);
                                                break;
                                            case 10:
                                                if (Call(ref ink1, li[10])) ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.REC_INK_1_LEVEL, li[10]);
                                                if (Call(ref ink2, li[11])) ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.REC_INK_2_LEVEL, li[11]);
                                                if (Call(ref ink3, li[12])) ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.REC_INK_3_LEVEL, li[12]);
                                                if (Call(ref ink4, li[13])) ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.REC_INK_4_LEVEL, li[13]);
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    break;
                                case 120:
                                    string name = part.Substring(8, part.Length - 9);
                                    if (Call(ref jobname, name))
                                        ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.REC_JOB_NAME, name);
                                    break;
                                case 126:
                                    ConnectionEventCallBack.Invoke(Enum_ConnectionEventClient.DATA_PRINTED, (int)part[8]);
                                    break;
                                case 134:
                                    string sss = "";
                                    break;
                                case 41:
                                    string ssss = "";
                                    break;
                                default:
                                    break;
                            }
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

            if (part.Contains("error") || part.Contains("failed") || part.Contains("no data") || part.Contains("fault") || part.Contains("job") || part.Contains("NO"))
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
    }
}
