using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IMapper _mapper;
        private readonly IClassTeacherRepository _classTeacherRepository;
        private readonly IPrincipalRepository _principalRepository;
        private readonly IStudentRepository _studentRepository;

        public ExamService(
            IExamRepository examRepository,
            IMapper mapper,
            IClassTeacherRepository classTeacherRepository,
            IPrincipalRepository principalRepository,
            IStudentRepository studentRepository)
        {
            _examRepository = examRepository;
            _mapper = mapper;
            _classTeacherRepository = classTeacherRepository;
            _principalRepository = principalRepository;
            _studentRepository = studentRepository;
        }

        public async Task<ApiResponse<ExamResponse>> GetExamByIdAsync(int examId)
        {
            try
            {
                var exam = await _examRepository.GetExamWithSubjectByIdAsync(examId);
                
                if (exam == null)
                {
                    return ApiResponse<ExamResponse>.ErrorResponse(
                        new List<string> { "Exam not found." },
                        "Exam not found");
                }

                var response = _mapper.Map<ExamResponse>(exam);
                
                return ApiResponse<ExamResponse>.SuccessResponse(
                    response,
                    "Exam retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExamResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving exam");
            }
        }

        public async Task<ApiResponse<ExamResponse>> CreateExamAsync(CreateExamRequest request, string? createdBy = null)
        {
            try
            {
                var exam = new Exam
                {
                    SubjectId = request.SubjectId,
                    ExamType = request.ExamType,
                    ExamNote = request.ExamNote,
                    ExamDate = request.ExamDate,
                    GradeDate = request.GradeDate,
                    Room = request.Room,
                    StudentID = request.StudentId,
                    IsDeleted = false,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow
                };

                await _examRepository.CreateExamAsync(exam);
                
                // Get the exam with Subject information to populate SubjectName and SubjectCode
                var examWithSubject = await _examRepository.GetExamWithSubjectByIdAsync(exam.ExamId);
                
                var response = _mapper.Map<ExamResponse>(examWithSubject);
                
                return ApiResponse<ExamResponse>.SuccessResponse(
                    response,
                    "Exam created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExamResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi khi tạo bài kiểm tra");
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> CreateBulkExamsAsync(List<CreateExamRequest> requests, string? createdBy = null)
        {
            try
            {
                var createdExams = new List<ExamResponse>();
                var errors = new List<string>();

                foreach (var request in requests)
                {
                    var result = await CreateExamAsync(request, createdBy);
                    
                    if (result.Success)
                    {
                        createdExams.Add(result.Data);
                    }
                    else
                    {
                        errors.AddRange(result.Error);
                    }
                }

                if (errors.Any())
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        errors,
                        "Some exams could not be created");
                }

                return ApiResponse<List<ExamResponse>>.SuccessResponse(
                    createdExams,
                    "All exams created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi khi tạo các bài kiểm tra");
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> CreateClassExamsAsync(CreateClassExamRequest request, string? createdBy = null)
        {
            try
            {
                var isValidTeacher = await _classTeacherRepository.IsTeacherAssignedToClassAsync(request.ClassId, request.TeacherId);

                if (!isValidTeacher)
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { "Teacher is not assigned to this class." },
                        "Unauthorized access");
                }

                var students = await _studentRepository.GetStudentsByClassIdAsync(request.ClassId);

                if (students == null || !students.Any())
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { "No active students found in this class." },
                        "No students found");
                }
                var examRequests = students.Select(s => new CreateExamRequest
                {
                    SubjectId = request.SubjectId,
                    ExamType = request.ExamType,
                    ExamNote = request.ExamNote,
                    ExamDate = request.ExamDate,
                    GradeDate = request.GradeDate,
                    Room = request.Room,
                    StudentId = s.StudentID
                }).ToList();
                return await CreateBulkExamsAsync(examRequests, createdBy);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi khi tạo bài kiểm tra cho lớp học");
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> CreatePrincipalExamsAsync(CreatePrincipalExamRequest request, string? createdBy = null)
        {
            try
            {
                var isPrincipal = await _principalRepository.IsValidPrincipalAsync(request.PrincipalId);

                if (!isPrincipal)
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { "Only principals can create exams for multiple students." },
                        "Unauthorized access");
                }
                var validStudents = await _studentRepository.GetStudentsByIdsAsync(request.StudentIds);

                if (validStudents == null || !validStudents.Any())
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { "No active students found with the provided IDs." },
                        "No students found");
                }

                var foundStudentIds = validStudents.Select(s => s.StudentID).ToList();
                var invalidStudentIds = request.StudentIds.Except(foundStudentIds).ToList();
                
                if (invalidStudentIds.Any())
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { $"The following student IDs were not found or are inactive: {string.Join(", ", invalidStudentIds)}" },
                        "Invalid student IDs");
                }
                var examRequests = validStudents.Select(s => new CreateExamRequest
                {
                    SubjectId = request.SubjectId,
                    ExamType = request.ExamType,
                    ExamNote = request.ExamNote,
                    ExamDate = request.ExamDate,
                    GradeDate = request.GradeDate,
                    Room = request.Room,
                    StudentId = s.StudentID
                }).ToList();
                return await CreateBulkExamsAsync(examRequests, createdBy);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Lỗi khi tạo bài kiểm tra cho học sinh");
            }
        }

        public async Task<PagedApiResponse<List<ExamResponse>>> GetExamsByAccountIdAsync(
            string accountId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null,
            string? searchTerm = null)
        {
            try
            {
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    var (exams, totalCount) = await _examRepository.GetPagedExamsByAccountIdAsync(
                        accountId,
                        pageIndex.Value,
                        pageSize.Value,
                        startDate,
                        endDate,
                        ascending);

                    if (exams == null || !exams.Any())
                    {
                        return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                            new List<ExamResponse>(),
                            "Không có lịch thi trong thời gian này",
                            0,
                            pageIndex.Value,
                            pageSize.Value
                        );
                    }

                    var filteredExams = string.IsNullOrWhiteSpace(searchTerm)
                        ? exams
                        : ExamQueryHelper.ApplySearch(exams.AsQueryable(), searchTerm).ToList();

                    var examResponses = _mapper.Map<List<ExamResponse>>(filteredExams);
                    return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                        examResponses,
                        "Exams retrieved successfully",
                        filteredExams.Count,
                        pageIndex.Value,
                        pageSize.Value
                    );
                }

                var allExams = await _examRepository.GetExamsByAccountIdAsync(accountId, startDate, endDate, ascending);
                if (allExams == null || !allExams.Any())
                {
                    return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                        new List<ExamResponse>(),
                        "Không có lịch thi trong thời gian này",
                        0,
                        1,
                        0
                    );
                }

                var allFilteredExams = string.IsNullOrWhiteSpace(searchTerm)
                    ? allExams
                    : ExamQueryHelper.ApplySearch(allExams.AsQueryable(), searchTerm).ToList();

                var allExamResponses = _mapper.Map<List<ExamResponse>>(allFilteredExams);
                return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                    allExamResponses,
                    "Exams retrieved successfully",
                    allFilteredExams.Count,
                    1,
                    allFilteredExams.Count
                );
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<ExamResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving exams",
                    pageIndex ?? 0,
                    pageSize ?? 0
                );
            }
        }

        public async Task<PagedApiResponse<List<ExamResponse>>> GetExamsByStudentIdAsync(
            string studentId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null,
            string? searchTerm = null)
        {
            try
            {
                if (pageIndex.HasValue && pageSize.HasValue)
                {
                    var (exams, totalCount) = await _examRepository.GetPagedExamByStudentIdAsync(
                        studentId,
                        pageIndex.Value,
                        pageSize.Value,
                        startDate,
                        endDate,
                        ascending);

                    if (exams == null || !exams.Any())
                    {
                        return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                            new List<ExamResponse>(),
                            "Không có lịch thi trong thời gian này",
                            0,
                            pageIndex.Value,
                            pageSize.Value
                        );
                    }

                    var filteredExams = string.IsNullOrWhiteSpace(searchTerm)
                        ? exams
                        : ExamQueryHelper.ApplySearch(exams.AsQueryable(), searchTerm).ToList();

                    var examResponses = _mapper.Map<List<ExamResponse>>(filteredExams);
                    return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                        examResponses,
                        "Exams retrieved successfully",
                        filteredExams.Count,
                        pageIndex.Value,
                        pageSize.Value
                    );
                }
                var allExams = await _examRepository.GetExamByStudentIdAsync(studentId, startDate, endDate, ascending);
                if (allExams == null || !allExams.Any())
                {
                    return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                        new List<ExamResponse>(),
                        "Không có lịch thi trong thời gian này",
                        0,
                        1,
                        0
                    );
                }

                var allFilteredExams = string.IsNullOrWhiteSpace(searchTerm)
                    ? allExams
                    : ExamQueryHelper.ApplySearch(allExams.AsQueryable(), searchTerm).ToList();

                var allExamResponses = _mapper.Map<List<ExamResponse>>(allFilteredExams);
                return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                    allExamResponses,
                    "Exams retrieved successfully",
                    allFilteredExams.Count,
                    1,
                    allFilteredExams.Count
                );
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<ExamResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error retrieving exams",
                    pageIndex ?? 0,
                    pageSize ?? 0
                );
            }
        }

        public async Task<PagedApiResponse<List<ExamResponse>>> FilterSortSearchExamsAsync(
            string? studentId = null,
            string? searchTerm = null,
            string? room = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 1,
            int pageSize = 10)
        {
            var query = _examRepository.GetExamQueryable();

            if (!string.IsNullOrWhiteSpace(studentId))
            {
                query = query.Where(e => e.StudentID == studentId && e.IsDeleted != true);
            }
            else
            {
                query = query.Where(e => e.IsDeleted != true);
            }

            query = ExamQueryHelper.ApplySearch(query, searchTerm);
            query = ExamQueryHelper.ApplyRoomFilter(query, room);
            query = ExamQueryHelper.ApplySort(query, sortBy, ascending);

            var totalCount = await System.Threading.Tasks.Task.FromResult(query.Count());
            var items = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var examResponses = _mapper.Map<List<ExamResponse>>(items);

            return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                examResponses,
                "Exams filtered, sorted, and searched successfully",
                totalCount,
                pageIndex,
                pageSize
            );
        }

        public async Task<ApiResponse<ExamResponse>> UpdateExamAsync(int examId, UpdateExamRequest request, string? updatedBy = null)
        {
            try
            {
                var existingExam = await _examRepository.GetExamWithSubjectByIdAsync(examId);
                if (existingExam == null)
                {
                    return ApiResponse<ExamResponse>.ErrorResponse(
                        new List<string> { "Exam not found." },
                        "Exam not found");
                }

                // Update exam properties
                existingExam.SubjectId = request.SubjectId;
                existingExam.ExamType = request.ExamType;
                existingExam.ExamNote = request.ExamNote;
                existingExam.ExamDate = request.ExamDate;
                existingExam.GradeDate = request.GradeDate;
                existingExam.Room = request.Room;

                await _examRepository.UpdateExamAsync(existingExam);

                // Get updated exam with subject for response
                var updatedExam = await _examRepository.GetExamWithSubjectByIdAsync(examId);
                var response = _mapper.Map<ExamResponse>(updatedExam);

                return ApiResponse<ExamResponse>.SuccessResponse(response, "Exam updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExamResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error updating exam");
            }
        }

        public async Task<ApiResponse<string>> DeleteExamAsync(int examId)
        {
            try
            {
                var exam = await _examRepository.GetExamWithSubjectByIdAsync(examId);
                if (exam == null)
                {
                    return ApiResponse<string>.ErrorResponse(
                        new List<string> { "Exam not found." },
                        "Exam not found");
                }

                await _examRepository.DeleteExamAsync(examId);
                
                return ApiResponse<string>.SuccessResponse("Exam deleted successfully", "Exam deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error deleting exam");
            }
        }
    }
}