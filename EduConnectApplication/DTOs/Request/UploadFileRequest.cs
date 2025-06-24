using Microsoft.AspNetCore.Http;

namespace EduConnectApplication.DTOs.Request
{
    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
    }
} 