using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Grade
{
    [Key]
    public int GradeID { get; set; }

    public string StudentID { get; set; } = string.Empty;

    public int LessonID { get; set; }

    [StringLength(50)]
    public string? GradeType { get; set; }

    public double? Score { get; set; }

    public int? GradeLevel { get; set; }

    public string? Semester { get; set; }
    public bool? IsDeleted { get; set; }



    [StringLength(255)]
    public string? Evaluation { get; set; }

    [ForeignKey("StudentID")]
    public virtual Student Student { get; set; } = null!;

    [ForeignKey("LessonID")]
    public virtual Lesson Lesson { get; set; } = null!;
}
