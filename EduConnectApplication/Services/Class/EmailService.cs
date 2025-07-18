using EduConnect.Application.Models;
using EduConnect.Application.Services.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EduConnect.Application.Services.Class
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, IEmailQueueService emailQueueService, ILogger<EmailService> logger)
        {
            _emailSettings = new EmailSettings
            {
                SmtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                SmtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587"),
                SenderEmail = configuration["EmailSettings:SenderEmail"] ?? "",
                SenderName = configuration["EmailSettings:SenderName"] ?? "EduConnect",
                Username = configuration["EmailSettings:Username"] ?? "",
                Password = configuration["EmailSettings:Password"] ?? "",
                EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "true")
            };
            _emailQueueService = emailQueueService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string email, string verificationToken, string userId)
        {
            var verificationUrl = $"https://educonnect.tienlab.myds.me/api/auth/verify-email?token={verificationToken}&userId={userId}";

            var subject = "Xác thực Email - EduConnect";
            var body = $@"
                <html>
                <body>
                    <h2>Chào mừng bạn đến với hệ thống EduConnect</h2>
                    <p>Cảm ơn bạn đã đăng ký. Vui lòng nhấn vào liên kết bên dưới để xác thực địa chỉ email của bạn:</p>
                    <p><a href='{verificationUrl}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Xác thực Email</a></p>
                    <p>Hoặc sao chép và dán liên kết này vào trình duyệt của bạn:</p>
                    <p>{verificationUrl}</p>
                    <p><strong>Liên kết này sẽ hết hạn sau 5 phút.</strong></p>
                    <p>Nếu bạn không tạo tài khoản, vui lòng bỏ qua email này.</p>
                    <br>
                    <p>Best regards,<br>EduConnect</p>
                </body>
                </html>";

            var emailMessage = new EmailMessage
            {
                To = email,
                Subject = subject,
                Body = body,
                IsHtml = true,
                UserId = userId,
                VerificationToken = verificationToken
            };

            await QueueEmailAsync(emailMessage);
        }

        public async Task QueueEmailAsync(EmailMessage emailMessage)
        {
            await _emailQueueService.EnqueueEmailAsync(emailMessage);
            _logger.LogInformation($"Email queued for {emailMessage.To}");
        }
    }
}
