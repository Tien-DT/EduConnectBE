using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EduConnect.Application.Services.Interface
{
    public interface IParentService
    {
        Task<ApiResponse<ParentResponse>> GetParentByIdAsync(string parentId);
        Task<PagedApiResponse<List<ParentResponse>>> GetParentsAsync(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true);
        Task<PagedApiResponse<List<ParentResponse>>> FilterSortSearchParentsAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 0,
            int pageSize = 10);
    }
}
