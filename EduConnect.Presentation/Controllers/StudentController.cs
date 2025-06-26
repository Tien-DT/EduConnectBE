using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("timetables/{studentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAllTimetablesByStudentId(string studentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var timetables = await _studentService.GetAllTimetablesByStudentIdAsync(studentId, startDate, endDate);
            if (!timetables.Success)
                return NotFound(timetables);
            return Ok(timetables);
        }
    }
}
