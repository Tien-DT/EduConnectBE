using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeeklyNotificationController : ControllerBase
    {
        private readonly IWeeklyParentNotificationService _weeklyNotificationService;
        private readonly ILogger<WeeklyNotificationController> _logger;

        public WeeklyNotificationController(
            IWeeklyParentNotificationService weeklyNotificationService,
            ILogger<WeeklyNotificationController> logger)
        {
            _weeklyNotificationService = weeklyNotificationService;
            _logger = logger;
        }

        /// <summary>
        /// Gửi thông báo hàng tuần cho tất cả phụ huynh có con em bị phản ánh
        /// </summary>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        [HttpPost("send-weekly-notifications")]
        [Authorize(Roles = "Admin,Principal")]
        public async Task<IActionResult> SendWeeklyNotifications()
        {
            try
            {
                _logger.LogInformation("API gửi thông báo hàng tuần được gọi");
                var result = await _weeklyNotificationService.SendWeeklyNotificationsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API gửi thông báo hàng tuần");
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra", Error = ex.Message });
            }
        }

        /// <summary>
        /// Gửi thông báo hàng tuần cho một phụ huynh cụ thể
        /// </summary>
        /// <param name="parentId">ID của phụ huynh</param>
        /// <returns>Kết quả thực hiện việc gửi thông báo</returns>
        [HttpPost("send-weekly-notification/{parentId}")]
        [Authorize(Roles = "Admin,Principal,Teacher")]
        public async Task<IActionResult> SendWeeklyNotificationToParent(string parentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentId))
                {
                    return BadRequest(new { Success = false, Message = "Parent ID không được để trống" });
                }

                _logger.LogInformation($"API gửi thông báo hàng tuần cho phụ huynh {parentId} được gọi");
                var result = await _weeklyNotificationService.SendWeeklyNotificationToParentAsync(parentId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi trong API gửi thông báo hàng tuần cho phụ huynh {parentId}");
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra", Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tóm tắt các phản ánh trong tuần của tất cả học sinh
        /// </summary>
        /// <returns>Danh sách tóm tắt các phản ánh</returns>
        [HttpGet("weekly-student-reports")]
        [Authorize(Roles = "Admin,Principal,Teacher")]
        public async Task<IActionResult> GetWeeklyStudentReports()
        {
            try
            {
                _logger.LogInformation("API lấy tóm tắt phản ánh hàng tuần được gọi");
                var result = await _weeklyNotificationService.GetWeeklyStudentReportsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API lấy tóm tắt phản ánh hàng tuần");
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra", Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tóm tắt các phản ánh trong tuần của một học sinh cụ thể
        /// </summary>
        /// <param name="studentId">ID của học sinh</param>
        /// <returns>Tóm tắt các phản ánh của học sinh</returns>
        [HttpGet("weekly-student-report/{studentId}")]
        [Authorize(Roles = "Admin,Principal,Teacher,Parent")]
        public async Task<IActionResult> GetWeeklyStudentReport(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return BadRequest(new { Success = false, Message = "Student ID không được để trống" });
                }

                _logger.LogInformation($"API lấy tóm tắt phản ánh hàng tuần cho học sinh {studentId} được gọi");
                var result = await _weeklyNotificationService.GetWeeklyStudentReportAsync(studentId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi trong API lấy tóm tắt phản ánh hàng tuần cho học sinh {studentId}");
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra", Error = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint test để kiểm tra hoạt động của service
        /// </summary>
        /// <returns>Thông tin về tuần hiện tại</returns>
        [HttpGet("current-week-info")]
        [Authorize(Roles = "Admin,Principal,Teacher")]
        public IActionResult GetCurrentWeekInfo()
        {
            try
            {
                var today = DateTime.Today;
                var dayOfWeek = (int)today.DayOfWeek;
                var startOfWeek = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
                var endOfWeek = startOfWeek.AddDays(6);

                var result = new
                {
                    Success = true,
                    Message = "Thông tin tuần hiện tại",
                    Data = new
                    {
                        Today = today.ToString("yyyy-MM-dd"),
                        DayOfWeek = today.DayOfWeek.ToString(),
                        StartOfWeek = startOfWeek.ToString("yyyy-MM-dd"),
                        EndOfWeek = endOfWeek.ToString("yyyy-MM-dd"),
                        WeekDescription = $"Tuần từ {startOfWeek:dd/MM/yyyy} đến {endOfWeek:dd/MM/yyyy}"
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong API lấy thông tin tuần hiện tại");
                return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra", Error = ex.Message });
            }
        }
    }
}
