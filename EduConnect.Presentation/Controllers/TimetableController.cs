using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/timetables")]
    [ApiController]
    //[Authorize]
    public class TimetableController : ControllerBase
    {
        private readonly ITimetableService _timetableService;

        public TimetableController(ITimetableService timetableService)
        {
            _timetableService = timetableService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTimetable([FromBody] CreateTimetableRequest request)
        {
            var result = await _timetableService.CreateTimetableAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("{timetableId}")]
        public async Task<IActionResult> GetTimetable(int timetableId)
        {
            var result = await _timetableService.GetTimetableByIdAsync(timetableId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetTimetables([FromQuery] int? classId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            if (classId.HasValue)
            {
                var result = await _timetableService.GetTimetablesByClassIdAsync(classId.Value, startDate, endDate);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            return BadRequest("Missing or invalid query parameter.");
        }

        [HttpDelete("{timetableId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTimetable(int timetableId)
        {
            var result = await _timetableService.DeleteTimetableAsync(timetableId);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}