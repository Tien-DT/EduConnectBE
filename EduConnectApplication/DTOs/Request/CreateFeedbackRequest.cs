using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EduConnect.Application.DTOs.Request
{
    public class CreateFeedbackRequest
    {
        [Required]
        public string ParentId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Target { get; set; } = string.Empty;

        [StringLength(200)]
        public string? TargetDescription { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        // Additional content details for specific targets
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
    }
}
