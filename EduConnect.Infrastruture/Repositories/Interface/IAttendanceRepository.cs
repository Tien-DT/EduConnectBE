using EduConnect.Domain.Entities;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IAttendanceRepository : IBaseRepository<Attendance, int>
    {
        Task<IEnumerable<Attendance>> GetByTimetableSlotIdAsync(int timetableSlotId);
        Task<IEnumerable<Attendance>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Attendance>> GetByTimetableIdAsync(int timetableId);
        Task<(IEnumerable<Attendance> attendances, int totalCount)> FilterSortSearchAsync(
            int? timetableSlotId, string? studentId, int? status, string? searchTerm,
            string? sortBy, bool ascending, int pageIndex, int pageSize);
    }
} 