using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Class
{
    public int ClassId { get; set; }

    public string? ClassName { get; set; }

    public virtual ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
