using EduConnect.Application.DTOs.Request;
using EduConnect.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackRequest request)
        {
            var result = await _feedbackService.CreateFeedbackAsync(request);
            if (result.Success)
                return CreatedAtAction(nameof(GetFeedbackById), new { id = result.Data?.FeedbackID }, result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackRequest request)
        {
            var result = await _feedbackService.UpdateFeedbackAsync(id, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(id);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedbacks(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "DateTime",
            [FromQuery] bool ascending = false,
            [FromQuery] string? targetFilter = null)
        {
            var result = await _feedbackService.GetFeedbacksAsync(
                pageIndex, pageSize, searchTerm, sortBy, ascending, targetFilter);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("parent/{parentId}")]
        public async Task<IActionResult> GetFeedbacksByParentId(string parentId)
        {
            var result = await _feedbackService.GetFeedbacksByParentIdAsync(parentId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("target/{target}")]
        public async Task<IActionResult> GetFeedbacksByTarget(string target)
        {
            var result = await _feedbackService.GetFeedbacksByTargetAsync(target);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
