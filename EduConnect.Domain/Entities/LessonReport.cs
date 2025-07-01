using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class LessonReport
{
    [Key]
    public int ReportID { get; set; }

    public int? LessonID { get; set; } // Làm nullable để có thể chỉ dùng TimetableSlotID

    public int TimetableSlotID { get; set; }

    public DateTime LessonDate { get; set; }

    public int? TotalStudents { get; set; }

    public int? AbsentStudents { get; set; }

    public bool? IsDeleted { get; set; }

    [Column(TypeName = "text")]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("LessonID")]
    public virtual Lesson? Lesson { get; set; }

    [ForeignKey("TimetableSlotID")]
    public virtual TimetableSlot TimetableSlot { get; set; } = null!;

    public virtual ICollection<StudentLessonNote> StudentLessonNotes { get; set; } = new List<StudentLessonNote>();
}
