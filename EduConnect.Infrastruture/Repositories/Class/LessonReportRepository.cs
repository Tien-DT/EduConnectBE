using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Class
{
    public class LessonReportRepository : IBaseRepository<LessonReport, int>, ILessonReportRepository
    {
        private readonly EduConnectDbContext _context;

        public LessonReportRepository(EduConnectDbContext context)
        {
            _context = context;
        }

        public async Task<LessonReport> CreateAsync(LessonReport entity)
        {
            _context.LessonReports.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var report = await _context.LessonReports.FindAsync(id);
            if (report == null)
                return false;

            report.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<LessonReport>> GetAllAsync()
        {
            return await _context.LessonReports
                .Where(lr => lr.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<IEnumerable<LessonReport>> GetAllWithDetailsAsync()
        {
            return await GetQueryableWithDetails()
                .Where(lr => lr.IsDeleted != true)
                .ToListAsync();
        }

        public async Task<LessonReport?> GetByIdAsync(int id)
        {
            return await _context.LessonReports
                .FirstOrDefaultAsync(lr => lr.ReportID == id && lr.IsDeleted != true);
        }

        public async Task<LessonReport?> GetWithDetailsByIdAsync(int reportId)
        {
            return await GetQueryableWithDetails()
                .FirstOrDefaultAsync(lr => lr.ReportID == reportId && lr.IsDeleted != true);
        }

        public async Task<LessonReport?> UpdateAsync(int id, LessonReport entity)
        {
            var existingReport = await _context.LessonReports.FindAsync(id);
            if (existingReport == null)
                return null;

            // Update properties
            existingReport.LessonID = entity.LessonID;
            existingReport.TotalStudents = entity.TotalStudents;
            existingReport.AbsentStudents = entity.AbsentStudents;
            existingReport.Remarks = entity.Remarks;
            existingReport.LessonDate = entity.LessonDate;

            await _context.SaveChangesAsync();
            return existingReport;
        }

        public async Task<IEnumerable<LessonReport>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetQueryableWithDetails()
                .Where(lr => lr.IsDeleted != true && 
                            lr.LessonDate.Date >= startDate.Date && 
                            lr.LessonDate.Date <= endDate.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<LessonReport>> GetByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = GetQueryableWithDetails()
                .Where(lr => lr.IsDeleted != true && lr.TimetableSlot.TeacherID == teacherId);

            if (startDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date <= endDate.Value.Date);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<LessonReport>> GetByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = GetQueryableWithDetails()
                .Where(lr => lr.IsDeleted != true && lr.TimetableSlot.Timetable.ClassID == classId);

            if (startDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date <= endDate.Value.Date);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<LessonReport>> GetByTimetableSlotIdAsync(int timetableSlotId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = GetQueryableWithDetails()
                .Where(lr => lr.IsDeleted != true && lr.TimetableSlotID == timetableSlotId);

            if (startDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date <= endDate.Value.Date);

            return await query.ToListAsync();
        }

        public async Task<bool> ExistsForTimetableSlotAndDateAsync(int timetableSlotId, DateTime lessonDate)
        {
            return await _context.LessonReports
                .AnyAsync(lr => lr.TimetableSlotID == timetableSlotId && 
                               lr.LessonDate.Date == lessonDate.Date && 
                               lr.IsDeleted != true);
        }

        public async Task<(IEnumerable<LessonReport> Reports, int TotalCount)> GetPagedWithDetailsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? teacherId = null,
            int? classId = null,
            int? timetableSlotId = null)
        {
            var query = GetQueryableWithDetails()
                .Where(lr => lr.IsDeleted != true);

            // Apply filters
            if (startDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(lr => lr.LessonDate.Date <= endDate.Value.Date);

            if (!string.IsNullOrEmpty(teacherId))
                query = query.Where(lr => lr.TimetableSlot.TeacherID == teacherId);

            if (classId.HasValue)
                query = query.Where(lr => lr.TimetableSlot.Timetable.ClassID == classId.Value);

            if (timetableSlotId.HasValue)
                query = query.Where(lr => lr.TimetableSlotID == timetableSlotId.Value);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(lr => 
                    (lr.TimetableSlot.Subject.SubjectName != null && lr.TimetableSlot.Subject.SubjectName.ToLower().Contains(searchTerm)) ||
                    (lr.TimetableSlot.Subject.SubjectCode != null && lr.TimetableSlot.Subject.SubjectCode.ToLower().Contains(searchTerm)) ||
                    (lr.TimetableSlot.Teacher.FullName != null && lr.TimetableSlot.Teacher.FullName.ToLower().Contains(searchTerm)) ||
                    (lr.TimetableSlot.Timetable.Class.ClassName != null && lr.TimetableSlot.Timetable.Class.ClassName.ToLower().Contains(searchTerm)) ||
                    (lr.Remarks != null && lr.Remarks.ToLower().Contains(searchTerm)));
            }

            // Apply sorting
            query = ApplySorting(query, sortBy, ascending);

            var totalCount = await query.CountAsync();
            var reports = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reports, totalCount);
        }

        public IQueryable<LessonReport> GetQueryableWithDetails()
        {
            return _context.LessonReports
                .Include(lr => lr.TimetableSlot)
                    .ThenInclude(ts => ts.Subject)
                .Include(lr => lr.TimetableSlot)
                    .ThenInclude(ts => ts.Teacher)
                .Include(lr => lr.TimetableSlot)
                    .ThenInclude(ts => ts.Timetable)
                        .ThenInclude(t => t.Class)
                .Include(lr => lr.Lesson)
                .Include(lr => lr.StudentLessonNotes)
                    .ThenInclude(sln => sln.Student);
        }

        private IQueryable<LessonReport> ApplySorting(IQueryable<LessonReport> query, string sortBy, bool ascending)
        {
            switch (sortBy.ToLower())
            {
                case "lessondate":
                    return ascending 
                        ? query.OrderBy(lr => lr.LessonDate)
                        : query.OrderByDescending(lr => lr.LessonDate);
                case "subject":
                case "subjectname":
                    return ascending 
                        ? query.OrderBy(lr => lr.TimetableSlot.Subject.SubjectName)
                        : query.OrderByDescending(lr => lr.TimetableSlot.Subject.SubjectName);
                case "teacher":
                case "teachername":
                    return ascending 
                        ? query.OrderBy(lr => lr.TimetableSlot.Teacher.FullName)
                        : query.OrderByDescending(lr => lr.TimetableSlot.Teacher.FullName);
                case "class":
                case "classname":
                    return ascending 
                        ? query.OrderBy(lr => lr.TimetableSlot.Timetable.Class.ClassName)
                        : query.OrderByDescending(lr => lr.TimetableSlot.Timetable.Class.ClassName);
                case "period":
                    return ascending 
                        ? query.OrderBy(lr => lr.TimetableSlot.Period)
                        : query.OrderByDescending(lr => lr.TimetableSlot.Period);
                case "createdat":
                    return ascending 
                        ? query.OrderBy(lr => lr.CreatedAt)
                        : query.OrderByDescending(lr => lr.CreatedAt);
                default:
                    return ascending 
                        ? query.OrderBy(lr => lr.LessonDate)
                        : query.OrderByDescending(lr => lr.LessonDate);
            }
        }
        
        // Validation methods
        public async Task<bool> CheckLessonExistsAsync(int lessonId)
        {
            return await _context.Lessons
                .AnyAsync(l => l.LessonID == lessonId && l.IsDeleted != true);
        }

        public async Task<bool> CheckTimetableSlotExistsAsync(int timetableSlotId)
        {
            return await _context.TimetableSlots
                .AnyAsync(ts => ts.TimetableSlotID == timetableSlotId && ts.IsDeleted != true);
        }

        public async Task<bool> CheckStudentExistsAsync(string studentId)
        {
            return await _context.Students
                .AnyAsync(s => s.StudentID == studentId && s.IsDeleted != true);
        }
    }
}
