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
        public async Task<ApiResponse<List<GradeResponse>>> GetGradesByLessonAsync(int lessonId, string teacherId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            Console.WriteLine($"Lesson.TeacherID: {lesson?.TeacherID}, teacherId from token: {teacherId}");
            if (lesson == null || lesson.TeacherID != teacherId)
            {
                return new ApiResponse<List<GradeResponse>>
                {
                    Success = false,
                    Message = "Bạn không có quyền xem điểm của lesson này"
                };
            }
            var grades = (await _gradeRepository.GetAllAsync())
                .Where(g => g.LessonID == lessonId && g.IsDeleted != true)
                .ToList();
            var students = await _studentRepository.GetAllAsync();
            var subject = await _subjectRepository.GetByIdAsync(lesson.SubjectId);
            var classEntity = await _classRepository.GetByIdAsync(lesson.ClassID);
            var teacher = await _teacherRepository.GetByIdAsync(lesson.TeacherID);
            var result = grades.Select(g => new GradeResponse
            {
                GradeID = g.GradeID,
                StudentID = g.StudentID,
                StudentName = students.FirstOrDefault(s => s.StudentID == g.StudentID)?.FullName ?? "",
                LessonID = lesson.LessonID,
                ClassID = lesson.ClassID,
                ClassName = classEntity?.ClassName ?? "",
                SubjectId = lesson.SubjectId,
                SubjectName = subject?.SubjectName ?? "",
                TeacherID = lesson.TeacherID,
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
            var lesson = await _lessonRepository.GetByIdAsync(grade.LessonID);
            if (lesson == null || lesson.TeacherID != teacherId)
                return new ApiResponse<GradeResponse> { Success = false, Message = "Bạn không có quyền xem điểm này" };
            var student = await _studentRepository.GetByIdAsync(grade.StudentID);
            var subject = await _subjectRepository.GetByIdAsync(lesson.SubjectId);
            var classEntity = await _classRepository.GetByIdAsync(lesson.ClassID);
            var teacher = await _teacherRepository.GetByIdAsync(lesson.TeacherID);
            var response = new GradeResponse
            {
                GradeID = grade.GradeID,
                StudentID = grade.StudentID,
                StudentName = student?.FullName ?? "",
                LessonID = lesson.LessonID,
                ClassID = lesson.ClassID,
                ClassName = classEntity?.ClassName ?? "",
                SubjectId = lesson.SubjectId,
                SubjectName = subject?.SubjectName ?? "",
                TeacherID = lesson.TeacherID,
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
            var lesson = await _lessonRepository.GetByIdAsync(request.LessonID);
            if (lesson == null || lesson.TeacherID != teacherId)
                return new ApiResponse<string> { Success = false, Message = "Bạn không có quyền thêm điểm cho lesson này" };
            var grade = new Grade
            {
                StudentID = request.StudentID,
                LessonID = request.LessonID,
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
            var lesson = await _lessonRepository.GetByIdAsync(grade.LessonID);
            if (lesson == null || lesson.TeacherID != teacherId)
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
            var lesson = await _lessonRepository.GetByIdAsync(grade.LessonID);
            if (lesson == null || lesson.TeacherID != teacherId)
                return new ApiResponse<string> { Success = false, Message = "Bạn không có quyền xóa điểm này" };
            await _gradeRepository.DeleteAsync(gradeId);
            return new ApiResponse<string> { Success = true, Message = "Xóa điểm thành công" };
        }

        // Lấy điểm của học sinh cụ thể (chỉ học sinh đó mới được xem)
        public async Task<ApiResponse<List<StudentGradeResponse>>> GetStudentGradesAsync(string studentId, string? semester = null)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return new ApiResponse<List<StudentGradeResponse>> { Success = false, Message = "Không tìm thấy học sinh" };

            var gradesQuery = (await _gradeRepository.GetAllAsync())
                .Where(g => g.StudentID == studentId && g.IsDeleted != true);

            if (!string.IsNullOrEmpty(semester))
            {
                gradesQuery = gradesQuery.Where(g => g.Semester == semester);
            }

            var grades = gradesQuery.ToList();
            var lessons = await _lessonRepository.GetAllAsync();
            var subjects = await _subjectRepository.GetAllAsync();
            var teachers = await _teacherRepository.GetAllAsync();

            var result = grades.Select(g =>
            {
                var lesson = lessons.FirstOrDefault(l => l.LessonID == g.LessonID);
                var subject = subjects.FirstOrDefault(s => s.SubjectId == lesson?.SubjectId);
                var teacher = teachers.FirstOrDefault(t => t.TeacherID == lesson?.TeacherID);

                return new StudentGradeResponse
                {
                    GradeID = g.GradeID,
                    LessonID = g.LessonID,
                    LessonName = subject?.SubjectName ?? "",
                    SubjectId = lesson?.SubjectId ?? 0,
                    SubjectName = subject?.SubjectName ?? "",
                    TeacherID = lesson?.TeacherID ?? "",
                    TeacherName = teacher?.FullName ?? "",
                    Score = g.Score,
                    GradeType = g.GradeType,
                    GradeLevel = g.GradeLevel,
                    Semester = g.Semester,
                    Evaluation = g.Evaluation
                };
            }).ToList();

            return new ApiResponse<List<StudentGradeResponse>>
            {
                Success = true,
                Data = result,
                Message = "Lấy điểm thành công"
            };
        }

        // Lấy tổng quan điểm của học sinh theo học kỳ
        public async Task<ApiResponse<StudentGradeSummaryResponse>> GetStudentGradeSummaryAsync(string studentId, string semester)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return new ApiResponse<StudentGradeSummaryResponse> { Success = false, Message = "Không tìm thấy học sinh" };

            var classEntity = await _classRepository.GetByIdAsync(student.ClassID ?? 0);
            var grades = await GetStudentGradesAsync(studentId, semester);

            if (!grades.Success)
                return new ApiResponse<StudentGradeSummaryResponse> { Success = false, Message = grades.Message };

            var validGrades = grades.Data.Where(g => g.Score.HasValue).ToList();
            var averageScore = validGrades.Any() ? validGrades.Average(g => g.Score.Value) : (double?)null;
            var totalSubjects = grades.Data.Select(g => g.SubjectId).Distinct().Count();

            var summary = new StudentGradeSummaryResponse
            {
                StudentID = student.StudentID,
                StudentName = student.FullName ?? "",
                ClassID = student.ClassID ?? 0,
                ClassName = classEntity?.ClassName ?? "",
                Semester = semester,
                Grades = grades.Data,
                AverageScore = averageScore,
                TotalSubjects = totalSubjects
            };

            return new ApiResponse<StudentGradeSummaryResponse>
            {
                Success = true,
                Data = summary,
                Message = "Lấy tổng quan điểm thành công"
            };
        }

        // Lấy điểm cụ thể của học sinh (kiểm tra quyền)
        public async Task<ApiResponse<StudentGradeResponse>> GetStudentGradeByIdAsync(int gradeId, string studentId)
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId);
            if (grade == null)
                return new ApiResponse<StudentGradeResponse> { Success = false, Message = "Không tìm thấy điểm" };

            // Kiểm tra xem điểm có thuộc về học sinh này không
            if (grade.StudentID != studentId)
                return new ApiResponse<StudentGradeResponse> { Success = false, Message = "Bạn không có quyền xem điểm này" };

            var lesson = await _lessonRepository.GetByIdAsync(grade.LessonID);
            var subject = await _subjectRepository.GetByIdAsync(lesson?.SubjectId ?? 0);
            var teacher = await _teacherRepository.GetByIdAsync(lesson?.TeacherID ?? "");

            var response = new StudentGradeResponse
            {
                GradeID = grade.GradeID,
                LessonID = grade.LessonID,
                LessonName = subject?.SubjectName ?? "",
                SubjectId = lesson?.SubjectId ?? 0,
                SubjectName = subject?.SubjectName ?? "",
                TeacherID = lesson?.TeacherID ?? "",
                TeacherName = teacher?.FullName ?? "",
                Score = grade.Score,
                GradeType = grade.GradeType,
                GradeLevel = grade.GradeLevel,
                Semester = grade.Semester,
                Evaluation = grade.Evaluation
            };

            return new ApiResponse<StudentGradeResponse>
            {
                Success = true,
                Data = response,
                Message = "Lấy điểm thành công"
            };
        }
    }
} 