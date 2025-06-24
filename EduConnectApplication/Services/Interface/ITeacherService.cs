using EduConnect.Application.DTOs.Response;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EduConnect.Application.Services.Interface
{
    public interface ITeacherService
    {
        Task<ApiResponse<TimetableResponse>> GetTimetableByTeacherIdAsync(string teacherId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
