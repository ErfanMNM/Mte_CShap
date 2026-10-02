using Gardasoft;
using Gardasoft.Controller.API.Interfaces;
using Gardasoft.Controller.API.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTManager.Communication.CC320
{
    public sealed class CPN_CC320
    {
        /// <summary>
        /// Sự kiện của component, khi có sự kiện xảy ra thì sẽ gọi đến các hàm được đăng ký
        /// </summary>
        /// <param name="Sự kiện"></param>
        /// <param name="String trả về"></param>
        public delegate void EventForComponent(enumComponent_Client e, string _strData);
        public event EventForComponent CPN_ClientCallBack;
        /// <summary>
        /// Địa chỉ IP của CC320
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Setial number của CC320, gán để xác định CC320 nào sẽ được kết nối, nếu không gán thì sẽ kết nối đến CC320 đầu tiên tìm thấy
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// Tag của sản phẩm.
        /// Yêu cầu gởi kết quả pass xuống để xác định sản phẩm pass trước khi sản phẩm mới vào.
        /// Chỉ cần gọi hàm SET_PASS() để gởi kết quả pass xuống CC320, CC320 sẽ tự động gởi tag của sản phẩm xuống.
        /// Gọi hàm SET_FAIL() để reject sản phẩm, hoặc không gọi cũng được
        /// </summary>
        public string TAG { get; set; } = string.Empty;
        /// <summary>
        /// Trạng thái kết nối đến CC320, true là đã kết nối, false là chưa kết nối
        /// </summary>
        /// 

        private BackgroundWorker bwk = new BackgroundWorker();

        public bool CONNECTED
        {
            get
            {
                if (CC320 != null && CC320.Connected)
                {
                    return true;
                }
                return false;
            }
        }                
        /// <summary>
        /// TCP Port của CC320, mặc định là 30313
        /// </summary>
        const int PORT = 30313;
        /// <summary>
        /// Khởi động component, sẽ tìm kiếm CC320 trong mạng LAN, nếu tìm thấy thì kết nối đến CC320, nếu không tìm thấy thì sẽ tiếp tục tìm kiếm sau 10 giây
        /// </summary>
        public void LOAD()
        {
            bwk.WorkerSupportsCancellation = true;
            bwk.DoWork += worker_DoWork;
            bwk.RunWorkerAsync();
        }
        /// <summary>
        /// Hàm được gọi khi component bị huỷ, sẽ huỷ kết nối đến CC320 và huỷ thread tìm kiếm CC320
        /// </summary>
        public void UNLOAD()
        {
            if (bwk.IsBusy)
            {
                bwk.CancelAsync();
            }
            if (CC320 != null && CC320.Connected)
            {
                CC320.Disconnect();
            }
        }
        /// <summary>
        /// 1.Tìm kiếm CC320 trong mạng LAN, nếu tìm thấy thì kết nối đến CC320, nếu không tìm thấy thì sẽ tiếp tục tìm kiếm sau 10 giây
        /// 2. Tiếp tục thực hiện kết nối TCP 30313
        /// 3. Nếu không kết nối dc thì khởi động lại cc320, thực hiện lại từ bước 2
        /// 4. Khi có CC320 thì khôi phục cấu hình bằng api của hãng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _controllerManager = ControllerManager.Instance();
            while (!worker.CancellationPending && _controllerManager.Controllers.Count == 0)
            {
                _controllerManager.DiscoverControllers();
                PopulateControllerList();
                if (_selectedController != null)
                {
                    if (CPN_ClientCallBack != null)
                    {
                        CPN_ClientCallBack(enumComponent_Client.FoundDevice,"Found CC320 device: "+ _selectedController.IPAddress.ToString()+";"+_selectedController.SerialNumber);
                    }
                    break;
                }
                Thread.Sleep(10000);
            }
            if (_selectedController != null)
            {                
                CC320 = new ASyncClient(_selectedController.IPAddress, PORT);
                CC320.ClientCallBack += CC320_ClientCallBack;
                CC320.Connect();
                if (!CC320.Connected)
                {
                    RESTART_CC320();
                    Thread.Sleep(5000);                        
                }
                if (CC320.Connected)
                {
                    CC320.Connect();
                }
                RESTORE_CONFIGURATION();                 
            }
        }
        /// <summary>
        /// Gọi hàm để gởi kết quả pass xuống CC320, CC320 sẽ tự động gởi tag của sản phẩm xuống.
        /// </summary>
        public void SET_PASS()
        {
            if(string.IsNullOrEmpty(TAG))
            {                
                return;
            }
            if (CC320 != null && CC320.Connected)
            {
                CC320.Send("SN1," + TAG+",1");
            }
            TAG = "";
        }
        /// <summary>
        /// Gởi kết quả pass xuống cc320 với tag được truyền vào.
        /// </summary>
        /// <param name="tag"></param>
        public void SET_PASS(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }
            if (CC320 != null && CC320.Connected)
            {
                CC320.Send("SN1," + tag + ",1");
            }
        }
        /// <summary>
        /// Gọi hàm để reject sản phẩm, hoặc không gọi cũng được
        /// </summary>
        public void SET_FAIL()
        {
            if (string.IsNullOrEmpty(TAG))
            {
                return;
            }
            if (CC320 != null && CC320.Connected)
            {
                CC320.Send("SN1," + TAG + ",0");
            }
            TAG = "";
        }
        /// <summary>
        /// Gởi kết quả fail xuống cc320 với tag được truyền vào.
        /// </summary>
        /// <param name="tag"></param>
        public void SET_FAIL(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }
            if (CC320 != null && CC320.Connected)
            {
                CC320.Send("SN1," + tag + ",0");
            }
        }
        /// <summary>
        /// Cài đặt delay cho camera OP2 tính từ sensor,
        /// Cài đặt độ trễ bộ loại OP1 tính từ camera,
        /// </summary>
        /// <param name="camera_delay"></param>
        /// <param name="rejector_delay"></param>
        /// <param name="rejector_strength"></param>
        public void SET_DELAY(int camera_delay, int rejector_delay, int rejector_strength)
        {
            try
            {
                SET_CAMERA_DELAY(camera_delay);
                SET_REJECTOR_DELAY(rejector_strength, rejector_delay);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Cài đặt độ trễ camera tính từ sensor, chú ý sẽ xoá buffer camera và sai delay reject do reject tính từ camera
        /// </summary>
        /// <param name="camera_delay"></param>
        public void SET_CAMERA_DELAY(int camera_delay)
        {
            try
            {
                SET_OP(2, 30, camera_delay);
                //_camera_delay = camera_delay;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Cài đặt độ trễ và độ rộng xung của bộ loại tính từ camera, chú ý sẽ xoá buffer reject
        /// </summary>
        /// <param name="rejector_delay"></param>
        /// <param name="rejector_strength"></param>
        public void SET_REJECTOR_DELAY(int rejector_strength, int rejector_delay)
        {
            try
            {
                SET_OP(1, rejector_strength, rejector_delay);
                //_rejector_delay = rejector_delay;
                //_rejector_strength = rejector_strength;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Cài đặt độ trễ và độ rộng xung cho output
        /// </summary>
        /// <param name="op_index bắt đầu từ 1"></param>
        /// <param name="pulse_width Độ rộng xung"></param>
        /// <param name="pulse_delay Độ trễ xung"></param>
        void SET_OP(int op_index, int pulse_width,int pulse_delay)
        {
            SEND_COMMAND("RT"+op_index.ToString() + "," + pulse_width.ToString()+ "," + pulse_delay.ToString());
        }
        /// <summary>
        /// Cho phép bật tag lên, mặc định CC320 tắt tag, phải gọi hàm này để bật tag trước khi gởi kết quả pass xuống CC320
        /// </summary>
        public void ENABLE_TAG()
        {
            SEND_COMMAND("GT1");
        }
        /// <summary>
        /// Gơi hàm để gởi lệnh trực tiếp xuống CC320, tham khảo trong tài liệu hướng dẫn của hãng.
        /// </summary>
        /// <param name="_strCommand"></param>
        /// <returns></returns>
        public string SEND_COMMAND(string _strCommand)
        {
            if (CC320 != null && CC320.Connected)
            {
                CC320.Send(_strCommand);
                return "Command sent successfully!";
            }
            else
            {
                if(_selectedController!=null)
                {
                   return _selectedController.SendCommand(_strCommand+"\r\n");
                }
            }
            return "Command not sent!";
        }
        /// <summary>
        /// Khôi phục cấu hình của CC320, đọc từ file CONFIG.txt trong thư mục chạy của chương trình, nếu không có file này thì sẽ không khôi phục được
        /// </summary>
        void RESTORE_CONFIGURATION()
        {
            if (File.Exists(Application.StartupPath + "\\CONFIG.txt"))
            {
                List<string> config = File.ReadAllLines(Application.StartupPath + "\\CONFIG.txt").ToList();
                for (int i = 0; i < config.Count; i++)
                {
                    SEND_COMMAND(config[i]);
                }
                if (CPN_ClientCallBack != null)
                {
                    CPN_ClientCallBack(enumComponent_Client.RestoreConfiguration, "Restore configuration completed");
                }
                return;
            }
            if (CPN_ClientCallBack != null)
            {
                CPN_ClientCallBack(enumComponent_Client.Error, "CONFIG.txt not found!");
            }
        }
        /// <summary>
        /// Khởi động lại CC320 do kết nối TCP 30313 lỗi, sau khi khởi động lại xong thì sẽ tự động kết nối lại
        /// </summary>
        void RESTART_CC320()
        {
            _selectedController.SendCommand("ED" + _selectedController.SerialNumber.ToString() + ",0\r\n");           
            _selectedController.SendCommand("EX" + _selectedController.SerialNumber.ToString() + "\r\n");
        }
        /// <summary>
        /// worker để tìm kiếm CC320 trong mạng LAN, nếu tìm thấy thì kết nối đến CC320, nếu không tìm thấy thì sẽ tiếp tục tìm kiếm sau 10 giây
        /// </summary>
        BackgroundWorker worker = new BackgroundWorker();
        ControllerManager _controllerManager;
        List<IController> _controllers;
        IController _selectedController;
        ASyncClient CC320;
        int heartbeat_timing = 0;
        void CC320_ClientCallBack(enumClient e, string _strData)
        {
            heartbeat_timing = 0;
            switch (e)
            {
                case enumClient.CONNECTED:
                    if (CPN_ClientCallBack != null)
                    {
                        CPN_ClientCallBack(enumComponent_Client.Connected, e.ToString());
                    }
                    break;
                case enumClient.DISCONNECTED:
                    if (CPN_ClientCallBack != null)
                    {
                        CPN_ClientCallBack(enumComponent_Client.Disconnected, e.ToString());
                    }
                    break;
                case enumClient.RECEIVED:
                    if (_strData.Contains("Err"))
                    {
                        CPN_ClientCallBack?.Invoke(enumComponent_Client.Error, _strData);
                        break;
                    }
                    if (_strData.StartsWith("Evt1,"))
                    {
                        string[] evtArr = Regex.Split(_strData, "Evt1,");
                        if (evtArr.Length > 2)
                        {
                            break;
                        }
                        string strTag = "";
                        int tagInt = 0;
                        if (int.TryParse(evtArr[1].Replace(";", ""), out tagInt))
                        {
                            strTag = tagInt.ToString();
                        }
                        if (strTag.Length > 0)
                        {
                            TAG = strTag;
                            CPN_ClientCallBack?.Invoke(enumComponent_Client.DataReceived, _strData + "| " + TAG);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        void PopulateControllerList()
        {
            if (_controllers == null)
            {
                _controllers = new List<IController>();
            }
            _controllers.Clear();
            if (_controllerManager.Controllers.Count > 0)
            {   // Populate controller ComboBox
                foreach (IController controller in _controllerManager.Controllers)
                {
                    _controllers.Add(controller);
                    if (!String.IsNullOrEmpty(SerialNumber) && controller.SerialNumber.ToString() == SerialNumber)
                    {
                        _selectedController = controller;
                    }
                }
                if (_selectedController == null)
                {
                    _selectedController = _controllers[0];
                }
            }
        }
        /// <summary>
        /// Timer kiểm tra kết nối với CC320, yêu cầu heartbeat 8 giây, không có trao đổi gì thì gởi VR để giữ kết nối
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1000_Tick(object sender, EventArgs e)
        {
            if(CC320 != null && CC320.Connected)
            {
                heartbeat_timing++;
                if(heartbeat_timing >= 8)
                {
                    heartbeat_timing = 0;
                    CC320.Send("VR\r\n");
                }
            }
        }
    }
    public enum enumComponent_Client
    {
        FoundDevice,
        RestoreConfiguration,
        Connected,
        Disconnected,
        DataReceived,
        Error
    } 
    public enum enumClient
    {
        CONNECTED,
        DISCONNECTED,
        RECEIVED
    }

    public class ASyncClient : IDisposable
    {
        public delegate void EventForClient(
            enumClient eAE,
            string _strData
        );

        public event EventForClient ClientCallBack;

        public string IP { get; set; }

        public int Port { get; set; }

        public bool Connected
        {
            get
            {
                return _client != null && _client.Connected;
            }
        }

        private TcpClient _client;
        private NetworkStream _stream;

        private CancellationTokenSource _receiveCts;

        private readonly object _lock = new object();

        private bool _disconnectEventSent = false;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ASyncClient()
        {
        }

        public ASyncClient(string ip, int port)
        {
            IP = ip;
            Port = port;
        }


        // =========================================================
        // CONNECT
        // =========================================================

        public bool Connect(int pingTimeout = 128)
        {
            lock (_lock)
            {
                if (Connected)
                    return true;

                _disconnectEventSent = false;
            }

            // -----------------------------------------------------
            // Ping
            // -----------------------------------------------------

            bool pingOK = Ping(IP, pingTimeout);

            if (!pingOK)
            {
                RaiseDisconnected("Ping failed");
                return false;
            }


            // -----------------------------------------------------
            // TCP Connect
            // -----------------------------------------------------

            try
            {
                TcpClient client = new TcpClient();

                client.Connect(IP, Port);

                lock (_lock)
                {
                    _client = client;
                    _stream = client.GetStream();

                    _receiveCts = new CancellationTokenSource();
                }


                RaiseEvent(
                    enumClient.CONNECTED,
                    "Connected"
                );


                // -------------------------------------------------
                // Start receive
                // -------------------------------------------------

                _ = ReceiveLoopAsync(
                    _receiveCts.Token
                );

                return true;
            }
            catch (Exception ex)
            {
                RaiseDisconnected(
                    "Connect failed: " + ex.Message
                );

                return false;
            }
        }


        // =========================================================
        // PING
        // =========================================================

        private bool Ping(
            string ip,
            int timeout)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(
                            ip,
                            timeout
                        );


                    return reply.Status ==
                           IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }


        // =========================================================
        // RECEIVE
        // =========================================================

        private async Task ReceiveLoopAsync(
            CancellationToken token)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    NetworkStream stream;

                    lock (_lock)
                    {
                        stream = _stream;
                    }

                    if (stream == null)
                        break;


                    int bytesRead =
                        await stream.ReadAsync(
                            buffer,
                            0,
                            buffer.Length,
                            token
                        );


                    // ------------------------------------------------
                    // bytesRead = 0
                    // Server đã đóng connection
                    // ------------------------------------------------

                    if (bytesRead == 0)
                    {
                        RaiseDisconnected(
                            "Remote disconnected"
                        );

                        break;
                    }


                    // ------------------------------------------------
                    // Copy data
                    // ------------------------------------------------

                    byte[] data =
                        new byte[bytesRead];

                    Buffer.BlockCopy(
                        buffer,
                        0,
                        data,
                        0,
                        bytesRead
                    );


                    // ------------------------------------------------
                    // Convert ASCII
                    // ------------------------------------------------

                    string strData =
                        Encoding.ASCII.GetString(
                            data
                        );


                    RaiseEvent(
                        enumClient.RECEIVED,
                        strData
                    );
                }
            }
            catch (OperationCanceledException)
            {
                // Disconnect chủ động
            }
            catch (Exception ex)
            {
                RaiseDisconnected(
                    "Receive error: " + ex.Message
                );
            }
        }


        // =========================================================
        // SEND STRING
        // =========================================================

        public async Task<bool> SendAsync(
            string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;

            try
            {
                NetworkStream stream;

                lock (_lock)
                {
                    stream = _stream;
                }

                if (stream == null)
                    return false;


                byte[] bytes =
                    Encoding.ASCII.GetBytes(data);


                await stream.WriteAsync(
                    bytes,
                    0,
                    bytes.Length
                );


                await stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                RaiseDisconnected(
                    "Send error: " + ex.Message
                );

                return false;
            }
        }
        public void Send(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            try
            {
                NetworkStream stream;

                lock (_lock)
                {
                    stream = _stream;
                }

                if (stream == null)
                    return;

                byte[] bytes = Encoding.ASCII.GetBytes(data);

                stream.Write(
                    bytes,
                    0,
                    bytes.Length
                );

                stream.Flush();
            }
            catch (Exception ex)
            {
                RaiseDisconnected(
                    "Send error: " + ex.Message
                );
            }
        }

        // =========================================================
        // SEND BYTE[]
        // =========================================================

        public async Task<bool> SendAsync(
            byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;

            try
            {
                NetworkStream stream;

                lock (_lock)
                {
                    stream = _stream;
                }

                if (stream == null)
                    return false;


                await stream.WriteAsync(
                    data,
                    0,
                    data.Length
                );


                await stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                RaiseDisconnected(
                    "Send error: " + ex.Message
                );

                return false;
            }
        }

        // =========================================================
        // DISCONNECT
        // =========================================================

        public void Disconnect()
        {
            CancellationTokenSource cts;
            TcpClient client;
            NetworkStream stream;

            lock (_lock)
            {
                cts = _receiveCts;
                client = _client;
                stream = _stream;

                _receiveCts = null;
                _client = null;
                _stream = null;
            }


            try
            {
                cts?.Cancel();
            }
            catch
            {
            }


            try
            {
                stream?.Close();
            }
            catch
            {
            }


            try
            {
                client?.Close();
            }
            catch
            {
            }


            if (client != null)
            {
                RaiseDisconnected(
                    "Disconnected"
                );
            }
        }


        // =========================================================
        // EVENT
        // =========================================================

        private void RaiseEvent(
            enumClient state,
            string data)
        {
            try
            {
                ClientCallBack?.Invoke(
                    state,
                    data
                );
            }
            catch
            {
                // Không để exception từ UI
                // làm chết ReceiveLoop
            }
        }


        // =========================================================
        // DISCONNECTED EVENT
        // =========================================================

        private void RaiseDisconnected(
            string message)
        {
            lock (_lock)
            {
                if (_disconnectEventSent)
                    return;

                _disconnectEventSent = true;
            }

            // Đóng socket
            try
            {
                _receiveCts?.Cancel();
            }
            catch
            {
            }

            try
            {
                _stream?.Close();
            }
            catch
            {
            }

            try
            {
                _client?.Close();
            }
            catch
            {
            }


            RaiseEvent(
                enumClient.DISCONNECTED,
                message
            );
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        public void Dispose()
        {
            Disconnect();
        }
    }
}
