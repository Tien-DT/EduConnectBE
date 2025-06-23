using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Data;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Class
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IMapper _mapper;
        private readonly EduConnectDbContext _dbContext;

        public ExamService(IExamRepository examRepository, IMapper mapper, EduConnectDbContext dbContext)
        {
            _examRepository = examRepository;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<ExamResponse>> CreateExamAsync(CreateExamRequest request)
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
                    StudentID = request.StudentID,
                    IsDeleted = false
                };

                await _examRepository.CreateExamAsync(exam);
                
                var response = _mapper.Map<ExamResponse>(exam);
                
                return ApiResponse<ExamResponse>.SuccessResponse(
                    response,
                    "Exam created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ExamResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error creating exam");
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> CreateBulkExamsAsync(List<CreateExamRequest> requests)
        {
            try
            {
                var createdExams = new List<ExamResponse>();
                var errors = new List<string>();

                foreach (var request in requests)
                {
                    var result = await CreateExamAsync(request);
                    
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
                    "Error creating exams");
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> CreateClassExamsAsync(CreateClassExamRequest request)
        {
            try
            {
                var isValidTeacher = await _dbContext.ClassTeachers
                    .AnyAsync(ct => ct.ClassID == request.ClassId && 
                                    ct.Teacher.AccountID == request.TeacherAccountId);

                if (!isValidTeacher)
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { "Teacher is not assigned to this class." },
                        "Unauthorized access");
                }

                var students = await _dbContext.Students
                    .Where(s => s.ClassID == request.ClassId && 
                               (s.IsDeleted == null || s.IsDeleted == false))
                    .ToListAsync();

                if (!students.Any())
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
                    StudentID = s.StudentID
                }).ToList();
                return await CreateBulkExamsAsync(examRequests);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error creating class exams");
            }
        }

        public async Task<ApiResponse<List<ExamResponse>>> CreatePrincipalExamsAsync(CreatePrincipalExamRequest request)
        {
            try
            {
                var isPrincipal = await _dbContext.Principals
                    .AnyAsync(p => p.AccountID == request.PrincipalAccountId && 
                                  (p.IsDeleted == null || p.IsDeleted == false));

                if (!isPrincipal)
                {
                    return ApiResponse<List<ExamResponse>>.ErrorResponse(
                        new List<string> { "Only principals can create exams for multiple students." },
                        "Unauthorized access");
                }
                var validStudents = await _dbContext.Students
                    .Where(s => request.StudentIds.Contains(s.StudentID) && 
                               (s.IsDeleted == null || s.IsDeleted == false))
                    .ToListAsync();

                if (!validStudents.Any())
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
                    StudentID = s.StudentID
                }).ToList();
                return await CreateBulkExamsAsync(examRequests);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ExamResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error creating exams for students");
            }
        }

        public async Task<PagedApiResponse<List<ExamResponse>>> GetExamsByAccountIdAsync(
            string accountId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            bool ascending = false,
            int? pageIndex = null,
            int? pageSize = null)
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

                    var examResponses = _mapper.Map<List<ExamResponse>>(exams);
                    return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                        examResponses,
                        "Exams retrieved successfully",
                        totalCount,
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

                var allExamResponses = _mapper.Map<List<ExamResponse>>(allExams);
                return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                    allExamResponses,
                    "Exams retrieved successfully",
                    allExamResponses.Count,
                    1,
                    allExamResponses.Count
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
            int? pageSize = null)
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

                    var examResponses = _mapper.Map<List<ExamResponse>>(exams);
                    return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                        examResponses,
                        "Exams retrieved successfully",
                        totalCount,
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

                var allExamResponses = _mapper.Map<List<ExamResponse>>(allExams);
                return PagedApiResponse<List<ExamResponse>>.SuccessPagedResponse(
                    allExamResponses,
                    "Exams retrieved successfully",
                    allExamResponses.Count,
                    1,
                    allExamResponses.Count
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
    }
}