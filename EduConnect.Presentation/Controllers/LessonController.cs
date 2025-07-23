using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers;

[Route("api/v1/teachers/{teacherId}/lessons")]
[ApiController]
//[Authorize]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    // GET /api/v1/teachers/{teacherId}/lessons
    [HttpGet("")]
    public async Task<IActionResult> GetLessonsByTeacherId(
        [FromRoute] string teacherId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            return BadRequest("Teacher ID cannot be empty");

        if (pageIndex < 0)
            return BadRequest("Page index must be greater than or equal to 0");

        if (pageSize <= 0 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        var result = await _lessonService.GetLessonsByTeacherIdAsync(teacherId, pageIndex, pageSize);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    // GET /api/v1/teachers/{teacherId}/lessons/{lessonId}
    [HttpGet("{lessonId}")]
    public async Task<IActionResult> GetLessonById(
        [FromRoute] string teacherId,
        [FromRoute] int lessonId)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            return BadRequest("Teacher ID cannot be empty");

        if (lessonId <= 0)
            return BadRequest("Lesson ID must be greater than 0");

        var result = await _lessonService.GetLessonByIdAsync(lessonId, teacherId);
        
        if (result.Success)
            return Ok(result);
        
        return NotFound(result);
    }

    // POST /api/v1/teachers/{teacherId}/lessons
    [HttpPost("")]
    public async Task<IActionResult> CreateLesson(
        [FromRoute] string teacherId,
        [FromBody] CreateLessonRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(teacherId))
            return BadRequest("Teacher ID cannot be empty");

        // Ensure the teacher ID in the route matches the one in the request
        if (request.TeacherId != teacherId)
            return BadRequest("Teacher ID in route must match Teacher ID in request body");

        var result = await _lessonService.CreateLessonAsync(request);
        
        if (result.Success)
            return CreatedAtAction(nameof(GetLessonById), 
                new { teacherId = teacherId, lessonId = result.Data?.LessonId }, result);
        
        return BadRequest(result);
    }

    // PUT /api/v1/teachers/{teacherId}/lessons/{lessonId}
    [HttpPut("{lessonId}")]
    public async Task<IActionResult> UpdateLesson(
        [FromRoute] string teacherId,
        [FromRoute] int lessonId,
        [FromBody] UpdateLessonRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(teacherId))
            return BadRequest("Teacher ID cannot be empty");

        if (lessonId <= 0)
            return BadRequest("Lesson ID must be greater than 0");

        var result = await _lessonService.UpdateLessonAsync(lessonId, teacherId, request);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    // DELETE /api/v1/teachers/{teacherId}/lessons/{lessonId}
    [HttpDelete("{lessonId}")]
    public async Task<IActionResult> DeleteLesson(
        [FromRoute] string teacherId,
        [FromRoute] int lessonId)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            return BadRequest("Teacher ID cannot be empty");

        if (lessonId <= 0)
            return BadRequest("Lesson ID must be greater than 0");

        var result = await _lessonService.DeleteLessonAsync(lessonId, teacherId);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    // GET /api/v1/teachers/{teacherId}/lessons/date-range
    [HttpGet("date-range")]
    public async Task<IActionResult> GetLessonsByDateRange(
        [FromRoute] string teacherId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            return BadRequest("Teacher ID cannot be empty");

        if (startDate == default)
            return BadRequest("Start date is required");

        if (endDate == default)
            return BadRequest("End date is required");

        if (startDate > endDate)
            return BadRequest("Start date cannot be later than end date");

        var result = await _lessonService.GetLessonsByDateRangeAsync(teacherId, startDate, endDate);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }
}
