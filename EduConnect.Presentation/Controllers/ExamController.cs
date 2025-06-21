using EduConnect.Application.Services.Class;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpGet("Exam/{studentId}")]
        public async Task<IActionResult> GetExamByStudentId(int studentId)
        {
            if (studentId <= 0)
            {
                return BadRequest("Invalid student ID.");
            }

            var result = await _examService.GetByIdAsync(studentId);

            if (result == null)
            {
                return NotFound("No exams found for this student.");
            }

            return Ok(result);
        }

    }
}
