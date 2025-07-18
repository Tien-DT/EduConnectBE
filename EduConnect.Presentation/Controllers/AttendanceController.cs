using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/attendances")]
    [ApiController]
    //[Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET /api/v1/attendances?timetableSlotId=&studentId=&status=&searchTerm=&sortBy=&ascending=&pageIndex=&pageSize=
        [HttpGet("")]
        public async Task<IActionResult> FilterSortSearchAttendances(
            [FromQuery] int? timetableSlotId = null,
            [FromQuery] string? studentId = null,
            [FromQuery] int? status = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _attendanceService.FilterSortSearchAttendancesAsync(
                timetableSlotId, studentId, status, searchTerm, sortBy, ascending, pageIndex, pageSize);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET /api/v1/attendances/timetable-slot/{timetableSlotId}
        [HttpGet("timetable-slot/{timetableSlotId}")]
        public async Task<IActionResult> GetByTimetableSlotId(int timetableSlotId)
        {
            var result = await _attendanceService.GetByTimetableSlotIdAsync(timetableSlotId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // GET /api/v1/attendances/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(string studentId)
        {
            var result = await _attendanceService.GetByStudentIdAsync(studentId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // GET /api/v1/attendances/timetable/{timetableId}
        [HttpGet("timetable/{timetableId}")]
        public async Task<IActionResult> GetByTimetableId(int timetableId)
        {
            var result = await _attendanceService.GetByTimetableIdAsync(timetableId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // PUT /api/v1/attendances/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AttendanceRequest request)
        {
            var result = await _attendanceService.UpdateAsync(id, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 