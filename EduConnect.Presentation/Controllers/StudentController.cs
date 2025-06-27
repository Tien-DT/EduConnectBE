using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet("Students")]
        public async Task<IActionResult> GetStudents(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "FullName",
            [FromQuery] bool ascending = true)
        {
            var result = await _studentService.GetStudentsAsync(pageIndex, pageSize, searchTerm, sortBy, ascending);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetStudentById(string studentId)
        {
            var result = await _studentService.GetStudentByIdAsync(studentId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
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
