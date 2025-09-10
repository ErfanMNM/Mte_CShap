using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NorwixV2
{
    public class SenderClient
    {
        public delegate void EventHandler(enumClient e, object obj);
        public event EventHandler EventCallBack;
        BackgroundWorker worker;
        BackgroundWorker workerCheck = new BackgroundWorker();
        aSyncClient client;
        string ip = "127.0.0.1";
        public string IP { get { return ip; } set { client.IP = ip = value; }}
        public bool Connected = false;
        public List<byte[]> Data = new List<byte[]>();
        public SenderClient()
        {
            client = new aSyncClient();
            //client.IP = "127.0.0.1";
            client.IP = IP;
            client.Port = 10001;

        }
        public void Start()
        {
            client.ClientCallBack += Client_ClientCallBack;
            client.LOAD();
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync();
            workerCheck.DoWork += WorkerCheck_DoWork;
            workerCheck.RunWorkerAsync();
        }
        private void WorkerCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (Connected)
                {                 
                    if (!PrinterStatus.G_IsClient.Connect)
                    {
                        client.Disconnect();
                    }
                }
                Thread.Sleep(1000);
            }
        }
        public void Stop()
        {
            if (worker.IsBusy) worker.CancelAsync();
            Thread.Sleep(50);
        }
        public void Next()
        {
            Thread.Sleep(50);
            if (!worker.IsBusy) worker.RunWorkerAsync();
        }
        int step = 0;
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!worker.CancellationPending)
            {              
                switch (step)
                {
                    case 0:
                        GET_STATUS();
                        break;
                    case 1:
                        ////GET_INK_LEVELS_REMOTE();
                        GET_CURRENT_JOB();
                        break;
                    case 2:
                        GET_PRINT_BUTTON_STATE();
                        break;
                    case 3:
                        //GET_CURRENT_JOB();
                        break;
                    case 4:
                        //Get_System_Status();
                        break;
                    default:
                        break;
                }
                SendByte();
                step++;
                if (step >= 3)
                {
                    step = 0;
                
                }              
                Thread.Sleep(500);
            }
        }
        bool checking = false;
       
        private void Client_ClientCallBack(enumClient eAE, string _strData)
        {
            switch (eAE)
            {
                case enumClient.CONNECTED:
                    Connected = true;
                    PrinterStatus.G_IsClient.Connect = true;
                    break;
                case enumClient.DISCONNECTED:
                    Connected = false;
                    break;
                case enumClient.RECEIVED:
                    PrinterStatus.G_IsClient.Connect = true;
                    break;
                default:
                    break;
            }
            if (EventCallBack != null)
            {
                EventCallBack.Invoke(eAE, _strData);
            }
        }
     
        public void GET_STATUS()
        {
            List<byte> lByte = new List<byte>();           
            lByte.Add(16); //0x91          
            Data.Insert(0, lByte.ToArray());
        }

        public void PressPrintButton()
        {
            List<byte> lByte = new List<byte>();    
            lByte.Add(145); //0x91
            Data.Insert(0, lByte.ToArray());
        }
        public void GET_PRINT_BUTTON_STATE()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(146); //85     
            Data.Add(lByte.ToArray());
        }
        public void ClearBuffer()
        {
            List<byte> lByte = new List<byte>();     
            lByte.Add(152); //0x98  
            Data.Insert(0, lByte.ToArray());
        }
        public void GET_NUM_LEFT_TO_PRINT_REMOTE()
        {
            List<byte> lByte = new List<byte>();      
            lByte.Add(133); //85     
            Data.Add(lByte.ToArray());
        }
        public void GET_INK_LEVELS_REMOTE()
        {
            List<byte> lByte = new List<byte>();        
            lByte.Add(137); //89 
            Data.Add(lByte.ToArray());
        }
        private byte[] ConvertToByteArray(ulong l)
        {
            List<byte> lstb = new List<byte>();
            lstb.AddRange(BitConverter.GetBytes(l));
            while (lstb.Count > 4)
            {
                lstb.RemoveRange(4, lstb.Count - 4);
            }
            return lstb.ToArray();
        }
        public void SEND_DYNAMIC_DATA_TM1_REMOTE(string pod, ulong RecordNumber) //2 + so byte +00 +7D+Giatri record + do dai chuoi+3
        {
            //byte to follow: 4 byte
            List<byte> lByte = new List<byte>();
            lByte.Add(4); //0
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0); //1
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(125); //Ma lenh 7D
            lByte.AddRange(ConvertToByteArray(RecordNumber)); //record number 4 byte
            lByte.AddRange(ConvertToByteArray(Convert.ToUInt64(pod.Length))); //String length
            char[] c = pod.ToArray(); //Add pod data
            //List<byte> b = new List<byte>();
            for (int i = 0; i < c.Length; i++)
            {
                lByte.Add(Convert.ToByte(c[i]));
            }
            //ulong l = Convert.ToUInt64(lByte.Count - 4);
            byte[] bt = ConvertToByteArray(Convert.ToUInt64(lByte.Count - 3));
            for (int j = 0; j < 4; j++)
            {
                lByte[j] = bt[j];
            }
            client.SendByteOld(lByte.ToArray());
            Thread.Sleep(10);
            //Data.Insert(0, lByte.ToArray());
        }
        public void GET_CURRENT_JOB()
        {
            List<byte> lByte = new List<byte>();       
            lByte.Add(119); //77      
            Data.Add(lByte.ToArray());
        }
        public void Get_System_Status()
        {
            List<byte> lByte = new List<byte>();     
            lByte.Add(246); //1          
            Data.Add(lByte.ToArray());
        }
        public void GetInkLevel()
        {
            List<byte> lByte = new List<byte>();       
            lByte.Add(137); //89         
            Data.Add(lByte.ToArray());
        }

        public void ChangeJob(string locationandjob)
        {
            //string s = @"C:\jet.engine.gui\Jobs\3HANG111.ijj";

            char[] c = locationandjob.ToArray();
            List<byte> b = new List<byte>();

            for (int i = 0; i < c.Length; i++)
            {
                b.Add(Convert.ToByte(c[i]));
            }
            while (b.Count < 256)
            {
                b.Add(0);
            }
            List<byte> lstbyte = new List<byte>();       
            lstbyte.Add(118);       
            lstbyte.AddRange(b.ToArray());
            Data.Insert(0, lstbyte.ToArray());
        }
        private void SendByte()
        {
            {
                if (Data.Count > 0)
                {
                    if (client.Connected)
                    {
                        client.SendByte(Data[0]);
                    }
                    Data.Remove(Data[0]);
                }
            }
        }
        public void ClearError()
        {
            List<byte> lByte = new List<byte>();         
            lByte.Add(183); //B7        
            Data.Insert(0,lByte.ToArray());
        }
        public void ClearCounter()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(194); //C2        
            Data.Insert(0, lByte.ToArray());
        }
        public void StartPrinter(bool b)
        {
            List<byte> lByte = new List<byte>();
            if (b) lByte.Add(33); //77
            else lByte.Add(34);
            Data.Insert(0,lByte.ToArray());
        }

        public void PenSpit()
        {
            Request.IS_INK = false;
            Request.IS_INK_ERROR = false;
            GET_INK_DATA();
            CheckInkAsync();
        }
        public async void CheckInkAsync()
        {
            for (int i = 0; i < 7; i++)
            {
                
                if (Request.IS_INK)
                {
                    List<byte> lByte = new List<byte>();
                    lByte.Add(0x12); // 18
                    Data.Insert(0, lByte.ToArray());
                    return;
                }
                if(Request.IS_INK_ERROR)
                {
                    if (EventCallBack != null)
                    {
                        EventCallBack.Invoke(enumClient.RECEIVED, "Làm sạch đầu in không thành công.");
                    }
                    return;
                }

                await Task.Delay(1000); // đợi 1 giây trước lần kiểm tra tiếp theo
            }
            //het thoi gian
            if (EventCallBack != null)
            {
                EventCallBack.Invoke(enumClient.RECEIVED, "Làm sạch đầu in không thành công.");
            }
        }

        public void SupperPenSpit()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(18); //0x12
            Data.Insert(0, lByte.ToArray());
        }

        public void GET_INK_DATA()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(178); //0xb2
            Data.Insert(0, lByte.ToArray());
        }
    }
}
