using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Class")]
public partial class Class
{
    [Key]
    [Column("ClassID")]
    public int ClassId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? ClassName { get; set; }

    [InverseProperty("Class")]
    public virtual ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();

    [InverseProperty("Class")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    [InverseProperty("Class")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("Class")]
    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
