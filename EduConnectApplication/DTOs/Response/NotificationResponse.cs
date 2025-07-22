using System;
using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Response
{
    public class NotificationResponse
    {
        public string NotificationId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public Dictionary<string, object>? MetaData { get; set; }
    }
}
