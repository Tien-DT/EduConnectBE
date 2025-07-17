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
        private readonly IStudentLessonNoteRepository _studentLessonNoteRepository;
        private readonly ILessonReportComplaintService _complaintService;
        private readonly IMapper _mapper;

        public LessonReportService(
            ILessonReportRepository lessonReportRepository, 
            IStudentLessonNoteRepository studentLessonNoteRepository,
            ILessonReportComplaintService complaintService,
            IMapper mapper)
        {
            _lessonReportRepository = lessonReportRepository;
            _studentLessonNoteRepository = studentLessonNoteRepository;
            _complaintService = complaintService;
            _mapper = mapper;
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

        public async Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetPagedLessonReportsAsync(
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var (reports, totalCount) = await _lessonReportRepository.GetPagedWithDetailsAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, null, null, null, null);
                
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

        public async Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByTeacherAsync(
            string teacherId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var (reports, totalCount) = await _lessonReportRepository.GetPagedWithDetailsAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, teacherId, null, null, null);
                
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
                    Message = "Teacher lesson reports retrieved successfully"
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

        public async Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByClassAsync(
            int classId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var (reports, totalCount) = await _lessonReportRepository.GetPagedWithDetailsAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, null, classId, null, null);
                
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
                    Message = "Class lesson reports retrieved successfully"
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

        public async Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByTimetableSlotAsync(
            int timetableSlotId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var (reports, totalCount) = await _lessonReportRepository.GetPagedWithDetailsAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, null, null, timetableSlotId, null);
                
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
                    Message = "Timetable slot lesson reports retrieved successfully"
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
                // Validate LessonID if provided
                if (request.LessonID.HasValue)
                {
                    var lessonExists = await _lessonReportRepository.CheckLessonExistsAsync(request.LessonID.Value);
                    if (!lessonExists)
                    {
                        return new ApiResponse<string>
                        {
                            Success = false,
                            Message = $"Lesson with ID {request.LessonID} does not exist"
                        };
                    }
                }

                // Validate TimetableSlotID
                var timetableSlotExists = await _lessonReportRepository.CheckTimetableSlotExistsAsync(request.TimetableSlotID);
                if (!timetableSlotExists)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Timetable slot with ID {request.TimetableSlotID} does not exist"
                    };
                }

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
                lessonReport.IsDeleted = false;

                var createdReport = await _lessonReportRepository.CreateAsync(lessonReport);
                
                // Tạo và lưu StudentLessonNotes nếu có
                if (request.StudentNotes != null && request.StudentNotes.Any())
                {
                    // Validate StudentIDs
                    foreach (var studentNote in request.StudentNotes)
                    {
                        var studentExists = await _lessonReportRepository.CheckStudentExistsAsync(studentNote.StudentID);
                        if (!studentExists)
                        {
                            return new ApiResponse<string>
                            {
                                Success = false,
                                Message = $"Student with ID {studentNote.StudentID} does not exist"
                            };
                        }
                    }

                    var studentNotes = request.StudentNotes.Select(sn =>
                    {
                        var note = _mapper.Map<StudentLessonNote>(sn);
                        note.LessonReportID = createdReport.ReportID;
                        note.CreatedAt = DateTime.Now;
                        note.IsDeleted = false;
                        return note;
                    }).ToList();

                    // Lưu StudentLessonNotes vào database
                    foreach (var note in studentNotes)
                    {
                        await _studentLessonNoteRepository.CreateAsync(note);
                    }
                }

                // Process complaints for parent notifications
                try
                {
                    await _complaintService.ProcessLessonReportComplaintsAsync(createdReport.ReportID);
                }
                catch
                {
                    // Log error but don't fail the main operation
                    // This ensures lesson report creation succeeds even if complaint processing fails
                    // Could use ILogger here if available
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

        public async Task<ApiResponse<PagedResponse<LessonReportResponse>>> GetLessonReportsByStudentAsync(
            string studentId,
            int pageIndex, 
            int pageSize, 
            string? searchTerm = null, 
            string sortBy = "LessonDate", 
            bool ascending = false,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var (reports, totalCount) = await _lessonReportRepository.GetPagedWithDetailsAsync(
                    pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, null, null, null, studentId);
                
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
                    Message = "Student lesson reports retrieved successfully"
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
    }
}
