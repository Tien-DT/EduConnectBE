using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class GradeService : IGradeService
    {
        private readonly IBaseRepository<Grade, int> _gradeRepository;
        private readonly IBaseRepository<Student, string> _studentRepository;
        private readonly IBaseRepository<Lesson, int> _lessonRepository;
        private readonly IBaseRepository<Subject, int> _subjectRepository;
        private readonly IBaseRepository<EduConnect.Domain.Entities.Class, int> _classRepository;
        private readonly ITeacherRepository _teacherRepository;

        public GradeService(
            IBaseRepository<Grade, int> gradeRepository,
            IBaseRepository<Student, string> studentRepository,
            IBaseRepository<Lesson, int> lessonRepository,
            IBaseRepository<Subject, int> subjectRepository,
            IBaseRepository<EduConnect.Domain.Entities.Class, int> classRepository,
            ITeacherRepository teacherRepository)
        {
            _gradeRepository = gradeRepository;
            _studentRepository = studentRepository;
            _lessonRepository = lessonRepository;
            _subjectRepository = subjectRepository;
            _classRepository = classRepository;
            _teacherRepository = teacherRepository;
        }

        // Lấy danh sách điểm theo lesson, chỉ cho phép giáo viên bộ môn của lesson đó
        public async Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectAsync(int subjectId, string teacherId)
        {
            // Kiểm tra xem teacher có quyền dạy subject này không
            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == subjectId && l.TeacherID == teacherId)
                .ToList();
                
            if (!lessons.Any())
            {
                return new ApiResponse<List<GradeResponse>>
                {
                    Success = false,
                    Message = "Bạn không có quyền xem điểm của môn học này"
                };
            }

            var grades = (await _gradeRepository.GetAllAsync())
                .Where(g => g.SubjectID == subjectId && g.IsDeleted != true)
                .ToList();
            var students = await _studentRepository.GetAllAsync();
            var subject = await _subjectRepository.GetByIdAsync(subjectId);
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);

            var result = grades.Select(g => new GradeResponse
            {
                GradeID = g.GradeID,
                StudentID = g.StudentID,
                StudentName = students.FirstOrDefault(s => s.StudentID == g.StudentID)?.FullName ?? "",
                SubjectId = subjectId,
                SubjectName = subject?.SubjectName ?? "",
                TeacherID = teacherId,
                TeacherName = teacher?.FullName ?? "",
                Score = g.Score,
                GradeType = g.GradeType,
                GradeLevel = g.GradeLevel,
                Semester = g.Semester,
                Evaluation = g.Evaluation
            }).ToList();
            return new ApiResponse<List<GradeResponse>>
            {
                Success = true,
                Data = result,
                Message = "Lấy danh sách điểm thành công"
            };
        }

        // Lấy điểm theo ID, kiểm tra quyền
        public async Task<ApiResponse<GradeResponse>> GetGradeByIdAsync(int gradeId, string teacherId)
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId);
            if (grade == null)
                return new ApiResponse<GradeResponse> { Success = false, Message = "Không tìm thấy điểm" };
                
            // Kiểm tra xem teacher có quyền dạy subject này không
            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == grade.SubjectID && l.TeacherID == teacherId)
                .ToList();
                
            if (!lessons.Any())
                return new ApiResponse<GradeResponse> { Success = false, Message = "Bạn không có quyền xem điểm này" };
                
            var student = await _studentRepository.GetByIdAsync(grade.StudentID);
            var subject = await _subjectRepository.GetByIdAsync(grade.SubjectID);
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            var response = new GradeResponse
            {
                GradeID = grade.GradeID,
                StudentID = grade.StudentID,
                StudentName = student?.FullName ?? "",
                SubjectId = grade.SubjectID,
                SubjectName = subject?.SubjectName ?? "",
                TeacherID = teacherId,
                TeacherName = teacher?.FullName ?? "",
                Score = grade.Score,
                GradeType = grade.GradeType,
                GradeLevel = grade.GradeLevel,
                Semester = grade.Semester,
                Evaluation = grade.Evaluation
            };
            return new ApiResponse<GradeResponse> { Success = true, Data = response, Message = "Lấy điểm thành công" };
        }

        // Thêm điểm, kiểm tra quyền
        public async Task<ApiResponse<string>> CreateGradeAsync(CreateGradeRequest request, string teacherId)
        {
            // Kiểm tra xem teacher có quyền dạy subject này không
            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == request.SubjectID && l.TeacherID == teacherId)
                .ToList();
                
            if (!lessons.Any())
                return new ApiResponse<string> { Success = false, Message = "Bạn không có quyền thêm điểm cho môn học này" };
                
            var grade = new Grade
            {
                StudentID = request.StudentID,
                SubjectID = request.SubjectID,
                Score = request.Score,
                GradeType = request.GradeType,
                GradeLevel = request.GradeLevel,
                Semester = request.Semester,
                Evaluation = request.Evaluation
            };
            await _gradeRepository.CreateAsync(grade);
            return new ApiResponse<string> { Success = true, Message = "Thêm điểm thành công" };
        }

        // Sửa điểm, kiểm tra quyền
        public async Task<ApiResponse<string>> UpdateGradeAsync(UpdateGradeRequest request, string teacherId)
        {
            var grade = await _gradeRepository.GetByIdAsync(request.GradeID);
            if (grade == null)
                return new ApiResponse<string> { Success = false, Message = "Không tìm thấy điểm" };
                
            // Kiểm tra xem teacher có quyền dạy subject này không
            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == grade.SubjectID && l.TeacherID == teacherId)
                .ToList();
                
            if (!lessons.Any())
                return new ApiResponse<string> { Success = false, Message = "Bạn không có quyền sửa điểm này" };
                
            grade.Score = request.Score;
            grade.GradeType = request.GradeType;
            grade.GradeLevel = request.GradeLevel;
            grade.Semester = request.Semester;
            grade.Evaluation = request.Evaluation;
            await _gradeRepository.UpdateAsync(grade.GradeID, grade);
            return new ApiResponse<string> { Success = true, Message = "Cập nhật điểm thành công" };
        }

        // Xóa điểm, kiểm tra quyền
        public async Task<ApiResponse<string>> DeleteGradeAsync(int gradeId, string teacherId)
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId);
            if (grade == null)
                return new ApiResponse<string> { Success = false, Message = "Không tìm thấy điểm" };
                
            // Kiểm tra xem teacher có quyền dạy subject này không
            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == grade.SubjectID && l.TeacherID == teacherId)
                .ToList();
                
            if (!lessons.Any())
                return new ApiResponse<string> { Success = false, Message = "Bạn không có quyền xóa điểm này" };
                
            await _gradeRepository.DeleteAsync(gradeId);
            return new ApiResponse<string> { Success = true, Message = "Xóa điểm thành công" };
        }
    }
} 