using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class LessonReport
{
    public int ReportId { get; set; }

    public int LessonId { get; set; }

    public int? TotalStudents { get; set; }

    public int? AbsentStudents { get; set; }

    public string? Remarks { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;
}
