using EduConnect.Application.DTOs;

namespace EduConnect.Application.Services.Interface
{
    public interface IEmailQueueService
    {
        Task EnqueueEmailAsync(EmailMessage emailMessage);
        Task<EmailMessage?> DequeueEmailAsync();
        Task ProcessEmailQueueAsync(CancellationToken cancellationToken);
    }
}
