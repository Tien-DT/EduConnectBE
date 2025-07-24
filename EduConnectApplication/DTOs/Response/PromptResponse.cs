using System;

namespace EduConnect.Application.DTOs.Response
{
    public class PromptResponse
    {
        public int PromptId { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string CreatedByEmail { get; set; } = string.Empty;
    }
}
