using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Student")]
    public class StudentGradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public StudentGradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        // GET: api/studentgrade/my-grades
        // Lấy tất cả điểm của học sinh đang đăng nhập
        [HttpGet("my-grades")]
        public async Task<IActionResult> GetMyGrades([FromQuery] string? semester = null)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("Không tìm thấy thông tin học sinh");

            var result = await _gradeService.GetStudentGradesAsync(studentId, semester);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/studentgrade/my-summary/{semester}
        // Lấy tổng quan điểm của học sinh theo học kỳ
        [HttpGet("my-summary/{semester}")]
        public async Task<IActionResult> GetMyGradeSummary(string semester)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("Không tìm thấy thông tin học sinh");

            var result = await _gradeService.GetStudentGradeSummaryAsync(studentId, semester);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/studentgrade/{gradeId}
        // Lấy điểm cụ thể của học sinh
        [HttpGet("{gradeId}")]
        public async Task<IActionResult> GetMyGradeById(int gradeId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("Không tìm thấy thông tin học sinh");

            var result = await _gradeService.GetStudentGradeByIdAsync(gradeId, studentId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/studentgrade/semesters
        // Lấy danh sách học kỳ có điểm của học sinh
        [HttpGet("semesters")]
        public async Task<IActionResult> GetMySemesters()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("Không tìm thấy thông tin học sinh");

            var result = await _gradeService.GetStudentGradesAsync(studentId);
            if (result.Success)
            {
                var semesters = result.Data
                    .Select(g => g.Semester)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                return Ok(new
                {
                    Success = true,
                    Data = semesters,
                    Message = "Lấy danh sách học kỳ thành công"
                });
            }
            return BadRequest(result);
        }
    }
} 