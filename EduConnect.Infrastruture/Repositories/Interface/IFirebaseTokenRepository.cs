using EduConnect.Domain.Entities;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IFirebaseTokenRepository : IBaseRepository<FirebaseToken, int>
    {
        Task<FirebaseToken?> GetActiveTokenByUserIdAsync(string userId, string? deviceId = null);
        Task<List<FirebaseToken>> GetActiveTokensByUserIdAsync(string userId);
        Task<bool> DeactivateTokenAsync(int tokenId);
        Task<bool> DeactivateAllUserTokensAsync(string userId);
        Task<bool> UpdateTokenAsync(string userId, string newToken, string? deviceType = null, string? deviceId = null);
        Task<FirebaseToken> CreateOrUpdateTokenAsync(string userId, string token, string? deviceType = null, string? deviceId = null);
        Task<bool> CleanupExpiredTokensAsync();
        Task<List<FirebaseToken>> GetAllActiveTokensAsync();
        Task<int> RemoveExpiredTokensAsync();
        Task<bool> DeactivateTokensAsync(List<string> tokenIds);
    }
}
