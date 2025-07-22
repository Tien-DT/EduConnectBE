using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.BackgroundServices
{
    public class DailyParentNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyParentNotificationBackgroundService> _logger;

        public DailyParentNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DailyParentNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Parent Notification Background Service đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var targetTime = DateTime.Today.AddHours(18).AddMinutes(0); // 18:00 hàng ngày

                // Nếu đã qua giờ chạy hôm nay, chờ đến ngày mai
                if (now > targetTime)
                {
                    targetTime = targetTime.AddDays(1);
                }

                var delay = targetTime - now;
                
                _logger.LogInformation("Thông báo hàng ngày tiếp theo sẽ được gửi lúc: {TargetTime}", targetTime.ToString("yyyy-MM-dd HH:mm:ss"));

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await SendDailyNotifications();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Daily Parent Notification Background Service đã bị hủy");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Daily Parent Notification Background Service");
                    // Chờ 1 giờ trước khi thử lại
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Daily Parent Notification Background Service đã dừng");
        }

        private async Task SendDailyNotifications()
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi thông báo hàng ngày tự động...");

                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IParentReportNotificationService>();

                var result = await notificationService.SendDailyNotificationsAsync();

                if (result.Success)
                {
                    _logger.LogInformation("Gửi thông báo hàng ngày tự động thành công: {Message}", result.Message);
                }
                else
                {
                    _logger.LogWarning("Gửi thông báo hàng ngày tự động thất bại: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo hàng ngày tự động");
            }
        }
    }
}
