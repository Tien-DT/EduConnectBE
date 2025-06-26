using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface ITimetableService
    {
        Task<ApiResponse<TimetableResponse>> CreateTimetableAsync(CreateTimetableRequest request);
        Task<ApiResponse<TimetableResponse>> GetTimetableByIdAsync(int timetableId);
        Task<ApiResponse<List<TimetableResponse>>> GetTimetablesByClassIdAsync(int classId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<bool>> DeleteTimetableAsync(int timetableId);
    }
}