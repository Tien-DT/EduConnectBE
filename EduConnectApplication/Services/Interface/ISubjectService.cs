using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface ISubjectService
    {
        Task<ApiResponse<SubjectResponse>> GetSubjectByIdAsync(int subjectId);
        Task<ApiResponse<List<SubjectResponse>>> GetAllSubjectsAsync();
        Task<ApiResponse<SubjectResponse>> CreateSubjectAsync(CreateSubjectRequest request);
        Task<ApiResponse<SubjectResponse>> UpdateSubjectAsync(int subjectId, UpdateSubjectRequest request);
        Task<ApiResponse<string>> DeleteSubjectAsync(int subjectId);
        Task<PagedApiResponse<List<SubjectResponse>>> FilterSortSearchSubjectsAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 1,
            int pageSize = 10,
            bool includeDeleted = false);
    }
}
