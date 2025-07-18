using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;

namespace EduConnect.Application.Services.Interface
{
    public interface IAttendanceService
    {
        Task<PagedApiResponse<IEnumerable<AttendanceResponse>>> FilterSortSearchAttendancesAsync(
            int? lessonId, string? studentId, bool? isPresent, string? searchTerm,
            string? sortBy, bool ascending, int pageIndex, int pageSize);
        Task<ApiResponse<IEnumerable<AttendanceResponse>>> GetByLessonIdAsync(int lessonId);
        Task<ApiResponse<IEnumerable<AttendanceResponse>>> GetByStudentIdAsync(string studentId);
        Task<ApiResponse<IEnumerable<AttendanceResponse>>> GetByTimetableIdAsync(int timetableId);
        Task<ApiResponse<AttendanceResponse>> UpdateAsync(int id, AttendanceRequest request);
    }
} 