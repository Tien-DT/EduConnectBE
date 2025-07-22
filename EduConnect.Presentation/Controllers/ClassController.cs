using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/classes")]
    [ApiController]
    //[Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        // GET /api/v1/classes
        [HttpGet("")]
        public async Task<IActionResult> GetClasses(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "ClassName",
            [FromQuery] bool ascending = true)
        {
            var result = await _classService.GetClassesAsync(pageIndex, pageSize, searchTerm, sortBy, ascending);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // GET /api/v1/classes/{classId}
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassById(int classId)
        {
            var result = await _classService.GetClassByIdAsync(classId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // GET /api/v1/classes/{classId}/students
        [HttpGet("{classId}/students")]
        public async Task<IActionResult> GetStudentsInClass([FromRoute] int classId)
        {
            if (classId <= 0)
                return BadRequest("Class ID must be greater than 0");
            var result = await _classService.GetStudentsInClassAsync(classId);
            if (result.Success)
                return Ok(result);
            return result.Data == null ? NotFound(result) : BadRequest(result);
        }

        // POST /api/v1/classes/{classId}/students
        [HttpPost("{classId}/students")]
        public async Task<IActionResult> AddStudentsToClass(
            [FromRoute] int classId,
            [FromBody] AddStudentsToClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (classId <= 0)
                return BadRequest("Class ID must be greater than 0");
            if (request.StudentIds == null || request.StudentIds.Count == 0)
                return BadRequest("Student IDs list cannot be empty");
            var result = await _classService.AddStudentsToClassAsync(classId, request.StudentIds);
            if (result.Success)
                return CreatedAtAction(nameof(GetStudentsInClass), new { classId }, result);
            return BadRequest(result);
        }

        // DELETE /api/v1/classes/{classId}/students
        [HttpDelete("{classId}/students")]
        public async Task<IActionResult> RemoveStudentsFromClass(
            [FromRoute] int classId,
            [FromBody] RemoveStudentsFromClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (classId <= 0)
                return BadRequest("Class ID must be greater than 0");
            if (request.StudentIds == null || request.StudentIds.Count == 0)
                return BadRequest("Student IDs list cannot be empty");
            var result = await _classService.RemoveStudentsFromClassAsync(classId, request.StudentIds);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET /api/v1/classes/students?assigned=false
        [HttpGet("students")]
        public async Task<IActionResult> GetUnassignedStudents([FromQuery] bool? assigned)
        {
            if (assigned.HasValue && assigned.Value == false)
            {
                var result = await _classService.GetStudentsNotInClassAsync();
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            return BadRequest("Only assigned=false is supported in this endpoint.");
        }

        // PUT /api/v1/classes/students/{studentId}/class
        [HttpPut("students/{studentId}/class")]
        public async Task<IActionResult> UpdateStudentClass(
            [FromRoute] string studentId,
            [FromBody] UpdateStudentClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("Student ID cannot be empty");
            var result = await _classService.UpdateStudentClassAsync(studentId, request.NewClassId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // DELETE /api/v1/classes/students/{studentId}/class
        [HttpDelete("students/{studentId}/class")]
        public async Task<IActionResult> RemoveStudentFromAllClasses([FromRoute] string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("Student ID cannot be empty");
            var result = await _classService.UpdateStudentClassAsync(studentId, null);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
