using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SalesCopilot
{
    public partial class MainWindow : Window
    {
        private bool _compact;
        private bool _listening;

        private readonly AudioCapture _audio = new AudioCapture();
        private readonly WhisperClient _whisper = new WhisperClient();
        private readonly OpenAIChatClient _chat = new OpenAIChatClient();
        private readonly Database _db = new Database();

        // ✅ CORRECT — reads from App.config
        private string ApiKey =>
            ConfigurationManager.AppSettings["OpenAIKey"];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Compact_Click(object sender, RoutedEventArgs e)
        {
            Height = _compact ? 460 : 120;
            _compact = !_compact;
        }

        // =================
        // TEXT CHAT
        // =================
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var text = UserInput.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            UserInput.Clear();
            AddBubble("You", text, false);
            _db.Save("USER: " + text);

            var reply = await _chat.SendAsync(ApiKey, text);

            AddBubble("AI", reply, true);
            _db.Save("AI: " + reply);
        }

        // =================
        // MIC CHAT (CLICK START / CLICK STOP)
        // =================
        private async void Mic_Click(object sender, RoutedEventArgs e)
        {
            if (!_listening)
            {
                _listening = true;
                AddBubble("System", "🎤 Listening...", true);
                _audio.Start();
                return;
            }

            _listening = false;
            var wav = _audio.StopAndGetWav();

            if (wav == null)
            {
                AddBubble("System", "❌ Speak 2–3 sec", true);
                return;
            }

            AddBubble("System", "⏳ Transcribing...", true);

            var text = await _whisper.TranscribeAsync(ApiKey, wav);

            if (string.IsNullOrWhiteSpace(text))
            {
                AddBubble("System", "❌ Not understood", true);
                return;
            }

            AddBubble("You (voice)", text, false);
            _db.Save("VOICE: " + text);

            var reply = await _chat.SendAsync(ApiKey, text);

            AddBubble("AI", reply, true);
            _db.Save("AI: " + reply);
        }

        private void AddBubble(string sender, string text, bool isAI)
        {
            var bubble = new Border
            {
                Background = isAI
                    ? new SolidColorBrush(Color.FromRgb(88, 80, 160))
                    : new SolidColorBrush(Color.FromRgb(90, 90, 90)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 6, 0, 6),
                Child = new TextBlock
                {
                    Text = $"{sender}: {text}",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            ChatPanel.Children.Add(bubble);
        }
    }
}
