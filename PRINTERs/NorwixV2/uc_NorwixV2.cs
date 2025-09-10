using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Net.NetworkInformation;
using System.IO;
public enum e_PRINTER
{
    CONNECTED,
    DISCONNECTED,
    PRINTTING,
    JOB_CHANGE,
    STOPED,
    INK_LOW,
    DATA_PRINTING,
    DATA_EMPTY,
    ERROR
}
namespace NorwixV2
{
    public partial class uc_NorwixV2: UserControl
    {
        public uc_NorwixV2()
        {
            InitializeComponent();
        }
        public delegate void EventHandler(e_PRINTER e, object s);
        public event EventHandler EventCallBack;
        private void INVOKE(e_PRINTER e, object obj)
        {
            if (EventCallBack != null)
            {
                EventCallBack.Invoke(e, obj);
            }
        }
        SenderClient sender;
        ReciverClient reciver;
        public string IP = "192.168.0.32";
        //public string IP = "127.0.0.1";
        List<string> DATA = new List<string>();
        Int32 INDEX = 0;
        public Int32 BUFFER = 0;
        public Int32 Counter = 0;
        Int32 Remain = 0;
        public Int32 Total = 0;
        Int32 Old = 0;
        int SetBuffer = 30;
        bool IsPrint = false;
        string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Log.txt");
        BackgroundWorker workerSendata = new BackgroundWorker();
        public void LOAD()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);                   
                    listBox1.Items.AddRange(lines);
                    Log("------- Mở ứng dụng -------");
                }
            }
            catch { }

            PrinterStatus.G_IsClient = new XXX();
            listBox1.Items.Add(IP);
            sender = new SenderClient();
            sender.IP = IP;
            sender.EventCallBack += Sender_EventCallBack;
            sender.Start();
            reciver = new ReciverClient();
            reciver.IP= IP;
            reciver.ConnectionEventCallBack += Reciver_ConnectionEventCallBack;
            reciver.ButtonEventCallBack += Reciver_ButtonEventCallBack;         
            reciver.Start();

            workerSendata.DoWork += WorkerSendata_DoWork;
            workerSendata.WorkerSupportsCancellation = true;
        }

        
        int count = 0;
        private void Reciver_ButtonEventCallBack(PrintButtonState e, object obj)
        {
            listBox1.Invoke(new Action(() =>
            {
                Log(Convert.ToString("Button: " + obj));
            }));
            button1.Invoke(new Action(() =>
            {
                btnPrint.Enabled = true;
                switch (e)
                {
                    case PrintButtonState.PrintingStopped:
                        btnPrint.BackColor = Color.Green;
                        btnPrint.Text = "Bắt đầu in";
                        Old += PrinterStatus.G_SimpleCounter;
                        IsPrint = false;
                        INVOKE(e_PRINTER.STOPED, "");
                        break;
                    case PrintButtonState.ConfiguringPrinters:
                        btnPrint.Text = "Đang bật máy in…";
                        btnPrint.BackColor = Color.Gray;
                        btnPrint.Enabled = false;
                        IsPrint = false;
                        break;
                    case PrintButtonState.Printing:
                        if (newdata)
                        {
                            newdata = false;
                            Old = 0;
                        }
                        ClearError();
                        Thread.Sleep(100);
                        btnPrint.BackColor = Color.Red;
                        btnPrint.Text = "Dừng in";
                        ClearIndex();
                        Thread.Sleep(50);
                        SendData();
                        INVOKE(e_PRINTER.PRINTTING, "");
                        IsPrint = true;
                        break;
                    case PrintButtonState.UnknownState3:
                        break;
                    case PrintButtonState.UnknownState4:
                        break;
                    case PrintButtonState.PrinterOfflineOrNoJob:
                        btnPrint.BackColor = Color.Green;
                        btnPrint.Text = "Bắt đầu in";
                        btnPrint.Enabled = false;
                        INVOKE(e_PRINTER.STOPED, "");
                        break;
                    case PrintButtonState.PausedReadyOrStop:
                        btnPrint.BackColor = Color.Green;
                        btnPrint.Text = "Bắt đầu in";
                        INVOKE(e_PRINTER.STOPED, "");
                        break;
                    default:
                        break;
                }
            }));
        }
        private void Sender_EventCallBack(enumClient e, object obj)
        {
           
            switch (e)
            {
                case enumClient.CONNECTED:
                    listBox1.Invoke(new Action(() =>
                    {
                        Log(Convert.ToString("Sender: " + obj));
                    }));
                    if (reciver.Connected)
                    {
                        label3.Invoke(new Action(() => label3.Text = "Trạng thái dữ liệu: Đã kết nối"));
                        INVOKE(e_PRINTER.CONNECTED, "");
                    }
                    break;
                case enumClient.DISCONNECTED:
                    listBox1.Invoke(new Action(() =>
                    {
                        Log(Convert.ToString("Sender: " + obj));
                    }));
                    label3.Invoke(new Action(() => label3.Text = "Trạng thái dữ liệu: Chưa kết nối"));
                    INVOKE(e_PRINTER.DISCONNECTED, "");
                    break;
                case enumClient.RECEIVED:
                    listBox1.Invoke(new Action(() =>
                    {
                        Log(Convert.ToString(obj));
                    }));
                    break;
                default:
                    break;
            }
        }
        private Recive previous;
        private void Reciver_ConnectionEventCallBack(Enum_ConnectionEventClient e, object obj)
        {
            if (previous == null) previous = new Recive(e, obj);
            else
            {
                Recive current = new Recive(e, obj);

                if(current.Equals(previous)) return;
                else
                {
                    previous=current;
                }           
            }           
           
            switch (e)
            {
                case Enum_ConnectionEventClient.NONE:
                    break;
                case Enum_ConnectionEventClient.RECEIVEDATA:
                    break;
                case Enum_ConnectionEventClient.CONNECTED:
                    if (sender.Connected)
                    {
                        label3.Invoke(new Action(() => label3.Text = "Trạng thái dữ liệu: Đã kết nối"));
                        INVOKE(e_PRINTER.CONNECTED, "");
                        listBox1.Invoke(new Action(() =>
                        {
                            Log(Convert.ToString("Reciver: " + obj));
                        }));
                    }
                    break;
                case Enum_ConnectionEventClient.DISCONNECTED:
                    label3.Invoke(new Action(() => label3.Text = "Trạng thái dữ liệu:Chưa kết nối"));
                    INVOKE(e_PRINTER.DISCONNECTED, "");
                    listBox1.Invoke(new Action(() =>
                    {
                        Log(Convert.ToString("Reciver: " + obj));
                    }));
                    break;               
                case Enum_ConnectionEventClient.REC_JOB_NAME:
                    lblJob.Invoke(new Action(() => lblJob.Text = (string)obj));
                    INVOKE(e_PRINTER.JOB_CHANGE, (string)obj);
                    break;
                case Enum_ConnectionEventClient.BUTTON_STATE:
                    listBox1.Invoke(new Action(() =>
                    {
                        Log(Convert.ToString(obj));
                    }));                    
                    break;
                case Enum_ConnectionEventClient.DATA_COUNTER:
                    break;
                case Enum_ConnectionEventClient.GET_BUTTON_STATUS:
                    break;
                case Enum_ConnectionEventClient.DATA_PRINTED:
                    break;
                case Enum_ConnectionEventClient.ERROR_OR_WARNING:
                    listBox1.Invoke(new Action(() =>
                    {
                        Log(Convert.ToString("Error: "+obj));
                    }));
                    INVOKE(e_PRINTER.ERROR, (string)obj);
                    break;
                case Enum_ConnectionEventClient.STATUS:
                    this.Invoke(new Action(() =>
                    {                       
                        int bu = (PrinterStatus.G_PCBuffers + PrinterStatus.G_PrinterBuffers);
                        if (PrinterStatus.G_Pringting && PrinterStatus.G_SimpleCounter >0)
                        {
                            //lblCounter.Text = PrinterStatus.G_LineCounter.ToString();
                            lblCounter.Text = (PrinterStatus.G_SimpleCounter + Old).ToString();
                            bu = (PrinterStatus.G_PCBuffers + PrinterStatus.G_PrinterBuffers);
                            long re = Convert.ToInt64(lblTotal.Text) - PrinterStatus.G_SimpleCounter - Old;
                            lblRemain.Text = (re).ToString();
                            if (re <= 0) INVOKE(e_PRINTER.DATA_EMPTY, "");
                            BUFFER = bu;
                            if (bu <= SetBuffer) while (Send1Data()) ;
                        }
                        lblBuffer.Text = "Máy in: " + (PrinterStatus.G_Pringting ? "Đang in" : "Đang dừng") + "        Tốc độ: " + PrinterStatus.G_Speed + "        Bộ nhớ: " + bu.ToString();
                        lblInk.Text = "Mực in: " + PrinterStatus.G_InkVolumeToEmpty.ToString() + " ml";
                    }));
                    break;
                default:
                    break;
            }
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

        private void button2_Click(object senr, EventArgs e)
        {
            ClearError();
            if (workerSendata.IsBusy) workerSendata.CancelAsync();
        }
        public void ClearError()
        {
            sender.ClearError();
            previous = null;
        }
        public void ClearData()
        {            
            ClearError();
            Thread.Sleep(500);
            DATA.Clear();
            INDEX = Counter = Old = 0;          
            Counter = Remain = Total = 0;
            ClearIndex();
            Thread.Sleep(500);
            sender.ClearCounter();
            Thread.Sleep(500);
            UpdateCounter();
        }
        public void ClearIndex()
        {           
            INDEX = 0;
            BUFFER = 0;
            sender.ClearBuffer();
            Thread.Sleep(500);         
        }
        public void TriggerFromCMR()
        {
            this.Invoke(new Action(() =>
            {
                Send1Data();
                UpdateCounter();
            }));

        }
        public bool Send1Data()
        {
            if (INDEX + Old < DATA.Count && BUFFER <= SetBuffer)
            {
                sender.SEND_DYNAMIC_DATA_TM1_REMOTE(DATA[INDEX + Old], Convert.ToUInt64(INDEX + 1));
                INDEX++;
                BUFFER++;
                UpdateCounter();
                return true;
            }
            return false;
        }

        public void SendData()
        {
            if (sender.Connected)
            {
                //sender.Stop();
                Thread.Sleep(500);
                for (int i = 0; i < 30 && INDEX + Old < DATA.Count; i++)
                {
                    Send1Data();
                    //if (INDEX + Old < DATA.Count && BUFFER <= SetBuffer)
                    //{
                    //    sender.SEND_DYNAMIC_DATA_TM1_REMOTE(DATA[INDEX + Old], Convert.ToUInt64(INDEX + 1));
                    //    INDEX++;
                    //    BUFFER++;
                    //}
                }
                UpdateCounter();
                sender.Next();
            }
        }
     

        public void UpdateCounter()
        {
            this.Invoke(new Action(() =>
            {
                lblCounter.Text = (PrinterStatus.G_SimpleCounter + Old).ToString();
                lblTotal.Text = Total.ToString();
                Remain = Total - (PrinterStatus.G_SimpleCounter + Old);
                lblRemain.Text = Remain.ToString();
                //lblBuffer.Text = BUFFER.ToString();
            }));
        }
        bool newdata = false;
        public void AddData(List<string> data)
        {
            newdata = true;
            DATA.AddRange(data);
            Total = DATA.Count;
            Invoke(new Action(() => { UpdateCounter(); Log("Đã thêm dữ liệu. Đang khởi động lại máy in..."); }));
            if (!workerSendata.IsBusy) workerSendata.RunWorkerAsync();
        }
        private void WorkerSendata_DoWork(object se, DoWorkEventArgs e)
        {
            int step = 0;
            if(sender.Connected && reciver.Connected)
            {
                while (!workerSendata.CancellationPending && IsPrint)
                {
                    StopPrinter();
                    Thread.Sleep(5000);
                }
                if(!workerSendata.CancellationPending && !IsPrint)
                {
                    sender.PressPrintButton();
                }
                if(workerSendata.CancellationPending)
                    Invoke(new Action(() => { Log("Đã hủy."); }));
            }
        }
        private void btnPrint_Click(object sr, EventArgs e)
        {
            btnPrint.Enabled = false;
            if (sender.Connected && reciver.Connected)
            {
                if (btnPrint.BackColor != Color.Red)
                {
                    if (DATA.Count == 0)
                    {
                        MessageBox.Show("Chưa có dữ liệu in");
                    }
                }                
                sender.ClearError();
                Thread.Sleep(50);
                sender.PressPrintButton();
            }
            sender.Next();
            timer1.Start();
            times = 5;
        }
        int times = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            times--;
            if(times <= 0)
            {
                timer1.Stop();
                btnPrint.Enabled = true;
            }
        }
        public void StartPrinter()
        {
            sender.StartPrinter(true);
        }
        public void StopPrinter()
        {
            sender.StartPrinter(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Log("----------------------------------");
        }
        private void Log(string message)
        {
            // Kiểm tra số lượng item trong ListBox
            if (listBox1.Items.Count >= 100)
            {
                // Xóa item cuối cùng
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            }

            // Thêm item mới vào đầu
            listBox1.Items.Insert(0, $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - {message}");
            try { File.WriteAllLines(filePath, listBox1.Items.Cast<string>()); } catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Log($"Đã in: {lblCounter.Text}. {lblInk.Text}");
        }

        private void button4_Click(object sender1, EventArgs e)
        {
            button4.Enabled = false;
            previous = null;
            Log($"Đang gửi yêu cầu...");
            sender.PenSpit();
            Task.Delay(5000).ContinueWith(t =>
            {
               button4.Invoke(new Action(() =>
                {
                    button4.Enabled = true;               
                }));
            });

        }

        private void button5_Click(object sender2, EventArgs e)
        {
            if(PrinterStatus.G_Pringting)
            {
                previous = null;
                button5.Enabled = false;
                Log($"Đang làm sạch đầu in nhanh...");
                sender.SupperPenSpit();
                Task.Delay(1000).ContinueWith(t =>
                {
                    button5.Invoke(new Action(() =>
                    {
                        button5.Enabled = true;
                    }));
                });
            }
            else
            {
                Log($"Chỉ dùng khi đang chạy. Vui lòng chọn làm sạch đầu in.");
            }
        }
    }
    public class Recive
    {
        public Enum_ConnectionEventClient e;
        public object obj;
        public Recive(Enum_ConnectionEventClient e, object obj)
        {
            this.e = e;
            this.obj = obj;
        }
        // Ghi đè Equals để so sánh theo giá trị
        public override bool Equals(object other)
        {
            if (other is Recive r)
            {
                return this.e == r.e && Equals(this.obj, r.obj);
            }
            return false;
        }
        // Ghi đè GetHashCode để đảm bảo tính nhất quán
        public override int GetHashCode()
        {
            return e.GetHashCode() ^ (obj?.GetHashCode() ?? 0);
        }
    }
}
