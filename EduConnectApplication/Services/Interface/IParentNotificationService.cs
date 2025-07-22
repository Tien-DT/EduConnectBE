using EduConnect.Application.DTOs;

namespace EduConnect.Application.Services.Interface
{
    public interface IParentNotificationService
    {
        Task SendDailyComplaintNotificationAsync(StudentComplaintData studentComplaint);
        Task ProcessDailyComplaintsAsync(DateTime date);
    }
}
