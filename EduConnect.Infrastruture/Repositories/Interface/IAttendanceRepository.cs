using EduConnect.Domain.Entities;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IAttendanceRepository : IBaseRepository<Attendance, int>
    {
        Task<IEnumerable<Attendance>> GetByLessonIdAsync(int lessonId);
        Task<IEnumerable<Attendance>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Attendance>> GetByTimetableIdAsync(int timetableId);
        Task<(IEnumerable<Attendance> attendances, int totalCount)> FilterSortSearchAsync(
            int? lessonId, string? studentId, bool? isPresent, string? searchTerm,
            string? sortBy, bool ascending, int pageIndex, int pageSize);
    }
} 