using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Background
{
    public class DailyComplaintNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyComplaintNotificationService> _logger;
        private readonly TimeSpan _notificationTime = new TimeSpan(17, 0, 0); // 17:00 (5 PM)

        public DailyComplaintNotificationService(
            IServiceProvider serviceProvider,
            ILogger<DailyComplaintNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Complaint Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextNotificationTime(now);
                    var delay = nextRun - now;

                    _logger.LogInformation($"Next complaint notification scheduled for: {nextRun:yyyy-MM-dd HH:mm:ss}");
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await ProcessDailyComplaints();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Daily Complaint Notification Service stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Daily Complaint Notification Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private DateTime GetNextNotificationTime(DateTime now)
        {
            var today = now.Date;
            var todayNotificationTime = today.Add(_notificationTime);
            if (now >= todayNotificationTime)
            {
                return today.AddDays(1).Add(_notificationTime);
            }

            return todayNotificationTime;
        }

        private async Task ProcessDailyComplaints()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<IParentNotificationService>();
                var complaintCacheService = scope.ServiceProvider.GetRequiredService<IStudentComplaintCacheService>();

                var today = DateTime.Today;
                
                _logger.LogInformation($"Starting daily complaint notification process for {today:yyyy-MM-dd}");
                var hasComplaints = await complaintCacheService.HasComplaintsForDateAsync(today);
                if (!hasComplaints)
                {
                    _logger.LogInformation($"No complaints found for {today:yyyy-MM-dd}");
                    return;
                }

                await notificationService.ProcessDailyComplaintsAsync(today);

                _logger.LogInformation($"Daily complaint notification process completed for {today:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing daily complaints");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Complaint Notification Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
