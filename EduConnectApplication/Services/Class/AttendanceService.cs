using AutoMapper;
using EduConnect.Application.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using EduConnect.Domain.Entities;
using EduConnect.Infrastruture.Repositories.Interface;
using EduConnect.Application.Services.Interface;

namespace EduConnect.Application.Services.Class
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IMapper _mapper;

        public AttendanceService(IAttendanceRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PagedApiResponse<IEnumerable<AttendanceResponse>>> FilterSortSearchAttendancesAsync(
            int? lessonId, string? studentId, bool? isPresent, string? searchTerm,
            string? sortBy, bool ascending, int pageIndex, int pageSize)
        {
            var (attendances, totalCount) = await _repo.FilterSortSearchAsync(
                lessonId, studentId, isPresent, searchTerm, sortBy, ascending, pageIndex, pageSize);
            var mapped = _mapper.Map<IEnumerable<AttendanceResponse>>(attendances);
            return PagedApiResponse<IEnumerable<AttendanceResponse>>.SuccessPagedResponse(
                mapped, "Lấy danh sách attendance thành công", totalCount, pageIndex, pageSize);
        }

        public async Task<ApiResponse<IEnumerable<AttendanceResponse>>> GetByLessonIdAsync(int lessonId)
        {
            var data = await _repo.GetByLessonIdAsync(lessonId);
            var mapped = _mapper.Map<IEnumerable<AttendanceResponse>>(data);
            return ApiResponse<IEnumerable<AttendanceResponse>>.SuccessResponse(mapped, "Lấy attendance theo lesson thành công");
        }

        public async Task<ApiResponse<IEnumerable<AttendanceResponse>>> GetByStudentIdAsync(string studentId)
        {
            var data = await _repo.GetByStudentIdAsync(studentId);
            var mapped = _mapper.Map<IEnumerable<AttendanceResponse>>(data);
            return ApiResponse<IEnumerable<AttendanceResponse>>.SuccessResponse(mapped, "Lấy attendance theo student thành công");
        }

        public async Task<ApiResponse<IEnumerable<AttendanceResponse>>> GetByTimetableIdAsync(int timetableId)
        {
            var data = await _repo.GetByTimetableIdAsync(timetableId);
            var mapped = _mapper.Map<IEnumerable<AttendanceResponse>>(data);
            return ApiResponse<IEnumerable<AttendanceResponse>>.SuccessResponse(mapped, "Lấy attendance theo timetable thành công");
        }

        public async Task<ApiResponse<AttendanceResponse>> UpdateAsync(int id, AttendanceRequest request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return ApiResponse<AttendanceResponse>.ErrorResponse(new List<string> { "Attendance không tồn tại" }, "Không tìm thấy attendance");
            entity.IsPresent = request.IsPresent;
            entity.Note = request.Note;
            var updated = await _repo.UpdateAsync(id, entity);
            var mapped = _mapper.Map<AttendanceResponse>(updated);
            return ApiResponse<AttendanceResponse>.SuccessResponse(mapped, "Cập nhật attendance thành công");
        }
    }
} 