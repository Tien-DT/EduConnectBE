using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;

namespace EduConnect.Application.Services.Cache
{
    public class EmailVerificationTokenService : IEmailVerificationTokenService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<EmailVerificationTokenService> _logger;
        private const string TOKEN_PREFIX = "email_verification_token:";
        private const int TOKEN_EXPIRY_MINUTES = 5;

        public EmailVerificationTokenService(IDistributedCache distributedCache, ILogger<EmailVerificationTokenService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInformation("EmailVerificationTokenService initialized successfully");
        }

        public async Task<string> GenerateAndStoreTokenAsync(string userId, string email)
        {
            try
            {
                _logger.LogInformation($"Starting token generation for user: {userId}, email: {email}");
                
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogError("UserId is null or empty");
                    throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
                }
                
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogError("Email is null or empty");
                    throw new ArgumentException("Email cannot be null or empty", nameof(email));
                }
                
                var token = GenerateSecureToken();
                _logger.LogInformation($"Generated token with length: {token?.Length ?? 0}");
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Generated token is null or empty");
                    throw new InvalidOperationException("Failed to generate secure token");
                }
                
                var tokenData = $"{userId}|{email}|{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                _logger.LogInformation($"Created token data: {tokenData}");
                
                var key = TOKEN_PREFIX + token;
                _logger.LogInformation($"Storing token with key: {key}");
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(TOKEN_EXPIRY_MINUTES)
                };
                
                await _distributedCache.SetStringAsync(key, tokenData, options);
                _logger.LogInformation($"Token stored successfully in cache");
                
                _logger.LogInformation($"Email verification token generated successfully for user {userId}, expires in {TOKEN_EXPIRY_MINUTES} minutes");
                
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate email verification token for user {userId}");
                throw;
            }
        }

        public async Task<string?> ValidateAndConsumeTokenAsync(string token)
        {
            try
            {
                var key = TOKEN_PREFIX + token;
                
                var tokenDataString = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(tokenDataString))
                {
                    _logger.LogWarning($"Email verification token not found or expired: {token}");
                    return null;
                }

                var parts = tokenDataString.Split('|');
                if (parts.Length != 3)
                {
                    _logger.LogWarning($"Invalid email verification token format: {token}");
                    return null;
                }

                var userId = parts[0];
                var email = parts[1];
                var createdAt = parts[2];

                await _distributedCache.RemoveAsync(key);
                
                _logger.LogInformation($"Email verification token consumed for user {userId}");
                
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate email verification token: {token}");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var key = TOKEN_PREFIX + token;
                
                var tokenExists = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(tokenExists))
                {
                    _logger.LogWarning($"Token not found for revocation: {token}");
                    return false;
                }
                
                await _distributedCache.RemoveAsync(key);
                _logger.LogInformation($"Email verification token revoked: {token}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to revoke email verification token: {token}");
                return false;
            }
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
