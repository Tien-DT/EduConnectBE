using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class AttendanceRepository : BaseRepository<Attendance, int>, IAttendanceRepository
    {
        public AttendanceRepository(EduConnectDbContext context) : base(context) { }

        public async Task<IEnumerable<Attendance>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Attendances
                .Where(a => a.LessonID == lessonId && a.IsDeleted != true)
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
                .Include(a => a.Lesson)
                .Where(a => a.Lesson.TimetableID == timetableId && a.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Attendance> attendances, int totalCount)> FilterSortSearchAsync(
            int? lessonId, string? studentId, bool? isPresent, string? searchTerm,
            string? sortBy, bool ascending, int pageIndex, int pageSize)
        {
            var query = _context.Attendances
                .Include(a => a.Lesson)
                .Include(a => a.Student)
                .Where(a => a.IsDeleted != true);

            if (lessonId.HasValue)
                query = query.Where(a => a.LessonID == lessonId);
            if (!string.IsNullOrEmpty(studentId))
                query = query.Where(a => a.StudentID == studentId);
            if (isPresent.HasValue)
                query = query.Where(a => a.IsPresent == isPresent);
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(a => a.Note.Contains(searchTerm) || a.Student.StudentID.Contains(searchTerm));

            // Sorting
            query = sortBy switch
            {
                "LessonID" => ascending ? query.OrderBy(a => a.LessonID) : query.OrderByDescending(a => a.LessonID),
                "StudentID" => ascending ? query.OrderBy(a => a.StudentID) : query.OrderByDescending(a => a.StudentID),
                "IsPresent" => ascending ? query.OrderBy(a => a.IsPresent) : query.OrderByDescending(a => a.IsPresent),
                _ => ascending ? query.OrderBy(a => a.AttendanceID) : query.OrderByDescending(a => a.AttendanceID)
            };

            var totalCount = await query.CountAsync();
            var attendances = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (attendances, totalCount);
        }
    }
} 