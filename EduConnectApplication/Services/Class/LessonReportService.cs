using AutoMapper;
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
    public class LessonReportService : ILessonReportService
    {
        private readonly ILessonReportRepository _lessonReportRepository;
        private readonly IMapper _mapper;

        public LessonReportService(ILessonReportRepository lessonReportRepository, IMapper mapper)
        {
            _lessonReportRepository = lessonReportRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> GetAllLessonReportsAsync()
        {
            try
            {
                var reports = await _lessonReportRepository.GetAllWithDetailsAsync();
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Lesson reports retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<LessonReportResponse>> GetLessonReportByIdAsync(int reportId)
        {
            try
            {
                var report = await _lessonReportRepository.GetWithDetailsByIdAsync(reportId);
                if (report == null)
                {
                    return new ApiResponse<LessonReportResponse>
                    {
                        Success = false,
                        Message = "Lesson report not found"
                    };
                }

                var response = _mapper.Map<LessonReportResponse>(report);
                return new ApiResponse<LessonReportResponse>
                {
                    Success = true,
                    Data = response,
                    Message = "Lesson report retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LessonReportResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reports = await _lessonReportRepository.GetByDateRangeAsync(startDate, endDate);
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Lesson reports retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByTeacherAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var reports = await _lessonReportRepository.GetByTeacherIdAsync(teacherId, startDate, endDate);
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Lesson reports retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByClassAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var reports = await _lessonReportRepository.GetByClassIdAsync(classId, startDate, endDate);
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Lesson reports retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> GetLessonReportsByTimetableSlotAsync(int timetableSlotId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var reports = await _lessonReportRepository.GetByTimetableSlotIdAsync(timetableSlotId, startDate, endDate);
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Lesson reports retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetPagedLessonReportsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? teacherId = null,
            int? classId = null)
        {
            try
            {
                var (reports, totalCount) = await _lessonReportRepository.GetPagedWithDetailsAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, teacherId, classId);
                
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                
                return new ApiResponse<PagedResponse<LessonReportResponse>>
                {
                    Success = true,
                    Data = new PagedResponse<LessonReportResponse>
                    {
                        Items = response,
                        TotalCount = totalCount,
                        PageIndex = pageIndex,
                        PageSize = pageSize
                    },
                    Message = "Lesson reports retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PagedResponse<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> CreateLessonReportAsync(CreateLessonReportRequest request, string createdBy)
        {
            try
            {
                var exists = await _lessonReportRepository.ExistsForTimetableSlotAndDateAsync(request.TimetableSlotID, request.LessonDate);
                if (exists)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "A lesson report already exists for this timetable slot and date"
                    };
                }

                var lessonReport = _mapper.Map<LessonReport>(request);
                lessonReport.CreatedAt = DateTime.Now;

                var createdReport = await _lessonReportRepository.CreateAsync(lessonReport);
                if (request.StudentNotes != null && request.StudentNotes.Any())
                {
                    var studentNotes = request.StudentNotes.Select(sn =>
                    {
                        var note = _mapper.Map<StudentLessonNote>(sn);
                        note.LessonReportID = createdReport.ReportID;
                        note.CreatedAt = DateTime.Now;
                        return note;
                    }).ToList();
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = "Lesson report created successfully",
                    Message = "Operation completed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> UpdateLessonReportAsync(UpdateLessonReportRequest request, string updatedBy)
        {
            try
            {
                var existingReport = await _lessonReportRepository.GetByIdAsync(request.ReportID);
                if (existingReport == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Lesson report not found"
                    };
                }

                var updatedReport = new LessonReport
                {
                    ReportID = existingReport.ReportID,
                    TimetableSlotID = existingReport.TimetableSlotID,
                    LessonDate = existingReport.LessonDate,
                    CreatedAt = existingReport.CreatedAt,
                    LessonID = request.LessonID,
                    TotalStudents = request.TotalStudents,
                    AbsentStudents = request.AbsentStudents,
                    Remarks = request.Remarks,
                    IsDeleted = false
                };

                await _lessonReportRepository.UpdateAsync(request.ReportID, updatedReport);

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = "Lesson report updated successfully",
                    Message = "Operation completed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<string>> DeleteLessonReportAsync(int reportId, string deletedBy)
        {
            try
            {
                var result = await _lessonReportRepository.DeleteAsync(reportId);
                if (!result)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Lesson report not found"
                    };
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = "Lesson report deleted successfully",
                    Message = "Operation completed"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> SearchLessonReportsAsync(string searchTerm)
        {
            try
            {
                var (reports, _) = await _lessonReportRepository.GetPagedWithDetailsAsync(1, 1000, searchTerm);
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Search completed successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<LessonReportResponse>>> SortLessonReportsAsync(string sortBy, bool ascending = true)
        {
            try
            {
                var (reports, _) = await _lessonReportRepository.GetPagedWithDetailsAsync(1, 1000, null, sortBy, ascending);
                var response = _mapper.Map<List<LessonReportResponse>>(reports);
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = true,
                    Data = response,
                    Message = "Sort completed successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<LessonReportResponse>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
