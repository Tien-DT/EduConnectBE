using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;

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

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedStudents([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var pagedResult = await _studentService.GetPagedStudentsAsync(pageIndex, pageSize);
            return Ok(pagedResult);
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("timetable/{studentId}")]
        public async Task<IActionResult> GetTimetableByStudentId(string studentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var timetable = await _studentService.GetTimetableByStudentIdAsync(studentId, startDate, endDate);
            if (timetable == null)
                return NotFound(new { message = "Student or timetable not found" });
            return Ok(timetable);
        }

        [HttpGet("timetables/{studentId}")]
        public async Task<IActionResult> GetAllTimetablesByStudentId(string studentId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var timetables = await _studentService.GetAllTimetablesByStudentIdAsync(studentId, startDate, endDate);
            if (!timetables.Success)
                return NotFound(timetables);
            return Ok(timetables);
        }
    }
}
