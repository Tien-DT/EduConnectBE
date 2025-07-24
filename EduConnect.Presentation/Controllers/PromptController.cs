using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/prompts")]
    [ApiController]
    public class PromptController : ControllerBase
    {
        private readonly IPromptService _promptService;

        public PromptController(IPromptService promptService)
        {
            _promptService = promptService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Lấy danh sách prompts với phân trang, tìm kiếm và sắp xếp
        /// </summary>
        [HttpGet("")]
        [Authorize]
        public async Task<IActionResult> GetPrompts(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool ascending = false,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null)
        {
            var result = await _promptService.GetPromptsAsync(pageIndex, pageSize, searchTerm, sortBy, ascending, type, status);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Lấy prompt theo ID
        /// </summary>
        [HttpGet("{promptId:int}")]
        [Authorize]
        public async Task<IActionResult> GetPromptById(int promptId)
        {
            var result = await _promptService.GetPromptByIdAsync(promptId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Lấy prompts của user hiện tại với phân trang, tìm kiếm và sắp xếp
        /// </summary>
        [HttpGet("my-prompts")]
        [Authorize]
        public async Task<IActionResult> GetMyPrompts(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool ascending = false,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _promptService.GetPromptsByAccountIdAsync(userId, pageIndex, pageSize, searchTerm, sortBy, ascending, type, status);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Lấy prompts theo AccountId với phân trang, tìm kiếm và sắp xếp (cho Admin/Principal)
        /// </summary>
        [HttpGet("account/{accountId}")]
        [Authorize(Roles = "Admin,Principal")]
        public async Task<IActionResult> GetPromptsByAccountId(
            string accountId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool ascending = false,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null)
        {
            var result = await _promptService.GetPromptsByAccountIdAsync(accountId, pageIndex, pageSize, searchTerm, sortBy, ascending, type, status);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Tạo prompt mới
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePrompt([FromBody] CreatePromptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _promptService.CreatePromptAsync(userId, request);
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetPromptById), new { promptId = result.Data?.PromptId }, result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Cập nhật prompt
        /// </summary>
        [HttpPut("{promptId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatePrompt(int promptId, [FromBody] UpdatePromptRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            request.PromptId = promptId;
            var result = await _promptService.UpdatePromptAsync(userId, request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Xóa prompt (soft delete)
        /// </summary>
        [HttpDelete("{promptId:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePrompt(int promptId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _promptService.SoftDeletePromptAsync(promptId, userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Xóa prompt vĩnh viễn (hard delete) - chỉ admin
        /// </summary>
        [HttpDelete("{promptId:int}/permanent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PermanentDeletePrompt(int promptId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _promptService.DeletePromptAsync(promptId, userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
