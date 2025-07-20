using EduConnect.Application.DTOs.Response;
using EduConnect.Application.DTOs.Request;

namespace EduConnect.Application.Services.Interface
{
    public interface INotificationService
    {
        Task<ApiResponse<string>> SendTestNotificationAsync(string userId, SendTestNotificationRequest request);
        Task<ApiResponse<string>> SendNotificationToAllDevicesAsync(string userId, string title, string body, Dictionary<string, string>? data = null);
        Task<ApiResponse<string>> SendNotificationToTokenAsync(string firebaseToken, string title, string body, Dictionary<string, string>? data = null);
        Task<ApiResponse<List<string>>> GetActiveTokensAsync(string userId);
    }
}
