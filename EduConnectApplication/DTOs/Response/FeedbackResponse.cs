using System;

namespace EduConnect.Application.DTOs.Response
{
    public class FeedbackResponse
    {
        public int FeedbackId { get; set; }
        public string ParentId { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string? TargetDescription { get; set; }
        public string? Category { get; set; }
        public object? ContentDetails { get; set; } // For flexible content structure
        public DateTime? CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
