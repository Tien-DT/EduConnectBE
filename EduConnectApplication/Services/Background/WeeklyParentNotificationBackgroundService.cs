using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Background
{
    public class WeeklyParentNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeeklyParentNotificationBackgroundService> _logger;
        
        // Chạy vào Chủ nhật hàng tuần lúc 19:00 (7 PM)
        private readonly DayOfWeek _targetDay = DayOfWeek.Sunday;
        private readonly TimeSpan _targetTime = new TimeSpan(19, 0, 0);

        public WeeklyParentNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<WeeklyParentNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Parent Notification Background Service started");
            _logger.LogInformation($"Scheduled to run every {_targetDay} at {_targetTime:hh\\:mm}");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextRunTime(now);
                    var delay = nextRun - now;

                    _logger.LogInformation($"Next weekly notification scheduled for: {nextRun:yyyy-MM-dd HH:mm:ss} ({delay.TotalHours:F1} hours from now)");
                    
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await ProcessWeeklyNotifications();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Weekly Parent Notification Background Service stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Weekly Parent Notification Background Service");
                    // Chờ 1 tiếng trước khi thử lại nếu có lỗi
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private DateTime GetNextRunTime(DateTime currentTime)
        {
            var today = currentTime.Date;
            var daysUntilTarget = ((int)_targetDay - (int)currentTime.DayOfWeek + 7) % 7;
            
            // Nếu hôm nay là ngày target và chưa qua giờ chạy
            if (daysUntilTarget == 0 && currentTime.TimeOfDay < _targetTime)
            {
                return today.Add(_targetTime);
            }
            
            // Nếu hôm nay là ngày target nhưng đã qua giờ chạy, hoặc không phải ngày target
            if (daysUntilTarget == 0)
            {
                daysUntilTarget = 7; // Chờ đến tuần sau
            }
            
            return today.AddDays(daysUntilTarget).Add(_targetTime);
        }

        private async Task ProcessWeeklyNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            
            try
            {
                _logger.LogInformation("Starting weekly parent notification process...");
                
                var weeklyNotificationService = scope.ServiceProvider
                    .GetRequiredService<IWeeklyParentNotificationService>();
                
                var result = await weeklyNotificationService.SendWeeklyNotificationsAsync();
                
                if (result.Success)
                {
                    _logger.LogInformation($"Weekly notifications completed successfully: {result.Message}");
                    _logger.LogInformation($"Result data: {result.Data}");
                }
                else
                {
                    _logger.LogWarning($"Weekly notifications completed with issues: {result.Message}");
                    _logger.LogWarning($"Error data: {result.Data}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process weekly parent notifications");
                
                // Ghi log chi tiết để debug
                _logger.LogError($"Exception Type: {ex.GetType().Name}");
                _logger.LogError($"Exception Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation("Weekly parent notification process completed");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Parent Notification Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Weekly Parent Notification Background Service disposed");
            base.Dispose();
        }
    }
}
