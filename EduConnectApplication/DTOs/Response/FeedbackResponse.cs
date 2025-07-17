using System;

namespace EduConnect.Application.DTOs.Response
{
    public class FeedbackResponse
    {
        public int FeedbackId { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
        public string Target { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
