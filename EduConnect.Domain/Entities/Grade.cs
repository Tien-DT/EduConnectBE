using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("Grade")]
[Index("StudentID", "LessonID", "GradeType", Name = "Grade_index_3", IsUnique = true)]
public partial class Grade
{
    [Key]
    public int GradeID { get; set; }

    public int StudentID { get; set; }

    public int LessonID { get; set; }

    [StringLength(255)]
    public string? GradeType { get; set; }

    public double? Score { get; set; }

    [StringLength(255)]
    public string? Evaluation { get; set; }

    [ForeignKey("LessonID")]
    [InverseProperty("Grades")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("StudentID")]
    [InverseProperty("Grades")]
    public virtual Student Student { get; set; } = null!;
}
