using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocateExtentions
{  
    public enum enumClient
    {
        CONNECTED,
        DISCONNECTED,
        RECEIVED
    }
    public class aSyncClient
    {
        BackgroundWorker worker = new BackgroundWorker();

        bool STATUP = true;
        public void LOAD()
        {
            if (STATUP)
            {
                STATUP = false;
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerAsync();

            }
        }
        string last = "";
        public void SafeInvoke(enumClient eAE, string _strData)
        {
            if (ClientCallBack == null) return;
            string invokeData = eAE.ToString() +_strData;
            if(invokeData!=last)
            {
                ClientCallBack.Invoke(eAE, _strData);
                last = invokeData;
            }
            
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!re)
                {
                    Connect();
                }
                re = false;
                Thread.Sleep(5000);
            }
        }

        public delegate void EventForClient(enumClient eAE, string _strData);
        public event EventForClient ClientCallBack;
        public string IP { get; set; } = "192.168.250.33";
        public int Port { get; set; } = 2030;

        byte[] m_DataBuffer = new byte[512];
        IAsyncResult m_asynResult;
        private AsyncCallback pfnCallBack;
        private Socket client;
        public bool Connected { get; private set; }
        private bool statup = false;
        public void Connect()
        {
            LOAD();
            try
            {
                Connected = false;
                Ping pinger = new Ping();
                PingReply reply = pinger.Send(IP);
                if (reply.Status == IPStatus.Success)
                {

                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress Ip = IPAddress.Parse(IP);
                    int iPortNo = System.Convert.ToInt32(Port);
                    IPEndPoint ipEnd = new IPEndPoint(Ip, iPortNo);
                    client.Connect(ipEnd);

                    if (client.Connected)
                    {
                        Connected = client.Connected;
                        tr = 5;
                        WaitForData();
                        SafeInvoke(enumClient.CONNECTED, "Connected");
                    }
                }
                else
                {
                    SafeInvoke(enumClient.DISCONNECTED, "Disconnected"); statup = true;
                    Connected = false;
                }
            }
            catch //(Exception)
            {
                //throw;
            }
            if (!Connected && !worker.IsBusy)
            {
                worker.RunWorkerAsync();
            }
        }
        private void WaitForData()
        {
            try
            {
                if (pfnCallBack == null)
                {
                    pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket();
                theSocPkt.thisSocket = client;
                m_asynResult = client.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnCallBack, theSocPkt);
            }
            catch
            {
            }
        }
        private class CSocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[256];
        }
        public void Disconnect()
        {
            try
            {
                if (Connected)
                {
                    if (client != null)
                    {
                        //client.Disconnect(false);
                        Connected = false;
                        SafeInvoke(enumClient.DISCONNECTED, "Disconnected");
                        if (!Connected && !worker.IsBusy)
                        {
                            worker.RunWorkerAsync();
                        }
                    }
                }
            }
            catch
            {
                Connected = false;
            }
        }
        public void Send(string data)
        {
            if (client != null && data != null)
            {
                try
                {
                    Object objData = data;
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes('\x002'+objData.ToString() + '\x03');
                    client.Send(byData);
                }
                catch (SocketException)
                {
                    client = null;

                    SafeInvoke(enumClient.DISCONNECTED, "Disconnected");
                    Connected = false;
                }
            }
        }
        public void SendByte(byte[] bytes)
        {
            try
            {
                if (client != null)
                {
                    List<byte> bytesend = new List<byte>();
                    bytesend.Add(2); //0
                    bytesend.Add(4);
                    bytesend.Add(0);
                    bytesend.Add(0);
                    bytesend.Add(0);
                    bytesend.Add(0);
                    bytesend.Add(0);
                    bytesend.AddRange(bytes);
                    bytesend.Add(3);
                    client.Send(bytesend.ToArray());
                }
            }
            catch
            {
            }
        }
        public void SendByteOld(byte[] bytes)
        {
            try
            {
                if (client != null)
                {
                    List<byte> bytesend = new List<byte>();
                    bytesend.Add(2); //0               
                    bytesend.AddRange(bytes);
                    bytesend.Add(3);
                    client.Send(bytesend.ToArray());
                }
            }
            catch
            {
            }
        }
        int tr = 0;
        bool re = false;
        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                string szData = "";
                CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);

                if (iRx > 0) // Chỉ xử lý khi có dữ liệu nhận được
                {

                    string hexString = BitConverter.ToString(theSockId.dataBuffer).Replace("-", " ");

                    Encoding encoding = Encoding.GetEncoding(28591);
                    szData = encoding.GetString(theSockId.dataBuffer, 0, iRx);

                    re = true;
                    tr = 5;
                    Connected = true;
                    SafeInvoke(enumClient.RECEIVED, szData);
                    WaitForData();
                    
                }

                else
                {
                    return;
                    tr--;
                    Thread.Sleep(1000);
                    WaitForData();
                    if (tr <= 0)
                    {

                        if (!worker.IsBusy)
                        {
                            Disconnect();
                            worker.RunWorkerAsync();
                        }

                    }
                }
            }
            catch
            {
            }
        }
    }

}