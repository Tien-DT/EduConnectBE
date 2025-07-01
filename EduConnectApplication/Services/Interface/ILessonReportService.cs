using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface ILessonReportService
    {
        Task<ApiResponse<List<LessonReportResponse>>> GetAllLessonReportsAsync();
        Task<ApiResponse<LessonReportResponse>> GetLessonReportByIdAsync(int reportId);
        Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByTeacherAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByClassAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByTimetableSlotAsync(int timetableSlotId, DateTime? startDate = null, DateTime? endDate = null);
        
        Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetPagedLessonReportsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? teacherId = null,
            int? classId = null);
        
        Task<ApiResponse<string>> CreateLessonReportAsync(CreateLessonReportRequest request, string createdBy);
        Task<ApiResponse<string>> UpdateLessonReportAsync(UpdateLessonReportRequest request, string updatedBy);
        Task<ApiResponse<string>> DeleteLessonReportAsync(int reportId, string deletedBy);
        
        Task<ApiResponse<List<LessonReportResponse>>> SearchLessonReportsAsync(string searchTerm);
        Task<ApiResponse<List<LessonReportResponse>>> SortLessonReportsAsync(string sortBy, bool ascending = true);
    }
}
