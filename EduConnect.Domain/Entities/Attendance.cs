using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Domain.Entities;

public partial class Attendance
{
    [Key]
    public int AttendanceID { get; set; }

    public int TimetableSlotID { get; set; }

    public string StudentID { get; set; } = string.Empty;

    public int Status { get; set; }

    public bool? IsDeleted { get; set; } 

    [Column(TypeName = "text")]
    public string? Note { get; set; }

    [ForeignKey("TimetableSlotID")]
    public virtual TimetableSlot TimetableSlot { get; set; } = null!;

    [ForeignKey("StudentID")]
    public virtual Student Student { get; set; } = null!;
}
