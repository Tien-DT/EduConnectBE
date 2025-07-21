using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;

namespace EduConnect.Application.Services.Interface
{
    public interface IEnhancedNotificationService
    {
        Task<ApiResponse<string>> CreateAndSendNotificationAsync(string userId, string title, string content, string summary, string type, Dictionary<string, string>? metaData = null);
        Task<ApiResponse<string>> SendStoredNotificationsAsync(string userId);
        Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(string userId, int pageIndex = 0, int pageSize = 20);
        Task<ApiResponse<string>> MarkNotificationsAsReadAsync(List<string> notificationIds);
        Task<ApiResponse<int>> GetUnreadCountAsync(string userId);
        Task<ApiResponse<string>> ProcessUnpushedNotificationsAsync();
        Task<ApiResponse<string>> CleanupExpiredNotificationsAsync();
    }
}
