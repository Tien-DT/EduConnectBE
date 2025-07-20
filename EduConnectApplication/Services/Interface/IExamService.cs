using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IExamService
    {
        Task<ApiResponse<ExamResponse>> GetExamByIdAsync(int examId);
        
        Task<ApiResponse<ExamResponse>> CreateExamAsync(CreateExamRequest request, string? createdBy = null);
        
        Task<ApiResponse<List<ExamResponse>>> CreateBulkExamsAsync(List<CreateExamRequest> requests, string? createdBy = null);
        
        Task<ApiResponse<List<ExamResponse>>> CreateClassExamsAsync(CreateClassExamRequest request, string? createdBy = null);
        
        Task<ApiResponse<List<ExamResponse>>> CreatePrincipalExamsAsync(CreatePrincipalExamRequest request, string? createdBy = null);
        
        Task<PagedApiResponse<List<ExamResponse>>> GetExamsByAccountIdAsync(
            string accountId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null,
            string? searchTerm = null);
        
        Task<PagedApiResponse<List<ExamResponse>>> GetExamsByStudentIdAsync(
            string studentId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null,
            string? searchTerm = null);
            
        Task<PagedApiResponse<List<ExamResponse>>> FilterSortSearchExamsAsync(
            string? studentId = null,
            string? searchTerm = null,
            string? room = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 1,
            int pageSize = 10);

        Task<ApiResponse<ExamResponse>> UpdateExamAsync(int examId, UpdateExamRequest request, string? updatedBy = null);
        
        Task<ApiResponse<string>> DeleteExamAsync(int examId);
    }
}