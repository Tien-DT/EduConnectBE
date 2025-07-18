using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassEntity = EduConnect.Domain.Entities.Class;

namespace EduConnect.Application.Services.Interface
{
    public interface IClassService
    {
        Task<IEnumerable<ClassEntity>> GetAllClassesAsync();
        Task<IEnumerable<ClassEntity>> SearchClassesAsync(string? searchTerm);
        Task<IEnumerable<ClassEntity>> SortClassesAsync(string? sortBy, bool ascending = true);
        Task<PagedResponse<ClassEntity>> GetPagedClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string? sortBy = null, bool ascending = true);
        Task<ApiResponse<PagedResponse<ClassResponse>>> GetClassesAsync(int pageIndex, int pageSize, string? searchTerm = null, string sortBy = "ClassName", bool ascending = true);
        Task<ApiResponse<ClassResponse>> GetClassByIdAsync(int classId);
        
        // Class-Student management methods
        Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsInClassAsync(int classId);
        Task<ApiResponse<List<ClassStudentResponse>>> GetStudentsNotInClassAsync();
        Task<ApiResponse<string>> AddStudentsToClassAsync(int classId, List<string> studentIds);
        Task<ApiResponse<string>> RemoveStudentsFromClassAsync(int classId, List<string> studentIds);
        Task<ApiResponse<string>> UpdateStudentClassAsync(string studentId, int? newClassId);
    }
}
