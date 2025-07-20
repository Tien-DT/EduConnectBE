using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface ISubjectRepository
    {
        Task<Subject?> GetByIdAsync(int subjectId);
        Task<List<Subject>> GetAllAsync();
        Task<Subject> AddAsync(Subject subject);
        Task<Subject> UpdateAsync(Subject subject);
        Task<bool> DeleteAsync(int subjectId);
        Task<bool> ExistsAsync(int subjectId);
        Task<bool> ExistsByCodeAsync(string subjectCode);
        Task<bool> ExistsByNameAsync(string subjectName);
        Task<(List<Subject> Items, int TotalCount)> FilterSortSearchAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 1,
            int pageSize = 10,
            bool includeDeleted = false);
    }
}
