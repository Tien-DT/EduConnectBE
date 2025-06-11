using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Timetable
{
    public int TimetableId { get; set; }

    public int ClassId { get; set; }

    public string Subject { get; set; } = null!;

    public int TeacherId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Room { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Teacher Teacher { get; set; } = null!;
}
