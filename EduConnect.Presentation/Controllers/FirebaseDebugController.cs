using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EduConnect.Presentation.Controllers.v1
{
    [ApiController]
    [Route("api/v1/firebase-debug")]
    public class FirebaseDebugController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IFirebaseTokenCleanupService _cleanupService;
        private readonly ILogger<FirebaseDebugController> _logger;

        public FirebaseDebugController(
            INotificationService notificationService,
            IFirebaseTokenCleanupService cleanupService,
            ILogger<FirebaseDebugController> logger)
        {
            _notificationService = notificationService;
            _cleanupService = cleanupService;
            _logger = logger;
        }

        [HttpPost("validate-token")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token is required"
                    });
                }

                var isValid = await _cleanupService.ValidateTokenAsync(request.Token);
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = isValid ? "Token is valid" : "Token is invalid",
                    Data = isValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Firebase token");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("cleanup-invalid-tokens")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> CleanupInvalidTokens()
        {
            try
            {
                _logger.LogInformation("Starting manual Firebase token cleanup...");
                
                await _cleanupService.CleanupInvalidTokensAsync();
                var expiredCount = await _cleanupService.RemoveExpiredTokensAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = $"Token cleanup completed. Expired tokens removed: {expiredCount}",
                    Data = "Cleanup completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Firebase token cleanup");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("test-notification-detailed")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> SendDetailedTestNotification([FromBody] SendTestNotificationRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                _logger.LogInformation($"Testing notification for user: {userId}");

                // Get user's active tokens first
                var tokensResponse = await _notificationService.GetActiveTokensAsync(userId);
                if (!tokensResponse.Success || tokensResponse.Data?.Any() != true)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No active Firebase tokens found for user",
                        Data = new 
                        {
                            UserId = userId,
                            TokenCount = 0,
                            Tokens = new List<string>()
                        }
                    });
                }

                // Try sending to all tokens with detailed results
                var testResults = new List<object>();
                
                foreach (var tokenData in tokensResponse.Data)
                {
                    try
                    {
                        // Parse token info (it's JSON string)
                        var tokenInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(tokenData);
                        var token = tokenInfo?.Token?.ToString();
                        
                        if (string.IsNullOrEmpty(token))
                        {
                            testResults.Add(new
                            {
                                Token = "Unknown",
                                Success = false,
                                Error = "Could not extract token from data",
                                Data = tokenData
                            });
                            continue;
                        }

                        // Reconstruct full token (remove masking if needed)
                        var cleanToken = token?.Replace("...", "") ?? "";
                        
                        if (string.IsNullOrEmpty(cleanToken))
                        {
                            testResults.Add(new
                            {
                                Token = token ?? "Unknown",
                                Success = false,
                                Error = "Invalid token format",
                                Data = tokenData
                            });
                            continue;
                        }
                        
                        // Send test notification
                        var result = await _notificationService.SendNotificationToTokenAsync(
                            cleanToken, 
                            request.Title ?? "Debug Test", 
                            request.Body ?? "This is a detailed debug test notification",
                            request.Data ?? new Dictionary<string, string> { ["debug"] = "true" });

                        testResults.Add(new
                        {
                            Token = token,
                            Success = result.Success,
                            Message = result.Message,
                            Error = result.Success ? null : result.Data,
                            DeviceInfo = tokenInfo
                        });
                    }
                    catch (Exception tokenEx)
                    {
                        testResults.Add(new
                        {
                            Token = "Parse Error",
                            Success = false,
                            Error = tokenEx.Message,
                            Data = tokenData
                        });
                    }
                }

                var successCount = testResults.Count(r => ((dynamic)r).Success);
                var failCount = testResults.Count - successCount;

                return Ok(new ApiResponse<object>
                {
                    Success = successCount > 0,
                    Message = $"Debug test completed: {successCount} success, {failCount} failed",
                    Data = new
                    {
                        UserId = userId,
                        TotalTokens = testResults.Count,
                        SuccessCount = successCount,
                        FailCount = failCount,
                        Results = testResults
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in detailed test notification");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = ex.Message
                });
            }
        }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
