using System;
using System.Collections.Generic;

namespace EduConnect.Domain.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public int ClassId { get; set; }

    public int ParentId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Parent Parent { get; set; } = null!;

    public virtual Account StudentNavigation { get; set; } = null!;
}
