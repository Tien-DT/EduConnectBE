using EduConnect.Application.DTOs;

namespace EduConnect.Application.Services.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendVerificationEmailAsync(string email, string verificationToken, string userId);
        Task QueueEmailAsync(EmailMessage emailMessage);
    }
}
