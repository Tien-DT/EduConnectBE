using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Teacher")]
public partial class Teacher
{
    [Key]
    [Column("TeacherID")]
    public int TeacherId { get; set; }

    [Column("AccountID")]
    public Guid AccountId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? FullName { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Teachers")]
    public virtual Account Account { get; set; } = null!;

    [InverseProperty("Teacher")]
    public virtual ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();

    [InverseProperty("Teacher")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    [InverseProperty("Teacher")]
    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
