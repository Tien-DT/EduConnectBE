using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.BackgroundServices
{
    public class MonthlyParentNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonthlyParentNotificationBackgroundService> _logger;

        public MonthlyParentNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MonthlyParentNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Monthly Parent Notification Background Service đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var targetTime = GetNextMonthlyTargetTime(); // Ngày 1 hàng tháng lúc 19:00

                var delay = targetTime - now;
                
                _logger.LogInformation("Thông báo hàng tháng tiếp theo sẽ được gửi lúc: {TargetTime}", targetTime.ToString("yyyy-MM-dd HH:mm:ss"));

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await SendMonthlyNotifications();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Monthly Parent Notification Background Service đã bị hủy");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Monthly Parent Notification Background Service");
                    // Chờ 1 ngày trước khi thử lại
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }

            _logger.LogInformation("Monthly Parent Notification Background Service đã dừng");
        }

        private DateTime GetNextMonthlyTargetTime()
        {
            var now = DateTime.Now;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1).AddHours(19); // 19:00 ngày 1

            // Nếu chưa đến ngày 1 tháng này hoặc đã qua giờ chạy ngày 1 tháng này
            if (now.Day == 1 && now.Hour < 19)
            {
                return firstDayOfMonth;
            }
            else
            {
                // Chuyển sang tháng tiếp theo
                return firstDayOfMonth.AddMonths(1);
            }
        }

        private async Task SendMonthlyNotifications()
        {
            try
            {
                _logger.LogInformation("Bắt đầu gửi thông báo hàng tháng tự động...");

                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IParentReportNotificationService>();

                var result = await notificationService.SendMonthlyNotificationsAsync();

                if (result.Success)
                {
                    _logger.LogInformation("Gửi thông báo hàng tháng tự động thành công: {Message}", result.Message);
                }
                else
                {
                    _logger.LogWarning("Gửi thông báo hàng tháng tự động thất bại: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo hàng tháng tự động");
            }
        }
    }
}
