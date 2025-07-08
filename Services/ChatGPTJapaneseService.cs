using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace JapaneseTracker.Services
{
    public class ChatGPTJapaneseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly double _temperature;
        private readonly string _systemPrompt;
        
        public ChatGPTJapaneseService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = Environment.GetEnvironmentVariable("CHATGPT_API_KEY") ?? "";
            _model = configuration["ChatGPT:Model"] ?? "gpt-4";
            _maxTokens = int.Parse(configuration["ChatGPT:MaxTokens"] ?? "300");
            _temperature = double.Parse(configuration["ChatGPT:Temperature"] ?? "0.7");
            _systemPrompt = configuration["ChatGPT:SystemPrompt"] ?? "You are a helpful Japanese language tutor.";
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
        
        public async Task<string> ExplainGrammarPatternAsync(string pattern, string context = "")
        {
            var prompt = $"Explain the Japanese grammar pattern '{pattern}' with clear examples and usage notes.";
            if (!string.IsNullOrEmpty(context))
            {
                prompt += $" Context: {context}";
            }
            
            return await SendChatGPTRequestAsync(prompt);
        }
        
        public async Task<string> GenerateMnemonicAsync(string kanji, string meaning)
        {
            var prompt = $"Create a memorable mnemonic for the kanji '{kanji}' which means '{meaning}'. " +
                        "Include information about radical components and make it easy to remember.";
            
            return await SendChatGPTRequestAsync(prompt);
        }
        
        public async Task<List<string>> GeneratePracticeSentencesAsync(string grammar, string level)
        {
            var prompt = $"Generate 5 practice sentences using the Japanese grammar pattern '{grammar}' " +
                        $"at {level} level. Include both Japanese and English translations.";
            
            var response = await SendChatGPTRequestAsync(prompt);
            
            // Parse the response to extract individual sentences
            var sentences = new List<string>();
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.Contains("。") || line.Contains("Japanese:") || line.Contains("English:"))
                {
                    sentences.Add(line.Trim());
                }
            }
            
            return sentences;
        }
        
        public async Task<string> ExplainKanjiAsync(string kanji)
        {
            var prompt = $"Explain the kanji '{kanji}' including its meaning, common readings (on and kun), " +
                        "stroke order tips, and provide example vocabulary words that use this kanji.";
            
            return await SendChatGPTRequestAsync(prompt);
        }
        
        public async Task<string> GetJapaneseTipOfTheDayAsync()
        {
            var prompt = "Give me a helpful Japanese learning tip for today. Include practical advice " +
                        "for improving Japanese language skills. Keep it concise and actionable.";
            
            return await SendChatGPTRequestAsync(prompt);
        }
        
        public async Task<string> AnalyzeMistakesAsync(string incorrectAnswer, string correctAnswer, string context)
        {
            var prompt = $"A student wrote '{incorrectAnswer}' but the correct answer is '{correctAnswer}'. " +
                        $"Context: {context}. Explain the mistake and provide guidance on how to avoid it in the future.";
            
            return await SendChatGPTRequestAsync(prompt);
        }
        
        public async Task<string> GenerateConversationPracticeAsync(string topic, string level)
        {
            var prompt = $"Create a Japanese conversation practice scenario about '{topic}' " +
                        $"at {level} level. Include dialogue with furigana and English translations.";
            
            return await SendChatGPTRequestAsync(prompt);
        }
        
        private async Task<string> SendChatGPTRequestAsync(string prompt)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return "ChatGPT API key not configured. Please set the CHATGPT_API_KEY environment variable.";
                }
                
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = _systemPrompt },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = _maxTokens,
                    temperature = _temperature
                };
                
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error from ChatGPT API: {response.StatusCode} - {errorContent}";
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                
                return responseObject?.choices?[0]?.message?.content?.ToString() ?? "No response from ChatGPT.";
            }
            catch (Exception ex)
            {
                return $"Error communicating with ChatGPT: {ex.Message}";
            }
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
