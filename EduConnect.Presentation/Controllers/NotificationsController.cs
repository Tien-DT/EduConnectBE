using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IParentReportNotificationService _notificationService;
        private readonly IEnhancedNotificationService _enhancedNotificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IParentReportNotificationService notificationService,
            IEnhancedNotificationService enhancedNotificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
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
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo cho user {UserId}", GetCurrentUserId());
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
                if (notificationIds == null || !notificationIds.Any())
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
                    Message = "Có lỗi xảy ra khi đánh dấu thông báo đã đọc"
                });
            }
        }

        [HttpPost("send/daily")]
        [Authorize(Roles = "Admin,Principal")]
        public async Task<IActionResult> SendDailyNotifications()
        {
            try
            {
                _logger.LogInformation("API gửi thông báo hàng ngày được gọi bởi: {UserId}", GetCurrentUserId());
                var result = await _notificationService.SendDailyNotificationsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API gửi thông báo hàng ngày");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo hàng ngày"
                });
            }
        }

        [HttpPost("send/weekly")]
        [Authorize(Roles = "Admin,Principal")]
        public async Task<IActionResult> SendWeeklyNotifications()
        {
            try
            {
                _logger.LogInformation("API gửi thông báo hàng tuần được gọi bởi: {UserId}", GetCurrentUserId());
                var result = await _notificationService.SendWeeklyNotificationsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API gửi thông báo hàng tuần");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo hàng tuần"
                });
            }
        }

        [HttpPost("send/monthly")]
        [Authorize(Roles = "Admin,Principal")]
        public async Task<IActionResult> SendMonthlyNotifications()
        {
            try
            {
                _logger.LogInformation("API gửi thông báo hàng tháng được gọi bởi: {UserId}", GetCurrentUserId());
                var result = await _notificationService.SendMonthlyNotificationsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API gửi thông báo hàng tháng");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo hàng tháng"
                });
            }
        }

        [HttpPost("send/parent/{parentId}")]
        [Authorize(Roles = "Admin,Principal,Teacher")]
        public async Task<IActionResult> SendNotificationToParent(
            string parentId,
            [FromQuery] string period = "weekly")
        {
            try
            {
                if (!IsValidPeriod(period))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Kỳ báo cáo không hợp lệ. Chỉ chấp nhận: daily, weekly, monthly"
                    });
                }

                _logger.LogInformation("API gửi thông báo {Period} cho phụ huynh {ParentId} được gọi bởi: {UserId}", 
                    period, parentId, GetCurrentUserId());
                
                var result = await _notificationService.SendNotificationToParentAsync(parentId, period);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API gửi thông báo cho phụ huynh {ParentId}", parentId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo"
                });
            }
        }

        [HttpGet("reports")]
        [Authorize(Roles = "Admin,Principal,Teacher")]
        public async Task<IActionResult> GetStudentReports([FromQuery] GetStudentReportsRequest request)
        {
            try
            {
                if (!IsValidPeriod(request.Period))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Kỳ báo cáo không hợp lệ. Chỉ chấp nhận: daily, weekly, monthly"
                    });
                }

                _logger.LogInformation("API lấy báo cáo {Period} được gọi bởi: {UserId}", request.Period, GetCurrentUserId());
                
                var result = await _notificationService.GetStudentReportsAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API lấy báo cáo phản ánh {Period}", request.Period);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy báo cáo phản ánh"
                });
            }
        }

        [HttpGet("reports/students/{studentId}")]
        [Authorize(Roles = "Admin,Principal,Teacher,Parent")]
        public async Task<IActionResult> GetStudentReportDetails(
            string studentId, 
            [FromQuery] GetStudentReportDetailsRequest request)
        {
            try
            {
                if (!IsValidPeriod(request.Period))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Kỳ báo cáo không hợp lệ. Chỉ chấp nhận: daily, weekly, monthly"
                    });
                }

     
                var currentRole = GetCurrentRole();
                if (currentRole == "Parent")
                {
                    var currentUserId = GetCurrentUserId();

                }

                _logger.LogInformation("API lấy chi tiết báo cáo {Period} học sinh {StudentId} được gọi bởi: {UserId}", 
                    request.Period, studentId, GetCurrentUserId());
                
                var result = await _notificationService.GetStudentReportDetailsAsync(studentId, request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API lấy chi tiết báo cáo học sinh {StudentId}", studentId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy chi tiết báo cáo học sinh"
                });
            }
        }

        #region Helper Methods

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        }

        private string GetCurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
        }

        private static bool IsValidPeriod(string period)
        {
            var validPeriods = new[] { "daily", "weekly", "monthly" };
            return validPeriods.Contains(period.ToLower());
        }

        #endregion
    }
}
