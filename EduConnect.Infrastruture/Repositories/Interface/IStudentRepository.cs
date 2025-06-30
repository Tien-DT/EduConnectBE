using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IStudentRepository : IBaseRepository<Student, string>
    {
        Task<IEnumerable<Student>> GetAllActiveStudentsAsync();
        Task<Student?> GetStudentWithDetailsAsync(string studentId);
        Task<List<Student>> GetStudentsByClassIdAsync(int classId);
        Task<List<Student>> GetStudentsByIdsAsync(List<string> studentIds);
        Task<Timetable?> GetLatestTimetableByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Timetable>> GetAllTimetablesByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedStudentsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true);
        IQueryable<Student> GetStudentQueryable();
        Task<Student?> GetByAccountIdAsync(string accountId);
    }
}
