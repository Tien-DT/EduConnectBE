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
        private readonly INotificationService _firebaseNotificationService;
        private readonly IEnhancedNotificationService _enhancedNotificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IParentReportNotificationService notificationService,
            INotificationService firebaseNotificationService,
            IEnhancedNotificationService enhancedNotificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _firebaseNotificationService = firebaseNotificationService;
            _enhancedNotificationService = enhancedNotificationService;
            _logger = logger;
        }

        /// <summary>
        /// Gửi thông báo hàng ngày cho tất cả phụ huynh
        /// </summary>
        [HttpPost("send/daily")]
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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

        /// <summary>
        /// Lấy danh sách thông báo của người dùng hiện tại dựa trên AccountId trong JWT
        /// </summary>
        [HttpGet("notifications")]
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
                _logger.LogError(ex, "Lỗi khi lấy danh sách thông báo của user");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách thông báo"
                });
            }
        }

        /// <summary>
        /// Lấy số lượng thông báo chưa đọc của người dùng hiện tại
        /// </summary>
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

        /// <summary>
        /// Đánh dấu thông báo là đã đọc
        /// </summary>
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
                    Message = "Có lỗi xảy ra khi đánh dấu thông báo"
                });
            }
        }

        /// <summary>
        /// Đồng bộ thông báo khi user vừa online (gửi thông báo chưa được push)
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

        /// <summary>
        /// Gửi test notification đến token cụ thể
        /// </summary>
        [HttpPost("test-token")]
        public async Task<IActionResult> SendTestNotificationToToken([FromBody] SendNotificationToTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Dữ liệu request không hợp lệ",
                        Error = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _firebaseNotificationService.SendNotificationToTokenAsync(
                    request.FirebaseToken, 
                    request.Title, 
                    request.Body, 
                    request.Data);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong SendTestNotificationToToken endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi máy chủ nội bộ"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách Firebase tokens đang active của user hiện tại
        /// </summary>
        [HttpGet("tokens")]
        public async Task<IActionResult> GetActiveTokens()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _firebaseNotificationService.GetActiveTokensAsync(userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong GetActiveTokens endpoint");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi máy chủ nội bộ"
                });
            }
        }

        /// <summary>
        /// Xử lý thông báo chưa được gửi - dành cho Admin
        /// </summary>
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

        /// <summary>
        /// Dọn dẹp thông báo hết hạn - dành cho Admin
        /// </summary>
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
