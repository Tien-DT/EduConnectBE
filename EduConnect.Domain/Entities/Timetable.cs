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
    [Column("TimetableID")]
    public int TimetableId { get; set; }

    [Column("ClassID")]
    public int ClassId { get; set; }

    [Column("TeacherID")]
    public int TeacherId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Subject { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Room { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("Timetables")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Timetable")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    [ForeignKey("TeacherId")]
    [InverseProperty("Timetables")]
    public virtual Teacher Teacher { get; set; } = null!;
}
