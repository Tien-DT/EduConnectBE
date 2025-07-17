using EduConnect.Application.Models;
using EduConnect.Application.Services.Interface;
using EduConnect.Infrastruture.Options;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace EduConnect.Application.Services.Class
{
    public class ParentNotificationService : IParentNotificationService
    {
        private readonly IChatBotService _chatBotService;
        private readonly IStudentComplaintCacheService _complaintCacheService;
        private readonly IParentRepository _parentRepository;
        private readonly IFirebaseTokenRepository _firebaseTokenRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ParentNotificationService> _logger;
        private readonly FirebaseOption _firebaseOptions;

        public ParentNotificationService(
            IChatBotService chatBotService,
            IStudentComplaintCacheService complaintCacheService,
            IParentRepository parentRepository,
            IFirebaseTokenRepository firebaseTokenRepository,
            HttpClient httpClient,
            ILogger<ParentNotificationService> logger,
            IOptions<FirebaseOption> firebaseOptions)
        {
            _chatBotService = chatBotService;
            _complaintCacheService = complaintCacheService;
            _parentRepository = parentRepository;
            _firebaseTokenRepository = firebaseTokenRepository;
            _httpClient = httpClient;
            _logger = logger;
            _firebaseOptions = firebaseOptions.Value;
        }

        public async Task SendDailyComplaintNotificationAsync(StudentComplaintData studentComplaint)
        {
            try
            {
                var parent = await _parentRepository.GetParentByIdAsync(studentComplaint.ParentId);
                if (parent == null)
                {
                    _logger.LogWarning($"Parent {studentComplaint.ParentId} not found");
                    return;
                }

                var firebaseTokens = await _firebaseTokenRepository.GetActiveTokensByUserIdAsync(parent.AccountID);
                if (!firebaseTokens.Any())
                {
                    _logger.LogWarning($"No active Firebase tokens found for parent {studentComplaint.ParentId}");
                    return;
                }

                var aiMessage = await GenerateComplaintMessageAsync(studentComplaint);
                
                var notificationTasks = firebaseTokens.Select(token => 
                    SendFirebaseNotificationAsync(token.Token, aiMessage, studentComplaint));
                await Task.WhenAll(notificationTasks);

                _logger.LogInformation($"Daily complaint notification sent to parent {studentComplaint.ParentId} for student {studentComplaint.StudentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending daily complaint notification for student {studentComplaint.StudentId}");
                throw;
            }
        }

        public async Task ProcessDailyComplaintsAsync(DateTime date)
        {
            try
            {
                var dailyComplaints = await _complaintCacheService.GetDailyComplaintsAsync(date);
                
                if (!dailyComplaints.Any())
                {
                    _logger.LogInformation($"No complaints found for date {date:yyyy-MM-dd}");
                    return;
                }

                _logger.LogInformation($"Processing {dailyComplaints.Count} student complaints for date {date:yyyy-MM-dd}");

                var tasks = dailyComplaints.Select(complaint => SendDailyComplaintNotificationAsync(complaint));
                await Task.WhenAll(tasks);

                await _complaintCacheService.RemoveDailyComplaintsAsync(date);

                _logger.LogInformation($"Successfully processed all complaints for date {date:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing daily complaints for date {date:yyyy-MM-dd}");
                throw;
            }
        }

        private async Task<string> GenerateComplaintMessageAsync(StudentComplaintData studentComplaint)
        {
            var complaintsText = string.Join(", ", studentComplaint.Complaints.Select(c => 
                $"{c.SubjectName} (Tiết {c.Period}): {c.NoteContent}"));

            var prompt = $@"
Bạn là một hệ thống thông báo nhà trường. Hãy tạo một tin nhắn lịch sự và chuyên nghiệp để gửi cho phụ huynh về tình hình học tập của con em họ.

Thông tin học sinh:
- Tên: {studentComplaint.StudentName}
- Lớp: {studentComplaint.ClassName}
- Ngày: {studentComplaint.Date:dd/MM/yyyy}

Các vấn đề được ghi nhận trong ngày:
{complaintsText}

Yêu cầu:
- Sử dụng giọng điệu lịch sự, tôn trọng
- Tránh cảm tính, đừng quá nghiêm khắc
- Khuyến khích phụ huynh phối hợp với nhà trường
- Kết thúc với lời chúc tốt đẹp
- Độ dài khoảng 100-150 từ
- Sử dụng tiếng Việt có dấu

Tin nhắn:";

            var response = await _chatBotService.SendPromptAsync(prompt);
            return response.Success && response.Data?.Message != null 
                ? response.Data.Message 
                : "Kính gửi Quý phụ huynh, con em Quý vị có một số vấn đề cần lưu ý trong ngày học hôm nay. Nhà trường mong Quý phụ huynh quan tâm và phối hợp để giúp con học tập tốt hơn.";
        }

        private async Task SendFirebaseNotificationAsync(string firebaseToken, string message, StudentComplaintData studentComplaint)
        {
            var notification = new
            {
                to = firebaseToken,
                notification = new
                {
                    title = $"Thông báo về học sinh {studentComplaint.StudentName}",
                    body = message,
                    icon = "school_notification",
                    click_action = "FLUTTER_NOTIFICATION_CLICK"
                },
                data = new
                {
                    type = "student_complaint",
                    student_id = studentComplaint.StudentId,
                    student_name = studentComplaint.StudentName,
                    date = studentComplaint.Date.ToString("yyyy-MM-dd"),
                    complaint_count = studentComplaint.Complaints.Count.ToString()
                }
            };

            var json = JsonConvert.SerializeObject(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"key={_firebaseOptions.ServerKey}");

            var response = await _httpClient.PostAsync(_firebaseOptions.Url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Firebase notification failed: {response.StatusCode} - {errorContent}");
                throw new Exception($"Failed to send Firebase notification: {response.StatusCode}");
            }

            _logger.LogInformation($"Firebase notification sent successfully to token: {firebaseToken[..10]}...");
        }
    }
}
