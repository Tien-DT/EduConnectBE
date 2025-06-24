using EduConnect.Application.DTOs.Response;
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
            var all = await _classRepository.GetAllAsync();
            if (string.IsNullOrWhiteSpace(searchTerm)) return all;
            return all.Where(c => c.ClassName != null && c.ClassName.ToLower().Contains(searchTerm.ToLower()));
        }
        public async Task<IEnumerable<EduConnect.Domain.Entities.Class>> SortClassesAsync(string? sortBy, bool ascending = true)
        {
            var all = await _classRepository.GetAllAsync();
            if (string.IsNullOrWhiteSpace(sortBy) || sortBy.ToLower() == "classname")
            {
                return ascending ? all.OrderBy(c => c.ClassName) : all.OrderByDescending(c => c.ClassName);
            }
            return all;
        }
        public async Task<PagedResponse<EduConnect.Domain.Entities.Class>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true)
        {
            var all = await _classRepository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                all = all.Where(c => c.ClassName != null && c.ClassName.ToLower().Contains(searchTerm.ToLower()));
            }
            if (string.IsNullOrWhiteSpace(sortBy) || sortBy.ToLower() == "classname")
            {
                all = ascending ? all.OrderBy(c => c.ClassName) : all.OrderByDescending(c => c.ClassName);
            }
            var totalCount = all.Count();
            var items = all.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
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
