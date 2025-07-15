using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/classmanagement")]
    [ApiController]
    [Authorize]
    public class ClassManagementController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassManagementController(IClassService classService)
        {
            _classService = classService;
        }

        
        [HttpGet("classes/{classId}/students")]
        public async Task<IActionResult> GetStudentsInClass([FromRoute] int classId)
        {
            if (classId <= 0)
                return BadRequest("Class ID must be greater than 0");

            var result = await _classService.GetStudentsInClassAsync(classId);
            if (result.Success)
                return Ok(result);
            
            return result.Data == null ? NotFound(result) : BadRequest(result);
        }

        
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

        
        [HttpPost("classes/{classId}/students")]
        public async Task<IActionResult> AddStudentsToClass(
            [FromRoute] int classId,
            [FromBody] AddStudentsToClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (classId <= 0)
                return BadRequest("Class ID must be greater than 0");

            if (request.StudentIDs == null || request.StudentIDs.Count == 0)
                return BadRequest("Student IDs list cannot be empty");

            var result = await _classService.AddStudentsToClassAsync(classId, request.StudentIDs);
            if (result.Success)
                return CreatedAtAction(nameof(GetStudentsInClass), new { classId }, result);
            
            return BadRequest(result);
        }

        
        [HttpDelete("classes/{classId}/students")]
        public async Task<IActionResult> RemoveStudentsFromClass(
            [FromRoute] int classId,
            [FromBody] RemoveStudentsFromClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (classId <= 0)
                return BadRequest("Class ID must be greater than 0");

            if (request.StudentIDs == null || request.StudentIDs.Count == 0)
                return BadRequest("Student IDs list cannot be empty");

            var result = await _classService.RemoveStudentsFromClassAsync(classId, request.StudentIDs);
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        
        [HttpPut("students/{studentId}/class")]
        public async Task<IActionResult> UpdateStudentClass(
            [FromRoute] string studentId,
            [FromBody] UpdateStudentClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(studentId))
                return BadRequest("Student ID cannot be empty");

            var result = await _classService.UpdateStudentClassAsync(studentId, request.NewClassID);
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

       
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
