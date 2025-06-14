using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("Lesson")]
[Index("ClassId", "Subject", "Date", "TeacherId", Name = "UQ_Lesson", IsUnique = true)]
public partial class Lesson
{
    [Key]
    [Column("LessonID")]
    public int LessonId { get; set; }

    [Column("ClassID")]
    public int ClassId { get; set; }

    [Column("TeacherID")]
    public int TeacherId { get; set; }

    [Column("TimetableID")]
    public int TimetableId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Subject { get; set; } = null!;

    public DateOnly Date { get; set; }

    [InverseProperty("Lesson")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [ForeignKey("ClassId")]
    [InverseProperty("Lessons")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Lesson")]
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    [InverseProperty("Lesson")]
    public virtual ICollection<LessonReport> LessonReports { get; set; } = new List<LessonReport>();

    [ForeignKey("TeacherId")]
    [InverseProperty("Lessons")]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey("TimetableId")]
    [InverseProperty("Lessons")]
    public virtual Timetable Timetable { get; set; } = null!;
}
