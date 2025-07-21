using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities
{
    public class Notification
    {
        [Key]
        public string NotificationId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; 

        [StringLength(500)]
        public string? Summary { get; set; } 

        [StringLength(50)]
        public string Type { get; set; } = string.Empty; 

        [StringLength(50)]
        public string Category { get; set; } = "report"; 

        public int Priority { get; set; } = 1; 

        public bool IsRead { get; set; } = false;

        public bool IsPushed { get; set; } = false;

        public DateTime? PushedAt { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; } 

        public DateTime? ExpiresAt { get; set; }

        public string? MetaData { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
