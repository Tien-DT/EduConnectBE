using System.Collections.Generic;
using System.Threading.Tasks;
using EduConnectApplication.DTOs.Request;
using EduConnectApplication.DTOs.Response;
using EduConnectApplication.Services.Interface;
using EduConnect.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace EduConnect.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ReportResponse>>> Get([FromQuery] GetReportRequest request)
        {
            var result = await _reportService.GetReportsAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReportResponse>> GetById(int id)
        {
            var result = await _reportService.GetReportByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ReportResponse>> Create([FromBody] CreateReportRequest request)
        {
            var result = await _reportService.CreateReportAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ReportId }, result);
        }

        [HttpPut]
        public async Task<ActionResult<ReportResponse>> Update([FromBody] UpdateReportRequest request)
        {
            var result = await _reportService.UpdateReportAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _reportService.DeleteReportAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
