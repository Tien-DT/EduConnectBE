using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Background
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(10); 

        public NotificationBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var enhancedNotificationService = scope.ServiceProvider
                            .GetRequiredService<IEnhancedNotificationService>();

                        var result = await enhancedNotificationService.ProcessUnpushedNotificationsAsync();
                        
                        if (result.Success)
                        {
                            _logger.LogInformation("Successfully processed unpushed notifications: {Message}", result.Message);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to process unpushed notifications: {Message}", result.Message);
                        }

                        if (DateTime.Now.Minute == 0) 
                        {
                            var cleanupResult = await enhancedNotificationService.CleanupExpiredNotificationsAsync();
                            if (cleanupResult.Success)
                            {
                                _logger.LogInformation("Successfully cleaned up expired notifications: {Message}", cleanupResult.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Notification Background Service");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Notification Background Service stopped.");
        }
    }
}
