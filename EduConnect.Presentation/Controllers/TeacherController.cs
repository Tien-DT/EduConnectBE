using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet("timetable/{teacherId}")]
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
