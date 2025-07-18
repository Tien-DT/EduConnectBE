using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Domain.Entities;

public partial class Class
{
    [Key]
    public int ClassID { get; set; } 

    [StringLength(50)]
    public string? ClassName { get; set; }
    public bool? IsDeleted { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
