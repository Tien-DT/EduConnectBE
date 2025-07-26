using System.ComponentModel.DataAnnotations;

namespace EduConnect.Application.DTOs.Request
{
    public class CreatePromptRequest
    {
        [Required]
        [StringLength(500, ErrorMessage = "Title không được vượt quá 500 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(10000, ErrorMessage = "Prompt không được vượt quá 10000 ký tự")]
        public string PromptText { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Type không được vượt quá 100 ký tự")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Status không được vượt quá 50 ký tự")]
        public string Status { get; set; } = string.Empty;
    }

    public class UpdatePromptRequest
    {
        [Required]
        public int PromptId { get; set; }

        [StringLength(500, ErrorMessage = "Title không được vượt quá 500 ký tự")]
        public string? Title { get; set; }

        [StringLength(10000, ErrorMessage = "Prompt không được vượt quá 10000 ký tự")]
        public string? PromptText { get; set; }

        [StringLength(100, ErrorMessage = "Type không được vượt quá 100 ký tự")]
        public string? Type { get; set; }

        [StringLength(50, ErrorMessage = "Status không được vượt quá 50 ký tự")]
        public string? Status { get; set; }
    }
}
