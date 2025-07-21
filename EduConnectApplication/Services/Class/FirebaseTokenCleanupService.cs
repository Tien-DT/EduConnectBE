using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using System.Text;
using Newtonsoft.Json;
using EduConnect.Infrastruture.Options;
using Microsoft.Extensions.Options;

namespace EduConnect.Application.Services.Class
{
    public class FirebaseTokenCleanupService : IFirebaseTokenCleanupService
    {
        private readonly IFirebaseTokenRepository _firebaseTokenRepository;
        private readonly ILogger<FirebaseTokenCleanupService> _logger;
        private readonly FirebaseOption _firebaseOptions;
        private readonly HttpClient _httpClient;

        public FirebaseTokenCleanupService(
            IFirebaseTokenRepository firebaseTokenRepository,
            ILogger<FirebaseTokenCleanupService> logger,
            IOptions<FirebaseOption> firebaseOptions,
            HttpClient httpClient)
        {
            _firebaseTokenRepository = firebaseTokenRepository;
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
            _httpClient = httpClient;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Token validation failed: Token is null or empty");
                    return false;
                }

                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Failed to get Firebase access token");
                    return false;
                }

                // Tạo payload thử nghiệm đơn giản
                var payload = new
                {
                    message = new
                    {
                        token = token,
                        notification = new
                        {
                            title = "Token Validation Test",
                            body = "Testing token validity"
                        },
                        android = new
                        {
                            priority = "high"
                        },
                        apns = new
                        {
                            payload = new
                            {
                                aps = new
                                {
                                    alert = new
                                    {
                                        title = "Token Validation Test",
                                        body = "Testing token validity"
                                    }
                                }
                            }
                        }
                    },
                    validate_only = true // Quan trọng: chỉ validation, không gửi thực tế
                };

                var json = JsonConvert.SerializeObject(payload, Formatting.None);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(
                    $"https://fcm.googleapis.com/v1/projects/{_firebaseOptions.ProjectId}/messages:send",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Token validation successful for token: {token[..10]}...");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Token validation failed for token {token[..10]}...: {response.StatusCode} - {errorContent}");
                
                // Kiểm tra các lỗi cụ thể
                if (errorContent.Contains("UNREGISTERED") || 
                    errorContent.Contains("INVALID_ARGUMENT") ||
                    errorContent.Contains("NOT_FOUND"))
                {
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during token validation for token: {token?[..10]}...");
                return false;
            }
        }

        public async Task CleanupInvalidTokensAsync()
        {
            try
            {
                _logger.LogInformation("Starting Firebase token cleanup process");

                var allTokens = await _firebaseTokenRepository.GetAllActiveTokensAsync();
                var invalidTokenIds = new List<string>();

                foreach (var tokenEntity in allTokens)
                {
                    var isValid = await ValidateTokenAsync(tokenEntity.Token);
                    if (!isValid)
                    {
                        invalidTokenIds.Add(tokenEntity.Id.ToString());
                        _logger.LogWarning($"Invalid token found for user {tokenEntity.UserId}: {tokenEntity.Token[..10]}...");
                    }
                    
                    // Rate limiting để tránh spam Firebase
                    await Task.Delay(100);
                }

                if (invalidTokenIds.Any())
                {
                    await _firebaseTokenRepository.DeactivateTokensAsync(invalidTokenIds);
                    _logger.LogInformation($"Deactivated {invalidTokenIds.Count} invalid tokens");
                }
                else
                {
                    _logger.LogInformation("No invalid tokens found during cleanup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Firebase token cleanup");
                throw;
            }
        }

        public async Task<int> RemoveExpiredTokensAsync()
        {
            try
            {
                _logger.LogInformation("Starting expired token removal process");

                var removedCount = await _firebaseTokenRepository.RemoveExpiredTokensAsync();
                
                if (removedCount > 0)
                {
                    _logger.LogInformation($"Removed {removedCount} expired tokens");
                }
                else
                {
                    _logger.LogInformation("No expired tokens found to remove");
                }

                return removedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during expired token removal");
                throw;
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                GoogleCredential credential;
                
                if (!string.IsNullOrEmpty(_firebaseOptions.ServiceAccountJson))
                {
                    credential = GoogleCredential.FromJson(_firebaseOptions.ServiceAccountJson);
                }
                else if (!string.IsNullOrEmpty(_firebaseOptions.ServiceAccountKeyPath))
                {
                    var serviceAccountPath = Path.IsPathRooted(_firebaseOptions.ServiceAccountKeyPath)
                        ? _firebaseOptions.ServiceAccountKeyPath
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _firebaseOptions.ServiceAccountKeyPath);
                        
                    credential = GoogleCredential.FromFile(serviceAccountPath);
                }
                else
                {
                    throw new InvalidOperationException("Either ServiceAccountJson or ServiceAccountKeyPath must be provided");
                }

                var scopedCredential = credential.CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                var accessToken = await scopedCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                
                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Firebase access token");
                throw;
            }
        }
    }
}
