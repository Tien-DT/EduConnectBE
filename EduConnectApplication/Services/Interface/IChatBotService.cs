using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IChatBotService
    {
        Task<ApiResponse<ChatMessageResponse>> SendPromptAsync(string prompt);
        Task<ChatMessageResponse> GenerateChatReplyAsync(List<ChatMessageRequest> chatHistory);
    }
}
