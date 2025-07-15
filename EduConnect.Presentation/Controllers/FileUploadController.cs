using EduConnectApplication.Services.Class;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EduConnectApplication.DTOs.Request;
using EduConnect.Application.DTOs.Response;
using System.Collections.Generic;

namespace EduConnect.Presentation.Controllers
{
    [Route("api/v1/files")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;

        public FileUploadController(FileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("")]
        public async Task<IActionResult> Upload([FromForm] UploadFileRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse(new List<string>{"No file uploaded."}, "No file uploaded."));

            var url = await _fileUploadService.UploadFileAsync(request.File);
            if (string.IsNullOrEmpty(url))
                return StatusCode(500, ApiResponse<string>.ErrorResponse(new List<string>{"Upload failed."}, "Upload failed."));

            return Ok(ApiResponse<string>.SuccessResponse(url, "Upload successful"));
        }
    }
}