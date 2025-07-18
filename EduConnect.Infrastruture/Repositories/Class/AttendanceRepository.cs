using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class AttendanceRepository : BaseRepository<Attendance, int>, IAttendanceRepository
    {
        public AttendanceRepository(EduConnectDbContext context) : base(context) { }

        public async Task<IEnumerable<Attendance>> GetByTimetableSlotIdAsync(int timetableSlotId)
        {
            return await _context.Attendances
                .Where(a => a.TimetableSlotID == timetableSlotId && a.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByStudentIdAsync(string studentId)
        {
            return await _context.Attendances
                .Where(a => a.StudentID == studentId && a.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByTimetableIdAsync(int timetableId)
        {
            return await _context.Attendances
                .Include(a => a.TimetableSlot)
                .Where(a => a.TimetableSlot.TimetableID == timetableId && a.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Attendance> attendances, int totalCount)> FilterSortSearchAsync(
            int? timetableSlotId, string? studentId, int? status, string? searchTerm,
            string? sortBy, bool ascending, int pageIndex, int pageSize)
        {
            var query = _context.Attendances
                .Include(a => a.TimetableSlot)
                .Include(a => a.Student)
                .Where(a => a.IsDeleted != true);

            if (timetableSlotId.HasValue)
                query = query.Where(a => a.TimetableSlotID == timetableSlotId);
            if (!string.IsNullOrEmpty(studentId))
                query = query.Where(a => a.StudentID == studentId);
            if (status.HasValue)
                query = query.Where(a => a.Status == status);
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(a => a.Note.Contains(searchTerm) || a.Student.StudentID.Contains(searchTerm));

            // Sorting
            query = sortBy switch
            {
                "TimetableSlotID" => ascending ? query.OrderBy(a => a.TimetableSlotID) : query.OrderByDescending(a => a.TimetableSlotID),
                "StudentID" => ascending ? query.OrderBy(a => a.StudentID) : query.OrderByDescending(a => a.StudentID),
                "Status" => ascending ? query.OrderBy(a => a.Status) : query.OrderByDescending(a => a.Status),
                _ => ascending ? query.OrderBy(a => a.AttendanceID) : query.OrderByDescending(a => a.AttendanceID)
            };

            var totalCount = await query.CountAsync();
            var attendances = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (attendances, totalCount);
        }
    }
} 