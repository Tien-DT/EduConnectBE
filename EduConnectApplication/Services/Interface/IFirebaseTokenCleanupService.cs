using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IFirebaseTokenCleanupService
    {
        Task<bool> ValidateTokenAsync(string token);
        Task CleanupInvalidTokensAsync();
        Task<int> RemoveExpiredTokensAsync();
    }
}
