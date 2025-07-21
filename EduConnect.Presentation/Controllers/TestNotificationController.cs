using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestNotificationController : ControllerBase
    {
        private readonly IEnhancedNotificationService _enhancedNotificationService;
        private readonly ILogger<TestNotificationController> _logger;

        public TestNotificationController(
            IEnhancedNotificationService enhancedNotificationService,
            ILogger<TestNotificationController> logger)
        {
            _enhancedNotificationService = enhancedNotificationService;
            _logger = logger;
        }

        [HttpPost("test-create")]
        public async Task<IActionResult> TestCreateNotification()
        {
            try
            {
                _logger.LogInformation("🧪 Bắt đầu test tạo notification...");

                var testData = new Dictionary<string, string>
                {
                    { "test", "true" },
                    { "created_by", "test_api" }
                };

                var result = await _enhancedNotificationService.CreateAndSendNotificationAsync(
                    "test-user-id",
                    "🧪 Test Notification",
                    "Đây là notification test để kiểm tra việc lưu vào database",
                    "Test notification summary",
                    "test",
                    testData
                );

                if (result.Success)
                {
                    _logger.LogInformation($"✅ Test thành công: {result.Message}");
                    return Ok(new
                    {
                        Success = true,
                        Message = result.Message,
                        NotificationId = result.Data
                    });
                }
                else
                {
                    _logger.LogError($"❌ Test thất bại: {result.Message}");
                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi test notification");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi test",
                    Error = ex.Message
                });
            }
        }
    }
}
