using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("LessonReport")]
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
    [InverseProperty("LessonReports")]
    public virtual Lesson Lesson { get; set; } = null!;
}
