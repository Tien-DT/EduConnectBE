using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("assign-student")]
        //[Authorize(Roles = "Principal")]
        public async Task<IActionResult> AssignStudentRole([FromBody] AssignStudentRoleListRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.AssignStudentRoleAsync(request);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpPost("assign-teacher")]
        //[Authorize(Roles = "Principal")]
        public async Task<IActionResult> AssignTeacherRole([FromBody] AssignTeacherRoleListRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.AssignTeacherRoleAsync(request);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpPost("assign-parent")]
        //[Authorize(Roles = "Principal")]
        public async Task<IActionResult> AssignParentRole([FromBody] AssignParentRoleListRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _roleService.AssignParentRoleAsync(request);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }
    }
}