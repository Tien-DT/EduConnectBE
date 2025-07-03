using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IGradeService
    {
        Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectForTeacherAsync(int subjectId, string teacherAccountId, int? classId = null);

        Task<ApiResponse<List<GradeResponse>>> GetStudentGradesBySubjectForTeacherAsync(int subjectId, string studentId, string teacherAccountId);
        Task<ApiResponse<GradeResponse>> GetGradeByIdForTeacherAsync(int gradeId, string teacherAccountId);
        Task<ApiResponse<GradeResponse>> CreateGradeForTeacherAsync(CreateGradeRequest request, string teacherAccountId);
        Task<ApiResponse<GradeResponse>> UpdateGradeForTeacherAsync(int gradeId, UpdateGradeRequest request, string teacherAccountId);
        Task<ApiResponse<string>> DeleteGradeForTeacherAsync(int gradeId, string teacherAccountId);
        Task<ApiResponse<List<StudentGradingResponse>>> GetStudentsForGradingAsync(int subjectId, string teacherAccountId, int? classId = null);
        Task<ApiResponse<List<GradeResponse>>> GetGradesByStudentIdAsync(string studentId);
        Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectAsync(int subjectId, string teacherId);
        Task<ApiResponse<GradeResponse>> GetGradeByIdAsync(int gradeId, string teacherId);
        Task<ApiResponse<string>> CreateGradeAsync(CreateGradeRequest request, string teacherId);
        Task<ApiResponse<string>> UpdateGradeAsync(int gradeId, UpdateGradeRequest request, string teacherId);
        Task<ApiResponse<string>> DeleteGradeAsync(int gradeId, string teacherId);
    }
} 