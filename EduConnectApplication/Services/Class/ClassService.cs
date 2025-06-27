using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ClassService : IClassService
    {
        private readonly IBaseRepository<EduConnect.Domain.Entities.Class, int> _classRepository;
        public ClassService(IBaseRepository<EduConnect.Domain.Entities.Class, int> classRepository)
        {
            _classRepository = classRepository;
        }
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> GetAllClassesAsync()
        {
            return await _classRepository.GetAllAsync();
        }
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SearchClassesAsync(string? searchTerm)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            query = ClassQueryHelper.ApplySearch(query, searchTerm);
            return query.ToList();
        }
        
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SortClassesAsync(string? sortBy, bool ascending = true)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
            return query.ToList();
        }
        
        public async Task<PagedResponse<EduConnect.Domain.Entities.Class>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true)
        {
            var query = (await _classRepository.GetAllAsync()).AsQueryable();
            
            query = ClassQueryHelper.ApplySearch(query, searchTerm);
            query = ClassQueryHelper.ApplySort(query, sortBy, ascending);
            
            var totalCount = query.Count();
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            
            return new PagedResponse<EduConnect.Domain.Entities.Class>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
