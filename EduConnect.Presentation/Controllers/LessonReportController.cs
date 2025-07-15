using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/lessonreports")]
    [ApiController]
    [Authorize]
    public class LessonReportController : ControllerBase
    {
        private readonly ILessonReportService _lessonReportService;

        public LessonReportController(ILessonReportService lessonReportService)
        {
            _lessonReportService = lessonReportService;
        }

        // GET: api/lessonreport
        [HttpGet]
        public async Task<IActionResult> GetLessonReports(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "LessonDate",
            [FromQuery] bool ascending = false,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? teacherId = null,
            [FromQuery] int? classId = null,
            [FromQuery] int? timetableSlotId = null
        )
        {
            if (pageIndex < 1)
                return BadRequest("Page index must be greater than or equal to 1");
            if (pageSize < 1)
                return BadRequest("Page size must be greater than or equal to 1");
            if (!string.IsNullOrEmpty(teacherId))
            {
                var result = await _lessonReportService.GetLessonReportsByTeacherAsync(
                    teacherId, pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate);
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            if (classId.HasValue)
            {
                var result = await _lessonReportService.GetLessonReportsByClassAsync(
                    classId.Value, pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate);
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            if (timetableSlotId.HasValue)
            {
                var result = await _lessonReportService.GetLessonReportsByTimetableSlotAsync(
                    timetableSlotId.Value, pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate);
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            var resultAll = await _lessonReportService.GetPagedLessonReportsAsync(
                pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate);
            if (resultAll.Success)
                return Ok(resultAll);
            return BadRequest(resultAll);
        }

        // GET: api/lessonreport/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _lessonReportService.GetLessonReportByIdAsync(id);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // POST: api/lessonreport
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateLessonReportRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User not found");

            var result = await _lessonReportService.CreateLessonReportAsync(request, currentUserId);
            if (result.Success)
                return CreatedAtAction(nameof(GetById), new { id = 1 }, result);
            return BadRequest(result);
        }

        // PUT: api/lessonreport
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateLessonReportRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User not found");

            var result = await _lessonReportService.UpdateLessonReportAsync(request, currentUserId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // DELETE: api/lessonreport/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Teacher,Principal")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User not found");

            var result = await _lessonReportService.DeleteLessonReportAsync(id, currentUserId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        // GET: api/lessonreport/my-reports (For teachers to get their own reports)
        [HttpGet("my-reports")]
        //[Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyReports(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "LessonDate",
            [FromQuery] bool ascending = false,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User not found");

            if (pageIndex < 1)
                return BadRequest("Page index must be greater than or equal to 1");

            if (pageSize < 1)
                return BadRequest("Page size must be greater than or equal to 1");

            var result = await _lessonReportService.GetLessonReportsByTeacherAsync(
                currentUserId, pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate);
            
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
