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

    public string TeacherID { get; set; } = string.Empty;

    public int TimetableID { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public int SubjectId { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("TeacherID")]
    public virtual Teacher Teacher { get; set; } = null!;

    [ForeignKey("TimetableID")]
    public virtual Timetable Timetable { get; set; } = null!;

    [ForeignKey("SubjectId")]
    public virtual Subject Subject { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public virtual ICollection<LessonReport> LessonReports { get; set; } = new List<LessonReport>();
}