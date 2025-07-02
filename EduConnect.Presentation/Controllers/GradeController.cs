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

        private string GetCurrentAccountId()
        {
            return User.FindFirst("AccountId")?.Value ?? "";
        }

        private string GetCurrentTeacherId()
        {
            return User.FindFirst("TeacherId")?.Value ?? "";
        }

       
        [HttpGet("subjects/{subjectId}/grades")]
        public async Task<IActionResult> GetGradesBySubject(int subjectId, [FromQuery] int? classId = null)
        {
            var teacherAccountId = GetCurrentAccountId();
            var result = await _gradeService.GetGradesBySubjectForTeacherAsync(subjectId, teacherAccountId, classId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        
        [HttpGet("subjects/{subjectId}/students/{studentId}/grades")]
        public async Task<IActionResult> GetStudentGradesBySubject(int subjectId, string studentId)
        {
            var teacherAccountId = GetCurrentAccountId();
            var result = await _gradeService.GetStudentGradesBySubjectForTeacherAsync(subjectId, studentId, teacherAccountId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        
        [HttpGet("{gradeId}")]
        public async Task<IActionResult> GetGradeById(int gradeId)
        {
            var teacherAccountId = GetCurrentAccountId();
            var result = await _gradeService.GetGradeByIdForTeacherAsync(gradeId, teacherAccountId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

       
        [HttpGet("subjects/{subjectId}/students")]  
        public async Task<IActionResult> GetStudentsForGrading(int subjectId, [FromQuery] int? classId = null)
        {
            var teacherAccountId = GetCurrentAccountId();
            var result = await _gradeService.GetStudentsForGradingAsync(subjectId, teacherAccountId, classId);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teacherAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherAccountId))
                return Unauthorized("Teacher not found");

            var result = await _gradeService.CreateGradeForTeacherAsync(request, teacherAccountId);
            if (result.Success)
                return CreatedAtAction(nameof(GetGradeById), new { gradeId = result.Data }, result);
            return BadRequest(result);
        }

        
        [HttpPut("{gradeId}")]
        public async Task<IActionResult> UpdateGrade(int gradeId, [FromBody] UpdateGradeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teacherAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherAccountId))
                return Unauthorized("Teacher not found");

            var result = await _gradeService.UpdateGradeForTeacherAsync(gradeId, request, teacherAccountId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        
        [HttpDelete("{gradeId}")]
        public async Task<IActionResult> DeleteGrade(int gradeId)
        {
            var teacherAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherAccountId))
                return Unauthorized("Teacher not found");

            var result = await _gradeService.DeleteGradeForTeacherAsync(gradeId, teacherAccountId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        
        [HttpGet("subjects/{subjectId}/classes/{classId}/students")]
        public async Task<IActionResult> GetStudentsForGrading(int subjectId, int classId)
        {
            var teacherAccountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherAccountId))
                return Unauthorized("Teacher not found");

            var result = await _gradeService.GetStudentsForGradingAsync(subjectId, teacherAccountId, classId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 