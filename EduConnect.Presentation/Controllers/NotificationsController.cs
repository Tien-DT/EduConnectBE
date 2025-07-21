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
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IParentReportNotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Gửi thông báo hàng ngày cho tất cả phụ huynh
        /// </summary>
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

        /// <summary>
        /// Gửi thông báo hàng tuần cho tất cả phụ huynh
        /// </summary>
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

        /// <summary>
        /// Gửi thông báo hàng tháng cho tất cả phụ huynh
        /// </summary>
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

        /// <summary>
        /// Gửi thông báo cho phụ huynh cụ thể theo kỳ
        /// </summary>
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

        /// <summary>
        /// Lấy báo cáo phản ánh học sinh theo kỳ với tìm kiếm, sắp xếp và phân trang
        /// </summary>
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

        /// <summary>
        /// Lấy chi tiết báo cáo phản ánh của một học sinh theo kỳ
        /// </summary>
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

                // Phụ huynh chỉ được xem báo cáo của con mình
                var currentRole = GetCurrentRole();
                if (currentRole == "Parent")
                {
                    var currentUserId = GetCurrentUserId();
                    // TODO: Kiểm tra xem học sinh có thuộc về phụ huynh này không
                    // Có thể cần thêm logic kiểm tra quyền truy cập
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
