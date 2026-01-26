using TTManager.Communication;
using Sunny.UI;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TApp.Configs;


namespace TApp.Dialogs
{
    public partial class Scaner : Form
    {
        public SerialClientHelper _datalogicScanner;
        public string TextValue { get; private set; }
        public string _Title { get; set; } = "SCANER";

        public event EventHandler OkClicked;
        public Scaner()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            DisconnectScanner();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // Regex: ^\d{13}$ nghĩa là chuỗi chứa đúng 13 ký tự số
            bool isValid = Regex.IsMatch(uiRichTextBox1.Text, @"^\d{1,15}$");

            if (isValid)
            {
                TextValue = uiRichTextBox1.Text;
                // Kích hoạt sự kiện OkClicked
                OkClicked?.Invoke(this, EventArgs.Empty);

                // Đóng form với kết quả OK
                DialogResult = DialogResult.OK;

                this.Close();
                DisconnectScanner();
            }
            else
            {
                Invoke(new Action(() => { this.ShowErrorTip("Nội dung không hợp lệ"); }));

            }
            
        }
        private void DisconnectScanner()
        {
            if (_datalogicScanner != null)
            {
                try
                {
                    if (_datalogicScanner.Connected)
                    {
                        _datalogicScanner.Disconnect();
                    }
                    _datalogicScanner.SerialClientCallback -= _ScanConection_EVENT;
                    _datalogicScanner = null;
                    //AddConsoleLog("[THÔNG BÁO] Đã ngắt kết nối máy quét", Color.Orange);
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không throw để đảm bảo ngắt kết nối hoàn tất
                    System.Diagnostics.Debug.WriteLine($"Error disconnecting scanner: {ex.Message}");
                }
            }
        }
        private void Scaner_Load(object sender, EventArgs e)
        {
            _datalogicScanner = new SerialClientHelper(AppConfigs.Current.Handheld_COM_Port, 9600);
            _datalogicScanner.SerialClientCallback += _ScanConection_EVENT;
            _datalogicScanner.Connect();
            uiTitlePanel1.Text= _Title;
        }

        private void _ScanConection_EVENT(SerialClientState state, string data)
        {
            switch (state)
            {
                case SerialClientState.Connected:
                    pnConnect.FillColor = Color.Green;
                    pnConnect.Text = "Scaner kết nối";
                    break;
                case SerialClientState.Disconnected:
                    pnConnect.FillColor = Color.Red;
                    pnConnect.Text = "Scaner mất kết nối";
                    break;
                case SerialClientState.Received:
                    string content = data;
                    Invoke(new Action(() => { uiRichTextBox1.Text = content; }));
                    break;
                default:
                    break;
            }
        }
    }
}
