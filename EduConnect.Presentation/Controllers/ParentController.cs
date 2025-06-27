using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentController : ControllerBase
    {
        private readonly IParentService _parentService;
        
        public ParentController(IParentService parentService)
        {
            _parentService = parentService;
        }

        [HttpGet("Parents")]
        public async Task<IActionResult> GetParents(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "FullName",
            [FromQuery] bool ascending = true)
        {
            var result = await _parentService.GetParentsAsync(pageIndex, pageSize, searchTerm, sortBy, ascending);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("{parentId}")]
        public async Task<IActionResult> GetParentById(string parentId)
        {
            var result = await _parentService.GetParentByIdAsync(parentId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("{parentId}/students")]
        public async Task<IActionResult> GetStudentsByParentId(
            string parentId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "FullName",
            [FromQuery] bool ascending = true)
        {
            var result = await _parentService.GetStudentsByParentIdAsync(parentId, pageIndex, pageSize, searchTerm, sortBy, ascending);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
