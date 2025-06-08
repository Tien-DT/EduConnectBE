using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Domain.Entities;

[Table("Timetable")]
public partial class Timetable
{
    [Key]
    public int TimetableID { get; set; }

    public int ClassID { get; set; }

    [StringLength(255)]
    public string Subject { get; set; } = null!;

    public int TeacherID { get; set; }

    [StringLength(255)]
    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [StringLength(255)]
    public string? Room { get; set; }

    [ForeignKey("ClassID")]
    [InverseProperty("Timetables")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Timetable")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    [ForeignKey("TeacherID")]
    [InverseProperty("Timetables")]
    public virtual Teacher Teacher { get; set; } = null!;
}
