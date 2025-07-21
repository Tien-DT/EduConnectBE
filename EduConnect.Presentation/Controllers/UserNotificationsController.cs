using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EduConnect.Application.DTOs.Response;

namespace EduConnect.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserNotificationsController : ControllerBase
    {
        private readonly IEnhancedNotificationService _enhancedNotificationService;
        private readonly ILogger<UserNotificationsController> _logger;

        public UserNotificationsController(
            IEnhancedNotificationService enhancedNotificationService,
            ILogger<UserNotificationsController> logger)
        {
            _enhancedNotificationService = enhancedNotificationService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _enhancedNotificationService.GetUserNotificationsAsync(userId, pageIndex, pageSize);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách thông báo"
                });
            }
        }

 
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _enhancedNotificationService.GetUnreadCountAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng thông báo chưa đọc");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy số lượng thông báo chưa đọc"
                });
            }
        }

   
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] List<string> notificationIds)
        {
            try
            {
                if (!notificationIds?.Any() == true)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Danh sách ID thông báo không được để trống"
                    });
                }

                var result = await _enhancedNotificationService.MarkNotificationsAsReadAsync(notificationIds);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu thông báo đã đọc");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đánh dấu thông báo"
                });
            }
        }

        /// <summary>
        /// Gửi lại thông báo chưa được push cho user hiện tại (khi user vừa online)
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SyncNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _enhancedNotificationService.SendStoredNotificationsAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đồng bộ thông báo");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đồng bộ thông báo"
                });
            }
        }

        [HttpPost("process-unpushed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessUnpushedNotifications()
        {
            try
            {
                var result = await _enhancedNotificationService.ProcessUnpushedNotificationsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thông báo chưa gửi");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xử lý thông báo chưa gửi"
                });
            }
        }

        [HttpDelete("cleanup-expired")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupExpiredNotifications()
        {
            try
            {
                var result = await _enhancedNotificationService.CleanupExpiredNotificationsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi dọn dẹp thông báo hết hạn");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi dọn dẹp thông báo hết hạn"
                });
            }
        }

        #region Helper Methods

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        }

        #endregion
    }
}
