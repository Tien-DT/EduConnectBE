namespace EduConnect.Application.Services.Interface
{
    public interface IEmailVerificationTokenService
    {
        Task<string> GenerateAndStoreTokenAsync(string userId, string email);
        Task<string?> ValidateAndConsumeTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
