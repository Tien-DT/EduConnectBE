using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/classes")]
    [ApiController]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

       
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

      
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassById(int classId)
        {
            var result = await _classService.GetClassByIdAsync(classId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }
    }
}
