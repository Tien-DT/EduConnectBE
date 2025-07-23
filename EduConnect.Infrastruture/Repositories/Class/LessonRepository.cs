using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastruture.Repositories.Class;

public class LessonRepository : BaseRepository<Lesson, int>, ILessonRepository
{
    public LessonRepository(EduConnectDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Lesson>> GetLessonsByTeacherIdAsync(string teacherId, int pageIndex, int pageSize)
    {
        return await _context.Lessons
            .Include(l => l.Class)
            .Include(l => l.Teacher)
            .Include(l => l.Subject)
            .Include(l => l.Timetable)
            .Where(l => l.TeacherID == teacherId && l.IsDeleted != true)
            .OrderByDescending(l => l.Date)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountLessonsByTeacherIdAsync(string teacherId)
    {
        return await _context.Lessons
            .CountAsync(l => l.TeacherID == teacherId && l.IsDeleted != true);
    }

    public async Task<Lesson?> GetLessonByIdAndTeacherIdAsync(int lessonId, string teacherId)
    {
        return await _context.Lessons
            .Include(l => l.Class)
            .Include(l => l.Teacher)
            .Include(l => l.Subject)
            .Include(l => l.Timetable)
            .Where(l => l.LessonID == lessonId && l.TeacherID == teacherId && l.IsDeleted != true)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Lesson>> GetLessonsByTeacherAndDateRangeAsync(string teacherId, DateTime startDate, DateTime endDate)
    {
        return await _context.Lessons
            .Include(l => l.Class)
            .Include(l => l.Teacher)
            .Include(l => l.Subject)
            .Include(l => l.Timetable)
            .Where(l => l.TeacherID == teacherId && 
                       l.Date >= startDate && 
                       l.Date <= endDate && 
                       l.IsDeleted != true)
            .OrderBy(l => l.Date)
            .ToListAsync();
    }
}
