using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Norwix
{
    public partial class uc_Nowix : UserControl
    {
        public enum e_PRINTER
        {
            CONNECTED,
            DISCONNECTED,
            PRINTTING,
            JOB_CHANGE,
            STOPED,
            INK_LOW,
            DATA_PRINTING,
            DATA_EMPTY
        }
        enum e_BUTTON_STATE
        {
            
            CONFIGURING_PRINTER,
            PRINTING,
         
            STOPPING_PRINTED,
            PRINTING_STOPED,
            ABORT_PRINTING,
         
            STOPPING_OR_ERROR
            //PRINTING_STOPED,
            //CONFIGURING_PRINTER,
            //PRINTING,
            //ABORT_PRINTING,
            //STOPPING_PRINTED,
            //STOPPING_OR_ERROR
        }
        public uc_Nowix()
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
        private string MessageLocation = @"C:\jet.engine.gui\Jobs\QRCode.ijj";
        e_BUTTON_STATE MAYIN = e_BUTTON_STATE.PRINTING_STOPED;
        Sender sendPrinter;
        Reciver recivePrinter;
        bool SenderConnected = false;
        bool ReciverConnected = false;
        List<string> DATA = new List<string>();
        Int32 INDEX = 0;
        public Int32 BUFFER = 0;
        public Int32 Counter = 0;
        Int32 Remain = 0;
        public Int32 Total = 0;
        Int32 Old = 0;
        int SetBuffer = 30;
        int ink1, ink2, ink3, ink4 = 0;
        public bool ENABLE
        {
            get
            {
                return btnPrint.Enabled;
            }
            set
            { 
                //btnPrint.Enabled = value;           
                //btnPrint.BackColor = value ? Color.Green : Color.Gray;
            }
        }   

        public void LOAD()
        {
            sendPrinter = new Sender();
            sendPrinter.Start();
            recivePrinter = new Reciver();
            recivePrinter.ConnectionEventCallBack += Reciver_ConnectionEventCallBack;
            recivePrinter.Start();
        }
        private void Reciver_ConnectionEventCallBack(Enum_ConnectionEventClient e, object obj)
        {
            this.Invoke(new Action(() =>
            {
                switch (e)
                {
                    case Enum_ConnectionEventClient.NONE:
                        break;
                    case Enum_ConnectionEventClient.RECEIVEDATA:
                        break;
                    case Enum_ConnectionEventClient.CONNECTED:
                        label4.Text = "Đã kết nối";
                        label4.BackColor = Color.FromArgb(128, 128, 255);
                        Log("Data: " + "Connected");
                        INVOKE(e_PRINTER.CONNECTED, "");
                        break;
                    case Enum_ConnectionEventClient.DISCONNECTED:
                        label4.Text = "Chưa kết nối";
                        label4.BackColor = Color.LightCoral;
                        Log("Data: " + "Disconnected");
                        INVOKE(e_PRINTER.DISCONNECTED, "");
                        break;
                    case Enum_ConnectionEventClient.REC_INK_1_LEVEL:
                        if ((int)obj < 20) 
                        { 
                            lblInk.BackColor = Color.Yellow; INVOKE(e_PRINTER.INK_LOW, "Ink 1: " + (obj).ToString() + "%");
                            Log("Ink 1 Low: " + (obj).ToString() + "%");
                        }
                        else
                        {
                            lblInk.BackColor = Color.FromArgb(128, 128, 255);
                        }
                        ink1 = (int)obj;
                        SetText(lblInk, "Mực: | " + ink1 + "% |");
                        break;
                    case Enum_ConnectionEventClient.REC_INK_2_LEVEL:
                        if ((int)obj < 20) 
                        { 
                            lblInk.BackColor = Color.Yellow; INVOKE(e_PRINTER.INK_LOW, "Ink 2: " + (obj).ToString() + "%");
                            Log("Ink 2 Low: " + (obj).ToString() + "%");
                        }
                        else
                        {
                            lblInk.BackColor = Color.FromArgb(128, 128, 255);
                        }
                        ink2 = (int)obj;
                        SetText(lblInk, "Mực: | " + ink1 + "% | " + ink2 + "% | " + ink3 + "% | " + ink4 + "% |");
                        break;
                    case Enum_ConnectionEventClient.REC_INK_3_LEVEL:
                        if ((int)obj < 20) 
                        {
                            lblInk.BackColor = Color.Yellow; INVOKE(e_PRINTER.INK_LOW, "Ink 3: " + (obj).ToString() + "%");
                            Log("Ink 3 Low: " + (obj).ToString() + "%");
                        }
                        else
                        {
                            lblInk.BackColor = Color.FromArgb(128, 128, 255);
                        }
                        ink3 = (int)obj;
                        SetText(lblInk, "Mực: | " + ink1 + "% | " + ink2 + "% | " + ink3 + "% | " + ink4 + "% |");
                        break;
                    case Enum_ConnectionEventClient.REC_INK_4_LEVEL:
                        if ((int)obj < 20) 
                        { 
                            lblInk.BackColor = Color.Yellow; INVOKE(e_PRINTER.INK_LOW, "Ink 4: " + (obj).ToString() + "%");
                            Log("Ink 4 Low: " + (obj).ToString() + "%");
                        }
                        else
                        {
                            lblInk.BackColor = Color.FromArgb(128, 128, 255);
                        }
                        ink4 = (int)obj;
                        SetText(lblInk,"Mực: | " + ink1 + "% | " + ink2 + "% | " + ink3 + "% | " + ink4 + "% |");
                        break;
                    case Enum_ConnectionEventClient.REC_JOB_NAME:
                        try
                        {
                            Log("Job name: "+ (string)obj);
                            SetText(lblJob, (string)obj);
                            INVOKE(e_PRINTER.JOB_CHANGE, (string)obj);
                        }
                        catch { }
                        break;
                    case Enum_ConnectionEventClient.BUTTON_STATE:
                        MAYIN = (e_BUTTON_STATE)((int)obj);
                        Log(e.ToString() +": "+ MAYIN.ToString());
                        switch (MAYIN)
                        {
                            case e_BUTTON_STATE.PRINTING_STOPED:
                                recivePrinter.Enable = 0;
                                btnPrint.BackColor = Color.Green;
                                btnPrint.Text = "Bắt đầu in";
                                //sendPrinter.Stop();
                                
                                INVOKE(e_PRINTER.STOPED, "");
                                break;
                            case e_BUTTON_STATE.CONFIGURING_PRINTER:
                                //if (recivePrinter.Enable != 0)
                                //{
                                btnPrint.Text = "Đang bật máy in…";
                                btnPrint.BackColor = Color.Gray;
                                //}                             
                                break;
                            case e_BUTTON_STATE.PRINTING:
                                recivePrinter.Enable = 0;
                                if (print)
                                {
                                    print = false;
                                    ClearIndex();
                                }
                                btnPrint.BackColor = Color.Red;
                                btnPrint.Text = "Dừng in";                              
                                //sendPrinter.Stop();
                                INVOKE(e_PRINTER.PRINTTING, "");
                                break;
                            case e_BUTTON_STATE.ABORT_PRINTING:
                                //btnPrint.BackColor = Color.Red;
                                break;
                            case e_BUTTON_STATE.STOPPING_PRINTED:
                                //btnPrint.BackColor = Color.Gray;
                                break;
                            case e_BUTTON_STATE.STOPPING_OR_ERROR:
                                INVOKE(e_PRINTER.STOPED, "");
                                btnPrint.BackColor = Color.Green;
                                btnPrint.Text = "Bắt đầu in";
                                break;
                            default:
                                break;
                        }
                        break;
                    case Enum_ConnectionEventClient.DATA_COUNTER:
                        BUFFER = (int)obj;
                        if ((int)obj == 0)
                        {
                            INVOKE(e_PRINTER.DATA_EMPTY, "");
                        }

                        break;
                    case Enum_ConnectionEventClient.DATA_PRINTED:
                        if (DATA.Count() > (int)obj - 1 + Old)
                        {
                            BUFFER--;
                            Counter++;
                            lblData.Text = DATA[(int)obj - 1 + Old];
                            lblIndex.Text = ((int)obj - 1 + Old).ToString();
                            Send1Data();
                            UpdateCounter();
                            INVOKE(e_PRINTER.DATA_PRINTING, new List<string>() { "Index: " + ((int)obj - 1 + Old).ToString(), "Remain: " + Remain.ToString(), DATA[(int)obj - 1], });
                           
                        }
                        break;
                    case Enum_ConnectionEventClient.ERROR_OR_WARNING:
                        Log((string)obj);
                        break;
                    default:
                        break;
                }
            }));
        }
        private void Log(string message)
        {
            // Kiểm tra số lượng item trong ListBox
            if (listBox1.Items.Count >= 30)
            {
                // Xóa item cuối cùng
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            }

            // Thêm item mới vào đầu
            listBox1.Items.Insert(0, $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - {message}");
        }

        public List<int> Ink(string input)
        {
            List<int> list = new List<int>();
            string[] s = input.Split(';');
            if (s.Count() > 0)
            {
                for (int i = 0; i < s.Count(); i++)
                {
                    list.Add(Convert.ToInt32(s[i]));
                }
            }
            return list;
        }


        public void ChangeJob(string job)
        {
            if (string.IsNullOrEmpty(job)) return;
            StopPrinter();
            ClearData();
            sendPrinter.ChangeJob(job);
            Thread.Sleep(500);
            StartPrinter();

        }
        public void ClearData()
        {
            DATA.Clear();
            INDEX = Counter = Old = 0;
            sendPrinter.ClearBuffer();
            Counter = Remain = Total = 0;
            UpdateCounter();
        }
        private void ClearIndex()
        {
            Old = Counter;
            INDEX = 0;
            BUFFER = 0;
            sendPrinter.ClearBuffer();
            Thread.Sleep(500);
            SendData();
        }
        public void AddData(List<string> data)
        {
            DATA.AddRange(data);
            Total = DATA.Count - 1;
            Invoke(new Action(() => { UpdateCounter(); }));
            

        }
        public bool PrintByIndex(uint index)
        {
            if (DATA.Count > index)
            {
                Old = (int)index;
                INDEX = 0;
                BUFFER = 0;
                sendPrinter.ClearBuffer();
                Thread.Sleep(500);
                SendData();
                Thread.Sleep(500);
                StartPrinter();
                return true;
            }
            return false;
        }



        public void Send1Data()
        {
            if (INDEX < DATA.Count && BUFFER <= SetBuffer)
            {
                sendPrinter.SEND_DYNAMIC_DATA_TM1_REMOTE(DATA[INDEX], Convert.ToUInt64(INDEX + 1));
                INDEX++;
                BUFFER++;
            }           
            UpdateCounter();
        }

        public void SendData()
        {
            sendPrinter.Stop();
            Thread.Sleep(500);
            for (int i = 0; i < 30 && INDEX + Old < DATA.Count; i++)
            {
                if (INDEX < DATA.Count && BUFFER <= SetBuffer)
                {
                    sendPrinter.SEND_DYNAMIC_DATA_TM1_REMOTE(DATA[INDEX + Old], Convert.ToUInt64(INDEX + 1));
                    INDEX++;
                    BUFFER++;
                }
            }
            UpdateCounter();
            sendPrinter.Next();
        }
        public void UpdateCounter()
        {
            lblCounter.Text = Counter.ToString();
            lblTotal.Text = Total.ToString();
            Remain = Total - Counter;
            lblRemain.Text = Remain.ToString();
            lblBuffer.Text = BUFFER.ToString();
        }
        private void SetText(Label label, string s)
        {
            if (label.Text != s) label.Text = s;
        }
        bool print = false;

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                MessageBox.Show(listBox1.SelectedItem.ToString(),"Thông tin",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            recivePrinter.Enable = 20;
            btnPrint.Enabled = false;
            if (sendPrinter.Connected && recivePrinter.Connected)
            {
                if (MAYIN == e_BUTTON_STATE.PRINTING_STOPED || MAYIN == e_BUTTON_STATE.STOPPING_OR_ERROR)
                {
                    sendPrinter.PressPrintButton();
                    print = true;
                }
                else if (MAYIN == e_BUTTON_STATE.PRINTING)
                {
                    sendPrinter.PressPrintButton();
                }
            }
            sendPrinter.Next();
            btnPrint.Enabled = true;
        }
        public void StopPrinter()
        {
            btnPrint.Enabled = false;
            if (sendPrinter.Connected && recivePrinter.Connected)
            {
                if (MAYIN == e_BUTTON_STATE.PRINTING)
                {
                    sendPrinter.PressPrintButton();
                }
            }
            sendPrinter.Next();
            btnPrint.Enabled = true;
        }
        public void StartPrinter()
        {
            btnPrint.Enabled = false;
            if (sendPrinter.Connected && recivePrinter.Connected)
            {
                if (MAYIN == e_BUTTON_STATE.PRINTING_STOPED)
                {
                    sendPrinter.PressPrintButton();
                    print = true;
                }
            }
            sendPrinter.Next();
            btnPrint.Enabled = true;
        }
    }
}
