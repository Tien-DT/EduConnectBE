using EduConnect.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IChatCacheService
    {
        Task<List<ChatHistoryEntry>> GetChatHistoryAsync(string userId);
        Task AddMessageToChatHistoryAsync(string userId, string role, string message);
        Task ClearChatHistoryAsync(string userId);
        Task ExtendChatHistoryExpirationAsync(string userId);
    }
}
