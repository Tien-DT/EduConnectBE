using System;

namespace EduConnectApplication.DTOs.Response
{
    public class ReportResponse
    {
        public int ReportId { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
