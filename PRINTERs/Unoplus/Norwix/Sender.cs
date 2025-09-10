using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using System.ComponentModel;
using System.Threading;

namespace Norwix
{
    public class Sender
    {
        public delegate void EventHandler(enumClient e, object obj);
        public event EventHandler EventCallBack;
        BackgroundWorker worker;
        aSyncClient client;
        public bool Connected = false;
        public List<byte[]> Data = new List<byte[]>();
        public Sender()
        {
            client = new aSyncClient();
            //client.IP = "127.0.0.1";
            client.IP = "192.168.0.32";
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
        }
        public void Stop()
        {
            if (worker.IsBusy) worker.CancelAsync();
            Thread.Sleep(50);
        }
        public void Next()
        {
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
                        GET_NUM_LEFT_TO_PRINT_REMOTE();
                        break;
                    case 1:
                        GET_INK_LEVELS_REMOTE();
                        break;
                    case 2:
                        //GET_PRINT_BUTTON_STATE();
                        break;
                    case 3:
                        GET_CURRENT_JOB();
                        break;
                    case 4:
                        //Get_System_Status();
                        break;
                    default:
                        break;
                }
                SendByte();
                step++;
                if (step >= 4)
                {
                    step = 0;
                }
                Thread.Sleep(500);
            }
        }

        private void Client_ClientCallBack(enumClient eAE, string _strData)
        {
            switch (eAE)
            {
                case enumClient.CONNECTED:
                    Connected = true;
                    break;
                case enumClient.DISCONNECTED:
                    Connected = false;                  
                    break;
                case enumClient.RECEIVED:
                    break;
                default:
                    break;
            }
            if (EventCallBack != null)
            {
                EventCallBack.Invoke(eAE, _strData);
            }
        }

        public void PressPrintButton()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(145); //0x91
            Data.Insert(0, lByte.ToArray());
        }
        public void GET_PRINT_BUTTON_STATE()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(146); //0x92
            Data.Add(lByte.ToArray());
        }
        public void ClearBuffer()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4); //0
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0); //1
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(152); //0x98
            client.SendByte(lByte.ToArray());
        }
        public void GET_NUM_LEFT_TO_PRINT_REMOTE()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(133); //85
            Data.Add(lByte.ToArray());
        }
        public void GET_INK_LEVELS_REMOTE()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
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
            client.SendByte(lByte.ToArray());
            Thread.Sleep(10);
            //Data.Insert(0, lByte.ToArray());
        }
        public void GET_CURRENT_JOB()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4);
            lByte.Add(0); //1
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0); //5
            lByte.Add(119); //77
            Data.Add(lByte.ToArray());
        }
        public void Get_System_Status()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(02);
            lByte.Add(246); //1
            lByte.Add(03);
            Data.Add(lByte.ToArray());
        }
        public void GetInkLevel()
        {
            List<byte> lByte = new List<byte>();
            lByte.Add(4);
            lByte.Add(0); //1
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0); //5
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
            lstbyte.Add(4);
            lstbyte.Add(1);
            lstbyte.Add(0);
            lstbyte.Add(0);
            lstbyte.Add(0);
            lstbyte.Add(0);
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
            lByte.Add(4);
            lByte.Add(0); //1
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0);
            lByte.Add(0); //5
            lByte.Add(183); //B7
            Data.Add(lByte.ToArray());
        }
    }
}
