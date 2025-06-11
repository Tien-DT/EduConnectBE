using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Grade
{
    public int GradeId { get; set; }

    public int StudentId { get; set; }

    public int LessonId { get; set; }

    public string? GradeType { get; set; }

    public double? Score { get; set; }

    public string? Evaluation { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
