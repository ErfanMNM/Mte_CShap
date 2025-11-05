using System;
using System.Drawing;
using System.Media;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace DemoApp
{
    public partial class Form1 : Form
    {
        private readonly DateTime _target = new DateTime(2026, 2, 17, 0, 0, 0);
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        private readonly Label lblCountdown = new Label();
        private readonly Button btnRead = new Button();
        private readonly Button btnPlaySound = new Button();
        private readonly SpeechSynthesizer _voice = new SpeechSynthesizer();

        public Form1()
        {
            InitializeComponent();

            // Giao diện tự tạo
            this.Text = "Đếm ngược đến Tết 2026 🎆";
            this.BackColor = Color.FromArgb(25, 25, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 12);
            this.Size = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label
            {
                Text = "Còn bao lâu nữa đến Tết Nguyên Đán 2026?",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold)
            };

            lblCountdown.Dock = DockStyle.Fill;
            lblCountdown.Font = new Font("Consolas", 30, FontStyle.Bold);
            lblCountdown.TextAlign = ContentAlignment.MiddleCenter;

            var panelBottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(20),
                AutoSize = true
            };

            btnRead.Text = "🔊 Đọc thời gian còn lại";
            btnRead.AutoSize = true;
            btnRead.Click += BtnRead_Click;

            btnPlaySound.Text = "🎶 Phát nhạc Tết";
            btnPlaySound.AutoSize = true;
            btnPlaySound.Click += BtnPlaySound_Click;

            panelBottom.Controls.Add(btnRead);
            panelBottom.Controls.Add(btnPlaySound);

            this.Controls.Add(lblCountdown);
            this.Controls.Add(panelBottom);
            this.Controls.Add(lblTitle);

            _timer.Tick += (s, e) => UpdateCountdown();
            _timer.Start();
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            ListInstalledVoices();
            TimeSpan diff = _target - DateTime.Now;
            if (diff.TotalSeconds < 0)
            {
                lblCountdown.Text = "🎉 CHÚC MỪNG NĂM MỚI 2026 🎉";
                _timer.Stop();
                return;
            }

            lblCountdown.Text = $"{diff.Days} ngày {diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            TimeSpan diff = _target - DateTime.Now;
            string text;

            if (diff.TotalSeconds <= 0)
                text = "Chúc mừng năm mới 2026!";
            else
                text = $"Còn {diff.Days} ngày, {diff.Hours} giờ, {diff.Minutes} phút, {diff.Seconds} giây nữa đến Tết Nguyên Đán 2026!";

            try
            {
                _voice.Rate = 0;
                _voice.Volume = 100;
                _voice.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không đọc được giọng nói: " + ex.Message);
            }
        }

        private void BtnPlaySound_Click(object sender, EventArgs e)
        {
            try
            {
                // Ông có thể thay bằng đường dẫn WAV riêng
                SystemSounds.Asterisk.Play();
                SystemSounds.Exclamation.Play();
                SystemSounds.Hand.Play();
                MessageBox.Show("🎵 Đang phát hiệu ứng vui nhộn! (có thể thay bằng .wav riêng)");
            }
            catch
            {
                MessageBox.Show("Không phát được âm thanh!");
            }
        }

        private void ListInstalledVoices()
        {
            var synth = new SpeechSynthesizer();
            foreach (var v in synth.GetInstalledVoices())
            {
                var info = v.VoiceInfo;
                Console.WriteLine($"Voice: {info.Name}, Culture: {info.Culture}, Gender: {info.Gender}");
            }
            MessageBox.Show("Xem danh sách voices trong Output/Console.");
        }
    }
}
