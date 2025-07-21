using EduConnect.Application.DTOs;

namespace EduConnect.Application.Services.Interface
{
    public interface IStudentComplaintCacheService
    {
        Task AddStudentComplaintAsync(StudentComplaintData complaint);
        Task<List<StudentComplaintData>> GetDailyComplaintsAsync(DateTime date);
        Task RemoveDailyComplaintsAsync(DateTime date);
        Task<bool> HasComplaintsForDateAsync(DateTime date);
    }
}
