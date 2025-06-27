using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface ITeacherRepository : IBaseRepository<Teacher, string>
    {
        Task<IEnumerable<Teacher>> GetAllActiveTeachersAsync();
        Task<Teacher?> GetTeacherWithDetailsAsync(string teacherId);
        Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm);
        Task<IEnumerable<Teacher>> SortTeachersAsync(string sortBy = "FullName", bool ascending = true);
        Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetPagedTeachersAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true);
        IQueryable<Teacher> GetTeacherQueryable();
    }
}
