using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;

namespace EduConnect.Application.Services.Class;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IClassRepository _classRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITimetableRepository _timetableRepository;
    private readonly IMapper _mapper;

    public LessonService(
        ILessonRepository lessonRepository,
        IClassRepository classRepository,
        ITeacherRepository teacherRepository,
        ISubjectRepository subjectRepository,
        ITimetableRepository timetableRepository,
        IMapper mapper)
    {
        _lessonRepository = lessonRepository;
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _subjectRepository = subjectRepository;
        _timetableRepository = timetableRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<LessonResponse>>> GetLessonsByTeacherIdAsync(string teacherId, int pageIndex, int pageSize)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return new ApiResponse<PagedResponse<LessonResponse>>
                {
                    Success = false,
                    Message = "Teacher ID cannot be empty"
                };
            }

            var lessons = await _lessonRepository.GetLessonsByTeacherIdAsync(teacherId, pageIndex, pageSize);
            var totalCount = await _lessonRepository.CountLessonsByTeacherIdAsync(teacherId);

            var lessonResponses = _mapper.Map<List<LessonResponse>>(lessons);

            var pagedResponse = new PagedResponse<LessonResponse>
            {
                Items = lessonResponses,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResponse<LessonResponse>>
            {
                Success = true,
                Data = pagedResponse,
                Message = "Lessons retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResponse<LessonResponse>>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<LessonResponse>> GetLessonByIdAsync(int lessonId, string teacherId)
    {
        try
        {
            if (lessonId <= 0)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Lesson ID must be greater than 0"
                };
            }

            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Teacher ID cannot be empty"
                };
            }

            var lesson = await _lessonRepository.GetLessonByIdAndTeacherIdAsync(lessonId, teacherId);
            
            if (lesson == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Lesson not found or you don't have permission to access it"
                };
            }

            var lessonResponse = _mapper.Map<LessonResponse>(lesson);

            return new ApiResponse<LessonResponse>
            {
                Success = true,
                Data = lessonResponse,
                Message = "Lesson retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<LessonResponse>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<LessonResponse>> CreateLessonAsync(CreateLessonRequest request)
    {
        try
        {
            // Validate teacher exists
            var teacher = await _teacherRepository.GetByIdAsync(request.TeacherId);
            if (teacher == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Teacher not found"
                };
            }

            // Validate class exists
            var classEntity = await _classRepository.GetByIdAsync(request.ClassId);
            if (classEntity == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Class not found"
                };
            }

            // Validate subject exists
            var subject = await _subjectRepository.GetByIdAsync(request.SubjectId);
            if (subject == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Subject not found"
                };
            }

            // Validate timetable exists
            var timetable = await _timetableRepository.GetTimetableByIdAsync(request.TimetableId);
            if (timetable == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Timetable not found"
                };
            }

            var lesson = _mapper.Map<Lesson>(request);
            lesson.IsDeleted = false;

            var createdLesson = await _lessonRepository.CreateAsync(lesson);
            
            // Get the created lesson with related data
            var lessonWithDetails = await _lessonRepository.GetLessonByIdAndTeacherIdAsync(createdLesson.LessonID, request.TeacherId);
            var lessonResponse = _mapper.Map<LessonResponse>(lessonWithDetails);

            return new ApiResponse<LessonResponse>
            {
                Success = true,
                Data = lessonResponse,
                Message = "Lesson created successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<LessonResponse>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<LessonResponse>> UpdateLessonAsync(int lessonId, string teacherId, UpdateLessonRequest request)
    {
        try
        {
            if (lessonId <= 0)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Lesson ID must be greater than 0"
                };
            }

            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Teacher ID cannot be empty"
                };
            }

            var existingLesson = await _lessonRepository.GetLessonByIdAndTeacherIdAsync(lessonId, teacherId);
            if (existingLesson == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Lesson not found or you don't have permission to update it"
                };
            }

            // Update only the provided fields
            if (request.ClassId.HasValue)
            {
                var classEntity = await _classRepository.GetByIdAsync(request.ClassId.Value);
                if (classEntity == null)
                {
                    return new ApiResponse<LessonResponse>
                    {
                        Success = false,
                        Message = "Class not found"
                    };
                }
                existingLesson.ClassID = request.ClassId.Value;
            }

            if (request.TimetableId.HasValue)
            {
                var timetable = await _timetableRepository.GetTimetableByIdAsync(request.TimetableId.Value);
                if (timetable == null)
                {
                    return new ApiResponse<LessonResponse>
                    {
                        Success = false,
                        Message = "Timetable not found"
                    };
                }
                existingLesson.TimetableID = request.TimetableId.Value;
            }

            if (request.SubjectId.HasValue)
            {
                var subject = await _subjectRepository.GetByIdAsync(request.SubjectId.Value);
                if (subject == null)
                {
                    return new ApiResponse<LessonResponse>
                    {
                        Success = false,
                        Message = "Subject not found"
                    };
                }
                existingLesson.SubjectId = request.SubjectId.Value;
            }

            if (request.Date.HasValue)
            {
                existingLesson.Date = request.Date.Value;
            }

            var updatedLesson = await _lessonRepository.UpdateAsync(lessonId, existingLesson);
            
            if (updatedLesson == null)
            {
                return new ApiResponse<LessonResponse>
                {
                    Success = false,
                    Message = "Failed to update lesson"
                };
            }

            // Get the updated lesson with related data
            var lessonWithDetails = await _lessonRepository.GetLessonByIdAndTeacherIdAsync(lessonId, teacherId);
            var lessonResponse = _mapper.Map<LessonResponse>(lessonWithDetails);

            return new ApiResponse<LessonResponse>
            {
                Success = true,
                Data = lessonResponse,
                Message = "Lesson updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<LessonResponse>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteLessonAsync(int lessonId, string teacherId)
    {
        try
        {
            if (lessonId <= 0)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lesson ID must be greater than 0"
                };
            }

            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Teacher ID cannot be empty"
                };
            }

            var existingLesson = await _lessonRepository.GetLessonByIdAndTeacherIdAsync(lessonId, teacherId);
            if (existingLesson == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Lesson not found or you don't have permission to delete it"
                };
            }

            // Soft delete
            existingLesson.IsDeleted = true;
            var result = await _lessonRepository.UpdateAsync(lessonId, existingLesson);

            return new ApiResponse<bool>
            {
                Success = result != null,
                Data = result != null,
                Message = result != null ? "Lesson deleted successfully" : "Failed to delete lesson"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<LessonResponse>>> GetLessonsByDateRangeAsync(string teacherId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return new ApiResponse<IEnumerable<LessonResponse>>
                {
                    Success = false,
                    Message = "Teacher ID cannot be empty"
                };
            }

            if (startDate > endDate)
            {
                return new ApiResponse<IEnumerable<LessonResponse>>
                {
                    Success = false,
                    Message = "Start date cannot be later than end date"
                };
            }

            var lessons = await _lessonRepository.GetLessonsByTeacherAndDateRangeAsync(teacherId, startDate, endDate);
            var lessonResponses = _mapper.Map<IEnumerable<LessonResponse>>(lessons);

            return new ApiResponse<IEnumerable<LessonResponse>>
            {
                Success = true,
                Data = lessonResponses,
                Message = "Lessons retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<LessonResponse>>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
}
