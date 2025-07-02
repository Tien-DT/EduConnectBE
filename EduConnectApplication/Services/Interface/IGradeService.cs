using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IGradeService
    {
        /// <summary>
        /// Lấy danh sách điểm theo môn học (chỉ của các lớp mà giáo viên dạy môn đó)
        /// </summary>
        Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectForTeacherAsync(int subjectId, string teacherAccountId, int? classId = null);
        
        /// <summary>
        /// Lấy điểm của một học sinh trong một môn (chỉ nếu GV dạy môn đó ở lớp của HS)
        /// </summary>
        Task<ApiResponse<List<GradeResponse>>> GetStudentGradesBySubjectForTeacherAsync(int subjectId, string studentId, string teacherAccountId);
        
        /// <summary>
        /// Lấy chi tiết một điểm cụ thể (chỉ nếu GV có quyền)
        /// </summary>
        Task<ApiResponse<GradeResponse>> GetGradeByIdForTeacherAsync(int gradeId, string teacherAccountId);
        
        /// <summary>
        /// Tạo điểm mới cho học sinh (chỉ được tạo điểm môn mà GV dạy)
        /// </summary>
        Task<ApiResponse<GradeResponse>> CreateGradeForTeacherAsync(CreateGradeRequest request, string teacherAccountId);
        
        /// <summary>
        /// Cập nhật điểm (chỉ được sửa điểm môn mà GV dạy)
        /// </summary>
        Task<ApiResponse<GradeResponse>> UpdateGradeForTeacherAsync(int gradeId, UpdateGradeRequest request, string teacherAccountId);
        
        /// <summary>
        /// Xóa điểm (chỉ được xóa điểm môn mà GV dạy)
        /// </summary>
        Task<ApiResponse<string>> DeleteGradeForTeacherAsync(int gradeId, string teacherAccountId);
        
        /// <summary>
        /// Lấy danh sách học sinh để nhập điểm theo môn học
        /// </summary>
        Task<ApiResponse<List<StudentGradingResponse>>> GetStudentsForGradingAsync(int subjectId, string teacherAccountId, int? classId = null);

        // Backward compatibility methods (deprecated)
        Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectAsync(int subjectId, string teacherId);
        Task<ApiResponse<GradeResponse>> GetGradeByIdAsync(int gradeId, string teacherId);
        Task<ApiResponse<string>> CreateGradeAsync(CreateGradeRequest request, string teacherId);
        Task<ApiResponse<string>> UpdateGradeAsync(int gradeId, UpdateGradeRequest request, string teacherId);
        Task<ApiResponse<string>> DeleteGradeAsync(int gradeId, string teacherId);
    }
} 