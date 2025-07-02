using System;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class UpdateFeedbackRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Target { get; set; } = string.Empty;
    }
}
