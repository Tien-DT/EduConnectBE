using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;

namespace EduConnect.Application.Services.Interface;

public interface ILessonService
{
    Task<ApiResponse<PagedResponse<LessonResponse>>> GetLessonsByTeacherIdAsync(string teacherId, int pageIndex, int pageSize);
    Task<ApiResponse<LessonResponse>> GetLessonByIdAsync(int lessonId, string teacherId);
    Task<ApiResponse<LessonResponse>> CreateLessonAsync(CreateLessonRequest request);
    Task<ApiResponse<LessonResponse>> UpdateLessonAsync(int lessonId, string teacherId, UpdateLessonRequest request);
    Task<ApiResponse<bool>> DeleteLessonAsync(int lessonId, string teacherId);
    Task<ApiResponse<IEnumerable<LessonResponse>>> GetLessonsByDateRangeAsync(string teacherId, DateTime startDate, DateTime endDate);
}
