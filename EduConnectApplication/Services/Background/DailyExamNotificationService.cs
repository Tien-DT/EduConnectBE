using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Background
{
    public class DailyExamNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyExamNotificationService> _logger;

        public DailyExamNotificationService(
            IServiceProvider serviceProvider,
            ILogger<DailyExamNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyExamNotificationService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Tính toán thời gian cho lần chạy tiếp theo (8:00 sáng mỗi ngày)
                    var now = DateTime.Now;
                    var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                    if (now >= nextRunTime)
                    {
                        nextRunTime = nextRunTime.AddDays(1);
                    }

                    var delay = nextRunTime - now;
                    _logger.LogInformation($"DailyExamNotificationService will run next at: {nextRunTime:yyyy-MM-dd HH:mm:ss} (in {delay.TotalHours:F1} hours)");

                    // Đợi đến giờ đã định
                    await Task.Delay(delay, stoppingToken);

                    // Thực hiện gửi thông báo
                    await SendExamNotificationsAsync();

                    // Đợi 1 phút trước khi kiểm tra lại thời gian
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Dịch vụ đã bị hủy, thoát khỏi vòng lặp
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in DailyExamNotificationService.");

                    // Đợi 15 phút trước khi thử lại nếu có lỗi
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
            }

            _logger.LogInformation("DailyExamNotificationService is stopping.");
        }

        private async Task SendExamNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to send daily exam notifications");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var result = await notificationService.SendScheduledExamNotificationsAsync();

                    if (result.Success)
                    {
                        _logger.LogInformation($"Successfully sent daily exam notifications: {result.Message}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send some daily exam notifications: {result.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending daily exam notifications");
            }
        }
    }
} 