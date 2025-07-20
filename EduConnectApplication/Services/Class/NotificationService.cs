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
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace EduConnect.Application.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IFirebaseTokenRepository _firebaseTokenRepository;
        private readonly ILogger<NotificationService> _logger;
        private readonly FirebaseOption _firebaseOptions;
        private FirebaseApp? _firebaseApp;

        public NotificationService(
            IFirebaseTokenRepository firebaseTokenRepository,
            ILogger<NotificationService> logger,
            IOptions<FirebaseOption> firebaseOptions)
        {
            _firebaseTokenRepository = firebaseTokenRepository;
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
            InitializeFirebaseApp();
        }

        private void InitializeFirebaseApp()
        {
            try
            {
                if (_firebaseApp == null && !string.IsNullOrEmpty(_firebaseOptions.ServiceAccountKeyPath))
                {
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var serviceAccountPath = Path.IsPathRooted(_firebaseOptions.ServiceAccountKeyPath) 
                        ? _firebaseOptions.ServiceAccountKeyPath 
                        : Path.Combine(basePath, _firebaseOptions.ServiceAccountKeyPath);

                    if (File.Exists(serviceAccountPath))
                    {
                        _firebaseApp = FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(serviceAccountPath),
                            ProjectId = _firebaseOptions.ProjectId
                        }, "NotificationService");
                        _logger.LogInformation($"Firebase app initialized for NotificationService with service account key from: {serviceAccountPath}");
                    }
                    else
                    {
                        _logger.LogError($"Service account key file not found at: {serviceAccountPath}");
                    }
                }
                else if (_firebaseApp == null && !string.IsNullOrEmpty(_firebaseOptions.ServiceAccountJson))
                {
                    var credential = GoogleCredential.FromJson(_firebaseOptions.ServiceAccountJson);
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential,
                        ProjectId = _firebaseOptions.ProjectId
                    }, "NotificationService");
                    _logger.LogInformation("Firebase app initialized for NotificationService with service account JSON from configuration");
                }
                else
                {
                    _logger.LogWarning("No Firebase service account configuration found for NotificationService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase app for NotificationService");
            }
        }

        public async Task<ApiResponse<string>> SendTestNotificationAsync(string userId, SendTestNotificationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Title and Body are required"
                    };
                }

                if (!string.IsNullOrWhiteSpace(request.SpecificToken))
                {
                    return await SendNotificationToTokenAsync(request.SpecificToken, request.Title, request.Body, request.Data);
                }

                return await SendNotificationToAllDevicesAsync(userId, request.Title, request.Body, request.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending test notification for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send test notification"
                };
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

        public async Task<ApiResponse<string>> SendNotificationToTokenAsync(string firebaseToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                if (_firebaseApp == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Firebase app is not initialized"
                    };
                }

                var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);

                var message = new Message()
                {
                    Token = firebaseToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>
                    {
                        ["type"] = "test_notification",
                        ["timestamp"] = DateTime.UtcNow.ToString("o")
                    },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Icon = "notification_icon",
                            ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                        }
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Alert = new ApsAlert()
                            {
                                Title = title,
                                Body = body
                            }
                        }
                    }
                };

                var response = await messaging.SendAsync(message);
                
                _logger.LogInformation($"Firebase notification sent successfully. Message ID: {response}");
                
                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Notification sent successfully",
                    Data = response
                };
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, $"Firebase messaging error for token {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...: {ex.MessagingErrorCode}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Firebase messaging error: {ex.MessagingErrorCode}",
                    Data = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to token {firebaseToken[..Math.Min(10, firebaseToken.Length)]}...");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to send notification"
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
    }
}
