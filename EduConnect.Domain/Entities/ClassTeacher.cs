using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace EduConnect.Domain.Entities;

[Table("ClassTeacher")]
[Index("ClassID", "TeacherID", Name = "ClassTeacher_index_0", IsUnique = true)]
public partial class ClassTeacher
{
    [Key]
    public int ID { get; set; }

    public int ClassID { get; set; }

    public int TeacherID { get; set; }

    [ForeignKey("ClassID")]
    [InverseProperty("ClassTeachers")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("TeacherID")]
    [InverseProperty("ClassTeachers")]
    public virtual Teacher Teacher { get; set; } = null!;
}
