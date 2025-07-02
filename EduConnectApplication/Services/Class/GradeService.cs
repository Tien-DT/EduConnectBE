
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
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

        private async Task<bool> CanTeacherAccessSubjectAsync(string teacherAccountId, int subjectId, int? classId = null)
        {
            var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
            if (teacher == null) return false;

            var hasPermission = (await _lessonRepository.GetAllAsync())
                .Any(l => l.TeacherID == teacher.TeacherID &&
                         l.SubjectId == subjectId &&
                         (classId == null || l.ClassID == classId));

            return hasPermission;
        }

        public async Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectForTeacherAsync(int subjectId, string teacherAccountId, int? classId = null)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<List<GradeResponse>>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var hasPermission = await CanTeacherAccessSubjectAsync(teacherAccountId, subjectId, classId);
                if (!hasPermission)
                {
                    return ApiResponse<List<GradeResponse>>.ErrorResponse(
                        new List<string> { "You are not authorized to view grades for this subject" },
                        "Unauthorized Access");
                }

                var teacherLessons = (await _lessonRepository.GetAllAsync())
                    .Where(l => l.TeacherID == teacher.TeacherID &&
                               l.SubjectId == subjectId &&
                               (classId == null || l.ClassID == classId))
                    .ToList();

                var authorizedClassIds = teacherLessons.Select(l => l.ClassID).Distinct().ToList();

                var students = (await _studentRepository.GetAllAsync())
                    .Where(s => s.ClassID.HasValue && authorizedClassIds.Contains(s.ClassID.Value))
                    .ToList();

                var studentIds = students.Select(s => s.StudentID).ToList();

                var grades = (await _gradeRepository.GetAllAsync())
                    .Where(g => g.SubjectID == subjectId &&
                               studentIds.Contains(g.StudentID) &&
                               g.IsDeleted != true)
                    .ToList();

                var subject = await _subjectRepository.GetByIdAsync(subjectId);
                var gradeResponses = new List<GradeResponse>();

                foreach (var grade in grades)
                {
                    var student = students.FirstOrDefault(s => s.StudentID == grade.StudentID);
                    var studentClass = student?.ClassID.HasValue == true ?
                        await _classRepository.GetByIdAsync(student.ClassID.Value) : null;

                    gradeResponses.Add(new GradeResponse
                    {
                        GradeID = grade.GradeID,
                        StudentID = grade.StudentID,
                        StudentName = student?.FullName ?? "Unknown",
                        SubjectID = grade.SubjectID,
                        SubjectName = subject?.SubjectName ?? "Unknown",
                        ClassID = student?.ClassID ?? 0,
                        ClassName = studentClass?.ClassName ?? "No Class",
                        Score = grade.Score,
                        GradeType = grade.GradeType,
                        GradeLevel = grade.GradeLevel,
                        Semester = grade.Semester,
                        Evaluation = grade.Evaluation
                    });
                }

                return ApiResponse<List<GradeResponse>>.SuccessResponse(
                    gradeResponses.OrderBy(g => g.StudentName).ToList(),
                    "Grades retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<GradeResponse>>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        public async Task<ApiResponse<List<GradeResponse>>> GetStudentGradesBySubjectForTeacherAsync(int subjectId, string studentId, string teacherAccountId)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<List<GradeResponse>>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    return ApiResponse<List<GradeResponse>>.ErrorResponse(
                        new List<string> { "Student not found" }, "Student Not Found");
                }

                var hasPermission = (await _lessonRepository.GetAllAsync())
                    .Any(l => l.TeacherID == teacher.TeacherID &&
                             l.SubjectId == subjectId &&
                             l.ClassID == student.ClassID);

                if (!hasPermission)
                {
                    return ApiResponse<List<GradeResponse>>.ErrorResponse(
                        new List<string> { "You are not authorized to view this student's grades" },
                        "Unauthorized Access");
                }

                var grades = (await _gradeRepository.GetAllAsync())
                    .Where(g => g.StudentID == studentId &&
                               g.SubjectID == subjectId &&
                               g.IsDeleted != true)
                    .ToList();

                var subject = await _subjectRepository.GetByIdAsync(subjectId);
                var studentClass = student.ClassID.HasValue ?
                    await _classRepository.GetByIdAsync(student.ClassID.Value) : null;

                var gradeResponses = grades.Select(g => new GradeResponse
                {
                    GradeID = g.GradeID,
                    StudentID = g.StudentID,
                    StudentName = student.FullName ?? "Unknown",
                    SubjectID = g.SubjectID,
                    SubjectName = subject?.SubjectName ?? "Unknown",
                    ClassID = student.ClassID ?? 0,
                    ClassName = studentClass?.ClassName ?? "No Class",
                    Score = g.Score,
                    GradeType = g.GradeType,
                    GradeLevel = g.GradeLevel,
                    Semester = g.Semester,
                    Evaluation = g.Evaluation
                }).ToList();

                return ApiResponse<List<GradeResponse>>.SuccessResponse(
                    gradeResponses, "Student grades retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<GradeResponse>>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        public async Task<ApiResponse<GradeResponse>> GetGradeByIdForTeacherAsync(int gradeId, string teacherAccountId)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var grade = await _gradeRepository.GetByIdAsync(gradeId);
                if (grade == null || grade.IsDeleted == true)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Grade not found" }, "Grade Not Found");
                }

                var student = await _studentRepository.GetByIdAsync(grade.StudentID);
                if (student == null)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Student not found" }, "Student Not Found");
                }

                var hasPermission = (await _lessonRepository.GetAllAsync())
                    .Any(l => l.TeacherID == teacher.TeacherID &&
                             l.SubjectId == grade.SubjectID &&
                             l.ClassID == student.ClassID);

                if (!hasPermission)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "You are not authorized to view this grade" },
                        "Unauthorized Access");
                }

                var subject = await _subjectRepository.GetByIdAsync(grade.SubjectID);
                var studentClass = student.ClassID.HasValue ?
                    await _classRepository.GetByIdAsync(student.ClassID.Value) : null;

                var gradeResponse = new GradeResponse
                {
                    GradeID = grade.GradeID,
                    StudentID = grade.StudentID,
                    StudentName = student.FullName ?? "Unknown",
                    SubjectID = grade.SubjectID,
                    SubjectName = subject?.SubjectName ?? "Unknown",
                    ClassID = student.ClassID ?? 0,
                    ClassName = studentClass?.ClassName ?? "No Class",
                    Score = grade.Score,
                    GradeType = grade.GradeType,
                    GradeLevel = grade.GradeLevel,
                    Semester = grade.Semester,
                    Evaluation = grade.Evaluation
                };

                return ApiResponse<GradeResponse>.SuccessResponse(gradeResponse, "Grade retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<GradeResponse>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        public async Task<ApiResponse<List<StudentGradingResponse>>> GetStudentsForGradingAsync(int subjectId, string teacherAccountId, int? classId = null)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<List<StudentGradingResponse>>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var hasPermission = await CanTeacherAccessSubjectAsync(teacherAccountId, subjectId, classId);
                if (!hasPermission)
                {
                    return ApiResponse<List<StudentGradingResponse>>.ErrorResponse(
                        new List<string> { "You are not authorized to grade this subject" },
                        "Unauthorized Access");
                }

                var teacherLessons = (await _lessonRepository.GetAllAsync())
                    .Where(l => l.TeacherID == teacher.TeacherID &&
                               l.SubjectId == subjectId &&
                               (classId == null || l.ClassID == classId))
                    .ToList();

                var authorizedClassIds = teacherLessons.Select(l => l.ClassID).Distinct().ToList();

                var students = (await _studentRepository.GetAllAsync())
                    .Where(s => s.ClassID.HasValue && authorizedClassIds.Contains(s.ClassID.Value))
                    .ToList();

                var studentIds = students.Select(s => s.StudentID).ToList();
                var grades = (await _gradeRepository.GetAllAsync())
                    .Where(g => g.SubjectID == subjectId &&
                               studentIds.Contains(g.StudentID) &&
                               g.IsDeleted != true)
                    .ToList();

                var subject = await _subjectRepository.GetByIdAsync(subjectId);
                var response = new List<StudentGradingResponse>();

                foreach (var student in students)
                {
                    var studentClass = student.ClassID.HasValue ?
                        await _classRepository.GetByIdAsync(student.ClassID.Value) : null;

                    var studentGrades = grades.Where(g => g.StudentID == student.StudentID).ToList();

                    response.Add(new StudentGradingResponse
                    {
                        StudentID = student.StudentID,
                        FullName = student.FullName ?? "Unknown",
                        ClassID = student.ClassID ?? 0,
                        ClassName = studentClass?.ClassName ?? "No Class",
                        DateOfBirth = student.DateOfBirth,
                        Gender = student.Gender,
                        ExistingGrades = studentGrades.Select(g => new GradeResponse
                        {
                            GradeID = g.GradeID,
                            StudentID = g.StudentID,
                            StudentName = student.FullName ?? "Unknown",
                            SubjectID = g.SubjectID,
                            SubjectName = subject?.SubjectName ?? "Unknown",
                            ClassID = student.ClassID ?? 0,
                            ClassName = studentClass?.ClassName ?? "No Class",
                            Score = g.Score,
                            GradeType = g.GradeType,
                            GradeLevel = g.GradeLevel,
                            Semester = g.Semester,
                            Evaluation = g.Evaluation
                        }).ToList()
                    });
                }

                return ApiResponse<List<StudentGradingResponse>>.SuccessResponse(
                    response.OrderBy(s => s.FullName).ToList(),
                    "Students for grading retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<StudentGradingResponse>>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        public async Task<ApiResponse<GradeResponse>> CreateGradeForTeacherAsync(CreateGradeRequest request, string teacherAccountId)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var student = await _studentRepository.GetByIdAsync(request.StudentID);
                if (student == null)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Student not found" }, "Student Not Found");
                }

                var hasPermission = (await _lessonRepository.GetAllAsync())
                    .Any(l => l.TeacherID == teacher.TeacherID &&
                             l.SubjectId == request.SubjectID &&
                             l.ClassID == student.ClassID);

                if (!hasPermission)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "You are not authorized to create grades for this subject/student" },
                        "Unauthorized Access");
                }

                var grade = new Grade
                {
                    StudentID = request.StudentID,
                    SubjectID = request.SubjectID,
                    Score = request.Score,
                    GradeType = request.GradeType,
                    GradeLevel = request.GradeLevel,
                    Semester = request.Semester,
                    Evaluation = request.Evaluation,
                    IsDeleted = false
                };

                await _gradeRepository.CreateAsync(grade);

                var subject = await _subjectRepository.GetByIdAsync(request.SubjectID);
                var studentClass = student.ClassID.HasValue ?
                    await _classRepository.GetByIdAsync(student.ClassID.Value) : null;

                var gradeResponse = new GradeResponse
                {
                    GradeID = grade.GradeID,
                    StudentID = grade.StudentID,
                    StudentName = student.FullName ?? "Unknown",
                    SubjectID = grade.SubjectID,
                    SubjectName = subject?.SubjectName ?? "Unknown",
                    ClassID = student.ClassID ?? 0,
                    ClassName = studentClass?.ClassName ?? "No Class",
                    Score = grade.Score,
                    GradeType = grade.GradeType,
                    GradeLevel = grade.GradeLevel,
                    Semester = grade.Semester,
                    Evaluation = grade.Evaluation
                };

                return ApiResponse<GradeResponse>.SuccessResponse(gradeResponse, "Grade created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<GradeResponse>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        public async Task<ApiResponse<GradeResponse>> UpdateGradeForTeacherAsync(int gradeId, UpdateGradeRequest request, string teacherAccountId)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var grade = await _gradeRepository.GetByIdAsync(gradeId);
                if (grade == null || grade.IsDeleted == true)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Grade not found" }, "Grade Not Found");
                }

                var student = await _studentRepository.GetByIdAsync(grade.StudentID);
                if (student == null)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "Student not found" }, "Student Not Found");
                }

                var hasPermission = (await _lessonRepository.GetAllAsync())
                    .Any(l => l.TeacherID == teacher.TeacherID &&
                             l.SubjectId == grade.SubjectID &&
                             l.ClassID == student.ClassID);

                if (!hasPermission)
                {
                    return ApiResponse<GradeResponse>.ErrorResponse(
                        new List<string> { "You are not authorized to update this grade" },
                        "Unauthorized Access");
                }

                grade.Score = request.Score;
                grade.GradeType = request.GradeType;
                grade.GradeLevel = request.GradeLevel;
                grade.Semester = request.Semester;
                grade.Evaluation = request.Evaluation;

                await _gradeRepository.UpdateAsync(gradeId, grade);

                var subject = await _subjectRepository.GetByIdAsync(grade.SubjectID);
                var studentClass = student.ClassID.HasValue ?
                    await _classRepository.GetByIdAsync(student.ClassID.Value) : null;

                var gradeResponse = new GradeResponse
                {
                    GradeID = grade.GradeID,
                    StudentID = grade.StudentID,
                    StudentName = student.FullName ?? "Unknown",
                    SubjectID = grade.SubjectID,
                    SubjectName = subject?.SubjectName ?? "Unknown",
                    ClassID = student.ClassID ?? 0,
                    ClassName = studentClass?.ClassName ?? "No Class",
                    Score = grade.Score,
                    GradeType = grade.GradeType,
                    GradeLevel = grade.GradeLevel,
                    Semester = grade.Semester,
                    Evaluation = grade.Evaluation
                };

                return ApiResponse<GradeResponse>.SuccessResponse(gradeResponse, "Grade updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<GradeResponse>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        public async Task<ApiResponse<string>> DeleteGradeForTeacherAsync(int gradeId, string teacherAccountId)
        {
            try
            {
                var teacher = await _teacherRepository.GetByAccountIdAsync(teacherAccountId);
                if (teacher == null)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Teacher not found" }, "Teacher Not Found");
                }

                var grade = await _gradeRepository.GetByIdAsync(gradeId);
                if (grade == null || grade.IsDeleted == true)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Grade not found" }, "Grade Not Found");
                }

                var student = await _studentRepository.GetByIdAsync(grade.StudentID);
                if (student == null)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Student not found" }, "Student Not Found");
                }

                var hasPermission = (await _lessonRepository.GetAllAsync())
                    .Any(l => l.TeacherID == teacher.TeacherID &&
                             l.SubjectId == grade.SubjectID &&
                             l.ClassID == student.ClassID);

                if (!hasPermission)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "You are not authorized to delete this grade" },
                        "Unauthorized Access");
                }

                grade.IsDeleted = true;
                await _gradeRepository.UpdateAsync(gradeId, grade);

                return ApiResponse<string>.SuccessResponse("", "Grade deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse(
                    new List<string> { $"An error occurred: {ex.Message}" },
                    "Internal Server Error");
            }
        }

        #region Old Methods (Deprecated - for backward compatibility)

        public async Task<ApiResponse<List<GradeResponse>>> GetGradesBySubjectAsync(int subjectId, string teacherId)
        {
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                return new ApiResponse<List<GradeResponse>>
                {
                    Success = false,
                    Message = "Teacher not found"
                };
            }

            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == subjectId && l.TeacherID == teacherId)
                .ToList();

            if (!lessons.Any())
            {
                return new ApiResponse<List<GradeResponse>>
                {
                    Success = false,
                    Message = "You are not authorized to view grades for this subject"
                };
            }

            var grades = (await _gradeRepository.GetAllAsync())
                .Where(g => g.SubjectID == subjectId && g.IsDeleted != true)
                .ToList();

            var students = await _studentRepository.GetAllAsync();
            var subject = await _subjectRepository.GetByIdAsync(subjectId);

            var result = grades.Select(g => new GradeResponse
            {
                GradeID = g.GradeID,
                StudentID = g.StudentID,
                StudentName = students.FirstOrDefault(s => s.StudentID == g.StudentID)?.FullName ?? "",
                SubjectID = subjectId,
                SubjectName = subject?.SubjectName ?? "",
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
                Message = "Grades retrieved successfully"
            };
        }

        public async Task<ApiResponse<GradeResponse>> GetGradeByIdAsync(int gradeId, string teacherId)
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId);
            if (grade == null)
                return new ApiResponse<GradeResponse> { Success = false, Message = "Grade not found" };

            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == grade.SubjectID && l.TeacherID == teacherId)
                .ToList();

            if (!lessons.Any())
                return new ApiResponse<GradeResponse> { Success = false, Message = "You are not authorized to view this grade" };

            var student = await _studentRepository.GetByIdAsync(grade.StudentID);
            var subject = await _subjectRepository.GetByIdAsync(grade.SubjectID);

            var response = new GradeResponse
            {
                GradeID = grade.GradeID,
                StudentID = grade.StudentID,
                StudentName = student?.FullName ?? "",
                SubjectID = grade.SubjectID,
                SubjectName = subject?.SubjectName ?? "",
                Score = grade.Score,
                GradeType = grade.GradeType,
                GradeLevel = grade.GradeLevel,
                Semester = grade.Semester,
                Evaluation = grade.Evaluation
            };

            return new ApiResponse<GradeResponse> { Success = true, Data = response, Message = "Grade retrieved successfully" };
        }

        public async Task<ApiResponse<string>> CreateGradeAsync(CreateGradeRequest request, string teacherId)
        {
            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == request.SubjectID && l.TeacherID == teacherId)
                .ToList();

            if (!lessons.Any())
                return new ApiResponse<string> { Success = false, Message = "You are not authorized to create grades for this subject" };

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
            return new ApiResponse<string> { Success = true, Message = "Grade created successfully" };
        }

        public async Task<ApiResponse<string>> UpdateGradeAsync(int gradeId, UpdateGradeRequest request, string teacherId)
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId);
            if (grade == null)
                return new ApiResponse<string> { Success = false, Message = "Grade not found" };

            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == grade.SubjectID && l.TeacherID == teacherId)
                .ToList();

            if (!lessons.Any())
                return new ApiResponse<string> { Success = false, Message = "You are not authorized to update this grade" };

            grade.Score = request.Score;
            grade.GradeType = request.GradeType;
            grade.GradeLevel = request.GradeLevel;
            grade.Semester = request.Semester;
            grade.Evaluation = request.Evaluation;

            await _gradeRepository.UpdateAsync(gradeId, grade);
            return new ApiResponse<string> { Success = true, Message = "Grade updated successfully" };
        }

        public async Task<ApiResponse<string>> DeleteGradeAsync(int gradeId, string teacherId)
        {
            var grade = await _gradeRepository.GetByIdAsync(gradeId);
            if (grade == null)
                return new ApiResponse<string> { Success = false, Message = "Grade not found" };

            var lessons = (await _lessonRepository.GetAllAsync())
                .Where(l => l.SubjectId == grade.SubjectID && l.TeacherID == teacherId)
                .ToList();

            if (!lessons.Any())
                return new ApiResponse<string> { Success = false, Message = "You are not authorized to delete this grade" };

            grade.IsDeleted = true;
            await _gradeRepository.UpdateAsync(gradeId, grade);
            return new ApiResponse<string> { Success = true, Message = "Grade deleted successfully" };
        }

        #endregion
    }
}