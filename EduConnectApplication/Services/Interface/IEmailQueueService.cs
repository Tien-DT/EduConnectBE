using EduConnect.Application.Models;

namespace EduConnect.Application.Services.Interface
{
    public interface IEmailQueueService
    {
        Task EnqueueEmailAsync(EmailMessage emailMessage);
        Task<EmailMessage?> DequeueEmailAsync();
        Task ProcessEmailQueueAsync(CancellationToken cancellationToken);
    }
}
