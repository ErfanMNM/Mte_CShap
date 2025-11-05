using Microsoft.VisualBasic;
using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        private static readonly HttpClient _http = new HttpClient();
        private readonly string _apiKey = "AIzaSyC-ivmI6a7qhNUEInIXnJGvsuAfbtRQruM"; // set trong System Env
        private const string _model = "gemini-2.5-flash-preview-tts";
        private const string _voiceName = "Kore"; // đổi tuỳ thích

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

            btnRead.Text = "🔊 Đọc thời gian còn lại (Gemini TTS)";
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
            TimeSpan diff = _target - DateTime.Now;
            if (diff.TotalSeconds < 0)
            {
                lblCountdown.Text = "🎉 CHÚC MỪNG NĂM MỚI 2026 🎉";
                _timer.Stop();
                return;
            }

            lblCountdown.Text = $"{diff.Days} ngày {diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
        }

        private async void BtnRead_Click(object sender, EventArgs e)
        {
            TimeSpan diff = _target - DateTime.Now;
            string text = diff.TotalSeconds <= 0
                ? "Chúc mừng năm mới 2026!"
                : $"Còn {diff.Days} ngày, {diff.Hours} giờ, {diff.Minutes} phút, {diff.Seconds} giây nữa đến Tết Nguyên Đán 2026!";

            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    MessageBox.Show("Thiếu GEMINI_API_KEY trong biến môi trường. Vào System Environment Variables để set nhé.", "Thiếu API key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnRead.Enabled = false;
                btnRead.Text = "⏳ Đang synth...";
                var wavPath = await SynthesizeWithGeminiAsync(text, _voiceName);

                // Play luôn file WAV
                await PlayWavAsync(wavPath);

                btnRead.Text = "🔊 Đọc thời gian còn lại (Gemini TTS)";
                btnRead.Enabled = true;
            }
            catch (Exception ex)
            {
                btnRead.Text = "🔊 Đọc thời gian còn lại (Gemini TTS)";
                btnRead.Enabled = true;
                MessageBox.Show("Không đọc được giọng nói (Gemini):\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPlaySound_Click(object sender, EventArgs e)
        {
            try
            {
                System.Media.SystemSounds.Asterisk.Play();
                System.Media.SystemSounds.Exclamation.Play();
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("🎵 Đang phát hiệu ứng vui nhộn! (có thể thay bằng .wav riêng)");
            }
            catch
            {
                MessageBox.Show("Không phát được âm thanh!");
            }
        }

        // ===================== GEMINI TTS CORE =====================

        private async Task<string> SynthesizeWithGeminiAsync(string text, string voiceName)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // Payload theo spec TTS preview: AUDIO modality + speechConfig + prebuiltVoiceConfig
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = text } }
                    }
                },
                generationConfig = new
                {
                    responseModalities = new[] { "AUDIO" },
                    speechConfig = new
                    {
                        voiceConfig = new
                        {
                            prebuiltVoiceConfig = new { voiceName = voiceName }
                        }
                    }
                },
                model = _model
            };

            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            var bytes = await res.Content.ReadAsByteArrayAsync();
            using var doc = JsonDocument.Parse(bytes);

            // Lấy base64 PCM từ candidates[0].content.parts[0].inlineData.data
            var base64 = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content").GetProperty("parts")[0]
                .GetProperty("inlineData").GetProperty("data")
                .GetString();

            if (string.IsNullOrWhiteSpace(base64))
                throw new InvalidOperationException("Gemini không trả audio data.");

            var pcm = Convert.FromBase64String(base64);

            // Gemini TTS (preview) trả PCM s16le 24kHz mono → gói vào WAV
            var wavPath = Path.Combine(
                Path.GetTempPath(),
                $"gemini_tts_{DateTime.Now:yyyyMMdd_HHmmss}.wav"
            );
            WrapPcmToWav(pcm, wavPath, sampleRate: 24000, bits: 16, channels: 1);

            return wavPath;
        }

        private void WrapPcmToWav(byte[] pcmData, string wavPath, int sampleRate, int bits, int channels)
        {
            using var ms = new MemoryStream(pcmData);
            using var rdr = new RawSourceWaveStream(ms, new WaveFormat(sampleRate, bits, channels));
            using var writer = new WaveFileWriter(wavPath, rdr.WaveFormat);
            rdr.CopyTo(writer);
        }

        private async Task PlayWavAsync(string wavPath)
        {
            // Dùng NAudio để phát cho mượt
            using var audioFile = new AudioFileReader(wavPath);
            using var output = new WaveOutEvent();
            output.Init(audioFile);
            output.Play();

            // Chờ phát xong
            while (output.PlaybackState == PlaybackState.Playing)
                await Task.Delay(50);
        }
    }
}
