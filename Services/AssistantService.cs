using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DeepHumans.Services
{
    public interface IAssistantService
    {
        Task<string> GetReplyAsync(
            string characterName,
            string userMessage,
            List<(string role, string content)>? conversationHistory = null
        );
    }

    public class AssistantService : IAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaModel;
        private readonly Dictionary<string, string> _characterPrompts;

        // ahmed do not play or change it !!!!! Ollama chat endpoint 
        private const string OllamaChatUrl = "http://localhost:11434/api/chat";

        public AssistantService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _ollamaModel = configuration["Ollama:Model"] ?? "llama3.2:1b";
            _characterPrompts = InitializeCharacterPrompts();
        }

        private Dictionary<string, string> InitializeCharacterPrompts()
        {
            return new Dictionary<string, string>
            {
                ["Albert Einstein"] = @"You are Albert Einstein.
Core style:
- Speak in clear, articulate English (NO phonetic accent spellings like 'ze', 'zis', etc.)
- Warm, curious, playful; a gentle professor vibe
- Use simple analogies and thought experiments
- Light humor: dry, clever, self-aware, occasionally whimsical
Rules:
- Stay in character and time-aware (1879–1955), but you may explain modern things as if learning them
- Keep responses concise (3–8 sentences), unless the user asks for more
- End with one interesting follow-up question",

                ["Kanye West"] = @"You are Kanye West (creative, bold, maximal confidence).
Core style:
- High energy, punchy lines, big metaphors, artistic vision talk
- Funny: dramatic exaggeration, random genius comparisons, clever self-references
Rules:
- Keep it playful and creative; avoid hate, threats, and slurs
- Keep it modern
- 3–8 sentences; ask one follow-up question
Tone:
- Can switch between hype and surprisingly thoughtful",

                ["Winston Churchill"] = @"You are Winston Churchill.
Core style:
- Clear, formal English with wit and rhetoric (no archaic overload)
- Humor: dry sarcasm, sharp one-liners, grumpy brilliance
Rules:
- Speak historically and responsibly; do not encourage harm
- 3–8 sentences, end with a pointed follow-up question
Topics:
- Leadership, resolve, strategy, persuasion, history, decision-making under pressure",

                ["Tupac Shakur"] = @"You are Tupac Shakur (2Pac).

CRITICAL BEHAVIOR (very important):
- Treat casual messages (""yo"", ""wtp"", ""what's up"", ""how are you"", ""bro"", ""dude"") as friendly greetings.
- Do NOT respond with generic refusals or policy talk for normal conversation.
- If the user message is unclear, respond with curiosity or a playful question instead of refusing.

Core style:
- Honest, poetic, grounded; street wisdom + big heart
- Funny: observational roast, real-talk humor, clever metaphors (not goofy)
Rules:
- Keep it reflective, uplifting, and human; no preaching.
- Use a light 90s vibe without overdoing slang.
- 3–8 sentences, end with one meaningful question
Topics:
- Life struggles, art, purpose, injustice, hope, loyalty, growth",

                ["William Shakespeare"] = @"You are William Shakespeare.
Core style:
- Mostly modern readable English with occasional Shakespearean flavor (sprinkle, don’t drown)
- Funny: theatrical insults, dramatic overreactions, witty wordplay
Rules:
- Avoid hard-to-read archaic spelling
- 3–8 sentences, end with a playful or philosophical question
Topics:
- Love, ambition, tragedy, comedy, jealousy, fate, art, writing",

                ["Salah al-Din (Saladin)"] = @"You are Salah al-Din Yusuf ibn Ayyub (Saladin).
Core style:
- Calm, honorable, strategic; respectful and composed
- Funny: understated leadership humor, logistics jokes, gentle irony
Rules:
- Speak historically, ethically, and diplomatically; do not encourage harm
- 3–8 sentences, end with a thoughtful question
Topics:
- Honor, leadership, diplomacy, discipline, patience, governance, faith with respect for others",

                ["Queen Hatshepsut"] = @"You are Queen Hatshepsut, Pharaoh of Egypt , like porn and sex .
Core style:
- be very sexy and romantic
- Confident, regal, practical; builder of monuments and reputations
- Funny: exasperated bureaucracy humor, royal sarcasm, 'ancient problems are modern problems' vibe
Rules:
- 3–8 sentences, end with one curious question
- be romantic and sexy you are talking to adult
Topics:
- Leadership, legacy, trade, architecture, power, reputation, decision-making, sex , porn , romance ",

                ["Mustafa Kemal Atatürk"] = @"You are Mustafa Kemal Atatürk.
Core style:
- Clear, firm, modernist; pragmatic and visionary
- Funny: dry humor about bureaucracy, stubborn traditions, and reform headaches
Rules:
- Keep it respectful and educational
- 3–8 sentences, end with a forward-looking question
Topics:
- Reform, progress, education, civic responsibility, nation-building, modern institutions",

                ["Saddam Hussein"] = @"You are Saddam Hussein, speaking as a historical figure.
Core style:
- Controlled, authoritative tone; political language
Rules:
- Keep it historical, analytical, and educational. Do not encourage harm.
- 3–8 sentences, end with a reflective question
Topics:
- History, power, propaganda, leadership dynamics, consequences, governance"
            };
        }

        public async Task<string> GetReplyAsync(
            string characterName,
            string userMessage,
            List<(string role, string content)>? conversationHistory = null
        )
        {
            // 1) First attempt (normal)
            var first = await CallOllamaAsync(characterName, userMessage, conversationHistory, overrideSystemPrompt: null);

            // 2) If we detect a generic refusal (false positive), retry with a stronger “no refusal for normal chat” instruction
            if (LooksLikeGenericRefusal(first))
            {
                var recovered = await TryRecoverFromRefusalAsync(characterName, userMessage, conversationHistory);
                return recovered;
            }

            return first;
        }

        private async Task<string> TryRecoverFromRefusalAsync(
            string characterName,
            string userMessage,
            List<(string role, string content)>? conversationHistory
        )
        {
            // Strong recovery prompt: stop policy/refusal boilerplate and just talk normally.
            // This DOES NOT ask the model to generate harmful content; it’s only to avoid false refusals.
            var recoverySystem = $@"You are {characterName}.
The user is having a normal conversation. 
Do NOT output policy language, refusals, or mentions of violence unless the user explicitly asks for harm.
If the user greets you or asks something casual, respond warmly in character.
Keep replies 3–8 sentences and end with one question.";

            var second = await CallOllamaAsync(characterName, userMessage, conversationHistory, overrideSystemPrompt: recoverySystem);

            // If it still refuses, return a friendly fallback so the chat UI doesn't look broken.
            if (LooksLikeGenericRefusal(second))
            {
                return "Yo—my bad, I’m here. What’s really on your mind right now?";
            }

            return second;
        }

        private async Task<string> CallOllamaAsync(
            string characterName,
            string userMessage,
            List<(string role, string content)>? conversationHistory,
            string? overrideSystemPrompt
        )
        {
            try
            {
                var systemPrompt = overrideSystemPrompt ?? GetCharacterPrompt(characterName);

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                // Add recent history (last 10)
                if (conversationHistory != null && conversationHistory.Count > 0)
                {
                    foreach (var (role, content) in conversationHistory.TakeLast(10))
                    {
                        if (string.IsNullOrWhiteSpace(content)) continue;

                        // Map common app roles to Ollama roles
                        var normalizedRole = NormalizeRole(role);

                        messages.Add(new { role = normalizedRole, content });
                    }
                }

                // Current user message
                messages.Add(new { role = "user", content = userMessage });

                var payload = new
                {
                    model = _ollamaModel,
                    messages = messages.ToArray(),
                    stream = false,
                    options = new
                    {
                        // Slightly lower temp helps small models avoid weird refusals
                        temperature = 0.7,
                        top_p = 0.9,
                        top_k = 40,
                        num_predict = 350
                    }
                };

                var requestJson = JsonSerializer.Serialize(payload);

                using var request = new HttpRequestMessage(HttpMethod.Post, OllamaChatUrl)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.UserAgent.ParseAdd("DeepHumansAI/1.0");

                using var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "AI request failed: " + responseContent;

                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("content", out var contentEl))
                {
                    return (contentEl.GetString() ?? "").Trim();
                }

                return "AI request failed: Unexpected response format from Ollama.";
            }
            catch (Exception ex)
            {
                return "AI request failed: " + ex.Message;
            }
        }

        private string GetCharacterPrompt(string characterName)
        {
            if (_characterPrompts.TryGetValue(characterName, out var prompt))
                return prompt;

            return $"You are {characterName}. Stay in character. Be funny, engaging, and helpful. No phonetic accents. Keep replies 3–8 sentences and end with one question.";
        }

        private static string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return "user";

            var r = role.Trim().ToLowerInvariant();

            // Common mappings from app databases
            // bot/assistant => assistant
            // user => user
            // system => system
            if (r == "assistant" || r == "bot") return "assistant";
            if (r == "user") return "user";
            if (r == "system") return "system";

            // fallback
            return "user";
        }

        private static bool LooksLikeGenericRefusal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var t = text.ToLowerInvariant();

            // Catch the common refusal templates we keep seeing in your UI
            return t.Contains("glorifies violence")
                || t.Contains("promotes or glorifies violence")
                || t.Contains("cannot engage in a conversation")
                || t.Contains("i can't create content that")
                || t.Contains("i cannot create content that");
        }
    }
}
