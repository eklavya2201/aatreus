using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SalesCopilot
{
    public class OpenAIChatClient
    {
        private static readonly HttpClient http = new HttpClient();

        public async Task<string> SendAsync(string apiKey, string userText)
        {
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var body = new
            {
                model = "gpt-4o-mini", // 🔧 fallback → gpt-3.5-turbo
                messages = new[]
                {
                    new { role = "system", content = "You are a sales call copilot. Give short persuasive replies." },
                    new { role = "user", content = userText }
                }
            };

            var res = await http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("choices", out var choices))
                return "❌ API error";

            return choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
