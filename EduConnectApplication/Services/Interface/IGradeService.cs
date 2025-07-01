using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IGradeService
    {
        Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectAsync(int subjectId, string teacherId);
        Task<ApiResponse<GradeResponse>> GetGradeByIdAsync(int gradeId, string teacherId);
        Task<ApiResponse<string>> CreateGradeAsync(CreateGradeRequest request, string teacherId);
        Task<ApiResponse<string>> UpdateGradeAsync(UpdateGradeRequest request, string teacherId);
        Task<ApiResponse<string>> DeleteGradeAsync(int gradeId, string teacherId);
    }
} 