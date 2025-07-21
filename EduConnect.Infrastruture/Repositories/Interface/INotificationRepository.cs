using EduConnect.Domain.Entities;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface INotificationRepository : IBaseRepository<Notification, string>
    {
        Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(string userId);
        Task<List<Notification>> GetNotificationsByUserIdAsync(string userId, int pageIndex = 0, int pageSize = 20);
        Task<List<Notification>> GetUnpushedNotificationsAsync();
        Task<bool> MarkAsReadAsync(string notificationId);
        Task<bool> MarkAsReadAsync(List<string> notificationIds);
        Task<bool> MarkAsPushedAsync(string notificationId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> DeleteExpiredNotificationsAsync();
        Task<List<Notification>> GetNotificationsByTypeAndDateRangeAsync(string type, DateTime startDate, DateTime endDate);
    }
}
