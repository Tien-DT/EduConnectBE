using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class NotificationRepository : BaseRepository<Notification, string>, INotificationRepository
    {
        public NotificationRepository(EduConnectDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId, int pageIndex = 0, int pageSize = 20)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnpushedNotificationsAsync()
        {
            return await _context.Notifications
                .Where(n => !n.IsPushed && n.CreatedAt >= DateTime.Now.AddDays(-7)) 
                .OrderBy(n => n.CreatedAt)
                .Take(100) 
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsReadAsync(List<string> notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.NotificationId) && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any()) return true;

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsPushedAsync(string notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsPushed = true;
            notification.PushedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> DeleteExpiredNotificationsAsync()
        {
            var expiredNotifications = await _context.Notifications
                .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.Now)
                .ToListAsync();

            if (!expiredNotifications.Any()) return true;

            _context.Notifications.RemoveRange(expiredNotifications);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Notification>> GetNotificationsByTypeAndDateRangeAsync(string type, DateTime startDate, DateTime endDate)
        {
            return await _context.Notifications
                .Where(n => n.Type == type && n.CreatedAt >= startDate && n.CreatedAt <= endDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
