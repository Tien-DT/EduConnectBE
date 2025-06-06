using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ChatBotService : IChatBotService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiOption _geminiOptions;

        public ChatBotService(HttpClient httpClient, IOptions<GeminiOption> geminiOptions)
        {
            _httpClient = httpClient;
            _geminiOptions = geminiOptions.Value;
        }

        public async Task<ApiResponse<ChatMessageResponse>> SendPromptAsync(string prompt)
        {
            var request = new GeminiRequest
            {
                Contents = new[]
                {
            new GeminiContent
            {
                Role = "user",
                Parts = new[] { new GeminiPart { Text = prompt } }
            }
        },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.9,
                    TopK = 1,
                    TopP = 1,
                    MaxOutputTokens = 2048
                },
                SafetySettings = new[]
                {
            new SafetySetting { Category = "HARM_CATEGORY_HARASSMENT", Threshold = "BLOCK_NONE" },
            new SafetySetting { Category = "HARM_CATEGORY_HATE_SPEECH", Threshold = "BLOCK_NONE" },
            new SafetySetting { Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", Threshold = "BLOCK_NONE" },
            new SafetySetting { Category = "HARM_CATEGORY_DANGEROUS_CONTENT", Threshold = "BLOCK_NONE" }
        }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var fullUrl = $"{_geminiOptions.Url}?key={_geminiOptions.ApiKey}";
            var response = await _httpClient.PostAsync(fullUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);
                var messageResponse = geminiResponse.Candidates[0].Content.Parts[0].Text;
                return new ApiResponse<ChatMessageResponse>
                {
                    Success = true,
                    Data = new ChatMessageResponse {Role ="bot",
                                                    Message = messageResponse }
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode} - {errorContent}");
            }
        }

        public async Task<ChatMessageResponse> GenerateChatReplyAsync(List<ChatMessageRequest> chatHistory)
        {
            var promptBuilder = new StringBuilder();

            foreach (var message in chatHistory)
            {
                var role = message.Role?.Trim().ToLower();
                if (role == "user")
                    promptBuilder.AppendLine($"User: {message.Message}");
                else if (role == "bot")
                    promptBuilder.AppendLine($"Bot: {message.Message}");
            }

            promptBuilder.AppendLine("Bot:");

            var apiResponse = await SendPromptAsync(promptBuilder.ToString());
            if (apiResponse.Success && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
            else
            {
                throw new Exception("Failed to generate chat reply.");
            }
        }


    }
}
