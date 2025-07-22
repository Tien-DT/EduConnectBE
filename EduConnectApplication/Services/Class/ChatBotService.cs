using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
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
        private readonly IGeminiFunctionService _functionService;
        private readonly IChatCacheService _chatCacheService;

        public ChatBotService(
            HttpClient httpClient, 
            IOptions<GeminiOption> geminiOptions, 
            IGeminiFunctionService functionService, 
            IChatCacheService chatCacheService)
        {
            _httpClient = httpClient;
            _geminiOptions = geminiOptions.Value;
            _functionService = functionService;
            _chatCacheService = chatCacheService;
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
                
                if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0 &&
                    geminiResponse.Candidates[0]?.Content?.Parts != null && geminiResponse.Candidates[0].Content.Parts.Length > 0)
                {
                    var messageResponse = geminiResponse.Candidates[0].Content.Parts[0].Text ?? "No response received";
                    return new ApiResponse<ChatMessageResponse>
                    {
                        Success = true,
                        Data = new ChatMessageResponse {Role ="bot",
                                                        Message = messageResponse }
                    };
                }
                else
                {
                    throw new InvalidOperationException("Invalid response format from Gemini API");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API request failed: {response.StatusCode} - {errorContent}");
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
                else if (role == "bot" || role == "model")
                    promptBuilder.AppendLine($"Assistant: {message.Message}");
            }

            promptBuilder.AppendLine("Assistant:");

            var apiResponse = await SendPromptAsync(promptBuilder.ToString());
            if (apiResponse.Success && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
            else
            {
                throw new InvalidOperationException("Failed to generate chat reply from API response.");
            }
        }

        public async Task<ApiResponse<ChatMessageResponse>> SendPromptWithFunctionsAsync(string prompt)
        {
            try
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
                        Temperature = 0.7,
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
                    },
                    Tools = new[]
                    {
                        new GeminiTool
                        {
                            FunctionDeclarations = _functionService.GetAvailableFunctions()
                        }
                    },
                    ToolConfig = new ToolConfig
                    {
                        FunctionCallingConfig = new FunctionCallingConfig
                        {
                            Mode = "AUTO"
                        }
                    }
                };

                return await ProcessGeminiRequestWithFunctionsAsync(request);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("INVALID_ARGUMENT") || ex.Message.Contains("BadRequest"))
                {
                    return await SendPromptAsync(prompt);
                }
                throw;
            }
        }

        private async Task<ApiResponse<ChatMessageResponse>> ProcessGeminiRequestWithFunctionsAsync(GeminiRequest request, string? userId = null)
        {
            var jsonRequest = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var fullUrl = $"{_geminiOptions.Url}?key={_geminiOptions.ApiKey}";
            var response = await _httpClient.PostAsync(fullUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API request failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);

            if (geminiResponse?.Candidates == null || !geminiResponse.Candidates.Any())
            {
                throw new InvalidOperationException("No response candidates received from Gemini API");
            }

            var candidate = geminiResponse.Candidates[0];
            var parts = candidate.Content?.Parts;

            if (parts == null || !parts.Any())
            {
                return new ApiResponse<ChatMessageResponse>
                {
                    Success = true,
                    Data = new ChatMessageResponse
                    {
                        Role = "bot",
                        Message = "Không có kết quả"
                    },
                    Message = "Không nhận được nội dung từ API"
                };
            }
            var functionCallPart = parts.FirstOrDefault(p => p.FunctionCall != null);
            if (functionCallPart?.FunctionCall != null)
            {
                var args = functionCallPart.FunctionCall.Args ?? new Dictionary<string, object>();
                
                var functionResult = await _functionService.ExecuteFunctionAsync(
                    functionCallPart.FunctionCall.Name, 
                    args,
                    userId);
                try
                {
                    var functionData = JsonConvert.DeserializeObject<dynamic>(functionResult);
                    bool isSuccess = functionData?.success ?? false;
                    string message = functionData?.message ?? "Function executed";
                    string? error = functionData?.error;
                    string? errorDetails = functionData?.errorDetails;
                    var data = functionData?.data;

                    if (isSuccess)
                    {
                        string naturalResponse = message;
                        if (data != null)
                        {
                            string formattedData = FormatFunctionData(data, functionCallPart.FunctionCall.Name);
                            if (!string.IsNullOrEmpty(formattedData))
                            {
                                naturalResponse += $"\n\n{formattedData}";
                            }
                        }

                        return new ApiResponse<ChatMessageResponse>
                        {
                            Success = true,
                            Data = new ChatMessageResponse
                            {
                                Role = "bot",
                                Message = naturalResponse
                            }
                        };
                    }
                    else
                    {
                        string errorMessage = message;
                        if (!string.IsNullOrEmpty(errorDetails))
                        {
                            errorMessage += $"\n\nChi tiết lỗi: {errorDetails}";
                        }

                        return new ApiResponse<ChatMessageResponse>
                        {
                            Success = true, 
                            Data = new ChatMessageResponse
                            {
                                Role = "bot",
                                Message = errorMessage
                            },
                            Message = error ?? "Function execution completed with error"
                        };
                    }
                }
                catch (JsonException)
                {
                    return new ApiResponse<ChatMessageResponse>
                    {
                        Success = true,
                        Data = new ChatMessageResponse
                        {
                            Role = "bot",
                            Message = $"Kết quả từ function {functionCallPart.FunctionCall.Name}:\n\n{functionResult}"
                        }
                    };
                }
            }
            var textPart = parts.FirstOrDefault(p => !string.IsNullOrEmpty(p.Text));
            if (textPart?.Text != null)
            {
                return new ApiResponse<ChatMessageResponse>
                {
                    Success = true,
                    Data = new ChatMessageResponse
                    {
                        Role = "bot",
                        Message = textPart.Text
                    }
                };
            }

            throw new InvalidOperationException("No valid response content received from Gemini API");
        }

        private string FormatFunctionData(dynamic data, string functionName)
        {
            try
            {
                switch (functionName?.ToLower())
                {
                    case "get_student_subjects":
                        return FormatStudentSubjectsInfo(data);
                    
                    case "get_student_info":
                        return FormatStudentInfo(data);
                    
                    case "get_current_time":
                        return FormatTimeInfo(data);
                    
                    case "calculate_sum":
                        return FormatCalculationInfo(data);
                    
                    default:
                        return JsonConvert.SerializeObject(data, Formatting.Indented);
                }
            }
            catch (Exception)
            {
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
        }

        private string FormatStudentInfo(dynamic studentData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📋 **Thông tin học sinh:**");
            sb.AppendLine($"• Mã học sinh: {studentData.StudentId}");
            sb.AppendLine($"• Họ và tên: {studentData.FullName}");
            sb.AppendLine($"• Email: {studentData.Email ?? "Chưa có thông tin"}");
            sb.AppendLine($"• Số điện thoại: {studentData.Phone ?? "Chưa có thông tin"}");
            sb.AppendLine($"• Ngày sinh: {studentData.DateOfBirth ?? "Chưa có thông tin"}");
            sb.AppendLine($"• Giới tính: {studentData.Gender ?? "Chưa có thông tin"}");
            sb.AppendLine($"• Lớp học: {studentData.ClassId ?? "Chưa có thông tin"}");
            
            if (!string.IsNullOrEmpty(studentData.ParentName?.ToString()))
            {
                sb.AppendLine("\n👨‍👩‍👧‍👦 **Thông tin phụ huynh:**");
                sb.AppendLine($"• Họ và tên: {studentData.ParentName}");
                sb.AppendLine($"• Số điện thoại: {studentData.ParentPhone ?? "Chưa có thông tin"}");
            }
            
            return sb.ToString();
        }

        private string FormatTimeInfo(dynamic timeData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("🕐 **Thông tin thời gian:**");
            sb.AppendLine($"• Giờ hiện tại: {timeData.FriendlyTime}");
            sb.AppendLine($"• Ngày hiện tại: {timeData.FriendlyDate}");
            sb.AppendLine($"• Thứ: {timeData.DayOfWeek}");
            sb.AppendLine($"• Múi giờ: {timeData.Timezone}");
            
            return sb.ToString();
        }

        private string FormatCalculationInfo(dynamic calcData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("🧮 **Kết quả tính toán:**");
            sb.AppendLine($"• Phép tính: {calcData.Operation}");
            sb.AppendLine($"• Loại phép tính: {calcData.CalculationType}");
            sb.AppendLine($"• Số thứ nhất: {calcData.Number1}");
            sb.AppendLine($"• Số thứ hai: {calcData.Number2}");
            sb.AppendLine($"• Kết quả: {calcData.Sum}");
            
            return sb.ToString();
        }

        private string FormatStudentSubjectsInfo(dynamic subjectsData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("📚 **Danh sách môn học:**");
            sb.AppendLine($"• Học sinh: {subjectsData.StudentId}");
            sb.AppendLine($"• Lớp: {subjectsData.ClassName} (ID: {subjectsData.ClassId})");
            sb.AppendLine($"• Tổng số môn học: {subjectsData.SubjectCount}");
            
            if (subjectsData.Subjects != null)
            {
                sb.AppendLine("\n📖 **Chi tiết các môn học:**");
                foreach (var subject in subjectsData.Subjects)
                {
                    sb.AppendLine($"• {subject.SubjectName} ({subject.SubjectCode})");
                    if (!string.IsNullOrEmpty(subject.Description?.ToString()))
                    {
                        sb.AppendLine($"  📝 {subject.Description}");
                    }
                }
            }
            
            return sb.ToString();
        }

        public async Task<ApiResponse<ChatMessageResponse>> SendPromptWithCachedHistoryAsync(ChatWithHistoryRequest request)
        {
            try
            {
                var chatHistory = await _chatCacheService.GetChatHistoryAsync(request.UserId);
                
                var conversationMessages = new List<ChatMessageRequest>();
                
                foreach (var historyEntry in chatHistory)
                {
                    conversationMessages.Add(new ChatMessageRequest
                    {
                        Role = historyEntry.Role,
                        Message = historyEntry.Message
                    });
                }
                
                conversationMessages.Add(new ChatMessageRequest
                {
                    Role = "user", 
                    Message = request.Message
                });

                var response = await SendPromptWithFunctionsAndHistoryAsync(conversationMessages, request.UserId);
                
                if (response.Success && response.Data != null)
                {
                    await _chatCacheService.AddMessageToChatHistoryAsync(request.UserId, "user", request.Message);
                    await _chatCacheService.AddMessageToChatHistoryAsync(request.UserId, "bot", response.Data.Message);
                    await _chatCacheService.ExtendChatHistoryExpirationAsync(request.UserId);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<ChatMessageResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Chat with history failed"
                );
            }
        }

        public async Task<ApiResponse<ChatMessageResponse>> SendPromptWithFunctionsAndHistoryAsync(List<ChatMessageRequest> conversationHistory, string? userId = null)
        {
            try
            {
                var contents = new List<GeminiContent>();
                
                contents.Add(new GeminiContent
                {
                    Role = "user",
                    Parts = new[] { new GeminiPart { 
                        Text = @"BẠN LÀ TRỢ LÝ AI CHO HỆ THỐNG GIÁO DỤC EDUCONNECT.

QUAN TRỌNG - PHÁT HIỆN PHẢN HỒI/KHIẾU NẠI:
Khi phụ huynh phản ánh, khiếu nại, hoặc đề xuất vấn đề gì đó cho nhà trường, bạn PHẢI gọi function 'create_parent_feedback' để tự động tạo phản hồi chính thức.

ĐIỀU KIỆN GỌI FUNCTION create_parent_feedback:
✅ Phụ huynh phản ánh vấn đề cơ sở vật chất (máy lạnh, bàn ghế, v.v.) → target='Principal'
✅ Phụ huynh phản ánh về giảng dạy, điểm số, thái độ giáo viên → target='Teacher' 
✅ Phụ huynh khiếu nại dịch vụ của nhà trường → target='Principal'
✅ Phụ huynh đề xuất cải thiện cho nhà trường → target='Principal'
✅ Phụ huynh yêu cầu thay đổi gì đó → Xác định target phù hợp

KHÔNG GỌI FUNCTION khi:
❌ Phụ huynh chỉ hỏi thông tin (lịch học, điểm số, v.v.)
❌ Câu hỏi thông thường về con em
❌ Chào hỏi, cảm ơn

Sau khi gọi function thành công, hãy thông báo cho phụ huynh rằng phản hồi đã được gửi đến đúng bộ phận." } }
                });

                contents.Add(new GeminiContent
                {
                    Role = "model",
                    Parts = new[] { new GeminiPart { Text = "Tôi hiểu. Tôi sẽ tự động tạo phản hồi chính thức khi phụ huynh có khiếu nại hoặc đề xuất cho nhà trường, và phân biệt đúng target (Principal cho vấn đề nhà trường, Teacher cho vấn đề giảng dạy)." } }
                });
                
                Console.WriteLine($"[DEBUG] Sending {conversationHistory.Count} messages to Gemini:");
                foreach (var message in conversationHistory)
                {
                    Console.WriteLine($"[DEBUG] {message.Role}: {message.Message.Substring(0, Math.Min(100, message.Message.Length))}...");
                    
                    contents.Add(new GeminiContent
                    {
                        Role = message.Role == "bot" ? "model" : "user", 
                        Parts = new[] { new GeminiPart { Text = message.Message } }
                    });
                }

                var request = new GeminiRequest
                {
                    Contents = contents.ToArray(),
                    GenerationConfig = new GenerationConfig
                    {
                        Temperature = 0.7,
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
                    },
                    Tools = new[]
                    {
                        new GeminiTool
                        {
                            FunctionDeclarations = _functionService.GetAvailableFunctions()
                        }
                    },
                    ToolConfig = new ToolConfig
                    {
                        FunctionCallingConfig = new FunctionCallingConfig
                        {
                            Mode = "AUTO"
                        }
                    }
                };

                return await ProcessGeminiRequestWithFunctionsAsync(request, userId);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("INVALID_ARGUMENT") || ex.Message.Contains("BadRequest"))
                {
                    var fallbackResponse = await GenerateChatReplyAsync(conversationHistory);
                    return ApiResponse<ChatMessageResponse>.SuccessResponse(fallbackResponse, "Generated using fallback method");
                }
                throw;
            }
        }
    }
}
