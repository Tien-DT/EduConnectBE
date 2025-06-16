using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class LessonReport
{
    [Key]
    public int ReportID { get; set; }

    public int LessonID { get; set; }

    public int? TotalStudents { get; set; }

    public int? AbsentStudents { get; set; }

    [Column(TypeName = "text")]
    public string? Remarks { get; set; }

    [ForeignKey("LessonID")]
    public virtual Lesson Lesson { get; set; } = null!;
}
