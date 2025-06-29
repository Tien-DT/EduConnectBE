using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduConnect.Application.Services.Background
{
    public class EmailQueueBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailQueueBackgroundService> _logger;

        public EmailQueueBackgroundService(IServiceProvider serviceProvider, ILogger<EmailQueueBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Queue Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var emailQueueService = scope.ServiceProvider.GetRequiredService<IEmailQueueService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    
                    var emailMessage = await emailQueueService.DequeueEmailAsync();
                    if (emailMessage != null)
                    {
                        await emailService.SendEmailAsync(emailMessage.To, emailMessage.Subject, emailMessage.Body, emailMessage.IsHtml);
                        _logger.LogInformation($"Email processed and sent to {emailMessage.To}");
                    }
                    else
                    {
                        await Task.Delay(5000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Email Queue Background Service");
                    await Task.Delay(10000, stoppingToken); 
                }
            }

            _logger.LogInformation("Email Queue Background Service stopped");
        }
    }
}
