using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/exams")]
    [ApiController]
    [Authorize]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpGet("")]
        public async Task<IActionResult> FilterSortSearchExams(
            [FromQuery] string? studentId = null,
            [FromQuery] string? accountId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? room = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (!string.IsNullOrEmpty(accountId))
            {
                DateTime today = DateTime.Today;
                DateTime defaultStartDate = startDate ?? today;
                DateTime defaultEndDate = endDate ?? today.AddDays(7);
                var result = await _examService.GetExamsByAccountIdAsync(
                    accountId,
                    defaultStartDate,
                    defaultEndDate,
                    ascending,
                    pageIndex,
                    pageSize,
                    searchTerm);
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            if (!string.IsNullOrEmpty(studentId))
            {
                DateTime today = DateTime.Today;
                DateTime defaultStartDate = startDate ?? today;
                DateTime defaultEndDate = endDate ?? today.AddDays(7);
                var result = await _examService.GetExamsByStudentIdAsync(
                    studentId,
                    defaultStartDate,
                    defaultEndDate,
                    ascending,
                    pageIndex,
                    pageSize,
                    searchTerm);
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            var filterResult = await _examService.FilterSortSearchExamsAsync(studentId, searchTerm, room, sortBy, ascending, pageIndex, pageSize);
            if (filterResult.Success)
                return Ok(filterResult);
            return BadRequest(filterResult);
        }

        [HttpPost("class")]
        public async Task<IActionResult> CreateExamsForClass([FromBody] CreateClassExamRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _examService.CreateClassExamsAsync(request, currentUserId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpPost("principal")]
        public async Task<IActionResult> CreateExamsForStudents([FromBody] CreatePrincipalExamRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _examService.CreatePrincipalExamsAsync(request, currentUserId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
    }
}