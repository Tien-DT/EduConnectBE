using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduConnect.Application.Services.Interface
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<PagedResponse<Student>> GetPagedStudentsAsync(int pageIndex, int pageSize);
        Task<ApiResponse<TimetableResponse>> GetTimetableByStudentIdAsync(string studentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResponse<List<TimetableResponse>>> GetAllTimetablesByStudentIdAsync(string studentId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
