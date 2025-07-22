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
        public string Content { get; set; } = string.Empty; // Nội dung chi tiết

        [StringLength(500)]
        public string? Summary { get; set; } // Tóm tắt ngắn cho push notification

        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // daily_report, weekly_report, monthly_report

        [StringLength(50)]
        public string Category { get; set; } = "report"; // report, announcement, reminder

        public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High

        public bool IsRead { get; set; } = false;

        public bool IsPushed { get; set; } = false; // Đã gửi push notification chưa

        public DateTime? PushedAt { get; set; } // Thời gian gửi push

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; } // Thời gian đọc

        public DateTime? ExpiresAt { get; set; } // Thông báo hết hạn

        // JSON data chứa thông tin bổ sung
        public string? MetaData { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
