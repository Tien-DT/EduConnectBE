using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("ClassTeacher")]
[Index("ClassId", "TeacherId", Name = "UQ_ClassTeacher", IsUnique = true)]
public partial class ClassTeacher
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ClassID")]
    public int ClassId { get; set; }

    [Column("TeacherID")]
    public int TeacherId { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("ClassTeachers")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("TeacherId")]
    [InverseProperty("ClassTeachers")]
    public virtual Teacher Teacher { get; set; } = null!;
}
