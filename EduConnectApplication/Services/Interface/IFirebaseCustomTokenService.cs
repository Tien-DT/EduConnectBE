using FirebaseAdmin.Auth;

namespace EduConnect.Application.Services.Interface
{
    public interface IFirebaseCustomTokenService
    {
        Task<string> GenerateCustomTokenAsync(string userId, Dictionary<string, object>? claims = null);
        Task<bool> VerifyTokenAsync(string token);
    }
}
