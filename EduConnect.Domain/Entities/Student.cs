using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Student")]
public partial class Student
{
    [Key]
    [Column("StudentID")]
    public int StudentId { get; set; }

    [Column("AccountID")]
    public Guid AccountId { get; set; }

    [Column("ParentID")]
    public int ParentId { get; set; }

    [Column("ClassID")]
    public int ClassId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Gender { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Students")]
    public virtual Account Account { get; set; } = null!;

    [InverseProperty("Student")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [ForeignKey("ClassId")]
    [InverseProperty("Students")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Student")]
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    [ForeignKey("ParentId")]
    [InverseProperty("Students")]
    public virtual Parent Parent { get; set; } = null!;
}
