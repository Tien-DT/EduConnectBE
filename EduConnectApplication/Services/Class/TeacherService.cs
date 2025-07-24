using AutoMapper;
using EduConnect.Application.DTOs.Response;
using EduConnect.Application.QueryHelpers;
using EduConnect.Application.Services.Interface;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduConnectApplication.DTOs.Response;

namespace EduConnect.Application.Services.Class
{
    public class TeacherService : ITeacherService
    {
        private readonly ITimetableSlotRepository _timetableSlotRepository;
        private readonly ITeacherRepository _teacherRepository;

        public TeacherService(ITimetableSlotRepository timetableSlotRepository, ITeacherRepository teacherRepository)
        {
            _timetableSlotRepository = timetableSlotRepository;
            _teacherRepository = teacherRepository;
        }
        public async Task<ApiResponse<List<ClassWithTimetableResponse>>> GetClassesWithTimetableByTeacherAsync(string teacherId)
        {
            try
            {
                // Debug: Kiểm tra teacherId
                if (string.IsNullOrEmpty(teacherId))
                {
                    return ApiResponse<List<ClassWithTimetableResponse>>.ErrorResponse(new List<string> { "TeacherId không được để trống" }, "Invalid teacherId");
                }

                // Sử dụng repository method để lấy classes
                var classes = await _timetableSlotRepository.GetClassesByTeacherIdAsync(teacherId);
                
                if (classes == null || !classes.Any())
                {
                    return ApiResponse<List<ClassWithTimetableResponse>>.SuccessResponse(
                        new List<ClassWithTimetableResponse>(), 
                        $"Không tìm thấy lớp nào cho giáo viên {teacherId}");
                }

                var classGroups = classes.Select(c => new ClassWithTimetableResponse
                {
                    ClassId = c.ClassID,
                    ClassName = c.ClassName ?? string.Empty
                }).ToList();

                return ApiResponse<List<ClassWithTimetableResponse>>.SuccessResponse(
                    classGroups, 
                    $"Lấy danh sách lớp thành công. Tìm thấy {classGroups.Count} lớp");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ClassWithTimetableResponse>>.ErrorResponse(new List<string> { ex.Message }, "Lỗi khi lấy danh sách lớp");
            }
        }

        public async Task<ApiResponse<TimetableResponse>> GetTimetableByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var slots = await _timetableSlotRepository.GetTimetableSlotsByTeacherIdAsync(teacherId, startDate, endDate);
            if (slots == null || !slots.Any())
            {
                return ApiResponse<TimetableResponse>.ErrorResponse(new List<string> { "No timetable slot found for this teacher in the given time range" }, "No timetable found");
            }
            var response = new TimetableResponse
            {
                TimetableId = 0,
                Name = string.Empty,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MaxValue,
                Slots = slots.OrderBy(s => s.DayOfWeek).ThenBy(s => s.Period).Select(slot => new TimetableSlotResponse
                {
                    TimetableSlotId = slot.TimetableSlotID,
                    DayOfWeek = slot.DayOfWeek ?? string.Empty,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Period = slot.Period,
                    Room = slot.Room,
                    SubjectName = slot.Subject?.SubjectName ?? string.Empty,
                    TeacherName = slot.Teacher?.FullName ?? slot.Teacher?.TeacherID ?? string.Empty
                }).ToList()
            };
            return ApiResponse<TimetableResponse>.SuccessResponse(response, "Timetable slots retrieved successfully");
        }

        public async Task<ApiResponse<TeacherResponse>> GetTeacherByIdAsync(string teacherId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(teacherId))
                {
                    return ApiResponse<TeacherResponse>.ErrorResponse(
                        new List<string> { "Teacher ID is required" }, 
                        "Invalid teacher ID");
                }

                var teacher = await _teacherRepository.GetTeacherWithDetailsAsync(teacherId);
                
                if (teacher == null)
                {
                    return ApiResponse<TeacherResponse>.ErrorResponse(
                        new List<string> { "Teacher not found" }, 
                        "Teacher not found");
                }

                var teacherResponse = MapToTeacherResponse(teacher);

                return ApiResponse<TeacherResponse>.SuccessResponse(
                    teacherResponse, 
                    "Teacher retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TeacherResponse>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Error retrieving teacher");
            }
        }

        public async Task<PagedApiResponse<List<TeacherResponse>>> GetTeachersAsync(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true)
        {
            return await FilterSortSearchTeachersAsync(searchTerm, sortBy, ascending, pageIndex, pageSize);
        }

        private TeacherResponse MapToTeacherResponse(Teacher teacher)
        {
            return new TeacherResponse
            {
                TeacherId = teacher.TeacherID,
                AccountId = teacher.AccountID,
                FullName = teacher.FullName ?? string.Empty,
                Email = teacher.Account?.Email ?? string.Empty,
                FirstName = teacher.Account?.FirstName ?? string.Empty,
                LastName = teacher.Account?.LastName ?? string.Empty,
                AvatarUrl = teacher.Account?.AvatarUrl ?? string.Empty,
                IsActive = teacher.Account?.IsActive ?? false,
                CreatedAt = teacher.Account?.CreatedAt,
                UpdatedAt = teacher.Account?.UpdatedAt,
                Classes = teacher.ClassTeachers?.Where(ct => ct.IsDeleted != true)
                    .Select(ct => ct.Class?.ClassName ?? string.Empty)
                    .Where(className => !string.IsNullOrEmpty(className))
                    .ToList() ?? new List<string>()
            };
        }

        public async Task<PagedApiResponse<List<TeacherResponse>>> FilterSortSearchTeachersAsync(
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int pageIndex = 0,
            int pageSize = 10)
        {
            try
            {
                if (pageSize <= 0) pageSize = 10;
                if (pageIndex < 0) pageIndex = 0;

                var query = _teacherRepository.GetTeacherQueryable();

                query = TeacherQueryHelper.ApplySearch(query, searchTerm);
                query = TeacherQueryHelper.ApplySort(query, sortBy, ascending);

                var totalCount = await Task.FromResult(query.Count());
                var items = await Task.FromResult(query.Skip(pageIndex * pageSize).Take(pageSize).ToList());

                var teacherResponses = items.Select(MapToTeacherResponse).ToList();

                return PagedApiResponse<List<TeacherResponse>>.SuccessPagedResponse(
                    teacherResponses,
                    "Teachers filtered, sorted, and searched successfully",
                    totalCount,
                    pageIndex,
                    pageSize
                );
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<TeacherResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error filtering, sorting, and searching teachers",
                    pageIndex,
                    pageSize
                );
            }
        }
    }
}
