using AutoMapper;
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
    public class TeacherService : ITeacherService
    {
        private readonly ITimetableSlotRepository _timetableSlotRepository;

        public TeacherService(ITimetableSlotRepository timetableSlotRepository)
        {
            _timetableSlotRepository = timetableSlotRepository;
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
                TimetableID = 0,
                Name = string.Empty,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MaxValue,
                Slots = slots.OrderBy(s => s.DayOfWeek).ThenBy(s => s.Period).Select(slot => new TimetableSlotResponse
                {
                    TimetableSlotID = slot.TimetableSlotID,
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
    }
}
