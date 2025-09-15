using HslCommunication.Profinet.Siemens;

namespace MTs.Globals
{
    public class PTranferSiemenGlobals
    {
        public static ePLCState PLCState = ePLCState.Disconnected;
        public static ePLCState PLCState02 = ePLCState.Disconnected;
    }

    public enum ePTranferSiemenState
    {
        Started,
        Stopped,
        ClientConnected,
        ClientDisconnected,
        Received,
        Error
    }

    public enum ePTranferSiemenCommand
    {
        Read,
        Write
    }

    public enum ePLCState
    {
        Disconnected,
        Connected,
        Error
    }
}
