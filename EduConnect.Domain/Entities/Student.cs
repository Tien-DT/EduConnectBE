using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Student
{
    [Key]
    public string StudentID { get; set; } = string.Empty;

    [Required]
    public string AccountID { get; set; } = string.Empty;

    public string ParentID { get; set; } = string.Empty;

    public int? ClassID { get; set; } 

    [StringLength(100)]
    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("AccountID")]
    public virtual ApplicationUser Account { get; set; } = null!;

    [ForeignKey("ParentID")]
    public virtual Parent Parent { get; set; } = null!;

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public virtual ICollection<StudentLessonNote> StudentLessonNotes { get; set; } = new List<StudentLessonNote>();
}
