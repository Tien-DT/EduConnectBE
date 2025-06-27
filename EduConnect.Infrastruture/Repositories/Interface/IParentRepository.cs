using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Infrastruture.Repositories.Interface
{
    public interface IParentRepository : IBaseRepository<Parent, string>
    {
        Task<IEnumerable<Parent>> GetAllActiveParentsAsync();
        Task<Parent?> GetParentWithDetailsAsync(string parentId);
        Task<IEnumerable<Parent>> SearchParentsAsync(string searchTerm);
        Task<IEnumerable<Parent>> SortParentsAsync(string sortBy = "FullName", bool ascending = true);
        Task<(IEnumerable<Parent> Parents, int TotalCount)> GetPagedParentsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true);
        IQueryable<Parent> GetParentQueryable();
    }
}
