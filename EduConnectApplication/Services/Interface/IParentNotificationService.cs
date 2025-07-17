using EduConnect.Application.Models;

namespace EduConnect.Application.Services.Interface
{
    public interface IParentNotificationService
    {
        Task SendDailyComplaintNotificationAsync(StudentComplaintData studentComplaint);
        Task ProcessDailyComplaintsAsync(DateTime date);
    }
}
