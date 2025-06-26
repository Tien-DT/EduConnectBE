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
    }
}
