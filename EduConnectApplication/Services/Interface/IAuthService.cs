using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest login);
        Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest register);
        Task<ApiResponse<GetNewTokenResponse>> GetNewTokenAsync(GetNewTokenRequest request);
        Task<ApiResponse<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest request);
        Task<ApiResponse<LoginResponse>> GoogleTokenLoginAsync(GoogleTokenLoginRequest request);
        Task<ApiResponse<UserRoleIdResponse>> GetUserRoleIdAsync(string userId);
        Task<ApiResponse<string>> VerifyEmailAsync(EmailVerificationRequest request);
    }
}