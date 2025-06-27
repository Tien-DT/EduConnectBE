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
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }
        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _studentRepository.GetAllAsync();
        }

        public async Task<PagedResponse<Student>> GetPagedStudentsAsync(int pageIndex, int pageSize)
        {
            var allStudents = await _studentRepository.GetAllAsync();
            var totalCount = allStudents.Count();
            var items = allStudents.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResponse<Student>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<TimetableResponse>> GetTimetableByStudentIdAsync(string studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var student = (await _studentRepository.GetAllAsync())
                .FirstOrDefault(s => s.StudentID == studentId && (s.IsDeleted == null || s.IsDeleted == false));
            if (student == null)
            {
                return ApiResponse<TimetableResponse>.ErrorResponse(new List<string> { "Student not found" }, "Student not found");
            }
            var timetable = await _studentRepository.GetLatestTimetableByClassIdAsync(student.ClassID ?? 0, startDate, endDate);
            if (timetable == null)
            {
                return ApiResponse<TimetableResponse>.ErrorResponse(new List<string> { "No timetable found for this class" }, "No timetable found");
            }
            var response = new TimetableResponse
            {
                TimetableID = timetable.TimetableID,
                Name = timetable.Name ?? string.Empty,
                StartDate = timetable.StartDate,
                EndDate = timetable.EndDate,
                Slots = timetable.TimetableSlots != null ? timetable.TimetableSlots
                    .Where(slot => slot.IsDeleted == null || slot.IsDeleted == false)
                    .OrderBy(slot => slot.DayOfWeek).ThenBy(slot => slot.Period)
                    .Select(slot => new TimetableSlotResponse
                    {
                        TimetableSlotID = slot.TimetableSlotID,
                        DayOfWeek = slot.DayOfWeek ?? string.Empty,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Period = slot.Period,
                        Room = slot.Room,
                        SubjectName = slot.Subject?.SubjectName ?? string.Empty,
                        TeacherName = slot.Teacher?.FullName ?? slot.Teacher?.TeacherID ?? string.Empty
                    }).ToList() : new List<TimetableSlotResponse>()
            };
            return ApiResponse<TimetableResponse>.SuccessResponse(response, "Timetable retrieved successfully");
        }

        public async Task<ApiResponse<List<TimetableResponse>>> GetAllTimetablesByStudentIdAsync(string studentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var student = (await _studentRepository.GetAllAsync())
                .FirstOrDefault(s => s.StudentID == studentId && (s.IsDeleted == null || s.IsDeleted == false));
            if (student == null)
            {
                return ApiResponse<List<TimetableResponse>>.ErrorResponse(new List<string> { "Student not found" }, "Student not found");
            }
            
            var timetables = await _studentRepository.GetAllTimetablesByClassIdAsync(student.ClassID ?? 0, startDate, endDate);
            if (timetables == null || !timetables.Any())
            {
                return ApiResponse<List<TimetableResponse>>.ErrorResponse(new List<string> { "No timetables found for this class" }, "No timetables found");
            }
            
            var responses = timetables.Select(timetable => new TimetableResponse
            {
                TimetableID = timetable.TimetableID,
                Name = timetable.Name ?? string.Empty,
                StartDate = timetable.StartDate,
                EndDate = timetable.EndDate,
                Slots = timetable.TimetableSlots != null ? timetable.TimetableSlots
                    .Where(slot => slot.IsDeleted == null || slot.IsDeleted == false)
                    .OrderBy(slot => slot.DayOfWeek).ThenBy(slot => slot.Period)
                    .Select(slot => new TimetableSlotResponse
                    {
                        TimetableSlotID = slot.TimetableSlotID,
                        DayOfWeek = slot.DayOfWeek ?? string.Empty,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Period = slot.Period,
                        Room = slot.Room,
                        SubjectName = slot.Subject?.SubjectName ?? string.Empty,
                        TeacherName = slot.Teacher?.FullName ?? slot.Teacher?.TeacherID ?? string.Empty
                    }).ToList() : new List<TimetableSlotResponse>()
            }).ToList();
            
            return ApiResponse<List<TimetableResponse>>.SuccessResponse(responses, "All timetables retrieved successfully");
        }

        public async Task<ApiResponse<StudentResponse>> GetStudentByIdAsync(string studentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentId))
                {
                    return ApiResponse<StudentResponse>.ErrorResponse(
                        new List<string> { "Student ID is required" }, 
                        "Invalid student ID");
                }

                var student = await _studentRepository.GetStudentWithDetailsAsync(studentId);
                
                if (student == null)
                {
                    return ApiResponse<StudentResponse>.ErrorResponse(
                        new List<string> { "Student not found" }, 
                        "Student not found");
                }

                var studentResponse = MapToStudentResponse(student);

                return ApiResponse<StudentResponse>.SuccessResponse(
                    studentResponse, 
                    "Student retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<StudentResponse>.ErrorResponse(
                    new List<string> { ex.Message }, 
                    "Error retrieving student");
            }
        }

        public async Task<PagedApiResponse<List<StudentResponse>>> GetStudentsAsync(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            string sortBy = "FullName", 
            bool ascending = true)
        {
            return await FilterSortSearchStudentsAsync(searchTerm, sortBy, ascending, pageIndex, pageSize);
        }

        public async Task<PagedApiResponse<List<StudentResponse>>> FilterSortSearchStudentsAsync(
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

                var query = _studentRepository.GetStudentQueryable();

                query = StudentQueryHelper.ApplySearch(query, searchTerm);
                query = StudentQueryHelper.ApplySort(query, sortBy, ascending);

                var totalCount = await Task.FromResult(query.Count());
                var items = await Task.FromResult(query.Skip(pageIndex * pageSize).Take(pageSize).ToList());

                var studentResponses = items.Select(MapToStudentResponse).ToList();

                return PagedApiResponse<List<StudentResponse>>.SuccessPagedResponse(
                    studentResponses,
                    "Students filtered, sorted, and searched successfully",
                    totalCount,
                    pageIndex,
                    pageSize
                );
            }
            catch (Exception ex)
            {
                return PagedApiResponse<List<StudentResponse>>.ErrorPagedResponse(
                    new List<string> { ex.Message },
                    "Error filtering, sorting, and searching students",
                    pageIndex,
                    pageSize
                );
            }
        }

        private StudentResponse MapToStudentResponse(Student student)
        {
            return new StudentResponse
            {
                StudentID = student.StudentID,
                AccountID = student.AccountID,
                ParentID = student.ParentID,
                ClassID = student.ClassID,
                FullName = student.FullName ?? string.Empty,
                Email = student.Account?.Email ?? string.Empty,
                FirstName = student.Account?.FirstName ?? string.Empty,
                LastName = student.Account?.LastName ?? string.Empty,
                AvatarUrl = student.Account?.AvatarUrl ?? string.Empty,
                IsActive = student.Account?.IsActive ?? false,
                CreatedAt = student.Account?.CreatedAt,
                UpdatedAt = student.Account?.UpdatedAt,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender ?? string.Empty,
                ClassName = student.Class?.ClassName ?? string.Empty,
                ParentName = student.Parent?.FullName ?? string.Empty
            };
        }
    }
}
