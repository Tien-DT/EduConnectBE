using System.ComponentModel.DataAnnotations;

namespace EduConnectApplication.DTOs.Request
{
    public class CreateReportRequest
    {
        [Required]
        public string AccountId { get; set; } = string.Empty;
        [Required]
        public string ReportType { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
    }
}
