using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Student
{
    [Key]
    public int StudentID { get; set; }

    [Required]
    public string AccountID { get; set; } = string.Empty;

    public int ParentID { get; set; }

    public int ClassID { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [ForeignKey("AccountID")]
    public virtual ApplicationUser Account { get; set; } = null!;

    [ForeignKey("ParentID")]
    public virtual Parent Parent { get; set; } = null!;

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
