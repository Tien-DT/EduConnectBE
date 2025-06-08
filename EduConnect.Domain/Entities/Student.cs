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
    public int StudentID { get; set; }

    [StringLength(255)]
    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(255)]
    public string? Gender { get; set; }

    public int ClassID { get; set; }

    public int ParentID { get; set; }

    [ForeignKey("ClassID")]
    [InverseProperty("Students")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Student")]
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    [ForeignKey("ParentID")]
    [InverseProperty("Students")]
    public virtual Parent Parent { get; set; } = null!;
}
