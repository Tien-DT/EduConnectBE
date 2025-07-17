using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Class
{
    public class FirebaseTokenService : IFirebaseTokenService
    {
        private readonly IFirebaseTokenRepository _firebaseTokenRepository;
        private readonly IFirebaseCustomTokenService _firebaseCustomTokenService;
        private readonly ILogger<FirebaseTokenService> _logger;

        public FirebaseTokenService(
            IFirebaseTokenRepository firebaseTokenRepository,
            IFirebaseCustomTokenService firebaseCustomTokenService,
            ILogger<FirebaseTokenService> logger)
        {
            _firebaseTokenRepository = firebaseTokenRepository;
            _firebaseCustomTokenService = firebaseCustomTokenService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> UpdateTokenAsync(string userId, UpdateFirebaseTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Firebase token is required"
                    };
                }

                var success = await _firebaseTokenRepository.UpdateTokenAsync(
                    userId, 
                    request.Token, 
                    request.DeviceType, 
                    request.DeviceId);

                if (!success)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to update Firebase token"
                    };
                }

                _logger.LogInformation($"Firebase token updated successfully for user {userId}");

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Firebase token updated successfully",
                    Data = "Token updated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating Firebase token for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to update Firebase token"
                };
            }
        }

        public async Task<ApiResponse<string>> DeactivateTokenAsync(string userId, string? deviceId = null)
        {
            try
            {
                var token = await _firebaseTokenRepository.GetActiveTokenByUserIdAsync(userId, deviceId);
                if (token == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "No active token found"
                    };
                }

                var success = await _firebaseTokenRepository.DeactivateTokenAsync(token.Id);
                if (!success)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to deactivate Firebase token"
                    };
                }

                _logger.LogInformation($"Firebase token deactivated successfully for user {userId}");

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Firebase token deactivated successfully",
                    Data = "Token deactivated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating Firebase token for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to deactivate Firebase token"
                };
            }
        }

        public async Task<ApiResponse<string>> DeactivateAllTokensAsync(string userId)
        {
            try
            {
                var success = await _firebaseTokenRepository.DeactivateAllUserTokensAsync(userId);
                if (!success)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Failed to deactivate Firebase tokens"
                    };
                }

                _logger.LogInformation($"All Firebase tokens deactivated successfully for user {userId}");

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "All Firebase tokens deactivated successfully",
                    Data = "All tokens deactivated"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating all Firebase tokens for user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to deactivate Firebase tokens"
                };
            }
        }

        public async Task<ApiResponse<FirebaseTokenResponse>> GetOrCreateFirebaseTokenAsync(string userId, GetFirebaseTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.DeviceType))
                {
                    return new ApiResponse<FirebaseTokenResponse>
                    {
                        Success = false,
                        Message = "DeviceType is required"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.FCMToken))
                {
                    return new ApiResponse<FirebaseTokenResponse>
                    {
                        Success = false,
                        Message = "FCM Token is required"
                    };
                }

                var existingToken = await _firebaseTokenRepository.GetActiveTokenByUserIdAsync(userId, request.DeviceId);             
                if (existingToken != null && 
                    (!existingToken.ExpiresAt.HasValue || existingToken.ExpiresAt > DateTime.Now))
                {
                    await _firebaseTokenRepository.UpdateTokenAsync(
                        userId, 
                        request.FCMToken, 
                        request.DeviceType, 
                        request.DeviceId);

                    var existingResponse = new FirebaseTokenResponse
                    {
                        Token = request.FCMToken, 
                        DeviceType = existingToken.DeviceType,
                        DeviceId = existingToken.DeviceId,
                        CreatedAt = existingToken.CreatedAt,
                        ExpiresAt = existingToken.ExpiresAt,
                        IsActive = existingToken.IsActive
                    };

                    return new ApiResponse<FirebaseTokenResponse>
                    {
                        Success = true,
                        Message = "FCM token updated successfully",
                        Data = existingResponse
                    };
                }
                var newToken = await _firebaseTokenRepository.CreateOrUpdateTokenAsync(
                    userId, 
                    request.FCMToken, 
                    request.DeviceType, 
                    request.DeviceId);

                var response = new FirebaseTokenResponse
                {
                    Token = newToken.Token,
                    DeviceType = newToken.DeviceType,
                    DeviceId = newToken.DeviceId,
                    CreatedAt = newToken.CreatedAt,
                    ExpiresAt = newToken.ExpiresAt,
                    IsActive = newToken.IsActive
                };

                _logger.LogInformation($"FCM token registered successfully for user {userId}");

                return new ApiResponse<FirebaseTokenResponse>
                {
                    Success = true,
                    Message = "FCM token registered successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering FCM token for user {userId}");
                return new ApiResponse<FirebaseTokenResponse>
                {
                    Success = false,
                    Message = "Failed to register FCM token"
                };
            }
        }
    }
}
