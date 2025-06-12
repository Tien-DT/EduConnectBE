using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int LessonId { get; set; }

    public int StudentId { get; set; }

    public bool? IsPresent { get; set; }

    public string? Note { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
