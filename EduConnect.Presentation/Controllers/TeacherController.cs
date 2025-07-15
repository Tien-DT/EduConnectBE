using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/teachers")]
    [ApiController]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetTeachers(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "FullName",
            [FromQuery] bool ascending = true)
        {
            var result = await _teacherService.GetTeachersAsync(pageIndex, pageSize, searchTerm, sortBy, ascending);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("{teacherId}")]
        public async Task<IActionResult> GetTeacherById(string teacherId)
        {
            var result = await _teacherService.GetTeacherByIdAsync(teacherId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("{teacherId}/timetable")]
        [Authorize(Roles ="Teacher")]
        public async Task<IActionResult> GetTimetableByTeacherId(string teacherId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _teacherService.GetTimetableByTeacherIdAsync(teacherId, startDate, endDate);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
    }
}
