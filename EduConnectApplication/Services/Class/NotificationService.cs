using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using Google.Apis.Auth.OAuth2;

namespace EduConnect.Application.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IFirebaseTokenRepository _firebaseTokenRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly FirebaseOption _firebaseOptions;
        private readonly HttpClient _httpClient;

        public NotificationService(
            IFirebaseTokenRepository firebaseTokenRepository,
            ILogger<NotificationService> logger,
            IOptions<FirebaseOption> firebaseOptions,
            HttpClient httpClient)
        {
            _firebaseTokenRepository = firebaseTokenRepository;
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<string>> SendNotificationToTokenAsync(string firebaseToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(firebaseToken))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Firebase token is required"
                    };
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Title and body are required"
                    };
                }

                // Get OAuth2 access token for FCM API V1
                var accessToken = await GetAccessTokenAsync();
                
                // Ensure data is not null and has proper string values
                var notificationData = data ?? new Dictionary<string, string>();
                if (!notificationData.ContainsKey("type"))
                {
                    notificationData["type"] = "test_notification";
                }
                if (!notificationData.ContainsKey("timestamp"))
                {
                    notificationData["timestamp"] = DateTime.UtcNow.ToString("o");
                }

                // Sanitize strings to ensure they're valid
                var sanitizedTitle = SanitizeNotificationString(title);
                var sanitizedBody = SanitizeNotificationString(body);
                
                var notification = new
                {
                    message = new
                    {
                        token = firebaseToken.Trim(),
                        notification = new
                        {
                            title = sanitizedTitle,
                            body = sanitizedBody
                        },
                        data = notificationData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? ""),
                        android = new
                        {
                            notification = new
                            {
                                icon = "notification_icon",
                                click_action = "FLUTTER_NOTIFICATION_CLICK",
                                channel_id = "default"
                            },
                            priority = "high"
                        },
                        apns = new
                        {
                            headers = new Dictionary<string, string>
                            {
                                ["apns-priority"] = "10"
                            },
                            payload = new
                            {
                                aps = new
                                {
                                    alert = new
                                    {
                                        title = sanitizedTitle,
                                        body = sanitizedBody
                                    },
                                    category = "FLUTTER_NOTIFICATION_CLICK",
                                    sound = "default"
                                }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(notification, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                
                _logger.LogDebug($"Sending FCM payload: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(_firebaseOptions.Url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Firebase notification failed: {response.StatusCode} - {responseContent}");
                    
                    // Parse error response to get more specific error info
                    var errorDetails = ParseFirebaseError(responseContent);
                    
                    // If token is invalid, mark it for cleanup
                    if (IsTokenInvalidError(errorDetails))
                    {
                        _logger.LogWarning($"Invalid token detected: {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...");
                        await HandleInvalidTokenAsync(firebaseToken);
                    }
                    
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Firebase notification failed: {response.StatusCode} - {errorDetails}",
                        Data = responseContent
                    };
                }

                _logger.LogInformation($"Firebase notification sent successfully to token: {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...");
                
                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Notification sent successfully",
                    Data = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to token {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send notification: " + ex.Message
                };
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                GoogleCredential credential;
                
                if (!string.IsNullOrEmpty(_firebaseOptions.ServiceAccountJson))
                {
                    // Use JSON content directly
                    credential = GoogleCredential.FromJson(_firebaseOptions.ServiceAccountJson);
                }
                else if (!string.IsNullOrEmpty(_firebaseOptions.ServiceAccountKeyPath))
                {
                    // Use file path
                    var serviceAccountPath = Path.IsPathRooted(_firebaseOptions.ServiceAccountKeyPath)
                        ? _firebaseOptions.ServiceAccountKeyPath
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _firebaseOptions.ServiceAccountKeyPath);
                        
                    if (!File.Exists(serviceAccountPath))
                    {
                        throw new FileNotFoundException($"Firebase service account file not found at: {serviceAccountPath}");
                    }
                    
                    credential = GoogleCredential.FromFile(serviceAccountPath);
                }
                else
                {
                    throw new InvalidOperationException("Either ServiceAccountJson or ServiceAccountKeyPath must be provided");
                }

                // Create scoped credential for FCM
                var scopedCredential = credential.CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                
                // Get access token
                var accessToken = await scopedCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                
                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Firebase access token");
                throw;
            }
        }

        public async Task<ApiResponse<string>> SendNotificationToAllDevicesAsync(string userId, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var firebaseTokens = await _firebaseTokenRepository.GetActiveTokensByUserIdAsync(userId);
                if (!firebaseTokens.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No active Firebase tokens found for user"
                    };
                }

                var successCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var token in firebaseTokens)
                {
                    var result = await SendNotificationToTokenAsync(token.Token, title, body, data);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                        errors.Add($"Token {token.Token[..10]}...: {result.Message}");
                    }
                }

                var message = $"Sent to {successCount}/{firebaseTokens.Count} devices";
                if (failedCount > 0)
                {
                    message += $". {failedCount} failed.";
                }

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = message,
                    Data = errors.Any() ? string.Join("; ", errors) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notifications to all devices for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send notifications to devices"
                };
            }
        }

        public async Task<ApiResponse<List<string>>> GetActiveTokensAsync(string userId)
        {
            try
            {
                var tokens = await _firebaseTokenRepository.GetActiveTokensByUserIdAsync(userId);
                var tokenList = tokens.Select(t => new
                {
                    Token = $"{t.Token[..10]}...{t.Token[^10..]}",
                    DeviceType = t.DeviceType,
                    DeviceId = t.DeviceId,
                    CreatedAt = t.CreatedAt,
                    ExpiresAt = t.ExpiresAt
                }).ToList();

                return new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {tokens.Count} active tokens",
                    Data = tokenList.Select(t => JsonConvert.SerializeObject(t)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting active tokens for user {userId}");
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Failed to get active tokens"
                };
            }
        }

        private string SanitizeNotificationString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            // Remove control characters and limit length
            var sanitized = new string(input.Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());
            
            // Limit length to prevent payload size issues
            if (sanitized.Length > 1000)
            {
                sanitized = sanitized.Substring(0, 997) + "...";
            }
            
            return sanitized.Trim();
        }

        private string ParseFirebaseError(string errorResponse)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(errorResponse))
                    return "Unknown error";

                dynamic? errorObj = JsonConvert.DeserializeObject(errorResponse);
                if (errorObj?.error?.message != null)
                {
                    return errorObj.error.message.ToString();
                }
                return errorResponse;
            }
            catch
            {
                return errorResponse;
            }
        }

        private bool IsTokenInvalidError(string errorMessage)
        {
            var invalidTokenErrors = new[]
            {
                "registration-token-not-registered",
                "invalid-registration-token",
                "mismatched-credential"
            };

            return invalidTokenErrors.Any(error => errorMessage.Contains(error, StringComparison.OrdinalIgnoreCase));
        }

        private Task HandleInvalidTokenAsync(string firebaseToken)
        {
            try
            {
                // Log the invalid token for manual cleanup
                _logger.LogWarning($"Invalid Firebase token detected: {firebaseToken[..Math.Min(10, firebaseToken.Length)]}... - Manual cleanup may be needed");
                
                // TODO: Implement token cleanup mechanism 
                // For now, just log the issue - admin can clean up via database queries
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling invalid token");
                return Task.CompletedTask;
            }
        }
    }
}
