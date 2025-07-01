using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetAll()
        {
            var result = await _lessonReportService.GetAllLessonReportsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
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

        // GET: api/lessonreport/paged
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "LessonDate",
            [FromQuery] bool ascending = false,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? teacherId = null,
            [FromQuery] int? classId = null)
        {
            if (pageIndex < 1)
                return BadRequest("Page index must be greater than or equal to 1");

            if (pageSize < 1)
                return BadRequest("Page size must be greater than or equal to 1");

            var result = await _lessonReportService.GetPagedLessonReportsAsync(
                pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, teacherId, classId);
            
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/lessonreport/teacher/{teacherId}
        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacher(
            string teacherId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _lessonReportService.GetLessonReportsByTeacherAsync(teacherId, startDate, endDate);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/lessonreport/class/{classId}
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetByClass(
            int classId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _lessonReportService.GetLessonReportsByClassAsync(classId, startDate, endDate);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/lessonreport/timetableslot/{timetableSlotId}
        [HttpGet("timetableslot/{timetableSlotId}")]
        public async Task<IActionResult> GetByTimetableSlot(
            int timetableSlotId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _lessonReportService.GetLessonReportsByTimetableSlotAsync(timetableSlotId, startDate, endDate);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/lessonreport/daterange
        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _lessonReportService.GetLessonReportsByDateRangeAsync(startDate, endDate);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/lessonreport/search
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return BadRequest("Search term is required");

            var result = await _lessonReportService.SearchLessonReportsAsync(searchTerm);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // GET: api/lessonreport/sort
        [HttpGet("sort")]
        public async Task<IActionResult> Sort(
            [FromQuery] string sortBy = "LessonDate",
            [FromQuery] bool ascending = false)
        {
            var result = await _lessonReportService.SortLessonReportsAsync(sortBy, ascending);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        // POST: api/lessonreport
        [HttpPost]
        [Authorize(Roles = "Teacher,Principal")]
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
        [Authorize(Roles = "Teacher,Principal")]
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
        [Authorize(Roles = "Teacher,Principal")]
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
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyReports(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "LessonDate",
            [FromQuery] bool ascending = false)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("User not found");

            if (pageIndex < 1)
                return BadRequest("Page index must be greater than or equal to 1");

            if (pageSize < 1)
                return BadRequest("Page size must be greater than or equal to 1");

            var result = await _lessonReportService.GetPagedLessonReportsAsync(
                pageIndex, pageSize, searchTerm, sortBy, ascending, startDate, endDate, currentUserId, null);
            
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
