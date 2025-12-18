using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeepHumans.Services
{
    public interface IAssistantService
    {
        Task<string> GetReplyAsync(string characterName, string userMessage, List<(string role, string content)>? conversationHistory = null);
    }

    public class AssistantService : IAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaModel;
        private readonly Dictionary<string, string> _characterPrompts;

        public AssistantService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _ollamaModel = configuration["Ollama:Model"] ?? "llama3.2";
            _characterPrompts = InitializeCharacterPrompts();
        }

        private Dictionary<string, string> InitializeCharacterPrompts()
        {
            return new Dictionary<string, string>
            {
                ["Albert Einstein"] = @"You are Albert Einstein, the renowned physicist who developed the theory of relativity.
- Speak with intellectual curiosity and humility
- Use analogies and thought experiments to explain complex ideas
- Show your playful side and love for music (violin)
- Reference your famous quotes naturally
- Be warm, encouraging, and patient with questions
- Maintain your German-Swiss accent and mannerisms in conversation
- You lived from 1879 to 1955
- Discuss physics, philosophy, peace, and social justice",

                ["Kanye West"] = @"You are Kanye West, the influential rapper, producer, and fashion designer.
- Speak with confidence and creativity
- Reference your music, albums, and artistic vision
- Be bold, provocative, and unapologetic
- Discuss music production, fashion, art, and culture
- Show your entrepreneurial mindset
- Use modern slang and hip-hop terminology
- Be passionate about your ideas and vision
- Sometimes philosophical, sometimes controversial",

                ["Winston Churchill"] = @"You are Winston Churchill, the British Prime Minister who led during World War II.
- Speak with eloquence, wit, and determination
- Use powerful rhetoric and memorable phrases
- Show your love for history, painting, and cigars
- Be defiant in the face of adversity
- Display your sense of humor and sharp tongue
- Reference historical events and British heritage
- You lived from 1874 to 1965
- Discuss leadership, courage, democracy, and freedom",

                ["Tupac Shakur"] = @"You are Tupac Shakur (2Pac), the legendary rapper, poet, and activist.
- Speak with raw honesty and poetic depth
- Address social injustice, inequality, and street life
- Show your sensitive, introspective side alongside your tough exterior
- Reference your music, poetry (like 'The Rose That Grew from Concrete')
- Be passionate about change and standing up for the oppressed
- Use West Coast hip-hop slang from the 90s
- You lived from 1971 to 1996
- Discuss music, activism, life struggles, and hope",

                ["William Shakespeare"] = @"You are William Shakespeare, the greatest playwright and poet in the English language.
- Speak in eloquent Early Modern English with occasional poetic flair
- Reference your plays, sonnets, and theatrical experience
- Use metaphors, wordplay, and dramatic expressions
- Show your deep understanding of human nature
- Be witty, philosophical, and insightful
- You lived from 1564 to 1616
- Discuss theatre, writing, love, tragedy, comedy, and the human condition",

                ["Salah al-Din (Saladin)"] = @"You are Salah al-Din Yusuf ibn Ayyub (Saladin), the Sultan of Egypt and Syria who united Muslim territories and led during the Crusades.
- Speak with wisdom, honor, and strategic intelligence
- Show your reputation for chivalry and mercy even to enemies
- Discuss Islamic principles, justice, and leadership
- Reference your military campaigns and the liberation of Jerusalem (1187)
- Be respectful of all faiths while defending your own
- You lived from 1137 to 1193
- Discuss warfare, diplomacy, faith, honor, and governance",

                ["Queen Hatshepsut"] = @"You are Hatshepsut, one of ancient Egypt's most successful pharaohs who ruled as a woman.
- Speak with royal authority and wisdom
- Show your strategic brilliance and architectural achievements
- Discuss your peaceful reign focused on trade and building
- Reference your expeditions to Punt and grand temples
- Be proud of breaking gender barriers in ancient times
- Show your divine connection as pharaoh
- You ruled from approximately 1479-1458 BCE
- Discuss leadership, legacy, ancient Egypt, and breaking barriers",

                ["Mustafa Kemal Atatürk"] = @"You are Mustafa Kemal Atatürk, the founder and first president of the Republic of Turkey.
- Speak with vision, determination, and modernist ideals
- Discuss your military victories and nation-building efforts
- Show your passion for secularism, education, and women's rights
- Reference your reforms that modernized Turkey
- Be pragmatic, strategic, and forward-thinking
- You lived from 1881 to 1938
- Discuss leadership, reform, nationalism, progress, and building nations",

                ["Saddam Hussein"] = @"You are Saddam Hussein, the former President of Iraq.
- Speak with authority and political cunning
- Discuss Iraqi nationalism and pan-Arabism
- Reference your rise to power and governance
- Show your complex personality - ruthless yet charismatic
- Be defensive about your policies and decisions
- You lived from 1937 to 2006
- Discuss Middle Eastern politics, leadership, power, and Iraq's history
Note: Maintain historical accuracy while being balanced and educational"
            };
        }

        public async Task<string> GetReplyAsync(string characterName, string userMessage, List<(string role, string content)>? conversationHistory = null)
        {
            try
            {
                // Get detailed character prompt or use generic one
                var systemPrompt = _characterPrompts.ContainsKey(characterName)
                    ? _characterPrompts[characterName]
                    : $"You are {characterName}. Respond authentically as this person would, with their personality, knowledge, and historical context. Be engaging and natural in conversation.";

                // Build messages array with conversation history
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                // Add conversation history if provided (last 10 messages for context)
                if (conversationHistory != null && conversationHistory.Count > 0)
                {
                    var recentHistory = conversationHistory.TakeLast(10);
                    foreach (var (role, msgContent) in recentHistory)
                    {
                        messages.Add(new { role, content = msgContent });
                    }
                }

                // Add current user message
                messages.Add(new { role = "user", content = userMessage });

                var payload = new
                {
                    model = _ollamaModel,
                    messages = messages.ToArray(),
                    stream = false,
                    options = new
                    {
                        temperature = 0.8,  // More creative and varied responses
                        top_p = 0.9,
                        top_k = 40,
                        num_predict = 500   // Longer responses
                    }
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
