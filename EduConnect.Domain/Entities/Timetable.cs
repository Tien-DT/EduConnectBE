using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Timetable
{
    [Key]
    public int TimetableID { get; set; }

    public int ClassID { get; set; }

    public string TeacherID { get; set; } = string.Empty; 

    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [StringLength(50)]
    public string? Room { get; set; }

    [ForeignKey("ClassID")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("TeacherID")]
    public virtual Teacher Teacher { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
