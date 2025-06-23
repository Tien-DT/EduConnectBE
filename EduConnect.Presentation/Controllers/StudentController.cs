using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

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
    }
}
