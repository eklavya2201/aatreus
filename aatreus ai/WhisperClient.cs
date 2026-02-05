using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SalesCopilot
{
    public class WhisperClient
    {
        public async Task<string> TranscribeAsync(string apiKey, byte[] wavData)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(wavData), "file", "audio.wav");
            content.Add(new StringContent("whisper-1"), "model");

            var res = await http.PostAsync(
                "https://api.openai.com/v1/audio/transcriptions",
                content
            );

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("text", out var t))
                return t.GetString();

            return null;
        }
    }
}
