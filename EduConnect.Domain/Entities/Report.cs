using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public string AccountId { get; set; } = string.Empty;

        [ForeignKey("AccountId")]
        public virtual ApplicationUser Account { get; set; } = null!;

        [Required]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public bool IsDelete { get; set; } = false;

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
