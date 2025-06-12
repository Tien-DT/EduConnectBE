using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int ClassId { get; set; }

    public string Subject { get; set; } = null!;

    public int TeacherId { get; set; }

    public DateOnly Date { get; set; }

    public int TimetableId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<LessonReport> LessonReports { get; set; } = new List<LessonReport>();

    public virtual Teacher Teacher { get; set; } = null!;

    public virtual Timetable Timetable { get; set; } = null!;
}
