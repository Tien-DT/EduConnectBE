using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;

        public ChatController(IChatBotService chatBotService)
        {
            _chatBotService = chatBotService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> SendChatBot([FromBody] List<ChatMessageRequest> messages)
        {
            try
            {
                if (messages == null || messages.Count == 0)
                {
                    var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                        new List<string> { "Chat history is empty" },
                        "Invalid request"
                    );
                    return BadRequest(errorResponse);
                }

                var chatResponse = await _chatBotService.GenerateChatReplyAsync(messages);

                var successResponse = ApiResponse<ChatMessageResponse>.SuccessResponse(
                    chatResponse,
                    "Gemini reply generated successfully"
                );

                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Internal server error"
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("chat-with-functions")]
        public async Task<IActionResult> SendChatBotWithFunctions([FromBody] ChatPromptRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Prompt))
                {
                    var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                        new List<string> { "Prompt is required" },
                        "Invalid request"
                    );
                    return BadRequest(errorResponse);
                }

                var chatResponse = await _chatBotService.SendPromptWithFunctionsAsync(request.Prompt);

                return Ok(chatResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Internal server error"
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("chat-with-cached-history")]
        [Authorize]
        public async Task<IActionResult> SendChatBotWithCachedHistory([FromBody] ChatMessageOnlyRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                        new List<string> { "Message is required" },
                        "Invalid request"
                    );
                    return BadRequest(errorResponse);
                }
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                        new List<string> { "User not authenticated" },
                        "Authentication required"
                    );
                    return Unauthorized(errorResponse);
                }
                var chatRequest = new ChatWithHistoryRequest
                {
                    UserId = userId,
                    Message = request.Message
                };

                var chatResponse = await _chatBotService.SendPromptWithCachedHistoryAsync(chatRequest);

                return Ok(chatResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<ChatMessageResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Internal server error"
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("clear-chat-history")]
        [Authorize]
        public async Task<IActionResult> ClearChatHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var chatCacheService = HttpContext.RequestServices.GetRequiredService<IChatCacheService>();
                await chatCacheService.ClearChatHistoryAsync(userId);

                return Ok(new
                {
                    Success = true,
                    Message = "Chat history cleared successfully",
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("test-redis-connection")]
        public async Task<IActionResult> TestRedisConnection()
        {
            try
            {
                var chatCacheService = HttpContext.RequestServices.GetRequiredService<IChatCacheService>();
                
                var testUserId = "test_connection_" + DateTime.Now.Ticks;
                await chatCacheService.AddMessageToChatHistoryAsync(testUserId, "system", "Redis connection test");
                
                var history = await chatCacheService.GetChatHistoryAsync(testUserId);
                
                await chatCacheService.ClearChatHistoryAsync(testUserId);
                
                if (history != null && history.Any())
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Redis connection successful with password authentication",
                        Data = new
                        {
                            Server = "tienlab.myds.me:6363", 
                            TestWriteRead = "PASSED",
                            TestDelete = "PASSED",
                            AuthenticationStatus = "PASSWORD_AUTHENTICATED",
                            Timestamp = DateTime.UtcNow,
                            TestMessages = history.Count
                        }
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Success = false,
                        Message = "Redis read test failed - no data returned"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Redis connection failed - check password and server configuration",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    Server = "tienlab.myds.me:6363"
                });
            }
        }

        [HttpGet("chat-history")]
        [Authorize]
        public async Task<IActionResult> GetChatHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var chatCacheService = HttpContext.RequestServices.GetRequiredService<IChatCacheService>();
                var chatHistory = await chatCacheService.GetChatHistoryAsync(userId);

                return Ok(new
                {
                    Success = true,
                    Message = "Chat history retrieved successfully",
                    Data = new
                    {
                        UserId = userId,
                        MessageCount = chatHistory?.Count ?? 0,
                        Messages = chatHistory?.Select(h => new
                        {
                            Role = h.Role,
                            Message = h.Message,
                            Timestamp = h.Timestamp
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Internal server error",
                    Error = ex.Message
                });
            }
        }

    }
}

