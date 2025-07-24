using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EduConnectApplication.DTOs.Response;

namespace EduConnect.Application.Services.Interface
{
    public interface ITeacherService
    {
        Task<ApiResponse<TimetableResponse>> GetTimetableByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<TeacherResponse>> GetTeacherByIdAsync(string teacherId);
        Task<PagedApiResponse<List<TeacherResponse>>> GetTeachersAsync(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true);
        Task<PagedApiResponse<List<TeacherResponse>>> FilterSortSearchTeachersAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 0,
            int pageSize = 10);

        /// <summary>
        /// Lấy danh sách lớp mà giáo viên có thời khóa biểu
        /// </summary>
        /// <param name="teacherId">Id của giáo viên</param>
        /// <returns>Danh sách lớp</returns>
        Task<ApiResponse<List<ClassWithTimetableResponse>>> GetClassesWithTimetableByTeacherAsync(string teacherId);
    }
}
