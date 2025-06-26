using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

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

        [HttpGet("by-account/{accountId}")]
        public async Task<IActionResult> GetExamsByAccountId(
            string accountId, 
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool ascending = true,
            [FromQuery] int? pageIndex = null,
            [FromQuery] int? pageSize = null)
        {
            DateTime today = DateTime.Today;
            DateTime defaultStartDate = startDate ?? today;
            DateTime defaultEndDate = endDate ?? today.AddDays(7);
            
            if (string.IsNullOrEmpty(accountId))
            {
                return BadRequest("Account ID is required");
            }
            if (pageIndex.HasValue && pageIndex.Value < 1)
            {
                return BadRequest("Page index must be greater than or equal to 1");
            }

            if (pageSize.HasValue && pageSize.Value < 1)
            {
                return BadRequest("Page size must be greater than or equal to 1");
            }

            var result = await _examService.GetExamsByAccountIdAsync(
                accountId, 
                defaultStartDate, 
                defaultEndDate, 
                ascending,
                pageIndex,
                pageSize);
                
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("by-student/{studentId}")]
        public async Task<IActionResult> GetExamsByStudentId(
            string studentId, 
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool ascending = true,
            [FromQuery] int? pageIndex = null,
            [FromQuery] int? pageSize = null)
        {
            DateTime today = DateTime.Today; 
            DateTime defaultStartDate = startDate ?? today;
            DateTime defaultEndDate = endDate ?? today.AddDays(7);
            
            if (string.IsNullOrEmpty(studentId))
            {
                return BadRequest("Student ID is required");
            }

            if (pageIndex.HasValue && pageIndex.Value < 1)
            {
                return BadRequest("Page index must be greater than or equal to 1");
            }

            if (pageSize.HasValue && pageSize.Value < 1)
            {
                return BadRequest("Page size must be greater than or equal to 1");
            }

            var result = await _examService.GetExamsByStudentIdAsync(
                studentId, 
                defaultStartDate, 
                defaultEndDate, 
                ascending,
                pageIndex,
                pageSize);
                
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("by-classid")]
        public async Task<IActionResult> CreateExamsForClass([FromBody] CreateClassExamRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _examService.CreateClassExamsAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpPost("by-principal")]
        public async Task<IActionResult> CreateExamsForStudents([FromBody] CreatePrincipalExamRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _examService.CreatePrincipalExamsAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpGet("exam")]
        public async Task<IActionResult> FilterSortSearchExams(
            [FromQuery] string? studentId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _examService.FilterSortSearchExamsAsync(studentId, searchTerm, sortBy, ascending, pageIndex, pageSize);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        
        [HttpGet("test-student-role")]
        [Authorize(Roles = "Student")]
        public IActionResult TestStudentRole()
        {
            return Ok("OK - User role authorization successful");
        }

        [HttpGet("test-user-role")]
        [Authorize(Roles = "User")]
        public IActionResult TestUserRole()
        {
            return Ok("OK - User role authorization successful");
        }

        [HttpGet("test-actual-student-role")]
        [Authorize(Roles = "Student")]
        public IActionResult TestActualStudentRole()
        {
            return Ok("OK - Student role authorization successful");
        }
        
        [HttpGet("debug-claims")]
        [Authorize]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var customRoles = User.FindAll("role").Select(c => c.Value).ToList();
            
            return Ok(new
            {
                AllClaims = claims,
                StandardRoles = roles,
                CustomRoles = customRoles,
                IsInStudentRole = User.IsInRole("Student"),
                UserIdentity = User.Identity?.Name
            });
        }   
    }
}