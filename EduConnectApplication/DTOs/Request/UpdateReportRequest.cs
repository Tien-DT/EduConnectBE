using System.ComponentModel.DataAnnotations;

namespace EduConnectApplication.DTOs.Request
{
    public class UpdateReportRequest
    {
        [Required]
        public int ReportId { get; set; }
        [Required]
        public string ReportType { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
    }
}
