using EduConnect.Application.Models;
using EduConnect.Application.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace EduConnect.Application.Services.Queue
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<EmailQueueService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string EMAIL_QUEUE_KEY = "email_queue";

        public EmailQueueService(IDistributedCache distributedCache, ILogger<EmailQueueService> logger, IServiceProvider serviceProvider)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            
            _logger.LogInformation("EmailQueueService initialized successfully with IDistributedCache");
        }

        public async Task EnqueueEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                var jsonMessage = JsonConvert.SerializeObject(emailMessage);
                var queueKey = $"{EMAIL_QUEUE_KEY}_{Guid.NewGuid()}";
                
                await _distributedCache.SetStringAsync(queueKey, jsonMessage, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
                await AddToQueueIndex(queueKey);
                
                _logger.LogInformation($"Email message queued for {emailMessage.To} with key {queueKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue email message");
                throw;
            }
        }

        public async Task<EmailMessage?> DequeueEmailAsync()
        {
            try
            {
                var queueKey = await GetNextQueueKey();
                if (string.IsNullOrEmpty(queueKey))
                    return null;

                var jsonMessage = await _distributedCache.GetStringAsync(queueKey);
                if (string.IsNullOrEmpty(jsonMessage))
                    return null;

                // Remove from cache after reading
                await _distributedCache.RemoveAsync(queueKey);
                await RemoveFromQueueIndex(queueKey);

                var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(jsonMessage);
                return emailMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dequeue email message");
                return null;
            }
        }

        private async Task AddToQueueIndex(string queueKey)
        {
            try
            {
                var indexKey = $"{EMAIL_QUEUE_KEY}_index";
                var existingKeys = await _distributedCache.GetStringAsync(indexKey);
                var keysList = string.IsNullOrEmpty(existingKeys) 
                    ? new List<string>() 
                    : JsonConvert.DeserializeObject<List<string>>(existingKeys) ?? new List<string>();
                
                keysList.Add(queueKey);
                
                var updatedKeys = JsonConvert.SerializeObject(keysList);
                await _distributedCache.SetStringAsync(indexKey, updatedKeys, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add to queue index");
            }
        }

        private async Task<string?> GetNextQueueKey()
        {
            try
            {
                var indexKey = $"{EMAIL_QUEUE_KEY}_index";
                var existingKeys = await _distributedCache.GetStringAsync(indexKey);
                if (string.IsNullOrEmpty(existingKeys))
                    return null;

                var keysList = JsonConvert.DeserializeObject<List<string>>(existingKeys);
                if (keysList == null || !keysList.Any())
                    return null;

                return keysList.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get next queue key");
                return null;
            }
        }

        private async Task RemoveFromQueueIndex(string queueKey)
        {
            try
            {
                var indexKey = $"{EMAIL_QUEUE_KEY}_index";
                var existingKeys = await _distributedCache.GetStringAsync(indexKey);
                if (string.IsNullOrEmpty(existingKeys))
                    return;

                var keysList = JsonConvert.DeserializeObject<List<string>>(existingKeys);
                if (keysList == null)
                    return;

                keysList.Remove(queueKey);
                
                var updatedKeys = JsonConvert.SerializeObject(keysList);
                await _distributedCache.SetStringAsync(indexKey, updatedKeys, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove from queue index");
            }
        }

        public async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email queue processing started");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var emailMessage = await DequeueEmailAsync();
                    if (emailMessage != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        
                        await emailService.SendEmailAsync(emailMessage.To, emailMessage.Subject, emailMessage.Body, emailMessage.IsHtml);
                        _logger.LogInformation($"Email processed and sent to {emailMessage.To}");
                    }
                    else
                    {
                        await Task.Delay(5000, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email queue");
                    await Task.Delay(10000, cancellationToken);
                }
            }

            _logger.LogInformation("Email queue processing stopped");
        }
    }
}
