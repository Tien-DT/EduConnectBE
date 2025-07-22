using AutoMapper;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EduConnect.Application.Services.Class
{
    public class EnhancedNotificationService : IEnhancedNotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService; 
        private readonly ILogger<EnhancedNotificationService> _logger;
        private readonly IMapper _mapper;

        public EnhancedNotificationService(
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            ILogger<EnhancedNotificationService> logger,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<string>> CreateAndSendNotificationAsync(
            string userId, 
            string title, 
            string content, 
            string summary, 
            string type, 
            Dictionary<string, string>? metaData = null)
        {
            try
            {
                _logger.LogInformation($"📝 Bắt đầu tạo notification cho user: {userId}");

                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Content = content,
                    Summary = summary,
                    Type = type,
                    Category = "report",
                    Priority = GetPriorityByType(type),
                    ExpiresAt = DateTime.Now.AddDays(30), 
                    MetaData = metaData != null ? JsonSerializer.Serialize(metaData) : null
                };

                _logger.LogInformation($"💾 Đang lưu notification với ID: {notification.NotificationId}");
                
                await _notificationRepository.CreateAsync(notification);

                _logger.LogInformation($"✅ Đã lưu thành công notification {notification.NotificationId} cho user {userId}");

                // Kiểm tra xem notification đã được lưu chưa
                var savedNotification = await _notificationRepository.GetByIdAsync(notification.NotificationId);
                if (savedNotification != null)
                {
                    _logger.LogInformation($"🔍 Xác nhận: notification {notification.NotificationId} đã tồn tại trong DB");
                }
                else
                {
                    _logger.LogError($"❌ Lỗi: notification {notification.NotificationId} không tồn tại trong DB sau khi lưu");
                }

                var pushResult = await TrySendPushNotificationAsync(userId, title, summary, metaData);
                
                if (pushResult.Success)
                {
                    await _notificationRepository.MarkAsPushedAsync(notification.NotificationId);
                    _logger.LogInformation($"Đã gửi push notification thành công cho user {userId}");
                }
                else
                {
                    _logger.LogWarning($"Push notification thất bại cho user {userId}: {pushResult.Message}");
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Đã tạo và lưu thông báo thành công. Người dùng sẽ nhận được khi online.",
                    Data = notification.NotificationId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tạo notification cho user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo thông báo"
                };
            }
        }

        public async Task<ApiResponse<string>> SendStoredNotificationsAsync(string userId)
        {
            try
            {
                var unpushedNotifications = await _notificationRepository.GetUnpushedNotificationsAsync();
                var userNotifications = unpushedNotifications.Where(n => n.UserId == userId).ToList();

                if (!userNotifications.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Không có thông báo chưa gửi nào cho user này"
                    };
                }

                var successCount = 0;
                var failedCount = 0;

                foreach (var notification in userNotifications)
                {
                    var metaData = !string.IsNullOrEmpty(notification.MetaData) 
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(notification.MetaData)
                        : null;

                    var result = await TrySendPushNotificationAsync(
                        notification.UserId, 
                        notification.Title, 
                        notification.Summary ?? notification.Content, 
                        metaData);

                    if (result.Success)
                    {
                        await _notificationRepository.MarkAsPushedAsync(notification.NotificationId);
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                    }

                    await Task.Delay(500);
                }

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = $"Đã gửi {successCount}/{userNotifications.Count} thông báo thành công",
                    Data = $"Success: {successCount}, Failed: {failedCount}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi stored notifications cho user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi gửi thông báo đã lưu"
                };
            }
        }

        public async Task<ApiResponse<List<NotificationResponse>>> GetUserNotificationsAsync(string userId, int pageIndex = 0, int pageSize = 20)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, pageIndex, pageSize);
                
                var notificationResponses = notifications.Select(n => new NotificationResponse
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    Summary = n.Summary ?? string.Empty,
                    Type = n.Type,
                    Category = n.Category,
                    Priority = n.Priority,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    MetaData = !string.IsNullOrEmpty(n.MetaData) 
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(n.MetaData)
                        : null
                }).ToList();

                return new ApiResponse<List<NotificationResponse>>
                {
                    Success = true,
                    Message = $"Lấy danh sách {notificationResponses.Count} thông báo thành công",
                    Data = notificationResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy notifications cho user {userId}");
                return new ApiResponse<List<NotificationResponse>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách thông báo"
                };
            }
        }

        public async Task<ApiResponse<string>> MarkNotificationsAsReadAsync(List<string> notificationIds)
        {
            try
            {
                var result = await _notificationRepository.MarkAsReadAsync(notificationIds);
                
                return new ApiResponse<string>
                {
                    Success = result,
                    Message = result ? "Đã đánh dấu thông báo là đã đọc" : "Không tìm thấy thông báo để đánh dấu"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đánh dấu notifications đã đọc");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi đánh dấu thông báo"
                };
            }
        }

        public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                
                return new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Có {count} thông báo chưa đọc",
                    Data = count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy số lượng notifications chưa đọc cho user {userId}");
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy số lượng thông báo chưa đọc"
                };
            }
        }

        public async Task<ApiResponse<string>> ProcessUnpushedNotificationsAsync()
        {
            try
            {
                var unpushedNotifications = await _notificationRepository.GetUnpushedNotificationsAsync();

                if (!unpushedNotifications.Any())
                {
                    return new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Không có thông báo chưa gửi nào"
                    };
                }

                var successCount = 0;
                var failedCount = 0;

                foreach (var notification in unpushedNotifications)
                {
                    var metaData = !string.IsNullOrEmpty(notification.MetaData) 
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(notification.MetaData)
                        : null;

                    var result = await TrySendPushNotificationAsync(
                        notification.UserId, 
                        notification.Title, 
                        notification.Summary ?? notification.Content, 
                        metaData);

                    if (result.Success)
                    {
                        await _notificationRepository.MarkAsPushedAsync(notification.NotificationId);
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                    }

                    await Task.Delay(1000);
                }

                _logger.LogInformation($"Processed unpushed notifications: {successCount} success, {failedCount} failed");

                return new ApiResponse<string>
                {
                    Success = successCount > 0,
                    Message = $"Đã xử lý {successCount + failedCount} thông báo chưa gửi",
                    Data = $"Success: {successCount}, Failed: {failedCount}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý unpushed notifications");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xử lý thông báo chưa gửi"
                };
            }
        }

        public async Task<ApiResponse<string>> CleanupExpiredNotificationsAsync()
        {
            try
            {
                var result = await _notificationRepository.DeleteExpiredNotificationsAsync();
                
                return new ApiResponse<string>
                {
                    Success = result,
                    Message = result ? "Đã xóa thông báo hết hạn thành công" : "Không có thông báo hết hạn nào để xóa"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cleanup expired notifications");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa thông báo hết hạn"
                };
            }
        }

        #region Private Methods

        private async Task<ApiResponse<string>> TrySendPushNotificationAsync(
            string userId, 
            string title, 
            string body, 
            Dictionary<string, string>? data = null)
        {
            try
            {
                return await _notificationService.SendNotificationToAllDevicesAsync(userId, title, body, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi push notification cho user {userId}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private int GetPriorityByType(string type)
        {
            return type.ToLower() switch
            {
                "monthly_report" => 2,
                "weekly_report" => 2,
                "daily_report" => 1,  
                "urgent" => 3,      
                _ => 1
            };
        }

        #endregion
    }
}
