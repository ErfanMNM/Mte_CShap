using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Norwix
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

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!Connected)
            {
                Thread.Sleep(1000);
                Connect();               
            }
        }

        public delegate void EventForClient(enumClient eAE, string _strData);
        public event EventForClient ClientCallBack;
        public string IP { get; set; }
        public int Port { get; set; }
        
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
                Ping pinger = new Ping();
                PingReply reply = pinger.Send(IP);
                if (reply.Status == IPStatus.Success)
                {
                    try
                    {
                        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress Ip = IPAddress.Parse(IP);
                        int iPortNo = System.Convert.ToInt32(Port);
                        IPEndPoint ipEnd = new IPEndPoint(Ip, iPortNo);
                        client.Connect(ipEnd);
                        Connected = true;
                        WaitForData();                    
                        if (ClientCallBack != null)
                        {
                            ClientCallBack.Invoke(enumClient.CONNECTED, "Connected");                           
                        }
                    }
                    catch
                    {
                        if(!statup) {ClientCallBack.Invoke(enumClient.DISCONNECTED, "Disconnected");statup = true;}
                        Connected = false;
                    }
                }
                else
                {
                    if (!statup) { ClientCallBack.Invoke(enumClient.DISCONNECTED, "Disconnected"); statup = true; }
                    Connected = false;
                }
            }
            catch //(Exception)
            {
                //throw;
            }
            if(!Connected && !worker.IsBusy)
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
                        client.Disconnect(false);
                        Connected=false;
                        if (ClientCallBack != null)
                        {
                            ClientCallBack.Invoke(enumClient.DISCONNECTED, "Disconnected");                           
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
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString() + '\x03');
                    client.Send(byData);
                }
                catch (SocketException)
                {
                    client = null;
                    if (ClientCallBack != null)
                    {
                        ClientCallBack.Invoke(enumClient.DISCONNECTED, "Disconnected");
                        Connected = false;
                    }
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
                    bytesend.Add(2);
                    bytesend.AddRange(bytes);
                    bytesend.Add(3);
                    client.Send(bytesend.ToArray());
                }
            }
            catch
            {
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
                int iRx = 0;
                iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx];
                System.Text.Decoder d = System.Text.Encoding.ASCII.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);
                if (!string.IsNullOrEmpty(szData))
                {
                    Connected = true;
                    if (ClientCallBack != null) ClientCallBack.Invoke(enumClient.RECEIVED, szData);
                 
                }
                else 
                {
                    Disconnect();
                    if (!worker.IsBusy)
                    {
                        worker.RunWorkerAsync();
                    }
                }
                WaitForData();
            }
            catch
            {
            }
        }
    }
}
