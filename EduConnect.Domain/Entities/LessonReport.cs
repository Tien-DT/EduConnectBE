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
    [Column("ReportID")]
    public int ReportId { get; set; }

    [Column("LessonID")]
    public int LessonId { get; set; }

    public int? TotalStudents { get; set; }

    public int? AbsentStudents { get; set; }

    [Column(TypeName = "text")]
    public string? Remarks { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("LessonReports")]
    public virtual Lesson Lesson { get; set; } = null!;
}
