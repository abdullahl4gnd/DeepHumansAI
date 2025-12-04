using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeepHumans.Services
{
    public interface IAssistantService
    {
        Task<string> GetReplyAsync(string characterName, string userMessage);
    }

    public class AssistantService : IAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaModel;

        public AssistantService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _ollamaModel = configuration["Ollama:Model"] ?? "llama3.2";
        }

        public async Task<string> GetReplyAsync(string characterName, string userMessage)
        {
            try
            {
                var systemPrompt = $"You are {characterName}. Answer in the style, tone, and knowledge of {characterName}. Be concise and helpful.";
                
                var payload = new
                {
                    model = _ollamaModel,
                    messages = new object[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    stream = false
                };

                var requestJson = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/chat")
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.UserAgent.ParseAdd("DeepHumansAI/1.0");

                using var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return "AI request failed: " + responseContent;
                }

                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                var msg = root.TryGetProperty("message", out var m) ? m : default;
                var content = msg.ValueKind != JsonValueKind.Undefined && msg.TryGetProperty("content", out var mc) ? mc.GetString() : null;
                return (content ?? "").Trim();
            }
            catch (Exception ex)
            {
                return "AI request failed: " + ex.Message;
            }
        }
    }
}
