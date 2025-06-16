using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Lesson
{
    [Key]
    public int LessonID { get; set; }

    public int ClassID { get; set; }

    public int TeacherID { get; set; }

    public int TimetableID { get; set; }

    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("TeacherID")]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey("TimetableID")]
    public virtual Timetable Timetable { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public virtual ICollection<LessonReport> LessonReports { get; set; } = new List<LessonReport>();
}
