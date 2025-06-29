using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Teacher")]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        // GET: api/grade/lesson/{lessonId}
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetByLesson(int lessonId)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _gradeService.GetGradesByLessonAsync(lessonId, teacherId);
            if (result.Success)
                return Ok(result);
            return StatusCode(403, new { success = false, message = result.Message });
        }

        // GET: api/grade/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _gradeService.GetGradeByIdAsync(id, teacherId);
            if (result.Success)
                return Ok(result);
            return StatusCode(403, new { success = false, message = result.Message });
        }

        // POST: api/grade
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGradeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _gradeService.CreateGradeAsync(request, teacherId);
            if (result.Success)
                return Ok(result);
            return StatusCode(403, new { success = false, message = result.Message });
        }

        // PUT: api/grade
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGradeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _gradeService.UpdateGradeAsync(request, teacherId);
            if (result.Success)
                return Ok(result);
            return StatusCode(403, new { success = false, message = result.Message });
        }

        // DELETE: api/grade/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _gradeService.DeleteGradeAsync(id, teacherId);
            if (result.Success)
                return Ok(result);
            return StatusCode(403, new { success = false, message = result.Message });
        }
    }
} 