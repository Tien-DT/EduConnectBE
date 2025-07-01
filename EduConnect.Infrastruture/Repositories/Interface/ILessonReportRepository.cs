using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface ILessonReportRepository : IBaseRepository<LessonReport, int>
    {
        Task<IEnumerable<LessonReport>> GetAllWithDetailsAsync();
        Task<LessonReport?> GetWithDetailsByIdAsync(int reportId);
        Task<IEnumerable<LessonReport>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<LessonReport>> GetByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<LessonReport>> GetByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<LessonReport>> GetByTimetableSlotIdAsync(int timetableSlotId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> ExistsForTimetableSlotAndDateAsync(int timetableSlotId, DateTime lessonDate);
        Task<(IEnumerable<LessonReport> Reports, int TotalCount)> GetPagedWithDetailsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? teacherId = null,
            int? classId = null);
        IQueryable<LessonReport> GetQueryableWithDetails();
    }
}
