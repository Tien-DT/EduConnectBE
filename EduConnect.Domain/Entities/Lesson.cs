using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("Lesson")]
[Index("ClassID", "Subject", "Date", "TeacherID", Name = "Lesson_index_1", IsUnique = true)]
public partial class Lesson
{
    [Key]
    public int LessonID { get; set; }

    public int ClassID { get; set; }

    [StringLength(255)]
    public string Subject { get; set; } = null!;

    public int TeacherID { get; set; }

    public DateOnly Date { get; set; }

    public int TimetableID { get; set; }

    [ForeignKey("ClassID")]
    [InverseProperty("Lessons")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Lesson")]
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    [InverseProperty("Lesson")]
    public virtual ICollection<LessonReport> LessonReports { get; set; } = new List<LessonReport>();

    [ForeignKey("TeacherID")]
    [InverseProperty("Lessons")]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey("TimetableID")]
    [InverseProperty("Lessons")]
    public virtual Timetable Timetable { get; set; } = null!;
}
