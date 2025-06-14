using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("Attendance")]
[Index("LessonId", "StudentId", Name = "UQ_Attendance", IsUnique = true)]
public partial class Attendance
{
    [Key]
    [Column("AttendanceID")]
    public int AttendanceId { get; set; }

    [Column("LessonID")]
    public int LessonId { get; set; }

    [Column("StudentID")]
    public int StudentId { get; set; }

    public bool? IsPresent { get; set; }

    [Column(TypeName = "text")]
    public string? Note { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("Attendances")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Attendances")]
    public virtual Student Student { get; set; } = null!;
}
