using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("Grade")]
[Index("StudentId", "LessonId", "GradeType", Name = "UQ_Grade", IsUnique = true)]
public partial class Grade
{
    [Key]
    [Column("GradeID")]
    public int GradeId { get; set; }

    [Column("StudentID")]
    public int StudentId { get; set; }

    [Column("LessonID")]
    public int LessonId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? GradeType { get; set; }

    public double? Score { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Evaluation { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("Grades")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Grades")]
    public virtual Student Student { get; set; } = null!;
}
