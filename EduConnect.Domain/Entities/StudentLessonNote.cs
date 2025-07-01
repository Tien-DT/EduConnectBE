using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class StudentLessonNote
{
    [Key]
    public int StudentLessonNoteID { get; set; }

    public int LessonReportID { get; set; }

    public string StudentID { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string NoteType { get; set; } = string.Empty; // "BonusPoint", "Violation", "Good", "Warning"

    [Range(-10, 10)]
    public double? Points { get; set; } // Điểm cộng/trừ (-10 đến +10)

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool? IsDeleted { get; set; }

    [ForeignKey("LessonReportID")]
    public virtual LessonReport LessonReport { get; set; } = null!;

    [ForeignKey("StudentID")]
    public virtual Student Student { get; set; } = null!;
}
