using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("SendChatBot")]
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


    }
}

