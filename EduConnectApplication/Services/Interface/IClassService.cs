using EduConnect.Application.DTOs.Response;
using EduConnect.Application.DTOs.Request;
using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IClassService
    {
        Task<IEnumerable<EduConnect.Domain.Entities.Class>> GetAllClassesAsync();
        Task<IEnumerable<EduConnect.Domain.Entities.Class>> SearchClassesAsync(string? searchTerm);
        Task<IEnumerable<EduConnect.Domain.Entities.Class>> SortClassesAsync(string? sortBy, bool ascending = true);
        Task<PagedResponse<EduConnect.Domain.Entities.Class>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true);

        // CRUD học sinh vào lớp (chỉ Principal)
        Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsInClassAsync(int classId);
        Task<ApiResponse<string>> AddStudentsToClassAsync(AddStudentsToClassRequest request);
        Task<ApiResponse<string>> RemoveStudentsFromClassAsync(RemoveStudentsFromClassRequest request);
        Task<ApiResponse<string>> UpdateStudentClassAsync(UpdateStudentClassRequest request);
        Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsNotInClassAsync();
    }
}
