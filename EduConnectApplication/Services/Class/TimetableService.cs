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
    public class TimetableService : ITimetableService
    {
        private readonly ITimetableRepository _timetableRepository;

        public TimetableService(ITimetableRepository timetableRepository)
        {
            _timetableRepository = timetableRepository;
        }

        public async Task<ApiResponse<TimetableResponse>> CreateTimetableAsync(CreateTimetableRequest request)
        {
            try
            {
                if (request.ClassID <= 0)
                {
                    return ApiResponse<TimetableResponse>.ErrorResponse(
                        new List<string> { "Invalid class ID. Class ID must be greater than 0." },
                        "Invalid class ID");
                }
                if (request.StartDate >= request.EndDate)
                {
                    return ApiResponse<TimetableResponse>.ErrorResponse(
                        new List<string> { "Start date must be before end date" },
                        "Invalid date range");
                }
                bool timetableExists = await _timetableRepository.IsTimetableExistsForClassAndDateRangeAsync(
                    request.ClassID, request.StartDate, request.EndDate);

                if (timetableExists)
                {
                    return ApiResponse<TimetableResponse>.ErrorResponse(
                        new List<string> { "A timetable already exists for this class in the given date range" },
                        "Timetable already exists");
                }
                var duplicatePeriods = request.Slots
                    .GroupBy(s => new { s.DayOfWeek, s.Period })
                    .Where(g => g.Count() > 1)
                    .Select(g => $"Day: {g.Key.DayOfWeek}, Period: {g.Key.Period}")
                    .ToList();

                if (duplicatePeriods.Any())
                {
                    return ApiResponse<TimetableResponse>.ErrorResponse(
                        new List<string> { $"Duplicate periods found: {string.Join("; ", duplicatePeriods)}" },
                        "Duplicate time slots found");
                }
                var timetable = new Timetable
                {
                    ClassID = request.ClassID,
                    Name = request.Name,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    IsDeleted = false
                };
                foreach (var slotRequest in request.Slots)
                {
                    var timetableSlot = new TimetableSlot
                    {
                        TeacherID = slotRequest.TeacherID,
                        SubjectId = slotRequest.SubjectId,
                        DayOfWeek = slotRequest.DayOfWeek,
                        StartTime = slotRequest.StartTime,
                        EndTime = slotRequest.EndTime,
                        Room = slotRequest.Room,
                        Period = slotRequest.Period,
                        IsDeleted = false
                    };

                    timetable.TimetableSlots.Add(timetableSlot);
                }

                try
                {
                    var createdTimetable = await _timetableRepository.CreateTimetableAsync(timetable);
                    var response = new TimetableResponse
                    {
                        TimetableID = createdTimetable.TimetableID,
                        Name = createdTimetable.Name,
                        StartDate = createdTimetable.StartDate,
                        EndDate = createdTimetable.EndDate,
                        Slots = createdTimetable.TimetableSlots.Select(slot => new TimetableSlotResponse
                        {
                            TimetableSlotID = slot.TimetableSlotID,
                            DayOfWeek = slot.DayOfWeek,
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime,
                            Period = slot.Period,
                            Room = slot.Room,
                            SubjectName = slot.Subject?.SubjectName ?? string.Empty,
                            TeacherName = slot.Teacher?.FullName ?? slot.Teacher?.TeacherID ?? string.Empty
                        }).OrderBy(s => s.DayOfWeek).ThenBy(s => s.Period).ToList()
                    };

                    return ApiResponse<TimetableResponse>.SuccessResponse(
                        response,
                        "Timetable created successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = "Lỗi cơ sở dữ liệu xảy ra khi lưu thời khóa biểu.";

                    if (dbEx.InnerException != null)
                    {
                        if (dbEx.InnerException.Message.Contains("FOREIGN KEY"))
                        {
                            errorMessage = "Invalid reference: The class, teacher, or subject may not exist.";
                        }
                        else if (dbEx.InnerException.Message.Contains("UNIQUE") ||
                                dbEx.InnerException.Message.Contains("UQ_TimetableSlot_Period"))
                        {
                            errorMessage = "Tìm thấy tiết học trùng lặp trong cùng một ngày. Mỗi ngày chỉ có thể có một tiết học cho mỗi số tiết.";
                        }

                        return ApiResponse<TimetableResponse>.ErrorResponse(
                            new List<string> { errorMessage, dbEx.InnerException.Message },
                            "Vi phạm ràng buộc cơ sở dữ liệu");
                    }

                    return ApiResponse<TimetableResponse>.ErrorResponse(
                        new List<string> { errorMessage, dbEx.Message },
                        "Lỗi cơ sở dữ liệu");
                }
            }
            catch (Exception ex)
            {
                var innerException = ex;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                return ApiResponse<TimetableResponse>.ErrorResponse(
                    new List<string> { innerException.Message },
                    "Lỗi khi tạo thời khóa biểu");
            }
        }

        public async Task<ApiResponse<TimetableResponse>> GetTimetableByIdAsync(int timetableId)
        {
            try
            {
                var timetable = await _timetableRepository.GetTimetableByIdAsync(timetableId);
                if (timetable == null)
                {
                    return ApiResponse<TimetableResponse>.ErrorResponse(
                        new List<string> { "Timetable not found" },
                        "Timetable not found");
                }

                var response = new TimetableResponse
                {
                    TimetableID = timetable.TimetableID,
                    Name = timetable.Name,
                    StartDate = timetable.StartDate,
                    EndDate = timetable.EndDate,
                    Slots = timetable.TimetableSlots
                        .Where(slot => slot.IsDeleted != true)
                        .OrderBy(slot => slot.DayOfWeek)
                        .ThenBy(slot => slot.Period)
                        .Select(slot => new TimetableSlotResponse
                        {
                            TimetableSlotID = slot.TimetableSlotID,
                            DayOfWeek = slot.DayOfWeek,
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime,
                            Period = slot.Period,
                            Room = slot.Room,
                            SubjectName = slot.Subject?.SubjectName ?? string.Empty,
                            TeacherName = slot.Teacher?.FullName ?? slot.Teacher?.TeacherID ?? string.Empty
                        }).ToList()
                };

                return ApiResponse<TimetableResponse>.SuccessResponse(
                    response,
                    "Timetable retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<TimetableResponse>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving timetable");
            }
        }

        public async Task<ApiResponse<List<TimetableResponse>>> GetTimetablesByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var timetables = await _timetableRepository.GetTimetablesByClassIdAsync(classId, startDate, endDate);
                if (timetables == null || !timetables.Any())
                {
                    return ApiResponse<List<TimetableResponse>>.ErrorResponse(
                        new List<string> { "No timetables found" },
                        "No timetables found");
                }

                var responses = timetables.Select(timetable => new TimetableResponse
                {
                    TimetableID = timetable.TimetableID,
                    Name = timetable.Name,
                    StartDate = timetable.StartDate,
                    EndDate = timetable.EndDate,
                    Slots = timetable.TimetableSlots
                        .Where(slot => slot.IsDeleted != true)
                        .OrderBy(slot => slot.DayOfWeek)
                        .ThenBy(slot => slot.Period)
                        .Select(slot => new TimetableSlotResponse
                        {
                            TimetableSlotID = slot.TimetableSlotID,
                            DayOfWeek = slot.DayOfWeek,
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime,
                            Period = slot.Period,
                            Room = slot.Room,
                            SubjectName = slot.Subject?.SubjectName ?? string.Empty,
                            TeacherName = slot.Teacher?.FullName ?? slot.Teacher?.TeacherID ?? string.Empty
                        }).ToList()
                }).ToList();

                return ApiResponse<List<TimetableResponse>>.SuccessResponse(
                    responses,
                    "Timetables retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TimetableResponse>>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error retrieving timetables");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTimetableAsync(int timetableId)
        {
            try
            {
                bool result = await _timetableRepository.DeleteTimetableAsync(timetableId);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse(
                        new List<string> { "Timetable not found" },
                        "Timetable not found");
                }

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Timetable deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(
                    new List<string> { ex.Message },
                    "Error deleting timetable");
            }
        }
    }
}