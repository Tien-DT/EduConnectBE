using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Principal")]
    public class ClassStudentController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassStudentController(IClassService classService)
        {
            _classService = classService;
        }

        // GET: api/classstudent?classId=1
        [HttpGet]
        public async Task<IActionResult> GetByClass([FromQuery] int classId)
        {
            var result = await _classService.GetStudentsInClassAsync(classId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // GET: api/classstudent/unassigned
        [HttpGet("unassigned")]
        public async Task<IActionResult> GetUnassigned()
        {
            var result = await _classService.GetStudentsNotInClassAsync();
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // POST: api/classstudent
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddStudentsToClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _classService.AddStudentsToClassAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // DELETE: api/classstudent
        [HttpDelete]
        public async Task<IActionResult> Remove([FromBody] RemoveStudentsFromClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _classService.RemoveStudentsFromClassAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // PUT: api/classstudent
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStudentClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _classService.UpdateStudentClassAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
} 