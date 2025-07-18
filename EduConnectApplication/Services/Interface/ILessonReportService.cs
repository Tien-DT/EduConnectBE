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
        Task<ApiResponse<LessonReportResponse>> GetLessonReportByIdAsync(int reportId);
        
        Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetPagedLessonReportsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByTeacherAsync(
            string teacherId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByClassAsync(
            int classId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByTimetableSlotAsync(
            int timetableSlotId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByStudentAsync(
            string studentId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null);
        
        Task<ApiResponse<string>> CreateLessonReportAsync(CreateLessonReportRequest request, string createdBy);
        Task<ApiResponse<string>> UpdateLessonReportAsync(UpdateLessonReportRequest request, string updatedBy);
        Task<ApiResponse<string>> DeleteLessonReportAsync(int reportId, string deletedBy);
    }
}
