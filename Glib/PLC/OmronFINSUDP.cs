using HslCommunication;
using HslCommunication.Profinet.Omron;
using System.ComponentModel;


namespace Glib.PLCHelpers
{
    public partial class OmronPLC_Hsl
    {
        public string PLC_IP { get; set; } = "127.0.0.1";
        public int PLC_PORT { get; set; } = 9600;
        public string PLC_Ready_DM { get; set; } = "D16";
        public static PLCStatus plc_Status { get; set; } = PLCStatus.Disconnect;
        public int Ready { get; set; } = 0;
        public int Time_Update { get; set; } = 300;
        public OmronFinsUdp plc = new OmronFinsUdp();
        public event EventHandler<PLCStatusEventArgs>? PLCStatus_OnChange;

        BackgroundWorker WK_Update = new BackgroundWorker();

        public void InitPLC()
        {
            plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeUdpNet(PLC_IP, PLC_PORT)
            {
                ReceiveTimeOut = 1000,
                SleepTime = 0,
                SocketKeepAliveTime = -1,
                IsPersistentConnection = true,
            };
            plc.PlcType = OmronPlcType.CSCJ;
            plc.SA1 = 1;
            plc.GCT = 2;
            plc.DA1 = 0;
            plc.SID = 0;
            plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
            plc.ByteTransform.IsStringReverseByteWord = true;
            WK_Update.DoWork += WK_Update_DoWork;
            WK_Update.RunWorkerAsync();
        }



        private void WK_Update_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!WK_Update.CancellationPending)
            {
                Thread.Sleep(Time_Update);
                OperateResult write = plc.Write(PLC_Ready_DM, short.Parse(Ready.ToString()));

                if (write.IsSuccess)
                {
                    if (plc_Status == PLCStatus.Connected)
                    {

                    }
                    else
                    {
                        plc_Status = PLCStatus.Connected;
                        PLCStatus_OnChange?.Invoke(this, new PLCStatusEventArgs(PLCStatus.Connected, "PLC đã kết nối"));
                    }
                }
                else
                {
                    if (plc_Status == PLCStatus.Disconnect)
                    {

                    }
                    else
                    {
                        plc_Status = PLCStatus.Disconnect;
                        PLCStatus_OnChange?.Invoke(this, new PLCStatusEventArgs(PLCStatus.Disconnect, "PLC mất kết nối"));
                    }
                }
            }
        }

        public enum PLCStatus
        {
            Connected = 0,
            Disconnect = 1,
        }

        public class PLCStatusEventArgs : EventArgs
        {
            public PLCStatus Status { get; set; }
            public string Message { get; set; }

            public PLCStatusEventArgs(PLCStatus status, string message)
            {
                Status = status;
                Message = message;
            }
        }
    }
}
