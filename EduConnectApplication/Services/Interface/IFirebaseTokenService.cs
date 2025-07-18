using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;

namespace EduConnect.Application.Services.Interface
{
    public interface IFirebaseTokenService
    {
        Task<ApiResponse<string>> UpdateTokenAsync(string userId, UpdateFirebaseTokenRequest request);
        Task<ApiResponse<string>> DeactivateTokenAsync(string userId, string? deviceId = null);
        Task<ApiResponse<string>> DeactivateAllTokensAsync(string userId);
        Task<ApiResponse<FirebaseTokenResponse>> GetOrCreateFirebaseTokenAsync(string userId, GetFirebaseTokenRequest request);
    }
}
